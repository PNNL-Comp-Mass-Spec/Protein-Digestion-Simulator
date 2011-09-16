Option Strict On

' This class will read a protein fasta file or delimited protein info file and parse it
' to create a delimited protein list output file, and optionally an in-silico digested output file
' 
' It can also create a fasta file containing reversed or scrambled sequences, and these can
' be based on all of the proteins in the input file, or a sampling of the proteins in the input file
'
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.
'
' Started October 2, 2004

Public Class clsParseProteinFile
	Inherits clsProcessFilesBaseClass

	Public Sub New()
		MyBase.mFileDate = "September 16, 2011"
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
		Public RefName As String				'e.g. in gi:12334  the RefName is "gi" and the RefAccession is "1234"
		Public RefAccession As String
	End Structure

	Public Structure udtProteinInfoType
		Public Name As String
		Public AlternateNameCount As Integer
		Public AlternateNames() As udtAddnlRefType
		Public Description As String
		Public Sequence As String
		Public Mass As Double
		Public pI As Single
		Public Hydrophobicity As Single
		Public UniqueSequenceID As Integer				' Only applies if reading a delimited text file containing peptide sequences and UniqueSequenceID values
	End Structure

	Private Structure udtScrambingResidueCacheType
		Public Cache As String							' Cache of residues parsed; when this reaches 4000 characters, then a portion of this text is appended to ResiduesToWrite
		Public CacheLength As Integer
		Public SamplingPercentage As Integer
		Public OutputCount As Integer
		Public ResiduesToWrite As String
	End Structure
#End Region

#Region "Classwide Variables"
	Private mAssumeDelimitedFile As Boolean
	Private mAssumeFastaFile As Boolean
	Private mInputFileDelimiter As Char								 ' Only used for delimited protein input files, not for fasta files
	Private mDelimitedInputFileFormatCode As ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode

	Private mInputFileProteinsProcessed As Integer
	Private mInputFileLinesRead As Integer
	Private mInputFileLineSkipCount As Integer

	Private mOutputFileDelimiter As Char
	Private mExcludeProteinSequence As Boolean
	Private mComputeProteinMass As Boolean
	Private mComputepI As Boolean
	Private mComputeSCXNET As Boolean

	Private mIncludeXResiduesInMass As Boolean
	Private mProteinScramblingMode As ProteinScramblingModeConstants
	Private mProteinScramblingSamplingPercentage As Integer
	Private mProteinScramblingLoopCount As Integer

	Private mCreateFastaOutputFile As Boolean				' Only valid if mCreateDigestedProteinOutputFile is False
	Private mCreateProteinOutputFile As Boolean				' If false, then caches the results in memory; Use DigestProteinSequence to retrieve the digested peptides, if desired
	Private mCreateDigestedProteinOutputFile As Boolean		' Only valid if mCreateProteinOutputFile is True
	Private mGenerateUniqueSequenceIDValues As Boolean		' Only valid if mCreateDigestedProteinOutputFile is True

	' pI options
	Private mHydrophobicityType As clspICalculation.eHydrophobicityTypeConstants
	Private mReportMaximumpI As Boolean
	Private mSequenceWidthToExamineForMaximumpI As Integer

	Public DigestionOptions As clsInSilicoDigest.DigestionOptionsClass

	Public FastaFileOptions As FastaFileOptionsClass
	Private mObjectVariablesLoaded As Boolean
	Private WithEvents mInSilicoDigest As clsInSilicoDigest
	Private objpICalculator As clspICalculation

#If IncludePNNLNETRoutines Then
	Private WithEvents objSCXNETCalculator As NETPrediction.SCXElutionTimePredictionKangas
#End If

	Private mProteinCount As Integer
	Private mProteins() As udtProteinInfoType
	Private mParsedFileIsFastaFile As Boolean

	Private mFileNameAbbreviated As String

	Private mShowDebugPrompts As Boolean
	Private mLocalErrorCode As eParseProteinFileErrorCodes

	Private mSubtaskProgressStepDescription As String = String.Empty
	Private mSubtaskProgressPercentComplete As Single = 0

	' PercentComplete ranges from 0 to 100, but can contain decimal percentage values
	Public Event SubtaskProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single)

#End Region

