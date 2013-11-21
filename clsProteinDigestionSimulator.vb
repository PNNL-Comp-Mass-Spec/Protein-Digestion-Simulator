Option Strict On

' This class will read two fasta files and look for overlap in protein sequence between the proteins of
'  the first fasta file and the second fasta file
'
' -------------------------------------------------------------------------------
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
' Program started October 11, 2004
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.

' E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com
' Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/
' -------------------------------------------------------------------------------
' 
' Licensed under the Apache License, Version 2.0; you may not use this file except
' in compliance with the License.  You may obtain a copy of the License at 
' http://www.apache.org/licenses/LICENSE-2.0
'
' Notice: This computer software was prepared by Battelle Memorial Institute, 
' hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
' Department of Energy (DOE).  All rights in the computer software are reserved 
' by DOE on behalf of the United States Government and the Contractor as 
' provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
' WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
' SOFTWARE.  This notice including this sentence must appear on any copies of 
' this computer software.

Public Class clsProteinDigestionSimulator
	Inherits clsProcessFilesBaseClass

	Public Sub New()
		MyBase.mFileDate = "November 20, 2013"
		InitializeLocalVariables()
	End Sub

#Region "Constants and Enums"

	Public Const XML_SECTION_PEAK_MATCHING_OPTIONS As String = "PeakMatchingOptions"

	Private Const PROTEIN_ID_COLUMN As String = "ProteinID"
	Private Const PEPTIDE_ID_MATCH_COLUMN As String = "PeptideIDMatch"

	Private Const ID_COUNT_DISTRIBUTION_MAX As Integer = 10

	' Error codes specialized for this class
	Public Enum eProteinDigestionSimulatorErrorCodes
		NoError = 0
		ProteinDigestionSimulatorSectionNotFound = 1
		ErrorReadingInputFile = 2
		ProteinsNotFoundInInputFile = 4
		ErrorIdentifyingSequences = 8
		ErrorWritingOutputFile = 16
		UserAbortedSearch = 32
		UnspecifiedError = -1
	End Enum

#End Region

#Region "Structures"

	Private Structure udtSingleBinStatsType
		Public MassBinStart As Double				' Mass is >= this value
		Public MassBinEnd As Double					' Mass is < this value
		Public UniqueResultIDCount As Integer
		Public NonUniqueResultIDCount As Integer
		Public ResultIDCountDistribution() As Integer
		Public PercentUnique As Single				' UniqueResultIDs().length / (UniqueResultIDs().length + NonUniqueResultIDs().length)
	End Structure

	Private Structure udtMassBinningOptionsType
		Public AutoDetermineMassRange As Boolean
		Public MassBinSizeDa As Single
		Public MassMinimum As Single	' This is ignored if AutoDetermineMassRange = True
		Public MassMaximum As Single	' This is ignored if AutoDetermineMassRange = True
		Public MinimumSLiCScore As Single
	End Structure

	Private Structure udtBinnedPeptideCountStatsType
		Public Settings As udtMassBinningOptionsType
		Public BinCount As Integer
		Public Bins() As udtSingleBinStatsType
	End Structure

#End Region

#Region "Classwide Variables"

	Public WithEvents mProteinFileParser As clsParseProteinFile		' This class is exposed as public so that we can directly access some of its properties without having to create wrapper properties in this class

	Private mDigestSequences As Boolean					' Ignored for fasta files; they are always digested
	Private mCysPeptidesOnly As Boolean

	Private mOutputFileDelimiter As Char
	Private mCreateSeparateOutputFileForEachThreshold As Boolean

	Private mSavePeakMatchingResults As Boolean
	Private mMaxPeakMatchingResultsPerFeatureToSave As Integer

	Private mPeptideUniquenessBinningSettings As udtMassBinningOptionsType
	Private mUseSLiCScoreForUniqueness As Boolean
	Private mUseEllipseSearchRegion As Boolean		   ' Only valid if mUseSLiCScoreForUniqueness = False; if both mUseSLiCScoreForUniqueness= False and mUseEllipseSearchRegion = False, then uses a rectangle to determine uniqueness

	Private mLocalErrorCode As eProteinDigestionSimulatorErrorCodes
	Private mLastErrorMessage As String

	Private WithEvents mPeakMatchingClass As clsPeakMatchingClass
	Private WithEvents mComparisonPeptideInfo As clsPeakMatchingClass.PMComparisonFeatureInfoClass			   ' Comparison peptides to match against
	Private WithEvents mProteinInfo As clsProteinInfo
	Private WithEvents mPeptideMatchResults As clsPeakMatchingClass.PMFeatureMatchResultsClass

	Private mProteinToIdentifiedPeptideMappingTable As System.Data.DataTable						' Holds the lists of peptides that were uniquely identified for each protein

	Private mThresholdLevels() As clsPeakMatchingClass.clsSearchThresholds		 ' Thresholds to use for searching

	''Private mUseSqlServerDBToCacheData As Boolean
	''Private mUseSqlServerForMatchResults As Boolean
	''Private mSqlServerConnectionString As String
	''Private mUseBulkInsert As Boolean
	''Private mSqlServerUseExistingData As Boolean

	''Private mTableNameFeaturesToIdentify As String
	''Private mTableNameComparisonPeptides As String
	''Private mTableNameProteinInfo As String
	''Private mTableNameProteinToPeptideMap As String

	Private mSubtaskProgressStepDescription As String = String.Empty
	Private mSubtaskProgressPercentComplete As Single = 0

	' PercentComplete ranges from 0 to 100, but can contain decimal percentage values
	Public Event SubtaskProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single)

#End Region

#Region "Processing Options Interface Functions"

	Public Property AutoDetermineMassRangeForBinning() As Boolean
		Get
			Return mPeptideUniquenessBinningSettings.AutoDetermineMassRange
		End Get
		Set(ByVal Value As Boolean)
			mPeptideUniquenessBinningSettings.AutoDetermineMassRange = Value
		End Set
	End Property

	Public Property CreateSeparateOutputFileForEachThreshold() As Boolean
		Get
			Return mCreateSeparateOutputFileForEachThreshold
		End Get
		Set(ByVal Value As Boolean)
			mCreateSeparateOutputFileForEachThreshold = Value
		End Set
	End Property

	Public Property CysPeptidesOnly() As Boolean
		Get
			Return mCysPeptidesOnly
		End Get
		Set(ByVal Value As Boolean)
			mCysPeptidesOnly = Value
		End Set
	End Property

	Public Property DigestSequences() As Boolean
		Get
			Return mDigestSequences
		End Get
		Set(ByVal Value As Boolean)
			mDigestSequences = Value
		End Set
	End Property

	Public Property ElementMassMode() As PeptideSequenceClass.ElementModeConstants
		Get
			If mProteinFileParser Is Nothing Then
				Return PeptideSequenceClass.ElementModeConstants.IsotopicMass
			Else
				Return mProteinFileParser.ElementMassMode
			End If
		End Get
		Set(ByVal Value As PeptideSequenceClass.ElementModeConstants)
			If mProteinFileParser Is Nothing Then
				InitializeProteinFileParser()
			End If
			mProteinFileParser.ElementMassMode = Value
		End Set
	End Property

	Public ReadOnly Property LocalErrorCode() As eProteinDigestionSimulatorErrorCodes
		Get
			Return mLocalErrorCode
		End Get
	End Property

	Public Property MaxPeakMatchingResultsPerFeatureToSave() As Integer
		Get
			Return mMaxPeakMatchingResultsPerFeatureToSave
		End Get
		Set(ByVal Value As Integer)
			If Value < 1 Then Value = 1
			mMaxPeakMatchingResultsPerFeatureToSave = Value
		End Set
	End Property

	Public Property MinimumSLiCScoreToBeConsideredUnique() As Single
		Get
			Return mPeptideUniquenessBinningSettings.MinimumSLiCScore
		End Get
		Set(ByVal Value As Single)
			mPeptideUniquenessBinningSettings.MinimumSLiCScore = Value
		End Set
	End Property

	Public Property PeptideUniquenessMassBinSizeForBinning() As Single
		Get
			Return mPeptideUniquenessBinningSettings.MassBinSizeDa
		End Get
		Set(ByVal Value As Single)
			If Value > 0 Then
				mPeptideUniquenessBinningSettings.MassBinSizeDa = Value
			End If
		End Set
	End Property

	Public Property OutputFileDelimiter() As Char
		Get
			Return mOutputFileDelimiter
		End Get
		Set(ByVal Value As Char)
			If Not Value = Nothing Then
				mOutputFileDelimiter = Value
			End If
		End Set
	End Property

	Public Property SavePeakMatchingResults() As Boolean
		Get
			Return mSavePeakMatchingResults
		End Get
		Set(ByVal Value As Boolean)
			mSavePeakMatchingResults = Value
		End Set
	End Property

	''Public Property SqlServerConnectionString() As String
	''    Get
	''        Return mSqlServerConnectionString
	''    End Get
	''    Set(ByVal Value As String)
	''        mSqlServerConnectionString = Value
	''    End Set
	''End Property

	''Public Property SqlServerTableNameComparisonPeptides() As String
	''    Get
	''        Return mTableNameComparisonPeptides
	''    End Get
	''    Set(ByVal Value As String)
	''        mTableNameComparisonPeptides = Value
	''    End Set
	''End Property

	''Public Property SqlServerTableNameFeaturesToIdentify() As String
	''    Get
	''        Return mTableNameFeaturesToIdentify
	''    End Get
	''    Set(ByVal Value As String)
	''        mTableNameFeaturesToIdentify = Value
	''    End Set
	''End Property

	''Public Property SqlServerUseBulkInsert() As Boolean
	''    Get
	''        Return mUseBulkInsert
	''    End Get
	''    Set(ByVal Value As Boolean)
	''        mUseBulkInsert = Value
	''    End Set
	''End Property

	''Public Property SqlServerUseExistingData() As Boolean
	''    Get
	''        Return mSqlServerUseExistingData
	''    End Get
	''    Set(ByVal Value As Boolean)
	''        mSqlServerUseExistingData = Value
	''    End Set
	''End Property

	Public Property UseEllipseSearchRegion() As Boolean
		Get
			Return mUseEllipseSearchRegion
		End Get
		Set(ByVal Value As Boolean)
			mUseEllipseSearchRegion = Value
		End Set
	End Property

	Public Property UseSLiCScoreForUniqueness() As Boolean
		Get
			Return mUseSLiCScoreForUniqueness
		End Get
		Set(ByVal Value As Boolean)
			mUseSLiCScoreForUniqueness = Value
		End Set
	End Property

	''Public Property UseSqlServerDBToCacheData() As Boolean
	''    Get
	''        Return mUseSqlServerDBToCacheData
	''    End Get
	''    Set(ByVal Value As Boolean)
	''        mUseSqlServerDBToCacheData = Value
	''    End Set
	''End Property

	''Public Property UseSqlServerForMatchResults() As Boolean
	''    Get
	''        Return mUseSqlServerForMatchResults
	''    End Get
	''    Set(ByVal Value As Boolean)
	''        mUseSqlServerForMatchResults = Value
	''    End Set
	''End Property

