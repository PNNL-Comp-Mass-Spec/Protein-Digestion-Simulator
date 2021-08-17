﻿Option Strict On

' -------------------------------------------------------------------------------
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004
'
' E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
' Website: https://omics.pnl.gov/ or https://www.pnnl.gov/sysbio/ or https://panomics.pnnl.gov/
' -------------------------------------------------------------------------------
'
' Licensed under the 2-Clause BSD License; you may not use this file except
' in compliance with the License.  You may obtain a copy of the License at
' https://opensource.org/licenses/BSD-2-Clause
'
' Copyright 2018 Battelle Memorial Institute

Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports PRISM
Imports ProteinFileReader
Imports DBUtils = PRISMDatabaseUtils.DataTableUtils

''' <summary>
''' This class will read two FASTA files and look for overlap in protein sequence between the proteins of
''' the first FASTA file and the second FASTA file
''' </summary>
Public Class DigestionSimulator
    Inherits FileProcessor.ProcessFilesBase

    ' Ignore Spelling: const, Da, pre, Sql

    Public Sub New()
        MyBase.mFileDate = "August 6, 2021"
        InitializeLocalVariables()
    End Sub

    Public Const XML_SECTION_PEAK_MATCHING_OPTIONS As String = "PeakMatchingOptions"

    Private Const PROTEIN_ID_COLUMN As String = "ProteinID"
    Private Const PEPTIDE_ID_MATCH_COLUMN As String = "PeptideIDMatch"

    Private Const ID_COUNT_DISTRIBUTION_MAX As Integer = 10

    ' Error codes specialized for this class
    Public Enum ErrorCodes
        NoError = 0
        ProteinDigestionSimulatorSectionNotFound = 1
        ErrorReadingInputFile = 2
        ProteinsNotFoundInInputFile = 4
        ErrorIdentifyingSequences = 8
        ErrorWritingOutputFile = 16
        UserAbortedSearch = 32
        UnspecifiedError = -1
    End Enum

    Private Structure SingleBinStats
        Public MassBinStart As Double               ' Mass is >= this value
        Public MassBinEnd As Double                 ' Mass is < this value
        Public UniqueResultIDCount As Integer
        Public NonUniqueResultIDCount As Integer
        Public ResultIDCountDistribution() As Integer
        Public PercentUnique As Single              ' UniqueResultIDs().length / (UniqueResultIDs().length + NonUniqueResultIDs().length)
    End Structure

    Private Structure MassBinningOptions
        Public AutoDetermineMassRange As Boolean
        Public MassBinSizeDa As Single
        Public MassMinimum As Single    ' This is ignored if AutoDetermineMassRange = True
        Public MassMaximum As Single    ' This is ignored if AutoDetermineMassRange = True
        Public MinimumSLiCScore As Single
    End Structure

    Private Structure BinnedPeptideCountStats
        Public Settings As MassBinningOptions
        Public BinCount As Integer
        Public Bins() As SingleBinStats
    End Structure

    ''' <summary>
    ''' Protein file parser
    ''' </summary>
    ''' <remarks>This class is exposed as public so that we can directly access some of its properties without having to create wrapper properties in this class</remarks>
    Public WithEvents mProteinFileParser As ProteinFileParser

    Private mOutputFileDelimiter As Char
    Private mMaxPeakMatchingResultsPerFeatureToSave As Integer

    Private mPeptideUniquenessBinningSettings As MassBinningOptions

    Private mLocalErrorCode As ErrorCodes
    Private mLastErrorMessage As String

    Private WithEvents mPeakMatching As PeakMatching

    ''' <summary>
    ''' Comparison peptides to match against
    ''' </summary>
    Private WithEvents mComparisonPeptideInfo As PeakMatching.PMComparisonFeatureInfo
    Private WithEvents mProteinInfo As ProteinCollection
    Private WithEvents mPeptideMatchResults As PeakMatching.PMFeatureMatchResults

    ''' <summary>
    ''' Holds the lists of peptides that were uniquely identified for each protein
    ''' </summary>
    Private mProteinToIdentifiedPeptideMappingTable As DataTable

    ''' <summary>
    ''' Thresholds to use for searching
    ''' </summary>
    Private mThresholdLevels() As PeakMatching.SearchThresholds

    Private mSubtaskProgressStepDescription As String = String.Empty
    Private mSubtaskProgressPercentComplete As Single = 0

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="taskDescription"></param>
    ''' <param name="percentComplete">Ranges from 0 to 100, but can contain decimal percentage values</param>
    Public Event SubtaskProgressChanged(taskDescription As String, percentComplete As Single)

    Public Property AutoDetermineMassRangeForBinning As Boolean
        Get
            Return mPeptideUniquenessBinningSettings.AutoDetermineMassRange
        End Get
        Set
            mPeptideUniquenessBinningSettings.AutoDetermineMassRange = Value
        End Set
    End Property

    Public Property CreateSeparateOutputFileForEachThreshold As Boolean

    Public Property CysPeptidesOnly As Boolean

    ''' <summary>
    '''
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>Ignored for FASTA files; they are always digested</remarks>
    Public Property DigestSequences As Boolean

    Public Property ElementMassMode As PeptideSequence.ElementModeConstants
        Get
            If mProteinFileParser Is Nothing Then
                Return PeptideSequence.ElementModeConstants.IsotopicMass
            Else
                Return mProteinFileParser.ElementMassMode
            End If
        End Get
        Set
            If mProteinFileParser Is Nothing Then
                InitializeProteinFileParser()
            End If
            mProteinFileParser.ElementMassMode = Value
        End Set
    End Property

    Public ReadOnly Property LocalErrorCode As ErrorCodes
        Get
            Return mLocalErrorCode
        End Get
    End Property

    Public Property MaxPeakMatchingResultsPerFeatureToSave As Integer
        Get
            Return mMaxPeakMatchingResultsPerFeatureToSave
        End Get
        Set
            If Value < 1 Then Value = 1
            mMaxPeakMatchingResultsPerFeatureToSave = Value
        End Set
    End Property

    Public Property MinimumSLiCScoreToBeConsideredUnique As Single
        Get
            Return mPeptideUniquenessBinningSettings.MinimumSLiCScore
        End Get
        Set
            mPeptideUniquenessBinningSettings.MinimumSLiCScore = Value
        End Set
    End Property

    Public Property PeptideUniquenessMassBinSizeForBinning As Single
        Get
            Return mPeptideUniquenessBinningSettings.MassBinSizeDa
        End Get
        Set
            If Value > 0 Then
                mPeptideUniquenessBinningSettings.MassBinSizeDa = Value
            End If
        End Set
    End Property

    Public Property OutputFileDelimiter As Char
        Get
            Return mOutputFileDelimiter
        End Get
        Set
            If Not Value = Nothing Then
                mOutputFileDelimiter = Value
            End If
        End Set
    End Property

    Public Property SavePeakMatchingResults As Boolean


    ''' <summary>
    ''' Use Ellipse Search Region
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' Only valid if mUseSLiCScoreForUniqueness = False
    ''' If both mUseSLiCScoreForUniqueness = False and mUseEllipseSearchRegion = False, uses a rectangle to determine uniqueness
    ''' </remarks>
    Public Property UseEllipseSearchRegion As Boolean

    Public Property UseSLiCScoreForUniqueness As Boolean

    Public Sub AddSearchThresholdLevel(massToleranceType As PeakMatching.SearchThresholds.MassToleranceConstants, massTolerance As Double, netTolerance As Double, clearExistingThresholds As Boolean)
        AddSearchThresholdLevel(massToleranceType, massTolerance, netTolerance, True, 0, 0, True, clearExistingThresholds)
    End Sub

    Public Sub AddSearchThresholdLevel(massToleranceType As PeakMatching.SearchThresholds.MassToleranceConstants, massTolerance As Double, netTolerance As Double, autoDefineSLiCScoreThresholds As Boolean, slicScoreMassPPMStDev As Double, slicScoreNETStDev As Double, slicScoreUseAMTNETStDev As Boolean, clearExistingThresholds As Boolean)
        AddSearchThresholdLevel(massToleranceType, massTolerance, netTolerance, True, 0, 0, True, clearExistingThresholds, PeakMatching.DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER)
    End Sub

    Public Sub AddSearchThresholdLevel(massToleranceType As PeakMatching.SearchThresholds.MassToleranceConstants, massTolerance As Double, netTolerance As Double, autoDefineSLiCScoreThresholds As Boolean, slicScoreMassPPMStDev As Double, slicScoreNETStDev As Double, slicScoreUseAMTNETStDev As Boolean, clearExistingThresholds As Boolean, slicScoreMaxSearchDistanceMultiplier As Single)

        If clearExistingThresholds Then
            InitializeThresholdLevels(mThresholdLevels, 1, False)
        Else
            InitializeThresholdLevels(mThresholdLevels, mThresholdLevels.Length + 1, True)
        End If

        Dim index = mThresholdLevels.Length - 1

        mThresholdLevels(index).AutoDefineSLiCScoreThresholds = autoDefineSLiCScoreThresholds

        mThresholdLevels(index).SLiCScoreMaxSearchDistanceMultiplier = slicScoreMaxSearchDistanceMultiplier
        mThresholdLevels(index).MassTolType = massToleranceType
        mThresholdLevels(index).MassTolerance = massTolerance
        mThresholdLevels(index).NETTolerance = netTolerance

        mThresholdLevels(index).SLiCScoreUseAMTNETStDev = slicScoreUseAMTNETStDev

        If Not autoDefineSLiCScoreThresholds Then
            mThresholdLevels(index).SLiCScoreMassPPMStDev = slicScoreMassPPMStDev
            mThresholdLevels(index).SLiCScoreNETStDev = slicScoreNETStDev
        End If
    End Sub

    ''' <summary>
    ''' Add/update a peptide
    ''' </summary>
    ''' <param name="uniqueSeqID"></param>
    ''' <param name="peptideMass"></param>
    ''' <param name="peptideNET"></param>
    ''' <param name="peptideNETStDev"></param>
    ''' <param name="peptideDiscriminantScore"></param>
    ''' <param name="proteinName"></param>
    ''' <param name="cleavageStateInProtein"></param>
    ''' <param name="peptideName"></param>
    ''' <remarks>
    ''' Assures that the peptide is present in mComparisonPeptideInfo and that the protein and protein/peptide mapping is present in mProteinInfo
    ''' Assumes that uniqueSeqID is truly unique for the given peptide
    ''' </remarks>
    Private Sub AddOrUpdatePeptide(
      uniqueSeqID As Integer,
      peptideMass As Double,
      peptideNET As Single,
      peptideNETStDev As Single,
      peptideDiscriminantScore As Single,
      proteinName As String,
      cleavageStateInProtein As ProteinCollection.CleavageStateConstants,
      peptideName As String)

        Dim proteinID As Integer

        mComparisonPeptideInfo.Add(uniqueSeqID, peptideName, peptideMass, peptideNET, peptideNETStDev, peptideDiscriminantScore)

        ' Determine the ProteinID for proteinName
        If proteinName IsNot Nothing AndAlso proteinName.Length > 0 Then
            ' Lookup the index for the given protein
            proteinID = AddOrUpdateProtein(proteinName)
        Else
            proteinID = -1
        End If

        If proteinID >= 0 Then
            ' Add the protein to the peptide to protein mapping table, if necessary
            mProteinInfo.AddProteinToPeptideMapping(proteinID, uniqueSeqID, cleavageStateInProtein)
        End If

    End Sub

    ''' <summary>
    ''' Add/update a protein
    ''' </summary>
    ''' <param name="proteinName"></param>
    ''' <returns>The index of the protein in mProteinInfo, or -1 if not found and unable to add it</returns>
    Private Function AddOrUpdateProtein(proteinName As String) As Integer

        Dim proteinID As Integer

        ' Note: proteinID is auto-assigned
        If Not mProteinInfo.Add(proteinName, proteinID) Then
            proteinID = -1
        End If

        Return proteinID

    End Function

    Private Function ExportPeakMatchingResults(
      thresholds As PeakMatching.SearchThresholds,
      thresholdIndex As Integer,
      comparisonFeatureCount As Integer,
      peptideMatchResults As PeakMatching.PMFeatureMatchResults,
      outputFolderPath As String,
      outputFilenameBase As String,
      pmResultsWriter As StreamWriter) As Boolean

        Dim workingFilenameBase As String
        Dim outputFilePath As String

        Dim linePrefix As String
        Dim lineOut As String

        Dim matchIndex As Integer

        Dim cachedMatchCount As Integer
        Dim cachedMatchCountFeatureID As Integer

        Dim currentFeatureID As Integer
        Dim matchResultInfo As PeakMatching.PMFeatureMatchResults.PeakMatchingResult

        Dim lastFlushTime As DateTime
        Dim success As Boolean

        Try

            ResetProgress("Exporting peak matching results")

            If CreateSeparateOutputFileForEachThreshold Then
                ' Create one file for each search threshold level
                workingFilenameBase = outputFilenameBase & "_PMResults" & (thresholdIndex + 1).ToString() & ".txt"
                outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase)

                pmResultsWriter = New StreamWriter(outputFilePath)

                ' Write the threshold values to the output file
                pmResultsWriter.WriteLine("Comparison feature count: " & comparisonFeatureCount.ToString())
                ExportThresholds(pmResultsWriter, thresholdIndex, thresholds)

                pmResultsWriter.WriteLine()

                ExportPeakMatchingResultsWriteHeaders(pmResultsWriter, CreateSeparateOutputFileForEachThreshold)
            End If


            cachedMatchCount = 0
            cachedMatchCountFeatureID = -1

            lastFlushTime = DateTime.UtcNow

            ' ToDo: Grab chunks of data from the server if caching into SqlServer (change this to a while loop)
            For matchIndex = 0 To peptideMatchResults.Count - 1

                If CreateSeparateOutputFileForEachThreshold Then
                    linePrefix = String.Empty
                Else
                    linePrefix = (thresholdIndex + 1).ToString() & mOutputFileDelimiter
                End If

                If peptideMatchResults.GetMatchInfoByRowIndex(matchIndex, currentFeatureID, matchResultInfo) Then

                    If currentFeatureID <> cachedMatchCountFeatureID Then
                        cachedMatchCount = peptideMatchResults.MatchCountForFeatureID(currentFeatureID)
                        cachedMatchCountFeatureID = currentFeatureID
                    End If


                    lineOut = linePrefix &
                                currentFeatureID.ToString() & mOutputFileDelimiter &
                                cachedMatchCount.ToString() & mOutputFileDelimiter &
                                matchResultInfo.MultiAMTHitCount.ToString() & mOutputFileDelimiter &
                                matchResultInfo.MatchingID.ToString() & mOutputFileDelimiter &
                                Math.Round(matchResultInfo.MassErr, 6).ToString() & mOutputFileDelimiter &
                                Math.Round(matchResultInfo.NETErr, 4).ToString() & mOutputFileDelimiter &
                                Math.Round(matchResultInfo.SLiCScore, 4).ToString() & mOutputFileDelimiter &
                                Math.Round(matchResultInfo.DelSLiC, 4).ToString()
                    pmResultsWriter.WriteLine(lineOut)

                End If

                If matchIndex Mod 100 = 0 Then
                    UpdateProgress(CSng(matchIndex / peptideMatchResults.Count * 100))

                    If DateTime.UtcNow.Subtract(lastFlushTime).TotalSeconds > 30 Then
                        pmResultsWriter.Flush()
                    End If

                End If

            Next matchIndex

            If CreateSeparateOutputFileForEachThreshold Then
                pmResultsWriter.Close()
            End If

            UpdateProgress(100)

            success = True

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Private Sub ExportPeakMatchingResultsWriteHeaders(pmResultsWriter As TextWriter, createSeparateOutputFiles As Boolean)

        Dim lineOut As String

        ' Write the column headers
        If createSeparateOutputFiles Then
            lineOut = String.Empty
        Else
            lineOut = "Threshold_Index" & mOutputFileDelimiter
        End If

        lineOut &= "Unique_Sequence_ID" &
                    mOutputFileDelimiter & "Match_Count" &
                    mOutputFileDelimiter & "Multi_AMT_Hit_Count" &
                    mOutputFileDelimiter & "Matching_ID_Index" &
                    mOutputFileDelimiter & "Mass_Err"
        lineOut &= mOutputFileDelimiter & "NET_Err"

        lineOut &= mOutputFileDelimiter & "SLiC_Score" &
                    mOutputFileDelimiter & "Del_SLiC"
        pmResultsWriter.WriteLine(lineOut)

    End Sub

    Private Function ExportPeptideUniquenessResults(thresholdIndex As Integer, ByRef binResults As BinnedPeptideCountStats, peptideUniquenessWriter As TextWriter) As Boolean

        Dim lineOut As String
        Dim binIndex As Integer
        Dim index As Integer

        Dim success As Boolean

        Try

            For binIndex = 0 To binResults.BinCount - 1
                If CreateSeparateOutputFileForEachThreshold Then
                    lineOut = String.Empty
                Else
                    lineOut = (thresholdIndex + 1).ToString() & mOutputFileDelimiter
                End If

                Dim peptideCountTotal = binResults.Bins(binIndex).NonUniqueResultIDCount + binResults.Bins(binIndex).UniqueResultIDCount

                lineOut &= Math.Round(binResults.Bins(binIndex).MassBinStart, 2).ToString() & mOutputFileDelimiter &
                           Math.Round(binResults.Bins(binIndex).PercentUnique, 3).ToString() & mOutputFileDelimiter &
                           peptideCountTotal.ToString()

                For index = 1 To Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave)
                    lineOut &= mOutputFileDelimiter & binResults.Bins(binIndex).ResultIDCountDistribution(index).ToString()
                Next index

                peptideUniquenessWriter.WriteLine(lineOut)

            Next binIndex

            peptideUniquenessWriter.Flush()
            success = True

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Private Function ExportProteinStats(
      thresholdIndex As Integer,
      proteinStatsWriter As TextWriter) As Boolean

        Try

            Dim lastFlushTime = DateTime.UtcNow
            For proteinIndex = 0 To mProteinInfo.Count - 1

                Dim lineOut As String
                If CreateSeparateOutputFileForEachThreshold Then
                    lineOut = String.Empty
                Else
                    lineOut = (thresholdIndex + 1).ToString() & mOutputFileDelimiter
                End If

                Dim proteinID As Integer
                Dim proteinName As String = String.Empty

                If mProteinInfo.GetProteinInfoByRowIndex(proteinIndex, proteinID, proteinName) Then
                    lineOut &= proteinName &
                                mOutputFileDelimiter & proteinID.ToString() &
                                mOutputFileDelimiter & mProteinInfo.GetPeptideCountForProteinByID(proteinID).ToString() &
                                mOutputFileDelimiter & GetPeptidesUniquelyIdentifiedCountByProteinID(proteinID).ToString()

                    proteinStatsWriter.WriteLine(lineOut)

                End If

                If proteinIndex Mod 100 = 0 AndAlso DateTime.UtcNow.Subtract(lastFlushTime).TotalSeconds > 30 Then
                    proteinStatsWriter.Flush()
                End If
            Next proteinIndex

            proteinStatsWriter.Flush()
            Return True

        Catch ex As Exception
            Return False
        End Try

    End Function

    Private Function ExtractServerFromConnectionString(connectionString As String) As String
        Const DATA_SOURCE_TEXT = "data source"

        Dim charLoc, charLoc2 As Integer
        Dim serverName As String = String.Empty

        Try
            charLoc = connectionString.IndexOf(DATA_SOURCE_TEXT, StringComparison.OrdinalIgnoreCase)
            If charLoc >= 0 Then
                charLoc += DATA_SOURCE_TEXT.Length
                charLoc2 = connectionString.ToLower.IndexOf(";"c, charLoc + 1)

                If charLoc2 <= charLoc Then
                    charLoc2 = connectionString.Length - 1
                End If

                serverName = connectionString.Substring(charLoc + 1, charLoc2 - charLoc - 1)
            End If
        Catch ex As Exception
        End Try

        Return serverName

    End Function

    Private Function FeatureContainsUniqueMatch(
      featureInfo As PeakMatching.FeatureInfo,
      peptideMatchResults As PeakMatching.PMFeatureMatchResults,
      ByRef matchCount As Integer,
      usingSLiCScoreForUniqueness As Boolean,
      minimumSLiCScore As Single) As Boolean

        Dim uniqueMatch As Boolean
        Dim matchResults() As PeakMatching.PMFeatureMatchResults.PeakMatchingResult = Nothing

        Dim matchIndex As Integer

        uniqueMatch = False

        If peptideMatchResults.GetMatchInfoByFeatureID(featureInfo.FeatureID, matchResults, matchCount) Then
            If matchCount > 0 Then
                ' The current feature has 1 or more matches

                If usingSLiCScoreForUniqueness Then
                    ' See if any of the matches have a SLiC Score >= the minimum SLiC Score
                    uniqueMatch = False
                    For matchIndex = 0 To matchCount - 1
                        If Math.Round(matchResults(matchIndex).SLiCScore, 4) >= Math.Round(minimumSLiCScore, 4) Then
                            uniqueMatch = True
                            Exit For
                        End If
                    Next matchIndex
                Else
                    ' Not using SLiC score; when we performed the peak matching, we used an ellipse or rectangle to find matching features
                    ' This feature is unique if it only has one match
                    If matchCount = 1 Then
                        uniqueMatch = True
                    Else
                        uniqueMatch = False
                    End If
                End If

            End If
        Else
            matchCount = 0
        End If

        Return uniqueMatch

    End Function

    Private Sub ExportThresholds(
      writer As TextWriter,
      thresholdIndex As Integer,
      searchThresholds As PeakMatching.SearchThresholds)

        Dim delimiter As String
        Dim lineOut As String

        delimiter = "; "

        ' Write the thresholds
        lineOut = "Threshold Index: " & (thresholdIndex + 1).ToString()
        lineOut &= delimiter & "Mass Tolerance: +- "
        Select Case searchThresholds.MassTolType
            Case PeakMatching.SearchThresholds.MassToleranceConstants.Absolute
                lineOut &= Math.Round(searchThresholds.MassTolerance, 5).ToString() & " Da"
            Case PeakMatching.SearchThresholds.MassToleranceConstants.PPM
                lineOut &= Math.Round(searchThresholds.MassTolerance, 2).ToString() & " ppm"
            Case Else
                lineOut &= "Unknown mass tolerance mode"
        End Select

        lineOut &= delimiter & "NET Tolerance: +- " & Math.Round(searchThresholds.NETTolerance, 4).ToString()

        If UseSLiCScoreForUniqueness Then
            lineOut &= delimiter &
                       "Minimum SLiC Score: " & Math.Round(mPeptideUniquenessBinningSettings.MinimumSLiCScore, 3).ToString() & "; " &
                       "Max search distance multiplier: " & Math.Round(searchThresholds.SLiCScoreMaxSearchDistanceMultiplier, 1).ToString()
        Else
            If UseEllipseSearchRegion Then
                lineOut &= delimiter & "Minimum SLiC Score: N/A; using ellipse to find matching features"
            Else
                lineOut &= delimiter & "Minimum SLiC Score: N/A; using rectangle to find matching features"
            End If
        End If

        writer.WriteLine(lineOut)

    End Sub

    Public Function GenerateUniquenessStats(proteinInputFilePath As String, outputFolderPath As String, outputFilenameBase As String) As Boolean

        Dim progressStepCount As Integer
        Dim progressStep As Integer

        Dim thresholdIndex As Integer

        Dim featuresToIdentify As PeakMatching.PMFeatureInfo

        Dim success As Boolean
        Dim searchAborted = False

        Dim rangeSearch As SearchRange = Nothing

        Dim pmResultsWriter As StreamWriter = Nothing
        Dim peptideUniquenessWriter As StreamWriter = Nothing
        Dim proteinStatsWriter As StreamWriter = Nothing

        Try

            If Path.HasExtension(outputFilenameBase) Then
                ' Remove the extension
                outputFilenameBase = Path.ChangeExtension(outputFilenameBase, Nothing)
            End If

            '----------------------------------------------------
            ' Initialize the progress bar
            '----------------------------------------------------
            progressStep = 0
            progressStepCount = 1 + mThresholdLevels.Length * 3

            MyBase.LogMessage("Caching data in memory", MessageTypeConstants.Normal)
            MyBase.LogMessage("Caching peak matching results in memory", MessageTypeConstants.Normal)

            '----------------------------------------------------
            ' Load the proteins and digest them, or simply load the peptides
            '----------------------------------------------------
            MyBase.LogMessage("Load proteins or peptides from " & Path.GetFileName(proteinInputFilePath), MessageTypeConstants.Normal)
            success = LoadProteinsOrPeptides(proteinInputFilePath)

            ' ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True

            progressStep = 1
            Me.UpdateProgress(CSng(progressStep / progressStepCount * 100))

        Catch ex As Exception
            SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile, ex)
            success = False
        End Try

        If success AndAlso mComparisonPeptideInfo.Count = 0 Then
            SetLocalErrorCode(ErrorCodes.ProteinsNotFoundInInputFile, True)
            success = False
        ElseIf Not success OrElse Me.AbortProcessing Then
            success = False
        Else
            Try
                '----------------------------------------------------
                ' Search mComparisonPeptideInfo against itself
                ' Since mComparisonPeptideInfo is class type PMComparisonFeatureInfo, which is a
                '  derived class of PMFeatureInfo, we can simply link the two objects
                ' This way, we don't use double the memory necessary
                '----------------------------------------------------

                UpdateProgress("Initializing structures")

                ' Initialize featuresToIdentify by linking to mComparisonPeptideInfo
                featuresToIdentify = mComparisonPeptideInfo

                Try
                    '----------------------------------------------------
                    ' Compare featuresToIdentify to mComparisonPeptideInfo for each threshold level
                    '----------------------------------------------------

                    '----------------------------------------------------
                    ' Validate mThresholdLevels to assure at least one threshold exists
                    '----------------------------------------------------

                    If mThresholdLevels.Length <= 0 Then
                        InitializeThresholdLevels(mThresholdLevels, 1, False)
                    End If

                    For thresholdIndex = 0 To mThresholdLevels.Length - 1
                        If mThresholdLevels(thresholdIndex) Is Nothing Then
                            If thresholdIndex = 0 Then
                                ' Need at least one set of thresholds
                                mThresholdLevels(thresholdIndex) = New PeakMatching.SearchThresholds
                            Else
                                ReDim Preserve mThresholdLevels(thresholdIndex - 1)
                                Exit For
                            End If
                        End If
                    Next

                    '----------------------------------------------------
                    ' Initialize rangeSearch
                    '----------------------------------------------------

                    MyBase.LogMessage("Uniqueness Stats processing starting, Threshold Count = " & mThresholdLevels.Length.ToString(), MessageTypeConstants.Normal)

                    If Not PeakMatching.FillRangeSearchObject(rangeSearch, mComparisonPeptideInfo) Then
                        success = False
                    Else

                        '----------------------------------------------------
                        ' Initialize the peak matching class
                        '----------------------------------------------------

                        mPeakMatching = New PeakMatching() With {
                            .MaxPeakMatchingResultsPerFeatureToSave = mMaxPeakMatchingResultsPerFeatureToSave,
                            .UseMaxSearchDistanceMultiplierAndSLiCScore = UseSLiCScoreForUniqueness,
                            .UseEllipseSearchRegion = UseEllipseSearchRegion
                        }

                        '----------------------------------------------------
                        ' Initialize the output files if combining all results
                        ' in a single file for each type of result
                        '----------------------------------------------------

                        If Not CreateSeparateOutputFileForEachThreshold Then
                            InitializePeptideAndProteinResultsFiles(outputFolderPath, outputFilenameBase, mThresholdLevels, peptideUniquenessWriter, proteinStatsWriter)

                            If SavePeakMatchingResults Then
                                InitializePMResultsFile(outputFolderPath, outputFilenameBase, mThresholdLevels, mComparisonPeptideInfo.Count, pmResultsWriter)
                            End If
                        End If

                        For thresholdIndex = 0 To mThresholdLevels.Length - 1

                            UpdateProgress("Generating uniqueness stats, threshold " & (thresholdIndex + 1).ToString() & " / " & mThresholdLevels.Length.ToString())

                            UpdateSubtaskProgress("Finding matching peptides for given search thresholds", 0)

                            ' Perform the actual peak matching
                            MyBase.LogMessage("Threshold " & (thresholdIndex + 1).ToString() & ", IdentifySequences", MessageTypeConstants.Normal)
                            success = mPeakMatching.IdentifySequences(mThresholdLevels(thresholdIndex), featuresToIdentify, mComparisonPeptideInfo, mPeptideMatchResults, rangeSearch)
                            If Not success Then Exit For

                            If SavePeakMatchingResults Then
                                ' Write out the raw peak matching results
                                success = ExportPeakMatchingResults(mThresholdLevels(thresholdIndex), thresholdIndex, mComparisonPeptideInfo.Count, mPeptideMatchResults, outputFolderPath, outputFilenameBase, pmResultsWriter)
                                If Not success Then Exit For
                            End If

                            progressStep += 1
                            UpdateProgress(CSng(progressStep / progressStepCount * 100))

                            ' Summarize the results by peptide
                            MyBase.LogMessage("Threshold " & (thresholdIndex + 1).ToString() & ", SummarizeResultsByPeptide", MessageTypeConstants.Normal)
                            success = SummarizeResultsByPeptide(mThresholdLevels(thresholdIndex), thresholdIndex, featuresToIdentify, mPeptideMatchResults, outputFolderPath, outputFilenameBase, peptideUniquenessWriter)
                            If Not success Then Exit For

                            progressStep += 1
                            UpdateProgress(CSng(progressStep / progressStepCount * 100))

                            ' Summarize the results by protein
                            MyBase.LogMessage("Threshold " & (thresholdIndex + 1).ToString() & ", SummarizeResultsByProtein", MessageTypeConstants.Normal)
                            success = SummarizeResultsByProtein(mThresholdLevels(thresholdIndex), thresholdIndex, featuresToIdentify, mPeptideMatchResults, outputFolderPath, outputFilenameBase, proteinStatsWriter)
                            If Not success Then Exit For

                            progressStep += 1
                            UpdateProgress(CSng(progressStep / progressStepCount * 100))

                            If Not success OrElse Me.AbortProcessing Then
                                success = False
                                Exit For
                            End If

                            success = True
                        Next thresholdIndex

                    End If

                Catch ex As Exception
                    SetLocalErrorCode(ErrorCodes.ErrorIdentifyingSequences, ex)
                    success = False
                End Try

                UpdateProgress(100)

            Catch ex As Exception
                SetLocalErrorCode(ErrorCodes.ErrorIdentifyingSequences, ex)
                success = False
            Finally
                Try
                    If pmResultsWriter IsNot Nothing Then pmResultsWriter.Close()
                    If peptideUniquenessWriter IsNot Nothing Then peptideUniquenessWriter.Close()
                    If proteinStatsWriter IsNot Nothing Then proteinStatsWriter.Close()
                    mPeakMatching = Nothing
                Catch ex As Exception
                    ' Ignore any errors closing files
                End Try
            End Try

        End If

        If Not searchAborted AndAlso success Then
            MyBase.LogMessage("Uniqueness Stats processing complete", MessageTypeConstants.Normal)
        End If

        Return success

    End Function

    Public Overrides Function GetDefaultExtensionsToParse() As IList(Of String)
        Dim extensionsToParse = New List(Of String) From {
            ".fasta",
            ".txt"
        }

        Return extensionsToParse

    End Function

    Public Overrides Function GetErrorMessage() As String
        ' Returns "" if no error

        Dim errorMessage As String

        If MyBase.ErrorCode = ProcessFilesErrorCodes.LocalizedError OrElse
           MyBase.ErrorCode = ProcessFilesErrorCodes.NoError Then
            Select Case mLocalErrorCode
                Case ErrorCodes.NoError
                    errorMessage = ""

                Case ErrorCodes.ProteinDigestionSimulatorSectionNotFound
                    errorMessage = "The section " & ProteinFileParser.XML_SECTION_OPTIONS & " was not found in the parameter file"

                Case ErrorCodes.ErrorReadingInputFile
                    errorMessage = "Error reading input file"
                Case ErrorCodes.ProteinsNotFoundInInputFile
                    errorMessage = "No proteins were found in the input file (make sure the Column Order is correct on the File Format Options tab)"

                Case ErrorCodes.ErrorIdentifyingSequences
                    errorMessage = "Error identifying sequences"

                Case ErrorCodes.ErrorWritingOutputFile
                    errorMessage = "Error writing to one of the output files"
                Case ErrorCodes.UserAbortedSearch
                    errorMessage = "User aborted search"

                Case ErrorCodes.UnspecifiedError
                    errorMessage = "Unspecified localized error"
                Case Else
                    ' This shouldn't happen
                    errorMessage = "Unknown error state"
            End Select
        Else
            errorMessage = MyBase.GetBaseClassErrorMessage()
        End If

        If mLastErrorMessage IsNot Nothing AndAlso mLastErrorMessage.Length > 0 Then
            errorMessage &= ControlChars.NewLine & mLastErrorMessage
        End If

        Return errorMessage

    End Function

    Private Function GetNextUniqueSequenceID(sequence As String, masterSequences As Hashtable, ByRef nextUniqueIDForMasterSeqs As Integer) As Integer
        Dim uniqueSeqID As Integer

        Try
            If masterSequences.ContainsKey(sequence) Then
                uniqueSeqID = CInt(masterSequences(sequence))
            Else
                masterSequences.Add(sequence, nextUniqueIDForMasterSeqs)
                uniqueSeqID = nextUniqueIDForMasterSeqs
            End If
            nextUniqueIDForMasterSeqs += 1
        Catch ex As Exception
            uniqueSeqID = 0
        End Try

        Return uniqueSeqID
    End Function

    Public Sub GetPeptideUniquenessMassRangeForBinning(<Out> ByRef massMinimum As Single, <Out> ByRef massMaximum As Single)
        massMinimum = mPeptideUniquenessBinningSettings.MassMinimum
        massMaximum = mPeptideUniquenessBinningSettings.MassMaximum
    End Sub

    Private Function GetPeptidesUniquelyIdentifiedCountByProteinID(proteinID As Integer) As Integer
        Dim dataRows() As DataRow

        dataRows = mProteinToIdentifiedPeptideMappingTable.Select(PROTEIN_ID_COLUMN & " = " & proteinID.ToString())
        If dataRows IsNot Nothing Then
            Return dataRows.Length
        Else
            Return 0
        End If

    End Function

    Private Sub InitializeBinnedStats(ByRef statsBinned As BinnedPeptideCountStats, binCount As Integer)

        Dim binIndex As Integer

        statsBinned.BinCount = binCount
        ReDim statsBinned.Bins(statsBinned.BinCount - 1)

        For binIndex = 0 To statsBinned.BinCount - 1
            statsBinned.Bins(binIndex).MassBinStart = statsBinned.Settings.MassMinimum + statsBinned.Settings.MassBinSizeDa * binIndex
            statsBinned.Bins(binIndex).MassBinEnd = statsBinned.Settings.MassMinimum + statsBinned.Settings.MassBinSizeDa * (binIndex + 1)

            statsBinned.Bins(binIndex).PercentUnique = 0
            statsBinned.Bins(binIndex).UniqueResultIDCount = 0
            statsBinned.Bins(binIndex).NonUniqueResultIDCount = 0
            ReDim statsBinned.Bins(binIndex).ResultIDCountDistribution(Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave))
        Next binIndex

    End Sub

    Private Sub InitializePeptideAndProteinResultsFiles(
      outputFolderPath As String,
      outputFilenameBase As String,
      thresholds As IList(Of PeakMatching.SearchThresholds),
      <Out> ByRef peptideUniquenessWriter As StreamWriter,
      <Out> ByRef proteinStatsWriter As StreamWriter)

        ' Initialize the output file so that the peptide and protein summary results for all thresholds can be saved in the same file

        Dim workingFilenameBase As String
        Dim outputFilePath As String
        Dim thresholdIndex As Integer

        '----------------------------------------------------
        ' Create the peptide uniqueness stats files
        '----------------------------------------------------

        workingFilenameBase = outputFilenameBase & "_PeptideStatsBinned.txt"
        outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase)

        peptideUniquenessWriter = New StreamWriter(outputFilePath)

        '----------------------------------------------------
        ' Create the protein stats file
        '----------------------------------------------------

        workingFilenameBase = outputFilenameBase & "_ProteinStats.txt"
        outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase)

        proteinStatsWriter = New StreamWriter(outputFilePath)

        '----------------------------------------------------
        ' Write the thresholds
        '----------------------------------------------------

        For thresholdIndex = 0 To thresholds.Count - 1
            ExportThresholds(peptideUniquenessWriter, thresholdIndex, thresholds(thresholdIndex))
            ExportThresholds(proteinStatsWriter, thresholdIndex, thresholds(thresholdIndex))
        Next thresholdIndex

        peptideUniquenessWriter.WriteLine()
        SummarizeResultsByPeptideWriteHeaders(peptideUniquenessWriter, CreateSeparateOutputFileForEachThreshold)

        proteinStatsWriter.WriteLine()
        SummarizeResultsByProteinWriteHeaders(proteinStatsWriter, CreateSeparateOutputFileForEachThreshold)

    End Sub

    Private Sub InitializePMResultsFile(
      outputFolderPath As String,
      outputFilenameBase As String,
      thresholds As IList(Of PeakMatching.SearchThresholds),
      comparisonFeaturesCount As Integer,
      <Out> ByRef pmResultsWriter As StreamWriter)

        ' Initialize the output file so that the peak matching results for all thresholds can be saved in the same file

        Dim workingFilenameBase As String
        Dim outputFilePath As String
        Dim thresholdIndex As Integer

        ' Combine all of the data together in one output file
        workingFilenameBase = outputFilenameBase & "_PMResults.txt"
        outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase)

        pmResultsWriter = New StreamWriter(outputFilePath)

        pmResultsWriter.WriteLine("Comparison feature count: " & comparisonFeaturesCount.ToString())

        For thresholdIndex = 0 To thresholds.Count - 1
            ExportThresholds(pmResultsWriter, thresholdIndex, thresholds(thresholdIndex))
        Next thresholdIndex

        pmResultsWriter.WriteLine()

        ExportPeakMatchingResultsWriteHeaders(pmResultsWriter, CreateSeparateOutputFileForEachThreshold)

    End Sub

    Private Sub InitializeLocalVariables()

        DigestSequences = False
        CysPeptidesOnly = False
        mOutputFileDelimiter = ControlChars.Tab

        SavePeakMatchingResults = False
        mMaxPeakMatchingResultsPerFeatureToSave = 3

        mPeptideUniquenessBinningSettings.AutoDetermineMassRange = True
        mPeptideUniquenessBinningSettings.MassBinSizeDa = 25
        mPeptideUniquenessBinningSettings.MassMinimum = 400
        mPeptideUniquenessBinningSettings.MassMaximum = 6000
        mPeptideUniquenessBinningSettings.MinimumSLiCScore = 0.99

        UseSLiCScoreForUniqueness = True
        UseEllipseSearchRegion = True

        CreateSeparateOutputFileForEachThreshold = False

        mLocalErrorCode = ErrorCodes.NoError
        mLastErrorMessage = String.Empty

        InitializeProteinToIdentifiedPeptideMappingTable()

        InitializeThresholdLevels(mThresholdLevels, 1, False)
        InitializeProteinFileParser()

        ''mUseSqlServerDBToCacheData = False
        ''mUseSqlServerForMatchResults = False

        ''mUseBulkInsert = False

        ''mSqlServerConnectionString = DBUtils.DEFAULT_CONNECTION_STRING_NO_PROVIDER
        ''mTableNameFeaturesToIdentify = PeakMatching.PMFeatureInfo.DEFAULT_FEATURE_INFO_TABLE_NAME
        ''mTableNameComparisonPeptides = PeakMatching.PMComparisonFeatureInfo.DEFAULT_COMPARISON_FEATURE_INFO_TABLE_NAME

        ''mTableNameProteinInfo = ProteinInfoCollection.DEFAULT_PROTEIN_INFO_TABLE_NAME
        ''mTableNameProteinToPeptideMap = ProteinInfoCollection.DEFAULT_PROTEIN_TO_PEPTIDE_MAP_TABLE_NAME

    End Sub

    Private Sub InitializeProteinToIdentifiedPeptideMappingTable()

        If mProteinToIdentifiedPeptideMappingTable Is Nothing Then
            mProteinToIdentifiedPeptideMappingTable = New DataTable

            '---------------------
            ' Protein stats uniquely identified peptides table
            '---------------------
            DBUtils.AppendColumnIntegerToTable(mProteinToIdentifiedPeptideMappingTable, PROTEIN_ID_COLUMN)
            DBUtils.AppendColumnStringToTable(mProteinToIdentifiedPeptideMappingTable, PEPTIDE_ID_MATCH_COLUMN)

            ' Define the PROTEIN_ID_COLUMN AND PEPTIDE_ID_COLUMN columns to be the primary key
            mProteinToIdentifiedPeptideMappingTable.PrimaryKey = New DataColumn() {
                mProteinToIdentifiedPeptideMappingTable.Columns(PROTEIN_ID_COLUMN),
                mProteinToIdentifiedPeptideMappingTable.Columns(PEPTIDE_ID_MATCH_COLUMN)
            }

        Else
            mProteinToIdentifiedPeptideMappingTable.Clear()
        End If

    End Sub

    '' The following was created to test the speed and performance when dealing with large dataset tables
    ''Private Sub FillWithLotsOfData(ByRef targetDataset as System.Data.DataSet)
    ''    Const MAX_FEATURE_COUNT As Integer = 300000

    ''    Dim randomGenerator As New Random
    ''    Dim newFeatureID As Integer
    ''    Dim index As Integer
    ''    Dim indexEnd As Integer

    ''    Dim newRow as System.Data.DataRow
    ''    Dim progressForm As New ProgressFormNET.frmProgress
    ''    Dim startTime As DateTime

    ''    indexEnd = CInt(MAX_FEATURE_COUNT * 1.5)
    ''    progressForm.InitializeProgressForm("Populating dataset table", 0, indexEnd, True)
    ''    progressForm.Visible = True
    ''    Application.DoEvents()

    ''    startTime = System.DateTime.UtcNow

    ''    For index = 0 To indexEnd
    ''        newFeatureID = randomGenerator.Next(0, MAX_FEATURE_COUNT)

    ''        ' Look for existing entry in table
    ''        If Not dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME).Rows.Contains(newFeatureID) Then
    ''            newRow = dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME).NewRow
    ''            newRow(COMPARISON_FEATURE_ID_COLUMN) = newFeatureID
    ''            newRow(FEATURE_NAME_COLUMN) = "Feature" & newFeatureID.ToString()
    ''            newRow(MASS_COLUMN) = newFeatureID / CSng(MAX_FEATURE_COUNT) * 1000
    ''            newRow(NET_COLUMN) = randomGenerator.Next(0, 1000) / 1000.0
    ''            newRow(NET_STDEV_COLUMN) = randomGenerator.Next(0, 1000) / 10000.0
    ''            newRow(DISCRIMINANT_SCORE_COLUMN) = randomGenerator.Next(0, 1000) / 1000.0
    ''            dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME).Rows.Add(newRow)
    ''        End If

    ''        If index Mod 100 = 0 Then
    ''            progressForm.UpdateProgressBar(index)
    ''            Application.DoEvents()

    ''            If progressForm.KeyPressAbortProcess Then Exit For
    ''        End If
    ''    Next index

    ''    MessageBox.Show("Elapsed time: " & Math.Round(System.DateTime.UtcNow.Subtract(startTime).TotalSeconds, 2).ToString() & " seconds", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
    ''End Sub

    Public Sub InitializeProteinFileParser(Optional resetToDefaults As Boolean = False)

        If mProteinFileParser Is Nothing Then
            mProteinFileParser = New ProteinFileParser
            resetToDefaults = True
        End If

        If resetToDefaults Then
            mProteinFileParser.ComputeProteinMass = True
            mProteinFileParser.ElementMassMode = PeptideSequence.ElementModeConstants.IsotopicMass

            mProteinFileParser.CreateDigestedProteinOutputFile = False
            mProteinFileParser.CreateProteinOutputFile = False

            mProteinFileParser.DelimitedFileFormatCode = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence

            mProteinFileParser.DigestionOptions.CleavageRuleID = InSilicoDigest.CleavageRuleConstants.ConventionalTrypsin
            mProteinFileParser.DigestionOptions.MaxMissedCleavages = 1

            mProteinFileParser.DigestionOptions.MinFragmentResidueCount = 0
            mProteinFileParser.DigestionOptions.MinFragmentMass = 600
            mProteinFileParser.DigestionOptions.MaxFragmentMass = 3000

            mProteinFileParser.DigestionOptions.RemoveDuplicateSequences = True

            mProteinFileParser.DigestionOptions.IncludePrefixAndSuffixResidues = False
        End If

    End Sub

    Private Sub InitializeThresholdLevels(ByRef thresholds() As PeakMatching.SearchThresholds, levels As Integer, preserveData As Boolean)
        Dim index As Integer

        If levels < 1 Then levels = 1

        If preserveData AndAlso thresholds.Length > 0 Then
            ReDim Preserve thresholds(levels - 1)
        Else
            ReDim thresholds(levels - 1)
        End If

        For index = 0 To levels - 1
            If thresholds(index) Is Nothing Then
                thresholds(index) = New PeakMatching.SearchThresholds
            ElseIf preserveData AndAlso index = levels - 1 Then
                ' Initialize this level to defaults
                thresholds(index).ResetToDefaults()
            End If
        Next

    End Sub

    Private Function LoadParameterFileSettings(parameterFilePath As String) As Boolean

        Try

            If parameterFilePath Is Nothing OrElse parameterFilePath.Length = 0 Then
                ' No parameter file specified; nothing to load
                Return True
            End If

            If Not File.Exists(parameterFilePath) Then
                ' See if parameterFilePath points to a file in the same directory as the application
                parameterFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(parameterFilePath))
                If Not File.Exists(parameterFilePath) Then
                    MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.ParameterFileNotFound)
                    Return False
                End If
            End If

            mProteinFileParser.LoadParameterFileSettings(parameterFilePath)

        Catch ex As Exception
            Throw New Exception("Error in LoadParameterFileSettings", ex)
        End Try

        Return True

    End Function

    Private Function LoadProteinsOrPeptides(proteinInputFilePath As String) As Boolean

        Dim delimitedFileFormatCode As DelimitedProteinFileReader.ProteinFileFormatCode

        Dim success As Boolean
        Dim digestionEnabled As Boolean

        Try
            If mComparisonPeptideInfo IsNot Nothing Then
                mComparisonPeptideInfo = Nothing
            End If

            mComparisonPeptideInfo = New PeakMatching.PMComparisonFeatureInfo

            If mProteinInfo IsNot Nothing Then
                mProteinInfo = Nothing
            End If

            mProteinInfo = New ProteinCollection

            ' Possibly initialize the ProteinFileParser object
            If mProteinFileParser Is Nothing Then
                InitializeProteinFileParser()
            End If

            ' Note that ProteinFileParser is exposed as public so that options can be set directly in it

            If ProteinFileParser.IsFastaFile(proteinInputFilePath) OrElse mProteinFileParser.AssumeFastaFile Then
                MyBase.LogMessage("Input file format = FASTA", MessageTypeConstants.Normal)
                digestionEnabled = True
            Else
                delimitedFileFormatCode = mProteinFileParser.DelimitedFileFormatCode
                MyBase.LogMessage("Input file format = " & delimitedFileFormatCode.ToString(), MessageTypeConstants.Normal)
                digestionEnabled = Me.DigestSequences
            End If

            Select Case delimitedFileFormatCode
                Case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID,
                     DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence
                    ' Reading peptides from a delimited file; digestionEnabled is typically False, but could be true
                Case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET,
                     DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore,
                     DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET
                    ' Reading peptides from a delimited file; digestionEnabled is typically False, but could be true
                Case Else
                    ' Force digest Sequences to true
                    digestionEnabled = True
            End Select

            MyBase.LogMessage("Digest sequences = " & digestionEnabled.ToString(), MessageTypeConstants.Normal)

            If digestionEnabled Then
                success = LoadProteinsOrPeptidesWork(proteinInputFilePath)
            Else
                ' We can assume peptides in the input file are already digested and thus do not need to pre-cache the entire file in memory
                ' Load the data directly using this class instead

                ' Load each peptide using ProteinFileReader.DelimitedFileReader
                success = LoadPeptidesFromDelimitedFile(proteinInputFilePath)
                ' ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True
            End If

            If success Then
                MyBase.LogMessage("Loaded " & mComparisonPeptideInfo.Count & " peptides corresponding to " & mProteinInfo.Count & " proteins", MessageTypeConstants.Normal)
            End If

        Catch ex As Exception
            SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile, ex)
            success = False
        End Try

        Return success
    End Function

    Private Function LoadPeptidesFromDelimitedFile(proteinInputFilePath As String) As Boolean

        ' Assumes the peptides in the input file are already digested and that they already have unique ID values generated
        ' They could optionally have Mass and NET values defined

        Dim delimitedFileReader As DelimitedProteinFileReader = Nothing

        Dim newPeptide As InSilicoDigest.PeptideSequenceWithNET

        Dim delimitedFileHasMassAndNET As Boolean
        Dim success As Boolean
        Dim inputPeptideFound As Boolean

        Dim inputFileLinesRead As Integer
        Dim inputFileLineSkipCount As Integer
        Dim skipMessage As String

        Try
            delimitedFileReader = New DelimitedProteinFileReader() With {
                .Delimiter = mProteinFileParser.InputFileDelimiter,
                .DelimitedFileFormatCode = mProteinFileParser.DelimitedFileFormatCode
            }

            ' Verify that the input file exists
            If Not File.Exists(proteinInputFilePath) Then
                MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath)
                success = False
                Exit Try
            End If

            ' Attempt to open the input file
            If Not delimitedFileReader.OpenFile(proteinInputFilePath) Then
                SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile)
                success = False
                Exit Try
            End If

            ResetProgress("Parsing digested input file")

            ' Read each peptide in the input file and add to mComparisonPeptideInfo

            ' Also need to initialize newPeptide
            newPeptide = New InSilicoDigest.PeptideSequenceWithNET

            Select Case delimitedFileReader.DelimitedFileFormatCode
                Case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET,
                     DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore,
                     DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET
                    ' Do not use the computed mass and NET
                    delimitedFileHasMassAndNET = True
                Case Else
                    delimitedFileHasMassAndNET = False
            End Select

            ' Always auto compute the NET and Mass in the newPeptide class
            ' However, if delimitedFileHasMassAndNET = True and valid Mass and NET values were read from the text file,
            '  they are passed to AddOrUpdatePeptide rather than the computed values
            newPeptide.AutoComputeNET = True

            Do
                inputPeptideFound = delimitedFileReader.ReadNextProteinEntry()

                inputFileLinesRead = delimitedFileReader.LinesRead
                inputFileLineSkipCount += delimitedFileReader.LineSkipCount

                If inputPeptideFound Then

                    newPeptide.SequenceOneLetter = delimitedFileReader.ProteinSequence

                    If Not delimitedFileHasMassAndNET OrElse
                          (Math.Abs(delimitedFileReader.PeptideMass - 0) < Single.Epsilon AndAlso
                           Math.Abs(delimitedFileReader.PeptideNET - 0) < Single.Epsilon) Then
                        AddOrUpdatePeptide(delimitedFileReader.EntryUniqueID, newPeptide.Mass, newPeptide.NET, 0, 0, delimitedFileReader.ProteinName, ProteinCollection.CleavageStateConstants.Unknown, String.Empty)
                    Else
                        AddOrUpdatePeptide(delimitedFileReader.EntryUniqueID, delimitedFileReader.PeptideMass, delimitedFileReader.PeptideNET,
                                           delimitedFileReader.PeptideNETStDev, delimitedFileReader.PeptideDiscriminantScore,
                                           delimitedFileReader.ProteinName, ProteinCollection.CleavageStateConstants.Unknown, String.Empty)

                        ' ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True
                    End If


                    UpdateProgress(delimitedFileReader.PercentFileProcessed())
                    If Me.AbortProcessing Then Exit Do
                End If

            Loop While inputPeptideFound

            If inputFileLineSkipCount > 0 Then
                skipMessage = "Note that " & inputFileLineSkipCount.ToString() & " out of " & inputFileLinesRead.ToString() & " lines were skipped in the input file because they did not match the column order defined on the File Format Options Tab (" & mProteinFileParser.DelimitedFileFormatCode.ToString() & ")"
                MyBase.LogMessage(skipMessage, MessageTypeConstants.Warning)
                mLastErrorMessage = String.Copy(skipMessage)
            End If

            success = Not (Me.AbortProcessing)

        Catch ex As Exception
            SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile, ex)
            success = False
        Finally
            delimitedFileReader.CloseFile()
        End Try

        Return success

    End Function

    Private Function LoadProteinsOrPeptidesWork(proteinInputFilePath As String) As Boolean

        Dim success As Boolean
        Dim generateUniqueSequenceID As Boolean
        Dim isFastaFile As Boolean

        Dim masterSequences As Hashtable = Nothing

        Dim digestedPeptides As List(Of InSilicoDigest.PeptideSequenceWithNET) = Nothing

        Try

            If mProteinFileParser.GenerateUniqueIDValuesForPeptides Then
                ' Need to generate unique sequence ID values
                generateUniqueSequenceID = True

                ' Initialize masterSequences
                masterSequences = New Hashtable
            Else
                generateUniqueSequenceID = False
            End If
            Dim nextUniqueIDForMasterSeqs = 1

            If ProteinFileParser.IsFastaFile(proteinInputFilePath) OrElse mProteinFileParser.AssumeFastaFile Then
                isFastaFile = True
            Else
                isFastaFile = False
            End If


            ' Disable mass calculation
            mProteinFileParser.ComputeProteinMass = False
            mProteinFileParser.CreateProteinOutputFile = False

            If CysPeptidesOnly Then
                mProteinFileParser.DigestionOptions.AminoAcidResidueFilterChars = New Char() {"C"c}
            Else
                mProteinFileParser.DigestionOptions.AminoAcidResidueFilterChars = New Char() {}
            End If

            ' Load the proteins in the input file into memory
            success = mProteinFileParser.ParseProteinFile(proteinInputFilePath, "")

            Dim skipMessage As String = String.Empty
            If mProteinFileParser.InputFileLineSkipCount > 0 AndAlso Not isFastaFile Then
                skipMessage = "Note that " & mProteinFileParser.InputFileLineSkipCount.ToString() &
                              " out of " & mProteinFileParser.InputFileLinesRead.ToString() &
                              " lines were skipped in the input file because they did not match the column order" &
                              " defined on the File Format Options Tab (" & mProteinFileParser.DelimitedFileFormatCode.ToString() & ")"
                MyBase.LogMessage(skipMessage, MessageTypeConstants.Warning)
                mLastErrorMessage = String.Copy(skipMessage)
            End If

            If mProteinFileParser.GetProteinCountCached <= 0 Then
                If success Then
                    ' File was successfully loaded, but no proteins were found
                    ' This could easily happen for delimited files that only have a header line, or that don't have a format that matches what the user specified
                    SetLocalErrorCode(ErrorCodes.ProteinsNotFoundInInputFile)
                    mLastErrorMessage = String.Empty
                    success = False
                Else
                    SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile)
                    mLastErrorMessage = "Error using ParseProteinFile to read the proteins from " & Path.GetFileName(proteinInputFilePath)
                End If

                If mProteinFileParser.InputFileLineSkipCount > 0 AndAlso Not isFastaFile Then
                    If mLastErrorMessage.Length > 0 Then
                        mLastErrorMessage &= ". "
                    End If
                    mLastErrorMessage &= skipMessage
                End If

            ElseIf success Then

                ' Re-enable mass calculation
                mProteinFileParser.ComputeProteinMass = True

                ResetProgress("Digesting proteins in input file")

                For proteinIndex = 0 To mProteinFileParser.GetProteinCountCached - 1
                    Dim proteinOrPeptide = mProteinFileParser.GetCachedProtein(proteinIndex)

                    Dim peptideCount = mProteinFileParser.GetDigestedPeptidesForCachedProtein(proteinIndex, digestedPeptides, mProteinFileParser.DigestionOptions)

                    If peptideCount > 0 Then
                        For Each digestedPeptide In digestedPeptides
                            Dim uniqueSeqID As Integer

                            If generateUniqueSequenceID Then
                                uniqueSeqID = GetNextUniqueSequenceID(digestedPeptide.SequenceOneLetter, masterSequences, nextUniqueIDForMasterSeqs)
                            Else
                                ' Must assume each sequence is unique; probably an incorrect assumption
                                uniqueSeqID = nextUniqueIDForMasterSeqs
                                nextUniqueIDForMasterSeqs += 1
                            End If

                            AddOrUpdatePeptide(uniqueSeqID,
                                               digestedPeptide.Mass, digestedPeptide.NET, 0, 0,
                                               proteinOrPeptide.Name,
                                               ProteinCollection.CleavageStateConstants.Unknown,
                                               digestedPeptide.PeptideName)
                        Next
                    End If

                    UpdateProgress(CSng((proteinIndex + 1) / mProteinFileParser.GetProteinCountCached * 100))

                    If Me.AbortProcessing Then Exit For
                Next proteinIndex

                success = Not (Me.AbortProcessing)
            End If

        Catch ex As Exception
            SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile, ex)
            success = False
        End Try

        Return success

    End Function

    Private Sub InitializeBinningRanges(
      thresholds As PeakMatching.SearchThresholds,
      featuresToIdentify As PeakMatching.PMFeatureInfo,
      peptideMatchResults As PeakMatching.PMFeatureMatchResults,
      peptideStatsBinned As BinnedPeptideCountStats)

        Dim featureMass As Single

        Dim matchIndex As Integer

        Dim currentFeatureID As Integer
        Dim featureInfo = New PeakMatching.FeatureInfo()

        Dim matchResultInfo As PeakMatching.PMFeatureMatchResults.PeakMatchingResult

        Dim IntegerMass, remainder As Integer

        If peptideStatsBinned.Settings.AutoDetermineMassRange Then

            ' Examine peptideMatchResults to determine the minimum and maximum masses for features with matches

            ' First, set the ranges to out-of-range values
            peptideStatsBinned.Settings.MassMinimum = Double.MaxValue
            peptideStatsBinned.Settings.MassMaximum = Double.MinValue

            ' Now examine .PeptideMatchResults()
            For matchIndex = 0 To peptideMatchResults.Count - 1

                peptideMatchResults.GetMatchInfoByRowIndex(matchIndex, currentFeatureID, matchResultInfo)

                featuresToIdentify.GetFeatureInfoByFeatureID(currentFeatureID, featureInfo)
                featureMass = CSng(featureInfo.Mass)

                If featureMass > peptideStatsBinned.Settings.MassMaximum Then
                    peptideStatsBinned.Settings.MassMaximum = featureMass
                End If

                If featureMass < peptideStatsBinned.Settings.MassMinimum Then
                    peptideStatsBinned.Settings.MassMinimum = featureMass
                End If

            Next matchIndex

            ' Round the minimum and maximum masses to the nearest 100

            If Math.Abs(peptideStatsBinned.Settings.MassMinimum - Double.MaxValue) < Double.Epsilon OrElse
               Math.Abs(peptideStatsBinned.Settings.MassMaximum - Double.MinValue) < Double.Epsilon Then
                ' No matches were found; set these to defaults
                peptideStatsBinned.Settings.MassMinimum = 400
                peptideStatsBinned.Settings.MassMaximum = 6000
            End If

            IntegerMass = CInt(Math.Round(peptideStatsBinned.Settings.MassMinimum, 0))
            Math.DivRem(IntegerMass, 100, remainder)
            IntegerMass -= remainder

            peptideStatsBinned.Settings.MassMinimum = IntegerMass

            IntegerMass = CInt(Math.Round(peptideStatsBinned.Settings.MassMaximum, 0))
            Math.DivRem(IntegerMass, 100, remainder)
            IntegerMass += (100 - remainder)

            peptideStatsBinned.Settings.MassMaximum = IntegerMass

        End If

        ' Determine BinCount; do not allow more than 1,000,000 bins
        If Math.Abs(peptideStatsBinned.Settings.MassBinSizeDa - 0) < 0.00001 Then peptideStatsBinned.Settings.MassBinSizeDa = 1

        Do
            Try
                peptideStatsBinned.BinCount = CInt(Math.Ceiling(peptideStatsBinned.Settings.MassMaximum - peptideStatsBinned.Settings.MassMinimum) / peptideStatsBinned.Settings.MassBinSizeDa)
                If peptideStatsBinned.BinCount > 1000000 Then
                    peptideStatsBinned.Settings.MassBinSizeDa *= 10
                End If
            Catch ex As Exception
                peptideStatsBinned.BinCount = 1000000000
            End Try
        Loop While peptideStatsBinned.BinCount > 1000000

    End Sub

    Private Function MassToBinIndex(ThisMass As Double, StartMass As Double, MassResolution As Double) As Integer
        Dim WorkingMass As Double

        ' First subtract StartMass from ThisMass
        ' For example, if StartMass is 500 and ThisMass is 500.28, then WorkingMass = 0.28
        ' Or, if StartMass is 500 and ThisMass is 530.83, then WorkingMass = 30.83
        WorkingMass = ThisMass - StartMass

        ' Now, dividing WorkingMass by MassResolution and rounding to the nearest integer
        '  actually gives the bin
        ' For example, given WorkingMass = 0.28 and MassResolution = 0.1, Bin = CInt(2.8) = 3
        ' Or, given WorkingMass = 30.83 and MassResolution = 0.1, Bin = CInt(308.3) = 308
        Return CInt(Math.Floor(WorkingMass / MassResolution))

    End Function

    ' Main processing function
    Public Overloads Overrides Function ProcessFile(inputFilePath As String, outputFolderPath As String, parameterFilePath As String, resetErrorCode As Boolean) As Boolean
        ' Returns True if success, False if failure

        Dim file As FileInfo

        Dim inputFilePathFull As String

        Dim success As Boolean

        If resetErrorCode Then
            SetLocalErrorCode(ErrorCodes.NoError)
        End If

        If Not LoadParameterFileSettings(parameterFilePath) Then
            Dim statusMessage = "Parameter file load error: " & parameterFilePath
            ShowErrorMessage(statusMessage)
            If MyBase.ErrorCode = ProcessFilesErrorCodes.NoError Then
                MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidParameterFile)
            End If
            Return False
        End If

        Try
            If inputFilePath Is Nothing OrElse inputFilePath.Length = 0 Then
                Console.WriteLine("Input file name is empty")
                MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath)
            Else

                Console.WriteLine()
                Console.WriteLine("Parsing " & Path.GetFileName(inputFilePath))

                If Not CleanupFilePaths(inputFilePath, outputFolderPath) Then
                    MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.FilePathError)
                Else
                    Try
                        ' Obtain the full path to the input file
                        file = New FileInfo(inputFilePath)
                        inputFilePathFull = file.FullName

                        success = GenerateUniquenessStats(inputFilePathFull, outputFolderPath, Path.GetFileNameWithoutExtension(inputFilePath))

                    Catch ex As Exception
                        Throw New Exception("Error calling ParseProteinFile", ex)
                    End Try
                End If
            End If
        Catch ex As Exception
            Throw New Exception("Error in ProcessFile", ex)
        End Try

        Return success

    End Function

    Protected Shadows Sub ResetProgress()
        Me.AbortProcessing = False
        MyBase.ResetProgress()
    End Sub

    Protected Shadows Sub ResetProgress(description As String)
        Me.AbortProcessing = False
        MyBase.ResetProgress(description)
    End Sub

    Public Function SetPeptideUniquenessMassRangeForBinning(massMinimum As Single, massMaximum As Single) As Boolean
        ' Returns True if the minimum and maximum mass specified were valid

        If massMinimum < massMaximum AndAlso massMinimum >= 0 Then
            mPeptideUniquenessBinningSettings.MassMinimum = massMinimum
            mPeptideUniquenessBinningSettings.MassMaximum = massMaximum
            Return True
        End If

        Return False

    End Function

    Private Function SummarizeResultsByPeptide(
      thresholds As PeakMatching.SearchThresholds,
      thresholdIndex As Integer,
      featuresToIdentify As PeakMatching.PMFeatureInfo,
      peptideMatchResults As PeakMatching.PMFeatureMatchResults,
      outputFolderPath As String,
      outputFilenameBase As String,
      peptideUniquenessWriter As StreamWriter) As Boolean

        Dim workingFilenameBase As String
        Dim outputFilePath As String

        Dim peptideIndex As Integer
        Dim featuresToIdentifyCount As Integer

        Dim featureInfo = New PeakMatching.FeatureInfo

        Dim matchCount As Integer
        Dim binIndex As Integer
        Dim peptideSkipCount As Integer
        Dim total As Integer

        Dim maxMatchCount As Integer

        Dim peptideStatsBinned = New BinnedPeptideCountStats()

        Dim success As Boolean

        Try

            ' Copy the binning settings
            peptideStatsBinned.Settings = mPeptideUniquenessBinningSettings

            ' Define the minimum and maximum mass ranges, plus the number of bins required
            InitializeBinningRanges(thresholds, featuresToIdentify, peptideMatchResults, peptideStatsBinned)

            featuresToIdentifyCount = featuresToIdentify.Count

            ResetProgress("Summarizing results by peptide")

            ' --------------------------------------
            ' Compute the stats for thresholds
            ' --------------------------------------

            ' Define the maximum match count that will be tracked
            maxMatchCount = Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave)

            ' Reserve memory for the bins, store the bin ranges for each bin, and reset the ResultIDs arrays
            InitializeBinnedStats(peptideStatsBinned, peptideStatsBinned.BinCount)

            ' ToDo: When using Sql Server, switch this to use a SP that performs the same function as this For Loop, sorting the results in a table in the DB, but using Bulk Update queries

            MyBase.LogMessage("SummarizeResultsByPeptide starting, total feature count = " & featuresToIdentifyCount.ToString(), MessageTypeConstants.Normal)

            peptideSkipCount = 0
            For peptideIndex = 0 To featuresToIdentifyCount - 1
                If featuresToIdentify.GetFeatureInfoByRowIndex(peptideIndex, featureInfo) Then
                    binIndex = MassToBinIndex(featureInfo.Mass, peptideStatsBinned.Settings.MassMinimum, peptideStatsBinned.Settings.MassBinSizeDa)

                    If binIndex < 0 OrElse binIndex > peptideStatsBinned.BinCount - 1 Then
                        ' Peptide mass is out-of-range, ignore the result
                        peptideSkipCount += 1
                    Else
                        If FeatureContainsUniqueMatch(featureInfo, peptideMatchResults, matchCount, UseSLiCScoreForUniqueness, peptideStatsBinned.Settings.MinimumSLiCScore) Then
                            peptideStatsBinned.Bins(binIndex).UniqueResultIDCount += 1
                            peptideStatsBinned.Bins(binIndex).ResultIDCountDistribution(1) += 1
                        Else
                            If matchCount > 0 Then
                                ' Feature has 1 or more matches, but they're not unique
                                peptideStatsBinned.Bins(binIndex).NonUniqueResultIDCount += 1
                                If matchCount < maxMatchCount Then
                                    peptideStatsBinned.Bins(binIndex).ResultIDCountDistribution(matchCount) += 1
                                Else
                                    peptideStatsBinned.Bins(binIndex).ResultIDCountDistribution(maxMatchCount) += 1
                                End If
                            End If
                        End If

                    End If
                End If

                If peptideIndex Mod 100 = 0 Then
                    UpdateProgress(CSng(peptideIndex / featuresToIdentifyCount * 100))
                    If Me.AbortProcessing Then Exit For
                End If

                If peptideIndex Mod 100000 = 0 AndAlso peptideIndex > 0 Then
                    MyBase.LogMessage("SummarizeResultsByPeptide, peptideIndex = " & peptideIndex.ToString(), MessageTypeConstants.Normal)
                End If
            Next peptideIndex

            For binIndex = 0 To peptideStatsBinned.BinCount - 1
                total = peptideStatsBinned.Bins(binIndex).UniqueResultIDCount + peptideStatsBinned.Bins(binIndex).NonUniqueResultIDCount
                If total > 0 Then
                    peptideStatsBinned.Bins(binIndex).PercentUnique = CSng(peptideStatsBinned.Bins(binIndex).UniqueResultIDCount / total * 100)
                Else
                    peptideStatsBinned.Bins(binIndex).PercentUnique = 0
                End If
            Next binIndex

            If peptideSkipCount > 0 Then
                MyBase.LogMessage("Skipped " & peptideSkipCount.ToString() & " peptides since their masses were outside the defined bin range", MessageTypeConstants.Warning)
            End If

            ' Write out the peptide results for this threshold level
            If CreateSeparateOutputFileForEachThreshold Then
                workingFilenameBase = outputFilenameBase & "_PeptideStatsBinned" & (thresholdIndex + 1).ToString() & ".txt"
                outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase)

                peptideUniquenessWriter = New StreamWriter(outputFilePath)

                ExportThresholds(peptideUniquenessWriter, thresholdIndex, thresholds)
                peptideUniquenessWriter.WriteLine()

                SummarizeResultsByPeptideWriteHeaders(peptideUniquenessWriter, CreateSeparateOutputFileForEachThreshold)
            End If

            success = ExportPeptideUniquenessResults(thresholdIndex, peptideStatsBinned, peptideUniquenessWriter)

            If CreateSeparateOutputFileForEachThreshold Then
                peptideUniquenessWriter.Close()
            End If

            MyBase.LogMessage("SummarizeResultsByPeptide complete", MessageTypeConstants.Normal)

            UpdateProgress(100)

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Private Sub SummarizeResultsByPeptideWriteHeaders(peptideUniquenessWriter As TextWriter, createSeparateOutputFiles As Boolean)

        Dim lineOut As String
        Dim index As Integer

        ' Write the column headers
        If createSeparateOutputFiles Then
            lineOut = String.Empty
        Else
            lineOut = "Threshold_Index" & mOutputFileDelimiter
        End If

        lineOut &= "Bin_Start_Mass" & mOutputFileDelimiter &
                   "Percent_Unique" & mOutputFileDelimiter &
                   "Peptide_Count_Total"

        For index = 1 To Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave) - 1
            lineOut &= mOutputFileDelimiter & "MatchCount_" & index.ToString()
        Next index

        lineOut &= mOutputFileDelimiter & "MatchCount_" & Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave).ToString() & "_OrMore"

        peptideUniquenessWriter.WriteLine(lineOut)
        peptideUniquenessWriter.Flush()

    End Sub

    Private Function SummarizeResultsByProtein(
      thresholds As PeakMatching.SearchThresholds,
      thresholdIndex As Integer,
      featuresToIdentify As PeakMatching.PMFeatureInfo,
      peptideMatchResults As PeakMatching.PMFeatureMatchResults,
      outputFolderPath As String, outputFilenameBase As String,
      proteinStatsWriter As StreamWriter) As Boolean

        Dim workingFilenameBase As String
        Dim outputFilePath As String

        Dim peptideIndex As Integer
        Dim featuresToIdentifyCount As Integer

        Dim proteinIndex As Integer
        Dim matchCount As Integer

        Dim proteinIDs() As Integer
        Dim newDataRow As DataRow

        Dim success As Boolean

        Dim featureInfo = New PeakMatching.FeatureInfo

        Try

            featuresToIdentifyCount = featuresToIdentify.Count
            ResetProgress("Summarizing results by protein")

            ' Compute the stats for thresholds

            ' Compute number of unique peptides seen for each protein and write out
            ' This will allow one to plot Unique peptide vs protein mass or total peptide count vs. protein mass

            ' Initialize mProteinToIdentifiedPeptideMappingTable
            mProteinToIdentifiedPeptideMappingTable.Clear()

            MyBase.LogMessage("SummarizeResultsByProtein starting, total feature count = " & featuresToIdentifyCount.ToString(), MessageTypeConstants.Normal)

            For peptideIndex = 0 To featuresToIdentifyCount - 1
                If featuresToIdentify.GetFeatureInfoByRowIndex(peptideIndex, featureInfo) Then
                    If FeatureContainsUniqueMatch(featureInfo, peptideMatchResults, matchCount, UseSLiCScoreForUniqueness, mPeptideUniquenessBinningSettings.MinimumSLiCScore) Then
                        proteinIDs = mProteinInfo.GetProteinIDsMappedToPeptideID(featureInfo.FeatureID)

                        For proteinIndex = 0 To proteinIDs.Length - 1
                            If Not mProteinToIdentifiedPeptideMappingTable.Rows.Contains(New Object() {proteinIDs(proteinIndex), featureInfo.FeatureID}) Then
                                newDataRow = mProteinToIdentifiedPeptideMappingTable.NewRow
                                newDataRow(PROTEIN_ID_COLUMN) = proteinIDs(proteinIndex)
                                newDataRow(PEPTIDE_ID_MATCH_COLUMN) = featureInfo.FeatureID
                                mProteinToIdentifiedPeptideMappingTable.Rows.Add(newDataRow)
                            End If
                        Next proteinIndex
                    End If

                End If

                If peptideIndex Mod 100 = 0 Then
                    UpdateProgress(CSng(peptideIndex / featuresToIdentifyCount * 100))
                    If Me.AbortProcessing Then Exit For
                End If

                If peptideIndex Mod 100000 = 0 AndAlso peptideIndex > 0 Then
                    MyBase.LogMessage("SummarizeResultsByProtein, peptideIndex = " & peptideIndex.ToString(), MessageTypeConstants.Normal)
                End If

            Next peptideIndex

            ' Write out the protein results for this threshold level
            If CreateSeparateOutputFileForEachThreshold Then
                workingFilenameBase = outputFilenameBase & "_ProteinStatsBinned" & (thresholdIndex + 1).ToString() & ".txt"
                outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase)

                proteinStatsWriter = New StreamWriter(outputFilePath)

                ExportThresholds(proteinStatsWriter, thresholdIndex, thresholds)
                proteinStatsWriter.WriteLine()

                SummarizeResultsByProteinWriteHeaders(proteinStatsWriter, CreateSeparateOutputFileForEachThreshold)
            End If

            success = ExportProteinStats(thresholdIndex, proteinStatsWriter)

            If CreateSeparateOutputFileForEachThreshold Then
                proteinStatsWriter.Close()
            End If

            UpdateProgress(100)

            MyBase.LogMessage("SummarizeResultsByProtein complete", MessageTypeConstants.Normal)

        Catch ex As Exception
            success = False
        End Try

        Return success

    End Function

    Private Sub SummarizeResultsByProteinWriteHeaders(proteinStatsWriter As TextWriter, createSeparateOutputFiles As Boolean)

        Dim lineOut As String

        ' Write the column headers
        If createSeparateOutputFiles Then
            lineOut = String.Empty
        Else
            lineOut = "Threshold_Index" & mOutputFileDelimiter
        End If

        lineOut &= "Protein_Name" &
                    mOutputFileDelimiter & "Protein_ID" &
                    mOutputFileDelimiter & "Peptide_Count_Total" &
                    mOutputFileDelimiter & "Peptide_Count_Uniquely_Identifiable"
        proteinStatsWriter.WriteLine(lineOut)
        proteinStatsWriter.Flush()

    End Sub

    Private Sub SetLocalErrorCode(newErrorCode As ErrorCodes)
        SetLocalErrorCode(newErrorCode, False)
    End Sub

    Private Sub SetLocalErrorCode(newErrorCode As ErrorCodes, ex As Exception)
        SetLocalErrorCode(newErrorCode, False)

        MyBase.LogMessage(newErrorCode.ToString() & ": " & ex.Message, MessageTypeConstants.ErrorMsg)
        mLastErrorMessage = ex.Message
    End Sub

    Private Sub SetLocalErrorCode(newErrorCode As ErrorCodes, leaveExistingErrorCodeUnchanged As Boolean)

        If leaveExistingErrorCodeUnchanged AndAlso mLocalErrorCode <> ErrorCodes.NoError Then
            ' An error code is already defined; do not change it
        Else
            mLocalErrorCode = newErrorCode
            mLastErrorMessage = String.Empty
        End If

    End Sub

    Protected Sub UpdateSubtaskProgress(description As String)
        UpdateProgress(description, mProgressPercentComplete)
    End Sub

    Protected Sub UpdateSubtaskProgress(percentComplete As Single)
        UpdateProgress(Me.ProgressStepDescription, percentComplete)
    End Sub

    Protected Sub UpdateSubtaskProgress(description As String, percentComplete As Single)
        Dim descriptionChanged = False

        If description <> mSubtaskProgressStepDescription Then
            descriptionChanged = True
        End If

        mSubtaskProgressStepDescription = String.Copy(description)
        If percentComplete < 0 Then
            percentComplete = 0
        ElseIf percentComplete > 100 Then
            percentComplete = 100
        End If
        mSubtaskProgressPercentComplete = percentComplete

        If descriptionChanged Then
            If Math.Abs(mSubtaskProgressPercentComplete - 0) < Single.Epsilon Then
                LogMessage(mSubtaskProgressStepDescription.Replace(ControlChars.NewLine, "; "))
            Else
                LogMessage(mSubtaskProgressStepDescription & " (" & mSubtaskProgressPercentComplete.ToString("0.0") & "% complete)".Replace(ControlChars.NewLine, "; "))
            End If
        End If

        RaiseEvent SubtaskProgressChanged(description, Me.ProgressPercentComplete)

    End Sub

    Private Sub mProteinInfo_SortingList() Handles mProteinInfo.SortingList
        Static sortCount As Integer = 0
        Static lastPostTime As DateTime = DateTime.UtcNow

        sortCount += 1
        If DateTime.UtcNow.Subtract(lastPostTime).TotalSeconds >= 10 Then
            MyBase.LogMessage("Sorting protein list (SortCount = " & sortCount & ")", MessageTypeConstants.Normal)
            lastPostTime = DateTime.UtcNow
        End If
    End Sub

    Private Sub mProteinInfo_SortingMappings() Handles mProteinInfo.SortingMappings
        Static sortCount As Integer = 0
        Static lastPostTime As DateTime = DateTime.UtcNow

        sortCount += 1
        If DateTime.UtcNow.Subtract(lastPostTime).TotalSeconds >= 10 Then
            MyBase.LogMessage("Sorting protein to peptide mapping info (SortCount = " & sortCount & ")", MessageTypeConstants.Normal)
            lastPostTime = DateTime.UtcNow
        End If
    End Sub

    Private Sub mPeptideMatchResults_SortingList() Handles mPeptideMatchResults.SortingList
        Static sortCount As Integer = 0
        Static lastPostTime As DateTime = DateTime.UtcNow

        sortCount += 1
        If DateTime.UtcNow.Subtract(lastPostTime).TotalSeconds >= 10 Then
            MyBase.LogMessage("Sorting peptide match results list (SortCount = " & sortCount & ")", MessageTypeConstants.Normal)
            lastPostTime = DateTime.UtcNow
        End If
    End Sub

    Private Sub mPeakMatching_LogEvent(Message As String, EventType As PeakMatching.MessageTypeConstants) Handles mPeakMatching.LogEvent
        Select Case EventType
            Case PeakMatching.MessageTypeConstants.Normal
                MyBase.LogMessage(Message, MessageTypeConstants.Normal)

            Case PeakMatching.MessageTypeConstants.Warning
                MyBase.LogMessage(Message, MessageTypeConstants.Warning)

            Case PeakMatching.MessageTypeConstants.ErrorMsg
                MyBase.LogMessage(Message, MessageTypeConstants.ErrorMsg)

            Case PeakMatching.MessageTypeConstants.Health
                ' Don't log this type of message
        End Select
    End Sub

    Private Sub mPeakMatching_ProgressContinues() Handles mPeakMatching.ProgressContinues
        UpdateSubtaskProgress(mPeakMatching.ProgressPct)
    End Sub

    Private Sub mProteinFileParser_ErrorEvent(message As String, ex As Exception) Handles mProteinFileParser.ErrorEvent
        ShowErrorMessage("Error in mProteinFileParser: " & message)
    End Sub

End Class