#Region "Processing Options Interface Functions"
	Public Property AssumeDelimitedFile() As Boolean
		Get
			Return mAssumeDelimitedFile
		End Get
		Set(ByVal Value As Boolean)
			mAssumeDelimitedFile = Value
		End Set
	End Property

	Public Property AssumeFastaFile() As Boolean
		Get
			Return mAssumeFastaFile
		End Get
		Set(ByVal Value As Boolean)
			mAssumeFastaFile = Value
		End Set
	End Property

	Public Property ComputepI() As Boolean
		Get
			Return mComputepI
		End Get
		Set(ByVal Value As Boolean)
			mComputepI = Value
		End Set
	End Property

	Public Property ComputeProteinMass() As Boolean
		Get
			Return mComputeProteinMass
		End Get
		Set(ByVal Value As Boolean)
			mComputeProteinMass = Value
		End Set
	End Property

	Public Property ComputeSCXNET() As Boolean
		Get
			Return mComputeSCXNET
		End Get
		Set(ByVal Value As Boolean)
			mComputeSCXNET = Value
		End Set
	End Property

	Public Property CreateFastaOutputFile() As Boolean
		Get
			Return mCreateFastaOutputFile
		End Get
		Set(ByVal Value As Boolean)
			mCreateFastaOutputFile = Value
		End Set
	End Property

	Public Property CreateProteinOutputFile() As Boolean
		Get
			Return mCreateProteinOutputFile
		End Get
		Set(ByVal Value As Boolean)
			mCreateProteinOutputFile = Value
		End Set
	End Property

	Public Property CreateDigestedProteinOutputFile() As Boolean
		Get
			Return mCreateDigestedProteinOutputFile
		End Get
		Set(ByVal Value As Boolean)
			mCreateDigestedProteinOutputFile = Value
		End Set
	End Property

	Public Property DelimitedFileFormatCode() As ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode
		Get
			Return mDelimitedInputFileFormatCode
		End Get
		Set(ByVal Value As ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode)
			mDelimitedInputFileFormatCode = Value
		End Set
	End Property

	Public Property ElementMassMode() As PeptideSequenceClass.ElementModeConstants
		Get
			If mInSilicoDigest Is Nothing Then
				Return PeptideSequenceClass.ElementModeConstants.IsotopicMass
			Else
				Return mInSilicoDigest.ElementMassMode
			End If
		End Get
		Set(ByVal Value As PeptideSequenceClass.ElementModeConstants)
			If mInSilicoDigest Is Nothing Then
				InitializeObjectVariables()
			End If
			mInSilicoDigest.ElementMassMode = Value
		End Set
	End Property
	Public Property ExcludeProteinSequence() As Boolean
		Get
			Return mExcludeProteinSequence
		End Get
		Set(ByVal Value As Boolean)
			mExcludeProteinSequence = Value
		End Set
	End Property
	Public Property GenerateUniqueIDValuesForPeptides() As Boolean
		Get
			Return mGenerateUniqueSequenceIDValues
		End Get
		Set(ByVal Value As Boolean)
			mGenerateUniqueSequenceIDValues = Value
		End Set
	End Property

	Public Property HydrophobicityType() As clspICalculation.eHydrophobicityTypeConstants
		Get
			Return mHydrophobicityType
		End Get
		Set(ByVal Value As clspICalculation.eHydrophobicityTypeConstants)
			mHydrophobicityType = Value
		End Set
	End Property

	Public Property IncludeXResiduesInMass() As Boolean
		Get
			Return mIncludeXResiduesInMass
		End Get
		Set(ByVal Value As Boolean)
			mIncludeXResiduesInMass = Value
		End Set
	End Property

	Public Property InputFileDelimiter() As Char
		Get
			Return mInputFileDelimiter
		End Get
		Set(ByVal Value As Char)
			If Not Value = Nothing Then
				mInputFileDelimiter = Value
			End If
		End Set
	End Property

	Public ReadOnly Property InputFileProteinsProcessed() As Integer
		Get
			Return mInputFileProteinsProcessed
		End Get
	End Property

	Public ReadOnly Property InputFileLinesRead() As Integer
		Get
			Return mInputFileLinesRead
		End Get
	End Property

	Public ReadOnly Property InputFileLineSkipCount() As Integer
		Get
			Return mInputFileLineSkipCount
		End Get
	End Property

	Public ReadOnly Property LocalErrorCode() As eParseProteinFileErrorCodes
		Get
			Return mLocalErrorCode
		End Get
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

	Public ReadOnly Property ParsedFileIsFastaFile() As Boolean
		Get
			Return mParsedFileIsFastaFile
		End Get
	End Property

	Public Property ProteinScramblingLoopCount() As Integer
		Get
			Return mProteinScramblingLoopCount
		End Get
		Set(ByVal Value As Integer)
			mProteinScramblingLoopCount = Value
		End Set
	End Property

	Public Property ProteinScramblingMode() As ProteinScramblingModeConstants
		Get
			Return mProteinScramblingMode
		End Get
		Set(ByVal Value As ProteinScramblingModeConstants)
			mProteinScramblingMode = Value
		End Set
	End Property

	Public Property ProteinScramblingSamplingPercentage() As Integer
		Get
			Return mProteinScramblingSamplingPercentage
		End Get
		Set(ByVal Value As Integer)
			mProteinScramblingSamplingPercentage = Value
		End Set
	End Property

	Public Property ReportMaximumpI() As Boolean
		Get
			Return mReportMaximumpI
		End Get
		Set(ByVal Value As Boolean)
			mReportMaximumpI = Value
		End Set
	End Property

	Public Property SequenceWidthToExamineForMaximumpI() As Integer
		Get
			Return mSequenceWidthToExamineForMaximumpI
		End Get
		Set(ByVal Value As Integer)
			If Value < 1 Then mSequenceWidthToExamineForMaximumpI = 1
			mSequenceWidthToExamineForMaximumpI = Value
		End Set
	End Property

	Public Property ShowDebugPrompts() As Boolean
		Get
			Return mShowDebugPrompts
		End Get
		Set(ByVal Value As Boolean)
			mShowDebugPrompts = Value
		End Set
	End Property
#End Region

	Private Function ComputeSequenceHydrophobicity(ByVal strSequence As String) As Single

		' Be sure to call InitializeObjectVariables before calling this function for the first time
		' Otherwise, objpICalculator will be nothing
		If objpICalculator Is Nothing Then
			Return 0
		Else
			Return objpICalculator.CalculateSequenceHydrophobicity(strSequence)
		End If

	End Function

	Private Function ComputeSequencepI(ByVal strSequence As String) As Single

		' Be sure to call InitializeObjectVariables before calling this function for the first time
		' Otherwise, objpICalculator will be nothing
		If objpICalculator Is Nothing Then
			Return 0
		Else
			Return objpICalculator.CalculateSequencepI(strSequence)
		End If

	End Function

	Private Function ComputeSequenceMass(ByVal strSequence As String) As Double

		' Be sure to call InitializeObjectVariables before calling this function for the first time
		' Otherwise, objInSilicoDigest will be nothing
		If mInSilicoDigest Is Nothing Then
			Return 0
		Else
			Return mInSilicoDigest.ComputeSequenceMass(strSequence, mIncludeXResiduesInMass)
		End If

	End Function

	Private Function ComputeSequenceSCXNET(ByVal strSequence As String) As Single

#If IncludePNNLNETRoutines Then
		' Be sure to call InitializeObjectVariables before calling this function for the first time
		' Otherwise, objpICalculator will be nothing
		If objSCXNETCalculator Is Nothing Then
			Return 0
		Else
			Return objSCXNETCalculator.GetElutionTime(strSequence)
		End If
#Else
        Return 0
#End If

	End Function

	Public Function DigestProteinSequence(ByVal strSequence As String, ByRef udtPeptides() As clsInSilicoDigest.PeptideInfoClass, ByVal objDigestionOptions As clsInSilicoDigest.DigestionOptionsClass, Optional ByVal strProteinName As String = "") As Integer
		' Returns the number of digested peptides in udtPeptides

		Dim intPeptideCount As Integer

		' Make sure the object variables are initialized
		If Not InitializeObjectVariables() Then Return 0

		Try
			intPeptideCount = mInSilicoDigest.DigestSequence(strSequence, udtPeptides, objDigestionOptions, mComputepI, strProteinName)
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

						intCharIndex = strDescription.IndexOf(strRefs(intIndex))
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
					strErrorMessage = "Error initializing In Silico Digestor class"

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

	Public Function GetCachedProtein(ByVal intIndex As Integer) As udtProteinInfoType
		If intIndex < mProteinCount Then
			Return mProteins(intIndex)
		Else
			Return Nothing
		End If
	End Function

	Public Function GetDigestedPeptidesForCachedProtein(ByVal intIndex As Integer, ByRef udtPeptides() As clsInSilicoDigest.PeptideInfoClass, ByVal objDigestionOptions As clsInSilicoDigest.DigestionOptionsClass) As Integer
		' Returns the number of entries in udtPeptides()

		If intIndex < mProteinCount Then
			Return DigestProteinSequence(mProteins(intIndex).Sequence, udtPeptides, objDigestionOptions, mProteins(intIndex).Name)
		Else
			Return 0
		End If

	End Function

	Private Sub InitializeLocalVariables()
		mLocalErrorCode = eParseProteinFileErrorCodes.NoError
		mShowDebugPrompts = False

		mAssumeDelimitedFile = False
		mAssumeFastaFile = False
		mInputFileDelimiter = ControlChars.Tab
		mDelimitedInputFileFormatCode = ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence

		mInputFileProteinsProcessed = 0
		mInputFileLinesRead = 0
		mInputFileLineSkipCount = 0

		mOutputFileDelimiter = ControlChars.Tab
		mExcludeProteinSequence = False
		mComputeProteinMass = True
		mComputepI = True
		mComputeSCXNET = True

		mIncludeXResiduesInMass = False
		mProteinScramblingMode = ProteinScramblingModeConstants.None
		mProteinScramblingSamplingPercentage = 100
		mProteinScramblingLoopCount = 1

		mCreateFastaOutputFile = False
		mCreateProteinOutputFile = False
		mCreateDigestedProteinOutputFile = False
		mGenerateUniqueSequenceIDValues = True

		DigestionOptions = New clsInSilicoDigest.DigestionOptionsClass
		FastaFileOptions = New FastaFileOptionsClass

		mProteinCount = 0
		ReDim mProteins(0)

		mFileNameAbbreviated = String.Empty

		mHydrophobicityType = clspICalculation.eHydrophobicityTypeConstants.HW
		mReportMaximumpI = False
		mSequenceWidthToExamineForMaximumpI = 10

	End Sub

	Public Shared Function IsFastaFile(ByVal strFilePath As String) As Boolean
		' Examines the file's extension and true if it ends in .fasta

		If System.IO.Path.GetFileName(strFilePath).ToLower.IndexOf(".fasta") > 0 Then
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
				objpICalculator = New clspICalculation
			Catch ex As Exception
				strErrorMessage = "Error initializing pI Calculation class"
				ShowErrorMessage(strErrorMessage)
				Me.SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
			End Try

