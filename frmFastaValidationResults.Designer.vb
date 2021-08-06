<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmFastaValidation
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.chkWrapLongResidueLines = New System.Windows.Forms.CheckBox()
        Me.chkSplitOutMultipleRefsForKnownAccession = New System.Windows.Forms.CheckBox()
        Me.chkTruncateLongProteinNames = New System.Windows.Forms.CheckBox()
        Me.txtResults = New System.Windows.Forms.TextBox()
        Me.TabPageErrorOptions = New System.Windows.Forms.TabPage()
        Me.chkSaveBasicProteinHashInfoFile = New System.Windows.Forms.CheckBox()
        Me.lblProteinNameLengthUnits = New System.Windows.Forms.Label()
        Me.lblProteinNameLength2 = New System.Windows.Forms.Label()
        Me.txtProteinNameLengthMaximum = New System.Windows.Forms.TextBox()
        Me.txtProteinNameLengthMinimum = New System.Windows.Forms.TextBox()
        Me.lblProteinNameLength = New System.Windows.Forms.Label()
        Me.txtMaximumResiduesPerLine = New System.Windows.Forms.TextBox()
        Me.lblMaximumResiduesPerLine = New System.Windows.Forms.Label()
        Me.chkAllowAsteriskInResidues = New System.Windows.Forms.CheckBox()
        Me.chkCheckForDuplicateProteinInfo = New System.Windows.Forms.CheckBox()
        Me.txtMaxFileErrorsToTrack = New System.Windows.Forms.TextBox()
        Me.lblMaxFileErrorsToTrack = New System.Windows.Forms.Label()
        Me.chkLogResults = New System.Windows.Forms.CheckBox()
        Me.chkConsolidateDupsIgnoreILDiff = New System.Windows.Forms.CheckBox()
        Me.chkRemoveInvalidResidues = New System.Windows.Forms.CheckBox()
        Me.TabPageNewFastaOptions = New System.Windows.Forms.TabPage()
        Me.txtResiduesPerLineForWrap = New System.Windows.Forms.TextBox()
        Me.lblResiduesPerLineForWrap = New System.Windows.Forms.Label()
        Me.chkKeepDuplicateNamedProteins = New System.Windows.Forms.CheckBox()
        Me.chkConsolidateDuplicateProteinSeqs = New System.Windows.Forms.CheckBox()
        Me.chkRenameDuplicateProteins = New System.Windows.Forms.CheckBox()
        Me.chkSplitOutMultipleRefsInProteinName = New System.Windows.Forms.CheckBox()
        Me.txtInvalidProteinNameCharsToRemove = New System.Windows.Forms.TextBox()
        Me.lblInvalidProteinNameCharsToRemove = New System.Windows.Forms.Label()
        Me.txtLongProteinNameSplitChars = New System.Windows.Forms.TextBox()
        Me.lblLongProteinNameSplitChars = New System.Windows.Forms.Label()
        Me.chkGenerateFixedFastaFile = New System.Windows.Forms.CheckBox()
        Me.lblCustomRulesFile = New System.Windows.Forms.Label()
        Me.TabPageRuleOptions = New System.Windows.Forms.TabPage()
        Me.cmdCreateDefaultValidationRulesFile = New System.Windows.Forms.Button()
        Me.txtCustomValidationRulesFilePath = New System.Windows.Forms.TextBox()
        Me.cmdSelectCustomRulesFile = New System.Windows.Forms.Button()
        Me.tbsOptions = New System.Windows.Forms.TabControl()
        Me.mnuEditCopyAllWarnings = New System.Windows.Forms.MenuItem()
        Me.MainMenuControl = New System.Windows.Forms.MainMenu(Me.components)
        Me.mnuFile = New System.Windows.Forms.MenuItem()
        Me.mnuFileExit = New System.Windows.Forms.MenuItem()
        Me.mnuEdit = New System.Windows.Forms.MenuItem()
        Me.mnuEditCopyAllResults = New System.Windows.Forms.MenuItem()
        Me.mnuEditSep1 = New System.Windows.Forms.MenuItem()
        Me.mnuEditCopySummary = New System.Windows.Forms.MenuItem()
        Me.mnuEditCopyAllErrors = New System.Windows.Forms.MenuItem()
        Me.mnuEditSep2 = New System.Windows.Forms.MenuItem()
        Me.mnuEditFontSizeDecrease = New System.Windows.Forms.MenuItem()
        Me.mnuEditFontSizeIncrease = New System.Windows.Forms.MenuItem()
        Me.MenuItem2 = New System.Windows.Forms.MenuItem()
        Me.mnuEditResetToDefaults = New System.Windows.Forms.MenuItem()
        Me.MenuItem1 = New System.Windows.Forms.MenuItem()
        Me.mnuHelpAbout = New System.Windows.Forms.MenuItem()
        Me.dgWarnings = New System.Windows.Forms.DataGrid()
        Me.lblFilterData = New System.Windows.Forms.Label()
        Me.txtFilterData = New System.Windows.Forms.TextBox()
        Me.pbarProgress = New System.Windows.Forms.ProgressBar()
        Me.cmdValidateFastaFile = New System.Windows.Forms.Button()
        Me.dgErrors = New System.Windows.Forms.DataGrid()
        Me.cmdCancel = New System.Windows.Forms.Button()
        Me.TabPageErrorOptions.SuspendLayout()
        Me.TabPageNewFastaOptions.SuspendLayout()
        Me.TabPageRuleOptions.SuspendLayout()
        Me.tbsOptions.SuspendLayout()
        CType(Me.dgWarnings, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgErrors, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'chkWrapLongResidueLines
        '
        Me.chkWrapLongResidueLines.Checked = True
        Me.chkWrapLongResidueLines.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkWrapLongResidueLines.Location = New System.Drawing.Point(8, 57)
        Me.chkWrapLongResidueLines.Margin = New System.Windows.Forms.Padding(4)
        Me.chkWrapLongResidueLines.Name = "chkWrapLongResidueLines"
        Me.chkWrapLongResidueLines.Size = New System.Drawing.Size(203, 21)
        Me.chkWrapLongResidueLines.TabIndex = 7
        Me.chkWrapLongResidueLines.Text = "Wrap long residue lines"
        '
        'chkSplitOutMultipleRefsForKnownAccession
        '
        Me.chkSplitOutMultipleRefsForKnownAccession.Location = New System.Drawing.Point(239, 81)
        Me.chkSplitOutMultipleRefsForKnownAccession.Margin = New System.Windows.Forms.Padding(4)
        Me.chkSplitOutMultipleRefsForKnownAccession.Name = "chkSplitOutMultipleRefsForKnownAccession"
        Me.chkSplitOutMultipleRefsForKnownAccession.Size = New System.Drawing.Size(256, 40)
        Me.chkSplitOutMultipleRefsForKnownAccession.TabIndex = 9
        Me.chkSplitOutMultipleRefsForKnownAccession.Text = "Split out multiple references only if IPI, GI, or JGI"
        '
        'chkTruncateLongProteinNames
        '
        Me.chkTruncateLongProteinNames.Checked = True
        Me.chkTruncateLongProteinNames.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkTruncateLongProteinNames.Location = New System.Drawing.Point(8, 91)
        Me.chkTruncateLongProteinNames.Margin = New System.Windows.Forms.Padding(4)
        Me.chkTruncateLongProteinNames.Name = "chkTruncateLongProteinNames"
        Me.chkTruncateLongProteinNames.Size = New System.Drawing.Size(235, 21)
        Me.chkTruncateLongProteinNames.TabIndex = 5
        Me.chkTruncateLongProteinNames.Text = "Truncate long protein names"
        '
        'txtResults
        '
        Me.txtResults.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtResults.Location = New System.Drawing.Point(584, 9)
        Me.txtResults.Margin = New System.Windows.Forms.Padding(4)
        Me.txtResults.Multiline = True
        Me.txtResults.Name = "txtResults"
        Me.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtResults.Size = New System.Drawing.Size(637, 186)
        Me.txtResults.TabIndex = 10
        Me.txtResults.WordWrap = False
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
        Me.TabPageErrorOptions.Location = New System.Drawing.Point(4, 25)
        Me.TabPageErrorOptions.Margin = New System.Windows.Forms.Padding(4)
        Me.TabPageErrorOptions.Name = "TabPageErrorOptions"
        Me.TabPageErrorOptions.Size = New System.Drawing.Size(537, 228)
        Me.TabPageErrorOptions.TabIndex = 0
        Me.TabPageErrorOptions.Text = "Error Options"
        '
        'chkSaveBasicProteinHashInfoFile
        '
        Me.chkSaveBasicProteinHashInfoFile.Location = New System.Drawing.Point(293, 119)
        Me.chkSaveBasicProteinHashInfoFile.Margin = New System.Windows.Forms.Padding(4)
        Me.chkSaveBasicProteinHashInfoFile.Name = "chkSaveBasicProteinHashInfoFile"
        Me.chkSaveBasicProteinHashInfoFile.Size = New System.Drawing.Size(171, 41)
        Me.chkSaveBasicProteinHashInfoFile.TabIndex = 12
        Me.chkSaveBasicProteinHashInfoFile.Text = "Save basic protein hash info file"
        '
        'lblProteinNameLengthUnits
        '
        Me.lblProteinNameLengthUnits.Location = New System.Drawing.Point(407, 65)
        Me.lblProteinNameLengthUnits.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProteinNameLengthUnits.Name = "lblProteinNameLengthUnits"
        Me.lblProteinNameLengthUnits.Size = New System.Drawing.Size(107, 21)
        Me.lblProteinNameLengthUnits.TabIndex = 8
        Me.lblProteinNameLengthUnits.Text = "characters"
        '
        'lblProteinNameLength2
        '
        Me.lblProteinNameLength2.Location = New System.Drawing.Point(311, 65)
        Me.lblProteinNameLength2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProteinNameLength2.Name = "lblProteinNameLength2"
        Me.lblProteinNameLength2.Size = New System.Drawing.Size(21, 21)
        Me.lblProteinNameLength2.TabIndex = 6
        Me.lblProteinNameLength2.Text = "to"
        '
        'txtProteinNameLengthMaximum
        '
        Me.txtProteinNameLengthMaximum.Location = New System.Drawing.Point(343, 62)
        Me.txtProteinNameLengthMaximum.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProteinNameLengthMaximum.Name = "txtProteinNameLengthMaximum"
        Me.txtProteinNameLengthMaximum.Size = New System.Drawing.Size(52, 22)
        Me.txtProteinNameLengthMaximum.TabIndex = 7
        Me.txtProteinNameLengthMaximum.Text = "34"
        '
        'txtProteinNameLengthMinimum
        '
        Me.txtProteinNameLengthMinimum.Location = New System.Drawing.Point(247, 62)
        Me.txtProteinNameLengthMinimum.Margin = New System.Windows.Forms.Padding(4)
        Me.txtProteinNameLengthMinimum.Name = "txtProteinNameLengthMinimum"
        Me.txtProteinNameLengthMinimum.Size = New System.Drawing.Size(52, 22)
        Me.txtProteinNameLengthMinimum.TabIndex = 5
        Me.txtProteinNameLengthMinimum.Text = "3"
        '
        'lblProteinNameLength
        '
        Me.lblProteinNameLength.Location = New System.Drawing.Point(5, 64)
        Me.lblProteinNameLength.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblProteinNameLength.Name = "lblProteinNameLength"
        Me.lblProteinNameLength.Size = New System.Drawing.Size(235, 21)
        Me.lblProteinNameLength.TabIndex = 4
        Me.lblProteinNameLength.Text = "Valid protein name length range"
        '
        'txtMaximumResiduesPerLine
        '
        Me.txtMaximumResiduesPerLine.Location = New System.Drawing.Point(247, 32)
        Me.txtMaximumResiduesPerLine.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaximumResiduesPerLine.Name = "txtMaximumResiduesPerLine"
        Me.txtMaximumResiduesPerLine.Size = New System.Drawing.Size(52, 22)
        Me.txtMaximumResiduesPerLine.TabIndex = 3
        Me.txtMaximumResiduesPerLine.Text = "120"
        '
        'lblMaximumResiduesPerLine
        '
        Me.lblMaximumResiduesPerLine.Location = New System.Drawing.Point(5, 34)
        Me.lblMaximumResiduesPerLine.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaximumResiduesPerLine.Name = "lblMaximumResiduesPerLine"
        Me.lblMaximumResiduesPerLine.Size = New System.Drawing.Size(224, 21)
        Me.lblMaximumResiduesPerLine.TabIndex = 2
        Me.lblMaximumResiduesPerLine.Text = "Maximum residues per line"
        '
        'chkAllowAsteriskInResidues
        '
        Me.chkAllowAsteriskInResidues.Location = New System.Drawing.Point(8, 91)
        Me.chkAllowAsteriskInResidues.Margin = New System.Windows.Forms.Padding(4)
        Me.chkAllowAsteriskInResidues.Name = "chkAllowAsteriskInResidues"
        Me.chkAllowAsteriskInResidues.Size = New System.Drawing.Size(224, 21)
        Me.chkAllowAsteriskInResidues.TabIndex = 9
        Me.chkAllowAsteriskInResidues.Text = "Allow asterisks in residues"
        '
        'chkCheckForDuplicateProteinInfo
        '
        Me.chkCheckForDuplicateProteinInfo.Checked = True
        Me.chkCheckForDuplicateProteinInfo.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCheckForDuplicateProteinInfo.Location = New System.Drawing.Point(8, 119)
        Me.chkCheckForDuplicateProteinInfo.Margin = New System.Windows.Forms.Padding(4)
        Me.chkCheckForDuplicateProteinInfo.Name = "chkCheckForDuplicateProteinInfo"
        Me.chkCheckForDuplicateProteinInfo.Size = New System.Drawing.Size(267, 41)
        Me.chkCheckForDuplicateProteinInfo.TabIndex = 10
        Me.chkCheckForDuplicateProteinInfo.Text = "Check for duplicate protein names and duplicate protein sequences"
        '
        'txtMaxFileErrorsToTrack
        '
        Me.txtMaxFileErrorsToTrack.Location = New System.Drawing.Point(385, 2)
        Me.txtMaxFileErrorsToTrack.Margin = New System.Windows.Forms.Padding(4)
        Me.txtMaxFileErrorsToTrack.Name = "txtMaxFileErrorsToTrack"
        Me.txtMaxFileErrorsToTrack.Size = New System.Drawing.Size(52, 22)
        Me.txtMaxFileErrorsToTrack.TabIndex = 1
        Me.txtMaxFileErrorsToTrack.Text = "10"
        '
        'lblMaxFileErrorsToTrack
        '
        Me.lblMaxFileErrorsToTrack.Location = New System.Drawing.Point(5, 5)
        Me.lblMaxFileErrorsToTrack.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblMaxFileErrorsToTrack.Name = "lblMaxFileErrorsToTrack"
        Me.lblMaxFileErrorsToTrack.Size = New System.Drawing.Size(364, 21)
        Me.lblMaxFileErrorsToTrack.TabIndex = 0
        Me.lblMaxFileErrorsToTrack.Text = "Maximum # of errors or warnings to track in detail"
        '
        'chkLogResults
        '
        Me.chkLogResults.Location = New System.Drawing.Point(8, 171)
        Me.chkLogResults.Margin = New System.Windows.Forms.Padding(4)
        Me.chkLogResults.Name = "chkLogResults"
        Me.chkLogResults.Size = New System.Drawing.Size(169, 21)
        Me.chkLogResults.TabIndex = 11
        Me.chkLogResults.Text = "Log results to file"
        '
        'chkConsolidateDupsIgnoreILDiff
        '
        Me.chkConsolidateDupsIgnoreILDiff.Checked = True
        Me.chkConsolidateDupsIgnoreILDiff.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkConsolidateDupsIgnoreILDiff.Location = New System.Drawing.Point(239, 202)
        Me.chkConsolidateDupsIgnoreILDiff.Margin = New System.Windows.Forms.Padding(4)
        Me.chkConsolidateDupsIgnoreILDiff.Name = "chkConsolidateDupsIgnoreILDiff"
        Me.chkConsolidateDupsIgnoreILDiff.Size = New System.Drawing.Size(277, 22)
        Me.chkConsolidateDupsIgnoreILDiff.TabIndex = 12
        Me.chkConsolidateDupsIgnoreILDiff.Text = "Ignore I/L diffs when consolidating"
        '
        'chkRemoveInvalidResidues
        '
        Me.chkRemoveInvalidResidues.Location = New System.Drawing.Point(8, 203)
        Me.chkRemoveInvalidResidues.Margin = New System.Windows.Forms.Padding(4)
        Me.chkRemoveInvalidResidues.Name = "chkRemoveInvalidResidues"
        Me.chkRemoveInvalidResidues.Size = New System.Drawing.Size(203, 20)
        Me.chkRemoveInvalidResidues.TabIndex = 8
        Me.chkRemoveInvalidResidues.Text = "Remove invalid residues"
        '
        'TabPageNewFastaOptions
        '
        Me.TabPageNewFastaOptions.Controls.Add(Me.txtResiduesPerLineForWrap)
        Me.TabPageNewFastaOptions.Controls.Add(Me.lblResiduesPerLineForWrap)
        Me.TabPageNewFastaOptions.Controls.Add(Me.chkKeepDuplicateNamedProteins)
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
        Me.TabPageNewFastaOptions.Location = New System.Drawing.Point(4, 25)
        Me.TabPageNewFastaOptions.Margin = New System.Windows.Forms.Padding(4)
        Me.TabPageNewFastaOptions.Name = "TabPageNewFastaOptions"
        Me.TabPageNewFastaOptions.Size = New System.Drawing.Size(537, 228)
        Me.TabPageNewFastaOptions.TabIndex = 1
        Me.TabPageNewFastaOptions.Text = "Fixed FASTA Options"
        '
        'txtResiduesPerLineForWrap
        '
        Me.txtResiduesPerLineForWrap.Location = New System.Drawing.Point(439, 56)
        Me.txtResiduesPerLineForWrap.Margin = New System.Windows.Forms.Padding(4)
        Me.txtResiduesPerLineForWrap.Name = "txtResiduesPerLineForWrap"
        Me.txtResiduesPerLineForWrap.Size = New System.Drawing.Size(52, 22)
        Me.txtResiduesPerLineForWrap.TabIndex = 15
        Me.txtResiduesPerLineForWrap.Text = "60"
        '
        'lblResiduesPerLineForWrap
        '
        Me.lblResiduesPerLineForWrap.Location = New System.Drawing.Point(239, 57)
        Me.lblResiduesPerLineForWrap.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblResiduesPerLineForWrap.Name = "lblResiduesPerLineForWrap"
        Me.lblResiduesPerLineForWrap.Size = New System.Drawing.Size(199, 21)
        Me.lblResiduesPerLineForWrap.TabIndex = 14
        Me.lblResiduesPerLineForWrap.Text = "Residues per line for wrap"
        '
        'chkKeepDuplicateNamedProteins
        '
        Me.chkKeepDuplicateNamedProteins.Location = New System.Drawing.Point(8, 158)
        Me.chkKeepDuplicateNamedProteins.Margin = New System.Windows.Forms.Padding(4)
        Me.chkKeepDuplicateNamedProteins.Name = "chkKeepDuplicateNamedProteins"
        Me.chkKeepDuplicateNamedProteins.Size = New System.Drawing.Size(171, 39)
        Me.chkKeepDuplicateNamedProteins.TabIndex = 13
        Me.chkKeepDuplicateNamedProteins.Text = "Retain duplicate named proteins"
        '
        'chkConsolidateDuplicateProteinSeqs
        '
        Me.chkConsolidateDuplicateProteinSeqs.Checked = True
        Me.chkConsolidateDuplicateProteinSeqs.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkConsolidateDuplicateProteinSeqs.Location = New System.Drawing.Point(239, 166)
        Me.chkConsolidateDuplicateProteinSeqs.Margin = New System.Windows.Forms.Padding(4)
        Me.chkConsolidateDuplicateProteinSeqs.Name = "chkConsolidateDuplicateProteinSeqs"
        Me.chkConsolidateDuplicateProteinSeqs.Size = New System.Drawing.Size(267, 22)
        Me.chkConsolidateDuplicateProteinSeqs.TabIndex = 11
        Me.chkConsolidateDuplicateProteinSeqs.Text = "Consolidate duplicate proteins"
        '
        'chkRenameDuplicateProteins
        '
        Me.chkRenameDuplicateProteins.Checked = True
        Me.chkRenameDuplicateProteins.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkRenameDuplicateProteins.Location = New System.Drawing.Point(8, 117)
        Me.chkRenameDuplicateProteins.Margin = New System.Windows.Forms.Padding(4)
        Me.chkRenameDuplicateProteins.Name = "chkRenameDuplicateProteins"
        Me.chkRenameDuplicateProteins.Size = New System.Drawing.Size(171, 40)
        Me.chkRenameDuplicateProteins.TabIndex = 6
        Me.chkRenameDuplicateProteins.Text = "Rename proteins with duplicate names"
        '
        'chkSplitOutMultipleRefsInProteinName
        '
        Me.chkSplitOutMultipleRefsInProteinName.Location = New System.Drawing.Point(239, 117)
        Me.chkSplitOutMultipleRefsInProteinName.Margin = New System.Windows.Forms.Padding(4)
        Me.chkSplitOutMultipleRefsInProteinName.Name = "chkSplitOutMultipleRefsInProteinName"
        Me.chkSplitOutMultipleRefsInProteinName.Size = New System.Drawing.Size(256, 40)
        Me.chkSplitOutMultipleRefsInProteinName.TabIndex = 10
        Me.chkSplitOutMultipleRefsInProteinName.Text = "Split out multiple references in protein names (always)"
        '
        'txtInvalidProteinNameCharsToRemove
        '
        Me.txtInvalidProteinNameCharsToRemove.Location = New System.Drawing.Point(439, 30)
        Me.txtInvalidProteinNameCharsToRemove.Margin = New System.Windows.Forms.Padding(4)
        Me.txtInvalidProteinNameCharsToRemove.Name = "txtInvalidProteinNameCharsToRemove"
        Me.txtInvalidProteinNameCharsToRemove.Size = New System.Drawing.Size(52, 22)
        Me.txtInvalidProteinNameCharsToRemove.TabIndex = 4
        '
        'lblInvalidProteinNameCharsToRemove
        '
        Me.lblInvalidProteinNameCharsToRemove.Location = New System.Drawing.Point(161, 32)
        Me.lblInvalidProteinNameCharsToRemove.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblInvalidProteinNameCharsToRemove.Name = "lblInvalidProteinNameCharsToRemove"
        Me.lblInvalidProteinNameCharsToRemove.Size = New System.Drawing.Size(277, 20)
        Me.lblInvalidProteinNameCharsToRemove.TabIndex = 3
        Me.lblInvalidProteinNameCharsToRemove.Text = "Invalid protein name chars to remove"
        '
        'txtLongProteinNameSplitChars
        '
        Me.txtLongProteinNameSplitChars.Location = New System.Drawing.Point(439, 2)
        Me.txtLongProteinNameSplitChars.Margin = New System.Windows.Forms.Padding(4)
        Me.txtLongProteinNameSplitChars.Name = "txtLongProteinNameSplitChars"
        Me.txtLongProteinNameSplitChars.Size = New System.Drawing.Size(52, 22)
        Me.txtLongProteinNameSplitChars.TabIndex = 2
        Me.txtLongProteinNameSplitChars.Text = "|"
        '
        'lblLongProteinNameSplitChars
        '
        Me.lblLongProteinNameSplitChars.Location = New System.Drawing.Point(161, 2)
        Me.lblLongProteinNameSplitChars.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLongProteinNameSplitChars.Name = "lblLongProteinNameSplitChars"
        Me.lblLongProteinNameSplitChars.Size = New System.Drawing.Size(277, 20)
        Me.lblLongProteinNameSplitChars.TabIndex = 1
        Me.lblLongProteinNameSplitChars.Text = "Char(s) to split long protein names on"
        '
        'chkGenerateFixedFastaFile
        '
        Me.chkGenerateFixedFastaFile.Location = New System.Drawing.Point(8, 2)
        Me.chkGenerateFixedFastaFile.Margin = New System.Windows.Forms.Padding(4)
        Me.chkGenerateFixedFastaFile.Name = "chkGenerateFixedFastaFile"
        Me.chkGenerateFixedFastaFile.Size = New System.Drawing.Size(145, 41)
        Me.chkGenerateFixedFastaFile.TabIndex = 0
        Me.chkGenerateFixedFastaFile.Text = "Generate fixed FASTA file"
        '
        'lblCustomRulesFile
        '
        Me.lblCustomRulesFile.Location = New System.Drawing.Point(5, 64)
        Me.lblCustomRulesFile.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblCustomRulesFile.Name = "lblCustomRulesFile"
        Me.lblCustomRulesFile.Size = New System.Drawing.Size(427, 18)
        Me.lblCustomRulesFile.TabIndex = 2
        Me.lblCustomRulesFile.Text = "Custom rules file (i.e. ValidateFastaFile parameter file)"
        '
        'TabPageRuleOptions
        '
        Me.TabPageRuleOptions.Controls.Add(Me.lblCustomRulesFile)
        Me.TabPageRuleOptions.Controls.Add(Me.cmdCreateDefaultValidationRulesFile)
        Me.TabPageRuleOptions.Controls.Add(Me.txtCustomValidationRulesFilePath)
        Me.TabPageRuleOptions.Controls.Add(Me.cmdSelectCustomRulesFile)
        Me.TabPageRuleOptions.Location = New System.Drawing.Point(4, 25)
        Me.TabPageRuleOptions.Margin = New System.Windows.Forms.Padding(4)
        Me.TabPageRuleOptions.Name = "TabPageRuleOptions"
        Me.TabPageRuleOptions.Size = New System.Drawing.Size(537, 228)
        Me.TabPageRuleOptions.TabIndex = 2
        Me.TabPageRuleOptions.Text = "Rule Options"
        '
        'cmdCreateDefaultValidationRulesFile
        '
        Me.cmdCreateDefaultValidationRulesFile.Location = New System.Drawing.Point(249, 10)
        Me.cmdCreateDefaultValidationRulesFile.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdCreateDefaultValidationRulesFile.Name = "cmdCreateDefaultValidationRulesFile"
        Me.cmdCreateDefaultValidationRulesFile.Size = New System.Drawing.Size(159, 45)
        Me.cmdCreateDefaultValidationRulesFile.TabIndex = 1
        Me.cmdCreateDefaultValidationRulesFile.Text = "Create XML file with default rules"
        '
        'txtCustomValidationRulesFilePath
        '
        Me.txtCustomValidationRulesFilePath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCustomValidationRulesFilePath.Location = New System.Drawing.Point(5, 93)
        Me.txtCustomValidationRulesFilePath.Margin = New System.Windows.Forms.Padding(4)
        Me.txtCustomValidationRulesFilePath.Name = "txtCustomValidationRulesFilePath"
        Me.txtCustomValidationRulesFilePath.Size = New System.Drawing.Size(511, 22)
        Me.txtCustomValidationRulesFilePath.TabIndex = 3
        '
        'cmdSelectCustomRulesFile
        '
        Me.cmdSelectCustomRulesFile.Location = New System.Drawing.Point(5, 10)
        Me.cmdSelectCustomRulesFile.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdSelectCustomRulesFile.Name = "cmdSelectCustomRulesFile"
        Me.cmdSelectCustomRulesFile.Size = New System.Drawing.Size(120, 45)
        Me.cmdSelectCustomRulesFile.TabIndex = 0
        Me.cmdSelectCustomRulesFile.Text = "Select Custom Rules File"
        '
        'tbsOptions
        '
        Me.tbsOptions.Controls.Add(Me.TabPageErrorOptions)
        Me.tbsOptions.Controls.Add(Me.TabPageNewFastaOptions)
        Me.tbsOptions.Controls.Add(Me.TabPageRuleOptions)
        Me.tbsOptions.Location = New System.Drawing.Point(7, 7)
        Me.tbsOptions.Margin = New System.Windows.Forms.Padding(4)
        Me.tbsOptions.Name = "tbsOptions"
        Me.tbsOptions.SelectedIndex = 0
        Me.tbsOptions.Size = New System.Drawing.Size(545, 257)
        Me.tbsOptions.TabIndex = 9
        '
        'mnuEditCopyAllWarnings
        '
        Me.mnuEditCopyAllWarnings.Index = 4
        Me.mnuEditCopyAllWarnings.Text = "Copy &Warnings"
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
        'dgWarnings
        '
        Me.dgWarnings.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgWarnings.DataMember = ""
        Me.dgWarnings.HeaderForeColor = System.Drawing.SystemColors.ControlText
        Me.dgWarnings.Location = New System.Drawing.Point(7, 428)
        Me.dgWarnings.Margin = New System.Windows.Forms.Padding(4)
        Me.dgWarnings.Name = "dgWarnings"
        Me.dgWarnings.Size = New System.Drawing.Size(1220, 128)
        Me.dgWarnings.TabIndex = 16
        '
        'lblFilterData
        '
        Me.lblFilterData.Location = New System.Drawing.Point(733, 218)
        Me.lblFilterData.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblFilterData.Name = "lblFilterData"
        Me.lblFilterData.Size = New System.Drawing.Size(85, 18)
        Me.lblFilterData.TabIndex = 13
        Me.lblFilterData.Text = "Filter data"
        '
        'txtFilterData
        '
        Me.txtFilterData.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtFilterData.Location = New System.Drawing.Point(829, 215)
        Me.txtFilterData.Margin = New System.Windows.Forms.Padding(4)
        Me.txtFilterData.Name = "txtFilterData"
        Me.txtFilterData.Size = New System.Drawing.Size(392, 22)
        Me.txtFilterData.TabIndex = 14
        '
        'pbarProgress
        '
        Me.pbarProgress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.pbarProgress.Location = New System.Drawing.Point(829, 215)
        Me.pbarProgress.Margin = New System.Windows.Forms.Padding(4)
        Me.pbarProgress.Name = "pbarProgress"
        Me.pbarProgress.Size = New System.Drawing.Size(393, 25)
        Me.pbarProgress.TabIndex = 17
        Me.pbarProgress.Visible = False
        '
        'cmdValidateFastaFile
        '
        Me.cmdValidateFastaFile.Location = New System.Drawing.Point(584, 213)
        Me.cmdValidateFastaFile.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdValidateFastaFile.Name = "cmdValidateFastaFile"
        Me.cmdValidateFastaFile.Size = New System.Drawing.Size(139, 30)
        Me.cmdValidateFastaFile.TabIndex = 11
        Me.cmdValidateFastaFile.Text = "&Start Validation"
        '
        'dgErrors
        '
        Me.dgErrors.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                  Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.dgErrors.DataMember = ""
        Me.dgErrors.HeaderForeColor = System.Drawing.SystemColors.ControlText
        Me.dgErrors.Location = New System.Drawing.Point(6, 272)
        Me.dgErrors.Margin = New System.Windows.Forms.Padding(4)
        Me.dgErrors.Name = "dgErrors"
        Me.dgErrors.Size = New System.Drawing.Size(1215, 148)
        Me.dgErrors.TabIndex = 15
        '
        'cmdCancel
        '
        Me.cmdCancel.Location = New System.Drawing.Point(584, 213)
        Me.cmdCancel.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(139, 30)
        Me.cmdCancel.TabIndex = 12
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.Visible = False
        '
        'frmFastaValidation
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1239, 583)
        Me.Controls.Add(Me.txtResults)
        Me.Controls.Add(Me.tbsOptions)
        Me.Controls.Add(Me.dgWarnings)
        Me.Controls.Add(Me.lblFilterData)
        Me.Controls.Add(Me.txtFilterData)
        Me.Controls.Add(Me.pbarProgress)
        Me.Controls.Add(Me.cmdValidateFastaFile)
        Me.Controls.Add(Me.dgErrors)
        Me.Controls.Add(Me.cmdCancel)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Menu = Me.MainMenuControl
        Me.Name = "frmFastaValidation"
        Me.Text = "FASTA File Validation"
        Me.TabPageErrorOptions.ResumeLayout(False)
        Me.TabPageErrorOptions.PerformLayout()
        Me.TabPageNewFastaOptions.ResumeLayout(False)
        Me.TabPageNewFastaOptions.PerformLayout()
        Me.TabPageRuleOptions.ResumeLayout(False)
        Me.TabPageRuleOptions.PerformLayout()
        Me.tbsOptions.ResumeLayout(False)
        CType(Me.dgWarnings, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgErrors, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents chkWrapLongResidueLines As System.Windows.Forms.CheckBox
    Friend WithEvents chkSplitOutMultipleRefsForKnownAccession As System.Windows.Forms.CheckBox
    Friend WithEvents chkTruncateLongProteinNames As System.Windows.Forms.CheckBox
    Friend WithEvents txtResults As System.Windows.Forms.TextBox
    Friend WithEvents TabPageErrorOptions As System.Windows.Forms.TabPage
    Friend WithEvents chkSaveBasicProteinHashInfoFile As System.Windows.Forms.CheckBox
    Friend WithEvents lblProteinNameLengthUnits As System.Windows.Forms.Label
    Friend WithEvents lblProteinNameLength2 As System.Windows.Forms.Label
    Friend WithEvents txtProteinNameLengthMaximum As System.Windows.Forms.TextBox
    Friend WithEvents txtProteinNameLengthMinimum As System.Windows.Forms.TextBox
    Friend WithEvents lblProteinNameLength As System.Windows.Forms.Label
    Friend WithEvents txtMaximumResiduesPerLine As System.Windows.Forms.TextBox
    Friend WithEvents lblMaximumResiduesPerLine As System.Windows.Forms.Label
    Friend WithEvents chkAllowAsteriskInResidues As System.Windows.Forms.CheckBox
    Friend WithEvents chkCheckForDuplicateProteinInfo As System.Windows.Forms.CheckBox
    Friend WithEvents txtMaxFileErrorsToTrack As System.Windows.Forms.TextBox
    Friend WithEvents lblMaxFileErrorsToTrack As System.Windows.Forms.Label
    Friend WithEvents chkLogResults As System.Windows.Forms.CheckBox
    Friend WithEvents chkConsolidateDupsIgnoreILDiff As System.Windows.Forms.CheckBox
    Friend WithEvents chkRemoveInvalidResidues As System.Windows.Forms.CheckBox
    Friend WithEvents TabPageNewFastaOptions As System.Windows.Forms.TabPage
    Friend WithEvents chkConsolidateDuplicateProteinSeqs As System.Windows.Forms.CheckBox
    Friend WithEvents chkRenameDuplicateProteins As System.Windows.Forms.CheckBox
    Friend WithEvents chkSplitOutMultipleRefsInProteinName As System.Windows.Forms.CheckBox
    Friend WithEvents txtInvalidProteinNameCharsToRemove As System.Windows.Forms.TextBox
    Friend WithEvents lblInvalidProteinNameCharsToRemove As System.Windows.Forms.Label
    Friend WithEvents txtLongProteinNameSplitChars As System.Windows.Forms.TextBox
    Friend WithEvents lblLongProteinNameSplitChars As System.Windows.Forms.Label
    Friend WithEvents chkGenerateFixedFastaFile As System.Windows.Forms.CheckBox
    Friend WithEvents lblCustomRulesFile As System.Windows.Forms.Label
    Friend WithEvents TabPageRuleOptions As System.Windows.Forms.TabPage
    Friend WithEvents cmdCreateDefaultValidationRulesFile As System.Windows.Forms.Button
    Friend WithEvents txtCustomValidationRulesFilePath As System.Windows.Forms.TextBox
    Friend WithEvents cmdSelectCustomRulesFile As System.Windows.Forms.Button
    Friend WithEvents tbsOptions As System.Windows.Forms.TabControl
    Friend WithEvents mnuEditCopyAllWarnings As System.Windows.Forms.MenuItem
    Friend WithEvents MainMenuControl As System.Windows.Forms.MainMenu
    Friend WithEvents mnuFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileExit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEdit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditCopyAllResults As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditSep1 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditCopySummary As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditCopyAllErrors As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditSep2 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditFontSizeDecrease As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditFontSizeIncrease As System.Windows.Forms.MenuItem
    Friend WithEvents MenuItem2 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditResetToDefaults As System.Windows.Forms.MenuItem
    Friend WithEvents MenuItem1 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelpAbout As System.Windows.Forms.MenuItem
    Friend WithEvents dgWarnings As System.Windows.Forms.DataGrid
    Friend WithEvents lblFilterData As System.Windows.Forms.Label
    Friend WithEvents txtFilterData As System.Windows.Forms.TextBox
    Friend WithEvents pbarProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents cmdValidateFastaFile As System.Windows.Forms.Button
    Friend WithEvents dgErrors As System.Windows.Forms.DataGrid
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
    Friend WithEvents chkKeepDuplicateNamedProteins As System.Windows.Forms.CheckBox
    Friend WithEvents txtResiduesPerLineForWrap As System.Windows.Forms.TextBox
    Friend WithEvents lblResiduesPerLineForWrap As System.Windows.Forms.Label
End Class
