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

    Public Const PROGRAM_DATE As String = "November 13, 2006"

    Private mInputFilePath As String
    Private mAssumeFastaFile As Boolean
    Private mCreateDigestedProteinOutputFile As Boolean
    Private mComputeProteinMass As Boolean

    Private mInputFileDelimiter As Char

    Private mOutputFolderName As String             ' Optional
    Private mParameterFilePath As String            ' Optional
    Private mOutputFolderAlternatePath As String    ' Optional

    Private mRecreateFolderHierarchyInAlternatePath As Boolean  ' Optional

    Private mRecurseFolders As Boolean
    Private mRecurseFoldersMaxLevels As Integer

    Private mQuietMode As Boolean
    Private mShowDebugPrompts As Boolean

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

    '    dtStartTime = Now
    '    dtLastUpdate = dtStartTime.AddSeconds(-10)

    '    Do While Now.Subtract(dtStartTime).TotalSeconds <= 60

    '        If Now.Subtract(dtLastUpdate).TotalSeconds >= 0.5 Then
    '            dtLastUpdate = Now
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

        Dim objParseProteinFile As clsParseProteinFile

        Dim intReturnCode As Integer
        Dim objParseCommandLine As New SharedVBNetRoutines.clsParseCommandLine
        Dim blnProceed As Boolean

        intReturnCode = 0
        mInputFilePath = ""
        mAssumeFastaFile = False
        mCreateDigestedProteinOutputFile = False
        mComputeProteinMass = False

        mInputFileDelimiter = ControlChars.Tab

        mOutputFolderName = ""
        mParameterFilePath = ""

        mRecurseFolders = False
        mRecurseFoldersMaxLevels = 0

        Try
            blnProceed = False
            If objParseCommandLine.ParseCommandLine Then
                If SetOptionsUsingCommandLineParameters(objParseCommandLine) Then blnProceed = True
            End If

            If objParseCommandLine.ParameterCount = 0 And Not objParseCommandLine.NeedToShowHelp Then
                ShowGUI()
            ElseIf Not blnProceed OrElse objParseCommandLine.NeedToShowHelp OrElse mInputFilePath.Length = 0 Then
                ShowProgramHelp()
                intReturnCode = -1
            Else

                objParseProteinFile = New clsParseProteinFile
                objParseProteinFile.ShowMessages = Not mQuietMode
                objParseProteinFile.ShowDebugPrompts = mShowDebugPrompts

                ' Note: the following settings will be overridden if mParameterFilePath points to a valid parameter file that has these settings defined
                With objParseProteinFile
                    .AssumeFastaFile = mAssumeFastaFile
                    .CreateProteinOutputFile = True
                    .CreateDigestedProteinOutputFile = mCreateDigestedProteinOutputFile
                    .ComputeProteinMass = mComputeProteinMass

                    .InputFileDelimiter = mInputFileDelimiter
                    .DelimitedFileFormatCode = ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence

                    With .DigestionOptions
                        .RemoveDuplicateSequences = False
                    End With
                End With

                If mRecurseFolders Then
                    If objParseProteinFile.ProcessFilesAndRecurseFolders(mInputFilePath, mOutputFolderName, mOutputFolderAlternatePath, mRecreateFolderHierarchyInAlternatePath, mParameterFilePath, mRecurseFoldersMaxLevels) Then
                        intReturnCode = 0
                    Else
                        intReturnCode = objParseProteinFile.ErrorCode
                    End If
                Else
                    If objParseProteinFile.ProcessFilesWildcard(mInputFilePath, mOutputFolderName, mParameterFilePath) Then
                        intReturnCode = 0
                    Else
                        intReturnCode = objParseProteinFile.ErrorCode
                        If intReturnCode <> 0 AndAlso Not mQuietMode Then
                            MsgBox("Error while processing: " & objParseProteinFile.GetErrorMessage(), MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, "Error")
                        End If
                    End If
                End If

            End If

        Catch ex As Exception
            If mQuietMode Then
                Throw ex
            Else
                MsgBox("Error occurred in modMain->Main: " & ControlChars.NewLine & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, "Error")
            End If
            intReturnCode = -1
        End Try

        Return intReturnCode

    End Function

    Private Function SetOptionsUsingCommandLineParameters(ByVal objParseCommandLine As SharedVBNetRoutines.clsParseCommandLine) As Boolean
        ' Returns True if no problems; otherwise, returns false

        Dim strValue As String
        Dim strValidParameters() As String = New String() {"I", "F", "D", "M", "AD", "O", "P", "S", "A", "R", "Q", "DEBUG"}

        Try
            ' Make sure no invalid parameters are present
            If objParseCommandLine.InvalidParametersPresent(strValidParameters) Then
                Return False
            Else
                With objParseCommandLine
                    ' Query objParseCommandLine to see if various parameters are present
                    If .RetrieveValueForParameter("I", strValue) Then mInputFilePath = strValue
                    If .RetrieveValueForParameter("F", strValue) Then mAssumeFastaFile = True
                    If .RetrieveValueForParameter("D", strValue) Then mCreateDigestedProteinOutputFile = True
                    If .RetrieveValueForParameter("M", strValue) Then mComputeProteinMass = True
                    If .RetrieveValueForParameter("AD", strValue) Then mInputFileDelimiter = strValue.Chars(0)
                    If .RetrieveValueForParameter("O", strValue) Then mOutputFolderName = strValue
                    If .RetrieveValueForParameter("P", strValue) Then mParameterFilePath = strValue

                    If .RetrieveValueForParameter("S", strValue) Then
                        mRecurseFolders = True
                        If IsNumeric(strValue) Then
                            mRecurseFoldersMaxLevels = CInt(strValue)
                        End If
                    End If
                    If .RetrieveValueForParameter("A", strValue) Then mOutputFolderAlternatePath = strValue
                    If .RetrieveValueForParameter("R", strValue) Then mRecreateFolderHierarchyInAlternatePath = True

                    If .RetrieveValueForParameter("Q", strValue) Then mQuietMode = True
                    If .RetrieveValueForParameter("DEBUG", strValue) Then mShowDebugPrompts = True
                End With

                Return True
            End If

        Catch ex As Exception
            If mQuietMode Then
                Throw New System.Exception("Error parsing the command line parameters", ex)
            Else
                MsgBox("Error parsing the command line parameters: " & ControlChars.NewLine & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, "Error")
            End If
        End Try

    End Function

    Public Sub ShowGUI()
        Dim objFormMain As frmMain

        objFormMain = New frmMain

        objFormMain.ShowDialog()

        objFormMain = Nothing

    End Sub

    Private Sub ShowProgramHelp()

        Dim strSyntax As String
        Dim ioPath As System.IO.Path

        Try
            strSyntax = String.Empty

            strSyntax &= "This program can be used to read a fasta file or tab delimited file containing protein or peptide sequences, then output "
            strSyntax &= "the data to a tab-delimited file.  It can optionally digest the input sequences using trypsin or partial trpysin rules, "
            strSyntax &= "and can add the predicted normalized elution time (NET) values for the peptides.Additionally, it can calculate the "
            strSyntax &= "number of uniquely identifiable peptides, using only mass, or both mass and NET, with appropriate tolerances." & ControlChars.NewLine & ControlChars.NewLine

            strSyntax &= "Program syntax:" & ControlChars.NewLine & ioPath.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location)
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
            strSyntax &= "Copyright 2005, Battelle Memorial Institute.  All Rights Reserved." & ControlChars.NewLine & ControlChars.NewLine

            strSyntax &= "This is version " & System.Windows.Forms.Application.ProductVersion & " (" & PROGRAM_DATE & ")" & ControlChars.NewLine & ControlChars.NewLine

            strSyntax &= "E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com" & ControlChars.NewLine
            strSyntax &= "Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/" & ControlChars.NewLine & ControlChars.NewLine

            strSyntax &= "Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.  "
            strSyntax &= "You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0" & ControlChars.NewLine & ControlChars.NewLine

            strSyntax &= "Notice: This computer software was prepared by Battelle Memorial Institute, "
            strSyntax &= "hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the "
            strSyntax &= "Department of Energy (DOE).  All rights in the computer software are reserved "
            strSyntax &= "by DOE on behalf of the United States Government and the Contractor as "
            strSyntax &= "provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY "
            strSyntax &= "WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS "
            strSyntax &= "SOFTWARE.  This notice including this sentence must appear on any copies of "
            strSyntax &= "this computer software." & ControlChars.NewLine

            If Not mQuietMode Then
                MsgBox(strSyntax, MsgBoxStyle.Information Or MsgBoxStyle.OKOnly, "Syntax")
            End If

        Catch ex As Exception
            If mQuietMode Then
                Throw New System.Exception("Error displaying the program syntax", ex)
            Else
                MsgBox("Error displaying the program syntax: " & ControlChars.NewLine & ex.Message, MsgBoxStyle.Exclamation Or MsgBoxStyle.OKOnly, "Error")
            End If
        End Try

    End Sub

End Module