#End Region

	Public Sub AddSearchThresholdLevel(ByVal eMassToleranceType As clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants, ByVal dblMassTolerance As Double, ByVal dblNETTolerance As Double, ByVal blnClearExistingThresholds As Boolean)
		AddSearchThresholdLevel(eMassToleranceType, dblMassTolerance, dblNETTolerance, True, 0, 0, True, blnClearExistingThresholds)
	End Sub

	Public Sub AddSearchThresholdLevel(ByVal eMassToleranceType As clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants, ByVal dblMassTolerance As Double, ByVal dblNETTolerance As Double, ByVal blnAutoDefineSLiCScoreThresholds As Boolean, ByVal SLiCScoreMassPPMStDev As Double, ByVal SLiCScoreNETStDev As Double, ByVal SLiCScoreUseAMTNETStDev As Boolean, ByVal blnClearExistingThresholds As Boolean)
		AddSearchThresholdLevel(eMassToleranceType, dblMassTolerance, dblNETTolerance, True, 0, 0, True, blnClearExistingThresholds, clsPeakMatchingClass.DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER)
	End Sub

	Public Sub AddSearchThresholdLevel(ByVal eMassToleranceType As clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants, ByVal dblMassTolerance As Double, ByVal dblNETTolerance As Double, ByVal blnAutoDefineSLiCScoreThresholds As Boolean, ByVal SLiCScoreMassPPMStDev As Double, ByVal SLiCScoreNETStDev As Double, ByVal SLiCScoreUseAMTNETStDev As Boolean, ByVal blnClearExistingThresholds As Boolean, ByVal sngSLiCScoreMaxSearchDistanceMultiplier As Single)

		If blnClearExistingThresholds Then
			InitializeThresholdLevels(mThresholdLevels, 1, False)
		Else
			InitializeThresholdLevels(mThresholdLevels, mThresholdLevels.Length + 1, True)
		End If

		With mThresholdLevels(mThresholdLevels.Length - 1)
			.AutoDefineSLiCScoreThresholds = blnAutoDefineSLiCScoreThresholds

			.SLiCScoreMaxSearchDistanceMultiplier = sngSLiCScoreMaxSearchDistanceMultiplier
			.MassTolType = eMassToleranceType
			.MassTolerance = dblMassTolerance
			.NETTolerance = dblNETTolerance

			.SLiCScoreUseAMTNETStDev = SLiCScoreUseAMTNETStDev

			If Not blnAutoDefineSLiCScoreThresholds Then
				.SLiCScoreMassPPMStDev = SLiCScoreMassPPMStDev
				.SLiCScoreNETStDev = SLiCScoreNETStDev
			End If
		End With
	End Sub

	Private Function AddOrUpdatePeptide(ByVal intUniqueSeqID As Integer, ByVal dblPeptideMass As Double, ByVal sngPeptideNET As Single, ByVal sngPeptideNETStDev As Single, ByVal sngPeptideDiscriminantScore As Single, ByVal strProteinName As String, ByVal eCleavageStateInProtein As clsProteinInfo.eCleavageStateConstants, ByVal strPeptideName As String) As Integer
		' Assures that the peptide is present in mComparisonPeptideInfo and that the protein and protein/peptide mapping is present in mProteinInfo
		' Assumes that intUniqueSeqID is truly unique for the given peptide

		Dim intProteinID As Integer

		mComparisonPeptideInfo.Add(intUniqueSeqID, strPeptideName, dblPeptideMass, sngPeptideNET, sngPeptideNETStDev, sngPeptideDiscriminantScore)

		' Determine the ProteinID for strProteinName
		If Not strProteinName Is Nothing AndAlso strProteinName.Length > 0 Then
			' Lookup the index for the given protein
			intProteinID = AddOrUpdateProtein(strProteinName)
		Else
			intProteinID = -1
		End If

		If intProteinID >= 0 Then
			' Add the protein to the peptide to protein mapping table, if necessary
			mProteinInfo.AddProteinToPeptideMapping(intProteinID, intUniqueSeqID, eCleavageStateInProtein)
		End If

		Return intUniqueSeqID

	End Function

	Private Function AddOrUpdateProtein(ByVal strProteinName As String) As Integer
		' Returns the index of the protein in mProteinInfo
		' Returns -1 if not found and unable to add it

		Dim intProteinID As Integer

		' Note: intProteinID is auto-assigned
		If Not mProteinInfo.Add(strProteinName, intProteinID) Then
			intProteinID = -1
		End If

		Return intProteinID

	End Function

	Private Function ExportPeakMatchingResults(ByRef objThresholds As clsPeakMatchingClass.clsSearchThresholds, _
											   ByVal intThresholdIndex As Integer, _
											   ByRef intComparisonFeatureCount As Integer, _
											   ByRef objPeptideMatchResults As clsPeakMatchingClass.PMFeatureMatchResultsClass, _
											   ByVal strOutputFolderPath As String, _
											   ByVal strOutputFilenameBase As String, _
											   ByRef srPMResultsOutFile As System.IO.StreamWriter) As Boolean

		Dim strWorkingFilenameBase As String
		Dim strOutputFilePath As String

		Dim strLinePrefix As String
		Dim strLineOut As String

		Dim intMatchIndex As Integer

		Dim intCachedMatchCount As Integer
		Dim intCachedMatchCountFeatureID As Integer

		Dim intCurrentFeatureID As Integer
		Dim udtMatchResultInfo As clsPeakMatchingClass.PMFeatureMatchResultsClass.udtPeakMatchingResultType

		Dim dtLastFlushTime As DateTime
		Dim blnSuccess As Boolean

		Try

			ResetProgress("Exporting peak matching results")

			If mCreateSeparateOutputFileForEachThreshold Then
				' Create one file for each search threshold level
				strWorkingFilenameBase = strOutputFilenameBase & "_PMResults" & (intThresholdIndex + 1).ToString & ".txt"
				strOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, strWorkingFilenameBase)

				srPMResultsOutFile = New System.IO.StreamWriter(strOutputFilePath)

				' Write the threshold values to the output file
				srPMResultsOutFile.WriteLine("Comparison feature count: " & intComparisonFeatureCount.ToString)
				ExportThresholds(srPMResultsOutFile, intThresholdIndex, objThresholds)

				srPMResultsOutFile.WriteLine()

				ExportPeakMatchingResultsWriteHeaders(srPMResultsOutFile, mCreateSeparateOutputFileForEachThreshold)
			End If


			intCachedMatchCount = 0
			intCachedMatchCountFeatureID = -1

			dtLastFlushTime = System.DateTime.UtcNow

			' ToDo: Grab chunks of data from the server if caching into SqlServer (change this to a while loop)
			For intMatchIndex = 0 To objPeptideMatchResults.Count - 1

				If mCreateSeparateOutputFileForEachThreshold Then
					strLinePrefix = String.Empty
				Else
					strLinePrefix = (intThresholdIndex + 1).ToString & mOutputFileDelimiter
				End If

				If objPeptideMatchResults.GetMatchInfoByRowIndex(intMatchIndex, intCurrentFeatureID, udtMatchResultInfo) Then

					If intCurrentFeatureID <> intCachedMatchCountFeatureID Then
						intCachedMatchCount = objPeptideMatchResults.MatchCountForFeatureID(intCurrentFeatureID)
						intCachedMatchCountFeatureID = intCurrentFeatureID
					End If


					strLineOut = strLinePrefix & _
								intCurrentFeatureID.ToString & mOutputFileDelimiter & _
								intCachedMatchCount.ToString & mOutputFileDelimiter & _
								udtMatchResultInfo.MultiAMTHitCount.ToString & mOutputFileDelimiter & _
								udtMatchResultInfo.MatchingID.ToString & mOutputFileDelimiter & _
								Math.Round(udtMatchResultInfo.MassErr, 6).ToString & mOutputFileDelimiter & _
								Math.Round(udtMatchResultInfo.NETErr, 4).ToString & mOutputFileDelimiter & _
								Math.Round(udtMatchResultInfo.SLiCScore, 4).ToString & mOutputFileDelimiter & _
								Math.Round(udtMatchResultInfo.DelSLiC, 4).ToString
					srPMResultsOutFile.WriteLine(strLineOut)

				End If

				If intMatchIndex Mod 100 = 0 Then
					UpdateProgress(CSng(intMatchIndex / objPeptideMatchResults.Count * 100))

					If System.DateTime.UtcNow.Subtract(dtLastFlushTime).TotalSeconds > 30 Then
						srPMResultsOutFile.Flush()
					End If

				End If

			Next intMatchIndex

			If mCreateSeparateOutputFileForEachThreshold Then
				srPMResultsOutFile.Close()
				srPMResultsOutFile = Nothing
			End If

			UpdateProgress(100)

			blnSuccess = True

		Catch ex As Exception
			blnSuccess = False
		End Try

		Return blnSuccess

	End Function

	Private Sub ExportPeakMatchingResultsWriteHeaders(ByRef srOutFile As System.IO.StreamWriter, ByVal blnCreateSeparateOutputFileForEachThreshold As Boolean)

		Dim strLineOut As String

		' Write the column headers
		If blnCreateSeparateOutputFileForEachThreshold Then
			strLineOut = String.Empty
		Else
			strLineOut = "Threshold_Index" & mOutputFileDelimiter
		End If

		strLineOut &= "Unique_Sequence_ID" & _
					mOutputFileDelimiter & "Match_Count" & _
					mOutputFileDelimiter & "Multi_AMT_Hit_Count" & _
					mOutputFileDelimiter & "Matching_ID_Index" & _
					mOutputFileDelimiter & "Mass_Err"
#If IncludePNNLNETRoutines Then
		strLineOut &= mOutputFileDelimiter & "NET_Err"
#Else
        strLineOut &= mOutputFileDelimiter & "Time_Err"
