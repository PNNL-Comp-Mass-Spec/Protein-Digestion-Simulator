Option Strict On

' This form uses class clsValidateFastaFile to examine a fasta file for problems & warnings
' Results are displayed in a textbox and in two data grids
' 
' When instantiating the form, you must provide the path to the fasta file to examine
'
' Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in April 2005
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.

Public Class frmFastaValidation
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New(ByVal strFastaFilePath As String)
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        InitializeForm(strFastaFilePath)
    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents MainMenuControl As System.Windows.Forms.MainMenu
    Friend WithEvents mnuFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileExit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEdit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditFontSizeDecrease As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditFontSizeIncrease As System.Windows.Forms.MenuItem
    Friend WithEvents dgErrors As System.Windows.Forms.DataGrid
    Friend WithEvents dgWarnings As System.Windows.Forms.DataGrid
    Friend WithEvents cmdValidateFastaFile As System.Windows.Forms.Button
    Friend WithEvents pbarProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents txtFilterData As System.Windows.Forms.TextBox
    Friend WithEvents lblFilterData As System.Windows.Forms.Label
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents mnuEditSep2 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditCopyAllErrors As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditCopyAllWarnings As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditSep1 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditCopySummary As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditCopyAllResults As System.Windows.Forms.MenuItem
    Friend WithEvents tbsOptions As System.Windows.Forms.TabControl
    Friend WithEvents TabPageNewFastaOptions As System.Windows.Forms.TabPage
    Friend WithEvents TabPageErrorOptions As System.Windows.Forms.TabPage
    Friend WithEvents txtInvalidProteinNameCharsToRemove As System.Windows.Forms.TextBox
    Friend WithEvents lblInvalidProteinNameCharsToRemove As System.Windows.Forms.Label
    Friend WithEvents txtLongProteinNameSplitChars As System.Windows.Forms.TextBox
    Friend WithEvents lblLongProteinNameSplitChars As System.Windows.Forms.Label
    Friend WithEvents chkGenerateFixedFastaFile As System.Windows.Forms.CheckBox
    Friend WithEvents txtMaxFileErrorsToTrack As System.Windows.Forms.TextBox
    Friend WithEvents lblMaxFileErrorsToTrack As System.Windows.Forms.Label
    Friend WithEvents chkLogResults As System.Windows.Forms.CheckBox
    Friend WithEvents chkSplitOutMultipleRefsInProteinName As System.Windows.Forms.CheckBox
    Friend WithEvents TabPageRuleOptions As System.Windows.Forms.TabPage
    Friend WithEvents cmdSelectCustomRulesFile As System.Windows.Forms.Button
    Friend WithEvents txtCustomValidationRulesFilePath As System.Windows.Forms.TextBox
    Friend WithEvents chkAllowAsteriskInResidues As System.Windows.Forms.CheckBox
    Friend WithEvents cmdCreateDefaultValidationRulesFile As System.Windows.Forms.Button
    Friend WithEvents MenuItem1 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelpAbout As System.Windows.Forms.MenuItem
    Friend WithEvents txtResults As System.Windows.Forms.TextBox
    Friend WithEvents chkCheckForDuplicateProteinInfo As System.Windows.Forms.CheckBox
    Friend WithEvents chkRenameDuplicateProteins As System.Windows.Forms.CheckBox
    Friend WithEvents chkConsolidateDuplicateProteinSeqs As System.Windows.Forms.CheckBox
    Friend WithEvents chkConsolidateDupsIgnoreILDiff As System.Windows.Forms.CheckBox
    Friend WithEvents chkTruncateLongProteinNames As System.Windows.Forms.CheckBox
    Friend WithEvents chkSplitOutMultipleRefsForKnownAccession As System.Windows.Forms.CheckBox
    Friend WithEvents chkWrapLongResidueLines As System.Windows.Forms.CheckBox
    Friend WithEvents chkRemoveInvalidResidues As System.Windows.Forms.CheckBox
    Friend WithEvents lblCustomRulesFile As System.Windows.Forms.Label
    Friend WithEvents txtMaximumResiduesPerLine As System.Windows.Forms.TextBox
    Friend WithEvents lblMaximumResiduesPerLine As System.Windows.Forms.Label
    Friend WithEvents lblProteinNameLengthUnits As System.Windows.Forms.Label
    Friend WithEvents lblProteinNameLength2 As System.Windows.Forms.Label
    Friend WithEvents txtProteinNameLengthMaximum As System.Windows.Forms.TextBox
    Friend WithEvents txtProteinNameLengthMinimum As System.Windows.Forms.TextBox
    Friend WithEvents lblProteinNameLength As System.Windows.Forms.Label
    Friend WithEvents MenuItem2 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditResetToDefaults As System.Windows.Forms.MenuItem
    Friend WithEvents chkSaveBasicProteinHashInfoFile As System.Windows.Forms.CheckBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.txtResults = New System.Windows.Forms.TextBox
        Me.MainMenuControl = New System.Windows.Forms.MainMenu
        Me.mnuFile = New System.Windows.Forms.MenuItem
        Me.mnuFileExit = New System.Windows.Forms.MenuItem
        Me.mnuEdit = New System.Windows.Forms.MenuItem
        Me.mnuEditCopyAllResults = New System.Windows.Forms.MenuItem
        Me.mnuEditSep1 = New System.Windows.Forms.MenuItem
        Me.mnuEditCopySummary = New System.Windows.Forms.MenuItem
        Me.mnuEditCopyAllErrors = New System.Windows.Forms.MenuItem
        Me.mnuEditCopyAllWarnings = New System.Windows.Forms.MenuItem
        Me.mnuEditSep2 = New System.Windows.Forms.MenuItem
        Me.mnuEditFontSizeDecrease = New System.Windows.Forms.MenuItem
        Me.mnuEditFontSizeIncrease = New System.Windows.Forms.MenuItem
        Me.MenuItem2 = New System.Windows.Forms.MenuItem
        Me.mnuEditResetToDefaults = New System.Windows.Forms.MenuItem
        Me.MenuItem1 = New System.Windows.Forms.MenuItem
        Me.mnuHelpAbout = New System.Windows.Forms.MenuItem
        Me.dgErrors = New System.Windows.Forms.DataGrid
        Me.dgWarnings = New System.Windows.Forms.DataGrid
        Me.cmdValidateFastaFile = New System.Windows.Forms.Button
        Me.pbarProgress = New System.Windows.Forms.ProgressBar
        Me.txtFilterData = New System.Windows.Forms.TextBox
        Me.lblFilterData = New System.Windows.Forms.Label
        Me.cmdCancel = New System.Windows.Forms.Button
        Me.tbsOptions = New System.Windows.Forms.TabControl
        Me.TabPageErrorOptions = New System.Windows.Forms.TabPage
        Me.lblProteinNameLengthUnits = New System.Windows.Forms.Label
        Me.lblProteinNameLength2 = New System.Windows.Forms.Label
        Me.txtProteinNameLengthMaximum = New System.Windows.Forms.TextBox
        Me.txtProteinNameLengthMinimum = New System.Windows.Forms.TextBox
        Me.lblProteinNameLength = New System.Windows.Forms.Label
        Me.txtMaximumResiduesPerLine = New System.Windows.Forms.TextBox
        Me.lblMaximumResiduesPerLine = New System.Windows.Forms.Label
        Me.chkAllowAsteriskInResidues = New System.Windows.Forms.CheckBox
        Me.chkCheckForDuplicateProteinInfo = New System.Windows.Forms.CheckBox
        Me.txtMaxFileErrorsToTrack = New System.Windows.Forms.TextBox
        Me.lblMaxFileErrorsToTrack = New System.Windows.Forms.Label
        Me.chkLogResults = New System.Windows.Forms.CheckBox
        Me.TabPageNewFastaOptions = New System.Windows.Forms.TabPage
        Me.chkRemoveInvalidResidues = New System.Windows.Forms.CheckBox
        Me.chkWrapLongResidueLines = New System.Windows.Forms.CheckBox
        Me.chkSplitOutMultipleRefsForKnownAccession = New System.Windows.Forms.CheckBox
        Me.chkTruncateLongProteinNames = New System.Windows.Forms.CheckBox
        Me.chkConsolidateDupsIgnoreILDiff = New System.Windows.Forms.CheckBox
        Me.chkConsolidateDuplicateProteinSeqs = New System.Windows.Forms.CheckBox
        Me.chkRenameDuplicateProteins = New System.Windows.Forms.CheckBox
        Me.chkSplitOutMultipleRefsInProteinName = New System.Windows.Forms.CheckBox
        Me.txtInvalidProteinNameCharsToRemove = New System.Windows.Forms.TextBox
        Me.lblInvalidProteinNameCharsToRemove = New System.Windows.Forms.Label
        Me.txtLongProteinNameSplitChars = New System.Windows.Forms.TextBox
        Me.lblLongProteinNameSplitChars = New System.Windows.Forms.Label
        Me.chkGenerateFixedFastaFile = New System.Windows.Forms.CheckBox
        Me.TabPageRuleOptions = New System.Windows.Forms.TabPage
        Me.lblCustomRulesFile = New System.Windows.Forms.Label
        Me.cmdCreateDefaultValidationRulesFile = New System.Windows.Forms.Button
        Me.txtCustomValidationRulesFilePath = New System.Windows.Forms.TextBox
        Me.cmdSelectCustomRulesFile = New System.Windows.Forms.Button
        Me.chkSaveBasicProteinHashInfoFile = New System.Windows.Forms.CheckBox
        CType(Me.dgErrors, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgWarnings, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tbsOptions.SuspendLayout()
        Me.TabPageErrorOptions.SuspendLayout()
        Me.TabPageNewFastaOptions.SuspendLayout()
        Me.TabPageRuleOptions.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtResults
        '
        Me.txtResults.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtResults.Location = New System.Drawing.Point(440, 8)
        Me.txtResults.Multiline = True
        Me.txtResults.Name = "txtResults"
        Me.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtResults.Size = New System.Drawing.Size(520, 152)
        Me.txtResults.TabIndex = 1
        Me.txtResults.Text = ""
        Me.txtResults.WordWrap = False
        '
        'MainMenuControl
        '
        Me.MainMenuControl.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFile, Me.mnuEdit, Me.MenuItem1})
        '
        'mnuFile
        '
        Me.mnuFile.Index = 0
        Me.mnuFile.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFileExit})
        Me.mnuFile.Text = "&File"
        '
        'mnuFileExit
        '
        Me.mnuFileExit.Index = 0
        Me.mnuFileExit.Text = "E&xit"
        '
        'mnuEdit
        '
        Me.mnuEdit.Index = 1
        Me.mnuEdit.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuEditCopyAllResults, Me.mnuEditSep1, Me.mnuEditCopySummary, Me.mnuEditCopyAllErrors, Me.mnuEditCopyAllWarnings, Me.mnuEditSep2, Me.mnuEditFontSizeDecrease, Me.mnuEditFontSizeIncrease, Me.MenuItem2, Me.mnuEditResetToDefaults})
        Me.mnuEdit.Text = "&Edit"
        '
        'mnuEditCopyAllResults
        '
        Me.mnuEditCopyAllResults.Index = 0
        Me.mnuEditCopyAllResults.Text = "Copy &All Results"
        '
        'mnuEditSep1
        '
        Me.mnuEditSep1.Index = 1
        Me.mnuEditSep1.Text = "-"
        '
        'mnuEditCopySummary
        '
        Me.mnuEditCopySummary.Index = 2
        Me.mnuEditCopySummary.Text = "Copy &Summary"
        '
        'mnuEditCopyAllErrors
        '
        Me.mnuEditCopyAllErrors.Index = 3
        Me.mnuEditCopyAllErrors.Text = "Copy &Errors"
        '
        'mnuEditCopyAllWarnings
        '
        Me.mnuEditCopyAllWarnings.Index = 4
        Me.mnuEditCopyAllWarnings.Text = "Copy &Warnings"
        '
        'mnuEditSep2
        '
        Me.mnuEditSep2.Index = 5
        Me.mnuEditSep2.Text = "-"
        '
        'mnuEditFontSizeDecrease
        '
        Me.mnuEditFontSizeDecrease.Index = 6
        Me.mnuEditFontSizeDecrease.Shortcut = System.Windows.Forms.Shortcut.F3
        Me.mnuEditFontSizeDecrease.Text = "&Decrease Font Size"
        '
        'mnuEditFontSizeIncrease
        '
        Me.mnuEditFontSizeIncrease.Index = 7
        Me.mnuEditFontSizeIncrease.Shortcut = System.Windows.Forms.Shortcut.F4
        Me.mnuEditFontSizeIncrease.Text = "&Increase Font Size"
        '
        'MenuItem2
        '
        Me.MenuItem2.Index = 8
        Me.MenuItem2.Text = "-"
        '
        'mnuEditResetToDefaults
        '
        Me.mnuEditResetToDefaults.Index = 9
        Me.mnuEditResetToDefaults.Text = "&Reset options to Default"
        '
        'MenuItem1
        '
        Me.MenuItem1.Index = 2
        Me.MenuItem1.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuHelpAbout})
        Me.MenuItem1.Text = "&Help"
        '
        'mnuHelpAbout
        '
        Me.mnuHelpAbout.Index = 0
        Me.mnuHelpAbout.Text = "&About"
        '
        'dgErrors
        '
        Me.dgErrors.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgErrors.DataMember = ""
        Me.dgErrors.HeaderForeColor = System.Drawing.SystemColors.ControlText
        Me.dgErrors.Location = New System.Drawing.Point(8, 208)
        Me.dgErrors.Name = "dgErrors"
        Me.dgErrors.Size = New System.Drawing.Size(952, 120)
        Me.dgErrors.TabIndex = 7
        '
        'dgWarnings
        '
        Me.dgWarnings.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgWarnings.DataMember = ""
        Me.dgWarnings.HeaderForeColor = System.Drawing.SystemColors.ControlText
        Me.dgWarnings.Location = New System.Drawing.Point(4, 336)
        Me.dgWarnings.Name = "dgWarnings"
        Me.dgWarnings.Size = New System.Drawing.Size(961, 104)
        Me.dgWarnings.TabIndex = 8
        '
        'cmdValidateFastaFile
        '
        Me.cmdValidateFastaFile.Location = New System.Drawing.Point(440, 174)
        Me.cmdValidateFastaFile.Name = "cmdValidateFastaFile"
        Me.cmdValidateFastaFile.Size = New System.Drawing.Size(104, 24)
        Me.cmdValidateFastaFile.TabIndex = 2
        Me.cmdValidateFastaFile.Text = "&Start Validation"
        '
        'pbarProgress
        '
        Me.pbarProgress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pbarProgress.Location = New System.Drawing.Point(624, 176)
        Me.pbarProgress.Name = "pbarProgress"
        Me.pbarProgress.Size = New System.Drawing.Size(336, 20)
        Me.pbarProgress.TabIndex = 8
        Me.pbarProgress.Visible = False
        '
        'txtFilterData
        '
        Me.txtFilterData.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtFilterData.Location = New System.Drawing.Point(624, 176)
        Me.txtFilterData.Name = "txtFilterData"
        Me.txtFilterData.Size = New System.Drawing.Size(336, 20)
        Me.txtFilterData.TabIndex = 6
        Me.txtFilterData.Text = ""
        '
        'lblFilterData
        '
        Me.lblFilterData.Location = New System.Drawing.Point(552, 178)
        Me.lblFilterData.Name = "lblFilterData"
        Me.lblFilterData.Size = New System.Drawing.Size(64, 15)
        Me.lblFilterData.TabIndex = 5
        Me.lblFilterData.Text = "Filter data"
        '
        'cmdCancel
        '
        Me.cmdCancel.Location = New System.Drawing.Point(440, 174)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(104, 24)
        Me.cmdCancel.TabIndex = 3
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.Visible = False
        '
        'tbsOptions
        '
        Me.tbsOptions.Controls.Add(Me.TabPageErrorOptions)
        Me.tbsOptions.Controls.Add(Me.TabPageNewFastaOptions)
        Me.tbsOptions.Controls.Add(Me.TabPageRuleOptions)
        Me.tbsOptions.Location = New System.Drawing.Point(7, 7)
        Me.tbsOptions.Name = "tbsOptions"
        Me.tbsOptions.SelectedIndex = 0
        Me.tbsOptions.Size = New System.Drawing.Size(409, 192)
        Me.tbsOptions.TabIndex = 0
        '
        'TabPageErrorOptions
        '
        Me.TabPageErrorOptions.Controls.Add(Me.chkSaveBasicProteinHashInfoFile)
        Me.TabPageErrorOptions.Controls.Add(Me.lblProteinNameLengthUnits)
        Me.TabPageErrorOptions.Controls.Add(Me.lblProteinNameLength2)
        Me.TabPageErrorOptions.Controls.Add(Me.txtProteinNameLengthMaximum)
        Me.TabPageErrorOptions.Controls.Add(Me.txtProteinNameLengthMinimum)
        Me.TabPageErrorOptions.Controls.Add(Me.lblProteinNameLength)
        Me.TabPageErrorOptions.Controls.Add(Me.txtMaximumResiduesPerLine)
        Me.TabPageErrorOptions.Controls.Add(Me.lblMaximumResiduesPerLine)
        Me.TabPageErrorOptions.Controls.Add(Me.chkAllowAsteriskInResidues)
        Me.TabPageErrorOptions.Controls.Add(Me.chkCheckForDuplicateProteinInfo)
        Me.TabPageErrorOptions.Controls.Add(Me.txtMaxFileErrorsToTrack)
        Me.TabPageErrorOptions.Controls.Add(Me.lblMaxFileErrorsToTrack)
        Me.TabPageErrorOptions.Controls.Add(Me.chkLogResults)
        Me.TabPageErrorOptions.Location = New System.Drawing.Point(4, 22)
        Me.TabPageErrorOptions.Name = "TabPageErrorOptions"
        Me.TabPageErrorOptions.Size = New System.Drawing.Size(401, 166)
        Me.TabPageErrorOptions.TabIndex = 0
        Me.TabPageErrorOptions.Text = "Error Options"
        '
        'lblProteinNameLengthUnits
        '
        Me.lblProteinNameLengthUnits.Location = New System.Drawing.Point(312, 59)
        Me.lblProteinNameLengthUnits.Name = "lblProteinNameLengthUnits"
        Me.lblProteinNameLengthUnits.Size = New System.Drawing.Size(80, 17)
        Me.lblProteinNameLengthUnits.TabIndex = 8
        Me.lblProteinNameLengthUnits.Text = "characters"
        '
        'lblProteinNameLength2
        '
        Me.lblProteinNameLength2.Location = New System.Drawing.Point(240, 59)
        Me.lblProteinNameLength2.Name = "lblProteinNameLength2"
        Me.lblProteinNameLength2.Size = New System.Drawing.Size(16, 17)
        Me.lblProteinNameLength2.TabIndex = 6
        Me.lblProteinNameLength2.Text = "to"
        '
        'txtProteinNameLengthMaximum
        '
        Me.txtProteinNameLengthMaximum.Location = New System.Drawing.Point(264, 56)
        Me.txtProteinNameLengthMaximum.Name = "txtProteinNameLengthMaximum"
        Me.txtProteinNameLengthMaximum.Size = New System.Drawing.Size(40, 20)
        Me.txtProteinNameLengthMaximum.TabIndex = 7
        Me.txtProteinNameLengthMaximum.Text = "34"
        '
        'txtProteinNameLengthMinimum
        '
        Me.txtProteinNameLengthMinimum.Location = New System.Drawing.Point(192, 56)
        Me.txtProteinNameLengthMinimum.Name = "txtProteinNameLengthMinimum"
        Me.txtProteinNameLengthMinimum.Size = New System.Drawing.Size(40, 20)
        Me.txtProteinNameLengthMinimum.TabIndex = 5
        Me.txtProteinNameLengthMinimum.Text = "3"
        '
        'lblProteinNameLength
        '
        Me.lblProteinNameLength.Location = New System.Drawing.Point(8, 58)
        Me.lblProteinNameLength.Name = "lblProteinNameLength"
        Me.lblProteinNameLength.Size = New System.Drawing.Size(176, 17)
        Me.lblProteinNameLength.TabIndex = 4
        Me.lblProteinNameLength.Text = "Valid protein name length range"
        '
        'txtMaximumResiduesPerLine
        '
        Me.txtMaximumResiduesPerLine.Location = New System.Drawing.Point(192, 32)
        Me.txtMaximumResiduesPerLine.Name = "txtMaximumResiduesPerLine"
        Me.txtMaximumResiduesPerLine.Size = New System.Drawing.Size(40, 20)
        Me.txtMaximumResiduesPerLine.TabIndex = 3
        Me.txtMaximumResiduesPerLine.Text = "120"
        '
        'lblMaximumResiduesPerLine
        '
        Me.lblMaximumResiduesPerLine.Location = New System.Drawing.Point(8, 34)
        Me.lblMaximumResiduesPerLine.Name = "lblMaximumResiduesPerLine"
        Me.lblMaximumResiduesPerLine.Size = New System.Drawing.Size(168, 17)
        Me.lblMaximumResiduesPerLine.TabIndex = 2
        Me.lblMaximumResiduesPerLine.Text = "Maximum residues per line"
        '
        'chkAllowAsteriskInResidues
        '
        Me.chkAllowAsteriskInResidues.Location = New System.Drawing.Point(8, 80)
        Me.chkAllowAsteriskInResidues.Name = "chkAllowAsteriskInResidues"
        Me.chkAllowAsteriskInResidues.Size = New System.Drawing.Size(168, 16)
        Me.chkAllowAsteriskInResidues.TabIndex = 9
        Me.chkAllowAsteriskInResidues.Text = "Allow asterisks in residues"
        '
        'chkCheckForDuplicateProteinInfo
        '
        Me.chkCheckForDuplicateProteinInfo.Checked = True
        Me.chkCheckForDuplicateProteinInfo.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCheckForDuplicateProteinInfo.Location = New System.Drawing.Point(8, 102)
        Me.chkCheckForDuplicateProteinInfo.Name = "chkCheckForDuplicateProteinInfo"
        Me.chkCheckForDuplicateProteinInfo.Size = New System.Drawing.Size(200, 30)
        Me.chkCheckForDuplicateProteinInfo.TabIndex = 10
        Me.chkCheckForDuplicateProteinInfo.Text = "Check for duplicate protein names and duplicate protein sequences"
        '
        'txtMaxFileErrorsToTrack
        '
        Me.txtMaxFileErrorsToTrack.Location = New System.Drawing.Point(296, 8)
        Me.txtMaxFileErrorsToTrack.Name = "txtMaxFileErrorsToTrack"
        Me.txtMaxFileErrorsToTrack.Size = New System.Drawing.Size(40, 20)
        Me.txtMaxFileErrorsToTrack.TabIndex = 1
        Me.txtMaxFileErrorsToTrack.Text = "10"
        '
        'lblMaxFileErrorsToTrack
        '
        Me.lblMaxFileErrorsToTrack.Location = New System.Drawing.Point(7, 10)
        Me.lblMaxFileErrorsToTrack.Name = "lblMaxFileErrorsToTrack"
        Me.lblMaxFileErrorsToTrack.Size = New System.Drawing.Size(273, 17)
        Me.lblMaxFileErrorsToTrack.TabIndex = 0
        Me.lblMaxFileErrorsToTrack.Text = "Maximum # of errors or warnings to track in detail"
        '
        'chkLogResults
        '
        Me.chkLogResults.Location = New System.Drawing.Point(8, 136)
        Me.chkLogResults.Name = "chkLogResults"
        Me.chkLogResults.Size = New System.Drawing.Size(127, 16)
        Me.chkLogResults.TabIndex = 11
        Me.chkLogResults.Text = "Log results to file"
        '
        'TabPageNewFastaOptions
        '
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkRemoveInvalidResidues)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkWrapLongResidueLines)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkSplitOutMultipleRefsForKnownAccession)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkTruncateLongProteinNames)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkConsolidateDupsIgnoreILDiff)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkConsolidateDuplicateProteinSeqs)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkRenameDuplicateProteins)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkSplitOutMultipleRefsInProteinName)
        Me.TabPageNewFastaOptions.Controls.Add(Me.txtInvalidProteinNameCharsToRemove)
        Me.TabPageNewFastaOptions.Controls.Add(Me.lblInvalidProteinNameCharsToRemove)
        Me.TabPageNewFastaOptions.Controls.Add(Me.txtLongProteinNameSplitChars)
        Me.TabPageNewFastaOptions.Controls.Add(Me.lblLongProteinNameSplitChars)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkGenerateFixedFastaFile)
        Me.TabPageNewFastaOptions.Location = New System.Drawing.Point(4, 22)
        Me.TabPageNewFastaOptions.Name = "TabPageNewFastaOptions"
        Me.TabPageNewFastaOptions.Size = New System.Drawing.Size(401, 166)
        Me.TabPageNewFastaOptions.TabIndex = 1
        Me.TabPageNewFastaOptions.Text = "Fixed Fasta Options"
        '
        'chkRemoveInvalidResidues
        '
        Me.chkRemoveInvalidResidues.Location = New System.Drawing.Point(8, 144)
        Me.chkRemoveInvalidResidues.Name = "chkRemoveInvalidResidues"
        Me.chkRemoveInvalidResidues.Size = New System.Drawing.Size(152, 16)
        Me.chkRemoveInvalidResidues.TabIndex = 8
        Me.chkRemoveInvalidResidues.Text = "Remove invalid residues"
        '
        'chkWrapLongResidueLines
        '
        Me.chkWrapLongResidueLines.Checked = True
        Me.chkWrapLongResidueLines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWrapLongResidueLines.Location = New System.Drawing.Point(8, 120)
        Me.chkWrapLongResidueLines.Name = "chkWrapLongResidueLines"
        Me.chkWrapLongResidueLines.Size = New System.Drawing.Size(152, 16)
        Me.chkWrapLongResidueLines.TabIndex = 7
        Me.chkWrapLongResidueLines.Text = "Wrap long residue lines"
        '
        'chkSplitOutMultipleRefsForKnownAccession
        '
        Me.chkSplitOutMultipleRefsForKnownAccession.Checked = True
        Me.chkSplitOutMultipleRefsForKnownAccession.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSplitOutMultipleRefsForKnownAccession.Location = New System.Drawing.Point(184, 64)
        Me.chkSplitOutMultipleRefsForKnownAccession.Name = "chkSplitOutMultipleRefsForKnownAccession"
        Me.chkSplitOutMultipleRefsForKnownAccession.Size = New System.Drawing.Size(192, 32)
        Me.chkSplitOutMultipleRefsForKnownAccession.TabIndex = 9
        Me.chkSplitOutMultipleRefsForKnownAccession.Text = "Split out multiple references only if IPI, GI, or JGI"
        '
        'chkTruncateLongProteinNames
        '
        Me.chkTruncateLongProteinNames.Checked = True
        Me.chkTruncateLongProteinNames.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkTruncateLongProteinNames.Location = New System.Drawing.Point(8, 64)
        Me.chkTruncateLongProteinNames.Name = "chkTruncateLongProteinNames"
        Me.chkTruncateLongProteinNames.Size = New System.Drawing.Size(176, 24)
        Me.chkTruncateLongProteinNames.TabIndex = 5
        Me.chkTruncateLongProteinNames.Text = "Truncate long protein names"
        '
        'chkConsolidateDupsIgnoreILDiff
        '
        Me.chkConsolidateDupsIgnoreILDiff.Checked = True
        Me.chkConsolidateDupsIgnoreILDiff.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkConsolidateDupsIgnoreILDiff.Location = New System.Drawing.Point(184, 144)
        Me.chkConsolidateDupsIgnoreILDiff.Name = "chkConsolidateDupsIgnoreILDiff"
        Me.chkConsolidateDupsIgnoreILDiff.Size = New System.Drawing.Size(208, 16)
        Me.chkConsolidateDupsIgnoreILDiff.TabIndex = 12
        Me.chkConsolidateDupsIgnoreILDiff.Text = "Ignore I/L diffs when consolidating"
        '
        'chkConsolidateDuplicateProteinSeqs
        '
        Me.chkConsolidateDuplicateProteinSeqs.Checked = True
        Me.chkConsolidateDuplicateProteinSeqs.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkConsolidateDuplicateProteinSeqs.Location = New System.Drawing.Point(184, 128)
        Me.chkConsolidateDuplicateProteinSeqs.Name = "chkConsolidateDuplicateProteinSeqs"
        Me.chkConsolidateDuplicateProteinSeqs.Size = New System.Drawing.Size(200, 16)
        Me.chkConsolidateDuplicateProteinSeqs.TabIndex = 11
        Me.chkConsolidateDuplicateProteinSeqs.Text = "Consolidate duplicate proteins"
        '
        'chkRenameDuplicateProteins
        '
        Me.chkRenameDuplicateProteins.Checked = True
        Me.chkRenameDuplicateProteins.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkRenameDuplicateProteins.Location = New System.Drawing.Point(8, 88)
        Me.chkRenameDuplicateProteins.Name = "chkRenameDuplicateProteins"
        Me.chkRenameDuplicateProteins.Size = New System.Drawing.Size(128, 32)
        Me.chkRenameDuplicateProteins.TabIndex = 6
        Me.chkRenameDuplicateProteins.Text = "Rename proteins with duplicate names"
        '
        'chkSplitOutMultipleRefsInProteinName
        '
        Me.chkSplitOutMultipleRefsInProteinName.Location = New System.Drawing.Point(184, 96)
        Me.chkSplitOutMultipleRefsInProteinName.Name = "chkSplitOutMultipleRefsInProteinName"
        Me.chkSplitOutMultipleRefsInProteinName.Size = New System.Drawing.Size(192, 32)
        Me.chkSplitOutMultipleRefsInProteinName.TabIndex = 10
        Me.chkSplitOutMultipleRefsInProteinName.Text = "Split out multiple references in protein names (always)"
        '
        'txtInvalidProteinNameCharsToRemove
        '
        Me.txtInvalidProteinNameCharsToRemove.Location = New System.Drawing.Point(336, 30)
        Me.txtInvalidProteinNameCharsToRemove.Name = "txtInvalidProteinNameCharsToRemove"
        Me.txtInvalidProteinNameCharsToRemove.Size = New System.Drawing.Size(40, 20)
        Me.txtInvalidProteinNameCharsToRemove.TabIndex = 4
        Me.txtInvalidProteinNameCharsToRemove.Text = ""
        '
        'lblInvalidProteinNameCharsToRemove
        '
        Me.lblInvalidProteinNameCharsToRemove.Location = New System.Drawing.Point(128, 32)
        Me.lblInvalidProteinNameCharsToRemove.Name = "lblInvalidProteinNameCharsToRemove"
        Me.lblInvalidProteinNameCharsToRemove.Size = New System.Drawing.Size(208, 16)
        Me.lblInvalidProteinNameCharsToRemove.TabIndex = 3
        Me.lblInvalidProteinNameCharsToRemove.Text = "Invalid protein name chars to remove"
        '
        'txtLongProteinNameSplitChars
        '
        Me.txtLongProteinNameSplitChars.Location = New System.Drawing.Point(336, 8)
        Me.txtLongProteinNameSplitChars.Name = "txtLongProteinNameSplitChars"
        Me.txtLongProteinNameSplitChars.Size = New System.Drawing.Size(40, 20)
        Me.txtLongProteinNameSplitChars.TabIndex = 2
        Me.txtLongProteinNameSplitChars.Text = "|"
        '
        'lblLongProteinNameSplitChars
        '
        Me.lblLongProteinNameSplitChars.Location = New System.Drawing.Point(128, 8)
        Me.lblLongProteinNameSplitChars.Name = "lblLongProteinNameSplitChars"
        Me.lblLongProteinNameSplitChars.Size = New System.Drawing.Size(208, 16)
        Me.lblLongProteinNameSplitChars.TabIndex = 1
        Me.lblLongProteinNameSplitChars.Text = "Char(s) to split long protein names on"
        '
        'chkGenerateFixedFastaFile
        '
        Me.chkGenerateFixedFastaFile.Location = New System.Drawing.Point(8, 7)
        Me.chkGenerateFixedFastaFile.Name = "chkGenerateFixedFastaFile"
        Me.chkGenerateFixedFastaFile.Size = New System.Drawing.Size(104, 25)
        Me.chkGenerateFixedFastaFile.TabIndex = 0
        Me.chkGenerateFixedFastaFile.Text = "Generate fixed Fasta file"
        '
        'TabPageRuleOptions
        '
        Me.TabPageRuleOptions.Controls.Add(Me.lblCustomRulesFile)
        Me.TabPageRuleOptions.Controls.Add(Me.cmdCreateDefaultValidationRulesFile)
        Me.TabPageRuleOptions.Controls.Add(Me.txtCustomValidationRulesFilePath)
        Me.TabPageRuleOptions.Controls.Add(Me.cmdSelectCustomRulesFile)
        Me.TabPageRuleOptions.Location = New System.Drawing.Point(4, 22)
        Me.TabPageRuleOptions.Name = "TabPageRuleOptions"
        Me.TabPageRuleOptions.Size = New System.Drawing.Size(401, 166)
        Me.TabPageRuleOptions.TabIndex = 2
        Me.TabPageRuleOptions.Text = "Rule Options"
        '
        'lblCustomRulesFile
        '
        Me.lblCustomRulesFile.Location = New System.Drawing.Point(8, 48)
        Me.lblCustomRulesFile.Name = "lblCustomRulesFile"
        Me.lblCustomRulesFile.Size = New System.Drawing.Size(320, 15)
        Me.lblCustomRulesFile.TabIndex = 2
        Me.lblCustomRulesFile.Text = "Custom rules file (i.e. ValidateFastaFile parameter file)"
        '
        'cmdCreateDefaultValidationRulesFile
        '
        Me.cmdCreateDefaultValidationRulesFile.Location = New System.Drawing.Point(144, 8)
        Me.cmdCreateDefaultValidationRulesFile.Name = "cmdCreateDefaultValidationRulesFile"
        Me.cmdCreateDefaultValidationRulesFile.Size = New System.Drawing.Size(104, 32)
        Me.cmdCreateDefaultValidationRulesFile.TabIndex = 1
        Me.cmdCreateDefaultValidationRulesFile.Text = "Create XML file with default rules"
        '
        'txtCustomValidationRulesFilePath
        '
        Me.txtCustomValidationRulesFilePath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCustomValidationRulesFilePath.Location = New System.Drawing.Point(8, 72)
        Me.txtCustomValidationRulesFilePath.Name = "txtCustomValidationRulesFilePath"
        Me.txtCustomValidationRulesFilePath.Size = New System.Drawing.Size(384, 20)
        Me.txtCustomValidationRulesFilePath.TabIndex = 3
        Me.txtCustomValidationRulesFilePath.Text = ""
        '
        'cmdSelectCustomRulesFile
        '
        Me.cmdSelectCustomRulesFile.Location = New System.Drawing.Point(8, 8)
        Me.cmdSelectCustomRulesFile.Name = "cmdSelectCustomRulesFile"
        Me.cmdSelectCustomRulesFile.Size = New System.Drawing.Size(104, 32)
        Me.cmdSelectCustomRulesFile.TabIndex = 0
        Me.cmdSelectCustomRulesFile.Text = "Select Custom Rules File"
        '
        'chkSaveBasicProteinHashInfoFile
        '
        Me.chkSaveBasicProteinHashInfoFile.Location = New System.Drawing.Point(224, 96)
        Me.chkSaveBasicProteinHashInfoFile.Name = "chkSaveBasicProteinHashInfoFile"
        Me.chkSaveBasicProteinHashInfoFile.Size = New System.Drawing.Size(128, 32)
        Me.chkSaveBasicProteinHashInfoFile.TabIndex = 12
        Me.chkSaveBasicProteinHashInfoFile.Text = "Save basic protein hash info file"
        '
        'frmFastaValidation
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(968, 457)
        Me.Controls.Add(Me.tbsOptions)
        Me.Controls.Add(Me.txtFilterData)
        Me.Controls.Add(Me.txtResults)
        Me.Controls.Add(Me.lblFilterData)
        Me.Controls.Add(Me.pbarProgress)
        Me.Controls.Add(Me.dgWarnings)
        Me.Controls.Add(Me.dgErrors)
        Me.Controls.Add(Me.cmdValidateFastaFile)
        Me.Controls.Add(Me.cmdCancel)
        Me.Menu = Me.MainMenuControl
        Me.Name = "frmFastaValidation"
        Me.Text = "Fasta File Validation"
        CType(Me.dgErrors, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgWarnings, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tbsOptions.ResumeLayout(False)
        Me.TabPageErrorOptions.ResumeLayout(False)
        Me.TabPageNewFastaOptions.ResumeLayout(False)
        Me.TabPageRuleOptions.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "Constants and Enums"

    Private Const COL_NAME_LINE As String = "Line"
    Private Const COL_NAME_COLUMN As String = "Column"
    Private Const COL_NAME_PROTEIN As String = "Protein"
    Private Const COL_NAME_DESCRIPTION As String = "Description"
    Private Const COL_NAME_CONTEXT As String = "Context"

#End Region

#Region "Classwide variables"
    Private mFastaFilePath As String

    Private mErrorsDataset As System.Data.DataSet
    Private mErrorsDataView As System.Data.DataView

    Private mWarningsDataset As System.Data.DataSet
    Private mWarningsDataView As System.Data.DataView

    ' This timer is used to cause StartValidation to be called after the form becomes visible
    Private WithEvents mValidationTriggerTimer As System.Windows.Forms.Timer
    Private WithEvents mValidateFastaFile As ValidateFastaFile.clsValidateFastaFile
#End Region

#Region "Processing Options Interface Functions"
    Public Property CustomRulesFilePath() As String
        Get
            Return txtCustomValidationRulesFilePath.Text
        End Get
        Set(ByVal Value As String)
            txtCustomValidationRulesFilePath.Text = Value
        End Set
    End Property

    Public ReadOnly Property FastaFilePath() As String
        Get
            Return mFastaFilePath
        End Get
    End Property

    Public Property TextFontSize() As Single
        Get
            Return txtResults.Font.SizeInPoints
        End Get
        Set(ByVal Value As Single)
            If Value < 6 Then
                Value = 6
            ElseIf Value > 72 Then
                Value = 72
            End If

            Try
                txtResults.Font = New System.Drawing.Font(txtResults.Font.FontFamily, Value)

                dgErrors.Font = New System.Drawing.Font(txtResults.Font.FontFamily, Value)
                dgWarnings.Font = New System.Drawing.Font(txtResults.Font.FontFamily, Value)

            Catch ex As Exception
                ' Ignore errors here
            End Try

        End Set
    End Property
#End Region

#Region "Procedures"

    Private Sub AppendToString(ByRef strText As String, ByVal strNewText As String)
        strText &= strNewText & ControlChars.NewLine
    End Sub

    Private Sub AppendToString(ByRef strText As String, ByVal strNumberDescription As String, ByVal lngNumber As Long)
        AppendToString(strText, strNumberDescription, lngNumber, True)
    End Sub

    Private Sub AppendToString(ByRef strText As String, ByVal strNumberDescription As String, ByVal lngNumber As Long, ByVal blnUseCommaSeparator As Boolean)

        ' Dim nfi As NumberFormatInfo = New CultureInfo("en-US", False).NumberFormat
        If blnUseCommaSeparator Then
            strText &= strNumberDescription & lngNumber.ToString("###,###,###,###,##0") & ControlChars.NewLine
        Else
            strText &= strNumberDescription & lngNumber.ToString & ControlChars.NewLine
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

        Dim objSaveFile As New System.Windows.Forms.SaveFileDialog
        Dim strCustomRuleDefsFilePath As String

        With objSaveFile
            .AddExtension = True
            .CheckFileExists = False
            .CheckPathExists = True
            .CreatePrompt = False
            .DefaultExt = ".xml"
            .DereferenceLinks = True
            .OverwritePrompt = False
            .ValidateNames = True

            .Filter = "XML Settings Files (*.xml)|*.xml|All files (*.*)|*.*"
            .FilterIndex = 1

            If Len(txtCustomValidationRulesFilePath.Text.Length) > 0 Then
                Try
                    .InitialDirectory = System.IO.Directory.GetParent(txtCustomValidationRulesFilePath.Text).ToString
                Catch
                    .InitialDirectory = GetApplicationDataFolderPath()
                End Try
            Else
                .InitialDirectory = GetApplicationDataFolderPath()
            End If

            .Title = "Select/Create file to save custom rule definitions"

            .ShowDialog()
            If .FileName.Length > 0 Then
                strCustomRuleDefsFilePath = .FileName

                Try
                    Dim objValidateFastaFile As New ValidateFastaFile.clsValidateFastaFile

                    objValidateFastaFile.ShowMessages = True
                    objValidateFastaFile.SaveSettingsToParameterFile(strCustomRuleDefsFilePath)
                    objValidateFastaFile = Nothing

                    If txtCustomValidationRulesFilePath.TextLength = 0 Then
                        txtCustomValidationRulesFilePath.Text = strCustomRuleDefsFilePath
                    End If

                    Windows.Forms.MessageBox.Show("File " & strCustomRuleDefsFilePath & " now contains the default rule validation settings.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)

                Catch ex As Exception
                    Windows.Forms.MessageBox.Show("Error creating/updating file: " & strCustomRuleDefsFilePath & ControlChars.NewLine & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End Try

            End If
        End With

    End Sub

    Private Sub EnableDisableControls()
        Dim blnEnableFixedFastaOptions As Boolean

        blnEnableFixedFastaOptions = chkGenerateFixedFastaFile.Checked

        txtLongProteinNameSplitChars.Enabled = blnEnableFixedFastaOptions
        txtInvalidProteinNameCharsToRemove.Enabled = blnEnableFixedFastaOptions
        chkSplitOutMultipleRefsInProteinName.Enabled = blnEnableFixedFastaOptions
        chkRenameDuplicateProteins.Enabled = blnEnableFixedFastaOptions
        chkConsolidateDuplicateProteinSeqs.Enabled = blnEnableFixedFastaOptions
        chkConsolidateDupsIgnoreILDiff.Enabled = blnEnableFixedFastaOptions And chkConsolidateDuplicateProteinSeqs.Checked

        chkTruncateLongProteinNames.Enabled = blnEnableFixedFastaOptions
        chkSplitOutMultipleRefsForKnownAccession.Enabled = blnEnableFixedFastaOptions
        chkWrapLongResidueLines.Enabled = blnEnableFixedFastaOptions
        chkRemoveInvalidResidues.Enabled = blnEnableFixedFastaOptions

        If txtCustomValidationRulesFilePath.TextLength > 0 Then
            chkAllowAsteriskInResidues.Enabled = False
        Else
            chkAllowAsteriskInResidues.Enabled = True
        End If

    End Sub

    Private Function FlattenDataView(ByVal dvDataView As System.Data.DataView) As String
        ' Copy the contents of the datagrid to the clipboard

        Const chSepChar As Char = ControlChars.Tab

        Dim strText As String
        Dim intIndex As Integer
        Dim intColumnCount As Integer

        'Dim txtResults As New System.Windows.Forms.TextBox

        Dim objRow As System.Data.DataRow

        Try
            strText = String.Empty

            intColumnCount = dvDataView.Table.Columns.Count
            With dvDataView.Table.Columns
                For intIndex = 0 To intColumnCount - 1
                    If intIndex < intColumnCount - 1 Then
                        strText &= .Item(intIndex).ColumnName.ToString & chSepChar
                    Else
                        strText &= .Item(intIndex).ColumnName.ToString & ControlChars.NewLine
                    End If
                Next intIndex
            End With

            For Each objRow In dvDataView.Table.Rows
                For intIndex = 0 To intColumnCount - 1
                    If intIndex < intColumnCount - 1 Then
                        strText &= objRow(intIndex).ToString & chSepChar
                    Else
                        strText &= objRow(intIndex).ToString & ControlChars.NewLine
                    End If
                Next intIndex
            Next objRow

        Catch ex As Exception
            strText &= ControlChars.NewLine & ControlChars.NewLine & "Error copying data: " & ex.Message
        End Try

        Return strText

    End Function

    Private Sub CopySummaryText()
        Dim intSelStart As Integer
        Dim intSelLength As Integer

        With txtResults
            intSelStart = .SelectionStart
            intSelLength = .SelectionLength

            .SelectAll()
            .Copy()

            .SelectionStart = intSelStart
            .SelectionLength = intSelLength
        End With

    End Sub

    Private Sub FilterLists()

        Dim strFilter As String

        Try
            strFilter = String.Empty
            If txtFilterData.TextLength > 0 Then
                strFilter = "[" & COL_NAME_PROTEIN & "] LIKE '%" & txtFilterData.Text & "%' OR [" & COL_NAME_DESCRIPTION & "] LIKE '%" & txtFilterData.Text & "%' OR [" & COL_NAME_CONTEXT & "] LIKE '%" & txtFilterData.Text & "%'"
            End If

            mErrorsDataView.RowFilter = strFilter
            dgErrors.Update()

            mWarningsDataView.RowFilter = strFilter
            dgWarnings.Update()

        Catch ex As Exception
            Windows.Forms.MessageBox.Show("Error filtering lists: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try

    End Sub

    Private Function GetApplicationDataFolderPath() As String
        Dim strAppDataFolderPath As String

        Try
            strAppDataFolderPath = System.IO.Path.Combine( _
                                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), _
                                    "PAST Toolkit\ProteinDigestionSimulator")

            If Not System.IO.Directory.Exists(strAppDataFolderPath) Then
                System.IO.Directory.CreateDirectory(strAppDataFolderPath)
            End If

        Catch ex As Exception
            ' Ignore errors here; an exception will likely be thrown by the calling function that is trying to access this non-existent application data folder
        End Try

        Return strAppDataFolderPath

    End Function

    Private Function GetMyDocsFolderPath() As String
        Return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)
    End Function

    Private Sub InitializeDataGrid(ByRef dgDataGrid As Windows.Forms.DataGrid, ByRef dsDataset As System.Data.DataSet, ByRef dvDataView As System.Data.DataView, ByVal eMsgType As ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants)

        Dim dtDataTable As System.Data.DataTable
        Dim tsTableStyle As Windows.Forms.DataGridTableStyle

        Dim strMsgColumnName As String
        Dim strDatasetName As String
        Dim strDatatableName As String

        If eMsgType = ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants.WarningMsg Then
            strMsgColumnName = "Warnings"
        ElseIf eMsgType = ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants.ErrorMsg Then
            strMsgColumnName = "Errors"
        Else
            strMsgColumnName = "Status"
        End If

        strDatasetName = "ds" & strMsgColumnName
        strDatatableName = "T_" & strMsgColumnName

        ' Create a DataTable
        dtDataTable = New System.Data.DataTable(strDatatableName)

        ' Add the columns to the datatable
        SharedVBNetRoutines.ADONetRoutines.AppendColumnIntegerToTable(dtDataTable, COL_NAME_LINE)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnIntegerToTable(dtDataTable, COL_NAME_COLUMN)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnStringToTable(dtDataTable, COL_NAME_PROTEIN)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnStringToTable(dtDataTable, COL_NAME_DESCRIPTION)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnStringToTable(dtDataTable, COL_NAME_CONTEXT)

        ' Could define a primary key
        ''With dtDataTable
        ''    Dim PrimaryKeyColumn As System.Data.DataColumn() = New System.Data.DataColumn() {.Columns(COL_NAME_LINE)}
        ''    .PrimaryKey = PrimaryKeyColumn
        ''End With

        ' Instantiate the DataSet
        dsDataset = New System.Data.DataSet(strDatasetName)

        ' Add the table to the DataSet
        dsDataset.Tables.Add(dtDataTable)

        ' Instantiate the DataView
        dvDataView = New System.Data.DataView
        With dvDataView
            .Table = dsDataset.Tables(strDatatableName)
            .RowFilter = String.Empty
        End With

        ' Bind the DataSet to the DataGrid
        With dgDataGrid
            .DataSource = dvDataView
            .ReadOnly = False
            .CaptionText = strMsgColumnName
        End With

        ' Update the grid's table style
        UpdateDatagridTableStyle(dgDataGrid, strDatatableName)

    End Sub

    Private Sub InitializeForm(ByVal strFastaFilePath As String)
        txtResults.ReadOnly = True
        Me.TextFontSize = 10

        mFastaFilePath = strFastaFilePath

        InitializeDataGrid(dgErrors, mErrorsDataset, mErrorsDataView, ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants.ErrorMsg)
        InitializeDataGrid(dgWarnings, mWarningsDataset, mWarningsDataView, ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants.WarningMsg)

        mValidationTriggerTimer = New System.Windows.Forms.Timer
        With mValidationTriggerTimer
            .Interval = 100
            .Enabled = True
        End With

        EnableDisableControls()
        SetToolTips()

        ResetOptionsToDefault()

    End Sub

    Private Sub PopulateMsgResultsDatagrid(ByRef objValidateFastaFile As ValidateFastaFile.clsValidateFastaFile, ByRef dgDataGrid As Windows.Forms.DataGrid, ByRef dsDataset As System.Data.DataSet, ByRef udtMsgInfoList() As ValidateFastaFile.IValidateFastaFile.udtMsgInfoType)

        Dim objRow As System.Data.DataRow
        Dim intIndex As Integer

        ' Clear the table
        dsDataset.Tables(0).Clear()

        ' Populate it with new data
        For intIndex = 0 To udtMsgInfoList.Length - 1
            objRow = dsDataset.Tables(0).NewRow()
            With udtMsgInfoList(intIndex)
                objRow(COL_NAME_LINE) = .LineNumber
                objRow(COL_NAME_COLUMN) = .ColNumber
                If .ProteinName Is Nothing OrElse .ProteinName.Length = 0 Then
                    objRow(COL_NAME_PROTEIN) = "N/A"
                Else
                    objRow(COL_NAME_PROTEIN) = .ProteinName
                End If

                objRow(COL_NAME_DESCRIPTION) = objValidateFastaFile.LookupMessageDescription(.MessageCode, .ExtraInfo)
                objRow(COL_NAME_CONTEXT) = .Context
            End With

            ' Add the row to the table
            dsDataset.Tables(0).Rows.Add(objRow)
        Next intIndex

        dgDataGrid.Update()

    End Sub

    Private Sub PositionControls()

        Const MAX_RATIO As Single = 1.5
        Const MENU_HEIGHT As Integer = 80

        Dim intDesiredHeight As Integer

        Dim dblErrorToWarningsRatio As Double
        dblErrorToWarningsRatio = 1

        Try
            If Not mErrorsDataset Is Nothing AndAlso Not mWarningsDataset Is Nothing Then
                If mErrorsDataset.Tables(0).Rows.Count = 0 And mWarningsDataset.Tables(0).Rows.Count = 0 Then
                    dblErrorToWarningsRatio = 1
                ElseIf mErrorsDataset.Tables(0).Rows.Count = 0 Then
                    dblErrorToWarningsRatio = 1 / MAX_RATIO
                ElseIf mWarningsDataset.Tables(0).Rows.Count = 0 Then
                    dblErrorToWarningsRatio = MAX_RATIO
                Else
                    dblErrorToWarningsRatio = mErrorsDataset.Tables(0).Rows.Count / mWarningsDataset.Tables(0).Rows.Count

                    If dblErrorToWarningsRatio < 1 / MAX_RATIO Then
                        dblErrorToWarningsRatio = 1 / MAX_RATIO
                    ElseIf dblErrorToWarningsRatio > MAX_RATIO Then
                        dblErrorToWarningsRatio = MAX_RATIO
                    End If
                End If
            End If
        Catch ex As Exception
            dblErrorToWarningsRatio = 1
        End Try

        If dblErrorToWarningsRatio >= 1 Then
            ' Errors grid should be taller
            intDesiredHeight = CInt(Math.Round((Me.Height - dgErrors.Top - MENU_HEIGHT) / (dblErrorToWarningsRatio + 1) * dblErrorToWarningsRatio, 0))
        Else
            ' Errors grid should be shorter
            intDesiredHeight = CInt(Math.Round((Me.Height - dgErrors.Top - MENU_HEIGHT) / (1 / dblErrorToWarningsRatio + 1), 0))
        End If

        If intDesiredHeight < 5 Then intDesiredHeight = 5
        dgErrors.Height = intDesiredHeight

        dgWarnings.Top = dgErrors.Top + dgErrors.Height + 10

        intDesiredHeight = CInt(Math.Round(intDesiredHeight / dblErrorToWarningsRatio, 0))
        If intDesiredHeight < 5 Then intDesiredHeight = 5
        dgWarnings.Height = intDesiredHeight

    End Sub

    Private Sub ResetOptionsToDefault()
        txtMaxFileErrorsToTrack.Text = "10"

        txtMaximumResiduesPerLine.Text = "120"

        txtProteinNameLengthMinimum.Text = "3"
        txtProteinNameLengthMaximum.Text = "34"

        txtLongProteinNameSplitChars.Text = ValidateFastaFile.clsValidateFastaFile.DEFAULT_LONG_PROTEIN_NAME_SPLIT_CHAR
        txtInvalidProteinNameCharsToRemove.Text = ""

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
        chkSplitOutMultipleRefsForKnownAccession.Checked = True
        chkSplitOutMultipleRefsInProteinName.Checked = False
        chkTruncateLongProteinNames.Checked = True
        chkWrapLongResidueLines.Checked = True
    End Sub

    Private Sub SelectCustomRulesFile()

        Dim objOpenFile As New System.Windows.Forms.OpenFileDialog

        With objOpenFile
            .AddExtension = True
            .CheckFileExists = True
            .CheckPathExists = True
            .DefaultExt = ".xml"
            .DereferenceLinks = True
            .Multiselect = False
            .ValidateNames = True

            .Filter = "XML Settings Files (*.xml)|*.xml|All files (*.*)|*.*"
            .FilterIndex = 1

            If Len(txtCustomValidationRulesFilePath.Text.Length) > 0 Then
                Try
                    .InitialDirectory = System.IO.Directory.GetParent(txtCustomValidationRulesFilePath.Text).ToString
                Catch
                    .InitialDirectory = GetApplicationDataFolderPath()
                End Try
            Else
                .InitialDirectory = GetApplicationDataFolderPath()
            End If

            .Title = "Select custom rules file"

            .ShowDialog()
            If .FileName.Length > 0 Then
                txtCustomValidationRulesFilePath.Text = .FileName
            End If
        End With

    End Sub

    Private Sub ShowAboutBox()
        Dim strMessage As String

        strMessage = String.Empty

        strMessage &= "Fasta File Validation module written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2005" & ControlChars.NewLine
        strMessage &= "Copyright 2005, Battelle Memorial Institute.  All Rights Reserved." & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= "This is version " & System.Windows.Forms.Application.ProductVersion & " (" & PROGRAM_DATE & ")" & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= "E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com" & ControlChars.NewLine
        strMessage &= "Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/" & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= "Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License.  "
        strMessage &= "You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0" & ControlChars.NewLine & ControlChars.NewLine

        strMessage &= "Notice: This computer software was prepared by Battelle Memorial Institute, "
        strMessage &= "hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the "
        strMessage &= "Department of Energy (DOE).  All rights in the computer software are reserved "
        strMessage &= "by DOE on behalf of the United States Government and the Contractor as "
        strMessage &= "provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY "
        strMessage &= "WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS "
        strMessage &= "SOFTWARE.  This notice including this sentence must appear on any copies of "
        strMessage &= "this computer software." & ControlChars.NewLine

        Windows.Forms.MessageBox.Show(strMessage, "About", MessageBoxButtons.OK, MessageBoxIcon.Information)

    End Sub

    Private Sub ShowHideObjectsDuringValidation(ByVal blnValidating As Boolean)
        pbarProgress.Visible = blnValidating

        lblFilterData.Visible = Not blnValidating
        txtFilterData.Visible = Not blnValidating

        cmdValidateFastaFile.Visible = Not blnValidating
        cmdValidateFastaFile.Enabled = Not blnValidating

        cmdCancel.Visible = blnValidating
        cmdCancel.Enabled = blnValidating

    End Sub

    Private Sub SummaryAppendText(ByVal strNewText As String)
        txtResults.Text &= strNewText
        txtResults.SelectionStart = txtResults.TextLength
        txtResults.ScrollToCaret()
    End Sub

    Private Sub SummarySetText(ByVal strNewText As String)
        txtResults.Text = strNewText
        txtResults.SelectionStart = 1
        txtResults.ScrollToCaret()
    End Sub

    Private Sub SetToolTips()
        Dim objToolTipControl As New System.Windows.Forms.ToolTip

        With objToolTipControl
            .SetToolTip(txtLongProteinNameSplitChars, "Enter one or more characters to look for when truncating long protein names (do not separate the characters by commas).  Default character is a vertical bar.")
            .SetToolTip(txtInvalidProteinNameCharsToRemove, "Enter one or more characters to look and replace with an underscore (do not separate the characters by commas).  Leave blank to not replace any characters.")
            .SetToolTip(chkSplitOutMultipleRefsForKnownAccession, "If a protein name matches the standard IPI, GI, or JGI accession numbers, and if it contains additional reference information, then the additional information will be moved to the protein's description.")
            .SetToolTip(chkSaveBasicProteinHashInfoFile, "To minimize memory usage, enable this option by disable 'Check for Duplicate Proteins'")
        End With

        objToolTipControl = Nothing

    End Sub

    Private Sub StartValidation()

        Dim strResults As String
        Dim strSepChar As String
        Dim strMessage As String

        Dim strParameterFilePath As String
        Dim blnFileExists As Boolean

        Dim blnSuccess As Boolean
        Dim intIndex As Integer

        Try
            If mValidateFastaFile Is Nothing Then
                mValidateFastaFile = New ValidateFastaFile.clsValidateFastaFile
            End If

            ShowHideObjectsDuringValidation(True)
            Me.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor
            Windows.Forms.Application.DoEvents()

            With mValidateFastaFile
                .ShowMessages = True

                ' Note: the following settings will be overridden if a parameter file with these settings defined is provided to .ProcessFile()
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.WarnBlankLinesBetweenProteins, True)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.AllowAsteriskInResidues, chkAllowAsteriskInResidues.Checked)

                .MinimumProteinNameLength = SharedVBNetRoutines.VBNetRoutines.ParseTextboxValueInt(txtProteinNameLengthMinimum, "Minimum protein name length should be a number", False, 3, False)
                .MaximumProteinNameLength = SharedVBNetRoutines.VBNetRoutines.ParseTextboxValueInt(txtProteinNameLengthMaximum, "Maximum protein name length should be a number", False, 34, False)
                .MaximumResiduesPerLine = SharedVBNetRoutines.VBNetRoutines.ParseTextboxValueInt(txtMaximumResiduesPerLine, "Maximum residues per line should be a number", False, 120, False)

                strParameterFilePath = txtCustomValidationRulesFilePath.Text
                If strParameterFilePath.Length > 0 Then
                    Try
                        blnFileExists = System.IO.File.Exists(strParameterFilePath)
                    Catch ex As Exception
                        blnFileExists = False
                    End Try

                    If Not blnFileExists Then
                        Windows.Forms.MessageBox.Show("Custom rules validation file not found: " & ControlChars.NewLine & strParameterFilePath & ControlChars.NewLine & "Tthe default validation rules will be used instead.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                        strParameterFilePath = String.Empty
                    Else
                        mValidateFastaFile.LoadParameterFileSettings(strParameterFilePath)
                    End If
                End If

                If strParameterFilePath.Length = 0 Then
                    .SetDefaultRules()
                End If

                .MaximumFileErrorsToTrack = SharedVBNetRoutines.VBNetRoutines.ParseTextboxValueInt(txtMaxFileErrorsToTrack, "Max file errors or warnings should be a positive number", False, 10, False)

                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.CheckForDuplicateProteinNames, chkCheckForDuplicateProteinInfo.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.CheckForDuplicateProteinSequences, chkCheckForDuplicateProteinInfo.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.SaveBasicProteinHashInfoFile, chkSaveBasicProteinHashInfoFile.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.OutputToStatsFile, chkLogResults.Checked)

                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.GenerateFixedFASTAFile, chkGenerateFixedFastaFile.Checked)

                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaSplitOutMultipleRefsForKnownAccession, chkSplitOutMultipleRefsForKnownAccession.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.SplitOutMultipleRefsInProteinName, chkSplitOutMultipleRefsInProteinName.Checked)

                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaRenameDuplicateNameProteins, chkRenameDuplicateProteins.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaConsolidateDuplicateProteinSeqs, chkConsolidateDuplicateProteinSeqs.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaConsolidateDupsIgnoreILDiff, chkConsolidateDupsIgnoreILDiff.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaTruncateLongProteinNames, chkTruncateLongProteinNames.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaWrapLongResidueLines, chkWrapLongResidueLines.Checked)
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaRemoveInvalidResidues, chkRemoveInvalidResidues.Checked())

                .ProteinNameFirstRefSepChars = ValidateFastaFile.clsValidateFastaFile.DEFAULT_PROTEIN_NAME_FIRST_REF_SEP_CHARS
                .ProteinNameSubsequentRefSepChars = ValidateFastaFile.clsValidateFastaFile.DEFAULT_PROTEIN_NAME_SUBSEQUENT_REF_SEP_CHARS

                ' Also apply chkGenerateFixedFastaFile to SaveProteinSequenceHashInfoFiles
                .SetOptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.SaveProteinSequenceHashInfoFiles, chkGenerateFixedFastaFile.Checked)

                If txtLongProteinNameSplitChars.TextLength > 0 Then
                    .LongProteinNameSplitChars = txtLongProteinNameSplitChars.Text
                Else
                    .LongProteinNameSplitChars = ValidateFastaFile.clsValidateFastaFile.DEFAULT_LONG_PROTEIN_NAME_SPLIT_CHAR
                End If

                .ProteinNameInvalidCharsToRemove = txtInvalidProteinNameCharsToRemove.Text

            End With

            With pbarProgress
                .Minimum = 0
                .Maximum = 100
                .Value = 0
            End With


            ' Analyze the fasta file; returns true if the analysis was successful (even if the file contains errors or warnings)
            blnSuccess = mValidateFastaFile.ProcessFile(mFastaFilePath, String.Empty, String.Empty)

            cmdCancel.Enabled = False

            If blnSuccess Then
                ' Display the results

                strSepChar = ControlChars.Tab

                With mValidateFastaFile
                    strResults = "Results for file " & .FastaFilePath & ControlChars.NewLine

                    AppendToString(strResults, "Protein count = " & .ProteinCount & strSepChar & strSepChar & "Residue count = ", .ResidueCount)
                    AppendToString(strResults, "Error count = " & .ErrorWarningCounts(ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants.ErrorMsg, ValidateFastaFile.IValidateFastaFile.ErrorWarningCountTypes.Total))
                    AppendToString(strResults, "Warning count = ", .ErrorWarningCounts(ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants.WarningMsg, ValidateFastaFile.IValidateFastaFile.ErrorWarningCountTypes.Total))

                    If .OptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.GenerateFixedFASTAFile) Then
                        AppendToString(strResults, "Count of long protein names that were truncated = " & .FixedFASTAFileStats(ValidateFastaFile.IValidateFastaFile.FixedFASTAFileValues.TruncatedProteinNameCount))
                        AppendToString(strResults, "Count of protein names with invalid chars removed = " & .FixedFASTAFileStats(ValidateFastaFile.IValidateFastaFile.FixedFASTAFileValues.ProteinNamesInvalidCharsReplaced))
                        AppendToString(strResults, "Count of protein names with multiple refs split out = " & .FixedFASTAFileStats(ValidateFastaFile.IValidateFastaFile.FixedFASTAFileValues.ProteinNamesMultipleRefsRemoved))
                        AppendToString(strResults, "Count of residue lines with invalid chars removed = " & .FixedFASTAFileStats(ValidateFastaFile.IValidateFastaFile.FixedFASTAFileValues.UpdatedResidueLines))

                        If .OptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaRenameDuplicateNameProteins) Then
                            AppendToString(strResults, "Count of proteins renamed due to duplicate names = " & .FixedFASTAFileStats(ValidateFastaFile.IValidateFastaFile.FixedFASTAFileValues.DuplicateProteinNamesRenamedCount))
                        ElseIf .OptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.CheckForDuplicateProteinNames) Then
                            AppendToString(strResults, "Count of proteins skipped due to duplicate names = " & .FixedFASTAFileStats(ValidateFastaFile.IValidateFastaFile.FixedFASTAFileValues.DuplicateProteinNamesSkippedCount))
                        End If

                        If .OptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.FixedFastaConsolidateDuplicateProteinSeqs) Then
                            AppendToString(strResults, "Count of proteins removed due to duplicate sequences = " & .FixedFASTAFileStats(ValidateFastaFile.IValidateFastaFile.FixedFASTAFileValues.DuplicateProteinSeqsSkippedCount))
                        End If

                    End If

                    If strParameterFilePath.Length > 0 Then
                        AppendToString(strResults, "Used validation rules from file " & strParameterFilePath)
                    Else
                        AppendToString(strResults, "Default validation rules were used.")
                    End If

                    If .OptionSwitch(ValidateFastaFile.IValidateFastaFile.SwitchOptions.OutputToStatsFile) Then
                        AppendToString(strResults, "Results were logged to file: " & .StatsFilePath())
                    End If

                    txtResults.Text = strResults
                    txtResults.SelectionStart = 0
                    txtResults.SelectionLength = 0

                    ' Clear the filters
                    txtFilterData.Text = String.Empty

                    If .ErrorWarningCounts(ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants.ErrorMsg, ValidateFastaFile.IValidateFastaFile.ErrorWarningCountTypes.Specified) > 0 Then
                        ' List all of the errors
                        PopulateMsgResultsDatagrid(mValidateFastaFile, dgErrors, mErrorsDataset, .FileErrorList)
                    Else
                        mErrorsDataset.Tables(0).Clear()
                    End If

                    If .ErrorWarningCounts(ValidateFastaFile.IValidateFastaFile.eMsgTypeConstants.WarningMsg, ValidateFastaFile.IValidateFastaFile.ErrorWarningCountTypes.Specified) > 0 Then
                        ' List all of the warnings in the datagrid
                        PopulateMsgResultsDatagrid(mValidateFastaFile, dgWarnings, mWarningsDataset, .FileWarningList)
                    Else
                        mWarningsDataset.Tables(0).Clear()
                    End If

                End With

                PositionControls()
                FilterLists()

            Else
                txtResults.Text = "Error calling objValidateFastaFile.ProcessFile: " & mValidateFastaFile.GetErrorMessage()
            End If

        Catch ex As Exception
            Windows.Forms.MessageBox.Show("Error validating fasta file: " & mFastaFilePath & ControlChars.NewLine & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        Finally
            mValidateFastaFile = Nothing
            ShowHideObjectsDuringValidation(False)

            Me.Cursor.Current = System.Windows.Forms.Cursors.Default
            Windows.Forms.Application.DoEvents()
        End Try

    End Sub

    Private Sub UpdateDatagridTableStyle(ByRef dgDataGrid As Windows.Forms.DataGrid, ByVal strTargetTableName As String)

        Dim tsTableStyle As System.Windows.Forms.DataGridTableStyle

        ' Instantiate the TableStyle
        tsTableStyle = New System.Windows.Forms.DataGridTableStyle

        ' Setting the MappingName of the table style to strTargetTableName will cause this style to be used with that table
        With tsTableStyle
            .MappingName = strTargetTableName
            .AllowSorting = True
            .ColumnHeadersVisible = True
            .RowHeadersVisible = False
            .ReadOnly = True
        End With

        SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsTableStyle, COL_NAME_LINE, "Line", 80)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsTableStyle, COL_NAME_COLUMN, "Column", 80)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsTableStyle, COL_NAME_PROTEIN, "Protein", 200)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsTableStyle, COL_NAME_DESCRIPTION, "Value", 550)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsTableStyle, COL_NAME_CONTEXT, "Context", 200)

        With dgDataGrid
            .TableStyles.Clear()

            If Not .TableStyles.Contains(tsTableStyle) Then
                .TableStyles.Add(tsTableStyle)
            End If

            .Refresh()
        End With

    End Sub

