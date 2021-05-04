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
Imports System.Text
Imports NETPrediction
Imports PRISM
Imports PRISM.FileProcessor
Imports PRISMWin
Imports ProteinFileReader
Imports DBUtils = PRISMDatabaseUtils.DataTableUtils

Public Class frmMain

    ' Ignore Spelling: al, cbo, chk, combobox, ComputepI, const, CrLf, Cys, Da, diff, Eisenberg, Engleman
    ' Ignore Spelling: frm, gauging, Hopp, Hydrophobicity, Iodoacetamide, Iodoacetic, Kangas, Kostas, Kyte
    ' Ignore Spelling: MaximumpI, MaxpI, MinimumpI, Petritis, Sep, Sql, tryptic

    Public Sub New()
        MyBase.New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        mDefaultFastaFileOptions = New clsParseProteinFile.FastaFileOptionsClass() With {
            .ReadonlyClass = True
            }

        pICalculator = New clspICalculation()
        NETCalculator = New ElutionTimePredictionKangas()
        SCXNETCalculator = New SCXElutionTimePredictionKangas()

        mCleavageRuleComboboxIndexToType = New Dictionary(Of Integer, clsInSilicoDigest.CleavageRuleConstants)

        InitializeControls()
    End Sub

    Private Const XML_SETTINGS_FILE_NAME As String = "ProteinDigestionSimulatorOptions.xml"

    Private Const OUTPUT_FILE_SUFFIX As String = "_output.txt"                         ' Note that this const starts with an underscore
    Private Const PEAK_MATCHING_STATS_FILE_SUFFIX As String = "_PeakMatching.txt"      ' Note that this const starts with an underscore

    Private Const PM_THRESHOLDS_DATA_TABLE As String = "PeakMatchingThresholds"

    Private Const COL_NAME_MASS_TOLERANCE As String = "MassTolerance"
    Private Const COL_NAME_NET_TOLERANCE As String = "NETTolerance"
    Private Const COL_NAME_SLIC_MASS_STDEV As String = "SLiCMassStDev"
    Private Const COL_NAME_SLIC_NET_STDEV As String = "SLiCNETStDev"
    Private Const COL_NAME_PM_THRESHOLD_ROW_ID As String = "PMThresholdRowID"

    Private Const DEFAULT_SLIC_MASS_STDEV As Double = 3
    Private Const DEFAULT_SLIC_NET_STDEV As Double = 0.025

    Private Const PROGRESS_TAB_INDEX As Integer = 4

    ''' <summary>
    ''' Input file format constants
    ''' </summary>
    Private Enum InputFileFormatConstants
        AutoDetermine = 0
        FastaFile = 1
        DelimitedText = 2
    End Enum

    Private Const PREDEFINED_PM_THRESHOLDS_COUNT As Integer = 5

    ''' <summary>
    ''' Predefined peak matching threshold constants
    ''' </summary>
    Private Enum PredefinedPMThresholdsConstants
        OneMassOneNET = 0
        OneMassThreeNET = 1
        ThreeMassOneNET = 2
        ThreeMassThreeNET = 3
        FiveMassThreeNET = 4
    End Enum

    Private Structure udtPeakMatchingThresholdsType
        Public MassTolerance As Double
        Public NETTolerance As Double
    End Structure

    Private Structure udtPredefinedPMThresholdsType
        Public MassTolType As clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants
        Public Thresholds() As udtPeakMatchingThresholdsType
    End Structure

    ' The following is used to lookup the default symbols for Fasta files, and should thus be treated as ReadOnly
    Private ReadOnly mDefaultFastaFileOptions As clsParseProteinFile.FastaFileOptionsClass

    Private mPeakMatchingThresholdsDataset As DataSet
    Private mPredefinedPMThresholds() As udtPredefinedPMThresholdsType

    Private mWorking As Boolean
    Private mCustomValidationRulesFilePath As String

    Private ReadOnly pICalculator As clspICalculation

    Private ReadOnly NETCalculator As ElutionTimePredictionKangas

    Private ReadOnly SCXNETCalculator As SCXElutionTimePredictionKangas

    ''' <summary>
    ''' Keys in this dictionary are the index in combobox cboCleavageRuleType, values are the cleavage rule enum for that index
    ''' </summary>
    Private ReadOnly mCleavageRuleComboboxIndexToType As Dictionary(Of Integer, clsInSilicoDigest.CleavageRuleConstants)

    Private mTabPageIndexSaved As Integer = 0

    Private mFastaValidationOptions As frmFastaValidation.udtFastaValidationOptionsType

    Private WithEvents mParseProteinFile As clsParseProteinFile

    Private WithEvents mProteinDigestionSimulator As clsProteinDigestionSimulator

    Private WithEvents mFastaValidation As frmFastaValidation


    Private Sub AbortProcessingNow()
        Try
            If mParseProteinFile IsNot Nothing Then
                mParseProteinFile.AbortProcessingNow()
            End If

            If mProteinDigestionSimulator IsNot Nothing Then
                mProteinDigestionSimulator.AbortProcessingNow()
            End If
        Catch ex As Exception
            ' Ignore errors here
        End Try
    End Sub

    Private Sub AddPMThresholdRow(massThreshold As Double, netThreshold As Double, Optional ByRef existingRowFound As Boolean = False)
        AddPMThresholdRow(massThreshold, netThreshold, DEFAULT_SLIC_MASS_STDEV, DEFAULT_SLIC_NET_STDEV, existingRowFound)
    End Sub

    Private Sub AddPMThresholdRow(massThreshold As Double, netThreshold As Double, slicMassStDev As Double, slicNETStDev As Double, Optional ByRef existingRowFound As Boolean = False)

        For Each myDataRow As DataRow In mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATA_TABLE).Rows
            If Math.Abs(CDbl(myDataRow.Item(0)) - massThreshold) < 0.000001 AndAlso
                       Math.Abs(CDbl(myDataRow.Item(1)) - netThreshold) < 0.000001 Then
                existingRowFound = True
                Exit For
            End If
        Next myDataRow

        If Not existingRowFound Then
            Dim myDataRow As DataRow = mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATA_TABLE).NewRow()
            myDataRow(0) = massThreshold
            myDataRow(1) = netThreshold
            myDataRow(2) = slicMassStDev
            myDataRow(3) = slicNETStDev
            mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATA_TABLE).Rows.Add(myDataRow)
        End If
    End Sub

    Private Sub AppendEnzymeToCleavageRuleCombobox(inSilicoDigest As clsInSilicoDigest, cleavageRuleId As clsInSilicoDigest.CleavageRuleConstants)

        Dim cleavageRule As clsCleavageRule = Nothing
        inSilicoDigest.GetCleavageRuleById(cleavageRuleId, cleavageRule)

        If cleavageRule Is Nothing Then Return

        Dim targetIndex = cboCleavageRuleType.Items.Count
        cboCleavageRuleType.Items.Add(cleavageRule.Description & " (" & cleavageRule.GetDetailedRuleDescription() & ")")

        mCleavageRuleComboboxIndexToType.Add(targetIndex, cleavageRuleId)
    End Sub

    Private Sub AutoDefineOutputFile()
        Try
            If txtProteinInputFilePath.Text.Length > 0 Then
                txtProteinOutputFilePath.Text = AutoDefineOutputFileWork(GetProteinInputFilePath())
            End If
        Catch ex As Exception
            ' Leave the TextBox unchanged
        End Try
    End Sub

    Private Function AutoDefineOutputFileWork(inputFilePath As String) As String

        Dim inputFileName = clsParseProteinFile.StripExtension(Path.GetFileName(inputFilePath), ".gz")

        Dim outputFileName As String

        If chkCreateFastaOutputFile.Enabled AndAlso chkCreateFastaOutputFile.Checked Then
            If clsParseProteinFile.IsFastaFile(inputFilePath) Then
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

    Private Sub AutoPopulatePMThresholds(udtPredefinedThresholds As udtPredefinedPMThresholdsType, confirmReplaceExistingResults As Boolean)

        Dim index As Integer

        If ClearPMThresholdsList(confirmReplaceExistingResults) Then
            cboMassTolType.SelectedIndex = udtPredefinedThresholds.MassTolType

            For index = 0 To udtPredefinedThresholds.Thresholds.Length - 1
                AddPMThresholdRow(udtPredefinedThresholds.Thresholds(index).MassTolerance, udtPredefinedThresholds.Thresholds(index).NETTolerance)
            Next index

        End If

    End Sub

    Private Sub AutoPopulatePMThresholdsByID(ePredefinedPMThreshold As PredefinedPMThresholdsConstants, confirmReplaceExistingResults As Boolean)

        Try
            AutoPopulatePMThresholds(mPredefinedPMThresholds(ePredefinedPMThreshold), confirmReplaceExistingResults)
        Catch ex As Exception
            ShowErrorMessage("Error calling AutoPopulatePMThresholds in AutoPopulatePMThresholdsByID: " & ex.Message, "Error")
        End Try

    End Sub

    Private Function ClearPMThresholdsList(confirmReplaceExistingResults As Boolean) As Boolean
        ' Returns true if the PM_THRESHOLDS_DATA_TABLE is empty or if it was cleared
        ' Returns false if the user is queried about clearing and they do not click Yes

        Dim eResult As DialogResult
        Dim success As Boolean

        success = False
        If mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATA_TABLE).Rows.Count > 0 Then
            If confirmReplaceExistingResults Then
                eResult = MessageBox.Show("Are you sure you want to clear the thresholds?", "Clear Thresholds", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            End If

            If eResult = DialogResult.Yes OrElse Not confirmReplaceExistingResults Then
                mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATA_TABLE).Rows.Clear()
                success = True
            End If
        Else
            success = True
        End If

        Return success
    End Function

    Private Sub ComputeSequencepI()

        If txtSequenceForpI.TextLength = 0 Then Exit Sub

        Dim sequence = txtSequenceForpI.Text
        Dim pI As Single
        Dim hydrophobicity As Single
        Dim lcNET As Single
        Dim scxNET As Single

        If pICalculator IsNot Nothing Then
            If cboHydrophobicityMode.SelectedIndex >= 0 Then
                pICalculator.HydrophobicityType = CType(cboHydrophobicityMode.SelectedIndex, clspICalculation.eHydrophobicityTypeConstants)
            End If

            pICalculator.ReportMaximumpI = chkMaxpIModeEnabled.Checked
            pICalculator.SequenceWidthToExamineForMaximumpI = LookupMaxpISequenceLength()


            pI = pICalculator.CalculateSequencepI(sequence)
            hydrophobicity = pICalculator.CalculateSequenceHydrophobicity(sequence)
            ' Could compute charge state: pICalculator.CalculateSequenceChargeState(sequence, pI)
        End If

        If NETCalculator IsNot Nothing Then
            ' Compute the LC-based normalized elution time
            lcNET = NETCalculator.GetElutionTime(sequence)
        End If

        If SCXNETCalculator IsNot Nothing Then
            ' Compute the SCX-based normalized elution time
            scxNET = SCXNETCalculator.GetElutionTime(sequence)
        End If

        Dim message = "pI = " & pI.ToString() & ControlChars.NewLine &
                      "Hydrophobicity = " & hydrophobicity.ToString() & ControlChars.NewLine &
                      "Predicted LC NET = " & lcNET.ToString("0.000") & ControlChars.NewLine &
                      "Predicted SCX NET = " & scxNET.ToString("0.000")
        ' "Predicted charge state = " & ControlChars.NewLine & charge.ToString() & " at pH = " & pI.ToString()

        txtpIStats.Text = message
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

        Dim index As Integer
        Dim massIndex, netIndex As Integer

        Dim netValues() As Double
        Dim massValues() As Double

        ReDim mPredefinedPMThresholds(PREDEFINED_PM_THRESHOLDS_COUNT - 1)

        ' All of the predefined thresholds have mass tolerances in units of PPM
        For index = 0 To PREDEFINED_PM_THRESHOLDS_COUNT - 1
            mPredefinedPMThresholds(index).MassTolType = clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.PPM
            ReDim mPredefinedPMThresholds(index).Thresholds(-1)
        Next index

        ReDim netValues(2)
        netValues(0) = 0.01
        netValues(1) = 0.05
        netValues(2) = 100

        ReDim massValues(4)
        massValues(0) = 0.5
        massValues(1) = 1
        massValues(2) = 5
        massValues(3) = 10
        massValues(4) = 50

        ' OneMassOneNET
        DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(PredefinedPMThresholdsConstants.OneMassOneNET), 5, 0.05)

        ' OneMassThreeNET
        For netIndex = 0 To netValues.Length - 1
            DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(PredefinedPMThresholdsConstants.OneMassThreeNET), 5, netValues(netIndex))
        Next netIndex

        ' ThreeMassOneNET
        For massIndex = 0 To 2
            DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(PredefinedPMThresholdsConstants.ThreeMassOneNET), massValues(massIndex), 0.05)
        Next massIndex

        ' ThreeMassThreeNET
        For netIndex = 0 To netValues.Length - 1
            For massIndex = 0 To 2
                DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(PredefinedPMThresholdsConstants.ThreeMassThreeNET), massValues(massIndex), netValues(netIndex))
            Next massIndex
        Next netIndex

        ' FiveMassThreeNET
        For netIndex = 0 To netValues.Length - 1
            For massIndex = 0 To massValues.Length - 1
                DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds(PredefinedPMThresholdsConstants.FiveMassThreeNET), massValues(massIndex), netValues(netIndex))
            Next massIndex
        Next netIndex

    End Sub

    Private Sub DefineDefaultPMThresholdAppendItem(ByRef udtPMThreshold As udtPredefinedPMThresholdsType, massTolerance As Double, netTolerance As Double)

        Dim newIndex = udtPMThreshold.Thresholds.Length
        ReDim Preserve udtPMThreshold.Thresholds(newIndex)

        udtPMThreshold.Thresholds(newIndex).MassTolerance = massTolerance
        udtPMThreshold.Thresholds(newIndex).NETTolerance = netTolerance
    End Sub

    Private Sub EnableDisableControls()
        Dim enableDelimitedFileOptions As Boolean
        Dim enableDigestionOptions As Boolean
        Dim allowSqlServerCaching As Boolean

        Dim inputFilePath = GetProteinInputFilePath()
        Dim sourceIsFasta = clsParseProteinFile.IsFastaFile(inputFilePath)

        If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.DelimitedText Then
            enableDelimitedFileOptions = True
        ElseIf cboInputFileFormat.SelectedIndex = InputFileFormatConstants.FastaFile OrElse
           txtProteinInputFilePath.TextLength = 0 OrElse
           sourceIsFasta Then
            ' Fasta file (or blank)
            enableDelimitedFileOptions = False
        Else
            enableDelimitedFileOptions = True
        End If

        cboInputFileColumnDelimiter.Enabled = enableDelimitedFileOptions
        lblInputFileColumnDelimiter.Enabled = enableDelimitedFileOptions
        chkAssumeInputFileIsDigested.Enabled = enableDelimitedFileOptions

        txtInputFileColumnDelimiter.Enabled = (cboInputFileColumnDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Other) And enableDelimitedFileOptions

        enableDigestionOptions = chkDigestProteins.Checked
        If enableDigestionOptions Then
            cmdParseInputFile.Text = "&Parse and Digest"
        Else
            cmdParseInputFile.Text = "&Parse File"
        End If

        If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.FastaFile OrElse sourceIsFasta Then
            cmdValidateFastaFile.Enabled = True
        Else
            cmdValidateFastaFile.Enabled = False
        End If

        chkCreateFastaOutputFile.Enabled = Not enableDigestionOptions

        chkComputeSequenceHashIgnoreILDiff.Enabled = chkComputeSequenceHashValues.Checked

        fraDigestionOptions.Enabled = enableDigestionOptions
        chkIncludePrefixAndSuffixResidues.Enabled = enableDigestionOptions

        txtOutputFileFieldDelimiter.Enabled = (cboOutputFileFieldDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Other)

        enableDelimitedFileOptions = chkLookForAddnlRefInDescription.Checked
        txtAddnlRefSepChar.Enabled = enableDelimitedFileOptions
        txtAddnlRefAccessionSepChar.Enabled = enableDelimitedFileOptions

        txtUniquenessBinStartMass.Enabled = Not chkAutoComputeRangeForBinning.Checked
        txtUniquenessBinEndMass.Enabled = txtUniquenessBinStartMass.Enabled

        allowSqlServerCaching = chkAllowSqlServerCaching.Checked
        chkUseSqlServerDBToCacheData.Enabled = allowSqlServerCaching

        txtSqlServerDatabase.Enabled = chkUseSqlServerDBToCacheData.Checked And allowSqlServerCaching
        txtSqlServerName.Enabled = txtSqlServerDatabase.Enabled
        chkSqlServerUseIntegratedSecurity.Enabled = txtSqlServerDatabase.Enabled

        chkSqlServerUseExistingData.Enabled = chkSqlServerUseIntegratedSecurity.Checked And allowSqlServerCaching

        txtSqlServerUsername.Enabled = chkUseSqlServerDBToCacheData.Checked And Not chkSqlServerUseIntegratedSecurity.Checked And allowSqlServerCaching
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

        txtDigestProteinsMinimumpI.Enabled = enableDigestionOptions And chkComputepIandNET.Checked
        txtDigestProteinsMaximumpI.Enabled = enableDigestionOptions And chkComputepIandNET.Checked

    End Sub

    Private Function FormatPercentComplete(percentComplete As Single) As String
        Return percentComplete.ToString("0.0") & "% complete"
    End Function

    Private Sub GenerateUniquenessStats()

        If Not mWorking AndAlso ConfirmFilePaths() Then
            Try

                If mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATA_TABLE).Rows.Count = 0 Then
                    ShowErrorMessage("Please define one or more peak matching thresholds before proceeding.", "Error")
                    Exit Try
                End If

                mProteinDigestionSimulator = New clsProteinDigestionSimulator()
                If chkEnableLogging.Checked Then
                    mProteinDigestionSimulator.LogMessagesToFile = True

                    Dim appFolderPath = ProcessFilesOrDirectoriesBase.GetAppDataDirectoryPath("ProteinDigestionSimulator")
                    Dim logFilePath = Path.Combine(appFolderPath, "ProteinDigestionSimulatorLog.txt")
                    mProteinDigestionSimulator.LogFilePath = logFilePath
                End If

                Dim success = InitializeProteinFileParserGeneralOptions(mProteinDigestionSimulator.mProteinFileParser)
                If Not success Then Exit Try

                Dim outputFilePath = txtProteinOutputFilePath.Text

                If Not Path.IsPathRooted(outputFilePath) Then
                    outputFilePath = Path.Combine(GetMyDocsFolderPath(), outputFilePath)
                End If

                If Directory.Exists(outputFilePath) Then
                    ' outputFilePath points to a folder and not a file
                    outputFilePath = Path.Combine(outputFilePath, Path.GetFileNameWithoutExtension(GetProteinInputFilePath()) & PEAK_MATCHING_STATS_FILE_SUFFIX)
                Else
                    ' Replace _output.txt" in outputFilePath with PEAK_MATCHING_STATS_FILE_SUFFIX
                    Dim charIndex = outputFilePath.IndexOf(OUTPUT_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase)
                    If charIndex > 0 Then
                        outputFilePath = outputFilePath.Substring(0, charIndex) & PEAK_MATCHING_STATS_FILE_SUFFIX
                    Else
                        outputFilePath = Path.Combine(Path.GetDirectoryName(outputFilePath), Path.GetFileNameWithoutExtension(outputFilePath) & PEAK_MATCHING_STATS_FILE_SUFFIX)
                    End If
                End If

                ' Check input file size and possibly warn user to enable/disable SQL Server DB Usage
                If chkAllowSqlServerCaching.Checked Then
                    If Not ValidateSqlServerCachingOptionsForInputFile(GetProteinInputFilePath(), chkAssumeInputFileIsDigested.Checked, mProteinDigestionSimulator.mProteinFileParser) Then
                        Exit Try
                    End If
                End If

                Dim eMassToleranceType As clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants
                If cboMassTolType.SelectedIndex >= 0 Then
                    eMassToleranceType = CType(cboMassTolType.SelectedIndex, clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants)
                End If
                Dim autoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked

                Dim clearExisting = True
                For Each myDataRow As DataRow In mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATA_TABLE).Rows
                    If autoDefineSLiCScoreThresholds Then
                        mProteinDigestionSimulator.AddSearchThresholdLevel(eMassToleranceType, CDbl(myDataRow.Item(0)), CDbl(myDataRow.Item(1)), clearExisting)
                    Else
                        mProteinDigestionSimulator.AddSearchThresholdLevel(eMassToleranceType, CDbl(myDataRow.Item(0)), CDbl(myDataRow.Item(1)), False, CDbl(myDataRow.Item(2)), CDbl(myDataRow.Item(3)), True, clearExisting)
                    End If

                    clearExisting = False
                Next myDataRow

                mProteinDigestionSimulator.DigestSequences = Not chkAssumeInputFileIsDigested.Checked
                mProteinDigestionSimulator.CysPeptidesOnly = chkCysPeptidesOnly.Checked
                If cboElementMassMode.SelectedIndex >= 0 Then
                    mProteinDigestionSimulator.ElementMassMode = CType(cboElementMassMode.SelectedIndex, PeptideSequenceClass.ElementModeConstants)
                End If

                mProteinDigestionSimulator.AutoDetermineMassRangeForBinning = chkAutoComputeRangeForBinning.Checked

                Dim invalidValue As Boolean
                mProteinDigestionSimulator.PeptideUniquenessMassBinSizeForBinning = ParseTextBoxValueInt(txtUniquenessBinWidth, lblUniquenessBinWidth.Text & " must be an integer value", invalidValue)
                If invalidValue Then Exit Try

                If Not mProteinDigestionSimulator.AutoDetermineMassRangeForBinning Then
                    Dim binStartMass = ParseTextBoxValueInt(txtUniquenessBinStartMass, "Uniqueness binning start mass must be an integer value", invalidValue)
                    If invalidValue Then Exit Try

                    Dim binEndMass = ParseTextBoxValueInt(txtUniquenessBinEndMass, "Uniqueness binning end mass must be an integer value", invalidValue)
                    If invalidValue Then Exit Try

                    If Not mProteinDigestionSimulator.SetPeptideUniquenessMassRangeForBinning(binStartMass, binEndMass) Then
                        mProteinDigestionSimulator.AutoDetermineMassRangeForBinning = True
                    End If
                End If

                mProteinDigestionSimulator.MinimumSLiCScoreToBeConsideredUnique = ParseTextBoxValueSng(txtMinimumSLiCScore, lblMinimumSLiCScore.Text & " must be a value", invalidValue)
                If invalidValue Then Exit Try

                mProteinDigestionSimulator.MaxPeakMatchingResultsPerFeatureToSave = ParseTextBoxValueInt(txtMaxPeakMatchingResultsPerFeatureToSave, lblMaxPeakMatchingResultsPerFeatureToSave.Text & " must be an integer value", invalidValue)
                If invalidValue Then Exit Try

                mProteinDigestionSimulator.SavePeakMatchingResults = chkExportPeakMatchingResults.Checked
                mProteinDigestionSimulator.UseSLiCScoreForUniqueness = chkUseSLiCScoreForUniqueness.Checked
                mProteinDigestionSimulator.UseEllipseSearchRegion = optUseEllipseSearchRegion.Checked             ' Only applicable if mProteinDigestionSimulator.UseSLiCScoreForUniqueness = True

                Cursor.Current = Cursors.WaitCursor
                mWorking = True
                cmdGenerateUniquenessStats.Enabled = False

                ResetProgress()
                SwitchToProgressTab()

                success = mProteinDigestionSimulator.GenerateUniquenessStats(GetProteinInputFilePath(), Path.GetDirectoryName(outputFilePath), Path.GetFileNameWithoutExtension(outputFilePath))

                Cursor.Current = Cursors.Default

                If success Then
                    MessageBox.Show("Uniqueness stats calculation complete ", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    SwitchFromProgressTab()
                Else
                    ShowErrorMessage("Unable to Generate Uniqueness Stats: " & mProteinDigestionSimulator.GetErrorMessage, "Error")
                End If

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

    Private Function GetSelectedCleavageRule() As clsInSilicoDigest.CleavageRuleConstants

        Dim selectedIndex = cboCleavageRuleType.SelectedIndex
        Dim selectedCleavageRule = clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin

        If selectedIndex < 0 OrElse Not mCleavageRuleComboboxIndexToType.TryGetValue(selectedIndex, selectedCleavageRule) Then
            Return clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin
        End If

        Return selectedCleavageRule

    End Function

    Private Function GetSettingsFilePath() As String
        Return ProcessFilesOrDirectoriesBase.GetSettingsFilePathLocal("ProteinDigestionSimulator", XML_SETTINGS_FILE_NAME)
    End Function

    Private Sub IniFileLoadOptions()

        Const OptionsSection As String = clsParseProteinFile.XML_SECTION_OPTIONS
        Const FASTAOptions As String = clsParseProteinFile.XML_SECTION_FASTA_OPTIONS
        Const ProcessingOptions As String = clsParseProteinFile.XML_SECTION_PROCESSING_OPTIONS
        Const DigestionOptions As String = clsParseProteinFile.XML_SECTION_DIGESTION_OPTIONS
        Const UniquenessStatsOptions As String = clsParseProteinFile.XML_SECTION_UNIQUENESS_STATS_OPTIONS
        Const PMOptions As String = clsProteinDigestionSimulator.XML_SECTION_PEAK_MATCHING_OPTIONS

        Const MAX_AUTO_WINDOW_HEIGHT = 775

        Dim xmlSettings As New XmlSettingsFileAccessor()

        Dim autoDefineSLiCScoreThresholds As Boolean
        Dim valueNotPresent As Boolean
        Dim radioButtonChecked As Boolean

        Dim index As Integer
        Dim windowHeight As Integer

        Dim thresholdData As String
        Dim thresholds() As String
        Dim thresholdDetails() As String

        Dim columnDelimiters = New Char() {ControlChars.Tab, ","c}

        ResetToDefaults(False)
        Dim settingsFilePath = GetSettingsFilePath()

        Try

            ' Pass False to .LoadSettings() here to turn off case sensitive matching
            xmlSettings.LoadSettings(settingsFilePath, False)
            ProcessFilesOrDirectoriesBase.CreateSettingsFileIfMissing(settingsFilePath)

            If Not File.Exists(settingsFilePath) Then
                ShowErrorMessage("Parameter file not Found: " & settingsFilePath)
                Exit Sub
            End If

            Try
                txtProteinInputFilePath.Text = xmlSettings.GetParam(OptionsSection, "InputFilePath", GetProteinInputFilePath())

                cboInputFileFormat.SelectedIndex = xmlSettings.GetParam(OptionsSection, "InputFileFormatIndex", cboInputFileFormat.SelectedIndex)
                cboInputFileColumnDelimiter.SelectedIndex = xmlSettings.GetParam(OptionsSection, "InputFileColumnDelimiterIndex", cboInputFileColumnDelimiter.SelectedIndex)
                txtInputFileColumnDelimiter.Text = xmlSettings.GetParam(OptionsSection, "InputFileColumnDelimiter", txtInputFileColumnDelimiter.Text)

                cboInputFileColumnOrdering.SelectedIndex = xmlSettings.GetParam(OptionsSection, "InputFileColumnOrdering", cboInputFileColumnOrdering.SelectedIndex)

                cboOutputFileFieldDelimiter.SelectedIndex = xmlSettings.GetParam(OptionsSection, "OutputFileFieldDelimiterIndex", cboOutputFileFieldDelimiter.SelectedIndex)
                txtOutputFileFieldDelimiter.Text = xmlSettings.GetParam(OptionsSection, "OutputFileFieldDelimiter", txtOutputFileFieldDelimiter.Text)

                chkIncludePrefixAndSuffixResidues.Checked = xmlSettings.GetParam(OptionsSection, "IncludePrefixAndSuffixResidues", chkIncludePrefixAndSuffixResidues.Checked)
                chkEnableLogging.Checked = xmlSettings.GetParam(OptionsSection, "EnableLogging", chkEnableLogging.Checked)

                mCustomValidationRulesFilePath = xmlSettings.GetParam(OptionsSection, "CustomValidationRulesFilePath", String.Empty)

                Me.Width = xmlSettings.GetParam(OptionsSection, "WindowWidth", Me.Width)
                windowHeight = xmlSettings.GetParam(OptionsSection, "WindowHeight", Me.Height)
                If windowHeight > MAX_AUTO_WINDOW_HEIGHT Then
                    windowHeight = MAX_AUTO_WINDOW_HEIGHT
                End If
                Me.Height = windowHeight

                chkLookForAddnlRefInDescription.Checked = xmlSettings.GetParam(FASTAOptions, "LookForAddnlRefInDescription", chkLookForAddnlRefInDescription.Checked)
                txtAddnlRefSepChar.Text = xmlSettings.GetParam(FASTAOptions, "AddnlRefSepChar", txtAddnlRefSepChar.Text)
                txtAddnlRefAccessionSepChar.Text = xmlSettings.GetParam(FASTAOptions, "AddnlRefAccessionSepChar", txtAddnlRefAccessionSepChar.Text)

                chkExcludeProteinSequence.Checked = xmlSettings.GetParam(ProcessingOptions, "ExcludeProteinSequence", chkExcludeProteinSequence.Checked)
                chkComputeProteinMass.Checked = xmlSettings.GetParam(ProcessingOptions, "ComputeProteinMass", chkComputeProteinMass.Checked)
                cboElementMassMode.SelectedIndex = xmlSettings.GetParam(ProcessingOptions, "ElementMassMode", cboElementMassMode.SelectedIndex)

                ' In the GUI, chkComputepI controls computing pI, NET, and SCX
                ' Running from the command line, you can toggle those options separately using "ComputepI" and "ComputeSCX"
                chkComputepIandNET.Checked = xmlSettings.GetParam(ProcessingOptions, "ComputepI", chkComputepIandNET.Checked)
                chkIncludeXResidues.Checked = xmlSettings.GetParam(ProcessingOptions, "IncludeXResidues", chkIncludeXResidues.Checked)

                chkComputeSequenceHashValues.Checked = xmlSettings.GetParam(ProcessingOptions, "ComputeSequenceHashValues", chkComputeSequenceHashValues.Checked)
                chkComputeSequenceHashIgnoreILDiff.Checked = xmlSettings.GetParam(ProcessingOptions, "ComputeSequenceHashIgnoreILDiff", chkComputeSequenceHashIgnoreILDiff.Checked)
                chkTruncateProteinDescription.Checked = xmlSettings.GetParam(ProcessingOptions, "TruncateProteinDescription", chkTruncateProteinDescription.Checked)
                chkExcludeProteinDescription.Checked = xmlSettings.GetParam(ProcessingOptions, "ExcludeProteinDescription", chkExcludeProteinDescription.Checked)

                chkDigestProteins.Checked = xmlSettings.GetParam(ProcessingOptions, "DigestProteins", chkDigestProteins.Checked)
                cboProteinReversalOptions.SelectedIndex = xmlSettings.GetParam(ProcessingOptions, "ProteinReversalIndex", cboProteinReversalOptions.SelectedIndex)
                txtProteinScramblingLoopCount.Text = xmlSettings.GetParam(ProcessingOptions, "ProteinScramblingLoopCount", txtProteinScramblingLoopCount.Text)

                Try
                    cboHydrophobicityMode.SelectedIndex = xmlSettings.GetParam(ProcessingOptions, "HydrophobicityMode", cboHydrophobicityMode.SelectedIndex)
                Catch ex As Exception
                    ' Ignore errors setting the selected index
                End Try
                chkMaxpIModeEnabled.Checked = xmlSettings.GetParam(ProcessingOptions, "MaxpIModeEnabled", chkMaxpIModeEnabled.Checked)
                txtMaxpISequenceLength.Text = xmlSettings.GetParam(ProcessingOptions, "MaxpISequenceLength", LookupMaxpISequenceLength).ToString()

                Dim cleavageRuleName = xmlSettings.GetParam(DigestionOptions, "CleavageRuleName", String.Empty)

                If Not String.IsNullOrWhiteSpace(cleavageRuleName) Then
                    SetSelectedCleavageRule(cleavageRuleName)
                Else
                    Dim legacyCleavageRuleIndexSetting = xmlSettings.GetParam(DigestionOptions, "CleavageRuleTypeIndex", -1)
                    If legacyCleavageRuleIndexSetting >= 0 Then

                        Try
                            Dim cleavageRule = CType(legacyCleavageRuleIndexSetting, clsInSilicoDigest.CleavageRuleConstants)
                            SetSelectedCleavageRule(cleavageRule)
                        Catch ex As Exception
                            ' Ignore errors here
                        End Try
                    End If
                End If

                chkIncludeDuplicateSequences.Checked = xmlSettings.GetParam(DigestionOptions, "IncludeDuplicateSequences", chkIncludeDuplicateSequences.Checked)
                chkCysPeptidesOnly.Checked = xmlSettings.GetParam(DigestionOptions, "CysPeptidesOnly", chkCysPeptidesOnly.Checked)

                txtDigestProteinsMinimumMass.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMinimumMass", txtDigestProteinsMinimumMass.Text)
                txtDigestProteinsMaximumMass.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMaximumMass", txtDigestProteinsMaximumMass.Text)
                txtDigestProteinsMinimumResidueCount.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMinimumResidueCount", txtDigestProteinsMinimumResidueCount.Text)
                txtDigestProteinsMaximumMissedCleavages.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMaximumMissedCleavages", txtDigestProteinsMaximumMissedCleavages.Text)

                txtDigestProteinsMinimumpI.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMinimumpI", txtDigestProteinsMinimumpI.Text)
                txtDigestProteinsMaximumpI.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMaximumpI", txtDigestProteinsMaximumpI.Text)

                cboFragmentMassMode.SelectedIndex = xmlSettings.GetParam(DigestionOptions, "FragmentMassModeIndex", cboFragmentMassMode.SelectedIndex)
                cboCysTreatmentMode.SelectedIndex = xmlSettings.GetParam(DigestionOptions, "CysTreatmentModeIndex", cboCysTreatmentMode.SelectedIndex)

                ' Load Uniqueness Options
                chkAssumeInputFileIsDigested.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "AssumeInputFileIsDigested", chkAssumeInputFileIsDigested.Checked)

                txtUniquenessBinWidth.Text = xmlSettings.GetParam(UniquenessStatsOptions, "UniquenessBinWidth", txtUniquenessBinWidth.Text)
                chkAutoComputeRangeForBinning.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "AutoComputeRangeForBinning", chkAutoComputeRangeForBinning.Checked)
                txtUniquenessBinStartMass.Text = xmlSettings.GetParam(UniquenessStatsOptions, "UniquenessBinStartMass", txtUniquenessBinStartMass.Text)
                txtUniquenessBinEndMass.Text = xmlSettings.GetParam(UniquenessStatsOptions, "UniquenessBinEndMass", txtUniquenessBinEndMass.Text)

                txtMaxPeakMatchingResultsPerFeatureToSave.Text = xmlSettings.GetParam(UniquenessStatsOptions, "MaxPeakMatchingResultsPerFeatureToSave", txtMaxPeakMatchingResultsPerFeatureToSave.Text)
                chkUseSLiCScoreForUniqueness.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "UseSLiCScoreForUniqueness", chkUseSLiCScoreForUniqueness.Checked)
                txtMinimumSLiCScore.Text = xmlSettings.GetParam(UniquenessStatsOptions, "MinimumSLiCScore", txtMinimumSLiCScore.Text)
                radioButtonChecked = xmlSettings.GetParam(UniquenessStatsOptions, "UseEllipseSearchRegion", True)
                If radioButtonChecked Then
                    optUseEllipseSearchRegion.Checked = radioButtonChecked
                Else
                    optUseRectangleSearchRegion.Checked = radioButtonChecked
                End If

                ''chkAllowSqlServerCaching.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "AllowSqlServerCaching", chkAllowSqlServerCaching.Checked)
                ''chkUseSqlServerDBToCacheData.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "UseSqlServerDBToCacheData", chkUseSqlServerDBToCacheData.Checked)
                ''txtSqlServerName.Text = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerName", txtSqlServerName.Text)
                ''txtSqlServerDatabase.Text = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerDatabase", txtSqlServerDatabase.Text)
                ''chkSqlServerUseIntegratedSecurity.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerUseIntegratedSecurity", chkSqlServerUseIntegratedSecurity.Checked)

                ''chkSqlServerUseExistingData.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerUseExistingData", chkSqlServerUseExistingData.Checked)

                ''txtSqlServerUsername.Text = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerUsername", txtSqlServerUsername.Text)
                ''txtSqlServerPassword.Text = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerPassword", txtSqlServerPassword.Text)

                ' Load the peak matching thresholds
                cboMassTolType.SelectedIndex = xmlSettings.GetParam(PMOptions, "MassToleranceType", cboMassTolType.SelectedIndex)
                chkAutoDefineSLiCScoreTolerances.Checked = xmlSettings.GetParam(PMOptions, "AutoDefineSLiCScoreThresholds", chkAutoDefineSLiCScoreTolerances.Checked)
                autoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked

                ' See if any peak matching data is present
                ' If it is, clear the table and load it; if not, leave the table unchanged

                valueNotPresent = False
                thresholdData = xmlSettings.GetParam(PMOptions, "ThresholdData", String.Empty, valueNotPresent)

                If Not valueNotPresent AndAlso thresholdData IsNot Nothing AndAlso thresholdData.Length > 0 Then
                    thresholds = thresholdData.Split(";"c)

                    If thresholds.Length > 0 Then
                        ClearPMThresholdsList(False)

                        For index = 0 To thresholds.Length - 1
                            thresholdDetails = thresholds(index).Split(columnDelimiters)

                            If thresholdDetails.Length > 2 AndAlso Not autoDefineSLiCScoreThresholds Then
                                If IsNumeric(thresholdDetails(0)) And IsNumeric(thresholdDetails(1)) And
                                 IsNumeric(thresholdDetails(2)) And IsNumeric(thresholdDetails(3)) Then
                                    AddPMThresholdRow(CDbl(thresholdDetails(0)), CDbl(thresholdDetails(1)),
                                         CDbl(thresholdDetails(2)), CDbl(thresholdDetails(3)))
                                End If
                            ElseIf thresholdDetails.Length >= 2 Then
                                If IsNumeric(thresholdDetails(0)) And IsNumeric(thresholdDetails(1)) Then
                                    AddPMThresholdRow(CDbl(thresholdDetails(0)), CDbl(thresholdDetails(1)))
                                End If
                            End If
                        Next index
                    End If
                End If

            Catch ex As Exception
                ShowErrorMessage("Invalid parameter in settings file: " & Path.GetFileName(settingsFilePath), "Error")
            End Try

        Catch ex As Exception
            ShowErrorMessage("Error loading settings from file: " & settingsFilePath, "Error")
        End Try

    End Sub

    Private Sub IniFileSaveOptions(showFilePath As Boolean, Optional saveWindowDimensionsOnly As Boolean = False)

        Const OptionsSection As String = clsParseProteinFile.XML_SECTION_OPTIONS
        Const FASTAOptions As String = clsParseProteinFile.XML_SECTION_FASTA_OPTIONS
        Const ProcessingOptions As String = clsParseProteinFile.XML_SECTION_PROCESSING_OPTIONS
        Const DigestionOptions As String = clsParseProteinFile.XML_SECTION_DIGESTION_OPTIONS
        Const UniquenessStatsOptions As String = clsParseProteinFile.XML_SECTION_UNIQUENESS_STATS_OPTIONS
        Const PMOptions As String = clsProteinDigestionSimulator.XML_SECTION_PEAK_MATCHING_OPTIONS

        Dim xmlSettings As New XmlSettingsFileAccessor()
        Dim settingsFilePath = GetSettingsFilePath()

        Try
            Dim settingsFile = New FileInfo(settingsFilePath)
            If Not settingsFile.Exists Then
                saveWindowDimensionsOnly = False
            End If
        Catch
            'Ignore errors here
        End Try

        Try

            ' Pass True to .LoadSettings() to turn on case sensitive matching
            xmlSettings.LoadSettings(settingsFilePath, True)

            Try
                If Not saveWindowDimensionsOnly Then
                    xmlSettings.SetParam(OptionsSection, "InputFilePath", GetProteinInputFilePath())
                    xmlSettings.SetParam(OptionsSection, "InputFileFormatIndex", cboInputFileFormat.SelectedIndex)
                    xmlSettings.SetParam(OptionsSection, "InputFileColumnDelimiterIndex", cboInputFileColumnDelimiter.SelectedIndex)
                    xmlSettings.SetParam(OptionsSection, "InputFileColumnDelimiter", txtInputFileColumnDelimiter.Text)

                    xmlSettings.SetParam(OptionsSection, "InputFileColumnOrdering", cboInputFileColumnOrdering.SelectedIndex)


                    xmlSettings.SetParam(OptionsSection, "OutputFileFieldDelimiterIndex", cboOutputFileFieldDelimiter.SelectedIndex)
                    xmlSettings.SetParam(OptionsSection, "OutputFileFieldDelimiter", txtOutputFileFieldDelimiter.Text)

                    xmlSettings.SetParam(OptionsSection, "IncludePrefixAndSuffixResidues", chkIncludePrefixAndSuffixResidues.Checked)
                    xmlSettings.SetParam(OptionsSection, "EnableLogging", chkEnableLogging.Checked)

                    xmlSettings.SetParam(OptionsSection, "CustomValidationRulesFilePath", mCustomValidationRulesFilePath)
                End If

                xmlSettings.SetParam(OptionsSection, "WindowWidth", Me.Width)
                xmlSettings.SetParam(OptionsSection, "WindowHeight", Me.Height)

                If Not saveWindowDimensionsOnly Then
                    xmlSettings.SetParam(FASTAOptions, "LookForAddnlRefInDescription", chkLookForAddnlRefInDescription.Checked)
                    xmlSettings.SetParam(FASTAOptions, "AddnlRefSepChar", txtAddnlRefSepChar.Text)
                    xmlSettings.SetParam(FASTAOptions, "AddnlRefAccessionSepChar", txtAddnlRefAccessionSepChar.Text)

                    xmlSettings.SetParam(ProcessingOptions, "ExcludeProteinSequence", chkExcludeProteinSequence.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "ComputeProteinMass", chkComputeProteinMass.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "ComputepI", chkComputepIandNET.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "ComputeNET", chkComputepIandNET.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "ComputeSCX", chkComputepIandNET.Checked)

                    xmlSettings.SetParam(ProcessingOptions, "IncludeXResidues", chkIncludeXResidues.Checked)

                    xmlSettings.SetParam(ProcessingOptions, "ComputeSequenceHashValues", chkComputeSequenceHashValues.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "ComputeSequenceHashIgnoreILDiff", chkComputeSequenceHashIgnoreILDiff.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "TruncateProteinDescription", chkTruncateProteinDescription.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "ExcludeProteinDescription", chkExcludeProteinDescription.Checked)

                    xmlSettings.SetParam(ProcessingOptions, "DigestProteins", chkDigestProteins.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "ProteinReversalIndex", cboProteinReversalOptions.SelectedIndex)
                    xmlSettings.SetParam(ProcessingOptions, "ProteinScramblingLoopCount", txtProteinScramblingLoopCount.Text)
                    xmlSettings.SetParam(ProcessingOptions, "ElementMassMode", cboElementMassMode.SelectedIndex)

                    xmlSettings.SetParam(ProcessingOptions, "HydrophobicityMode", cboHydrophobicityMode.SelectedIndex)
                    xmlSettings.SetParam(ProcessingOptions, "MaxpIModeEnabled", chkMaxpIModeEnabled.Checked)
                    xmlSettings.SetParam(ProcessingOptions, "MaxpISequenceLength", LookupMaxpISequenceLength())

                    xmlSettings.SetParam(DigestionOptions, "CleavageRuleName", GetSelectedCleavageRule().ToString())

                    xmlSettings.SetParam(DigestionOptions, "IncludeDuplicateSequences", chkIncludeDuplicateSequences.Checked)
                    xmlSettings.SetParam(DigestionOptions, "CysPeptidesOnly", chkCysPeptidesOnly.Checked)

                    xmlSettings.SetParam(DigestionOptions, "DigestProteinsMinimumMass", txtDigestProteinsMinimumMass.Text)
                    xmlSettings.SetParam(DigestionOptions, "DigestProteinsMaximumMass", txtDigestProteinsMaximumMass.Text)
                    xmlSettings.SetParam(DigestionOptions, "DigestProteinsMinimumResidueCount", txtDigestProteinsMinimumResidueCount.Text)
                    xmlSettings.SetParam(DigestionOptions, "DigestProteinsMaximumMissedCleavages", txtDigestProteinsMaximumMissedCleavages.Text)

                    xmlSettings.SetParam(DigestionOptions, "DigestProteinsMinimumpI", txtDigestProteinsMinimumpI.Text)
                    xmlSettings.SetParam(DigestionOptions, "DigestProteinsMaximumpI", txtDigestProteinsMaximumpI.Text)

                    xmlSettings.SetParam(DigestionOptions, "FragmentMassModeIndex", cboFragmentMassMode.SelectedIndex)
                    xmlSettings.SetParam(DigestionOptions, "CysTreatmentModeIndex", cboCysTreatmentMode.SelectedIndex)

                    ' Load Uniqueness Options
                    xmlSettings.SetParam(UniquenessStatsOptions, "AssumeInputFileIsDigested", chkAssumeInputFileIsDigested.Checked)

                    xmlSettings.SetParam(UniquenessStatsOptions, "UniquenessBinWidth", txtUniquenessBinWidth.Text)
                    xmlSettings.SetParam(UniquenessStatsOptions, "AutoComputeRangeForBinning", chkAutoComputeRangeForBinning.Checked)
                    xmlSettings.SetParam(UniquenessStatsOptions, "UniquenessBinStartMass", txtUniquenessBinStartMass.Text)
                    xmlSettings.SetParam(UniquenessStatsOptions, "UniquenessBinEndMass", txtUniquenessBinEndMass.Text)

                    xmlSettings.SetParam(UniquenessStatsOptions, "MaxPeakMatchingResultsPerFeatureToSave", txtMaxPeakMatchingResultsPerFeatureToSave.Text)
                    xmlSettings.SetParam(UniquenessStatsOptions, "UseSLiCScoreForUniqueness", chkUseSLiCScoreForUniqueness.Checked)
                    xmlSettings.SetParam(UniquenessStatsOptions, "MinimumSLiCScore", txtMinimumSLiCScore.Text)
                    xmlSettings.SetParam(UniquenessStatsOptions, "UseEllipseSearchRegion", optUseEllipseSearchRegion.Checked)

                    ''xmlSettings.SetParam(UniquenessStatsOptions, "AllowSqlServerCaching", chkAllowSqlServerCaching.Checked)
                    ''xmlSettings.SetParam(UniquenessStatsOptions, "UseSqlServerDBToCacheData", chkUseSqlServerDBToCacheData.Checked)
                    ''xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerName", txtSqlServerName.Text)
                    ''xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerDatabase", txtSqlServerDatabase.Text)
                    ''xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerUseIntegratedSecurity", chkSqlServerUseIntegratedSecurity.Checked)
                    ''xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerUseExistingData", chkSqlServerUseExistingData.Checked)
                    ''xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerUsername", txtSqlServerUsername.Text)
                    ''xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerPassword", txtSqlServerPassword.Text)

                    ' Save the peak matching thresholds
                    xmlSettings.SetParam(PMOptions, "MassToleranceType", cboMassTolType.SelectedIndex.ToString())

                    Dim autoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked
                    xmlSettings.SetParam(PMOptions, "AutoDefineSLiCScoreThresholds", autoDefineSLiCScoreThresholds.ToString())

                    Dim thresholdData = String.Empty
                    For Each myDataRow As DataRow In mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATA_TABLE).Rows
                        If thresholdData.Length > 0 Then thresholdData &= "; "
                        If autoDefineSLiCScoreThresholds Then
                            thresholdData &= CStr(myDataRow.Item(0)) & "," & CStr(myDataRow.Item(1))
                        Else
                            thresholdData &= CStr(myDataRow.Item(0)) & "," & CStr(myDataRow.Item(1)) & "," & CStr(myDataRow.Item(2)) & "," & CStr(myDataRow.Item(3))
                        End If
                    Next myDataRow
                    xmlSettings.SetParam(PMOptions, "ThresholdData", thresholdData)
                End If
            Catch ex As Exception
                ShowErrorMessage("Error storing parameter in settings file: " & Path.GetFileName(settingsFilePath), "Error")
            End Try

            xmlSettings.SaveSettings()

            If showFilePath Then
                MessageBox.Show("Saved settings to file " & settingsFilePath, "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            ShowErrorMessage("Error saving settings to file: " & settingsFilePath, "Error")
        End Try

    End Sub

    Private Sub InitializeControls()
        DefineDefaultPMThresholds()

        Me.Text = "Protein Digestion Simulator"
        lblUniquenessCalculationsNote.Text = "The Protein Digestion Simulator uses an elution time prediction algorithm " &
                                             "developed by Lars Kangas and Kostas Petritis. See Help->About Elution Time Prediction for more info. " &
                                             "Note that you can provide custom time values for peptides by separately " &
                                             "generating a tab or comma delimited text file with information corresponding " &
                                             "to one of the options in the 'Column Order' list on the 'File Format' option tab, " &
                                             "then checking 'Assume Input file is Already Digested' on this tab."

        PopulateComboBoxes()
        InitializePeakMatchingDataGrid()

        IniFileLoadOptions()
        SetToolTips()

        ShowSplashScreen()

        EnableDisableControls()

        ResetProgress()
    End Sub

    Private Sub InitializePeakMatchingDataGrid()


        ' Make the Peak Matching Thresholds DATA_TABLE
        Dim pmThresholds = New DataTable(PM_THRESHOLDS_DATA_TABLE)

        ' Add the columns to the data table
        DBUtils.AppendColumnDoubleToTable(pmThresholds, COL_NAME_MASS_TOLERANCE)
        DBUtils.AppendColumnDoubleToTable(pmThresholds, COL_NAME_NET_TOLERANCE)
        DBUtils.AppendColumnDoubleToTable(pmThresholds, COL_NAME_SLIC_MASS_STDEV, DEFAULT_SLIC_MASS_STDEV)
        DBUtils.AppendColumnDoubleToTable(pmThresholds, COL_NAME_SLIC_NET_STDEV, DEFAULT_SLIC_NET_STDEV)
        DBUtils.AppendColumnIntegerToTable(pmThresholds, COL_NAME_PM_THRESHOLD_ROW_ID, 0, True, True)

        Dim PrimaryKeyColumn = New DataColumn() {pmThresholds.Columns(COL_NAME_PM_THRESHOLD_ROW_ID)}
        pmThresholds.PrimaryKey = PrimaryKeyColumn

        ' Instantiate the dataset
        mPeakMatchingThresholdsDataset = New DataSet(PM_THRESHOLDS_DATA_TABLE)

        ' Add the new System.Data.DataTable to the DataSet.
        mPeakMatchingThresholdsDataset.Tables.Add(pmThresholds)

        ' Bind the DataSet to the DataGrid
        dgPeakMatchingThresholds.DataSource = mPeakMatchingThresholdsDataset
        dgPeakMatchingThresholds.DataMember = PM_THRESHOLDS_DATA_TABLE

        ' Update the grid's table style
        UpdateDataGridTableStyle()

        ' Populate the table
        AutoPopulatePMThresholdsByID(PredefinedPMThresholdsConstants.OneMassOneNET, False)

    End Sub

    Private Sub UpdateDataGridTableStyle()
        Dim tsPMThresholdsTableStyle As DataGridTableStyle

        ' Define the PM Thresholds table style
        ' Setting the MappingName of the table style to PM_THRESHOLDS_DATA_TABLE will cause this style to be used with that table
        tsPMThresholdsTableStyle = New DataGridTableStyle With {
            .MappingName = PM_THRESHOLDS_DATA_TABLE,
            .AllowSorting = True,
            .ColumnHeadersVisible = True,
            .RowHeadersVisible = True,
            .ReadOnly = False
        }

        DataGridUtils.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_MASS_TOLERANCE, "Mass Tolerance", 90)
        DataGridUtils.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_NET_TOLERANCE, "NET Tolerance", 90)

        If chkAutoDefineSLiCScoreTolerances.Checked Then
            dgPeakMatchingThresholds.Width = 250
        Else
            dgPeakMatchingThresholds.Width = 425
            DataGridUtils.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_MASS_STDEV, "SLiC Mass StDev", 90)
            DataGridUtils.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_NET_STDEV, "SLiC NET StDev", 90)
        End If

        cmdPastePMThresholdsList.Left = dgPeakMatchingThresholds.Left + dgPeakMatchingThresholds.Width + 15
        cmdClearPMThresholdsList.Left = cmdPastePMThresholdsList.Left

        dgPeakMatchingThresholds.TableStyles.Clear()

        If Not dgPeakMatchingThresholds.TableStyles.Contains(tsPMThresholdsTableStyle) Then
            dgPeakMatchingThresholds.TableStyles.Add(tsPMThresholdsTableStyle)
        End If

        dgPeakMatchingThresholds.Refresh()

    End Sub

    Private Function InitializeProteinFileParserGeneralOptions(ByRef parseProteinFile As clsParseProteinFile) As Boolean
        ' Returns true if all values were valid

        Dim invalidValue As Boolean

        If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.FastaFile Then
            parseProteinFile.AssumeFastaFile = True
        ElseIf cboInputFileFormat.SelectedIndex = InputFileFormatConstants.DelimitedText Then
            parseProteinFile.AssumeDelimitedFile = True
        Else
            parseProteinFile.AssumeFastaFile = False
        End If

        If cboInputFileColumnOrdering.SelectedIndex >= 0 Then
            parseProteinFile.DelimitedFileFormatCode = CType(cboInputFileColumnOrdering.SelectedIndex, DelimitedProteinFileReader.ProteinFileFormatCode)
        End If

        parseProteinFile.InputFileDelimiter = LookupColumnDelimiter(cboInputFileColumnDelimiter, txtInputFileColumnDelimiter, ControlChars.Tab)
        parseProteinFile.OutputFileDelimiter = LookupColumnDelimiter(cboOutputFileFieldDelimiter, txtOutputFileFieldDelimiter, ControlChars.Tab)

        parseProteinFile.FastaFileOptions.LookForAddnlRefInDescription = chkLookForAddnlRefInDescription.Checked

        ValidateTextBox(txtAddnlRefSepChar, mDefaultFastaFileOptions.AddnlRefSepChar)
        ValidateTextBox(txtAddnlRefAccessionSepChar, mDefaultFastaFileOptions.AddnlRefAccessionSepChar)

        parseProteinFile.FastaFileOptions.AddnlRefSepChar = txtAddnlRefSepChar.Text.Chars(0)
        parseProteinFile.FastaFileOptions.AddnlRefAccessionSepChar = txtAddnlRefAccessionSepChar.Text.Chars(0)

        parseProteinFile.ExcludeProteinSequence = chkExcludeProteinSequence.Checked
        parseProteinFile.ComputeProteinMass = chkComputeProteinMass.Checked
        parseProteinFile.ComputepI = chkComputepIandNET.Checked
        parseProteinFile.ComputeNET = chkComputepIandNET.Checked
        parseProteinFile.ComputeSCXNET = chkComputepIandNET.Checked

        parseProteinFile.ComputeSequenceHashValues = chkComputeSequenceHashValues.Checked
        parseProteinFile.ComputeSequenceHashIgnoreILDiff = chkComputeSequenceHashIgnoreILDiff.Checked
        parseProteinFile.TruncateProteinDescription = chkTruncateProteinDescription.Checked
        parseProteinFile.ExcludeProteinDescription = chkExcludeProteinDescription.Checked

        If cboHydrophobicityMode.SelectedIndex >= 0 Then
            parseProteinFile.HydrophobicityType = CType(cboHydrophobicityMode.SelectedIndex, clspICalculation.eHydrophobicityTypeConstants)
        End If

        parseProteinFile.ReportMaximumpI = chkMaxpIModeEnabled.Checked
        parseProteinFile.SequenceWidthToExamineForMaximumpI = LookupMaxpISequenceLength()

        parseProteinFile.IncludeXResiduesInMass = chkIncludeXResidues.Checked

        parseProteinFile.GenerateUniqueIDValuesForPeptides = chkGenerateUniqueIDValues.Checked

        If cboCleavageRuleType.SelectedIndex >= 0 Then
            parseProteinFile.DigestionOptions.CleavageRuleID = GetSelectedCleavageRule()
        End If

        parseProteinFile.DigestionOptions.IncludePrefixAndSuffixResidues = chkIncludePrefixAndSuffixResidues.Checked

        parseProteinFile.DigestionOptions.MinFragmentMass = ParseTextBoxValueInt(txtDigestProteinsMinimumMass, lblDigestProteinsMinimumMass.Text & " must be an integer value", invalidValue)
        If invalidValue Then Return False

        parseProteinFile.DigestionOptions.MaxFragmentMass = ParseTextBoxValueInt(txtDigestProteinsMaximumMass, lblDigestProteinsMaximumMass.Text & " must be an integer value", invalidValue)
        If invalidValue Then Return False

        parseProteinFile.DigestionOptions.MaxMissedCleavages = ParseTextBoxValueInt(txtDigestProteinsMaximumMissedCleavages, lblDigestProteinsMaximumMissedCleavages.Text & " must be an integer value", invalidValue)
        If invalidValue Then Return False

        parseProteinFile.DigestionOptions.MinFragmentResidueCount = ParseTextBoxValueInt(txtDigestProteinsMinimumResidueCount, lblDigestProteinsMinimumResidueCount.Text & " must be an integer value", invalidValue)
        If invalidValue Then Return False

        parseProteinFile.DigestionOptions.MinIsoelectricPoint = ParseTextBoxValueSng(txtDigestProteinsMinimumpI, lblDigestProteinsMinimumpI.Text & " must be a decimal value", invalidValue)
        If invalidValue Then Return False

        parseProteinFile.DigestionOptions.MaxIsoelectricPoint = ParseTextBoxValueSng(txtDigestProteinsMaximumpI, lblDigestProteinsMaximumpI.Text & " must be a decimal value", invalidValue)
        If invalidValue Then Return False

        If cboCysTreatmentMode.SelectedIndex >= 0 Then
            parseProteinFile.DigestionOptions.CysTreatmentMode = CType(cboCysTreatmentMode.SelectedIndex, PeptideSequenceClass.CysTreatmentModeConstants)
        End If

        If cboFragmentMassMode.SelectedIndex >= 0 Then
            parseProteinFile.DigestionOptions.FragmentMassMode = CType(cboFragmentMassMode.SelectedIndex, clsInSilicoDigest.FragmentMassConstants)
        End If

        parseProteinFile.DigestionOptions.RemoveDuplicateSequences = Not chkIncludeDuplicateSequences.Checked
        If chkCysPeptidesOnly.Checked Then
            parseProteinFile.DigestionOptions.AminoAcidResidueFilterChars = New Char() {"C"c}
        Else
            parseProteinFile.DigestionOptions.AminoAcidResidueFilterChars = New Char() {}
        End If

        Return True

    End Function

    Private Function LookupColumnDelimiter(delimiterCombobox As ListControl, delimiterTextBox As Control, defaultDelimiter As Char) As Char
        Try
            Return clsParseProteinFile.LookupColumnDelimiterChar(delimiterCombobox.SelectedIndex, delimiterTextBox.Text, defaultDelimiter)
        Catch ex As Exception
            Return ControlChars.Tab
        End Try
    End Function

    Private Function LookupMaxpISequenceLength() As Integer
        Dim invalidValue As Boolean
        Dim length As Integer

        Try
            length = TextBoxUtils.ParseTextBoxValueInt(txtMaxpISequenceLength, String.Empty, invalidValue, 10)
            If invalidValue Then
                txtMaxpISequenceLength.Text = length.ToString()
            End If
        Catch ex As Exception
            length = 10
        End Try

        If length < 1 Then length = 1
        Return length
    End Function

    Private Sub SetToolTips()
        Dim toolTipControl As New ToolTip()

        toolTipControl.SetToolTip(cmdParseInputFile, "Parse proteins in input file to create output file(s).")
        toolTipControl.SetToolTip(cboInputFileColumnDelimiter, "Character separating columns in a delimited text input file.")
        toolTipControl.SetToolTip(txtInputFileColumnDelimiter, "Custom character separating columns in a delimited text input file.")
        toolTipControl.SetToolTip(txtOutputFileFieldDelimiter, "Character separating the fields in the output file.")

        toolTipControl.SetToolTip(txtAddnlRefSepChar, "Character separating additional protein accession entries in a protein's description in a Fasta file.")
        toolTipControl.SetToolTip(txtAddnlRefAccessionSepChar, "Character separating source name and accession number for additional protein accession entries in a Fasta file.")

        toolTipControl.SetToolTip(chkGenerateUniqueIDValues, "Set this to false to use less memory when digesting huge protein input files.")
        toolTipControl.SetToolTip(txtProteinReversalSamplingPercentage, "Set this to a value less than 100 to only include a portion of the residues from the input file in the output file.")
        toolTipControl.SetToolTip(txtProteinScramblingLoopCount, "Set this to a value greater than 1 to create multiple scrambled versions of the input file.")

        toolTipControl.SetToolTip(optUseEllipseSearchRegion, "This setting only takes effect if 'Use SLiC Score when gauging uniqueness' is false.")
        toolTipControl.SetToolTip(optUseRectangleSearchRegion, "This setting only takes effect if 'Use SLiC Score when gauging uniqueness' is false.")

        toolTipControl.SetToolTip(lblPeptideUniquenessMassMode, "Current mass mode; to change go to the 'Parse and Digest File Options' tab")

        toolTipControl.SetToolTip(chkExcludeProteinSequence, "Enabling this setting will prevent protein sequences from being written to the output file; useful when processing extremely large files.")
        toolTipControl.SetToolTip(chkTruncateProteinDescription, "Truncate description (if over 7995 chars)")

        toolTipControl.SetToolTip(chkEnableLogging, "Logs status and error messages to file ProteinDigestionSimulatorLog*.txt in the program directory.")

    End Sub

    Private Sub ParseProteinInputFile()
        Dim success As Boolean

        If Not mWorking AndAlso ConfirmFilePaths() Then
            Try
                If mParseProteinFile Is Nothing Then
                    mParseProteinFile = New clsParseProteinFile
                End If

                success = InitializeProteinFileParserGeneralOptions(mParseProteinFile)
                If Not success Then Exit Try

                mParseProteinFile.CreateProteinOutputFile = True

                If cboProteinReversalOptions.SelectedIndex >= 0 Then
                    mParseProteinFile.ProteinScramblingMode = CType(cboProteinReversalOptions.SelectedIndex, clsParseProteinFile.ProteinScramblingModeConstants)
                End If

                mParseProteinFile.ProteinScramblingSamplingPercentage = TextBoxUtils.ParseTextBoxValueInt(txtProteinReversalSamplingPercentage, "", False, 100, False)
                mParseProteinFile.ProteinScramblingLoopCount = TextBoxUtils.ParseTextBoxValueInt(txtProteinScramblingLoopCount, "", False, 1, False)
                mParseProteinFile.CreateDigestedProteinOutputFile = chkDigestProteins.Checked
                mParseProteinFile.CreateFastaOutputFile = chkCreateFastaOutputFile.Checked

                If cboElementMassMode.SelectedIndex >= 0 Then
                    mParseProteinFile.ElementMassMode = CType(cboElementMassMode.SelectedIndex, PeptideSequenceClass.ElementModeConstants)
                End If

                Cursor.Current = Cursors.WaitCursor
                mWorking = True
                cmdParseInputFile.Enabled = False

                ResetProgress()
                SwitchToProgressTab()

                Dim outputFolderPath As String = String.Empty
                Dim outputFileNameBaseOverride As String = String.Empty

                If txtProteinOutputFilePath.TextLength > 0 Then
                    outputFolderPath = Path.GetDirectoryName(txtProteinOutputFilePath.Text)
                    outputFileNameBaseOverride = Path.GetFileNameWithoutExtension(txtProteinOutputFilePath.Text)
                End If

                success = mParseProteinFile.ParseProteinFile(GetProteinInputFilePath(), outputFolderPath, outputFileNameBaseOverride)

                Cursor.Current = Cursors.Default

                If success Then
                    MessageBox.Show(mParseProteinFile.ProcessingSummary, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    SwitchFromProgressTab()
                Else
                    ShowErrorMessage("Error parsing protein file: " & mParseProteinFile.GetErrorMessage(), "Error")
                End If

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

    Private Function ParseTextBoxValueInt(thisTextBox As Control, messageIfError As String, ByRef invalidValue As Boolean, Optional valueIfError As Integer = 0) As Integer

        invalidValue = False

        Try
            Return Integer.Parse(thisTextBox.Text)
        Catch ex As Exception
            ShowErrorMessage(messageIfError, "Error")
            invalidValue = True
            Return valueIfError
        End Try

    End Function

    Private Function ParseTextBoxValueSng(thisTextBox As Control, messageIfError As String, ByRef invalidValue As Boolean, Optional valueIfError As Single = 0) As Single

        invalidValue = False

        Try
            Return Single.Parse(thisTextBox.Text)
        Catch ex As Exception
            ShowErrorMessage(messageIfError, "Error")
            invalidValue = True
            Return valueIfError
        End Try

    End Function

    Private Sub PastePMThresholdsValues(clearList As Boolean)

        Dim lineDelimiters = New Char() {ControlChars.Cr, ControlChars.Lf}
        Dim columnDelimiters = New Char() {ControlChars.Tab, ","c}

        ' Examine the clipboard contents
        Dim clipboardObject = Clipboard.GetDataObject()

        If clipboardObject IsNot Nothing Then
            If clipboardObject.GetDataPresent(DataFormats.StringFormat, True) Then
                Dim clipboardData = CType(clipboardObject.GetData(DataFormats.StringFormat, True), String)

                ' Split clipboardData on carriage return or line feed characters
                ' Lines that end in CrLf will give two separate lines; one with the text, and one blank; that's OK
                Dim dataLines = clipboardData.Split(lineDelimiters, 1000)

                If dataLines.Length > 0 Then
                    If clearList Then
                        If Not ClearPMThresholdsList(True) Then Return
                    End If

                    Dim rowsAlreadyPresent = 0
                    Dim rowsSkipped = 0

                    For lineIndex = 0 To dataLines.Length - 1
                        If dataLines(lineIndex) IsNot Nothing AndAlso dataLines(lineIndex).Length > 0 Then
                            Dim dataColumns = dataLines(lineIndex).Split(columnDelimiters, 5)
                            If dataColumns.Length >= 2 Then
                                Try
                                    Dim massThreshold = Double.Parse(dataColumns(0))
                                    Dim netThreshold = Double.Parse(dataColumns(1))

                                    If massThreshold >= 0 And netThreshold >= 0 Then
                                        Dim useSLiC As Boolean
                                        If Not chkAutoDefineSLiCScoreTolerances.Checked AndAlso dataColumns.Length >= 4 Then
                                            useSLiC = True
                                        Else
                                            useSLiC = False
                                        End If

                                        Dim slicMassStDev As Double
                                        Dim slicNETStDev As Double

                                        If useSLiC Then
                                            Try
                                                slicMassStDev = Double.Parse(dataColumns(2))
                                                slicNETStDev = Double.Parse(dataColumns(3))
                                            Catch ex As Exception
                                                useSLiC = False
                                            End Try
                                        End If

                                        Dim existingRowFound = False
                                        If useSLiC Then
                                            AddPMThresholdRow(massThreshold, netThreshold, slicMassStDev, slicNETStDev, existingRowFound)
                                        Else
                                            AddPMThresholdRow(massThreshold, netThreshold, existingRowFound)
                                        End If

                                        If existingRowFound Then
                                            rowsAlreadyPresent += 1
                                        End If
                                    End If

                                Catch ex As Exception
                                    ' Skip this row
                                    rowsSkipped += 1
                                End Try
                            Else
                                rowsSkipped += 1
                            End If
                        End If
                    Next lineIndex

                    If rowsAlreadyPresent > 0 Then
                        Dim errorMessage As String
                        If rowsAlreadyPresent = 1 Then
                            errorMessage = "1 row of thresholds was"
                        Else
                            errorMessage = rowsAlreadyPresent.ToString() & " rows of thresholds were"
                        End If

                        ShowErrorMessage(errorMessage & " already present in the table; duplicate rows are not allowed.", "Warning")
                    End If

                    If rowsSkipped > 0 Then
                        Dim errorMessage As String
                        If rowsSkipped = 1 Then
                            errorMessage = "1 row was skipped because it"
                        Else
                            errorMessage = rowsSkipped.ToString() & " rows were skipped because they"
                        End If

                        ShowErrorMessage(errorMessage & " didn't contain two columns of numeric data.", "Warning")
                    End If

                End If
            End If

        End If
    End Sub

    Private Sub PopulateComboBoxes()

        Const NET_UNITS = "NET"

        Try
            cboInputFileFormat.Items.Clear()
            cboInputFileFormat.Items.Insert(InputFileFormatConstants.AutoDetermine, "Auto-determine")
            cboInputFileFormat.Items.Insert(InputFileFormatConstants.FastaFile, "Fasta file")
            cboInputFileFormat.Items.Insert(InputFileFormatConstants.DelimitedText, "Delimited text")
            cboInputFileFormat.SelectedIndex = InputFileFormatConstants.AutoDetermine

            cboInputFileColumnDelimiter.Items.Clear()
            cboInputFileColumnDelimiter.Items.Insert(clsParseProteinFile.DelimiterCharConstants.Space, "Space")
            cboInputFileColumnDelimiter.Items.Insert(clsParseProteinFile.DelimiterCharConstants.Tab, "Tab")
            cboInputFileColumnDelimiter.Items.Insert(clsParseProteinFile.DelimiterCharConstants.Comma, "Comma")
            cboInputFileColumnDelimiter.Items.Insert(clsParseProteinFile.DelimiterCharConstants.Other, "Other")
            cboInputFileColumnDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Tab

            cboOutputFileFieldDelimiter.Items.Clear()
            For index = 0 To cboInputFileColumnDelimiter.Items.Count - 1
                cboOutputFileFieldDelimiter.Items.Insert(index, cboInputFileColumnDelimiter.Items(index))
            Next
            cboOutputFileFieldDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Space

            cboInputFileColumnOrdering.Items.Clear()
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.SequenceOnly, "Sequence Only")
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Sequence, "ProteinName and Sequence")
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence, "ProteinName, Description, Sequence")
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence, "UniqueID and Seq")
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID, "ProteinName, Seq, UniqueID")
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, "ProteinName, Seq, UniqueID, Mass, NET")
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore, "ProteinName, Seq, UniqueID, Mass, NET, NETStDev, DiscriminantScore")
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET, "UniqueID, Seq, Mass, NET")
            cboInputFileColumnOrdering.Items.Insert(DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Hash_Sequence, "ProteinName, Description, Hash, Sequence")
            cboInputFileColumnOrdering.SelectedIndex = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence

            cboElementMassMode.Items.Clear()
            cboElementMassMode.Items.Insert(PeptideSequenceClass.ElementModeConstants.AverageMass, "Average")
            cboElementMassMode.Items.Insert(PeptideSequenceClass.ElementModeConstants.IsotopicMass, "Monoisotopic")
            cboElementMassMode.SelectedIndex = PeptideSequenceClass.ElementModeConstants.IsotopicMass

            cboProteinReversalOptions.Items.Clear()
            cboProteinReversalOptions.Items.Insert(clsParseProteinFile.ProteinScramblingModeConstants.None, "Normal output")
            cboProteinReversalOptions.Items.Insert(clsParseProteinFile.ProteinScramblingModeConstants.Reversed, "Reverse ORF sequences")
            cboProteinReversalOptions.Items.Insert(clsParseProteinFile.ProteinScramblingModeConstants.Randomized, "Randomized ORF sequences")
            cboProteinReversalOptions.SelectedIndex = clsParseProteinFile.ProteinScramblingModeConstants.None

            Dim inSilicoDigest = New clsInSilicoDigest()
            cboCleavageRuleType.Items.Clear()
            mCleavageRuleComboboxIndexToType.Clear()

            ' Add Trypsin rules first
            AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, clsInSilicoDigest.CleavageRuleConstants.NoRule)
            AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin)
            AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, clsInSilicoDigest.CleavageRuleConstants.TrypsinWithoutProlineException)
            AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, clsInSilicoDigest.CleavageRuleConstants.KROneEnd)
            AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, clsInSilicoDigest.CleavageRuleConstants.TrypsinPlusFVLEY)
            AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, clsInSilicoDigest.CleavageRuleConstants.TrypsinPlusLysC)
            AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, clsInSilicoDigest.CleavageRuleConstants.TrypsinPlusThermolysin)

            ' Add the remaining enzymes based on the description, but skip CleavageRuleConstants.EricPartialTrypsin

            ' Keys in this dictionary are cleavage rule enums, values are the rule description
            Dim additionalRulesToAppend = New Dictionary(Of clsInSilicoDigest.CleavageRuleConstants, String)

            For Each cleavageRule In inSilicoDigest.CleavageRules
                Dim cleavageRuleId = cleavageRule.Key
                If mCleavageRuleComboboxIndexToType.ContainsValue(cleavageRuleId) Then Continue For

                additionalRulesToAppend.Add(cleavageRuleId, cleavageRule.Value.Description)
            Next

            For Each ruleToAdd In (From item In additionalRulesToAppend Order By item.Value Select item.Key)
                If ruleToAdd = clsInSilicoDigest.CleavageRuleConstants.EricPartialTrypsin Then Continue For

                AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, ruleToAdd)
            Next

            ' Select the fully tryptic enzyme rule
            SetSelectedCleavageRule(clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin)

            cboMassTolType.Items.Clear()
            cboMassTolType.Items.Insert(clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.PPM, "PPM")
            cboMassTolType.Items.Insert(clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.Absolute, "Absolute (Da)")
            cboMassTolType.SelectedIndex = clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants.PPM

            cboPMPredefinedThresholds.Items.Clear()
            cboPMPredefinedThresholds.Items.Insert(PredefinedPMThresholdsConstants.OneMassOneNET, "5 ppm; 0.05 " & NET_UNITS)
            cboPMPredefinedThresholds.Items.Insert(PredefinedPMThresholdsConstants.OneMassThreeNET, "5 ppm; 0.01, 0.05, 100 " & NET_UNITS)
            cboPMPredefinedThresholds.Items.Insert(PredefinedPMThresholdsConstants.ThreeMassOneNET, "0.5, 1, & 5 ppm; 0.05 " & NET_UNITS)
            cboPMPredefinedThresholds.Items.Insert(PredefinedPMThresholdsConstants.ThreeMassThreeNET, "0.5, 1, 5 ppm; 0.01, 0.05, & 100 " & NET_UNITS)
            cboPMPredefinedThresholds.Items.Insert(PredefinedPMThresholdsConstants.FiveMassThreeNET, "0.5, 1, 5, 10, & 50 ppm; 0.01, 0.05, & 100 " & NET_UNITS)
            cboPMPredefinedThresholds.SelectedIndex = PredefinedPMThresholdsConstants.OneMassOneNET

            cboHydrophobicityMode.Items.Clear()
            cboHydrophobicityMode.Items.Insert(clspICalculation.eHydrophobicityTypeConstants.HW, "Hopp and Woods")
            cboHydrophobicityMode.Items.Insert(clspICalculation.eHydrophobicityTypeConstants.KD, "Kyte and Doolittle")
            cboHydrophobicityMode.Items.Insert(clspICalculation.eHydrophobicityTypeConstants.Eisenberg, "Eisenberg")
            cboHydrophobicityMode.Items.Insert(clspICalculation.eHydrophobicityTypeConstants.GES, "Engleman et. al.")
            cboHydrophobicityMode.Items.Insert(clspICalculation.eHydrophobicityTypeConstants.MeekPH7p4, "Meek pH 7.4")
            cboHydrophobicityMode.Items.Insert(clspICalculation.eHydrophobicityTypeConstants.MeekPH2p1, "Meek pH 2.1")
            cboHydrophobicityMode.SelectedIndex = clspICalculation.eHydrophobicityTypeConstants.HW

            cboCysTreatmentMode.Items.Clear()
            cboCysTreatmentMode.Items.Insert(PeptideSequenceClass.CysTreatmentModeConstants.Untreated, "Untreated")
            cboCysTreatmentMode.Items.Insert(PeptideSequenceClass.CysTreatmentModeConstants.Iodoacetamide, "Iodoacetamide (+57.02)")
            cboCysTreatmentMode.Items.Insert(PeptideSequenceClass.CysTreatmentModeConstants.IodoaceticAcid, "Iodoacetic Acid (+58.01)")
            cboCysTreatmentMode.SelectedIndex = PeptideSequenceClass.CysTreatmentModeConstants.Untreated

            cboFragmentMassMode.Items.Clear()
            cboFragmentMassMode.Items.Insert(clsInSilicoDigest.FragmentMassConstants.Monoisotopic, "Monoisotopic")
            cboFragmentMassMode.Items.Insert(clsInSilicoDigest.FragmentMassConstants.MH, "M+H")
            cboFragmentMassMode.SelectedIndex = clsInSilicoDigest.FragmentMassConstants.Monoisotopic

        Catch ex As Exception
            ShowErrorMessage("Error initializing the combo boxes: " & ex.Message)
        End Try

    End Sub

    Private Sub ResetProgress()
        lblProgressDescription.Text = String.Empty
        lblProgress.Text = FormatPercentComplete(0)
        pbarProgress.Value = 0
        pbarProgress.Visible = True

        lblSubtaskProgressDescription.Text = String.Empty
        lblSubtaskProgress.Text = FormatPercentComplete(0)

        lblErrorMessage.Text = String.Empty

        Application.DoEvents()
    End Sub

    Private Sub ResetToDefaults(confirm As Boolean)
        Dim eResponse As DialogResult

        If confirm Then
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

        SetSelectedCleavageRule(clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin)

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

        Dim settingsFilePath = GetSettingsFilePath()
        ProcessFilesOrDirectoriesBase.CreateSettingsFileIfMissing(settingsFilePath)

    End Sub

    Private Sub SelectInputFile()

        Dim currentExtension = String.Empty
        If txtProteinInputFilePath.TextLength > 0 Then
            Try
                currentExtension = Path.GetExtension(GetProteinInputFilePath())
            Catch ex As Exception
                ' Ignore errors here
            End Try
        End If

        Dim openFile As New OpenFileDialog() With {
            .AddExtension = True,
            .CheckFileExists = False,
            .CheckPathExists = True,
            .DefaultExt = ".txt",
            .DereferenceLinks = True,
            .Multiselect = False,
            .ValidateNames = True,
            .Filter = "Fasta files (*.fasta)|*.fasta|Fasta files (*.fasta.gz)|*.fasta.gz|Text files (*.txt)|*.txt|All files (*.*)|*.*"
        }

        If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.DelimitedText Then
            openFile.FilterIndex = 3
        ElseIf currentExtension.ToLower() = ".txt" Then
            openFile.FilterIndex = 3
        ElseIf currentExtension.ToLower() = ".gz" Then
            openFile.FilterIndex = 2
        Else
            openFile.FilterIndex = 1
        End If

        If Len(GetProteinInputFilePath().Length) > 0 Then
            Try
                openFile.InitialDirectory = Directory.GetParent(GetProteinInputFilePath()).FullName
            Catch
                openFile.InitialDirectory = GetMyDocsFolderPath()
            End Try
        Else
            openFile.InitialDirectory = GetMyDocsFolderPath()
        End If

        openFile.Title = "Select input file"

        openFile.ShowDialog()
        If openFile.FileName.Length > 0 Then
            txtProteinInputFilePath.Text = openFile.FileName
        End If

    End Sub

    Private Sub SelectOutputFile()

        Dim saveFile As New SaveFileDialog() With {
            .AddExtension = True,
            .CheckFileExists = False,
            .CheckPathExists = True,
            .CreatePrompt = False,
            .DefaultExt = ".txt",
            .DereferenceLinks = True,
            .OverwritePrompt = True,
            .ValidateNames = True,
            .Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            .FilterIndex = 1
        }

        If Len(txtProteinOutputFilePath.Text.Length) > 0 Then
            Try
                saveFile.InitialDirectory = Directory.GetParent(txtProteinOutputFilePath.Text).ToString()
            Catch
                saveFile.InitialDirectory = GetMyDocsFolderPath()
            End Try
        Else
            saveFile.InitialDirectory = GetMyDocsFolderPath()
        End If

        saveFile.Title = "Select/Create output file"

        saveFile.ShowDialog()
        If saveFile.FileName.Length > 0 Then
            txtProteinOutputFilePath.Text = saveFile.FileName
        End If

    End Sub

    Private Sub SetSelectedCleavageRule(cleavageRuleName As String)
        Dim cleavageRule As clsInSilicoDigest.CleavageRuleConstants

        If [Enum].TryParse(cleavageRuleName, True, cleavageRule) Then
            SetSelectedCleavageRule(cleavageRule)
        End If

    End Sub

    Private Sub SetSelectedCleavageRule(cleavageRule As clsInSilicoDigest.CleavageRuleConstants)

        Dim query = From item In mCleavageRuleComboboxIndexToType
                    Where item.Value = cleavageRule
                    Select item.Key

        For Each item In query.Take(1)
            cboCleavageRuleType.SelectedIndex = item
        Next

    End Sub

    Private Sub ShowAboutBox()
        Dim message = New StringBuilder()

        message.AppendLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004")
        message.AppendLine("Copyright 2018 Battelle Memorial Institute")
        message.AppendLine()
        message.AppendLine("This is version " & Application.ProductVersion & " (" & PROGRAM_DATE & ")")
        message.AppendLine()
        message.AppendLine("E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov")
        message.AppendLine("Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/")
        message.AppendLine()
        message.AppendLine(frmDisclaimer.GetKangasPetritisDisclaimerText())
        message.AppendLine()
        message.AppendLine("Licensed under the 2-Clause BSD License; https://opensource.org/licenses/BSD-2-Clause")
        message.AppendLine()
        message.Append("This software is provided by the copyright holders and contributors ""as is"" and ")
        message.Append("any express or implied warranties, including, but not limited to, the implied ")
        message.Append("warranties of merchantability and fitness for a particular purpose are ")
        message.Append("disclaimed. In no event shall the copyright holder or contributors be liable ")
        message.Append("for any direct, indirect, incidental, special, exemplary, or consequential ")
        message.Append("damages (including, but not limited to, procurement of substitute goods or ")
        message.Append("services; loss of use, data, or profits; or business interruption) however ")
        message.Append("caused and on any theory of liability, whether in contract, strict liability, ")
        message.Append("or tort (including negligence or otherwise) arising in any way out of the use ")
        message.Append("of this software, even if advised of the possibility of such damage.")

        MessageBox.Show(message.ToString(), "About", MessageBoxButtons.OK, MessageBoxIcon.Information)

    End Sub

    Private Sub ShowElutionTimeInfo()
        MessageBox.Show(NETCalculator.ProgramDescription, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub ShowErrorMessage(message As String)
        ShowErrorMessage(message, "Error")
    End Sub

    Private Sub ShowErrorMessage(message As String, caption As String)
        Dim messageIcon As MessageBoxIcon

        If caption.ToLower().Contains("error") Then
            messageIcon = MessageBoxIcon.Exclamation
        Else
            messageIcon = MessageBoxIcon.Information
        End If

        MessageBox.Show(message, caption, MessageBoxButtons.OK, messageIcon)
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
        Const REG_KEY_SPLASH_DATE = "SplashDate"
        Const DEFAULT_DATE = #1/1/2000#

        Const SPLASH_INTERVAL_DAYS = 182

        Dim lastSplashDateText As String
        Dim lastSplashDate = DEFAULT_DATE
        Dim currentDateTime = DateTime.Now()

        Try
            lastSplashDateText = GetSetting(APP_NAME_IN_REGISTRY, REG_SECTION_OPTIONS, REG_KEY_SPLASH_DATE, "")
        Catch ex As Exception
            ' Error looking up the last splash date; don't continue
            Exit Sub
        End Try

        If Not String.IsNullOrWhiteSpace(lastSplashDateText) Then
            Try
                ' Convert the text to a date
                lastSplashDate = DateTime.Parse(lastSplashDateText)
            Catch ex As Exception
                ' Conversion failed
                lastSplashDateText = String.Empty
                lastSplashDate = DEFAULT_DATE
            End Try
        End If

        If String.IsNullOrWhiteSpace(lastSplashDateText) Then
            ' Entry isn't present (or it is present, but isn't the correct format)
            ' Try to add it
            Try
                SaveSetting(APP_NAME_IN_REGISTRY, REG_SECTION_OPTIONS, REG_KEY_SPLASH_DATE, lastSplashDate.ToShortDateString())
            Catch ex As Exception
                ' Error adding the splash date; don't continue
                Exit Sub
            End Try
        End If

        If currentDateTime.Subtract(lastSplashDate).TotalDays >= SPLASH_INTERVAL_DAYS Then
            Try
                lastSplashDate = currentDateTime
                SaveSetting(APP_NAME_IN_REGISTRY, REG_SECTION_OPTIONS, REG_KEY_SPLASH_DATE, lastSplashDate.ToShortDateString())

                ' Now make sure the setting actually saved
                lastSplashDateText = GetSetting(APP_NAME_IN_REGISTRY, REG_SECTION_OPTIONS, REG_KEY_SPLASH_DATE, "")
                lastSplashDate = DateTime.Parse(lastSplashDateText)

                If lastSplashDate.ToShortDateString() <> currentDateTime.ToShortDateString() Then
                    ' Error saving/retrieving date; don't continue
                    Exit Sub
                End If

            Catch ex As Exception
                ' Error saving the new splash date; don't continue
                Exit Sub
            End Try

            Dim splashForm As New frmDisclaimer
            splashForm.ShowDialog()

        End If

    End Sub

    Private Sub SwitchToProgressTab()

        mTabPageIndexSaved = tbsOptions.SelectedIndex

        tbsOptions.SelectedIndex = PROGRESS_TAB_INDEX
        Application.DoEvents()

    End Sub

    Private Sub SwitchFromProgressTab()
        ' Wait 500 msec, then switch from the progress tab back to the tab that was visible before we started, but only if the current tab is the progress tab

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

    Private Sub ValidateFastaFile(fastaFilePath As String)

        Try
            ' Make sure an existing file has been chosen
            If fastaFilePath Is Nothing OrElse fastaFilePath.Length = 0 Then Exit Try

            If Not File.Exists(fastaFilePath) Then
                ShowErrorMessage("File not found: " & fastaFilePath, "Error")
            Else
                If mFastaValidation Is Nothing Then
                    mFastaValidation = New frmFastaValidation(fastaFilePath)
                Else
                    mFastaValidation.SetNewFastaFile(fastaFilePath)
                End If

                Try
                    If mCustomValidationRulesFilePath IsNot Nothing AndAlso mCustomValidationRulesFilePath.Length > 0 Then
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
            If mFastaValidation IsNot Nothing Then
                mCustomValidationRulesFilePath = mFastaValidation.CustomRulesFilePath
            End If
        End Try

    End Sub

    Private Function ValidateSqlServerCachingOptionsForInputFile(inputFilePath As String, assumeDigested As Boolean, ByRef proteinFileParser As clsParseProteinFile) As Boolean
        ' Returns True if the user OK's or updates the current Sql Server caching options
        ' Returns False if the user cancels processing
        ' Assumes that inputFilePath exists, and thus does not have a Try-Catch block

        Const SAMPLING_LINE_COUNT = 10000

        Dim totalLineCount As Integer

        Dim suggestEnableSqlServer = False
        Dim suggestDisableSqlServer = False

        Dim isFastaFile = clsParseProteinFile.IsFastaFile(inputFilePath) Or proteinFileParser.AssumeFastaFile

        ' Lookup the file size
        Dim inputFile = New FileInfo(inputFilePath)
        Dim fileSizeKB = CType(inputFile.Length / 1024.0, Integer)

        If isFastaFile Then
            If proteinFileParser.DigestionOptions.CleavageRuleID = clsInSilicoDigest.CleavageRuleConstants.KROneEnd Or
               proteinFileParser.DigestionOptions.CleavageRuleID = clsInSilicoDigest.CleavageRuleConstants.NoRule Then
                suggestEnableSqlServer = True
            ElseIf fileSizeKB > 500 Then
                suggestEnableSqlServer = True
            ElseIf fileSizeKB <= 500 Then
                suggestDisableSqlServer = True
            End If
        Else
            ' Assume a delimited text file
            ' Estimate the total line count by reading the first SAMPLING_LINE_COUNT lines
            Try
                Using reader = New StreamReader(inputFilePath)

                    Dim bytesRead = 0
                    Dim lineCount = 0
                    Do While Not reader.EndOfStream AndAlso lineCount < SAMPLING_LINE_COUNT
                        Dim dataLine = reader.ReadLine()
                        lineCount += 1
                        bytesRead += dataLine.Length + 2
                    Loop

                    If lineCount < SAMPLING_LINE_COUNT OrElse bytesRead = 0 Then
                        totalLineCount = lineCount
                    Else
                        totalLineCount = CInt(lineCount * fileSizeKB / (bytesRead / 1024))
                    End If
                End Using

            Catch ex As Exception
                ' Error reading input file
                suggestEnableSqlServer = False
                suggestDisableSqlServer = False
            End Try

            If assumeDigested Then
                If totalLineCount > 50000 Then
                    suggestEnableSqlServer = True
                ElseIf totalLineCount <= 50000 Then
                    suggestDisableSqlServer = True
                End If
            Else
                If totalLineCount > 1000 Then
                    suggestEnableSqlServer = True
                ElseIf totalLineCount <= 1000 Then
                    suggestDisableSqlServer = True
                End If

            End If
        End If

        Dim proceed As Boolean
        If suggestEnableSqlServer And Not chkUseSqlServerDBToCacheData.Checked Then
            Dim eResponse = MessageBox.Show("Warning, memory usage could be quite large.  Enable Sql Server caching using Server " & txtSqlServerName.Text & "?  If no, then will continue using memory caching.", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
            If eResponse = DialogResult.Yes Then chkUseSqlServerDBToCacheData.Checked = True
            If eResponse = DialogResult.Cancel Then
                proceed = False
            Else
                proceed = True
            End If
        ElseIf suggestDisableSqlServer And chkUseSqlServerDBToCacheData.Checked Then
            Dim eResponse = MessageBox.Show("Memory usage is expected to be minimal.  Continue caching data using Server " & txtSqlServerName.Text & "?  If no, then will switch to using memory caching.", "Note", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
            If eResponse = DialogResult.No Then chkUseSqlServerDBToCacheData.Checked = False
            If eResponse = DialogResult.Cancel Then
                proceed = False
            Else
                proceed = True
            End If
        Else
            proceed = True
        End If

        Return proceed

    End Function

    Private Sub ValidateTextBox(ByRef thisTextBox As TextBox, defaultText As String)
        If thisTextBox.TextLength = 0 Then
            thisTextBox.Text = defaultText
        End If
    End Sub

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
        IniFileSaveOptions(False, True)
    End Sub

    Private Sub txtDigestProteinsMinimumMass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDigestProteinsMinimumMass.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtDigestProteinsMinimumMass, e, True)
    End Sub

    Private Sub txtDigestProteinsMaximumMissedCleavages_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDigestProteinsMaximumMissedCleavages.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtDigestProteinsMaximumMissedCleavages, e, True)
    End Sub

    Private Sub txtDigestProteinsMinimumResidueCount_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDigestProteinsMinimumResidueCount.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtDigestProteinsMinimumResidueCount, e, True)
    End Sub

    Private Sub txtDigestProteinsMaximumMass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtDigestProteinsMaximumMass.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtDigestProteinsMaximumMass, e, True)
    End Sub

    Private Sub txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtMaxPeakMatchingResultsPerFeatureToSave.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtMaxPeakMatchingResultsPerFeatureToSave, e, True)
    End Sub

    Private Sub txtMaxPeakMatchingResultsPerFeatureToSave_Validating(sender As Object, e As CancelEventArgs) Handles txtMaxPeakMatchingResultsPerFeatureToSave.Validating
        If txtMaxPeakMatchingResultsPerFeatureToSave.Text.Trim = "0" Then txtMaxPeakMatchingResultsPerFeatureToSave.Text = "1"
        TextBoxUtils.ValidateTextBoxInt(txtMaxPeakMatchingResultsPerFeatureToSave, 1, 100, 3)
    End Sub

    Private Sub txtMaxpISequenceLength_KeyDown(sender As Object, e As KeyEventArgs) Handles txtMaxpISequenceLength.KeyDown
        If e.KeyCode = Keys.Enter AndAlso chkMaxpIModeEnabled.Checked Then ComputeSequencepI()
    End Sub

    Private Sub txtMaxpISequenceLength_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtMaxpISequenceLength.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtMaxpISequenceLength, e, True, False)
    End Sub

    Private Sub txtMaxpISequenceLength_Validating(sender As Object, e As CancelEventArgs) Handles txtMaxpISequenceLength.Validating
        TextBoxUtils.ValidateTextBoxInt(txtMaxpISequenceLength, 1, 10000, 10)
    End Sub

    Private Sub txtMaxpISequenceLength_Validated(sender As Object, e As EventArgs) Handles txtMaxpISequenceLength.Validated
        ComputeSequencepI()
    End Sub

    Private Sub txtMinimumSLiCScore_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtMinimumSLiCScore.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtMinimumSLiCScore, e, True, True)
    End Sub

    Private Sub txtMinimumSLiCScore_Validating(sender As Object, e As CancelEventArgs) Handles txtMinimumSLiCScore.Validating
        TextBoxUtils.ValidateTextBoxFloat(txtMinimumSLiCScore, 0, 1, 0.95)
    End Sub

    Private Sub txtProteinInputFilePath_TextChanged(sender As Object, e As EventArgs) Handles txtProteinInputFilePath.TextChanged
        EnableDisableControls()
        AutoDefineOutputFile()
    End Sub

    Private Sub txtProteinReversalSamplingPercentage_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtProteinReversalSamplingPercentage.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtProteinReversalSamplingPercentage, e, True)
    End Sub

    Private Sub txtProteinScramblingLoopCount_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtProteinScramblingLoopCount.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtProteinScramblingLoopCount, e, True)
    End Sub

    ' ReSharper disable once IdentifierTypo
    Private Sub txtSequenceForpI_TextChanged(sender As Object, e As EventArgs) Handles txtSequenceForpI.TextChanged
        ComputeSequencepI()
    End Sub

    Private Sub txtUniquenessBinWidth_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUniquenessBinWidth.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtUniquenessBinWidth, e, True)
    End Sub

    Private Sub txtUniquenessBinStartMass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUniquenessBinStartMass.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtUniquenessBinStartMass, e, True)
    End Sub

    Private Sub txtUniquenessBinEndMass_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtUniquenessBinEndMass.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtUniquenessBinEndMass, e, True)
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
        IniFileSaveOptions(True)
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

    Private Sub mParseProteinFile_ErrorEvent(message As String, ex As Exception) Handles mParseProteinFile.ErrorEvent
        lblErrorMessage.Text = "Error in mParseProteinFile: " & message
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

        Application.DoEvents()
    End Sub

    Private Sub mParseProteinFile_ProgressReset() Handles mParseProteinFile.ProgressReset
        ResetProgress()
    End Sub

    Private Sub mParseProteinFile_SubtaskProgressChanged(taskDescription As String, percentComplete As Single) Handles mParseProteinFile.SubtaskProgressChanged
        lblSubtaskProgressDescription.Text = taskDescription
        lblSubtaskProgress.Text = FormatPercentComplete(percentComplete)
        Application.DoEvents()
    End Sub

    Private Sub mProteinDigestionSimulator_ErrorEvent(message As String, ex As Exception) Handles mProteinDigestionSimulator.ErrorEvent
        lblErrorMessage.Text = "Error in mProteinDigestionSimulator: " & message
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

        Application.DoEvents()
    End Sub

    Private Sub mProteinDigestionSimulator_ProgressReset() Handles mProteinDigestionSimulator.ProgressReset
        ResetProgress()
    End Sub

    Private Sub mProteinDigestionSimulator_SubtaskProgressChanged(taskDescription As String, percentComplete As Single) Handles mProteinDigestionSimulator.SubtaskProgressChanged
        lblSubtaskProgressDescription.Text = taskDescription
        lblSubtaskProgress.Text = FormatPercentComplete(percentComplete)
        Application.DoEvents()
    End Sub

#End Region
End Class