using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace ProteinDigestionSimulator
{
    [DesignerGenerated()]
    public partial class Main : Form
    {
        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            txtProteinInputFilePath = new TextBox();
            txtProteinInputFilePath.TextChanged += new EventHandler(txtProteinInputFilePath_TextChanged);
            optUseRectangleSearchRegion = new RadioButton();
            optUseEllipseSearchRegion = new RadioButton();
            lblUniquenessCalculationsNote = new Label();
            lblProteinScramblingLoopCount = new Label();
            fraPeakMatchingOptions = new GroupBox();
            txtMaxPeakMatchingResultsPerFeatureToSave = new TextBox();
            txtMaxPeakMatchingResultsPerFeatureToSave.KeyPress += new KeyPressEventHandler(txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress);
            txtMaxPeakMatchingResultsPerFeatureToSave.Validating += new System.ComponentModel.CancelEventHandler(txtMaxPeakMatchingResultsPerFeatureToSave_Validating);
            lblMaxPeakMatchingResultsPerFeatureToSave = new Label();
            chkExportPeakMatchingResults = new CheckBox();
            txtMinimumSLiCScore = new TextBox();
            txtMinimumSLiCScore.KeyPress += new KeyPressEventHandler(txtMinimumSLiCScore_KeyPress);
            txtMinimumSLiCScore.Validating += new System.ComponentModel.CancelEventHandler(txtMinimumSLiCScore_Validating);
            lblMinimumSLiCScore = new Label();
            chkUseSLiCScoreForUniqueness = new CheckBox();
            chkUseSLiCScoreForUniqueness.CheckedChanged += new EventHandler(chkUseSLiCScoreForUniqueness_CheckedChanged);
            TabPageUniquenessStats = new TabPage();
            fraSqlServerOptions = new GroupBox();
            chkSqlServerUseExistingData = new CheckBox();
            chkAllowSqlServerCaching = new CheckBox();
            chkAllowSqlServerCaching.CheckedChanged += new EventHandler(chkAllowSqlServerCaching_CheckedChanged);
            lblSqlServerPassword = new Label();
            lblSqlServerUsername = new Label();
            txtSqlServerPassword = new TextBox();
            txtSqlServerUsername = new TextBox();
            lblSqlServerDatabase = new Label();
            lblSqlServerServerName = new Label();
            chkSqlServerUseIntegratedSecurity = new CheckBox();
            chkSqlServerUseIntegratedSecurity.CheckedChanged += new EventHandler(chkSqlServerUseIntegratedSecurity_CheckedChanged);
            txtSqlServerDatabase = new TextBox();
            txtSqlServerName = new TextBox();
            chkUseSqlServerDBToCacheData = new CheckBox();
            chkUseSqlServerDBToCacheData.CheckedChanged += new EventHandler(chkUseSqlServerDBToCacheData_CheckedChanged);
            fraUniquenessBinningOptions = new GroupBox();
            lblPeptideUniquenessMassMode = new Label();
            txtUniquenessBinWidth = new TextBox();
            txtUniquenessBinWidth.KeyPress += new KeyPressEventHandler(txtUniquenessBinWidth_KeyPress);
            lblUniquenessBinWidth = new Label();
            chkAutoComputeRangeForBinning = new CheckBox();
            chkAutoComputeRangeForBinning.CheckedChanged += new EventHandler(chkAutoComputeRangeForBinning_CheckedChanged);
            txtUniquenessBinEndMass = new TextBox();
            txtUniquenessBinEndMass.KeyPress += new KeyPressEventHandler(txtUniquenessBinEndMass_KeyPress);
            lblUniquenessBinEndMass = new Label();
            txtUniquenessBinStartMass = new TextBox();
            txtUniquenessBinStartMass.KeyPress += new KeyPressEventHandler(txtUniquenessBinStartMass_KeyPress);
            lblUniquenessBinStartMass = new Label();
            lblUniquenessStatsNote = new Label();
            cmdGenerateUniquenessStats = new Button();
            cmdGenerateUniquenessStats.Click += new EventHandler(cmdGenerateUniquenessStats_Click);
            chkAssumeInputFileIsDigested = new CheckBox();
            txtProteinScramblingLoopCount = new TextBox();
            txtProteinScramblingLoopCount.KeyPress += new KeyPressEventHandler(txtProteinScramblingLoopCount_KeyPress);
            lblSamplingPercentageUnits = new Label();
            txtMaxpISequenceLength = new TextBox();
            txtMaxpISequenceLength.KeyDown += new KeyEventHandler(txtMaxpISequenceLength_KeyDown);
            txtMaxpISequenceLength.KeyPress += new KeyPressEventHandler(txtMaxpISequenceLength_KeyPress);
            txtMaxpISequenceLength.Validating += new System.ComponentModel.CancelEventHandler(txtMaxpISequenceLength_Validating);
            txtMaxpISequenceLength.Validated += new EventHandler(txtMaxpISequenceLength_Validated);
            lblProteinReversalSamplingPercentage = new Label();
            lblMaxpISequenceLength = new Label();
            chkMaxpIModeEnabled = new CheckBox();
            chkMaxpIModeEnabled.CheckedChanged += new EventHandler(chkMaxpIModeEnabled_CheckedChanged);
            frapIAndHydrophobicity = new GroupBox();
            lblHydrophobicityMode = new Label();
            cboHydrophobicityMode = new ComboBox();
            cboHydrophobicityMode.SelectedIndexChanged += new EventHandler(cboHydrophobicityMode_SelectedIndexChanged);
            txtpIStats = new TextBox();
            txtSequenceForpI = new TextBox();
            txtSequenceForpI.TextChanged += new EventHandler(txtSequenceForpI_TextChanged);
            lblSequenceForpI = new Label();
            fraDelimitedFileOptions = new GroupBox();
            cboInputFileColumnOrdering = new ComboBox();
            lblInputFileColumnOrdering = new Label();
            txtInputFileColumnDelimiter = new TextBox();
            lblInputFileColumnDelimiter = new Label();
            cboInputFileColumnDelimiter = new ComboBox();
            cboInputFileColumnDelimiter.SelectedIndexChanged += new EventHandler(cboInputFileColumnDelimiter_SelectedIndexChanged);
            TabPageFileFormatOptions = new TabPage();
            tbsOptions = new TabControl();
            TabPageParseAndDigest = new TabPage();
            fraProcessingOptions = new GroupBox();
            txtProteinReversalSamplingPercentage = new TextBox();
            txtProteinReversalSamplingPercentage.KeyPress += new KeyPressEventHandler(txtProteinReversalSamplingPercentage_KeyPress);
            lbltxtAddnlRefAccessionSepChar = new Label();
            chkLookForAddnlRefInDescription = new CheckBox();
            chkLookForAddnlRefInDescription.CheckedChanged += new EventHandler(chkLookForAddnlRefInDescription_CheckedChanged);
            cboProteinReversalOptions = new ComboBox();
            cboProteinReversalOptions.SelectedIndexChanged += new EventHandler(cboProteinReversalOptions_SelectedIndexChanged);
            lblProteinReversalOptions = new Label();
            chkDigestProteins = new CheckBox();
            chkDigestProteins.CheckedChanged += new EventHandler(chkDigestProteins_CheckedChanged);
            lblAddnlRefSepChar = new Label();
            txtAddnlRefAccessionSepChar = new TextBox();
            txtAddnlRefSepChar = new TextBox();
            chkCreateFastaOutputFile = new CheckBox();
            chkCreateFastaOutputFile.CheckedChanged += new EventHandler(chkCreateFastaOutputFile_CheckedChanged);
            fraCalculationOptions = new GroupBox();
            cmdNETInfo = new Button();
            cmdNETInfo.Click += new EventHandler(cmdNETInfo_Click);
            chkExcludeProteinDescription = new CheckBox();
            chkComputeSequenceHashIgnoreILDiff = new CheckBox();
            chkTruncateProteinDescription = new CheckBox();
            chkComputeSequenceHashValues = new CheckBox();
            chkComputeSequenceHashValues.CheckedChanged += new EventHandler(chkComputeSequenceHashValues_CheckedChanged);
            lblMassMode = new Label();
            cboElementMassMode = new ComboBox();
            cboElementMassMode.SelectedIndexChanged += new EventHandler(cboElementMassMode_SelectedIndexChanged);
            chkExcludeProteinSequence = new CheckBox();
            chkComputepIandNET = new CheckBox();
            chkComputepIandNET.CheckedChanged += new EventHandler(chkComputepI_CheckedChanged);
            chkIncludeXResidues = new CheckBox();
            chkComputeProteinMass = new CheckBox();
            fraDigestionOptions = new GroupBox();
            cboFragmentMassMode = new ComboBox();
            lblFragmentMassMode = new Label();
            cboCysTreatmentMode = new ComboBox();
            lblCysTreatment = new Label();
            txtDigestProteinsMaximumpI = new TextBox();
            lblDigestProteinsMaximumpI = new Label();
            txtDigestProteinsMinimumpI = new TextBox();
            lblDigestProteinsMinimumpI = new Label();
            chkGenerateUniqueIDValues = new CheckBox();
            chkCysPeptidesOnly = new CheckBox();
            txtDigestProteinsMinimumResidueCount = new TextBox();
            txtDigestProteinsMinimumResidueCount.KeyPress += new KeyPressEventHandler(txtDigestProteinsMinimumResidueCount_KeyPress);
            lblDigestProteinsMinimumResidueCount = new Label();
            txtDigestProteinsMaximumMissedCleavages = new TextBox();
            txtDigestProteinsMaximumMissedCleavages.KeyPress += new KeyPressEventHandler(txtDigestProteinsMaximumMissedCleavages_KeyPress);
            lblDigestProteinsMaximumMissedCleavages = new Label();
            txtDigestProteinsMaximumMass = new TextBox();
            txtDigestProteinsMaximumMass.KeyPress += new KeyPressEventHandler(txtDigestProteinsMaximumMass_KeyPress);
            lblDigestProteinsMaximumMass = new Label();
            txtDigestProteinsMinimumMass = new TextBox();
            txtDigestProteinsMinimumMass.KeyPress += new KeyPressEventHandler(txtDigestProteinsMinimumMass_KeyPress);
            lblDigestProteinsMinimumMass = new Label();
            cboCleavageRuleType = new ComboBox();
            chkIncludeDuplicateSequences = new CheckBox();
            cmdParseInputFile = new Button();
            cmdParseInputFile.Click += new EventHandler(cmdParseInputFile_Click);
            TabPagePeakMatchingThresholds = new TabPage();
            chkAutoDefineSLiCScoreTolerances = new CheckBox();
            chkAutoDefineSLiCScoreTolerances.CheckedChanged += new EventHandler(chkAutoDefineSLiCScoreTolerances_CheckedChanged);
            cmdPastePMThresholdsList = new Button();
            cmdPastePMThresholdsList.Click += new EventHandler(cmdPastePMThresholdsList_Click);
            cboPMPredefinedThresholds = new ComboBox();
            cmdPMThresholdsAutoPopulate = new Button();
            cmdPMThresholdsAutoPopulate.Click += new EventHandler(cmdPMThresholdsAutoPopulate_Click);
            cmdClearPMThresholdsList = new Button();
            cmdClearPMThresholdsList.Click += new EventHandler(cmdClearPMThresholdsList_Click);
            cboMassTolType = new ComboBox();
            lblMassTolType = new Label();
            dgPeakMatchingThresholds = new DataGrid();
            TabPageProgress = new TabPage();
            pbarProgress = new ProgressBar();
            lblErrorMessage = new Label();
            lblSubtaskProgress = new Label();
            lblProgress = new Label();
            lblSubtaskProgressDescription = new Label();
            lblProgressDescription = new Label();
            cmdAbortProcessing = new Button();
            cmdAbortProcessing.Click += new EventHandler(cmdAbortProcessing_Click);
            mnuHelpAboutElutionTime = new MenuItem();
            mnuHelpAboutElutionTime.Click += new EventHandler(mnuHelpAboutElutionTime_Click);
            cboInputFileFormat = new ComboBox();
            cboInputFileFormat.SelectedIndexChanged += new EventHandler(cboInputFileFormat_SelectedIndexChanged);
            lblInputFileFormat = new Label();
            cmdSelectFile = new Button();
            cmdSelectFile.Click += new EventHandler(cmdSelectFile_Click);
            fraInputFilePath = new GroupBox();
            cmdValidateFastaFile = new Button();
            cmdValidateFastaFile.Click += new EventHandler(cmdValidateFastaFile_Click);
            chkEnableLogging = new CheckBox();
            mnuFileSelectOutputFile = new MenuItem();
            mnuFileSelectOutputFile.Click += new EventHandler(mnuFileSelectOutputFile_Click);
            cmdSelectOutputFile = new Button();
            cmdSelectOutputFile.Click += new EventHandler(cmdSelectOutputFile_Click);
            mnuFileSep1 = new MenuItem();
            mnuFile = new MenuItem();
            mnuFileSelectInputFile = new MenuItem();
            mnuFileSelectInputFile.Click += new EventHandler(mnuFileSelectInputFile_Click);
            mnuFileSaveDefaultOptions = new MenuItem();
            mnuFileSaveDefaultOptions.Click += new EventHandler(mnuFileSaveDefaultOptions_Click);
            mnuFileSep2 = new MenuItem();
            mnuFileExit = new MenuItem();
            mnuFileExit.Click += new EventHandler(mnuFileExit_Click);
            txtProteinOutputFilePath = new TextBox();
            chkIncludePrefixAndSuffixResidues = new CheckBox();
            mnuEditResetOptions = new MenuItem();
            mnuEditResetOptions.Click += new EventHandler(mnuEditResetOptions_Click);
            mnuHelp = new MenuItem();
            mnuHelpAbout = new MenuItem();
            mnuHelpAbout.Click += new EventHandler(mnuHelpAbout_Click);
            mnuEditSep1 = new MenuItem();
            mnuEditMakeUniquenessStats = new MenuItem();
            mnuEditMakeUniquenessStats.Click += new EventHandler(mnuEditMakeUniquenessStats_Click);
            mnuEdit = new MenuItem();
            mnuEditParseFile = new MenuItem();
            mnuEditParseFile.Click += new EventHandler(mnuEditParseFile_Click);
            MainMenuControl = new MainMenu(components);
            lblOutputFileFieldDelimiter = new Label();
            cboOutputFileFieldDelimiter = new ComboBox();
            cboOutputFileFieldDelimiter.SelectedIndexChanged += new EventHandler(cboOutputFileFieldDelimiter_SelectedIndexChanged);
            txtOutputFileFieldDelimiter = new TextBox();
            fraOutputTextOptions = new GroupBox();
            fraPeakMatchingOptions.SuspendLayout();
            TabPageUniquenessStats.SuspendLayout();
            fraSqlServerOptions.SuspendLayout();
            fraUniquenessBinningOptions.SuspendLayout();
            frapIAndHydrophobicity.SuspendLayout();
            fraDelimitedFileOptions.SuspendLayout();
            TabPageFileFormatOptions.SuspendLayout();
            tbsOptions.SuspendLayout();
            TabPageParseAndDigest.SuspendLayout();
            fraProcessingOptions.SuspendLayout();
            fraCalculationOptions.SuspendLayout();
            fraDigestionOptions.SuspendLayout();
            TabPagePeakMatchingThresholds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgPeakMatchingThresholds).BeginInit();
            TabPageProgress.SuspendLayout();
            fraInputFilePath.SuspendLayout();
            fraOutputTextOptions.SuspendLayout();
            SuspendLayout();
            // 
            // txtProteinInputFilePath
            // 
            txtProteinInputFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtProteinInputFilePath.Location = new Point(104, 26);
            txtProteinInputFilePath.Name = "txtProteinInputFilePath";
            txtProteinInputFilePath.Size = new Size(611, 20);
            txtProteinInputFilePath.TabIndex = 1;
            // 
            // optUseRectangleSearchRegion
            // 
            optUseRectangleSearchRegion.Location = new Point(232, 96);
            optUseRectangleSearchRegion.Name = "optUseRectangleSearchRegion";
            optUseRectangleSearchRegion.Size = new Size(136, 16);
            optUseRectangleSearchRegion.TabIndex = 7;
            optUseRectangleSearchRegion.Text = "Use rectangle region";
            // 
            // optUseEllipseSearchRegion
            // 
            optUseEllipseSearchRegion.Checked = true;
            optUseEllipseSearchRegion.Location = new Point(232, 72);
            optUseEllipseSearchRegion.Name = "optUseEllipseSearchRegion";
            optUseEllipseSearchRegion.Size = new Size(152, 16);
            optUseEllipseSearchRegion.TabIndex = 6;
            optUseEllipseSearchRegion.TabStop = true;
            optUseEllipseSearchRegion.Text = "Use ellipse search region";
            // 
            // lblUniquenessCalculationsNote
            // 
            lblUniquenessCalculationsNote.Location = new Point(240, 192);
            lblUniquenessCalculationsNote.Name = "lblUniquenessCalculationsNote";
            lblUniquenessCalculationsNote.Size = new Size(384, 88);
            lblUniquenessCalculationsNote.TabIndex = 6;
            // 
            // lblProteinScramblingLoopCount
            // 
            lblProteinScramblingLoopCount.Location = new Point(232, 42);
            lblProteinScramblingLoopCount.Name = "lblProteinScramblingLoopCount";
            lblProteinScramblingLoopCount.Size = new Size(72, 16);
            lblProteinScramblingLoopCount.TabIndex = 12;
            lblProteinScramblingLoopCount.Text = "Loop Count";
            lblProteinScramblingLoopCount.TextAlign = ContentAlignment.TopRight;
            // 
            // fraPeakMatchingOptions
            // 
            fraPeakMatchingOptions.Controls.Add(optUseRectangleSearchRegion);
            fraPeakMatchingOptions.Controls.Add(optUseEllipseSearchRegion);
            fraPeakMatchingOptions.Controls.Add(txtMaxPeakMatchingResultsPerFeatureToSave);
            fraPeakMatchingOptions.Controls.Add(lblMaxPeakMatchingResultsPerFeatureToSave);
            fraPeakMatchingOptions.Controls.Add(chkExportPeakMatchingResults);
            fraPeakMatchingOptions.Controls.Add(txtMinimumSLiCScore);
            fraPeakMatchingOptions.Controls.Add(lblMinimumSLiCScore);
            fraPeakMatchingOptions.Controls.Add(chkUseSLiCScoreForUniqueness);
            fraPeakMatchingOptions.Location = new Point(232, 48);
            fraPeakMatchingOptions.Name = "fraPeakMatchingOptions";
            fraPeakMatchingOptions.Size = new Size(392, 136);
            fraPeakMatchingOptions.TabIndex = 2;
            fraPeakMatchingOptions.TabStop = false;
            fraPeakMatchingOptions.Text = "Peak Matching Options";
            // 
            // txtMaxPeakMatchingResultsPerFeatureToSave
            // 
            txtMaxPeakMatchingResultsPerFeatureToSave.Location = new Point(272, 16);
            txtMaxPeakMatchingResultsPerFeatureToSave.Name = "txtMaxPeakMatchingResultsPerFeatureToSave";
            txtMaxPeakMatchingResultsPerFeatureToSave.Size = new Size(40, 20);
            txtMaxPeakMatchingResultsPerFeatureToSave.TabIndex = 1;
            txtMaxPeakMatchingResultsPerFeatureToSave.Text = "3";
            // 
            // lblMaxPeakMatchingResultsPerFeatureToSave
            // 
            lblMaxPeakMatchingResultsPerFeatureToSave.Location = new Point(16, 18);
            lblMaxPeakMatchingResultsPerFeatureToSave.Name = "lblMaxPeakMatchingResultsPerFeatureToSave";
            lblMaxPeakMatchingResultsPerFeatureToSave.Size = new Size(256, 16);
            lblMaxPeakMatchingResultsPerFeatureToSave.TabIndex = 0;
            lblMaxPeakMatchingResultsPerFeatureToSave.Text = "Max Peak Matching Results Per Feature To Save";
            // 
            // chkExportPeakMatchingResults
            // 
            chkExportPeakMatchingResults.Location = new Point(32, 36);
            chkExportPeakMatchingResults.Name = "chkExportPeakMatchingResults";
            chkExportPeakMatchingResults.Size = new Size(192, 17);
            chkExportPeakMatchingResults.TabIndex = 2;
            chkExportPeakMatchingResults.Text = "Export peak matching results";
            // 
            // txtMinimumSLiCScore
            // 
            txtMinimumSLiCScore.Location = new Point(144, 104);
            txtMinimumSLiCScore.Name = "txtMinimumSLiCScore";
            txtMinimumSLiCScore.Size = new Size(40, 20);
            txtMinimumSLiCScore.TabIndex = 5;
            txtMinimumSLiCScore.Text = "0.99";
            // 
            // lblMinimumSLiCScore
            // 
            lblMinimumSLiCScore.Location = new Point(16, 96);
            lblMinimumSLiCScore.Name = "lblMinimumSLiCScore";
            lblMinimumSLiCScore.Size = new Size(128, 32);
            lblMinimumSLiCScore.TabIndex = 4;
            lblMinimumSLiCScore.Text = "Minimum SLiC score to be considered unique";
            // 
            // chkUseSLiCScoreForUniqueness
            // 
            chkUseSLiCScoreForUniqueness.Checked = true;
            chkUseSLiCScoreForUniqueness.CheckState = CheckState.Checked;
            chkUseSLiCScoreForUniqueness.Location = new Point(16, 60);
            chkUseSLiCScoreForUniqueness.Name = "chkUseSLiCScoreForUniqueness";
            chkUseSLiCScoreForUniqueness.Size = new Size(168, 32);
            chkUseSLiCScoreForUniqueness.TabIndex = 3;
            chkUseSLiCScoreForUniqueness.Text = "Use SLiC Score when gauging peptide uniqueness";
            // 
            // TabPageUniquenessStats
            // 
            TabPageUniquenessStats.Controls.Add(lblUniquenessCalculationsNote);
            TabPageUniquenessStats.Controls.Add(fraPeakMatchingOptions);
            TabPageUniquenessStats.Controls.Add(fraSqlServerOptions);
            TabPageUniquenessStats.Controls.Add(fraUniquenessBinningOptions);
            TabPageUniquenessStats.Controls.Add(lblUniquenessStatsNote);
            TabPageUniquenessStats.Controls.Add(cmdGenerateUniquenessStats);
            TabPageUniquenessStats.Controls.Add(chkAssumeInputFileIsDigested);
            TabPageUniquenessStats.Location = new Point(4, 22);
            TabPageUniquenessStats.Name = "TabPageUniquenessStats";
            TabPageUniquenessStats.Size = new Size(696, 328);
            TabPageUniquenessStats.TabIndex = 1;
            TabPageUniquenessStats.Text = "Peptide Uniqueness Options";
            TabPageUniquenessStats.UseVisualStyleBackColor = true;
            // 
            // fraSqlServerOptions
            // 
            fraSqlServerOptions.Controls.Add(chkSqlServerUseExistingData);
            fraSqlServerOptions.Controls.Add(chkAllowSqlServerCaching);
            fraSqlServerOptions.Controls.Add(lblSqlServerPassword);
            fraSqlServerOptions.Controls.Add(lblSqlServerUsername);
            fraSqlServerOptions.Controls.Add(txtSqlServerPassword);
            fraSqlServerOptions.Controls.Add(txtSqlServerUsername);
            fraSqlServerOptions.Controls.Add(lblSqlServerDatabase);
            fraSqlServerOptions.Controls.Add(lblSqlServerServerName);
            fraSqlServerOptions.Controls.Add(chkSqlServerUseIntegratedSecurity);
            fraSqlServerOptions.Controls.Add(txtSqlServerDatabase);
            fraSqlServerOptions.Controls.Add(txtSqlServerName);
            fraSqlServerOptions.Controls.Add(chkUseSqlServerDBToCacheData);
            fraSqlServerOptions.Location = new Point(232, 192);
            fraSqlServerOptions.Name = "fraSqlServerOptions";
            fraSqlServerOptions.Size = new Size(376, 112);
            fraSqlServerOptions.TabIndex = 4;
            fraSqlServerOptions.TabStop = false;
            fraSqlServerOptions.Text = "Sql Server Options";
            fraSqlServerOptions.Visible = false;
            // 
            // chkSqlServerUseExistingData
            // 
            chkSqlServerUseExistingData.Checked = true;
            chkSqlServerUseExistingData.CheckState = CheckState.Checked;
            chkSqlServerUseExistingData.Location = new Point(8, 88);
            chkSqlServerUseExistingData.Name = "chkSqlServerUseExistingData";
            chkSqlServerUseExistingData.Size = new Size(144, 16);
            chkSqlServerUseExistingData.TabIndex = 11;
            chkSqlServerUseExistingData.Text = "Use Existing Data";
            // 
            // chkAllowSqlServerCaching
            // 
            chkAllowSqlServerCaching.Location = new Point(8, 16);
            chkAllowSqlServerCaching.Name = "chkAllowSqlServerCaching";
            chkAllowSqlServerCaching.Size = new Size(144, 32);
            chkAllowSqlServerCaching.TabIndex = 0;
            chkAllowSqlServerCaching.Text = "Allow data caching using Sql Server";
            // 
            // lblSqlServerPassword
            // 
            lblSqlServerPassword.Location = new Point(264, 64);
            lblSqlServerPassword.Name = "lblSqlServerPassword";
            lblSqlServerPassword.Size = new Size(56, 16);
            lblSqlServerPassword.TabIndex = 9;
            lblSqlServerPassword.Text = "Password";
            // 
            // lblSqlServerUsername
            // 
            lblSqlServerUsername.Location = new Point(184, 64);
            lblSqlServerUsername.Name = "lblSqlServerUsername";
            lblSqlServerUsername.Size = new Size(56, 16);
            lblSqlServerUsername.TabIndex = 7;
            lblSqlServerUsername.Text = "Username";
            // 
            // txtSqlServerPassword
            // 
            txtSqlServerPassword.Location = new Point(264, 80);
            txtSqlServerPassword.Name = "txtSqlServerPassword";
            txtSqlServerPassword.PasswordChar = '*';
            txtSqlServerPassword.Size = new Size(88, 20);
            txtSqlServerPassword.TabIndex = 10;
            txtSqlServerPassword.Text = "pw";
            // 
            // txtSqlServerUsername
            // 
            txtSqlServerUsername.Location = new Point(184, 80);
            txtSqlServerUsername.Name = "txtSqlServerUsername";
            txtSqlServerUsername.Size = new Size(72, 20);
            txtSqlServerUsername.TabIndex = 8;
            txtSqlServerUsername.Text = "user";
            // 
            // lblSqlServerDatabase
            // 
            lblSqlServerDatabase.Location = new Point(264, 16);
            lblSqlServerDatabase.Name = "lblSqlServerDatabase";
            lblSqlServerDatabase.Size = new Size(56, 16);
            lblSqlServerDatabase.TabIndex = 4;
            lblSqlServerDatabase.Text = "Database";
            // 
            // lblSqlServerServerName
            // 
            lblSqlServerServerName.Location = new Point(184, 16);
            lblSqlServerServerName.Name = "lblSqlServerServerName";
            lblSqlServerServerName.Size = new Size(56, 16);
            lblSqlServerServerName.TabIndex = 2;
            lblSqlServerServerName.Text = "Server";
            // 
            // chkSqlServerUseIntegratedSecurity
            // 
            chkSqlServerUseIntegratedSecurity.Checked = true;
            chkSqlServerUseIntegratedSecurity.CheckState = CheckState.Checked;
            chkSqlServerUseIntegratedSecurity.Location = new Point(8, 72);
            chkSqlServerUseIntegratedSecurity.Name = "chkSqlServerUseIntegratedSecurity";
            chkSqlServerUseIntegratedSecurity.Size = new Size(144, 16);
            chkSqlServerUseIntegratedSecurity.TabIndex = 6;
            chkSqlServerUseIntegratedSecurity.Text = "Use Integrated Security";
            // 
            // txtSqlServerDatabase
            // 
            txtSqlServerDatabase.Location = new Point(264, 32);
            txtSqlServerDatabase.Name = "txtSqlServerDatabase";
            txtSqlServerDatabase.Size = new Size(88, 20);
            txtSqlServerDatabase.TabIndex = 5;
            txtSqlServerDatabase.Text = "TempDB";
            // 
            // txtSqlServerName
            // 
            txtSqlServerName.Location = new Point(184, 32);
            txtSqlServerName.Name = "txtSqlServerName";
            txtSqlServerName.Size = new Size(72, 20);
            txtSqlServerName.TabIndex = 3;
            txtSqlServerName.Text = "Monroe";
            // 
            // chkUseSqlServerDBToCacheData
            // 
            chkUseSqlServerDBToCacheData.Checked = true;
            chkUseSqlServerDBToCacheData.CheckState = CheckState.Checked;
            chkUseSqlServerDBToCacheData.Location = new Point(8, 56);
            chkUseSqlServerDBToCacheData.Name = "chkUseSqlServerDBToCacheData";
            chkUseSqlServerDBToCacheData.Size = new Size(144, 16);
            chkUseSqlServerDBToCacheData.TabIndex = 1;
            chkUseSqlServerDBToCacheData.Text = "Enable data caching";
            // 
            // fraUniquenessBinningOptions
            // 
            fraUniquenessBinningOptions.Controls.Add(lblPeptideUniquenessMassMode);
            fraUniquenessBinningOptions.Controls.Add(txtUniquenessBinWidth);
            fraUniquenessBinningOptions.Controls.Add(lblUniquenessBinWidth);
            fraUniquenessBinningOptions.Controls.Add(chkAutoComputeRangeForBinning);
            fraUniquenessBinningOptions.Controls.Add(txtUniquenessBinEndMass);
            fraUniquenessBinningOptions.Controls.Add(lblUniquenessBinEndMass);
            fraUniquenessBinningOptions.Controls.Add(txtUniquenessBinStartMass);
            fraUniquenessBinningOptions.Controls.Add(lblUniquenessBinStartMass);
            fraUniquenessBinningOptions.Location = new Point(8, 120);
            fraUniquenessBinningOptions.Name = "fraUniquenessBinningOptions";
            fraUniquenessBinningOptions.Size = new Size(208, 160);
            fraUniquenessBinningOptions.TabIndex = 3;
            fraUniquenessBinningOptions.TabStop = false;
            fraUniquenessBinningOptions.Text = "Binning Options";
            // 
            // lblPeptideUniquenessMassMode
            // 
            lblPeptideUniquenessMassMode.Location = new Point(16, 136);
            lblPeptideUniquenessMassMode.Name = "lblPeptideUniquenessMassMode";
            lblPeptideUniquenessMassMode.Size = new Size(176, 16);
            lblPeptideUniquenessMassMode.TabIndex = 7;
            lblPeptideUniquenessMassMode.Text = "Using monoisotopic masses";
            // 
            // txtUniquenessBinWidth
            // 
            txtUniquenessBinWidth.Location = new Point(80, 24);
            txtUniquenessBinWidth.Name = "txtUniquenessBinWidth";
            txtUniquenessBinWidth.Size = new Size(40, 20);
            txtUniquenessBinWidth.TabIndex = 1;
            txtUniquenessBinWidth.Text = "25";
            // 
            // lblUniquenessBinWidth
            // 
            lblUniquenessBinWidth.Location = new Point(16, 26);
            lblUniquenessBinWidth.Name = "lblUniquenessBinWidth";
            lblUniquenessBinWidth.Size = new Size(64, 16);
            lblUniquenessBinWidth.TabIndex = 0;
            lblUniquenessBinWidth.Text = "Bin Width";
            // 
            // chkAutoComputeRangeForBinning
            // 
            chkAutoComputeRangeForBinning.Checked = true;
            chkAutoComputeRangeForBinning.CheckState = CheckState.Checked;
            chkAutoComputeRangeForBinning.Location = new Point(16, 56);
            chkAutoComputeRangeForBinning.Name = "chkAutoComputeRangeForBinning";
            chkAutoComputeRangeForBinning.Size = new Size(184, 17);
            chkAutoComputeRangeForBinning.TabIndex = 2;
            chkAutoComputeRangeForBinning.Text = "Auto compute range for binning";
            // 
            // txtUniquenessBinEndMass
            // 
            txtUniquenessBinEndMass.Location = new Point(80, 104);
            txtUniquenessBinEndMass.Name = "txtUniquenessBinEndMass";
            txtUniquenessBinEndMass.Size = new Size(40, 20);
            txtUniquenessBinEndMass.TabIndex = 6;
            txtUniquenessBinEndMass.Text = "6000";
            // 
            // lblUniquenessBinEndMass
            // 
            lblUniquenessBinEndMass.Location = new Point(16, 106);
            lblUniquenessBinEndMass.Name = "lblUniquenessBinEndMass";
            lblUniquenessBinEndMass.Size = new Size(64, 16);
            lblUniquenessBinEndMass.TabIndex = 5;
            lblUniquenessBinEndMass.Text = "End Mass";
            // 
            // txtUniquenessBinStartMass
            // 
            txtUniquenessBinStartMass.Location = new Point(80, 80);
            txtUniquenessBinStartMass.Name = "txtUniquenessBinStartMass";
            txtUniquenessBinStartMass.Size = new Size(40, 20);
            txtUniquenessBinStartMass.TabIndex = 4;
            txtUniquenessBinStartMass.Text = "400";
            // 
            // lblUniquenessBinStartMass
            // 
            lblUniquenessBinStartMass.Location = new Point(16, 82);
            lblUniquenessBinStartMass.Name = "lblUniquenessBinStartMass";
            lblUniquenessBinStartMass.Size = new Size(64, 16);
            lblUniquenessBinStartMass.TabIndex = 3;
            lblUniquenessBinStartMass.Text = "Start Mass";
            // 
            // lblUniquenessStatsNote
            // 
            lblUniquenessStatsNote.Location = new Point(8, 56);
            lblUniquenessStatsNote.Name = "lblUniquenessStatsNote";
            lblUniquenessStatsNote.Size = new Size(200, 48);
            lblUniquenessStatsNote.TabIndex = 1;
            lblUniquenessStatsNote.Text = "Note that Digestion Options and Mass Calculation Options also apply for uniquenes" + "s stats generation.";
            // 
            // cmdGenerateUniquenessStats
            // 
            cmdGenerateUniquenessStats.Location = new Point(232, 16);
            cmdGenerateUniquenessStats.Name = "cmdGenerateUniquenessStats";
            cmdGenerateUniquenessStats.Size = new Size(176, 24);
            cmdGenerateUniquenessStats.TabIndex = 5;
            cmdGenerateUniquenessStats.Text = "&Generate Uniqueness Stats";
            // 
            // chkAssumeInputFileIsDigested
            // 
            chkAssumeInputFileIsDigested.Location = new Point(8, 16);
            chkAssumeInputFileIsDigested.Name = "chkAssumeInputFileIsDigested";
            chkAssumeInputFileIsDigested.Size = new Size(192, 32);
            chkAssumeInputFileIsDigested.TabIndex = 0;
            chkAssumeInputFileIsDigested.Text = "Assume input file is already digested (for Delimited files only)";
            // 
            // txtProteinScramblingLoopCount
            // 
            txtProteinScramblingLoopCount.Location = new Point(312, 40);
            txtProteinScramblingLoopCount.MaxLength = 3;
            txtProteinScramblingLoopCount.Name = "txtProteinScramblingLoopCount";
            txtProteinScramblingLoopCount.Size = new Size(32, 20);
            txtProteinScramblingLoopCount.TabIndex = 13;
            txtProteinScramblingLoopCount.Text = "1";
            // 
            // lblSamplingPercentageUnits
            // 
            lblSamplingPercentageUnits.Location = new Point(208, 42);
            lblSamplingPercentageUnits.Name = "lblSamplingPercentageUnits";
            lblSamplingPercentageUnits.Size = new Size(16, 16);
            lblSamplingPercentageUnits.TabIndex = 4;
            lblSamplingPercentageUnits.Text = "%";
            // 
            // txtMaxpISequenceLength
            // 
            txtMaxpISequenceLength.Location = new Point(168, 70);
            txtMaxpISequenceLength.Name = "txtMaxpISequenceLength";
            txtMaxpISequenceLength.Size = new Size(40, 20);
            txtMaxpISequenceLength.TabIndex = 4;
            txtMaxpISequenceLength.Text = "10";
            // 
            // lblProteinReversalSamplingPercentage
            // 
            lblProteinReversalSamplingPercentage.Location = new Point(48, 42);
            lblProteinReversalSamplingPercentage.Name = "lblProteinReversalSamplingPercentage";
            lblProteinReversalSamplingPercentage.Size = new Size(112, 16);
            lblProteinReversalSamplingPercentage.TabIndex = 2;
            lblProteinReversalSamplingPercentage.Text = "Sampling Percentage";
            lblProteinReversalSamplingPercentage.TextAlign = ContentAlignment.TopRight;
            // 
            // lblMaxpISequenceLength
            // 
            lblMaxpISequenceLength.Location = new Point(32, 72);
            lblMaxpISequenceLength.Name = "lblMaxpISequenceLength";
            lblMaxpISequenceLength.Size = new Size(144, 16);
            lblMaxpISequenceLength.TabIndex = 3;
            lblMaxpISequenceLength.Text = "Sub-sequence Length";
            // 
            // chkMaxpIModeEnabled
            // 
            chkMaxpIModeEnabled.Location = new Point(8, 48);
            chkMaxpIModeEnabled.Name = "chkMaxpIModeEnabled";
            chkMaxpIModeEnabled.Size = new Size(224, 16);
            chkMaxpIModeEnabled.TabIndex = 2;
            chkMaxpIModeEnabled.Text = "Report maximum of all sub-sequences";
            // 
            // frapIAndHydrophobicity
            // 
            frapIAndHydrophobicity.Controls.Add(txtMaxpISequenceLength);
            frapIAndHydrophobicity.Controls.Add(lblMaxpISequenceLength);
            frapIAndHydrophobicity.Controls.Add(chkMaxpIModeEnabled);
            frapIAndHydrophobicity.Controls.Add(lblHydrophobicityMode);
            frapIAndHydrophobicity.Controls.Add(cboHydrophobicityMode);
            frapIAndHydrophobicity.Controls.Add(txtpIStats);
            frapIAndHydrophobicity.Controls.Add(txtSequenceForpI);
            frapIAndHydrophobicity.Controls.Add(lblSequenceForpI);
            frapIAndHydrophobicity.Location = new Point(8, 108);
            frapIAndHydrophobicity.Name = "frapIAndHydrophobicity";
            frapIAndHydrophobicity.Size = new Size(616, 136);
            frapIAndHydrophobicity.TabIndex = 2;
            frapIAndHydrophobicity.TabStop = false;
            frapIAndHydrophobicity.Text = "pI And Hydrophobicity";
            // 
            // lblHydrophobicityMode
            // 
            lblHydrophobicityMode.Location = new Point(8, 24);
            lblHydrophobicityMode.Name = "lblHydrophobicityMode";
            lblHydrophobicityMode.Size = new Size(120, 16);
            lblHydrophobicityMode.TabIndex = 0;
            lblHydrophobicityMode.Text = "Hydrophobicity Mode";
            // 
            // cboHydrophobicityMode
            // 
            cboHydrophobicityMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboHydrophobicityMode.DropDownWidth = 70;
            cboHydrophobicityMode.Location = new Point(128, 18);
            cboHydrophobicityMode.Name = "cboHydrophobicityMode";
            cboHydrophobicityMode.Size = new Size(184, 21);
            cboHydrophobicityMode.TabIndex = 1;
            // 
            // txtpIStats
            // 
            txtpIStats.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            txtpIStats.Location = new Point(336, 48);
            txtpIStats.MaxLength = 1;
            txtpIStats.Multiline = true;
            txtpIStats.Name = "txtpIStats";
            txtpIStats.ReadOnly = true;
            txtpIStats.Size = new Size(272, 80);
            txtpIStats.TabIndex = 7;
            // 
            // txtSequenceForpI
            // 
            txtSequenceForpI.Location = new Point(400, 16);
            txtSequenceForpI.Name = "txtSequenceForpI";
            txtSequenceForpI.Size = new Size(208, 20);
            txtSequenceForpI.TabIndex = 6;
            txtSequenceForpI.Text = "FKDLGEEQFK";
            // 
            // lblSequenceForpI
            // 
            lblSequenceForpI.Location = new Point(328, 20);
            lblSequenceForpI.Name = "lblSequenceForpI";
            lblSequenceForpI.Size = new Size(72, 16);
            lblSequenceForpI.TabIndex = 5;
            lblSequenceForpI.Text = "Sequence";
            // 
            // fraDelimitedFileOptions
            // 
            fraDelimitedFileOptions.Controls.Add(cboInputFileColumnOrdering);
            fraDelimitedFileOptions.Controls.Add(lblInputFileColumnOrdering);
            fraDelimitedFileOptions.Controls.Add(txtInputFileColumnDelimiter);
            fraDelimitedFileOptions.Controls.Add(lblInputFileColumnDelimiter);
            fraDelimitedFileOptions.Controls.Add(cboInputFileColumnDelimiter);
            fraDelimitedFileOptions.Location = new Point(8, 12);
            fraDelimitedFileOptions.Name = "fraDelimitedFileOptions";
            fraDelimitedFileOptions.Size = new Size(496, 88);
            fraDelimitedFileOptions.TabIndex = 1;
            fraDelimitedFileOptions.TabStop = false;
            fraDelimitedFileOptions.Text = "Delimited Input File Options";
            // 
            // cboInputFileColumnOrdering
            // 
            cboInputFileColumnOrdering.DropDownStyle = ComboBoxStyle.DropDownList;
            cboInputFileColumnOrdering.DropDownWidth = 70;
            cboInputFileColumnOrdering.Location = new Point(88, 24);
            cboInputFileColumnOrdering.Name = "cboInputFileColumnOrdering";
            cboInputFileColumnOrdering.Size = new Size(392, 21);
            cboInputFileColumnOrdering.TabIndex = 1;
            // 
            // lblInputFileColumnOrdering
            // 
            lblInputFileColumnOrdering.Location = new Point(8, 26);
            lblInputFileColumnOrdering.Name = "lblInputFileColumnOrdering";
            lblInputFileColumnOrdering.Size = new Size(80, 16);
            lblInputFileColumnOrdering.TabIndex = 0;
            lblInputFileColumnOrdering.Text = "Column Order";
            // 
            // txtInputFileColumnDelimiter
            // 
            txtInputFileColumnDelimiter.Location = new Point(192, 56);
            txtInputFileColumnDelimiter.MaxLength = 1;
            txtInputFileColumnDelimiter.Name = "txtInputFileColumnDelimiter";
            txtInputFileColumnDelimiter.Size = new Size(32, 20);
            txtInputFileColumnDelimiter.TabIndex = 4;
            txtInputFileColumnDelimiter.Text = ";";
            // 
            // lblInputFileColumnDelimiter
            // 
            lblInputFileColumnDelimiter.Location = new Point(8, 58);
            lblInputFileColumnDelimiter.Name = "lblInputFileColumnDelimiter";
            lblInputFileColumnDelimiter.Size = new Size(96, 16);
            lblInputFileColumnDelimiter.TabIndex = 2;
            lblInputFileColumnDelimiter.Text = "Column Delimiter";
            // 
            // cboInputFileColumnDelimiter
            // 
            cboInputFileColumnDelimiter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboInputFileColumnDelimiter.DropDownWidth = 70;
            cboInputFileColumnDelimiter.Location = new Point(112, 56);
            cboInputFileColumnDelimiter.Name = "cboInputFileColumnDelimiter";
            cboInputFileColumnDelimiter.Size = new Size(70, 21);
            cboInputFileColumnDelimiter.TabIndex = 3;
            // 
            // TabPageFileFormatOptions
            // 
            TabPageFileFormatOptions.Controls.Add(frapIAndHydrophobicity);
            TabPageFileFormatOptions.Controls.Add(fraDelimitedFileOptions);
            TabPageFileFormatOptions.Location = new Point(4, 22);
            TabPageFileFormatOptions.Name = "TabPageFileFormatOptions";
            TabPageFileFormatOptions.Size = new Size(696, 328);
            TabPageFileFormatOptions.TabIndex = 2;
            TabPageFileFormatOptions.Text = "File Format Options";
            TabPageFileFormatOptions.UseVisualStyleBackColor = true;
            // 
            // tbsOptions
            // 
            tbsOptions.Controls.Add(TabPageFileFormatOptions);
            tbsOptions.Controls.Add(TabPageParseAndDigest);
            tbsOptions.Controls.Add(TabPageUniquenessStats);
            tbsOptions.Controls.Add(TabPagePeakMatchingThresholds);
            tbsOptions.Controls.Add(TabPageProgress);
            tbsOptions.Location = new Point(12, 212);
            tbsOptions.Name = "tbsOptions";
            tbsOptions.SelectedIndex = 0;
            tbsOptions.Size = new Size(704, 354);
            tbsOptions.TabIndex = 5;
            // 
            // TabPageParseAndDigest
            // 
            TabPageParseAndDigest.Controls.Add(fraProcessingOptions);
            TabPageParseAndDigest.Controls.Add(fraCalculationOptions);
            TabPageParseAndDigest.Controls.Add(fraDigestionOptions);
            TabPageParseAndDigest.Controls.Add(cmdParseInputFile);
            TabPageParseAndDigest.Location = new Point(4, 22);
            TabPageParseAndDigest.Name = "TabPageParseAndDigest";
            TabPageParseAndDigest.Size = new Size(696, 328);
            TabPageParseAndDigest.TabIndex = 0;
            TabPageParseAndDigest.Text = "Parse and Digest File Options";
            TabPageParseAndDigest.UseVisualStyleBackColor = true;
            // 
            // fraProcessingOptions
            // 
            fraProcessingOptions.Controls.Add(lblProteinScramblingLoopCount);
            fraProcessingOptions.Controls.Add(txtProteinScramblingLoopCount);
            fraProcessingOptions.Controls.Add(lblSamplingPercentageUnits);
            fraProcessingOptions.Controls.Add(lblProteinReversalSamplingPercentage);
            fraProcessingOptions.Controls.Add(txtProteinReversalSamplingPercentage);
            fraProcessingOptions.Controls.Add(lbltxtAddnlRefAccessionSepChar);
            fraProcessingOptions.Controls.Add(chkLookForAddnlRefInDescription);
            fraProcessingOptions.Controls.Add(cboProteinReversalOptions);
            fraProcessingOptions.Controls.Add(lblProteinReversalOptions);
            fraProcessingOptions.Controls.Add(chkDigestProteins);
            fraProcessingOptions.Controls.Add(lblAddnlRefSepChar);
            fraProcessingOptions.Controls.Add(txtAddnlRefAccessionSepChar);
            fraProcessingOptions.Controls.Add(txtAddnlRefSepChar);
            fraProcessingOptions.Controls.Add(chkCreateFastaOutputFile);
            fraProcessingOptions.Location = new Point(8, 8);
            fraProcessingOptions.Name = "fraProcessingOptions";
            fraProcessingOptions.Size = new Size(360, 176);
            fraProcessingOptions.TabIndex = 0;
            fraProcessingOptions.TabStop = false;
            fraProcessingOptions.Text = "Processing Options";
            // 
            // txtProteinReversalSamplingPercentage
            // 
            txtProteinReversalSamplingPercentage.Location = new Point(168, 40);
            txtProteinReversalSamplingPercentage.MaxLength = 3;
            txtProteinReversalSamplingPercentage.Name = "txtProteinReversalSamplingPercentage";
            txtProteinReversalSamplingPercentage.Size = new Size(32, 20);
            txtProteinReversalSamplingPercentage.TabIndex = 3;
            txtProteinReversalSamplingPercentage.Text = "100";
            // 
            // lbltxtAddnlRefAccessionSepChar
            // 
            lbltxtAddnlRefAccessionSepChar.Location = new Point(96, 96);
            lbltxtAddnlRefAccessionSepChar.Name = "lbltxtAddnlRefAccessionSepChar";
            lbltxtAddnlRefAccessionSepChar.Size = new Size(160, 16);
            lbltxtAddnlRefAccessionSepChar.TabIndex = 8;
            lbltxtAddnlRefAccessionSepChar.Text = "Addnl Ref Accession Sep Char";
            lbltxtAddnlRefAccessionSepChar.TextAlign = ContentAlignment.TopRight;
            // 
            // chkLookForAddnlRefInDescription
            // 
            chkLookForAddnlRefInDescription.Location = new Point(16, 72);
            chkLookForAddnlRefInDescription.Name = "chkLookForAddnlRefInDescription";
            chkLookForAddnlRefInDescription.Size = new Size(120, 32);
            chkLookForAddnlRefInDescription.TabIndex = 5;
            chkLookForAddnlRefInDescription.Text = "Look for addnl Ref in description";
            // 
            // cboProteinReversalOptions
            // 
            cboProteinReversalOptions.DropDownStyle = ComboBoxStyle.DropDownList;
            cboProteinReversalOptions.Location = new Point(168, 16);
            cboProteinReversalOptions.Name = "cboProteinReversalOptions";
            cboProteinReversalOptions.Size = new Size(184, 21);
            cboProteinReversalOptions.TabIndex = 1;
            // 
            // lblProteinReversalOptions
            // 
            lblProteinReversalOptions.Location = new Point(16, 20);
            lblProteinReversalOptions.Name = "lblProteinReversalOptions";
            lblProteinReversalOptions.Size = new Size(160, 16);
            lblProteinReversalOptions.TabIndex = 0;
            lblProteinReversalOptions.Text = "Protein Reversal / Scrambling";
            // 
            // chkDigestProteins
            // 
            chkDigestProteins.Location = new Point(16, 115);
            chkDigestProteins.Name = "chkDigestProteins";
            chkDigestProteins.Size = new Size(160, 32);
            chkDigestProteins.TabIndex = 10;
            chkDigestProteins.Text = "In Silico digest of all proteins in input file";
            // 
            // lblAddnlRefSepChar
            // 
            lblAddnlRefSepChar.Location = new Point(144, 72);
            lblAddnlRefSepChar.Name = "lblAddnlRefSepChar";
            lblAddnlRefSepChar.Size = new Size(112, 16);
            lblAddnlRefSepChar.TabIndex = 6;
            lblAddnlRefSepChar.Text = "Addnl Ref Sep Char";
            lblAddnlRefSepChar.TextAlign = ContentAlignment.TopRight;
            // 
            // txtAddnlRefAccessionSepChar
            // 
            txtAddnlRefAccessionSepChar.Location = new Point(264, 96);
            txtAddnlRefAccessionSepChar.MaxLength = 1;
            txtAddnlRefAccessionSepChar.Name = "txtAddnlRefAccessionSepChar";
            txtAddnlRefAccessionSepChar.Size = new Size(32, 20);
            txtAddnlRefAccessionSepChar.TabIndex = 9;
            txtAddnlRefAccessionSepChar.Text = ":";
            // 
            // txtAddnlRefSepChar
            // 
            txtAddnlRefSepChar.Location = new Point(264, 72);
            txtAddnlRefSepChar.MaxLength = 1;
            txtAddnlRefSepChar.Name = "txtAddnlRefSepChar";
            txtAddnlRefSepChar.Size = new Size(32, 20);
            txtAddnlRefSepChar.TabIndex = 7;
            txtAddnlRefSepChar.Text = "|";
            // 
            // chkCreateFastaOutputFile
            // 
            chkCreateFastaOutputFile.Location = new Point(192, 128);
            chkCreateFastaOutputFile.Name = "chkCreateFastaOutputFile";
            chkCreateFastaOutputFile.Size = new Size(160, 16);
            chkCreateFastaOutputFile.TabIndex = 11;
            chkCreateFastaOutputFile.Text = "Create FASTA Output File";
            // 
            // fraCalculationOptions
            // 
            fraCalculationOptions.Controls.Add(cmdNETInfo);
            fraCalculationOptions.Controls.Add(chkExcludeProteinDescription);
            fraCalculationOptions.Controls.Add(chkComputeSequenceHashIgnoreILDiff);
            fraCalculationOptions.Controls.Add(chkTruncateProteinDescription);
            fraCalculationOptions.Controls.Add(chkComputeSequenceHashValues);
            fraCalculationOptions.Controls.Add(lblMassMode);
            fraCalculationOptions.Controls.Add(cboElementMassMode);
            fraCalculationOptions.Controls.Add(chkExcludeProteinSequence);
            fraCalculationOptions.Controls.Add(chkComputepIandNET);
            fraCalculationOptions.Controls.Add(chkIncludeXResidues);
            fraCalculationOptions.Controls.Add(chkComputeProteinMass);
            fraCalculationOptions.Location = new Point(376, 40);
            fraCalculationOptions.Name = "fraCalculationOptions";
            fraCalculationOptions.Size = new Size(308, 150);
            fraCalculationOptions.TabIndex = 1;
            fraCalculationOptions.TabStop = false;
            fraCalculationOptions.Text = "Calculation Options";
            // 
            // cmdNETInfo
            // 
            cmdNETInfo.Location = new Point(268, 87);
            cmdNETInfo.Margin = new Padding(1);
            cmdNETInfo.Name = "cmdNETInfo";
            cmdNETInfo.Size = new Size(34, 20);
            cmdNETInfo.TabIndex = 4;
            cmdNETInfo.Text = "Info";
            // 
            // chkExcludeProteinDescription
            // 
            chkExcludeProteinDescription.Location = new Point(185, 128);
            chkExcludeProteinDescription.Name = "chkExcludeProteinDescription";
            chkExcludeProteinDescription.Size = new Size(120, 19);
            chkExcludeProteinDescription.TabIndex = 9;
            chkExcludeProteinDescription.Text = "Exclude Description";
            // 
            // chkComputeSequenceHashIgnoreILDiff
            // 
            chkComputeSequenceHashIgnoreILDiff.Checked = true;
            chkComputeSequenceHashIgnoreILDiff.CheckState = CheckState.Checked;
            chkComputeSequenceHashIgnoreILDiff.Location = new Point(185, 107);
            chkComputeSequenceHashIgnoreILDiff.Name = "chkComputeSequenceHashIgnoreILDiff";
            chkComputeSequenceHashIgnoreILDiff.Size = new Size(104, 19);
            chkComputeSequenceHashIgnoreILDiff.TabIndex = 8;
            chkComputeSequenceHashIgnoreILDiff.Text = "Ignore I/L Diff";
            // 
            // chkTruncateProteinDescription
            // 
            chkTruncateProteinDescription.Checked = true;
            chkTruncateProteinDescription.CheckState = CheckState.Checked;
            chkTruncateProteinDescription.Location = new Point(16, 128);
            chkTruncateProteinDescription.Name = "chkTruncateProteinDescription";
            chkTruncateProteinDescription.Size = new Size(164, 19);
            chkTruncateProteinDescription.TabIndex = 7;
            chkTruncateProteinDescription.Text = "Truncate long description";
            // 
            // chkComputeSequenceHashValues
            // 
            chkComputeSequenceHashValues.Checked = true;
            chkComputeSequenceHashValues.CheckState = CheckState.Checked;
            chkComputeSequenceHashValues.Location = new Point(16, 107);
            chkComputeSequenceHashValues.Name = "chkComputeSequenceHashValues";
            chkComputeSequenceHashValues.Size = new Size(164, 19);
            chkComputeSequenceHashValues.TabIndex = 6;
            chkComputeSequenceHashValues.Text = "Compute sequence hashes";
            // 
            // lblMassMode
            // 
            lblMassMode.Location = new Point(16, 66);
            lblMassMode.Name = "lblMassMode";
            lblMassMode.Size = new Size(64, 16);
            lblMassMode.TabIndex = 5;
            lblMassMode.Text = "Mass type";
            lblMassMode.TextAlign = ContentAlignment.TopRight;
            // 
            // cboElementMassMode
            // 
            cboElementMassMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboElementMassMode.Location = new Point(88, 65);
            cboElementMassMode.Name = "cboElementMassMode";
            cboElementMassMode.Size = new Size(144, 21);
            cboElementMassMode.TabIndex = 4;
            // 
            // chkExcludeProteinSequence
            // 
            chkExcludeProteinSequence.Location = new Point(16, 16);
            chkExcludeProteinSequence.Name = "chkExcludeProteinSequence";
            chkExcludeProteinSequence.Size = new Size(192, 16);
            chkExcludeProteinSequence.TabIndex = 0;
            chkExcludeProteinSequence.Text = "Exclude Protein Sequence";
            // 
            // chkComputepIandNET
            // 
            chkComputepIandNET.Location = new Point(16, 89);
            chkComputepIandNET.Name = "chkComputepIandNET";
            chkComputepIandNET.Size = new Size(252, 18);
            chkComputepIandNET.TabIndex = 3;
            chkComputepIandNET.Text = "Compute pI and Normalized Elution Time (NET)";
            // 
            // chkIncludeXResidues
            // 
            chkIncludeXResidues.Location = new Point(16, 49);
            chkIncludeXResidues.Name = "chkIncludeXResidues";
            chkIncludeXResidues.Size = new Size(216, 16);
            chkIncludeXResidues.TabIndex = 2;
            chkIncludeXResidues.Text = "Include X Residues in Mass (113 Da)";
            // 
            // chkComputeProteinMass
            // 
            chkComputeProteinMass.Location = new Point(16, 33);
            chkComputeProteinMass.Name = "chkComputeProteinMass";
            chkComputeProteinMass.Size = new Size(144, 16);
            chkComputeProteinMass.TabIndex = 1;
            chkComputeProteinMass.Text = "Compute Protein Mass";
            // 
            // fraDigestionOptions
            // 
            fraDigestionOptions.Controls.Add(cboFragmentMassMode);
            fraDigestionOptions.Controls.Add(lblFragmentMassMode);
            fraDigestionOptions.Controls.Add(cboCysTreatmentMode);
            fraDigestionOptions.Controls.Add(lblCysTreatment);
            fraDigestionOptions.Controls.Add(txtDigestProteinsMaximumpI);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMaximumpI);
            fraDigestionOptions.Controls.Add(txtDigestProteinsMinimumpI);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMinimumpI);
            fraDigestionOptions.Controls.Add(chkGenerateUniqueIDValues);
            fraDigestionOptions.Controls.Add(chkCysPeptidesOnly);
            fraDigestionOptions.Controls.Add(txtDigestProteinsMinimumResidueCount);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMinimumResidueCount);
            fraDigestionOptions.Controls.Add(txtDigestProteinsMaximumMissedCleavages);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMaximumMissedCleavages);
            fraDigestionOptions.Controls.Add(txtDigestProteinsMaximumMass);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMaximumMass);
            fraDigestionOptions.Controls.Add(txtDigestProteinsMinimumMass);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMinimumMass);
            fraDigestionOptions.Controls.Add(cboCleavageRuleType);
            fraDigestionOptions.Controls.Add(chkIncludeDuplicateSequences);
            fraDigestionOptions.Location = new Point(8, 190);
            fraDigestionOptions.Name = "fraDigestionOptions";
            fraDigestionOptions.Size = new Size(675, 128);
            fraDigestionOptions.TabIndex = 2;
            fraDigestionOptions.TabStop = false;
            fraDigestionOptions.Text = "Digestion Options";
            // 
            // cboFragmentMassMode
            // 
            cboFragmentMassMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFragmentMassMode.Location = new Point(75, 104);
            cboFragmentMassMode.Name = "cboFragmentMassMode";
            cboFragmentMassMode.Size = new Size(117, 21);
            cboFragmentMassMode.TabIndex = 19;
            // 
            // lblFragmentMassMode
            // 
            lblFragmentMassMode.Location = new Point(8, 106);
            lblFragmentMassMode.Name = "lblFragmentMassMode";
            lblFragmentMassMode.Size = new Size(68, 16);
            lblFragmentMassMode.TabIndex = 18;
            lblFragmentMassMode.Text = "Mass Mode";
            // 
            // cboCysTreatmentMode
            // 
            cboCysTreatmentMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCysTreatmentMode.Location = new Point(553, 76);
            cboCysTreatmentMode.Name = "cboCysTreatmentMode";
            cboCysTreatmentMode.Size = new Size(117, 21);
            cboCysTreatmentMode.TabIndex = 17;
            // 
            // lblCysTreatment
            // 
            lblCysTreatment.Location = new Point(553, 56);
            lblCysTreatment.Name = "lblCysTreatment";
            lblCysTreatment.Size = new Size(104, 21);
            lblCysTreatment.TabIndex = 16;
            lblCysTreatment.Text = "Cys treatment:";
            // 
            // txtDigestProteinsMaximumpI
            // 
            txtDigestProteinsMaximumpI.Location = new Point(497, 80);
            txtDigestProteinsMaximumpI.Name = "txtDigestProteinsMaximumpI";
            txtDigestProteinsMaximumpI.Size = new Size(40, 20);
            txtDigestProteinsMaximumpI.TabIndex = 13;
            txtDigestProteinsMaximumpI.Text = "14";
            // 
            // lblDigestProteinsMaximumpI
            // 
            lblDigestProteinsMaximumpI.Location = new Point(420, 80);
            lblDigestProteinsMaximumpI.Name = "lblDigestProteinsMaximumpI";
            lblDigestProteinsMaximumpI.Size = new Size(72, 16);
            lblDigestProteinsMaximumpI.TabIndex = 12;
            lblDigestProteinsMaximumpI.Text = "Maximum pI";
            // 
            // txtDigestProteinsMinimumpI
            // 
            txtDigestProteinsMinimumpI.Location = new Point(497, 56);
            txtDigestProteinsMinimumpI.Name = "txtDigestProteinsMinimumpI";
            txtDigestProteinsMinimumpI.Size = new Size(40, 20);
            txtDigestProteinsMinimumpI.TabIndex = 11;
            txtDigestProteinsMinimumpI.Text = "0";
            // 
            // lblDigestProteinsMinimumpI
            // 
            lblDigestProteinsMinimumpI.Location = new Point(420, 56);
            lblDigestProteinsMinimumpI.Name = "lblDigestProteinsMinimumpI";
            lblDigestProteinsMinimumpI.Size = new Size(72, 16);
            lblDigestProteinsMinimumpI.TabIndex = 10;
            lblDigestProteinsMinimumpI.Text = "Minimum pI";
            // 
            // chkGenerateUniqueIDValues
            // 
            chkGenerateUniqueIDValues.Checked = true;
            chkGenerateUniqueIDValues.CheckState = CheckState.Checked;
            chkGenerateUniqueIDValues.Location = new Point(218, 102);
            chkGenerateUniqueIDValues.Name = "chkGenerateUniqueIDValues";
            chkGenerateUniqueIDValues.Size = new Size(176, 16);
            chkGenerateUniqueIDValues.TabIndex = 14;
            chkGenerateUniqueIDValues.Text = "Generate UniqueID Values";
            // 
            // chkCysPeptidesOnly
            // 
            chkCysPeptidesOnly.Location = new Point(486, 16);
            chkCysPeptidesOnly.Name = "chkCysPeptidesOnly";
            chkCysPeptidesOnly.Size = new Size(112, 32);
            chkCysPeptidesOnly.TabIndex = 15;
            chkCysPeptidesOnly.Text = "Include cysteine peptides only";
            // 
            // txtDigestProteinsMinimumResidueCount
            // 
            txtDigestProteinsMinimumResidueCount.Location = new Point(359, 56);
            txtDigestProteinsMinimumResidueCount.Name = "txtDigestProteinsMinimumResidueCount";
            txtDigestProteinsMinimumResidueCount.Size = new Size(32, 20);
            txtDigestProteinsMinimumResidueCount.TabIndex = 7;
            txtDigestProteinsMinimumResidueCount.Text = "0";
            // 
            // lblDigestProteinsMinimumResidueCount
            // 
            lblDigestProteinsMinimumResidueCount.Location = new Point(216, 58);
            lblDigestProteinsMinimumResidueCount.Name = "lblDigestProteinsMinimumResidueCount";
            lblDigestProteinsMinimumResidueCount.Size = new Size(136, 16);
            lblDigestProteinsMinimumResidueCount.TabIndex = 6;
            lblDigestProteinsMinimumResidueCount.Text = "Minimum Residue Count";
            // 
            // txtDigestProteinsMaximumMissedCleavages
            // 
            txtDigestProteinsMaximumMissedCleavages.Location = new Point(359, 80);
            txtDigestProteinsMaximumMissedCleavages.Name = "txtDigestProteinsMaximumMissedCleavages";
            txtDigestProteinsMaximumMissedCleavages.Size = new Size(32, 20);
            txtDigestProteinsMaximumMissedCleavages.TabIndex = 9;
            txtDigestProteinsMaximumMissedCleavages.Text = "3";
            // 
            // lblDigestProteinsMaximumMissedCleavages
            // 
            lblDigestProteinsMaximumMissedCleavages.Location = new Point(216, 82);
            lblDigestProteinsMaximumMissedCleavages.Name = "lblDigestProteinsMaximumMissedCleavages";
            lblDigestProteinsMaximumMissedCleavages.Size = new Size(136, 16);
            lblDigestProteinsMaximumMissedCleavages.TabIndex = 8;
            lblDigestProteinsMaximumMissedCleavages.Text = "Max Missed Cleavages";
            // 
            // txtDigestProteinsMaximumMass
            // 
            txtDigestProteinsMaximumMass.Location = new Point(152, 80);
            txtDigestProteinsMaximumMass.Name = "txtDigestProteinsMaximumMass";
            txtDigestProteinsMaximumMass.Size = new Size(40, 20);
            txtDigestProteinsMaximumMass.TabIndex = 5;
            txtDigestProteinsMaximumMass.Text = "6000";
            // 
            // lblDigestProteinsMaximumMass
            // 
            lblDigestProteinsMaximumMass.Location = new Point(8, 82);
            lblDigestProteinsMaximumMass.Name = "lblDigestProteinsMaximumMass";
            lblDigestProteinsMaximumMass.Size = new Size(144, 16);
            lblDigestProteinsMaximumMass.TabIndex = 4;
            lblDigestProteinsMaximumMass.Text = "Maximum Fragment Mass";
            // 
            // txtDigestProteinsMinimumMass
            // 
            txtDigestProteinsMinimumMass.Location = new Point(152, 56);
            txtDigestProteinsMinimumMass.Name = "txtDigestProteinsMinimumMass";
            txtDigestProteinsMinimumMass.Size = new Size(40, 20);
            txtDigestProteinsMinimumMass.TabIndex = 3;
            txtDigestProteinsMinimumMass.Text = "400";
            // 
            // lblDigestProteinsMinimumMass
            // 
            lblDigestProteinsMinimumMass.Location = new Point(8, 58);
            lblDigestProteinsMinimumMass.Name = "lblDigestProteinsMinimumMass";
            lblDigestProteinsMinimumMass.Size = new Size(144, 16);
            lblDigestProteinsMinimumMass.TabIndex = 2;
            lblDigestProteinsMinimumMass.Text = "Minimum Fragment Mass";
            // 
            // cboCleavageRuleType
            // 
            cboCleavageRuleType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCleavageRuleType.Location = new Point(8, 24);
            cboCleavageRuleType.Name = "cboCleavageRuleType";
            cboCleavageRuleType.Size = new Size(288, 21);
            cboCleavageRuleType.TabIndex = 0;
            // 
            // chkIncludeDuplicateSequences
            // 
            chkIncludeDuplicateSequences.Location = new Point(312, 16);
            chkIncludeDuplicateSequences.Name = "chkIncludeDuplicateSequences";
            chkIncludeDuplicateSequences.Size = new Size(168, 32);
            chkIncludeDuplicateSequences.TabIndex = 1;
            chkIncludeDuplicateSequences.Text = "Include duplicate sequences for given protein";
            // 
            // cmdParseInputFile
            // 
            cmdParseInputFile.Location = new Point(384, 8);
            cmdParseInputFile.Name = "cmdParseInputFile";
            cmdParseInputFile.Size = new Size(112, 24);
            cmdParseInputFile.TabIndex = 3;
            cmdParseInputFile.Text = "&Parse and Digest";
            // 
            // TabPagePeakMatchingThresholds
            // 
            TabPagePeakMatchingThresholds.Controls.Add(chkAutoDefineSLiCScoreTolerances);
            TabPagePeakMatchingThresholds.Controls.Add(cmdPastePMThresholdsList);
            TabPagePeakMatchingThresholds.Controls.Add(cboPMPredefinedThresholds);
            TabPagePeakMatchingThresholds.Controls.Add(cmdPMThresholdsAutoPopulate);
            TabPagePeakMatchingThresholds.Controls.Add(cmdClearPMThresholdsList);
            TabPagePeakMatchingThresholds.Controls.Add(cboMassTolType);
            TabPagePeakMatchingThresholds.Controls.Add(lblMassTolType);
            TabPagePeakMatchingThresholds.Controls.Add(dgPeakMatchingThresholds);
            TabPagePeakMatchingThresholds.Location = new Point(4, 22);
            TabPagePeakMatchingThresholds.Name = "TabPagePeakMatchingThresholds";
            TabPagePeakMatchingThresholds.Size = new Size(696, 328);
            TabPagePeakMatchingThresholds.TabIndex = 3;
            TabPagePeakMatchingThresholds.Text = "Peak Matching Thresholds";
            TabPagePeakMatchingThresholds.UseVisualStyleBackColor = true;
            // 
            // chkAutoDefineSLiCScoreTolerances
            // 
            chkAutoDefineSLiCScoreTolerances.Checked = true;
            chkAutoDefineSLiCScoreTolerances.CheckState = CheckState.Checked;
            chkAutoDefineSLiCScoreTolerances.Location = new Point(16, 256);
            chkAutoDefineSLiCScoreTolerances.Name = "chkAutoDefineSLiCScoreTolerances";
            chkAutoDefineSLiCScoreTolerances.Size = new Size(208, 16);
            chkAutoDefineSLiCScoreTolerances.TabIndex = 3;
            chkAutoDefineSLiCScoreTolerances.Text = "Auto Define SLiC Score Tolerances";
            // 
            // cmdPastePMThresholdsList
            // 
            cmdPastePMThresholdsList.Location = new Point(456, 96);
            cmdPastePMThresholdsList.Name = "cmdPastePMThresholdsList";
            cmdPastePMThresholdsList.Size = new Size(104, 24);
            cmdPastePMThresholdsList.TabIndex = 6;
            cmdPastePMThresholdsList.Text = "Paste Values";
            // 
            // cboPMPredefinedThresholds
            // 
            cboPMPredefinedThresholds.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPMPredefinedThresholds.Location = new Point(336, 256);
            cboPMPredefinedThresholds.Name = "cboPMPredefinedThresholds";
            cboPMPredefinedThresholds.Size = new Size(264, 21);
            cboPMPredefinedThresholds.TabIndex = 5;
            // 
            // cmdPMThresholdsAutoPopulate
            // 
            cmdPMThresholdsAutoPopulate.Location = new Point(336, 224);
            cmdPMThresholdsAutoPopulate.Name = "cmdPMThresholdsAutoPopulate";
            cmdPMThresholdsAutoPopulate.Size = new Size(104, 24);
            cmdPMThresholdsAutoPopulate.TabIndex = 4;
            cmdPMThresholdsAutoPopulate.Text = "Auto-Populate";
            // 
            // cmdClearPMThresholdsList
            // 
            cmdClearPMThresholdsList.Location = new Point(456, 128);
            cmdClearPMThresholdsList.Name = "cmdClearPMThresholdsList";
            cmdClearPMThresholdsList.Size = new Size(104, 24);
            cmdClearPMThresholdsList.TabIndex = 7;
            cmdClearPMThresholdsList.Text = "Clear List";
            // 
            // cboMassTolType
            // 
            cboMassTolType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMassTolType.Location = new Point(144, 224);
            cboMassTolType.Name = "cboMassTolType";
            cboMassTolType.Size = new Size(136, 21);
            cboMassTolType.TabIndex = 2;
            // 
            // lblMassTolType
            // 
            lblMassTolType.Location = new Point(16, 226);
            lblMassTolType.Name = "lblMassTolType";
            lblMassTolType.Size = new Size(136, 16);
            lblMassTolType.TabIndex = 1;
            lblMassTolType.Text = "Mass Tolerance Type";
            // 
            // dgPeakMatchingThresholds
            // 
            dgPeakMatchingThresholds.CaptionText = "Peak Matching Thresholds";
            dgPeakMatchingThresholds.DataMember = "";
            dgPeakMatchingThresholds.HeaderForeColor = SystemColors.ControlText;
            dgPeakMatchingThresholds.Location = new Point(16, 8);
            dgPeakMatchingThresholds.Name = "dgPeakMatchingThresholds";
            dgPeakMatchingThresholds.Size = new Size(424, 208);
            dgPeakMatchingThresholds.TabIndex = 0;
            // 
            // TabPageProgress
            // 
            TabPageProgress.Controls.Add(pbarProgress);
            TabPageProgress.Controls.Add(lblErrorMessage);
            TabPageProgress.Controls.Add(lblSubtaskProgress);
            TabPageProgress.Controls.Add(lblProgress);
            TabPageProgress.Controls.Add(lblSubtaskProgressDescription);
            TabPageProgress.Controls.Add(lblProgressDescription);
            TabPageProgress.Controls.Add(cmdAbortProcessing);
            TabPageProgress.Location = new Point(4, 22);
            TabPageProgress.Name = "TabPageProgress";
            TabPageProgress.Size = new Size(696, 328);
            TabPageProgress.TabIndex = 4;
            TabPageProgress.Text = "Progress";
            TabPageProgress.UseVisualStyleBackColor = true;
            // 
            // pbarProgress
            // 
            pbarProgress.Location = new Point(13, 12);
            pbarProgress.Name = "pbarProgress";
            pbarProgress.Size = new Size(122, 20);
            pbarProgress.TabIndex = 12;
            // 
            // lblErrorMessage
            // 
            lblErrorMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblErrorMessage.Location = new Point(137, 112);
            lblErrorMessage.Name = "lblErrorMessage";
            lblErrorMessage.Size = new Size(515, 32);
            lblErrorMessage.TabIndex = 11;
            lblErrorMessage.Text = "Error message ...";
            lblErrorMessage.Visible = false;
            // 
            // lblSubtaskProgress
            // 
            lblSubtaskProgress.Location = new Point(13, 61);
            lblSubtaskProgress.Name = "lblSubtaskProgress";
            lblSubtaskProgress.Size = new Size(118, 18);
            lblSubtaskProgress.TabIndex = 8;
            lblSubtaskProgress.Text = "0";
            lblSubtaskProgress.Visible = false;
            // 
            // lblProgress
            // 
            lblProgress.Location = new Point(13, 35);
            lblProgress.Name = "lblProgress";
            lblProgress.Size = new Size(118, 18);
            lblProgress.TabIndex = 7;
            lblProgress.Text = "0";
            // 
            // lblSubtaskProgressDescription
            // 
            lblSubtaskProgressDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblSubtaskProgressDescription.Location = new Point(140, 61);
            lblSubtaskProgressDescription.Name = "lblSubtaskProgressDescription";
            lblSubtaskProgressDescription.Size = new Size(515, 32);
            lblSubtaskProgressDescription.TabIndex = 6;
            lblSubtaskProgressDescription.Text = "Subtask progress description ...";
            lblSubtaskProgressDescription.Visible = false;
            // 
            // lblProgressDescription
            // 
            lblProgressDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblProgressDescription.Location = new Point(140, 12);
            lblProgressDescription.Name = "lblProgressDescription";
            lblProgressDescription.Size = new Size(515, 32);
            lblProgressDescription.TabIndex = 5;
            lblProgressDescription.Text = "Progress description ...";
            // 
            // cmdAbortProcessing
            // 
            cmdAbortProcessing.Location = new Point(10, 106);
            cmdAbortProcessing.Name = "cmdAbortProcessing";
            cmdAbortProcessing.Size = new Size(121, 24);
            cmdAbortProcessing.TabIndex = 4;
            cmdAbortProcessing.Text = "Abort Processing";
            // 
            // mnuHelpAboutElutionTime
            // 
            mnuHelpAboutElutionTime.Index = 1;
            mnuHelpAboutElutionTime.Text = "About &Elution Time Prediction";
            // 
            // cboInputFileFormat
            // 
            cboInputFileFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            cboInputFileFormat.Location = new Point(112, 56);
            cboInputFileFormat.Name = "cboInputFileFormat";
            cboInputFileFormat.Size = new Size(112, 21);
            cboInputFileFormat.TabIndex = 3;
            // 
            // lblInputFileFormat
            // 
            lblInputFileFormat.Location = new Point(8, 58);
            lblInputFileFormat.Name = "lblInputFileFormat";
            lblInputFileFormat.Size = new Size(104, 16);
            lblInputFileFormat.TabIndex = 2;
            lblInputFileFormat.Text = "Input File Format";
            // 
            // cmdSelectFile
            // 
            cmdSelectFile.Location = new Point(8, 24);
            cmdSelectFile.Name = "cmdSelectFile";
            cmdSelectFile.Size = new Size(80, 24);
            cmdSelectFile.TabIndex = 0;
            cmdSelectFile.Text = "&Select file";
            // 
            // fraInputFilePath
            // 
            fraInputFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            fraInputFilePath.Controls.Add(cmdValidateFastaFile);
            fraInputFilePath.Controls.Add(cboInputFileFormat);
            fraInputFilePath.Controls.Add(lblInputFileFormat);
            fraInputFilePath.Controls.Add(cmdSelectFile);
            fraInputFilePath.Controls.Add(txtProteinInputFilePath);
            fraInputFilePath.Location = new Point(12, 12);
            fraInputFilePath.Name = "fraInputFilePath";
            fraInputFilePath.Size = new Size(730, 88);
            fraInputFilePath.TabIndex = 3;
            fraInputFilePath.TabStop = false;
            fraInputFilePath.Text = "Protein Input File Path (FASTA or Tab-delimited)";
            // 
            // cmdValidateFastaFile
            // 
            cmdValidateFastaFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmdValidateFastaFile.Location = new Point(595, 56);
            cmdValidateFastaFile.Name = "cmdValidateFastaFile";
            cmdValidateFastaFile.Size = new Size(120, 24);
            cmdValidateFastaFile.TabIndex = 4;
            cmdValidateFastaFile.Text = "&Validate FASTA File";
            // 
            // chkEnableLogging
            // 
            chkEnableLogging.Checked = true;
            chkEnableLogging.CheckState = CheckState.Checked;
            chkEnableLogging.Location = new Point(422, 19);
            chkEnableLogging.Name = "chkEnableLogging";
            chkEnableLogging.Size = new Size(112, 24);
            chkEnableLogging.TabIndex = 4;
            chkEnableLogging.Text = "Enable logging";
            // 
            // mnuFileSelectOutputFile
            // 
            mnuFileSelectOutputFile.Index = 1;
            mnuFileSelectOutputFile.Text = "Select &Output File...";
            // 
            // cmdSelectOutputFile
            // 
            cmdSelectOutputFile.Location = new Point(8, 56);
            cmdSelectOutputFile.Name = "cmdSelectOutputFile";
            cmdSelectOutputFile.Size = new Size(88, 33);
            cmdSelectOutputFile.TabIndex = 5;
            cmdSelectOutputFile.Text = "Select / &Create File";
            // 
            // mnuFileSep1
            // 
            mnuFileSep1.Index = 2;
            mnuFileSep1.Text = "-";
            // 
            // mnuFile
            // 
            mnuFile.Index = 0;
            mnuFile.MenuItems.AddRange(new MenuItem[] { mnuFileSelectInputFile, mnuFileSelectOutputFile, mnuFileSep1, mnuFileSaveDefaultOptions, mnuFileSep2, mnuFileExit });
            mnuFile.Text = "&File";
            // 
            // mnuFileSelectInputFile
            // 
            mnuFileSelectInputFile.Index = 0;
            mnuFileSelectInputFile.Text = "Select &Input File...";
            // 
            // mnuFileSaveDefaultOptions
            // 
            mnuFileSaveDefaultOptions.Index = 3;
            mnuFileSaveDefaultOptions.Text = "Save &Default Options";
            // 
            // mnuFileSep2
            // 
            mnuFileSep2.Index = 4;
            mnuFileSep2.Text = "-";
            // 
            // mnuFileExit
            // 
            mnuFileExit.Index = 5;
            mnuFileExit.Text = "E&xit";
            // 
            // txtProteinOutputFilePath
            // 
            txtProteinOutputFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtProteinOutputFilePath.Location = new Point(104, 62);
            txtProteinOutputFilePath.Name = "txtProteinOutputFilePath";
            txtProteinOutputFilePath.Size = new Size(611, 20);
            txtProteinOutputFilePath.TabIndex = 6;
            // 
            // chkIncludePrefixAndSuffixResidues
            // 
            chkIncludePrefixAndSuffixResidues.Location = new Point(256, 16);
            chkIncludePrefixAndSuffixResidues.Name = "chkIncludePrefixAndSuffixResidues";
            chkIncludePrefixAndSuffixResidues.Size = new Size(160, 32);
            chkIncludePrefixAndSuffixResidues.TabIndex = 3;
            chkIncludePrefixAndSuffixResidues.Text = "Include prefix and suffix residues for the sequences";
            // 
            // mnuEditResetOptions
            // 
            mnuEditResetOptions.Index = 3;
            mnuEditResetOptions.Text = "&Reset options to Defaults...";
            // 
            // mnuHelp
            // 
            mnuHelp.Index = 2;
            mnuHelp.MenuItems.AddRange(new MenuItem[] { mnuHelpAbout, mnuHelpAboutElutionTime });
            mnuHelp.Text = "&Help";
            // 
            // mnuHelpAbout
            // 
            mnuHelpAbout.Index = 0;
            mnuHelpAbout.Text = "&About";
            // 
            // mnuEditSep1
            // 
            mnuEditSep1.Index = 2;
            mnuEditSep1.Text = "-";
            // 
            // mnuEditMakeUniquenessStats
            // 
            mnuEditMakeUniquenessStats.Index = 1;
            mnuEditMakeUniquenessStats.Text = "&Make Uniqueness Stats";
            // 
            // mnuEdit
            // 
            mnuEdit.Index = 1;
            mnuEdit.MenuItems.AddRange(new MenuItem[] { mnuEditParseFile, mnuEditMakeUniquenessStats, mnuEditSep1, mnuEditResetOptions });
            mnuEdit.Text = "&Edit";
            // 
            // mnuEditParseFile
            // 
            mnuEditParseFile.Index = 0;
            mnuEditParseFile.Text = "&Parse File";
            // 
            // MainMenuControl
            // 
            MainMenuControl.MenuItems.AddRange(new MenuItem[] { mnuFile, mnuEdit, mnuHelp });
            // 
            // lblOutputFileFieldDelimiter
            // 
            lblOutputFileFieldDelimiter.Location = new Point(8, 26);
            lblOutputFileFieldDelimiter.Name = "lblOutputFileFieldDelimiter";
            lblOutputFileFieldDelimiter.Size = new Size(112, 18);
            lblOutputFileFieldDelimiter.TabIndex = 0;
            lblOutputFileFieldDelimiter.Text = "Field delimiter";
            // 
            // cboOutputFileFieldDelimiter
            // 
            cboOutputFileFieldDelimiter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOutputFileFieldDelimiter.Location = new Point(128, 24);
            cboOutputFileFieldDelimiter.Name = "cboOutputFileFieldDelimiter";
            cboOutputFileFieldDelimiter.Size = new Size(70, 21);
            cboOutputFileFieldDelimiter.TabIndex = 1;
            // 
            // txtOutputFileFieldDelimiter
            // 
            txtOutputFileFieldDelimiter.Location = new Point(208, 24);
            txtOutputFileFieldDelimiter.MaxLength = 1;
            txtOutputFileFieldDelimiter.Name = "txtOutputFileFieldDelimiter";
            txtOutputFileFieldDelimiter.Size = new Size(32, 20);
            txtOutputFileFieldDelimiter.TabIndex = 2;
            txtOutputFileFieldDelimiter.Text = ";";
            // 
            // fraOutputTextOptions
            // 
            fraOutputTextOptions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            fraOutputTextOptions.Controls.Add(chkEnableLogging);
            fraOutputTextOptions.Controls.Add(cmdSelectOutputFile);
            fraOutputTextOptions.Controls.Add(txtProteinOutputFilePath);
            fraOutputTextOptions.Controls.Add(chkIncludePrefixAndSuffixResidues);
            fraOutputTextOptions.Controls.Add(cboOutputFileFieldDelimiter);
            fraOutputTextOptions.Controls.Add(txtOutputFileFieldDelimiter);
            fraOutputTextOptions.Controls.Add(lblOutputFileFieldDelimiter);
            fraOutputTextOptions.Location = new Point(12, 108);
            fraOutputTextOptions.Name = "fraOutputTextOptions";
            fraOutputTextOptions.Size = new Size(730, 96);
            fraOutputTextOptions.TabIndex = 4;
            fraOutputTextOptions.TabStop = false;
            fraOutputTextOptions.Text = "Output Options";
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(6.0f, 13.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(754, 577);
            Controls.Add(tbsOptions);
            Controls.Add(fraInputFilePath);
            Controls.Add(fraOutputTextOptions);
            Menu = MainMenuControl;
            Name = "frmMain";
            Text = "Protein Digestion Simulator";
            fraPeakMatchingOptions.ResumeLayout(false);
            fraPeakMatchingOptions.PerformLayout();
            TabPageUniquenessStats.ResumeLayout(false);
            fraSqlServerOptions.ResumeLayout(false);
            fraSqlServerOptions.PerformLayout();
            fraUniquenessBinningOptions.ResumeLayout(false);
            fraUniquenessBinningOptions.PerformLayout();
            frapIAndHydrophobicity.ResumeLayout(false);
            frapIAndHydrophobicity.PerformLayout();
            fraDelimitedFileOptions.ResumeLayout(false);
            fraDelimitedFileOptions.PerformLayout();
            TabPageFileFormatOptions.ResumeLayout(false);
            tbsOptions.ResumeLayout(false);
            TabPageParseAndDigest.ResumeLayout(false);
            fraProcessingOptions.ResumeLayout(false);
            fraProcessingOptions.PerformLayout();
            fraCalculationOptions.ResumeLayout(false);
            fraDigestionOptions.ResumeLayout(false);
            fraDigestionOptions.PerformLayout();
            TabPagePeakMatchingThresholds.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgPeakMatchingThresholds).EndInit();
            TabPageProgress.ResumeLayout(false);
            fraInputFilePath.ResumeLayout(false);
            fraInputFilePath.PerformLayout();
            fraOutputTextOptions.ResumeLayout(false);
            fraOutputTextOptions.PerformLayout();
            Load += new EventHandler(frmMain_Load);
            Closing += new System.ComponentModel.CancelEventHandler(frmMain_Closing);
            ResumeLayout(false);
        }

        internal TextBox txtProteinInputFilePath;
        internal RadioButton optUseRectangleSearchRegion;
        internal RadioButton optUseEllipseSearchRegion;
        internal Label lblUniquenessCalculationsNote;
        internal Label lblProteinScramblingLoopCount;
        internal GroupBox fraPeakMatchingOptions;
        internal TextBox txtMaxPeakMatchingResultsPerFeatureToSave;
        internal Label lblMaxPeakMatchingResultsPerFeatureToSave;
        internal CheckBox chkExportPeakMatchingResults;
        internal TextBox txtMinimumSLiCScore;
        internal Label lblMinimumSLiCScore;
        internal CheckBox chkUseSLiCScoreForUniqueness;
        internal TabPage TabPageUniquenessStats;
        internal GroupBox fraSqlServerOptions;
        internal CheckBox chkSqlServerUseExistingData;
        internal CheckBox chkAllowSqlServerCaching;
        internal Label lblSqlServerPassword;
        internal Label lblSqlServerUsername;
        internal TextBox txtSqlServerPassword;
        internal TextBox txtSqlServerUsername;
        internal Label lblSqlServerDatabase;
        internal Label lblSqlServerServerName;
        internal CheckBox chkSqlServerUseIntegratedSecurity;
        internal TextBox txtSqlServerDatabase;
        internal TextBox txtSqlServerName;
        internal CheckBox chkUseSqlServerDBToCacheData;
        internal GroupBox fraUniquenessBinningOptions;
        internal Label lblPeptideUniquenessMassMode;
        internal TextBox txtUniquenessBinWidth;
        internal Label lblUniquenessBinWidth;
        internal CheckBox chkAutoComputeRangeForBinning;
        internal TextBox txtUniquenessBinEndMass;
        internal Label lblUniquenessBinEndMass;
        internal TextBox txtUniquenessBinStartMass;
        internal Label lblUniquenessBinStartMass;
        internal Label lblUniquenessStatsNote;
        internal Button cmdGenerateUniquenessStats;
        internal CheckBox chkAssumeInputFileIsDigested;
        internal TextBox txtProteinScramblingLoopCount;
        internal Label lblSamplingPercentageUnits;
        internal TextBox txtMaxpISequenceLength;
        internal Label lblProteinReversalSamplingPercentage;
        internal Label lblMaxpISequenceLength;
        internal CheckBox chkMaxpIModeEnabled;
        internal GroupBox frapIAndHydrophobicity;
        internal Label lblHydrophobicityMode;
        internal ComboBox cboHydrophobicityMode;
        internal TextBox txtpIStats;
        internal TextBox txtSequenceForpI;
        internal Label lblSequenceForpI;
        internal GroupBox fraDelimitedFileOptions;
        internal ComboBox cboInputFileColumnOrdering;
        internal Label lblInputFileColumnOrdering;
        internal TextBox txtInputFileColumnDelimiter;
        internal Label lblInputFileColumnDelimiter;
        internal ComboBox cboInputFileColumnDelimiter;
        internal TabPage TabPageFileFormatOptions;
        internal TabControl tbsOptions;
        internal TabPage TabPageParseAndDigest;
        internal GroupBox fraProcessingOptions;
        internal TextBox txtProteinReversalSamplingPercentage;
        internal Label lbltxtAddnlRefAccessionSepChar;
        internal CheckBox chkLookForAddnlRefInDescription;
        internal ComboBox cboProteinReversalOptions;
        internal Label lblProteinReversalOptions;
        internal CheckBox chkDigestProteins;
        internal Label lblAddnlRefSepChar;
        internal TextBox txtAddnlRefAccessionSepChar;
        internal TextBox txtAddnlRefSepChar;
        internal CheckBox chkCreateFastaOutputFile;
        internal GroupBox fraCalculationOptions;
        internal Label lblMassMode;
        internal ComboBox cboElementMassMode;
        internal CheckBox chkExcludeProteinSequence;
        internal CheckBox chkComputepIandNET;
        internal CheckBox chkIncludeXResidues;
        internal CheckBox chkComputeProteinMass;
        internal GroupBox fraDigestionOptions;
        internal TextBox txtDigestProteinsMaximumpI;
        internal Label lblDigestProteinsMaximumpI;
        internal TextBox txtDigestProteinsMinimumpI;
        internal Label lblDigestProteinsMinimumpI;
        internal CheckBox chkGenerateUniqueIDValues;
        internal CheckBox chkCysPeptidesOnly;
        internal TextBox txtDigestProteinsMinimumResidueCount;
        internal Label lblDigestProteinsMinimumResidueCount;
        internal TextBox txtDigestProteinsMaximumMissedCleavages;
        internal Label lblDigestProteinsMaximumMissedCleavages;
        internal TextBox txtDigestProteinsMaximumMass;
        internal Label lblDigestProteinsMaximumMass;
        internal TextBox txtDigestProteinsMinimumMass;
        internal Label lblDigestProteinsMinimumMass;
        internal ComboBox cboCleavageRuleType;
        internal CheckBox chkIncludeDuplicateSequences;
        internal Button cmdParseInputFile;
        internal TabPage TabPagePeakMatchingThresholds;
        internal CheckBox chkAutoDefineSLiCScoreTolerances;
        internal Button cmdPastePMThresholdsList;
        internal ComboBox cboPMPredefinedThresholds;
        internal Button cmdPMThresholdsAutoPopulate;
        internal Button cmdClearPMThresholdsList;
        internal ComboBox cboMassTolType;
        internal Label lblMassTolType;
        internal DataGrid dgPeakMatchingThresholds;
        internal TabPage TabPageProgress;
        internal ProgressBar pbarProgress;
        internal Label lblErrorMessage;
        internal Label lblSubtaskProgress;
        internal Label lblProgress;
        internal Label lblSubtaskProgressDescription;
        internal Label lblProgressDescription;
        internal Button cmdAbortProcessing;
        internal MenuItem mnuHelpAboutElutionTime;
        internal ComboBox cboInputFileFormat;
        internal Label lblInputFileFormat;
        internal Button cmdSelectFile;
        internal GroupBox fraInputFilePath;
        internal Button cmdValidateFastaFile;
        internal CheckBox chkEnableLogging;
        internal MenuItem mnuFileSelectOutputFile;
        internal Button cmdSelectOutputFile;
        internal MenuItem mnuFileSep1;
        internal MenuItem mnuFile;
        internal MenuItem mnuFileSelectInputFile;
        internal MenuItem mnuFileSaveDefaultOptions;
        internal MenuItem mnuFileSep2;
        internal MenuItem mnuFileExit;
        internal TextBox txtProteinOutputFilePath;
        internal CheckBox chkIncludePrefixAndSuffixResidues;
        internal MenuItem mnuEditResetOptions;
        internal MenuItem mnuHelp;
        internal MenuItem mnuHelpAbout;
        internal MenuItem mnuEditSep1;
        internal MenuItem mnuEditMakeUniquenessStats;
        internal MenuItem mnuEdit;
        internal MenuItem mnuEditParseFile;
        internal MainMenu MainMenuControl;
        internal Label lblOutputFileFieldDelimiter;
        internal ComboBox cboOutputFileFieldDelimiter;
        internal TextBox txtOutputFileFieldDelimiter;
        internal GroupBox fraOutputTextOptions;
        internal CheckBox chkTruncateProteinDescription;
        internal CheckBox chkComputeSequenceHashValues;
        internal CheckBox chkComputeSequenceHashIgnoreILDiff;
        internal CheckBox chkExcludeProteinDescription;
        internal Button cmdNETInfo;
        internal ComboBox cboCysTreatmentMode;
        internal Label lblCysTreatment;
        internal ComboBox cboFragmentMassMode;
        internal Label lblFragmentMassMode;
    }
}