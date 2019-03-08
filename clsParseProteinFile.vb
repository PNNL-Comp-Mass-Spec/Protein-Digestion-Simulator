Option Strict On

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
Imports System.Text
Imports System.Threading
Imports NETPrediction
Imports PRISM
Imports ProteinFileReader
Imports ValidateFastaFile

''' <summary>
''' This class will read a protein fasta file or delimited protein info file and parse it
''' to create a delimited protein list output file, and optionally an in-silico digested output file
'''
''' It can also create a fasta file containing reversed or scrambled sequences, and these can
''' be based on all of the proteins in the input file, or a sampling of the proteins in the input file
''' </summary>
Public Class clsParseProteinFile
    Inherits FileProcessor.ProcessFilesBase

    Public Sub New()
        MyBase.mFileDate = "March 2, 2018"
        InitializeLocalVariables()
    End Sub

#Region "Constants and Enums"

    Public Const XML_SECTION_OPTIONS As String = "ProteinDigestionSimulatorOptions"
    Public Const XML_SECTION_FASTA_OPTIONS As String = "FastaInputOptions"
    Public Const XML_SECTION_PROCESSING_OPTIONS As String = "ProcessingOptions"
    Public Const XML_SECTION_DIGESTION_OPTIONS As String = "DigestionOptions"
    Public Const XML_SECTION_UNIQUENESS_STATS_OPTIONS As String = "UniquenessStatsOptions"

    Private Const PROTEIN_CACHE_MEMORY_RESERVE_COUNT As Integer = 500

    Private Const SCRAMBLING_CACHE_LENGTH As Integer = 4000
    Private Const PROTEIN_PREFIX_SCRAMBLED As String = "Random_"
    Private Const PROTEIN_PREFIX_REVERSED As String = "XXX."

    Private Const MAXIMUM_PROTEIN_NAME_LENGTH As Integer = 34

    Private Const MAX_ABBREVIATED_FILENAME_LENGTH As Integer = 15

    ' The value of 7995 is chosen because the maximum varchar() value in Sql Server is varchar(8000)
    ' and we want to prevent truncation errors when importing protein names and descriptions into Sql Server
    Private Const MAX_PROTEIN_DESCRIPTION_LENGTH As Integer = clsValidateFastaFile.MAX_PROTEIN_DESCRIPTION_LENGTH

    ' Error codes specialized for this class
    Public Enum eParseProteinFileErrorCodes
        NoError = 0
        ProteinFileParsingOptionsSectionNotFound = 1
        ErrorReadingInputFile = 2
        ErrorCreatingProteinOutputFile = 4
        ErrorCreatingDigestedProteinOutputFile = 8
        ErrorCreatingScrambledProteinOutputFile = 16
        ErrorWritingOutputFile = 32
        ErrorInitializingObjectVariables = 64
        DigestProteinSequenceError = 128
        UnspecifiedError = -1
    End Enum

    Public Enum ProteinScramblingModeConstants
        None = 0
        Reversed = 1
        Randomized = 2
    End Enum

    Public Enum DelimiterCharConstants
        Space = 0
        Tab = 1
        Comma = 2
        Other = 3
    End Enum

#End Region

#Region "Structures"
    Public Structure udtAddnlRefType
        Public RefName As String                'e.g. in gi:12334  the RefName is "gi" and the RefAccession is "1234"
        Public RefAccession As String
    End Structure

    Public Structure udtProteinInfoType
        Public Name As String
        Public AlternateNameCount As Integer
        Public AlternateNames() As udtAddnlRefType
        Public Description As String
        Public Sequence As String
        Public SequenceHash As String                   ' Only populated if ComputeSequenceHashValues=true
        Public Mass As Double
        Public pI As Single
        Public Hydrophobicity As Single
        Public ProteinNET As Single
        Public ProteinSCXNET As Single
    End Structure

    Private Structure udtFilePathInfoType
        Public ProteinInputFilePath As String
        Public OutputFileNameBaseOverride As String
        Public OutputFolderPath As String
        Public ProteinOutputFilePath As String
        Public DigestedProteinOutputFilePath As String
    End Structure

    Private Structure udtScramblingResidueCacheType
        Public Cache As String                          ' Cache of residues parsed; when this reaches 4000 characters, then a portion of this text is appended to ResiduesToWrite
        Public CacheLength As Integer
        Public SamplingPercentage As Integer
        Public OutputCount As Integer
        Public ResiduesToWrite As String
    End Structure
#End Region

#Region "Classwide Variables"
    Private mInputFileDelimiter As Char                              ' Only used for delimited protein input files, not for fasta files
    Private mDelimitedInputFileFormatCode As DelimitedFileReader.eDelimitedFileFormatCode

    Private mInputFileProteinsProcessed As Integer
    Private mInputFileLinesRead As Integer
    Private mInputFileLineSkipCount As Integer

    Private mOutputFileDelimiter As Char

    Private mProteinScramblingMode As ProteinScramblingModeConstants
    Private mProteinScramblingSamplingPercentage As Integer
    Private mProteinScramblingLoopCount As Integer


    ' pI options
    Private mHydrophobicityType As clspICalculation.eHydrophobicityTypeConstants

    Private mSequenceWidthToExamineForMaximumpI As Integer

    Public DigestionOptions As clsInSilicoDigest.DigestionOptionsClass

    Public FastaFileOptions As FastaFileOptionsClass
    Private mObjectVariablesLoaded As Boolean
    Private WithEvents mInSilicoDigest As clsInSilicoDigest
    Private mpICalculator As clspICalculation

    Private WithEvents mNETCalculator As ElutionTimePredictionKangas
    Private WithEvents mSCXNETCalculator As SCXElutionTimePredictionKangas

    Private mProteinCount As Integer
    Private mProteins() As udtProteinInfoType
    Private mParsedFileIsFastaFile As Boolean

    Private mFileNameAbbreviated As String

    Private mMasterSequencesHashTable As Hashtable
    Private mNextUniqueIDForMasterSeqs As Integer

    Private mLocalErrorCode As eParseProteinFileErrorCodes

    Private mSubtaskProgressStepDescription As String = String.Empty
    Private mSubtaskProgressPercentComplete As Single = 0

    ' PercentComplete ranges from 0 to 100, but can contain decimal percentage values
    Public Event SubtaskProgressChanged(taskDescription As String, percentComplete As Single)

#End Region

#Region "Auto-properties"
    Public Property AssumeDelimitedFile As Boolean

    Public Property AssumeFastaFile As Boolean

    Public Property ComputeNET As Boolean

    Public Property ComputepI As Boolean

    Public Property ComputeProteinMass As Boolean

    Public Property ComputeSequenceHashValues As Boolean

    Public Property ComputeSequenceHashIgnoreILDiff As Boolean

    Public Property ComputeSCXNET As Boolean

    ''' <summary>
    ''' True to create a fasta output file; false for a tab-delimited text file
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Only valid if mCreateDigestedProteinOutputFile is False</remarks>
    Public Property CreateFastaOutputFile As Boolean

    ''' <summary>
    ''' When True, then writes the proteins to a file; When false, then caches the results in memory
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Use DigestProteinSequence to obtained digested peptides instead of proteins</remarks>
    Public Property CreateProteinOutputFile As Boolean

    ''' <summary>
    ''' True to in-silico digest the proteins
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Only valid if CreateProteinOutputFile is True</remarks>
    Public Property CreateDigestedProteinOutputFile As Boolean

    ''' <summary>
    ''' When true, do not include protein description in the output file
    ''' </summary>
    ''' <returns></returns>
    Public Property ExcludeProteinDescription As Boolean

    ''' <summary>
    ''' When true, do not include protein sequence in the output file
    ''' </summary>
    ''' <returns></returns>
    Public Property ExcludeProteinSequence As Boolean

    ''' <summary>
    ''' When true, assign UniqueID values to the digested peptides (requires more memory)
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks>Only valid if CreateDigestedProteinOutputFile is True</remarks>
    Public Property GenerateUniqueIDValuesForPeptides As Boolean

    ''' <summary>
    ''' When true, include X residues when computing protein mass (using the mass of Ile/Leu)
    ''' </summary>
    ''' <returns></returns>
    Public Property IncludeXResiduesInMass As Boolean

    ''' <summary>
    ''' Summary of the result of processing
    ''' </summary>
    ''' <returns></returns>
    Public Property ProcessingSummary As String

    ''' <summary>
    ''' When true, report the maximum pI
    ''' </summary>
    ''' <returns></returns>
    Public Property ReportMaximumpI As Boolean

    Public Property ShowDebugPrompts As Boolean

    Public Property TruncateProteinDescription As Boolean

#End Region

#Region "Processing Options Interface Functions"

    Public Property DelimitedFileFormatCode As DelimitedFileReader.eDelimitedFileFormatCode
        Get
            Return mDelimitedInputFileFormatCode
        End Get
        Set
            mDelimitedInputFileFormatCode = Value
        End Set
    End Property

    Public Property ElementMassMode As PeptideSequenceClass.ElementModeConstants
        Get
            If mInSilicoDigest Is Nothing Then
                Return PeptideSequenceClass.ElementModeConstants.IsotopicMass
            Else
                Return mInSilicoDigest.ElementMassMode
            End If
        End Get
        Set
            If mInSilicoDigest Is Nothing Then
                InitializeObjectVariables()
            End If
            mInSilicoDigest.ElementMassMode = Value
        End Set
    End Property

    Public Property HydrophobicityType As clspICalculation.eHydrophobicityTypeConstants
        Get
            Return mHydrophobicityType
        End Get
        Set
            mHydrophobicityType = Value
        End Set
    End Property

    Public Property InputFileDelimiter As Char
        Get
            Return mInputFileDelimiter
        End Get
        Set
            If Not Value = Nothing Then
                mInputFileDelimiter = Value
            End If
        End Set
    End Property

    Public ReadOnly Property InputFileProteinsProcessed As Integer
        Get
            Return mInputFileProteinsProcessed
        End Get
    End Property

    Public ReadOnly Property InputFileLinesRead As Integer
        Get
            Return mInputFileLinesRead
        End Get
    End Property

    Public ReadOnly Property InputFileLineSkipCount As Integer
        Get
            Return mInputFileLineSkipCount
        End Get
    End Property

    Public ReadOnly Property LocalErrorCode As eParseProteinFileErrorCodes
        Get
            Return mLocalErrorCode
        End Get
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

    Public ReadOnly Property ParsedFileIsFastaFile As Boolean
        Get
            Return mParsedFileIsFastaFile
        End Get
    End Property

    Public Property ProteinScramblingLoopCount As Integer
        Get
            Return mProteinScramblingLoopCount
        End Get
        Set
            mProteinScramblingLoopCount = Value
        End Set
    End Property

    Public Property ProteinScramblingMode As ProteinScramblingModeConstants
        Get
            Return mProteinScramblingMode
        End Get
        Set
            mProteinScramblingMode = Value
        End Set
    End Property

    Public Property ProteinScramblingSamplingPercentage As Integer
        Get
            Return mProteinScramblingSamplingPercentage
        End Get
        Set
            mProteinScramblingSamplingPercentage = Value
        End Set
    End Property

    Public Property SequenceWidthToExamineForMaximumpI As Integer
        Get
            Return mSequenceWidthToExamineForMaximumpI
        End Get
        Set
            If Value < 1 Then mSequenceWidthToExamineForMaximumpI = 1
            mSequenceWidthToExamineForMaximumpI = Value
        End Set
    End Property

#End Region

    Private Function ComputeSequenceHydrophobicity(strSequence As String) As Single

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mpICalculator will be nothing
        If mpICalculator Is Nothing Then
            Return 0
        Else
            Return mpICalculator.CalculateSequenceHydrophobicity(strSequence)
        End If

    End Function

    Private Function ComputeSequencepI(strSequence As String) As Single

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mpICalculator will be nothing
        If mpICalculator Is Nothing Then
            Return 0
        Else
            Return mpICalculator.CalculateSequencepI(strSequence)
        End If

    End Function

    Private Function ComputeSequenceMass(strSequence As String) As Double

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, objInSilicoDigest will be nothing
        If mInSilicoDigest Is Nothing Then
            Return 0
        Else
            Return mInSilicoDigest.ComputeSequenceMass(strSequence, IncludeXResiduesInMass)
        End If

    End Function

    Private Function ComputeSequenceNET(strSequence As String) As Single

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mNETCalculator will be nothing
        If mNETCalculator Is Nothing Then
            Return 0
        Else
            Return mNETCalculator.GetElutionTime(strSequence)
        End If

    End Function

    Private Function ComputeSequenceSCXNET(strSequence As String) As Single

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mSCXNETCalculator will be nothing
        If mSCXNETCalculator Is Nothing Then
            Return 0
        Else
            Return mSCXNETCalculator.GetElutionTime(strSequence)
        End If

    End Function

    Public Function DigestProteinSequence(strSequence As String, ByRef udtPeptides() As clsInSilicoDigest.PeptideInfoClass, objDigestionOptions As clsInSilicoDigest.DigestionOptionsClass, Optional strProteinName As String = "") As Integer
        ' Returns the number of digested peptides in udtPeptides

        Dim intPeptideCount As Integer

        ' Make sure the object variables are initialized
        If Not InitializeObjectVariables() Then Return 0

        Try
            intPeptideCount = mInSilicoDigest.DigestSequence(strSequence, udtPeptides, objDigestionOptions, ComputepI, strProteinName)
        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.DigestProteinSequenceError)
            intPeptideCount = 0
        End Try
        Return intPeptideCount

    End Function

    Private Function ExtractAlternateProteinNamesFromDescription(ByRef strDescription As String, ByRef udtAlternateNames() As udtAddnlRefType) As Integer
        ' Searches in strDescription for additional protein Ref names
        ' strDescription is passed ByRef since the additional protein references will be removed from it

        Dim intIndex As Integer
        Dim intCharIndex, intSpaceIndex As Integer

        Dim intAlternateNameCount As Integer
        Dim strRefs() As String

        intAlternateNameCount = 0
        ReDim udtAlternateNames(0)

        Try
            strRefs = strDescription.Split(FastaFileOptions.AddnlRefSepChar)

            If strRefs.Length > 0 Then
                ReDim udtAlternateNames(strRefs.Length - 1)

                For intIndex = 0 To strRefs.Length - 1
                    intCharIndex = strRefs(intIndex).IndexOf(FastaFileOptions.AddnlRefAccessionSepChar)

                    If intCharIndex > 0 Then
                        If intIndex = strRefs.Length - 1 Then
                            ' Need to find the next space after intCharIndex and truncate strRefs() at that location
                            intSpaceIndex = strRefs(intIndex).IndexOf(" "c, intCharIndex)
                            If intSpaceIndex >= 0 Then
                                strRefs(intIndex) = strRefs(intIndex).Substring(0, intSpaceIndex)
                            End If
                        End If

                        With udtAlternateNames(intIndex)
                            If intCharIndex >= strRefs(intIndex).Length - 1 Then
                                ' No accession after the colon; invalid entry so discard this entry and stop parsing
                                ReDim Preserve udtAlternateNames(intIndex - 1)
                                Exit For
                            End If

                            .RefName = strRefs(intIndex).Substring(0, intCharIndex)
                            .RefAccession = strRefs(intIndex).Substring(intCharIndex + 1)
                            intAlternateNameCount += 1
                        End With

                        intCharIndex = strDescription.IndexOf(strRefs(intIndex), StringComparison.Ordinal)
                        If intCharIndex >= 0 Then
                            If intCharIndex + strRefs(intIndex).Length + 1 < strDescription.Length Then
                                strDescription = strDescription.Substring(intCharIndex + strRefs(intIndex).Length + 1)
                            Else
                                strDescription = String.Empty
                            End If
                        Else
                            ShowErrorMessage("This code in ExtractAlternateProteinNamesFromDescription should never be reached")
                        End If
                    Else
                        Exit For
                    End If
                Next intIndex

            End If

        Catch ex As Exception
            ShowErrorMessage("Error parsing out additional Ref Names from the Protein Description: " & ex.Message)
        End Try

        Return intAlternateNameCount

    End Function

    Private Function FractionLowercase(proteinSequence As String) As Double
        Dim lowerCount = 0
        Dim upperCount = 0

        For index = 0 To proteinSequence.Length - 1
            If Char.IsLower(proteinSequence.Chars(index)) Then
                lowerCount += 1
            ElseIf Char.IsUpper(proteinSequence.Chars(index)) Then
                upperCount += 1
            End If
        Next

        If lowerCount + upperCount = 0 Then
            Return 0
        End If

        Return lowerCount / (lowerCount + upperCount)

    End Function

    Public Overrides Function GetDefaultExtensionsToParse() As IList(Of String)
        Dim strExtensionsToParse = New List(Of String) From {
                ".fasta",
                ".txt"
                }

        Return strExtensionsToParse

    End Function

    Public Overrides Function GetErrorMessage() As String
        ' Returns "" if no error

        Dim strErrorMessage As String

        If MyBase.ErrorCode = ProcessFilesErrorCodes.LocalizedError Or
           MyBase.ErrorCode = ProcessFilesErrorCodes.NoError Then
            Select Case mLocalErrorCode
                Case eParseProteinFileErrorCodes.NoError
                    strErrorMessage = ""
                Case eParseProteinFileErrorCodes.ProteinFileParsingOptionsSectionNotFound
                    strErrorMessage = "The section " & XML_SECTION_OPTIONS & " was not found in the parameter file"

                Case eParseProteinFileErrorCodes.ErrorReadingInputFile
                    strErrorMessage = "Error reading input file"
                Case eParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile
                    strErrorMessage = "Error creating parsed proteins output file"
                Case eParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile
                    strErrorMessage = "Error creating digested proteins output file"
                Case eParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile
                    strErrorMessage = "Error creating scrambled proteins output file"

                Case eParseProteinFileErrorCodes.ErrorWritingOutputFile
                    strErrorMessage = "Error writing to one of the output files"

                Case eParseProteinFileErrorCodes.ErrorInitializingObjectVariables
                    strErrorMessage = "Error initializing In Silico Digester class"

                Case eParseProteinFileErrorCodes.DigestProteinSequenceError
                    strErrorMessage = "Error in DigestProteinSequence function"
                Case eParseProteinFileErrorCodes.UnspecifiedError
                    strErrorMessage = "Unspecified localized error"
                Case Else
                    ' This shouldn't happen
                    strErrorMessage = "Unknown error state"
            End Select
        Else
            strErrorMessage = MyBase.GetBaseClassErrorMessage()
        End If

        Return strErrorMessage

    End Function

    Public Function GetProteinCountCached() As Integer
        Return mProteinCount
    End Function

    Public Function GetCachedProtein(intIndex As Integer) As udtProteinInfoType
        If intIndex < mProteinCount Then
            Return mProteins(intIndex)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetDigestedPeptidesForCachedProtein(intIndex As Integer, ByRef udtPeptides() As clsInSilicoDigest.PeptideInfoClass, objDigestionOptions As clsInSilicoDigest.DigestionOptionsClass) As Integer
        ' Returns the number of entries in udtPeptides()

        If intIndex < mProteinCount Then
            Return DigestProteinSequence(mProteins(intIndex).Sequence, udtPeptides, objDigestionOptions, mProteins(intIndex).Name)
        Else
            Return 0
        End If

    End Function

    Private Sub InitializeLocalVariables()
        mLocalErrorCode = eParseProteinFileErrorCodes.NoError
        ShowDebugPrompts = False

        AssumeDelimitedFile = False
        AssumeFastaFile = False
        mInputFileDelimiter = ControlChars.Tab
        mDelimitedInputFileFormatCode = DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence

        mInputFileProteinsProcessed = 0
        mInputFileLinesRead = 0
        mInputFileLineSkipCount = 0

        mOutputFileDelimiter = ControlChars.Tab
        ExcludeProteinSequence = False
        ComputeProteinMass = True
        ComputepI = True
        ComputeNET = True
        ComputeSCXNET = True

        ComputeSequenceHashValues = False
        ComputeSequenceHashIgnoreILDiff = True
        TruncateProteinDescription = True

        IncludeXResiduesInMass = False
        mProteinScramblingMode = ProteinScramblingModeConstants.None
        mProteinScramblingSamplingPercentage = 100
        mProteinScramblingLoopCount = 1

        CreateFastaOutputFile = False
        CreateProteinOutputFile = False
        CreateDigestedProteinOutputFile = False
        GenerateUniqueIDValuesForPeptides = True

        DigestionOptions = New clsInSilicoDigest.DigestionOptionsClass
        FastaFileOptions = New FastaFileOptionsClass

        ProcessingSummary = String.Empty

        mProteinCount = 0
        ReDim mProteins(0)

        mFileNameAbbreviated = String.Empty

        mHydrophobicityType = clspICalculation.eHydrophobicityTypeConstants.HW
        ReportMaximumpI = False
        mSequenceWidthToExamineForMaximumpI = 10

    End Sub

    Public Shared Function IsFastaFile(strFilePath As String) As Boolean
        ' Examines the file's extension and true if it ends in .fasta

        If Path.GetFileName(strFilePath).ToLower.EndsWith(".fasta") Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Function InitializeObjectVariables() As Boolean

        Dim strErrorMessage As String = String.Empty

        If Not mObjectVariablesLoaded Then
            ' Need to initialize the object variables

            Try
                mInSilicoDigest = New clsInSilicoDigest
            Catch ex As Exception
                strErrorMessage = "Error initializing InSilicoDigest class"
                Me.SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
            End Try

            Try
                mpICalculator = New clspICalculation
            Catch ex As Exception
                strErrorMessage = "Error initializing pI Calculation class"
                ShowErrorMessage(strErrorMessage)
                Me.SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
            End Try

            Try
                mNETCalculator = New ElutionTimePredictionKangas()
            Catch ex As Exception
                strErrorMessage = "Error initializing LC NET Calculation class"
                ShowErrorMessage(strErrorMessage)
                Me.SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
            End Try

            Try
                mSCXNETCalculator = New SCXElutionTimePredictionKangas()
            Catch ex As Exception
                strErrorMessage = "Error initializing SCX NET Calculation class"
                ShowErrorMessage(strErrorMessage)
                Me.SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
            End Try

            If strErrorMessage.Length > 0 Then
                ShowErrorMessage(strErrorMessage)
                mObjectVariablesLoaded = False
            Else
                mObjectVariablesLoaded = True
            End If
        End If

        If Not mInSilicoDigest Is Nothing Then
            If Not mpICalculator Is Nothing Then
                mInSilicoDigest.InitializepICalculator(mpICalculator)
            End If
        End If

        Return mObjectVariablesLoaded

    End Function

    Private Function InitializeScrambledOutput(
      pathInfo As udtFilePathInfoType,
      udtResidueCache As udtScramblingResidueCacheType,
      eScramblingMode As ProteinScramblingModeConstants,
      <Out> ByRef srScrambledOutStream As StreamWriter,
      <Out> ByRef objRandomNumberGenerator As Random) As Boolean


        Dim blnSuccess As Boolean
        Dim intRandomNumberSeed As Integer
        Dim strSuffix As String
        Dim strScrambledFastaOutputFilePath As String


        ' Wait to allow the timer to advance.
        Thread.Sleep(1)
        intRandomNumberSeed = Environment.TickCount

        objRandomNumberGenerator = New Random(intRandomNumberSeed)

        If eScramblingMode = ProteinScramblingModeConstants.Reversed Then
            ' Reversed fasta file
            strSuffix = "_reversed"
            If udtResidueCache.SamplingPercentage < 100 Then
                strSuffix &= "_" & udtResidueCache.SamplingPercentage.ToString & "pct"
            End If
        Else
            ' Scrambled fasta file
            strSuffix = "_scrambled_seed" & intRandomNumberSeed.ToString
            If udtResidueCache.SamplingPercentage < 100 Then
                strSuffix &= "_" & udtResidueCache.SamplingPercentage.ToString & "pct"
            End If
        End If
        strScrambledFastaOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) & strSuffix & ".fasta")

        ' Define the abbreviated name of the input file; used in the protein names
        mFileNameAbbreviated = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath)
        If mFileNameAbbreviated.Length > MAX_ABBREVIATED_FILENAME_LENGTH Then
            mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, MAX_ABBREVIATED_FILENAME_LENGTH)

            If mFileNameAbbreviated.Substring(mFileNameAbbreviated.Length - 1, 1) = "_" Then
                mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.Length - 1)
            Else
                If mFileNameAbbreviated.LastIndexOf("-"c) > MAX_ABBREVIATED_FILENAME_LENGTH / 3 Then
                    mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.LastIndexOf("-"c))
                ElseIf mFileNameAbbreviated.LastIndexOf("_"c) > MAX_ABBREVIATED_FILENAME_LENGTH / 3 Then
                    mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.LastIndexOf("_"c))
                End If
            End If
        End If

        ' Make sure there aren't any spaces in the abbreviated filename
        mFileNameAbbreviated = mFileNameAbbreviated.Replace(" ", "_")

        Try
            ' Open the scrambled protein output fasta file (if required)
            srScrambledOutStream = New StreamWriter(strScrambledFastaOutputFilePath)
            blnSuccess = True
        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile)
            blnSuccess = False
            srScrambledOutStream = Nothing
        End Try

        Return blnSuccess

    End Function

    Public Function LoadParameterFileSettings(strParameterFilePath As String) As Boolean

        Dim objSettingsFile As New XmlSettingsFileAccessor()

        Dim strCustomDelimiter As String
        Dim delimiterIndex As Integer

        Dim blnCysPeptidesOnly As Boolean

        Try

            If strParameterFilePath Is Nothing OrElse strParameterFilePath.Length = 0 Then
                ' No parameter file specified; nothing to load
                Return True
            End If

            If Not File.Exists(strParameterFilePath) Then
                ' See if strParameterFilePath points to a file in the same directory as the application
                strParameterFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(strParameterFilePath))
                If Not File.Exists(strParameterFilePath) Then
                    MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.ParameterFileNotFound)
                    Return False
                End If
            End If

            ' Pass False to .LoadSettings() here to turn off case sensitive matching
            If objSettingsFile.LoadSettings(strParameterFilePath, False) Then
                If Not objSettingsFile.SectionPresent(XML_SECTION_OPTIONS) Then
                    ShowErrorMessage("The node '<section name=""" & XML_SECTION_OPTIONS & """> was not found in the parameter file: " & strParameterFilePath)
                    SetLocalErrorCode(eParseProteinFileErrorCodes.ProteinFileParsingOptionsSectionNotFound)
                    Return False
                Else
                    ' ComparisonFastaFile = settingsFile.GetParam(XML_SECTION_OPTIONS, "ComparisonFastaFile", ComparisonFastaFile)
                    delimiterIndex = DelimiterCharConstants.Tab
                    customDelimiter = ControlChars.Tab

                    delimiterIndex = settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnDelimiterIndex", delimiterIndex)
                    customDelimiter = settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnDelimiter", customDelimiter)

                    InputFileDelimiter = LookupColumnDelimiterChar(delimiterIndex, customDelimiter, InputFileDelimiter)

                    DelimitedFileFormatCode = CType(settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnOrdering", DelimitedFileFormatCode), DelimitedFileReader.eDelimitedFileFormatCode)

                    delimiterIndex = DelimiterCharConstants.Tab
                    customDelimiter = ControlChars.Tab
                    delimiterIndex = settingsFile.GetParam(XML_SECTION_OPTIONS, "OutputFileFieldDelimiterIndex", delimiterIndex)
                    customDelimiter = settingsFile.GetParam(XML_SECTION_OPTIONS, "OutputFileFieldDelimiter", customDelimiter)

                    OutputFileDelimiter = LookupColumnDelimiterChar(delimiterIndex, customDelimiter, OutputFileDelimiter)

                    With Me.DigestionOptions
                        .IncludePrefixAndSuffixResidues = objSettingsFile.GetParam(XML_SECTION_OPTIONS, "IncludePrefixAndSuffixResidues", .IncludePrefixAndSuffixResidues)
                    End With

                    With Me.FastaFileOptions
                        .ProteinLineStartChar = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "RefStartChar", .ProteinLineStartChar.ToString).Chars(0)

                        delimiterIndex = DelimiterCharConstants.Space
                        customDelimiter = " "c

                        delimiterIndex = settingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "RefEndCharIndex", delimiterIndex)

                        .ProteinLineAccessionEndChar = LookupColumnDelimiterChar(delimiterIndex, customDelimiter, .ProteinLineAccessionEndChar)

                        .LookForAddnlRefInDescription = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "LookForAddnlRefInDescription", .LookForAddnlRefInDescription)

                        .AddnlRefSepChar = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "AddnlRefSepChar", .AddnlRefSepChar.ToString).Chars(0)
                        .AddnlRefAccessionSepChar = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "AddnlRefAccessionSepChar", .AddnlRefAccessionSepChar.ToString).Chars(0)
                    End With

                    Me.ExcludeProteinSequence = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ExcludeProteinSequence", Me.ExcludeProteinSequence)
                    Me.ComputeProteinMass = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeProteinMass", Me.ComputeProteinMass)
                    Me.IncludeXResiduesInMass = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "IncludeXResidues", Me.IncludeXResiduesInMass)
                    Me.ElementMassMode = CType(objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ElementMassMode", CInt(Me.ElementMassMode)), PeptideSequenceClass.ElementModeConstants)

                    Me.ComputepI = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputepI", Me.ComputepI)
                    Me.ComputeNET = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeNET", Me.ComputeNET)
                    Me.ComputeSCXNET = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeSCX", Me.ComputeSCXNET)

                    Me.CreateDigestedProteinOutputFile = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "DigestProteins", Me.CreateDigestedProteinOutputFile)
                    Me.ProteinScramblingMode = CType(objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ProteinReversalIndex", CInt(Me.ProteinScramblingMode)), ProteinScramblingModeConstants)
                    Me.ProteinScramblingLoopCount = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ProteinScramblingLoopCount", Me.ProteinScramblingLoopCount)

                    With Me.DigestionOptions
                        .CleavageRuleID = CType(objSettingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "CleavageRuleTypeIndex", CInt(.CleavageRuleID)), clsInSilicoDigest.CleavageRuleConstants)
                        .RemoveDuplicateSequences = Not objSettingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "IncludeDuplicateSequences", Not .RemoveDuplicateSequences)

                        blnCysPeptidesOnly = objSettingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "CysPeptidesOnly", False)
                        If blnCysPeptidesOnly Then
                            .AminoAcidResidueFilterChars = New Char() {"C"c}
                        Else
                            .AminoAcidResidueFilterChars = New Char() {}
                        End If

                        .MinFragmentMass = objSettingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMinimumMass", .MinFragmentMass)
                        .MaxFragmentMass = objSettingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMaximumMass", .MaxFragmentMass)
                        .MinFragmentResidueCount = objSettingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMinimumResidueCount", .MinFragmentResidueCount)
                        .MaxMissedCleavages = objSettingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMaximumMissedCleavages", .MaxMissedCleavages)
                    End With

                End If
            End If

        Catch ex As Exception
            ShowErrorMessage("Error in LoadParameterFileSettings: " & ex.Message)
            Return False
        End Try

        Return True

    End Function

    Public Shared Function LookupColumnDelimiterChar(intDelimiterIndex As Integer, strCustomDelimiter As String, strDefaultDelimiter As Char) As Char

        Dim strDelimiter As String

        Select Case intDelimiterIndex
            Case DelimiterCharConstants.Space
                strDelimiter = " "
            Case DelimiterCharConstants.Tab
                strDelimiter = ControlChars.Tab
            Case DelimiterCharConstants.Comma
                strDelimiter = ","
            Case Else
                ' Includes DelimiterCharConstants.Other
                strDelimiter = String.Copy(strCustomDelimiter)
        End Select

        If strDelimiter Is Nothing OrElse strDelimiter.Length = 0 Then
            strDelimiter = String.Copy(strDefaultDelimiter)
        End If

        Try
            Return strDelimiter.Chars(0)
        Catch ex As Exception
            Return ControlChars.Tab
        End Try

    End Function

    Public Function ParseProteinFile(strProteinInputFilePath As String, strOutputFolderPath As String) As Boolean
        Return ParseProteinFile(strProteinInputFilePath, strOutputFolderPath, String.Empty)
    End Function

    ''' <summary>
    ''' Process the protein FASTA file or tab-delimited text file
    ''' </summary>
    ''' <param name="strProteinInputFilePath"></param>
    ''' <param name="strOutputFolderPath"></param>
    ''' <param name="strOutputFileNameBaseOverride">Name for the protein output filename (auto-defined if empty)</param>
    ''' <returns></returns>
    Public Function ParseProteinFile(
      strProteinInputFilePath As String,
      strOutputFolderPath As String,
      strOutputFileNameBaseOverride As String) As Boolean

        Dim objProteinFileReader As ProteinFileReaderBaseClass = Nothing

        Dim swProteinOutputFile As StreamWriter = Nothing
        Dim swDigestOutputFile As StreamWriter = Nothing
        Dim srScrambledOutStream As StreamWriter = Nothing

        Dim blnSuccess As Boolean
        Dim blnInputProteinFound As Boolean
        Dim blnHeaderChecked As Boolean

        Dim blnAllowLookForAddnlRefInDescription As Boolean
        Dim blnLookForAddnlRefInDescription As Boolean
        Dim lstAddnlRefMasterNames As SortedSet(Of String)

        Dim udtAddnlRefsToOutput() As udtAddnlRefType = Nothing

        Dim blnGenerateUniqueSequenceID As Boolean

        Dim objRandomNumberGenerator As Random = Nothing

        Dim eScramblingMode As ProteinScramblingModeConstants
        Dim udtResidueCache = New udtScramblingResidueCacheType()

        Dim dtStartTime As Date

        ProcessingSummary = String.Empty

        Dim pathInfo As udtFilePathInfoType
        pathInfo.ProteinInputFilePath = strProteinInputFilePath
        pathInfo.OutputFolderPath = strOutputFolderPath
        pathInfo.OutputFileNameBaseOverride = strOutputFileNameBaseOverride
        pathInfo.ProteinOutputFilePath = String.Empty
        pathInfo.DigestedProteinOutputFilePath = String.Empty

        Try

            If String.IsNullOrWhiteSpace(pathInfo.ProteinInputFilePath) Then
                SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath)
                Return False
            End If

            ' Make sure the object variables are initialized
            If Not InitializeObjectVariables() Then
                blnSuccess = False
                Exit Try
            End If

            blnSuccess = ParseProteinFileCreateOutputFile(pathInfo, objProteinFileReader)

        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
            blnSuccess = False
        End Try

        ' Abort processing if we couldn't successfully open the input file
        If Not blnSuccess Then Return False

        Try
            ' Set the options for mpICalculator
            ' Note that this will also update the pICalculator object in objInSilicoDigest
            If Not mpICalculator Is Nothing Then
                With mpICalculator
                    .HydrophobicityType = mHydrophobicityType
                    .ReportMaximumpI = ReportMaximumpI
                    .SequenceWidthToExamineForMaximumpI = mSequenceWidthToExamineForMaximumpI
                End With
            End If

            If CreateProteinOutputFile Then
                Try
                    ' Open the protein output file (if required)
                    ' This will cause an error if the input file is the same as the output file
                    swProteinOutputFile = New StreamWriter(pathInfo.ProteinOutputFilePath)
                    blnSuccess = True
                Catch ex As Exception
                    SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile)
                    blnSuccess = False
                End Try
                If Not blnSuccess Then Exit Try
            End If

            If mMasterSequencesHashTable Is Nothing Then
                mMasterSequencesHashTable = New Hashtable
            Else
                mMasterSequencesHashTable.Clear()
            End If
            mNextUniqueIDForMasterSeqs = 1

            If CreateProteinOutputFile AndAlso CreateDigestedProteinOutputFile AndAlso Not CreateFastaOutputFile Then
                blnSuccess = ParseProteinFileCreateDigestedProteinOutputFile(pathInfo.DigestedProteinOutputFilePath, swDigestOutputFile)
                If Not blnSuccess Then Exit Try

                If GenerateUniqueIDValuesForPeptides Then
                    ' Initialize mMasterSequencesHashTable
                    blnGenerateUniqueSequenceID = True
                End If

            End If

            Dim intLoopcount = 1
            If mProteinScramblingMode = ProteinScramblingModeConstants.Randomized Then
                intLoopcount = mProteinScramblingLoopCount
                If intLoopcount < 1 Then intLoopcount = 1
                If intLoopcount > 10000 Then intLoopcount = 10000

                blnAllowLookForAddnlRefInDescription = False
            Else
                blnAllowLookForAddnlRefInDescription = FastaFileOptions.LookForAddnlRefInDescription
            End If

            Dim objHashGenerator As clsHashGenerator = Nothing
            Dim outLine = New StringBuilder()

            For intLoopIndex = 1 To intLoopcount

                ' Attempt to open the input file
                If Not objProteinFileReader.OpenFile(pathInfo.ProteinInputFilePath) Then
                    SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
                    blnSuccess = False
                    Exit Try
                End If

                If CreateProteinOutputFile Then
                    eScramblingMode = mProteinScramblingMode
                    With udtResidueCache
                        .SamplingPercentage = mProteinScramblingSamplingPercentage
                        If .SamplingPercentage <= 0 Then .SamplingPercentage = 100
                        If .SamplingPercentage > 100 Then .SamplingPercentage = 100

                        .Cache = String.Empty
                        .CacheLength = SCRAMBLING_CACHE_LENGTH
                        .OutputCount = 0
                        .ResiduesToWrite = String.Empty
                    End With

                    If eScramblingMode <> ProteinScramblingModeConstants.None Then

                        blnSuccess = InitializeScrambledOutput(pathInfo, udtResidueCache, eScramblingMode, srScrambledOutStream, objRandomNumberGenerator)
                        If Not blnSuccess Then Exit Try

                    End If
                Else
                    eScramblingMode = ProteinScramblingModeConstants.None
                End If

                dtStartTime = DateTime.UtcNow

                If CreateProteinOutputFile AndAlso mParsedFileIsFastaFile AndAlso blnAllowLookForAddnlRefInDescription Then
                    ' Need to pre-scan the fasta file to find all of the possible additional reference values

                    lstAddnlRefMasterNames = New SortedSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
                    PreScanProteinFileForAddnlRefsInDescription(pathInfo.ProteinInputFilePath, lstAddnlRefMasterNames)

                    If lstAddnlRefMasterNames.Count > 0 Then
                        ' Need to extract out the key names from htAddnlRefMasterNames and sort them alphabetically
                        ReDim udtAddnlRefsToOutput(lstAddnlRefMasterNames.Count - 1)
                        blnLookForAddnlRefInDescription = True

                        Dim intIndex = 0
                        For Each addnlRef In lstAddnlRefMasterNames
                            udtAddnlRefsToOutput(intIndex).RefName = String.Copy(addnlRef)
                            intIndex += 1
                        Next

                        Dim iAddnlRefComparerClass As New AddnlRefComparerClass
                        Array.Sort(udtAddnlRefsToOutput, iAddnlRefComparerClass)
                    Else
                        ReDim udtAddnlRefsToOutput(0)
                        blnLookForAddnlRefInDescription = False
                    End If
                End If

                ResetProgress("Parsing protein input file")

                If CreateProteinOutputFile AndAlso Not CreateFastaOutputFile Then
                    outLine.Clear()

                    ' Write the header line to the output file
                    outLine.Append("ProteinName" & mOutputFileDelimiter)

                    If blnLookForAddnlRefInDescription Then

                        For intIndex = 0 To udtAddnlRefsToOutput.Length - 1
                            With udtAddnlRefsToOutput(intIndex)
                                outLine.Append(.RefName & mOutputFileDelimiter)
                            End With
                        Next intIndex
                    End If

                    ' Include Description in the header line, even if we are excluding the description for all proteins
                    outLine.Append("Description")

                    If ComputeSequenceHashValues Then
                        outLine.Append(mOutputFileDelimiter & "SequenceHash")
                        objHashGenerator = New clsHashGenerator()
                    End If

                    If Not ExcludeProteinSequence Then
                        outLine.Append(mOutputFileDelimiter & "Sequence")
                    End If

                    If ComputeProteinMass Then
                        If ElementMassMode = PeptideSequenceClass.ElementModeConstants.AverageMass Then
                            outLine.Append(mOutputFileDelimiter & "Average Mass")
                        Else
                            outLine.Append(mOutputFileDelimiter & "Mass")
                        End If
                    End If

                    If ComputepI Then
                        outLine.Append(mOutputFileDelimiter & "pI" & mOutputFileDelimiter & "Hydrophobicity")
                    End If

                    If ComputeNET Then
                        outLine.Append(mOutputFileDelimiter & "LC_NET")
                    End If

                    If ComputeSCXNET Then
                        outLine.Append(mOutputFileDelimiter & "SCX_NET")
                    End If

                    swProteinOutputFile.WriteLine(outLine.ToString())
                End If

                ' Read each protein in the input file and process appropriately
                mProteinCount = 0
                ReDim mProteins(PROTEIN_CACHE_MEMORY_RESERVE_COUNT)

                mInputFileProteinsProcessed = 0
                mInputFileLineSkipCount = 0
                mInputFileLinesRead = 0
                Do
                    blnInputProteinFound = objProteinFileReader.ReadNextProteinEntry()
                    mInputFileLineSkipCount += objProteinFileReader.LineSkipCount

                    If Not blnInputProteinFound Then Continue Do

                    If Not blnHeaderChecked Then
                        If Not mParsedFileIsFastaFile Then
                            blnHeaderChecked = True

                            ' This may be a header line; possibly skip it
                            If objProteinFileReader.ProteinName.ToLower().StartsWith("protein") Then
                                If objProteinFileReader.ProteinDescription.ToLower().Contains("description") AndAlso
                                    objProteinFileReader.ProteinSequence.ToLower().Contains("sequence") Then
                                    ' Skip this entry since it's a header line, for example:
                                    ' ProteinName    Description    Sequence
                                    Continue Do
                                End If
                            End If

                            If objProteinFileReader.ProteinName.ToLower().Contains("protein") AndAlso
                               FractionLowercase(objProteinFileReader.ProteinSequence) > 0.2 Then
                                ' Skip this entry since it's a header line, for example:
                                ' FirstProtein    ProteinDesc    Sequence
                                Continue Do
                            End If

                        End If
                    End If

                    mInputFileProteinsProcessed += 1
                    mInputFileLinesRead = objProteinFileReader.LinesRead

                    ParseProteinFileStoreProtein(objProteinFileReader, objHashGenerator, blnLookForAddnlRefInDescription)

                    If CreateProteinOutputFile Then
                        If intLoopIndex = 1 Then

                            If CreateFastaOutputFile Then
                                ParseProteinFileWriteFasta(swProteinOutputFile, outLine)
                            Else
                                ParseProteinFileWriteTextDelimited(swProteinOutputFile, outLine, blnLookForAddnlRefInDescription, udtAddnlRefsToOutput)
                            End If

                        End If

                        If intLoopIndex = 1 AndAlso Not swDigestOutputFile Is Nothing Then
                            ParseProteinFileWriteDigested(swDigestOutputFile, outLine, blnGenerateUniqueSequenceID)
                        End If

                        If eScramblingMode <> ProteinScramblingModeConstants.None Then
                            WriteScrambledFasta(srScrambledOutStream, objRandomNumberGenerator, mProteins(mProteinCount), eScramblingMode, udtResidueCache)
                        End If

                    Else
                        ' Cache the proteins in memory
                        mProteinCount += 1
                        If mProteinCount >= mProteins.Length Then
                            ReDim Preserve mProteins(mProteins.Length + PROTEIN_CACHE_MEMORY_RESERVE_COUNT)
                        End If
                    End If

                    Dim sngPercentProcessed = (intLoopIndex - 1) / CSng(intLoopcount) * 100.0! + objProteinFileReader.PercentFileProcessed() / intLoopcount
                    UpdateProgress(sngPercentProcessed)

                    If Me.AbortProcessing Then Exit Do

                Loop While blnInputProteinFound

                If CreateProteinOutputFile And eScramblingMode <> ProteinScramblingModeConstants.None Then
                    ' Write out anything remaining in the cache

                    Dim strProteinNamePrefix As String
                    If eScramblingMode = ProteinScramblingModeConstants.Reversed Then
                        strProteinNamePrefix = PROTEIN_PREFIX_REVERSED
                    Else
                        strProteinNamePrefix = PROTEIN_PREFIX_SCRAMBLED
                    End If

                    WriteFastaAppendToCache(srScrambledOutStream, udtResidueCache, strProteinNamePrefix, True)
                End If

                If mProteinCount > 0 Then
                    ReDim Preserve mProteins(mProteinCount - 1)
                Else
                    ReDim mProteins(0)
                End If

                objProteinFileReader.CloseFile()

                If Not swProteinOutputFile Is Nothing Then
                    swProteinOutputFile.Close()
                End If

                If Not swDigestOutputFile Is Nothing Then
                    swDigestOutputFile.Close()
                End If

                If Not srScrambledOutStream Is Nothing Then
                    srScrambledOutStream.Close()
                End If
            Next intLoopIndex

            If ShowDebugPrompts Then
                MessageBox.Show(Path.GetFileName(pathInfo.ProteinInputFilePath) & ControlChars.NewLine & "Elapsed time: " & Math.Round(DateTime.UtcNow.Subtract(dtStartTime).TotalSeconds, 2).ToString & " seconds", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            Dim strMessage = "Done: Processed " & mInputFileProteinsProcessed.ToString("###,##0") & " proteins (" & mInputFileLinesRead.ToString("###,###,##0") & " lines)"
            If mInputFileLineSkipCount > 0 Then
                strMessage &= ControlChars.NewLine & "Note that " & mInputFileLineSkipCount.ToString("###,##0") & " lines were skipped in the input file due to having an unexpected format. "
                If mParsedFileIsFastaFile Then
                    strMessage &= "This is an unexpected error for fasta files."
                Else
                    strMessage &= "Make sure that " & mDelimitedInputFileFormatCode.ToString & " is the appropriate format for this file (see the File Format Options tab)."
                End If
            End If

            If intLoopcount > 1 Then
                strMessage &= ControlChars.NewLine & "Created " & intLoopcount.ToString & " replicates of the scrambled output file"
            End If

            ProcessingSummary = strMessage
            OnStatusEvent(strMessage)

            blnSuccess = True

        Catch ex As Exception
            ShowErrorMessage("Error in ParseProteinFile: " & ex.Message)
            If CreateProteinOutputFile Or CreateDigestedProteinOutputFile Then
                SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorWritingOutputFile)
                blnSuccess = False
            Else
                SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
                blnSuccess = False
            End If
        Finally
            If Not swProteinOutputFile Is Nothing Then
                swProteinOutputFile.Close()
            End If
            If Not swDigestOutputFile Is Nothing Then
                swDigestOutputFile.Close()
            End If
            If Not srScrambledOutStream Is Nothing Then
                srScrambledOutStream.Close()
            End If
        End Try

        Return blnSuccess

    End Function

    Private Sub ParseProteinFileStoreProtein(
      objProteinFileReader As ProteinFileReaderBaseClass,
      objHashGenerator As clsHashGenerator,
      blnLookForAddnlRefInDescription As Boolean)


        With mProteins(mProteinCount)
            .Name = objProteinFileReader.ProteinName
            .Description = objProteinFileReader.ProteinDescription

            If TruncateProteinDescription AndAlso .Description.Length > MAX_PROTEIN_DESCRIPTION_LENGTH Then
                .Description = .Description.Substring(0, MAX_PROTEIN_DESCRIPTION_LENGTH - 3) & "..."
            End If

            If blnLookForAddnlRefInDescription Then
                ' Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
                .AlternateNameCount = ExtractAlternateProteinNamesFromDescription(.Description, .AlternateNames)
            Else
                .AlternateNameCount = 0
                ReDim .AlternateNames(0)
            End If

            .Sequence = objProteinFileReader.ProteinSequence

            If Not objHashGenerator Is Nothing Then
                If ComputeSequenceHashIgnoreILDiff Then
                    .SequenceHash = objHashGenerator.GenerateHash(.Sequence.Replace("L"c, "I"c))
                Else
                    .SequenceHash = objHashGenerator.GenerateHash(.Sequence)
                End If
            End If

            If ComputeProteinMass Then
                .Mass = ComputeSequenceMass(.Sequence)
            Else
                .Mass = 0
            End If

            If ComputepI Then
                .pI = ComputeSequencepI(.Sequence)
                .Hydrophobicity = ComputeSequenceHydrophobicity(.Sequence)
            Else
                .pI = 0
                .Hydrophobicity = 0
            End If

            If ComputeNET Then
                .ProteinNET = ComputeSequenceNET(.Sequence)
            End If

            If ComputeSCXNET Then
                .ProteinSCXNET = ComputeSequenceSCXNET(.Sequence)
            End If

        End With

    End Sub

    Private Sub ParseProteinFileWriteDigested(
      swDigestOutputFile As TextWriter,
      outLine As StringBuilder,
      blnGenerateUniqueSequenceID As Boolean)

        Dim udtPeptides() As clsInSilicoDigest.PeptideInfoClass = Nothing
        Dim intPeptideCount = DigestProteinSequence(mProteins(mProteinCount).Sequence, udtPeptides, DigestionOptions, mProteins(mProteinCount).Name)

        For intIndex = 0 To intPeptideCount - 1
            With udtPeptides(intIndex)
                Dim intUniqueSeqID As Integer

                If blnGenerateUniqueSequenceID Then
                    Try
                        If mMasterSequencesHashTable.ContainsKey(.SequenceOneLetter) Then
                            intUniqueSeqID = CInt(mMasterSequencesHashTable(.SequenceOneLetter))
                        Else
                            mMasterSequencesHashTable.Add(.SequenceOneLetter, mNextUniqueIDForMasterSeqs)
                            intUniqueSeqID = mNextUniqueIDForMasterSeqs
                        End If
                        mNextUniqueIDForMasterSeqs += 1
                    Catch ex As Exception
                        intUniqueSeqID = 0
                    End Try
                Else
                    intUniqueSeqID = 0
                End If

                Dim strBaseSequence = .SequenceOneLetter
                outLine.Clear()

                If Not ExcludeProteinSequence Then
                    outLine.Append(mProteins(mProteinCount).Name & mOutputFileDelimiter)
                    If DigestionOptions.IncludePrefixAndSuffixResidues Then
                        outLine.Append(.PrefixResidue & "." & strBaseSequence & "." & .SuffixResidue & mOutputFileDelimiter)
                    Else
                        outLine.Append(strBaseSequence & mOutputFileDelimiter)
                    End If
                End If
                outLine.Append(intUniqueSeqID.ToString & mOutputFileDelimiter &
                               .Mass & mOutputFileDelimiter &
                               Math.Round(.NET, 4).ToString & mOutputFileDelimiter &
                               .PeptideName)

                If ComputepI Then
                    Dim sngpI = ComputeSequencepI(strBaseSequence)
                    Dim sngHydrophobicity = ComputeSequenceHydrophobicity(strBaseSequence)

                    outLine.Append(mOutputFileDelimiter & sngpI.ToString("0.000") &
                                   mOutputFileDelimiter & sngHydrophobicity.ToString("0.0000"))
                End If

                If ComputeNET Then
                    Dim sngLCNET = ComputeSequenceSCXNET(strBaseSequence)

                    outLine.Append(mOutputFileDelimiter & sngLCNET.ToString("0.0000"))
                End If

                If ComputeSCXNET Then
                    Dim sngSCXNET = ComputeSequenceSCXNET(strBaseSequence)

                    outLine.Append(mOutputFileDelimiter & sngSCXNET.ToString("0.0000"))
                End If

                swDigestOutputFile.WriteLine(outLine.ToString())
            End With
        Next intIndex

    End Sub

    Private Sub ParseProteinFileWriteFasta(swProteinOutputFile As TextWriter, outLine As StringBuilder)
        ' Write the entry to the output fasta file

        With mProteins(mProteinCount)
            If .Name = "ProteinName" AndAlso .Description = "Description" AndAlso .Sequence = "Sequence" Then
                ' Skip this entry; it's an artifact from converting from a fasta file to a text file, then back to a fasta file
                Return
            End If

            outLine.Clear()
            outLine.Append(FastaFileOptions.ProteinLineStartChar & .Name)
            If Not ExcludeProteinDescription Then
                outLine.Append(FastaFileOptions.ProteinLineAccessionEndChar & .Description)
            End If
            swProteinOutputFile.WriteLine(outLine.ToString())

            If Not ExcludeProteinSequence Then
                Dim intIndex = 0
                Do While intIndex < .Sequence.Length
                    Dim intLength = Math.Min(60, .Sequence.Length - intIndex)
                    swProteinOutputFile.WriteLine(.Sequence.Substring(intIndex, intLength))
                    intIndex += 60
                Loop
            End If
        End With
    End Sub

    Private Sub ParseProteinFileWriteTextDelimited(
      swProteinOutputFile As TextWriter,
      outLine As StringBuilder,
      blnLookForAddnlRefInDescription As Boolean,
      ByRef udtAddnlRefsToOutput() As udtAddnlRefType)

        ' Write the entry to the protein output file, and possibly digest it

        outLine.Clear()

        With mProteins(mProteinCount)

            If blnLookForAddnlRefInDescription Then
                ' Reset the Accession numbers in udtAddnlRefsToOutput
                For intIndex = 0 To udtAddnlRefsToOutput.Length - 1
                    udtAddnlRefsToOutput(intIndex).RefAccession = String.Empty
                Next intIndex

                ' Update the accession numbers in udtAddnlRefsToOutput
                For intIndex = 0 To .AlternateNameCount - 1
                    For intCompareIndex = 0 To udtAddnlRefsToOutput.Length - 1
                        If udtAddnlRefsToOutput(intCompareIndex).RefName.ToUpper = .AlternateNames(intIndex).RefName.ToUpper Then
                            udtAddnlRefsToOutput(intCompareIndex).RefAccession = .AlternateNames(intIndex).RefAccession
                            Exit For
                        End If
                    Next intCompareIndex
                Next intIndex

                outLine.Append(.Name & mOutputFileDelimiter)
                For intIndex = 0 To udtAddnlRefsToOutput.Length - 1
                    With udtAddnlRefsToOutput(intIndex)
                        outLine.Append(.RefAccession & mOutputFileDelimiter)
                    End With
                Next intIndex

                If Not ExcludeProteinDescription Then
                    outLine.Append(.Description)
                End If

            Else
                outLine.Append(.Name & mOutputFileDelimiter)
                If Not ExcludeProteinDescription Then
                    outLine.Append(.Description)
                End If

            End If

            If ComputeSequenceHashValues Then
                outLine.Append(mOutputFileDelimiter & .SequenceHash)
            End If

            If Not ExcludeProteinSequence Then
                outLine.Append(mOutputFileDelimiter & .Sequence)
            End If

            If ComputeProteinMass Then
                outLine.Append(mOutputFileDelimiter & Math.Round(.Mass, 5).ToString)
            End If

            If ComputepI Then
                outLine.Append(mOutputFileDelimiter & .pI.ToString("0.000") & mOutputFileDelimiter & .Hydrophobicity.ToString("0.0000"))
            End If

            If ComputeNET Then
                outLine.Append(mOutputFileDelimiter & .ProteinNET.ToString("0.0000"))
            End If

            If ComputeSCXNET Then
                outLine.Append(mOutputFileDelimiter & .ProteinSCXNET.ToString("0.0000"))
            End If

        End With
        swProteinOutputFile.WriteLine(outLine.ToString())

    End Sub

    Private Function ParseProteinFileCreateOutputFile(
      ByRef pathInfo As udtFilePathInfoType,
      <Out> ByRef objProteinFileReader As ProteinFileReaderBaseClass) As Boolean

        Dim strOutputFileName As String
        Dim blnSuccess As Boolean

        If AssumeFastaFile OrElse IsFastaFile(pathInfo.ProteinInputFilePath) Then
            If AssumeDelimitedFile Then
                mParsedFileIsFastaFile = False
            Else
                mParsedFileIsFastaFile = True
            End If
        Else
            mParsedFileIsFastaFile = False
        End If

        If CreateDigestedProteinOutputFile Then
            ' Make sure mCreateFastaOutputFile is false
            CreateFastaOutputFile = False
        End If

        If Not String.IsNullOrEmpty(pathInfo.OutputFileNameBaseOverride) Then
            If Path.HasExtension(pathInfo.OutputFileNameBaseOverride) Then
                strOutputFileName = String.Copy(pathInfo.OutputFileNameBaseOverride)

                If CreateFastaOutputFile Then
                    If Path.GetExtension(strOutputFileName).ToLower <> ".fasta" Then
                        strOutputFileName &= ".fasta"
                    End If
                Else
                    If Path.GetExtension(strOutputFileName).Length > 4 Then
                        strOutputFileName &= ".txt"
                    End If
                End If
            Else
                If CreateFastaOutputFile Then
                    strOutputFileName = pathInfo.OutputFileNameBaseOverride & ".fasta"
                Else
                    strOutputFileName = pathInfo.OutputFileNameBaseOverride & ".txt"
                End If
            End If
        Else
            strOutputFileName = String.Empty
        End If

        If mParsedFileIsFastaFile Then
            Dim objFastaFileReader = New FastaFileReader
            With objFastaFileReader
                .ProteinLineStartChar = FastaFileOptions.ProteinLineStartChar
                .ProteinLineAccessionEndChar = FastaFileOptions.ProteinLineAccessionEndChar
            End With
            objProteinFileReader = objFastaFileReader
        Else
            Dim objDelimitedFileReader = New DelimitedFileReader
            With objDelimitedFileReader
                .Delimiter = mInputFileDelimiter
                .DelimitedFileFormatCode = mDelimitedInputFileFormatCode
            End With
            objProteinFileReader = objDelimitedFileReader
        End If

        ' Verify that the input file exists
        If Not File.Exists(pathInfo.ProteinInputFilePath) Then
            MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath)
            blnSuccess = False
            Return blnSuccess
        End If

        If mParsedFileIsFastaFile Then
            If strOutputFileName.Length = 0 Then
                strOutputFileName = Path.GetFileName(pathInfo.ProteinInputFilePath)
                If Path.GetExtension(strOutputFileName).ToLower = ".fasta" Then
                    ' Nothing special to do; will replace the extension below
                Else
                    ' .Fasta appears somewhere in the middle
                    ' Remove the text .Fasta, then add the extension .txt (unless it already ends in .txt)
                    Dim intLoc As Integer
                    intLoc = strOutputFileName.ToLower.LastIndexOf(".fasta", StringComparison.Ordinal)
                    If intLoc > 0 Then
                        If intLoc < strOutputFileName.Length Then
                            strOutputFileName = strOutputFileName.Substring(0, intLoc) & strOutputFileName.Substring(intLoc + 6)
                        Else
                            strOutputFileName = strOutputFileName.Substring(0, intLoc)
                        End If
                    Else
                        ' This shouldn't happen
                    End If
                End If

                If CreateFastaOutputFile Then
                    strOutputFileName = Path.GetFileNameWithoutExtension(strOutputFileName) & "_new.fasta"
                Else
                    strOutputFileName = Path.ChangeExtension(strOutputFileName, ".txt")
                End If
            End If
        Else
            If strOutputFileName.Length = 0 Then
                If CreateFastaOutputFile Then
                    strOutputFileName = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) & ".fasta"
                Else
                    strOutputFileName = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) & "_parsed.txt"
                End If
            End If
        End If

        ' Make sure the output file isn't the same as the input file
        If Path.GetFileName(pathInfo.ProteinInputFilePath).ToLower = Path.GetFileName(strOutputFileName).ToLower Then
            strOutputFileName = Path.GetFileNameWithoutExtension(strOutputFileName) & "_new" & Path.GetExtension(strOutputFileName)
        End If

        ' Define the full path to the parsed proteins output file
        pathInfo.ProteinOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, strOutputFileName)

        If strOutputFileName.EndsWith("_parsed.txt") Then
            strOutputFileName = strOutputFileName.Substring(0, strOutputFileName.Length - "_parsed.txt".Length) & "_digested"
        Else
            strOutputFileName = Path.GetFileNameWithoutExtension(strOutputFileName) & "_digested"
        End If

        strOutputFileName &= "_Mass" & Math.Round(DigestionOptions.MinFragmentMass, 0).ToString & "to" & Math.Round(DigestionOptions.MaxFragmentMass, 0).ToString

        If ComputepI AndAlso (DigestionOptions.MinIsoelectricPoint > 0 OrElse DigestionOptions.MaxIsoelectricPoint < 14) Then
            strOutputFileName &= "_pI" & Math.Round(DigestionOptions.MinIsoelectricPoint, 1).ToString & "to" & Math.Round(DigestionOptions.MaxIsoelectricPoint, 2).ToString
        End If

        strOutputFileName &= ".txt"

        ' Define the full path to the digested proteins output file
        pathInfo.DigestedProteinOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, strOutputFileName)

        blnSuccess = True
        Return blnSuccess
    End Function

    Private Function ParseProteinFileCreateDigestedProteinOutputFile(strDigestedProteinOutputFilePath As String, ByRef swDigestOutputFile As StreamWriter) As Boolean

        Try
            ' Create the digested protein output file
            swDigestOutputFile = New StreamWriter(strDigestedProteinOutputFilePath)

            Dim strLineOut As String
            If Not ExcludeProteinSequence Then
                strLineOut = "Protein_Name" & mOutputFileDelimiter & "Sequence" & mOutputFileDelimiter
            Else
                strLineOut = String.Empty
            End If
            strLineOut &= "Unique_ID" & mOutputFileDelimiter & "Monoisotopic_Mass" & mOutputFileDelimiter & "Predicted_NET" & mOutputFileDelimiter & "Tryptic_Name"

            If ComputepI Then
                strLineOut &= mOutputFileDelimiter & "pI" & mOutputFileDelimiter & "Hydrophobicity"
            End If

            If ComputeNET Then
                strLineOut &= mOutputFileDelimiter & "LC_NET"
            End If

            If ComputeSCXNET Then
                strLineOut &= mOutputFileDelimiter & "SCX_NET"
            End If

            swDigestOutputFile.WriteLine(strLineOut)

            Return True
        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile)
            Return False
        End Try

    End Function

    Private Sub PreScanProteinFileForAddnlRefsInDescription(
      strProteinInputFilePath As String,
      lstAddnlRefMasterNames As ISet(Of String))

        Dim objFastaFileReader As FastaFileReader = Nothing
        Dim udtProtein As udtProteinInfoType

        Dim intIndex As Integer

        Dim blnSuccess As Boolean
        Dim blnInputProteinFound As Boolean

        Try
            objFastaFileReader = New FastaFileReader
            With objFastaFileReader
                .ProteinLineStartChar = FastaFileOptions.ProteinLineStartChar
                .ProteinLineAccessionEndChar = FastaFileOptions.ProteinLineAccessionEndChar
            End With

            ' Attempt to open the input file
            If Not objFastaFileReader.OpenFile(strProteinInputFilePath) Then
                blnSuccess = False
                Exit Try
            End If

            blnSuccess = True

        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
            blnSuccess = False
        End Try

        ' Abort processing if we couldn't successfully open the input file
        If Not blnSuccess Then Return

        Try

            ResetProgress("Pre-reading Fasta file; looking for possible additional reference names")

            ' Read each protein in the output file and process appropriately
            Do
                blnInputProteinFound = objFastaFileReader.ReadNextProteinEntry()

                If blnInputProteinFound Then
                    With udtProtein
                        .Name = objFastaFileReader.ProteinName
                        .Description = objFastaFileReader.ProteinDescription

                        ' Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
                        ReDim .AlternateNames(0)
                        .AlternateNameCount = ExtractAlternateProteinNamesFromDescription(.Description, .AlternateNames)

                        ' Make sure each of the names in .AlternateNames() is in htAddnlRefMasterNames
                        For intIndex = 0 To .AlternateNameCount - 1
                            If Not lstAddnlRefMasterNames.Contains(.AlternateNames(intIndex).RefName) Then
                                lstAddnlRefMasterNames.Add(.AlternateNames(intIndex).RefName)
                            End If
                        Next intIndex
                    End With

                    UpdateProgress(objFastaFileReader.PercentFileProcessed())

                    If Me.AbortProcessing Then Exit Do
                End If
            Loop While blnInputProteinFound

            objFastaFileReader.CloseFile()

        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
        End Try

        Return
    End Sub

    Private Function ValidateProteinName(strProteinName As String, intMaximumLength As Integer) As String
        Dim chSepChars = New Char() {" "c, ","c, ";"c, ":"c, "_"c, "-"c, "|"c, "/"c}

        If intMaximumLength < 1 Then intMaximumLength = 1

        If strProteinName Is Nothing Then
            strProteinName = String.Empty
        Else
            If strProteinName.Length > intMaximumLength Then
                ' Truncate protein name to maximum length
                strProteinName = strProteinName.Substring(0, intMaximumLength)

                ' Make sure the protein name doesn't end in a space, dash, underscore, semicolon, colon, etc.
                strProteinName = strProteinName.TrimEnd(chSepChars)
            End If
        End If

        Return strProteinName

    End Function

    Private Sub WriteFastaAppendToCache(srScrambledOutStream As TextWriter, ByRef udtResidueCache As udtScramblingResidueCacheType, strProteinNamePrefix As String, blnFlushResiduesToWrite As Boolean)

        Dim intResidueCount As Integer
        Dim intResiduesToAppend As Integer

        With udtResidueCache
            If .Cache.Length > 0 Then
                intResidueCount = CInt(Math.Round(.Cache.Length * .SamplingPercentage / 100.0, 0))
                If intResidueCount < 1 Then intResidueCount = 1
                If intResidueCount > .Cache.Length Then intResidueCount = .Cache.Length

                Do While intResidueCount > 0
                    If .ResiduesToWrite.Length + intResidueCount <= .CacheLength Then
                        .ResiduesToWrite &= .Cache.Substring(0, intResidueCount)
                        .Cache = String.Empty
                        intResidueCount = 0
                    Else
                        intResiduesToAppend = .CacheLength - .ResiduesToWrite.Length
                        .ResiduesToWrite &= .Cache.Substring(0, intResiduesToAppend)
                        .Cache = .Cache.Substring(intResiduesToAppend)
                        intResidueCount -= intResiduesToAppend
                    End If

                    If .ResiduesToWrite.Length >= udtResidueCache.CacheLength Then
                        ' Write out .ResiduesToWrite
                        WriteFastaEmptyCache(srScrambledOutStream, udtResidueCache, strProteinNamePrefix, .SamplingPercentage)
                    End If

                Loop

            End If

            If blnFlushResiduesToWrite AndAlso .ResiduesToWrite.Length > 0 Then
                WriteFastaEmptyCache(srScrambledOutStream, udtResidueCache, strProteinNamePrefix, .SamplingPercentage)
            End If
        End With

    End Sub

    Private Sub WriteFastaEmptyCache(srScrambledOutStream As TextWriter, ByRef udtResidueCache As udtScramblingResidueCacheType, strProteinNamePrefix As String, intSamplingPercentage As Integer)
        Dim strProteinName As String
        Dim strHeaderLine As String

        With udtResidueCache
            If .ResiduesToWrite.Length > 0 Then
                .OutputCount += 1

                strProteinName = strProteinNamePrefix & mFileNameAbbreviated

                If intSamplingPercentage < 100 Then
                    strProteinName = ValidateProteinName(strProteinName, MAXIMUM_PROTEIN_NAME_LENGTH - 7 - Math.Max(5, .OutputCount.ToString.Length))
                Else
                    strProteinName = ValidateProteinName(strProteinName, MAXIMUM_PROTEIN_NAME_LENGTH - 1 - Math.Max(5, .OutputCount.ToString.Length))
                End If

                If intSamplingPercentage < 100 Then
                    strProteinName &= "_" & intSamplingPercentage.ToString & "pct" & "_"
                Else
                    strProteinName &= strProteinName & "_"
                End If

                strProteinName &= .OutputCount.ToString

                strHeaderLine = FastaFileOptions.ProteinLineStartChar & strProteinName & FastaFileOptions.ProteinLineAccessionEndChar & strProteinName

                WriteFastaProteinAndResidues(srScrambledOutStream, strHeaderLine, .ResiduesToWrite)
                .ResiduesToWrite = String.Empty
            End If
        End With

    End Sub

    Private Sub WriteFastaProteinAndResidues(srScrambledOutStream As TextWriter, strHeaderLine As String, strSequence As String)
        srScrambledOutStream.WriteLine(strHeaderLine)
        Do While strSequence.Length > 0
            If strSequence.Length >= 60 Then
                srScrambledOutStream.WriteLine(strSequence.Substring(0, 60))
                strSequence = strSequence.Substring(60)
            Else
                srScrambledOutStream.WriteLine(strSequence)
                strSequence = String.Empty
            End If
        Loop
    End Sub

    Private Sub WriteScrambledFasta(srScrambledOutStream As TextWriter, ByRef objRandomNumberGenerator As Random, udtProtein As udtProteinInfoType, eScramblingMode As ProteinScramblingModeConstants, ByRef udtResidueCache As udtScramblingResidueCacheType)

        Dim strSequence As String
        Dim strScrambledSequence As String
        Dim strHeaderLine As String

        Dim strProteinNamePrefix As String
        Dim strProteinName As String

        Dim intIndex As Integer
        Dim intResidueCount As Integer

        If eScramblingMode = ProteinScramblingModeConstants.Reversed Then
            strProteinNamePrefix = PROTEIN_PREFIX_REVERSED
            strScrambledSequence = StrReverse(udtProtein.Sequence)
        Else
            strProteinNamePrefix = PROTEIN_PREFIX_SCRAMBLED

            strScrambledSequence = String.Empty

            strSequence = String.Copy(udtProtein.Sequence)
            intResidueCount = strSequence.Length

            Do While intResidueCount > 0
                If intResidueCount <> strSequence.Length Then
                    ShowErrorMessage("Assertion failed in WriteScrambledFasta: intResidueCount should equal strSequence.Length: " & intResidueCount & " vs. " & strSequence.Length)
                End If

                ' Randomly extract residues from strSequence
                intIndex = objRandomNumberGenerator.Next(intResidueCount)

                strScrambledSequence &= strSequence.Substring(intIndex, 1)

                If intIndex > 0 Then
                    If intIndex < strSequence.Length - 1 Then
                        strSequence = strSequence.Substring(0, intIndex) & strSequence.Substring(intIndex + 1)
                    Else
                        strSequence = strSequence.Substring(0, intIndex)
                    End If
                Else
                    strSequence = strSequence.Substring(intIndex + 1)
                End If
                intResidueCount -= 1
            Loop

        End If

        If udtResidueCache.SamplingPercentage >= 100 Then
            With udtProtein
                strProteinName = ValidateProteinName(strProteinNamePrefix & .Name, MAXIMUM_PROTEIN_NAME_LENGTH)
                strHeaderLine = FastaFileOptions.ProteinLineStartChar & strProteinName & FastaFileOptions.ProteinLineAccessionEndChar & .Description
            End With

            WriteFastaProteinAndResidues(srScrambledOutStream, strHeaderLine, strScrambledSequence)
        Else
            ' Writing a sampling of the residues to the output file

            Do While strScrambledSequence.Length > 0
                With udtResidueCache
                    ' Append to the cache
                    If .Cache.Length + strScrambledSequence.Length <= .CacheLength Then
                        .Cache &= strScrambledSequence
                        strScrambledSequence = String.Empty
                    Else
                        intResidueCount = .CacheLength - .Cache.Length
                        .Cache &= strScrambledSequence.Substring(0, intResidueCount)
                        strScrambledSequence = strScrambledSequence.Substring(intResidueCount)
                    End If
                End With

                If udtResidueCache.Cache.Length >= udtResidueCache.CacheLength Then
                    ' Write out a portion of the cache
                    WriteFastaAppendToCache(srScrambledOutStream, udtResidueCache, strProteinNamePrefix, False)
                End If
            Loop

        End If

    End Sub

    ''' <summary>
    ''' Main processing function -- Calls ParseProteinFile
    ''' </summary>
    ''' <param name="strInputFilePath"></param>
    ''' <param name="strOutputFolderPath"></param>
    ''' <param name="strParameterFilePath"></param>
    ''' <param name="blnResetErrorCode"></param>
    ''' <returns></returns>
    Public Overloads Overrides Function ProcessFile(strInputFilePath As String, strOutputFolderPath As String, strParameterFilePath As String, blnResetErrorCode As Boolean) As Boolean
        ' Returns True if success, False if failure

        Dim ioFile As FileInfo
        Dim strInputFilePathFull As String
        Dim strStatusMessage As String

        Dim blnSuccess As Boolean

        If blnResetErrorCode Then
            SetLocalErrorCode(eParseProteinFileErrorCodes.NoError)
        End If

        If Not LoadParameterFileSettings(strParameterFilePath) Then
            strStatusMessage = "Parameter file load error: " & strParameterFilePath
            ShowErrorMessage(strStatusMessage)
            If MyBase.ErrorCode = ProcessFilesErrorCodes.NoError Then
                MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidParameterFile)
            End If
            Return False
        End If

        Try
            If strInputFilePath Is Nothing OrElse strInputFilePath.Length = 0 Then
                ShowErrorMessage("Input file name is empty")
                MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath)
            Else

                Console.WriteLine()
                ShowMessage("Parsing " & Path.GetFileName(strInputFilePath))

                If Not CleanupFilePaths(strInputFilePath, strOutputFolderPath) Then
                    MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.FilePathError)
                Else
                    Try
                        ' Obtain the full path to the input file
                        ioFile = New FileInfo(strInputFilePath)
                        strInputFilePathFull = ioFile.FullName

                        blnSuccess = ParseProteinFile(strInputFilePathFull, strOutputFolderPath)

                    Catch ex As Exception
                        Throw New Exception("Error calling ParseProteinFile", ex)
                    End Try
                End If
            End If
        Catch ex As Exception
            Throw New Exception("Error in ProcessFile", ex)
        End Try

        Return blnSuccess

    End Function

    Private Sub SetLocalErrorCode(eNewErrorCode As eParseProteinFileErrorCodes)
        SetLocalErrorCode(eNewErrorCode, False)
    End Sub

    Private Sub SetLocalErrorCode(eNewErrorCode As eParseProteinFileErrorCodes, blnLeaveExistingErrorCodeUnchanged As Boolean)

        If blnLeaveExistingErrorCodeUnchanged AndAlso mLocalErrorCode <> eParseProteinFileErrorCodes.NoError Then
            ' An error code is already defined; do not change it
        Else
            mLocalErrorCode = eNewErrorCode

            If eNewErrorCode = eParseProteinFileErrorCodes.NoError Then
                If MyBase.ErrorCode = ProcessFilesErrorCodes.LocalizedError Then
                    MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.NoError)
                End If
            Else
                MyBase.SetBaseClassErrorCode(ProcessFilesErrorCodes.LocalizedError)
            End If
        End If

    End Sub

    Protected Sub UpdateSubtaskProgress(strProgressStepDescription As String)
        UpdateProgress(strProgressStepDescription, mProgressPercentComplete)
    End Sub

    Protected Sub UpdateSubtaskProgress(sngPercentComplete As Single)
        UpdateProgress(Me.ProgressStepDescription, sngPercentComplete)
    End Sub

    Protected Sub UpdateSubtaskProgress(strProgressStepDescription As String, sngPercentComplete As Single)
        Dim blnDescriptionChanged = strProgressStepDescription <> mSubtaskProgressStepDescription

        mSubtaskProgressStepDescription = String.Copy(strProgressStepDescription)
        If sngPercentComplete < 0 Then
            sngPercentComplete = 0
        ElseIf sngPercentComplete > 100 Then
            sngPercentComplete = 100
        End If
        mSubtaskProgressPercentComplete = sngPercentComplete

        If blnDescriptionChanged Then
            If Math.Abs(mSubtaskProgressPercentComplete - 0) < Single.Epsilon Then
                LogMessage(mSubtaskProgressStepDescription.Replace(ControlChars.NewLine, "; "))
            Else
                LogMessage(mSubtaskProgressStepDescription & " (" & mSubtaskProgressPercentComplete.ToString("0.0") & "% complete)".Replace(ControlChars.NewLine, "; "))
            End If
        End If

        RaiseEvent SubtaskProgressChanged(strProgressStepDescription, sngPercentComplete)

    End Sub

    ' IComparer class to allow comparison of additional protein references
    Private Class AddnlRefComparerClass
        Implements IComparer(Of udtAddnlRefType)

        Public Function Compare(x As udtAddnlRefType, y As udtAddnlRefType) As Integer Implements IComparer(Of udtAddnlRefType).Compare

            If x.RefName > y.RefName Then
                Return 1
            ElseIf x.RefName < y.RefName Then
                Return -1
            Else
                If x.RefAccession > y.RefAccession Then
                    Return 1
                ElseIf x.RefAccession < y.RefAccession Then
                    Return -1
                Else
                    Return 0
                End If
            End If

        End Function

    End Class

    ' Options class
    Public Class FastaFileOptionsClass

        Public Sub New()
            mProteinLineStartChar = ">"c
            mProteinLineAccessionEndChar = " "c

            mLookForAddnlRefInDescription = False
            mAddnlRefSepChar = "|"c
            mAddnlRefAccessionSepChar = ":"c
        End Sub

