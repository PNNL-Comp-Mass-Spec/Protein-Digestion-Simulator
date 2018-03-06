Option Strict On

Imports System.IO
Imports System.Threading
Imports PRISM
Imports PRISM.FileProcessor
Imports ProteinFileReader

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

' E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com
' Website: http://omics.pnl.gov/ or http://www.sysbio.org/resources/staff/ or http://panomics.pnnl.gov/
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

    Public Const PROGRAM_DATE As String = "March 5, 2018"

    Private Declare Auto Function ShowWindow Lib "user32.dll" (hWnd As IntPtr, nCmdShow As Integer) As Boolean
    Private Declare Auto Function GetConsoleWindow Lib "kernel32.dll" () As IntPtr
    Private Const SW_HIDE As Integer = 0
    Private Const SW_SHOW As Integer = 5

    Private mInputFilePath As String
    Private mAssumeFastaFile As Boolean
    Private mCreateDigestedProteinOutputFile As Boolean
    Private mComputeProteinMass As Boolean

    Private mInputFileDelimiter As Char

    Private mOutputFolderPath As String             ' Optional
    Private mParameterFilePath As String            ' Optional
    Private mOutputFolderAlternatePath As String    ' Optional

    Private mRecreateFolderHierarchyInAlternatePath As Boolean  ' Optional

    Private mRecurseFolders As Boolean
    Private mRecurseFoldersMaxLevels As Integer

    Private mLogMessagesToFile As Boolean
    Private mLogFilePath As String = String.Empty
    Private mLogFolderPath As String = String.Empty

    Private mShowDebugPrompts As Boolean

    Private mParseProteinFile As clsParseProteinFile
    Private mLastProgressReportTime As DateTime
    Private mLastProgressReportValue As Integer

    Public Function Main() As Integer
        ' Returns 0 if no error, error code if an error

        Dim returnCode As Integer
        Dim objParseCommandLine As New clsParseCommandLine
        Dim blnProceed As Boolean

        mInputFilePath = String.Empty
        mAssumeFastaFile = False
        mCreateDigestedProteinOutputFile = False
        mComputeProteinMass = False

        mInputFileDelimiter = ControlChars.Tab

        mOutputFolderPath = String.Empty
        mParameterFilePath = String.Empty

        mRecurseFolders = False
        mRecurseFoldersMaxLevels = 0

        mLogMessagesToFile = False
        mLogFilePath = String.Empty
        mLogFolderPath = String.Empty

        Try
            blnProceed = False
            If objParseCommandLine.ParseCommandLine Then
                If SetOptionsUsingCommandLineParameters(objParseCommandLine) Then blnProceed = True
            End If

            If (objParseCommandLine.ParameterCount + objParseCommandLine.NonSwitchParameterCount) = 0 AndAlso
               Not objParseCommandLine.NeedToShowHelp Then
                ShowGUI()
                Return 0
            End If

            If Not blnProceed OrElse objParseCommandLine.NeedToShowHelp OrElse mInputFilePath.Length = 0 Then
                ShowProgramHelp()
                Return -1
            End If

            mParseProteinFile = New clsParseProteinFile() With {
                    .ShowDebugPrompts = mShowDebugPrompts
                }

            AddHandler mParseProteinFile.ProgressUpdate, AddressOf ProcessingClass_ProgressChanged
            AddHandler mParseProteinFile.ProgressReset, AddressOf ProcessingClass_ProgressReset

            ' Note: the following settings will be overridden if mParameterFilePath points to a valid parameter file that has these settings defined
            With mParseProteinFile
                .AssumeFastaFile = mAssumeFastaFile
                .CreateProteinOutputFile = True
                .CreateDigestedProteinOutputFile = mCreateDigestedProteinOutputFile
                .ComputeProteinMass = mComputeProteinMass

                .InputFileDelimiter = mInputFileDelimiter
                .DelimitedFileFormatCode = DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence

                With .DigestionOptions
                    .RemoveDuplicateSequences = False
                End With

                .LogMessagesToFile = mLogMessagesToFile
                .LogFilePath = mLogFilePath
                .LogFolderPath = mLogFolderPath
            End With

            If mRecurseFolders Then
                If mParseProteinFile.ProcessFilesAndRecurseFolders(mInputFilePath, mOutputFolderPath, mOutputFolderAlternatePath, mRecreateFolderHierarchyInAlternatePath, mParameterFilePath, mRecurseFoldersMaxLevels) Then
                    returnCode = 0
                Else
                    returnCode = mParseProteinFile.ErrorCode
                End If
            Else
                If mParseProteinFile.ProcessFilesWildcard(mInputFilePath, mOutputFolderPath, mParameterFilePath) Then
                    returnCode = 0
                Else
                    returnCode = mParseProteinFile.ErrorCode
                    If returnCode <> 0 Then
                        ShowErrorMessage("Error while processing: " & mParseProteinFile.GetErrorMessage())
                    End If
                End If
            End If

            DisplayProgressPercent(mLastProgressReportValue, True)


        Catch ex As Exception
            ShowErrorMessage("Error occurred in modMain->Main", ex)
            returnCode = -1
        End Try


        Thread.Sleep(1500)

        Return returnCode

    End Function

    Private Sub DisplayProgressPercent(intPercentComplete As Integer, blnAddCarriageReturn As Boolean)
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
        Return ProcessFilesBase.GetAppVersion(PROGRAM_DATE)
    End Function

    Private Function SetOptionsUsingCommandLineParameters(objParseCommandLine As clsParseCommandLine) As Boolean
        ' Returns True if no problems; otherwise, returns false

        Dim strValue As String = String.Empty
        Dim lstValidParameters = New List(Of String) From {"I", "F", "D", "M", "AD", "O", "P", "S", "A", "R", "DEBUG"}
        Dim intValue As Integer

        Try
            ' Make sure no invalid parameters are present
            If objParseCommandLine.InvalidParametersPresent(lstValidParameters) Then
                ConsoleMsgUtils.ShowErrors("Invalid commmand line parameters",
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

                    If .RetrieveValueForParameter("DEBUG", strValue) Then mShowDebugPrompts = True
                End With

                Return True
            End If

        Catch ex As Exception
            ShowErrorMessage("Error parsing the command line parameters: " & ex.Message, ex)
        End Try

        Return False

    End Function

    Private Sub ShowErrorMessage(message As String, Optional ex As Exception = Nothing)
        ConsoleMsgUtils.ShowError(message, ex)

        WriteToErrorStream(message)
    End Sub

    Public Sub ShowGUI()

        ' Hide the console
        Dim hWndConsole As IntPtr
        hWndConsole = GetConsoleWindow()
        ShowWindow(hWndConsole, SW_HIDE)

        Application.EnableVisualStyles()
        Application.DoEvents()

        Dim objFormMain = New frmMain()

        objFormMain.ShowDialog()

        ShowWindow(hWndConsole, SW_SHOW)
    End Sub

    Private Sub ShowProgramHelp()

        Try

            Console.WriteLine(WrapParagraph(
              "This program can be used to read a fasta file or tab delimited file containing protein or peptide sequences, then output " &
              "the data to a tab-delimited file.  It can optionally digest the input sequences using trypsin or partial trpysin rules, " &
              "and can add the predicted normalized elution time (NET) values for the peptides.Additionally, it can calculate the " &
              "number of uniquely identifiable peptides, using only mass, or both mass and NET, with appropriate tolerances."))
            Console.WriteLine()
            Console.WriteLine("Program syntax:")
            Console.WriteLine(WrapParagraph(
                Path.GetFileName(ProcessFilesOrFoldersBase.GetAppPath()) &
                " /I:SourceFastaOrTextFile [/F] [/D] [/M] [/AD:AlternateDelimeter] " &
                "[/O:OutputFolderPath] [/P:ParameterFilePath] [/S:[MaxLevel]] " &
                "[/A:AlternateOutputFolderPath] [/R] [/Q]"))
            Console.WriteLine()
            Console.WriteLine(WrapParagraph("The input file path can contain the wildcard character * and should point to a fasta file or tab-delimited text file."))
            Console.WriteLine()
            Console.WriteLine(WrapParagraph("Use /F to indicate that the input file is a fasta file.  If /F is not used, then the format will be assumed to be fasta only if the file contains .fasta in the name"))
            Console.WriteLine()
            Console.WriteLine(WrapParagraph("Use /D to indicate that an in-silico digestion of the proteins should be performed.  Digestion options must be specified in the Parameter file."))
            Console.WriteLine()
            Console.WriteLine(WrapParagraph("Use /M to indicate that protein mass should be computed."))
            Console.WriteLine()
            Console.WriteLine(WrapParagraph("Use /AD to specify a delimiter other than the Tab character (not applicable for fasta files)."))
            Console.WriteLine()
            Console.WriteLine(WrapParagraph("The output folder path is optional.  If omitted, the output files will be created in the same folder as the input file."))
            Console.WriteLine()
            Console.WriteLine(WrapParagraph("The parameter file path is optional.  If included, it should point to a valid XML parameter file."))
            Console.WriteLine()
            Console.WriteLine(WrapParagraph("Use /S to process all valid files in the input folder and subfolders. Include a number after /S (like /S:2) to limit the level of subfolders to examine."))
            Console.WriteLine(WrapParagraph("When using /S, you can redirect the output of the results using /A."))
            Console.WriteLine(WrapParagraph("When using /S, you can use /R to re-create the input folder hierarchy in the alternate output folder (if defined)."))
            Console.WriteLine()
            Console.WriteLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004")
            Console.WriteLine("Version: " & GetAppVersion())
            Console.WriteLine()
            Console.WriteLine("E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov")
            Console.WriteLine("Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/")
            Console.WriteLine()
            Console.WriteLine(WrapParagraph(frmDisclaimer.GetKangasPetritisDisclaimerText(False)))

            Thread.Sleep(2000)

        Catch ex As Exception
            ShowErrorMessage("Error displaying the program syntax", ex)
        End Try

    End Sub

    Private Function WrapParagraph(message As String, Optional wrapWidth As Integer = 80) As String
        Return CommandLineParser(Of clsParseCommandLine).WrapParagraph(message, wrapWidth)
    End Function

    Private Sub WriteToErrorStream(strErrorMessage As String)
        Try
            Using swErrorStream = New StreamWriter(Console.OpenStandardError())
                swErrorStream.WriteLine(strErrorMessage)
            End Using
        Catch ex As Exception
            ' Ignore errors here
        End Try
    End Sub

    Private Sub ProcessingClass_ProgressChanged(taskDescription As String, percentComplete As Single)
        Const PERCENT_REPORT_INTERVAL = 25
        Const PROGRESS_DOT_INTERVAL_MSEC = 250

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

    Private Sub ProcessingClass_ProgressReset()
        mLastProgressReportTime = DateTime.UtcNow
        mLastProgressReportValue = 0
    End Sub
End Module