#If IncludePNNLNETRoutines Then
			Try
				objSCXNETCalculator = New NETPrediction.SCXElutionTimePredictionKangas
			Catch ex As Exception
				strErrorMessage = "Error initializing SCX NET Calculation class"
				ShowErrorMessage(strErrorMessage)
				Me.SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorInitializingObjectVariables)
			End Try
#End If

			If strErrorMessage.Length > 0 Then
				If MyBase.ShowMessages Then MsgBox(strErrorMessage, MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Error")
				ShowErrorMessage(strErrorMessage)
				mObjectVariablesLoaded = False
			Else
				mObjectVariablesLoaded = True
			End If
		End If

		If Not mInSilicoDigest Is Nothing Then
			If Not objpICalculator Is Nothing Then
				mInSilicoDigest.InitializepICalculator(objpICalculator)
			End If
		End If

		Return mObjectVariablesLoaded

	End Function

	Public Function LoadParameterFileSettings(ByVal strParameterFilePath As String) As Boolean

		Dim objSettingsFile As New XmlSettingsFileAccessor

		Dim intDelimeterIndex As Integer
		Dim strCustomDelimiter As String

		Dim blnCysPeptidesOnly As Boolean

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

			' Pass False to .LoadSettings() here to turn off case sensitive matching
			If objSettingsFile.LoadSettings(strParameterFilePath, False) Then
				If Not objSettingsFile.SectionPresent(XML_SECTION_OPTIONS) Then
					If MyBase.ShowMessages Then
						MsgBox("The node '<section name=""" & XML_SECTION_OPTIONS & """> was not found in the parameter file: " & strParameterFilePath, MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Invalid File")
					End If
					SetLocalErrorCode(eParseProteinFileErrorCodes.ProteinFileParsingOptionsSectionNotFound)
					Return False
				Else
					'Me.ComparisonFastaFile = objSettingsFile.GetParam(XML_SECTION_OPTIONS, "ComparisonFastaFile", Me.ComparisonFastaFile)
					intDelimeterIndex = DelimiterCharConstants.Tab
					strCustomDelimiter = ControlChars.Tab

					intDelimeterIndex = objSettingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnDelimiterIndex", intDelimeterIndex)
					strCustomDelimiter = objSettingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnDelimiter", strCustomDelimiter)

					Me.InputFileDelimiter = LookupColumnDelimiterChar(intDelimeterIndex, strCustomDelimiter, Me.InputFileDelimiter)

					Me.DelimitedFileFormatCode = CType(objSettingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnOrdering", CInt(Me.DelimitedFileFormatCode)), ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode)

					intDelimeterIndex = DelimiterCharConstants.Tab
					strCustomDelimiter = ControlChars.Tab
					intDelimeterIndex = objSettingsFile.GetParam(XML_SECTION_OPTIONS, "OutputFileFieldDelimeterIndex", intDelimeterIndex)
					strCustomDelimiter = objSettingsFile.GetParam(XML_SECTION_OPTIONS, "OutputFileFieldDelimeter", strCustomDelimiter)

					Me.OutputFileDelimiter = LookupColumnDelimiterChar(intDelimeterIndex, strCustomDelimiter, Me.OutputFileDelimiter)

					With Me.DigestionOptions
						.IncludePrefixAndSuffixResidues = objSettingsFile.GetParam(XML_SECTION_OPTIONS, "IncludePrefixAndSuffixResidues", .IncludePrefixAndSuffixResidues)
					End With

					With Me.FastaFileOptions
						.ProteinLineStartChar = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "RefStartChar", .ProteinLineStartChar.ToString).Chars(0)

						intDelimeterIndex = DelimiterCharConstants.Space
						strCustomDelimiter = " "c

						intDelimeterIndex = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "RefEndCharIndex", intDelimeterIndex)

						.ProteinLineAccessionEndChar = LookupColumnDelimiterChar(intDelimeterIndex, strCustomDelimiter, .ProteinLineAccessionEndChar)

						.LookForAddnlRefInDescription = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "LookForAddnlRefInDescription", .LookForAddnlRefInDescription)

						.AddnlRefSepChar = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "AddnlRefSepChar", .AddnlRefSepChar.ToString).Chars(0)
						.AddnlRefAccessionSepChar = objSettingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "AddnlRefAccessionSepChar", .AddnlRefAccessionSepChar.ToString).Chars(0)
					End With

					Me.ComputeProteinMass = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeProteinMass", Me.ComputeProteinMass)
					Me.IncludeXResiduesInMass = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "IncludeXResidues", Me.IncludeXResiduesInMass)
					Me.ElementMassMode = CType(objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ElementMassMode", CInt(Me.ElementMassMode)), PeptideSequenceClass.ElementModeConstants)

					Me.CreateDigestedProteinOutputFile = objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "DigestProteins", Me.CreateDigestedProteinOutputFile)
					Me.ProteinScramblingMode = CType(objSettingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ProteinReversalIndex", CInt(Me.ProteinScramblingMode)), clsParseProteinFile.ProteinScramblingModeConstants)
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
			If MyBase.ShowMessages Then
				MsgBox("Error in LoadParameterFileSettings:" & ControlChars.NewLine & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Error")
			Else
				Throw New System.Exception("Error in LoadParameterFileSettings", ex)
			End If
			Return False
		End Try

		Return True

	End Function

	Public Shared Function LookupColumnDelimiterChar(ByVal intDelimiterIndex As Integer, ByVal strCustomDelimiter As String, ByVal strDefaultDelimiter As Char) As Char

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

	Public Function ParseProteinFile(ByVal strProteinInputFilePath As String, ByVal strOutputFolderPath As String) As Boolean
		Return ParseProteinFile(strProteinInputFilePath, strOutputFolderPath, String.Empty)
	End Function

	Public Function ParseProteinFile(ByVal strProteinInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strOutputFileNameBaseOverride As String) As Boolean
		' If strOutputFileNameBaseOverride is defined, then uses that name for the protein output filename rather than auto-defining the name

		Dim objProteinFileReader As ProteinFileReader.ProteinFileReaderBaseClass
		Dim objFastaFileReader As ProteinFileReader.FastaFileReader
		Dim objDelimitedFileReader As ProteinFileReader.DelimitedFileReader

		Dim srProteinOutputFile As System.IO.StreamWriter
		Dim srDigestOutputFile As System.IO.StreamWriter
		Dim srScrambledOutStream As System.IO.StreamWriter

		Dim strLineOut As String = String.Empty

		Dim strOutputFileName As String
		Dim strProteinOutputFilePath As String = String.Empty
		Dim strDigestedProteinOutputFilePath As String = String.Empty
		Dim strScrambledFastaOutputFilePath As String
		Dim strSuffix As String

		Dim blnSuccess As Boolean = False
		Dim blnInputProteinFound As Boolean
		Dim blnUseUniqueIDValuesFromInputFile As Boolean

		Dim intIndex, intCompareIndex As Integer
		Dim intLength As Integer
		Dim intPeptideCount As Integer
		Dim udtPeptides() As clsInSilicoDigest.PeptideInfoClass

		Dim blnAllowLookForAddnlRefInDescription As Boolean
		Dim blnLookForAddnlRefInDescription As Boolean
		Dim htAddnlRefMasterNames As System.Collections.Specialized.StringDictionary		' Note that StringDictionary keys are case-insensitive, and are therefore stored lowercase

		Dim udtAddnlRefsToOutput() As udtAddnlRefType

		Dim blnGenerateUniqueSequenceID As Boolean
		Dim htMasterSequences As Hashtable
		Dim intUniqueSeqID As Integer
		Dim intNextUniqueIDForMasterSeqs As Integer

		Dim strBaseSequence As String
		Dim sngpI As Single
		Dim sngHydrophobicity As Single
		Dim sngSCXNET As Single

		Dim sngProteinSCXNET As Single

		Dim objRandomNumberGenerator As Random
		Dim intRandomNumberSeed As Integer
		Dim eScramblingMode As ProteinScramblingModeConstants
		Dim udtResidueCache As udtScrambingResidueCacheType

		Dim dtStartTime As Date

		Dim intLoopCount As Integer
		Dim intLoopIndex As Integer
		Dim sngPercentProcessed As Single

		Dim strMessage As String

		Try

			If strProteinInputFilePath Is Nothing OrElse strProteinInputFilePath.Length = 0 Then
				SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidInputFilePath)
			Else

				' Make sure the object variables are initialized
				If Not InitializeObjectVariables() Then
					blnSuccess = False
					Exit Try
				End If

				If mAssumeFastaFile OrElse IsFastaFile(strProteinInputFilePath) Then
					If mAssumeDelimitedFile Then
						mParsedFileIsFastaFile = False
					Else
						mParsedFileIsFastaFile = True
					End If
				Else
					mParsedFileIsFastaFile = False
				End If

				If mCreateDigestedProteinOutputFile Then
					' Make sure mCreateFastaOutputFile is false
					mCreateFastaOutputFile = False
				End If

				If Not strOutputFileNameBaseOverride Is Nothing AndAlso strOutputFileNameBaseOverride.Length > 0 Then
					If System.IO.Path.HasExtension(strOutputFileNameBaseOverride) Then
						strOutputFileName = String.Copy(strOutputFileNameBaseOverride)

						If mCreateFastaOutputFile Then
							If System.IO.Path.GetExtension(strOutputFileName).ToLower <> ".fasta" Then
								strOutputFileName &= ".fasta"
							End If
						Else
							If System.IO.Path.GetExtension(strOutputFileName).Length > 4 Then
								strOutputFileName &= ".txt"
							End If
						End If
					Else
						If mCreateFastaOutputFile Then
							strOutputFileName = strOutputFileNameBaseOverride & ".fasta"
						Else
							strOutputFileName = strOutputFileNameBaseOverride & ".txt"
						End If
					End If
				Else
					strOutputFileName = String.Empty
				End If

				If mParsedFileIsFastaFile Then
					objFastaFileReader = New ProteinFileReader.FastaFileReader
					With objFastaFileReader
						.ProteinLineStartChar = FastaFileOptions.ProteinLineStartChar
						.ProteinLineAccessionEndChar = FastaFileOptions.ProteinLineAccessionEndChar
					End With
					objProteinFileReader = objFastaFileReader

					blnUseUniqueIDValuesFromInputFile = False
				Else
					objDelimitedFileReader = New ProteinFileReader.DelimitedFileReader
					With objDelimitedFileReader
						.Delimiter = mInputFileDelimiter
						.DelimitedFileFormatCode = mDelimitedInputFileFormatCode
					End With
					objProteinFileReader = objDelimitedFileReader

					If mDelimitedInputFileFormatCode = ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID Or _
					   mDelimitedInputFileFormatCode = ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence Then
						blnUseUniqueIDValuesFromInputFile = True
					Else
						blnUseUniqueIDValuesFromInputFile = False
					End If

				End If

				' Verify that the input file exists
				If Not System.IO.File.Exists(strProteinInputFilePath) Then
					MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidInputFilePath)
					blnSuccess = False
					Exit Try
				End If

				If mParsedFileIsFastaFile Then
					If strOutputFileName.Length = 0 Then
						strOutputFileName = System.IO.Path.GetFileName(strProteinInputFilePath)
						If System.IO.Path.GetExtension(strOutputFileName).ToLower = ".fasta" Then
							' Nothing special to do; will replace the extension below
						Else
							' .Fasta appears somewhere in the middle
							' Remove the text .Fasta, then add the extension .txt (unless it already ends in .txt)
							Dim intLoc As Integer
							intLoc = strOutputFileName.ToLower.LastIndexOf(".fasta")
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

						If mCreateFastaOutputFile Then
							strOutputFileName = System.IO.Path.GetFileNameWithoutExtension(strOutputFileName) & "_new.fasta"
						Else
							strOutputFileName = System.IO.Path.ChangeExtension(strOutputFileName, ".txt")
						End If
					End If
				Else
					If strOutputFileName.Length = 0 Then
						If mCreateFastaOutputFile Then
							strOutputFileName = System.IO.Path.GetFileNameWithoutExtension(strProteinInputFilePath) & ".fasta"
						Else
							strOutputFileName = System.IO.Path.GetFileNameWithoutExtension(strProteinInputFilePath) & "_parsed.txt"
						End If
					End If
				End If

				' Make sure the output file isn't the same as the input file
				If System.IO.Path.GetFileName(strProteinInputFilePath).ToLower = System.IO.Path.GetFileName(strOutputFileName).ToLower Then
					strOutputFileName = System.IO.Path.GetFileNameWithoutExtension(strOutputFileName) & "_new" & System.IO.Path.GetExtension(strOutputFileName)
				End If

				' Define the full path to the parsed proteins output file
				strProteinOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, strOutputFileName)

				If strOutputFileName.EndsWith("_parsed.txt") Then
					strOutputFileName = strOutputFileName.Substring(0, strOutputFileName.Length - "_parsed.txt".Length) & "_digested"
				Else
					strOutputFileName = System.IO.Path.GetFileNameWithoutExtension(strOutputFileName) & "_digested"
				End If

				strOutputFileName &= "_Mass" & Math.Round(DigestionOptions.MinFragmentMass, 0).ToString & "to" & Math.Round(DigestionOptions.MaxFragmentMass, 0).ToString

				If mComputepI AndAlso (DigestionOptions.MinIsoelectricPoint > 0 OrElse DigestionOptions.MaxIsoelectricPoint < 14) Then
					strOutputFileName &= "_pI" & Math.Round(DigestionOptions.MinIsoelectricPoint, 1).ToString & "to" & Math.Round(DigestionOptions.MaxIsoelectricPoint, 2).ToString
				End If

				strOutputFileName &= ".txt"

				' Define the full path to the digested proteins output file
				strDigestedProteinOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, strOutputFileName)

				blnSuccess = True
			End If

		Catch ex As Exception
			SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
			blnSuccess = False
		End Try

		' Abort processing if we couldn't successfully open the input file
		If Not blnSuccess Then Return False

		Try
			' Set the options for objpICalculator
			' Note that this will also update the pICalculator object in objInSilicoDigest
			If Not objpICalculator Is Nothing Then
				With objpICalculator
					.HydrophobicityType = mHydrophobicityType
					.ReportMaximumpI = mReportMaximumpI
					.SequenceWidthToExamineForMaximumpI = mSequenceWidthToExamineForMaximumpI
				End With
			End If

			If mCreateProteinOutputFile Then
				Try
					' Open the protein output file (if required)
					' This will cause an error if the input file is the same as the output file
					srProteinOutputFile = New System.IO.StreamWriter(strProteinOutputFilePath)
					blnSuccess = True
				Catch ex As Exception
					SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile)
					blnSuccess = False
				End Try
				If Not blnSuccess Then Exit Try
			End If

			If mCreateProteinOutputFile AndAlso mCreateDigestedProteinOutputFile AndAlso Not mCreateFastaOutputFile Then
				Try
					' Create the digested protein output file
					srDigestOutputFile = New System.IO.StreamWriter(strDigestedProteinOutputFilePath)
#If IncludePNNLNETRoutines Then
					strLineOut = "Protein_Name" & mOutputFileDelimiter & "Sequence" & mOutputFileDelimiter & "Unique_ID" & mOutputFileDelimiter & "Monoisotopic_Mass" & mOutputFileDelimiter & "Predicted_NET" & mOutputFileDelimiter & "Tryptic_Name"
#Else
                    strLineOut = "Protein_Name" & mOutputFileDelimiter & "Sequence" & mOutputFileDelimiter & "Unique_ID" & mOutputFileDelimiter & "Monoisotopic_Mass" & mOutputFileDelimiter & "Time" & mOutputFileDelimiter & "Tryptic_Name"
#End If

					If mComputepI Then
						strLineOut &= mOutputFileDelimiter & "pI" & mOutputFileDelimiter & "Hydrophobicity"
					End If

#If IncludePNNLNETRoutines Then
					If mComputeSCXNET Then
						strLineOut &= mOutputFileDelimiter & "SCX_NET"
					End If
#End If

					srDigestOutputFile.WriteLine(strLineOut)

					If mGenerateUniqueSequenceIDValues Then
						' Initialize htMasterSequences
						blnGenerateUniqueSequenceID = True
						htMasterSequences = New Hashtable
						intNextUniqueIDForMasterSeqs = 1
					End If

					blnSuccess = True
				Catch ex As Exception
					SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile)
					blnSuccess = False
				End Try
				If Not blnSuccess Then Exit Try
			End If

			If mProteinScramblingMode = ProteinScramblingModeConstants.Randomized Then
				intLoopCount = mProteinScramblingLoopCount
				If intLoopCount < 1 Then intLoopCount = 1
				If intLoopCount > 10000 Then intLoopCount = 10000

				blnAllowLookForAddnlRefInDescription = False
			Else
				intLoopCount = 1
				blnAllowLookForAddnlRefInDescription = FastaFileOptions.LookForAddnlRefInDescription
			End If

			For intLoopIndex = 1 To intLoopCount

				' Attempt to open the input file
				If Not objProteinFileReader.OpenFile(strProteinInputFilePath) Then
					SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
					blnSuccess = False
					Exit Try
				End If

				If mCreateProteinOutputFile Then
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

						' Wait to allow the timer to advance.
						System.Threading.Thread.Sleep(1)
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
						strScrambledFastaOutputFilePath = System.IO.Path.Combine(strOutputFolderPath, System.IO.Path.GetFileNameWithoutExtension(strProteinInputFilePath) & strSuffix & ".fasta")

						' Define the abbreviated name of the input file; used in the protein names
						mFileNameAbbreviated = System.IO.Path.GetFileNameWithoutExtension(strProteinInputFilePath)
						If mFileNameAbbreviated.Length > MAX_ABBREVIATED_FILENAME_LENGTH Then
							mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, MAX_ABBREVIATED_FILENAME_LENGTH)

							If mFileNameAbbreviated.Substring(mFileNameAbbreviated.Length - 1, 1) = "_" Then
								mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.Length - 1)
							Else
								If mFileNameAbbreviated.LastIndexOf("-") > MAX_ABBREVIATED_FILENAME_LENGTH / 3 Then
									mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.LastIndexOf("-"))
								ElseIf mFileNameAbbreviated.LastIndexOf("_") > MAX_ABBREVIATED_FILENAME_LENGTH / 3 Then
									mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.LastIndexOf("_"))
								End If
							End If
						End If

						' Make sure there aren't any spaces in the abbreviated filename
						mFileNameAbbreviated = mFileNameAbbreviated.Replace(" ", "_")

						Try
							' Open the scrambled protein output fasta file (if required)
							srScrambledOutStream = New System.IO.StreamWriter(strScrambledFastaOutputFilePath)
							blnSuccess = True
						Catch ex As Exception
							SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile)
							blnSuccess = False
						End Try
						If Not blnSuccess Then Exit Try

					End If
				Else
					eScramblingMode = ProteinScramblingModeConstants.None
				End If

				dtStartTime = System.DateTime.UtcNow

				If mCreateProteinOutputFile AndAlso mParsedFileIsFastaFile AndAlso blnAllowLookForAddnlRefInDescription Then
					' Need to pre-scan the fasta file to find all of the possible additional reference values

					htAddnlRefMasterNames = New System.Collections.Specialized.StringDictionary
					PreScanProteinFileForAddnlRefsInDescription(strProteinInputFilePath, htAddnlRefMasterNames)

					If htAddnlRefMasterNames.Keys.Count > 0 Then
						' Need to extract out the key names from htAddnlRefMasterNames and sort them alphabetically
						ReDim udtAddnlRefsToOutput(htAddnlRefMasterNames.Keys.Count - 1)
						blnLookForAddnlRefInDescription = True

						Dim myEnumerator As IEnumerator = htAddnlRefMasterNames.GetEnumerator()
						intIndex = 0
						While myEnumerator.MoveNext()
							udtAddnlRefsToOutput(intIndex).RefName = CType(myEnumerator.Current, System.Collections.DictionaryEntry).Key.ToString.ToUpper
							intIndex += 1
						End While

						Dim iAddnlRefComparerClass As New AddnlRefComparerClass
						Array.Sort(udtAddnlRefsToOutput, iAddnlRefComparerClass)
						iAddnlRefComparerClass = Nothing
					Else
						ReDim udtAddnlRefsToOutput(0)
						blnLookForAddnlRefInDescription = False
					End If
				End If

				ResetProgress("Parsing protein input file")

				If mCreateProteinOutputFile And Not mCreateFastaOutputFile Then
					' Write the header line to the output file
					strLineOut = "ProteinName" & mOutputFileDelimiter

					If blnLookForAddnlRefInDescription Then

						For intIndex = 0 To udtAddnlRefsToOutput.Length - 1
							With udtAddnlRefsToOutput(intIndex)
								strLineOut &= .RefName & mOutputFileDelimiter
							End With
						Next intIndex
					End If

					strLineOut &= "Description"

					If Not mExcludeProteinSequence Then
						strLineOut &= mOutputFileDelimiter & "Sequence"
					End If

					If mComputeProteinMass OrElse mComputepI OrElse mComputeSCXNET Then
						If ElementMassMode = PeptideSequenceClass.ElementModeConstants.AverageMass Then
							strLineOut &= mOutputFileDelimiter & "Average Mass"
						Else
							strLineOut &= mOutputFileDelimiter & "Mass"
						End If

						If mComputepI Then
							strLineOut &= mOutputFileDelimiter & "pI" & mOutputFileDelimiter & "Hydrophobicity"
						End If

#If IncludePNNLNETRoutines Then
						If mComputeSCXNET Then
							strLineOut &= mOutputFileDelimiter & "SCX_NET"
						End If
#End If
					End If

					srProteinOutputFile.WriteLine(strLineOut)
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

					If blnInputProteinFound Then
						mInputFileProteinsProcessed += 1
						mInputFileLinesRead = objProteinFileReader.LinesRead

						With mProteins(mProteinCount)
							.Name = objProteinFileReader.ProteinName
							.Description = objProteinFileReader.ProteinDescription

							If blnLookForAddnlRefInDescription Then
								' Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
								.AlternateNameCount = ExtractAlternateProteinNamesFromDescription(.Description, .AlternateNames)
							Else
								.AlternateNameCount = 0
								ReDim .AlternateNames(0)
							End If

							.Sequence = objProteinFileReader.ProteinSequence

							If mComputeProteinMass Then
								.Mass = ComputeSequenceMass(.Sequence)
							Else
								.Mass = 0
							End If

							If mComputepI Then
								.pI = ComputeSequencepI(.Sequence)
								.Hydrophobicity = ComputeSequenceHydrophobicity(.Sequence)
							Else
								.pI = 0
								.Hydrophobicity = 0
							End If

#If IncludePNNLNETRoutines Then
							If mComputeSCXNET Then
								sngProteinSCXNET = objSCXNETCalculator.GetElutionTime(.Sequence)
							End If
#End If

							If blnUseUniqueIDValuesFromInputFile Then
								.UniqueSequenceID = objProteinFileReader.EntryUniqueID
							Else
								.UniqueSequenceID = 0
							End If

						End With

						If mCreateProteinOutputFile Then
							If intLoopIndex = 1 Then
								With mProteins(mProteinCount)
									If mCreateFastaOutputFile Then
										' Write the entry to the output fasta file

										strLineOut = FastaFileOptions.ProteinLineStartChar & .Name & FastaFileOptions.ProteinLineAccessionEndChar & .Description
										srProteinOutputFile.WriteLine(strLineOut)

										If Not mExcludeProteinSequence Then
											intIndex = 0
											Do While intIndex < .Sequence.Length
												intLength = Math.Min(60, .Sequence.Length - intIndex)
												srProteinOutputFile.WriteLine(.Sequence.Substring(intIndex, intLength))
												intIndex += 60
											Loop
										End If

									Else
										' Write the entry to the protein output file, and possibly digest it

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

											strLineOut = .Name & mOutputFileDelimiter
											For intIndex = 0 To udtAddnlRefsToOutput.Length - 1
												With udtAddnlRefsToOutput(intIndex)
													strLineOut &= .RefAccession & mOutputFileDelimiter
												End With
											Next intIndex

											strLineOut &= .Description
										Else
											strLineOut = .Name & mOutputFileDelimiter & .Description
										End If

										If Not mExcludeProteinSequence Then
											strLineOut &= mOutputFileDelimiter & .Sequence
										End If

										If mComputeProteinMass OrElse mComputepI OrElse mComputeSCXNET Then
											strLineOut &= mOutputFileDelimiter & Math.Round(.Mass, 5).ToString

											If mComputepI Then
												strLineOut &= mOutputFileDelimiter & .pI.ToString("0.000") & mOutputFileDelimiter & .Hydrophobicity.ToString("0.0000")
											End If
#If IncludePNNLNETRoutines Then
											If mComputeSCXNET Then
												strLineOut &= mOutputFileDelimiter & sngProteinSCXNET.ToString("0.0000")
											End If
#End If
										End If

										srProteinOutputFile.WriteLine(strLineOut)

									End If

								End With
							End If

							If intLoopIndex = 1 AndAlso Not srDigestOutputFile Is Nothing Then
								intPeptideCount = DigestProteinSequence(mProteins(mProteinCount).Sequence, udtPeptides, DigestionOptions, mProteins(mProteinCount).Name)

								For intIndex = 0 To intPeptideCount - 1
									With udtPeptides(intIndex)
										If blnGenerateUniqueSequenceID Then
											Try
												If htMasterSequences.ContainsKey(.SequenceOneLetter) Then
													intUniqueSeqID = CInt(htMasterSequences(.SequenceOneLetter))
												Else
													htMasterSequences.Add(.SequenceOneLetter, intNextUniqueIDForMasterSeqs)
													intUniqueSeqID = intNextUniqueIDForMasterSeqs
												End If
												intNextUniqueIDForMasterSeqs += 1
											Catch ex As Exception
												intUniqueSeqID = 0
											End Try
										Else
											intUniqueSeqID = 0
										End If

										strBaseSequence = .SequenceOneLetter
										If Not mExcludeProteinSequence Then
											If DigestionOptions.IncludePrefixAndSuffixResidues Then
												strLineOut = mProteins(mProteinCount).Name & mOutputFileDelimiter & .PrefixResidue & "." & strBaseSequence & "." & .SuffixResidue
											Else
												strLineOut = mProteins(mProteinCount).Name & mOutputFileDelimiter & strBaseSequence
											End If
										End If
										strLineOut &= mOutputFileDelimiter & intUniqueSeqID.ToString & mOutputFileDelimiter & .Mass & mOutputFileDelimiter & Math.Round(.NET, 4).ToString & mOutputFileDelimiter & .PeptideName

										If mComputepI Then
											sngpI = ComputeSequencepI(strBaseSequence)
											sngHydrophobicity = ComputeSequenceHydrophobicity(strBaseSequence)

											strLineOut &= mOutputFileDelimiter & sngpI.ToString("0.000") & mOutputFileDelimiter & sngHydrophobicity.ToString("0.0000")
										End If

#If IncludePNNLNETRoutines Then
										If mComputeSCXNET Then
											sngSCXNET = ComputeSequenceSCXNET(strBaseSequence)

											strLineOut &= mOutputFileDelimiter & sngSCXNET.ToString("0.0000")
										End If
#End If

										srDigestOutputFile.WriteLine(strLineOut)
									End With
								Next intIndex
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

						sngPercentProcessed = (intLoopIndex - 1) / CSng(intLoopCount) * 100.0! + objProteinFileReader.PercentFileProcessed() / intLoopCount
						UpdateProgress(sngPercentProcessed)

						If Me.AbortProcessing Then Exit Do

					End If
				Loop While blnInputProteinFound

				If mCreateProteinOutputFile And eScramblingMode <> ProteinScramblingModeConstants.None Then
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

				If Not srProteinOutputFile Is Nothing Then
					srProteinOutputFile.Close()
				End If

				If Not srDigestOutputFile Is Nothing Then
					srDigestOutputFile.Close()
				End If

				If Not srScrambledOutStream Is Nothing Then
					srScrambledOutStream.Close()
				End If
			Next intLoopIndex

			If mShowDebugPrompts Then
				MsgBox(System.IO.Path.GetFileName(strProteinInputFilePath) & ControlChars.NewLine & "Elapsed time: " & Math.Round(System.DateTime.UtcNow.Subtract(dtStartTime).TotalSeconds, 2).ToString & " seconds", MsgBoxStyle.Information Or MsgBoxStyle.OkOnly, "Status")
			End If

			If MyBase.ShowMessages Then
				strMessage = "Done: Processed " & mInputFileProteinsProcessed.ToString("###,##0") & " proteins (" & mInputFileLinesRead.ToString("###,###,##0") & " lines)"
				If mInputFileLineSkipCount > 0 Then
					strMessage &= ControlChars.NewLine & "Note that " & mInputFileLineSkipCount.ToString("###,##0") & " lines were skipped in the input file due to having an unexpected format. "
					If mParsedFileIsFastaFile Then
						strMessage &= "This is an unexpected error for fasta files."
					Else
						strMessage &= "Make sure that " & mDelimitedInputFileFormatCode.ToString & " is the appropriate format for this file (see the File Format Options tab)."
					End If
				End If

				If intLoopCount > 1 Then
					strMessage &= ControlChars.NewLine & "Created " & intLoopCount.ToString & " replicates of the scrambled output file"
				End If
				MsgBox(strMessage, MsgBoxStyle.Information Or MsgBoxStyle.OkOnly, "Done")
			End If

			blnSuccess = True

		Catch ex As Exception
			If mCreateProteinOutputFile Or mCreateDigestedProteinOutputFile Then
				SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorWritingOutputFile)
				blnSuccess = False
			Else
				SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
				blnSuccess = False
			End If
		Finally
			If Not srProteinOutputFile Is Nothing Then
				srProteinOutputFile.Close()
			End If
			If Not srDigestOutputFile Is Nothing Then
				srDigestOutputFile.Close()
			End If
			If Not srScrambledOutStream Is Nothing Then
				srScrambledOutStream.Close()
			End If
		End Try

		Return blnSuccess

	End Function

	Private Function PreScanProteinFileForAddnlRefsInDescription(ByVal strProteinInputFilePath As String, ByRef htAddnlRefMasterNames As System.Collections.Specialized.StringDictionary) As Boolean

		Dim objFastaFileReader As ProteinFileReader.FastaFileReader
		Dim udtProtein As udtProteinInfoType

		Dim intIndex As Integer

		Dim blnSuccess As Boolean = False
		Dim blnInputProteinFound As Boolean

		Try

			objFastaFileReader = New ProteinFileReader.FastaFileReader
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
		If Not blnSuccess Then Return False

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
						.AlternateNameCount = ExtractAlternateProteinNamesFromDescription(.Description, .AlternateNames)

						' Make sure each of the names in .AlternateNames() is in htAddnlRefMasterNames
						For intIndex = 0 To .AlternateNameCount - 1
							If Not htAddnlRefMasterNames.ContainsKey(.AlternateNames(intIndex).RefName) Then
								htAddnlRefMasterNames.Add(.AlternateNames(intIndex).RefName, Nothing)
							End If
						Next intIndex
					End With

					UpdateProgress(objFastaFileReader.PercentFileProcessed())

					If Me.AbortProcessing Then Exit Do
				End If
			Loop While blnInputProteinFound

			objFastaFileReader.CloseFile()

			blnSuccess = True

		Catch ex As Exception
			SetLocalErrorCode(eParseProteinFileErrorCodes.ErrorReadingInputFile)
		End Try

		Return blnSuccess

	End Function

	Private Function ValidateProteinName(ByVal strProteinName As String, ByVal intMaximumLength As Integer) As String
		Dim chSepChars As Char() = New Char() {" "c, ","c, ";"c, ":"c, "_"c, "-"c, "|"c, "/"c}

		If intMaximumLength < 1 Then intMaximumLength = 1

		If strProteinName Is Nothing Then
			strProteinName = String.Empty
		Else
			If strProteinName.Length > intMaximumLength Then
				' Truncate protein name to maximum length
				strProteinName = strProteinName.Substring(0, intMaximumLength)

				' Make sure the protein name doesn't end in a space, dash, underscore, semicolon, colon, etc.
				strProteinName.TrimEnd(chSepChars)
			End If
		End If

		Return strProteinName

	End Function

	Private Sub WriteFastaAppendToCache(ByRef srScrambledOutStream As System.IO.StreamWriter, ByRef udtResidueCache As udtScrambingResidueCacheType, ByVal strProteinNamePrefix As String, ByVal blnFlushResiduesToWrite As Boolean)

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

	Private Sub WriteFastaEmptyCache(ByRef srScrambledOutStream As System.IO.StreamWriter, ByRef udtResidueCache As udtScrambingResidueCacheType, ByVal strProteinNamePrefix As String, ByVal intSamplingPercentage As Integer)
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

	Private Sub WriteFastaProteinAndResidues(ByRef srScrambledOutStream As System.IO.StreamWriter, ByVal strHeaderline As String, ByVal strSequence As String)
		srScrambledOutStream.WriteLine(strHeaderline)
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

	Private Sub WriteScrambledFasta(ByRef srScrambledOutStream As System.IO.StreamWriter, ByRef objRandomNumberGenerator As Random, ByVal udtProtein As udtProteinInfoType, ByVal eScramblingMode As ProteinScramblingModeConstants, ByRef udtResidueCache As udtScrambingResidueCacheType)

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

	' Main processing function -- Calls ParseProteinFile
	Public Overloads Overrides Function ProcessFile(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strParameterFilePath As String, ByVal blnResetErrorCode As Boolean) As Boolean
		' Returns True if success, False if failure

		Dim ioFile As System.IO.FileInfo
		Dim strInputFilePathFull As String
		Dim strStatusMessage As String

		Dim blnSuccess As Boolean

		If blnResetErrorCode Then
			SetLocalErrorCode(eParseProteinFileErrorCodes.NoError)
		End If

		If Not LoadParameterFileSettings(strParameterFilePath) Then
			strStatusMessage = "Parameter file load error: " & strParameterFilePath
			If MyBase.ShowMessages Then MsgBox(strStatusMessage, MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Error")
			ShowErrorMessage(strStatusMessage)
			If MyBase.ErrorCode = clsProcessFilesBaseClass.eProcessFilesErrorCodes.NoError Then
				MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidParameterFile)
			End If
			Return False
		End If

		Try
			If strInputFilePath Is Nothing OrElse strInputFilePath.Length = 0 Then
				ShowErrorMessage("Input file name is empty")
				MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.InvalidInputFilePath)
			Else

				Console.WriteLine()
				ShowMessage("Parsing " & System.IO.Path.GetFileName(strInputFilePath))

				If Not CleanupFilePaths(strInputFilePath, strOutputFolderPath) Then
					MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.FilePathError)
				Else
					Try
						' Obtain the full path to the input file
						ioFile = New System.IO.FileInfo(strInputFilePath)
						strInputFilePathFull = ioFile.FullName

						blnSuccess = ParseProteinFile(strInputFilePathFull, strOutputFolderPath)

					Catch ex As Exception
						If MyBase.ShowMessages Then
							MsgBox("Error calling ParseProteinFile" & ControlChars.NewLine & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Error")
						Else
							Throw New System.Exception("Error calling ParseProteinFile", ex)
						End If
					End Try
				End If
			End If
		Catch ex As Exception
			If MyBase.ShowMessages Then
				MsgBox("Error in ProcessFile:" & ControlChars.NewLine & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Error")
			Else
				Throw New System.Exception("Error in ProcessFile", ex)
			End If
		End Try

		Return blnSuccess

	End Function

	Private Sub SetLocalErrorCode(ByVal eNewErrorCode As eParseProteinFileErrorCodes)
		SetLocalErrorCode(eNewErrorCode, False)
	End Sub

	Private Sub SetLocalErrorCode(ByVal eNewErrorCode As eParseProteinFileErrorCodes, ByVal blnLeaveExistingErrorCodeUnchanged As Boolean)

		If blnLeaveExistingErrorCodeUnchanged AndAlso mLocalErrorCode <> eParseProteinFileErrorCodes.NoError Then
			' An error code is already defined; do not change it
		Else
			mLocalErrorCode = eNewErrorCode

			If eNewErrorCode = eParseProteinFileErrorCodes.NoError Then
				If MyBase.ErrorCode = clsProcessFilesBaseClass.eProcessFilesErrorCodes.LocalizedError Then
					MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.NoError)
				End If
			Else
				MyBase.SetBaseClassErrorCode(clsProcessFilesBaseClass.eProcessFilesErrorCodes.LocalizedError)
			End If
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

		RaiseEvent SubtaskProgressChanged(strProgressStepDescription, sngPercentComplete)

	End Sub

	' IComparer class to allow comparison of additional protein references
	Private Class AddnlRefComparerClass
		Implements IComparer

		Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare

			Dim udtAddnlRef1, udtAddnlRef2 As udtAddnlRefType

			udtAddnlRef1 = CType(x, udtAddnlRefType)
			udtAddnlRef2 = CType(y, udtAddnlRefType)

			If udtAddnlRef1.RefName > udtAddnlRef2.RefName Then
				Return 1
			ElseIf udtAddnlRef1.RefName < udtAddnlRef2.RefName Then
				Return -1
			Else
				If udtAddnlRef1.RefAccession > udtAddnlRef2.RefAccession Then
					Return 1
				ElseIf udtAddnlRef1.RefAccession < udtAddnlRef2.RefAccession Then
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
		Public Property ReadonlyClass() As Boolean
			Get
				Return mReadonlyClass
			End Get
			Set(ByVal Value As Boolean)
				If Not mReadonlyClass Then
					mReadonlyClass = Value
				End If
			End Set
		End Property

		Public Property ProteinLineStartChar() As Char
			Get
				Return mProteinLineStartChar
			End Get
			Set(ByVal Value As Char)
				If Not Value = Nothing AndAlso Not mReadonlyClass Then
					mProteinLineStartChar = Value
				End If
			End Set
		End Property

		Public Property ProteinLineAccessionEndChar() As Char
			Get
				Return mProteinLineAccessionEndChar
			End Get
			Set(ByVal Value As Char)
				If Not Value = Nothing AndAlso Not mReadonlyClass Then
					mProteinLineAccessionEndChar = Value
				End If
			End Set
		End Property

		Public Property LookForAddnlRefInDescription() As Boolean
			Get
				Return mLookForAddnlRefInDescription
			End Get
			Set(ByVal Value As Boolean)
				If Not mReadonlyClass Then
					mLookForAddnlRefInDescription = Value
				End If
			End Set
		End Property

		Public Property AddnlRefSepChar() As Char
			Get
				Return mAddnlRefSepChar
			End Get
			Set(ByVal Value As Char)
				If Not Value = Nothing AndAlso Not mReadonlyClass Then
					mAddnlRefSepChar = Value
				End If
			End Set
		End Property

		Public Property AddnlRefAccessionSepChar() As Char
			Get
				Return mAddnlRefAccessionSepChar
			End Get
			Set(ByVal Value As Char)
				If Not Value = Nothing AndAlso Not mReadonlyClass Then
					mAddnlRefAccessionSepChar = Value
				End If
			End Set
		End Property
#End Region

	End Class

	Private Sub mInSilicoDigest_ErrorEvent(ByVal strMessage As String) Handles mInSilicoDigest.ErrorEvent
		ShowErrorMessage("Error in mInSilicoDigest: " & strMessage)
	End Sub

	Private Sub mInSilicoDigest_ProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single) Handles mInSilicoDigest.ProgressChanged
		UpdateSubtaskProgress(taskDescription, percentComplete)
	End Sub

	Private Sub mInSilicoDigest_ProgressComplete() Handles mInSilicoDigest.ProgressComplete
		UpdateSubtaskProgress(100)
	End Sub

	Private Sub mInSilicoDigest_ProgressReset() Handles mInSilicoDigest.ProgressReset
		' Don't do anything with this event
	End Sub

#If IncludePNNLNETRoutines Then
	Private Sub objSCXNETCalculator_ErrorEvent(Message As String) Handles objSCXNETCalculator.ErrorEvent
		ShowErrorMessage(Message)
	End Sub
#End If

End Class