#Region "Classwide Variables"
        Private mReadonlyClass As Boolean

        Private mProteinLineStartChar As Char
        Private mProteinLineAccessionEndChar As Char

        Private mLookForAddnlRefInDescription As Boolean

        Private mAddnlRefSepChar As Char
        Private mAddnlRefAccessionSepChar As Char

#End Region

#Region "Processing Options Interface Functions"
        Public Property ReadonlyClass As Boolean
            Get
                Return mReadonlyClass
            End Get
            Set
                If Not mReadonlyClass Then
                    mReadonlyClass = Value
                End If
            End Set
        End Property

        Public Property ProteinLineStartChar As Char
            Get
                Return mProteinLineStartChar
            End Get
            Set
                If Not Value = Nothing AndAlso Not mReadonlyClass Then
                    mProteinLineStartChar = Value
                End If
            End Set
        End Property

        Public Property ProteinLineAccessionEndChar As Char
            Get
                Return mProteinLineAccessionEndChar
            End Get
            Set
                If Not Value = Nothing AndAlso Not mReadonlyClass Then
                    mProteinLineAccessionEndChar = Value
                End If
            End Set
        End Property

        Public Property LookForAddnlRefInDescription As Boolean
            Get
                Return mLookForAddnlRefInDescription
            End Get
            Set
                If Not mReadonlyClass Then
                    mLookForAddnlRefInDescription = Value
                End If
            End Set
        End Property

        Public Property AddnlRefSepChar As Char
            Get
                Return mAddnlRefSepChar
            End Get
            Set
                If Not Value = Nothing AndAlso Not mReadonlyClass Then
                    mAddnlRefSepChar = Value
                End If
            End Set
        End Property

        Public Property AddnlRefAccessionSepChar As Char
            Get
                Return mAddnlRefAccessionSepChar
            End Get
            Set
                If Not Value = Nothing AndAlso Not mReadonlyClass Then
                    mAddnlRefAccessionSepChar = Value
                End If
            End Set
        End Property
#End Region

    End Class

    Private Sub InSilicoDigest_ErrorEvent(message As String) Handles mInSilicoDigest.ErrorEvent
        ShowErrorMessage("Error in mInSilicoDigest: " & message)
    End Sub

    Private Sub InSilicoDigest_ProgressChanged(taskDescription As String, percentComplete As Single) Handles mInSilicoDigest.ProgressChanged
        UpdateSubtaskProgress(taskDescription, percentComplete)
    End Sub

    Private Sub InSilicoDigest_ProgressComplete() Handles mInSilicoDigest.ProgressComplete
        UpdateSubtaskProgress(100)
    End Sub

    Private Sub InSilicoDigest_ProgressReset() Handles mInSilicoDigest.ProgressReset
        ' Don't do anything with this event
    End Sub

    Private Sub NETCalculator_ErrorEvent(message As String) Handles mNETCalculator.ErrorEvent
        ShowErrorMessage(message)
    End Sub

    Private Sub SCXNETCalculator_ErrorEvent(message As String) Handles mSCXNETCalculator.ErrorEvent
        ShowErrorMessage(message)
    End Sub

End Class