#End Region

#Region "Control Handlers"

    Private Sub chkGenerateFixedFastaFile_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkGenerateFixedFastaFile.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkConsolidateDuplicateProteinSeqs_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkConsolidateDuplicateProteinSeqs.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        If Not mValidateFastaFile Is Nothing Then
            cmdCancel.Enabled = False
            mValidateFastaFile.AbortProcessingNow()
        End If
    End Sub

    Private Sub cmdCreateDefaultValidationRulesFile_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCreateDefaultValidationRulesFile.Click
        CreateDefaultValidationRulesFile()
    End Sub

    Private Sub cmdSelectCustomRulesFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectCustomRulesFile.Click
        SelectCustomRulesFile()
    End Sub

    Private Sub cmdValidateFastaFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdValidateFastaFile.Click
        StartValidation()
    End Sub

    Private Sub frmFastaValidationResults_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        PositionControls()
    End Sub

    Private Sub txtCustomValidationRulesFilePath_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtCustomValidationRulesFilePath.TextChanged
        EnableDisableControls()
    End Sub

    Private Sub txtFilterData_Validated(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtFilterData.Validated
        FilterLists()
    End Sub

    Private Sub txtFilterData_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtFilterData.KeyDown
        If e.KeyCode = Keys.Enter Then
            FilterLists()
        End If
    End Sub

    Private Sub txtMaxFileErrorsToTrack_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtMaxFileErrorsToTrack, e, True)
    End Sub

    Private Sub txtMaxFileErrorsToTrack_KeyPress1(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMaxFileErrorsToTrack.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtMaxFileErrorsToTrack, e, True)
    End Sub

    Private Sub txtMaximumResiduesPerLine_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMaximumResiduesPerLine.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtMaximumResiduesPerLine, e, True)
    End Sub

    Private Sub txtProteinNameLengthMinimum_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtProteinNameLengthMinimum.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtProteinNameLengthMinimum, e, True)
    End Sub

    Private Sub txtProteinNameLengthMaximum_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtProteinNameLengthMaximum.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtProteinNameLengthMaximum, e, True)
    End Sub

    Private Sub txtResults_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtResults.KeyDown
        If e.Control = True Then
            If e.KeyCode = Keys.A Then
                txtResults.SelectAll()
            End If
        End If
    End Sub

    Private Sub mValidationTriggerTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles mValidationTriggerTimer.Tick
        ' This timer is used to cause StartValidation to be called after the form becomes visible
        mValidationTriggerTimer.Enabled = False
        StartValidation()
    End Sub

