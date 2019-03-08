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

Imports System.ComponentModel
Imports System.IO
Imports NETPrediction
Imports PRISM
Imports ProteinFileReader
Imports SharedVBNetRoutines

Public Class frmMain

    Public Sub New()
        MyBase.New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        mDefaultFastaFileOptions = New clsParseProteinFile.FastaFileOptionsClass() With {
            .ReadonlyClass = True
            }

        objpICalculator = New clspICalculation()
        objNETCalculator = New ElutionTimePredictionKangas()
        objSCXNETCalculator = New SCXElutionTimePredictionKangas()

        InitializeControls()
    End Sub

#Region "Constants and Enums"

    Private Const XML_SETTINGS_FILE_NAME As String = "ProteinDigestionSimulatorOptions.xml"

    Private Const OUTPUT_FILE_SUFFIX As String = "_output.txt"                              ' Note that this const starts with an underscore
    Private Const PEAK_MATCHING_STATS_FILE_SUFFIX As String = "_PeakMatching.txt"      ' Note that this const starts with an underscore

    Private Const PM_THRESHOLDS_DATATABLE As String = "PeakMatchingThresholds"

    Private Const COL_NAME_MASS_TOLERANCE As String = "MassTolerance"
    Private Const COL_NAME_NET_TOLERANCE As String = "NETTolerance"
    Private Const COL_NAME_SLIC_MASS_STDEV As String = "SLiCMassStDev"
    Private Const COL_NAME_SLIC_NET_STDEV As String = "SLiCNETStDev"
    Private Const COL_NAME_PM_THRESHOLD_ROW_ID As String = "PMThresholdRowID"

    Private Const DEFAULT_SLIC_MASS_STDEV As Double = 3
    Private Const DEFAULT_SLIC_NET_STDEV As Double = 0.025

    Private Const PROGRESS_TAB_INDEX As Integer = 4

    Private Enum InputFileFormatConstants
        AutoDetermine = 0
        FastaFile = 1
        DelimitedText = 2
    End Enum

    Private Const PREDEFINED_PM_THRESHOLDS_COUNT As Integer = 5
    Private Enum PredefinedPMThresholdsConstants
        OneMassOneNET = 0
        OneMassThreeNET = 1
        OneNETThreeMass = 2
        ThreeMassThreeNET = 3
        FiveMassThreeNET = 4
    End Enum
#End Region

#Region "Structures"

    Private Structure udtPeakMatchingThresholdsType
        Public MassTolerance As Double
        Public NETTolerance As Double
    End Structure

    Private Structure udtPredefinedPMThresholdsType
        Public MassTolType As clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants
        Public Thresholds() As udtPeakMatchingThresholdsType
    End Structure

#End Region

#Region "Classwide Variables"

    ' The following is used to lookup the default symbols for Fasta files, and should thus be treated as ReadOnly
    Private ReadOnly mDefaultFastaFileOptions As clsParseProteinFile.FastaFileOptionsClass

    Private mPeakMatchingThresholdsDataset As DataSet
    Private mPredefinedPMThresholds() As udtPredefinedPMThresholdsType

    Private mWorking As Boolean
    Private mCustomValidationRulesFilePath As String

    Private ReadOnly objpICalculator As clspICalculation

    Private ReadOnly objNETCalculator As ElutionTimePredictionKangas

    Private ReadOnly objSCXNETCalculator As SCXElutionTimePredictionKangas

    Private mTabPageIndexSaved As Integer = 0

    Private mFastaValidationOptions As frmFastaValidation.udtFastaValidationOptionsType

    Private WithEvents mParseProteinFile As clsParseProteinFile

    Private WithEvents mProteinDigestionSimulator As clsProteinDigestionSimulator

    Private WithEvents mFastaValidation As frmFastaValidation

#End Region

#Region "Properties"
    Private Property SubtaskProgressIsVisible As Boolean
        Get
            Return lblSubtaskProgressDescription.Visible
        End Get
        Set
            lblSubtaskProgress.Visible = Value
            lblSubtaskProgressDescription.Visible = Value
        End Set
    End Property
#End Region

