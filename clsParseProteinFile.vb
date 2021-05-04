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
Imports PRISM.FileProcessor
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
    Inherits ProcessFilesBase

    ' Ignore Spelling: ComputepI, Cys, gi, hydrophobicity, Ile, Leu, pre, SepChar, silico, varchar

    Public Sub New()
        mFileDate = "May 4, 2021"
        InitializeLocalVariables()
    End Sub

    Public Const XML_SECTION_OPTIONS As String = "ProteinDigestionSimulatorOptions"
    Public Const XML_SECTION_FASTA_OPTIONS As String = "FastaInputOptions"
    Public Const XML_SECTION_PROCESSING_OPTIONS As String = "ProcessingOptions"
    Public Const XML_SECTION_DIGESTION_OPTIONS As String = "DigestionOptions"
    Public Const XML_SECTION_UNIQUENESS_STATS_OPTIONS As String = "UniquenessStatsOptions"

    Private Const PROTEIN_CACHE_MEMORY_RESERVE_COUNT As Integer = 500

    Private Const SCRAMBLING_CACHE_LENGTH As Integer = 4000
    Private Const PROTEIN_PREFIX_SCRAMBLED As String = "Random_"
    Private Const PROTEIN_PREFIX_REVERSED As String = "XXX."

    Private Const MAXIMUM_PROTEIN_NAME_LENGTH As Integer = 512

    Private Const MAX_ABBREVIATED_FILENAME_LENGTH As Integer = 15

    ' The value of 7995 is chosen because the maximum varchar() value in SQL Server is varchar(8000)
    ' and we want to prevent truncation errors when importing protein names and descriptions into SQL Server
    Private Const MAX_PROTEIN_DESCRIPTION_LENGTH As Integer = FastaValidator.MAX_PROTEIN_DESCRIPTION_LENGTH

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

    ''' <summary>
    ''' Column delimiter to use when processing TSV or CSV files
    ''' </summary>
    ''' <remarks>The GUI uses this enum</remarks>
    Public Enum DelimiterCharConstants
        Space = 0
        Tab = 1
        Comma = 2
        ' ReSharper disable once UnusedMember.Global
        Other = 3
    End Enum

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

    Private mInputFileDelimiter As Char                              ' Only used for delimited protein input files, not for fasta files

    Private mInputFileProteinsProcessed As Integer
    Private mInputFileLinesRead As Integer
    Private mInputFileLineSkipCount As Integer

    Private mOutputFileDelimiter As Char

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

    Public Property DelimitedFileFormatCode As DelimitedProteinFileReader.ProteinFileFormatCode

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

    Public Property ProteinScramblingMode As ProteinScramblingModeConstants

    Public Property ProteinScramblingSamplingPercentage As Integer

    Public Property SequenceWidthToExamineForMaximumpI As Integer
        Get
            Return mSequenceWidthToExamineForMaximumpI
        End Get
        Set
            If Value < 1 Then mSequenceWidthToExamineForMaximumpI = 1
            mSequenceWidthToExamineForMaximumpI = Value
        End Set
    End Property

    Private Function ComputeSequenceHydrophobicity(peptideSequence As String) As Single

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mpICalculator will be nothing
        If mpICalculator Is Nothing Then
            Return 0
        Else
            Return mpICalculator.CalculateSequenceHydrophobicity(peptideSequence)
        End If

    End Function

    Private Function ComputeSequencepI(peptideSequence As String) As Single

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mpICalculator will be nothing
        If mpICalculator Is Nothing Then
            Return 0
        Else
            Return mpICalculator.CalculateSequencepI(peptideSequence)
        End If

    End Function

    Private Function ComputeSequenceMass(peptideSequence As String) As Double

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mInSilicoDigest will be nothing
        If mInSilicoDigest Is Nothing Then
            Return 0
        Else
            Return mInSilicoDigest.ComputeSequenceMass(peptideSequence, IncludeXResiduesInMass)
        End If

    End Function

    Private Function ComputeSequenceNET(peptideSequence As String) As Single

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mNETCalculator will be nothing
        If mNETCalculator Is Nothing Then
            Return 0
        Else
            Return mNETCalculator.GetElutionTime(peptideSequence)
        End If

    End Function

    Private Function ComputeSequenceSCXNET(peptideSequence As String) As Single

        ' Be sure to call InitializeObjectVariables before calling this function for the first time
        ' Otherwise, mSCXNETCalculator will be nothing
        If mSCXNETCalculator Is Nothing Then
            Return 0
        Else
            Return mSCXNETCalculator.GetElutionTime(peptideSequence)
        End If

    End Function

    ''' <summary>
    ''' Digest the protein sequence using the given cleavage options
    ''' </summary>
    ''' <param name="peptideSequence"></param>
    ''' <param name="peptideFragments"></param>
    ''' <param name="options"></param>
    ''' <param name="proteinName"></param>
    ''' <returns>The number of digested peptides in peptideFragments</returns>
    Public Function DigestProteinSequence(
        peptideSequence As String,
        <Out> ByRef peptideFragments As List(Of clsInSilicoDigest.PeptideInfoClass),
        options As clsInSilicoDigest.DigestionOptionsClass,
        Optional proteinName As String = "") As Integer

        ' Make sure the object variables are initialized
        If Not InitializeObjectVariables() Then
            peptideFragments = New List(Of clsInSilicoDigest.PeptideInfoClass)()
            Return 0
        End If

        Try
            Dim peptideCount = mInSilicoDigest.DigestSequence(peptideSequence, peptideFragments, options, ComputepI, proteinName)
            Return peptideCount
        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.DigestProteinSequenceError)
            peptideFragments = New List(Of clsInSilicoDigest.PeptideInfoClass)()
            Return 0
        End Try

    End Function

    Private Function ExtractAlternateProteinNamesFromDescription(ByRef description As String, ByRef udtAlternateNames() As udtAddnlRefType) As Integer
        ' Searches in description for additional protein Ref names
        ' description is passed ByRef since the additional protein references will be removed from it

        Dim alternateNameCount = 0
        ReDim udtAlternateNames(0)

        Try
            Dim proteinNames = description.Split(FastaFileOptions.AddnlRefSepChar)

            If proteinNames.Length > 0 Then
                ReDim udtAlternateNames(proteinNames.Length - 1)

                For index = 0 To proteinNames.Length - 1
                    Dim charIndex = proteinNames(index).IndexOf(FastaFileOptions.AddnlRefAccessionSepChar)

                    If charIndex > 0 Then
                        If index = proteinNames.Length - 1 Then
                            ' Need to find the next space after charIndex and truncate proteinNames() at that location
                            Dim spaceIndex = proteinNames(index).IndexOf(" "c, charIndex)
                            If spaceIndex >= 0 Then
                                proteinNames(index) = proteinNames(index).Substring(0, spaceIndex)
                            End If
                        End If

                        If charIndex >= proteinNames(index).Length - 1 Then
                            ' No accession after the colon; invalid entry so discard this entry and stop parsing
                            ReDim Preserve udtAlternateNames(index - 1)
                            Exit For
                        End If

                        udtAlternateNames(index).RefName = proteinNames(index).Substring(0, charIndex)
                        udtAlternateNames(index).RefAccession = proteinNames(index).Substring(charIndex + 1)
                        alternateNameCount += 1

                        charIndex = description.IndexOf(proteinNames(index), StringComparison.Ordinal)
                        If charIndex >= 0 Then
                            If charIndex + proteinNames(index).Length + 1 < description.Length Then
                                description = description.Substring(charIndex + proteinNames(index).Length + 1)
                            Else
                                description = String.Empty
                            End If
                        Else
                            ShowErrorMessage("This code in ExtractAlternateProteinNamesFromDescription should never be reached")
                        End If
                    Else
                        Exit For
                    End If
                Next index

            End If

        Catch ex As Exception
            ShowErrorMessage("Error parsing out additional Ref Names from the Protein Description: " & ex.Message)
        End Try

        Return alternateNameCount

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
        Dim extensionsToParse = New List(Of String) From {
                ".fasta",
                ".fasta.gz",
                ".txt"
                }

        Return extensionsToParse

    End Function

    Public Overrides Function GetErrorMessage() As String
        ' Returns "" if no error

        Dim errorMessage As String

        If ErrorCode = ProcessFilesErrorCodes.LocalizedError Or
           ErrorCode = ProcessFilesErrorCodes.NoError Then
            Select Case mLocalErrorCode
                Case eParseProteinFileErrorCodes.NoError
                    errorMessage = ""
                Case eParseProteinFileErrorCodes.ProteinFileParsingOptionsSectionNotFound
                    errorMessage = "The section " & XML_SECTION_OPTIONS & " was not found in the parameter file"

                Case eParseProteinFileErrorCodes.ErrorReadingInputFile
                    errorMessage = "Error reading input file"
                Case eParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile
                    errorMessage = "Error creating parsed proteins output file"
                Case eParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile
                    errorMessage = "Error creating digested proteins output file"
                Case eParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile
                    errorMessage = "Error creating scrambled proteins output file"

                Case eParseProteinFileErrorCodes.ErrorWritingOutputFile
                    errorMessage = "Error writing to one of the output files"

                Case eParseProteinFileErrorCodes.ErrorInitializingObjectVariables
                    errorMessage = "Error initializing In Silico Digester class"

                Case eParseProteinFileErrorCodes.DigestProteinSequenceError
                    errorMessage = "Error in DigestProteinSequence function"
                Case eParseProteinFileErrorCodes.UnspecifiedError
                    errorMessage = "Unspecified localized error"
                Case Else
                    ' This shouldn't happen
                    errorMessage = "Unknown error state"
            End Select
        Else
            errorMessage = GetBaseClassErrorMessage()
        End If

        Return errorMessage

    End Function

    Public Function GetProteinCountCached() As Integer
        Return mProteinCount
    End Function

    Public Function GetCachedProtein(index As Integer) As udtProteinInfoType
        If index < mProteinCount Then
            Return mProteins(index)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    '''
    ''' </summary>
    ''' <param name="index"></param>
    ''' <param name="digestedPeptides"></param>
    ''' <param name="options"></param>
    ''' <returns>The number of peptides in digestedPeptides</returns>
    Public Function GetDigestedPeptidesForCachedProtein(
        index As Integer,
        <Out> ByRef digestedPeptides As List(Of clsInSilicoDigest.PeptideInfoClass),
        options As clsInSilicoDigest.DigestionOptionsClass) As Integer

        If index < mProteinCount Then
            Return DigestProteinSequence(mProteins(index).Sequence, digestedPeptides, options, mProteins(index).Name)
        Else
            digestedPeptides = New List(Of clsInSilicoDigest.PeptideInfoClass)()
            Return 0
        End If

    End Function

    Private Sub InitializeLocalVariables()
        mLocalErrorCode = eParseProteinFileErrorCodes.NoError
        ShowDebugPrompts = False

        AssumeDelimitedFile = False
        AssumeFastaFile = False
        mInputFileDelimiter = ControlChars.Tab
        DelimitedFileFormatCode = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence

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
        ProteinScramblingMode = ProteinScramblingModeConstants.None
        ProteinScramblingSamplingPercentage = 100
        ProteinScramblingLoopCount = 1

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

        HydrophobicityType = clspICalculation.eHydrophobicityTypeConstants.HW
        ReportMaximumpI = False
        mSequenceWidthToExamineForMaximumpI = 10

    End Sub

    ''' <summary>
    ''' Examines the file's extension and returns true if it ends in .fasta or .fasta.gz
    ''' </summary>
    ''' <param name="filePath"></param>
    ''' <returns></returns>
    Public Shared Function IsFastaFile(filePath As String) As Boolean

        If String.IsNullOrWhiteSpace(filePath) Then
            Return False
        End If

        If Path.GetExtension(StripExtension(filePath, ".gz")).Equals(".fasta", StringComparison.OrdinalIgnoreCase) Then
            Return True
        Else
            Return False
        End If

    End Function

    Private Function InitializeObjectVariables() As Boolean

        Dim errorMessage As String = String.Empty

        If Not mObjectVariablesLoaded Then
            ' Need to initialize the object variables

            Try
                mInSilicoDigest = New clsInSilicoDigest
            Catch ex As Exception
                errorMessage = "Error initializing InSilicoDigest class"
                SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
            End Try

            Try
                mpICalculator = New clspICalculation
            Catch ex As Exception
                errorMessage = "Error initializing pI Calculation class"
                ShowErrorMessage(errorMessage)
                SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
            End Try

            Try
                mNETCalculator = New ElutionTimePredictionKangas()
            Catch ex As Exception
                errorMessage = "Error initializing LC NET Calculation class"
                ShowErrorMessage(errorMessage)
                SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
            End Try

            Try
                mSCXNETCalculator = New SCXElutionTimePredictionKangas()
            Catch ex As Exception
                errorMessage = "Error initializing SCX NET Calculation class"
                ShowErrorMessage(errorMessage)
                SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
            End Try

            If errorMessage.Length > 0 Then
                ShowErrorMessage(errorMessage)
                mObjectVariablesLoaded = False
            Else
                mObjectVariablesLoaded = True
            End If
        End If

        If mInSilicoDigest IsNot Nothing Then
            If mpICalculator IsNot Nothing Then
                mInSilicoDigest.InitializepICalculator(mpICalculator)
            End If
        End If

        Return mObjectVariablesLoaded

    End Function

    Private Function InitializeScrambledOutput(
      pathInfo As udtFilePathInfoType,
      udtResidueCache As udtScramblingResidueCacheType,
      eScramblingMode As ProteinScramblingModeConstants,
      <Out> ByRef scrambledFileWriter As StreamWriter,
      <Out> ByRef randomNumberGenerator As Random) As Boolean


        Dim success As Boolean
        Dim randomNumberSeed As Integer
        Dim suffix As String
        Dim scrambledFastaOutputFilePath As String


        ' Wait to allow the timer to advance.
        Thread.Sleep(1)
        randomNumberSeed = Environment.TickCount

        randomNumberGenerator = New Random(randomNumberSeed)

        If eScramblingMode = ProteinScramblingModeConstants.Reversed Then
            ' Reversed fasta file
            suffix = "_reversed"
            If udtResidueCache.SamplingPercentage < 100 Then
                suffix &= "_" & udtResidueCache.SamplingPercentage.ToString() & "pct"
            End If
        Else
            ' Scrambled fasta file
            suffix = "_scrambled_seed" & randomNumberSeed.ToString()
            If udtResidueCache.SamplingPercentage < 100 Then
                suffix &= "_" & udtResidueCache.SamplingPercentage.ToString() & "pct"
            End If
        End If
        scrambledFastaOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) & suffix & ".fasta")

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
            scrambledFileWriter = New StreamWriter(scrambledFastaOutputFilePath)
            success = True
        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile)
            success = False
            scrambledFileWriter = Nothing
        End Try

        Return success

    End Function

    Public Function LoadParameterFileSettings(parameterFilePath As String) As Boolean

        Dim settingsFile As New XmlSettingsFileAccessor()

        Dim cysPeptidesOnly As Boolean

        Try

            If parameterFilePath Is Nothing OrElse parameterFilePath.Length = 0 Then
                ' No parameter file specified; nothing to load
                Return True
            End If

            If Not File.Exists(parameterFilePath) Then
                ' See if parameterFilePath points to a file in the same directory as the application
                parameterFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(parameterFilePath))
                If Not File.Exists(parameterFilePath) Then
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.ParameterFileNotFound)
                    Return False
                End If
            End If

            ' Pass False to .LoadSettings() here to turn off case sensitive matching
            If settingsFile.LoadSettings(parameterFilePath, False) Then
                If Not settingsFile.SectionPresent(XML_SECTION_OPTIONS) Then
                    ShowErrorMessage("The node '<section name=""" & XML_SECTION_OPTIONS & """> was not found in the parameter file: " & parameterFilePath)
                    SetLocalErrorCode(eParseProteinFileErrorCodes.ProteinFileParsingOptionsSectionNotFound)
                    Return False
                Else
                    ' ComparisonFastaFile = settingsFile.GetParam(XML_SECTION_OPTIONS, "ComparisonFastaFile", ComparisonFastaFile)

                    Dim inputFileColumnDelimiterIndex = settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnDelimiterIndex", DelimiterCharConstants.Tab)
                    Dim customInputFileColumnDelimiter = settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnDelimiter", ControlChars.Tab)

                    InputFileDelimiter = LookupColumnDelimiterChar(inputFileColumnDelimiterIndex, customInputFileColumnDelimiter, InputFileDelimiter)

                    DelimitedFileFormatCode = CType(settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnOrdering", DelimitedFileFormatCode), DelimitedProteinFileReader.ProteinFileFormatCode)

                    Dim outputFileFieldDelimiterIndex = settingsFile.GetParam(XML_SECTION_OPTIONS, "OutputFileFieldDelimiterIndex", DelimiterCharConstants.Tab)
                    Dim outputFileFieldDelimiter = settingsFile.GetParam(XML_SECTION_OPTIONS, "OutputFileFieldDelimiter", ControlChars.Tab)

                    OutputFileDelimiter = LookupColumnDelimiterChar(outputFileFieldDelimiterIndex, outputFileFieldDelimiter, OutputFileDelimiter)

                    DigestionOptions.IncludePrefixAndSuffixResidues = settingsFile.GetParam(XML_SECTION_OPTIONS, "IncludePrefixAndSuffixResidues", DigestionOptions.IncludePrefixAndSuffixResidues)

                    FastaFileOptions.LookForAddnlRefInDescription = settingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "LookForAddnlRefInDescription", FastaFileOptions.LookForAddnlRefInDescription)

                    FastaFileOptions.AddnlRefSepChar = settingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "AddnlRefSepChar", FastaFileOptions.AddnlRefSepChar.ToString()).Chars(0)
                    FastaFileOptions.AddnlRefAccessionSepChar = settingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "AddnlRefAccessionSepChar", FastaFileOptions.AddnlRefAccessionSepChar.ToString()).Chars(0)

                    ExcludeProteinSequence = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ExcludeProteinSequence", ExcludeProteinSequence)
                    ComputeProteinMass = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeProteinMass", ComputeProteinMass)
                    IncludeXResiduesInMass = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "IncludeXResidues", IncludeXResiduesInMass)
                    ElementMassMode = CType(settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ElementMassMode", ElementMassMode), PeptideSequenceClass.ElementModeConstants)

                    ComputepI = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputepI", ComputepI)
                    ComputeNET = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeNET", ComputeNET)
                    ComputeSCXNET = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeSCX", ComputeSCXNET)

                    CreateDigestedProteinOutputFile = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "DigestProteins", CreateDigestedProteinOutputFile)
                    ProteinScramblingMode = CType(settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ProteinReversalIndex", ProteinScramblingMode), ProteinScramblingModeConstants)
                    ProteinScramblingLoopCount = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ProteinScramblingLoopCount", ProteinScramblingLoopCount)

                    DigestionOptions.CleavageRuleID = CType(settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "CleavageRuleTypeIndex", DigestionOptions.CleavageRuleID), clsInSilicoDigest.CleavageRuleConstants)
                    DigestionOptions.RemoveDuplicateSequences = Not settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "IncludeDuplicateSequences", Not DigestionOptions.RemoveDuplicateSequences)

                    cysPeptidesOnly = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "CysPeptidesOnly", False)
                    If cysPeptidesOnly Then
                        DigestionOptions.AminoAcidResidueFilterChars = New Char() {"C"c}
                    Else
                        DigestionOptions.AminoAcidResidueFilterChars = New Char() {}
                    End If

                    DigestionOptions.MinFragmentMass = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMinimumMass", DigestionOptions.MinFragmentMass)
                    DigestionOptions.MaxFragmentMass = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMaximumMass", DigestionOptions.MaxFragmentMass)
                    DigestionOptions.MinFragmentResidueCount = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMinimumResidueCount", DigestionOptions.MinFragmentResidueCount)
                    DigestionOptions.MaxMissedCleavages = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMaximumMissedCleavages", DigestionOptions.MaxMissedCleavages)

                End If
            End If

        Catch ex As Exception
            ShowErrorMessage("Error in LoadParameterFileSettings: " & ex.Message)
            Return False
        End Try

        Return True

    End Function

    Public Shared Function LookupColumnDelimiterChar(delimiterIndex As Integer, customDelimiter As String, defaultDelimiter As Char) As Char

        Dim delimiter As String

        Select Case delimiterIndex
            Case DelimiterCharConstants.Space
                delimiter = " "
            Case DelimiterCharConstants.Tab
                delimiter = ControlChars.Tab
            Case DelimiterCharConstants.Comma
                delimiter = ","
            Case Else
                ' Includes DelimiterCharConstants.Other
                delimiter = String.Copy(customDelimiter)
        End Select

        If delimiter Is Nothing OrElse delimiter.Length = 0 Then
            delimiter = String.Copy(defaultDelimiter)
        End If

        Try
            Return delimiter.Chars(0)
        Catch ex As Exception
            Return ControlChars.Tab
        End Try

    End Function

    Public Function ParseProteinFile(proteinInputFilePath As String, outputFolderPath As String) As Boolean
        Return ParseProteinFile(proteinInputFilePath, outputFolderPath, String.Empty)
    End Function

    ''' <summary>
    ''' Process the protein FASTA file or tab-delimited text file
    ''' </summary>
    ''' <param name="proteinInputFilePath"></param>
    ''' <param name="outputFolderPath"></param>
    ''' <param name="outputFileNameBaseOverride">Name for the protein output filename (auto-defined if empty)</param>
    ''' <returns></returns>
    Public Function ParseProteinFile(
      proteinInputFilePath As String,
      outputFolderPath As String,
      outputFileNameBaseOverride As String) As Boolean

        Dim proteinFileReader As ProteinFileReaderBaseClass = Nothing

        Dim proteinFileWriter As StreamWriter = Nothing
        Dim digestFileWriter As StreamWriter = Nothing
        Dim scrambledFileWriter As StreamWriter = Nothing

        Dim success As Boolean
        Dim inputProteinFound As Boolean
        Dim headerChecked As Boolean

        Dim allowLookForAddnlRefInDescription As Boolean
        Dim lookForAddnlRefInDescription As Boolean
        Dim addnlRefMasterNames As SortedSet(Of String)

        Dim addnlRefsToOutput() As udtAddnlRefType = Nothing

        Dim generateUniqueSequenceID As Boolean

        Dim randomNumberGenerator As Random = Nothing

        Dim eScramblingMode As ProteinScramblingModeConstants
        Dim udtResidueCache = New udtScramblingResidueCacheType()

        Dim startTime As Date

        ProcessingSummary = String.Empty

        Dim pathInfo As udtFilePathInfoType
        pathInfo.ProteinInputFilePath = proteinInputFilePath
        pathInfo.OutputFolderPath = outputFolderPath
        pathInfo.OutputFileNameBaseOverride = outputFileNameBaseOverride
        pathInfo.ProteinOutputFilePath = String.Empty
        pathInfo.DigestedProteinOutputFilePath = String.Empty

        Try

            If String.IsNullOrWhiteSpace(pathInfo.ProteinInputFilePath) Then
                SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath)
                Return False
            End If

            ' Make sure the object variables are initialized
            If Not InitializeObjectVariables() Then
                success = False
                Exit Try
            End If

            success = ParseProteinFileCreateOutputFile(pathInfo, proteinFileReader)

        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
            success = False
        End Try

        ' Abort processing if we couldn't successfully open the input file
        If Not success Then Return False

        Try
            ' Set the options for mpICalculator
            ' Note that this will also update the pICalculator object in mInSilicoDigest
            If mpICalculator IsNot Nothing Then
                mpICalculator.HydrophobicityType = HydrophobicityType
                mpICalculator.ReportMaximumpI = ReportMaximumpI
                mpICalculator.SequenceWidthToExamineForMaximumpI = mSequenceWidthToExamineForMaximumpI
            End If

            If CreateProteinOutputFile Then
                Try
                    ' Open the protein output file (if required)
                    ' This will cause an error if the input file is the same as the output file
                    proteinFileWriter = New StreamWriter(pathInfo.ProteinOutputFilePath)
                    success = True
                Catch ex As Exception
                    SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile)
                    success = False
                End Try
                If Not success Then Exit Try
            End If

            If mMasterSequencesHashTable Is Nothing Then
                mMasterSequencesHashTable = New Hashtable
            Else
                mMasterSequencesHashTable.Clear()
            End If
            mNextUniqueIDForMasterSeqs = 1

            If CreateProteinOutputFile AndAlso CreateDigestedProteinOutputFile AndAlso Not CreateFastaOutputFile Then
                success = ParseProteinFileCreateDigestedProteinOutputFile(pathInfo.DigestedProteinOutputFilePath, digestFileWriter)
                If Not success Then Exit Try

                If GenerateUniqueIDValuesForPeptides Then
                    ' Initialize mMasterSequencesHashTable
                    generateUniqueSequenceID = True
                End If

            End If

            Dim loopCount = 1
            If ProteinScramblingMode = ProteinScramblingModeConstants.Randomized Then
                loopCount = ProteinScramblingLoopCount
                If loopCount < 1 Then loopCount = 1
                If loopCount > 10000 Then loopCount = 10000

                allowLookForAddnlRefInDescription = False
            Else
                allowLookForAddnlRefInDescription = FastaFileOptions.LookForAddnlRefInDescription
            End If

            Dim outLine = New StringBuilder()

            For loopIndex = 1 To loopCount

                ' Attempt to open the input file
                If Not proteinFileReader.OpenFile(pathInfo.ProteinInputFilePath) Then
                    SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
                    success = False
                    Exit Try
                End If

                If CreateProteinOutputFile Then
                    eScramblingMode = ProteinScramblingMode
                    udtResidueCache.SamplingPercentage = ProteinScramblingSamplingPercentage
                    If udtResidueCache.SamplingPercentage <= 0 Then udtResidueCache.SamplingPercentage = 100
                    If udtResidueCache.SamplingPercentage > 100 Then udtResidueCache.SamplingPercentage = 100

                    udtResidueCache.Cache = String.Empty
                    udtResidueCache.CacheLength = SCRAMBLING_CACHE_LENGTH
                    udtResidueCache.OutputCount = 0
                    udtResidueCache.ResiduesToWrite = String.Empty

                    If eScramblingMode <> ProteinScramblingModeConstants.None Then

                        success = InitializeScrambledOutput(pathInfo, udtResidueCache, eScramblingMode, scrambledFileWriter, randomNumberGenerator)
                        If Not success Then Exit Try

                    End If
                Else
                    eScramblingMode = ProteinScramblingModeConstants.None
                End If

                startTime = DateTime.UtcNow

                If CreateProteinOutputFile AndAlso mParsedFileIsFastaFile AndAlso allowLookForAddnlRefInDescription Then
                    ' Need to pre-scan the fasta file to find all of the possible additional reference values

                    addnlRefMasterNames = New SortedSet(Of String)(StringComparer.CurrentCultureIgnoreCase)
                    PreScanProteinFileForAddnlRefsInDescription(pathInfo.ProteinInputFilePath, addnlRefMasterNames)

                    If addnlRefMasterNames.Count > 0 Then
                        ' Need to extract out the key names from addnlRefMasterNames and sort them alphabetically
                        ReDim addnlRefsToOutput(addnlRefMasterNames.Count - 1)
                        lookForAddnlRefInDescription = True

                        Dim index = 0
                        For Each addnlRef In addnlRefMasterNames
                            addnlRefsToOutput(index).RefName = String.Copy(addnlRef)
                            index += 1
                        Next

                        Dim iAddnlRefComparerClass As New AddnlRefComparerClass
                        Array.Sort(addnlRefsToOutput, iAddnlRefComparerClass)
                    Else
                        ReDim addnlRefsToOutput(0)
                        lookForAddnlRefInDescription = False
                    End If
                End If

                ResetProgress("Parsing protein input file")

                If CreateProteinOutputFile AndAlso Not CreateFastaOutputFile Then
                    outLine.Clear()

                    ' Write the header line to the output file
                    outLine.Append("ProteinName" & mOutputFileDelimiter)

                    If lookForAddnlRefInDescription Then

                        For index = 0 To addnlRefsToOutput.Length - 1
                            outLine.Append(addnlRefsToOutput(index).RefName & mOutputFileDelimiter)
                        Next index
                    End If

                    ' Include Description in the header line, even if we are excluding the description for all proteins
                    outLine.Append("Description")

                    If ComputeSequenceHashValues Then
                        outLine.Append(mOutputFileDelimiter & "SequenceHash")
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

                    proteinFileWriter.WriteLine(outLine.ToString())
                End If

                ' Read each protein in the input file and process appropriately
                mProteinCount = 0
                ReDim mProteins(PROTEIN_CACHE_MEMORY_RESERVE_COUNT)

                mInputFileProteinsProcessed = 0
                mInputFileLineSkipCount = 0
                mInputFileLinesRead = 0
                Do
                    inputProteinFound = proteinFileReader.ReadNextProteinEntry()
                    mInputFileLineSkipCount += proteinFileReader.LineSkipCount

                    If Not inputProteinFound Then Continue Do

                    If Not headerChecked Then
                        If Not mParsedFileIsFastaFile Then
                            headerChecked = True

                            ' This may be a header line; possibly skip it
                            If proteinFileReader.ProteinName.ToLower().StartsWith("protein") Then
                                If proteinFileReader.ProteinDescription.ToLower().Contains("description") AndAlso
                                    proteinFileReader.ProteinSequence.ToLower().Contains("sequence") Then
                                    ' Skip this entry since it's a header line, for example:
                                    ' ProteinName    Description    Sequence
                                    Continue Do
                                End If
                            End If

                            If proteinFileReader.ProteinName.ToLower().Contains("protein") AndAlso
                               FractionLowercase(proteinFileReader.ProteinSequence) > 0.2 Then
                                ' Skip this entry since it's a header line, for example:
                                ' FirstProtein    ProteinDesc    Sequence
                                Continue Do
                            End If

                        End If
                    End If

                    mInputFileProteinsProcessed += 1
                    mInputFileLinesRead = proteinFileReader.LinesRead

                    ParseProteinFileStoreProtein(proteinFileReader, lookForAddnlRefInDescription)

                    If CreateProteinOutputFile Then
                        If loopIndex = 1 Then

                            If CreateFastaOutputFile Then
                                ParseProteinFileWriteFasta(proteinFileWriter, outLine)
                            Else
                                ParseProteinFileWriteTextDelimited(proteinFileWriter, outLine, lookForAddnlRefInDescription, addnlRefsToOutput)
                            End If

                        End If

                        If loopIndex = 1 AndAlso digestFileWriter IsNot Nothing Then
                            ParseProteinFileWriteDigested(digestFileWriter, outLine, generateUniqueSequenceID)
                        End If

                        If eScramblingMode <> ProteinScramblingModeConstants.None Then
                            WriteScrambledFasta(scrambledFileWriter, randomNumberGenerator, mProteins(mProteinCount), eScramblingMode, udtResidueCache)
                        End If

                    Else
                        ' Cache the proteins in memory
                        mProteinCount += 1
                        If mProteinCount >= mProteins.Length Then
                            ReDim Preserve mProteins(mProteins.Length + PROTEIN_CACHE_MEMORY_RESERVE_COUNT)
                        End If
                    End If

                    Dim percentProcessed = (loopIndex - 1) / CSng(loopCount) * 100.0! + proteinFileReader.PercentFileProcessed() / loopCount
                    UpdateProgress(percentProcessed)

                    If AbortProcessing Then Exit Do

                Loop While inputProteinFound

                If CreateProteinOutputFile And eScramblingMode <> ProteinScramblingModeConstants.None Then
                    ' Write out anything remaining in the cache

                    Dim proteinNamePrefix As String
                    If eScramblingMode = ProteinScramblingModeConstants.Reversed Then
                        proteinNamePrefix = PROTEIN_PREFIX_REVERSED
                    Else
                        proteinNamePrefix = PROTEIN_PREFIX_SCRAMBLED
                    End If

                    WriteFastaAppendToCache(scrambledFileWriter, udtResidueCache, proteinNamePrefix, True)
                End If

                If mProteinCount > 0 Then
                    ReDim Preserve mProteins(mProteinCount - 1)
                Else
                    ReDim mProteins(0)
                End If

                proteinFileReader.CloseFile()

                If proteinFileWriter IsNot Nothing Then
                    proteinFileWriter.Close()
                End If

                If digestFileWriter IsNot Nothing Then
                    digestFileWriter.Close()
                End If

                If scrambledFileWriter IsNot Nothing Then
                    scrambledFileWriter.Close()
                End If
            Next loopIndex

            If ShowDebugPrompts Then
                Dim statusMessage = String.Format("{0}{1}Elapsed time: {2:F2} seconds",
                                                  Path.GetFileName(pathInfo.ProteinInputFilePath), ControlChars.NewLine,
                                                  DateTime.UtcNow.Subtract(startTime).TotalSeconds)

                MessageBox.Show(statusMessage, "Status", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            Dim message = "Done: Processed " & mInputFileProteinsProcessed.ToString("###,##0") & " proteins (" & mInputFileLinesRead.ToString("###,###,##0") & " lines)"
            If mInputFileLineSkipCount > 0 Then
                message &= ControlChars.NewLine & "Note that " & mInputFileLineSkipCount.ToString("###,##0") & " lines were skipped in the input file due to having an unexpected format. "
                If mParsedFileIsFastaFile Then
                    message &= "This is an unexpected error for fasta files."
                Else
                    message &= "Make sure that " & DelimitedFileFormatCode.ToString() & " is the appropriate format for this file (see the File Format Options tab)."
                End If
            End If

            If loopCount > 1 Then
                message &= ControlChars.NewLine & "Created " & loopCount.ToString() & " replicates of the scrambled output file"
            End If

            ProcessingSummary = message
            OnStatusEvent(message)

            success = True

        Catch ex As Exception
            ShowErrorMessage("Error in ParseProteinFile: " & ex.Message)
            If CreateProteinOutputFile Or CreateDigestedProteinOutputFile Then
                SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorWritingOutputFile)
                success = False
            Else
                SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
                success = False
            End If
        Finally
            If proteinFileWriter IsNot Nothing Then
                proteinFileWriter.Close()
            End If
            If digestFileWriter IsNot Nothing Then
                digestFileWriter.Close()
            End If
            If scrambledFileWriter IsNot Nothing Then
                scrambledFileWriter.Close()
            End If
        End Try

        Return success

    End Function

    Private Sub ParseProteinFileStoreProtein(
      proteinFileReader As ProteinFileReaderBaseClass,
      lookForAddnlRefInDescription As Boolean)


        mProteins(mProteinCount).Name = proteinFileReader.ProteinName
        mProteins(mProteinCount).Description = proteinFileReader.ProteinDescription

        If TruncateProteinDescription AndAlso mProteins(mProteinCount).Description.Length > MAX_PROTEIN_DESCRIPTION_LENGTH Then
            mProteins(mProteinCount).Description = mProteins(mProteinCount).Description.Substring(0, MAX_PROTEIN_DESCRIPTION_LENGTH - 3) & "..."
        End If

        If lookForAddnlRefInDescription Then
            ' Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
            mProteins(mProteinCount).AlternateNameCount = ExtractAlternateProteinNamesFromDescription(
                mProteins(mProteinCount).Description,
                mProteins(mProteinCount).AlternateNames)
        Else
            mProteins(mProteinCount).AlternateNameCount = 0
            ReDim mProteins(mProteinCount).AlternateNames(0)
        End If

        Dim sequence = proteinFileReader.ProteinSequence
        mProteins(mProteinCount).Sequence = sequence

        If ComputeSequenceHashIgnoreILDiff Then
            mProteins(mProteinCount).SequenceHash = HashUtilities.ComputeStringHashSha1(sequence.Replace("L"c, "I"c)).ToUpper()
        Else
            mProteins(mProteinCount).SequenceHash = HashUtilities.ComputeStringHashSha1(sequence).ToUpper()
        End If

        If ComputeProteinMass Then
            mProteins(mProteinCount).Mass = ComputeSequenceMass(sequence)
        Else
            mProteins(mProteinCount).Mass = 0
        End If

        If ComputepI Then
            mProteins(mProteinCount).pI = ComputeSequencepI(sequence)
            mProteins(mProteinCount).Hydrophobicity = ComputeSequenceHydrophobicity(sequence)
        Else
            mProteins(mProteinCount).pI = 0
            mProteins(mProteinCount).Hydrophobicity = 0
        End If

        If ComputeNET Then
            mProteins(mProteinCount).ProteinNET = ComputeSequenceNET(sequence)
        End If

        If ComputeSCXNET Then
            mProteins(mProteinCount).ProteinSCXNET = ComputeSequenceSCXNET(sequence)
        End If

    End Sub

    Private Sub ParseProteinFileWriteDigested(
      digestFileWriter As TextWriter,
      outLine As StringBuilder,
      generateUniqueSequenceID As Boolean)

        Dim peptideFragments As List(Of clsInSilicoDigest.PeptideInfoClass) = Nothing
        DigestProteinSequence(mProteins(mProteinCount).Sequence, peptideFragments, DigestionOptions, mProteins(mProteinCount).Name)

        For Each peptideFragment In peptideFragments

            Dim uniqueSeqID As Integer

            If generateUniqueSequenceID Then
                Try
                    If mMasterSequencesHashTable.ContainsKey(peptideFragment.SequenceOneLetter) Then
                        uniqueSeqID = CInt(mMasterSequencesHashTable(peptideFragment.SequenceOneLetter))
                    Else
                        mMasterSequencesHashTable.Add(peptideFragment.SequenceOneLetter, mNextUniqueIDForMasterSeqs)
                        uniqueSeqID = mNextUniqueIDForMasterSeqs
                    End If
                    mNextUniqueIDForMasterSeqs += 1
                Catch ex As Exception
                    uniqueSeqID = 0
                End Try
            Else
                uniqueSeqID = 0
            End If

            Dim baseSequence = peptideFragment.SequenceOneLetter
            outLine.Clear()

            If Not ExcludeProteinSequence Then
                outLine.Append(mProteins(mProteinCount).Name & mOutputFileDelimiter)
                If DigestionOptions.IncludePrefixAndSuffixResidues Then
                    outLine.Append(peptideFragment.PrefixResidue & "." & baseSequence & "." & peptideFragment.SuffixResidue & mOutputFileDelimiter)
                Else
                    outLine.Append(baseSequence & mOutputFileDelimiter)
                End If
            End If

            outLine.Append(uniqueSeqID.ToString() & mOutputFileDelimiter &
                           peptideFragment.Mass & mOutputFileDelimiter &
                           Math.Round(peptideFragment.NET, 4).ToString() & mOutputFileDelimiter &
                           peptideFragment.PeptideName)

            If ComputepI Then
                Dim pI = ComputeSequencepI(baseSequence)
                Dim hydrophobicity = ComputeSequenceHydrophobicity(baseSequence)

                outLine.Append(mOutputFileDelimiter & pI.ToString("0.000") &
                               mOutputFileDelimiter & hydrophobicity.ToString("0.0000"))
            End If

            If ComputeNET Then
                Dim lcNET = ComputeSequenceNET(baseSequence)

                outLine.Append(mOutputFileDelimiter & lcNET.ToString("0.0000"))
            End If

            If ComputeSCXNET Then
                Dim scxNET = ComputeSequenceSCXNET(baseSequence)

                outLine.Append(mOutputFileDelimiter & scxNET.ToString("0.0000"))
            End If

            digestFileWriter.WriteLine(outLine.ToString())
        Next

    End Sub

    Private Sub ParseProteinFileWriteFasta(proteinFileWriter As TextWriter, outLine As StringBuilder)
        ' Write the entry to the output fasta file

        If mProteins(mProteinCount).Name = "ProteinName" AndAlso
           mProteins(mProteinCount).Description = "Description" AndAlso
           mProteins(mProteinCount).Sequence = "Sequence" Then
            ' Skip this entry; it's an artifact from converting from a fasta file to a text file, then back to a fasta file
            Return
        End If

        outLine.Clear()
        outLine.Append(FastaFileOptions.ProteinLineStartChar & mProteins(mProteinCount).Name)
        If Not ExcludeProteinDescription Then
            outLine.Append(FastaFileOptions.ProteinLineAccessionEndChar & mProteins(mProteinCount).Description)
        End If
        proteinFileWriter.WriteLine(outLine.ToString())

        If Not ExcludeProteinSequence Then
            Dim index = 0
            Do While index < mProteins(mProteinCount).Sequence.Length
                Dim length = Math.Min(60, mProteins(mProteinCount).Sequence.Length - index)
                proteinFileWriter.WriteLine(mProteins(mProteinCount).Sequence.Substring(index, length))
                index += 60
            Loop
        End If
    End Sub

    Private Sub ParseProteinFileWriteTextDelimited(
      proteinFileWriter As TextWriter,
      outLine As StringBuilder,
      lookForAddnlRefInDescription As Boolean,
      ByRef addnlRefsToOutput() As udtAddnlRefType)

        ' Write the entry to the protein output file, and possibly digest it

        outLine.Clear()


        If lookForAddnlRefInDescription Then
            ' Reset the Accession numbers in addnlRefsToOutput
            For index = 0 To addnlRefsToOutput.Length - 1
                addnlRefsToOutput(index).RefAccession = String.Empty
            Next index

            ' Update the accession numbers in addnlRefsToOutput
            For index = 0 To mProteins(mProteinCount).AlternateNameCount - 1
                For compareIndex = 0 To addnlRefsToOutput.Length - 1
                    If addnlRefsToOutput(compareIndex).RefName.ToUpper = mProteins(mProteinCount).AlternateNames(index).RefName.ToUpper Then
                        addnlRefsToOutput(compareIndex).RefAccession = mProteins(mProteinCount).AlternateNames(index).RefAccession
                        Exit For
                    End If
                Next compareIndex
            Next index

            outLine.Append(mProteins(mProteinCount).Name & mOutputFileDelimiter)
            For index = 0 To addnlRefsToOutput.Length - 1
                outLine.Append(addnlRefsToOutput(index).RefAccession & mOutputFileDelimiter)
            Next index

            If Not ExcludeProteinDescription Then
                outLine.Append(mProteins(mProteinCount).Description)
            End If

        Else
            outLine.Append(mProteins(mProteinCount).Name & mOutputFileDelimiter)
            If Not ExcludeProteinDescription Then
                outLine.Append(mProteins(mProteinCount).Description)
            End If

        End If

        If ComputeSequenceHashValues Then
            outLine.Append(mOutputFileDelimiter & mProteins(mProteinCount).SequenceHash)
        End If

        If Not ExcludeProteinSequence Then
            outLine.Append(mOutputFileDelimiter & mProteins(mProteinCount).Sequence)
        End If

        If ComputeProteinMass Then
            outLine.Append(mOutputFileDelimiter & Math.Round(mProteins(mProteinCount).Mass, 5).ToString())
        End If

        If ComputepI Then
            outLine.Append(mOutputFileDelimiter & mProteins(mProteinCount).pI.ToString("0.000") &
                           mOutputFileDelimiter & mProteins(mProteinCount).Hydrophobicity.ToString("0.0000"))
        End If

        If ComputeNET Then
            outLine.Append(mOutputFileDelimiter & mProteins(mProteinCount).ProteinNET.ToString("0.0000"))
        End If

        If ComputeSCXNET Then
            outLine.Append(mOutputFileDelimiter & mProteins(mProteinCount).ProteinSCXNET.ToString("0.0000"))
        End If

        proteinFileWriter.WriteLine(outLine.ToString())

    End Sub

    Private Function ParseProteinFileCreateOutputFile(
      ByRef pathInfo As udtFilePathInfoType,
      <Out> ByRef proteinFileReader As ProteinFileReaderBaseClass) As Boolean

        Dim outputFileName As String
        Dim success As Boolean

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
                outputFileName = String.Copy(pathInfo.OutputFileNameBaseOverride)

                If CreateFastaOutputFile Then
                    If Not Path.GetExtension(outputFileName).Equals(".fasta", StringComparison.OrdinalIgnoreCase) Then
                        outputFileName &= ".fasta"
                    End If
                Else
                    If Path.GetExtension(outputFileName).Length > 4 Then
                        outputFileName &= ".txt"
                    End If
                End If
            Else
                If CreateFastaOutputFile Then
                    outputFileName = pathInfo.OutputFileNameBaseOverride & ".fasta"
                Else
                    outputFileName = pathInfo.OutputFileNameBaseOverride & ".txt"
                End If
            End If
        Else
            outputFileName = String.Empty
        End If

        If mParsedFileIsFastaFile Then
            Dim reader = New FastaFileReader()
            proteinFileReader = reader
        Else
            Dim reader = New DelimitedProteinFileReader With {
                .Delimiter = mInputFileDelimiter,
                .DelimitedFileFormatCode = DelimitedFileFormatCode}
            proteinFileReader = reader
        End If

        ' Verify that the input file exists
        If Not File.Exists(pathInfo.ProteinInputFilePath) Then
            SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath)
            success = False
            Return success
        End If

        If mParsedFileIsFastaFile Then
            If outputFileName.Length = 0 Then
                outputFileName = Path.GetFileName(pathInfo.ProteinInputFilePath)

                If Path.GetExtension(outputFileName).Equals(".fasta", StringComparison.OrdinalIgnoreCase) Then
                    ' Nothing special to do; will replace the extension below

                ElseIf outputFileName.EndsWith(".fasta.gz", StringComparison.OrdinalIgnoreCase) Then
                    ' Remove .gz from outputFileName
                    outputFileName = StripExtension(outputFileName, ".gz")

                Else
                    ' .Fasta appears somewhere in the middle
                    ' Remove the text .Fasta, then add the extension .txt (unless it already ends in .txt)
                    Dim charIndex = outputFileName.ToLower().LastIndexOf(".fasta", StringComparison.Ordinal)
                    If charIndex > 0 Then
                        If charIndex < outputFileName.Length Then
                            outputFileName = outputFileName.Substring(0, charIndex) & outputFileName.Substring(charIndex + 6)
                        Else
                            outputFileName = outputFileName.Substring(0, charIndex)
                        End If
                    Else
                        ' This shouldn't happen
                    End If
                End If

                If CreateFastaOutputFile Then
                    outputFileName = Path.GetFileNameWithoutExtension(outputFileName) & "_new.fasta"
                Else
                    outputFileName = Path.ChangeExtension(outputFileName, ".txt")
                End If
            End If
        Else
            If outputFileName.Length = 0 Then
                If CreateFastaOutputFile Then
                    outputFileName = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) & ".fasta"
                Else
                    outputFileName = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) & "_parsed.txt"
                End If
            End If
        End If

        ' Make sure the output file isn't the same as the input file
        If Path.GetFileName(pathInfo.ProteinInputFilePath).ToLower = Path.GetFileName(outputFileName).ToLower Then
            outputFileName = Path.GetFileNameWithoutExtension(outputFileName) & "_new" & Path.GetExtension(outputFileName)
        End If

        ' Define the full path to the parsed proteins output file
        pathInfo.ProteinOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, outputFileName)

        If outputFileName.EndsWith("_parsed.txt") Then
            outputFileName = outputFileName.Substring(0, outputFileName.Length - "_parsed.txt".Length) & "_digested"
        Else
            outputFileName = Path.GetFileNameWithoutExtension(outputFileName) & "_digested"
        End If

        outputFileName &= "_Mass" & Math.Round(DigestionOptions.MinFragmentMass, 0).ToString() & "to" & Math.Round(DigestionOptions.MaxFragmentMass, 0).ToString()

        If ComputepI AndAlso (DigestionOptions.MinIsoelectricPoint > 0 OrElse DigestionOptions.MaxIsoelectricPoint < 14) Then
            outputFileName &= "_pI" & Math.Round(DigestionOptions.MinIsoelectricPoint, 1).ToString() & "to" & Math.Round(DigestionOptions.MaxIsoelectricPoint, 2).ToString()
        End If

        outputFileName &= ".txt"

        ' Define the full path to the digested proteins output file
        pathInfo.DigestedProteinOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, outputFileName)

        success = True
        Return success
    End Function

    Private Function ParseProteinFileCreateDigestedProteinOutputFile(digestedProteinOutputFilePath As String, ByRef digestFileWriter As StreamWriter) As Boolean

        Try
            ' Create the digested protein output file
            digestFileWriter = New StreamWriter(digestedProteinOutputFilePath)

            Dim lineOut As String
            If Not ExcludeProteinSequence Then
                lineOut = "Protein_Name" & mOutputFileDelimiter & "Sequence" & mOutputFileDelimiter
            Else
                lineOut = String.Empty
            End If
            lineOut &= "Unique_ID" & mOutputFileDelimiter & "Monoisotopic_Mass" & mOutputFileDelimiter & "Predicted_NET" & mOutputFileDelimiter & "Tryptic_Name"

            If ComputepI Then
                lineOut &= mOutputFileDelimiter & "pI" & mOutputFileDelimiter & "Hydrophobicity"
            End If

            If ComputeNET Then
                lineOut &= mOutputFileDelimiter & "LC_NET"
            End If

            If ComputeSCXNET Then
                lineOut &= mOutputFileDelimiter & "SCX_NET"
            End If

            digestFileWriter.WriteLine(lineOut)

            Return True
        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile)
            Return False
        End Try

    End Function

    Private Sub PreScanProteinFileForAddnlRefsInDescription(
      proteinInputFilePath As String,
      addnlRefMasterNames As ISet(Of String))

        Dim reader As FastaFileReader = Nothing
        Dim udtProtein As udtProteinInfoType

        Dim index As Integer

        Dim success As Boolean
        Dim inputProteinFound As Boolean

        Try
            reader = New FastaFileReader()

            ' Attempt to open the input file
            If Not reader.OpenFile(proteinInputFilePath) Then
                success = False
                Exit Try
            End If

            success = True

        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
            success = False
        End Try

        ' Abort processing if we couldn't successfully open the input file
        If Not success Then Return

        Try

            ResetProgress("Pre-reading Fasta file; looking for possible additional reference names")

            ' Read each protein in the output file and process appropriately
            Do
                inputProteinFound = reader.ReadNextProteinEntry()

                If inputProteinFound Then
                    udtProtein.Name = reader.ProteinName
                    udtProtein.Description = reader.ProteinDescription

                    ' Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
                    ReDim udtProtein.AlternateNames(0)
                    udtProtein.AlternateNameCount = ExtractAlternateProteinNamesFromDescription(udtProtein.Description, udtProtein.AlternateNames)

                    ' Make sure each of the names in .AlternateNames() is in addnlRefMasterNames
                    For index = 0 To udtProtein.AlternateNameCount - 1
                        If Not addnlRefMasterNames.Contains(udtProtein.AlternateNames(index).RefName) Then
                            addnlRefMasterNames.Add(udtProtein.AlternateNames(index).RefName)
                        End If
                    Next index

                    UpdateProgress(reader.PercentFileProcessed())

                    If AbortProcessing Then Exit Do
                End If
            Loop While inputProteinFound

            reader.CloseFile()

        Catch ex As Exception
            SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
        End Try

        Return
    End Sub

    ''' <summary>
    ''' If filePath ends with extension, remove it
    ''' Supports multi-part extensions like .fasta.gz
    ''' </summary>
    ''' <param name="filePath">File name or path</param>
    ''' <param name="extension">Extension, with or without the leading period</param>
    ''' <returns></returns>
    Public Shared Function StripExtension(filePath As String, extension As String) As String
        If String.IsNullOrWhiteSpace(extension) Then
            Return filePath
        End If

        If Not extension.StartsWith(".") Then
            extension = "." & extension
        End If

        If Not filePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase) Then
            Return filePath
        End If

        Return filePath.Substring(0, filePath.Length - extension.Length)
    End Function

    Private Function ValidateProteinName(proteinName As String, maximumLength As Integer) As String
        Dim sepChars = New Char() {" "c, ","c, ";"c, ":"c, "_"c, "-"c, "|"c, "/"c}

        If maximumLength < 1 Then maximumLength = 1

        If proteinName Is Nothing Then
            proteinName = String.Empty
        Else
            If proteinName.Length > maximumLength Then
                ' Truncate protein name to maximum length
                proteinName = proteinName.Substring(0, maximumLength)

                ' Make sure the protein name doesn't end in a space, dash, underscore, semicolon, colon, etc.
                proteinName = proteinName.TrimEnd(sepChars)
            End If
        End If

        Return proteinName

    End Function

    Private Sub WriteFastaAppendToCache(scrambledFileWriter As TextWriter, ByRef udtResidueCache As udtScramblingResidueCacheType, proteinNamePrefix As String, flushResiduesToWrite As Boolean)

        Dim residueCount As Integer
        Dim residuesToAppend As Integer

        If udtResidueCache.Cache.Length > 0 Then
            residueCount = CInt(Math.Round(udtResidueCache.Cache.Length * udtResidueCache.SamplingPercentage / 100.0, 0))
            If residueCount < 1 Then residueCount = 1
            If residueCount > udtResidueCache.Cache.Length Then residueCount = udtResidueCache.Cache.Length

            Do While residueCount > 0
                If udtResidueCache.ResiduesToWrite.Length + residueCount <= udtResidueCache.CacheLength Then
                    udtResidueCache.ResiduesToWrite &= udtResidueCache.Cache.Substring(0, residueCount)
                    udtResidueCache.Cache = String.Empty
                    residueCount = 0
                Else
                    residuesToAppend = udtResidueCache.CacheLength - udtResidueCache.ResiduesToWrite.Length
                    udtResidueCache.ResiduesToWrite &= udtResidueCache.Cache.Substring(0, residuesToAppend)
                    udtResidueCache.Cache = udtResidueCache.Cache.Substring(residuesToAppend)
                    residueCount -= residuesToAppend
                End If

                If udtResidueCache.ResiduesToWrite.Length >= udtResidueCache.CacheLength Then
                    ' Write out .ResiduesToWrite
                    WriteFastaEmptyCache(scrambledFileWriter, udtResidueCache, proteinNamePrefix, udtResidueCache.SamplingPercentage)
                End If

            Loop

        End If

        If flushResiduesToWrite AndAlso udtResidueCache.ResiduesToWrite.Length > 0 Then
            WriteFastaEmptyCache(scrambledFileWriter, udtResidueCache, proteinNamePrefix, udtResidueCache.SamplingPercentage)
        End If

    End Sub

    Private Sub WriteFastaEmptyCache(scrambledFileWriter As TextWriter, ByRef udtResidueCache As udtScramblingResidueCacheType, proteinNamePrefix As String, samplingPercentage As Integer)
        Dim proteinName As String
        Dim headerLine As String

        If udtResidueCache.ResiduesToWrite.Length > 0 Then
            udtResidueCache.OutputCount += 1

            proteinName = proteinNamePrefix & mFileNameAbbreviated

            If samplingPercentage < 100 Then
                proteinName = ValidateProteinName(proteinName, MAXIMUM_PROTEIN_NAME_LENGTH - 7 - Math.Max(5, udtResidueCache.OutputCount.ToString.Length))
            Else
                proteinName = ValidateProteinName(proteinName, MAXIMUM_PROTEIN_NAME_LENGTH - 1 - Math.Max(5, udtResidueCache.OutputCount.ToString.Length))
            End If

            If samplingPercentage < 100 Then
                proteinName &= "_" & samplingPercentage.ToString() & "pct" & "_"
            Else
                proteinName &= proteinName & "_"
            End If

            proteinName &= udtResidueCache.OutputCount.ToString()

            headerLine = FastaFileOptions.ProteinLineStartChar & proteinName & FastaFileOptions.ProteinLineAccessionEndChar & proteinName

            WriteFastaProteinAndResidues(scrambledFileWriter, headerLine, udtResidueCache.ResiduesToWrite)
            udtResidueCache.ResiduesToWrite = String.Empty
        End If

    End Sub

    Private Sub WriteFastaProteinAndResidues(scrambledFileWriter As TextWriter, headerLine As String, sequence As String)
        scrambledFileWriter.WriteLine(headerLine)
        Do While sequence.Length > 0
            If sequence.Length >= 60 Then
                scrambledFileWriter.WriteLine(sequence.Substring(0, 60))
                sequence = sequence.Substring(60)
            Else
                scrambledFileWriter.WriteLine(sequence)
                sequence = String.Empty
            End If
        Loop
    End Sub

    Private Sub WriteScrambledFasta(scrambledFileWriter As TextWriter, ByRef randomNumberGenerator As Random, udtProtein As udtProteinInfoType, eScramblingMode As ProteinScramblingModeConstants, ByRef udtResidueCache As udtScramblingResidueCacheType)

        Dim sequence As String
        Dim scrambledSequence As String
        Dim headerLine As String

        Dim proteinNamePrefix As String
        Dim proteinName As String

        Dim index As Integer
        Dim residueCount As Integer

        If eScramblingMode = ProteinScramblingModeConstants.Reversed Then
            proteinNamePrefix = PROTEIN_PREFIX_REVERSED
            scrambledSequence = StrReverse(udtProtein.Sequence)
        Else
            proteinNamePrefix = PROTEIN_PREFIX_SCRAMBLED

            scrambledSequence = String.Empty

            sequence = String.Copy(udtProtein.Sequence)
            residueCount = sequence.Length

            Do While residueCount > 0
                If residueCount <> sequence.Length Then
                    ShowErrorMessage("Assertion failed in WriteScrambledFasta: residueCount should equal sequence.Length: " & residueCount & " vs. " & sequence.Length)
                End If

                ' Randomly extract residues from sequence
                index = randomNumberGenerator.Next(residueCount)

                scrambledSequence &= sequence.Substring(index, 1)

                If index > 0 Then
                    If index < sequence.Length - 1 Then
                        sequence = sequence.Substring(0, index) & sequence.Substring(index + 1)
                    Else
                        sequence = sequence.Substring(0, index)
                    End If
                Else
                    sequence = sequence.Substring(index + 1)
                End If
                residueCount -= 1
            Loop

        End If

        If udtResidueCache.SamplingPercentage >= 100 Then
            proteinName = ValidateProteinName(proteinNamePrefix & udtProtein.Name, MAXIMUM_PROTEIN_NAME_LENGTH)
            headerLine = FastaFileOptions.ProteinLineStartChar & proteinName & FastaFileOptions.ProteinLineAccessionEndChar & udtProtein.Description

            WriteFastaProteinAndResidues(scrambledFileWriter, headerLine, scrambledSequence)
        Else
            ' Writing a sampling of the residues to the output file

            Do While scrambledSequence.Length > 0
                ' Append to the cache
                If udtResidueCache.Cache.Length + scrambledSequence.Length <= udtResidueCache.CacheLength Then
                    udtResidueCache.Cache &= scrambledSequence
                    scrambledSequence = String.Empty
                Else
                    residueCount = udtResidueCache.CacheLength - udtResidueCache.Cache.Length
                    udtResidueCache.Cache &= scrambledSequence.Substring(0, residueCount)
                    scrambledSequence = scrambledSequence.Substring(residueCount)
                End If

                If udtResidueCache.Cache.Length >= udtResidueCache.CacheLength Then
                    ' Write out a portion of the cache
                    WriteFastaAppendToCache(scrambledFileWriter, udtResidueCache, proteinNamePrefix, False)
                End If
            Loop

        End If

    End Sub

    ''' <summary>
    ''' Main processing function -- Calls ParseProteinFile
    ''' </summary>
    ''' <param name="inputFilePath"></param>
    ''' <param name="outputFolderPath"></param>
    ''' <param name="parameterFilePath"></param>
    ''' <param name="resetErrorCode"></param>
    ''' <returns></returns>
    Public Overloads Overrides Function ProcessFile(inputFilePath As String, outputFolderPath As String, parameterFilePath As String, resetErrorCode As Boolean) As Boolean
        ' Returns True if success, False if failure

        Dim inputFile As FileInfo
        Dim inputFilePathFull As String
        Dim statusMessage As String

        Dim success As Boolean

        If resetErrorCode Then
            SetLocalErrorCode(eParseProteinFileErrorCodes.NoError)
        End If

        If Not LoadParameterFileSettings(parameterFilePath) Then
            statusMessage = "Parameter file load error: " & parameterFilePath
            ShowErrorMessage(statusMessage)
            If ErrorCode = ProcessFilesErrorCodes.NoError Then
                SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidParameterFile)
            End If
            Return False
        End If

        Try
            If inputFilePath Is Nothing OrElse inputFilePath.Length = 0 Then
                ShowErrorMessage("Input file name is empty")
                SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath)
            Else

                Console.WriteLine()
                ShowMessage("Parsing " & Path.GetFileName(inputFilePath))

                If Not CleanupFilePaths(inputFilePath, outputFolderPath) Then
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.FilePathError)
                Else
                    Try
                        ' Obtain the full path to the input file
                        inputFile = New FileInfo(inputFilePath)
                        inputFilePathFull = inputFile.FullName

                        success = ParseProteinFile(inputFilePathFull, outputFolderPath)

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

    Private Sub SetLocalErrorCode(eNewErrorCode As eParseProteinFileErrorCodes)
        SetLocalErrorCode(eNewErrorCode, False)
    End Sub

    Private Sub SetLocalErrorCode(eNewErrorCode As eParseProteinFileErrorCodes, leaveExistingErrorCodeUnchanged As Boolean)

        If leaveExistingErrorCodeUnchanged AndAlso mLocalErrorCode <> eParseProteinFileErrorCodes.NoError Then
            ' An error code is already defined; do not change it
        Else
            mLocalErrorCode = eNewErrorCode

            If eNewErrorCode = eParseProteinFileErrorCodes.NoError Then
                If ErrorCode = ProcessFilesErrorCodes.LocalizedError Then
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.NoError)
                End If
            Else
                SetBaseClassErrorCode(ProcessFilesErrorCodes.LocalizedError)
            End If
        End If

    End Sub

    Protected Sub UpdateSubtaskProgress(description As String)
        UpdateProgress(description, mProgressPercentComplete)
    End Sub

    Protected Sub UpdateSubtaskProgress(percentComplete As Single)
        UpdateProgress(ProgressStepDescription, percentComplete)
    End Sub

    Protected Sub UpdateSubtaskProgress(description As String, percentComplete As Single)
        Dim descriptionChanged = description <> mSubtaskProgressStepDescription

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

        RaiseEvent SubtaskProgressChanged(description, percentComplete)

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
            ProteinLineStartChar = ">"c
            ProteinLineAccessionEndChar = " "c

            mLookForAddnlRefInDescription = False
            mAddnlRefSepChar = "|"c
            mAddnlRefAccessionSepChar = ":"c
        End Sub

        Private mReadonlyClass As Boolean

        Private mLookForAddnlRefInDescription As Boolean

        Private mAddnlRefSepChar As Char
        Private mAddnlRefAccessionSepChar As Char

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

        Public ReadOnly Property ProteinLineStartChar As Char = ">"c

        Public ReadOnly Property ProteinLineAccessionEndChar As Char = " "c

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

    End Class

    Private Sub InSilicoDigest_ErrorEvent(message As String) Handles mInSilicoDigest.ErrorEvent
        ShowErrorMessage("Error in mInSilicoDigest: " & message)
    End Sub

    Private Sub InSilicoDigest_ProgressChanged(taskDescription As String, percentComplete As Single) Handles mInSilicoDigest.ProgressChanged
        UpdateSubtaskProgress(taskDescription, percentComplete)
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
