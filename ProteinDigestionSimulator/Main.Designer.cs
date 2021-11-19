using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProteinDigestionSimulator
{
    public partial class Main : Form
    {
        // Ignore Spelling: addnl, cys, cysteine, Da, diff, diffs, gauging, Hydrophobicity, sep, silico, Username

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtProteinInputFilePath = new System.Windows.Forms.TextBox();
            this.optUseRectangleSearchRegion = new System.Windows.Forms.RadioButton();
            this.optUseEllipseSearchRegion = new System.Windows.Forms.RadioButton();
            this.lblUniquenessCalculationsNote = new System.Windows.Forms.Label();
            this.lblProteinScramblingLoopCount = new System.Windows.Forms.Label();
            this.fraPeakMatchingOptions = new System.Windows.Forms.GroupBox();
            this.txtMaxPeakMatchingResultsPerFeatureToSave = new System.Windows.Forms.TextBox();
            this.lblMaxPeakMatchingResultsPerFeatureToSave = new System.Windows.Forms.Label();
            this.chkExportPeakMatchingResults = new System.Windows.Forms.CheckBox();
            this.txtMinimumSLiCScore = new System.Windows.Forms.TextBox();
            this.lblMinimumSLiCScore = new System.Windows.Forms.Label();
            this.chkUseSLiCScoreForUniqueness = new System.Windows.Forms.CheckBox();
            this.TabPageUniquenessStats = new System.Windows.Forms.TabPage();
            this.fraSqlServerOptions = new System.Windows.Forms.GroupBox();
            this.chkSqlServerUseExistingData = new System.Windows.Forms.CheckBox();
            this.chkAllowSqlServerCaching = new System.Windows.Forms.CheckBox();
            this.lblSqlServerPassword = new System.Windows.Forms.Label();
            this.lblSqlServerUsername = new System.Windows.Forms.Label();
            this.txtSqlServerPassword = new System.Windows.Forms.TextBox();
            this.txtSqlServerUsername = new System.Windows.Forms.TextBox();
            this.lblSqlServerDatabase = new System.Windows.Forms.Label();
            this.lblSqlServerServerName = new System.Windows.Forms.Label();
            this.chkSqlServerUseIntegratedSecurity = new System.Windows.Forms.CheckBox();
            this.txtSqlServerDatabase = new System.Windows.Forms.TextBox();
            this.txtSqlServerName = new System.Windows.Forms.TextBox();
            this.chkUseSqlServerDBToCacheData = new System.Windows.Forms.CheckBox();
            this.fraUniquenessBinningOptions = new System.Windows.Forms.GroupBox();
            this.lblPeptideUniquenessMassMode = new System.Windows.Forms.Label();
            this.txtUniquenessBinWidth = new System.Windows.Forms.TextBox();
            this.lblUniquenessBinWidth = new System.Windows.Forms.Label();
            this.chkAutoComputeRangeForBinning = new System.Windows.Forms.CheckBox();
            this.txtUniquenessBinEndMass = new System.Windows.Forms.TextBox();
            this.lblUniquenessBinEndMass = new System.Windows.Forms.Label();
            this.txtUniquenessBinStartMass = new System.Windows.Forms.TextBox();
            this.lblUniquenessBinStartMass = new System.Windows.Forms.Label();
            this.lblUniquenessStatsNote = new System.Windows.Forms.Label();
            this.cmdGenerateUniquenessStats = new System.Windows.Forms.Button();
            this.chkAssumeInputFileIsDigested = new System.Windows.Forms.CheckBox();
            this.txtProteinScramblingLoopCount = new System.Windows.Forms.TextBox();
            this.lblSamplingPercentageUnits = new System.Windows.Forms.Label();
            this.txtMaxpISequenceLength = new System.Windows.Forms.TextBox();
            this.lblProteinReversalSamplingPercentage = new System.Windows.Forms.Label();
            this.lblMaxpISequenceLength = new System.Windows.Forms.Label();
            this.chkMaxpIModeEnabled = new System.Windows.Forms.CheckBox();
            this.frapIAndHydrophobicity = new System.Windows.Forms.GroupBox();
            this.lblHydrophobicityMode = new System.Windows.Forms.Label();
            this.cboHydrophobicityMode = new System.Windows.Forms.ComboBox();
            this.txtpIStats = new System.Windows.Forms.TextBox();
            this.txtSequenceForpI = new System.Windows.Forms.TextBox();
            this.lblSequenceForpI = new System.Windows.Forms.Label();
            this.fraDelimitedFileOptions = new System.Windows.Forms.GroupBox();
            this.cboInputFileColumnOrdering = new System.Windows.Forms.ComboBox();
            this.lblInputFileColumnOrdering = new System.Windows.Forms.Label();
            this.txtInputFileColumnDelimiter = new System.Windows.Forms.TextBox();
            this.lblInputFileColumnDelimiter = new System.Windows.Forms.Label();
            this.cboInputFileColumnDelimiter = new System.Windows.Forms.ComboBox();
            this.TabPageFileFormatOptions = new System.Windows.Forms.TabPage();
            this.tbsOptions = new System.Windows.Forms.TabControl();
            this.TabPageParseAndDigest = new System.Windows.Forms.TabPage();
            this.fraProcessingOptions = new System.Windows.Forms.GroupBox();
            this.txtProteinReversalSamplingPercentage = new System.Windows.Forms.TextBox();
            this.lbltxtAddnlRefAccessionSepChar = new System.Windows.Forms.Label();
            this.chkLookForAddnlRefInDescription = new System.Windows.Forms.CheckBox();
            this.cboProteinReversalOptions = new System.Windows.Forms.ComboBox();
            this.lblProteinReversalOptions = new System.Windows.Forms.Label();
            this.chkDigestProteins = new System.Windows.Forms.CheckBox();
            this.lblAddnlRefSepChar = new System.Windows.Forms.Label();
            this.txtAddnlRefAccessionSepChar = new System.Windows.Forms.TextBox();
            this.txtAddnlRefSepChar = new System.Windows.Forms.TextBox();
            this.chkCreateFastaOutputFile = new System.Windows.Forms.CheckBox();
            this.fraCalculationOptions = new System.Windows.Forms.GroupBox();
            this.cmdNETInfo = new System.Windows.Forms.Button();
            this.chkExcludeProteinDescription = new System.Windows.Forms.CheckBox();
            this.chkComputeSequenceHashIgnoreILDiff = new System.Windows.Forms.CheckBox();
            this.chkTruncateProteinDescription = new System.Windows.Forms.CheckBox();
            this.chkComputeSequenceHashValues = new System.Windows.Forms.CheckBox();
            this.lblMassMode = new System.Windows.Forms.Label();
            this.cboElementMassMode = new System.Windows.Forms.ComboBox();
            this.chkExcludeProteinSequence = new System.Windows.Forms.CheckBox();
            this.chkComputepIandNET = new System.Windows.Forms.CheckBox();
            this.chkIncludeXResidues = new System.Windows.Forms.CheckBox();
            this.chkComputeProteinMass = new System.Windows.Forms.CheckBox();
            this.fraDigestionOptions = new System.Windows.Forms.GroupBox();
            this.cboFragmentMassMode = new System.Windows.Forms.ComboBox();
            this.lblFragmentMassMode = new System.Windows.Forms.Label();
            this.cboCysTreatmentMode = new System.Windows.Forms.ComboBox();
            this.lblCysTreatment = new System.Windows.Forms.Label();
            this.txtDigestProteinsMaximumpI = new System.Windows.Forms.TextBox();
            this.lblDigestProteinsMaximumpI = new System.Windows.Forms.Label();
            this.txtDigestProteinsMinimumpI = new System.Windows.Forms.TextBox();
            this.lblDigestProteinsMinimumpI = new System.Windows.Forms.Label();
            this.chkGenerateUniqueIDValues = new System.Windows.Forms.CheckBox();
            this.chkCysPeptidesOnly = new System.Windows.Forms.CheckBox();
            this.txtDigestProteinsMinimumResidueCount = new System.Windows.Forms.TextBox();
            this.lblDigestProteinsMinimumResidueCount = new System.Windows.Forms.Label();
            this.txtDigestProteinsMaximumMissedCleavages = new System.Windows.Forms.TextBox();
            this.lblDigestProteinsMaximumMissedCleavages = new System.Windows.Forms.Label();
            this.txtDigestProteinsMaximumMass = new System.Windows.Forms.TextBox();
            this.lblDigestProteinsMaximumMass = new System.Windows.Forms.Label();
            this.txtDigestProteinsMinimumMass = new System.Windows.Forms.TextBox();
            this.lblDigestProteinsMinimumMass = new System.Windows.Forms.Label();
            this.cboCleavageRuleType = new System.Windows.Forms.ComboBox();
            this.chkIncludeDuplicateSequences = new System.Windows.Forms.CheckBox();
            this.cmdParseInputFile = new System.Windows.Forms.Button();
            this.TabPagePeakMatchingThresholds = new System.Windows.Forms.TabPage();
            this.chkAutoDefineSLiCScoreTolerances = new System.Windows.Forms.CheckBox();
            this.cmdPastePMThresholdsList = new System.Windows.Forms.Button();
            this.cboPMPredefinedThresholds = new System.Windows.Forms.ComboBox();
            this.cmdPMThresholdsAutoPopulate = new System.Windows.Forms.Button();
            this.cmdClearPMThresholdsList = new System.Windows.Forms.Button();
            this.cboMassTolType = new System.Windows.Forms.ComboBox();
            this.lblMassTolType = new System.Windows.Forms.Label();
            this.dgPeakMatchingThresholds = new System.Windows.Forms.DataGrid();
            this.TabPageProgress = new System.Windows.Forms.TabPage();
            this.pbarProgress = new System.Windows.Forms.ProgressBar();
            this.lblErrorMessage = new System.Windows.Forms.Label();
            this.lblSubtaskProgress = new System.Windows.Forms.Label();
            this.lblProgress = new System.Windows.Forms.Label();
            this.lblSubtaskProgressDescription = new System.Windows.Forms.Label();
            this.lblProgressDescription = new System.Windows.Forms.Label();
            this.cmdAbortProcessing = new System.Windows.Forms.Button();
            this.mnuHelpAboutElutionTime = new System.Windows.Forms.MenuItem();
            this.cboInputFileFormat = new System.Windows.Forms.ComboBox();
            this.lblInputFileFormat = new System.Windows.Forms.Label();
            this.cmdSelectFile = new System.Windows.Forms.Button();
            this.fraInputFilePath = new System.Windows.Forms.GroupBox();
            this.cmdValidateFastaFile = new System.Windows.Forms.Button();
            this.chkEnableLogging = new System.Windows.Forms.CheckBox();
            this.mnuFileSelectOutputFile = new System.Windows.Forms.MenuItem();
            this.cmdSelectOutputFile = new System.Windows.Forms.Button();
            this.mnuFileSep1 = new System.Windows.Forms.MenuItem();
            this.mnuFile = new System.Windows.Forms.MenuItem();
            this.mnuFileSelectInputFile = new System.Windows.Forms.MenuItem();
            this.mnuFileSaveDefaultOptions = new System.Windows.Forms.MenuItem();
            this.mnuFileSep2 = new System.Windows.Forms.MenuItem();
            this.mnuFileExit = new System.Windows.Forms.MenuItem();
            this.txtProteinOutputFilePath = new System.Windows.Forms.TextBox();
            this.chkIncludePrefixAndSuffixResidues = new System.Windows.Forms.CheckBox();
            this.mnuEditResetOptions = new System.Windows.Forms.MenuItem();
            this.mnuHelp = new System.Windows.Forms.MenuItem();
            this.mnuHelpAbout = new System.Windows.Forms.MenuItem();
            this.mnuEditSep1 = new System.Windows.Forms.MenuItem();
            this.mnuEditMakeUniquenessStats = new System.Windows.Forms.MenuItem();
            this.mnuEdit = new System.Windows.Forms.MenuItem();
            this.mnuEditParseFile = new System.Windows.Forms.MenuItem();
            this.MainMenuControl = new System.Windows.Forms.MainMenu(this.components);
            this.lblOutputFileFieldDelimiter = new System.Windows.Forms.Label();
            this.cboOutputFileFieldDelimiter = new System.Windows.Forms.ComboBox();
            this.txtOutputFileFieldDelimiter = new System.Windows.Forms.TextBox();
            this.fraOutputTextOptions = new System.Windows.Forms.GroupBox();
            this.fraPeakMatchingOptions.SuspendLayout();
            this.TabPageUniquenessStats.SuspendLayout();
            this.fraSqlServerOptions.SuspendLayout();
            this.fraUniquenessBinningOptions.SuspendLayout();
            this.frapIAndHydrophobicity.SuspendLayout();
            this.fraDelimitedFileOptions.SuspendLayout();
            this.TabPageFileFormatOptions.SuspendLayout();
            this.tbsOptions.SuspendLayout();
            this.TabPageParseAndDigest.SuspendLayout();
            this.fraProcessingOptions.SuspendLayout();
            this.fraCalculationOptions.SuspendLayout();
            this.fraDigestionOptions.SuspendLayout();
            this.TabPagePeakMatchingThresholds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgPeakMatchingThresholds)).BeginInit();
            this.TabPageProgress.SuspendLayout();
            this.fraInputFilePath.SuspendLayout();
            this.fraOutputTextOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtProteinInputFilePath
            // 
            this.txtProteinInputFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProteinInputFilePath.Location = new System.Drawing.Point(104, 26);
            this.txtProteinInputFilePath.Name = "txtProteinInputFilePath";
            this.txtProteinInputFilePath.Size = new System.Drawing.Size(611, 20);
            this.txtProteinInputFilePath.TabIndex = 1;
            this.txtProteinInputFilePath.TextChanged += new System.EventHandler(this.txtProteinInputFilePath_TextChanged);
            // 
            // optUseRectangleSearchRegion
            // 
            this.optUseRectangleSearchRegion.Location = new System.Drawing.Point(232, 96);
            this.optUseRectangleSearchRegion.Name = "optUseRectangleSearchRegion";
            this.optUseRectangleSearchRegion.Size = new System.Drawing.Size(136, 16);
            this.optUseRectangleSearchRegion.TabIndex = 7;
            this.optUseRectangleSearchRegion.Text = "Use rectangle region";
            // 
            // optUseEllipseSearchRegion
            // 
            this.optUseEllipseSearchRegion.Checked = true;
            this.optUseEllipseSearchRegion.Location = new System.Drawing.Point(232, 72);
            this.optUseEllipseSearchRegion.Name = "optUseEllipseSearchRegion";
            this.optUseEllipseSearchRegion.Size = new System.Drawing.Size(152, 16);
            this.optUseEllipseSearchRegion.TabIndex = 6;
            this.optUseEllipseSearchRegion.TabStop = true;
            this.optUseEllipseSearchRegion.Text = "Use ellipse search region";
            // 
            // lblUniquenessCalculationsNote
            // 
            this.lblUniquenessCalculationsNote.Location = new System.Drawing.Point(240, 192);
            this.lblUniquenessCalculationsNote.Name = "lblUniquenessCalculationsNote";
            this.lblUniquenessCalculationsNote.Size = new System.Drawing.Size(384, 88);
            this.lblUniquenessCalculationsNote.TabIndex = 6;
            // 
            // lblProteinScramblingLoopCount
            // 
            this.lblProteinScramblingLoopCount.Location = new System.Drawing.Point(232, 42);
            this.lblProteinScramblingLoopCount.Name = "lblProteinScramblingLoopCount";
            this.lblProteinScramblingLoopCount.Size = new System.Drawing.Size(72, 16);
            this.lblProteinScramblingLoopCount.TabIndex = 12;
            this.lblProteinScramblingLoopCount.Text = "Loop Count";
            this.lblProteinScramblingLoopCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // fraPeakMatchingOptions
            // 
            this.fraPeakMatchingOptions.Controls.Add(this.optUseRectangleSearchRegion);
            this.fraPeakMatchingOptions.Controls.Add(this.optUseEllipseSearchRegion);
            this.fraPeakMatchingOptions.Controls.Add(this.txtMaxPeakMatchingResultsPerFeatureToSave);
            this.fraPeakMatchingOptions.Controls.Add(this.lblMaxPeakMatchingResultsPerFeatureToSave);
            this.fraPeakMatchingOptions.Controls.Add(this.chkExportPeakMatchingResults);
            this.fraPeakMatchingOptions.Controls.Add(this.txtMinimumSLiCScore);
            this.fraPeakMatchingOptions.Controls.Add(this.lblMinimumSLiCScore);
            this.fraPeakMatchingOptions.Controls.Add(this.chkUseSLiCScoreForUniqueness);
            this.fraPeakMatchingOptions.Location = new System.Drawing.Point(232, 48);
            this.fraPeakMatchingOptions.Name = "fraPeakMatchingOptions";
            this.fraPeakMatchingOptions.Size = new System.Drawing.Size(392, 136);
            this.fraPeakMatchingOptions.TabIndex = 2;
            this.fraPeakMatchingOptions.TabStop = false;
            this.fraPeakMatchingOptions.Text = "Peak Matching Options";
            // 
            // txtMaxPeakMatchingResultsPerFeatureToSave
            // 
            this.txtMaxPeakMatchingResultsPerFeatureToSave.Location = new System.Drawing.Point(272, 16);
            this.txtMaxPeakMatchingResultsPerFeatureToSave.Name = "txtMaxPeakMatchingResultsPerFeatureToSave";
            this.txtMaxPeakMatchingResultsPerFeatureToSave.Size = new System.Drawing.Size(40, 20);
            this.txtMaxPeakMatchingResultsPerFeatureToSave.TabIndex = 1;
            this.txtMaxPeakMatchingResultsPerFeatureToSave.Text = "3";
            this.txtMaxPeakMatchingResultsPerFeatureToSave.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress);
            this.txtMaxPeakMatchingResultsPerFeatureToSave.Validating += new System.ComponentModel.CancelEventHandler(this.txtMaxPeakMatchingResultsPerFeatureToSave_Validating);
            // 
            // lblMaxPeakMatchingResultsPerFeatureToSave
            // 
            this.lblMaxPeakMatchingResultsPerFeatureToSave.Location = new System.Drawing.Point(16, 18);
            this.lblMaxPeakMatchingResultsPerFeatureToSave.Name = "lblMaxPeakMatchingResultsPerFeatureToSave";
            this.lblMaxPeakMatchingResultsPerFeatureToSave.Size = new System.Drawing.Size(256, 16);
            this.lblMaxPeakMatchingResultsPerFeatureToSave.TabIndex = 0;
            this.lblMaxPeakMatchingResultsPerFeatureToSave.Text = "Max Peak Matching Results Per Feature To Save";
            // 
            // chkExportPeakMatchingResults
            // 
            this.chkExportPeakMatchingResults.Location = new System.Drawing.Point(32, 36);
            this.chkExportPeakMatchingResults.Name = "chkExportPeakMatchingResults";
            this.chkExportPeakMatchingResults.Size = new System.Drawing.Size(192, 17);
            this.chkExportPeakMatchingResults.TabIndex = 2;
            this.chkExportPeakMatchingResults.Text = "Export peak matching results";
            // 
            // txtMinimumSLiCScore
            // 
            this.txtMinimumSLiCScore.Location = new System.Drawing.Point(144, 104);
            this.txtMinimumSLiCScore.Name = "txtMinimumSLiCScore";
            this.txtMinimumSLiCScore.Size = new System.Drawing.Size(40, 20);
            this.txtMinimumSLiCScore.TabIndex = 5;
            this.txtMinimumSLiCScore.Text = "0.99";
            this.txtMinimumSLiCScore.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMinimumSLiCScore_KeyPress);
            this.txtMinimumSLiCScore.Validating += new System.ComponentModel.CancelEventHandler(this.txtMinimumSLiCScore_Validating);
            // 
            // lblMinimumSLiCScore
            // 
            this.lblMinimumSLiCScore.Location = new System.Drawing.Point(16, 96);
            this.lblMinimumSLiCScore.Name = "lblMinimumSLiCScore";
            this.lblMinimumSLiCScore.Size = new System.Drawing.Size(128, 32);
            this.lblMinimumSLiCScore.TabIndex = 4;
            this.lblMinimumSLiCScore.Text = "Minimum SLiC score to be considered unique";
            // 
            // chkUseSLiCScoreForUniqueness
            // 
            this.chkUseSLiCScoreForUniqueness.Checked = true;
            this.chkUseSLiCScoreForUniqueness.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseSLiCScoreForUniqueness.Location = new System.Drawing.Point(16, 60);
            this.chkUseSLiCScoreForUniqueness.Name = "chkUseSLiCScoreForUniqueness";
            this.chkUseSLiCScoreForUniqueness.Size = new System.Drawing.Size(168, 32);
            this.chkUseSLiCScoreForUniqueness.TabIndex = 3;
            this.chkUseSLiCScoreForUniqueness.Text = "Use SLiC Score when gauging peptide uniqueness";
            this.chkUseSLiCScoreForUniqueness.CheckedChanged += new System.EventHandler(this.chkUseSLiCScoreForUniqueness_CheckedChanged);
            // 
            // TabPageUniquenessStats
            // 
            this.TabPageUniquenessStats.Controls.Add(this.lblUniquenessCalculationsNote);
            this.TabPageUniquenessStats.Controls.Add(this.fraPeakMatchingOptions);
            this.TabPageUniquenessStats.Controls.Add(this.fraSqlServerOptions);
            this.TabPageUniquenessStats.Controls.Add(this.fraUniquenessBinningOptions);
            this.TabPageUniquenessStats.Controls.Add(this.lblUniquenessStatsNote);
            this.TabPageUniquenessStats.Controls.Add(this.cmdGenerateUniquenessStats);
            this.TabPageUniquenessStats.Controls.Add(this.chkAssumeInputFileIsDigested);
            this.TabPageUniquenessStats.Location = new System.Drawing.Point(4, 22);
            this.TabPageUniquenessStats.Name = "TabPageUniquenessStats";
            this.TabPageUniquenessStats.Size = new System.Drawing.Size(696, 328);
            this.TabPageUniquenessStats.TabIndex = 1;
            this.TabPageUniquenessStats.Text = "Peptide Uniqueness Options";
            this.TabPageUniquenessStats.UseVisualStyleBackColor = true;
            // 
            // fraSqlServerOptions
            // 
            this.fraSqlServerOptions.Controls.Add(this.chkSqlServerUseExistingData);
            this.fraSqlServerOptions.Controls.Add(this.chkAllowSqlServerCaching);
            this.fraSqlServerOptions.Controls.Add(this.lblSqlServerPassword);
            this.fraSqlServerOptions.Controls.Add(this.lblSqlServerUsername);
            this.fraSqlServerOptions.Controls.Add(this.txtSqlServerPassword);
            this.fraSqlServerOptions.Controls.Add(this.txtSqlServerUsername);
            this.fraSqlServerOptions.Controls.Add(this.lblSqlServerDatabase);
            this.fraSqlServerOptions.Controls.Add(this.lblSqlServerServerName);
            this.fraSqlServerOptions.Controls.Add(this.chkSqlServerUseIntegratedSecurity);
            this.fraSqlServerOptions.Controls.Add(this.txtSqlServerDatabase);
            this.fraSqlServerOptions.Controls.Add(this.txtSqlServerName);
            this.fraSqlServerOptions.Controls.Add(this.chkUseSqlServerDBToCacheData);
            this.fraSqlServerOptions.Location = new System.Drawing.Point(232, 192);
            this.fraSqlServerOptions.Name = "fraSqlServerOptions";
            this.fraSqlServerOptions.Size = new System.Drawing.Size(376, 112);
            this.fraSqlServerOptions.TabIndex = 4;
            this.fraSqlServerOptions.TabStop = false;
            this.fraSqlServerOptions.Text = "SQL Server Options";
            this.fraSqlServerOptions.Visible = false;
            // 
            // chkSqlServerUseExistingData
            // 
            this.chkSqlServerUseExistingData.Checked = true;
            this.chkSqlServerUseExistingData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSqlServerUseExistingData.Location = new System.Drawing.Point(8, 88);
            this.chkSqlServerUseExistingData.Name = "chkSqlServerUseExistingData";
            this.chkSqlServerUseExistingData.Size = new System.Drawing.Size(144, 16);
            this.chkSqlServerUseExistingData.TabIndex = 11;
            this.chkSqlServerUseExistingData.Text = "Use Existing Data";
            // 
            // chkAllowSqlServerCaching
            // 
            this.chkAllowSqlServerCaching.Location = new System.Drawing.Point(8, 16);
            this.chkAllowSqlServerCaching.Name = "chkAllowSqlServerCaching";
            this.chkAllowSqlServerCaching.Size = new System.Drawing.Size(144, 32);
            this.chkAllowSqlServerCaching.TabIndex = 0;
            this.chkAllowSqlServerCaching.Text = "Allow data caching using SQL Server";
            this.chkAllowSqlServerCaching.CheckedChanged += new System.EventHandler(this.chkAllowSqlServerCaching_CheckedChanged);
            // 
            // lblSqlServerPassword
            // 
            this.lblSqlServerPassword.Location = new System.Drawing.Point(264, 64);
            this.lblSqlServerPassword.Name = "lblSqlServerPassword";
            this.lblSqlServerPassword.Size = new System.Drawing.Size(56, 16);
            this.lblSqlServerPassword.TabIndex = 9;
            this.lblSqlServerPassword.Text = "Password";
            // 
            // lblSqlServerUsername
            // 
            this.lblSqlServerUsername.Location = new System.Drawing.Point(184, 64);
            this.lblSqlServerUsername.Name = "lblSqlServerUsername";
            this.lblSqlServerUsername.Size = new System.Drawing.Size(56, 16);
            this.lblSqlServerUsername.TabIndex = 7;
            this.lblSqlServerUsername.Text = "Username";
            // 
            // txtSqlServerPassword
            // 
            this.txtSqlServerPassword.Location = new System.Drawing.Point(264, 80);
            this.txtSqlServerPassword.Name = "txtSqlServerPassword";
            this.txtSqlServerPassword.PasswordChar = '*';
            this.txtSqlServerPassword.Size = new System.Drawing.Size(88, 20);
            this.txtSqlServerPassword.TabIndex = 10;
            this.txtSqlServerPassword.Text = "Password";
            // 
            // txtSqlServerUsername
            // 
            this.txtSqlServerUsername.Location = new System.Drawing.Point(184, 80);
            this.txtSqlServerUsername.Name = "txtSqlServerUsername";
            this.txtSqlServerUsername.Size = new System.Drawing.Size(72, 20);
            this.txtSqlServerUsername.TabIndex = 8;
            this.txtSqlServerUsername.Text = "user";
            // 
            // lblSqlServerDatabase
            // 
            this.lblSqlServerDatabase.Location = new System.Drawing.Point(264, 16);
            this.lblSqlServerDatabase.Name = "lblSqlServerDatabase";
            this.lblSqlServerDatabase.Size = new System.Drawing.Size(56, 16);
            this.lblSqlServerDatabase.TabIndex = 4;
            this.lblSqlServerDatabase.Text = "Database";
            // 
            // lblSqlServerServerName
            // 
            this.lblSqlServerServerName.Location = new System.Drawing.Point(184, 16);
            this.lblSqlServerServerName.Name = "lblSqlServerServerName";
            this.lblSqlServerServerName.Size = new System.Drawing.Size(56, 16);
            this.lblSqlServerServerName.TabIndex = 2;
            this.lblSqlServerServerName.Text = "Server";
            // 
            // chkSqlServerUseIntegratedSecurity
            // 
            this.chkSqlServerUseIntegratedSecurity.Checked = true;
            this.chkSqlServerUseIntegratedSecurity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSqlServerUseIntegratedSecurity.Location = new System.Drawing.Point(8, 72);
            this.chkSqlServerUseIntegratedSecurity.Name = "chkSqlServerUseIntegratedSecurity";
            this.chkSqlServerUseIntegratedSecurity.Size = new System.Drawing.Size(144, 16);
            this.chkSqlServerUseIntegratedSecurity.TabIndex = 6;
            this.chkSqlServerUseIntegratedSecurity.Text = "Use Integrated Security";
            this.chkSqlServerUseIntegratedSecurity.CheckedChanged += new System.EventHandler(this.chkSqlServerUseIntegratedSecurity_CheckedChanged);
            // 
            // txtSqlServerDatabase
            // 
            this.txtSqlServerDatabase.Location = new System.Drawing.Point(264, 32);
            this.txtSqlServerDatabase.Name = "txtSqlServerDatabase";
            this.txtSqlServerDatabase.Size = new System.Drawing.Size(88, 20);
            this.txtSqlServerDatabase.TabIndex = 5;
            this.txtSqlServerDatabase.Text = "TempDB";
            // 
            // txtSqlServerName
            // 
            this.txtSqlServerName.Location = new System.Drawing.Point(184, 32);
            this.txtSqlServerName.Name = "txtSqlServerName";
            this.txtSqlServerName.Size = new System.Drawing.Size(72, 20);
            this.txtSqlServerName.TabIndex = 3;
            this.txtSqlServerName.Text = "Monroe";
            // 
            // chkUseSqlServerDBToCacheData
            // 
            this.chkUseSqlServerDBToCacheData.Checked = true;
            this.chkUseSqlServerDBToCacheData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseSqlServerDBToCacheData.Location = new System.Drawing.Point(8, 56);
            this.chkUseSqlServerDBToCacheData.Name = "chkUseSqlServerDBToCacheData";
            this.chkUseSqlServerDBToCacheData.Size = new System.Drawing.Size(144, 16);
            this.chkUseSqlServerDBToCacheData.TabIndex = 1;
            this.chkUseSqlServerDBToCacheData.Text = "Enable data caching";
            this.chkUseSqlServerDBToCacheData.CheckedChanged += new System.EventHandler(this.chkUseSqlServerDBToCacheData_CheckedChanged);
            // 
            // fraUniquenessBinningOptions
            // 
            this.fraUniquenessBinningOptions.Controls.Add(this.lblPeptideUniquenessMassMode);
            this.fraUniquenessBinningOptions.Controls.Add(this.txtUniquenessBinWidth);
            this.fraUniquenessBinningOptions.Controls.Add(this.lblUniquenessBinWidth);
            this.fraUniquenessBinningOptions.Controls.Add(this.chkAutoComputeRangeForBinning);
            this.fraUniquenessBinningOptions.Controls.Add(this.txtUniquenessBinEndMass);
            this.fraUniquenessBinningOptions.Controls.Add(this.lblUniquenessBinEndMass);
            this.fraUniquenessBinningOptions.Controls.Add(this.txtUniquenessBinStartMass);
            this.fraUniquenessBinningOptions.Controls.Add(this.lblUniquenessBinStartMass);
            this.fraUniquenessBinningOptions.Location = new System.Drawing.Point(8, 120);
            this.fraUniquenessBinningOptions.Name = "fraUniquenessBinningOptions";
            this.fraUniquenessBinningOptions.Size = new System.Drawing.Size(208, 160);
            this.fraUniquenessBinningOptions.TabIndex = 3;
            this.fraUniquenessBinningOptions.TabStop = false;
            this.fraUniquenessBinningOptions.Text = "Binning Options";
            // 
            // lblPeptideUniquenessMassMode
            // 
            this.lblPeptideUniquenessMassMode.Location = new System.Drawing.Point(16, 136);
            this.lblPeptideUniquenessMassMode.Name = "lblPeptideUniquenessMassMode";
            this.lblPeptideUniquenessMassMode.Size = new System.Drawing.Size(176, 16);
            this.lblPeptideUniquenessMassMode.TabIndex = 7;
            this.lblPeptideUniquenessMassMode.Text = "Using monoisotopic masses";
            // 
            // txtUniquenessBinWidth
            // 
            this.txtUniquenessBinWidth.Location = new System.Drawing.Point(80, 24);
            this.txtUniquenessBinWidth.Name = "txtUniquenessBinWidth";
            this.txtUniquenessBinWidth.Size = new System.Drawing.Size(40, 20);
            this.txtUniquenessBinWidth.TabIndex = 1;
            this.txtUniquenessBinWidth.Text = "25";
            this.txtUniquenessBinWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUniquenessBinWidth_KeyPress);
            // 
            // lblUniquenessBinWidth
            // 
            this.lblUniquenessBinWidth.Location = new System.Drawing.Point(16, 26);
            this.lblUniquenessBinWidth.Name = "lblUniquenessBinWidth";
            this.lblUniquenessBinWidth.Size = new System.Drawing.Size(64, 16);
            this.lblUniquenessBinWidth.TabIndex = 0;
            this.lblUniquenessBinWidth.Text = "Bin Width";
            // 
            // chkAutoComputeRangeForBinning
            // 
            this.chkAutoComputeRangeForBinning.Checked = true;
            this.chkAutoComputeRangeForBinning.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoComputeRangeForBinning.Location = new System.Drawing.Point(16, 56);
            this.chkAutoComputeRangeForBinning.Name = "chkAutoComputeRangeForBinning";
            this.chkAutoComputeRangeForBinning.Size = new System.Drawing.Size(184, 17);
            this.chkAutoComputeRangeForBinning.TabIndex = 2;
            this.chkAutoComputeRangeForBinning.Text = "Auto compute range for binning";
            this.chkAutoComputeRangeForBinning.CheckedChanged += new System.EventHandler(this.chkAutoComputeRangeForBinning_CheckedChanged);
            // 
            // txtUniquenessBinEndMass
            // 
            this.txtUniquenessBinEndMass.Location = new System.Drawing.Point(80, 104);
            this.txtUniquenessBinEndMass.Name = "txtUniquenessBinEndMass";
            this.txtUniquenessBinEndMass.Size = new System.Drawing.Size(40, 20);
            this.txtUniquenessBinEndMass.TabIndex = 6;
            this.txtUniquenessBinEndMass.Text = "6000";
            this.txtUniquenessBinEndMass.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUniquenessBinEndMass_KeyPress);
            // 
            // lblUniquenessBinEndMass
            // 
            this.lblUniquenessBinEndMass.Location = new System.Drawing.Point(16, 106);
            this.lblUniquenessBinEndMass.Name = "lblUniquenessBinEndMass";
            this.lblUniquenessBinEndMass.Size = new System.Drawing.Size(64, 16);
            this.lblUniquenessBinEndMass.TabIndex = 5;
            this.lblUniquenessBinEndMass.Text = "End Mass";
            // 
            // txtUniquenessBinStartMass
            // 
            this.txtUniquenessBinStartMass.Location = new System.Drawing.Point(80, 80);
            this.txtUniquenessBinStartMass.Name = "txtUniquenessBinStartMass";
            this.txtUniquenessBinStartMass.Size = new System.Drawing.Size(40, 20);
            this.txtUniquenessBinStartMass.TabIndex = 4;
            this.txtUniquenessBinStartMass.Text = "400";
            this.txtUniquenessBinStartMass.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtUniquenessBinStartMass_KeyPress);
            // 
            // lblUniquenessBinStartMass
            // 
            this.lblUniquenessBinStartMass.Location = new System.Drawing.Point(16, 82);
            this.lblUniquenessBinStartMass.Name = "lblUniquenessBinStartMass";
            this.lblUniquenessBinStartMass.Size = new System.Drawing.Size(64, 16);
            this.lblUniquenessBinStartMass.TabIndex = 3;
            this.lblUniquenessBinStartMass.Text = "Start Mass";
            // 
            // lblUniquenessStatsNote
            // 
            this.lblUniquenessStatsNote.Location = new System.Drawing.Point(8, 56);
            this.lblUniquenessStatsNote.Name = "lblUniquenessStatsNote";
            this.lblUniquenessStatsNote.Size = new System.Drawing.Size(200, 48);
            this.lblUniquenessStatsNote.TabIndex = 1;
            this.lblUniquenessStatsNote.Text = "Note that Digestion Options and Mass Calculation Options also apply for uniquenes" +
    "s stats generation.";
            // 
            // cmdGenerateUniquenessStats
            // 
            this.cmdGenerateUniquenessStats.Location = new System.Drawing.Point(232, 16);
            this.cmdGenerateUniquenessStats.Name = "cmdGenerateUniquenessStats";
            this.cmdGenerateUniquenessStats.Size = new System.Drawing.Size(176, 24);
            this.cmdGenerateUniquenessStats.TabIndex = 5;
            this.cmdGenerateUniquenessStats.Text = "&Generate Uniqueness Stats";
            this.cmdGenerateUniquenessStats.Click += new System.EventHandler(this.cmdGenerateUniquenessStats_Click);
            // 
            // chkAssumeInputFileIsDigested
            // 
            this.chkAssumeInputFileIsDigested.Location = new System.Drawing.Point(8, 16);
            this.chkAssumeInputFileIsDigested.Name = "chkAssumeInputFileIsDigested";
            this.chkAssumeInputFileIsDigested.Size = new System.Drawing.Size(192, 32);
            this.chkAssumeInputFileIsDigested.TabIndex = 0;
            this.chkAssumeInputFileIsDigested.Text = "Assume input file is already digested (for Delimited files only)";
            // 
            // txtProteinScramblingLoopCount
            // 
            this.txtProteinScramblingLoopCount.Location = new System.Drawing.Point(312, 40);
            this.txtProteinScramblingLoopCount.MaxLength = 3;
            this.txtProteinScramblingLoopCount.Name = "txtProteinScramblingLoopCount";
            this.txtProteinScramblingLoopCount.Size = new System.Drawing.Size(32, 20);
            this.txtProteinScramblingLoopCount.TabIndex = 13;
            this.txtProteinScramblingLoopCount.Text = "1";
            this.txtProteinScramblingLoopCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtProteinScramblingLoopCount_KeyPress);
            // 
            // lblSamplingPercentageUnits
            // 
            this.lblSamplingPercentageUnits.Location = new System.Drawing.Point(208, 42);
            this.lblSamplingPercentageUnits.Name = "lblSamplingPercentageUnits";
            this.lblSamplingPercentageUnits.Size = new System.Drawing.Size(16, 16);
            this.lblSamplingPercentageUnits.TabIndex = 4;
            this.lblSamplingPercentageUnits.Text = "%";
            // 
            // txtMaxpISequenceLength
            // 
            this.txtMaxpISequenceLength.Location = new System.Drawing.Point(168, 70);
            this.txtMaxpISequenceLength.Name = "txtMaxpISequenceLength";
            this.txtMaxpISequenceLength.Size = new System.Drawing.Size(40, 20);
            this.txtMaxpISequenceLength.TabIndex = 4;
            this.txtMaxpISequenceLength.Text = "10";
            this.txtMaxpISequenceLength.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMaxpISequenceLength_KeyDown);
            this.txtMaxpISequenceLength.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMaxpISequenceLength_KeyPress);
            this.txtMaxpISequenceLength.Validating += new System.ComponentModel.CancelEventHandler(this.txtMaxpISequenceLength_Validating);
            this.txtMaxpISequenceLength.Validated += new System.EventHandler(this.txtMaxpISequenceLength_Validated);
            // 
            // lblProteinReversalSamplingPercentage
            // 
            this.lblProteinReversalSamplingPercentage.Location = new System.Drawing.Point(48, 42);
            this.lblProteinReversalSamplingPercentage.Name = "lblProteinReversalSamplingPercentage";
            this.lblProteinReversalSamplingPercentage.Size = new System.Drawing.Size(112, 16);
            this.lblProteinReversalSamplingPercentage.TabIndex = 2;
            this.lblProteinReversalSamplingPercentage.Text = "Sampling Percentage";
            this.lblProteinReversalSamplingPercentage.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblMaxpISequenceLength
            // 
            this.lblMaxpISequenceLength.Location = new System.Drawing.Point(32, 72);
            this.lblMaxpISequenceLength.Name = "lblMaxpISequenceLength";
            this.lblMaxpISequenceLength.Size = new System.Drawing.Size(144, 16);
            this.lblMaxpISequenceLength.TabIndex = 3;
            this.lblMaxpISequenceLength.Text = "Sub-sequence Length";
            // 
            // chkMaxpIModeEnabled
            // 
            this.chkMaxpIModeEnabled.Location = new System.Drawing.Point(8, 48);
            this.chkMaxpIModeEnabled.Name = "chkMaxpIModeEnabled";
            this.chkMaxpIModeEnabled.Size = new System.Drawing.Size(224, 16);
            this.chkMaxpIModeEnabled.TabIndex = 2;
            this.chkMaxpIModeEnabled.Text = "Report maximum of all sub-sequences";
            this.chkMaxpIModeEnabled.CheckedChanged += new System.EventHandler(this.chkMaxpIModeEnabled_CheckedChanged);
            // 
            // frapIAndHydrophobicity
            // 
            this.frapIAndHydrophobicity.Controls.Add(this.txtMaxpISequenceLength);
            this.frapIAndHydrophobicity.Controls.Add(this.lblMaxpISequenceLength);
            this.frapIAndHydrophobicity.Controls.Add(this.chkMaxpIModeEnabled);
            this.frapIAndHydrophobicity.Controls.Add(this.lblHydrophobicityMode);
            this.frapIAndHydrophobicity.Controls.Add(this.cboHydrophobicityMode);
            this.frapIAndHydrophobicity.Controls.Add(this.txtpIStats);
            this.frapIAndHydrophobicity.Controls.Add(this.txtSequenceForpI);
            this.frapIAndHydrophobicity.Controls.Add(this.lblSequenceForpI);
            this.frapIAndHydrophobicity.Location = new System.Drawing.Point(8, 108);
            this.frapIAndHydrophobicity.Name = "frapIAndHydrophobicity";
            this.frapIAndHydrophobicity.Size = new System.Drawing.Size(616, 136);
            this.frapIAndHydrophobicity.TabIndex = 2;
            this.frapIAndHydrophobicity.TabStop = false;
            this.frapIAndHydrophobicity.Text = "pI And Hydrophobicity";
            // 
            // lblHydrophobicityMode
            // 
            this.lblHydrophobicityMode.Location = new System.Drawing.Point(8, 24);
            this.lblHydrophobicityMode.Name = "lblHydrophobicityMode";
            this.lblHydrophobicityMode.Size = new System.Drawing.Size(120, 16);
            this.lblHydrophobicityMode.TabIndex = 0;
            this.lblHydrophobicityMode.Text = "Hydrophobicity Mode";
            // 
            // cboHydrophobicityMode
            // 
            this.cboHydrophobicityMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboHydrophobicityMode.DropDownWidth = 70;
            this.cboHydrophobicityMode.Location = new System.Drawing.Point(128, 18);
            this.cboHydrophobicityMode.Name = "cboHydrophobicityMode";
            this.cboHydrophobicityMode.Size = new System.Drawing.Size(184, 21);
            this.cboHydrophobicityMode.TabIndex = 1;
            this.cboHydrophobicityMode.SelectedIndexChanged += new System.EventHandler(this.cboHydrophobicityMode_SelectedIndexChanged);
            // 
            // txtpIStats
            // 
            this.txtpIStats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtpIStats.Location = new System.Drawing.Point(336, 48);
            this.txtpIStats.MaxLength = 1;
            this.txtpIStats.Multiline = true;
            this.txtpIStats.Name = "txtpIStats";
            this.txtpIStats.ReadOnly = true;
            this.txtpIStats.Size = new System.Drawing.Size(272, 80);
            this.txtpIStats.TabIndex = 7;
            // 
            // txtSequenceForpI
            // 
            this.txtSequenceForpI.Location = new System.Drawing.Point(400, 16);
            this.txtSequenceForpI.Name = "txtSequenceForpI";
            this.txtSequenceForpI.Size = new System.Drawing.Size(208, 20);
            this.txtSequenceForpI.TabIndex = 6;
            this.txtSequenceForpI.Text = "FKDLGEEQFK";
            this.txtSequenceForpI.TextChanged += new System.EventHandler(this.txtSequenceForpI_TextChanged);
            // 
            // lblSequenceForpI
            // 
            this.lblSequenceForpI.Location = new System.Drawing.Point(328, 20);
            this.lblSequenceForpI.Name = "lblSequenceForpI";
            this.lblSequenceForpI.Size = new System.Drawing.Size(72, 16);
            this.lblSequenceForpI.TabIndex = 5;
            this.lblSequenceForpI.Text = "Sequence";
            // 
            // fraDelimitedFileOptions
            // 
            this.fraDelimitedFileOptions.Controls.Add(this.cboInputFileColumnOrdering);
            this.fraDelimitedFileOptions.Controls.Add(this.lblInputFileColumnOrdering);
            this.fraDelimitedFileOptions.Controls.Add(this.txtInputFileColumnDelimiter);
            this.fraDelimitedFileOptions.Controls.Add(this.lblInputFileColumnDelimiter);
            this.fraDelimitedFileOptions.Controls.Add(this.cboInputFileColumnDelimiter);
            this.fraDelimitedFileOptions.Location = new System.Drawing.Point(8, 12);
            this.fraDelimitedFileOptions.Name = "fraDelimitedFileOptions";
            this.fraDelimitedFileOptions.Size = new System.Drawing.Size(496, 88);
            this.fraDelimitedFileOptions.TabIndex = 1;
            this.fraDelimitedFileOptions.TabStop = false;
            this.fraDelimitedFileOptions.Text = "Delimited Input File Options";
            // 
            // cboInputFileColumnOrdering
            // 
            this.cboInputFileColumnOrdering.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInputFileColumnOrdering.DropDownWidth = 70;
            this.cboInputFileColumnOrdering.Location = new System.Drawing.Point(88, 24);
            this.cboInputFileColumnOrdering.Name = "cboInputFileColumnOrdering";
            this.cboInputFileColumnOrdering.Size = new System.Drawing.Size(392, 21);
            this.cboInputFileColumnOrdering.TabIndex = 1;
            // 
            // lblInputFileColumnOrdering
            // 
            this.lblInputFileColumnOrdering.Location = new System.Drawing.Point(8, 26);
            this.lblInputFileColumnOrdering.Name = "lblInputFileColumnOrdering";
            this.lblInputFileColumnOrdering.Size = new System.Drawing.Size(80, 16);
            this.lblInputFileColumnOrdering.TabIndex = 0;
            this.lblInputFileColumnOrdering.Text = "Column Order";
            // 
            // txtInputFileColumnDelimiter
            // 
            this.txtInputFileColumnDelimiter.Location = new System.Drawing.Point(192, 56);
            this.txtInputFileColumnDelimiter.MaxLength = 1;
            this.txtInputFileColumnDelimiter.Name = "txtInputFileColumnDelimiter";
            this.txtInputFileColumnDelimiter.Size = new System.Drawing.Size(32, 20);
            this.txtInputFileColumnDelimiter.TabIndex = 4;
            this.txtInputFileColumnDelimiter.Text = ";";
            // 
            // lblInputFileColumnDelimiter
            // 
            this.lblInputFileColumnDelimiter.Location = new System.Drawing.Point(8, 58);
            this.lblInputFileColumnDelimiter.Name = "lblInputFileColumnDelimiter";
            this.lblInputFileColumnDelimiter.Size = new System.Drawing.Size(96, 16);
            this.lblInputFileColumnDelimiter.TabIndex = 2;
            this.lblInputFileColumnDelimiter.Text = "Column Delimiter";
            // 
            // cboInputFileColumnDelimiter
            // 
            this.cboInputFileColumnDelimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInputFileColumnDelimiter.DropDownWidth = 70;
            this.cboInputFileColumnDelimiter.Location = new System.Drawing.Point(112, 56);
            this.cboInputFileColumnDelimiter.Name = "cboInputFileColumnDelimiter";
            this.cboInputFileColumnDelimiter.Size = new System.Drawing.Size(70, 21);
            this.cboInputFileColumnDelimiter.TabIndex = 3;
            this.cboInputFileColumnDelimiter.SelectedIndexChanged += new System.EventHandler(this.cboInputFileColumnDelimiter_SelectedIndexChanged);
            // 
            // TabPageFileFormatOptions
            // 
            this.TabPageFileFormatOptions.Controls.Add(this.frapIAndHydrophobicity);
            this.TabPageFileFormatOptions.Controls.Add(this.fraDelimitedFileOptions);
            this.TabPageFileFormatOptions.Location = new System.Drawing.Point(4, 22);
            this.TabPageFileFormatOptions.Name = "TabPageFileFormatOptions";
            this.TabPageFileFormatOptions.Size = new System.Drawing.Size(696, 328);
            this.TabPageFileFormatOptions.TabIndex = 2;
            this.TabPageFileFormatOptions.Text = "File Format Options";
            this.TabPageFileFormatOptions.UseVisualStyleBackColor = true;
            // 
            // tbsOptions
            // 
            this.tbsOptions.Controls.Add(this.TabPageFileFormatOptions);
            this.tbsOptions.Controls.Add(this.TabPageParseAndDigest);
            this.tbsOptions.Controls.Add(this.TabPageUniquenessStats);
            this.tbsOptions.Controls.Add(this.TabPagePeakMatchingThresholds);
            this.tbsOptions.Controls.Add(this.TabPageProgress);
            this.tbsOptions.Location = new System.Drawing.Point(12, 212);
            this.tbsOptions.Name = "tbsOptions";
            this.tbsOptions.SelectedIndex = 0;
            this.tbsOptions.Size = new System.Drawing.Size(704, 354);
            this.tbsOptions.TabIndex = 5;
            // 
            // TabPageParseAndDigest
            // 
            this.TabPageParseAndDigest.Controls.Add(this.fraProcessingOptions);
            this.TabPageParseAndDigest.Controls.Add(this.fraCalculationOptions);
            this.TabPageParseAndDigest.Controls.Add(this.fraDigestionOptions);
            this.TabPageParseAndDigest.Controls.Add(this.cmdParseInputFile);
            this.TabPageParseAndDigest.Location = new System.Drawing.Point(4, 22);
            this.TabPageParseAndDigest.Name = "TabPageParseAndDigest";
            this.TabPageParseAndDigest.Size = new System.Drawing.Size(696, 328);
            this.TabPageParseAndDigest.TabIndex = 0;
            this.TabPageParseAndDigest.Text = "Parse and Digest File Options";
            this.TabPageParseAndDigest.UseVisualStyleBackColor = true;
            // 
            // fraProcessingOptions
            // 
            this.fraProcessingOptions.Controls.Add(this.lblProteinScramblingLoopCount);
            this.fraProcessingOptions.Controls.Add(this.txtProteinScramblingLoopCount);
            this.fraProcessingOptions.Controls.Add(this.lblSamplingPercentageUnits);
            this.fraProcessingOptions.Controls.Add(this.lblProteinReversalSamplingPercentage);
            this.fraProcessingOptions.Controls.Add(this.txtProteinReversalSamplingPercentage);
            this.fraProcessingOptions.Controls.Add(this.lbltxtAddnlRefAccessionSepChar);
            this.fraProcessingOptions.Controls.Add(this.chkLookForAddnlRefInDescription);
            this.fraProcessingOptions.Controls.Add(this.cboProteinReversalOptions);
            this.fraProcessingOptions.Controls.Add(this.lblProteinReversalOptions);
            this.fraProcessingOptions.Controls.Add(this.chkDigestProteins);
            this.fraProcessingOptions.Controls.Add(this.lblAddnlRefSepChar);
            this.fraProcessingOptions.Controls.Add(this.txtAddnlRefAccessionSepChar);
            this.fraProcessingOptions.Controls.Add(this.txtAddnlRefSepChar);
            this.fraProcessingOptions.Controls.Add(this.chkCreateFastaOutputFile);
            this.fraProcessingOptions.Location = new System.Drawing.Point(8, 8);
            this.fraProcessingOptions.Name = "fraProcessingOptions";
            this.fraProcessingOptions.Size = new System.Drawing.Size(360, 176);
            this.fraProcessingOptions.TabIndex = 0;
            this.fraProcessingOptions.TabStop = false;
            this.fraProcessingOptions.Text = "Processing Options";
            // 
            // txtProteinReversalSamplingPercentage
            // 
            this.txtProteinReversalSamplingPercentage.Location = new System.Drawing.Point(168, 40);
            this.txtProteinReversalSamplingPercentage.MaxLength = 3;
            this.txtProteinReversalSamplingPercentage.Name = "txtProteinReversalSamplingPercentage";
            this.txtProteinReversalSamplingPercentage.Size = new System.Drawing.Size(32, 20);
            this.txtProteinReversalSamplingPercentage.TabIndex = 3;
            this.txtProteinReversalSamplingPercentage.Text = "100";
            this.txtProteinReversalSamplingPercentage.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtProteinReversalSamplingPercentage_KeyPress);
            // 
            // lbltxtAddnlRefAccessionSepChar
            // 
            this.lbltxtAddnlRefAccessionSepChar.Location = new System.Drawing.Point(96, 96);
            this.lbltxtAddnlRefAccessionSepChar.Name = "lbltxtAddnlRefAccessionSepChar";
            this.lbltxtAddnlRefAccessionSepChar.Size = new System.Drawing.Size(160, 16);
            this.lbltxtAddnlRefAccessionSepChar.TabIndex = 8;
            this.lbltxtAddnlRefAccessionSepChar.Text = "Addnl Ref Accession Sep Char";
            this.lbltxtAddnlRefAccessionSepChar.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // chkLookForAddnlRefInDescription
            // 
            this.chkLookForAddnlRefInDescription.Location = new System.Drawing.Point(16, 72);
            this.chkLookForAddnlRefInDescription.Name = "chkLookForAddnlRefInDescription";
            this.chkLookForAddnlRefInDescription.Size = new System.Drawing.Size(120, 32);
            this.chkLookForAddnlRefInDescription.TabIndex = 5;
            this.chkLookForAddnlRefInDescription.Text = "Look for addnl Ref in description";
            this.chkLookForAddnlRefInDescription.CheckedChanged += new System.EventHandler(this.chkLookForAddnlRefInDescription_CheckedChanged);
            // 
            // cboProteinReversalOptions
            // 
            this.cboProteinReversalOptions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboProteinReversalOptions.Location = new System.Drawing.Point(168, 16);
            this.cboProteinReversalOptions.Name = "cboProteinReversalOptions";
            this.cboProteinReversalOptions.Size = new System.Drawing.Size(184, 21);
            this.cboProteinReversalOptions.TabIndex = 1;
            this.cboProteinReversalOptions.SelectedIndexChanged += new System.EventHandler(this.cboProteinReversalOptions_SelectedIndexChanged);
            // 
            // lblProteinReversalOptions
            // 
            this.lblProteinReversalOptions.Location = new System.Drawing.Point(3, 20);
            this.lblProteinReversalOptions.Name = "lblProteinReversalOptions";
            this.lblProteinReversalOptions.Size = new System.Drawing.Size(170, 16);
            this.lblProteinReversalOptions.TabIndex = 0;
            this.lblProteinReversalOptions.Text = "Protein Reversal / Scrambling";
            // 
            // chkDigestProteins
            // 
            this.chkDigestProteins.Location = new System.Drawing.Point(16, 115);
            this.chkDigestProteins.Name = "chkDigestProteins";
            this.chkDigestProteins.Size = new System.Drawing.Size(160, 32);
            this.chkDigestProteins.TabIndex = 10;
            this.chkDigestProteins.Text = "In Silico digest of all proteins in input file";
            this.chkDigestProteins.CheckedChanged += new System.EventHandler(this.chkDigestProteins_CheckedChanged);
            // 
            // lblAddnlRefSepChar
            // 
            this.lblAddnlRefSepChar.Location = new System.Drawing.Point(144, 72);
            this.lblAddnlRefSepChar.Name = "lblAddnlRefSepChar";
            this.lblAddnlRefSepChar.Size = new System.Drawing.Size(112, 16);
            this.lblAddnlRefSepChar.TabIndex = 6;
            this.lblAddnlRefSepChar.Text = "Addnl Ref Sep Char";
            this.lblAddnlRefSepChar.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // txtAddnlRefAccessionSepChar
            // 
            this.txtAddnlRefAccessionSepChar.Location = new System.Drawing.Point(264, 96);
            this.txtAddnlRefAccessionSepChar.MaxLength = 1;
            this.txtAddnlRefAccessionSepChar.Name = "txtAddnlRefAccessionSepChar";
            this.txtAddnlRefAccessionSepChar.Size = new System.Drawing.Size(32, 20);
            this.txtAddnlRefAccessionSepChar.TabIndex = 9;
            this.txtAddnlRefAccessionSepChar.Text = ":";
            // 
            // txtAddnlRefSepChar
            // 
            this.txtAddnlRefSepChar.Location = new System.Drawing.Point(264, 72);
            this.txtAddnlRefSepChar.MaxLength = 1;
            this.txtAddnlRefSepChar.Name = "txtAddnlRefSepChar";
            this.txtAddnlRefSepChar.Size = new System.Drawing.Size(32, 20);
            this.txtAddnlRefSepChar.TabIndex = 7;
            this.txtAddnlRefSepChar.Text = "|";
            // 
            // chkCreateFastaOutputFile
            // 
            this.chkCreateFastaOutputFile.Location = new System.Drawing.Point(192, 128);
            this.chkCreateFastaOutputFile.Name = "chkCreateFastaOutputFile";
            this.chkCreateFastaOutputFile.Size = new System.Drawing.Size(160, 16);
            this.chkCreateFastaOutputFile.TabIndex = 11;
            this.chkCreateFastaOutputFile.Text = "Create FASTA Output File";
            this.chkCreateFastaOutputFile.CheckedChanged += new System.EventHandler(this.chkCreateFastaOutputFile_CheckedChanged);
            // 
            // fraCalculationOptions
            // 
            this.fraCalculationOptions.Controls.Add(this.cmdNETInfo);
            this.fraCalculationOptions.Controls.Add(this.chkExcludeProteinDescription);
            this.fraCalculationOptions.Controls.Add(this.chkComputeSequenceHashIgnoreILDiff);
            this.fraCalculationOptions.Controls.Add(this.chkTruncateProteinDescription);
            this.fraCalculationOptions.Controls.Add(this.chkComputeSequenceHashValues);
            this.fraCalculationOptions.Controls.Add(this.lblMassMode);
            this.fraCalculationOptions.Controls.Add(this.cboElementMassMode);
            this.fraCalculationOptions.Controls.Add(this.chkExcludeProteinSequence);
            this.fraCalculationOptions.Controls.Add(this.chkComputepIandNET);
            this.fraCalculationOptions.Controls.Add(this.chkIncludeXResidues);
            this.fraCalculationOptions.Controls.Add(this.chkComputeProteinMass);
            this.fraCalculationOptions.Location = new System.Drawing.Point(376, 40);
            this.fraCalculationOptions.Name = "fraCalculationOptions";
            this.fraCalculationOptions.Size = new System.Drawing.Size(308, 150);
            this.fraCalculationOptions.TabIndex = 1;
            this.fraCalculationOptions.TabStop = false;
            this.fraCalculationOptions.Text = "Calculation Options";
            // 
            // cmdNETInfo
            // 
            this.cmdNETInfo.Location = new System.Drawing.Point(268, 87);
            this.cmdNETInfo.Margin = new System.Windows.Forms.Padding(1);
            this.cmdNETInfo.Name = "cmdNETInfo";
            this.cmdNETInfo.Size = new System.Drawing.Size(34, 20);
            this.cmdNETInfo.TabIndex = 4;
            this.cmdNETInfo.Text = "Info";
            this.cmdNETInfo.Click += new System.EventHandler(this.cmdNETInfo_Click);
            // 
            // chkExcludeProteinDescription
            // 
            this.chkExcludeProteinDescription.Location = new System.Drawing.Point(185, 128);
            this.chkExcludeProteinDescription.Name = "chkExcludeProteinDescription";
            this.chkExcludeProteinDescription.Size = new System.Drawing.Size(120, 19);
            this.chkExcludeProteinDescription.TabIndex = 9;
            this.chkExcludeProteinDescription.Text = "Exclude Description";
            // 
            // chkComputeSequenceHashIgnoreILDiff
            // 
            this.chkComputeSequenceHashIgnoreILDiff.Checked = true;
            this.chkComputeSequenceHashIgnoreILDiff.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkComputeSequenceHashIgnoreILDiff.Location = new System.Drawing.Point(185, 107);
            this.chkComputeSequenceHashIgnoreILDiff.Name = "chkComputeSequenceHashIgnoreILDiff";
            this.chkComputeSequenceHashIgnoreILDiff.Size = new System.Drawing.Size(104, 19);
            this.chkComputeSequenceHashIgnoreILDiff.TabIndex = 8;
            this.chkComputeSequenceHashIgnoreILDiff.Text = "Ignore I/L Diff";
            // 
            // chkTruncateProteinDescription
            // 
            this.chkTruncateProteinDescription.Checked = true;
            this.chkTruncateProteinDescription.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTruncateProteinDescription.Location = new System.Drawing.Point(16, 128);
            this.chkTruncateProteinDescription.Name = "chkTruncateProteinDescription";
            this.chkTruncateProteinDescription.Size = new System.Drawing.Size(164, 19);
            this.chkTruncateProteinDescription.TabIndex = 7;
            this.chkTruncateProteinDescription.Text = "Truncate long description";
            // 
            // chkComputeSequenceHashValues
            // 
            this.chkComputeSequenceHashValues.Checked = true;
            this.chkComputeSequenceHashValues.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkComputeSequenceHashValues.Location = new System.Drawing.Point(16, 107);
            this.chkComputeSequenceHashValues.Name = "chkComputeSequenceHashValues";
            this.chkComputeSequenceHashValues.Size = new System.Drawing.Size(164, 19);
            this.chkComputeSequenceHashValues.TabIndex = 6;
            this.chkComputeSequenceHashValues.Text = "Compute sequence hashes";
            this.chkComputeSequenceHashValues.CheckedChanged += new System.EventHandler(this.chkComputeSequenceHashValues_CheckedChanged);
            // 
            // lblMassMode
            // 
            this.lblMassMode.Location = new System.Drawing.Point(16, 66);
            this.lblMassMode.Name = "lblMassMode";
            this.lblMassMode.Size = new System.Drawing.Size(64, 16);
            this.lblMassMode.TabIndex = 5;
            this.lblMassMode.Text = "Mass type";
            this.lblMassMode.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // cboElementMassMode
            // 
            this.cboElementMassMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboElementMassMode.Location = new System.Drawing.Point(88, 65);
            this.cboElementMassMode.Name = "cboElementMassMode";
            this.cboElementMassMode.Size = new System.Drawing.Size(144, 21);
            this.cboElementMassMode.TabIndex = 4;
            this.cboElementMassMode.SelectedIndexChanged += new System.EventHandler(this.cboElementMassMode_SelectedIndexChanged);
            // 
            // chkExcludeProteinSequence
            // 
            this.chkExcludeProteinSequence.Location = new System.Drawing.Point(16, 16);
            this.chkExcludeProteinSequence.Name = "chkExcludeProteinSequence";
            this.chkExcludeProteinSequence.Size = new System.Drawing.Size(192, 16);
            this.chkExcludeProteinSequence.TabIndex = 0;
            this.chkExcludeProteinSequence.Text = "Exclude Protein Sequence";
            // 
            // chkComputepIandNET
            // 
            this.chkComputepIandNET.Location = new System.Drawing.Point(16, 89);
            this.chkComputepIandNET.Name = "chkComputepIandNET";
            this.chkComputepIandNET.Size = new System.Drawing.Size(252, 18);
            this.chkComputepIandNET.TabIndex = 3;
            this.chkComputepIandNET.Text = "Compute pI and Normalized Elution Time (NET)";
            this.chkComputepIandNET.CheckedChanged += new System.EventHandler(this.chkComputepI_CheckedChanged);
            // 
            // chkIncludeXResidues
            // 
            this.chkIncludeXResidues.Location = new System.Drawing.Point(16, 49);
            this.chkIncludeXResidues.Name = "chkIncludeXResidues";
            this.chkIncludeXResidues.Size = new System.Drawing.Size(216, 16);
            this.chkIncludeXResidues.TabIndex = 2;
            this.chkIncludeXResidues.Text = "Include X Residues in Mass (113 Da)";
            // 
            // chkComputeProteinMass
            // 
            this.chkComputeProteinMass.Location = new System.Drawing.Point(16, 33);
            this.chkComputeProteinMass.Name = "chkComputeProteinMass";
            this.chkComputeProteinMass.Size = new System.Drawing.Size(144, 16);
            this.chkComputeProteinMass.TabIndex = 1;
            this.chkComputeProteinMass.Text = "Compute Protein Mass";
            // 
            // fraDigestionOptions
            // 
            this.fraDigestionOptions.Controls.Add(this.cboFragmentMassMode);
            this.fraDigestionOptions.Controls.Add(this.lblFragmentMassMode);
            this.fraDigestionOptions.Controls.Add(this.cboCysTreatmentMode);
            this.fraDigestionOptions.Controls.Add(this.lblCysTreatment);
            this.fraDigestionOptions.Controls.Add(this.txtDigestProteinsMaximumpI);
            this.fraDigestionOptions.Controls.Add(this.lblDigestProteinsMaximumpI);
            this.fraDigestionOptions.Controls.Add(this.txtDigestProteinsMinimumpI);
            this.fraDigestionOptions.Controls.Add(this.lblDigestProteinsMinimumpI);
            this.fraDigestionOptions.Controls.Add(this.chkGenerateUniqueIDValues);
            this.fraDigestionOptions.Controls.Add(this.chkCysPeptidesOnly);
            this.fraDigestionOptions.Controls.Add(this.txtDigestProteinsMinimumResidueCount);
            this.fraDigestionOptions.Controls.Add(this.lblDigestProteinsMinimumResidueCount);
            this.fraDigestionOptions.Controls.Add(this.txtDigestProteinsMaximumMissedCleavages);
            this.fraDigestionOptions.Controls.Add(this.lblDigestProteinsMaximumMissedCleavages);
            this.fraDigestionOptions.Controls.Add(this.txtDigestProteinsMaximumMass);
            this.fraDigestionOptions.Controls.Add(this.lblDigestProteinsMaximumMass);
            this.fraDigestionOptions.Controls.Add(this.txtDigestProteinsMinimumMass);
            this.fraDigestionOptions.Controls.Add(this.lblDigestProteinsMinimumMass);
            this.fraDigestionOptions.Controls.Add(this.cboCleavageRuleType);
            this.fraDigestionOptions.Controls.Add(this.chkIncludeDuplicateSequences);
            this.fraDigestionOptions.Location = new System.Drawing.Point(8, 190);
            this.fraDigestionOptions.Name = "fraDigestionOptions";
            this.fraDigestionOptions.Size = new System.Drawing.Size(675, 128);
            this.fraDigestionOptions.TabIndex = 2;
            this.fraDigestionOptions.TabStop = false;
            this.fraDigestionOptions.Text = "Digestion Options";
            // 
            // cboFragmentMassMode
            // 
            this.cboFragmentMassMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFragmentMassMode.Location = new System.Drawing.Point(75, 104);
            this.cboFragmentMassMode.Name = "cboFragmentMassMode";
            this.cboFragmentMassMode.Size = new System.Drawing.Size(117, 21);
            this.cboFragmentMassMode.TabIndex = 19;
            // 
            // lblFragmentMassMode
            // 
            this.lblFragmentMassMode.Location = new System.Drawing.Point(8, 106);
            this.lblFragmentMassMode.Name = "lblFragmentMassMode";
            this.lblFragmentMassMode.Size = new System.Drawing.Size(68, 16);
            this.lblFragmentMassMode.TabIndex = 18;
            this.lblFragmentMassMode.Text = "Mass Mode";
            // 
            // cboCysTreatmentMode
            // 
            this.cboCysTreatmentMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCysTreatmentMode.Location = new System.Drawing.Point(553, 76);
            this.cboCysTreatmentMode.Name = "cboCysTreatmentMode";
            this.cboCysTreatmentMode.Size = new System.Drawing.Size(117, 21);
            this.cboCysTreatmentMode.TabIndex = 17;
            // 
            // lblCysTreatment
            // 
            this.lblCysTreatment.Location = new System.Drawing.Point(553, 56);
            this.lblCysTreatment.Name = "lblCysTreatment";
            this.lblCysTreatment.Size = new System.Drawing.Size(104, 21);
            this.lblCysTreatment.TabIndex = 16;
            this.lblCysTreatment.Text = "Cys treatment:";
            // 
            // txtDigestProteinsMaximumpI
            // 
            this.txtDigestProteinsMaximumpI.Location = new System.Drawing.Point(497, 80);
            this.txtDigestProteinsMaximumpI.Name = "txtDigestProteinsMaximumpI";
            this.txtDigestProteinsMaximumpI.Size = new System.Drawing.Size(40, 20);
            this.txtDigestProteinsMaximumpI.TabIndex = 13;
            this.txtDigestProteinsMaximumpI.Text = "14";
            // 
            // lblDigestProteinsMaximumpI
            // 
            this.lblDigestProteinsMaximumpI.Location = new System.Drawing.Point(420, 80);
            this.lblDigestProteinsMaximumpI.Name = "lblDigestProteinsMaximumpI";
            this.lblDigestProteinsMaximumpI.Size = new System.Drawing.Size(72, 16);
            this.lblDigestProteinsMaximumpI.TabIndex = 12;
            this.lblDigestProteinsMaximumpI.Text = "Maximum pI";
            // 
            // txtDigestProteinsMinimumpI
            // 
            this.txtDigestProteinsMinimumpI.Location = new System.Drawing.Point(497, 56);
            this.txtDigestProteinsMinimumpI.Name = "txtDigestProteinsMinimumpI";
            this.txtDigestProteinsMinimumpI.Size = new System.Drawing.Size(40, 20);
            this.txtDigestProteinsMinimumpI.TabIndex = 11;
            this.txtDigestProteinsMinimumpI.Text = "0";
            // 
            // lblDigestProteinsMinimumpI
            // 
            this.lblDigestProteinsMinimumpI.Location = new System.Drawing.Point(420, 56);
            this.lblDigestProteinsMinimumpI.Name = "lblDigestProteinsMinimumpI";
            this.lblDigestProteinsMinimumpI.Size = new System.Drawing.Size(72, 16);
            this.lblDigestProteinsMinimumpI.TabIndex = 10;
            this.lblDigestProteinsMinimumpI.Text = "Minimum pI";
            // 
            // chkGenerateUniqueIDValues
            // 
            this.chkGenerateUniqueIDValues.Checked = true;
            this.chkGenerateUniqueIDValues.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateUniqueIDValues.Location = new System.Drawing.Point(218, 102);
            this.chkGenerateUniqueIDValues.Name = "chkGenerateUniqueIDValues";
            this.chkGenerateUniqueIDValues.Size = new System.Drawing.Size(176, 16);
            this.chkGenerateUniqueIDValues.TabIndex = 14;
            this.chkGenerateUniqueIDValues.Text = "Generate UniqueID Values";
            // 
            // chkCysPeptidesOnly
            // 
            this.chkCysPeptidesOnly.Location = new System.Drawing.Point(486, 16);
            this.chkCysPeptidesOnly.Name = "chkCysPeptidesOnly";
            this.chkCysPeptidesOnly.Size = new System.Drawing.Size(112, 32);
            this.chkCysPeptidesOnly.TabIndex = 15;
            this.chkCysPeptidesOnly.Text = "Include cysteine peptides only";
            // 
            // txtDigestProteinsMinimumResidueCount
            // 
            this.txtDigestProteinsMinimumResidueCount.Location = new System.Drawing.Point(359, 56);
            this.txtDigestProteinsMinimumResidueCount.Name = "txtDigestProteinsMinimumResidueCount";
            this.txtDigestProteinsMinimumResidueCount.Size = new System.Drawing.Size(32, 20);
            this.txtDigestProteinsMinimumResidueCount.TabIndex = 7;
            this.txtDigestProteinsMinimumResidueCount.Text = "0";
            this.txtDigestProteinsMinimumResidueCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDigestProteinsMinimumResidueCount_KeyPress);
            // 
            // lblDigestProteinsMinimumResidueCount
            // 
            this.lblDigestProteinsMinimumResidueCount.Location = new System.Drawing.Point(216, 58);
            this.lblDigestProteinsMinimumResidueCount.Name = "lblDigestProteinsMinimumResidueCount";
            this.lblDigestProteinsMinimumResidueCount.Size = new System.Drawing.Size(136, 16);
            this.lblDigestProteinsMinimumResidueCount.TabIndex = 6;
            this.lblDigestProteinsMinimumResidueCount.Text = "Minimum Residue Count";
            // 
            // txtDigestProteinsMaximumMissedCleavages
            // 
            this.txtDigestProteinsMaximumMissedCleavages.Location = new System.Drawing.Point(359, 80);
            this.txtDigestProteinsMaximumMissedCleavages.Name = "txtDigestProteinsMaximumMissedCleavages";
            this.txtDigestProteinsMaximumMissedCleavages.Size = new System.Drawing.Size(32, 20);
            this.txtDigestProteinsMaximumMissedCleavages.TabIndex = 9;
            this.txtDigestProteinsMaximumMissedCleavages.Text = "3";
            this.txtDigestProteinsMaximumMissedCleavages.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDigestProteinsMaximumMissedCleavages_KeyPress);
            // 
            // lblDigestProteinsMaximumMissedCleavages
            // 
            this.lblDigestProteinsMaximumMissedCleavages.Location = new System.Drawing.Point(216, 82);
            this.lblDigestProteinsMaximumMissedCleavages.Name = "lblDigestProteinsMaximumMissedCleavages";
            this.lblDigestProteinsMaximumMissedCleavages.Size = new System.Drawing.Size(136, 16);
            this.lblDigestProteinsMaximumMissedCleavages.TabIndex = 8;
            this.lblDigestProteinsMaximumMissedCleavages.Text = "Max Missed Cleavages";
            // 
            // txtDigestProteinsMaximumMass
            // 
            this.txtDigestProteinsMaximumMass.Location = new System.Drawing.Point(152, 80);
            this.txtDigestProteinsMaximumMass.Name = "txtDigestProteinsMaximumMass";
            this.txtDigestProteinsMaximumMass.Size = new System.Drawing.Size(40, 20);
            this.txtDigestProteinsMaximumMass.TabIndex = 5;
            this.txtDigestProteinsMaximumMass.Text = "6000";
            this.txtDigestProteinsMaximumMass.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDigestProteinsMaximumMass_KeyPress);
            // 
            // lblDigestProteinsMaximumMass
            // 
            this.lblDigestProteinsMaximumMass.Location = new System.Drawing.Point(8, 82);
            this.lblDigestProteinsMaximumMass.Name = "lblDigestProteinsMaximumMass";
            this.lblDigestProteinsMaximumMass.Size = new System.Drawing.Size(144, 16);
            this.lblDigestProteinsMaximumMass.TabIndex = 4;
            this.lblDigestProteinsMaximumMass.Text = "Maximum Fragment Mass";
            // 
            // txtDigestProteinsMinimumMass
            // 
            this.txtDigestProteinsMinimumMass.Location = new System.Drawing.Point(152, 56);
            this.txtDigestProteinsMinimumMass.Name = "txtDigestProteinsMinimumMass";
            this.txtDigestProteinsMinimumMass.Size = new System.Drawing.Size(40, 20);
            this.txtDigestProteinsMinimumMass.TabIndex = 3;
            this.txtDigestProteinsMinimumMass.Text = "400";
            this.txtDigestProteinsMinimumMass.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDigestProteinsMinimumMass_KeyPress);
            // 
            // lblDigestProteinsMinimumMass
            // 
            this.lblDigestProteinsMinimumMass.Location = new System.Drawing.Point(8, 58);
            this.lblDigestProteinsMinimumMass.Name = "lblDigestProteinsMinimumMass";
            this.lblDigestProteinsMinimumMass.Size = new System.Drawing.Size(144, 16);
            this.lblDigestProteinsMinimumMass.TabIndex = 2;
            this.lblDigestProteinsMinimumMass.Text = "Minimum Fragment Mass";
            // 
            // cboCleavageRuleType
            // 
            this.cboCleavageRuleType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCleavageRuleType.Location = new System.Drawing.Point(8, 24);
            this.cboCleavageRuleType.Name = "cboCleavageRuleType";
            this.cboCleavageRuleType.Size = new System.Drawing.Size(288, 21);
            this.cboCleavageRuleType.TabIndex = 0;
            // 
            // chkIncludeDuplicateSequences
            // 
            this.chkIncludeDuplicateSequences.Location = new System.Drawing.Point(312, 16);
            this.chkIncludeDuplicateSequences.Name = "chkIncludeDuplicateSequences";
            this.chkIncludeDuplicateSequences.Size = new System.Drawing.Size(168, 32);
            this.chkIncludeDuplicateSequences.TabIndex = 1;
            this.chkIncludeDuplicateSequences.Text = "Include duplicate sequences for given protein";
            // 
            // cmdParseInputFile
            // 
            this.cmdParseInputFile.Location = new System.Drawing.Point(384, 8);
            this.cmdParseInputFile.Name = "cmdParseInputFile";
            this.cmdParseInputFile.Size = new System.Drawing.Size(112, 24);
            this.cmdParseInputFile.TabIndex = 3;
            this.cmdParseInputFile.Text = "&Parse and Digest";
            this.cmdParseInputFile.Click += new System.EventHandler(this.cmdParseInputFile_Click);
            // 
            // TabPagePeakMatchingThresholds
            // 
            this.TabPagePeakMatchingThresholds.Controls.Add(this.chkAutoDefineSLiCScoreTolerances);
            this.TabPagePeakMatchingThresholds.Controls.Add(this.cmdPastePMThresholdsList);
            this.TabPagePeakMatchingThresholds.Controls.Add(this.cboPMPredefinedThresholds);
            this.TabPagePeakMatchingThresholds.Controls.Add(this.cmdPMThresholdsAutoPopulate);
            this.TabPagePeakMatchingThresholds.Controls.Add(this.cmdClearPMThresholdsList);
            this.TabPagePeakMatchingThresholds.Controls.Add(this.cboMassTolType);
            this.TabPagePeakMatchingThresholds.Controls.Add(this.lblMassTolType);
            this.TabPagePeakMatchingThresholds.Controls.Add(this.dgPeakMatchingThresholds);
            this.TabPagePeakMatchingThresholds.Location = new System.Drawing.Point(4, 22);
            this.TabPagePeakMatchingThresholds.Name = "TabPagePeakMatchingThresholds";
            this.TabPagePeakMatchingThresholds.Size = new System.Drawing.Size(696, 328);
            this.TabPagePeakMatchingThresholds.TabIndex = 3;
            this.TabPagePeakMatchingThresholds.Text = "Peak Matching Thresholds";
            this.TabPagePeakMatchingThresholds.UseVisualStyleBackColor = true;
            // 
            // chkAutoDefineSLiCScoreTolerances
            // 
            this.chkAutoDefineSLiCScoreTolerances.Checked = true;
            this.chkAutoDefineSLiCScoreTolerances.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAutoDefineSLiCScoreTolerances.Location = new System.Drawing.Point(16, 256);
            this.chkAutoDefineSLiCScoreTolerances.Name = "chkAutoDefineSLiCScoreTolerances";
            this.chkAutoDefineSLiCScoreTolerances.Size = new System.Drawing.Size(208, 16);
            this.chkAutoDefineSLiCScoreTolerances.TabIndex = 3;
            this.chkAutoDefineSLiCScoreTolerances.Text = "Auto Define SLiC Score Tolerances";
            this.chkAutoDefineSLiCScoreTolerances.CheckedChanged += new System.EventHandler(this.chkAutoDefineSLiCScoreTolerances_CheckedChanged);
            // 
            // cmdPastePMThresholdsList
            // 
            this.cmdPastePMThresholdsList.Location = new System.Drawing.Point(456, 96);
            this.cmdPastePMThresholdsList.Name = "cmdPastePMThresholdsList";
            this.cmdPastePMThresholdsList.Size = new System.Drawing.Size(104, 24);
            this.cmdPastePMThresholdsList.TabIndex = 6;
            this.cmdPastePMThresholdsList.Text = "Paste Values";
            this.cmdPastePMThresholdsList.Click += new System.EventHandler(this.cmdPastePMThresholdsList_Click);
            // 
            // cboPMPredefinedThresholds
            // 
            this.cboPMPredefinedThresholds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPMPredefinedThresholds.Location = new System.Drawing.Point(336, 256);
            this.cboPMPredefinedThresholds.Name = "cboPMPredefinedThresholds";
            this.cboPMPredefinedThresholds.Size = new System.Drawing.Size(264, 21);
            this.cboPMPredefinedThresholds.TabIndex = 5;
            // 
            // cmdPMThresholdsAutoPopulate
            // 
            this.cmdPMThresholdsAutoPopulate.Location = new System.Drawing.Point(336, 224);
            this.cmdPMThresholdsAutoPopulate.Name = "cmdPMThresholdsAutoPopulate";
            this.cmdPMThresholdsAutoPopulate.Size = new System.Drawing.Size(104, 24);
            this.cmdPMThresholdsAutoPopulate.TabIndex = 4;
            this.cmdPMThresholdsAutoPopulate.Text = "Auto-Populate";
            this.cmdPMThresholdsAutoPopulate.Click += new System.EventHandler(this.cmdPMThresholdsAutoPopulate_Click);
            // 
            // cmdClearPMThresholdsList
            // 
            this.cmdClearPMThresholdsList.Location = new System.Drawing.Point(456, 128);
            this.cmdClearPMThresholdsList.Name = "cmdClearPMThresholdsList";
            this.cmdClearPMThresholdsList.Size = new System.Drawing.Size(104, 24);
            this.cmdClearPMThresholdsList.TabIndex = 7;
            this.cmdClearPMThresholdsList.Text = "Clear List";
            this.cmdClearPMThresholdsList.Click += new System.EventHandler(this.cmdClearPMThresholdsList_Click);
            // 
            // cboMassTolType
            // 
            this.cboMassTolType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMassTolType.Location = new System.Drawing.Point(144, 224);
            this.cboMassTolType.Name = "cboMassTolType";
            this.cboMassTolType.Size = new System.Drawing.Size(136, 21);
            this.cboMassTolType.TabIndex = 2;
            // 
            // lblMassTolType
            // 
            this.lblMassTolType.Location = new System.Drawing.Point(16, 226);
            this.lblMassTolType.Name = "lblMassTolType";
            this.lblMassTolType.Size = new System.Drawing.Size(136, 16);
            this.lblMassTolType.TabIndex = 1;
            this.lblMassTolType.Text = "Mass Tolerance Type";
            // 
            // dgPeakMatchingThresholds
            // 
            this.dgPeakMatchingThresholds.CaptionText = "Peak Matching Thresholds";
            this.dgPeakMatchingThresholds.DataMember = "";
            this.dgPeakMatchingThresholds.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgPeakMatchingThresholds.Location = new System.Drawing.Point(16, 8);
            this.dgPeakMatchingThresholds.Name = "dgPeakMatchingThresholds";
            this.dgPeakMatchingThresholds.Size = new System.Drawing.Size(424, 208);
            this.dgPeakMatchingThresholds.TabIndex = 0;
            // 
            // TabPageProgress
            // 
            this.TabPageProgress.Controls.Add(this.pbarProgress);
            this.TabPageProgress.Controls.Add(this.lblErrorMessage);
            this.TabPageProgress.Controls.Add(this.lblSubtaskProgress);
            this.TabPageProgress.Controls.Add(this.lblProgress);
            this.TabPageProgress.Controls.Add(this.lblSubtaskProgressDescription);
            this.TabPageProgress.Controls.Add(this.lblProgressDescription);
            this.TabPageProgress.Controls.Add(this.cmdAbortProcessing);
            this.TabPageProgress.Location = new System.Drawing.Point(4, 22);
            this.TabPageProgress.Name = "TabPageProgress";
            this.TabPageProgress.Size = new System.Drawing.Size(696, 328);
            this.TabPageProgress.TabIndex = 4;
            this.TabPageProgress.Text = "Progress";
            this.TabPageProgress.UseVisualStyleBackColor = true;
            // 
            // pbarProgress
            // 
            this.pbarProgress.Location = new System.Drawing.Point(13, 12);
            this.pbarProgress.Name = "pbarProgress";
            this.pbarProgress.Size = new System.Drawing.Size(122, 20);
            this.pbarProgress.TabIndex = 12;
            // 
            // lblErrorMessage
            // 
            this.lblErrorMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblErrorMessage.Location = new System.Drawing.Point(137, 112);
            this.lblErrorMessage.Name = "lblErrorMessage";
            this.lblErrorMessage.Size = new System.Drawing.Size(515, 32);
            this.lblErrorMessage.TabIndex = 11;
            this.lblErrorMessage.Text = "Error message ...";
            this.lblErrorMessage.Visible = false;
            // 
            // lblSubtaskProgress
            // 
            this.lblSubtaskProgress.Location = new System.Drawing.Point(13, 61);
            this.lblSubtaskProgress.Name = "lblSubtaskProgress";
            this.lblSubtaskProgress.Size = new System.Drawing.Size(118, 18);
            this.lblSubtaskProgress.TabIndex = 8;
            this.lblSubtaskProgress.Text = "0";
            this.lblSubtaskProgress.Visible = false;
            // 
            // lblProgress
            // 
            this.lblProgress.Location = new System.Drawing.Point(13, 35);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(118, 18);
            this.lblProgress.TabIndex = 7;
            this.lblProgress.Text = "0";
            // 
            // lblSubtaskProgressDescription
            // 
            this.lblSubtaskProgressDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSubtaskProgressDescription.Location = new System.Drawing.Point(140, 61);
            this.lblSubtaskProgressDescription.Name = "lblSubtaskProgressDescription";
            this.lblSubtaskProgressDescription.Size = new System.Drawing.Size(515, 32);
            this.lblSubtaskProgressDescription.TabIndex = 6;
            this.lblSubtaskProgressDescription.Text = "Subtask progress description ...";
            this.lblSubtaskProgressDescription.Visible = false;
            // 
            // lblProgressDescription
            // 
            this.lblProgressDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgressDescription.Location = new System.Drawing.Point(140, 12);
            this.lblProgressDescription.Name = "lblProgressDescription";
            this.lblProgressDescription.Size = new System.Drawing.Size(515, 32);
            this.lblProgressDescription.TabIndex = 5;
            this.lblProgressDescription.Text = "Progress description ...";
            // 
            // cmdAbortProcessing
            // 
            this.cmdAbortProcessing.Location = new System.Drawing.Point(10, 106);
            this.cmdAbortProcessing.Name = "cmdAbortProcessing";
            this.cmdAbortProcessing.Size = new System.Drawing.Size(121, 24);
            this.cmdAbortProcessing.TabIndex = 4;
            this.cmdAbortProcessing.Text = "Abort Processing";
            this.cmdAbortProcessing.Click += new System.EventHandler(this.cmdAbortProcessing_Click);
            // 
            // mnuHelpAboutElutionTime
            // 
            this.mnuHelpAboutElutionTime.Index = 1;
            this.mnuHelpAboutElutionTime.Text = "About &Elution Time Prediction";
            this.mnuHelpAboutElutionTime.Click += new System.EventHandler(this.mnuHelpAboutElutionTime_Click);
            // 
            // cboInputFileFormat
            // 
            this.cboInputFileFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInputFileFormat.Location = new System.Drawing.Point(112, 56);
            this.cboInputFileFormat.Name = "cboInputFileFormat";
            this.cboInputFileFormat.Size = new System.Drawing.Size(112, 21);
            this.cboInputFileFormat.TabIndex = 3;
            this.cboInputFileFormat.SelectedIndexChanged += new System.EventHandler(this.cboInputFileFormat_SelectedIndexChanged);
            // 
            // lblInputFileFormat
            // 
            this.lblInputFileFormat.Location = new System.Drawing.Point(8, 58);
            this.lblInputFileFormat.Name = "lblInputFileFormat";
            this.lblInputFileFormat.Size = new System.Drawing.Size(104, 16);
            this.lblInputFileFormat.TabIndex = 2;
            this.lblInputFileFormat.Text = "Input File Format";
            // 
            // cmdSelectFile
            // 
            this.cmdSelectFile.Location = new System.Drawing.Point(8, 24);
            this.cmdSelectFile.Name = "cmdSelectFile";
            this.cmdSelectFile.Size = new System.Drawing.Size(80, 24);
            this.cmdSelectFile.TabIndex = 0;
            this.cmdSelectFile.Text = "&Select file";
            this.cmdSelectFile.Click += new System.EventHandler(this.cmdSelectFile_Click);
            // 
            // fraInputFilePath
            // 
            this.fraInputFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fraInputFilePath.Controls.Add(this.cmdValidateFastaFile);
            this.fraInputFilePath.Controls.Add(this.cboInputFileFormat);
            this.fraInputFilePath.Controls.Add(this.lblInputFileFormat);
            this.fraInputFilePath.Controls.Add(this.cmdSelectFile);
            this.fraInputFilePath.Controls.Add(this.txtProteinInputFilePath);
            this.fraInputFilePath.Location = new System.Drawing.Point(12, 12);
            this.fraInputFilePath.Name = "fraInputFilePath";
            this.fraInputFilePath.Size = new System.Drawing.Size(730, 88);
            this.fraInputFilePath.TabIndex = 3;
            this.fraInputFilePath.TabStop = false;
            this.fraInputFilePath.Text = "Protein Input File Path (FASTA or Tab-delimited)";
            // 
            // cmdValidateFastaFile
            // 
            this.cmdValidateFastaFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdValidateFastaFile.Location = new System.Drawing.Point(595, 56);
            this.cmdValidateFastaFile.Name = "cmdValidateFastaFile";
            this.cmdValidateFastaFile.Size = new System.Drawing.Size(120, 24);
            this.cmdValidateFastaFile.TabIndex = 4;
            this.cmdValidateFastaFile.Text = "&Validate FASTA File";
            this.cmdValidateFastaFile.Click += new System.EventHandler(this.cmdValidateFastaFile_Click);
            // 
            // chkEnableLogging
            // 
            this.chkEnableLogging.Checked = true;
            this.chkEnableLogging.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableLogging.Location = new System.Drawing.Point(422, 19);
            this.chkEnableLogging.Name = "chkEnableLogging";
            this.chkEnableLogging.Size = new System.Drawing.Size(112, 24);
            this.chkEnableLogging.TabIndex = 4;
            this.chkEnableLogging.Text = "Enable logging";
            // 
            // mnuFileSelectOutputFile
            // 
            this.mnuFileSelectOutputFile.Index = 1;
            this.mnuFileSelectOutputFile.Text = "Select &Output File...";
            this.mnuFileSelectOutputFile.Click += new System.EventHandler(this.mnuFileSelectOutputFile_Click);
            // 
            // cmdSelectOutputFile
            // 
            this.cmdSelectOutputFile.Location = new System.Drawing.Point(8, 56);
            this.cmdSelectOutputFile.Name = "cmdSelectOutputFile";
            this.cmdSelectOutputFile.Size = new System.Drawing.Size(88, 33);
            this.cmdSelectOutputFile.TabIndex = 5;
            this.cmdSelectOutputFile.Text = "Select / &Create File";
            this.cmdSelectOutputFile.Click += new System.EventHandler(this.cmdSelectOutputFile_Click);
            // 
            // mnuFileSep1
            // 
            this.mnuFileSep1.Index = 2;
            this.mnuFileSep1.Text = "-";
            // 
            // mnuFile
            // 
            this.mnuFile.Index = 0;
            this.mnuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuFileSelectInputFile,
            this.mnuFileSelectOutputFile,
            this.mnuFileSep1,
            this.mnuFileSaveDefaultOptions,
            this.mnuFileSep2,
            this.mnuFileExit});
            this.mnuFile.Text = "&File";
            // 
            // mnuFileSelectInputFile
            // 
            this.mnuFileSelectInputFile.Index = 0;
            this.mnuFileSelectInputFile.Text = "Select &Input File...";
            this.mnuFileSelectInputFile.Click += new System.EventHandler(this.mnuFileSelectInputFile_Click);
            // 
            // mnuFileSaveDefaultOptions
            // 
            this.mnuFileSaveDefaultOptions.Index = 3;
            this.mnuFileSaveDefaultOptions.Text = "Save &Default Options";
            this.mnuFileSaveDefaultOptions.Click += new System.EventHandler(this.mnuFileSaveDefaultOptions_Click);
            // 
            // mnuFileSep2
            // 
            this.mnuFileSep2.Index = 4;
            this.mnuFileSep2.Text = "-";
            // 
            // mnuFileExit
            // 
            this.mnuFileExit.Index = 5;
            this.mnuFileExit.Text = "E&xit";
            this.mnuFileExit.Click += new System.EventHandler(this.mnuFileExit_Click);
            // 
            // txtProteinOutputFilePath
            // 
            this.txtProteinOutputFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProteinOutputFilePath.Location = new System.Drawing.Point(104, 62);
            this.txtProteinOutputFilePath.Name = "txtProteinOutputFilePath";
            this.txtProteinOutputFilePath.Size = new System.Drawing.Size(611, 20);
            this.txtProteinOutputFilePath.TabIndex = 6;
            // 
            // chkIncludePrefixAndSuffixResidues
            // 
            this.chkIncludePrefixAndSuffixResidues.Location = new System.Drawing.Point(256, 16);
            this.chkIncludePrefixAndSuffixResidues.Name = "chkIncludePrefixAndSuffixResidues";
            this.chkIncludePrefixAndSuffixResidues.Size = new System.Drawing.Size(160, 32);
            this.chkIncludePrefixAndSuffixResidues.TabIndex = 3;
            this.chkIncludePrefixAndSuffixResidues.Text = "Include prefix and suffix residues for the sequences";
            // 
            // mnuEditResetOptions
            // 
            this.mnuEditResetOptions.Index = 3;
            this.mnuEditResetOptions.Text = "&Reset options to Defaults...";
            this.mnuEditResetOptions.Click += new System.EventHandler(this.mnuEditResetOptions_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.Index = 2;
            this.mnuHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuHelpAbout,
            this.mnuHelpAboutElutionTime});
            this.mnuHelp.Text = "&Help";
            // 
            // mnuHelpAbout
            // 
            this.mnuHelpAbout.Index = 0;
            this.mnuHelpAbout.Text = "&About";
            this.mnuHelpAbout.Click += new System.EventHandler(this.mnuHelpAbout_Click);
            // 
            // mnuEditSep1
            // 
            this.mnuEditSep1.Index = 2;
            this.mnuEditSep1.Text = "-";
            // 
            // mnuEditMakeUniquenessStats
            // 
            this.mnuEditMakeUniquenessStats.Index = 1;
            this.mnuEditMakeUniquenessStats.Text = "&Make Uniqueness Stats";
            this.mnuEditMakeUniquenessStats.Click += new System.EventHandler(this.mnuEditMakeUniquenessStats_Click);
            // 
            // mnuEdit
            // 
            this.mnuEdit.Index = 1;
            this.mnuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuEditParseFile,
            this.mnuEditMakeUniquenessStats,
            this.mnuEditSep1,
            this.mnuEditResetOptions});
            this.mnuEdit.Text = "&Edit";
            // 
            // mnuEditParseFile
            // 
            this.mnuEditParseFile.Index = 0;
            this.mnuEditParseFile.Text = "&Parse File";
            this.mnuEditParseFile.Click += new System.EventHandler(this.mnuEditParseFile_Click);
            // 
            // MainMenuControl
            // 
            this.MainMenuControl.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuFile,
            this.mnuEdit,
            this.mnuHelp});
            // 
            // lblOutputFileFieldDelimiter
            // 
            this.lblOutputFileFieldDelimiter.Location = new System.Drawing.Point(8, 26);
            this.lblOutputFileFieldDelimiter.Name = "lblOutputFileFieldDelimiter";
            this.lblOutputFileFieldDelimiter.Size = new System.Drawing.Size(112, 18);
            this.lblOutputFileFieldDelimiter.TabIndex = 0;
            this.lblOutputFileFieldDelimiter.Text = "Field delimiter";
            // 
            // cboOutputFileFieldDelimiter
            // 
            this.cboOutputFileFieldDelimiter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOutputFileFieldDelimiter.Location = new System.Drawing.Point(128, 24);
            this.cboOutputFileFieldDelimiter.Name = "cboOutputFileFieldDelimiter";
            this.cboOutputFileFieldDelimiter.Size = new System.Drawing.Size(70, 21);
            this.cboOutputFileFieldDelimiter.TabIndex = 1;
            this.cboOutputFileFieldDelimiter.SelectedIndexChanged += new System.EventHandler(this.cboOutputFileFieldDelimiter_SelectedIndexChanged);
            // 
            // txtOutputFileFieldDelimiter
            // 
            this.txtOutputFileFieldDelimiter.Location = new System.Drawing.Point(208, 24);
            this.txtOutputFileFieldDelimiter.MaxLength = 1;
            this.txtOutputFileFieldDelimiter.Name = "txtOutputFileFieldDelimiter";
            this.txtOutputFileFieldDelimiter.Size = new System.Drawing.Size(32, 20);
            this.txtOutputFileFieldDelimiter.TabIndex = 2;
            this.txtOutputFileFieldDelimiter.Text = ";";
            // 
            // fraOutputTextOptions
            // 
            this.fraOutputTextOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fraOutputTextOptions.Controls.Add(this.chkEnableLogging);
            this.fraOutputTextOptions.Controls.Add(this.cmdSelectOutputFile);
            this.fraOutputTextOptions.Controls.Add(this.txtProteinOutputFilePath);
            this.fraOutputTextOptions.Controls.Add(this.chkIncludePrefixAndSuffixResidues);
            this.fraOutputTextOptions.Controls.Add(this.cboOutputFileFieldDelimiter);
            this.fraOutputTextOptions.Controls.Add(this.txtOutputFileFieldDelimiter);
            this.fraOutputTextOptions.Controls.Add(this.lblOutputFileFieldDelimiter);
            this.fraOutputTextOptions.Location = new System.Drawing.Point(12, 108);
            this.fraOutputTextOptions.Name = "fraOutputTextOptions";
            this.fraOutputTextOptions.Size = new System.Drawing.Size(730, 96);
            this.fraOutputTextOptions.TabIndex = 4;
            this.fraOutputTextOptions.TabStop = false;
            this.fraOutputTextOptions.Text = "Output Options";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(754, 577);
            this.Controls.Add(this.tbsOptions);
            this.Controls.Add(this.fraInputFilePath);
            this.Controls.Add(this.fraOutputTextOptions);
            this.Menu = this.MainMenuControl;
            this.Name = "Main";
            this.Text = "Protein Digestion Simulator";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.fraPeakMatchingOptions.ResumeLayout(false);
            this.fraPeakMatchingOptions.PerformLayout();
            this.TabPageUniquenessStats.ResumeLayout(false);
            this.fraSqlServerOptions.ResumeLayout(false);
            this.fraSqlServerOptions.PerformLayout();
            this.fraUniquenessBinningOptions.ResumeLayout(false);
            this.fraUniquenessBinningOptions.PerformLayout();
            this.frapIAndHydrophobicity.ResumeLayout(false);
            this.frapIAndHydrophobicity.PerformLayout();
            this.fraDelimitedFileOptions.ResumeLayout(false);
            this.fraDelimitedFileOptions.PerformLayout();
            this.TabPageFileFormatOptions.ResumeLayout(false);
            this.tbsOptions.ResumeLayout(false);
            this.TabPageParseAndDigest.ResumeLayout(false);
            this.fraProcessingOptions.ResumeLayout(false);
            this.fraProcessingOptions.PerformLayout();
            this.fraCalculationOptions.ResumeLayout(false);
            this.fraDigestionOptions.ResumeLayout(false);
            this.fraDigestionOptions.PerformLayout();
            this.TabPagePeakMatchingThresholds.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgPeakMatchingThresholds)).EndInit();
            this.TabPageProgress.ResumeLayout(false);
            this.fraInputFilePath.ResumeLayout(false);
            this.fraInputFilePath.PerformLayout();
            this.fraOutputTextOptions.ResumeLayout(false);
            this.fraOutputTextOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        private TextBox txtProteinInputFilePath;
        private RadioButton optUseRectangleSearchRegion;
        private RadioButton optUseEllipseSearchRegion;
        private Label lblUniquenessCalculationsNote;
        private Label lblProteinScramblingLoopCount;
        private GroupBox fraPeakMatchingOptions;
        private TextBox txtMaxPeakMatchingResultsPerFeatureToSave;
        private Label lblMaxPeakMatchingResultsPerFeatureToSave;
        private CheckBox chkExportPeakMatchingResults;
        private TextBox txtMinimumSLiCScore;
        private Label lblMinimumSLiCScore;
        private CheckBox chkUseSLiCScoreForUniqueness;
        private TabPage TabPageUniquenessStats;
        private GroupBox fraSqlServerOptions;
        private CheckBox chkSqlServerUseExistingData;
        private CheckBox chkAllowSqlServerCaching;
        private Label lblSqlServerPassword;
        private Label lblSqlServerUsername;
        private TextBox txtSqlServerPassword;
        private TextBox txtSqlServerUsername;
        private Label lblSqlServerDatabase;
        private Label lblSqlServerServerName;
        private CheckBox chkSqlServerUseIntegratedSecurity;
        private TextBox txtSqlServerDatabase;
        private TextBox txtSqlServerName;
        private CheckBox chkUseSqlServerDBToCacheData;
        private GroupBox fraUniquenessBinningOptions;
        private Label lblPeptideUniquenessMassMode;
        private TextBox txtUniquenessBinWidth;
        private Label lblUniquenessBinWidth;
        private CheckBox chkAutoComputeRangeForBinning;
        private TextBox txtUniquenessBinEndMass;
        private Label lblUniquenessBinEndMass;
        private TextBox txtUniquenessBinStartMass;
        private Label lblUniquenessBinStartMass;
        private Label lblUniquenessStatsNote;
        private Button cmdGenerateUniquenessStats;
        private CheckBox chkAssumeInputFileIsDigested;
        private TextBox txtProteinScramblingLoopCount;
        private Label lblSamplingPercentageUnits;
        private TextBox txtMaxpISequenceLength;
        private Label lblProteinReversalSamplingPercentage;
        private Label lblMaxpISequenceLength;
        private CheckBox chkMaxpIModeEnabled;
        private GroupBox frapIAndHydrophobicity;
        private Label lblHydrophobicityMode;
        private ComboBox cboHydrophobicityMode;
        private TextBox txtpIStats;
        private TextBox txtSequenceForpI;
        private Label lblSequenceForpI;
        private GroupBox fraDelimitedFileOptions;
        private ComboBox cboInputFileColumnOrdering;
        private Label lblInputFileColumnOrdering;
        private TextBox txtInputFileColumnDelimiter;
        private Label lblInputFileColumnDelimiter;
        private ComboBox cboInputFileColumnDelimiter;
        private TabPage TabPageFileFormatOptions;
        private TabControl tbsOptions;
        private TabPage TabPageParseAndDigest;
        private GroupBox fraProcessingOptions;
        private TextBox txtProteinReversalSamplingPercentage;
        private Label lbltxtAddnlRefAccessionSepChar;
        private CheckBox chkLookForAddnlRefInDescription;
        private ComboBox cboProteinReversalOptions;
        private Label lblProteinReversalOptions;
        private CheckBox chkDigestProteins;
        private Label lblAddnlRefSepChar;
        private TextBox txtAddnlRefAccessionSepChar;
        private TextBox txtAddnlRefSepChar;
        private CheckBox chkCreateFastaOutputFile;
        private GroupBox fraCalculationOptions;
        private Label lblMassMode;
        private ComboBox cboElementMassMode;
        private CheckBox chkExcludeProteinSequence;
        private CheckBox chkComputepIandNET;
        private CheckBox chkIncludeXResidues;
        private CheckBox chkComputeProteinMass;
        private GroupBox fraDigestionOptions;
        private TextBox txtDigestProteinsMaximumpI;
        private Label lblDigestProteinsMaximumpI;
        private TextBox txtDigestProteinsMinimumpI;
        private Label lblDigestProteinsMinimumpI;
        private CheckBox chkGenerateUniqueIDValues;
        private CheckBox chkCysPeptidesOnly;
        private TextBox txtDigestProteinsMinimumResidueCount;
        private Label lblDigestProteinsMinimumResidueCount;
        private TextBox txtDigestProteinsMaximumMissedCleavages;
        private Label lblDigestProteinsMaximumMissedCleavages;
        private TextBox txtDigestProteinsMaximumMass;
        private Label lblDigestProteinsMaximumMass;
        private TextBox txtDigestProteinsMinimumMass;
        private Label lblDigestProteinsMinimumMass;
        private ComboBox cboCleavageRuleType;
        private CheckBox chkIncludeDuplicateSequences;
        private Button cmdParseInputFile;
        private TabPage TabPagePeakMatchingThresholds;
        private CheckBox chkAutoDefineSLiCScoreTolerances;
        private Button cmdPastePMThresholdsList;
        private ComboBox cboPMPredefinedThresholds;
        private Button cmdPMThresholdsAutoPopulate;
        private Button cmdClearPMThresholdsList;
        private ComboBox cboMassTolType;
        private Label lblMassTolType;
        private DataGrid dgPeakMatchingThresholds;
        private TabPage TabPageProgress;
        private ProgressBar pbarProgress;
        private Label lblErrorMessage;
        private Label lblSubtaskProgress;
        private Label lblProgress;
        private Label lblSubtaskProgressDescription;
        private Label lblProgressDescription;
        private Button cmdAbortProcessing;
        private MenuItem mnuHelpAboutElutionTime;
        private ComboBox cboInputFileFormat;
        private Label lblInputFileFormat;
        private Button cmdSelectFile;
        private GroupBox fraInputFilePath;
        private Button cmdValidateFastaFile;
        private CheckBox chkEnableLogging;
        private MenuItem mnuFileSelectOutputFile;
        private Button cmdSelectOutputFile;
        private MenuItem mnuFileSep1;
        private MenuItem mnuFile;
        private MenuItem mnuFileSelectInputFile;
        private MenuItem mnuFileSaveDefaultOptions;
        private MenuItem mnuFileSep2;
        private MenuItem mnuFileExit;
        private TextBox txtProteinOutputFilePath;
        private CheckBox chkIncludePrefixAndSuffixResidues;
        private MenuItem mnuEditResetOptions;
        private MenuItem mnuHelp;
        private MenuItem mnuHelpAbout;
        private MenuItem mnuEditSep1;
        private MenuItem mnuEditMakeUniquenessStats;
        private MenuItem mnuEdit;
        private MenuItem mnuEditParseFile;
        private MainMenu MainMenuControl;
        private Label lblOutputFileFieldDelimiter;
        private ComboBox cboOutputFileFieldDelimiter;
        private TextBox txtOutputFileFieldDelimiter;
        private GroupBox fraOutputTextOptions;
        private CheckBox chkTruncateProteinDescription;
        private CheckBox chkComputeSequenceHashValues;
        private CheckBox chkComputeSequenceHashIgnoreILDiff;
        private CheckBox chkExcludeProteinDescription;
        private Button cmdNETInfo;
        private ComboBox cboCysTreatmentMode;
        private Label lblCysTreatment;
        private ComboBox cboFragmentMassMode;
        private Label lblFragmentMassMode;
    }
}