#End If

		strLineOut &= mOutputFileDelimiter & "SLiC_Score" & _
					mOutputFileDelimiter & "Del_SLiC"
		srOutFile.WriteLine(strLineOut)

	End Sub

	Private Function ExportPeptideUniquenessResults(ByVal intThresholdIndex As Integer, ByRef udtBinResults As udtBinnedPeptideCountStatsType, ByRef srOutFile As System.IO.StreamWriter) As Boolean

		Dim strLineOut As String
		Dim intBinIndex As Integer
		Dim intIndex As Integer

		Dim blnSuccess As Boolean

		Try

			With udtBinResults
				For intBinIndex = 0 To .BinCount - 1
					With .Bins(intBinIndex)
						If mCreateSeparateOutputFileForEachThreshold Then
							strLineOut = String.Empty
						Else
							strLineOut = (intThresholdIndex + 1).ToString & mOutputFileDelimiter
						End If
						strLineOut &= Math.Round(.MassBinStart, 2).ToString & _
									mOutputFileDelimiter & Math.Round(.PercentUnique, 3).ToString & _
									mOutputFileDelimiter & Math.Round(.NonUniqueResultIDCount + .UniqueResultIDCount, 3).ToString

						For intIndex = 1 To Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave)
							strLineOut &= mOutputFileDelimiter & .ResultIDCountDistribution(intIndex).ToString
						Next intIndex
					End With

					srOutFile.WriteLine(strLineOut)

				Next intBinIndex
			End With

			srOutFile.Flush()
			blnSuccess = True

		Catch ex As Exception
			blnSuccess = False
		End Try

		Return blnSuccess

	End Function

	Private Function ExportProteinStats(ByVal intThresholdIndex As Integer, ByRef SearchThresholds As clsPeakMatchingClass.clsSearchThresholds, ByVal strOutputFilePath As String, ByRef srOutFile As System.IO.StreamWriter) As Boolean

		Dim strLineOut As String

		Dim intProteinIndex As Integer

		Dim intProteinID As Integer
		Dim strProteinName As String = String.Empty

		Dim dtLastFlushTime As DateTime
		Dim blnSuccess As Boolean

		Try

			dtLastFlushTime = System.DateTime.UtcNow
			For intProteinIndex = 0 To mProteinInfo.Count - 1

				If mCreateSeparateOutputFileForEachThreshold Then
					strLineOut = String.Empty
				Else
					strLineOut = (intThresholdIndex + 1).ToString & mOutputFileDelimiter
				End If

				If mProteinInfo.GetProteinInfoByRowIndex(intProteinIndex, intProteinID, strProteinName) Then
					strLineOut &= strProteinName & _
								mOutputFileDelimiter & intProteinID.ToString & _
								mOutputFileDelimiter & mProteinInfo.GetPeptideCountForProteinByID(intProteinID).ToString & _
								mOutputFileDelimiter & GetPeptidesUniquelyIdentifiedCountByProteinID(intProteinID).ToString

					srOutFile.WriteLine(strLineOut)

				End If

				If intProteinIndex Mod 100 = 0 AndAlso System.DateTime.UtcNow.Subtract(dtLastFlushTime).TotalSeconds > 30 Then
					srOutFile.Flush()
				End If
			Next intProteinIndex

			srOutFile.Flush()
			blnSuccess = True

		Catch ex As Exception
			blnSuccess = False
		End Try

		Return blnSuccess

	End Function

	Private Function ExtractServerFromConnectionString(ByVal strConnectionString As String) As String
		Const DATA_SOURCE_TEXT As String = "data source"

		Dim intCharLoc, intCharLoc2 As Integer
		Dim strServerName As String = String.Empty

		Try
			intCharLoc = strConnectionString.ToLower.IndexOf(DATA_SOURCE_TEXT)
			If intCharLoc >= 0 Then
				intCharLoc += DATA_SOURCE_TEXT.Length
				intCharLoc2 = strConnectionString.ToLower.IndexOf(";"c, intCharLoc + 1)

				If intCharLoc2 <= intCharLoc Then
					intCharLoc2 = strConnectionString.Length - 1
				End If

				strServerName = strConnectionString.Substring(intCharLoc + 1, intCharLoc2 - intCharLoc - 1)
			End If
		Catch ex As Exception
		End Try

		Return strServerName

	End Function

	Private Function FeatureContainsUniqueMatch(ByRef udtFeatureInfo As clsPeakMatchingClass.udtFeatureInfoType, ByRef objPeptideMatchResults As clsPeakMatchingClass.PMFeatureMatchResultsClass, ByRef intMatchCount As Integer, ByVal blnUseSLiCScoreForUniqueness As Boolean, ByVal sngMinimumSLiCScore As Single) As Boolean
		Dim blnUniqueMatch As Boolean
		Dim udtMatchResults() As clsPeakMatchingClass.PMFeatureMatchResultsClass.udtPeakMatchingResultType = Nothing

		Dim intMatchIndex As Integer

		blnUniqueMatch = False

		If objPeptideMatchResults.GetMatchInfoByFeatureID(udtFeatureInfo.FeatureID, udtMatchResults, intMatchCount) Then
			If intMatchCount > 0 Then
				' The current feature has 1 or more matches

				If blnUseSLiCScoreForUniqueness Then
					' See if any of the matches have a SLiC Score >= the minimum SLiC Score
					blnUniqueMatch = False
					For intMatchIndex = 0 To intMatchCount - 1
						If Math.Round(udtMatchResults(intMatchIndex).SLiCScore, 4) >= Math.Round(sngMinimumSLiCScore, 4) Then
							blnUniqueMatch = True
							Exit For
						End If
					Next intMatchIndex
				Else
					' Not using SLiC score; when we performed the peak matching, we used an ellipse or rectangle to find matching features
					' This feature is unique if it only has one match
					If intMatchCount = 1 Then
						blnUniqueMatch = True
					Else
						blnUniqueMatch = False
					End If
				End If

			End If
		Else
			intMatchCount = 0
		End If

		Return blnUniqueMatch

	End Function

	Private Sub ExportThresholds(ByVal srOutFile As System.IO.StreamWriter, ByVal intThresholdIndex As Integer, ByRef SearchThresholds As clsPeakMatchingClass.clsSearchThresholds)

		Dim strDelimiter As String
		Dim strLineOut As String

		strDelimiter = "; "

		With SearchThresholds
			' Write the thresholds
			strLineOut = "Threshold Index: " & (intThresholdIndex + 1).ToString
			strLineOut &= strDelimiter & "Mass Tolerance: +- "
			Select Case .MassTolType
				Case clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.Absolute
					strLineOut &= Math.Round(.MassTolerance, 5).ToString & " Da"
				Case clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.PPM
					strLineOut &= Math.Round(.MassTolerance, 2).ToString & " ppm"
				Case Else
					strLineOut &= "Unknown mass tolerance mode"
			End Select

#If IncludePNNLNETRoutines Then
			strLineOut &= strDelimiter & "NET Tolerance: +- " & Math.Round(.NETTolerance, 4).ToString
#Else
            strLineOut &= strDelimiter & "Time Tolerance: +- " & Math.Round(.NETTolerance, 4).ToString
