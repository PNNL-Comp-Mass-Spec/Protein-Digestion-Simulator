Option Strict On

' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in October 2004
' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.

Public Class frmMain
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        InitializeControls()
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
    Friend WithEvents txtProteinInputFilePath As System.Windows.Forms.TextBox
    Friend WithEvents cmdSelectFile As System.Windows.Forms.Button
    Friend WithEvents fraInputFilePath As System.Windows.Forms.GroupBox
    Friend WithEvents fraInputOptions As System.Windows.Forms.GroupBox
    Friend WithEvents lblRefStartChar As System.Windows.Forms.Label
    Friend WithEvents txtRefStartChar As System.Windows.Forms.TextBox
    Friend WithEvents chkLookForAddnlRefInDescription As System.Windows.Forms.CheckBox
    Friend WithEvents lbltxtAddnlRefAccessionSepChar As System.Windows.Forms.Label
    Friend WithEvents txtAddnlRefSepChar As System.Windows.Forms.TextBox
    Friend WithEvents lblAddnlRefSepChar As System.Windows.Forms.Label
    Friend WithEvents txtAddnlRefAccessionSepChar As System.Windows.Forms.TextBox
    Friend WithEvents txtRefEndChar As System.Windows.Forms.TextBox
    Friend WithEvents cboInputFileFormat As System.Windows.Forms.ComboBox
    Friend WithEvents lblInputFileFormat As System.Windows.Forms.Label
    Friend WithEvents cboRefEndChar As System.Windows.Forms.ComboBox
    Friend WithEvents lblRefEndChar As System.Windows.Forms.Label
    Friend WithEvents chkComputeProteinMass As System.Windows.Forms.CheckBox
    Friend WithEvents chkIncludeXResidues As System.Windows.Forms.CheckBox
    Friend WithEvents lblInputFileColumnDelimiter As System.Windows.Forms.Label
    Friend WithEvents cboProteinReversalOptions As System.Windows.Forms.ComboBox
    Friend WithEvents lblProteinReversalOptions As System.Windows.Forms.Label
    Friend WithEvents fraDigestionOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkIncludeDuplicateSequences As System.Windows.Forms.CheckBox
    Friend WithEvents fraOutputTextOptions As System.Windows.Forms.GroupBox
    Friend WithEvents txtOutputFileFieldDelimeter As System.Windows.Forms.TextBox
    Friend WithEvents lblOutputFileFieldDelimiter As System.Windows.Forms.Label
    Friend WithEvents cboOutputFileFieldDelimeter As System.Windows.Forms.ComboBox
    Friend WithEvents chkIncludePrefixAndSuffixResidues As System.Windows.Forms.CheckBox
    Friend WithEvents cmdSelectOutputFile As System.Windows.Forms.Button
    Friend WithEvents cboCleavageRuleType As System.Windows.Forms.ComboBox
    Friend WithEvents txtDigestProteinsMinimumMass As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMinimumMass As System.Windows.Forms.Label
    Friend WithEvents txtDigestProteinsMaximumMass As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMaximumMass As System.Windows.Forms.Label
    Friend WithEvents txtDigestProteinsMaximumMissedCleavages As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMaximumMissedCleavages As System.Windows.Forms.Label
    Friend WithEvents lblDigestProteinsMinimumResidueCount As System.Windows.Forms.Label
    Friend WithEvents chkDigestProteins As System.Windows.Forms.CheckBox
    Friend WithEvents cmdParseInputFile As System.Windows.Forms.Button
    Friend WithEvents txtDigestProteinsMinimumResidueCount As System.Windows.Forms.TextBox
    Friend WithEvents txtProteinOutputFilePath As System.Windows.Forms.TextBox
    Friend WithEvents txtInputFileColumnDelimiter As System.Windows.Forms.TextBox
    Friend WithEvents cboInputFileColumnDelimiter As System.Windows.Forms.ComboBox
    Friend WithEvents mnuFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileSelectInputFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileSelectOutputFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileSep1 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileSep2 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileExit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEdit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditResetOptions As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelp As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelpAbout As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditParseFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileSaveDefaultOptions As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditSep1 As System.Windows.Forms.MenuItem
    Friend WithEvents MainMenuControl As System.Windows.Forms.MainMenu
    Friend WithEvents lblInputFileColumnOrdering As System.Windows.Forms.Label
    Friend WithEvents cboInputFileColumnOrdering As System.Windows.Forms.ComboBox
    Friend WithEvents fraDelimitedFileOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkCysPeptidesOnly As System.Windows.Forms.CheckBox
    Friend WithEvents cmdGenerateUniquenessStats As System.Windows.Forms.Button
    Friend WithEvents mnuEditMakeUniquenessStats As System.Windows.Forms.MenuItem
    Friend WithEvents tbsOptions As System.Windows.Forms.TabControl
    Friend WithEvents TabPageParseAndDigest As System.Windows.Forms.TabPage
    Friend WithEvents TabPageUniquenessStats As System.Windows.Forms.TabPage
    Friend WithEvents fraProcessingOptions As System.Windows.Forms.GroupBox
    Friend WithEvents TabPageFileFormatOptions As System.Windows.Forms.TabPage
    Friend WithEvents chkAssumeInputFileIsDigested As System.Windows.Forms.CheckBox
    Friend WithEvents lblUniquenessStatsNote As System.Windows.Forms.Label
    Friend WithEvents chkGenerateUniqueIDValues As System.Windows.Forms.CheckBox
    Friend WithEvents chkUseSLiCScoreForUniqueness As System.Windows.Forms.CheckBox
    Friend WithEvents chkExportPeakMatchingResults As System.Windows.Forms.CheckBox
    Friend WithEvents fraUniquenessBinningOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkAutoComputeRangeForBinning As System.Windows.Forms.CheckBox
    Friend WithEvents lblUniquenessBinEndMass As System.Windows.Forms.Label
    Friend WithEvents txtUniquenessBinStartMass As System.Windows.Forms.TextBox
    Friend WithEvents lblUniquenessBinStartMass As System.Windows.Forms.Label
    Friend WithEvents txtUniquenessBinWidth As System.Windows.Forms.TextBox
    Friend WithEvents lblUniquenessBinWidth As System.Windows.Forms.Label
    Friend WithEvents txtMinimumSLiCScore As System.Windows.Forms.TextBox
    Friend WithEvents lblMinimumSLiCScore As System.Windows.Forms.Label
    Friend WithEvents fraPeakMatchingOptions As System.Windows.Forms.GroupBox
    Friend WithEvents txtMaxPeakMatchingResultsPerFeatureToSave As System.Windows.Forms.TextBox
    Friend WithEvents lblMaxPeakMatchingResultsPerFeatureToSave As System.Windows.Forms.Label
    Friend WithEvents txtUniquenessBinEndMass As System.Windows.Forms.TextBox
    Friend WithEvents TabPagePeakMatchingThresholds As System.Windows.Forms.TabPage
    Friend WithEvents dgPeakMatchingThresholds As System.Windows.Forms.DataGrid
    Friend WithEvents cboMassTolType As System.Windows.Forms.ComboBox
    Friend WithEvents lblMassTolType As System.Windows.Forms.Label
    Friend WithEvents cmdClearPMThresholdsList As System.Windows.Forms.Button
    Friend WithEvents cmdPMThresholdsAutoPopulate As System.Windows.Forms.Button
    Friend WithEvents cboPMPredefinedThresholds As System.Windows.Forms.ComboBox
    Friend WithEvents cmdPastePMThresholdsList As System.Windows.Forms.Button
    Friend WithEvents chkAutoDefineSLiCScoreTolerances As System.Windows.Forms.CheckBox
    Friend WithEvents chkCreateFastaOutputFile As System.Windows.Forms.CheckBox
    Friend WithEvents lblProteinReversalSamplingPercentage As System.Windows.Forms.Label
    Friend WithEvents txtProteinReversalSamplingPercentage As System.Windows.Forms.TextBox
    Friend WithEvents lblSamplingPercentageUnits As System.Windows.Forms.Label
    Friend WithEvents txtSqlServerName As System.Windows.Forms.TextBox
    Friend WithEvents txtSqlServerDatabase As System.Windows.Forms.TextBox
    Friend WithEvents lblSqlServerServerName As System.Windows.Forms.Label
    Friend WithEvents lblSqlServerDatabase As System.Windows.Forms.Label
    Friend WithEvents fraSqlServerOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkSqlServerUseIntegratedSecurity As System.Windows.Forms.CheckBox
    Friend WithEvents txtSqlServerPassword As System.Windows.Forms.TextBox
    Friend WithEvents txtSqlServerUsername As System.Windows.Forms.TextBox
    Friend WithEvents lblSqlServerPassword As System.Windows.Forms.Label
    Friend WithEvents lblSqlServerUsername As System.Windows.Forms.Label
    Friend WithEvents chkUseSqlServerDBToCacheData As System.Windows.Forms.CheckBox
    Friend WithEvents chkEnableLogging As System.Windows.Forms.CheckBox
    Friend WithEvents chkAllowSqlServerCaching As System.Windows.Forms.CheckBox
    Friend WithEvents chkSqlServerUseExistingData As System.Windows.Forms.CheckBox
    Friend WithEvents lblUniquenessCalculationsNote As System.Windows.Forms.Label
    Friend WithEvents cmdValidateFastaFile As System.Windows.Forms.Button
    Friend WithEvents optUseEllipseSearchRegion As System.Windows.Forms.RadioButton
    Friend WithEvents optUseRectangleSearchRegion As System.Windows.Forms.RadioButton
    Friend WithEvents fraCalculationOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkComputepI As System.Windows.Forms.CheckBox
    Friend WithEvents txtSequenceForpI As System.Windows.Forms.TextBox
    Friend WithEvents txtpIStats As System.Windows.Forms.TextBox
    Friend WithEvents cboHydrophobicityMode As System.Windows.Forms.ComboBox
    Friend WithEvents lblHydrophobicityMode As System.Windows.Forms.Label
    Friend WithEvents lblMaxpISequenceLength As System.Windows.Forms.Label
    Friend WithEvents chkMaxpIModeEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents txtMaxpISequenceLength As System.Windows.Forms.TextBox
    Friend WithEvents lblSequenceForpI As System.Windows.Forms.Label
    Friend WithEvents frapIAndHydrophobicity As System.Windows.Forms.GroupBox
    Friend WithEvents mnuHelpAboutElutionTime As System.Windows.Forms.MenuItem
    Friend WithEvents txtDigestProteinsMaximumpI As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMaximumpI As System.Windows.Forms.Label
    Friend WithEvents txtDigestProteinsMinimumpI As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMinimumpI As System.Windows.Forms.Label
    Friend WithEvents chkExcludeProteinSequence As System.Windows.Forms.CheckBox
    Friend WithEvents lblProteinScramblingLoopCount As System.Windows.Forms.Label
    Friend WithEvents txtProteinScramblingLoopCount As System.Windows.Forms.TextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.txtProteinInputFilePath = New System.Windows.Forms.TextBox
        Me.cmdSelectFile = New System.Windows.Forms.Button
        Me.fraInputFilePath = New System.Windows.Forms.GroupBox
        Me.cmdValidateFastaFile = New System.Windows.Forms.Button
        Me.cboInputFileFormat = New System.Windows.Forms.ComboBox
        Me.lblInputFileFormat = New System.Windows.Forms.Label
        Me.cmdGenerateUniquenessStats = New System.Windows.Forms.Button
        Me.cmdParseInputFile = New System.Windows.Forms.Button
        Me.txtInputFileColumnDelimiter = New System.Windows.Forms.TextBox
        Me.lblInputFileColumnDelimiter = New System.Windows.Forms.Label
        Me.cboInputFileColumnDelimiter = New System.Windows.Forms.ComboBox
        Me.fraInputOptions = New System.Windows.Forms.GroupBox
        Me.txtRefEndChar = New System.Windows.Forms.TextBox
        Me.cboRefEndChar = New System.Windows.Forms.ComboBox
        Me.lblRefEndChar = New System.Windows.Forms.Label
        Me.txtRefStartChar = New System.Windows.Forms.TextBox
        Me.lblRefStartChar = New System.Windows.Forms.Label
        Me.lbltxtAddnlRefAccessionSepChar = New System.Windows.Forms.Label
        Me.lblAddnlRefSepChar = New System.Windows.Forms.Label
        Me.txtAddnlRefAccessionSepChar = New System.Windows.Forms.TextBox
        Me.txtAddnlRefSepChar = New System.Windows.Forms.TextBox
        Me.chkLookForAddnlRefInDescription = New System.Windows.Forms.CheckBox
        Me.fraCalculationOptions = New System.Windows.Forms.GroupBox
        Me.chkExcludeProteinSequence = New System.Windows.Forms.CheckBox
        Me.chkComputepI = New System.Windows.Forms.CheckBox
        Me.chkIncludeXResidues = New System.Windows.Forms.CheckBox
        Me.chkComputeProteinMass = New System.Windows.Forms.CheckBox
        Me.cboProteinReversalOptions = New System.Windows.Forms.ComboBox
        Me.lblProteinReversalOptions = New System.Windows.Forms.Label
        Me.chkDigestProteins = New System.Windows.Forms.CheckBox
        Me.fraDigestionOptions = New System.Windows.Forms.GroupBox
        Me.txtDigestProteinsMaximumpI = New System.Windows.Forms.TextBox
        Me.lblDigestProteinsMaximumpI = New System.Windows.Forms.Label
        Me.txtDigestProteinsMinimumpI = New System.Windows.Forms.TextBox
        Me.lblDigestProteinsMinimumpI = New System.Windows.Forms.Label
        Me.chkGenerateUniqueIDValues = New System.Windows.Forms.CheckBox
        Me.chkCysPeptidesOnly = New System.Windows.Forms.CheckBox
        Me.txtDigestProteinsMinimumResidueCount = New System.Windows.Forms.TextBox
        Me.lblDigestProteinsMinimumResidueCount = New System.Windows.Forms.Label
        Me.txtDigestProteinsMaximumMissedCleavages = New System.Windows.Forms.TextBox
        Me.lblDigestProteinsMaximumMissedCleavages = New System.Windows.Forms.Label
        Me.txtDigestProteinsMaximumMass = New System.Windows.Forms.TextBox
        Me.lblDigestProteinsMaximumMass = New System.Windows.Forms.Label
        Me.txtDigestProteinsMinimumMass = New System.Windows.Forms.TextBox
        Me.lblDigestProteinsMinimumMass = New System.Windows.Forms.Label
        Me.cboCleavageRuleType = New System.Windows.Forms.ComboBox
        Me.chkIncludeDuplicateSequences = New System.Windows.Forms.CheckBox
        Me.fraOutputTextOptions = New System.Windows.Forms.GroupBox
        Me.chkEnableLogging = New System.Windows.Forms.CheckBox
        Me.cmdSelectOutputFile = New System.Windows.Forms.Button
        Me.txtProteinOutputFilePath = New System.Windows.Forms.TextBox
        Me.chkIncludePrefixAndSuffixResidues = New System.Windows.Forms.CheckBox
        Me.cboOutputFileFieldDelimeter = New System.Windows.Forms.ComboBox
        Me.txtOutputFileFieldDelimeter = New System.Windows.Forms.TextBox
        Me.lblOutputFileFieldDelimiter = New System.Windows.Forms.Label
        Me.MainMenuControl = New System.Windows.Forms.MainMenu
        Me.mnuFile = New System.Windows.Forms.MenuItem
        Me.mnuFileSelectInputFile = New System.Windows.Forms.MenuItem
        Me.mnuFileSelectOutputFile = New System.Windows.Forms.MenuItem
        Me.mnuFileSep1 = New System.Windows.Forms.MenuItem
        Me.mnuFileSaveDefaultOptions = New System.Windows.Forms.MenuItem
        Me.mnuFileSep2 = New System.Windows.Forms.MenuItem
        Me.mnuFileExit = New System.Windows.Forms.MenuItem
        Me.mnuEdit = New System.Windows.Forms.MenuItem
        Me.mnuEditParseFile = New System.Windows.Forms.MenuItem
        Me.mnuEditMakeUniquenessStats = New System.Windows.Forms.MenuItem
        Me.mnuEditSep1 = New System.Windows.Forms.MenuItem
        Me.mnuEditResetOptions = New System.Windows.Forms.MenuItem
        Me.mnuHelp = New System.Windows.Forms.MenuItem
        Me.mnuHelpAbout = New System.Windows.Forms.MenuItem
        Me.mnuHelpAboutElutionTime = New System.Windows.Forms.MenuItem
        Me.lblInputFileColumnOrdering = New System.Windows.Forms.Label
        Me.cboInputFileColumnOrdering = New System.Windows.Forms.ComboBox
        Me.fraDelimitedFileOptions = New System.Windows.Forms.GroupBox
        Me.tbsOptions = New System.Windows.Forms.TabControl
        Me.TabPageFileFormatOptions = New System.Windows.Forms.TabPage
        Me.frapIAndHydrophobicity = New System.Windows.Forms.GroupBox
        Me.txtMaxpISequenceLength = New System.Windows.Forms.TextBox
        Me.lblMaxpISequenceLength = New System.Windows.Forms.Label
        Me.chkMaxpIModeEnabled = New System.Windows.Forms.CheckBox
        Me.lblHydrophobicityMode = New System.Windows.Forms.Label
        Me.cboHydrophobicityMode = New System.Windows.Forms.ComboBox
        Me.txtpIStats = New System.Windows.Forms.TextBox
        Me.txtSequenceForpI = New System.Windows.Forms.TextBox
        Me.lblSequenceForpI = New System.Windows.Forms.Label
        Me.TabPageParseAndDigest = New System.Windows.Forms.TabPage
        Me.fraProcessingOptions = New System.Windows.Forms.GroupBox
        Me.lblSamplingPercentageUnits = New System.Windows.Forms.Label
        Me.lblProteinReversalSamplingPercentage = New System.Windows.Forms.Label
        Me.txtProteinReversalSamplingPercentage = New System.Windows.Forms.TextBox
        Me.chkCreateFastaOutputFile = New System.Windows.Forms.CheckBox
        Me.TabPageUniquenessStats = New System.Windows.Forms.TabPage
        Me.lblUniquenessCalculationsNote = New System.Windows.Forms.Label
        Me.fraPeakMatchingOptions = New System.Windows.Forms.GroupBox
        Me.optUseRectangleSearchRegion = New System.Windows.Forms.RadioButton
        Me.optUseEllipseSearchRegion = New System.Windows.Forms.RadioButton
        Me.txtMaxPeakMatchingResultsPerFeatureToSave = New System.Windows.Forms.TextBox
        Me.lblMaxPeakMatchingResultsPerFeatureToSave = New System.Windows.Forms.Label
        Me.chkExportPeakMatchingResults = New System.Windows.Forms.CheckBox
        Me.txtMinimumSLiCScore = New System.Windows.Forms.TextBox
        Me.lblMinimumSLiCScore = New System.Windows.Forms.Label
        Me.chkUseSLiCScoreForUniqueness = New System.Windows.Forms.CheckBox
        Me.fraSqlServerOptions = New System.Windows.Forms.GroupBox
        Me.chkSqlServerUseExistingData = New System.Windows.Forms.CheckBox
        Me.chkAllowSqlServerCaching = New System.Windows.Forms.CheckBox
        Me.lblSqlServerPassword = New System.Windows.Forms.Label
        Me.lblSqlServerUsername = New System.Windows.Forms.Label
        Me.txtSqlServerPassword = New System.Windows.Forms.TextBox
        Me.txtSqlServerUsername = New System.Windows.Forms.TextBox
        Me.lblSqlServerDatabase = New System.Windows.Forms.Label
        Me.lblSqlServerServerName = New System.Windows.Forms.Label
        Me.chkSqlServerUseIntegratedSecurity = New System.Windows.Forms.CheckBox
        Me.txtSqlServerDatabase = New System.Windows.Forms.TextBox
        Me.txtSqlServerName = New System.Windows.Forms.TextBox
        Me.chkUseSqlServerDBToCacheData = New System.Windows.Forms.CheckBox
        Me.fraUniquenessBinningOptions = New System.Windows.Forms.GroupBox
        Me.txtUniquenessBinWidth = New System.Windows.Forms.TextBox
        Me.lblUniquenessBinWidth = New System.Windows.Forms.Label
        Me.chkAutoComputeRangeForBinning = New System.Windows.Forms.CheckBox
        Me.txtUniquenessBinEndMass = New System.Windows.Forms.TextBox
        Me.lblUniquenessBinEndMass = New System.Windows.Forms.Label
        Me.txtUniquenessBinStartMass = New System.Windows.Forms.TextBox
        Me.lblUniquenessBinStartMass = New System.Windows.Forms.Label
        Me.lblUniquenessStatsNote = New System.Windows.Forms.Label
        Me.chkAssumeInputFileIsDigested = New System.Windows.Forms.CheckBox
        Me.TabPagePeakMatchingThresholds = New System.Windows.Forms.TabPage
        Me.chkAutoDefineSLiCScoreTolerances = New System.Windows.Forms.CheckBox
        Me.cmdPastePMThresholdsList = New System.Windows.Forms.Button
        Me.cboPMPredefinedThresholds = New System.Windows.Forms.ComboBox
        Me.cmdPMThresholdsAutoPopulate = New System.Windows.Forms.Button
        Me.cmdClearPMThresholdsList = New System.Windows.Forms.Button
        Me.cboMassTolType = New System.Windows.Forms.ComboBox
        Me.lblMassTolType = New System.Windows.Forms.Label
        Me.dgPeakMatchingThresholds = New System.Windows.Forms.DataGrid
        Me.lblProteinScramblingLoopCount = New System.Windows.Forms.Label
        Me.txtProteinScramblingLoopCount = New System.Windows.Forms.TextBox
        Me.fraInputFilePath.SuspendLayout()
        Me.fraInputOptions.SuspendLayout()
        Me.fraCalculationOptions.SuspendLayout()
        Me.fraDigestionOptions.SuspendLayout()
        Me.fraOutputTextOptions.SuspendLayout()
        Me.fraDelimitedFileOptions.SuspendLayout()
        Me.tbsOptions.SuspendLayout()
        Me.TabPageFileFormatOptions.SuspendLayout()
        Me.frapIAndHydrophobicity.SuspendLayout()
        Me.TabPageParseAndDigest.SuspendLayout()
        Me.fraProcessingOptions.SuspendLayout()
        Me.TabPageUniquenessStats.SuspendLayout()
        Me.fraPeakMatchingOptions.SuspendLayout()
        Me.fraSqlServerOptions.SuspendLayout()
        Me.fraUniquenessBinningOptions.SuspendLayout()
        Me.TabPagePeakMatchingThresholds.SuspendLayout()
        CType(Me.dgPeakMatchingThresholds, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'txtProteinInputFilePath
        '
        Me.txtProteinInputFilePath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtProteinInputFilePath.Location = New System.Drawing.Point(104, 26)
        Me.txtProteinInputFilePath.Name = "txtProteinInputFilePath"
        Me.txtProteinInputFilePath.Size = New System.Drawing.Size(601, 20)
        Me.txtProteinInputFilePath.TabIndex = 1
        Me.txtProteinInputFilePath.Text = ""
        '
        'cmdSelectFile
        '
        Me.cmdSelectFile.Location = New System.Drawing.Point(8, 24)
        Me.cmdSelectFile.Name = "cmdSelectFile"
        Me.cmdSelectFile.Size = New System.Drawing.Size(80, 24)
        Me.cmdSelectFile.TabIndex = 0
        Me.cmdSelectFile.Text = "&Select file"
        '
        'fraInputFilePath
        '
        Me.fraInputFilePath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.fraInputFilePath.Controls.Add(Me.cmdValidateFastaFile)
        Me.fraInputFilePath.Controls.Add(Me.cboInputFileFormat)
        Me.fraInputFilePath.Controls.Add(Me.lblInputFileFormat)
        Me.fraInputFilePath.Controls.Add(Me.cmdSelectFile)
        Me.fraInputFilePath.Controls.Add(Me.txtProteinInputFilePath)
        Me.fraInputFilePath.Location = New System.Drawing.Point(8, 8)
        Me.fraInputFilePath.Name = "fraInputFilePath"
        Me.fraInputFilePath.Size = New System.Drawing.Size(721, 88)
        Me.fraInputFilePath.TabIndex = 0
        Me.fraInputFilePath.TabStop = False
        Me.fraInputFilePath.Text = "Protein Input File Path (Fasta or Tab-delimited)"
        '
        'cmdValidateFastaFile
        '
        Me.cmdValidateFastaFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdValidateFastaFile.Location = New System.Drawing.Point(585, 56)
        Me.cmdValidateFastaFile.Name = "cmdValidateFastaFile"
        Me.cmdValidateFastaFile.Size = New System.Drawing.Size(120, 24)
        Me.cmdValidateFastaFile.TabIndex = 4
        Me.cmdValidateFastaFile.Text = "&Validate Fasta File"
        '
        'cboInputFileFormat
        '
        Me.cboInputFileFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboInputFileFormat.Location = New System.Drawing.Point(112, 56)
        Me.cboInputFileFormat.Name = "cboInputFileFormat"
        Me.cboInputFileFormat.Size = New System.Drawing.Size(112, 21)
        Me.cboInputFileFormat.TabIndex = 3
        '
        'lblInputFileFormat
        '
        Me.lblInputFileFormat.Location = New System.Drawing.Point(8, 58)
        Me.lblInputFileFormat.Name = "lblInputFileFormat"
        Me.lblInputFileFormat.Size = New System.Drawing.Size(104, 16)
        Me.lblInputFileFormat.TabIndex = 2
        Me.lblInputFileFormat.Text = "Input File Format"
        '
        'cmdGenerateUniquenessStats
        '
        Me.cmdGenerateUniquenessStats.Location = New System.Drawing.Point(232, 16)
        Me.cmdGenerateUniquenessStats.Name = "cmdGenerateUniquenessStats"
        Me.cmdGenerateUniquenessStats.Size = New System.Drawing.Size(176, 24)
        Me.cmdGenerateUniquenessStats.TabIndex = 5
        Me.cmdGenerateUniquenessStats.Text = "&Generate Uniqueness Stats"
        '
        'cmdParseInputFile
        '
        Me.cmdParseInputFile.Location = New System.Drawing.Point(384, 8)
        Me.cmdParseInputFile.Name = "cmdParseInputFile"
        Me.cmdParseInputFile.Size = New System.Drawing.Size(112, 24)
        Me.cmdParseInputFile.TabIndex = 3
        Me.cmdParseInputFile.Text = "&Parse and Digest"
        '
        'txtInputFileColumnDelimiter
        '
        Me.txtInputFileColumnDelimiter.Location = New System.Drawing.Point(192, 56)
        Me.txtInputFileColumnDelimiter.MaxLength = 1
        Me.txtInputFileColumnDelimiter.Name = "txtInputFileColumnDelimiter"
        Me.txtInputFileColumnDelimiter.Size = New System.Drawing.Size(32, 20)
        Me.txtInputFileColumnDelimiter.TabIndex = 4
        Me.txtInputFileColumnDelimiter.Text = ";"
        '
        'lblInputFileColumnDelimiter
        '
        Me.lblInputFileColumnDelimiter.Location = New System.Drawing.Point(8, 58)
        Me.lblInputFileColumnDelimiter.Name = "lblInputFileColumnDelimiter"
        Me.lblInputFileColumnDelimiter.Size = New System.Drawing.Size(96, 16)
        Me.lblInputFileColumnDelimiter.TabIndex = 2
        Me.lblInputFileColumnDelimiter.Text = "Column Delimiter"
        '
        'cboInputFileColumnDelimiter
        '
        Me.cboInputFileColumnDelimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboInputFileColumnDelimiter.DropDownWidth = 70
        Me.cboInputFileColumnDelimiter.Location = New System.Drawing.Point(112, 56)
        Me.cboInputFileColumnDelimiter.Name = "cboInputFileColumnDelimiter"
        Me.cboInputFileColumnDelimiter.Size = New System.Drawing.Size(70, 21)
        Me.cboInputFileColumnDelimiter.TabIndex = 3
        '
        'fraInputOptions
        '
        Me.fraInputOptions.Controls.Add(Me.txtRefEndChar)
        Me.fraInputOptions.Controls.Add(Me.cboRefEndChar)
        Me.fraInputOptions.Controls.Add(Me.lblRefEndChar)
        Me.fraInputOptions.Controls.Add(Me.txtRefStartChar)
        Me.fraInputOptions.Controls.Add(Me.lblRefStartChar)
        Me.fraInputOptions.Location = New System.Drawing.Point(8, 8)
        Me.fraInputOptions.Name = "fraInputOptions"
        Me.fraInputOptions.Size = New System.Drawing.Size(288, 88)
        Me.fraInputOptions.TabIndex = 0
        Me.fraInputOptions.TabStop = False
        Me.fraInputOptions.Text = "Fasta File Input Options"
        '
        'txtRefEndChar
        '
        Me.txtRefEndChar.Location = New System.Drawing.Point(208, 48)
        Me.txtRefEndChar.MaxLength = 1
        Me.txtRefEndChar.Name = "txtRefEndChar"
        Me.txtRefEndChar.Size = New System.Drawing.Size(32, 20)
        Me.txtRefEndChar.TabIndex = 4
        Me.txtRefEndChar.Text = "|"
        '
        'cboRefEndChar
        '
        Me.cboRefEndChar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboRefEndChar.Location = New System.Drawing.Point(128, 48)
        Me.cboRefEndChar.Name = "cboRefEndChar"
        Me.cboRefEndChar.Size = New System.Drawing.Size(70, 21)
        Me.cboRefEndChar.TabIndex = 3
        '
        'lblRefEndChar
        '
        Me.lblRefEndChar.Location = New System.Drawing.Point(8, 50)
        Me.lblRefEndChar.Name = "lblRefEndChar"
        Me.lblRefEndChar.Size = New System.Drawing.Size(112, 16)
        Me.lblRefEndChar.TabIndex = 2
        Me.lblRefEndChar.Text = "Fasta Ref End Char"
        '
        'txtRefStartChar
        '
        Me.txtRefStartChar.Location = New System.Drawing.Point(128, 24)
        Me.txtRefStartChar.MaxLength = 1
        Me.txtRefStartChar.Name = "txtRefStartChar"
        Me.txtRefStartChar.Size = New System.Drawing.Size(32, 20)
        Me.txtRefStartChar.TabIndex = 1
        Me.txtRefStartChar.Text = ">"
        '
        'lblRefStartChar
        '
        Me.lblRefStartChar.Location = New System.Drawing.Point(8, 26)
        Me.lblRefStartChar.Name = "lblRefStartChar"
        Me.lblRefStartChar.Size = New System.Drawing.Size(112, 16)
        Me.lblRefStartChar.TabIndex = 0
        Me.lblRefStartChar.Text = "Fasta Ref Start Char"
        '
        'lbltxtAddnlRefAccessionSepChar
        '
        Me.lbltxtAddnlRefAccessionSepChar.Location = New System.Drawing.Point(96, 96)
        Me.lbltxtAddnlRefAccessionSepChar.Name = "lbltxtAddnlRefAccessionSepChar"
        Me.lbltxtAddnlRefAccessionSepChar.Size = New System.Drawing.Size(160, 16)
        Me.lbltxtAddnlRefAccessionSepChar.TabIndex = 8
        Me.lbltxtAddnlRefAccessionSepChar.Text = "Addnl Ref Accession Sep Char"
        Me.lbltxtAddnlRefAccessionSepChar.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'lblAddnlRefSepChar
        '
        Me.lblAddnlRefSepChar.Location = New System.Drawing.Point(144, 72)
        Me.lblAddnlRefSepChar.Name = "lblAddnlRefSepChar"
        Me.lblAddnlRefSepChar.Size = New System.Drawing.Size(112, 16)
        Me.lblAddnlRefSepChar.TabIndex = 6
        Me.lblAddnlRefSepChar.Text = "Addnl Ref Sep Char"
        Me.lblAddnlRefSepChar.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtAddnlRefAccessionSepChar
        '
        Me.txtAddnlRefAccessionSepChar.Location = New System.Drawing.Point(264, 96)
        Me.txtAddnlRefAccessionSepChar.MaxLength = 1
        Me.txtAddnlRefAccessionSepChar.Name = "txtAddnlRefAccessionSepChar"
        Me.txtAddnlRefAccessionSepChar.Size = New System.Drawing.Size(32, 20)
        Me.txtAddnlRefAccessionSepChar.TabIndex = 9
        Me.txtAddnlRefAccessionSepChar.Text = ":"
        '
        'txtAddnlRefSepChar
        '
        Me.txtAddnlRefSepChar.Location = New System.Drawing.Point(264, 72)
        Me.txtAddnlRefSepChar.MaxLength = 1
        Me.txtAddnlRefSepChar.Name = "txtAddnlRefSepChar"
        Me.txtAddnlRefSepChar.Size = New System.Drawing.Size(32, 20)
        Me.txtAddnlRefSepChar.TabIndex = 7
        Me.txtAddnlRefSepChar.Text = "|"
        '
        'chkLookForAddnlRefInDescription
        '
        Me.chkLookForAddnlRefInDescription.Location = New System.Drawing.Point(16, 72)
        Me.chkLookForAddnlRefInDescription.Name = "chkLookForAddnlRefInDescription"
        Me.chkLookForAddnlRefInDescription.Size = New System.Drawing.Size(120, 32)
        Me.chkLookForAddnlRefInDescription.TabIndex = 5
        Me.chkLookForAddnlRefInDescription.Text = "Look for addnl Ref in description"
        '
        'fraCalculationOptions
        '
        Me.fraCalculationOptions.Controls.Add(Me.chkExcludeProteinSequence)
        Me.fraCalculationOptions.Controls.Add(Me.chkComputepI)
        Me.fraCalculationOptions.Controls.Add(Me.chkIncludeXResidues)
        Me.fraCalculationOptions.Controls.Add(Me.chkComputeProteinMass)
        Me.fraCalculationOptions.Location = New System.Drawing.Point(376, 56)
        Me.fraCalculationOptions.Name = "fraCalculationOptions"
        Me.fraCalculationOptions.Size = New System.Drawing.Size(248, 104)
        Me.fraCalculationOptions.TabIndex = 1
        Me.fraCalculationOptions.TabStop = False
        Me.fraCalculationOptions.Text = "Calculation Options"
        '
        'chkExcludeProteinSequence
        '
        Me.chkExcludeProteinSequence.Location = New System.Drawing.Point(16, 16)
        Me.chkExcludeProteinSequence.Name = "chkExcludeProteinSequence"
        Me.chkExcludeProteinSequence.Size = New System.Drawing.Size(192, 16)
        Me.chkExcludeProteinSequence.TabIndex = 0
        Me.chkExcludeProteinSequence.Text = "Exclude Protein Sequence"
        '
        'chkComputepI
        '
        Me.chkComputepI.Checked = True
        Me.chkComputepI.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkComputepI.Location = New System.Drawing.Point(16, 80)
        Me.chkComputepI.Name = "chkComputepI"
        Me.chkComputepI.Size = New System.Drawing.Size(216, 16)
        Me.chkComputepI.TabIndex = 3
        Me.chkComputepI.Text = "Compute Isoelectric Point (pI)"
        '
        'chkIncludeXResidues
        '
        Me.chkIncludeXResidues.Checked = True
        Me.chkIncludeXResidues.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkIncludeXResidues.Location = New System.Drawing.Point(16, 56)
        Me.chkIncludeXResidues.Name = "chkIncludeXResidues"
        Me.chkIncludeXResidues.Size = New System.Drawing.Size(216, 16)
        Me.chkIncludeXResidues.TabIndex = 2
        Me.chkIncludeXResidues.Text = "Include X Residues in Mass (113 Da)"
        '
        'chkComputeProteinMass
        '
        Me.chkComputeProteinMass.Checked = True
        Me.chkComputeProteinMass.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkComputeProteinMass.Location = New System.Drawing.Point(16, 40)
        Me.chkComputeProteinMass.Name = "chkComputeProteinMass"
        Me.chkComputeProteinMass.Size = New System.Drawing.Size(144, 16)
        Me.chkComputeProteinMass.TabIndex = 1
        Me.chkComputeProteinMass.Text = "Compute Protein Mass"
        '
        'cboProteinReversalOptions
        '
        Me.cboProteinReversalOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboProteinReversalOptions.Location = New System.Drawing.Point(168, 16)
        Me.cboProteinReversalOptions.Name = "cboProteinReversalOptions"
        Me.cboProteinReversalOptions.Size = New System.Drawing.Size(184, 21)
        Me.cboProteinReversalOptions.TabIndex = 1
        '
        'lblProteinReversalOptions
        '
        Me.lblProteinReversalOptions.Location = New System.Drawing.Point(16, 20)
        Me.lblProteinReversalOptions.Name = "lblProteinReversalOptions"
        Me.lblProteinReversalOptions.Size = New System.Drawing.Size(160, 16)
        Me.lblProteinReversalOptions.TabIndex = 0
        Me.lblProteinReversalOptions.Text = "Protein Reversal / Scrambling"
        '
        'chkDigestProteins
        '
        Me.chkDigestProteins.Checked = True
        Me.chkDigestProteins.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkDigestProteins.Location = New System.Drawing.Point(16, 115)
        Me.chkDigestProteins.Name = "chkDigestProteins"
        Me.chkDigestProteins.Size = New System.Drawing.Size(160, 32)
        Me.chkDigestProteins.TabIndex = 10
        Me.chkDigestProteins.Text = "In Silico digest of all proteins in input file"
        '
        'fraDigestionOptions
        '
        Me.fraDigestionOptions.Controls.Add(Me.txtDigestProteinsMaximumpI)
        Me.fraDigestionOptions.Controls.Add(Me.lblDigestProteinsMaximumpI)
        Me.fraDigestionOptions.Controls.Add(Me.txtDigestProteinsMinimumpI)
        Me.fraDigestionOptions.Controls.Add(Me.lblDigestProteinsMinimumpI)
        Me.fraDigestionOptions.Controls.Add(Me.chkGenerateUniqueIDValues)
        Me.fraDigestionOptions.Controls.Add(Me.chkCysPeptidesOnly)
        Me.fraDigestionOptions.Controls.Add(Me.txtDigestProteinsMinimumResidueCount)
        Me.fraDigestionOptions.Controls.Add(Me.lblDigestProteinsMinimumResidueCount)
        Me.fraDigestionOptions.Controls.Add(Me.txtDigestProteinsMaximumMissedCleavages)
        Me.fraDigestionOptions.Controls.Add(Me.lblDigestProteinsMaximumMissedCleavages)
        Me.fraDigestionOptions.Controls.Add(Me.txtDigestProteinsMaximumMass)
        Me.fraDigestionOptions.Controls.Add(Me.lblDigestProteinsMaximumMass)
        Me.fraDigestionOptions.Controls.Add(Me.txtDigestProteinsMinimumMass)
        Me.fraDigestionOptions.Controls.Add(Me.lblDigestProteinsMinimumMass)
        Me.fraDigestionOptions.Controls.Add(Me.cboCleavageRuleType)
        Me.fraDigestionOptions.Controls.Add(Me.chkIncludeDuplicateSequences)
        Me.fraDigestionOptions.Location = New System.Drawing.Point(8, 168)
        Me.fraDigestionOptions.Name = "fraDigestionOptions"
        Me.fraDigestionOptions.Size = New System.Drawing.Size(616, 128)
        Me.fraDigestionOptions.TabIndex = 2
        Me.fraDigestionOptions.TabStop = False
        Me.fraDigestionOptions.Text = "Digestion Options"
        '
        'txtDigestProteinsMaximumpI
        '
        Me.txtDigestProteinsMaximumpI.Location = New System.Drawing.Point(520, 80)
        Me.txtDigestProteinsMaximumpI.Name = "txtDigestProteinsMaximumpI"
        Me.txtDigestProteinsMaximumpI.Size = New System.Drawing.Size(40, 20)
        Me.txtDigestProteinsMaximumpI.TabIndex = 13
        Me.txtDigestProteinsMaximumpI.Text = "14"
        '
        'lblDigestProteinsMaximumpI
        '
        Me.lblDigestProteinsMaximumpI.Location = New System.Drawing.Point(440, 80)
        Me.lblDigestProteinsMaximumpI.Name = "lblDigestProteinsMaximumpI"
        Me.lblDigestProteinsMaximumpI.Size = New System.Drawing.Size(72, 16)
        Me.lblDigestProteinsMaximumpI.TabIndex = 12
        Me.lblDigestProteinsMaximumpI.Text = "Maximum pI"
        '
        'txtDigestProteinsMinimumpI
        '
        Me.txtDigestProteinsMinimumpI.Location = New System.Drawing.Point(520, 56)
        Me.txtDigestProteinsMinimumpI.Name = "txtDigestProteinsMinimumpI"
        Me.txtDigestProteinsMinimumpI.Size = New System.Drawing.Size(40, 20)
        Me.txtDigestProteinsMinimumpI.TabIndex = 11
        Me.txtDigestProteinsMinimumpI.Text = "0"
        '
        'lblDigestProteinsMinimumpI
        '
        Me.lblDigestProteinsMinimumpI.Location = New System.Drawing.Point(440, 56)
        Me.lblDigestProteinsMinimumpI.Name = "lblDigestProteinsMinimumpI"
        Me.lblDigestProteinsMinimumpI.Size = New System.Drawing.Size(72, 16)
        Me.lblDigestProteinsMinimumpI.TabIndex = 10
        Me.lblDigestProteinsMinimumpI.Text = "Minimum pI"
        '
        'chkGenerateUniqueIDValues
        '
        Me.chkGenerateUniqueIDValues.Checked = True
        Me.chkGenerateUniqueIDValues.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkGenerateUniqueIDValues.Location = New System.Drawing.Point(8, 104)
        Me.chkGenerateUniqueIDValues.Name = "chkGenerateUniqueIDValues"
        Me.chkGenerateUniqueIDValues.Size = New System.Drawing.Size(176, 16)
        Me.chkGenerateUniqueIDValues.TabIndex = 14
        Me.chkGenerateUniqueIDValues.Text = "Generate UniqueID Values"
        '
        'chkCysPeptidesOnly
        '
        Me.chkCysPeptidesOnly.Location = New System.Drawing.Point(424, 24)
        Me.chkCysPeptidesOnly.Name = "chkCysPeptidesOnly"
        Me.chkCysPeptidesOnly.Size = New System.Drawing.Size(176, 16)
        Me.chkCysPeptidesOnly.TabIndex = 15
        Me.chkCysPeptidesOnly.Text = "Include cysteine peptides only"
        '
        'txtDigestProteinsMinimumResidueCount
        '
        Me.txtDigestProteinsMinimumResidueCount.Location = New System.Drawing.Point(368, 56)
        Me.txtDigestProteinsMinimumResidueCount.Name = "txtDigestProteinsMinimumResidueCount"
        Me.txtDigestProteinsMinimumResidueCount.Size = New System.Drawing.Size(32, 20)
        Me.txtDigestProteinsMinimumResidueCount.TabIndex = 7
        Me.txtDigestProteinsMinimumResidueCount.Text = "0"
        '
        'lblDigestProteinsMinimumResidueCount
        '
        Me.lblDigestProteinsMinimumResidueCount.Location = New System.Drawing.Point(216, 58)
        Me.lblDigestProteinsMinimumResidueCount.Name = "lblDigestProteinsMinimumResidueCount"
        Me.lblDigestProteinsMinimumResidueCount.Size = New System.Drawing.Size(136, 16)
        Me.lblDigestProteinsMinimumResidueCount.TabIndex = 6
        Me.lblDigestProteinsMinimumResidueCount.Text = "Minimum Residue Count"
        '
        'txtDigestProteinsMaximumMissedCleavages
        '
        Me.txtDigestProteinsMaximumMissedCleavages.Location = New System.Drawing.Point(368, 80)
        Me.txtDigestProteinsMaximumMissedCleavages.Name = "txtDigestProteinsMaximumMissedCleavages"
        Me.txtDigestProteinsMaximumMissedCleavages.Size = New System.Drawing.Size(32, 20)
        Me.txtDigestProteinsMaximumMissedCleavages.TabIndex = 9
        Me.txtDigestProteinsMaximumMissedCleavages.Text = "3"
        '
        'lblDigestProteinsMaximumMissedCleavages
        '
        Me.lblDigestProteinsMaximumMissedCleavages.Location = New System.Drawing.Point(216, 82)
        Me.lblDigestProteinsMaximumMissedCleavages.Name = "lblDigestProteinsMaximumMissedCleavages"
        Me.lblDigestProteinsMaximumMissedCleavages.Size = New System.Drawing.Size(136, 16)
        Me.lblDigestProteinsMaximumMissedCleavages.TabIndex = 8
        Me.lblDigestProteinsMaximumMissedCleavages.Text = "Max Missed Cleavages"
        '
        'txtDigestProteinsMaximumMass
        '
        Me.txtDigestProteinsMaximumMass.Location = New System.Drawing.Point(152, 80)
        Me.txtDigestProteinsMaximumMass.Name = "txtDigestProteinsMaximumMass"
        Me.txtDigestProteinsMaximumMass.Size = New System.Drawing.Size(40, 20)
        Me.txtDigestProteinsMaximumMass.TabIndex = 5
        Me.txtDigestProteinsMaximumMass.Text = "6000"
        '
        'lblDigestProteinsMaximumMass
        '
        Me.lblDigestProteinsMaximumMass.Location = New System.Drawing.Point(8, 82)
        Me.lblDigestProteinsMaximumMass.Name = "lblDigestProteinsMaximumMass"
        Me.lblDigestProteinsMaximumMass.Size = New System.Drawing.Size(144, 16)
        Me.lblDigestProteinsMaximumMass.TabIndex = 4
        Me.lblDigestProteinsMaximumMass.Text = "Maximum Fragment Mass"
        '
        'txtDigestProteinsMinimumMass
        '
        Me.txtDigestProteinsMinimumMass.Location = New System.Drawing.Point(152, 56)
        Me.txtDigestProteinsMinimumMass.Name = "txtDigestProteinsMinimumMass"
        Me.txtDigestProteinsMinimumMass.Size = New System.Drawing.Size(40, 20)
        Me.txtDigestProteinsMinimumMass.TabIndex = 3
        Me.txtDigestProteinsMinimumMass.Text = "400"
        '
        'lblDigestProteinsMinimumMass
        '
        Me.lblDigestProteinsMinimumMass.Location = New System.Drawing.Point(8, 58)
        Me.lblDigestProteinsMinimumMass.Name = "lblDigestProteinsMinimumMass"
        Me.lblDigestProteinsMinimumMass.Size = New System.Drawing.Size(144, 16)
        Me.lblDigestProteinsMinimumMass.TabIndex = 2
        Me.lblDigestProteinsMinimumMass.Text = "Minimum Fragment Mass"
        '
        'cboCleavageRuleType
        '
        Me.cboCleavageRuleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCleavageRuleType.Location = New System.Drawing.Point(8, 24)
        Me.cboCleavageRuleType.Name = "cboCleavageRuleType"
        Me.cboCleavageRuleType.Size = New System.Drawing.Size(216, 21)
        Me.cboCleavageRuleType.TabIndex = 0
        '
        'chkIncludeDuplicateSequences
        '
        Me.chkIncludeDuplicateSequences.Location = New System.Drawing.Point(240, 16)
        Me.chkIncludeDuplicateSequences.Name = "chkIncludeDuplicateSequences"
        Me.chkIncludeDuplicateSequences.Size = New System.Drawing.Size(168, 32)
        Me.chkIncludeDuplicateSequences.TabIndex = 1
        Me.chkIncludeDuplicateSequences.Text = "Include duplicate sequences for given protein"
        '
        'fraOutputTextOptions
        '
        Me.fraOutputTextOptions.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.fraOutputTextOptions.Controls.Add(Me.chkEnableLogging)
        Me.fraOutputTextOptions.Controls.Add(Me.cmdSelectOutputFile)
        Me.fraOutputTextOptions.Controls.Add(Me.txtProteinOutputFilePath)
        Me.fraOutputTextOptions.Controls.Add(Me.chkIncludePrefixAndSuffixResidues)
        Me.fraOutputTextOptions.Controls.Add(Me.cboOutputFileFieldDelimeter)
        Me.fraOutputTextOptions.Controls.Add(Me.txtOutputFileFieldDelimeter)
        Me.fraOutputTextOptions.Controls.Add(Me.lblOutputFileFieldDelimiter)
        Me.fraOutputTextOptions.Location = New System.Drawing.Point(8, 104)
        Me.fraOutputTextOptions.Name = "fraOutputTextOptions"
        Me.fraOutputTextOptions.Size = New System.Drawing.Size(721, 96)
        Me.fraOutputTextOptions.TabIndex = 1
        Me.fraOutputTextOptions.TabStop = False
        Me.fraOutputTextOptions.Text = "Output Options"
        '
        'chkEnableLogging
        '
        Me.chkEnableLogging.Checked = True
        Me.chkEnableLogging.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkEnableLogging.Location = New System.Drawing.Point(472, 24)
        Me.chkEnableLogging.Name = "chkEnableLogging"
        Me.chkEnableLogging.Size = New System.Drawing.Size(112, 16)
        Me.chkEnableLogging.TabIndex = 4
        Me.chkEnableLogging.Text = "Enable logging"
        '
        'cmdSelectOutputFile
        '
        Me.cmdSelectOutputFile.Location = New System.Drawing.Point(8, 56)
        Me.cmdSelectOutputFile.Name = "cmdSelectOutputFile"
        Me.cmdSelectOutputFile.Size = New System.Drawing.Size(88, 32)
        Me.cmdSelectOutputFile.TabIndex = 5
        Me.cmdSelectOutputFile.Text = "Select / &Create File"
        '
        'txtProteinOutputFilePath
        '
        Me.txtProteinOutputFilePath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtProteinOutputFilePath.Location = New System.Drawing.Point(104, 62)
        Me.txtProteinOutputFilePath.Name = "txtProteinOutputFilePath"
        Me.txtProteinOutputFilePath.Size = New System.Drawing.Size(601, 20)
        Me.txtProteinOutputFilePath.TabIndex = 6
        Me.txtProteinOutputFilePath.Text = ""
        '
        'chkIncludePrefixAndSuffixResidues
        '
        Me.chkIncludePrefixAndSuffixResidues.Location = New System.Drawing.Point(256, 16)
        Me.chkIncludePrefixAndSuffixResidues.Name = "chkIncludePrefixAndSuffixResidues"
        Me.chkIncludePrefixAndSuffixResidues.Size = New System.Drawing.Size(160, 32)
        Me.chkIncludePrefixAndSuffixResidues.TabIndex = 3
        Me.chkIncludePrefixAndSuffixResidues.Text = "Include prefix and suffix residues for the sequences"
        '
        'cboOutputFileFieldDelimeter
        '
        Me.cboOutputFileFieldDelimeter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboOutputFileFieldDelimeter.Location = New System.Drawing.Point(128, 24)
        Me.cboOutputFileFieldDelimeter.Name = "cboOutputFileFieldDelimeter"
        Me.cboOutputFileFieldDelimeter.Size = New System.Drawing.Size(70, 21)
        Me.cboOutputFileFieldDelimeter.TabIndex = 1
        '
        'txtOutputFileFieldDelimeter
        '
        Me.txtOutputFileFieldDelimeter.Location = New System.Drawing.Point(208, 24)
        Me.txtOutputFileFieldDelimeter.MaxLength = 1
        Me.txtOutputFileFieldDelimeter.Name = "txtOutputFileFieldDelimeter"
        Me.txtOutputFileFieldDelimeter.Size = New System.Drawing.Size(32, 20)
        Me.txtOutputFileFieldDelimeter.TabIndex = 2
        Me.txtOutputFileFieldDelimeter.Text = ";"
        '
        'lblOutputFileFieldDelimiter
        '
        Me.lblOutputFileFieldDelimiter.Location = New System.Drawing.Point(8, 26)
        Me.lblOutputFileFieldDelimiter.Name = "lblOutputFileFieldDelimiter"
        Me.lblOutputFileFieldDelimiter.Size = New System.Drawing.Size(112, 18)
        Me.lblOutputFileFieldDelimiter.TabIndex = 0
        Me.lblOutputFileFieldDelimiter.Text = "Field delimiter"
        '
        'MainMenuControl
        '
        Me.MainMenuControl.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFile, Me.mnuEdit, Me.mnuHelp})
        '
        'mnuFile
        '
        Me.mnuFile.Index = 0
        Me.mnuFile.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFileSelectInputFile, Me.mnuFileSelectOutputFile, Me.mnuFileSep1, Me.mnuFileSaveDefaultOptions, Me.mnuFileSep2, Me.mnuFileExit})
        Me.mnuFile.Text = "&File"
        '
        'mnuFileSelectInputFile
        '
        Me.mnuFileSelectInputFile.Index = 0
        Me.mnuFileSelectInputFile.Text = "Select &Input File..."
        '
        'mnuFileSelectOutputFile
        '
        Me.mnuFileSelectOutputFile.Index = 1
        Me.mnuFileSelectOutputFile.Text = "Select &Output File..."
        '
        'mnuFileSep1
        '
        Me.mnuFileSep1.Index = 2
        Me.mnuFileSep1.Text = "-"
        '
        'mnuFileSaveDefaultOptions
        '
        Me.mnuFileSaveDefaultOptions.Index = 3
        Me.mnuFileSaveDefaultOptions.Text = "Save &Default Options"
        '
        'mnuFileSep2
        '
        Me.mnuFileSep2.Index = 4
        Me.mnuFileSep2.Text = "-"
        '
        'mnuFileExit
        '
        Me.mnuFileExit.Index = 5
        Me.mnuFileExit.Text = "E&xit"
        '
        'mnuEdit
        '
        Me.mnuEdit.Index = 1
        Me.mnuEdit.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuEditParseFile, Me.mnuEditMakeUniquenessStats, Me.mnuEditSep1, Me.mnuEditResetOptions})
        Me.mnuEdit.Text = "&Edit"
        '
        'mnuEditParseFile
        '
        Me.mnuEditParseFile.Index = 0
        Me.mnuEditParseFile.Text = "&Parse File"
        '
        'mnuEditMakeUniquenessStats
        '
        Me.mnuEditMakeUniquenessStats.Index = 1
        Me.mnuEditMakeUniquenessStats.Text = "&Make Uniqueness Stats"
        '
        'mnuEditSep1
        '
        Me.mnuEditSep1.Index = 2
        Me.mnuEditSep1.Text = "-"
        '
        'mnuEditResetOptions
        '
        Me.mnuEditResetOptions.Index = 3
        Me.mnuEditResetOptions.Text = "&Reset options to Defaults..."
        '
        'mnuHelp
        '
        Me.mnuHelp.Index = 2
        Me.mnuHelp.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuHelpAbout, Me.mnuHelpAboutElutionTime})
        Me.mnuHelp.Text = "&Help"
        '
        'mnuHelpAbout
        '
        Me.mnuHelpAbout.Index = 0
        Me.mnuHelpAbout.Text = "&About"
        '
        'mnuHelpAboutElutionTime
        '
        Me.mnuHelpAboutElutionTime.Index = 1
        Me.mnuHelpAboutElutionTime.Text = "About &Elution Time Prediction"
        '
        'lblInputFileColumnOrdering
        '
        Me.lblInputFileColumnOrdering.Location = New System.Drawing.Point(8, 26)
        Me.lblInputFileColumnOrdering.Name = "lblInputFileColumnOrdering"
        Me.lblInputFileColumnOrdering.Size = New System.Drawing.Size(80, 16)
        Me.lblInputFileColumnOrdering.TabIndex = 0
        Me.lblInputFileColumnOrdering.Text = "Column Order"
        '
        'cboInputFileColumnOrdering
        '
        Me.cboInputFileColumnOrdering.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboInputFileColumnOrdering.DropDownWidth = 70
        Me.cboInputFileColumnOrdering.Location = New System.Drawing.Point(88, 24)
        Me.cboInputFileColumnOrdering.Name = "cboInputFileColumnOrdering"
        Me.cboInputFileColumnOrdering.Size = New System.Drawing.Size(392, 21)
        Me.cboInputFileColumnOrdering.TabIndex = 1
        '
        'fraDelimitedFileOptions
        '
        Me.fraDelimitedFileOptions.Controls.Add(Me.cboInputFileColumnOrdering)
        Me.fraDelimitedFileOptions.Controls.Add(Me.lblInputFileColumnOrdering)
        Me.fraDelimitedFileOptions.Controls.Add(Me.txtInputFileColumnDelimiter)
        Me.fraDelimitedFileOptions.Controls.Add(Me.lblInputFileColumnDelimiter)
        Me.fraDelimitedFileOptions.Controls.Add(Me.cboInputFileColumnDelimiter)
        Me.fraDelimitedFileOptions.Location = New System.Drawing.Point(8, 104)
        Me.fraDelimitedFileOptions.Name = "fraDelimitedFileOptions"
        Me.fraDelimitedFileOptions.Size = New System.Drawing.Size(496, 88)
        Me.fraDelimitedFileOptions.TabIndex = 1
        Me.fraDelimitedFileOptions.TabStop = False
        Me.fraDelimitedFileOptions.Text = "Delimited Input File Options"
        '
        'tbsOptions
        '
        Me.tbsOptions.Controls.Add(Me.TabPageFileFormatOptions)
        Me.tbsOptions.Controls.Add(Me.TabPageParseAndDigest)
        Me.tbsOptions.Controls.Add(Me.TabPageUniquenessStats)
        Me.tbsOptions.Controls.Add(Me.TabPagePeakMatchingThresholds)
        Me.tbsOptions.Location = New System.Drawing.Point(8, 208)
        Me.tbsOptions.Name = "tbsOptions"
        Me.tbsOptions.SelectedIndex = 0
        Me.tbsOptions.Size = New System.Drawing.Size(640, 336)
        Me.tbsOptions.TabIndex = 2
        '
        'TabPageFileFormatOptions
        '
        Me.TabPageFileFormatOptions.Controls.Add(Me.frapIAndHydrophobicity)
        Me.TabPageFileFormatOptions.Controls.Add(Me.fraInputOptions)
        Me.TabPageFileFormatOptions.Controls.Add(Me.fraDelimitedFileOptions)
        Me.TabPageFileFormatOptions.Location = New System.Drawing.Point(4, 22)
        Me.TabPageFileFormatOptions.Name = "TabPageFileFormatOptions"
        Me.TabPageFileFormatOptions.Size = New System.Drawing.Size(632, 310)
        Me.TabPageFileFormatOptions.TabIndex = 2
        Me.TabPageFileFormatOptions.Text = "File Format Options"
        '
        'frapIAndHydrophobicity
        '
        Me.frapIAndHydrophobicity.Controls.Add(Me.txtMaxpISequenceLength)
        Me.frapIAndHydrophobicity.Controls.Add(Me.lblMaxpISequenceLength)
        Me.frapIAndHydrophobicity.Controls.Add(Me.chkMaxpIModeEnabled)
        Me.frapIAndHydrophobicity.Controls.Add(Me.lblHydrophobicityMode)
        Me.frapIAndHydrophobicity.Controls.Add(Me.cboHydrophobicityMode)
        Me.frapIAndHydrophobicity.Controls.Add(Me.txtpIStats)
        Me.frapIAndHydrophobicity.Controls.Add(Me.txtSequenceForpI)
        Me.frapIAndHydrophobicity.Controls.Add(Me.lblSequenceForpI)
        Me.frapIAndHydrophobicity.Location = New System.Drawing.Point(8, 200)
        Me.frapIAndHydrophobicity.Name = "frapIAndHydrophobicity"
        Me.frapIAndHydrophobicity.Size = New System.Drawing.Size(616, 104)
        Me.frapIAndHydrophobicity.TabIndex = 2
        Me.frapIAndHydrophobicity.TabStop = False
        Me.frapIAndHydrophobicity.Text = "pI And Hydrophobicity"
        '
        'txtMaxpISequenceLength
        '
        Me.txtMaxpISequenceLength.Location = New System.Drawing.Point(168, 70)
        Me.txtMaxpISequenceLength.Name = "txtMaxpISequenceLength"
        Me.txtMaxpISequenceLength.Size = New System.Drawing.Size(40, 20)
        Me.txtMaxpISequenceLength.TabIndex = 4
        Me.txtMaxpISequenceLength.Text = "10"
        '
        'lblMaxpISequenceLength
        '
        Me.lblMaxpISequenceLength.Location = New System.Drawing.Point(32, 72)
        Me.lblMaxpISequenceLength.Name = "lblMaxpISequenceLength"
        Me.lblMaxpISequenceLength.Size = New System.Drawing.Size(144, 16)
        Me.lblMaxpISequenceLength.TabIndex = 3
        Me.lblMaxpISequenceLength.Text = "Sub-sequence Length"
        '
        'chkMaxpIModeEnabled
        '
        Me.chkMaxpIModeEnabled.Location = New System.Drawing.Point(8, 48)
        Me.chkMaxpIModeEnabled.Name = "chkMaxpIModeEnabled"
        Me.chkMaxpIModeEnabled.Size = New System.Drawing.Size(224, 16)
        Me.chkMaxpIModeEnabled.TabIndex = 2
        Me.chkMaxpIModeEnabled.Text = "Report maximum of all sub-sequences"
        '
        'lblHydrophobicityMode
        '
        Me.lblHydrophobicityMode.Location = New System.Drawing.Point(8, 24)
        Me.lblHydrophobicityMode.Name = "lblHydrophobicityMode"
        Me.lblHydrophobicityMode.Size = New System.Drawing.Size(120, 16)
        Me.lblHydrophobicityMode.TabIndex = 0
        Me.lblHydrophobicityMode.Text = "Hydrophobicity Mode"
        '
        'cboHydrophobicityMode
        '
        Me.cboHydrophobicityMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboHydrophobicityMode.DropDownWidth = 70
        Me.cboHydrophobicityMode.Location = New System.Drawing.Point(128, 18)
        Me.cboHydrophobicityMode.Name = "cboHydrophobicityMode"
        Me.cboHydrophobicityMode.Size = New System.Drawing.Size(184, 21)
        Me.cboHydrophobicityMode.TabIndex = 1
        '
        'txtpIStats
        '
        Me.txtpIStats.Location = New System.Drawing.Point(336, 48)
        Me.txtpIStats.MaxLength = 1
        Me.txtpIStats.Multiline = True
        Me.txtpIStats.Name = "txtpIStats"
        Me.txtpIStats.ReadOnly = True
        Me.txtpIStats.Size = New System.Drawing.Size(272, 40)
        Me.txtpIStats.TabIndex = 7
        Me.txtpIStats.Text = ""
        '
        'txtSequenceForpI
        '
        Me.txtSequenceForpI.Location = New System.Drawing.Point(400, 16)
        Me.txtSequenceForpI.Name = "txtSequenceForpI"
        Me.txtSequenceForpI.Size = New System.Drawing.Size(208, 20)
        Me.txtSequenceForpI.TabIndex = 6
        Me.txtSequenceForpI.Text = "FKDLGEEQFK"
        '
        'lblSequenceForpI
        '
        Me.lblSequenceForpI.Location = New System.Drawing.Point(328, 20)
        Me.lblSequenceForpI.Name = "lblSequenceForpI"
        Me.lblSequenceForpI.Size = New System.Drawing.Size(72, 16)
        Me.lblSequenceForpI.TabIndex = 5
        Me.lblSequenceForpI.Text = "Sequence"
        '
        'TabPageParseAndDigest
        '
        Me.TabPageParseAndDigest.Controls.Add(Me.fraProcessingOptions)
        Me.TabPageParseAndDigest.Controls.Add(Me.fraCalculationOptions)
        Me.TabPageParseAndDigest.Controls.Add(Me.fraDigestionOptions)
        Me.TabPageParseAndDigest.Controls.Add(Me.cmdParseInputFile)
        Me.TabPageParseAndDigest.Location = New System.Drawing.Point(4, 22)
        Me.TabPageParseAndDigest.Name = "TabPageParseAndDigest"
        Me.TabPageParseAndDigest.Size = New System.Drawing.Size(632, 310)
        Me.TabPageParseAndDigest.TabIndex = 0
        Me.TabPageParseAndDigest.Text = "Parse and Digest File Options"
        '
        'fraProcessingOptions
        '
        Me.fraProcessingOptions.Controls.Add(Me.lblProteinScramblingLoopCount)
        Me.fraProcessingOptions.Controls.Add(Me.txtProteinScramblingLoopCount)
        Me.fraProcessingOptions.Controls.Add(Me.lblSamplingPercentageUnits)
        Me.fraProcessingOptions.Controls.Add(Me.lblProteinReversalSamplingPercentage)
        Me.fraProcessingOptions.Controls.Add(Me.txtProteinReversalSamplingPercentage)
        Me.fraProcessingOptions.Controls.Add(Me.lbltxtAddnlRefAccessionSepChar)
        Me.fraProcessingOptions.Controls.Add(Me.chkLookForAddnlRefInDescription)
        Me.fraProcessingOptions.Controls.Add(Me.cboProteinReversalOptions)
        Me.fraProcessingOptions.Controls.Add(Me.lblProteinReversalOptions)
        Me.fraProcessingOptions.Controls.Add(Me.chkDigestProteins)
        Me.fraProcessingOptions.Controls.Add(Me.lblAddnlRefSepChar)
        Me.fraProcessingOptions.Controls.Add(Me.txtAddnlRefAccessionSepChar)
        Me.fraProcessingOptions.Controls.Add(Me.txtAddnlRefSepChar)
        Me.fraProcessingOptions.Controls.Add(Me.chkCreateFastaOutputFile)
        Me.fraProcessingOptions.Location = New System.Drawing.Point(8, 8)
        Me.fraProcessingOptions.Name = "fraProcessingOptions"
        Me.fraProcessingOptions.Size = New System.Drawing.Size(360, 152)
        Me.fraProcessingOptions.TabIndex = 0
        Me.fraProcessingOptions.TabStop = False
        Me.fraProcessingOptions.Text = "Processing Options"
        '
        'lblSamplingPercentageUnits
        '
        Me.lblSamplingPercentageUnits.Location = New System.Drawing.Point(208, 42)
        Me.lblSamplingPercentageUnits.Name = "lblSamplingPercentageUnits"
        Me.lblSamplingPercentageUnits.Size = New System.Drawing.Size(16, 16)
        Me.lblSamplingPercentageUnits.TabIndex = 4
        Me.lblSamplingPercentageUnits.Text = "%"
        '
        'lblProteinReversalSamplingPercentage
        '
        Me.lblProteinReversalSamplingPercentage.Location = New System.Drawing.Point(48, 42)
        Me.lblProteinReversalSamplingPercentage.Name = "lblProteinReversalSamplingPercentage"
        Me.lblProteinReversalSamplingPercentage.Size = New System.Drawing.Size(112, 16)
        Me.lblProteinReversalSamplingPercentage.TabIndex = 2
        Me.lblProteinReversalSamplingPercentage.Text = "Sampling Percentage"
        Me.lblProteinReversalSamplingPercentage.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtProteinReversalSamplingPercentage
        '
        Me.txtProteinReversalSamplingPercentage.Location = New System.Drawing.Point(168, 40)
        Me.txtProteinReversalSamplingPercentage.MaxLength = 3
        Me.txtProteinReversalSamplingPercentage.Name = "txtProteinReversalSamplingPercentage"
        Me.txtProteinReversalSamplingPercentage.Size = New System.Drawing.Size(32, 20)
        Me.txtProteinReversalSamplingPercentage.TabIndex = 3
        Me.txtProteinReversalSamplingPercentage.Text = "100"
        '
        'chkCreateFastaOutputFile
        '
        Me.chkCreateFastaOutputFile.Location = New System.Drawing.Point(192, 128)
        Me.chkCreateFastaOutputFile.Name = "chkCreateFastaOutputFile"
        Me.chkCreateFastaOutputFile.Size = New System.Drawing.Size(160, 16)
        Me.chkCreateFastaOutputFile.TabIndex = 11
        Me.chkCreateFastaOutputFile.Text = "Create Fasta Output File"
        '
        'TabPageUniquenessStats
        '
        Me.TabPageUniquenessStats.Controls.Add(Me.lblUniquenessCalculationsNote)
        Me.TabPageUniquenessStats.Controls.Add(Me.fraPeakMatchingOptions)
        Me.TabPageUniquenessStats.Controls.Add(Me.fraSqlServerOptions)
        Me.TabPageUniquenessStats.Controls.Add(Me.fraUniquenessBinningOptions)
        Me.TabPageUniquenessStats.Controls.Add(Me.lblUniquenessStatsNote)
        Me.TabPageUniquenessStats.Controls.Add(Me.cmdGenerateUniquenessStats)
        Me.TabPageUniquenessStats.Controls.Add(Me.chkAssumeInputFileIsDigested)
        Me.TabPageUniquenessStats.Location = New System.Drawing.Point(4, 22)
        Me.TabPageUniquenessStats.Name = "TabPageUniquenessStats"
        Me.TabPageUniquenessStats.Size = New System.Drawing.Size(632, 310)
        Me.TabPageUniquenessStats.TabIndex = 1
        Me.TabPageUniquenessStats.Text = "Peptide Uniqueness Options"
        '
        'lblUniquenessCalculationsNote
        '
        Me.lblUniquenessCalculationsNote.Location = New System.Drawing.Point(240, 192)
        Me.lblUniquenessCalculationsNote.Name = "lblUniquenessCalculationsNote"
        Me.lblUniquenessCalculationsNote.Size = New System.Drawing.Size(384, 88)
        Me.lblUniquenessCalculationsNote.TabIndex = 6
        '
        'fraPeakMatchingOptions
        '
        Me.fraPeakMatchingOptions.Controls.Add(Me.optUseRectangleSearchRegion)
        Me.fraPeakMatchingOptions.Controls.Add(Me.optUseEllipseSearchRegion)
        Me.fraPeakMatchingOptions.Controls.Add(Me.txtMaxPeakMatchingResultsPerFeatureToSave)
        Me.fraPeakMatchingOptions.Controls.Add(Me.lblMaxPeakMatchingResultsPerFeatureToSave)
        Me.fraPeakMatchingOptions.Controls.Add(Me.chkExportPeakMatchingResults)
        Me.fraPeakMatchingOptions.Controls.Add(Me.txtMinimumSLiCScore)
        Me.fraPeakMatchingOptions.Controls.Add(Me.lblMinimumSLiCScore)
        Me.fraPeakMatchingOptions.Controls.Add(Me.chkUseSLiCScoreForUniqueness)
        Me.fraPeakMatchingOptions.Location = New System.Drawing.Point(232, 48)
        Me.fraPeakMatchingOptions.Name = "fraPeakMatchingOptions"
        Me.fraPeakMatchingOptions.Size = New System.Drawing.Size(392, 136)
        Me.fraPeakMatchingOptions.TabIndex = 2
        Me.fraPeakMatchingOptions.TabStop = False
        Me.fraPeakMatchingOptions.Text = "Peak Matching Options"
        '
        'optUseRectangleSearchRegion
        '
        Me.optUseRectangleSearchRegion.Location = New System.Drawing.Point(232, 96)
        Me.optUseRectangleSearchRegion.Name = "optUseRectangleSearchRegion"
        Me.optUseRectangleSearchRegion.Size = New System.Drawing.Size(136, 16)
        Me.optUseRectangleSearchRegion.TabIndex = 7
        Me.optUseRectangleSearchRegion.Text = "Use rectangle region"
        '
        'optUseEllipseSearchRegion
        '
        Me.optUseEllipseSearchRegion.Checked = True
        Me.optUseEllipseSearchRegion.Location = New System.Drawing.Point(232, 72)
        Me.optUseEllipseSearchRegion.Name = "optUseEllipseSearchRegion"
        Me.optUseEllipseSearchRegion.Size = New System.Drawing.Size(152, 16)
        Me.optUseEllipseSearchRegion.TabIndex = 6
        Me.optUseEllipseSearchRegion.TabStop = True
        Me.optUseEllipseSearchRegion.Text = "Use ellipse search region"
        '
        'txtMaxPeakMatchingResultsPerFeatureToSave
        '
        Me.txtMaxPeakMatchingResultsPerFeatureToSave.Location = New System.Drawing.Point(272, 16)
        Me.txtMaxPeakMatchingResultsPerFeatureToSave.Name = "txtMaxPeakMatchingResultsPerFeatureToSave"
        Me.txtMaxPeakMatchingResultsPerFeatureToSave.Size = New System.Drawing.Size(40, 20)
        Me.txtMaxPeakMatchingResultsPerFeatureToSave.TabIndex = 1
        Me.txtMaxPeakMatchingResultsPerFeatureToSave.Text = "3"
        '
        'lblMaxPeakMatchingResultsPerFeatureToSave
        '
        Me.lblMaxPeakMatchingResultsPerFeatureToSave.Location = New System.Drawing.Point(16, 18)
        Me.lblMaxPeakMatchingResultsPerFeatureToSave.Name = "lblMaxPeakMatchingResultsPerFeatureToSave"
        Me.lblMaxPeakMatchingResultsPerFeatureToSave.Size = New System.Drawing.Size(256, 16)
        Me.lblMaxPeakMatchingResultsPerFeatureToSave.TabIndex = 0
        Me.lblMaxPeakMatchingResultsPerFeatureToSave.Text = "Max Peak Matching Results Per Feature To Save"
        '
        'chkExportPeakMatchingResults
        '
        Me.chkExportPeakMatchingResults.Location = New System.Drawing.Point(32, 36)
        Me.chkExportPeakMatchingResults.Name = "chkExportPeakMatchingResults"
        Me.chkExportPeakMatchingResults.Size = New System.Drawing.Size(192, 17)
        Me.chkExportPeakMatchingResults.TabIndex = 2
        Me.chkExportPeakMatchingResults.Text = "Export peak matching results"
        '
        'txtMinimumSLiCScore
        '
        Me.txtMinimumSLiCScore.Location = New System.Drawing.Point(144, 104)
        Me.txtMinimumSLiCScore.Name = "txtMinimumSLiCScore"
        Me.txtMinimumSLiCScore.Size = New System.Drawing.Size(40, 20)
        Me.txtMinimumSLiCScore.TabIndex = 5
        Me.txtMinimumSLiCScore.Text = "0.99"
        '
        'lblMinimumSLiCScore
        '
        Me.lblMinimumSLiCScore.Location = New System.Drawing.Point(16, 96)
        Me.lblMinimumSLiCScore.Name = "lblMinimumSLiCScore"
        Me.lblMinimumSLiCScore.Size = New System.Drawing.Size(128, 32)
        Me.lblMinimumSLiCScore.TabIndex = 4
        Me.lblMinimumSLiCScore.Text = "Minimum SLiC score to be considered unique"
        '
        'chkUseSLiCScoreForUniqueness
        '
        Me.chkUseSLiCScoreForUniqueness.Checked = True
        Me.chkUseSLiCScoreForUniqueness.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUseSLiCScoreForUniqueness.Location = New System.Drawing.Point(16, 60)
        Me.chkUseSLiCScoreForUniqueness.Name = "chkUseSLiCScoreForUniqueness"
        Me.chkUseSLiCScoreForUniqueness.Size = New System.Drawing.Size(168, 32)
        Me.chkUseSLiCScoreForUniqueness.TabIndex = 3
        Me.chkUseSLiCScoreForUniqueness.Text = "Use SLiC Score when gauging peptide uniqueness"
        '
        'fraSqlServerOptions
        '
        Me.fraSqlServerOptions.Controls.Add(Me.chkSqlServerUseExistingData)
        Me.fraSqlServerOptions.Controls.Add(Me.chkAllowSqlServerCaching)
        Me.fraSqlServerOptions.Controls.Add(Me.lblSqlServerPassword)
        Me.fraSqlServerOptions.Controls.Add(Me.lblSqlServerUsername)
        Me.fraSqlServerOptions.Controls.Add(Me.txtSqlServerPassword)
        Me.fraSqlServerOptions.Controls.Add(Me.txtSqlServerUsername)
        Me.fraSqlServerOptions.Controls.Add(Me.lblSqlServerDatabase)
        Me.fraSqlServerOptions.Controls.Add(Me.lblSqlServerServerName)
        Me.fraSqlServerOptions.Controls.Add(Me.chkSqlServerUseIntegratedSecurity)
        Me.fraSqlServerOptions.Controls.Add(Me.txtSqlServerDatabase)
        Me.fraSqlServerOptions.Controls.Add(Me.txtSqlServerName)
        Me.fraSqlServerOptions.Controls.Add(Me.chkUseSqlServerDBToCacheData)
        Me.fraSqlServerOptions.Location = New System.Drawing.Point(576, 192)
        Me.fraSqlServerOptions.Name = "fraSqlServerOptions"
        Me.fraSqlServerOptions.Size = New System.Drawing.Size(376, 112)
        Me.fraSqlServerOptions.TabIndex = 4
        Me.fraSqlServerOptions.TabStop = False
        Me.fraSqlServerOptions.Text = "Sql Server Options"
        Me.fraSqlServerOptions.Visible = False
        '
        'chkSqlServerUseExistingData
        '
        Me.chkSqlServerUseExistingData.Checked = True
        Me.chkSqlServerUseExistingData.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSqlServerUseExistingData.Location = New System.Drawing.Point(8, 88)
        Me.chkSqlServerUseExistingData.Name = "chkSqlServerUseExistingData"
        Me.chkSqlServerUseExistingData.Size = New System.Drawing.Size(144, 16)
        Me.chkSqlServerUseExistingData.TabIndex = 11
        Me.chkSqlServerUseExistingData.Text = "Use Existing Data"
        '
        'chkAllowSqlServerCaching
        '
        Me.chkAllowSqlServerCaching.Location = New System.Drawing.Point(8, 16)
        Me.chkAllowSqlServerCaching.Name = "chkAllowSqlServerCaching"
        Me.chkAllowSqlServerCaching.Size = New System.Drawing.Size(144, 32)
        Me.chkAllowSqlServerCaching.TabIndex = 0
        Me.chkAllowSqlServerCaching.Text = "Allow data caching using Sql Server"
        '
        'lblSqlServerPassword
        '
        Me.lblSqlServerPassword.Location = New System.Drawing.Point(264, 64)
        Me.lblSqlServerPassword.Name = "lblSqlServerPassword"
        Me.lblSqlServerPassword.Size = New System.Drawing.Size(56, 16)
        Me.lblSqlServerPassword.TabIndex = 9
        Me.lblSqlServerPassword.Text = "Password"
        '
        'lblSqlServerUsername
        '
        Me.lblSqlServerUsername.Location = New System.Drawing.Point(184, 64)
        Me.lblSqlServerUsername.Name = "lblSqlServerUsername"
        Me.lblSqlServerUsername.Size = New System.Drawing.Size(56, 16)
        Me.lblSqlServerUsername.TabIndex = 7
        Me.lblSqlServerUsername.Text = "Username"
        '
        'txtSqlServerPassword
        '
        Me.txtSqlServerPassword.Location = New System.Drawing.Point(264, 80)
        Me.txtSqlServerPassword.Name = "txtSqlServerPassword"
        Me.txtSqlServerPassword.PasswordChar = Microsoft.VisualBasic.ChrW(42)
        Me.txtSqlServerPassword.Size = New System.Drawing.Size(88, 20)
        Me.txtSqlServerPassword.TabIndex = 10
        Me.txtSqlServerPassword.Text = "pw"
        '
        'txtSqlServerUsername
        '
        Me.txtSqlServerUsername.Location = New System.Drawing.Point(184, 80)
        Me.txtSqlServerUsername.Name = "txtSqlServerUsername"
        Me.txtSqlServerUsername.Size = New System.Drawing.Size(72, 20)
        Me.txtSqlServerUsername.TabIndex = 8
        Me.txtSqlServerUsername.Text = "user"
        '
        'lblSqlServerDatabase
        '
        Me.lblSqlServerDatabase.Location = New System.Drawing.Point(264, 16)
        Me.lblSqlServerDatabase.Name = "lblSqlServerDatabase"
        Me.lblSqlServerDatabase.Size = New System.Drawing.Size(56, 16)
        Me.lblSqlServerDatabase.TabIndex = 4
        Me.lblSqlServerDatabase.Text = "Database"
        '
        'lblSqlServerServerName
        '
        Me.lblSqlServerServerName.Location = New System.Drawing.Point(184, 16)
        Me.lblSqlServerServerName.Name = "lblSqlServerServerName"
        Me.lblSqlServerServerName.Size = New System.Drawing.Size(56, 16)
        Me.lblSqlServerServerName.TabIndex = 2
        Me.lblSqlServerServerName.Text = "Server"
        '
        'chkSqlServerUseIntegratedSecurity
        '
        Me.chkSqlServerUseIntegratedSecurity.Checked = True
        Me.chkSqlServerUseIntegratedSecurity.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkSqlServerUseIntegratedSecurity.Location = New System.Drawing.Point(8, 72)
        Me.chkSqlServerUseIntegratedSecurity.Name = "chkSqlServerUseIntegratedSecurity"
        Me.chkSqlServerUseIntegratedSecurity.Size = New System.Drawing.Size(144, 16)
        Me.chkSqlServerUseIntegratedSecurity.TabIndex = 6
        Me.chkSqlServerUseIntegratedSecurity.Text = "Use Integrated Security"
        '
        'txtSqlServerDatabase
        '
        Me.txtSqlServerDatabase.Location = New System.Drawing.Point(264, 32)
        Me.txtSqlServerDatabase.Name = "txtSqlServerDatabase"
        Me.txtSqlServerDatabase.Size = New System.Drawing.Size(88, 20)
        Me.txtSqlServerDatabase.TabIndex = 5
        Me.txtSqlServerDatabase.Text = "TempDB"
        '
        'txtSqlServerName
        '
        Me.txtSqlServerName.Location = New System.Drawing.Point(184, 32)
        Me.txtSqlServerName.Name = "txtSqlServerName"
        Me.txtSqlServerName.Size = New System.Drawing.Size(72, 20)
        Me.txtSqlServerName.TabIndex = 3
        Me.txtSqlServerName.Text = "Monroe"
        '
        'chkUseSqlServerDBToCacheData
        '
        Me.chkUseSqlServerDBToCacheData.Checked = True
        Me.chkUseSqlServerDBToCacheData.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkUseSqlServerDBToCacheData.Location = New System.Drawing.Point(8, 56)
        Me.chkUseSqlServerDBToCacheData.Name = "chkUseSqlServerDBToCacheData"
        Me.chkUseSqlServerDBToCacheData.Size = New System.Drawing.Size(144, 16)
        Me.chkUseSqlServerDBToCacheData.TabIndex = 1
        Me.chkUseSqlServerDBToCacheData.Text = "Enable data caching"
        '
        'fraUniquenessBinningOptions
        '
        Me.fraUniquenessBinningOptions.Controls.Add(Me.txtUniquenessBinWidth)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.lblUniquenessBinWidth)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.chkAutoComputeRangeForBinning)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.txtUniquenessBinEndMass)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.lblUniquenessBinEndMass)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.txtUniquenessBinStartMass)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.lblUniquenessBinStartMass)
        Me.fraUniquenessBinningOptions.Location = New System.Drawing.Point(8, 144)
        Me.fraUniquenessBinningOptions.Name = "fraUniquenessBinningOptions"
        Me.fraUniquenessBinningOptions.Size = New System.Drawing.Size(208, 136)
        Me.fraUniquenessBinningOptions.TabIndex = 3
        Me.fraUniquenessBinningOptions.TabStop = False
        Me.fraUniquenessBinningOptions.Text = "Binning Options"
        '
        'txtUniquenessBinWidth
        '
        Me.txtUniquenessBinWidth.Location = New System.Drawing.Point(80, 24)
        Me.txtUniquenessBinWidth.Name = "txtUniquenessBinWidth"
        Me.txtUniquenessBinWidth.Size = New System.Drawing.Size(40, 20)
        Me.txtUniquenessBinWidth.TabIndex = 1
        Me.txtUniquenessBinWidth.Text = "25"
        '
        'lblUniquenessBinWidth
        '
        Me.lblUniquenessBinWidth.Location = New System.Drawing.Point(16, 26)
        Me.lblUniquenessBinWidth.Name = "lblUniquenessBinWidth"
        Me.lblUniquenessBinWidth.Size = New System.Drawing.Size(64, 16)
        Me.lblUniquenessBinWidth.TabIndex = 0
        Me.lblUniquenessBinWidth.Text = "Bin Width"
        '
        'chkAutoComputeRangeForBinning
        '
        Me.chkAutoComputeRangeForBinning.Checked = True
        Me.chkAutoComputeRangeForBinning.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAutoComputeRangeForBinning.Location = New System.Drawing.Point(16, 56)
        Me.chkAutoComputeRangeForBinning.Name = "chkAutoComputeRangeForBinning"
        Me.chkAutoComputeRangeForBinning.Size = New System.Drawing.Size(184, 17)
        Me.chkAutoComputeRangeForBinning.TabIndex = 2
        Me.chkAutoComputeRangeForBinning.Text = "Auto compute range for binning"
        '
        'txtUniquenessBinEndMass
        '
        Me.txtUniquenessBinEndMass.Location = New System.Drawing.Point(80, 104)
        Me.txtUniquenessBinEndMass.Name = "txtUniquenessBinEndMass"
        Me.txtUniquenessBinEndMass.Size = New System.Drawing.Size(40, 20)
        Me.txtUniquenessBinEndMass.TabIndex = 6
        Me.txtUniquenessBinEndMass.Text = "6000"
        '
        'lblUniquenessBinEndMass
        '
        Me.lblUniquenessBinEndMass.Location = New System.Drawing.Point(16, 106)
        Me.lblUniquenessBinEndMass.Name = "lblUniquenessBinEndMass"
        Me.lblUniquenessBinEndMass.Size = New System.Drawing.Size(64, 16)
        Me.lblUniquenessBinEndMass.TabIndex = 5
        Me.lblUniquenessBinEndMass.Text = "End Mass"
        '
        'txtUniquenessBinStartMass
        '
        Me.txtUniquenessBinStartMass.Location = New System.Drawing.Point(80, 80)
        Me.txtUniquenessBinStartMass.Name = "txtUniquenessBinStartMass"
        Me.txtUniquenessBinStartMass.Size = New System.Drawing.Size(40, 20)
        Me.txtUniquenessBinStartMass.TabIndex = 4
        Me.txtUniquenessBinStartMass.Text = "400"
        '
        'lblUniquenessBinStartMass
        '
        Me.lblUniquenessBinStartMass.Location = New System.Drawing.Point(16, 82)
        Me.lblUniquenessBinStartMass.Name = "lblUniquenessBinStartMass"
        Me.lblUniquenessBinStartMass.Size = New System.Drawing.Size(64, 16)
        Me.lblUniquenessBinStartMass.TabIndex = 3
        Me.lblUniquenessBinStartMass.Text = "Start Mass"
        '
        'lblUniquenessStatsNote
        '
        Me.lblUniquenessStatsNote.Location = New System.Drawing.Point(8, 56)
        Me.lblUniquenessStatsNote.Name = "lblUniquenessStatsNote"
        Me.lblUniquenessStatsNote.Size = New System.Drawing.Size(200, 48)
        Me.lblUniquenessStatsNote.TabIndex = 1
        Me.lblUniquenessStatsNote.Text = "Note that Digestion Options and Mass Calculation Options also apply for uniquenes" & _
        "s stats generation."
        '
        'chkAssumeInputFileIsDigested
        '
        Me.chkAssumeInputFileIsDigested.Location = New System.Drawing.Point(8, 16)
        Me.chkAssumeInputFileIsDigested.Name = "chkAssumeInputFileIsDigested"
        Me.chkAssumeInputFileIsDigested.Size = New System.Drawing.Size(192, 32)
        Me.chkAssumeInputFileIsDigested.TabIndex = 0
        Me.chkAssumeInputFileIsDigested.Text = "Assume input file is already digested (for Delimited files only)"
        '
        'TabPagePeakMatchingThresholds
        '
        Me.TabPagePeakMatchingThresholds.Controls.Add(Me.chkAutoDefineSLiCScoreTolerances)
        Me.TabPagePeakMatchingThresholds.Controls.Add(Me.cmdPastePMThresholdsList)
        Me.TabPagePeakMatchingThresholds.Controls.Add(Me.cboPMPredefinedThresholds)
        Me.TabPagePeakMatchingThresholds.Controls.Add(Me.cmdPMThresholdsAutoPopulate)
        Me.TabPagePeakMatchingThresholds.Controls.Add(Me.cmdClearPMThresholdsList)
        Me.TabPagePeakMatchingThresholds.Controls.Add(Me.cboMassTolType)
        Me.TabPagePeakMatchingThresholds.Controls.Add(Me.lblMassTolType)
        Me.TabPagePeakMatchingThresholds.Controls.Add(Me.dgPeakMatchingThresholds)
        Me.TabPagePeakMatchingThresholds.Location = New System.Drawing.Point(4, 22)
        Me.TabPagePeakMatchingThresholds.Name = "TabPagePeakMatchingThresholds"
        Me.TabPagePeakMatchingThresholds.Size = New System.Drawing.Size(632, 310)
        Me.TabPagePeakMatchingThresholds.TabIndex = 3
        Me.TabPagePeakMatchingThresholds.Text = "Peak Matching Thresholds"
        '
        'chkAutoDefineSLiCScoreTolerances
        '
        Me.chkAutoDefineSLiCScoreTolerances.Checked = True
        Me.chkAutoDefineSLiCScoreTolerances.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAutoDefineSLiCScoreTolerances.Location = New System.Drawing.Point(16, 256)
        Me.chkAutoDefineSLiCScoreTolerances.Name = "chkAutoDefineSLiCScoreTolerances"
        Me.chkAutoDefineSLiCScoreTolerances.Size = New System.Drawing.Size(208, 16)
        Me.chkAutoDefineSLiCScoreTolerances.TabIndex = 3
        Me.chkAutoDefineSLiCScoreTolerances.Text = "Auto Define SLiC Score Tolerances"
        '
        'cmdPastePMThresholdsList
        '
        Me.cmdPastePMThresholdsList.Location = New System.Drawing.Point(456, 96)
        Me.cmdPastePMThresholdsList.Name = "cmdPastePMThresholdsList"
        Me.cmdPastePMThresholdsList.Size = New System.Drawing.Size(104, 24)
        Me.cmdPastePMThresholdsList.TabIndex = 6
        Me.cmdPastePMThresholdsList.Text = "Paste Values"
        '
        'cboPMPredefinedThresholds
        '
        Me.cboPMPredefinedThresholds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboPMPredefinedThresholds.Location = New System.Drawing.Point(336, 256)
        Me.cboPMPredefinedThresholds.Name = "cboPMPredefinedThresholds"
        Me.cboPMPredefinedThresholds.Size = New System.Drawing.Size(264, 21)
        Me.cboPMPredefinedThresholds.TabIndex = 5
        '
        'cmdPMThresholdsAutoPopulate
        '
        Me.cmdPMThresholdsAutoPopulate.Location = New System.Drawing.Point(336, 224)
        Me.cmdPMThresholdsAutoPopulate.Name = "cmdPMThresholdsAutoPopulate"
        Me.cmdPMThresholdsAutoPopulate.Size = New System.Drawing.Size(104, 24)
        Me.cmdPMThresholdsAutoPopulate.TabIndex = 4
        Me.cmdPMThresholdsAutoPopulate.Text = "Auto-Populate"
        '
        'cmdClearPMThresholdsList
        '
        Me.cmdClearPMThresholdsList.Location = New System.Drawing.Point(456, 128)
        Me.cmdClearPMThresholdsList.Name = "cmdClearPMThresholdsList"
        Me.cmdClearPMThresholdsList.Size = New System.Drawing.Size(104, 24)
        Me.cmdClearPMThresholdsList.TabIndex = 7
        Me.cmdClearPMThresholdsList.Text = "Clear List"
        '
        'cboMassTolType
        '
        Me.cboMassTolType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMassTolType.Location = New System.Drawing.Point(144, 224)
        Me.cboMassTolType.Name = "cboMassTolType"
        Me.cboMassTolType.Size = New System.Drawing.Size(136, 21)
        Me.cboMassTolType.TabIndex = 2
        '
        'lblMassTolType
        '
        Me.lblMassTolType.Location = New System.Drawing.Point(16, 226)
        Me.lblMassTolType.Name = "lblMassTolType"
        Me.lblMassTolType.Size = New System.Drawing.Size(136, 16)
        Me.lblMassTolType.TabIndex = 1
        Me.lblMassTolType.Text = "Mass Tolerance Type"
        '
        'dgPeakMatchingThresholds
        '
        Me.dgPeakMatchingThresholds.CaptionText = "Peak Matching Thresholds"
        Me.dgPeakMatchingThresholds.DataMember = ""
        Me.dgPeakMatchingThresholds.HeaderForeColor = System.Drawing.SystemColors.ControlText
        Me.dgPeakMatchingThresholds.Location = New System.Drawing.Point(16, 8)
        Me.dgPeakMatchingThresholds.Name = "dgPeakMatchingThresholds"
        Me.dgPeakMatchingThresholds.Size = New System.Drawing.Size(424, 208)
        Me.dgPeakMatchingThresholds.TabIndex = 0
        '
        'lblProteinScramblingLoopCount
        '
        Me.lblProteinScramblingLoopCount.Location = New System.Drawing.Point(232, 42)
        Me.lblProteinScramblingLoopCount.Name = "lblProteinScramblingLoopCount"
        Me.lblProteinScramblingLoopCount.Size = New System.Drawing.Size(72, 16)
        Me.lblProteinScramblingLoopCount.TabIndex = 12
        Me.lblProteinScramblingLoopCount.Text = "Loop Count"
        Me.lblProteinScramblingLoopCount.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'txtProteinScramblingLoopCount
        '
        Me.txtProteinScramblingLoopCount.Location = New System.Drawing.Point(312, 40)
        Me.txtProteinScramblingLoopCount.MaxLength = 3
        Me.txtProteinScramblingLoopCount.Name = "txtProteinScramblingLoopCount"
        Me.txtProteinScramblingLoopCount.Size = New System.Drawing.Size(32, 20)
        Me.txtProteinScramblingLoopCount.TabIndex = 13
        Me.txtProteinScramblingLoopCount.Text = "1"
        '
        'frmMain
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(736, 554)
        Me.Controls.Add(Me.tbsOptions)
        Me.Controls.Add(Me.fraOutputTextOptions)
        Me.Controls.Add(Me.fraInputFilePath)
        Me.Menu = Me.MainMenuControl
        Me.MinimumSize = New System.Drawing.Size(664, 0)
        Me.Name = "frmMain"
        Me.Text = "Protein Digestion Simulator"
        Me.fraInputFilePath.ResumeLayout(False)
        Me.fraInputOptions.ResumeLayout(False)
        Me.fraCalculationOptions.ResumeLayout(False)
        Me.fraDigestionOptions.ResumeLayout(False)
        Me.fraOutputTextOptions.ResumeLayout(False)
        Me.fraDelimitedFileOptions.ResumeLayout(False)
        Me.tbsOptions.ResumeLayout(False)
        Me.TabPageFileFormatOptions.ResumeLayout(False)
        Me.frapIAndHydrophobicity.ResumeLayout(False)
        Me.TabPageParseAndDigest.ResumeLayout(False)
        Me.fraProcessingOptions.ResumeLayout(False)
        Me.TabPageUniquenessStats.ResumeLayout(False)
        Me.fraPeakMatchingOptions.ResumeLayout(False)
        Me.fraSqlServerOptions.ResumeLayout(False)
        Me.fraUniquenessBinningOptions.ResumeLayout(False)
        Me.TabPagePeakMatchingThresholds.ResumeLayout(False)
        CType(Me.dgPeakMatchingThresholds, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

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
    Private mDefaultFastaFileOptions As clsParseProteinFile.FastaFileOptionsClass

    Private mPeakMatchingThresholdsDataset As System.Data.DataSet
    Private mPredefinedPMThresholds() As udtPredefinedPMThresholdsType

    Private mWorking As Boolean
    Private mCustomValidationRulesFilePath As String

    Private objpICalculator As clspICalculation

#End Region

#Region "Procedures"

    Private Sub AddPMThresholdRow(ByVal dblMassThreshold As Double, ByVal dblNETThreshold As Double, Optional ByRef blnExistingRowFound As Boolean = False)
        AddPMThresholdRow(dblMassThreshold, dblNETThreshold, DEFAULT_SLIC_MASS_STDEV, DEFAULT_SLIC_NET_STDEV, blnExistingRowFound)
    End Sub

    Private Sub AddPMThresholdRow(ByVal dblMassThreshold As Double, ByVal dblNETThreshold As Double, ByVal dblSLiCMassStDev As Double, ByVal dblSLiCNETStDev As Double, Optional ByRef blnExistingRowFound As Boolean = False)
        Dim myDataRow As System.Data.DataRow

        With mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE)

            For Each myDataRow In .Rows
                With myDataRow
                    If CDbl(.Item(0)) = dblMassThreshold And CDbl(.Item(1)) = dblNETThreshold Then
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
                txtProteinOutputFilePath.Text = AutoDefineOutputFileWork(txtProteinInputFilePath.Text)
            End If
        Catch ex As Exception
            ' Leave the textbox unchanged
        End Try
    End Sub

    Private Function AutoDefineOutputFileWork(ByVal strInputFilePath As String) As String
        Dim strInputFileName As String
        Dim strOutputFileName As String

        strInputFileName = System.IO.Path.GetFileName(txtProteinInputFilePath.Text)

        If chkCreateFastaOutputFile.Enabled AndAlso chkCreateFastaOutputFile.Checked Then
            If System.IO.Path.GetExtension(strInputFileName).ToLower = ".fasta" Then
                strOutputFileName = System.IO.Path.GetFileNameWithoutExtension(strInputFileName) & "_new.fasta"
            Else
                strOutputFileName = System.IO.Path.ChangeExtension(strInputFileName, ".fasta")
            End If
        Else
            If System.IO.Path.GetExtension(strInputFileName).ToLower = ".txt" Then
                strOutputFileName = System.IO.Path.GetFileNameWithoutExtension(strInputFileName) & "_output.txt"
            Else
                strOutputFileName = System.IO.Path.ChangeExtension(strInputFileName, ".txt")
            End If
        End If


        Return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(strInputFilePath), strOutputFileName)

    End Function

    Private Sub AutoPopulatePMThresholds(ByVal udtPredefinedThresholds As udtPredefinedPMThresholdsType, ByVal blnConfirmReplaceExistingResults As Boolean)

        Dim intIndex As Integer

        If ClearPMThresholdsList(blnConfirmReplaceExistingResults) Then
            cboMassTolType.SelectedIndex = udtPredefinedThresholds.MassTolType

            For intIndex = 0 To udtPredefinedThresholds.Thresholds.Length - 1
                AddPMThresholdRow(udtPredefinedThresholds.Thresholds(intIndex).MassTolerance, udtPredefinedThresholds.Thresholds(intIndex).NETTolerance)
            Next intIndex

        End If

    End Sub

    Private Sub AutoPopulatePMThresholdsByID(ByVal ePredefinedPMThreshold As PredefinedPMThresholdsConstants, ByVal blnConfirmReplaceExistingResults As Boolean)

        Try
            AutoPopulatePMThresholds(mPredefinedPMThresholds(ePredefinedPMThreshold), blnConfirmReplaceExistingResults)
        Catch ex As Exception
            Windows.Forms.MessageBox.Show("Error calling AutoPopulatePMThresholds in AutoPopulatePMThresholdsByID: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try

    End Sub

    Private Function ClearPMThresholdsList(ByVal blnConfirmReplaceExistingResults As Boolean) As Boolean
        ' Returns true if the PM_THRESHOLDS_DATATABLE is empty or if it was cleared
        ' Returns false if the user is queried about clearing and they do not click Yes

        Dim eResult As System.Windows.Forms.DialogResult
        Dim blnSuccess As Boolean

        blnSuccess = False
        With mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE)
            If .Rows.Count > 0 Then
                If blnConfirmReplaceExistingResults Then
                    eResult = Windows.Forms.MessageBox.Show("Are you sure you want to clear the thresholds?", "Clear Thresholds", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
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

        Dim strSequence As String
        Dim sngpI As Single
        Dim sngHydrophobicity As Single
        Dim intCharge As Integer
        Dim strMessage As String

        If txtSequenceForpI.TextLength = 0 Then Exit Sub

        If objpICalculator Is Nothing Then
            objpICalculator = New clspICalculation
        End If

        strSequence = txtSequenceForpI.Text

        objpICalculator.HydrophobicityType = CType(cboHydrophobicityMode.SelectedIndex, clspICalculation.eHydrophobicityTypeConstants)
        objpICalculator.ReportMaximumpI = chkMaxpIModeEnabled.Checked
        objpICalculator.SequenceWidthToExamineForMaximumpI = LookupMaxpISequenceLength()

        sngpI = objpICalculator.CalculateSequencepI(strSequence)
        sngHydrophobicity = objpICalculator.CalculateSequenceHydrophobicity(strSequence)
        intCharge = objpICalculator.CalculateSequenceChargeState(strSequence, sngpI)

        strMessage = "pI = " & sngpI.ToString & ControlChars.NewLine
        strMessage &= "Hydrophobicity = " & sngHydrophobicity.ToString & ControlChars.NewLine
        'strMessage &= "Predicted charge state = " & intCharge.ToString & " at pH = " & sngpI.ToString

        txtpIStats.Text = strMessage
    End Sub

    Private Function ConfirmFilePaths() As Boolean
        If txtProteinInputFilePath.TextLength = 0 Then
            Windows.Forms.MessageBox.Show("Please define an input file path", "Missing Value", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            txtProteinInputFilePath.Focus()
            Return False
        ElseIf txtProteinOutputFilePath.TextLength = 0 Then
            Windows.Forms.MessageBox.Show("Please define an output file path", "Missing Value", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
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

    Private Sub DefineDefaultPMThresholdAppendItem(ByRef udtPMThreshold As udtPredefinedPMThresholdsType, ByVal dblMassTolerance As Double, ByVal dblNETTolerance As Double)
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

        If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.DelimitedText Then
            blnEnableDelimitedFileOptions = True
        ElseIf cboInputFileFormat.SelectedIndex = InputFileFormatConstants.FastaFile OrElse _
            txtProteinInputFilePath.TextLength = 0 OrElse _
            System.IO.Path.GetFileName(txtProteinInputFilePath.Text).ToLower.IndexOf(".fasta") > 0 Then
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

        If cboInputFileFormat.SelectedIndex = InputFileFormatConstants.FastaFile OrElse _
            System.IO.Path.GetFileName(txtProteinInputFilePath.Text).ToLower.IndexOf(".fasta") > 0 Then
            cmdValidateFastaFile.Enabled = True
        Else
            cmdValidateFastaFile.Enabled = False
        End If

        chkCreateFastaOutputFile.Enabled = Not blnEnableDigestionOptions


        fraDigestionOptions.Enabled = blnEnableDigestionOptions
        chkIncludePrefixAndSuffixResidues.Enabled = blnEnableDigestionOptions

        txtOutputFileFieldDelimeter.Enabled = (cboOutputFileFieldDelimeter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Other)
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

        txtDigestProteinsMinimumpI.Enabled = blnEnableDigestionOptions And chkComputepI.Checked
        txtDigestProteinsMaximumpI.Enabled = blnEnableDigestionOptions And chkComputepI.Checked

    End Sub

    Private Sub GenerateUniquenessStats()

        Dim objProteinDigestionSimulator As clsProteinDigestionSimulator
        Dim objLogger As PRISM.Logging.ILogger

        Dim strLogFilePath As String
        Dim strOutputFilePath As String
        Dim intCharLoc As Integer

        Dim intBinStartMass, intBinEndMass As Integer

        Dim myDataRow As System.Data.DataRow

        Dim blnClearExisting As Boolean
        Dim blnAutoDefineSLiCScoreThresholds As Boolean
        Dim eMassToleranceType As clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants

        Dim strNewConnectionString As String

        Dim blnSuccess As Boolean
        Dim blnError As Boolean

        If Not mWorking AndAlso ConfirmFilePaths() Then
            Try

                If mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE).Rows.Count = 0 Then
                    Windows.Forms.MessageBox.Show("Please define one or more peak matching thresholds before proceeding.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    Exit Try
                End If

                If chkEnableLogging.Checked Then
                    strLogFilePath = System.IO.Path.Combine(GetAppFolderPath(), "ProteinDigestionSimulatorLog")
                    objLogger = New PRISM.Logging.clsFileLogger(strLogFilePath)
                    objProteinDigestionSimulator = New clsProteinDigestionSimulator(False, objLogger)
                Else
                    objProteinDigestionSimulator = New clsProteinDigestionSimulator
                End If

                blnSuccess = InitializeProteinFileParserGeneralOptions(objProteinDigestionSimulator.ProteinFileParser)
                If Not blnSuccess Then Exit Try

                strOutputFilePath = txtProteinOutputFilePath.Text

                If Not System.IO.Path.IsPathRooted(strOutputFilePath) Then
                    strOutputFilePath = System.IO.Path.Combine(GetAppFolderPath(), strOutputFilePath)
                End If

                If System.IO.Directory.Exists(strOutputFilePath) Then
                    ' strOutputFilePath points to a folder and not a file
                    strOutputFilePath = System.IO.Path.Combine(strOutputFilePath, System.IO.Path.GetFileNameWithoutExtension(txtProteinInputFilePath.Text) & PEAK_MATCHING_STATS_FILE_SUFFIX)
                Else
                    ' Replace _output.txt" in strOutputFilePath with PEAK_MATCHING_STATS_FILE_SUFFIX
                    intCharLoc = strOutputFilePath.ToLower.IndexOf(OUTPUT_FILE_SUFFIX.ToLower)
                    If intCharLoc > 0 Then
                        strOutputFilePath = strOutputFilePath.Substring(0, intCharLoc) & PEAK_MATCHING_STATS_FILE_SUFFIX
                    Else
                        strOutputFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(strOutputFilePath), System.IO.Path.GetFileNameWithoutExtension(strOutputFilePath) & PEAK_MATCHING_STATS_FILE_SUFFIX)
                    End If
                End If

                ' Check input file size and possibly warn user to enable/disable Sql Server DB Usage
                If chkAllowSqlServerCaching.Checked Then
                    If Not ValidateSqlServerCachingOptionsForInputFile(txtProteinInputFilePath.Text, chkAssumeInputFileIsDigested.Checked, objProteinDigestionSimulator.ProteinFileParser) Then
                        Exit Try
                    End If
                End If

                With objProteinDigestionSimulator

                    eMassToleranceType = CType(cboMassTolType.SelectedIndex, clsPeakMatchingClass.clsSearchThresholds.MassToleranceConstants)
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

                    Me.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor
                    mWorking = True
                    cmdGenerateUniquenessStats.Enabled = False

                    blnSuccess = .GenerateUniquenessStats(txtProteinInputFilePath.Text, System.IO.Path.GetDirectoryName(strOutputFilePath), System.IO.Path.GetFileNameWithoutExtension(strOutputFilePath))

                    Me.Cursor.Current = System.Windows.Forms.Cursors.Default

                    If Not blnSuccess Then
                        Windows.Forms.MessageBox.Show("Unable to Generate Uniqueness Stats: " & .GetErrorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    Else
                        Windows.Forms.MessageBox.Show("Uniqueness stats calculation complete ", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End With

            Catch ex As Exception
                Windows.Forms.MessageBox.Show("Error in frmMain->GenerateUniquenessStats: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Finally
                mWorking = False
                cmdGenerateUniquenessStats.Enabled = True
                objProteinDigestionSimulator = Nothing
            End Try
        End If

    End Sub

    Private Function GetAppFolderPath() As String
        Return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
    End Function

    Private Function GetSettingsFilePath() As String
        Return System.IO.Path.Combine(GetAppFolderPath(), XML_SETTINGS_FILE_NAME)
    End Function

    Private Sub IniFileLoadOptions()

        Const OptionsSection As String = clsParseProteinFile.XML_SECTION_OPTIONS
        Const FASTAOptions As String = clsParseProteinFile.XML_SECTION_FASTA_OPTIONS
        Const ProcessingOptions As String = clsParseProteinFile.XML_SECTION_PROCESSING_OPTIONS
        Const DigestionOptions As String = clsParseProteinFile.XML_SECTION_DIGESTION_OPTIONS
        Const UniquenessStatsOptions As String = clsParseProteinFile.XML_SECTION_UNIQUENESS_STATS_OPTIONS
        Const PMOptions As String = clsProteinDigestionSimulator.XML_SECTION_PEAK_MATCHING_OPTIONS

        Const MAX_AUTO_WINDOW_HEIGHT As Integer = 650

        Dim objXmlFile As New PRISM.Files.XmlSettingsFileAccessor

        Dim blnAutoDefineSLiCScoreThresholds As Boolean
        Dim valueNotPresent As Boolean
        Dim blnListCleared As Boolean
        Dim blnRadioButtonChecked As Boolean

        Dim intIndex As Integer
        Dim intWindowHeight As Integer

        Dim strThresholdData As String
        Dim strThresholds() As String
        Dim strThresholdDetails() As String

        Dim ColumnDelimiters() As Char = New Char() {ControlChars.Tab, ","c}

        ResetToDefaults(False)

        Try
            With objXmlFile
                ' Pass False to .LoadSettings() here to turn off case sensitive matching
                .LoadSettings(GetSettingsFilePath(), False)

                Try
                    txtProteinInputFilePath.Text = .GetParam(OptionsSection, "InputFilePath", txtProteinInputFilePath.Text)

                    cboInputFileFormat.SelectedIndex = .GetParam(OptionsSection, "InputFileFormatIndex", cboInputFileFormat.SelectedIndex)
                    cboInputFileColumnDelimiter.SelectedIndex = .GetParam(OptionsSection, "InputFileColumnDelimiterIndex", cboInputFileColumnDelimiter.SelectedIndex)
                    txtInputFileColumnDelimiter.Text = .GetParam(OptionsSection, "InputFileColumnDelimiter", txtInputFileColumnDelimiter.Text)

                    cboInputFileColumnOrdering.SelectedIndex = .GetParam(OptionsSection, "InputFileColumnOrdering", cboInputFileColumnOrdering.SelectedIndex)

                    cboOutputFileFieldDelimeter.SelectedIndex = .GetParam(OptionsSection, "OutputFileFieldDelimeterIndex", cboOutputFileFieldDelimeter.SelectedIndex)
                    txtOutputFileFieldDelimeter.Text = .GetParam(OptionsSection, "OutputFileFieldDelimeter", txtOutputFileFieldDelimeter.Text)

                    chkIncludePrefixAndSuffixResidues.Checked = .GetParam(OptionsSection, "IncludePrefixAndSuffixResidues", chkIncludePrefixAndSuffixResidues.Checked)
                    chkEnableLogging.Checked = .GetParam(OptionsSection, "EnableLogging", chkEnableLogging.Checked)

                    mCustomValidationRulesFilePath = .GetParam(OptionsSection, "CustomValidationRulesFilePath", String.Empty)

                    Me.Width = .GetParam(OptionsSection, "WindowWidth", Me.Width)
                    intWindowHeight = .GetParam(OptionsSection, "WindowHeight", Me.Height)
                    If intWindowHeight > MAX_AUTO_WINDOW_HEIGHT Then
                        intWindowHeight = MAX_AUTO_WINDOW_HEIGHT
                    End If
                    Me.Height = intWindowHeight

                    txtRefStartChar.Text = .GetParam(FASTAOptions, "RefStartChar", txtRefStartChar.Text)
                    cboRefEndChar.SelectedIndex = .GetParam(FASTAOptions, "RefEndCharIndex", cboRefEndChar.SelectedIndex)
                    txtRefEndChar.Text = .GetParam(FASTAOptions, "RefEndChar", txtRefEndChar.Text)

                    chkLookForAddnlRefInDescription.Checked = .GetParam(FASTAOptions, "LookForAddnlRefInDescription", chkLookForAddnlRefInDescription.Checked)
                    txtAddnlRefSepChar.Text = .GetParam(FASTAOptions, "AddnlRefSepChar", txtAddnlRefSepChar.Text)
                    txtAddnlRefAccessionSepChar.Text = .GetParam(FASTAOptions, "AddnlRefAccessionSepChar", txtAddnlRefAccessionSepChar.Text)

                    chkExcludeProteinSequence.Checked = .GetParam(ProcessingOptions, "ExcludeProteinSequence", chkExcludeProteinSequence.Checked)
                    chkComputeProteinMass.Checked = .GetParam(ProcessingOptions, "ComputeProteinMass", chkComputeProteinMass.Checked)
                    chkComputepI.Checked = .GetParam(ProcessingOptions, "ComputepI", chkComputepI.Checked)
                    chkIncludeXResidues.Checked = .GetParam(ProcessingOptions, "IncludeXResidues", chkIncludeXResidues.Checked)
                    chkDigestProteins.Checked = .GetParam(ProcessingOptions, "DigestProteins", chkDigestProteins.Checked)
                    cboProteinReversalOptions.SelectedIndex = .GetParam(ProcessingOptions, "ProteinReversalIndex", cboProteinReversalOptions.SelectedIndex)
                    txtProteinScramblingLoopCount.Text = .GetParam(ProcessingOptions, "ProteinScramblingLoopCount", txtProteinScramblingLoopCount.Text)

                    Try
                        cboHydrophobicityMode.SelectedIndex = .GetParam(ProcessingOptions, "HydrophobicityMode", cboHydrophobicityMode.SelectedIndex)
                    Catch ex As Exception
                        ' Ignore errors setting the selected index
                    End Try
                    chkMaxpIModeEnabled.Checked = .GetParam(ProcessingOptions, "MaxpIModeEnabled", chkMaxpIModeEnabled.Checked)
                    txtMaxpISequenceLength.Text = .GetParam(ProcessingOptions, "MaxpISequenceLength", LookupMaxpISequenceLength).ToString

                    cboCleavageRuleType.SelectedIndex = .GetParam(DigestionOptions, "CleavageRuleTypeIndex", cboCleavageRuleType.SelectedIndex)
                    chkIncludeDuplicateSequences.Checked = .GetParam(DigestionOptions, "IncludeDuplicateSequences", chkIncludeDuplicateSequences.Checked)
                    chkCysPeptidesOnly.Checked = .GetParam(DigestionOptions, "CysPeptidesOnly", chkCysPeptidesOnly.Checked)

                    txtDigestProteinsMinimumMass.Text = .GetParam(DigestionOptions, "DigestProteinsMinimumMass", txtDigestProteinsMinimumMass.Text)
                    txtDigestProteinsMaximumMass.Text = .GetParam(DigestionOptions, "DigestProteinsMaximumMass", txtDigestProteinsMaximumMass.Text)
                    txtDigestProteinsMinimumResidueCount.Text = .GetParam(DigestionOptions, "DigestProteinsMinimumResidueCount", txtDigestProteinsMinimumResidueCount.Text)
                    txtDigestProteinsMaximumMissedCleavages.Text = .GetParam(DigestionOptions, "DigestProteinsMaximumMissedCleavages", txtDigestProteinsMaximumMissedCleavages.Text)

                    txtDigestProteinsMinimumpI.Text = .GetParam(DigestionOptions, "DigestProteinsMinimumpI", txtDigestProteinsMinimumpI.Text)
                    txtDigestProteinsMaximumpI.Text = .GetParam(DigestionOptions, "DigestProteinsMaximumpI", txtDigestProteinsMaximumpI.Text)

                    ' Load Uniqueness Options
                    chkAssumeInputFileIsDigested.Checked = .GetParam(UniquenessStatsOptions, "AssumeInputFileIsDigested", chkAssumeInputFileIsDigested.Checked)

                    txtUniquenessBinWidth.Text = .GetParam(UniquenessStatsOptions, "UniquenessBinWidth", txtUniquenessBinWidth.Text)
                    chkAutoComputeRangeForBinning.Checked = .GetParam(UniquenessStatsOptions, "AutoComputeRangeForBinning", chkAutoComputeRangeForBinning.Checked)
                    txtUniquenessBinStartMass.Text = .GetParam(UniquenessStatsOptions, "UniquenessBinStartMass", txtUniquenessBinStartMass.Text)
                    txtUniquenessBinEndMass.Text = .GetParam(UniquenessStatsOptions, "UniquenessBinEndMass", txtUniquenessBinEndMass.Text)

                    txtMaxPeakMatchingResultsPerFeatureToSave.Text = .GetParam(UniquenessStatsOptions, "MaxPeakMatchingResultsPerFeatureToSave", txtMaxPeakMatchingResultsPerFeatureToSave.Text)
                    chkUseSLiCScoreForUniqueness.Checked = .GetParam(UniquenessStatsOptions, "UseSLiCScoreForUniqueness", chkUseSLiCScoreForUniqueness.Checked)
                    txtMinimumSLiCScore.Text = .GetParam(UniquenessStatsOptions, "MinimumSLiCScore", txtMinimumSLiCScore.Text)
                    blnRadioButtonChecked = .GetParam(UniquenessStatsOptions, "UseEllipseSearchRegion", True)
                    If blnRadioButtonChecked Then
                        optUseEllipseSearchRegion.Checked = blnRadioButtonChecked
                    Else
                        optUseRectangleSearchRegion.Checked = blnRadioButtonChecked
                    End If

                    ''chkAllowSqlServerCaching.Checked = .GetParam(UniquenessStatsOptions, "AllowSqlServerCaching", chkAllowSqlServerCaching.Checked)
                    ''chkUseSqlServerDBToCacheData.Checked = .GetParam(UniquenessStatsOptions, "UseSqlServerDBToCacheData", chkUseSqlServerDBToCacheData.Checked)
                    ''txtSqlServerName.Text = .GetParam(UniquenessStatsOptions, "SqlServerName", txtSqlServerName.Text)
                    ''txtSqlServerDatabase.Text = .GetParam(UniquenessStatsOptions, "SqlServerDatabase", txtSqlServerDatabase.Text)
                    ''chkSqlServerUseIntegratedSecurity.Checked = .GetParam(UniquenessStatsOptions, "SqlServerUseIntegratedSecurity", chkSqlServerUseIntegratedSecurity.Checked)

                    ''chkSqlServerUseExistingData.Checked = .GetParam(UniquenessStatsOptions, "SqlServerUseExistingData", chkSqlServerUseExistingData.Checked)

                    ''txtSqlServerUsername.Text = .GetParam(UniquenessStatsOptions, "SqlServerUsername", txtSqlServerUsername.Text)
                    ''txtSqlServerPassword.Text = .GetParam(UniquenessStatsOptions, "SqlServerPassword", txtSqlServerPassword.Text)

                    ' Load the peak matching thresholds
                    cboMassTolType.SelectedIndex = .GetParam(PMOptions, "MassToleranceType", cboMassTolType.SelectedIndex)
                    chkAutoDefineSLiCScoreTolerances.Checked = .GetParam(PMOptions, "AutoDefineSLiCScoreThresholds", chkAutoDefineSLiCScoreTolerances.Checked)
                    blnAutoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked

                    ' See if any peak matching data is present
                    ' If it is, clear the table and load it; if not, leave the table unchanged

                    valueNotPresent = False
                    strThresholdData = .GetParam(PMOptions, "ThresholdData", String.Empty, valueNotPresent)

                    If Not valueNotPresent AndAlso Not strThresholdData Is Nothing AndAlso strThresholdData.Length > 0 Then
                        strThresholds = strThresholdData.Split(";"c)

                        If strThresholds.Length > 0 Then
                            ClearPMThresholdsList(False)

                            For intIndex = 0 To strThresholds.Length - 1
                                strThresholdDetails = strThresholds(intIndex).Split(ColumnDelimiters)

                                If strThresholdDetails.Length > 2 AndAlso Not blnAutoDefineSLiCScoreThresholds Then
                                    If IsNumeric(strThresholdDetails(0)) And IsNumeric(strThresholdDetails(1)) And _
                                        IsNumeric(strThresholdDetails(2)) And IsNumeric(strThresholdDetails(3)) Then
                                        AddPMThresholdRow(CDbl(strThresholdDetails(0)), CDbl(strThresholdDetails(1)), _
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
                    System.Windows.Forms.MessageBox.Show("Invalid parameter in settings file: " & System.IO.Path.GetFileName(GetSettingsFilePath()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End Try
            End With

        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show("Error loading settings from file: " & GetSettingsFilePath(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try

    End Sub

    Private Sub IniFileSaveOptions(Optional ByVal blnSaveWindowDimensionsOnly As Boolean = False)

        Const OptionsSection As String = clsParseProteinFile.XML_SECTION_OPTIONS
        Const FASTAOptions As String = clsParseProteinFile.XML_SECTION_FASTA_OPTIONS
        Const ProcessingOptions As String = clsParseProteinFile.XML_SECTION_PROCESSING_OPTIONS
        Const DigestionOptions As String = clsParseProteinFile.XML_SECTION_DIGESTION_OPTIONS
        Const UniquenessStatsOptions As String = clsParseProteinFile.XML_SECTION_UNIQUENESS_STATS_OPTIONS
        Const PMOptions As String = clsProteinDigestionSimulator.XML_SECTION_PEAK_MATCHING_OPTIONS

        Dim objXmlFile As New PRISM.Files.XmlSettingsFileAccessor
        Dim myDataRow As System.Data.DataRow
        Dim blnAutoDefineSLiCScoreThresholds As Boolean
        Dim strThresholdData As String

        Try
            With objXmlFile
                ' Pass True to .LoadSettings() to turn on case sensitive matching
                .LoadSettings(GetSettingsFilePath(), True)

                Try
                    If Not blnSaveWindowDimensionsOnly Then
                        .SetParam(OptionsSection, "InputFilePath", txtProteinInputFilePath.Text)
                        .SetParam(OptionsSection, "InputFileFormatIndex", cboInputFileFormat.SelectedIndex)
                        .SetParam(OptionsSection, "InputFileColumnDelimiterIndex", cboInputFileColumnDelimiter.SelectedIndex)
                        .SetParam(OptionsSection, "InputFileColumnDelimiter", txtInputFileColumnDelimiter.Text)

                        .SetParam(OptionsSection, "InputFileColumnOrdering", cboInputFileColumnOrdering.SelectedIndex)


                        .SetParam(OptionsSection, "OutputFileFieldDelimeterIndex", cboOutputFileFieldDelimeter.SelectedIndex)
                        .SetParam(OptionsSection, "OutputFileFieldDelimeter", txtOutputFileFieldDelimeter.Text)

                        .SetParam(OptionsSection, "IncludePrefixAndSuffixResidues", chkIncludePrefixAndSuffixResidues.Checked)
                        .SetParam(OptionsSection, "EnableLogging", chkEnableLogging.Checked)

                        .SetParam(OptionsSection, "CustomValidationRulesFilePath", mCustomValidationRulesFilePath)
                    End If

                    .SetParam(OptionsSection, "WindowWidth", Me.Width)
                    .SetParam(OptionsSection, "WindowHeight", Me.Height)

                    If Not blnSaveWindowDimensionsOnly Then
                        .SetParam(FASTAOptions, "RefStartChar", txtRefStartChar.Text)
                        .SetParam(FASTAOptions, "RefEndCharIndex", cboRefEndChar.SelectedIndex)
                        .SetParam(FASTAOptions, "RefEndChar", txtRefEndChar.Text)

                        .SetParam(FASTAOptions, "LookForAddnlRefInDescription", chkLookForAddnlRefInDescription.Checked)
                        .SetParam(FASTAOptions, "AddnlRefSepChar", txtAddnlRefSepChar.Text)
                        .SetParam(FASTAOptions, "AddnlRefAccessionSepChar", txtAddnlRefAccessionSepChar.Text)

                        .SetParam(ProcessingOptions, "ExcludeProteinSequence", chkExcludeProteinSequence.Checked)
                        .SetParam(ProcessingOptions, "ComputeProteinMass", chkComputeProteinMass.Checked)
                        .SetParam(ProcessingOptions, "ComputepI", chkComputepI.Checked)
                        .SetParam(ProcessingOptions, "IncludeXResidues", chkIncludeXResidues.Checked)
                        .SetParam(ProcessingOptions, "DigestProteins", chkDigestProteins.Checked)
                        .SetParam(ProcessingOptions, "ProteinReversalIndex", cboProteinReversalOptions.SelectedIndex)
                        .SetParam(ProcessingOptions, "ProteinScramblingLoopCount", txtProteinScramblingLoopCount.text)

                        .SetParam(ProcessingOptions, "HydrophobicityMode", cboHydrophobicityMode.SelectedIndex)
                        .SetParam(ProcessingOptions, "MaxpIModeEnabled", chkMaxpIModeEnabled.Checked)
                        .SetParam(ProcessingOptions, "MaxpISequenceLength", LookupMaxpISequenceLength())

                        .SetParam(DigestionOptions, "CleavageRuleTypeIndex", cboCleavageRuleType.SelectedIndex)
                        .SetParam(DigestionOptions, "IncludeDuplicateSequences", chkIncludeDuplicateSequences.Checked)
                        .SetParam(DigestionOptions, "CysPeptidesOnly", chkCysPeptidesOnly.Checked)

                        .SetParam(DigestionOptions, "DigestProteinsMinimumMass", txtDigestProteinsMinimumMass.Text)
                        .SetParam(DigestionOptions, "DigestProteinsMaximumMass", txtDigestProteinsMaximumMass.Text)
                        .SetParam(DigestionOptions, "DigestProteinsMinimumResidueCount", txtDigestProteinsMinimumResidueCount.Text)
                        .SetParam(DigestionOptions, "DigestProteinsMaximumMissedCleavages", txtDigestProteinsMaximumMissedCleavages.Text)

                        .SetParam(DigestionOptions, "DigestProteinsMinimumpI", txtDigestProteinsMinimumpI.Text)
                        .SetParam(DigestionOptions, "DigestProteinsMaximumpI", txtDigestProteinsMaximumpI.Text)

                        ' Load Uniqueness Options
                        .SetParam(UniquenessStatsOptions, "AssumeInputFileIsDigested", chkAssumeInputFileIsDigested.Checked)

                        .SetParam(UniquenessStatsOptions, "UniquenessBinWidth", txtUniquenessBinWidth.Text)
                        .SetParam(UniquenessStatsOptions, "AutoComputeRangeForBinning", chkAutoComputeRangeForBinning.Checked)
                        .SetParam(UniquenessStatsOptions, "UniquenessBinStartMass", txtUniquenessBinStartMass.Text)
                        .SetParam(UniquenessStatsOptions, "UniquenessBinEndMass", txtUniquenessBinEndMass.Text)

                        .SetParam(UniquenessStatsOptions, "MaxPeakMatchingResultsPerFeatureToSave", txtMaxPeakMatchingResultsPerFeatureToSave.Text)
                        .SetParam(UniquenessStatsOptions, "UseSLiCScoreForUniqueness", chkUseSLiCScoreForUniqueness.Checked)
                        .SetParam(UniquenessStatsOptions, "MinimumSLiCScore", txtMinimumSLiCScore.Text)
                        .SetParam(UniquenessStatsOptions, "UseEllipseSearchRegion", optUseEllipseSearchRegion.Checked)

                        ''.SetParam(UniquenessStatsOptions, "AllowSqlServerCaching", chkAllowSqlServerCaching.Checked)
                        ''.SetParam(UniquenessStatsOptions, "UseSqlServerDBToCacheData", chkUseSqlServerDBToCacheData.Checked)
                        ''.SetParam(UniquenessStatsOptions, "SqlServerName", txtSqlServerName.Text)
                        ''.SetParam(UniquenessStatsOptions, "SqlServerDatabase", txtSqlServerDatabase.Text)
                        ''.SetParam(UniquenessStatsOptions, "SqlServerUseIntegratedSecurity", chkSqlServerUseIntegratedSecurity.Checked)
                        ''.SetParam(UniquenessStatsOptions, "SqlServerUseExistingData", chkSqlServerUseExistingData.Checked)
                        ''.SetParam(UniquenessStatsOptions, "SqlServerUsername", txtSqlServerUsername.Text)
                        ''.SetParam(UniquenessStatsOptions, "SqlServerPassword", txtSqlServerPassword.Text)


                        ' Save the peak matching thresholds
                        .SetParam(PMOptions, "MassToleranceType", cboMassTolType.SelectedIndex.ToString)

                        blnAutoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked
                        .SetParam(PMOptions, "AutoDefineSLiCScoreThresholds", blnAutoDefineSLiCScoreThresholds.ToString)

                        strThresholdData = String.Empty
                        For Each myDataRow In mPeakMatchingThresholdsDataset.Tables(PM_THRESHOLDS_DATATABLE).Rows
                            If strThresholdData.Length > 0 Then strThresholdData &= "; "
                            If blnAutoDefineSLiCScoreThresholds Then
                                strThresholdData &= CStr(myDataRow.Item(0)) & "," & CStr(myDataRow.Item(1))
                            Else
                                strThresholdData &= CStr(myDataRow.Item(0)) & "," & CStr(myDataRow.Item(1)) & "," & CStr(myDataRow.Item(2)) & "," & CStr(myDataRow.Item(3))
                            End If
                        Next myDataRow
                        .SetParam(PMOptions, "ThresholdData", strThresholdData)
                    End If
                Catch ex As Exception
                    System.Windows.Forms.MessageBox.Show("Error storing parameter in settings file: " & System.IO.Path.GetFileName(GetSettingsFilePath()), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End Try

                .SaveSettings()
            End With
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show("Error saving settings to file: " & GetSettingsFilePath(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End Try


    End Sub

    Private Sub InitializeControls()
        mDefaultFastaFileOptions = New clsParseProteinFile.FastaFileOptionsClass
        mDefaultFastaFileOptions.ReadonlyClass = True

        DefineDefaultPMThresholds()

#If IncludePNNLNETRoutines Then
        Me.Text = "Protein Digestion Simulator"
        lblUniquenessCalculationsNote.Text = "The Protein Digestion Simulator uses an elution time prediction algorithm developed by Lars Kangas and Kostas Petritis. See Help->About Elution Time Prediction for more info.  Note that you can provide custom time values for peptides by separately generating a tab or comma delimited text file with information corresponding to one of the options in the 'Column Order' list on the 'File Format' option tab, then checking 'Assume Input file is Already Digested' on this tab."
#Else
        Me.Text = "Protein Digestion Simulator Basic"
        lblUniquenessCalculationsNote.Text = "The Protein Digestion Simulator Basic uses an elution time prediction algorithm developed by Oleg Krokhin. See Help->About Elution Time Prediction for more info.  Note that you can provide custom time values for peptides by separately generating a tab or comma delimited text file with information corresponding to one of the options in the 'Column Order' list on the 'File Format' option tab, then checking 'Assume Input file is Already Digested' on this tab."
#End If

        PopulateComboBoxes()
        InitializePeakMatchingDataGrid()

        IniFileLoadOptions()
        SetToolTips()

        EnableDisableControls()
    End Sub

    Private Sub InitializePeakMatchingDataGrid()


        ' Make the Peak Matching Thresholds datatable
        Dim dtPMThresholds As System.Data.DataTable = New System.Data.DataTable(PM_THRESHOLDS_DATATABLE)

        ' Add the columns to the datatable
        SharedVBNetRoutines.ADONetRoutines.AppendColumnDoubleToTable(dtPMThresholds, COL_NAME_MASS_TOLERANCE)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnDoubleToTable(dtPMThresholds, COL_NAME_NET_TOLERANCE)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnDoubleToTable(dtPMThresholds, COL_NAME_SLIC_MASS_STDEV, DEFAULT_SLIC_MASS_STDEV)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnDoubleToTable(dtPMThresholds, COL_NAME_SLIC_NET_STDEV, DEFAULT_SLIC_NET_STDEV)
        SharedVBNetRoutines.ADONetRoutines.AppendColumnIntegerToTable(dtPMThresholds, COL_NAME_PM_THRESHOLD_ROW_ID, 0, True, True)

        With dtPMThresholds
            Dim PrimaryKeyColumn As System.Data.DataColumn() = New System.Data.DataColumn() {.Columns(COL_NAME_PM_THRESHOLD_ROW_ID)}
            .PrimaryKey = PrimaryKeyColumn
        End With

        ' Instantiate the dataset
        mPeakMatchingThresholdsDataset = New System.Data.DataSet(PM_THRESHOLDS_DATATABLE)

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
        Dim tsPMThresholdsTableStyle As System.Windows.Forms.DataGridTableStyle

        ' Define the PM Thresholds table style 
        tsPMThresholdsTableStyle = New System.Windows.Forms.DataGridTableStyle

        ' Setting the MappingName of the table style to PM_THRESHOLDS_DATATABLE will cause this style to be used with that table
        With tsPMThresholdsTableStyle
            .MappingName = PM_THRESHOLDS_DATATABLE
            .AllowSorting = True
            .ColumnHeadersVisible = True
            .RowHeadersVisible = True
            .ReadOnly = False
        End With

        SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_MASS_TOLERANCE, "Mass Tolerance", 90)
#If IncludePNNLNETRoutines Then
        SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_NET_TOLERANCE, "NET Tolerance", 90)
#Else
        SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_NET_TOLERANCE, "Time Tolerance", 90)
#End If

        If chkAutoDefineSLiCScoreTolerances.Checked Then
            dgPeakMatchingThresholds.Width = 250
        Else
            dgPeakMatchingThresholds.Width = 425
            SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_MASS_STDEV, "SLiC Mass StDev", 90)
#If IncludePNNLNETRoutines Then
            SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_NET_STDEV, "SLiC NET StDev", 90)
#Else
            SharedVBNetRoutines.ADONetRoutines.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_NET_STDEV, "SLiC Time StDev", 90)
#End If
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

            .DelimitedFileFormatCode = CType(cboInputFileColumnOrdering.SelectedIndex, ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode)

            .InputFileDelimiter = LookupColumnDelimiter(cboInputFileColumnDelimiter, txtInputFileColumnDelimiter, ControlChars.Tab)
            .OutputFileDelimiter = LookupColumnDelimiter(cboOutputFileFieldDelimeter, txtOutputFileFieldDelimeter, ControlChars.Tab)

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
            .ComputepI = chkComputepI.Checked

            .HydrophobicityType = CType(cboHydrophobicityMode.SelectedIndex, clspICalculation.eHydrophobicityTypeConstants)
            .ReportMaximumpI = chkMaxpIModeEnabled.Checked
            .SequenceWidthToExamineForMaximumpI = LookupMaxpISequenceLength()

            .IncludeXResiduesInMass = chkIncludeXResidues.Checked

            .GenerateUniqueIDValuesForPeptides = chkGenerateUniqueIDValues.Checked

            With .DigestionOptions
                .CleavageRuleID = CType(cboCleavageRuleType.SelectedIndex, clsInSilicoDigest.CleavageRuleConstants)
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

            .ShowMessages = True
        End With

        Return True

    End Function

    Private Function LookupColumnDelimiter(ByVal DelimiterCombobox As ComboBox, ByVal DelimiterTextbox As TextBox, ByVal strDefaultDelimiter As Char) As Char
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
            intLength = SharedVBNetRoutines.VBNetRoutines.ParseTextboxValueInt(txtMaxpISequenceLength, String.Empty, blnError, 10)
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
        Dim objToolTipControl As New System.Windows.Forms.ToolTip

        With objToolTipControl
            .SetToolTip(cmdParseInputFile, "Parse proteins in input file to create output file(s).")
            .SetToolTip(cboInputFileColumnDelimiter, "Character separating columns in a delimited text input file.")
            .SetToolTip(txtInputFileColumnDelimiter, "Custom character separating columns in a delimited text input file.")
            .SetToolTip(txtOutputFileFieldDelimeter, "Character separating the fields in the output file.")
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

            .SetToolTip(chkExcludeProteinSequence, "Enabling this setting will prevent protein sequences from being written to the output file; useful when processing extremely large files.")
        End With

        objToolTipControl = Nothing

    End Sub

    Private Sub ParseProteinInputFile()
        Dim objParseProteinFile As clsParseProteinFile
        Dim blnSuccess As Boolean

        If Not mWorking AndAlso ConfirmFilePaths() Then
            Try
                objParseProteinFile = New clsParseProteinFile
                blnSuccess = InitializeProteinFileParserGeneralOptions(objParseProteinFile)
                If Not blnSuccess Then Exit Try

                With objParseProteinFile
                    .CreateProteinOutputFile = True
                    .ProteinScramblingMode = CType(cboProteinReversalOptions.SelectedIndex, clsParseProteinFile.ProteinScramblingModeConstants)
                    .ProteinScramblingSamplingPercentage = SharedVBNetRoutines.VBNetRoutines.ParseTextboxValueInt(txtProteinReversalSamplingPercentage, "", False, 100, False)
                    .ProteinScramblingLoopCount = SharedVBNetRoutines.VBNetRoutines.ParseTextboxValueInt(txtProteinScramblingLoopCount, "", False, 1, False)
                    .CreateDigestedProteinOutputFile = chkDigestProteins.Checked
                    .CreateFastaOutputFile = chkCreateFastaOutputFile.Checked

                    Me.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor
                    mWorking = True
                    cmdParseInputFile.Enabled = False

                    If txtProteinOutputFilePath.TextLength > 0 Then
                        blnSuccess = .ParseProteinFile(txtProteinInputFilePath.Text, _
                                            System.IO.Path.GetDirectoryName(txtProteinOutputFilePath.Text), _
                                            System.IO.Path.GetFileNameWithoutExtension(txtProteinOutputFilePath.Text))
                    Else
                        blnSuccess = .ParseProteinFile(txtProteinInputFilePath.Text, "", "")
                    End If

                    Me.Cursor.Current = System.Windows.Forms.Cursors.Default

                    If Not blnSuccess Then
                        Windows.Forms.MessageBox.Show("Error parsing protein file: " & .GetErrorMessage(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    End If
                End With

            Catch ex As Exception
                Windows.Forms.MessageBox.Show("Error in frmMain->ParseProteinInputFile: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Finally
                mWorking = False
                cmdParseInputFile.Enabled = True
                objParseProteinFile = Nothing
            End Try
        End If

    End Sub

    Private Function ParseTextboxValueInt(ByVal ThisTextBox As TextBox, ByVal strMessageIfError As String, ByRef blnError As Boolean, Optional ByVal ValueIfError As Integer = 0) As Integer

        blnError = False

        Try
            Return Integer.Parse(ThisTextBox.Text)
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show(strMessageIfError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            blnError = True
            Return ValueIfError
        End Try

    End Function

    Private Function ParseTextboxValueSng(ByVal ThisTextBox As TextBox, ByVal strMessageIfError As String, ByRef blnError As Boolean, Optional ByVal ValueIfError As Single = 0) As Single

        blnError = False

        Try
            Return Single.Parse(ThisTextBox.Text)
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show(strMessageIfError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            blnError = True
            Return ValueIfError
        End Try

    End Function

    Private Function ParseTextboxValueDbl(ByVal ThisTextBox As TextBox, ByVal strMessageIfError As String, ByRef blnError As Boolean, Optional ByVal ValueIfError As Double = 0) As Double

        blnError = False

        Try
            Return Double.Parse(ThisTextBox.Text)
        Catch ex As Exception
            System.Windows.Forms.MessageBox.Show(strMessageIfError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            blnError = True
            Return ValueIfError
        End Try

    End Function

    Private Sub PastePMThresholdsValues(ByVal blnClearList As Boolean)
        Dim objData As System.Windows.Forms.IDataObject

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

        Dim LineDelimiters() As Char = New Char() {ControlChars.Cr, ControlChars.Lf}
        Dim ColumnDelimiters() As Char = New Char() {ControlChars.Tab, ","c}

        ' Examine the clipboard contents
        objData = Clipboard.GetDataObject()

        If Not objData Is Nothing Then
            If objData.GetDataPresent(System.Windows.Forms.DataFormats.StringFormat, True) Then
                strData = CType(objData.GetData(System.Windows.Forms.DataFormats.StringFormat, True), String)

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

                        Windows.Forms.MessageBox.Show(strMessage & " already present in the table; duplicate rows are not allowed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    End If

                    If intRowsSkipped > 0 Then
                        If intRowsSkipped = 1 Then
                            strMessage = "1 row was skipped because it"
                        Else
                            strMessage = intRowsSkipped.ToString & " rows were skipped because they"
                        End If

                        Windows.Forms.MessageBox.Show(strMessage & " didn't contain two columns of numeric data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    End If

                End If
            End If

        End If
    End Sub

    Private Sub PopulateComboBoxes()

        Dim intIndex As Integer
        Dim objInSilicoDigest As clsInSilicoDigest
        Dim eRuleID As clsInSilicoDigest.CleavageRuleConstants

#If IncludePNNLNETRoutines Then
        Const NET_UNITS As String = "NET"
#Else
        Const NET_UNITS As String = "time"
#End If

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

        With cboOutputFileFieldDelimeter
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
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.SequenceOnly, "Sequence Only")
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Sequence, "ProteinName and Sequence")
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence, "ProteinName, Descr, Seq")
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence, "UniqueID and Seq")
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID, "ProteinName, Seq, UniqueID")
#If IncludePNNLNETRoutines Then
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, "ProteinName, Seq, UniqueID, Mass, NET")
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore, "ProteinName, Seq, UniqueID, Mass, NET, NETStDev, DiscriminantScore")
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET, "UniqueID, Seq, Mass, NET")
#Else
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, "ProteinName, Seq, UniqueID, Mass, Time")
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore, "ProteinName, Seq, UniqueID, Mass, Time, TimeStDev, DiscriminantScore")
                .Insert(ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.UniqueID_Sequence_Mass_NET, "UniqueID, Seq, Mass, Time")
#End If
            End With
            .SelectedIndex = ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode.ProteinName_Description_Sequence
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

        objInSilicoDigest = New clsInSilicoDigest
        With cboCleavageRuleType
            With .Items
                For intIndex = 0 To objInSilicoDigest.CleaveageRuleCount - 1
                    eRuleID = CType(intIndex, clsInSilicoDigest.CleavageRuleConstants)
                    .Add(objInSilicoDigest.GetCleaveageRuleName(eRuleID) & " (" & objInSilicoDigest.GetCleaveageRuleResiduesDescription(eRuleID) & ")")
                Next intIndex
            End With
            If .Items.Count > clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin Then
                .SelectedIndex = clsInSilicoDigest.CleavageRuleConstants.ConventionalTrypsin
            End If
        End With
        objInSilicoDigest = Nothing

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
    End Sub

    Private Sub ResetToDefaults(ByVal blnConfirm As Boolean)
        Dim eResponse As System.Windows.Forms.DialogResult

        If blnConfirm Then
            eResponse = System.Windows.Forms.MessageBox.Show("Are you sure you want to reset all settings to their default values?", "Reset to Defaults", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
            If eResponse <> DialogResult.Yes Then Exit Sub
        End If

        cboInputFileFormat.SelectedIndex = InputFileFormatConstants.AutoDetermine
        cboInputFileColumnDelimiter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Tab
        txtInputFileColumnDelimiter.Text = ";"c

        cboOutputFileFieldDelimeter.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Tab
        txtOutputFileFieldDelimeter.Text = ";"c

        chkEnableLogging.Checked = False

        chkIncludePrefixAndSuffixResidues.Checked = False

        txtRefStartChar.Text = mDefaultFastaFileOptions.ProteinLineStartChar            ' ">"
        cboRefEndChar.SelectedIndex = clsParseProteinFile.DelimiterCharConstants.Space
        txtRefEndChar.Text = mDefaultFastaFileOptions.ProteinLineAccessionEndChar                ' " "

        chkLookForAddnlRefInDescription.Checked = False
        txtAddnlRefSepChar.Text = mDefaultFastaFileOptions.AddnlRefSepChar                      ' "|"
        txtAddnlRefAccessionSepChar.Text = mDefaultFastaFileOptions.AddnlRefAccessionSepChar    ' ":"

        chkExcludeProteinSequence.Checked = False
        chkComputeProteinMass.Checked = True
        chkComputepI.Checked = True

        cboHydrophobicityMode.SelectedIndex = clspICalculation.eHydrophobicityTypeConstants.HW
        chkMaxpIModeEnabled.Checked = False
        txtMaxpISequenceLength.Text = "10"

        chkIncludeXResidues.Checked = True
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

        Me.Height = 650
        Me.Width = 650

        mCustomValidationRulesFilePath = String.Empty

    End Sub

    Private Sub SelectInputFile()

        Dim objOpenFile As New System.Windows.Forms.OpenFileDialog

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
            Else
                .FilterIndex = 1
            End If

            If Len(txtProteinInputFilePath.Text.Length) > 0 Then
                Try
                    .InitialDirectory = System.IO.Directory.GetParent(txtProteinInputFilePath.Text).ToString
                Catch
                    ' Could use Application.StartupPath, but .GetExecutingAssembly is better
                    .InitialDirectory = GetAppFolderPath()
                End Try
            Else
                .InitialDirectory = GetAppFolderPath()
            End If

            .Title = "Select input file"

            .ShowDialog()
            If .FileName.Length > 0 Then
                txtProteinInputFilePath.Text = .FileName
            End If
        End With

    End Sub

    Private Sub SelectOutputFile()

        Dim objSaveFile As New System.Windows.Forms.SaveFileDialog

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
                    .InitialDirectory = System.IO.Directory.GetParent(txtProteinOutputFilePath.Text).ToString
                Catch
                    ' Could use Application.StartupPath, but .GetExecutingAssembly is better
                    .InitialDirectory = GetAppFolderPath()
                End Try
            Else
                .InitialDirectory = GetAppFolderPath()
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

    Private Sub ShowElutionTimeInfo()
#If IncludePNNLNETRoutines Then
        Dim objNETPrediction As New NETPrediction.ElutionTimePredictionKangas
        Windows.Forms.MessageBox.Show(objNETPrediction.ProgramDescription, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
#Else
        Dim objNETPrediction As New NETPredictionBasic.ElutionTimePredictionKrokhin
        Windows.Forms.MessageBox.Show(objNETPrediction.ProgramDescription, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
#End If
    End Sub

    Private Sub ValidateFastaFile(ByVal strFastaFilePath As String)

        Dim objFastaValidation As frmFastaValidation

        Try
            ' Make sure an existing file has been chosen
            If strFastaFilePath Is Nothing OrElse strFastaFilePath.Length = 0 Then Exit Try

            If Not System.IO.File.Exists(strFastaFilePath) Then
                Windows.Forms.MessageBox.Show("File not found: " & ControlChars.NewLine & strFastaFilePath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else
                objFastaValidation = New frmFastaValidation(strFastaFilePath)

                Try
                    If Not mCustomValidationRulesFilePath Is Nothing AndAlso mCustomValidationRulesFilePath.Length > 0 Then
                        If System.IO.File.Exists(mCustomValidationRulesFilePath) Then
                            objFastaValidation.CustomRulesFilePath = mCustomValidationRulesFilePath
                        Else
                            mCustomValidationRulesFilePath = String.Empty
                        End If
                    End If
                Catch ex As Exception
                    Windows.Forms.MessageBox.Show("Error trying to validate or set the custom validation rules file path: " & ControlChars.NewLine & mCustomValidationRulesFilePath & ControlChars.NewLine & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End Try

                objFastaValidation.ShowDialog()
            End If

        Catch ex As Exception
            Windows.Forms.MessageBox.Show("Error occurred in frmFastaValidation: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        Finally
            If Not objFastaValidation Is Nothing Then
                mCustomValidationRulesFilePath = objFastaValidation.CustomRulesFilePath
                objFastaValidation = Nothing
            End If
        End Try

    End Sub

    Private Function ValidateSqlServerCachingOptionsForInputFile(ByVal strInputFilePath As String, ByVal blnAssumeDigested As Boolean, ByRef objProteinFileParser As clsParseProteinFile) As Boolean
        ' Returns True if the user OK's or updates the current Sql Server caching options
        ' Returns False if the user cancels processing
        ' Assumes that strInputFilePath exists, and thus does not have a Try-Catch block

        Const SAMPLING_LINE_COUNT As Integer = 10000

        Dim blnIsFastaFile As Boolean
        Dim eDelimitedFileFormatCode As ProteinFileReader.DelimitedFileReader.eDelimitedFileFormatCode

        Dim fiFileInfo As System.IO.FileInfo
        Dim intFileSizeKB As Integer

        Dim srStreamReader As System.IO.StreamReader
        Dim strLineIn As String
        Dim intLineCount As Integer
        Dim intTotalLineCount As Integer
        Dim lngBytesRead As Long

        Dim blnProceed As Boolean = False
        Dim blnSuggestEnableSqlServer As Boolean = False
        Dim blnSuggestDisableSqlServer As Boolean = False

        Dim eResponse As System.Windows.Forms.DialogResult

        blnIsFastaFile = clsParseProteinFile.IsFastaFile(strInputFilePath) Or objProteinFileParser.AssumeFastaFile

        ' Lookup the file size
        fiFileInfo = New System.IO.FileInfo(strInputFilePath)
        intFileSizeKB = CType(fiFileInfo.Length / 1024.0, Integer)

        If blnIsFastaFile Then
            If objProteinFileParser.DigestionOptions.CleavageRuleID = clsInSilicoDigest.CleavageRuleConstants.KROneEnd Or _
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
                srStreamReader = New System.IO.StreamReader(strInputFilePath)

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
            Catch ex As Exception
                ' Error reading input file
                blnSuggestEnableSqlServer = False
                blnSuggestDisableSqlServer = False
            Finally
                If Not srStreamReader Is Nothing Then
                    srStreamReader.Close()
                End If
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
            eResponse = Windows.Forms.MessageBox.Show("Warning, memory usage could be quite large.  Enable Sql Server caching using Server " & txtSqlServerName.Text & "?  If no, then will continue using memory caching.", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
            If eResponse = DialogResult.Yes Then chkUseSqlServerDBToCacheData.Checked = True
            If eResponse = DialogResult.Cancel Then
                blnProceed = False
            Else
                blnProceed = True
            End If
        ElseIf blnSuggestDisableSqlServer And chkUseSqlServerDBToCacheData.Checked Then
            eResponse = Windows.Forms.MessageBox.Show("Memory usage is expected to be minimal.  Continue caching data using Server " & txtSqlServerName.Text & "?  If no, then will switch to using memory caching.", "Note", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
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

    Private Sub ValidateTextbox(ByRef ThisTextBox As TextBox, ByVal strDefaultText As String)
        If ThisTextBox.TextLength = 0 Then
            ThisTextBox.Text = strDefaultText
        End If
    End Sub
#End Region

#Region "Control Handlers"

    Private Sub cboHydrophobicityMode_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboHydrophobicityMode.SelectedIndexChanged
        ComputeSequencepI()
    End Sub
    Private Sub cboInputFileColumnDelimiter_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboInputFileColumnDelimiter.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub cboInputFileFormat_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboInputFileFormat.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub cboProteinReversalOptions_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboProteinReversalOptions.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub chkAllowSqlServerCaching_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAllowSqlServerCaching.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkAutoDefineSLiCScoreTolerances_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAutoDefineSLiCScoreTolerances.CheckedChanged
        UpdateDataGridTableStyle()
    End Sub

    Private Sub chkComputepI_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkComputepI.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkDigestProteins_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDigestProteins.CheckedChanged
        EnableDisableControls()
        AutoDefineOutputFile()
    End Sub

    Private Sub chkUseSLiCScoreForUniqueness_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseSLiCScoreForUniqueness.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkUseSqlServerDBToCacheData_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkUseSqlServerDBToCacheData.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkSqlServerUseIntegratedSecurity_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSqlServerUseIntegratedSecurity.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub cboOutputFileFieldDelimeter_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboOutputFileFieldDelimeter.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub cboRefEndChar_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cboRefEndChar.SelectedIndexChanged
        EnableDisableControls()
    End Sub

    Private Sub chkAutoComputeRangeForBinning_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAutoComputeRangeForBinning.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub cmdClearPMThresholdsList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClearPMThresholdsList.Click
        ClearPMThresholdsList(True)
    End Sub

    Private Sub chkCreateFastaOutputFile_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkCreateFastaOutputFile.CheckedChanged
        AutoDefineOutputFile()
    End Sub

    Private Sub chkLookForAddnlRefInDescription_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkLookForAddnlRefInDescription.CheckedChanged
        EnableDisableControls()
    End Sub

    Private Sub chkMaxpIModeEnabled_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkMaxpIModeEnabled.CheckedChanged
        EnableDisableControls()
        ComputeSequencepI()
    End Sub

    Private Sub cmdGenerateUniquenessStats_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdGenerateUniquenessStats.Click
        GenerateUniquenessStats()
    End Sub

    Private Sub cmdParseInputFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdParseInputFile.Click
        ParseProteinInputFile()
    End Sub

    Private Sub cmdPastePMThresholdsList_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPastePMThresholdsList.Click
        PastePMThresholdsValues(False)
    End Sub

    Private Sub cmdPMThresholdsAutoPopulate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPMThresholdsAutoPopulate.Click
        AutoPopulatePMThresholdsByID(CType(cboPMPredefinedThresholds.SelectedIndex, PredefinedPMThresholdsConstants), True)
    End Sub

    Private Sub cmdSelectFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectFile.Click
        SelectInputFile()
    End Sub

    Private Sub cmdSelectOutputFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSelectOutputFile.Click
        SelectOutputFile()
    End Sub

    Private Sub cmdValidateFastaFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdValidateFastaFile.Click
        ValidateFastaFile(txtProteinInputFilePath.Text)
    End Sub

    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        ' Note that InitializeControls() is called in Sub New()
    End Sub

    Private Sub frmMain_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        IniFileSaveOptions(True)
    End Sub

    Private Sub txtDigestProteinsMinimumMass_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtDigestProteinsMinimumMass.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtDigestProteinsMinimumMass, e, True)
    End Sub

    Private Sub txtDigestProteinsMaximumMissedCleavages_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtDigestProteinsMaximumMissedCleavages.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtDigestProteinsMaximumMissedCleavages, e, True)
    End Sub

    Private Sub txtDigestProteinsMinimumResidueCount_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtDigestProteinsMinimumResidueCount.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtDigestProteinsMinimumResidueCount, e, True)
    End Sub

    Private Sub txtDigestProteinsMaximumMass_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtDigestProteinsMaximumMass.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtDigestProteinsMaximumMass, e, True)
    End Sub

    Private Sub txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMaxPeakMatchingResultsPerFeatureToSave.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtMaxPeakMatchingResultsPerFeatureToSave, e, True)
    End Sub

    Private Sub txtMaxPeakMatchingResultsPerFeatureToSave_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtMaxPeakMatchingResultsPerFeatureToSave.Validating
        If txtMaxPeakMatchingResultsPerFeatureToSave.Text.Trim = "0" Then txtMaxPeakMatchingResultsPerFeatureToSave.Text = "1"
        SharedVBNetRoutines.VBNetRoutines.ValidateTextboxInt(txtMaxPeakMatchingResultsPerFeatureToSave, 1, 100, 3)
    End Sub

    Private Sub txtMaxpISequenceLength_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtMaxpISequenceLength.KeyDown
        If e.KeyCode = Keys.Enter AndAlso chkMaxpIModeEnabled.Checked Then ComputeSequencepI()
    End Sub

    Private Sub txtMaxpISequenceLength_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMaxpISequenceLength.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtMaxpISequenceLength, e, True, False)
    End Sub

    Private Sub txtMaxpISequenceLength_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtMaxpISequenceLength.Validating
        SharedVBNetRoutines.VBNetRoutines.ValidateTextboxInt(txtMaxpISequenceLength, 1, 10000, 10)
    End Sub

    Private Sub txtMaxpISequenceLength_Validated(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtMaxpISequenceLength.Validated
        ComputeSequencepI()
    End Sub

    Private Sub txtMinimumSLiCScore_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtMinimumSLiCScore.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtMinimumSLiCScore, e, True, True)
    End Sub

    Private Sub txtMinimumSLiCScore_Validating(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles txtMinimumSLiCScore.Validating
        SharedVBNetRoutines.VBNetRoutines.ValidateTextboxSng(txtMinimumSLiCScore, 0, 1, 0.95)
    End Sub

    Private Sub txtProteinInputFilePath_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtProteinInputFilePath.TextChanged
        EnableDisableControls()
        AutoDefineOutputFile()
    End Sub

    Private Sub txtProteinReversalSamplingPercentage_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtProteinReversalSamplingPercentage.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtProteinReversalSamplingPercentage, e, True)
    End Sub

    Private Sub txtProteinScramblingLoopCount_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtProteinScramblingLoopCount.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtProteinScramblingLoopCount, e, True)
    End Sub

    Private Sub txtSequenceForpI_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSequenceForpI.TextChanged
        ComputeSequencepI()
    End Sub

    Private Sub txtUniquenessBinWidth_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtUniquenessBinWidth.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtUniquenessBinWidth, e, True)
    End Sub

    Private Sub txtUniquenessBinStartMass_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtUniquenessBinStartMass.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtUniquenessBinStartMass, e, True)
    End Sub

    Private Sub txtUniquenessBinEndMass_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtUniquenessBinEndMass.KeyPress
        SharedVBNetRoutines.VBNetRoutines.TextBoxKeyPressHandler(txtUniquenessBinEndMass, e, True)
    End Sub

#End Region

#Region "Menu Handlers"
    Private Sub mnuFileSelectInputFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileSelectInputFile.Click
        SelectInputFile()
    End Sub

    Private Sub mnuFileSelectOutputFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileSelectOutputFile.Click
        SelectOutputFile()
    End Sub

    Private Sub mnuFileSaveDefaultOptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileSaveDefaultOptions.Click
        IniFileSaveOptions()
    End Sub

    Private Sub mnuFileExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFileExit.Click
        Me.Close()
    End Sub

    Private Sub mnuEditMakeUniquenessStats_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditMakeUniquenessStats.Click
        GenerateUniquenessStats()
    End Sub

    Private Sub mnuEditParseFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditParseFile.Click
        ParseProteinInputFile()
    End Sub

    Private Sub mnuEditResetOptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuEditResetOptions.Click
        ResetToDefaults(True)
    End Sub

    Private Sub mnuHelpAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuHelpAbout.Click
        ShowAboutBox()
    End Sub

    Private Sub mnuHelpAboutElutionTime_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuHelpAboutElutionTime.Click
        ShowElutionTimeInfo()
    End Sub

#End Region

End Class
