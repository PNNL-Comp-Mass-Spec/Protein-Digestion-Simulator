Option Strict On

Imports System.IO
Imports System.Text
Imports PRISM.DatabaseUtils
Imports PRISMWin
Imports ValidateFastaFile

Public Class frmFastaValidation

    Public Sub New(fastaFilePath As String)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        InitializeForm(fastaFilePath)
    End Sub


#Region "Constants and Enums"

    Private Const COL_NAME_LINE As String = "Line"
    Private Const COL_NAME_COLUMN As String = "Column"
    Private Const COL_NAME_PROTEIN As String = "Protein"
    Private Const COL_NAME_DESCRIPTION As String = "Description"
    Private Const COL_NAME_CONTEXT As String = "Context"

#End Region

#Region "Structures"
    Public Structure udtFastaValidationOptionsType
        Public Initialized As Boolean

        Public MaximumErrorsToTrackInDetail As Integer
        Public MaximumResiduesPerLine As Integer

        Public ValidProteinNameLengthMinimum As Integer
        Public ValidProteinNameLengthMaximum As Integer
        Public AllowAsterisksInResidues As Boolean
        Public CheckForDuplicateProteinNames As Boolean
        Public LogResultsToFile As Boolean
        Public SaveHashInfoFile As Boolean

        Public FixedFastaOptions As udtFixedFastaOptionsType
    End Structure

    Public Structure udtFixedFastaOptionsType
        Public GenerateFixedFasta As Boolean
        Public TruncateLongProteinName As Boolean
        Public RenameProteinWithDuplicateNames As Boolean
        Public KeepDuplicateNamedProteins As Boolean
        Public WrapLongResidueLines As Boolean
        Public ResiduesPerLineForWrap As Integer
        Public RemoveInvalidResidues As Boolean
        Public SplitOutMultipleRefsForKnownAccession As Boolean
        Public SplitOutMultipleRefsInProteinName As Boolean
        Public ConsolidateDuplicateProteins As Boolean
        Public ConsolidateDupsIgnoreILDiff As Boolean
    End Structure

#End Region
#Region "Classwide variables"

    Private mErrorsDataset As DataSet
    Private mErrorsDataView As DataView

    Private mWarningsDataset As DataSet
    Private mWarningsDataView As DataView

    Private mKeepDuplicateNamedProteinsLastValue As Boolean = False

    ' This timer is used to cause StartValidation to be called after the form becomes visible
    Private WithEvents mValidationTriggerTimer As Timer
    Private WithEvents mValidateFastaFile As clsValidateFastaFile

    Private mValidatorErrorMessage As String

    Public Event FastaValidationStarted()
#End Region

#Region "Processing Options Interface Functions"
    Public Property CustomRulesFilePath As String
        Get
            Return txtCustomValidationRulesFilePath.Text
        End Get
        Set
            txtCustomValidationRulesFilePath.Text = Value
        End Set
    End Property

    Public Property FastaFilePath As String

    Public Property TextFontSize As Single
        Get
            Return txtResults.Font.SizeInPoints
        End Get
        Set
            If Value < 6 Then
                Value = 6
            ElseIf Value > 72 Then
                Value = 72
            End If

            Try
                txtResults.Font = New Font(txtResults.Font.FontFamily, Value)

                dgErrors.Font = New Font(txtResults.Font.FontFamily, Value)
                dgWarnings.Font = New Font(txtResults.Font.FontFamily, Value)

            Catch ex As Exception
                ' Ignore errors here
            End Try

        End Set
    End Property
#End Region

