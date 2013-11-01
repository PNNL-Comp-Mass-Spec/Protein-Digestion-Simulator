Option Strict On

' This program can be used to read a fasta file or tab delimited file
' containing protein or peptide sequences, then output the data to a tab-delimited file
' It can optionally digest the input sequences using trypsin or partial trpysin rules,
'  and can add the predicted normalized elution time (NET) values for the peptides
' Additionally, it can calculate the number of uniquely identifiable peptides, using
'  only mass, or both mass and NET, with appropriate tolerances
'
' Example command line: /I:Yeast_2003-01-06.fasta /debug /d /p:ProteindigestionSettings.xml

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

Module modMain

	Public Const PROGRAM_DATE As String = "November 1, 2013"

	Private mInputFilePath As String
	Private mAssumeFastaFile As Boolean
	Private mCreateDigestedProteinOutputFile As Boolean
	Private mComputeProteinMass As Boolean

	Private mInputFileDelimiter As Char

	Private mOutputFolderPath As String				' Optional
	Private mParameterFilePath As String			' Optional
	Private mOutputFolderAlternatePath As String	' Optional

	Private mRecreateFolderHierarchyInAlternatePath As Boolean	' Optional

	Private mRecurseFolders As Boolean
	Private mRecurseFoldersMaxLevels As Integer

	Private mLogMessagesToFile As Boolean
	Private mLogFilePath As String = String.Empty
	Private mLogFolderPath As String = String.Empty

	Private mQuietMode As Boolean
	Private mShowDebugPrompts As Boolean

	Private WithEvents mParseProteinFile As clsParseProteinFile
	Private mLastProgressReportTime As System.DateTime
	Private mLastProgressReportValue As Integer

	'Private Sub TestPeptideCode()

	'    Dim objPeptide1 As New PeptideSequenceClass
	'    Dim objPeptide2 As New PeptideSequenceClass

	'    Dim intReturnResidueStart, intReturnResidueEnd As Integer

	'    With objPeptide1

	'        .ElementMode = PeptideSequenceClass.ElementModeConstants.AverageMass
	'        .SetSequence("GlyLeuPheArgGlyArgAspLysPheLeuPheArgGlyLeuPheArg", PeptideSequenceClass.NTerminusGroupConstants.Hydrogen, PeptideSequenceClass.CTerminusGroupConstants.Hydroxyl, True)
	'        Console.WriteLine("Peptide1 mass is " & .Mass)

	'        .SetSequenceOneLetterCharactersOnly("DSFKJDKFVDLS")
	'        Console.WriteLine("Peptide1 mass is " & .Mass)
	'    End With

	'End Sub

	'Private Sub TestProgress()
	'    Dim objProgressForm As New ProgressFormNET.frmProgress

	'    Dim dtStartTime As DateTime
	'    Dim dtLastUpdate As DateTime
	'    Dim intIndex As Integer

	'    objProgressForm.InitializeProgressForm("Testing form for 60 seconds", 0, 60, True)
	'    objProgressForm.Visible = True
	'    Windows.Forms.Application.DoEvents()

	'    dtStartTime = System.DateTime.UtcNow
	'    dtLastUpdate = dtStartTime.AddSeconds(-10)

	'    Do While System.DateTime.UtcNow.Subtract(dtStartTime).TotalSeconds <= 60

	'        If System.DateTime.UtcNow.Subtract(dtLastUpdate).TotalSeconds >= 0.5 Then
	'            dtLastUpdate = System.DateTime.UtcNow
	'            objProgressForm.UpdateProgressBar(Now.Subtract(dtStartTime).TotalSeconds)
	'            Windows.Forms.Application.DoEvents()
	'            If objProgressForm.KeyPressAbortProcess Then Exit Do
	'        End If
	'        System.Threading.Thread.Sleep(50)
	'    Loop

	'End Sub

	'Private Sub TestCollections()

	'    Dim intData() As Integer

	'    Dim htList As Hashtable
	'    Dim intIndex As Integer
	'    Dim objRandom As New Random

	'    ReDim intData(10000000)
	'    For intIndex = 0 To intData.Length - 1
	'        intData(intIndex) = objRandom.Next(0, 1000000)
	'    Next intIndex

	'    htList = New Hashtable
	'    For intIndex = 0 To intData.Length - 1
	'        If Not htList.ContainsKey(intData(intIndex)) Then
	'            htList.Add(intData(intIndex), "testing1234567890")
	'        End If
	'    Next intIndex

	'End Sub

	Public Function Main() As Integer
		' Returns 0 if no error, error code if an error

		Dim intReturnCode As Integer
		Dim objParseCommandLine As New clsParseCommandLine
		Dim blnProceed As Boolean

		intReturnCode = 0
		mInputFilePath = String.Empty
		mAssumeFastaFile = False
		mCreateDigestedProteinOutputFile = False
		mComputeProteinMass = False

		mInputFileDelimiter = ControlChars.Tab

		mOutputFolderPath = String.Empty
		mParameterFilePath = String.Empty

		mRecurseFolders = False
		mRecurseFoldersMaxLevels = 0

		mQuietMode = False
		mLogMessagesToFile = False
		mLogFilePath = String.Empty
		mLogFolderPath = String.Empty

		Try
			blnProceed = False
			If objParseCommandLine.ParseCommandLine Then
				If SetOptionsUsingCommandLineParameters(objParseCommandLine) Then blnProceed = True
			End If

			If (objParseCommandLine.ParameterCount + objParseCommandLine.NonSwitchParameterCount) = 0 AndAlso _
			   Not objParseCommandLine.NeedToShowHelp Then
				ShowGUI()
			ElseIf Not blnProceed OrElse objParseCommandLine.NeedToShowHelp OrElse mInputFilePath.Length = 0 Then
				ShowProgramHelp()
				intReturnCode = -1
			Else

				mParseProteinFile = New clsParseProteinFile
				mParseProteinFile.ShowMessages = Not mQuietMode
				mParseProteinFile.ShowDebugPrompts = mShowDebugPrompts

				' Note: the following settings will be overridden if mParameterFilePath points to a valid parameter file that has these settings defined
				With mParseProteinFile
					.AssumeFastaFile = mAssumeFastaFile
					.CreateProteinOutputFile = True
					.CreateDigestedProteinOutputFile = mCreateDigestedProteinOutputFile
					.ComputeProteinMass = mComputeProteinMass

					.InputFileDelimiter = mInputFileDelimiter
					.DelimitedFileFormatCode = ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence

					With .DigestionOptions
						.RemoveDuplicateSequences = False
					End With

					.LogMessagesToFile = mLogMessagesToFile
					.LogFilePath = mLogFilePath
					.LogFolderPath = mLogFolderPath
				End With

				If mRecurseFolders Then
					If mParseProteinFile.ProcessFilesAndRecurseFolders(mInputFilePath, mOutputFolderPath, mOutputFolderAlternatePath, mRecreateFolderHierarchyInAlternatePath, mParameterFilePath, mRecurseFoldersMaxLevels) Then
						intReturnCode = 0
					Else
						intReturnCode = mParseProteinFile.ErrorCode
					End If
				Else
					If mParseProteinFile.ProcessFilesWildcard(mInputFilePath, mOutputFolderPath, mParameterFilePath) Then
						intReturnCode = 0
					Else
						intReturnCode = mParseProteinFile.ErrorCode
						If intReturnCode <> 0 AndAlso Not mQuietMode Then
							System.Windows.Forms.MessageBox.Show("Error while processing: " & mParseProteinFile.GetErrorMessage(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
						End If
					End If
				End If

				DisplayProgressPercent(mLastProgressReportValue, True)
			End If

		Catch ex As Exception
			ShowErrorMessage("Error occurred in modMain->Main: " & System.Environment.NewLine & ex.Message)
			intReturnCode = -1
		End Try

		Return intReturnCode

	End Function

	Private Sub DisplayProgressPercent(ByVal intPercentComplete As Integer, ByVal blnAddCarriageReturn As Boolean)
		If blnAddCarriageReturn Then
			Console.WriteLine()
		End If
		If intPercentComplete > 100 Then intPercentComplete = 100
		Console.Write("Processing: " & intPercentComplete.ToString() & "% ")
		If blnAddCarriageReturn Then
			Console.WriteLine()
		End If
	End Sub

	Private Function GetAppVersion() As String
		Return clsProcessFilesBaseClass.GetAppVersion(PROGRAM_DATE)
	End Function

	Private Function SetOptionsUsingCommandLineParameters(ByVal objParseCommandLine As clsParseCommandLine) As Boolean
		' Returns True if no problems; otherwise, returns false

		Dim strValue As String = String.Empty
		Dim lstValidParameters As Generic.List(Of String) = New Generic.List(Of String) From {"I", "F", "D", "M", "AD", "O", "P", "S", "A", "R", "Q", "DEBUG"}
		Dim intValue As Integer

		Try
			' Make sure no invalid parameters are present
			If objParseCommandLine.InvalidParametersPresent(lstValidParameters) Then
				ShowErrorMessage("Invalid commmand line parameters",
					(From item In objParseCommandLine.InvalidParameters(lstValidParameters) Select "/" + item).ToList())
				Return False
			Else
				With objParseCommandLine
					' Query objParseCommandLine to see if various parameters are present
					If .RetrieveValueForParameter("I", strValue) Then
						mInputFilePath = strValue
					ElseIf .NonSwitchParameterCount > 0 Then
						mInputFilePath = .RetrieveNonSwitchParameter(0)
					End If

					If .RetrieveValueForParameter("F", strValue) Then mAssumeFastaFile = True
					If .RetrieveValueForParameter("D", strValue) Then mCreateDigestedProteinOutputFile = True
					If .RetrieveValueForParameter("M", strValue) Then mComputeProteinMass = True
					If .RetrieveValueForParameter("AD", strValue) Then mInputFileDelimiter = strValue.Chars(0)
					If .RetrieveValueForParameter("O", strValue) Then mOutputFolderPath = strValue
					If .RetrieveValueForParameter("P", strValue) Then mParameterFilePath = strValue

					If .RetrieveValueForParameter("S", strValue) Then
						mRecurseFolders = True
						If Integer.TryParse(strValue, intValue) Then
							mRecurseFoldersMaxLevels = intValue
						End If
					End If
					If .RetrieveValueForParameter("A", strValue) Then mOutputFolderAlternatePath = strValue
					If .RetrieveValueForParameter("R", strValue) Then mRecreateFolderHierarchyInAlternatePath = True

					'If .RetrieveValueForParameter("L", strValue) Then
					'	mLogMessagesToFile = True
					'	If Not String.IsNullOrEmpty(strValue) Then
					'		mLogFilePath = strValue
					'	End If
					'End If

					'If .RetrieveValueForParameter("LogFolder", strValue) Then
					'	mLogMessagesToFile = True
					'	If Not String.IsNullOrEmpty(strValue) Then
					'		mLogFolderPath = strValue
					'	End If
					'End If

					If .RetrieveValueForParameter("Q", strValue) Then mQuietMode = True
					If .RetrieveValueForParameter("DEBUG", strValue) Then mShowDebugPrompts = True
				End With

				Return True
			End If

		Catch ex As Exception
			ShowErrorMessage("Error parsing the command line parameters: " & System.Environment.NewLine & ex.Message)
		End Try

		Return False

	End Function

	Private Sub ShowErrorMessage(ByVal strMessage As String)
		Dim strSeparator As String = "------------------------------------------------------------------------------"

		Console.WriteLine()
		Console.WriteLine(strSeparator)
		Console.WriteLine(strMessage)
		Console.WriteLine(strSeparator)
		Console.WriteLine()

		WriteToErrorStream(strMessage)
	End Sub

	Private Sub ShowErrorMessage(ByVal strTitle As String, ByVal items As List(Of String))
		Dim strSeparator As String = "------------------------------------------------------------------------------"
		Dim strMessage As String

		Console.WriteLine()
		Console.WriteLine(strSeparator)
		Console.WriteLine(strTitle)
		strMessage = strTitle & ":"

		For Each item As String In items
			Console.WriteLine("   " + item)
			strMessage &= " " & item
		Next
		Console.WriteLine(strSeparator)
		Console.WriteLine()

		WriteToErrorStream(strMessage)
	End Sub

	Public Sub ShowGUI()
		Dim objFormMain As frmMain

		objFormMain = New frmMain

		objFormMain.ShowDialog()

		objFormMain = Nothing

	End Sub

	Private Sub ShowProgramHelp()

		Dim strSyntax As String

		Try

			strSyntax = String.Empty

			strSyntax &= "This program can be used to read a fasta file or tab delimited file containing protein or peptide sequences, then output "
			strSyntax &= "the data to a tab-delimited file.  It can optionally digest the input sequences using trypsin or partial trpysin rules, "
			strSyntax &= "and can add the predicted normalized elution time (NET) values for the peptides.Additionally, it can calculate the "
			strSyntax &= "number of uniquely identifiable peptides, using only mass, or both mass and NET, with appropriate tolerances." & ControlChars.NewLine & ControlChars.NewLine

			strSyntax &= "Program syntax:" & ControlChars.NewLine & System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location)
			strSyntax &= " /I:SourceFastaOrTextFile [/F] [/D] [/M] [/AD:AlternateDelimeter] [/O:OutputFolderPath] [/P:ParameterFilePath] [/S:[MaxLevel]] [/A:AlternateOutputFolderPath] [/R] [/Q]" & ControlChars.NewLine & ControlChars.NewLine

			strSyntax &= "The input file path can contain the wildcard character * and should point to a fasta file or tab-delimited text file." & ControlChars.NewLine
			strSyntax &= "Use /F to indicate that the input file is a fasta file.  If /F is not used, then the format will be assumed to be fasta only if the file contains .fasta in the name" & ControlChars.NewLine
			strSyntax &= "Use /D to indicate that an in-silico digestion of the proteins should be performed.  Digestion options must be specified in the Parameter file." & ControlChars.NewLine
			strSyntax &= "Use /M to indicate that protein mass should be computed." & ControlChars.NewLine
			strSyntax &= "Use /AD to specify a delimiter other than the Tab character (not applicable for fasta files)." & ControlChars.NewLine
			strSyntax &= "The output folder path is optional.  If omitted, the output files will be created in the same folder as the input file." & ControlChars.NewLine
			strSyntax &= "The parameter file path is optional.  If included, it should point to a valid XML parameter file." & ControlChars.NewLine
			strSyntax &= "Use /S to process all valid files in the input folder and subfolders. Include a number after /S (like /S:2) to limit the level of subfolders to examine." & ControlChars.NewLine
			strSyntax &= "When using /S, you can redirect the output of the results using /A." & ControlChars.NewLine
			strSyntax &= "When using /S, you can use /R to re-create the input folder hierarchy in the alternate output folder (if defined)." & ControlChars.NewLine
			strSyntax &= "The optional /Q switch will suppress all error messages." & ControlChars.NewLine & ControlChars.NewLine

			strSyntax &= "Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004" & ControlChars.NewLine
			strSyntax &= "Version: " & GetAppVersion() & ControlChars.NewLine & ControlChars.NewLine

			strSyntax &= "E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com" & ControlChars.NewLine
			strSyntax &= "Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov/" & ControlChars.NewLine & ControlChars.NewLine

			strSyntax &= frmDisclaimer.GetKangasPetritisDisclaimerText() & ControlChars.NewLine & ControlChars.NewLine


			If Not mQuietMode Then
				System.Windows.Forms.MessageBox.Show(strSyntax, "Syntax", MessageBoxButtons.OK, MessageBoxIcon.Information)
			End If

		Catch ex As Exception
			ShowErrorMessage("Error displaying the program syntax: " & ex.Message)
		End Try

	End Sub

	Private Sub WriteToErrorStream(strErrorMessage As String)
		Try
			Using swErrorStream As System.IO.StreamWriter = New System.IO.StreamWriter(Console.OpenStandardError())
				swErrorStream.WriteLine(strErrorMessage)
			End Using
		Catch ex As Exception
			' Ignore errors here
		End Try
	End Sub

	Private Sub mParseProteinFile_ProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single) Handles mParseProteinFile.ProgressChanged
		Const PERCENT_REPORT_INTERVAL As Integer = 25
		Const PROGRESS_DOT_INTERVAL_MSEC As Integer = 250

		If percentComplete >= mLastProgressReportValue Then
			If mLastProgressReportValue > 0 Then
				Console.WriteLine()
			End If
			DisplayProgressPercent(mLastProgressReportValue, False)
			mLastProgressReportValue += PERCENT_REPORT_INTERVAL
			mLastProgressReportTime = DateTime.UtcNow
		Else
			If DateTime.UtcNow.Subtract(mLastProgressReportTime).TotalMilliseconds > PROGRESS_DOT_INTERVAL_MSEC Then
				mLastProgressReportTime = DateTime.UtcNow
				Console.Write(".")
			End If
		End If
	End Sub

	Private Sub mProcessingClass_ProgressReset() Handles mParseProteinFile.ProgressReset
		mLastProgressReportTime = DateTime.UtcNow
		mLastProgressReportValue = 0
	End Sub
End Module