#End Region

#Region "Menu Handlers"

    Private Sub mnuFileExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileExit.Click
        Me.Close()
    End Sub

    Private Sub mnuEditCopySummary_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditCopySummary.Click
        CopySummaryText()
    End Sub

    Private Sub mnuEditCopyAllResults_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditCopyAllResults.Click
        CopyAllResults()
    End Sub

    Private Sub mnuEditCopyAllErrors_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditCopyAllErrors.Click
        CopyErrorsDataView()
    End Sub

    Private Sub mnuEditCopyAllWarnings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditCopyAllWarnings.Click
        CopyWarningsDataView()
    End Sub

    Private Sub mnuEditFontSizeDecrease_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditFontSizeDecrease.Click
        If Me.TextFontSize > 14 Then
            Me.TextFontSize = Me.TextFontSize - 2
        Else
            Me.TextFontSize = Me.TextFontSize - 1
        End If
    End Sub

    Private Sub mnuEditFontSizeIncrease_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditFontSizeIncrease.Click
        If Me.TextFontSize >= 14 Then
            Me.TextFontSize = Me.TextFontSize + 2
        Else
            Me.TextFontSize = Me.TextFontSize + 1
        End If
    End Sub

    Private Sub mnuEditResetToDefaults_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditResetToDefaults.Click
        ResetOptionsToDefault()
    End Sub

    Private Sub mnuHelpAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuHelpAbout.Click
        ShowAboutBox()
    End Sub

#End Region

    Private Sub mValidateFastaFile_ProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single) Handles mValidateFastaFile.ProgressChanged
        Try
            pbarProgress.Value = CType(percentComplete, Integer)
            Windows.Forms.Application.DoEvents()
        Catch ex As Exception
            ' Ignore errors here
        End Try
    End Sub

End Class