#End If

			If mUseSLiCScoreForUniqueness Then
				strLineOut &= strDelimiter & "Minimum SLiC Score: " & Math.Round(mPeptideUniquenessBinningSettings.MinimumSLiCScore, 3).ToString & "; Max search distance multiplier: " & Math.Round(.SLiCScoreMaxSearchDistanceMultiplier, 1).ToString
			Else
				If mUseEllipseSearchRegion Then
					strLineOut &= strDelimiter & "Minimum SLiC Score: N/A; using ellipse to find matching features"
				Else
					strLineOut &= strDelimiter & "Minimum SLiC Score: N/A; using rectangle to find matching features"
				End If
			End If

			srOutFile.WriteLine(strLineOut)
		End With

	End Sub

	Public Function GenerateUniquenessStats(ByVal strProteinInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strOutputFilenameBase As String) As Boolean

		Dim intProgressStepCount As Integer
		Dim intProgressStep As Integer

		Dim intThresholdIndex As Integer

		Dim objFeaturesToIdentify As clsPeakMatchingClass.PMFeatureInfoClass

		Dim blnSuccess As Boolean
		Dim blnSearchAborted As Boolean = False

		Dim objRangeSearch As clsSearchRange = Nothing

		Dim srPMResultsOutFile As System.IO.StreamWriter = Nothing
		Dim srPeptideUniquenessOutFile As System.IO.StreamWriter = Nothing
		Dim srProteinStatsOutFile As System.IO.StreamWriter = Nothing

		Try

			If System.IO.Path.HasExtension(strOutputFilenameBase) Then
				' Remove the extension
				strOutputFilenameBase = System.IO.Path.ChangeExtension(strOutputFilenameBase, Nothing)
			End If

			'----------------------------------------------------
			' Initialize the progress bar
			'----------------------------------------------------
			intProgressStep = 0
			intProgressStepCount = 1 + mThresholdLevels.Length * 3

			MyBase.LogMessage("Caching data in memory", eMessageTypeConstants.Normal)
			MyBase.LogMessage("Caching peak matching results in memory", eMessageTypeConstants.Normal)

			'----------------------------------------------------
			' Load the proteins and digest them, or simply load the peptides
			'----------------------------------------------------
			MyBase.LogMessage("Load proteins or peptides from " & System.IO.Path.GetFileName(strProteinInputFilePath), eMessageTypeConstants.Normal)
			blnSuccess = LoadProteinsOrPeptides(strProteinInputFilePath)

			' ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True

			intProgressStep = 1
			Me.UpdateProgress(CSng(intProgressStep / intProgressStepCount * 100))

		Catch ex As Exception
			SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ErrorReadingInputFile, ex)
			blnSuccess = False
		End Try

		If blnSuccess AndAlso mComparisonPeptideInfo.Count = 0 Then
			SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ProteinsNotFoundInInputFile, True)
			blnSuccess = False
		ElseIf Not blnSuccess OrElse Me.AbortProcessing Then
			blnSuccess = False
		Else
			Try
				'----------------------------------------------------
				' Search mComparisonPeptideInfo against itself
				' Since mComparisonPeptideInfo is class type PMComparisonFeatureInfoClass, which is a 
				'  derived class of PMFeatureInfoClass, we can simply link the two objects
				' This way, we don't use double the memory necessary
				'----------------------------------------------------

				UpdateProgress("Initializing structures")

				' Initialize objFeaturesToIdentify by linking to mComparisonPeptideInfo
				objFeaturesToIdentify = mComparisonPeptideInfo

				Try
					'----------------------------------------------------
					' Compare objFeaturesToIdentify to mComparisonPeptideInfo for each threshold level
					'----------------------------------------------------

					'----------------------------------------------------
					' Validate mThresholdLevels to assure at least one threshold exists
					'----------------------------------------------------

					If mThresholdLevels.Length <= 0 Then
						InitializeThresholdLevels(mThresholdLevels, 1, False)
					End If

					For intThresholdIndex = 0 To mThresholdLevels.Length - 1
						If mThresholdLevels(intThresholdIndex) Is Nothing Then
							If intThresholdIndex = 0 Then
								' Need at least one set of thresholds
								mThresholdLevels(intThresholdIndex) = New clsPeakMatchingClass.clsSearchThresholds
							Else
								ReDim Preserve mThresholdLevels(intThresholdIndex - 1)
								Exit For
							End If
						End If
					Next

					'----------------------------------------------------
					' Initialize objRangeSearch
					'----------------------------------------------------

					MyBase.LogMessage("Uniqueness Stats processing starting, Threshold Count = " & mThresholdLevels.Length.ToString, eMessageTypeConstants.Normal)

					If Not clsPeakMatchingClass.FillRangeSearchObject(objRangeSearch, mComparisonPeptideInfo) Then
						blnSuccess = False
					Else

						'----------------------------------------------------
						' Initialize the peak matching class
						'----------------------------------------------------

						mPeakMatchingClass = New clsPeakMatchingClass()
						With mPeakMatchingClass
							.MaxPeakMatchingResultsPerFeatureToSave = mMaxPeakMatchingResultsPerFeatureToSave
							.UseMaxSearchDistanceMultiplierAndSLiCScore = mUseSLiCScoreForUniqueness
							.UseEllipseSearchRegion = mUseEllipseSearchRegion
						End With

						'----------------------------------------------------
						' Initialize the output files if combining all results 
						' in a single file for each type of result
						'----------------------------------------------------

						If Not mCreateSeparateOutputFileForEachThreshold Then
							InitializePeptideAndProteinResultsFiles(strOutputFolderPath, strOutputFilenameBase, mThresholdLevels, srPeptideUniquenessOutFile, srProteinStatsOutFile)

							If mSavePeakMatchingResults Then
								InitializePMResultsFile(strOutputFolderPath, strOutputFilenameBase, mThresholdLevels, mComparisonPeptideInfo.Count, srPMResultsOutFile)
							End If
						End If

						For intThresholdIndex = 0 To mThresholdLevels.Length - 1

							UpdateProgress("Generating uniqueness stats, threshold " & (intThresholdIndex + 1).ToString & " / " & mThresholdLevels.Length.ToString)

							UpdateSubtaskProgress("Finding matching peptides for given search thresholds", 0)

							' Perform the actual peak matching
							MyBase.LogMessage("Threshold " & (intThresholdIndex + 1).ToString & ", IdentifySequences", eMessageTypeConstants.Normal)
							blnSuccess = mPeakMatchingClass.IdentifySequences(mThresholdLevels(intThresholdIndex), objFeaturesToIdentify, mComparisonPeptideInfo, mPeptideMatchResults, objRangeSearch)
							If Not blnSuccess Then Exit For

							If mSavePeakMatchingResults Then
								' Write out the raw peak matching results
								blnSuccess = ExportPeakMatchingResults(mThresholdLevels(intThresholdIndex), intThresholdIndex, mComparisonPeptideInfo.Count, mPeptideMatchResults, strOutputFolderPath, strOutputFilenameBase, srPMResultsOutFile)
								If Not blnSuccess Then Exit For
							End If

							intProgressStep += 1
							UpdateProgress(CSng(intProgressStep / intProgressStepCount * 100))

							' Summarize the results by peptide
							MyBase.LogMessage("Threshold " & (intThresholdIndex + 1).ToString & ", SummarizeResultsByPeptide", eMessageTypeConstants.Normal)
							blnSuccess = SummarizeResultsByPeptide(mThresholdLevels(intThresholdIndex), intThresholdIndex, objFeaturesToIdentify, mPeptideMatchResults, strOutputFolderPath, strOutputFilenameBase, srPeptideUniquenessOutFile)
							If Not blnSuccess Then Exit For

							intProgressStep += 1
							UpdateProgress(CSng(intProgressStep / intProgressStepCount * 100))

							' Summarize the results by protein
							MyBase.LogMessage("Threshold " & (intThresholdIndex + 1).ToString & ", SummarizeResultsByProtein", eMessageTypeConstants.Normal)
							blnSuccess = SummarizeResultsByProtein(mThresholdLevels(intThresholdIndex), intThresholdIndex, objFeaturesToIdentify, mPeptideMatchResults, strOutputFolderPath, strOutputFilenameBase, srProteinStatsOutFile)
							If Not blnSuccess Then Exit For

							intProgressStep += 1
							UpdateProgress(CSng(intProgressStep / intProgressStepCount * 100))

							If Not blnSuccess OrElse Me.AbortProcessing Then
								blnSuccess = False
								Exit For
							End If

							blnSuccess = True
						Next intThresholdIndex

					End If

				Catch ex As Exception
					SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ErrorIdentifyingSequences, ex)
					blnSuccess = False
				End Try

				UpdateProgress(100)

			Catch ex As Exception
				SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ErrorIdentifyingSequences, ex)
				blnSuccess = False
			Finally
				Try
					If Not srPMResultsOutFile Is Nothing Then srPMResultsOutFile.Close()
					If Not srPeptideUniquenessOutFile Is Nothing Then srPeptideUniquenessOutFile.Close()
					If Not srProteinStatsOutFile Is Nothing Then srProteinStatsOutFile.Close()
					mPeakMatchingClass = Nothing
				Catch ex As Exception
					' Ignore any errors closing files
				End Try
			End Try

		End If

		If Not blnSearchAborted And blnSuccess Then
			MyBase.LogMessage("Uniqueness Stats processing complete", eMessageTypeConstants.Normal)
		End If

		Return blnSuccess

	End Function

	Public Overrides Function GetDefaultExtensionsToParse() As String()
		Dim strExtensionsToParse(1) As String

		strExtensionsToParse(0) = ".fasta"
		strExtensionsToParse(1) = ".txt"

		Return strExtensionsToParse

	End Function

	Public Overrides Function GetErrorMessage() As String
		' Returns "" if no error

		Dim strErrorMessage As String

		If MyBase.ErrorCode = clsProcessFilesBaseClass.eProcessFilesErrorCodes.LocalizedError Or _
		   MyBase.ErrorCode = clsProcessFilesBaseClass.eProcessFilesErrorCodes.NoError Then
			Select Case mLocalErrorCode
				Case eProteinDigestionSimulatorErrorCodes.NoError
					strErrorMessage = ""

				Case eProteinDigestionSimulatorErrorCodes.ProteinDigestionSimulatorSectionNotFound
					strErrorMessage = "The section " & clsParseProteinFile.XML_SECTION_OPTIONS & " was not found in the parameter file"

				Case eProteinDigestionSimulatorErrorCodes.ErrorReadingInputFile
					strErrorMessage = "Error reading input file"
				Case eProteinDigestionSimulatorErrorCodes.ProteinsNotFoundInInputFile
					strErrorMessage = "No proteins were found in the input file (make sure the Column Order is correct on the File Format Options tab)"

				Case eProteinDigestionSimulatorErrorCodes.ErrorIdentifyingSequences
					strErrorMessage = "Error identifying sequences"

				Case eProteinDigestionSimulatorErrorCodes.ErrorWritingOutputFile
					strErrorMessage = "Error writing to one of the output files"
				Case eProteinDigestionSimulatorErrorCodes.UserAbortedSearch
					strErrorMessage = "User aborted search"

				Case eProteinDigestionSimulatorErrorCodes.UnspecifiedError
					strErrorMessage = "Unspecified localized error"
				Case Else
					' This shouldn't happen
					strErrorMessage = "Unknown error state"
			End Select
		Else
			strErrorMessage = MyBase.GetBaseClassErrorMessage()
		End If

		If Not mLastErrorMessage Is Nothing AndAlso mLastErrorMessage.Length > 0 Then
			strErrorMessage &= ControlChars.NewLine & mLastErrorMessage
		End If

		Return strErrorMessage

	End Function

	Private Function GetNextUniqueSequenceID(ByVal strSequence As String, ByRef htMasterSequences As Hashtable, ByRef intNextUniqueIDForMasterSeqs As Integer) As Integer
		Dim intUniqueSeqID As Integer

		Try
			If htMasterSequences.ContainsKey(strSequence) Then
				intUniqueSeqID = CInt(htMasterSequences(strSequence))
			Else
				htMasterSequences.Add(strSequence, intNextUniqueIDForMasterSeqs)
				intUniqueSeqID = intNextUniqueIDForMasterSeqs
			End If
			intNextUniqueIDForMasterSeqs += 1
		Catch ex As Exception
			intUniqueSeqID = 0
		End Try

		Return intUniqueSeqID
	End Function

	Public Sub GetPeptideUniquenessMassRangeForBinning(ByRef MassMinimum As Single, ByRef MassMaximum As Single)
		With mPeptideUniquenessBinningSettings
			MassMinimum = .MassMinimum
			MassMaximum = .MassMaximum
		End With
	End Sub

	Private Function GetPeptidesUniquelyIdentifiedCountByProteinID(ByVal intProteinID As Integer) As Integer
		Dim objDataRows() As System.Data.DataRow

		objDataRows = mProteinToIdentifiedPeptideMappingTable.Select(PROTEIN_ID_COLUMN & " = " & intProteinID.ToString)
		If Not objDataRows Is Nothing Then
			Return objDataRows.Length
		Else
			Return 0
		End If

	End Function

	Private Sub InitializeBinnedStats(ByRef udtStatsBinned As udtBinnedPeptideCountStatsType, ByVal intBinCount As Integer)

		Dim intBinIndex As Integer

		With udtStatsBinned
			.BinCount = intBinCount
			ReDim .Bins(.BinCount - 1)

			For intBinIndex = 0 To .BinCount - 1
				.Bins(intBinIndex).MassBinStart = .Settings.MassMinimum + .Settings.MassBinSizeDa * intBinIndex
				.Bins(intBinIndex).MassBinEnd = .Settings.MassMinimum + .Settings.MassBinSizeDa * (intBinIndex + 1)
				With .Bins(intBinIndex)
					.PercentUnique = 0
					.UniqueResultIDCount = 0
					.NonUniqueResultIDCount = 0
					ReDim .ResultIDCountDistribution(Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave))
				End With
			Next intBinIndex
		End With

	End Sub

	Private Sub InitializePeptideAndProteinResultsFiles(ByVal strOutputFolderPath As String, ByVal strOutputFilenameBase As String, ByRef objThresholds() As clsPeakMatchingClass.clsSearchThresholds, ByRef srPeptideUniquenessOutFile As System.IO.StreamWriter, ByRef srProteinStatsOutFile As System.IO.StreamWriter)
		' Initialize the output file so that the peptide and protein summary results for all thresholds can be saved in the same file

		Dim strWorkingFilenameBase As String
		Dim strOutputFilePath As String
		Dim intThresholdIndex As Integer

		'----------------------------------------------------
		' Create the peptide uniqueness stats files
		'----------------------------------------------------

		strWorkingFilenameBase = strOutputFilenameBase & "_PeptideStatsBinned.txt"
		strOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, strWorkingFilenameBase)

		srPeptideUniquenessOutFile = New System.IO.StreamWriter(strOutputFilePath)

		'----------------------------------------------------
		' Create the protein stats file
		'----------------------------------------------------

		strWorkingFilenameBase = strOutputFilenameBase & "_ProteinStats.txt"
		strOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, strWorkingFilenameBase)

		srProteinStatsOutFile = New System.IO.StreamWriter(strOutputFilePath)

		'----------------------------------------------------
		' Write the thresholds
		'----------------------------------------------------

		For intThresholdIndex = 0 To objThresholds.Length - 1
			ExportThresholds(srPeptideUniquenessOutFile, intThresholdIndex, objThresholds(intThresholdIndex))
			ExportThresholds(srProteinStatsOutFile, intThresholdIndex, objThresholds(intThresholdIndex))
		Next intThresholdIndex

		srPeptideUniquenessOutFile.WriteLine()
		SummarizeResultsByPeptideWriteHeaders(srPeptideUniquenessOutFile, mCreateSeparateOutputFileForEachThreshold)

		srProteinStatsOutFile.WriteLine()
		SummarizeResultsByProteinWriteHeaders(srProteinStatsOutFile, mCreateSeparateOutputFileForEachThreshold)

	End Sub

	Private Sub InitializePMResultsFile(ByVal strOutputFolderPath As String, ByVal strOutputFilenameBase As String, ByRef objThresholds() As clsPeakMatchingClass.clsSearchThresholds, ByVal intComparisonFeaturesCount As Integer, ByRef srOutFile As System.IO.StreamWriter)
		' Initialize the output file so that the peak matching results for all thresholds can be saved in the same file

		Dim strWorkingFilenameBase As String
		Dim strOutputFilePath As String
		Dim intThresholdIndex As Integer

		' Combine all of the data together in one output file
		strWorkingFilenameBase = strOutputFilenameBase & "_PMResults.txt"
		strOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, strWorkingFilenameBase)

		srOutFile = New System.IO.StreamWriter(strOutputFilePath)

		srOutFile.WriteLine("Comparison feature count: " & intComparisonFeaturesCount.ToString)

		For intThresholdIndex = 0 To objThresholds.Length - 1
			ExportThresholds(srOutFile, intThresholdIndex, objThresholds(intThresholdIndex))
		Next intThresholdIndex

		srOutFile.WriteLine()

		ExportPeakMatchingResultsWriteHeaders(srOutFile, mCreateSeparateOutputFileForEachThreshold)

	End Sub

	Private Sub InitializeLocalVariables()

		mDigestSequences = False
		mCysPeptidesOnly = False
		mOutputFileDelimiter = ControlChars.Tab

		mSavePeakMatchingResults = False
		mMaxPeakMatchingResultsPerFeatureToSave = 3

		With mPeptideUniquenessBinningSettings
			.AutoDetermineMassRange = True
			.MassBinSizeDa = 25
			.MassMinimum = 400
			.MassMaximum = 6000
			.MinimumSLiCScore = 0.99
		End With

		mUseSLiCScoreForUniqueness = True
		mUseEllipseSearchRegion = True

		mCreateSeparateOutputFileForEachThreshold = False

		mLocalErrorCode = eProteinDigestionSimulatorErrorCodes.NoError
		mLastErrorMessage = String.Empty

		InitializeProteinToIdentifiedPeptideMappingTable()

		InitializeThresholdLevels(mThresholdLevels, 1, False)
		InitializeProteinFileParser()

		''mUseSqlServerDBToCacheData = False
		''mUseSqlServerForMatchResults = False

		''mUseBulkInsert = False

		''mSqlServerConnectionString = SharedVBNetRoutines.ADONetRoutines.DEFAULT_CONNECTION_STRING_NO_PROVIDER
		''mTableNameFeaturesToIdentify = clsPeakMatchingClass.PMFeatureInfoClass.DEFAULT_FEATURE_INFO_TABLE_NAME
		''mTableNameComparisonPeptides = clsPeakMatchingClass.PMComparisonFeatureInfoClass.DEFAULT_COMPARISON_FEATURE_INFO_TABLE_NAME

		''mTableNameProteinInfo = clsProteinInfo.DEFAULT_PROTEIN_INFO_TABLE_NAME
		''mTableNameProteinToPeptideMap = clsProteinInfo.DEFAULT_PROTEIN_TO_PEPTIDE_MAP_TABLE_NAME

	End Sub

	Private Sub InitializeProteinToIdentifiedPeptideMappingTable()

		If mProteinToIdentifiedPeptideMappingTable Is Nothing Then
			mProteinToIdentifiedPeptideMappingTable = New System.Data.DataTable

			'---------------------
			' Protein stats uniquely identified peptides table
			'---------------------
			SharedVBNetRoutines.ADONetRoutines.AppendColumnIntegerToTable(mProteinToIdentifiedPeptideMappingTable, PROTEIN_ID_COLUMN)
			SharedVBNetRoutines.ADONetRoutines.AppendColumnStringToTable(mProteinToIdentifiedPeptideMappingTable, PEPTIDE_ID_MATCH_COLUMN)

			' Define the PROTEIN_ID_COLUMN AND PEPTIDE_ID_COLUMN columns to be the primary key
			With mProteinToIdentifiedPeptideMappingTable
				.PrimaryKey = New System.Data.DataColumn() {.Columns(PROTEIN_ID_COLUMN), .Columns(PEPTIDE_ID_MATCH_COLUMN)}
			End With

		Else
			mProteinToIdentifiedPeptideMappingTable.Clear()
		End If

	End Sub

	'' The following was created to test the speed and performance when dealing with large dataset tables
	''Private Sub FillWithLotsOfData(ByRef dsDataset as System.Data.DataSet)
	''    Const MAX_FEATURE_COUNT As Integer = 300000

	''    Dim objRnd As New Random
	''    Dim intNewFeatureID As Integer
	''    Dim intIndex As Integer
	''    Dim intIndexEnd As Integer

	''    Dim drNewRow as System.Data.DataRow
	''    Dim objProgressForm As New ProgressFormNET.frmProgress
	''    Dim dtStartTime As DateTime

	''    intIndexEnd = CInt(MAX_FEATURE_COUNT * 1.5)
	''    objProgressForm.InitializeProgressForm("Populating dataset table", 0, intIndexEnd, True)
	''    objProgressForm.Visible = True
	''    Windows.Forms.Application.DoEvents()

	''    dtStartTime = System.DateTime.UtcNow

	''    With dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME)

	''        For intIndex = 0 To intIndexEnd
	''            intNewFeatureID = objRnd.Next(0, MAX_FEATURE_COUNT)

	''            ' Look for existing entry in table
	''            If Not .Rows.Contains(intNewFeatureID) Then
	''                drNewRow = .NewRow
	''                drNewRow(COMPARISON_FEATURE_ID_COLUMN) = intNewFeatureID
	''                drNewRow(FEATURE_NAME_COLUMN) = "Feature" & intNewFeatureID.ToString
	''                drNewRow(MASS_COLUMN) = intNewFeatureID / CSng(MAX_FEATURE_COUNT) * 1000
	''                drNewRow(NET_COLUMN) = objRnd.Next(0, 1000) / 1000.0
	''                drNewRow(NET_STDEV_COLUMN) = objRnd.Next(0, 1000) / 10000.0
	''                drNewRow(DISCRIMINANT_SCORE_COLUMN) = objRnd.Next(0, 1000) / 1000.0
	''                .Rows.Add(drNewRow)
	''            End If

	''            If intIndex Mod 100 = 0 Then
	''                objProgressForm.UpdateProgressBar(intIndex)
	''                Windows.Forms.Application.DoEvents()

	''                If objProgressForm.KeyPressAbortProcess Then Exit For
	''            End If
	''        Next intIndex
	''    End With

	''    Windows.Forms.MessageBox.Show("Elapsed time: " & Math.Round(System.DateTime.UtcNow.Subtract(dtStartTime).TotalSeconds, 2).ToString & " seconds", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
	''End Sub

	Public Sub InitializeProteinFileParser(Optional ByVal blnResetToDefaults As Boolean = False)

		If mProteinFileParser Is Nothing Then
			mProteinFileParser = New clsParseProteinFile
			blnResetToDefaults = True
		End If

		If blnResetToDefaults Then
			With mProteinFileParser
				.ComputeProteinMass = True
				.ElementMassMode = PeptideSequenceClass.ElementModeConstants.IsotopicMass

				.CreateDigestedProteinOutputFile = False
				.CreateProteinOutputFile = False

				.DelimitedFileFormatCode = ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence

				With .DigestionOptions
					.CleavageRuleID = clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin
					.MaxMissedCleavages = 1

					.MinFragmentResidueCount = 0
					.MinFragmentMass = 600
					.MaxFragmentMass = 3000

					.RemoveDuplicateSequences = True

					.IncludePrefixAndSuffixResidues = False
				End With

				.ShowMessages = MyBase.ShowMessages
			End With
		End If

	End Sub

	Private Sub InitializeThresholdLevels(ByRef objThresholds() As clsPeakMatchingClass.clsSearchThresholds, ByVal intLevels As Integer, ByVal blnPreserve As Boolean)
		Dim intIndex As Integer

		If intLevels < 1 Then intLevels = 1

		If blnPreserve AndAlso objThresholds.Length > 0 Then
			ReDim Preserve objThresholds(intLevels - 1)
		Else
			ReDim objThresholds(intLevels - 1)
		End If

		For intIndex = 0 To intLevels - 1
			If objThresholds(intIndex) Is Nothing Then
				objThresholds(intIndex) = New clsPeakMatchingClass.clsSearchThresholds
			ElseIf blnPreserve And intIndex = intLevels - 1 Then
				' Initialize this level to defaults
				objThresholds(intIndex).ResetToDefaults()
			End If
		Next

	End Sub

	Private Function LoadParameterFileSettings(ByVal strParameterFilePath As String) As Boolean

		Try

			If strParameterFilePath Is Nothing OrElse strParameterFilePath.Length = 0 Then
				' No parameter file specified; nothing to load
				Return True
			End If

			If Not System.IO.File.Exists(strParameterFilePath) Then
				' See if strParameterFilePath points to a file in the same directory as the application
				strParameterFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), System.IO.Path.GetFileName(strParameterFilePath))
				If Not System.IO.File.Exists(strParameterFilePath) Then
					MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.ParameterFileNotFound)
					Return False
				End If
			End If

			mProteinFileParser.LoadParameterFileSettings(strParameterFilePath)

		Catch ex As Exception
			If MyBase.ShowMessages Then
				System.Windows.Forms.MessageBox.Show("Error in LoadParameterFileSettings: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
			Else
				Throw New System.Exception("Error in LoadParameterFileSettings", ex)
			End If
			Return False
		End Try

		Return True

	End Function

	Private Function LoadProteinsOrPeptides(ByVal strProteinInputFilePath As String) As Boolean

		Dim eDelimitedFileFormatCode As ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode

		Dim blnSuccess As Boolean
		Dim blnDigestSequences As Boolean
		Dim blnDelimitedFileHasMassAndNET As Boolean

		Try
			If Not mComparisonPeptideInfo Is Nothing Then
				mComparisonPeptideInfo = Nothing
			End If

			mComparisonPeptideInfo = New clsPeakMatchingClass.PMComparisonFeatureInfoClass

			If Not mProteinInfo Is Nothing Then
				mProteinInfo = Nothing
			End If

			mProteinInfo = New clsProteinInfo

			' Possibly initialize the ProteinFileParser object
			If mProteinFileParser Is Nothing Then
				InitializeProteinFileParser()
			End If

			' Note that ProteinFileParser is exposed as public so that options can be set directly in it

			If clsParseProteinFile.IsFastaFile(strProteinInputFilePath) Or mProteinFileParser.AssumeFastaFile Then
				MyBase.LogMessage("Input file format = Fasta", eMessageTypeConstants.Normal)
				blnDigestSequences = True
			Else
				eDelimitedFileFormatCode = mProteinFileParser.DelimitedFileFormatCode
				MyBase.LogMessage("Input file format = " & eDelimitedFileFormatCode.ToString, eMessageTypeConstants.Normal)
				blnDigestSequences = mDigestSequences
			End If

			Select Case eDelimitedFileFormatCode
				Case ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID, _
					 ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence
					' Reading peptides from a delimited file; blnDigestSequences is typically False, but could be true
					blnDelimitedFileHasMassAndNET = False
				Case ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, _
					 ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore, _
					 ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET
					' Reading peptides from a delimited file; blnDigestSequences is typically False, but could be true
					blnDelimitedFileHasMassAndNET = True
				Case Else
					' Force digest Sequences to true
					blnDigestSequences = True
					blnDelimitedFileHasMassAndNET = False
			End Select

			MyBase.LogMessage("Digest sequences = " & blnDigestSequences.ToString, eMessageTypeConstants.Normal)

			If blnDigestSequences Then
				blnSuccess = LoadProteinsOrPeptidesWork(strProteinInputFilePath)
			Else
				' We can assume peptides in the input file are already digested and thus do not need to pre-cache the entire file in memory
				' Load the data directly using this class instead

				' Load each peptide using ProteinFileReader.DelimitedFileReader
				blnSuccess = LoadPeptidesFromDelimitedFile(strProteinInputFilePath, blnDelimitedFileHasMassAndNET)
				' ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True
			End If

			If blnSuccess Then
				MyBase.LogMessage("Loaded " & mComparisonPeptideInfo.Count & " peptides corresponding to " & mProteinInfo.Count & " proteins", eMessageTypeConstants.Normal)
			End If

		Catch ex As Exception
			SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ErrorReadingInputFile, ex)
			blnSuccess = False
		End Try

		Return blnSuccess
	End Function

	Private Function LoadPeptidesFromDelimitedFile(ByVal strProteinInputFilePath As String, _
												   ByVal blnDelimitedFileHasMassAndNET As Boolean) As Boolean

		' Assumes the peptides in the input file are already digested and that they already have unique ID values generated
		' They could optionally have Mass and NET values defined

		Dim objDelimitedFileReader As ProteinFileReader.DelimitedFileReader = Nothing

		Dim objNewPeptide As clsInSilicoDigest.PeptideInfoClass

		Dim blnSuccess As Boolean
		Dim blnInputPeptideFound As Boolean

		Dim intInputFileLinesRead As Integer
		Dim intInputFileLineSkipCount As Integer
		Dim strSkipMessage As String

		Try
			objDelimitedFileReader = New ProteinFileReader.DelimitedFileReader
			With objDelimitedFileReader
				.Delimiter = mProteinFileParser.InputFileDelimiter
				.DelimitedFileFormatCode = mProteinFileParser.DelimitedFileFormatCode
			End With

			' Verify that the input file exists
			If Not System.IO.File.Exists(strProteinInputFilePath) Then
				MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidInputFilePath)
				blnSuccess = False
				Exit Try
			End If

			' Attempt to open the input file
			If Not objDelimitedFileReader.OpenFile(strProteinInputFilePath) Then
				SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ErrorReadingInputFile)
				blnSuccess = False
				Exit Try
			End If

			ResetProgress("Parsing digested input file")

			' Read each peptide in the input file and add to mComparisonPeptideInfo

			' Also need to initialize objNewPeptide
			objNewPeptide = New clsInSilicoDigest.PeptideInfoClass

			Select Case objDelimitedFileReader.DelimitedFileFormatCode
				Case ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, _
					 ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore, _
					 ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET
					' Do not use the computed mass and NET
					blnDelimitedFileHasMassAndNET = True
				Case Else
					blnDelimitedFileHasMassAndNET = False
			End Select

			' Always auto compute the NET and Mass in the objNewPeptide class
			' However, if blnDelimitedFileHasMassAndNET = True and valid Mass and NET values were read from the text file,
			'  they they are passed to AddOrUpdatePeptide rather than the computed values
			objNewPeptide.AutoComputeNET = True

			Do
				blnInputPeptideFound = objDelimitedFileReader.ReadNextProteinEntry()

				intInputFileLinesRead = objDelimitedFileReader.LinesRead
				intInputFileLineSkipCount += objDelimitedFileReader.LineSkipCount

				If blnInputPeptideFound Then

					With objNewPeptide
						.SequenceOneLetter = objDelimitedFileReader.ProteinSequence

						If Not blnDelimitedFileHasMassAndNET OrElse objDelimitedFileReader.PeptideMass = 0 And objDelimitedFileReader.PeptideNET = 0 Then
							AddOrUpdatePeptide(objDelimitedFileReader.EntryUniqueID, .Mass, .NET, 0, 0, objDelimitedFileReader.ProteinName, clsProteinInfo.eCleavageStateConstants.Unknown, String.Empty)
						Else
							AddOrUpdatePeptide(objDelimitedFileReader.EntryUniqueID, objDelimitedFileReader.PeptideMass, objDelimitedFileReader.PeptideNET, _
											   objDelimitedFileReader.PeptideNETStDev, objDelimitedFileReader.PeptideDiscriminantScore, _
											   objDelimitedFileReader.ProteinName, clsProteinInfo.eCleavageStateConstants.Unknown, String.Empty)

							' ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True
						End If

					End With

					UpdateProgress(objDelimitedFileReader.PercentFileProcessed())
					If Me.AbortProcessing Then Exit Do
				End If

			Loop While blnInputPeptideFound

			If intInputFileLineSkipCount > 0 Then
				strSkipMessage = "Note that " & intInputFileLineSkipCount.ToString & " out of " & intInputFileLinesRead.ToString & " lines were skipped in the input file because they did not match the column order defined on the File Format Options Tab (" & mProteinFileParser.DelimitedFileFormatCode.ToString & ")"
				MyBase.LogMessage(strSkipMessage, eMessageTypeConstants.Warning)
				mLastErrorMessage = String.Copy(strSkipMessage)
			End If

			blnSuccess = Not (Me.AbortProcessing)

		Catch ex As Exception
			SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ErrorReadingInputFile, ex)
			blnSuccess = False
		Finally
			objDelimitedFileReader.CloseFile()
			objDelimitedFileReader = Nothing
		End Try

		Return blnSuccess

	End Function

	Private Function LoadProteinsOrPeptidesWork(ByVal strProteinInputFilePath As String) As Boolean

		Dim blnSuccess As Boolean
		Dim blnGenerateUniqueSequenceID As Boolean
		Dim blnIsFastaFile As Boolean

		Dim strSkipMessage As String = String.Empty

		Dim htMasterSequences As Hashtable = Nothing
		Dim intUniqueSeqID As Integer
		Dim intNextUniqueIDForMasterSeqs As Integer

		Dim intProteinIndex As Integer
		Dim intPeptideIndex As Integer

		Dim intPeptideCount As Integer
		Dim objPeptides() As clsInSilicoDigest.PeptideInfoClass = Nothing

		Dim objProteinOrPeptide As clsParseProteinFile.udtProteinInfoType

		Try

			If mProteinFileParser.GenerateUniqueIDValuesForPeptides Then
				' Need to generate unique sequence ID values
				blnGenerateUniqueSequenceID = True

				' Initialize htMasterSequences
				blnGenerateUniqueSequenceID = True
				htMasterSequences = New Hashtable
			Else
				blnGenerateUniqueSequenceID = False
			End If
			intNextUniqueIDForMasterSeqs = 1

			With mProteinFileParser

				If clsParseProteinFile.IsFastaFile(strProteinInputFilePath) Or .AssumeFastaFile Then
					blnIsFastaFile = True
				Else
					blnIsFastaFile = False
				End If


				' Disable mass calculation
				.ComputeProteinMass = False
				.CreateProteinOutputFile = False

				.ShowMessages = False

				If mCysPeptidesOnly Then
					.DigestionOptions.AminoAcidResidueFilterChars = New Char() {"C"c}
				Else
					.DigestionOptions.AminoAcidResidueFilterChars = New Char() {}
				End If

				' Load the proteins in the input file into memory
				blnSuccess = .ParseProteinFile(strProteinInputFilePath, "")
			End With

			If mProteinFileParser.InputFileLineSkipCount > 0 And Not blnIsFastaFile Then
				strSkipMessage = "Note that " & mProteinFileParser.InputFileLineSkipCount.ToString & " out of " & mProteinFileParser.InputFileLinesRead.ToString & " lines were skipped in the input file because they did not match the column order defined on the File Format Options Tab (" & mProteinFileParser.DelimitedFileFormatCode.ToString & ")"
				MyBase.LogMessage(strSkipMessage, eMessageTypeConstants.Warning)
				mLastErrorMessage = String.Copy(strSkipMessage)
			End If

			If mProteinFileParser.GetProteinCountCached <= 0 Then
				If blnSuccess Then
					' File was successfully loaded, but no proteins were found
					' This could easily happen for delimited files that only have a header line, or that don't have a format that matches what the user specified
					SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ProteinsNotFoundInInputFile)
					mLastErrorMessage = String.Empty
					blnSuccess = False
				Else
					SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ErrorReadingInputFile)
					mLastErrorMessage = "Error using ParseProteinFile to read the proteins from " & System.IO.Path.GetFileName(strProteinInputFilePath)
				End If

				If mProteinFileParser.InputFileLineSkipCount > 0 And Not blnIsFastaFile Then
					If mLastErrorMessage.Length > 0 Then
						mLastErrorMessage &= ". "
					End If
					mLastErrorMessage &= strSkipMessage
				End If

			ElseIf blnSuccess Then

				blnSuccess = False

				' Re-enable mass calculation
				mProteinFileParser.ComputeProteinMass = True

				ResetProgress("Digesting proteins in input file")

				For intProteinIndex = 0 To mProteinFileParser.GetProteinCountCached - 1
					objProteinOrPeptide = mProteinFileParser.GetCachedProtein(intProteinIndex)

					intPeptideCount = mProteinFileParser.GetDigestedPeptidesForCachedProtein(intProteinIndex, objPeptides, mProteinFileParser.DigestionOptions)

					If intPeptideCount > 0 Then
						For intPeptideIndex = 0 To intPeptideCount - 1
							If blnGenerateUniqueSequenceID Then
								intUniqueSeqID = GetNextUniqueSequenceID(objPeptides(intPeptideIndex).SequenceOneLetter, htMasterSequences, intNextUniqueIDForMasterSeqs)
							Else
								' Must assume each sequence is unique; probably an incorrect assumption
								intUniqueSeqID = intNextUniqueIDForMasterSeqs
								intNextUniqueIDForMasterSeqs += 1
							End If

							With objPeptides(intPeptideIndex)
								AddOrUpdatePeptide(intUniqueSeqID, .Mass, .NET, 0, 0, objProteinOrPeptide.Name, clsProteinInfo.eCleavageStateConstants.Unknown, .PeptideName)
							End With
						Next
					End If

					UpdateProgress(CSng((intProteinIndex + 1) / mProteinFileParser.GetProteinCountCached * 100))

					If Me.AbortProcessing Then Exit For
				Next intProteinIndex

				blnSuccess = Not (Me.AbortProcessing)
			End If

		Catch ex As Exception
			SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.ErrorReadingInputFile, ex)
			blnSuccess = False
		End Try

		Return blnSuccess

	End Function

	Private Sub InitializeBinningRanges(ByRef objThresholds As clsPeakMatchingClass.clsSearchThresholds, ByRef objFeaturesToIdentify As clsPeakMatchingClass.PMFeatureInfoClass, ByRef objPeptideMatchResults As clsPeakMatchingClass.PMFeatureMatchResultsClass, ByRef udtPeptideStatsBinned As udtBinnedPeptideCountStatsType)

		Dim sngFeatureMass As Single

		Dim intMatchIndex As Integer

		Dim intCurrentFeatureID As Integer
		Dim udtFeatureInfo As clsPeakMatchingClass.udtFeatureInfoType = New clsPeakMatchingClass.udtFeatureInfoType

		Dim udtMatchResultInfo As clsPeakMatchingClass.PMFeatureMatchResultsClass.udtPeakMatchingResultType

		Dim IntegerMass, intRemainder As Integer

		If udtPeptideStatsBinned.Settings.AutoDetermineMassRange Then

			' Examine objPeptideMatchResults to determine the minimum and maximum masses for features with matches

			' First, set the ranges to out-of-range values
			With udtPeptideStatsBinned.Settings
				.MassMinimum = Double.MaxValue
				.MassMaximum = Double.MinValue
			End With

			' Now examine .PeptideMatchResults()
			With objThresholds
				For intMatchIndex = 0 To objPeptideMatchResults.Count - 1

					objPeptideMatchResults.GetMatchInfoByRowIndex(intMatchIndex, intCurrentFeatureID, udtMatchResultInfo)

					objFeaturesToIdentify.GetFeatureInfoByFeatureID(intCurrentFeatureID, udtFeatureInfo)
					sngFeatureMass = CSng(udtFeatureInfo.Mass)

					If sngFeatureMass > udtPeptideStatsBinned.Settings.MassMaximum Then
						udtPeptideStatsBinned.Settings.MassMaximum = sngFeatureMass
					End If

					If sngFeatureMass < udtPeptideStatsBinned.Settings.MassMinimum Then
						udtPeptideStatsBinned.Settings.MassMinimum = sngFeatureMass
					End If

				Next intMatchIndex
			End With

			' Round the minimum and maximum masses to the nearest 100

			With udtPeptideStatsBinned.Settings
				If .MassMinimum = Double.MaxValue Or .MassMaximum = Double.MinValue Then
					' No matches were found; set these to defaults
					.MassMinimum = 400
					.MassMaximum = 6000
				End If

				IntegerMass = CInt(Math.Round(.MassMinimum, 0))
				Math.DivRem(IntegerMass, 100, intRemainder)
				IntegerMass -= intRemainder

				.MassMinimum = IntegerMass

				IntegerMass = CInt(Math.Round(.MassMaximum, 0))
				Math.DivRem(IntegerMass, 100, intRemainder)
				IntegerMass += (100 - intRemainder)

				.MassMaximum = IntegerMass

			End With

		End If

		' Determine BinCount; do not allow more than 1,000,000 bins
		With udtPeptideStatsBinned
			If .Settings.MassBinSizeDa = 0 Then .Settings.MassBinSizeDa = 1

			Do
				Try
					.BinCount = CInt(Math.Ceiling(.Settings.MassMaximum - .Settings.MassMinimum) / .Settings.MassBinSizeDa)
					If .BinCount > 1000000 Then
						.Settings.MassBinSizeDa *= 10
					End If
				Catch ex As Exception
					.BinCount = 1000000000
				End Try
			Loop While .BinCount > 1000000
		End With

	End Sub

	Private Function MassToBinIndex(ByVal ThisMass As Double, ByVal StartMass As Double, ByVal MassResolution As Double) As Integer
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
	Public Overloads Overrides Function ProcessFile(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strParameterFilePath As String, ByVal blnResetErrorCode As Boolean) As Boolean
		' Returns True if success, False if failure

		Dim ioFile As System.IO.FileInfo

		Dim strInputFilePathFull As String
		Dim strStatusMessage As String

		Dim blnSuccess As Boolean

		If blnResetErrorCode Then
			SetLocalErrorCode(eProteinDigestionSimulatorErrorCodes.NoError)
		End If

		If Not LoadParameterFileSettings(strParameterFilePath) Then
			strStatusMessage = "Parameter file load error: " & strParameterFilePath
			If MyBase.ShowMessages Then System.Windows.Forms.MessageBox.Show(strStatusMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
			Console.WriteLine(strStatusMessage)
			If MyBase.ErrorCode = clsProcessFilesBaseClass.eProcessFilesErrorCodes.NoError Then
				MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidParameterFile)
			End If
			Return False
		End If

		Try
			If strInputFilePath Is Nothing OrElse strInputFilePath.Length = 0 Then
				Console.WriteLine("Input file name is empty")
				MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidInputFilePath)
			Else

				Console.WriteLine()
				Console.WriteLine("Parsing " & System.IO.Path.GetFileName(strInputFilePath))

				If Not CleanupFilePaths(strInputFilePath, strOutputFolderPath) Then
					MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.FilePathError)
				Else
					Try
						' Obtain the full path to the input file
						ioFile = New System.IO.FileInfo(strInputFilePath)
						strInputFilePathFull = ioFile.FullName

						blnSuccess = GenerateUniquenessStats(strInputFilePathFull, strOutputFolderPath, System.IO.Path.GetFileNameWithoutExtension(strInputFilePath))

					Catch ex As Exception
						If MyBase.ShowMessages Then
							System.Windows.Forms.MessageBox.Show("Error calling ParseProteinFile: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
						Else
							Throw New System.Exception("Error calling ParseProteinFile", ex)
						End If
					End Try
				End If
			End If
		Catch ex As Exception
			If MyBase.ShowMessages Then
				System.Windows.Forms.MessageBox.Show("Error in ProcessFile: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
			Else
				Throw New System.Exception("Error in ProcessFile", ex)
			End If
		End Try

		Return blnSuccess

	End Function

	Protected Shadows Sub ResetProgress()
		Me.AbortProcessing = False
		MyBase.ResetProgress()
	End Sub

	Protected Shadows Sub ResetProgress(ByVal strProgressStepDescription As String)
		Me.AbortProcessing = False
		MyBase.ResetProgress(strProgressStepDescription)
	End Sub

	Public Function SetPeptideUniquenessMassRangeForBinning(ByVal MassMinimum As Single, ByVal MassMaximum As Single) As Boolean
		' Returns True if the minimum and maximum mass specified were valid

		With mPeptideUniquenessBinningSettings
			If MassMinimum < MassMaximum AndAlso MassMinimum >= 0 Then
				.MassMinimum = MassMinimum
				.MassMaximum = MassMaximum
				Return True
			Else
				Return False
			End If
		End With
	End Function

	Private Function SummarizeResultsByPeptide(ByRef objThresholds As clsPeakMatchingClass.clsSearchThresholds, _
											   ByVal intThresholdIndex As Integer, _
											   ByRef objFeaturesToIdentify As clsPeakMatchingClass.PMFeatureInfoClass, _
											   ByRef objPeptideMatchResults As clsPeakMatchingClass.PMFeatureMatchResultsClass, _
											   ByVal strOutputFolderPath As String, _
											   ByVal strOutputFilenameBase As String, _
											   ByRef srPeptideUniquenessOutFile As System.IO.StreamWriter) As Boolean

		Dim strWorkingFilenameBase As String
		Dim strOutputFilePath As String

		Dim intPeptideIndex As Integer
		Dim intFeaturesToIdentifyCount As Integer

		Dim udtFeatureInfo As clsPeakMatchingClass.udtFeatureInfoType = New clsPeakMatchingClass.udtFeatureInfoType

		Dim intMatchCount As Integer
		Dim intBinIndex As Integer
		Dim intPeptideSkipCount As Integer
		Dim intTotal As Integer

		Dim intMaxMatchCount As Integer

		Dim udtPeptideStatsBinned As udtBinnedPeptideCountStatsType = New udtBinnedPeptideCountStatsType

		Dim blnSuccess As Boolean

		Try

			' Copy the binning settings
			udtPeptideStatsBinned.Settings = mPeptideUniquenessBinningSettings

			' Define the minimum and maximum mass ranges, plus the number of bins required
			InitializeBinningRanges(objThresholds, objFeaturesToIdentify, objPeptideMatchResults, udtPeptideStatsBinned)

			intFeaturesToIdentifyCount = objFeaturesToIdentify.Count

			ResetProgress("Summarizing results by peptide")

			' --------------------------------------
			' Compute the stats for objThresholds
			' --------------------------------------

			' Define the maximum match count that will be tracked
			intMaxMatchCount = Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave)

			' Reserve memory for the bins, store the bin ranges for each bin, and reset the ResultIDs arrays
			InitializeBinnedStats(udtPeptideStatsBinned, udtPeptideStatsBinned.BinCount)

			' ToDo: When using Sql Server, switch this to use a SP that performs the same function as this For Loop, sotring the results in a table in the DB, but using Bulk Update queries

			MyBase.LogMessage("SummarizeResultsByPeptide starting, total feature count = " & intFeaturesToIdentifyCount.ToString, eMessageTypeConstants.Normal)

			intPeptideSkipCount = 0
			For intPeptideIndex = 0 To intFeaturesToIdentifyCount - 1
				If objFeaturesToIdentify.GetFeatureInfoByRowIndex(intPeptideIndex, udtFeatureInfo) Then
					intBinIndex = MassToBinIndex(udtFeatureInfo.Mass, udtPeptideStatsBinned.Settings.MassMinimum, udtPeptideStatsBinned.Settings.MassBinSizeDa)

					If intBinIndex < 0 Or intBinIndex > udtPeptideStatsBinned.BinCount - 1 Then
						' Peptide mass is out-of-range, ignore the result
						intPeptideSkipCount += 1
					Else
						If FeatureContainsUniqueMatch(udtFeatureInfo, objPeptideMatchResults, intMatchCount, mUseSLiCScoreForUniqueness, udtPeptideStatsBinned.Settings.MinimumSLiCScore) Then
							udtPeptideStatsBinned.Bins(intBinIndex).UniqueResultIDCount += 1
							udtPeptideStatsBinned.Bins(intBinIndex).ResultIDCountDistribution(1) += 1
						Else
							If intMatchCount > 0 Then
								' Feature has 1 or more matches, but they're not unique
								udtPeptideStatsBinned.Bins(intBinIndex).NonUniqueResultIDCount += 1
								If intMatchCount < intMaxMatchCount Then
									udtPeptideStatsBinned.Bins(intBinIndex).ResultIDCountDistribution(intMatchCount) += 1
								Else
									udtPeptideStatsBinned.Bins(intBinIndex).ResultIDCountDistribution(intMaxMatchCount) += 1
								End If
							End If
						End If

					End If
				End If

				If intPeptideIndex Mod 100 = 0 Then
					UpdateProgress(CSng(intPeptideIndex / intFeaturesToIdentifyCount * 100))
					If Me.AbortProcessing Then Exit For
				End If

				If intPeptideIndex Mod 100000 = 0 AndAlso intPeptideIndex > 0 Then
					MyBase.LogMessage("SummarizeResultsByPeptide, intPeptideIndex = " & intPeptideIndex.ToString, eMessageTypeConstants.Normal)
				End If
			Next intPeptideIndex

			With udtPeptideStatsBinned
				For intBinIndex = 0 To .BinCount - 1
					With .Bins(intBinIndex)
						intTotal = .UniqueResultIDCount + .NonUniqueResultIDCount
						If intTotal > 0 Then
							.PercentUnique = CSng(.UniqueResultIDCount / intTotal * 100)
						Else
							.PercentUnique = 0
						End If
					End With
				Next intBinIndex
			End With


			If intPeptideSkipCount > 0 Then
				MyBase.LogMessage("Skipped " & intPeptideSkipCount.ToString & " peptides since their masses were outside the defined bin range", eMessageTypeConstants.Warning)
			End If

			' Write out the peptide results for this threshold level
			If mCreateSeparateOutputFileForEachThreshold Then
				strWorkingFilenameBase = strOutputFilenameBase & "_PeptideStatsBinned" & (intThresholdIndex + 1).ToString & ".txt"
				strOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, strWorkingFilenameBase)

				srPeptideUniquenessOutFile = New System.IO.StreamWriter(strOutputFilePath)

				ExportThresholds(srPeptideUniquenessOutFile, intThresholdIndex, objThresholds)
				srPeptideUniquenessOutFile.WriteLine()

				SummarizeResultsByPeptideWriteHeaders(srPeptideUniquenessOutFile, mCreateSeparateOutputFileForEachThreshold)
			End If

			blnSuccess = ExportPeptideUniquenessResults(intThresholdIndex, udtPeptideStatsBinned, srPeptideUniquenessOutFile)

			If mCreateSeparateOutputFileForEachThreshold Then
				srPeptideUniquenessOutFile.Close()
				srPeptideUniquenessOutFile = Nothing
			End If

			MyBase.LogMessage("SummarizeResultsByPeptide complete", eMessageTypeConstants.Normal)

			UpdateProgress(100)

		Catch ex As Exception
			blnSuccess = False
		End Try

		Return blnSuccess

	End Function

	Private Sub SummarizeResultsByPeptideWriteHeaders(ByRef srOutFile As System.IO.StreamWriter, ByVal blnCreateSeparateOutputFileForEachThreshold As Boolean)

		Dim strLineOut As String
		Dim intIndex As Integer

		' Write the column headers
		If blnCreateSeparateOutputFileForEachThreshold Then
			strLineOut = String.Empty
		Else
			strLineOut = "Threshold_Index" & mOutputFileDelimiter
		End If

		strLineOut &= "Bin_Start_Mass" & _
					mOutputFileDelimiter & "Percent_Unique" & _
					mOutputFileDelimiter & "Peptide_Count_Total"

		For intIndex = 1 To Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave) - 1
			strLineOut &= mOutputFileDelimiter & "MatchCount_" & intIndex.ToString
		Next intIndex

		strLineOut &= mOutputFileDelimiter & "MatchCount_" & Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave).ToString & "_OrMore"

		srOutFile.WriteLine(strLineOut)
		srOutFile.Flush()

	End Sub

	Private Function SummarizeResultsByProtein(ByRef objThresholds As clsPeakMatchingClass.clsSearchThresholds, _
											   ByVal intThresholdIndex As Integer, _
											   ByRef objFeaturesToIdentify As clsPeakMatchingClass.PMFeatureInfoClass, _
											   ByRef objPeptideMatchResults As clsPeakMatchingClass.PMFeatureMatchResultsClass, _
											   ByVal strOutputFolderPath As String, ByVal strOutputFilenameBase As String, _
											   ByRef srProteinStatsOutFile As System.IO.StreamWriter) As Boolean

		Dim strWorkingFilenameBase As String
		Dim strOutputFilePath As String = String.Empty

		Dim intPeptideIndex As Integer
		Dim intFeaturesToIdentifyCount As Integer

		Dim intProteinIndex As Integer
		Dim intMatchCount As Integer

		Dim intProteinIDs() As Integer
		Dim objNewDataRow As System.Data.DataRow

		Dim blnSuccess As Boolean

		Dim udtFeatureInfo As clsPeakMatchingClass.udtFeatureInfoType = New clsPeakMatchingClass.udtFeatureInfoType

		Try

			intFeaturesToIdentifyCount = objFeaturesToIdentify.Count
			ResetProgress("Summarizing results by protein")

			' Compute the stats for objThresholds

			' Compute number of unique peptides seen for each protein and write out
			' This will allow one to plot Unique peptide vs protein mass or total peptide countvs. protein mass

			' Initialize mProteinToIdentifiedPeptideMappingTable
			mProteinToIdentifiedPeptideMappingTable.Clear()

			MyBase.LogMessage("SummarizeResultsByProtein starting, total feature count = " & intFeaturesToIdentifyCount.ToString, eMessageTypeConstants.Normal)

			For intPeptideIndex = 0 To intFeaturesToIdentifyCount - 1
				If objFeaturesToIdentify.GetFeatureInfoByRowIndex(intPeptideIndex, udtFeatureInfo) Then
					If FeatureContainsUniqueMatch(udtFeatureInfo, objPeptideMatchResults, intMatchCount, mUseSLiCScoreForUniqueness, mPeptideUniquenessBinningSettings.MinimumSLiCScore) Then
						intProteinIDs = mProteinInfo.GetProteinIDsMappedToPeptideID(udtFeatureInfo.FeatureID)

						For intProteinIndex = 0 To intProteinIDs.Length - 1
							If Not mProteinToIdentifiedPeptideMappingTable.Rows.Contains(New Object() {intProteinIDs(intProteinIndex), udtFeatureInfo.FeatureID}) Then
								objNewDataRow = mProteinToIdentifiedPeptideMappingTable.NewRow
								objNewDataRow(PROTEIN_ID_COLUMN) = intProteinIDs(intProteinIndex)
								objNewDataRow(PEPTIDE_ID_MATCH_COLUMN) = udtFeatureInfo.FeatureID
								mProteinToIdentifiedPeptideMappingTable.Rows.Add(objNewDataRow)
							End If
						Next intProteinIndex
					End If

				End If

				If intPeptideIndex Mod 100 = 0 Then
					UpdateProgress(CSng(intPeptideIndex / intFeaturesToIdentifyCount * 100))
					If Me.AbortProcessing Then Exit For
				End If

				If intPeptideIndex Mod 100000 = 0 AndAlso intPeptideIndex > 0 Then
					MyBase.LogMessage("SummarizeResultsByProtein, intPeptideIndex = " & intPeptideIndex.ToString, eMessageTypeConstants.Normal)
				End If

			Next intPeptideIndex

			' Write out the protein results for this threshold level
			If mCreateSeparateOutputFileForEachThreshold Then
				strWorkingFilenameBase = strOutputFilenameBase & "_ProteinStatsBinned" & (intThresholdIndex + 1).ToString & ".txt"
				strOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, strWorkingFilenameBase)

				srProteinStatsOutFile = New System.IO.StreamWriter(strOutputFilePath)

				ExportThresholds(srProteinStatsOutFile, intThresholdIndex, objThresholds)
				srProteinStatsOutFile.WriteLine()

				SummarizeResultsByProteinWriteHeaders(srProteinStatsOutFile, mCreateSeparateOutputFileForEachThreshold)
			End If

			blnSuccess = ExportProteinStats(intThresholdIndex, objThresholds, strOutputFilePath, srProteinStatsOutFile)

			If mCreateSeparateOutputFileForEachThreshold Then
				srProteinStatsOutFile.Close()
				srProteinStatsOutFile = Nothing
			End If

			UpdateProgress(100)

			MyBase.LogMessage("SummarizeResultsByProtein complete", eMessageTypeConstants.Normal)

		Catch ex As Exception
			blnSuccess = False
		End Try

		Return blnSuccess

	End Function

	Private Sub SummarizeResultsByProteinWriteHeaders(ByRef srOutFile As System.IO.StreamWriter, ByVal blnCreateSeparateOutputFileForEachThreshold As Boolean)

		Dim strLineOut As String

		' Write the column headers
		If blnCreateSeparateOutputFileForEachThreshold Then
			strLineOut = String.Empty
		Else
			strLineOut = "Threshold_Index" & mOutputFileDelimiter
		End If

		strLineOut &= "Protein_Name" & _
					mOutputFileDelimiter & "Protein_ID" & _
					mOutputFileDelimiter & "Peptide_Count_Total" & _
					mOutputFileDelimiter & "Peptide_Count_Uniquely_Identifiable"
		srOutFile.WriteLine(strLineOut)
		srOutFile.Flush()

	End Sub

	Private Sub SetLocalErrorCode(ByVal eNewErrorCode As eProteinDigestionSimulatorErrorCodes)
		SetLocalErrorCode(eNewErrorCode, False)
	End Sub

	Private Sub SetLocalErrorCode(ByVal eNewErrorCode As eProteinDigestionSimulatorErrorCodes, ByVal ex As Exception)
		SetLocalErrorCode(eNewErrorCode, False)

		MyBase.LogMessage(eNewErrorCode.ToString & ": " & ex.Message, eMessageTypeConstants.ErrorMsg)
		mLastErrorMessage = ex.Message
	End Sub

	Private Sub SetLocalErrorCode(ByVal eNewErrorCode As eProteinDigestionSimulatorErrorCodes, ByVal blnLeaveExistingErrorCodeUnchanged As Boolean)

		If blnLeaveExistingErrorCodeUnchanged AndAlso mLocalErrorCode <> eProteinDigestionSimulatorErrorCodes.NoError Then
			' An error code is already defined; do not change it
		Else
			mLocalErrorCode = eNewErrorCode
			mLastErrorMessage = String.Empty
		End If

	End Sub

	Protected Sub UpdateSubtaskProgress(ByVal strProgressStepDescription As String)
		UpdateProgress(strProgressStepDescription, mProgressPercentComplete)
	End Sub

	Protected Sub UpdateSubtaskProgress(ByVal sngPercentComplete As Single)
		UpdateProgress(Me.ProgressStepDescription, sngPercentComplete)
	End Sub

	Protected Sub UpdateSubtaskProgress(ByVal strProgressStepDescription As String, ByVal sngPercentComplete As Single)
		Dim blnDescriptionChanged As Boolean = False

		If strProgressStepDescription <> mSubtaskProgressStepDescription Then
			blnDescriptionChanged = True
		End If

		mSubtaskProgressStepDescription = String.Copy(strProgressStepDescription)
		If sngPercentComplete < 0 Then
			sngPercentComplete = 0
		ElseIf sngPercentComplete > 100 Then
			sngPercentComplete = 100
		End If
		mSubtaskProgressPercentComplete = sngPercentComplete

		If blnDescriptionChanged Then
			If mSubtaskProgressPercentComplete = 0 Then
				LogMessage(mSubtaskProgressStepDescription.Replace(ControlChars.NewLine, "; "))
			Else
				LogMessage(mSubtaskProgressStepDescription & " (" & mSubtaskProgressPercentComplete.ToString("0.0") & "% complete)".Replace(ControlChars.NewLine, "; "))
			End If
		End If

		RaiseEvent SubtaskProgressChanged(Me.ProgressStepDescription, Me.ProgressPercentComplete)

	End Sub