#Region "Procedures"

    Private Sub AppendToString(ByRef textLine As String, newText As String)
        textLine &= newText & ControlChars.NewLine
    End Sub

    Private Sub AppendToString(ByRef textLine As String, numberDescription As String, number As Long)
        AppendToString(textLine, numberDescription, number, True)
    End Sub

    Private Sub AppendToString(ByRef textLine As String, numberDescription As String, number As Long, useCommaSeparator As Boolean)

        ' Dim nfi As NumberFormatInfo = New CultureInfo("en-US", False).NumberFormat
        If useCommaSeparator Then
            textLine &= numberDescription & number.ToString("###,###,###,###,##0") & ControlChars.NewLine
        Else
            textLine &= numberDescription & number.ToString() & ControlChars.NewLine
        End If

    End Sub

    Private Sub CopyAllResults()
        Clipboard.SetDataObject(txtResults.Text & ControlChars.NewLine & FlattenDataView(mErrorsDataView) & ControlChars.NewLine & FlattenDataView(mWarningsDataView), True)
    End Sub

    Private Sub CopyErrorsDataView()
        Clipboard.SetDataObject(FlattenDataView(mErrorsDataView), True)
    End Sub

    Private Sub CopyWarningsDataView()
        Clipboard.SetDataObject(FlattenDataView(mWarningsDataView), True)
    End Sub

    Private Sub CreateDefaultValidationRulesFile()

        Dim dialog As New SaveFileDialog() With {
            .AddExtension = True,
            .CheckFileExists = False,
            .CheckPathExists = True,
            .CreatePrompt = False,
            .DefaultExt = ".xml",
            .DereferenceLinks = True,
            .OverwritePrompt = False,
            .ValidateNames = True,
            .Filter = "XML Settings Files (*.xml)|*.xml|All files (*.*)|*.*",
            .FilterIndex = 1
        }

        If Len(txtCustomValidationRulesFilePath.Text.Length) > 0 Then
            Try
                dialog.InitialDirectory = Directory.GetParent(txtCustomValidationRulesFilePath.Text).ToString()
            Catch
                dialog.InitialDirectory = GetApplicationDataFolderPath()
            End Try
        Else
            dialog.InitialDirectory = GetApplicationDataFolderPath()
        End If

        dialog.Title = "Select/Create file to save custom rule definitions"

        dialog.ShowDialog()
        If dialog.FileName.Length > 0 Then
            Dim customRuleDefinitionsFilePath = dialog.FileName

            Try
                Dim validateFastaFile As New clsValidateFastaFile()
                validateFastaFile.SaveSettingsToParameterFile(customRuleDefinitionsFilePath)

                If txtCustomValidationRulesFilePath.TextLength = 0 Then
                    txtCustomValidationRulesFilePath.Text = customRuleDefinitionsFilePath
                End If

                MessageBox.Show("File " & customRuleDefinitionsFilePath & " now contains the default rule validation settings.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Catch ex As Exception
                MessageBox.Show("Error creating/updating file: " & customRuleDefinitionsFilePath & ControlChars.NewLine & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End Try

        End If

    End Sub

    ''' <summary>
    ''' Display the results
    ''' </summary>
    ''' <param name="parameterFilePath"></param>
    Private Sub DisplayResults(parameterFilePath As String)

        Const sepChar = ControlChars.Tab

        Dim results = "Results for file " & mValidateFastaFile.FastaFilePath & ControlChars.NewLine

        AppendToString(results, "Protein count = " & mValidateFastaFile.ProteinCount & sepChar & sepChar & "Residue count = ", mValidateFastaFile.ResidueCount)
        AppendToString(results, "Error count = " & mValidateFastaFile.ErrorWarningCounts(clsValidateFastaFile.eMsgTypeConstants.ErrorMsg, clsValidateFastaFile.ErrorWarningCountTypes.Total))
        AppendToString(results, "Warning count = ", mValidateFastaFile.ErrorWarningCounts(clsValidateFastaFile.eMsgTypeConstants.WarningMsg, clsValidateFastaFile.ErrorWarningCountTypes.Total))

        If mValidateFastaFile.OptionSwitch(clsValidateFastaFile.SwitchOptions.GenerateFixedFASTAFile) Then
            AppendToString(results, "Count of long protein names that were truncated = " & mValidateFastaFile.FixedFASTAFileStats(clsValidateFastaFile.FixedFASTAFileValues.TruncatedProteinNameCount))
            AppendToString(results, "Count of protein names with invalid chars removed = " & mValidateFastaFile.FixedFASTAFileStats(clsValidateFastaFile.FixedFASTAFileValues.ProteinNamesInvalidCharsReplaced))
            AppendToString(results, "Count of protein names with multiple refs split out = " & mValidateFastaFile.FixedFASTAFileStats(clsValidateFastaFile.FixedFASTAFileValues.ProteinNamesMultipleRefsRemoved))
            AppendToString(results, "Count of residue lines with invalid chars removed = " & mValidateFastaFile.FixedFASTAFileStats(clsValidateFastaFile.FixedFASTAFileValues.UpdatedResidueLines))

            If mValidateFastaFile.OptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaRenameDuplicateNameProteins) Then
                AppendToString(results, "Count of proteins renamed due to duplicate names = " & mValidateFastaFile.FixedFASTAFileStats(clsValidateFastaFile.FixedFASTAFileValues.DuplicateProteinNamesRenamedCount))
            ElseIf mValidateFastaFile.OptionSwitch(clsValidateFastaFile.SwitchOptions.CheckForDuplicateProteinNames) Then
                AppendToString(results, "Count of proteins skipped due to duplicate names = " & mValidateFastaFile.FixedFASTAFileStats(clsValidateFastaFile.FixedFASTAFileValues.DuplicateProteinNamesSkippedCount))
            End If

            If mValidateFastaFile.OptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaConsolidateDuplicateProteinSeqs) Then
                AppendToString(results, "Count of proteins removed due to duplicate sequences = " & mValidateFastaFile.FixedFASTAFileStats(clsValidateFastaFile.FixedFASTAFileValues.DuplicateProteinSeqsSkippedCount))
            End If

        End If

        If parameterFilePath.Length > 0 Then
            AppendToString(results, "Used validation rules from file " & parameterFilePath)
        Else
            AppendToString(results, "Default validation rules were used.")
        End If

        If mValidateFastaFile.OptionSwitch(clsValidateFastaFile.SwitchOptions.OutputToStatsFile) Then
            AppendToString(results, "Results were logged to file: " & mValidateFastaFile.StatsFilePath())
        End If

        txtResults.Text = results
        txtResults.SelectionStart = 0
        txtResults.SelectionLength = 0

        ' Clear the filters
        txtFilterData.Text = String.Empty

        If mValidateFastaFile.ErrorWarningCounts(clsValidateFastaFile.eMsgTypeConstants.ErrorMsg, clsValidateFastaFile.ErrorWarningCountTypes.Specified) > 0 Then
            ' List all of the errors
            PopulateMsgResultsDataGrid(dgErrors, mErrorsDataset, mValidateFastaFile.FileErrorList)
        Else
            mErrorsDataset.Tables(0).Clear()
        End If

        If mValidateFastaFile.ErrorWarningCounts(clsValidateFastaFile.eMsgTypeConstants.WarningMsg, clsValidateFastaFile.ErrorWarningCountTypes.Specified) > 0 Then
            ' List all of the warnings in the DataGrid
            PopulateMsgResultsDataGrid(dgWarnings, mWarningsDataset, mValidateFastaFile.FileWarningList)
        Else
            mWarningsDataset.Tables(0).Clear()
        End If

        PositionControls()
        FilterLists()
    End Sub

    Private Sub EnableDisableControls()
        Dim enableFixedFastaOptions As Boolean

        enableFixedFastaOptions = chkGenerateFixedFastaFile.Checked

        txtLongProteinNameSplitChars.Enabled = enableFixedFastaOptions
        txtInvalidProteinNameCharsToRemove.Enabled = enableFixedFastaOptions
        txtResiduesPerLineForWrap.Enabled = enableFixedFastaOptions And chkWrapLongResidueLines.Checked

        chkSplitOutMultipleRefsInProteinName.Enabled = enableFixedFastaOptions
        chkRenameDuplicateProteins.Enabled = enableFixedFastaOptions

        If enableFixedFastaOptions Then
            If chkRenameDuplicateProteins.Checked Then
                chkKeepDuplicateNamedProteins.Enabled = False
                chkKeepDuplicateNamedProteins.Checked = False
            Else
                chkKeepDuplicateNamedProteins.Enabled = True
                chkKeepDuplicateNamedProteins.Checked = mKeepDuplicateNamedProteinsLastValue
            End If
        Else
            chkKeepDuplicateNamedProteins.Enabled = False
        End If


        chkConsolidateDuplicateProteinSeqs.Enabled = enableFixedFastaOptions
        chkConsolidateDupsIgnoreILDiff.Enabled = enableFixedFastaOptions And chkConsolidateDuplicateProteinSeqs.Checked

        chkTruncateLongProteinNames.Enabled = enableFixedFastaOptions
        chkSplitOutMultipleRefsForKnownAccession.Enabled = enableFixedFastaOptions
        chkWrapLongResidueLines.Enabled = enableFixedFastaOptions
        chkRemoveInvalidResidues.Enabled = enableFixedFastaOptions

        If txtCustomValidationRulesFilePath.TextLength > 0 Then
            chkAllowAsteriskInResidues.Enabled = False
        Else
            chkAllowAsteriskInResidues.Enabled = True
        End If

    End Sub

    ''' <summary>
    ''' Copy the contents of the DataGrid to the clipboard
    ''' </summary>
    ''' <param name="dvDataView"></param>
    ''' <returns></returns>
    Private Function FlattenDataView(dvDataView As DataView) As String

        Const sepChar As Char = ControlChars.Tab

        Dim flattenedText As String = String.Empty
        Dim index As Integer
        Dim columnCount As Integer

        Try
            columnCount = dvDataView.Table.Columns.Count
            For index = 0 To columnCount - 1
                If index < columnCount - 1 Then
                    flattenedText &= dvDataView.Table.Columns.Item(index).ColumnName.ToString() & sepChar
                Else
                    flattenedText &= dvDataView.Table.Columns.Item(index).ColumnName.ToString() & ControlChars.NewLine
                End If
            Next index

            For Each currentRow As DataRow In dvDataView.Table.Rows
                For index = 0 To columnCount - 1
                    If index < columnCount - 1 Then
                        flattenedText &= currentRow(index).ToString() & sepChar
                    Else
                        flattenedText &= currentRow(index).ToString() & ControlChars.NewLine
                    End If
                Next index
            Next

        Catch ex As Exception
            flattenedText &= ControlChars.NewLine & ControlChars.NewLine & "Error copying data: " & ex.Message
        End Try

        Return flattenedText

    End Function

    Private Sub CopySummaryText()

        Dim selStart = txtResults.SelectionStart
        Dim selLength = txtResults.SelectionLength

        txtResults.SelectAll()
        txtResults.Copy()

        txtResults.SelectionStart = selStart
        txtResults.SelectionLength = selLength

    End Sub

    Private Sub FilterLists()

        Dim filter As String

        Try
            filter = String.Empty
            If txtFilterData.TextLength > 0 Then
                filter = "[" & COL_NAME_PROTEIN & "] LIKE '%" & txtFilterData.Text & "%' OR [" & COL_NAME_DESCRIPTION & "] LIKE '%" & txtFilterData.Text & "%' OR [" & COL_NAME_CONTEXT & "] LIKE '%" & txtFilterData.Text & "%'"
            End If

            mErrorsDataView.RowFilter = filter
            dgErrors.Update()

            mWarningsDataView.RowFilter = filter
            dgWarnings.Update()

        Catch ex As Exception
            MessageBox.Show("Error filtering lists: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try

    End Sub

    Private Function GetApplicationDataFolderPath() As String
        Dim appDataFolderPath As String = String.Empty

        Try
            appDataFolderPath = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
               "PAST Toolkit\ProteinDigestionSimulator")

            If Not Directory.Exists(appDataFolderPath) Then
                Directory.CreateDirectory(appDataFolderPath)
            End If

        Catch ex As Exception
            ' Ignore errors here; an exception will likely be thrown by the calling function that is trying to access this non-existent application data folder
        End Try

        Return appDataFolderPath

    End Function

    Public Function GetOptions() As udtFastaValidationOptionsType

        Dim udtFastaValidationOptions = New udtFastaValidationOptionsType With {
            .MaximumErrorsToTrackInDetail = TextBoxUtils.ParseTextBoxValueInt(txtMaxFileErrorsToTrack, "", False, 10, False),
            .MaximumResiduesPerLine = TextBoxUtils.ParseTextBoxValueInt(txtMaximumResiduesPerLine, "", False, 120, False),
            .ValidProteinNameLengthMinimum = TextBoxUtils.ParseTextBoxValueInt(txtProteinNameLengthMinimum, "", False, 3, False),
            .ValidProteinNameLengthMaximum = TextBoxUtils.ParseTextBoxValueInt(txtProteinNameLengthMaximum, "", False, 34, False),
            .AllowAsterisksInResidues = chkAllowAsteriskInResidues.Checked,
            .CheckForDuplicateProteinNames = chkCheckForDuplicateProteinInfo.Checked,
            .LogResultsToFile = chkLogResults.Checked,
            .SaveHashInfoFile = chkSaveBasicProteinHashInfoFile.Checked
        }

        udtFastaValidationOptions.FixedFastaOptions.GenerateFixedFasta = chkGenerateFixedFastaFile.Checked
        udtFastaValidationOptions.FixedFastaOptions.TruncateLongProteinName = chkTruncateLongProteinNames.Checked
        udtFastaValidationOptions.FixedFastaOptions.RenameProteinWithDuplicateNames = chkRenameDuplicateProteins.Checked
        udtFastaValidationOptions.FixedFastaOptions.KeepDuplicateNamedProteins = chkKeepDuplicateNamedProteins.Checked
        udtFastaValidationOptions.FixedFastaOptions.WrapLongResidueLines = chkWrapLongResidueLines.Checked
        udtFastaValidationOptions.FixedFastaOptions.RemoveInvalidResidues = chkRemoveInvalidResidues.Checked
        udtFastaValidationOptions.FixedFastaOptions.SplitOutMultipleRefsForKnownAccession = chkSplitOutMultipleRefsForKnownAccession.Checked
        udtFastaValidationOptions.FixedFastaOptions.SplitOutMultipleRefsInProteinName = chkSplitOutMultipleRefsInProteinName.Checked
        udtFastaValidationOptions.FixedFastaOptions.ConsolidateDuplicateProteins = chkConsolidateDuplicateProteinSeqs.Checked
        udtFastaValidationOptions.FixedFastaOptions.ConsolidateDupsIgnoreILDiff = chkConsolidateDupsIgnoreILDiff.Checked
        udtFastaValidationOptions.FixedFastaOptions.ResiduesPerLineForWrap = TextBoxUtils.ParseTextBoxValueInt(txtResiduesPerLineForWrap, "", False, 60, False)

        udtFastaValidationOptions.Initialized = True

        Return udtFastaValidationOptions

    End Function

    Private Sub InitializeDataGrid(dgDataGrid As DataGrid, ByRef dsDataset As DataSet, ByRef dvDataView As DataView, eMsgType As clsValidateFastaFile.eMsgTypeConstants)

        Dim dtDataTable As DataTable

        Dim msgColumnName As String
        Dim datasetName As String
        Dim dataTableName As String

        If eMsgType = clsValidateFastaFile.eMsgTypeConstants.WarningMsg Then
            msgColumnName = "Warnings"
        ElseIf eMsgType = clsValidateFastaFile.eMsgTypeConstants.ErrorMsg Then
            msgColumnName = "Errors"
        Else
            msgColumnName = "Status"
        End If

        datasetName = "ds" & msgColumnName
        dataTableName = "T_" & msgColumnName

        ' Create a DataTable
        dtDataTable = New DataTable(dataTableName)

        ' Add the columns to the DataTable
        DataTableUtils.AppendColumnIntegerToTable(dtDataTable, COL_NAME_LINE)
        DataTableUtils.AppendColumnIntegerToTable(dtDataTable, COL_NAME_COLUMN)
        DataTableUtils.AppendColumnStringToTable(dtDataTable, COL_NAME_PROTEIN)
        DataTableUtils.AppendColumnStringToTable(dtDataTable, COL_NAME_DESCRIPTION)
        DataTableUtils.AppendColumnStringToTable(dtDataTable, COL_NAME_CONTEXT)

        ' Could define a primary key
        ''With dtDataTable
        ''    Dim PrimaryKeyColumn As DataColumn() = New DataColumn() {.Columns(COL_NAME_LINE)}
        ''    .PrimaryKey = PrimaryKeyColumn
        ''End With

        ' Instantiate the DataSet
        dsDataset = New DataSet(datasetName)

        ' Add the table to the DataSet
        dsDataset.Tables.Add(dtDataTable)

        ' Instantiate the DataView
        dvDataView = New DataView With {
            .Table = dsDataset.Tables(dataTableName),
            .RowFilter = String.Empty
        }

        ' Bind the DataSet to the DataGrid
        dgDataGrid.DataSource = dvDataView
        dgDataGrid.ReadOnly = False
        dgDataGrid.CaptionText = msgColumnName

        ' Update the grid's table style
        UpdateDataGridTableStyle(dgDataGrid, dataTableName)

    End Sub

    Private Sub InitializeForm(fastaFilePathToValidate As String)
        txtResults.ReadOnly = True
        Me.TextFontSize = 10

        InitializeDataGrid(dgErrors, mErrorsDataset, mErrorsDataView, clsValidateFastaFile.eMsgTypeConstants.ErrorMsg)
        InitializeDataGrid(dgWarnings, mWarningsDataset, mWarningsDataView, clsValidateFastaFile.eMsgTypeConstants.WarningMsg)

        mValidationTriggerTimer = New Timer With {
            .Interval = 100,
            .Enabled = True
        }

        SetNewFastaFile(fastaFilePathToValidate)

        EnableDisableControls()
        SetToolTips()

        ResetOptionsToDefault()

    End Sub

    Private Sub PopulateMsgResultsDataGrid(dgDataGrid As Control, dsDataset As DataSet, itemList As IEnumerable(Of clsValidateFastaFile.udtMsgInfoType))

        ' Clear the table
        dsDataset.Tables(0).Clear()

        ' Populate it with new data
        For Each item In itemList
            Dim currentRow = dsDataset.Tables(0).NewRow()
            currentRow(COL_NAME_LINE) = item.LineNumber
            currentRow(COL_NAME_COLUMN) = item.ColNumber
            If item.ProteinName Is Nothing OrElse item.ProteinName.Length = 0 Then
                currentRow(COL_NAME_PROTEIN) = "N/A"
            Else
                currentRow(COL_NAME_PROTEIN) = item.ProteinName
            End If

            currentRow(COL_NAME_DESCRIPTION) = mValidateFastaFile.LookupMessageDescription(item.MessageCode, item.ExtraInfo)
            currentRow(COL_NAME_CONTEXT) = item.Context

            ' Add the row to the table
            dsDataset.Tables(0).Rows.Add(currentRow)
        Next

        dgDataGrid.Update()

    End Sub

    Private Sub PositionControls()

        Const MAX_RATIO As Single = 1.5
        Const MENU_HEIGHT = 80

        Dim desiredHeight As Integer

        Dim errorToWarningsRatio As Double = 1

        Try
            If Not mErrorsDataset Is Nothing AndAlso Not mWarningsDataset Is Nothing Then
                If mErrorsDataset.Tables(0).Rows.Count = 0 And mWarningsDataset.Tables(0).Rows.Count = 0 Then
                    errorToWarningsRatio = 1
                ElseIf mErrorsDataset.Tables(0).Rows.Count = 0 Then
                    errorToWarningsRatio = 1 / MAX_RATIO
                ElseIf mWarningsDataset.Tables(0).Rows.Count = 0 Then
                    errorToWarningsRatio = MAX_RATIO
                Else
                    errorToWarningsRatio = mErrorsDataset.Tables(0).Rows.Count / mWarningsDataset.Tables(0).Rows.Count

                    If errorToWarningsRatio < 1 / MAX_RATIO Then
                        errorToWarningsRatio = 1 / MAX_RATIO
                    ElseIf errorToWarningsRatio > MAX_RATIO Then
                        errorToWarningsRatio = MAX_RATIO
                    End If
                End If
            End If
        Catch ex As Exception
            errorToWarningsRatio = 1
        End Try

        If errorToWarningsRatio >= 1 Then
            ' Errors grid should be taller
            desiredHeight = CInt(Math.Round((Me.Height - dgErrors.Top - MENU_HEIGHT) / (errorToWarningsRatio + 1) * errorToWarningsRatio, 0))
        Else
            ' Errors grid should be shorter
            desiredHeight = CInt(Math.Round((Me.Height - dgErrors.Top - MENU_HEIGHT) / (1 / errorToWarningsRatio + 1), 0)) - 2
        End If

        If desiredHeight < 5 Then desiredHeight = 5
        dgErrors.Height = desiredHeight

        dgWarnings.Top = dgErrors.Top + dgErrors.Height + 10

        desiredHeight = CInt(Math.Round(desiredHeight / errorToWarningsRatio, 0))
        If desiredHeight < 5 Then desiredHeight = 5
        dgWarnings.Height = desiredHeight

    End Sub

    Private Sub ResetOptionsToDefault()
        txtMaxFileErrorsToTrack.Text = "10"

        txtMaximumResiduesPerLine.Text = "120"

        txtProteinNameLengthMinimum.Text = "3"
        txtProteinNameLengthMaximum.Text = "34"

        txtLongProteinNameSplitChars.Text = clsValidateFastaFile.DEFAULT_LONG_PROTEIN_NAME_SPLIT_CHAR
        txtInvalidProteinNameCharsToRemove.Text = ""
        txtResiduesPerLineForWrap.Text = "60"

        chkAllowAsteriskInResidues.Checked = False
        chkCheckForDuplicateProteinInfo.Checked = True
        chkSaveBasicProteinHashInfoFile.Checked = False

        chkLogResults.Checked = False

        ' Note: Leave this option unchanged
        'chkGenerateFixedFastaFile.Checked = False

        chkConsolidateDuplicateProteinSeqs.Checked = True
        chkConsolidateDupsIgnoreILDiff.Checked = True

        chkRemoveInvalidResidues.Checked = False
        chkRenameDuplicateProteins.Checked = True
        chkKeepDuplicateNamedProteins.Checked = False
        chkSplitOutMultipleRefsForKnownAccession.Checked = False
        chkSplitOutMultipleRefsInProteinName.Checked = False
        chkTruncateLongProteinNames.Checked = True
        chkWrapLongResidueLines.Checked = True
    End Sub

    Private Sub SelectCustomRulesFile()

        Dim dialog As New OpenFileDialog() With {
            .AddExtension = True,
            .CheckFileExists = True,
            .CheckPathExists = True,
            .DefaultExt = ".xml",
            .DereferenceLinks = True,
            .Multiselect = False,
            .ValidateNames = True,
            .Filter = "XML Settings Files (*.xml)|*.xml|All files (*.*)|*.*",
            .FilterIndex = 1
        }

        If Len(txtCustomValidationRulesFilePath.Text.Length) > 0 Then
            Try
                dialog.InitialDirectory = Directory.GetParent(txtCustomValidationRulesFilePath.Text).ToString()
            Catch
                dialog.InitialDirectory = GetApplicationDataFolderPath()
            End Try
        Else
            dialog.InitialDirectory = GetApplicationDataFolderPath()
        End If

        dialog.Title = "Select custom rules file"

        dialog.ShowDialog()
        If dialog.FileName.Length > 0 Then
            txtCustomValidationRulesFilePath.Text = dialog.FileName
        End If

    End Sub

    Public Sub SetNewFastaFile(fastaFilePathToValidate As String)
        Me.FastaFilePath = fastaFilePathToValidate

        txtResults.Text = String.Empty

        ' Clear the filters
        txtFilterData.Text = String.Empty

        mErrorsDataset.Tables(0).Clear()
        mWarningsDataset.Tables(0).Clear()

    End Sub

    Private Sub ShowAboutBox()
        Dim message = New StringBuilder()

        message.AppendLine("Fasta File Validation module written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2005")
        message.AppendLine("Copyright 2018 Battelle Memorial Institute")
        message.AppendLine()
        message.AppendLine("This is version " & Application.ProductVersion & " (" & PROGRAM_DATE & ")")
        message.AppendLine()
        message.AppendLine("E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov")
        message.AppendLine("Website: https://omics.pnl.gov/ or https://panomics.pnnl.gov/")
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

    Private Sub ShowHideObjectsDuringValidation(validating As Boolean)
        pbarProgress.Visible = validating

        lblFilterData.Visible = Not validating
        txtFilterData.Visible = Not validating

        cmdValidateFastaFile.Visible = Not validating
        cmdValidateFastaFile.Enabled = Not validating

        cmdCancel.Visible = validating
        cmdCancel.Enabled = validating

    End Sub

    Public Sub SetOptions(udtFastaValidationOptions As udtFastaValidationOptionsType)

        txtMaxFileErrorsToTrack.Text = udtFastaValidationOptions.MaximumErrorsToTrackInDetail.ToString()
        txtMaximumResiduesPerLine.Text = udtFastaValidationOptions.MaximumResiduesPerLine.ToString()

        txtProteinNameLengthMinimum.Text = udtFastaValidationOptions.ValidProteinNameLengthMinimum.ToString()
        txtProteinNameLengthMaximum.Text = udtFastaValidationOptions.ValidProteinNameLengthMaximum.ToString()

        chkAllowAsteriskInResidues.Checked = udtFastaValidationOptions.AllowAsterisksInResidues
        chkCheckForDuplicateProteinInfo.Checked = udtFastaValidationOptions.CheckForDuplicateProteinNames
        chkLogResults.Checked = udtFastaValidationOptions.LogResultsToFile
        chkSaveBasicProteinHashInfoFile.Checked = udtFastaValidationOptions.SaveHashInfoFile

        chkGenerateFixedFastaFile.Checked = udtFastaValidationOptions.FixedFastaOptions.GenerateFixedFasta
        chkTruncateLongProteinNames.Checked = udtFastaValidationOptions.FixedFastaOptions.TruncateLongProteinName
        chkRenameDuplicateProteins.Checked = udtFastaValidationOptions.FixedFastaOptions.RenameProteinWithDuplicateNames
        chkKeepDuplicateNamedProteins.Checked = udtFastaValidationOptions.FixedFastaOptions.KeepDuplicateNamedProteins
        chkWrapLongResidueLines.Checked = udtFastaValidationOptions.FixedFastaOptions.WrapLongResidueLines
        chkRemoveInvalidResidues.Checked = udtFastaValidationOptions.FixedFastaOptions.RemoveInvalidResidues
        chkSplitOutMultipleRefsForKnownAccession.Checked = udtFastaValidationOptions.FixedFastaOptions.SplitOutMultipleRefsForKnownAccession
        chkSplitOutMultipleRefsInProteinName.Checked = udtFastaValidationOptions.FixedFastaOptions.SplitOutMultipleRefsInProteinName
        chkConsolidateDuplicateProteinSeqs.Checked = udtFastaValidationOptions.FixedFastaOptions.ConsolidateDuplicateProteins
        chkConsolidateDupsIgnoreILDiff.Checked = udtFastaValidationOptions.FixedFastaOptions.ConsolidateDupsIgnoreILDiff
        txtResiduesPerLineForWrap.Text = udtFastaValidationOptions.FixedFastaOptions.ResiduesPerLineForWrap.ToString()

        Application.DoEvents()

    End Sub

    Private Sub SetToolTips()
        Dim toolTipControl As New ToolTip()

        toolTipControl.SetToolTip(txtLongProteinNameSplitChars, "Enter one or more characters to look for when truncating long protein names (do not separate the characters by commas).  Default character is a vertical bar.")
        toolTipControl.SetToolTip(txtInvalidProteinNameCharsToRemove, "Enter one or more characters to look and replace with an underscore (do not separate the characters by commas).  Leave blank to not replace any characters.")
        toolTipControl.SetToolTip(chkSplitOutMultipleRefsForKnownAccession, "If a protein name matches the standard IPI, GI, or JGI accession numbers, and if it contains additional reference information, then the additional information will be moved to the protein's description.")
        toolTipControl.SetToolTip(chkSaveBasicProteinHashInfoFile, "To minimize memory usage, enable this option by disable 'Check for Duplicate Proteins'")

    End Sub

    Private Sub StartValidation()

        Dim parameterFilePath As String
        Dim fileExists As Boolean

        Dim success As Boolean

        Try
            If mValidateFastaFile Is Nothing Then
                mValidateFastaFile = New clsValidateFastaFile
            End If

            mValidatorErrorMessage = String.Empty

            RaiseEvent FastaValidationStarted()

            ShowHideObjectsDuringValidation(True)
            Cursor.Current = Cursors.WaitCursor
            Application.DoEvents()

            ' Note: the following settings will be overridden if a parameter file with these settings defined is provided to .ProcessFile()
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.WarnBlankLinesBetweenProteins, True)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.AllowAsteriskInResidues, chkAllowAsteriskInResidues.Checked)

            mValidateFastaFile.MinimumProteinNameLength = TextBoxUtils.ParseTextBoxValueInt(txtProteinNameLengthMinimum, "Minimum protein name length should be a number", False, 3, False)
            mValidateFastaFile.MaximumProteinNameLength = TextBoxUtils.ParseTextBoxValueInt(txtProteinNameLengthMaximum, "Maximum protein name length should be a number", False, 34, False)

            If chkGenerateFixedFastaFile.Checked AndAlso chkWrapLongResidueLines.Checked Then
                mValidateFastaFile.MaximumResiduesPerLine = TextBoxUtils.ParseTextBoxValueInt(txtResiduesPerLineForWrap, "Residues per line for wrapping should be a number", False, 60, False)
            Else
                mValidateFastaFile.MaximumResiduesPerLine = TextBoxUtils.ParseTextBoxValueInt(txtMaximumResiduesPerLine, "Maximum residues per line should be a number", False, 120, False)
            End If

            parameterFilePath = txtCustomValidationRulesFilePath.Text
            If parameterFilePath.Length > 0 Then
                Try
                    fileExists = File.Exists(parameterFilePath)
                Catch ex As Exception
                    fileExists = False
                End Try

                If Not fileExists Then
                    MessageBox.Show("Custom rules validation file not found: " & ControlChars.NewLine & parameterFilePath & ControlChars.NewLine &
                                    "The default validation rules will be used instead.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    parameterFilePath = String.Empty
                Else
                    mValidateFastaFile.LoadParameterFileSettings(parameterFilePath)
                End If
            End If

            If parameterFilePath.Length = 0 Then
                mValidateFastaFile.SetDefaultRules()
            End If

            mValidateFastaFile.MaximumFileErrorsToTrack = TextBoxUtils.ParseTextBoxValueInt(txtMaxFileErrorsToTrack, "Max file errors or warnings should be a positive number", False, 10, False)

            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.CheckForDuplicateProteinNames, chkCheckForDuplicateProteinInfo.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.CheckForDuplicateProteinSequences, chkCheckForDuplicateProteinInfo.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.SaveBasicProteinHashInfoFile, chkSaveBasicProteinHashInfoFile.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.OutputToStatsFile, chkLogResults.Checked)

            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.GenerateFixedFASTAFile, chkGenerateFixedFastaFile.Checked)

            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaSplitOutMultipleRefsForKnownAccession, chkSplitOutMultipleRefsForKnownAccession.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.SplitOutMultipleRefsInProteinName, chkSplitOutMultipleRefsInProteinName.Checked)

            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaRenameDuplicateNameProteins, chkRenameDuplicateProteins.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaKeepDuplicateNamedProteins, chkKeepDuplicateNamedProteins.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaConsolidateDuplicateProteinSeqs, chkConsolidateDuplicateProteinSeqs.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaConsolidateDupsIgnoreILDiff, chkConsolidateDupsIgnoreILDiff.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaTruncateLongProteinNames, chkTruncateLongProteinNames.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaWrapLongResidueLines, chkWrapLongResidueLines.Checked)
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.FixedFastaRemoveInvalidResidues, chkRemoveInvalidResidues.Checked())

            mValidateFastaFile.ProteinNameFirstRefSepChars = clsValidateFastaFile.DEFAULT_PROTEIN_NAME_FIRST_REF_SEP_CHARS
            mValidateFastaFile.ProteinNameSubsequentRefSepChars = clsValidateFastaFile.DEFAULT_PROTEIN_NAME_SUBSEQUENT_REF_SEP_CHARS

            ' Also apply chkGenerateFixedFastaFile to SaveProteinSequenceHashInfoFiles
            mValidateFastaFile.SetOptionSwitch(clsValidateFastaFile.SwitchOptions.SaveProteinSequenceHashInfoFiles, chkGenerateFixedFastaFile.Checked)

            If txtLongProteinNameSplitChars.TextLength > 0 Then
                mValidateFastaFile.LongProteinNameSplitChars = txtLongProteinNameSplitChars.Text
            Else
                mValidateFastaFile.LongProteinNameSplitChars = clsValidateFastaFile.DEFAULT_LONG_PROTEIN_NAME_SPLIT_CHAR
            End If

            mValidateFastaFile.ProteinNameInvalidCharsToRemove = txtInvalidProteinNameCharsToRemove.Text

            pbarProgress.Minimum = 0
            pbarProgress.Maximum = 100
            pbarProgress.Value = 0


            ' Analyze the fasta file; returns true if the analysis was successful (even if the file contains errors or warnings)
            success = mValidateFastaFile.ProcessFile(FastaFilePath, String.Empty, String.Empty)

            cmdCancel.Enabled = False

            If success Then
                DisplayResults(parameterFilePath)
            Else
                txtResults.Text = "Error calling mValidateFastaFile.ProcessFile: " & mValidateFastaFile.GetErrorMessage()
                If Not String.IsNullOrEmpty(mValidatorErrorMessage) Then
                    txtResults.AppendText(ControlChars.NewLine & mValidatorErrorMessage)
                End If
            End If

        Catch ex As Exception
            MessageBox.Show("Error validating fasta file: " & FastaFilePath & ControlChars.NewLine & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        Finally
            mValidateFastaFile = Nothing
            ShowHideObjectsDuringValidation(False)

            Cursor.Current = Cursors.Default
            Application.DoEvents()
        End Try

    End Sub

    Private Sub UpdateDataGridTableStyle(dgDataGrid As DataGrid, targetTableName As String)

        ' Instantiate the TableStyle
        ' Setting the MappingName of the table style to targetTableName will cause this style to be used with that table
        Dim tsTableStyle = New DataGridTableStyle With {
            .MappingName = targetTableName,
            .AllowSorting = True,
            .ColumnHeadersVisible = True,
            .RowHeadersVisible = False,
            .ReadOnly = True
        }

        DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_LINE, "Line", 80)
        DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_COLUMN, "Column", 80)
        DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_PROTEIN, "Protein", 200)
        DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_DESCRIPTION, "Value", 550)
        DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_CONTEXT, "Context", 200)

        dgDataGrid.TableStyles.Clear()

        If Not dgDataGrid.TableStyles.Contains(tsTableStyle) Then
            dgDataGrid.TableStyles.Add(tsTableStyle)
        End If

        dgDataGrid.Refresh()

    End Sub

#End Region

#Region "Control Handlers"

    Private Sub chkGenerateFixedFastaFile_CheckedChanged(sender As Object, e As EventArgs) Handles chkGenerateFixedFastaFile.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkConsolidateDuplicateProteinSeqs_CheckedChanged(sender As Object, e As EventArgs) Handles chkConsolidateDuplicateProteinSeqs.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkRenameDuplicateProteins_CheckedChanged(sender As Object, e As EventArgs) Handles chkRenameDuplicateProteins.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkKeepDuplicateNamedProteins_CheckedChanged(sender As Object, e As EventArgs) Handles chkKeepDuplicateNamedProteins.CheckedChanged
        If chkKeepDuplicateNamedProteins.Enabled Then
            mKeepDuplicateNamedProteinsLastValue = chkKeepDuplicateNamedProteins.Checked
        End If
    End Sub

    Private Sub chkWrapLongResidueLines_CheckedChanged(sender As Object, e As EventArgs) Handles chkWrapLongResidueLines.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub cmdCancel_Click(sender As Object, e As EventArgs) Handles cmdCancel.Click
        If Not mValidateFastaFile Is Nothing Then
            cmdCancel.Enabled = False
            mValidateFastaFile.AbortProcessingNow()
        End If
    End Sub

    Private Sub cmdCreateDefaultValidationRulesFile_Click_1(sender As Object, e As EventArgs) Handles cmdCreateDefaultValidationRulesFile.Click
        CreateDefaultValidationRulesFile()
    End Sub

    Private Sub cmdSelectCustomRulesFile_Click(sender As Object, e As EventArgs) Handles cmdSelectCustomRulesFile.Click
        SelectCustomRulesFile()
    End Sub

    Private Sub cmdValidateFastaFile_Click(sender As Object, e As EventArgs) Handles cmdValidateFastaFile.Click
        StartValidation()
    End Sub

    Private Sub frmFastaValidationResults_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        PositionControls()
    End Sub

    Private Sub txtCustomValidationRulesFilePath_TextChanged(sender As Object, e As EventArgs) Handles txtCustomValidationRulesFilePath.TextChanged
        EnableDisableControls()
    End Sub

    Private Sub txtFilterData_Validated(sender As Object, e As EventArgs) Handles txtFilterData.Validated
        FilterLists()
    End Sub

    Private Sub txtFilterData_KeyDown(sender As Object, e As KeyEventArgs) Handles txtFilterData.KeyDown
        If e.KeyCode = Keys.Enter Then
            FilterLists()
        End If
    End Sub

    Private Sub txtMaxFileErrorsToTrack_KeyPress1(sender As Object, e As KeyPressEventArgs) Handles txtMaxFileErrorsToTrack.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtMaxFileErrorsToTrack, e, True)
    End Sub

    Private Sub txtMaximumResiduesPerLine_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtMaximumResiduesPerLine.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtMaximumResiduesPerLine, e, True)
    End Sub

    Private Sub txtProteinNameLengthMinimum_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtProteinNameLengthMinimum.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtProteinNameLengthMinimum, e, True)
    End Sub

    Private Sub txtProteinNameLengthMaximum_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtProteinNameLengthMaximum.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtProteinNameLengthMaximum, e, True)
    End Sub

    Private Sub txtResiduesPerLineForWrap_TextChanged(sender As Object, e As KeyPressEventArgs) Handles txtResiduesPerLineForWrap.KeyPress
        TextBoxUtils.TextBoxKeyPressHandler(txtResiduesPerLineForWrap, e, True)
    End Sub

    Private Sub txtResults_KeyDown(sender As Object, e As KeyEventArgs) Handles txtResults.KeyDown
        If e.Control = True Then
            If e.KeyCode = Keys.A Then
                txtResults.SelectAll()
            End If
        End If
    End Sub

    ''' <summary>
    ''' This timer is used to cause StartValidation to be called after the form becomes visible
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub mValidationTriggerTimer_Tick(sender As Object, e As EventArgs) Handles mValidationTriggerTimer.Tick
        mValidationTriggerTimer.Enabled = False

        Try
            ' Check whether the fasta file is over 250 MB in size
            ' If it is, auto-disable the check for duplicate proteins (to avoid using too much memory)
            Dim fastaFile = New FileInfo(FastaFilePath)
            If fastaFile.Exists Then
                If fastaFile.Length > 250 * 1024 * 1024 Then
                    chkCheckForDuplicateProteinInfo.Checked = False
                End If
                StartValidation()
            End If

        Catch ex As Exception
            MessageBox.Show("Error examining the fasta file size: " & ex.Message)
        End Try

    End Sub

#End Region

#Region "Menu Handlers"

    Private Sub mnuFileExit_Click(sender As Object, e As EventArgs) Handles mnuFileExit.Click
        Me.Close()
    End Sub

    Private Sub mnuEditCopySummary_Click(sender As Object, e As EventArgs) Handles mnuEditCopySummary.Click
        CopySummaryText()
    End Sub

    Private Sub mnuEditCopyAllResults_Click(sender As Object, e As EventArgs) Handles mnuEditCopyAllResults.Click
        CopyAllResults()
    End Sub

    Private Sub mnuEditCopyAllErrors_Click(sender As Object, e As EventArgs) Handles mnuEditCopyAllErrors.Click
        CopyErrorsDataView()
    End Sub

    Private Sub mnuEditCopyAllWarnings_Click(sender As Object, e As EventArgs) Handles mnuEditCopyAllWarnings.Click
        CopyWarningsDataView()
    End Sub

    Private Sub mnuEditFontSizeDecrease_Click(sender As Object, e As EventArgs) Handles mnuEditFontSizeDecrease.Click
        If Me.TextFontSize > 14 Then
            Me.TextFontSize = Me.TextFontSize - 2
        Else
            Me.TextFontSize = Me.TextFontSize - 1
        End If
    End Sub

    Private Sub mnuEditFontSizeIncrease_Click(sender As Object, e As EventArgs) Handles mnuEditFontSizeIncrease.Click
        If Me.TextFontSize >= 14 Then
            Me.TextFontSize = Me.TextFontSize + 2
        Else
            Me.TextFontSize = Me.TextFontSize + 1
        End If
    End Sub

    Private Sub mnuEditResetToDefaults_Click(sender As Object, e As EventArgs) Handles mnuEditResetToDefaults.Click
        ResetOptionsToDefault()
    End Sub

    Private Sub mnuHelpAbout_Click(sender As Object, e As EventArgs) Handles mnuHelpAbout.Click
        ShowAboutBox()
    End Sub

#End Region

#Region "Event Handlers"

    Private Sub mValidateFastaFile_ErrorEvent(message As String, ex As Exception) Handles mValidateFastaFile.ErrorEvent
        mValidatorErrorMessage = message
    End Sub

    Private Sub mValidateFastaFile_ProgressChanged(taskDescription As String, percentComplete As Single) Handles mValidateFastaFile.ProgressUpdate
        Try
            pbarProgress.Value = CType(percentComplete, Integer)
            Application.DoEvents()
        Catch ex As Exception
            ' Ignore errors here
        End Try
    End Sub
#End Region

End Class