#Region "Procedures"

    Private Sub AbortProcessingNow()
        Try
            If Not mParseProteinFile Is Nothing Then
                mParseProteinFile.AbortProcessingNow()
            End If

            If Not mProteinDigestionSimulator Is Nothing Then
                mProteinDigestionSimulator.AbortProcessingNow()
            End If
        Catch ex As Exception
            ' Ignore errors here
        End Try
    End Sub

    Private Sub AddPMThresholdRow(dblMassThreshold As Double, dblNETThreshold As Double, Optional ByRef blnExistingRowFound As Boolean = False)
        AddPMThresholdRow(dblMassThreshold, dblNETThreshold, DEFAULT_SLIC_MASS_STDEV, DEFAULT_SLIC_NET_STDEV, blnExistingRowFound)
    End Sub

    Private Sub AddPMThresholdRow(dblMassThreshold As Double, dblNETThreshold As Double, dblSLiCMassStDev As Double, dblSLiCNETStDev As Double, Optional ByRef blnExistingRowFound As Boolean = False)
        Dim myDataRow As DataRow

        With mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE)

            For Each myDataRow In .Rows
                With myDataRow
                    If Math.Abs(CDbl(.Item(0)) - dblMassThreshold) < 0.000001 AndAlso Math.Abs(CDbl(.Item(1)) - dblNETThreshold) < 0.000001 Then
                        blnExistingRowFound = True
                        Exit For
                    End If
                End With
            Next myDataRow

            If Not blnExistingRowFound Then
                myDataRow = .NewRow
                myDataRow(0) = dblMassThreshold
                myDataRow(1) = dblNETThreshold
                myDataRow(2) = dblSLiCMassStDev
                myDataRow(3) = dblSLiCNETStDev
                .Rows.Add(myDataRow)
            End If
        End With

    End Sub

    Private Sub AutoDefineOutputFile()
        Try
            If txtProteinInputFilePath.Text.Length > 0 Then
                txtProteinOutputFilePath.Text = AutoDefineOutputFileWork(GetProteinInputFilePath())
            End If
        Catch ex As Exception
            ' Leave the textbox unchanged
        End Try
    End Sub

    Private Function AutoDefineOutputFileWork(inputFilePath As String) As String

        Dim inputFileName = Path.GetFileName(inputFilePath)
        Dim outputFileName As String

        If chkCreateFastaOutputFile.Enabled AndAlso chkCreateFastaOutputFile.Checked Then
            If Path.GetExtension(inputFileName).ToLower = ".fasta" Then
                outputFileName = Path.GetFileNameWithoutExtension(inputFileName) & "_new.fasta"
            Else
                outputFileName = Path.ChangeExtension(inputFileName, ".fasta")
            End If
        Else
            If Path.GetExtension(inputFileName).ToLower = ".txt" Then
                outputFileName = Path.GetFileNameWithoutExtension(inputFileName) & "_output.txt"
            Else
                outputFileName = Path.ChangeExtension(inputFileName, ".txt")
            End If
        End If

        If Not String.Equals(inputFilePath, txtProteinInputFilePath.Text) Then
            txtProteinInputFilePath.Text = inputFilePath
        End If

        Return Path.Combine(Path.GetDirectoryName(inputFilePath), outputFileName)

    End Function

    Private Sub AutoPopulatePMThresholds(udtPredefinedThresholds As udtPredefinedPMThresholdsType, blnConfirmReplaceExistingResults As Boolean)

        Dim intIndex As Integer

        If ClearPMThresholdsList(blnConfirmReplaceExistingResults) Then
            cboMassTolType.SelectedIndex = udtPredefinedThresholds.MassTolType

            For intIndex = 0 To udtPredefinedThresholds.Thresholds.Length - 1
                AddPMThresholdRow(udtPredefinedThresholds.Thresholds(intIndex).MassTolerance, udtPredefinedThresholds.Thresholds(intIndex).NETTolerance)
            Next intIndex

        End If

    End Sub

    Private Sub AutoPopulatePMThresholdsByID(ePredefinedPMThreshold As PredefinedPMThresholdsConstants, blnConfirmReplaceExistingResults As Boolean)

        Try
            AutoPopulatePMThresholds(mPredefinedPMThresholds(ePredefinedPMThreshold), blnConfirmReplaceExistingResults)
        Catch ex As Exception
            ShowErrorMessage("Error calling AutoPopulatePMThresholds in AutoPopulatePMThresholdsByID: " & ex.Message, "Error")
        End Try

    End Sub

    Private Function ClearPMThresholdsList(blnConfirmReplaceExistingResults As Boolean) As Boolean
        ' Returns true if the PM_THRESHOLDS_DATATABLE is empty or if it was cleared
        ' Returns false if the user is queried about clearing and they do not click Yes

        Dim eResult As DialogResult
        Dim blnSuccess As Boolean

        blnSuccess = False
        With mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE)
            If .Rows.Count > 0 Then
                If blnConfirmReplaceExistingResults Then
                    eResult = MessageBox.Show("Are you sure you want to clear the thresholds?", "Clear Thresholds", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
                End If

                If eResult = DialogResult.Yes OrElse Not blnConfirmReplaceExistingResults Then
                    .Rows.Clear()
                    blnSuccess = True
                End If
            Else
                blnSuccess = True
            End If
        End With

        Return blnSuccess
    End Function

    Private Sub ComputeSequencepI()

        If txtSequenceForpI.TextLength = 0 Then Exit Sub

        Dim strSequence = txtSequenceForpI.Text
        Dim sngpI As Single
        Dim sngHydrophobicity As Single
        Dim sngLCNET As Single
        Dim sngSCXNET As Single

        If Not objpICalculator Is Nothing Then
            If cboHydrophobicityMode.SelectedIndex >= 0 Then
                objpICalculator.HydrophobicityType = CType(cboHydrophobicityMode.SelectedIndex, clspICalculation.eHydrophobicityTypeConstants)
            End If

            objpICalculator.ReportMaximumpI = chkMaxpIModeEnabled.Checked
            objpICalculator.SequenceWidthToExamineForMaximumpI = LookupMaxpISequenceLength()


            sngpI = objpICalculator.CalculateSequencepI(strSequence)
            sngHydrophobicity = objpICalculator.CalculateSequenceHydrophobicity(strSequence)
            ' Could compute charge state: objpICalculator.CalculateSequenceChargeState(strSequence, sngpI)
        End If

        If Not objNETCalculator Is Nothing Then
            ' Compute the LC-based normalized elution time
            sngLCNET = objNETCalculator.GetElutionTime(strSequence)
        End If

        If Not objSCXNETCalculator Is Nothing Then
            ' Compute the SCX-based normalized elution time
            sngSCXNET = objSCXNETCalculator.GetElutionTime(strSequence)
        End If

        Dim strMessage = "pI = " & sngpI.ToString & ControlChars.NewLine &
                         "Hydrophobicity = " & sngHydrophobicity.ToString & ControlChars.NewLine &
                         "Predicted LC NET = " & sngLCNET.ToString("0.000") & ControlChars.NewLine &
                         "Predicted SCX NET = " & sngSCXNET.ToString("0.000")
        ' "Predicted charge state = " & ControlChars.NewLine & intCharge.ToString & " at pH = " & sngpI.ToString

        txtpIStats.Text = strMessage
    End Sub

    Private Function ConfirmFilePaths() As Boolean
        If txtProteinInputFilePath.TextLength = 0 Then
            ShowErrorMessage("Please define an input file path", "Missing Value")
            txtProteinInputFilePath.Focus()
            Return False
        ElseIf txtProteinOutputFilePath.TextLength = 0 Then
            ShowErrorMessage("Please define an output file path", "Missing Value")
            txtProteinOutputFilePath.Focus()
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub DefineDefaultPMThresholds()

        Dim intIndex As Integer
        Dim intMassIndex, intNETIndex As Integer

        Dim dblNETValues() As Double
        Dim dblMassvalues() As Double

        ReDim mPredefinedPMThresholds(PREDEFINED_PM_THRESHOLDS_COUNT - 1)

        ' All of the predefined thresholds have mass tolerances in units of PPM
        For intIndex = 0 To PREDEFINED_PM_THRESHOLDS_COUNT - 1
            With mPredefinedPMThresholds(intIndex)
                .MassTolType = clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.PPM
                ReDim .Thresholds(-1)
            End With
        Next intIndex

        ReDim dblNETValues(2)
        dblNETValues(0) = 0.01
        dblNETValues(1) = 0.05
        dblNETValues(2) = 100

        ReDim dblMassvalues(4)
        dblMassvalues(0) = 0.5
        dblMassvalues(1) = 1
        dblMassvalues(2) = 5
        dblMassvalues(3) = 10
        dblMassvalues(4) = 50

        ' OneMassOneNET
        DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(0), 5, 0.05)

        ' OneMassThreeNET
        For intNETIndex = 0 To dblNETValues.Length - 1
            DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(1), 5, dblNETValues(intNETIndex))
        Next intNETIndex

        ' ThreeMassOneNET
        For intMassIndex = 0 To 2
            DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(2), dblMassvalues(intMassIndex), 0.05)
        Next intMassIndex

        ' ThreeMassThreeNET
        For intNETIndex = 0 To dblNETValues.Length - 1
            For intMassIndex = 0 To 2
                DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(3), dblMassvalues(intMassIndex), dblNETValues(intNETIndex))
            Next intMassIndex
        Next intNETIndex

        ' FiveMassThreeNET
        For intNETIndex = 0 To dblNETValues.Length - 1
            For intMassIndex = 0 To dblMassvalues.Length - 1
                DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(4), dblMassvalues(intMassIndex), dblNETValues(intNETIndex))
            Next intMassIndex
        Next intNETIndex

    End Sub

    Private Sub DefineDefaultPMThresholdAppendItem(ByRef udtPMThreshold As udtPredefinedPMThresholdsType, dblMassTolerance As Double, dblNETTolerance As Double)
        With udtPMThreshold
            ReDim Preserve .Thresholds(.Thresholds.Length)
            With .Thresholds(.Thresholds.Length - 1)
                .MassTolerance = dblMassTolerance
                .NETTolerance = dblNETTolerance
            End With
        End With
    End Sub

    Private Sub EnableDisableControls()
        Dim blnEnableDelimitedFileOptions As Boolean
        Dim blnEnableDigestionOptions As Boolean
        Dim blnAllowSqlServerCaching As Boolean

        Dim inputFilePath = GetProteinInputFilePath()
        Dim sourceIsFasta = Path.GetFileName(inputFilePath).ToLower().EndsWith(".fasta")

        If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.DelimitedText Then
            blnEnableDelimitedFileOptions = True
        ElseIf cboInputFileFormat.SelectedIndex = InputFileFormatConstants.FastaFile OrElse
           txtProteinInputFilePath.TextLength = 0 OrElse
           sourceIsFasta Then
            ' Fasta file (or blank)
            blnEnableDelimitedFileOptions = False
        Else
            blnEnableDelimitedFileOptions = True
        End If

        cboInputFileColumnDelimiter.Enabled = blnEnableDelimitedFileOptions
        lblInputFileColumnDelimiter.Enabled = blnEnableDelimitedFileOptions
        chkAssumeInputFileIsDigested.Enabled = blnEnableDelimitedFileOptions

        txtInputFileColumnDelimiter.Enabled = (cboInputFileColumnDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Other) And blnEnableDelimitedFileOptions

        blnEnableDigestionOptions = chkDigestProteins.Checked
        If blnEnableDigestionOptions Then
            cmdParseInputFile.Text = "&Parse and Digest"
        Else
            cmdParseInputFile.Text = "&Parse File"
        End If

        If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.FastaFile OrElse sourceIsFasta Then
            cmdValidateFastaFile.Enabled = True
        Else
            cmdValidateFastaFile.Enabled = False
        End If

        chkCreateFastaOutputFile.Enabled = Not blnEnableDigestionOptions

        chkComputeSequenceHashIgnoreILDiff.Enabled = chkComputeSequenceHashValues.Checked

        fraDigestionOptions.Enabled = blnEnableDigestionOptions
        chkIncludePrefixAndSuffixResidues.Enabled = blnEnableDigestionOptions

        txtOutputFileFieldDelimiter.Enabled = (cboOutputFileFieldDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Other)
        txtRefEndChar.Enabled = (cboRefEndChar.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Other)

        blnEnableDelimitedFileOptions = chkLookForAddnlRefInDescription.Checked
        txtAddnlRefSepChar.Enabled = blnEnableDelimitedFileOptions
        txtAddnlRefAccessionSepChar.Enabled = blnEnableDelimitedFileOptions

        txtUniquenessBinStartMass.Enabled = Not chkAutoComputeRangeForBinning.Checked
        txtUniquenessBinEndMass.Enabled = txtUniquenessBinStartMass.Enabled

        blnAllowSqlServerCaching = chkAllowSqlServerCaching.Checked
        chkUseSqlServerDBToCacheData.Enabled = blnAllowSqlServerCaching

        txtSqlServerDatabase.Enabled = chkUseSqlServerDBToCacheData.Checked And blnAllowSqlServerCaching
        txtSqlServerName.Enabled = txtSqlServerDatabase.Enabled
        chkSqlServerUseIntegratedSecurity.Enabled = txtSqlServerDatabase.Enabled

        chkSqlServerUseExistingData.Enabled = chkSqlServerUseIntegratedSecurity.Checked And blnAllowSqlServerCaching

        txtSqlServerUsername.Enabled = chkUseSqlServerDBToCacheData.Checked And Not chkSqlServerUseIntegratedSecurity.Checked And blnAllowSqlServerCaching
        txtSqlServerPassword.Enabled = txtSqlServerUsername.Enabled

        If cboProteinReversalOptions.SelectedIndex <= 0 Then
            txtProteinReversalSamplingPercentage.Enabled = False
        Else
            txtProteinReversalSamplingPercentage.Enabled = True
        End If

        If cboProteinReversalOptions.SelectedIndex = 2 Then
            txtProteinScramblingLoopCount.Enabled = True
        Else
            txtProteinScramblingLoopCount.Enabled = False
        End If

        txtMinimumSLiCScore.Enabled = chkUseSLiCScoreForUniqueness.Checked
        optUseEllipseSearchRegion.Enabled = Not chkUseSLiCScoreForUniqueness.Checked
        optUseRectangleSearchRegion.Enabled = optUseEllipseSearchRegion.Enabled

        txtMaxpISequenceLength.Enabled = chkMaxpIModeEnabled.Checked

        txtDigestProteinsMinimumpI.Enabled = blnEnableDigestionOptions And chkComputepIandNET.Checked
        txtDigestProteinsMaximumpI.Enabled = blnEnableDigestionOptions And chkComputepIandNET.Checked

    End Sub

    Private Function FormatPercentComplete(sngPercentComplete As Single) As String
        Return sngPercentComplete.ToString("0.0") & "% complete"
    End Function

    Private Sub GenerateUniquenessStats()

        Dim strLogFilePath As String
        Dim strOutputFilePath As String
        Dim intCharLoc As Integer

        Dim intBinStartMass, intBinEndMass As Integer

        Dim myDataRow As DataRow

        Dim blnClearExisting As Boolean
        Dim blnAutoDefineSLiCScoreThresholds As Boolean
        Dim eMassToleranceType As clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants

        Dim blnSuccess As Boolean
        Dim blnError As Boolean

        If Not mWorking AndAlso ConfirmFilePaths() Then
            Try

                If mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE).Rows.Count = 0 Then
                    ShowErrorMessage("Please define one or more peak matching thresholds before proceeding.", "Error")
                    Exit Try
                End If

                mProteinDigestionSimulator = New clsProteinDigestionSimulator()
                If chkEnableLogging.Checked Then
                    mProteinDigestionSimulator.LogMessagesToFile = True

                    Dim appFolderPath = FileProcessor.ProcessFilesBase.GetAppDataDirectoryPath("ProteinDigestionSimulator")
                    strLogFilePath = Path.Combine(appFolderPath, "ProteinDigestionSimulatorLog.txt")
                    mProteinDigestionSimulator.LogFilePath = strLogFilePath
                End If

                blnSuccess = InitializeProteinFileParserGeneralOptions(mProteinDigestionSimulator.mProteinFileParser)
                If Not blnSuccess Then Exit Try

                strOutputFilePath = txtProteinOutputFilePath.Text

                If Not Path.IsPathRooted(strOutputFilePath) Then
                    strOutputFilePath = Path.Combine(GetMyDocsFolderPath(), strOutputFilePath)
                End If

                If Directory.Exists(strOutputFilePath) Then
                    ' strOutputFilePath points to a folder and not a file
                    strOutputFilePath = Path.Combine(strOutputFilePath, Path.GetFileNameWithoutExtension(GetProteinInputFilePath()) & PEAK_MATCHING_STATS_FILE_SUFFIX)
                Else
                    ' Replace _output.txt" in strOutputFilePath with PEAK_MATCHING_STATS_FILE_SUFFIX
                    intCharLoc = strOutputFilePath.IndexOf(OUTPUT_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase)
                    If intCharLoc > 0 Then
                        strOutputFilePath = strOutputFilePath.Substring(0, intCharLoc) & PEAK_MATCHING_STATS_FILE_SUFFIX
                    Else
                        strOutputFilePath = Path.Combine(Path.GetDirectoryName(strOutputFilePath), Path.GetFileNameWithoutExtension(strOutputFilePath) & PEAK_MATCHING_STATS_FILE_SUFFIX)
                    End If
                End If

                ' Check input file size and possibly warn user to enable/disable Sql Server DB Usage
                If chkAllowSqlServerCaching.Checked Then
                    If Not ValidateSqlServerCachingOptionsForInputFile(GetProteinInputFilePath(), chkAssumeInputFileIsDigested.Checked, mProteinDigestionSimulator.mProteinFileParser) Then
                        Exit Try
                    End If
                End If

                With mProteinDigestionSimulator

                    If cboMassTolType.SelectedIndex >= 0 Then
                        eMassToleranceType = CType(cboMassTolType.SelectedIndex, clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants)
                    End If
                    blnAutoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked

                    blnClearExisting = True
                    For Each myDataRow In mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE).Rows
                        If blnAutoDefineSLiCScoreThresholds Then
                            .AddSearchThresholdLevel(eMassToleranceType, CDbl(myDataRow.Item(0)), CDbl(myDataRow.Item(1)), blnClearExisting)
                        Else
                            .AddSearchThresholdLevel(eMassToleranceType, CDbl(myDataRow.Item(0)), CDbl(myDataRow.Item(1)), False, CDbl(myDataRow.Item(2)), CDbl(myDataRow.Item(3)), True, blnClearExisting)
                        End If

                        blnClearExisting = False
                    Next myDataRow

                    .DigestSequences = Not chkAssumeInputFileIsDigested.Checked
                    .CysPeptidesOnly = chkCysPeptidesOnly.Checked
                    If cboElementMassMode.SelectedIndex >= 0 Then
                        .ElementMassMode = CType(cboElementMassMode.SelectedIndex, PeptideSequenceClass.ElementModeConstants)
                    End If

                    .AutoDetermineMassRangeForBinning = chkAutoComputeRangeForBinning.Checked

                    .PeptideUniquenessMassBinSizeForBinning = ParseTextboxValueInt(txtUniquenessBinWidth, lblUniquenessBinWidth.Text & " must be an integer value", blnError)
                    If blnError Then Exit Try

                    If Not .AutoDetermineMassRangeForBinning Then
                        intBinStartMass = ParseTextboxValueInt(txtUniquenessBinStartMass, "Uniqueness binning start mass must be an integer value", blnError)
                        If blnError Then Exit Try

                        intBinEndMass = ParseTextboxValueInt(txtUniquenessBinEndMass, "Uniqueness binning end mass must be an integer value", blnError)
                        If blnError Then Exit Try

                        If Not .SetPeptideUniquenessMassRangeForBinning(intBinStartMass, intBinEndMass) Then
                            .AutoDetermineMassRangeForBinning = True
                        End If
                    End If

                    .MinimumSLiCScoreToBeConsideredUnique = ParseTextboxValueSng(txtMinimumSLiCScore, lblMinimumSLiCScore.Text & " must be a value", blnError)
                    If blnError Then Exit Try

                    .MaxPeakMatchingResultsPerFeatureToSave = ParseTextboxValueInt(txtMaxPeakMatchingResultsPerFeatureToSave, lblMaxPeakMatchingResultsPerFeatureToSave.Text & " must be an integer value", blnError)
                    If blnError Then Exit Try

                    .SavePeakMatchingResults = chkExportPeakMatchingResults.Checked
                    .UseSLiCScoreForUniqueness = chkUseSLiCScoreForUniqueness.Checked
                    .UseEllipseSearchRegion = optUseEllipseSearchRegion.Checked             ' Only applicable if .UseSLiCScoreForUniqueness = True

                    Cursor.Current = Cursors.WaitCursor
                    mWorking = True
                    cmdGenerateUniquenessStats.Enabled = False

                    ResetProgress(True)
                    SwitchToProgressTab()

                    blnSuccess = .GenerateUniquenessStats(GetProteinInputFilePath(), Path.GetDirectoryName(strOutputFilePath), Path.GetFileNameWithoutExtension(strOutputFilePath))

                    Cursor.Current = Cursors.Default

                    If blnSuccess Then
                        MessageBox.Show("Uniqueness stats calculation complete ", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        SwitchFromProgressTab()
                    Else
                        ShowErrorMessage("Unable to Generate Uniqueness Stats: " & .GetErrorMessage, "Error")
                    End If
                End With

            Catch ex As Exception
                ShowErrorMessage("Error in frmMain->GenerateUniquenessStats: " & ex.Message, "Error")
            Finally
                mWorking = False
                cmdGenerateUniquenessStats.Enabled = True
                mProteinDigestionSimulator = Nothing
            End Try
        End If

    End Sub

    Private Function GetMyDocsFolderPath() As String
        Return Environment.GetFolderPath(Environment.SpecialFolder.Personal)
    End Function

    Private Function GetProteinInputFilePath() As String
        Return txtProteinInputFilePath.Text.Trim({""""c, " "c})
    End Function

    Private Function GetSettingsFilePath() As String
        Return FileProcessor.ProcessFilesBase.GetSettingsFilePathLocal("ProteinDigestionSimulator", XML_SETTINGS_FILE_NAME)
    End Function

    Private Sub IniFileLoadOptions()

        Const OptionsSection As String = clsParseProteinFile.XML_SECTION_OPTIONS
        Const FASTAOptions As String = clsParseProteinFile.XML_SECTION_FASTA_OPTIONS
        Const ProcessingOptions As String = clsParseProteinFile.XML_SECTION_PROCESSING_OPTIONS
        Const DigestionOptions As String = clsParseProteinFile.XML_SECTION_DIGESTION_OPTIONS
        Const UniquenessStatsOptions As String = clsParseProteinFile.XML_SECTION_UNIQUENESS_STATS_OPTIONS
        Const PMOptions As String = clsProteinDigestionSimulator.XML_SECTION_PEAK_MATCHING_OPTIONS

        Const MAX_AUTO_WINDOW_HEIGHT = 775

        Dim objXmlFile As New XmlSettingsFileAccessor()

        Dim blnAutoDefineSLiCScoreThresholds As Boolean
        Dim valueNotPresent As Boolean
        Dim blnRadioButtonChecked As Boolean

        Dim intIndex As Integer
        Dim intWindowHeight As Integer

        Dim strThresholdData As String
        Dim strThresholds() As String
        Dim strThresholdDetails() As String

        Dim ColumnDelimiters = New Char() {ControlChars.Tab, ","c}

        ResetToDefaults(False)

        Try

            ' Pass False to .LoadSettings() here to turn off case sensitive matching
            Dim strSettingsFilePath = GetSettingsFilePath()
            objXmlFile.LoadSettings(strSettingsFilePath, False)
            PRISM.FileProcessor.ProcessFilesBase.CreateSettingsFileIfMissing(strSettingsFilePath)

            If Not File.Exists(strSettingsFilePath) Then
                ShowErrorMessage("Parameter file not Found: " & strSettingsFilePath)
                Exit Sub
            End If

            Try
                txtProteinInputFilePath.Text = objXmlFile.GetParam(OptionsSection, "InputFilePath", GetProteinInputFilePath())

                cboInputFileFormat.SelectedIndex = objXmlFile.GetParam(OptionsSection, "InputFileFormatIndex", cboInputFileFormat.SelectedIndex)
                cboInputFileColumnDelimiter.SelectedIndex = objXmlFile.GetParam(OptionsSection, "InputFileColumnDelimiterIndex", cboInputFileColumnDelimiter.SelectedIndex)
                txtInputFileColumnDelimiter.Text = objXmlFile.GetParam(OptionsSection, "InputFileColumnDelimiter", txtInputFileColumnDelimiter.Text)

                cboInputFileColumnOrdering.SelectedIndex = objXmlFile.GetParam(OptionsSection, "InputFileColumnOrdering", cboInputFileColumnOrdering.SelectedIndex)

                cboOutputFileFieldDelimiter.SelectedIndex = xmlSettings.GetParam(OptionsSection, "OutputFileFieldDelimiterIndex", cboOutputFileFieldDelimiter.SelectedIndex)
                txtOutputFileFieldDelimiter.Text = xmlSettings.GetParam(OptionsSection, "OutputFileFieldDelimiter", txtOutputFileFieldDelimiter.Text)

                chkIncludePrefixAndSuffixResidues.Checked = objXmlFile.GetParam(OptionsSection, "IncludePrefixAndSuffixResidues", chkIncludePrefixAndSuffixResidues.Checked)
                chkEnableLogging.Checked = objXmlFile.GetParam(OptionsSection, "EnableLogging", chkEnableLogging.Checked)

                mCustomValidationRulesFilePath = objXmlFile.GetParam(OptionsSection, "CustomValidationRulesFilePath", String.Empty)

                Me.Width = objXmlFile.GetParam(OptionsSection, "WindowWidth", Me.Width)
                intWindowHeight = objXmlFile.GetParam(OptionsSection, "WindowHeight", Me.Height)
                If intWindowHeight > MAX_AUTO_WINDOW_HEIGHT Then
                    intWindowHeight = MAX_AUTO_WINDOW_HEIGHT
                End If
                Me.Height = intWindowHeight

                txtRefStartChar.Text = objXmlFile.GetParam(FASTAOptions, "RefStartChar", txtRefStartChar.Text)
                cboRefEndChar.SelectedIndex = objXmlFile.GetParam(FASTAOptions, "RefEndCharIndex", cboRefEndChar.SelectedIndex)
                txtRefEndChar.Text = objXmlFile.GetParam(FASTAOptions, "RefEndChar", txtRefEndChar.Text)

                chkLookForAddnlRefInDescription.Checked = objXmlFile.GetParam(FASTAOptions, "LookForAddnlRefInDescription", chkLookForAddnlRefInDescription.Checked)
                txtAddnlRefSepChar.Text = objXmlFile.GetParam(FASTAOptions, "AddnlRefSepChar", txtAddnlRefSepChar.Text)
                txtAddnlRefAccessionSepChar.Text = objXmlFile.GetParam(FASTAOptions, "AddnlRefAccessionSepChar", txtAddnlRefAccessionSepChar.Text)

                chkExcludeProteinSequence.Checked = objXmlFile.GetParam(ProcessingOptions, "ExcludeProteinSequence", chkExcludeProteinSequence.Checked)
                chkComputeProteinMass.Checked = objXmlFile.GetParam(ProcessingOptions, "ComputeProteinMass", chkComputeProteinMass.Checked)
                cboElementMassMode.SelectedIndex = objXmlFile.GetParam(ProcessingOptions, "ElementMassMode", cboElementMassMode.SelectedIndex)

                ' In the GUI, chkComputepI controls both computing pI and SCX
                ' Running from the command line, you can toggle those options separately using "ComputepI" and "ComputeSCX"
                chkComputepIandNET.Checked = objXmlFile.GetParam(ProcessingOptions, "ComputepI", chkComputepIandNET.Checked)
                chkIncludeXResidues.Checked = objXmlFile.GetParam(ProcessingOptions, "IncludeXResidues", chkIncludeXResidues.Checked)

                chkComputeSequenceHashValues.Checked = objXmlFile.GetParam(ProcessingOptions, "ComputeSequenceHashValues", chkComputeSequenceHashValues.Checked)
                chkComputeSequenceHashIgnoreILDiff.Checked = objXmlFile.GetParam(ProcessingOptions, "ComputeSequenceHashIgnoreILDiff", chkComputeSequenceHashIgnoreILDiff.Checked)
                chkTruncateProteinDescription.Checked = objXmlFile.GetParam(ProcessingOptions, "TruncateProteinDescription", chkTruncateProteinDescription.Checked)
                chkExcludeProteinDescription.Checked = objXmlFile.GetParam(ProcessingOptions, "ExcludeProteinDescription", chkExcludeProteinDescription.Checked)

                chkDigestProteins.Checked = objXmlFile.GetParam(ProcessingOptions, "DigestProteins", chkDigestProteins.Checked)
                cboProteinReversalOptions.SelectedIndex = objXmlFile.GetParam(ProcessingOptions, "ProteinReversalIndex", cboProteinReversalOptions.SelectedIndex)
                txtProteinScramblingLoopCount.Text = objXmlFile.GetParam(ProcessingOptions, "ProteinScramblingLoopCount", txtProteinScramblingLoopCount.Text)

                Try
                    cboHydrophobicityMode.SelectedIndex = objXmlFile.GetParam(ProcessingOptions, "HydrophobicityMode", cboHydrophobicityMode.SelectedIndex)
                Catch ex As Exception
                    ' Ignore errors setting the selected index
                End Try
                chkMaxpIModeEnabled.Checked = objXmlFile.GetParam(ProcessingOptions, "MaxpIModeEnabled", chkMaxpIModeEnabled.Checked)
                txtMaxpISequenceLength.Text = objXmlFile.GetParam(ProcessingOptions, "MaxpISequenceLength", LookupMaxpISequenceLength).ToString

                cboCleavageRuleType.SelectedIndex = objXmlFile.GetParam(DigestionOptions, "CleavageRuleTypeIndex", cboCleavageRuleType.SelectedIndex)
                chkIncludeDuplicateSequences.Checked = objXmlFile.GetParam(DigestionOptions, "IncludeDuplicateSequences", chkIncludeDuplicateSequences.Checked)
                chkCysPeptidesOnly.Checked = objXmlFile.GetParam(DigestionOptions, "CysPeptidesOnly", chkCysPeptidesOnly.Checked)

                txtDigestProteinsMinimumMass.Text = objXmlFile.GetParam(DigestionOptions, "DigestProteinsMinimumMass", txtDigestProteinsMinimumMass.Text)
                txtDigestProteinsMaximumMass.Text = objXmlFile.GetParam(DigestionOptions, "DigestProteinsMaximumMass", txtDigestProteinsMaximumMass.Text)
                txtDigestProteinsMinimumResidueCount.Text = objXmlFile.GetParam(DigestionOptions, "DigestProteinsMinimumResidueCount", txtDigestProteinsMinimumResidueCount.Text)
                txtDigestProteinsMaximumMissedCleavages.Text = objXmlFile.GetParam(DigestionOptions, "DigestProteinsMaximumMissedCleavages", txtDigestProteinsMaximumMissedCleavages.Text)

                txtDigestProteinsMinimumpI.Text = objXmlFile.GetParam(DigestionOptions, "DigestProteinsMinimumpI", txtDigestProteinsMinimumpI.Text)
                txtDigestProteinsMaximumpI.Text = objXmlFile.GetParam(DigestionOptions, "DigestProteinsMaximumpI", txtDigestProteinsMaximumpI.Text)

                ' Load Uniqueness Options
                chkAssumeInputFileIsDigested.Checked = objXmlFile.GetParam(UniquenessStatsOptions, "AssumeInputFileIsDigested", chkAssumeInputFileIsDigested.Checked)

                txtUniquenessBinWidth.Text = objXmlFile.GetParam(UniquenessStatsOptions, "UniquenessBinWidth", txtUniquenessBinWidth.Text)
                chkAutoComputeRangeForBinning.Checked = objXmlFile.GetParam(UniquenessStatsOptions, "AutoComputeRangeForBinning", chkAutoComputeRangeForBinning.Checked)
                txtUniquenessBinStartMass.Text = objXmlFile.GetParam(UniquenessStatsOptions, "UniquenessBinStartMass", txtUniquenessBinStartMass.Text)
                txtUniquenessBinEndMass.Text = objXmlFile.GetParam(UniquenessStatsOptions, "UniquenessBinEndMass", txtUniquenessBinEndMass.Text)

                txtMaxPeakMatchingResultsPerFeatureToSave.Text = objXmlFile.GetParam(UniquenessStatsOptions, "MaxPeakMatchingResultsPerFeatureToSave", txtMaxPeakMatchingResultsPerFeatureToSave.Text)
                chkUseSLiCScoreForUniqueness.Checked = objXmlFile.GetParam(UniquenessStatsOptions, "UseSLiCScoreForUniqueness", chkUseSLiCScoreForUniqueness.Checked)
                txtMinimumSLiCScore.Text = objXmlFile.GetParam(UniquenessStatsOptions, "MinimumSLiCScore", txtMinimumSLiCScore.Text)
                blnRadioButtonChecked = objXmlFile.GetParam(UniquenessStatsOptions, "UseEllipseSearchRegion", True)
                If blnRadioButtonChecked Then
                    optUseEllipseSearchRegion.Checked = blnRadioButtonChecked
                Else
                    optUseRectangleSearchRegion.Checked = blnRadioButtonChecked
                End If

                ''chkAllowSqlServerCaching.Checked = objXmlFile.GetParam(UniquenessStatsOptions, "AllowSqlServerCaching", chkAllowSqlServerCaching.Checked)
                ''chkUseSqlServerDBToCacheData.Checked = objXmlFile.GetParam(UniquenessStatsOptions, "UseSqlServerDBToCacheData", chkUseSqlServerDBToCacheData.Checked)
                ''txtSqlServerName.Text = objXmlFile.GetParam(UniquenessStatsOptions, "SqlServerName", txtSqlServerName.Text)
                ''txtSqlServerDatabase.Text = objXmlFile.GetParam(UniquenessStatsOptions, "SqlServerDatabase", txtSqlServerDatabase.Text)
                ''chkSqlServerUseIntegratedSecurity.Checked = objXmlFile.GetParam(UniquenessStatsOptions, "SqlServerUseIntegratedSecurity", chkSqlServerUseIntegratedSecurity.Checked)

                ''chkSqlServerUseExistingData.Checked = objXmlFile.GetParam(UniquenessStatsOptions, "SqlServerUseExistingData", chkSqlServerUseExistingData.Checked)

                ''txtSqlServerUsername.Text = objXmlFile.GetParam(UniquenessStatsOptions, "SqlServerUsername", txtSqlServerUsername.Text)
                ''txtSqlServerPassword.Text = objXmlFile.GetParam(UniquenessStatsOptions, "SqlServerPassword", txtSqlServerPassword.Text)

                ' Load the peak matching thresholds
                cboMassTolType.SelectedIndex = objXmlFile.GetParam(PMOptions, "MassToleranceType", cboMassTolType.SelectedIndex)
                chkAutoDefineSLiCScoreTolerances.Checked = objXmlFile.GetParam(PMOptions, "AutoDefineSLiCScoreThresholds", chkAutoDefineSLiCScoreTolerances.Checked)
                blnAutoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked

                ' See if any peak matching data is present
                ' If it is, clear the table and load it; if not, leave the table unchanged

                valueNotPresent = False
                strThresholdData = objXmlFile.GetParam(PMOptions, "ThresholdData", String.Empty, valueNotPresent)

                If Not valueNotPresent AndAlso Not strThresholdData Is Nothing AndAlso strThresholdData.Length > 0 Then
                    strThresholds = strThresholdData.Split(";"c)

                    If strThresholds.Length > 0 Then
                        ClearPMThresholdsList(False)

                        For intIndex = 0 To strThresholds.Length - 1
                            strThresholdDetails = strThresholds(intIndex).Split(ColumnDelimiters)

                            If strThresholdDetails.Length > 2 AndAlso Not blnAutoDefineSLiCScoreThresholds Then
                                If IsNumeric(strThresholdDetails(0)) And IsNumeric(strThresholdDetails(1)) And
                                 IsNumeric(strThresholdDetails(2)) And IsNumeric(strThresholdDetails(3)) Then
                                    AddPMThresholdRow(CDbl(strThresholdDetails(0)), CDbl(strThresholdDetails(1)),
                                         CDbl(strThresholdDetails(2)), CDbl(strThresholdDetails(3)))
                                End If
                            ElseIf strThresholdDetails.Length >= 2 Then
                                If IsNumeric(strThresholdDetails(0)) And IsNumeric(strThresholdDetails(1)) Then
                                    AddPMThresholdRow(CDbl(strThresholdDetails(0)), CDbl(strThresholdDetails(1)))
                                End If
                            End If
                        Next intIndex
                    End If
                End If

            Catch ex As Exception
                ShowErrorMessage("Invalid parameter in settings file: " & Path.GetFileName(GetSettingsFilePath()), "Error")
            End Try

        Catch ex As Exception
            ShowErrorMessage("Error loading settings from file: " & GetSettingsFilePath(), "Error")
        End Try

    End Sub

    Private Sub IniFileSaveOptions(Optional blnSaveWindowDimensionsOnly As Boolean = False)

        Const OptionsSection As String = clsParseProteinFile.XML_SECTION_OPTIONS
        Const FASTAOptions As String = clsParseProteinFile.XML_SECTION_FASTA_OPTIONS
        Const ProcessingOptions As String = clsParseProteinFile.XML_SECTION_PROCESSING_OPTIONS
        Const DigestionOptions As String = clsParseProteinFile.XML_SECTION_DIGESTION_OPTIONS
        Const UniquenessStatsOptions As String = clsParseProteinFile.XML_SECTION_UNIQUENESS_STATS_OPTIONS
        Const PMOptions As String = clsProteinDigestionSimulator.XML_SECTION_PEAK_MATCHING_OPTIONS

        Dim objXmlFile As New XmlSettingsFileAccessor()


        Try
            Dim fiSettingsFile = New FileInfo(GetSettingsFilePath())
            If Not fiSettingsFile.Exists Then
                blnSaveWindowDimensionsOnly = False
            End If
        Catch
            'Ignore errors here
        End Try

        Try

            ' Pass True to .LoadSettings() to turn on case sensitive matching
            objXmlFile.LoadSettings(GetSettingsFilePath(), True)

            Try
                If Not blnSaveWindowDimensionsOnly Then
                    objXmlFile.SetParam(OptionsSection, "InputFilePath", GetProteinInputFilePath())
                    objXmlFile.SetParam(OptionsSection, "InputFileFormatIndex", cboInputFileFormat.SelectedIndex)
                    objXmlFile.SetParam(OptionsSection, "InputFileColumnDelimiterIndex", cboInputFileColumnDelimiter.SelectedIndex)
                    objXmlFile.SetParam(OptionsSection, "InputFileColumnDelimiter", txtInputFileColumnDelimiter.Text)

                    objXmlFile.SetParam(OptionsSection, "InputFileColumnOrdering", cboInputFileColumnOrdering.SelectedIndex)


                    xmlSettings.SetParam(OptionsSection, "OutputFileFieldDelimiterIndex", cboOutputFileFieldDelimiter.SelectedIndex)
                    xmlSettings.SetParam(OptionsSection, "OutputFileFieldDelimiter", txtOutputFileFieldDelimiter.Text)

                    objXmlFile.SetParam(OptionsSection, "IncludePrefixAndSuffixResidues", chkIncludePrefixAndSuffixResidues.Checked)
                    objXmlFile.SetParam(OptionsSection, "EnableLogging", chkEnableLogging.Checked)

                    objXmlFile.SetParam(OptionsSection, "CustomValidationRulesFilePath", mCustomValidationRulesFilePath)
                End If

                objXmlFile.SetParam(OptionsSection, "WindowWidth", Me.Width)
                objXmlFile.SetParam(OptionsSection, "WindowHeight", Me.Height)

                If Not blnSaveWindowDimensionsOnly Then
                    objXmlFile.SetParam(FASTAOptions, "RefStartChar", txtRefStartChar.Text)
                    objXmlFile.SetParam(FASTAOptions, "RefEndCharIndex", cboRefEndChar.SelectedIndex)
                    objXmlFile.SetParam(FASTAOptions, "RefEndChar", txtRefEndChar.Text)

                    objXmlFile.SetParam(FASTAOptions, "LookForAddnlRefInDescription", chkLookForAddnlRefInDescription.Checked)
                    objXmlFile.SetParam(FASTAOptions, "AddnlRefSepChar", txtAddnlRefSepChar.Text)
                    objXmlFile.SetParam(FASTAOptions, "AddnlRefAccessionSepChar", txtAddnlRefAccessionSepChar.Text)

                    objXmlFile.SetParam(ProcessingOptions, "ExcludeProteinSequence", chkExcludeProteinSequence.Checked)
                    objXmlFile.SetParam(ProcessingOptions, "ComputeProteinMass", chkComputeProteinMass.Checked)
                    objXmlFile.SetParam(ProcessingOptions, "ComputepI", chkComputepIandNET.Checked)
                    objXmlFile.SetParam(ProcessingOptions, "IncludeXResidues", chkIncludeXResidues.Checked)

                    objXmlFile.SetParam(ProcessingOptions, "ComputeSequenceHashValues", chkComputeSequenceHashValues.Checked)
                    objXmlFile.SetParam(ProcessingOptions, "ComputeSequenceHashIgnoreILDiff", chkComputeSequenceHashIgnoreILDiff.Checked)
                    objXmlFile.SetParam(ProcessingOptions, "TruncateProteinDescription", chkTruncateProteinDescription.Checked)
                    objXmlFile.SetParam(ProcessingOptions, "ExcludeProteinDescription", chkExcludeProteinDescription.Checked)

                    objXmlFile.SetParam(ProcessingOptions, "DigestProteins", chkDigestProteins.Checked)
                    objXmlFile.SetParam(ProcessingOptions, "ProteinReversalIndex", cboProteinReversalOptions.SelectedIndex)
                    objXmlFile.SetParam(ProcessingOptions, "ProteinScramblingLoopCount", txtProteinScramblingLoopCount.Text)
                    objXmlFile.SetParam(ProcessingOptions, "ElementMassMode", cboElementMassMode.SelectedIndex)

                    objXmlFile.SetParam(ProcessingOptions, "HydrophobicityMode", cboHydrophobicityMode.SelectedIndex)
                    objXmlFile.SetParam(ProcessingOptions, "MaxpIModeEnabled", chkMaxpIModeEnabled.Checked)
                    objXmlFile.SetParam(ProcessingOptions, "MaxpISequenceLength", LookupMaxpISequenceLength())

                    objXmlFile.SetParam(DigestionOptions, "CleavageRuleTypeIndex", cboCleavageRuleType.SelectedIndex)
                    objXmlFile.SetParam(DigestionOptions, "IncludeDuplicateSequences", chkIncludeDuplicateSequences.Checked)
                    objXmlFile.SetParam(DigestionOptions, "CysPeptidesOnly", chkCysPeptidesOnly.Checked)

                    objXmlFile.SetParam(DigestionOptions, "DigestProteinsMinimumMass", txtDigestProteinsMinimumMass.Text)
                    objXmlFile.SetParam(DigestionOptions, "DigestProteinsMaximumMass", txtDigestProteinsMaximumMass.Text)
                    objXmlFile.SetParam(DigestionOptions, "DigestProteinsMinimumResidueCount", txtDigestProteinsMinimumResidueCount.Text)
                    objXmlFile.SetParam(DigestionOptions, "DigestProteinsMaximumMissedCleavages", txtDigestProteinsMaximumMissedCleavages.Text)

                    objXmlFile.SetParam(DigestionOptions, "DigestProteinsMinimumpI", txtDigestProteinsMinimumpI.Text)
                    objXmlFile.SetParam(DigestionOptions, "DigestProteinsMaximumpI", txtDigestProteinsMaximumpI.Text)

                    ' Load Uniqueness Options
                    objXmlFile.SetParam(UniquenessStatsOptions, "AssumeInputFileIsDigested", chkAssumeInputFileIsDigested.Checked)

                    objXmlFile.SetParam(UniquenessStatsOptions, "UniquenessBinWidth", txtUniquenessBinWidth.Text)
                    objXmlFile.SetParam(UniquenessStatsOptions, "AutoComputeRangeForBinning", chkAutoComputeRangeForBinning.Checked)
                    objXmlFile.SetParam(UniquenessStatsOptions, "UniquenessBinStartMass", txtUniquenessBinStartMass.Text)
                    objXmlFile.SetParam(UniquenessStatsOptions, "UniquenessBinEndMass", txtUniquenessBinEndMass.Text)

                    objXmlFile.SetParam(UniquenessStatsOptions, "MaxPeakMatchingResultsPerFeatureToSave", txtMaxPeakMatchingResultsPerFeatureToSave.Text)
                    objXmlFile.SetParam(UniquenessStatsOptions, "UseSLiCScoreForUniqueness", chkUseSLiCScoreForUniqueness.Checked)
                    objXmlFile.SetParam(UniquenessStatsOptions, "MinimumSLiCScore", txtMinimumSLiCScore.Text)
                    objXmlFile.SetParam(UniquenessStatsOptions, "UseEllipseSearchRegion", optUseEllipseSearchRegion.Checked)

                    ''objXmlFile.SetParam(UniquenessStatsOptions, "AllowSqlServerCaching", chkAllowSqlServerCaching.Checked)
                    ''objXmlFile.SetParam(UniquenessStatsOptions, "UseSqlServerDBToCacheData", chkUseSqlServerDBToCacheData.Checked)
                    ''objXmlFile.SetParam(UniquenessStatsOptions, "SqlServerName", txtSqlServerName.Text)
                    ''objXmlFile.SetParam(UniquenessStatsOptions, "SqlServerDatabase", txtSqlServerDatabase.Text)
                    ''objXmlFile.SetParam(UniquenessStatsOptions, "SqlServerUseIntegratedSecurity", chkSqlServerUseIntegratedSecurity.Checked)
                    ''objXmlFile.SetParam(UniquenessStatsOptions, "SqlServerUseExistingData", chkSqlServerUseExistingData.Checked)
                    ''objXmlFile.SetParam(UniquenessStatsOptions, "SqlServerUsername", txtSqlServerUsername.Text)
                    ''objXmlFile.SetParam(UniquenessStatsOptions, "SqlServerPassword", txtSqlServerPassword.Text)


                    ' Save the peak matching thresholds
                    objXmlFile.SetParam(PMOptions, "MassToleranceType", cboMassTolType.SelectedIndex.ToString)

                    Dim blnAutoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked
                    objXmlFile.SetParam(PMOptions, "AutoDefineSLiCScoreThresholds", blnAutoDefineSLiCScoreThresholds.ToString)

                    Dim strThresholdData = String.Empty
                    For Each myDataRow As DataRow In mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE).Rows
                        If strThresholdData.Length > 0 Then strThresholdData &= "; "
                        If blnAutoDefineSLiCScoreThresholds Then
                            strThresholdData &= CStr(myDataRow.Item(0)) & "," & CStr(myDataRow.Item(1))
                        Else
                            strThresholdData &= CStr(myDataRow.Item(0)) & "," & CStr(myDataRow.Item(1)) & "," & CStr(myDataRow.Item(2)) & "," & CStr(myDataRow.Item(3))
                        End If
                    Next myDataRow
                    objXmlFile.SetParam(PMOptions, "ThresholdData", strThresholdData)
                End If
            Catch ex As Exception
                ShowErrorMessage("Error storing parameter in settings file: " & Path.GetFileName(GetSettingsFilePath()), "Error")
            End Try

            objXmlFile.SaveSettings()

        Catch ex As Exception
            ShowErrorMessage("Error saving settings to file: " & GetSettingsFilePath(), "Error")
        End Try


    End Sub

    Private Sub InitializeControls()
        DefineDefaultPMThresholds()

        Me.Text = "Protein Digestion Simulator"
        lblUniquenessCalculationsNote.Text = "The Protein Digestion Simulator uses an elution time prediction algorithm developed by Lars Kangas and Kostas Petritis. See Help->About Elution Time Prediction for more info.  Note that you can provide custom time values for peptides by separately generating a tab or comma delimited text file with information corresponding to one of the options in the 'Column Order' list on the 'File Format' option tab, then checking 'Assume Input file is Already Digested' on this tab."

        PopulateComboBoxes()
        InitializePeakMatchingDataGrid()

        IniFileLoadOptions()
        SetToolTips()

        ShowSplashScreen()

        EnableDisableControls()

        ResetProgress(True)
    End Sub

    Private Sub InitializePeakMatchingDataGrid()


        ' Make the Peak Matching Thresholds datatable
        Dim dtPMThresholds = New DataTable(PM_THRESHOLDS_DATATABLE)

        ' Add the columns to the datatable
        ADONetRoutines.AppendColumnDoubleToTable(dtPMThresholds, COL_NAME_MASS_TOLERANCE)
        ADONetRoutines.AppendColumnDoubleToTable(dtPMThresholds, COL_NAME_NET_TOLERANCE)
        ADONetRoutines.AppendColumnDoubleToTable(dtPMThresholds, COL_NAME_SLIC_MASS_STDEV, DEFAULT_SLIC_MASS_STDEV)
        ADONetRoutines.AppendColumnDoubleToTable(dtPMThresholds, COL_NAME_SLIC_NET_STDEV, DEFAULT_SLIC_NET_STDEV)
        ADONetRoutines.AppendColumnIntegerToTable(dtPMThresholds, COL_NAME_PM_THRESHOLD_ROW_ID, 0, True, True)

        With dtPMThresholds
            Dim PrimaryKeyColumn = New DataColumn() { .Columns(COL_NAME_PM_THRESHOLD_ROW_ID)}
            .PrimaryKey = PrimaryKeyColumn
        End With

        ' Instantiate the dataset
        mPeakMatchingThresholdsDataset = New DataSet(PM_THRESHOLDS_DATATABLE)

        ' Add the new System.Data.DataTable to the DataSet.
        mPeakMatchingThresholdsDataset.Tables.Add(dtPMThresholds)

        ' Bind the DataSet to the DataGrid
        With dgPeakMatchingThresholds
            .DataSource = mPeakMatchingThresholdsDataset
            .DataMember = PM_THRESHOLDS_DATATABLE
        End With

        ' Update the grid's table style
        UpdateDataGridTableStyle()

        ' Populate the table
        AutoPopulatePMThresholdsByID(PredefinedPMThresholdsConstants.OneMassOneNET, False)

    End Sub

    Private Sub UpdateDataGridTableStyle()
        Dim tsPMThresholdsTableStyle As DataGridTableStyle

        ' Define the PM Thresholds table style
        tsPMThresholdsTableStyle = New DataGridTableStyle

        ' Setting the MappingName of the table style to PM_THRESHOLDS_DATATABLE will cause this style to be used with that table
        With tsPMThresholdsTableStyle
            .MappingName = PM_THRESHOLDS_DATATABLE
            .AllowSorting = True
            .ColumnHeadersVisible = True
            .RowHeadersVisible = True
            .ReadOnly = False
        End With

        ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_MASS_TOLERANCE, "Mass Tolerance", 90)
        ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_NET_TOLERANCE, "NET Tolerance", 90)

        If chkAutoDefineSLiCScoreTolerances.Checked Then
            dgPeakMatchingThresholds.Width = 250
        Else
            dgPeakMatchingThresholds.Width = 425
            ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_MASS_STDEV, "SLiC Mass StDev", 90)
            ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_NET_STDEV, "SLiC NET StDev", 90)
        End If

        cmdPastePMThresholdsList.Left = dgPeakMatchingThresholds.Left + dgPeakMatchingThresholds.Width + 15
        cmdClearPMThresholdsList.Left = cmdPastePMThresholdsList.Left

        With dgPeakMatchingThresholds
            .TableStyles.Clear()

            If Not .TableStyles.Contains(tsPMThresholdsTableStyle) Then
                .TableStyles.Add(tsPMThresholdsTableStyle)
            End If

            .Refresh()
        End With

    End Sub

    Private Function InitializeProteinFileParserGeneralOptions(ByRef objParseProteinFile As clsParseProteinFile) As Boolean
        ' Returns true if all values were valid

        Dim blnError As Boolean

        With objParseProteinFile
            If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.FastaFile Then
                .AssumeFastaFile = True
            ElseIf cboInputFileFormat.SelectedIndex = InputFileFormatConstants.DelimitedText Then
                .AssumeDelimitedFile = True
            Else
                .AssumeFastaFile = False
            End If

            If cboInputFileColumnOrdering.SelectedIndex >= 0 Then
                .DelimitedFileFormatCode = CType(cboInputFileColumnOrdering.SelectedIndex, DelimitedFileReader.eDelimitedFileFormatCode)
            End If

            .InputFileDelimiter = LookupColumnDelimiter(cboInputFileColumnDelimiter, txtInputFileColumnDelimiter, ControlChars.Tab)
            .OutputFileDelimiter = LookupColumnDelimiter(cboOutputFileFieldDelimiter, txtOutputFileFieldDelimiter, ControlChars.Tab)

            With .FastaFileOptions
                ValidateTextbox(txtRefStartChar, mDefaultFastaFileOptions.ProteinLineStartChar)
                .ProteinLineStartChar = txtRefStartChar.Text.Chars(0)
                .ProteinLineAccessionEndChar = LookupColumnDelimiter(cboRefEndChar, txtRefEndChar, mDefaultFastaFileOptions.ProteinLineAccessionEndChar)

                .LookForAddnlRefInDescription = chkLookForAddnlRefInDescription.Checked

                ValidateTextbox(txtAddnlRefSepChar, mDefaultFastaFileOptions.AddnlRefSepChar)
                ValidateTextbox(txtAddnlRefAccessionSepChar, mDefaultFastaFileOptions.AddnlRefAccessionSepChar)

                .AddnlRefSepChar = txtAddnlRefSepChar.Text.Chars(0)
                .AddnlRefAccessionSepChar = txtAddnlRefAccessionSepChar.Text.Chars(0)

            End With

            .ExcludeProteinSequence = chkExcludeProteinSequence.Checked
            .ComputeProteinMass = chkComputeProteinMass.Checked
            .ComputepI = chkComputepIandNET.Checked
            .ComputeNET = chkComputepIandNET.Checked
            .ComputeSCXNET = chkComputepIandNET.Checked

            .ComputeSequenceHashValues = chkComputeSequenceHashValues.Checked
            .ComputeSequenceHashIgnoreILDiff = chkComputeSequenceHashIgnoreILDiff.Checked
            .TruncateProteinDescription = chkTruncateProteinDescription.Checked
            .ExcludeProteinDescription = chkExcludeProteinDescription.Checked

            If cboHydrophobicityMode.SelectedIndex >= 0 Then
                .HydrophobicityType = CType(cboHydrophobicityMode.SelectedIndex, clspICalculation.eHydrophobicityTypeConstants)
            End If

            .ReportMaximumpI = chkMaxpIModeEnabled.Checked
            .SequenceWidthToExamineForMaximumpI = LookupMaxpISequenceLength()

            .IncludeXResiduesInMass = chkIncludeXResidues.Checked

            .GenerateUniqueIDValuesForPeptides = chkGenerateUniqueIDValues.Checked

            With .DigestionOptions
                If cboCleavageRuleType.SelectedIndex >= 0 Then
                    .CleavageRuleID = CType(cboCleavageRuleType.SelectedIndex, clsInSilicoDigest.CleavageRuleConstants)
                End If

                .IncludePrefixAndSuffixResidues = chkIncludePrefixAndSuffixResidues.Checked

                .MinFragmentMass = ParseTextboxValueInt(txtDigestProteinsMinimumMass, lblDigestProteinsMinimumMass.Text & " must be an integer value", blnError)
                If blnError Then Return False

                .MaxFragmentMass = ParseTextboxValueInt(txtDigestProteinsMaximumMass, lblDigestProteinsMaximumMass.Text & " must be an integer value", blnError)
                If blnError Then Return False

                .MaxMissedCleavages = ParseTextboxValueInt(txtDigestProteinsMaximumMissedCleavages, lblDigestProteinsMaximumMissedCleavages.Text & " must be an integer value", blnError)
                If blnError Then Return False

                .MinFragmentResidueCount = ParseTextboxValueInt(txtDigestProteinsMinimumResidueCount, lblDigestProteinsMinimumResidueCount.Text & " must be an integer value", blnError)
                If blnError Then Return False

                .MinIsoelectricPoint = ParseTextboxValueSng(txtDigestProteinsMinimumpI, lblDigestProteinsMinimumpI.Text & " must be a decimal value", blnError)
                If blnError Then Return False

                .MaxIsoelectricPoint = ParseTextboxValueSng(txtDigestProteinsMaximumpI, lblDigestProteinsMaximumpI.Text & " must be a decimal value", blnError)
                If blnError Then Return False

                .RemoveDuplicateSequences = Not chkIncludeDuplicateSequences.Checked
                If chkCysPeptidesOnly.Checked Then
                    .AminoAcidResidueFilterChars = New Char() {"C"c}
                Else
                    .AminoAcidResidueFilterChars = New Char() {}
                End If
            End With
        End With

        Return True

    End Function

    Private Function LookupColumnDelimiter(DelimiterCombobox As ListControl, DelimiterTextbox As Control, strDefaultDelimiter As Char) As Char
        Try
            Return clsParseProteinFile.LookupColumnDelimiterChar(DelimiterCombobox.SelectedIndex, DelimiterTextbox.Text, strDefaultDelimiter)
        Catch ex As Exception
            Return ControlChars.Tab
        End Try
    End Function

    Private Function LookupMaxpISequenceLength() As Integer
        Dim blnError As Boolean
        Dim intLength As Integer

        Try
            intLength = VBNetRoutines.ParseTextboxValueInt(txtMaxpISequenceLength, String.Empty, blnError, 10)
            If blnError Then
                txtMaxpISequenceLength.Text = intLength.ToString
            End If
        Catch ex As Exception
            intLength = 10
        End Try

        If intLength < 1 Then intLength = 1
        Return intLength
    End Function

    Private Sub SetToolTips()
        Dim objToolTipControl As New ToolTip

        With objToolTipControl
            .SetToolTip(cmdParseInputFile, "Parse proteins in input file to create output file(s).")
            .SetToolTip(cboInputFileColumnDelimiter, "Character separating columns in a delimited text input file.")
            .SetToolTip(txtInputFileColumnDelimiter, "Custom character separating columns in a delimited text input file.")
            .SetToolTip(txtOutputFileFieldDelimiter, "Character separating the fields in the output file.")
            .SetToolTip(txtRefStartChar, "Character at the start of each protein description line in a Fasta file.")
            .SetToolTip(cboRefEndChar, "Character at the end of the protein accession name in a Fasta file.")
            .SetToolTip(txtRefEndChar, "Custom character at the end of the protein accession name in a Fasta file.")

            .SetToolTip(txtAddnlRefSepChar, "Character separating additional protein accession entries in a protein's description in a Fasta file.")
            .SetToolTip(txtAddnlRefAccessionSepChar, "Character separating source name and accession number for additional protein accession entries in a Fasta file.")

            .SetToolTip(chkGenerateUniqueIDValues, "Set this to false to use less memory when digesting huge protein input files.")
            .SetToolTip(txtProteinReversalSamplingPercentage, "Set this to a value less than 100 to only include a portion of the residues from the input file in the output file.")
            .SetToolTip(txtProteinScramblingLoopCount, "Set this to a value greater than 1 to create multiple scrambled versions of the intput file.")

            .SetToolTip(optUseEllipseSearchRegion, "This setting only takes effect if 'Use SLiC Score when gauging uniqueness' is false.")
            .SetToolTip(optUseRectangleSearchRegion, "This setting only takes effect if 'Use SLiC Score when gauging uniqueness' is false.")

            .SetToolTip(lblPeptideUniquenessMassMode, "Current mass mode; to change go to the 'Parse and Digest File Options' tab")

            .SetToolTip(chkExcludeProteinSequence, "Enabling this setting will prevent protein sequences from being written to the output file; useful when processing extremely large files.")
            .SetToolTip(chkTruncateProteinDescription, "Truncate description (if over 7995 chars)")

            .SetToolTip(chkEnableLogging, "Logs status and error messages to file ProteinDigestionSimulatorLog*.txt in the program directory.")
        End With

        objToolTipControl = Nothing

    End Sub

    Private Sub ParseProteinInputFile()
        Dim blnSuccess As Boolean

        If Not mWorking AndAlso ConfirmFilePaths() Then
            Try
                If mParseProteinFile Is Nothing Then
                    mParseProteinFile = New clsParseProteinFile
                End If

                blnSuccess = InitializeProteinFileParserGeneralOptions(mParseProteinFile)
                If Not blnSuccess Then Exit Try

                With mParseProteinFile
                    .CreateProteinOutputFile = True

                    If cboProteinReversalOptions.SelectedIndex >= 0 Then
                        .ProteinScramblingMode = CType(cboProteinReversalOptions.SelectedIndex, clsParseProteinFile.ProteinScramblingModeConstants)
                    End If

                    .ProteinScramblingSamplingPercentage = VBNetRoutines.ParseTextboxValueInt(txtProteinReversalSamplingPercentage, "", False, 100, False)
                    .ProteinScramblingLoopCount = VBNetRoutines.ParseTextboxValueInt(txtProteinScramblingLoopCount, "", False, 1, False)
                    .CreateDigestedProteinOutputFile = chkDigestProteins.Checked
                    .CreateFastaOutputFile = chkCreateFastaOutputFile.Checked

                    If cboElementMassMode.SelectedIndex >= 0 Then
                        .ElementMassMode = CType(cboElementMassMode.SelectedIndex, PeptideSequenceClass.ElementModeConstants)
                    End If

                    Cursor.Current = Cursors.WaitCursor
                    mWorking = True
                    cmdParseInputFile.Enabled = False

                    ResetProgress(True)
                    SwitchToProgressTab()

                    Dim strOutputFolderPath As String = String.Empty
                    Dim strOutputFileNameBaseOverride As String = String.Empty

                    If txtProteinOutputFilePath.TextLength > 0 Then
                        strOutputFolderPath = Path.GetDirectoryName(txtProteinOutputFilePath.Text)
                        strOutputFileNameBaseOverride = Path.GetFileNameWithoutExtension(txtProteinOutputFilePath.Text)
                    End If

                    blnSuccess = .ParseProteinFile(GetProteinInputFilePath(), strOutputFolderPath, strOutputFileNameBaseOverride)

                    Cursor.Current = Cursors.Default

                    If blnSuccess Then
                        MessageBox.Show(mParseProteinFile.ProcessingSummary, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        SwitchFromProgressTab()
                    Else
                        ShowErrorMessage("Error parsing protein file: " & .GetErrorMessage(), "Error")
                    End If
                End With

            Catch ex As Exception
                ShowErrorMessage("Error in frmMain->ParseProteinInputFile: " & ex.Message, "Error")
            Finally
                mWorking = False
                cmdParseInputFile.Enabled = True
                mParseProteinFile.CloseLogFileNow()
                mParseProteinFile = Nothing
            End Try
        End If

    End Sub

    Private Function ParseTextboxValueInt(ThisTextBox As Control, strMessageIfError As String, ByRef blnError As Boolean, Optional ValueIfError As Integer = 0) As Integer

        blnError = False

        Try
            Return Integer.Parse(ThisTextBox.Text)
        Catch ex As Exception
            ShowErrorMessage(strMessageIfError, "Error")
            blnError = True
            Return ValueIfError
        End Try

    End Function

    Private Function ParseTextboxValueSng(ThisTextBox As Control, strMessageIfError As String, ByRef blnError As Boolean, Optional ValueIfError As Single = 0) As Single

        blnError = False

        Try
            Return Single.Parse(ThisTextBox.Text)
        Catch ex As Exception
            ShowErrorMessage(strMessageIfError, "Error")
            blnError = True
            Return ValueIfError
        End Try

    End Function

    Private Sub PastePMThresholdsValues(blnClearList As Boolean)
        Dim objData As IDataObject

        Dim strData As String
        Dim strLines() As String
        Dim strColumns() As String

        Dim dblMassThreshold As Double, dblNETThreshold As Double
        Dim dblSLiCMassStDev As Double, dblSLiCNETStDev As Double

        Dim strMessage As String

        Dim blnUseSLiC As Boolean
        Dim blnExistingRowFound As Boolean
        Dim intRowsAlreadyPresent As Integer
        Dim intRowsSkipped As Integer

        Dim intLineIndex As Integer

        Dim LineDelimiters = New Char() {ControlChars.Cr, ControlChars.Lf}
        Dim ColumnDelimiters = New Char() {ControlChars.Tab, ","c}

        ' Examine the clipboard contents
        objData = Clipboard.GetDataObject()

        If Not objData Is Nothing Then
            If objData.GetDataPresent(DataFormats.StringFormat, True) Then
                strData = CType(objData.GetData(DataFormats.StringFormat, True), String)

                ' Split strData on carriage return or line feed characters
                ' Lines that end in CrLf will give two separate lines; one with with the text, and one blank; that's OK
                strLines = strData.Split(LineDelimiters, 1000)

                If strLines.Length > 0 Then
                    If blnClearList Then
                        If Not ClearPMThresholdsList(True) Then Return
                    End If

                    intRowsAlreadyPresent = 0
                    intRowsSkipped = 0

                    For intLineIndex = 0 To strLines.Length - 1
                        If Not strLines(intLineIndex) Is Nothing AndAlso strLines(intLineIndex).Length > 0 Then
                            strColumns = strLines(intLineIndex).Split(ColumnDelimiters, 5)
                            If strColumns.Length >= 2 Then
                                Try
                                    dblMassThreshold = Double.Parse(strColumns(0))
                                    dblNETThreshold = Double.Parse(strColumns(1))

                                    If dblMassThreshold >= 0 And dblNETThreshold >= 0 Then
                                        If Not chkAutoDefineSLiCScoreTolerances.Checked AndAlso strColumns.Length >= 4 Then
                                            blnUseSLiC = True
                                        Else
                                            blnUseSLiC = False
                                        End If

                                        If blnUseSLiC Then
                                            Try
                                                dblSLiCMassStDev = Double.Parse(strColumns(2))
                                                dblSLiCNETStDev = Double.Parse(strColumns(3))
                                            Catch ex As Exception
                                                blnUseSLiC = False
                                            End Try
                                        End If

                                        blnExistingRowFound = False
                                        If blnUseSLiC Then
                                            AddPMThresholdRow(dblMassThreshold, dblNETThreshold, dblSLiCMassStDev, dblSLiCNETStDev, blnExistingRowFound)
                                        Else
                                            AddPMThresholdRow(dblMassThreshold, dblNETThreshold, blnExistingRowFound)
                                        End If

                                        If blnExistingRowFound Then
                                            intRowsAlreadyPresent += 1
                                        End If
                                    End If

                                Catch ex As Exception
                                    ' Skip this row
                                    intRowsSkipped += 1
                                End Try
                            Else
                                intRowsSkipped += 1
                            End If
                        End If
                    Next intLineIndex

                    If intRowsAlreadyPresent > 0 Then
                        If intRowsAlreadyPresent = 1 Then
                            strMessage = "1 row of thresholds was"
                        Else
                            strMessage = intRowsAlreadyPresent.ToString & " rows of thresholds were"
                        End If

                        ShowErrorMessage(strMessage & " already present in the table; duplicate rows are not allowed.", "Warning")
                    End If

                    If intRowsSkipped > 0 Then
                        If intRowsSkipped = 1 Then
                            strMessage = "1 row was skipped because it"
                        Else
                            strMessage = intRowsSkipped.ToString & " rows were skipped because they"
                        End If

                        ShowErrorMessage(strMessage & " didn't contain two columns of numeric data.", "Warning")
                    End If

                End If
            End If

        End If
    End Sub

    Private Sub PopulateComboBoxes()

        Const NET_UNITS = "NET"

        Try
            With cboInputFileFormat
                With .Items
                    .Clear()
                    .Insert(InputFileFormatConstants.AutoDetermine, "Auto-determine")
                    .Insert(InputFileFormatConstants.FastaFile, "Fasta file")
                    .Insert(InputFileFormatConstants.DelimitedText, "Delimited text")
                End With
                .SelectedIndex = InputFileFormatConstants.AutoDetermine
            End With

            With cboInputFileColumnDelimiter
                With .Items
                    .Clear()
                    .Insert(clsParseProteinFile.DelimiterCharConstants.Space, "Space")
                    .Insert(clsParseProteinFile.DelimiterCharConstants.Tab, "Tab")
                    .Insert(clsParseProteinFile.DelimiterCharConstants.Comma, "Comma")
                    .Insert(clsParseProteinFile.DelimiterCharConstants.Other, "Other")
                End With
                .SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Tab
            End With

            With cboOutputFileFieldDelimiter
                With .Items
                    .Clear()
                    For intIndex = 0 To cboInputFileColumnDelimiter.Items.Count - 1
                        .Insert(intIndex, cboInputFileColumnDelimiter.Items(intIndex))
                    Next
                End With
                .SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Space
            End With

            With cboRefEndChar
                With .Items()
                    .Clear()
                    For intIndex = 0 To cboInputFileColumnDelimiter.Items.Count - 1
                        .Insert(intIndex, cboInputFileColumnDelimiter.Items(intIndex))
                    Next
                End With
                .SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Space
            End With

            With cboInputFileColumnOrdering
                With .Items
                    .Clear()
                    .Insert(DelimitedFileReader.eDelimitedFileFormatCode.SequenceOnly, "Sequence Only")
                    .Insert(DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Sequence, "ProteinName and Sequence")
                    .Insert(DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence, "ProteinName, Descr, Seq")
                    .Insert(DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence, "UniqueID and Seq")
                    .Insert(DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID, "ProteinName, Seq, UniqueID")
                    .Insert(DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, "ProteinName, Seq, UniqueID, Mass, NET")
                    .Insert(DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore, "ProteinName, Seq, UniqueID, Mass, NET, NETStDev, DiscriminantScore")
                    .Insert(DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET, "UniqueID, Seq, Mass, NET")
                End With
                .SelectedIndex = DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence
            End With

            With cboElementMassMode
                With .Items
                    .Clear()
                    .Insert(PeptideSequenceClass.ElementModeConstants.AverageMass, "Average")
                    .Insert(PeptideSequenceClass.ElementModeConstants.IsotopicMass, "Monoisotopic")
                End With
                .SelectedIndex = PeptideSequenceClass.ElementModeConstants.IsotopicMass
            End With

            With cboProteinReversalOptions
                With .Items
                    .Clear()
                    .Insert(clsParseProteinFile.ProteinScramblingModeConstants.None, "Normal output")
                    .Insert(clsParseProteinFile.ProteinScramblingModeConstants.Reversed, "Reverse ORF sequences")
                    .Insert(clsParseProteinFile.ProteinScramblingModeConstants.Randomized, "Randomized ORF sequences")
                End With
                .SelectedIndex = clsParseProteinFile.ProteinScramblingModeConstants.None
            End With

            Dim objInSilicoDigest = New clsInSilicoDigest()
            With cboCleavageRuleType
                With .Items
                    For intIndex = 0 To objInSilicoDigest.CleaveageRuleCount - 1
                        Dim eRuleID = CType(intIndex, clsInSilicoDigest.CleavageRuleConstants)
                        .Add(objInSilicoDigest.GetCleaveageRuleName(eRuleID) & " (" & objInSilicoDigest.GetCleaveageRuleResiduesDescription(eRuleID) & ")")
                    Next intIndex
                End With
                If .Items.Count > clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin Then
                    .SelectedIndex = clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin
                End If
            End With

            With cboMassTolType
                With .Items
                    .Clear()
                    .Insert(clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.PPM, "PPM")
                    .Insert(clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.Absolute, "Absolute (Da)")
                End With
                .SelectedIndex = clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.PPM
            End With

            With cboPMPredefinedThresholds
                With .Items
                    .Clear()
                    .Insert(PredefinedPMThresholdsConstants.OneMassOneNET, "5 ppm; 0.05 " & NET_UNITS)
                    .Insert(PredefinedPMThresholdsConstants.OneMassThreeNET, "5 ppm; 0.01, 0.05, 100 " & NET_UNITS)
                    .Insert(PredefinedPMThresholdsConstants.OneNETThreeMass, "0.5, 1, & 5 ppm; 0.05 " & NET_UNITS)
                    .Insert(PredefinedPMThresholdsConstants.ThreeMassThreeNET, "0.5, 1, 5 ppm; 0.01, 0.05, & 100 " & NET_UNITS)
                    .Insert(PredefinedPMThresholdsConstants.FiveMassThreeNET, "0.5, 1, 5, 10, & 50 ppm; 0.01, 0.05, & 100 " & NET_UNITS)
                End With
                .SelectedIndex = PredefinedPMThresholdsConstants.OneMassOneNET
            End With

            With cboHydrophobicityMode
                With .Items
                    .Clear()
                    .Insert(clspICalculation.eHydrophobicityTypeConstants.HW, "Hopp and Woods")
                    .Insert(clspICalculation.eHydrophobicityTypeConstants.KD, "Kyte and Doolittle")
                    .Insert(clspICalculation.eHydrophobicityTypeConstants.Eisenberg, "Eisenberg")
                    .Insert(clspICalculation.eHydrophobicityTypeConstants.GES, "Engleman et. al.")
                    .Insert(clspICalculation.eHydrophobicityTypeConstants.MeekPH7p4, "Meek pH 7.4")
                    .Insert(clspICalculation.eHydrophobicityTypeConstants.MeekPH2p1, "Meek pH 2.1")
                End With
                .SelectedIndex = clspICalculation.eHydrophobicityTypeConstants.HW
            End With

        Catch ex As Exception
            ShowErrorMessage("Error initializing the combo boxes: " & ex.Message)
        End Try

    End Sub

    Private Sub ResetProgress(blnHideSubtaskProgress As Boolean)
        lblProgressDescription.Text = String.Empty
        lblProgress.Text = FormatPercentComplete(0)
        pbarProgress.Value = 0
        pbarProgress.Visible = True

        lblSubtaskProgressDescription.Text = String.Empty
        lblSubtaskProgress.Text = FormatPercentComplete(0)

        If blnHideSubtaskProgress Then
            Me.SubtaskProgressIsVisible = False
        End If


        lblErrorMessage.Text = String.Empty

        Application.DoEvents()
    End Sub

    Private Sub ResetToDefaults(blnConfirm As Boolean)
        Dim eResponse As DialogResult

        If blnConfirm Then
            eResponse = MessageBox.Show("Are you sure you want to reset all settings to their default values?", "Reset to Defaults", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
            If eResponse <> DialogResult.Yes Then Exit Sub
        End If

        cboInputFileFormat.SelectedIndex = InputFileFormatConstants.AutoDetermine
        cboInputFileColumnDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Tab
        txtInputFileColumnDelimiter.Text = ";"c

        cboOutputFileFieldDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Tab
        txtOutputFileFieldDelimiter.Text = ";"c

        chkEnableLogging.Checked = False

        chkIncludePrefixAndSuffixResidues.Checked = False

        txtRefStartChar.Text = mDefaultFastaFileOptions.ProteinLineStartChar            ' ">"
        cboRefEndChar.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Space
        txtRefEndChar.Text = mDefaultFastaFileOptions.ProteinLineAccessionEndChar                ' " "

        chkLookForAddnlRefInDescription.Checked = False
        txtAddnlRefSepChar.Text = mDefaultFastaFileOptions.AddnlRefSepChar                      ' "|"
        txtAddnlRefAccessionSepChar.Text = mDefaultFastaFileOptions.AddnlRefAccessionSepChar    ' ":"

        chkExcludeProteinSequence.Checked = False
        chkComputeProteinMass.Checked = False
        cboElementMassMode.SelectedIndex = PeptideSequenceClass.ElementModeConstants.IsotopicMass

        chkComputepIandNET.Checked = False
        chkIncludeXResidues.Checked = False

        chkComputeSequenceHashValues.Checked = True
        chkComputeSequenceHashIgnoreILDiff.Checked = True
        chkTruncateProteinDescription.Checked = True
        chkExcludeProteinDescription.Checked = False

        cboHydrophobicityMode.SelectedIndex = clspICalculation.eHydrophobicityTypeConstants.HW
        chkMaxpIModeEnabled.Checked = False
        txtMaxpISequenceLength.Text = "10"

        chkDigestProteins.Checked = False
        cboProteinReversalOptions.SelectedIndex = clsParseProteinFile.ProteinScramblingModeConstants.None
        txtProteinReversalSamplingPercentage.Text = "100"
        txtProteinScramblingLoopCount.Text = "1"

        cboCleavageRuleType.SelectedIndex = clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin
        chkIncludeDuplicateSequences.Checked = False
        chkCysPeptidesOnly.Checked = False
        chkGenerateUniqueIDValues.Checked = True

        txtDigestProteinsMinimumMass.Text = "400"
        txtDigestProteinsMaximumMass.Text = "6000"
        txtDigestProteinsMinimumResidueCount.Text = "0"
        txtDigestProteinsMaximumMissedCleavages.Text = "3"

        txtDigestProteinsMinimumpI.Text = "0"
        txtDigestProteinsMaximumpI.Text = "14"

        ' Load Uniqueness Options
        chkAssumeInputFileIsDigested.Checked = True

        txtUniquenessBinWidth.Text = "25"
        chkAutoComputeRangeForBinning.Checked = True
        txtUniquenessBinStartMass.Text = "400"
        txtUniquenessBinEndMass.Text = "4000"

        txtMaxPeakMatchingResultsPerFeatureToSave.Text = "3"
        chkUseSLiCScoreForUniqueness.Checked = True
        txtMinimumSLiCScore.Text = "0.95"
        optUseEllipseSearchRegion.Checked = True

        chkUseSqlServerDBToCacheData.Checked = False
        txtSqlServerName.Text = SystemInformation.ComputerName
        txtSqlServerDatabase.Text = "TempDB"
        chkSqlServerUseIntegratedSecurity.Checked = True
        chkSqlServerUseExistingData.Checked = False

        txtSqlServerUsername.Text = "user"
        txtSqlServerPassword.Text = String.Empty

        Me.Width = 960
        Me.Height = 780

        mCustomValidationRulesFilePath = String.Empty

        Dim strSettingsFilePath = GetSettingsFilePath()
        PRISM.FileProcessor.ProcessFilesBase.CreateSettingsFileIfMissing(strSettingsFilePath)

    End Sub

    Private Sub SelectInputFile()

        Dim objOpenFile As New OpenFileDialog

        Dim currentExtension = String.Empty
        If txtProteinInputFilePath.TextLength > 0 Then
            Try
                currentExtension = Path.GetExtension(GetProteinInputFilePath())
            Catch ex As Exception
                ' Ignore errors here
            End Try
        End If

        With objOpenFile
            .AddExtension = True
            .CheckFileExists = False
            .CheckPathExists = True
            .DefaultExt = ".txt"
            .DereferenceLinks = True
            .Multiselect = False
            .ValidateNames = True

            .Filter = "Fasta files (*.fasta)|*.fasta|Text files (*.txt)|*.txt|All files (*.*)|*.*"

            If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.DelimitedText Then
                .FilterIndex = 2
            ElseIf currentExtension.ToLower() = ".txt" Then
                .FilterIndex = 2
            Else
                .FilterIndex = 1
            End If

            If Len(GetProteinInputFilePath().Length) > 0 Then
                Try
                    .InitialDirectory = Directory.GetParent(GetProteinInputFilePath()).FullName
                Catch
                    .InitialDirectory = GetMyDocsFolderPath()
                End Try
            Else
                .InitialDirectory = GetMyDocsFolderPath()
            End If

            .Title = "Select input file"

            .ShowDialog()
            If .FileName.Length > 0 Then
                txtProteinInputFilePath.Text = .FileName
            End If
        End With

    End Sub

    Private Sub SelectOutputFile()

        Dim objSaveFile As New SaveFileDialog

        With objSaveFile
            .AddExtension = True
            .CheckFileExists = False
            .CheckPathExists = True
            .CreatePrompt = False
            .DefaultExt = ".txt"
            .DereferenceLinks = True
            .OverwritePrompt = True
            .ValidateNames = True

            .Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            .FilterIndex = 1
            If Len(txtProteinOutputFilePath.Text.Length) > 0 Then
                Try
                    .InitialDirectory = Directory.GetParent(txtProteinOutputFilePath.Text).ToString
                Catch
                    .InitialDirectory = GetMyDocsFolderPath()
                End Try
            Else
                .InitialDirectory = GetMyDocsFolderPath()
            End If

            .Title = "Select/Create output file"

            .ShowDialog()
            If .FileName.Length > 0 Then
                txtProteinOutputFilePath.Text = .FileName
            End If
        End With

    End Sub

    Private Sub ShowAboutBox()
        Dim strMessage As String

        strMessage = String.Empty

        strMessage &= "Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004" & ControlChars.NewLine
        strMessage &= "Copyright 2018 Battelle Memorial Institute" & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= "This is version " & Application.ProductVersion & " (" & PROGRAM_DATE & ")" & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= "E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov" & ControlChars.NewLine
        strMessage &= "Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/" & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= frmDisclaimer.GetKangasPetritisDisclaimerText() & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= "Licensed under the 2-Clause BSD License; https://opensource.org/licenses/BSD-2-Clause" & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= "This software is provided by the copyright holders and contributors ""as is"" and "
        strMessage &= "any express or implied warranties, including, but not limited to, the implied "
        strMessage &= "warranties of merchantability and fitness for a particular purpose are "
        strMessage &= "disclaimed. In no event shall the copyright holder or contributors be liable "
        strMessage &= "for any direct, indirect, incidental, special, exemplary, or consequential "
        strMessage &= "damages (including, but not limited to, procurement of substitute goods or "
        strMessage &= "services; loss of use, data, or profits; or business interruption) however "
        strMessage &= "caused and on any theory of liability, whether in contract, strict liability, "
        strMessage &= "or tort (including negligence or otherwise) arising in any way out of the use "
        strMessage &= "of this software, even if advised of the possibility of such damage."

        MessageBox.Show(strMessage, "About", MessageBoxButtons.OK, MessageBoxIcon.Information)

    End Sub

    Private Sub ShowElutionTimeInfo()
        MessageBox.Show(objNETCalculator.ProgramDescription, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub ShowErrorMessage(strMessage As String)
        ShowErrorMessage(strMessage, "Error")
    End Sub

    Private Sub ShowErrorMessage(strMessage As String, strCaption As String)
        ShowErrorMessage(strMessage, strCaption)
    End Sub

    Private Sub ShowSplashScreen()

        ' See if the user has been shown the splash screen sometime in the last 6 months (SPLASH_INTERVAL_DAYS)
        ' Keep track of the last splash screen display date using the registry
        ' The data is stored in key HKEY_CURRENT_USER\Software\VB and VBA Program Settings\PNNL_ProteinDigestionSimulator\Options
        '
        ' If the current user cannot update the registry due to permissions errors, then we will not show
        ' the splash screen (so that they don't end up seeing the splash every time the program runs)

        Const APP_NAME_IN_REGISTRY = "PNNL_ProteinDigestionSimulator"
        Const REG_SECTION_OPTIONS = "Options"
        Const REG_KEY_SPLASHDATE = "SplashDate"
        Const DEFAULT_DATE = #1/1/2000#

        Const SPLASH_INTERVAL_DAYS = 182

        Dim strLastSplashDate As String
        Dim dtLastSplashDate As DateTime = DEFAULT_DATE
        Dim dtCurrentDateTime As DateTime = DateTime.Now()

        Try
            strLastSplashDate = GetSetting(APP_NAME_IN_REGISTRY, REG_SECTION_OPTIONS, REG_KEY_SPLASHDATE, "")
        Catch ex As Exception
            ' Error looking up the last splash date; don't continue
            Exit Sub
        End Try

        If strLastSplashDate Is Nothing Then strLastSplashDate = String.Empty

        If strLastSplashDate.Length > 0 Then
            Try
                ' Convert the text to a date
                dtLastSplashDate = DateTime.Parse(strLastSplashDate)
            Catch ex As Exception
                ' Conversion failed
                strLastSplashDate = String.Empty
                dtLastSplashDate = DEFAULT_DATE
            End Try
        End If

        If strLastSplashDate = String.Empty Then
            ' Entry isn't present (or it is present, but isn't the correct format)
            ' Try to add it
            Try
                SaveSetting(APP_NAME_IN_REGISTRY, REG_SECTION_OPTIONS, REG_KEY_SPLASHDATE, dtLastSplashDate.ToShortDateString)
            Catch ex As Exception
                ' Error adding the splash date; don't continue
                Exit Sub
            End Try
        End If

        If dtCurrentDateTime.Subtract(dtLastSplashDate).TotalDays >= SPLASH_INTERVAL_DAYS Then
            Try
                dtLastSplashDate = dtCurrentDateTime
                SaveSetting(APP_NAME_IN_REGISTRY, REG_SECTION_OPTIONS, REG_KEY_SPLASHDATE, dtLastSplashDate.ToShortDateString)

                ' Now make sure the setting actually saved
                strLastSplashDate = GetSetting(APP_NAME_IN_REGISTRY, REG_SECTION_OPTIONS, REG_KEY_SPLASHDATE, "")
                dtLastSplashDate = DateTime.Parse(strLastSplashDate)

                If dtLastSplashDate.ToShortDateString <> dtCurrentDateTime.ToShortDateString Then
                    ' Error saving/retrieving date; don't continue
                    Exit Sub
                End If

            Catch ex As Exception
                ' Error saving the new splash date; don't continue
                Exit Sub
            End Try

            Dim objSplashForm As New frmDisclaimer
            objSplashForm.ShowDialog()

        End If

    End Sub

    Private Sub SwitchToProgressTab()

        mTabPageIndexSaved = tbsOptions.SelectedIndex

        tbsOptions.SelectedIndex = PROGRESS_TAB_INDEX
        Application.DoEvents()

    End Sub

    Private Sub SwitchFromProgressTab()
        ' Wait 500 msec, then switch from the progress tab back to the tab that was visible before we started, but only if the current tab is the progress tb

        If tbsOptions.SelectedIndex = PROGRESS_TAB_INDEX Then
            tbsOptions.SelectedIndex = mTabPageIndexSaved
            Application.DoEvents()
        End If
    End Sub

    Private Sub UpdatePeptideUniquenessMassMode()
        If cboElementMassMode.SelectedIndex = PeptideSequenceClass.ElementModeConstants.AverageMass Then
            lblPeptideUniquenessMassMode.Text = "Using average masses"
        Else
            lblPeptideUniquenessMassMode.Text = "Using monoisotopic masses"
        End If
    End Sub

    Private Sub ValidateFastaFile(strFastaFilePath As String)

        Try
            ' Make sure an existing file has been chosen
            If strFastaFilePath Is Nothing OrElse strFastaFilePath.Length = 0 Then Exit Try

            If Not File.Exists(strFastaFilePath) Then
                ShowErrorMessage("File not found: " & strFastaFilePath, "Error")
            Else
                If mFastaValidation Is Nothing Then
                    mFastaValidation = New frmFastaValidation(strFastaFilePath)
                Else
                    mFastaValidation.SetNewFastaFile(strFastaFilePath)
                End If

                Try
                    If Not mCustomValidationRulesFilePath Is Nothing AndAlso mCustomValidationRulesFilePath.Length > 0 Then
                        If File.Exists(mCustomValidationRulesFilePath) Then
                            mFastaValidation.CustomRulesFilePath = mCustomValidationRulesFilePath
                        Else
                            mCustomValidationRulesFilePath = String.Empty
                        End If
                    End If
                Catch ex As Exception
                    ShowErrorMessage("Error trying to validate or set the custom validation rules file path: " & mCustomValidationRulesFilePath & "; " & ex.Message, "Error")
                End Try

                If mFastaValidationOptions.Initialized Then
                    mFastaValidation.SetOptions(mFastaValidationOptions)
                End If

                mFastaValidation.ShowDialog()

                ' Note that mFastaValidation.GetOptions() will be called when event FastaValidationStarted fires

            End If

        Catch ex As Exception
            ShowErrorMessage("Error occurred in frmFastaValidation: " & ex.Message, "Error")
        Finally
            If Not mFastaValidation Is Nothing Then
                mCustomValidationRulesFilePath = mFastaValidation.CustomRulesFilePath
            End If
        End Try

    End Sub

    Private Function ValidateSqlServerCachingOptionsForInputFile(strInputFilePath As String, blnAssumeDigested As Boolean, ByRef objProteinFileParser As clsParseProteinFile) As Boolean
        ' Returns True if the user OK's or updates the current Sql Server caching options
        ' Returns False if the user cancels processing
        ' Assumes that strInputFilePath exists, and thus does not have a Try-Catch block

        Const SAMPLING_LINE_COUNT = 10000

        Dim blnIsFastaFile As Boolean

        Dim fiFileInfo As FileInfo
        Dim intFileSizeKB As Integer

        Dim strLineIn As String
        Dim intLineCount As Integer
        Dim intTotalLineCount As Integer
        Dim lngBytesRead As Long

        Dim blnProceed As Boolean
        Dim blnSuggestEnableSqlServer = False
        Dim blnSuggestDisableSqlServer = False

        Dim eResponse As DialogResult

        blnIsFastaFile = clsParseProteinFile.IsFastaFile(strInputFilePath) Or objProteinFileParser.AssumeFastaFile

        ' Lookup the file size
        fiFileInfo = New FileInfo(strInputFilePath)
        intFileSizeKB = CType(fiFileInfo.Length / 1024.0, Integer)

        If blnIsFastaFile Then
            If objProteinFileParser.DigestionOptions.CleavageRuleID = clsInSilicoDigest.CleavageRuleConstants.KROneEnd Or
               objProteinFileParser.DigestionOptions.CleavageRuleID = clsInSilicoDigest.CleavageRuleConstants.NoRule Then
                blnSuggestEnableSqlServer = True
            ElseIf intFileSizeKB > 500 Then
                blnSuggestEnableSqlServer = True
            ElseIf intFileSizeKB <= 500 Then
                blnSuggestDisableSqlServer = True
            End If
        Else
            ' Assume a delimited text file
            ' Estimate the total line count by reading the first SAMPLING_LINE_COUNT lines
            Try
                Using srStreamReader = New StreamReader(strInputFilePath)

                    lngBytesRead = 0
                    intLineCount = 0
                    Do While srStreamReader.Peek() >= 0 AndAlso intLineCount < SAMPLING_LINE_COUNT
                        strLineIn = srStreamReader.ReadLine
                        intLineCount += 1
                        lngBytesRead += strLineIn.Length + 2
                    Loop

                    If intLineCount < SAMPLING_LINE_COUNT OrElse lngBytesRead = 0 Then
                        intTotalLineCount = intLineCount
                    Else
                        intTotalLineCount = CInt(intLineCount * intFileSizeKB / (lngBytesRead / 1024))
                    End If
                End Using

            Catch ex As Exception
                ' Error reading input file
                blnSuggestEnableSqlServer = False
                blnSuggestDisableSqlServer = False
            End Try

            If blnAssumeDigested Then
                If intTotalLineCount > 50000 Then
                    blnSuggestEnableSqlServer = True
                ElseIf intTotalLineCount <= 50000 Then
                    blnSuggestDisableSqlServer = True
                End If
            Else
                If intTotalLineCount > 1000 Then
                    blnSuggestEnableSqlServer = True
                ElseIf intTotalLineCount <= 1000 Then
                    blnSuggestDisableSqlServer = True
                End If

            End If
        End If

        If blnSuggestEnableSqlServer And Not chkUseSqlServerDBToCacheData.Checked Then
            eResponse = MessageBox.Show("Warning, memory usage could be quite large.  Enable Sql Server caching using Server " & txtSqlServerName.Text & "?  If no, then will continue using memory caching.", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
            If eResponse = DialogResult.Yes Then chkUseSqlServerDBToCacheData.Checked = True
            If eResponse = DialogResult.Cancel Then
                blnProceed = False
            Else
                blnProceed = True
            End If
        ElseIf blnSuggestDisableSqlServer And chkUseSqlServerDBToCacheData.Checked Then
            eResponse = MessageBox.Show("Memory usage is expected to be minimal.  Continue caching data using Server " & txtSqlServerName.Text & "?  If no, then will switch to using memory caching.", "Note", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            If eResponse = DialogResult.No Then chkUseSqlServerDBToCacheData.Checked = False
            If eResponse = DialogResult.Cancel Then
                blnProceed = False
            Else
                blnProceed = True
            End If
        Else
            blnProceed = True
        End If

        Return blnProceed

    End Function

    Private Sub ValidateTextbox(ByRef ThisTextBox As TextBox, strDefaultText As String)
        If ThisTextBox.TextLength = 0 Then
            ThisTextBox.Text = strDefaultText
        End If
    End Sub
#End Region

#Region "Control Handlers"

    Private Sub cboElementMassMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboElementMassMode.SelectedIndexChanged
        UpdatePeptideUniquenessMassMode()
    End Sub

    Private Sub cboHydrophobicityMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboHydrophobicityMode.SelectedIndexChanged
        ComputeSequencepI()
    End Sub
    Private Sub cboInputFileColumnDelimiter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboInputFileColumnDelimiter.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub cboInputFileFormat_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboInputFileFormat.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub cboProteinReversalOptions_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboProteinReversalOptions.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub chkAllowSqlServerCaching_CheckedChanged(sender As Object, e As EventArgs) Handles chkAllowSqlServerCaching.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkAutoDefineSLiCScoreTolerances_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoDefineSLiCScoreTolerances.CheckedChanged
        UpdateDataGridTableStyle()
    End Sub

    Private Sub chkComputepI_CheckedChanged(sender As Object, e As EventArgs) Handles chkComputepIandNET.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkComputeSequenceHashValues_CheckedChanged(sender As Object, e As EventArgs) Handles chkComputeSequenceHashValues.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkDigestProteins_CheckedChanged(sender As Object, e As EventArgs) Handles chkDigestProteins.CheckedChanged
        EnableDisableControls()
        AutoDefineOutputFile()
    End Sub

    Private Sub chkUseSLiCScoreForUniqueness_CheckedChanged(sender As Object, e As EventArgs) Handles chkUseSLiCScoreForUniqueness.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkUseSqlServerDBToCacheData_CheckedChanged(sender As Object, e As EventArgs) Handles chkUseSqlServerDBToCacheData.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkSqlServerUseIntegratedSecurity_CheckedChanged(sender As Object, e As EventArgs) Handles chkSqlServerUseIntegratedSecurity.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub cboOutputFileFieldDelimiter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboOutputFileFieldDelimiter.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub cboRefEndChar_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboRefEndChar.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub chkAutoComputeRangeForBinning_CheckedChanged(sender As Object, e As EventArgs) Handles chkAutoComputeRangeForBinning.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub cmdAbortProcessing_Click(sender As Object, e As EventArgs) Handles cmdAbortProcessing.Click
        AbortProcessingNow()
    End Sub

    Private Sub cmdClearPMThresholdsList_Click(sender As Object, e As EventArgs) Handles cmdClearPMThresholdsList.Click
        ClearPMThresholdsList(True)
    End Sub

    Private Sub chkCreateFastaOutputFile_CheckedChanged(sender As Object, e As EventArgs) Handles chkCreateFastaOutputFile.CheckedChanged
        AutoDefineOutputFile()
    End Sub

    Private Sub chkLookForAddnlRefInDescription_CheckedChanged(sender As Object, e As EventArgs) Handles chkLookForAddnlRefInDescription.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkMaxpIModeEnabled_CheckedChanged(sender As Object, e As EventArgs) Handles chkMaxpIModeEnabled.CheckedChanged
        EnableDisableControls()
        ComputeSequencepI()
    End Sub

    Private Sub cmdGenerateUniquenessStats_Click(sender As Object, e As EventArgs) Handles cmdGenerateUniquenessStats.Click
        GenerateUniquenessStats()
    End Sub

    Private Sub cmdNETInfo_Click(sender As Object, e As EventArgs) Handles cmdNETInfo.Click
        ShowElutionTimeInfo()
    End Sub

    Private Sub cmdParseInputFile_Click(sender As Object, e As EventArgs) Handles cmdParseInputFile.Click
        ParseProteinInputFile()
    End Sub

    Private Sub cmdPastePMThresholdsList_Click(sender As Object, e As EventArgs) Handles cmdPastePMThresholdsList.Click
        PastePMThresholdsValues(False)
    End Sub

    Private Sub cmdPMThresholdsAutoPopulate_Click(sender As Object, e As EventArgs) Handles cmdPMThresholdsAutoPopulate.Click
        If cboPMPredefinedThresholds.SelectedIndex >= 0 Then
            AutoPopulatePMThresholdsByID(CType(cboPMPredefinedThresholds.SelectedIndex, PredefinedPMThresholdsConstants), True)
        End If
    End Sub

    Private Sub cmdSelectFile_Click(sender As Object, e As EventArgs) Handles cmdSelectFile.Click
        SelectInputFile()
    End Sub

    Private Sub cmdSelectOutputFile_Click(sender As Object, e As EventArgs) Handles cmdSelectOutputFile.Click
        SelectOutputFile()
    End Sub

    Private Sub cmdValidateFastaFile_Click(sender As Object, e As EventArgs) Handles cmdValidateFastaFile.Click
        ValidateFastaFile(GetProteinInputFilePath())
    End Sub

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Note that InitializeControls() is called in Sub New()
    End Sub

    Private Sub frmMain_Closing(sender As Object, e As CancelEventArgs) Handles MyBase.Closing
        IniFileSaveOptions(True)
    End Sub

    Private Sub txtDigestProteinsMinimumMass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDigestProteinsMinimumMass.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtDigestProteinsMinimumMass, e, True)
    End Sub

    Private Sub txtDigestProteinsMaximumMissedCleavages_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDigestProteinsMaximumMissedCleavages.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtDigestProteinsMaximumMissedCleavages, e, True)
    End Sub

    Private Sub txtDigestProteinsMinimumResidueCount_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDigestProteinsMinimumResidueCount.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtDigestProteinsMinimumResidueCount, e, True)
    End Sub

    Private Sub txtDigestProteinsMaximumMass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDigestProteinsMaximumMass.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtDigestProteinsMaximumMass, e, True)
    End Sub

    Private Sub txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtMaxPeakMatchingResultsPerFeatureToSave.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtMaxPeakMatchingResultsPerFeatureToSave, e, True)
    End Sub

    Private Sub txtMaxPeakMatchingResultsPerFeatureToSave_Validating(sender As Object, e As CancelEventArgs) Handles txtMaxPeakMatchingResultsPerFeatureToSave.Validating
        If txtMaxPeakMatchingResultsPerFeatureToSave.Text.Trim = "0" Then txtMaxPeakMatchingResultsPerFeatureToSave.Text = "1"
        VBNetRoutines.ValidateTextboxInt(txtMaxPeakMatchingResultsPerFeatureToSave, 1, 100, 3)
    End Sub

    Private Sub txtMaxpISequenceLength_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMaxpISequenceLength.KeyDown
        If e.KeyCode = Keys.Enter AndAlso chkMaxpIModeEnabled.Checked Then ComputeSequencepI()
    End Sub

    Private Sub txtMaxpISequenceLength_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtMaxpISequenceLength.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtMaxpISequenceLength, e, True, False)
    End Sub

    Private Sub txtMaxpISequenceLength_Validating(sender As Object, e As CancelEventArgs) Handles txtMaxpISequenceLength.Validating
        VBNetRoutines.ValidateTextboxInt(txtMaxpISequenceLength, 1, 10000, 10)
    End Sub

    Private Sub txtMaxpISequenceLength_Validated(sender As Object, e As EventArgs) Handles txtMaxpISequenceLength.Validated
        ComputeSequencepI()
    End Sub

    Private Sub txtMinimumSLiCScore_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtMinimumSLiCScore.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtMinimumSLiCScore, e, True, True)
    End Sub

    Private Sub txtMinimumSLiCScore_Validating(sender As Object, e As CancelEventArgs) Handles txtMinimumSLiCScore.Validating
        VBNetRoutines.ValidateTextboxSng(txtMinimumSLiCScore, 0, 1, 0.95)
    End Sub

    Private Sub txtProteinInputFilePath_TextChanged(sender As Object, e As EventArgs) Handles txtProteinInputFilePath.TextChanged
        EnableDisableControls()
        AutoDefineOutputFile()
    End Sub

    Private Sub txtProteinReversalSamplingPercentage_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtProteinReversalSamplingPercentage.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtProteinReversalSamplingPercentage, e, True)
    End Sub

    Private Sub txtProteinScramblingLoopCount_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtProteinScramblingLoopCount.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtProteinScramblingLoopCount, e, True)
    End Sub

    Private Sub txtSequenceForpI_TextChanged(sender As Object, e As EventArgs) Handles txtSequenceForpI.TextChanged
        ComputeSequencepI()
    End Sub

    Private Sub txtUniquenessBinWidth_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUniquenessBinWidth.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtUniquenessBinWidth, e, True)
    End Sub

    Private Sub txtUniquenessBinStartMass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUniquenessBinStartMass.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtUniquenessBinStartMass, e, True)
    End Sub

    Private Sub txtUniquenessBinEndMass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUniquenessBinEndMass.KeyPress
        VBNetRoutines.TextBoxKeyPressHandler(txtUniquenessBinEndMass, e, True)
    End Sub

#End Region

#Region "Menu Handlers"
    Private Sub mnuFileSelectInputFile_Click(sender As Object, e As EventArgs) Handles mnuFileSelectInputFile.Click
        SelectInputFile()
    End Sub

    Private Sub mnuFileSelectOutputFile_Click(sender As Object, e As EventArgs) Handles mnuFileSelectOutputFile.Click
        SelectOutputFile()
    End Sub

    Private Sub mnuFileSaveDefaultOptions_Click(sender As Object, e As EventArgs) Handles mnuFileSaveDefaultOptions.Click
        IniFileSaveOptions()
    End Sub

    Private Sub mnuFileExit_Click(sender As Object, e As EventArgs) Handles mnuFileExit.Click
        Me.Close()
    End Sub

    Private Sub mnuEditMakeUniquenessStats_Click(sender As Object, e As EventArgs) Handles mnuEditMakeUniquenessStats.Click
        GenerateUniquenessStats()
    End Sub

    Private Sub mnuEditParseFile_Click(sender As Object, e As EventArgs) Handles mnuEditParseFile.Click
        ParseProteinInputFile()
    End Sub

    Private Sub mnuEditResetOptions_Click(sender As Object, e As EventArgs) Handles mnuEditResetOptions.Click
        ResetToDefaults(True)
    End Sub

    Private Sub mnuHelpAbout_Click(sender As Object, e As EventArgs) Handles mnuHelpAbout.Click
        ShowAboutBox()
    End Sub

    Private Sub mnuHelpAboutElutionTime_Click(sender As Object, e As EventArgs) Handles mnuHelpAboutElutionTime.Click
        ShowElutionTimeInfo()
    End Sub

#End Region

#Region "Event Handlers"

    Private Sub mFastaValidation_FastaValidationStarted() Handles mFastaValidation.FastaValidationStarted
        mFastaValidationOptions = mFastaValidation.GetOptions()
    End Sub

    Private Sub mParseProteinFile_ErrorEvent(strMessage As String, ex As Exception) Handles mParseProteinFile.ErrorEvent
        lblErrorMessage.Text = "Error in mParseProteinFile: " & strMessage
        Application.DoEvents()
    End Sub

    Private Sub mParseProteinFile_ProgressChanged(taskDescription As String, percentComplete As Single) Handles mParseProteinFile.ProgressUpdate
        lblProgressDescription.Text = taskDescription
        lblProgress.Text = FormatPercentComplete(percentComplete)
        pbarProgress.Value = CInt(percentComplete)
        Application.DoEvents()
    End Sub

    Private Sub mParseProteinFile_ProgressComplete() Handles mParseProteinFile.ProgressComplete
        lblProgressDescription.Text = "Processing complete"
        lblProgress.Text = FormatPercentComplete(100)
        pbarProgress.Value = 100

        lblSubtaskProgress.Text = ""
        lblSubtaskProgressDescription.Text = ""
        Me.SubtaskProgressIsVisible = False

        Application.DoEvents()
    End Sub

    Private Sub mParseProteinFile_ProgressReset() Handles mParseProteinFile.ProgressReset
        ResetProgress(False)
    End Sub

    Private Sub mParseProteinFile_SubtaskProgressChanged(taskDescription As String, percentComplete As Single) Handles mParseProteinFile.SubtaskProgressChanged
        lblSubtaskProgressDescription.Text = taskDescription
        lblSubtaskProgress.Text = FormatPercentComplete(percentComplete)
        SubtaskProgressIsVisible = True
        Application.DoEvents()
    End Sub

    Private Sub mProteinDigestionSimulator_ErrorEvent(strMessage As String, ex As Exception) Handles mProteinDigestionSimulator.ErrorEvent
        lblErrorMessage.Text = "Error in mProteinDigestionSimulator: " & strMessage
        Application.DoEvents()
    End Sub

    Private Sub mProteinDigestionSimulator_ProgressChanged(taskDescription As String, percentComplete As Single) Handles mProteinDigestionSimulator.ProgressUpdate
        lblProgressDescription.Text = taskDescription
        lblProgress.Text = FormatPercentComplete(percentComplete)
        pbarProgress.Value = CInt(percentComplete)
        Application.DoEvents()
    End Sub

    Private Sub mProteinDigestionSimulator_ProgressComplete() Handles mProteinDigestionSimulator.ProgressComplete
        lblProgressDescription.Text = "Processing complete"
        lblProgress.Text = FormatPercentComplete(100)
        pbarProgress.Value = 100

        lblSubtaskProgress.Text = ""
        lblSubtaskProgressDescription.Text = ""
        Me.SubtaskProgressIsVisible = False

        Application.DoEvents()
    End Sub

    Private Sub mProteinDigestionSimulator_ProgressReset() Handles mProteinDigestionSimulator.ProgressReset
        ResetProgress(False)
    End Sub

    Private Sub mProteinDigestionSimulator_SubtaskProgressChanged(taskDescription As String, percentComplete As Single) Handles mProteinDigestionSimulator.SubtaskProgressChanged
        lblSubtaskProgressDescription.Text = taskDescription
        lblSubtaskProgress.Text = FormatPercentComplete(percentComplete)
        SubtaskProgressIsVisible = True
        Application.DoEvents()
    End Sub

#End Region

End Class