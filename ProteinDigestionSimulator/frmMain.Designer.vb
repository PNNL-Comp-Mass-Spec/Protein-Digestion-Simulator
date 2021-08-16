<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
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
        Me.txtProteinInputFilePath = New System.Windows.Forms.TextBox()
        Me.optUseRectangleSearchRegion = New System.Windows.Forms.RadioButton()
        Me.optUseEllipseSearchRegion = New System.Windows.Forms.RadioButton()
        Me.lblUniquenessCalculationsNote = New System.Windows.Forms.Label()
        Me.lblProteinScramblingLoopCount = New System.Windows.Forms.Label()
        Me.fraPeakMatchingOptions = New System.Windows.Forms.GroupBox()
        Me.txtMaxPeakMatchingResultsPerFeatureToSave = New System.Windows.Forms.TextBox()
        Me.lblMaxPeakMatchingResultsPerFeatureToSave = New System.Windows.Forms.Label()
        Me.chkExportPeakMatchingResults = New System.Windows.Forms.CheckBox()
        Me.txtMinimumSLiCScore = New System.Windows.Forms.TextBox()
        Me.lblMinimumSLiCScore = New System.Windows.Forms.Label()
        Me.chkUseSLiCScoreForUniqueness = New System.Windows.Forms.CheckBox()
        Me.TabPageUniquenessStats = New System.Windows.Forms.TabPage()
        Me.fraSqlServerOptions = New System.Windows.Forms.GroupBox()
        Me.chkSqlServerUseExistingData = New System.Windows.Forms.CheckBox()
        Me.chkAllowSqlServerCaching = New System.Windows.Forms.CheckBox()
        Me.lblSqlServerPassword = New System.Windows.Forms.Label()
        Me.lblSqlServerUsername = New System.Windows.Forms.Label()
        Me.txtSqlServerPassword = New System.Windows.Forms.TextBox()
        Me.txtSqlServerUsername = New System.Windows.Forms.TextBox()
        Me.lblSqlServerDatabase = New System.Windows.Forms.Label()
        Me.lblSqlServerServerName = New System.Windows.Forms.Label()
        Me.chkSqlServerUseIntegratedSecurity = New System.Windows.Forms.CheckBox()
        Me.txtSqlServerDatabase = New System.Windows.Forms.TextBox()
        Me.txtSqlServerName = New System.Windows.Forms.TextBox()
        Me.chkUseSqlServerDBToCacheData = New System.Windows.Forms.CheckBox()
        Me.fraUniquenessBinningOptions = New System.Windows.Forms.GroupBox()
        Me.lblPeptideUniquenessMassMode = New System.Windows.Forms.Label()
        Me.txtUniquenessBinWidth = New System.Windows.Forms.TextBox()
        Me.lblUniquenessBinWidth = New System.Windows.Forms.Label()
        Me.chkAutoComputeRangeForBinning = New System.Windows.Forms.CheckBox()
        Me.txtUniquenessBinEndMass = New System.Windows.Forms.TextBox()
        Me.lblUniquenessBinEndMass = New System.Windows.Forms.Label()
        Me.txtUniquenessBinStartMass = New System.Windows.Forms.TextBox()
        Me.lblUniquenessBinStartMass = New System.Windows.Forms.Label()
        Me.lblUniquenessStatsNote = New System.Windows.Forms.Label()
        Me.cmdGenerateUniquenessStats = New System.Windows.Forms.Button()
        Me.chkAssumeInputFileIsDigested = New System.Windows.Forms.CheckBox()
        Me.txtProteinScramblingLoopCount = New System.Windows.Forms.TextBox()
        Me.lblSamplingPercentageUnits = New System.Windows.Forms.Label()
        Me.txtMaxpISequenceLength = New System.Windows.Forms.TextBox()
        Me.lblProteinReversalSamplingPercentage = New System.Windows.Forms.Label()
        Me.lblMaxpISequenceLength = New System.Windows.Forms.Label()
        Me.chkMaxpIModeEnabled = New System.Windows.Forms.CheckBox()
        Me.frapIAndHydrophobicity = New System.Windows.Forms.GroupBox()
        Me.lblHydrophobicityMode = New System.Windows.Forms.Label()
        Me.cboHydrophobicityMode = New System.Windows.Forms.ComboBox()
        Me.txtpIStats = New System.Windows.Forms.TextBox()
        Me.txtSequenceForpI = New System.Windows.Forms.TextBox()
        Me.lblSequenceForpI = New System.Windows.Forms.Label()
        Me.fraDelimitedFileOptions = New System.Windows.Forms.GroupBox()
        Me.cboInputFileColumnOrdering = New System.Windows.Forms.ComboBox()
        Me.lblInputFileColumnOrdering = New System.Windows.Forms.Label()
        Me.txtInputFileColumnDelimiter = New System.Windows.Forms.TextBox()
        Me.lblInputFileColumnDelimiter = New System.Windows.Forms.Label()
        Me.cboInputFileColumnDelimiter = New System.Windows.Forms.ComboBox()
        Me.TabPageFileFormatOptions = New System.Windows.Forms.TabPage()
        Me.tbsOptions = New System.Windows.Forms.TabControl()
        Me.TabPageParseAndDigest = New System.Windows.Forms.TabPage()
        Me.fraProcessingOptions = New System.Windows.Forms.GroupBox()
        Me.txtProteinReversalSamplingPercentage = New System.Windows.Forms.TextBox()
        Me.lbltxtAddnlRefAccessionSepChar = New System.Windows.Forms.Label()
        Me.chkLookForAddnlRefInDescription = New System.Windows.Forms.CheckBox()
        Me.cboProteinReversalOptions = New System.Windows.Forms.ComboBox()
        Me.lblProteinReversalOptions = New System.Windows.Forms.Label()
        Me.chkDigestProteins = New System.Windows.Forms.CheckBox()
        Me.lblAddnlRefSepChar = New System.Windows.Forms.Label()
        Me.txtAddnlRefAccessionSepChar = New System.Windows.Forms.TextBox()
        Me.txtAddnlRefSepChar = New System.Windows.Forms.TextBox()
        Me.chkCreateFastaOutputFile = New System.Windows.Forms.CheckBox()
        Me.fraCalculationOptions = New System.Windows.Forms.GroupBox()
        Me.cmdNETInfo = New System.Windows.Forms.Button()
        Me.chkExcludeProteinDescription = New System.Windows.Forms.CheckBox()
        Me.chkComputeSequenceHashIgnoreILDiff = New System.Windows.Forms.CheckBox()
        Me.chkTruncateProteinDescription = New System.Windows.Forms.CheckBox()
        Me.chkComputeSequenceHashValues = New System.Windows.Forms.CheckBox()
        Me.lblMassMode = New System.Windows.Forms.Label()
        Me.cboElementMassMode = New System.Windows.Forms.ComboBox()
        Me.chkExcludeProteinSequence = New System.Windows.Forms.CheckBox()
        Me.chkComputepIandNET = New System.Windows.Forms.CheckBox()
        Me.chkIncludeXResidues = New System.Windows.Forms.CheckBox()
        Me.chkComputeProteinMass = New System.Windows.Forms.CheckBox()
        Me.fraDigestionOptions = New System.Windows.Forms.GroupBox()
        Me.cboFragmentMassMode = New System.Windows.Forms.ComboBox()
        Me.lblFragmentMassMode = New System.Windows.Forms.Label()
        Me.cboCysTreatmentMode = New System.Windows.Forms.ComboBox()
        Me.lblCysTreatment = New System.Windows.Forms.Label()
        Me.txtDigestProteinsMaximumpI = New System.Windows.Forms.TextBox()
        Me.lblDigestProteinsMaximumpI = New System.Windows.Forms.Label()
        Me.txtDigestProteinsMinimumpI = New System.Windows.Forms.TextBox()
        Me.lblDigestProteinsMinimumpI = New System.Windows.Forms.Label()
        Me.chkGenerateUniqueIDValues = New System.Windows.Forms.CheckBox()
        Me.chkCysPeptidesOnly = New System.Windows.Forms.CheckBox()
        Me.txtDigestProteinsMinimumResidueCount = New System.Windows.Forms.TextBox()
        Me.lblDigestProteinsMinimumResidueCount = New System.Windows.Forms.Label()
        Me.txtDigestProteinsMaximumMissedCleavages = New System.Windows.Forms.TextBox()
        Me.lblDigestProteinsMaximumMissedCleavages = New System.Windows.Forms.Label()
        Me.txtDigestProteinsMaximumMass = New System.Windows.Forms.TextBox()
        Me.lblDigestProteinsMaximumMass = New System.Windows.Forms.Label()
        Me.txtDigestProteinsMinimumMass = New System.Windows.Forms.TextBox()
        Me.lblDigestProteinsMinimumMass = New System.Windows.Forms.Label()
        Me.cboCleavageRuleType = New System.Windows.Forms.ComboBox()
        Me.chkIncludeDuplicateSequences = New System.Windows.Forms.CheckBox()
        Me.cmdParseInputFile = New System.Windows.Forms.Button()
        Me.TabPagePeakMatchingThresholds = New System.Windows.Forms.TabPage()
        Me.chkAutoDefineSLiCScoreTolerances = New System.Windows.Forms.CheckBox()
        Me.cmdPastePMThresholdsList = New System.Windows.Forms.Button()
        Me.cboPMPredefinedThresholds = New System.Windows.Forms.ComboBox()
        Me.cmdPMThresholdsAutoPopulate = New System.Windows.Forms.Button()
        Me.cmdClearPMThresholdsList = New System.Windows.Forms.Button()
        Me.cboMassTolType = New System.Windows.Forms.ComboBox()
        Me.lblMassTolType = New System.Windows.Forms.Label()
        Me.dgPeakMatchingThresholds = New System.Windows.Forms.DataGrid()
        Me.TabPageProgress = New System.Windows.Forms.TabPage()
        Me.pbarProgress = New System.Windows.Forms.ProgressBar()
        Me.lblErrorMessage = New System.Windows.Forms.Label()
        Me.lblSubtaskProgress = New System.Windows.Forms.Label()
        Me.lblProgress = New System.Windows.Forms.Label()
        Me.lblSubtaskProgressDescription = New System.Windows.Forms.Label()
        Me.lblProgressDescription = New System.Windows.Forms.Label()
        Me.cmdAbortProcessing = New System.Windows.Forms.Button()
        Me.mnuHelpAboutElutionTime = New System.Windows.Forms.MenuItem()
        Me.cboInputFileFormat = New System.Windows.Forms.ComboBox()
        Me.lblInputFileFormat = New System.Windows.Forms.Label()
        Me.cmdSelectFile = New System.Windows.Forms.Button()
        Me.fraInputFilePath = New System.Windows.Forms.GroupBox()
        Me.cmdValidateFastaFile = New System.Windows.Forms.Button()
        Me.chkEnableLogging = New System.Windows.Forms.CheckBox()
        Me.mnuFileSelectOutputFile = New System.Windows.Forms.MenuItem()
        Me.cmdSelectOutputFile = New System.Windows.Forms.Button()
        Me.mnuFileSep1 = New System.Windows.Forms.MenuItem()
        Me.mnuFile = New System.Windows.Forms.MenuItem()
        Me.mnuFileSelectInputFile = New System.Windows.Forms.MenuItem()
        Me.mnuFileSaveDefaultOptions = New System.Windows.Forms.MenuItem()
        Me.mnuFileSep2 = New System.Windows.Forms.MenuItem()
        Me.mnuFileExit = New System.Windows.Forms.MenuItem()
        Me.txtProteinOutputFilePath = New System.Windows.Forms.TextBox()
        Me.chkIncludePrefixAndSuffixResidues = New System.Windows.Forms.CheckBox()
        Me.mnuEditResetOptions = New System.Windows.Forms.MenuItem()
        Me.mnuHelp = New System.Windows.Forms.MenuItem()
        Me.mnuHelpAbout = New System.Windows.Forms.MenuItem()
        Me.mnuEditSep1 = New System.Windows.Forms.MenuItem()
        Me.mnuEditMakeUniquenessStats = New System.Windows.Forms.MenuItem()
        Me.mnuEdit = New System.Windows.Forms.MenuItem()
        Me.mnuEditParseFile = New System.Windows.Forms.MenuItem()
        Me.MainMenuControl = New System.Windows.Forms.MainMenu(Me.components)
        Me.lblOutputFileFieldDelimiter = New System.Windows.Forms.Label()
        Me.cboOutputFileFieldDelimiter = New System.Windows.Forms.ComboBox()
        Me.txtOutputFileFieldDelimiter = New System.Windows.Forms.TextBox()
        Me.fraOutputTextOptions = New System.Windows.Forms.GroupBox()
        Me.fraPeakMatchingOptions.SuspendLayout()
        Me.TabPageUniquenessStats.SuspendLayout()
        Me.fraSqlServerOptions.SuspendLayout()
        Me.fraUniquenessBinningOptions.SuspendLayout()
        Me.frapIAndHydrophobicity.SuspendLayout()
        Me.fraDelimitedFileOptions.SuspendLayout()
        Me.TabPageFileFormatOptions.SuspendLayout()
        Me.tbsOptions.SuspendLayout()
        Me.TabPageParseAndDigest.SuspendLayout()
        Me.fraProcessingOptions.SuspendLayout()
        Me.fraCalculationOptions.SuspendLayout()
        Me.fraDigestionOptions.SuspendLayout()
        Me.TabPagePeakMatchingThresholds.SuspendLayout()
        CType(Me.dgPeakMatchingThresholds, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPageProgress.SuspendLayout()
        Me.fraInputFilePath.SuspendLayout()
        Me.fraOutputTextOptions.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtProteinInputFilePath
        '
        Me.txtProteinInputFilePath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtProteinInputFilePath.Location = New System.Drawing.Point(104, 26)
        Me.txtProteinInputFilePath.Name = "txtProteinInputFilePath"
        Me.txtProteinInputFilePath.Size = New System.Drawing.Size(611, 20)
        Me.txtProteinInputFilePath.TabIndex = 1
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
        'lblUniquenessCalculationsNote
        '
        Me.lblUniquenessCalculationsNote.Location = New System.Drawing.Point(240, 192)
        Me.lblUniquenessCalculationsNote.Name = "lblUniquenessCalculationsNote"
        Me.lblUniquenessCalculationsNote.Size = New System.Drawing.Size(384, 88)
        Me.lblUniquenessCalculationsNote.TabIndex = 6
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
        Me.TabPageUniquenessStats.Size = New System.Drawing.Size(696, 328)
        Me.TabPageUniquenessStats.TabIndex = 1
        Me.TabPageUniquenessStats.Text = "Peptide Uniqueness Options"
        Me.TabPageUniquenessStats.UseVisualStyleBackColor = True
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
        Me.fraSqlServerOptions.Location = New System.Drawing.Point(232, 192)
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
        Me.txtSqlServerPassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
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
        Me.fraUniquenessBinningOptions.Controls.Add(Me.lblPeptideUniquenessMassMode)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.txtUniquenessBinWidth)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.lblUniquenessBinWidth)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.chkAutoComputeRangeForBinning)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.txtUniquenessBinEndMass)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.lblUniquenessBinEndMass)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.txtUniquenessBinStartMass)
        Me.fraUniquenessBinningOptions.Controls.Add(Me.lblUniquenessBinStartMass)
        Me.fraUniquenessBinningOptions.Location = New System.Drawing.Point(8, 120)
        Me.fraUniquenessBinningOptions.Name = "fraUniquenessBinningOptions"
        Me.fraUniquenessBinningOptions.Size = New System.Drawing.Size(208, 160)
        Me.fraUniquenessBinningOptions.TabIndex = 3
        Me.fraUniquenessBinningOptions.TabStop = False
        Me.fraUniquenessBinningOptions.Text = "Binning Options"
        '
        'lblPeptideUniquenessMassMode
        '
        Me.lblPeptideUniquenessMassMode.Location = New System.Drawing.Point(16, 136)
        Me.lblPeptideUniquenessMassMode.Name = "lblPeptideUniquenessMassMode"
        Me.lblPeptideUniquenessMassMode.Size = New System.Drawing.Size(176, 16)
        Me.lblPeptideUniquenessMassMode.TabIndex = 7
        Me.lblPeptideUniquenessMassMode.Text = "Using monoisotopic masses"
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
        Me.lblUniquenessStatsNote.Text = "Note that Digestion Options and Mass Calculation Options also apply for uniquenes" &
    "s stats generation."
        '
        'cmdGenerateUniquenessStats
        '
        Me.cmdGenerateUniquenessStats.Location = New System.Drawing.Point(232, 16)
        Me.cmdGenerateUniquenessStats.Name = "cmdGenerateUniquenessStats"
        Me.cmdGenerateUniquenessStats.Size = New System.Drawing.Size(176, 24)
        Me.cmdGenerateUniquenessStats.TabIndex = 5
        Me.cmdGenerateUniquenessStats.Text = "&Generate Uniqueness Stats"
        '
        'chkAssumeInputFileIsDigested
        '
        Me.chkAssumeInputFileIsDigested.Location = New System.Drawing.Point(8, 16)
        Me.chkAssumeInputFileIsDigested.Name = "chkAssumeInputFileIsDigested"
        Me.chkAssumeInputFileIsDigested.Size = New System.Drawing.Size(192, 32)
        Me.chkAssumeInputFileIsDigested.TabIndex = 0
        Me.chkAssumeInputFileIsDigested.Text = "Assume input file is already digested (for Delimited files only)"
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
        'lblSamplingPercentageUnits
        '
        Me.lblSamplingPercentageUnits.Location = New System.Drawing.Point(208, 42)
        Me.lblSamplingPercentageUnits.Name = "lblSamplingPercentageUnits"
        Me.lblSamplingPercentageUnits.Size = New System.Drawing.Size(16, 16)
        Me.lblSamplingPercentageUnits.TabIndex = 4
        Me.lblSamplingPercentageUnits.Text = "%"
        '
        'txtMaxpISequenceLength
        '
        Me.txtMaxpISequenceLength.Location = New System.Drawing.Point(168, 70)
        Me.txtMaxpISequenceLength.Name = "txtMaxpISequenceLength"
        Me.txtMaxpISequenceLength.Size = New System.Drawing.Size(40, 20)
        Me.txtMaxpISequenceLength.TabIndex = 4
        Me.txtMaxpISequenceLength.Text = "10"
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
        Me.frapIAndHydrophobicity.Location = New System.Drawing.Point(8, 108)
        Me.frapIAndHydrophobicity.Name = "frapIAndHydrophobicity"
        Me.frapIAndHydrophobicity.Size = New System.Drawing.Size(616, 136)
        Me.frapIAndHydrophobicity.TabIndex = 2
        Me.frapIAndHydrophobicity.TabStop = False
        Me.frapIAndHydrophobicity.Text = "pI And Hydrophobicity"
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
        Me.txtpIStats.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtpIStats.Location = New System.Drawing.Point(336, 48)
        Me.txtpIStats.MaxLength = 1
        Me.txtpIStats.Multiline = True
        Me.txtpIStats.Name = "txtpIStats"
        Me.txtpIStats.ReadOnly = True
        Me.txtpIStats.Size = New System.Drawing.Size(272, 80)
        Me.txtpIStats.TabIndex = 7
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
        'fraDelimitedFileOptions
        '
        Me.fraDelimitedFileOptions.Controls.Add(Me.cboInputFileColumnOrdering)
        Me.fraDelimitedFileOptions.Controls.Add(Me.lblInputFileColumnOrdering)
        Me.fraDelimitedFileOptions.Controls.Add(Me.txtInputFileColumnDelimiter)
        Me.fraDelimitedFileOptions.Controls.Add(Me.lblInputFileColumnDelimiter)
        Me.fraDelimitedFileOptions.Controls.Add(Me.cboInputFileColumnDelimiter)
        Me.fraDelimitedFileOptions.Location = New System.Drawing.Point(8, 12)
        Me.fraDelimitedFileOptions.Name = "fraDelimitedFileOptions"
        Me.fraDelimitedFileOptions.Size = New System.Drawing.Size(496, 88)
        Me.fraDelimitedFileOptions.TabIndex = 1
        Me.fraDelimitedFileOptions.TabStop = False
        Me.fraDelimitedFileOptions.Text = "Delimited Input File Options"
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
        'lblInputFileColumnOrdering
        '
        Me.lblInputFileColumnOrdering.Location = New System.Drawing.Point(8, 26)
        Me.lblInputFileColumnOrdering.Name = "lblInputFileColumnOrdering"
        Me.lblInputFileColumnOrdering.Size = New System.Drawing.Size(80, 16)
        Me.lblInputFileColumnOrdering.TabIndex = 0
        Me.lblInputFileColumnOrdering.Text = "Column Order"
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
        'TabPageFileFormatOptions
        '
        Me.TabPageFileFormatOptions.Controls.Add(Me.frapIAndHydrophobicity)
        Me.TabPageFileFormatOptions.Controls.Add(Me.fraDelimitedFileOptions)
        Me.TabPageFileFormatOptions.Location = New System.Drawing.Point(4, 22)
        Me.TabPageFileFormatOptions.Name = "TabPageFileFormatOptions"
        Me.TabPageFileFormatOptions.Size = New System.Drawing.Size(696, 328)
        Me.TabPageFileFormatOptions.TabIndex = 2
        Me.TabPageFileFormatOptions.Text = "File Format Options"
        Me.TabPageFileFormatOptions.UseVisualStyleBackColor = True
        '
        'tbsOptions
        '
        Me.tbsOptions.Controls.Add(Me.TabPageFileFormatOptions)
        Me.tbsOptions.Controls.Add(Me.TabPageParseAndDigest)
        Me.tbsOptions.Controls.Add(Me.TabPageUniquenessStats)
        Me.tbsOptions.Controls.Add(Me.TabPagePeakMatchingThresholds)
        Me.tbsOptions.Controls.Add(Me.TabPageProgress)
        Me.tbsOptions.Location = New System.Drawing.Point(12, 212)
        Me.tbsOptions.Name = "tbsOptions"
        Me.tbsOptions.SelectedIndex = 0
        Me.tbsOptions.Size = New System.Drawing.Size(704, 354)
        Me.tbsOptions.TabIndex = 5
        '
        'TabPageParseAndDigest
        '
        Me.TabPageParseAndDigest.Controls.Add(Me.fraProcessingOptions)
        Me.TabPageParseAndDigest.Controls.Add(Me.fraCalculationOptions)
        Me.TabPageParseAndDigest.Controls.Add(Me.fraDigestionOptions)
        Me.TabPageParseAndDigest.Controls.Add(Me.cmdParseInputFile)
        Me.TabPageParseAndDigest.Location = New System.Drawing.Point(4, 22)
        Me.TabPageParseAndDigest.Name = "TabPageParseAndDigest"
        Me.TabPageParseAndDigest.Size = New System.Drawing.Size(696, 328)
        Me.TabPageParseAndDigest.TabIndex = 0
        Me.TabPageParseAndDigest.Text = "Parse and Digest File Options"
        Me.TabPageParseAndDigest.UseVisualStyleBackColor = True
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
        Me.fraProcessingOptions.Size = New System.Drawing.Size(360, 176)
        Me.fraProcessingOptions.TabIndex = 0
        Me.fraProcessingOptions.TabStop = False
        Me.fraProcessingOptions.Text = "Processing Options"
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
        'lbltxtAddnlRefAccessionSepChar
        '
        Me.lbltxtAddnlRefAccessionSepChar.Location = New System.Drawing.Point(96, 96)
        Me.lbltxtAddnlRefAccessionSepChar.Name = "lbltxtAddnlRefAccessionSepChar"
        Me.lbltxtAddnlRefAccessionSepChar.Size = New System.Drawing.Size(160, 16)
        Me.lbltxtAddnlRefAccessionSepChar.TabIndex = 8
        Me.lbltxtAddnlRefAccessionSepChar.Text = "Addnl Ref Accession Sep Char"
        Me.lbltxtAddnlRefAccessionSepChar.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'chkLookForAddnlRefInDescription
        '
        Me.chkLookForAddnlRefInDescription.Location = New System.Drawing.Point(16, 72)
        Me.chkLookForAddnlRefInDescription.Name = "chkLookForAddnlRefInDescription"
        Me.chkLookForAddnlRefInDescription.Size = New System.Drawing.Size(120, 32)
        Me.chkLookForAddnlRefInDescription.TabIndex = 5
        Me.chkLookForAddnlRefInDescription.Text = "Look for addnl Ref in description"
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
        Me.chkDigestProteins.Location = New System.Drawing.Point(16, 115)
        Me.chkDigestProteins.Name = "chkDigestProteins"
        Me.chkDigestProteins.Size = New System.Drawing.Size(160, 32)
        Me.chkDigestProteins.TabIndex = 10
        Me.chkDigestProteins.Text = "In Silico digest of all proteins in input file"
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
        'chkCreateFastaOutputFile
        '
        Me.chkCreateFastaOutputFile.Location = New System.Drawing.Point(192, 128)
        Me.chkCreateFastaOutputFile.Name = "chkCreateFastaOutputFile"
        Me.chkCreateFastaOutputFile.Size = New System.Drawing.Size(160, 16)
        Me.chkCreateFastaOutputFile.TabIndex = 11
        Me.chkCreateFastaOutputFile.Text = "Create FASTA Output File"
        '
        'fraCalculationOptions
        '
        Me.fraCalculationOptions.Controls.Add(Me.cmdNETInfo)
        Me.fraCalculationOptions.Controls.Add(Me.chkExcludeProteinDescription)
        Me.fraCalculationOptions.Controls.Add(Me.chkComputeSequenceHashIgnoreILDiff)
        Me.fraCalculationOptions.Controls.Add(Me.chkTruncateProteinDescription)
        Me.fraCalculationOptions.Controls.Add(Me.chkComputeSequenceHashValues)
        Me.fraCalculationOptions.Controls.Add(Me.lblMassMode)
        Me.fraCalculationOptions.Controls.Add(Me.cboElementMassMode)
        Me.fraCalculationOptions.Controls.Add(Me.chkExcludeProteinSequence)
        Me.fraCalculationOptions.Controls.Add(Me.chkComputepIandNET)
        Me.fraCalculationOptions.Controls.Add(Me.chkIncludeXResidues)
        Me.fraCalculationOptions.Controls.Add(Me.chkComputeProteinMass)
        Me.fraCalculationOptions.Location = New System.Drawing.Point(376, 40)
        Me.fraCalculationOptions.Name = "fraCalculationOptions"
        Me.fraCalculationOptions.Size = New System.Drawing.Size(308, 150)
        Me.fraCalculationOptions.TabIndex = 1
        Me.fraCalculationOptions.TabStop = False
        Me.fraCalculationOptions.Text = "Calculation Options"
        '
        'cmdNETInfo
        '
        Me.cmdNETInfo.Location = New System.Drawing.Point(268, 87)
        Me.cmdNETInfo.Margin = New System.Windows.Forms.Padding(1)
        Me.cmdNETInfo.Name = "cmdNETInfo"
        Me.cmdNETInfo.Size = New System.Drawing.Size(34, 20)
        Me.cmdNETInfo.TabIndex = 4
        Me.cmdNETInfo.Text = "Info"
        '
        'chkExcludeProteinDescription
        '
        Me.chkExcludeProteinDescription.Location = New System.Drawing.Point(185, 128)
        Me.chkExcludeProteinDescription.Name = "chkExcludeProteinDescription"
        Me.chkExcludeProteinDescription.Size = New System.Drawing.Size(120, 19)
        Me.chkExcludeProteinDescription.TabIndex = 9
        Me.chkExcludeProteinDescription.Text = "Exclude Description"
        '
        'chkComputeSequenceHashIgnoreILDiff
        '
        Me.chkComputeSequenceHashIgnoreILDiff.Checked = True
        Me.chkComputeSequenceHashIgnoreILDiff.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkComputeSequenceHashIgnoreILDiff.Location = New System.Drawing.Point(185, 107)
        Me.chkComputeSequenceHashIgnoreILDiff.Name = "chkComputeSequenceHashIgnoreILDiff"
        Me.chkComputeSequenceHashIgnoreILDiff.Size = New System.Drawing.Size(104, 19)
        Me.chkComputeSequenceHashIgnoreILDiff.TabIndex = 8
        Me.chkComputeSequenceHashIgnoreILDiff.Text = "Ignore I/L Diff"
        '
        'chkTruncateProteinDescription
        '
        Me.chkTruncateProteinDescription.Checked = True
        Me.chkTruncateProteinDescription.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkTruncateProteinDescription.Location = New System.Drawing.Point(16, 128)
        Me.chkTruncateProteinDescription.Name = "chkTruncateProteinDescription"
        Me.chkTruncateProteinDescription.Size = New System.Drawing.Size(164, 19)
        Me.chkTruncateProteinDescription.TabIndex = 7
        Me.chkTruncateProteinDescription.Text = "Truncate long description"
        '
        'chkComputeSequenceHashValues
        '
        Me.chkComputeSequenceHashValues.Checked = True
        Me.chkComputeSequenceHashValues.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkComputeSequenceHashValues.Location = New System.Drawing.Point(16, 107)
        Me.chkComputeSequenceHashValues.Name = "chkComputeSequenceHashValues"
        Me.chkComputeSequenceHashValues.Size = New System.Drawing.Size(164, 19)
        Me.chkComputeSequenceHashValues.TabIndex = 6
        Me.chkComputeSequenceHashValues.Text = "Compute sequence hashes"
        '
        'lblMassMode
        '
        Me.lblMassMode.Location = New System.Drawing.Point(16, 66)
        Me.lblMassMode.Name = "lblMassMode"
        Me.lblMassMode.Size = New System.Drawing.Size(64, 16)
        Me.lblMassMode.TabIndex = 5
        Me.lblMassMode.Text = "Mass type"
        Me.lblMassMode.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'cboElementMassMode
        '
        Me.cboElementMassMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboElementMassMode.Location = New System.Drawing.Point(88, 65)
        Me.cboElementMassMode.Name = "cboElementMassMode"
        Me.cboElementMassMode.Size = New System.Drawing.Size(144, 21)
        Me.cboElementMassMode.TabIndex = 4
        '
        'chkExcludeProteinSequence
        '
        Me.chkExcludeProteinSequence.Location = New System.Drawing.Point(16, 16)
        Me.chkExcludeProteinSequence.Name = "chkExcludeProteinSequence"
        Me.chkExcludeProteinSequence.Size = New System.Drawing.Size(192, 16)
        Me.chkExcludeProteinSequence.TabIndex = 0
        Me.chkExcludeProteinSequence.Text = "Exclude Protein Sequence"
        '
        'chkComputepIandNET
        '
        Me.chkComputepIandNET.Location = New System.Drawing.Point(16, 89)
        Me.chkComputepIandNET.Name = "chkComputepIandNET"
        Me.chkComputepIandNET.Size = New System.Drawing.Size(252, 18)
        Me.chkComputepIandNET.TabIndex = 3
        Me.chkComputepIandNET.Text = "Compute pI and Normalized Elution Time (NET)"
        '
        'chkIncludeXResidues
        '
        Me.chkIncludeXResidues.Location = New System.Drawing.Point(16, 49)
        Me.chkIncludeXResidues.Name = "chkIncludeXResidues"
        Me.chkIncludeXResidues.Size = New System.Drawing.Size(216, 16)
        Me.chkIncludeXResidues.TabIndex = 2
        Me.chkIncludeXResidues.Text = "Include X Residues in Mass (113 Da)"
        '
        'chkComputeProteinMass
        '
        Me.chkComputeProteinMass.Location = New System.Drawing.Point(16, 33)
        Me.chkComputeProteinMass.Name = "chkComputeProteinMass"
        Me.chkComputeProteinMass.Size = New System.Drawing.Size(144, 16)
        Me.chkComputeProteinMass.TabIndex = 1
        Me.chkComputeProteinMass.Text = "Compute Protein Mass"
        '
        'fraDigestionOptions
        '
        Me.fraDigestionOptions.Controls.Add(Me.cboFragmentMassMode)
        Me.fraDigestionOptions.Controls.Add(Me.lblFragmentMassMode)
        Me.fraDigestionOptions.Controls.Add(Me.cboCysTreatmentMode)
        Me.fraDigestionOptions.Controls.Add(Me.lblCysTreatment)
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
        Me.fraDigestionOptions.Location = New System.Drawing.Point(8, 190)
        Me.fraDigestionOptions.Name = "fraDigestionOptions"
        Me.fraDigestionOptions.Size = New System.Drawing.Size(675, 128)
        Me.fraDigestionOptions.TabIndex = 2
        Me.fraDigestionOptions.TabStop = False
        Me.fraDigestionOptions.Text = "Digestion Options"
        '
        'cboFragmentMassMode
        '
        Me.cboFragmentMassMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboFragmentMassMode.Location = New System.Drawing.Point(75, 104)
        Me.cboFragmentMassMode.Name = "cboFragmentMassMode"
        Me.cboFragmentMassMode.Size = New System.Drawing.Size(117, 21)
        Me.cboFragmentMassMode.TabIndex = 19
        '
        'lblFragmentMassMode
        '
        Me.lblFragmentMassMode.Location = New System.Drawing.Point(8, 106)
        Me.lblFragmentMassMode.Name = "lblFragmentMassMode"
        Me.lblFragmentMassMode.Size = New System.Drawing.Size(68, 16)
        Me.lblFragmentMassMode.TabIndex = 18
        Me.lblFragmentMassMode.Text = "Mass Mode"
        '
        'cboCysTreatmentMode
        '
        Me.cboCysTreatmentMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboCysTreatmentMode.Location = New System.Drawing.Point(553, 76)
        Me.cboCysTreatmentMode.Name = "cboCysTreatmentMode"
        Me.cboCysTreatmentMode.Size = New System.Drawing.Size(117, 21)
        Me.cboCysTreatmentMode.TabIndex = 17
        '
        'lblCysTreatment
        '
        Me.lblCysTreatment.Location = New System.Drawing.Point(553, 56)
        Me.lblCysTreatment.Name = "lblCysTreatment"
        Me.lblCysTreatment.Size = New System.Drawing.Size(104, 21)
        Me.lblCysTreatment.TabIndex = 16
        Me.lblCysTreatment.Text = "Cys treatment:"
        '
        'txtDigestProteinsMaximumpI
        '
        Me.txtDigestProteinsMaximumpI.Location = New System.Drawing.Point(497, 80)
        Me.txtDigestProteinsMaximumpI.Name = "txtDigestProteinsMaximumpI"
        Me.txtDigestProteinsMaximumpI.Size = New System.Drawing.Size(40, 20)
        Me.txtDigestProteinsMaximumpI.TabIndex = 13
        Me.txtDigestProteinsMaximumpI.Text = "14"
        '
        'lblDigestProteinsMaximumpI
        '
        Me.lblDigestProteinsMaximumpI.Location = New System.Drawing.Point(420, 80)
        Me.lblDigestProteinsMaximumpI.Name = "lblDigestProteinsMaximumpI"
        Me.lblDigestProteinsMaximumpI.Size = New System.Drawing.Size(72, 16)
        Me.lblDigestProteinsMaximumpI.TabIndex = 12
        Me.lblDigestProteinsMaximumpI.Text = "Maximum pI"
        '
        'txtDigestProteinsMinimumpI
        '
        Me.txtDigestProteinsMinimumpI.Location = New System.Drawing.Point(497, 56)
        Me.txtDigestProteinsMinimumpI.Name = "txtDigestProteinsMinimumpI"
        Me.txtDigestProteinsMinimumpI.Size = New System.Drawing.Size(40, 20)
        Me.txtDigestProteinsMinimumpI.TabIndex = 11
        Me.txtDigestProteinsMinimumpI.Text = "0"
        '
        'lblDigestProteinsMinimumpI
        '
        Me.lblDigestProteinsMinimumpI.Location = New System.Drawing.Point(420, 56)
        Me.lblDigestProteinsMinimumpI.Name = "lblDigestProteinsMinimumpI"
        Me.lblDigestProteinsMinimumpI.Size = New System.Drawing.Size(72, 16)
        Me.lblDigestProteinsMinimumpI.TabIndex = 10
        Me.lblDigestProteinsMinimumpI.Text = "Minimum pI"
        '
        'chkGenerateUniqueIDValues
        '
        Me.chkGenerateUniqueIDValues.Checked = True
        Me.chkGenerateUniqueIDValues.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkGenerateUniqueIDValues.Location = New System.Drawing.Point(218, 102)
        Me.chkGenerateUniqueIDValues.Name = "chkGenerateUniqueIDValues"
        Me.chkGenerateUniqueIDValues.Size = New System.Drawing.Size(176, 16)
        Me.chkGenerateUniqueIDValues.TabIndex = 14
        Me.chkGenerateUniqueIDValues.Text = "Generate UniqueID Values"
        '
        'chkCysPeptidesOnly
        '
        Me.chkCysPeptidesOnly.Location = New System.Drawing.Point(486, 16)
        Me.chkCysPeptidesOnly.Name = "chkCysPeptidesOnly"
        Me.chkCysPeptidesOnly.Size = New System.Drawing.Size(112, 32)
        Me.chkCysPeptidesOnly.TabIndex = 15
        Me.chkCysPeptidesOnly.Text = "Include cysteine peptides only"
        '
        'txtDigestProteinsMinimumResidueCount
        '
        Me.txtDigestProteinsMinimumResidueCount.Location = New System.Drawing.Point(359, 56)
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
        Me.txtDigestProteinsMaximumMissedCleavages.Location = New System.Drawing.Point(359, 80)
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
        Me.cboCleavageRuleType.Size = New System.Drawing.Size(288, 21)
        Me.cboCleavageRuleType.TabIndex = 0
        '
        'chkIncludeDuplicateSequences
        '
        Me.chkIncludeDuplicateSequences.Location = New System.Drawing.Point(312, 16)
        Me.chkIncludeDuplicateSequences.Name = "chkIncludeDuplicateSequences"
        Me.chkIncludeDuplicateSequences.Size = New System.Drawing.Size(168, 32)
        Me.chkIncludeDuplicateSequences.TabIndex = 1
        Me.chkIncludeDuplicateSequences.Text = "Include duplicate sequences for given protein"
        '
        'cmdParseInputFile
        '
        Me.cmdParseInputFile.Location = New System.Drawing.Point(384, 8)
        Me.cmdParseInputFile.Name = "cmdParseInputFile"
        Me.cmdParseInputFile.Size = New System.Drawing.Size(112, 24)
        Me.cmdParseInputFile.TabIndex = 3
        Me.cmdParseInputFile.Text = "&Parse and Digest"
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
        Me.TabPagePeakMatchingThresholds.Size = New System.Drawing.Size(696, 328)
        Me.TabPagePeakMatchingThresholds.TabIndex = 3
        Me.TabPagePeakMatchingThresholds.Text = "Peak Matching Thresholds"
        Me.TabPagePeakMatchingThresholds.UseVisualStyleBackColor = True
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
        'TabPageProgress
        '
        Me.TabPageProgress.Controls.Add(Me.pbarProgress)
        Me.TabPageProgress.Controls.Add(Me.lblErrorMessage)
        Me.TabPageProgress.Controls.Add(Me.lblSubtaskProgress)
        Me.TabPageProgress.Controls.Add(Me.lblProgress)
        Me.TabPageProgress.Controls.Add(Me.lblSubtaskProgressDescription)
        Me.TabPageProgress.Controls.Add(Me.lblProgressDescription)
        Me.TabPageProgress.Controls.Add(Me.cmdAbortProcessing)
        Me.TabPageProgress.Location = New System.Drawing.Point(4, 22)
        Me.TabPageProgress.Name = "TabPageProgress"
        Me.TabPageProgress.Size = New System.Drawing.Size(696, 328)
        Me.TabPageProgress.TabIndex = 4
        Me.TabPageProgress.Text = "Progress"
        Me.TabPageProgress.UseVisualStyleBackColor = True
        '
        'pbarProgress
        '
        Me.pbarProgress.Location = New System.Drawing.Point(13, 12)
        Me.pbarProgress.Name = "pbarProgress"
        Me.pbarProgress.Size = New System.Drawing.Size(122, 20)
        Me.pbarProgress.TabIndex = 12
        '
        'lblErrorMessage
        '
        Me.lblErrorMessage.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblErrorMessage.Location = New System.Drawing.Point(137, 112)
        Me.lblErrorMessage.Name = "lblErrorMessage"
        Me.lblErrorMessage.Size = New System.Drawing.Size(515, 32)
        Me.lblErrorMessage.TabIndex = 11
        Me.lblErrorMessage.Text = "Error message ..."
        Me.lblErrorMessage.Visible = False
        '
        'lblSubtaskProgress
        '
        Me.lblSubtaskProgress.Location = New System.Drawing.Point(13, 61)
        Me.lblSubtaskProgress.Name = "lblSubtaskProgress"
        Me.lblSubtaskProgress.Size = New System.Drawing.Size(118, 18)
        Me.lblSubtaskProgress.TabIndex = 8
        Me.lblSubtaskProgress.Text = "0"
        Me.lblSubtaskProgress.Visible = False
        '
        'lblProgress
        '
        Me.lblProgress.Location = New System.Drawing.Point(13, 35)
        Me.lblProgress.Name = "lblProgress"
        Me.lblProgress.Size = New System.Drawing.Size(118, 18)
        Me.lblProgress.TabIndex = 7
        Me.lblProgress.Text = "0"
        '
        'lblSubtaskProgressDescription
        '
        Me.lblSubtaskProgressDescription.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblSubtaskProgressDescription.Location = New System.Drawing.Point(140, 61)
        Me.lblSubtaskProgressDescription.Name = "lblSubtaskProgressDescription"
        Me.lblSubtaskProgressDescription.Size = New System.Drawing.Size(515, 32)
        Me.lblSubtaskProgressDescription.TabIndex = 6
        Me.lblSubtaskProgressDescription.Text = "Subtask progress description ..."
        Me.lblSubtaskProgressDescription.Visible = False
        '
        'lblProgressDescription
        '
        Me.lblProgressDescription.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblProgressDescription.Location = New System.Drawing.Point(140, 12)
        Me.lblProgressDescription.Name = "lblProgressDescription"
        Me.lblProgressDescription.Size = New System.Drawing.Size(515, 32)
        Me.lblProgressDescription.TabIndex = 5
        Me.lblProgressDescription.Text = "Progress description ..."
        '
        'cmdAbortProcessing
        '
        Me.cmdAbortProcessing.Location = New System.Drawing.Point(10, 106)
        Me.cmdAbortProcessing.Name = "cmdAbortProcessing"
        Me.cmdAbortProcessing.Size = New System.Drawing.Size(121, 24)
        Me.cmdAbortProcessing.TabIndex = 4
        Me.cmdAbortProcessing.Text = "Abort Processing"
        '
        'mnuHelpAboutElutionTime
        '
        Me.mnuHelpAboutElutionTime.Index = 1
        Me.mnuHelpAboutElutionTime.Text = "About &Elution Time Prediction"
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
        Me.fraInputFilePath.Location = New System.Drawing.Point(12, 12)
        Me.fraInputFilePath.Name = "fraInputFilePath"
        Me.fraInputFilePath.Size = New System.Drawing.Size(730, 88)
        Me.fraInputFilePath.TabIndex = 3
        Me.fraInputFilePath.TabStop = False
        Me.fraInputFilePath.Text = "Protein Input File Path (FASTA or Tab-delimited)"
        '
        'cmdValidateFastaFile
        '
        Me.cmdValidateFastaFile.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdValidateFastaFile.Location = New System.Drawing.Point(595, 56)
        Me.cmdValidateFastaFile.Name = "cmdValidateFastaFile"
        Me.cmdValidateFastaFile.Size = New System.Drawing.Size(120, 24)
        Me.cmdValidateFastaFile.TabIndex = 4
        Me.cmdValidateFastaFile.Text = "&Validate FASTA File"
        '
        'chkEnableLogging
        '
        Me.chkEnableLogging.Checked = True
        Me.chkEnableLogging.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkEnableLogging.Location = New System.Drawing.Point(422, 19)
        Me.chkEnableLogging.Name = "chkEnableLogging"
        Me.chkEnableLogging.Size = New System.Drawing.Size(112, 24)
        Me.chkEnableLogging.TabIndex = 4
        Me.chkEnableLogging.Text = "Enable logging"
        '
        'mnuFileSelectOutputFile
        '
        Me.mnuFileSelectOutputFile.Index = 1
        Me.mnuFileSelectOutputFile.Text = "Select &Output File..."
        '
        'cmdSelectOutputFile
        '
        Me.cmdSelectOutputFile.Location = New System.Drawing.Point(8, 56)
        Me.cmdSelectOutputFile.Name = "cmdSelectOutputFile"
        Me.cmdSelectOutputFile.Size = New System.Drawing.Size(88, 33)
        Me.cmdSelectOutputFile.TabIndex = 5
        Me.cmdSelectOutputFile.Text = "Select / &Create File"
        '
        'mnuFileSep1
        '
        Me.mnuFileSep1.Index = 2
        Me.mnuFileSep1.Text = "-"
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
        'txtProteinOutputFilePath
        '
        Me.txtProteinOutputFilePath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtProteinOutputFilePath.Location = New System.Drawing.Point(104, 62)
        Me.txtProteinOutputFilePath.Name = "txtProteinOutputFilePath"
        Me.txtProteinOutputFilePath.Size = New System.Drawing.Size(611, 20)
        Me.txtProteinOutputFilePath.TabIndex = 6
        '
        'chkIncludePrefixAndSuffixResidues
        '
        Me.chkIncludePrefixAndSuffixResidues.Location = New System.Drawing.Point(256, 16)
        Me.chkIncludePrefixAndSuffixResidues.Name = "chkIncludePrefixAndSuffixResidues"
        Me.chkIncludePrefixAndSuffixResidues.Size = New System.Drawing.Size(160, 32)
        Me.chkIncludePrefixAndSuffixResidues.TabIndex = 3
        Me.chkIncludePrefixAndSuffixResidues.Text = "Include prefix and suffix residues for the sequences"
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
        'mnuEditSep1
        '
        Me.mnuEditSep1.Index = 2
        Me.mnuEditSep1.Text = "-"
        '
        'mnuEditMakeUniquenessStats
        '
        Me.mnuEditMakeUniquenessStats.Index = 1
        Me.mnuEditMakeUniquenessStats.Text = "&Make Uniqueness Stats"
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
        'MainMenuControl
        '
        Me.MainMenuControl.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.mnuFile, Me.mnuEdit, Me.mnuHelp})
        '
        'lblOutputFileFieldDelimiter
        '
        Me.lblOutputFileFieldDelimiter.Location = New System.Drawing.Point(8, 26)
        Me.lblOutputFileFieldDelimiter.Name = "lblOutputFileFieldDelimiter"
        Me.lblOutputFileFieldDelimiter.Size = New System.Drawing.Size(112, 18)
        Me.lblOutputFileFieldDelimiter.TabIndex = 0
        Me.lblOutputFileFieldDelimiter.Text = "Field delimiter"
        '
        'cboOutputFileFieldDelimiter
        '
        Me.cboOutputFileFieldDelimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboOutputFileFieldDelimiter.Location = New System.Drawing.Point(128, 24)
        Me.cboOutputFileFieldDelimiter.Name = "cboOutputFileFieldDelimiter"
        Me.cboOutputFileFieldDelimiter.Size = New System.Drawing.Size(70, 21)
        Me.cboOutputFileFieldDelimiter.TabIndex = 1
        '
        'txtOutputFileFieldDelimiter
        '
        Me.txtOutputFileFieldDelimiter.Location = New System.Drawing.Point(208, 24)
        Me.txtOutputFileFieldDelimiter.MaxLength = 1
        Me.txtOutputFileFieldDelimiter.Name = "txtOutputFileFieldDelimiter"
        Me.txtOutputFileFieldDelimiter.Size = New System.Drawing.Size(32, 20)
        Me.txtOutputFileFieldDelimiter.TabIndex = 2
        Me.txtOutputFileFieldDelimiter.Text = ";"
        '
        'fraOutputTextOptions
        '
        Me.fraOutputTextOptions.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.fraOutputTextOptions.Controls.Add(Me.chkEnableLogging)
        Me.fraOutputTextOptions.Controls.Add(Me.cmdSelectOutputFile)
        Me.fraOutputTextOptions.Controls.Add(Me.txtProteinOutputFilePath)
        Me.fraOutputTextOptions.Controls.Add(Me.chkIncludePrefixAndSuffixResidues)
        Me.fraOutputTextOptions.Controls.Add(Me.cboOutputFileFieldDelimiter)
        Me.fraOutputTextOptions.Controls.Add(Me.txtOutputFileFieldDelimiter)
        Me.fraOutputTextOptions.Controls.Add(Me.lblOutputFileFieldDelimiter)
        Me.fraOutputTextOptions.Location = New System.Drawing.Point(12, 108)
        Me.fraOutputTextOptions.Name = "fraOutputTextOptions"
        Me.fraOutputTextOptions.Size = New System.Drawing.Size(730, 96)
        Me.fraOutputTextOptions.TabIndex = 4
        Me.fraOutputTextOptions.TabStop = False
        Me.fraOutputTextOptions.Text = "Output Options"
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(754, 577)
        Me.Controls.Add(Me.tbsOptions)
        Me.Controls.Add(Me.fraInputFilePath)
        Me.Controls.Add(Me.fraOutputTextOptions)
        Me.Menu = Me.MainMenuControl
        Me.Name = "frmMain"
        Me.Text = "Protein Digestion Simulator"
        Me.fraPeakMatchingOptions.ResumeLayout(False)
        Me.fraPeakMatchingOptions.PerformLayout()
        Me.TabPageUniquenessStats.ResumeLayout(False)
        Me.fraSqlServerOptions.ResumeLayout(False)
        Me.fraSqlServerOptions.PerformLayout()
        Me.fraUniquenessBinningOptions.ResumeLayout(False)
        Me.fraUniquenessBinningOptions.PerformLayout()
        Me.frapIAndHydrophobicity.ResumeLayout(False)
        Me.frapIAndHydrophobicity.PerformLayout()
        Me.fraDelimitedFileOptions.ResumeLayout(False)
        Me.fraDelimitedFileOptions.PerformLayout()
        Me.TabPageFileFormatOptions.ResumeLayout(False)
        Me.tbsOptions.ResumeLayout(False)
        Me.TabPageParseAndDigest.ResumeLayout(False)
        Me.fraProcessingOptions.ResumeLayout(False)
        Me.fraProcessingOptions.PerformLayout()
        Me.fraCalculationOptions.ResumeLayout(False)
        Me.fraDigestionOptions.ResumeLayout(False)
        Me.fraDigestionOptions.PerformLayout()
        Me.TabPagePeakMatchingThresholds.ResumeLayout(False)
        CType(Me.dgPeakMatchingThresholds, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPageProgress.ResumeLayout(False)
        Me.fraInputFilePath.ResumeLayout(False)
        Me.fraInputFilePath.PerformLayout()
        Me.fraOutputTextOptions.ResumeLayout(False)
        Me.fraOutputTextOptions.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtProteinInputFilePath As System.Windows.Forms.TextBox
    Friend WithEvents optUseRectangleSearchRegion As System.Windows.Forms.RadioButton
    Friend WithEvents optUseEllipseSearchRegion As System.Windows.Forms.RadioButton
    Friend WithEvents lblUniquenessCalculationsNote As System.Windows.Forms.Label
    Friend WithEvents lblProteinScramblingLoopCount As System.Windows.Forms.Label
    Friend WithEvents fraPeakMatchingOptions As System.Windows.Forms.GroupBox
    Friend WithEvents txtMaxPeakMatchingResultsPerFeatureToSave As System.Windows.Forms.TextBox
    Friend WithEvents lblMaxPeakMatchingResultsPerFeatureToSave As System.Windows.Forms.Label
    Friend WithEvents chkExportPeakMatchingResults As System.Windows.Forms.CheckBox
    Friend WithEvents txtMinimumSLiCScore As System.Windows.Forms.TextBox
    Friend WithEvents lblMinimumSLiCScore As System.Windows.Forms.Label
    Friend WithEvents chkUseSLiCScoreForUniqueness As System.Windows.Forms.CheckBox
    Friend WithEvents TabPageUniquenessStats As System.Windows.Forms.TabPage
    Friend WithEvents fraSqlServerOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkSqlServerUseExistingData As System.Windows.Forms.CheckBox
    Friend WithEvents chkAllowSqlServerCaching As System.Windows.Forms.CheckBox
    Friend WithEvents lblSqlServerPassword As System.Windows.Forms.Label
    Friend WithEvents lblSqlServerUsername As System.Windows.Forms.Label
    Friend WithEvents txtSqlServerPassword As System.Windows.Forms.TextBox
    Friend WithEvents txtSqlServerUsername As System.Windows.Forms.TextBox
    Friend WithEvents lblSqlServerDatabase As System.Windows.Forms.Label
    Friend WithEvents lblSqlServerServerName As System.Windows.Forms.Label
    Friend WithEvents chkSqlServerUseIntegratedSecurity As System.Windows.Forms.CheckBox
    Friend WithEvents txtSqlServerDatabase As System.Windows.Forms.TextBox
    Friend WithEvents txtSqlServerName As System.Windows.Forms.TextBox
    Friend WithEvents chkUseSqlServerDBToCacheData As System.Windows.Forms.CheckBox
    Friend WithEvents fraUniquenessBinningOptions As System.Windows.Forms.GroupBox
    Friend WithEvents lblPeptideUniquenessMassMode As System.Windows.Forms.Label
    Friend WithEvents txtUniquenessBinWidth As System.Windows.Forms.TextBox
    Friend WithEvents lblUniquenessBinWidth As System.Windows.Forms.Label
    Friend WithEvents chkAutoComputeRangeForBinning As System.Windows.Forms.CheckBox
    Friend WithEvents txtUniquenessBinEndMass As System.Windows.Forms.TextBox
    Friend WithEvents lblUniquenessBinEndMass As System.Windows.Forms.Label
    Friend WithEvents txtUniquenessBinStartMass As System.Windows.Forms.TextBox
    Friend WithEvents lblUniquenessBinStartMass As System.Windows.Forms.Label
    Friend WithEvents lblUniquenessStatsNote As System.Windows.Forms.Label
    Friend WithEvents cmdGenerateUniquenessStats As System.Windows.Forms.Button
    Friend WithEvents chkAssumeInputFileIsDigested As System.Windows.Forms.CheckBox
    Friend WithEvents txtProteinScramblingLoopCount As System.Windows.Forms.TextBox
    Friend WithEvents lblSamplingPercentageUnits As System.Windows.Forms.Label
    Friend WithEvents txtMaxpISequenceLength As System.Windows.Forms.TextBox
    Friend WithEvents lblProteinReversalSamplingPercentage As System.Windows.Forms.Label
    Friend WithEvents lblMaxpISequenceLength As System.Windows.Forms.Label
    Friend WithEvents chkMaxpIModeEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents frapIAndHydrophobicity As System.Windows.Forms.GroupBox
    Friend WithEvents lblHydrophobicityMode As System.Windows.Forms.Label
    Friend WithEvents cboHydrophobicityMode As System.Windows.Forms.ComboBox
    Friend WithEvents txtpIStats As System.Windows.Forms.TextBox
    Friend WithEvents txtSequenceForpI As System.Windows.Forms.TextBox
    Friend WithEvents lblSequenceForpI As System.Windows.Forms.Label
    Friend WithEvents fraDelimitedFileOptions As System.Windows.Forms.GroupBox
    Friend WithEvents cboInputFileColumnOrdering As System.Windows.Forms.ComboBox
    Friend WithEvents lblInputFileColumnOrdering As System.Windows.Forms.Label
    Friend WithEvents txtInputFileColumnDelimiter As System.Windows.Forms.TextBox
    Friend WithEvents lblInputFileColumnDelimiter As System.Windows.Forms.Label
    Friend WithEvents cboInputFileColumnDelimiter As System.Windows.Forms.ComboBox
    Friend WithEvents TabPageFileFormatOptions As System.Windows.Forms.TabPage
    Friend WithEvents tbsOptions As System.Windows.Forms.TabControl
    Friend WithEvents TabPageParseAndDigest As System.Windows.Forms.TabPage
    Friend WithEvents fraProcessingOptions As System.Windows.Forms.GroupBox
    Friend WithEvents txtProteinReversalSamplingPercentage As System.Windows.Forms.TextBox
    Friend WithEvents lbltxtAddnlRefAccessionSepChar As System.Windows.Forms.Label
    Friend WithEvents chkLookForAddnlRefInDescription As System.Windows.Forms.CheckBox
    Friend WithEvents cboProteinReversalOptions As System.Windows.Forms.ComboBox
    Friend WithEvents lblProteinReversalOptions As System.Windows.Forms.Label
    Friend WithEvents chkDigestProteins As System.Windows.Forms.CheckBox
    Friend WithEvents lblAddnlRefSepChar As System.Windows.Forms.Label
    Friend WithEvents txtAddnlRefAccessionSepChar As System.Windows.Forms.TextBox
    Friend WithEvents txtAddnlRefSepChar As System.Windows.Forms.TextBox
    Friend WithEvents chkCreateFastaOutputFile As System.Windows.Forms.CheckBox
    Friend WithEvents fraCalculationOptions As System.Windows.Forms.GroupBox
    Friend WithEvents lblMassMode As System.Windows.Forms.Label
    Friend WithEvents cboElementMassMode As System.Windows.Forms.ComboBox
    Friend WithEvents chkExcludeProteinSequence As System.Windows.Forms.CheckBox
    Friend WithEvents chkComputepIandNET As System.Windows.Forms.CheckBox
    Friend WithEvents chkIncludeXResidues As System.Windows.Forms.CheckBox
    Friend WithEvents chkComputeProteinMass As System.Windows.Forms.CheckBox
    Friend WithEvents fraDigestionOptions As System.Windows.Forms.GroupBox
    Friend WithEvents txtDigestProteinsMaximumpI As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMaximumpI As System.Windows.Forms.Label
    Friend WithEvents txtDigestProteinsMinimumpI As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMinimumpI As System.Windows.Forms.Label
    Friend WithEvents chkGenerateUniqueIDValues As System.Windows.Forms.CheckBox
    Friend WithEvents chkCysPeptidesOnly As System.Windows.Forms.CheckBox
    Friend WithEvents txtDigestProteinsMinimumResidueCount As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMinimumResidueCount As System.Windows.Forms.Label
    Friend WithEvents txtDigestProteinsMaximumMissedCleavages As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMaximumMissedCleavages As System.Windows.Forms.Label
    Friend WithEvents txtDigestProteinsMaximumMass As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMaximumMass As System.Windows.Forms.Label
    Friend WithEvents txtDigestProteinsMinimumMass As System.Windows.Forms.TextBox
    Friend WithEvents lblDigestProteinsMinimumMass As System.Windows.Forms.Label
    Friend WithEvents cboCleavageRuleType As System.Windows.Forms.ComboBox
    Friend WithEvents chkIncludeDuplicateSequences As System.Windows.Forms.CheckBox
    Friend WithEvents cmdParseInputFile As System.Windows.Forms.Button
    Friend WithEvents TabPagePeakMatchingThresholds As System.Windows.Forms.TabPage
    Friend WithEvents chkAutoDefineSLiCScoreTolerances As System.Windows.Forms.CheckBox
    Friend WithEvents cmdPastePMThresholdsList As System.Windows.Forms.Button
    Friend WithEvents cboPMPredefinedThresholds As System.Windows.Forms.ComboBox
    Friend WithEvents cmdPMThresholdsAutoPopulate As System.Windows.Forms.Button
    Friend WithEvents cmdClearPMThresholdsList As System.Windows.Forms.Button
    Friend WithEvents cboMassTolType As System.Windows.Forms.ComboBox
    Friend WithEvents lblMassTolType As System.Windows.Forms.Label
    Friend WithEvents dgPeakMatchingThresholds As System.Windows.Forms.DataGrid
    Friend WithEvents TabPageProgress As System.Windows.Forms.TabPage
    Friend WithEvents pbarProgress As System.Windows.Forms.ProgressBar
    Friend WithEvents lblErrorMessage As System.Windows.Forms.Label
    Friend WithEvents lblSubtaskProgress As System.Windows.Forms.Label
    Friend WithEvents lblProgress As System.Windows.Forms.Label
    Friend WithEvents lblSubtaskProgressDescription As System.Windows.Forms.Label
    Friend WithEvents lblProgressDescription As System.Windows.Forms.Label
    Friend WithEvents cmdAbortProcessing As System.Windows.Forms.Button
    Friend WithEvents mnuHelpAboutElutionTime As System.Windows.Forms.MenuItem
    Friend WithEvents cboInputFileFormat As System.Windows.Forms.ComboBox
    Friend WithEvents lblInputFileFormat As System.Windows.Forms.Label
    Friend WithEvents cmdSelectFile As System.Windows.Forms.Button
    Friend WithEvents fraInputFilePath As System.Windows.Forms.GroupBox
    Friend WithEvents cmdValidateFastaFile As System.Windows.Forms.Button
    Friend WithEvents chkEnableLogging As System.Windows.Forms.CheckBox
    Friend WithEvents mnuFileSelectOutputFile As System.Windows.Forms.MenuItem
    Friend WithEvents cmdSelectOutputFile As System.Windows.Forms.Button
    Friend WithEvents mnuFileSep1 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileSelectInputFile As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileSaveDefaultOptions As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileSep2 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFileExit As System.Windows.Forms.MenuItem
    Friend WithEvents txtProteinOutputFilePath As System.Windows.Forms.TextBox
    Friend WithEvents chkIncludePrefixAndSuffixResidues As System.Windows.Forms.CheckBox
    Friend WithEvents mnuEditResetOptions As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelp As System.Windows.Forms.MenuItem
    Friend WithEvents mnuHelpAbout As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditSep1 As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditMakeUniquenessStats As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEdit As System.Windows.Forms.MenuItem
    Friend WithEvents mnuEditParseFile As System.Windows.Forms.MenuItem
    Friend WithEvents MainMenuControl As System.Windows.Forms.MainMenu
    Friend WithEvents lblOutputFileFieldDelimiter As System.Windows.Forms.Label
    Friend WithEvents cboOutputFileFieldDelimiter As System.Windows.Forms.ComboBox
    Friend WithEvents txtOutputFileFieldDelimiter As System.Windows.Forms.TextBox
    Friend WithEvents fraOutputTextOptions As System.Windows.Forms.GroupBox
    Friend WithEvents chkTruncateProteinDescription As System.Windows.Forms.CheckBox
    Friend WithEvents chkComputeSequenceHashValues As System.Windows.Forms.CheckBox
    Friend WithEvents chkComputeSequenceHashIgnoreILDiff As System.Windows.Forms.CheckBox
    Friend WithEvents chkExcludeProteinDescription As CheckBox
    Friend WithEvents cmdNETInfo As Button
    Friend WithEvents cboCysTreatmentMode As ComboBox
    Friend WithEvents lblCysTreatment As Label
    Friend WithEvents cboFragmentMassMode As ComboBox
    Friend WithEvents lblFragmentMassMode As Label
End Class