#Region "Event Handlers"

	Private Sub mProteinInfo_SortingList() Handles mProteinInfo.SortingList
		Static intSortCount As Integer = 0
		Static dtLastPostTime As DateTime = System.DateTime.UtcNow

		intSortCount += 1
		If System.DateTime.UtcNow.Subtract(dtLastPostTime).TotalSeconds >= 10 Then
			MyBase.LogMessage("Sorting protein list (SortCount = " & intSortCount & ")", eMessageTypeConstants.Normal)
			dtLastPostTime = System.DateTime.UtcNow
		End If
	End Sub

	Private Sub mProteinInfo_SortingMappings() Handles mProteinInfo.SortingMappings
		Static intSortCount As Integer = 0
		Static dtLastPostTime As DateTime = System.DateTime.UtcNow

		intSortCount += 1
		If System.DateTime.UtcNow.Subtract(dtLastPostTime).TotalSeconds >= 10 Then
			MyBase.LogMessage("Sorting protein to peptide mapping info (SortCount = " & intSortCount & ")", eMessageTypeConstants.Normal)
			dtLastPostTime = System.DateTime.UtcNow
		End If
	End Sub

	Private Sub mPeptideMatchResults_SortingList() Handles mPeptideMatchResults.SortingList
		Static intSortCount As Integer = 0
		Static dtLastPostTime As DateTime = System.DateTime.UtcNow

		intSortCount += 1
		If System.DateTime.UtcNow.Subtract(dtLastPostTime).TotalSeconds >= 10 Then
			MyBase.LogMessage("Sorting peptide match results list (SortCount = " & intSortCount & ")", eMessageTypeConstants.Normal)
			dtLastPostTime = System.DateTime.UtcNow
		End If
	End Sub

	Private Sub mPeakMatchingClass_LogEvent(ByVal Message As String, ByVal EventType As clsPeakMatchingClass.eMessageTypeConstants) Handles mPeakMatchingClass.LogEvent
		Select Case EventType
			Case clsPeakMatchingClass.eMessageTypeConstants.Normal
				MyBase.LogMessage(Message, eMessageTypeConstants.Normal)

			Case clsPeakMatchingClass.eMessageTypeConstants.Warning
				MyBase.LogMessage(Message, eMessageTypeConstants.Warning)

			Case clsPeakMatchingClass.eMessageTypeConstants.ErrorMsg
				MyBase.LogMessage(Message, eMessageTypeConstants.ErrorMsg)

			Case clsPeakMatchingClass.eMessageTypeConstants.Health
				' Don't log this type of message
		End Select
	End Sub

	Private Sub mPeakMatchingClass_ProgressContinues() Handles mPeakMatchingClass.ProgressContinues
		UpdateSubtaskProgress(mPeakMatchingClass.ProgessPct)
	End Sub

	Private Sub mProteinFileParser_ErrorEvent(ByVal strMessage As String) Handles mProteinFileParser.ErrorEvent
		ShowErrorMessage("Error in mProteinFileParser: " & strMessage)
	End Sub

#End Region

End Class

