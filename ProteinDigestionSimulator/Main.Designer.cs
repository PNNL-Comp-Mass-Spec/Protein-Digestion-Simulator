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
            _txtProteinInputFilePath = new TextBox();
            _txtProteinInputFilePath.TextChanged += new EventHandler(txtProteinInputFilePath_TextChanged);
            optUseRectangleSearchRegion = new RadioButton();
            optUseEllipseSearchRegion = new RadioButton();
            lblUniquenessCalculationsNote = new Label();
            lblProteinScramblingLoopCount = new Label();
            fraPeakMatchingOptions = new GroupBox();
            _txtMaxPeakMatchingResultsPerFeatureToSave = new TextBox();
            _txtMaxPeakMatchingResultsPerFeatureToSave.KeyPress += new KeyPressEventHandler(txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress);
            _txtMaxPeakMatchingResultsPerFeatureToSave.Validating += new System.ComponentModel.CancelEventHandler(txtMaxPeakMatchingResultsPerFeatureToSave_Validating);
            lblMaxPeakMatchingResultsPerFeatureToSave = new Label();
            chkExportPeakMatchingResults = new CheckBox();
            _txtMinimumSLiCScore = new TextBox();
            _txtMinimumSLiCScore.KeyPress += new KeyPressEventHandler(txtMinimumSLiCScore_KeyPress);
            _txtMinimumSLiCScore.Validating += new System.ComponentModel.CancelEventHandler(txtMinimumSLiCScore_Validating);
            lblMinimumSLiCScore = new Label();
            _chkUseSLiCScoreForUniqueness = new CheckBox();
            _chkUseSLiCScoreForUniqueness.CheckedChanged += new EventHandler(chkUseSLiCScoreForUniqueness_CheckedChanged);
            TabPageUniquenessStats = new TabPage();
            fraSqlServerOptions = new GroupBox();
            chkSqlServerUseExistingData = new CheckBox();
            _chkAllowSqlServerCaching = new CheckBox();
            _chkAllowSqlServerCaching.CheckedChanged += new EventHandler(chkAllowSqlServerCaching_CheckedChanged);
            lblSqlServerPassword = new Label();
            lblSqlServerUsername = new Label();
            txtSqlServerPassword = new TextBox();
            txtSqlServerUsername = new TextBox();
            lblSqlServerDatabase = new Label();
            lblSqlServerServerName = new Label();
            _chkSqlServerUseIntegratedSecurity = new CheckBox();
            _chkSqlServerUseIntegratedSecurity.CheckedChanged += new EventHandler(chkSqlServerUseIntegratedSecurity_CheckedChanged);
            txtSqlServerDatabase = new TextBox();
            txtSqlServerName = new TextBox();
            _chkUseSqlServerDBToCacheData = new CheckBox();
            _chkUseSqlServerDBToCacheData.CheckedChanged += new EventHandler(chkUseSqlServerDBToCacheData_CheckedChanged);
            fraUniquenessBinningOptions = new GroupBox();
            lblPeptideUniquenessMassMode = new Label();
            _txtUniquenessBinWidth = new TextBox();
            _txtUniquenessBinWidth.KeyPress += new KeyPressEventHandler(txtUniquenessBinWidth_KeyPress);
            lblUniquenessBinWidth = new Label();
            _chkAutoComputeRangeForBinning = new CheckBox();
            _chkAutoComputeRangeForBinning.CheckedChanged += new EventHandler(chkAutoComputeRangeForBinning_CheckedChanged);
            _txtUniquenessBinEndMass = new TextBox();
            _txtUniquenessBinEndMass.KeyPress += new KeyPressEventHandler(txtUniquenessBinEndMass_KeyPress);
            lblUniquenessBinEndMass = new Label();
            _txtUniquenessBinStartMass = new TextBox();
            _txtUniquenessBinStartMass.KeyPress += new KeyPressEventHandler(txtUniquenessBinStartMass_KeyPress);
            lblUniquenessBinStartMass = new Label();
            lblUniquenessStatsNote = new Label();
            _cmdGenerateUniquenessStats = new Button();
            _cmdGenerateUniquenessStats.Click += new EventHandler(cmdGenerateUniquenessStats_Click);
            chkAssumeInputFileIsDigested = new CheckBox();
            _txtProteinScramblingLoopCount = new TextBox();
            _txtProteinScramblingLoopCount.KeyPress += new KeyPressEventHandler(txtProteinScramblingLoopCount_KeyPress);
            lblSamplingPercentageUnits = new Label();
            _txtMaxpISequenceLength = new TextBox();
            _txtMaxpISequenceLength.KeyDown += new KeyEventHandler(txtMaxpISequenceLength_KeyDown);
            _txtMaxpISequenceLength.KeyPress += new KeyPressEventHandler(txtMaxpISequenceLength_KeyPress);
            _txtMaxpISequenceLength.Validating += new System.ComponentModel.CancelEventHandler(txtMaxpISequenceLength_Validating);
            _txtMaxpISequenceLength.Validated += new EventHandler(txtMaxpISequenceLength_Validated);
            lblProteinReversalSamplingPercentage = new Label();
            lblMaxpISequenceLength = new Label();
            _chkMaxpIModeEnabled = new CheckBox();
            _chkMaxpIModeEnabled.CheckedChanged += new EventHandler(chkMaxpIModeEnabled_CheckedChanged);
            frapIAndHydrophobicity = new GroupBox();
            lblHydrophobicityMode = new Label();
            _cboHydrophobicityMode = new ComboBox();
            _cboHydrophobicityMode.SelectedIndexChanged += new EventHandler(cboHydrophobicityMode_SelectedIndexChanged);
            txtpIStats = new TextBox();
            _txtSequenceForpI = new TextBox();
            _txtSequenceForpI.TextChanged += new EventHandler(txtSequenceForpI_TextChanged);
            lblSequenceForpI = new Label();
            fraDelimitedFileOptions = new GroupBox();
            cboInputFileColumnOrdering = new ComboBox();
            lblInputFileColumnOrdering = new Label();
            txtInputFileColumnDelimiter = new TextBox();
            lblInputFileColumnDelimiter = new Label();
            _cboInputFileColumnDelimiter = new ComboBox();
            _cboInputFileColumnDelimiter.SelectedIndexChanged += new EventHandler(cboInputFileColumnDelimiter_SelectedIndexChanged);
            TabPageFileFormatOptions = new TabPage();
            tbsOptions = new TabControl();
            TabPageParseAndDigest = new TabPage();
            fraProcessingOptions = new GroupBox();
            _txtProteinReversalSamplingPercentage = new TextBox();
            _txtProteinReversalSamplingPercentage.KeyPress += new KeyPressEventHandler(txtProteinReversalSamplingPercentage_KeyPress);
            lbltxtAddnlRefAccessionSepChar = new Label();
            _chkLookForAddnlRefInDescription = new CheckBox();
            _chkLookForAddnlRefInDescription.CheckedChanged += new EventHandler(chkLookForAddnlRefInDescription_CheckedChanged);
            _cboProteinReversalOptions = new ComboBox();
            _cboProteinReversalOptions.SelectedIndexChanged += new EventHandler(cboProteinReversalOptions_SelectedIndexChanged);
            lblProteinReversalOptions = new Label();
            _chkDigestProteins = new CheckBox();
            _chkDigestProteins.CheckedChanged += new EventHandler(chkDigestProteins_CheckedChanged);
            lblAddnlRefSepChar = new Label();
            txtAddnlRefAccessionSepChar = new TextBox();
            txtAddnlRefSepChar = new TextBox();
            _chkCreateFastaOutputFile = new CheckBox();
            _chkCreateFastaOutputFile.CheckedChanged += new EventHandler(chkCreateFastaOutputFile_CheckedChanged);
            fraCalculationOptions = new GroupBox();
            _cmdNETInfo = new Button();
            _cmdNETInfo.Click += new EventHandler(cmdNETInfo_Click);
            chkExcludeProteinDescription = new CheckBox();
            chkComputeSequenceHashIgnoreILDiff = new CheckBox();
            chkTruncateProteinDescription = new CheckBox();
            _chkComputeSequenceHashValues = new CheckBox();
            _chkComputeSequenceHashValues.CheckedChanged += new EventHandler(chkComputeSequenceHashValues_CheckedChanged);
            lblMassMode = new Label();
            _cboElementMassMode = new ComboBox();
            _cboElementMassMode.SelectedIndexChanged += new EventHandler(cboElementMassMode_SelectedIndexChanged);
            chkExcludeProteinSequence = new CheckBox();
            _chkComputepIandNET = new CheckBox();
            _chkComputepIandNET.CheckedChanged += new EventHandler(chkComputepI_CheckedChanged);
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
            _txtDigestProteinsMinimumResidueCount = new TextBox();
            _txtDigestProteinsMinimumResidueCount.KeyPress += new KeyPressEventHandler(txtDigestProteinsMinimumResidueCount_KeyPress);
            lblDigestProteinsMinimumResidueCount = new Label();
            _txtDigestProteinsMaximumMissedCleavages = new TextBox();
            _txtDigestProteinsMaximumMissedCleavages.KeyPress += new KeyPressEventHandler(txtDigestProteinsMaximumMissedCleavages_KeyPress);
            lblDigestProteinsMaximumMissedCleavages = new Label();
            _txtDigestProteinsMaximumMass = new TextBox();
            _txtDigestProteinsMaximumMass.KeyPress += new KeyPressEventHandler(txtDigestProteinsMaximumMass_KeyPress);
            lblDigestProteinsMaximumMass = new Label();
            _txtDigestProteinsMinimumMass = new TextBox();
            _txtDigestProteinsMinimumMass.KeyPress += new KeyPressEventHandler(txtDigestProteinsMinimumMass_KeyPress);
            lblDigestProteinsMinimumMass = new Label();
            cboCleavageRuleType = new ComboBox();
            chkIncludeDuplicateSequences = new CheckBox();
            _cmdParseInputFile = new Button();
            _cmdParseInputFile.Click += new EventHandler(cmdParseInputFile_Click);
            TabPagePeakMatchingThresholds = new TabPage();
            _chkAutoDefineSLiCScoreTolerances = new CheckBox();
            _chkAutoDefineSLiCScoreTolerances.CheckedChanged += new EventHandler(chkAutoDefineSLiCScoreTolerances_CheckedChanged);
            _cmdPastePMThresholdsList = new Button();
            _cmdPastePMThresholdsList.Click += new EventHandler(cmdPastePMThresholdsList_Click);
            cboPMPredefinedThresholds = new ComboBox();
            _cmdPMThresholdsAutoPopulate = new Button();
            _cmdPMThresholdsAutoPopulate.Click += new EventHandler(cmdPMThresholdsAutoPopulate_Click);
            _cmdClearPMThresholdsList = new Button();
            _cmdClearPMThresholdsList.Click += new EventHandler(cmdClearPMThresholdsList_Click);
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
            _cmdAbortProcessing = new Button();
            _cmdAbortProcessing.Click += new EventHandler(cmdAbortProcessing_Click);
            _mnuHelpAboutElutionTime = new MenuItem();
            _mnuHelpAboutElutionTime.Click += new EventHandler(mnuHelpAboutElutionTime_Click);
            _cboInputFileFormat = new ComboBox();
            _cboInputFileFormat.SelectedIndexChanged += new EventHandler(cboInputFileFormat_SelectedIndexChanged);
            lblInputFileFormat = new Label();
            _cmdSelectFile = new Button();
            _cmdSelectFile.Click += new EventHandler(cmdSelectFile_Click);
            fraInputFilePath = new GroupBox();
            _cmdValidateFastaFile = new Button();
            _cmdValidateFastaFile.Click += new EventHandler(cmdValidateFastaFile_Click);
            chkEnableLogging = new CheckBox();
            _mnuFileSelectOutputFile = new MenuItem();
            _mnuFileSelectOutputFile.Click += new EventHandler(mnuFileSelectOutputFile_Click);
            _cmdSelectOutputFile = new Button();
            _cmdSelectOutputFile.Click += new EventHandler(cmdSelectOutputFile_Click);
            mnuFileSep1 = new MenuItem();
            mnuFile = new MenuItem();
            _mnuFileSelectInputFile = new MenuItem();
            _mnuFileSelectInputFile.Click += new EventHandler(mnuFileSelectInputFile_Click);
            _mnuFileSaveDefaultOptions = new MenuItem();
            _mnuFileSaveDefaultOptions.Click += new EventHandler(mnuFileSaveDefaultOptions_Click);
            mnuFileSep2 = new MenuItem();
            _mnuFileExit = new MenuItem();
            _mnuFileExit.Click += new EventHandler(mnuFileExit_Click);
            txtProteinOutputFilePath = new TextBox();
            chkIncludePrefixAndSuffixResidues = new CheckBox();
            _mnuEditResetOptions = new MenuItem();
            _mnuEditResetOptions.Click += new EventHandler(mnuEditResetOptions_Click);
            mnuHelp = new MenuItem();
            _mnuHelpAbout = new MenuItem();
            _mnuHelpAbout.Click += new EventHandler(mnuHelpAbout_Click);
            mnuEditSep1 = new MenuItem();
            _mnuEditMakeUniquenessStats = new MenuItem();
            _mnuEditMakeUniquenessStats.Click += new EventHandler(mnuEditMakeUniquenessStats_Click);
            mnuEdit = new MenuItem();
            _mnuEditParseFile = new MenuItem();
            _mnuEditParseFile.Click += new EventHandler(mnuEditParseFile_Click);
            MainMenuControl = new MainMenu(components);
            lblOutputFileFieldDelimiter = new Label();
            _cboOutputFileFieldDelimiter = new ComboBox();
            _cboOutputFileFieldDelimiter.SelectedIndexChanged += new EventHandler(cboOutputFileFieldDelimiter_SelectedIndexChanged);
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
            _txtProteinInputFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtProteinInputFilePath.Location = new Point(104, 26);
            _txtProteinInputFilePath.Name = "_txtProteinInputFilePath";
            _txtProteinInputFilePath.Size = new Size(611, 20);
            _txtProteinInputFilePath.TabIndex = 1;
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
            fraPeakMatchingOptions.Controls.Add(_txtMaxPeakMatchingResultsPerFeatureToSave);
            fraPeakMatchingOptions.Controls.Add(lblMaxPeakMatchingResultsPerFeatureToSave);
            fraPeakMatchingOptions.Controls.Add(chkExportPeakMatchingResults);
            fraPeakMatchingOptions.Controls.Add(_txtMinimumSLiCScore);
            fraPeakMatchingOptions.Controls.Add(lblMinimumSLiCScore);
            fraPeakMatchingOptions.Controls.Add(_chkUseSLiCScoreForUniqueness);
            fraPeakMatchingOptions.Location = new Point(232, 48);
            fraPeakMatchingOptions.Name = "fraPeakMatchingOptions";
            fraPeakMatchingOptions.Size = new Size(392, 136);
            fraPeakMatchingOptions.TabIndex = 2;
            fraPeakMatchingOptions.TabStop = false;
            fraPeakMatchingOptions.Text = "Peak Matching Options";
            // 
            // txtMaxPeakMatchingResultsPerFeatureToSave
            // 
            _txtMaxPeakMatchingResultsPerFeatureToSave.Location = new Point(272, 16);
            _txtMaxPeakMatchingResultsPerFeatureToSave.Name = "_txtMaxPeakMatchingResultsPerFeatureToSave";
            _txtMaxPeakMatchingResultsPerFeatureToSave.Size = new Size(40, 20);
            _txtMaxPeakMatchingResultsPerFeatureToSave.TabIndex = 1;
            _txtMaxPeakMatchingResultsPerFeatureToSave.Text = "3";
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
            _txtMinimumSLiCScore.Location = new Point(144, 104);
            _txtMinimumSLiCScore.Name = "_txtMinimumSLiCScore";
            _txtMinimumSLiCScore.Size = new Size(40, 20);
            _txtMinimumSLiCScore.TabIndex = 5;
            _txtMinimumSLiCScore.Text = "0.99";
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
            _chkUseSLiCScoreForUniqueness.Checked = true;
            _chkUseSLiCScoreForUniqueness.CheckState = CheckState.Checked;
            _chkUseSLiCScoreForUniqueness.Location = new Point(16, 60);
            _chkUseSLiCScoreForUniqueness.Name = "_chkUseSLiCScoreForUniqueness";
            _chkUseSLiCScoreForUniqueness.Size = new Size(168, 32);
            _chkUseSLiCScoreForUniqueness.TabIndex = 3;
            _chkUseSLiCScoreForUniqueness.Text = "Use SLiC Score when gauging peptide uniqueness";
            // 
            // TabPageUniquenessStats
            // 
            TabPageUniquenessStats.Controls.Add(lblUniquenessCalculationsNote);
            TabPageUniquenessStats.Controls.Add(fraPeakMatchingOptions);
            TabPageUniquenessStats.Controls.Add(fraSqlServerOptions);
            TabPageUniquenessStats.Controls.Add(fraUniquenessBinningOptions);
            TabPageUniquenessStats.Controls.Add(lblUniquenessStatsNote);
            TabPageUniquenessStats.Controls.Add(_cmdGenerateUniquenessStats);
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
            fraSqlServerOptions.Controls.Add(_chkAllowSqlServerCaching);
            fraSqlServerOptions.Controls.Add(lblSqlServerPassword);
            fraSqlServerOptions.Controls.Add(lblSqlServerUsername);
            fraSqlServerOptions.Controls.Add(txtSqlServerPassword);
            fraSqlServerOptions.Controls.Add(txtSqlServerUsername);
            fraSqlServerOptions.Controls.Add(lblSqlServerDatabase);
            fraSqlServerOptions.Controls.Add(lblSqlServerServerName);
            fraSqlServerOptions.Controls.Add(_chkSqlServerUseIntegratedSecurity);
            fraSqlServerOptions.Controls.Add(txtSqlServerDatabase);
            fraSqlServerOptions.Controls.Add(txtSqlServerName);
            fraSqlServerOptions.Controls.Add(_chkUseSqlServerDBToCacheData);
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
            _chkAllowSqlServerCaching.Location = new Point(8, 16);
            _chkAllowSqlServerCaching.Name = "_chkAllowSqlServerCaching";
            _chkAllowSqlServerCaching.Size = new Size(144, 32);
            _chkAllowSqlServerCaching.TabIndex = 0;
            _chkAllowSqlServerCaching.Text = "Allow data caching using Sql Server";
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
            _chkSqlServerUseIntegratedSecurity.Checked = true;
            _chkSqlServerUseIntegratedSecurity.CheckState = CheckState.Checked;
            _chkSqlServerUseIntegratedSecurity.Location = new Point(8, 72);
            _chkSqlServerUseIntegratedSecurity.Name = "_chkSqlServerUseIntegratedSecurity";
            _chkSqlServerUseIntegratedSecurity.Size = new Size(144, 16);
            _chkSqlServerUseIntegratedSecurity.TabIndex = 6;
            _chkSqlServerUseIntegratedSecurity.Text = "Use Integrated Security";
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
            _chkUseSqlServerDBToCacheData.Checked = true;
            _chkUseSqlServerDBToCacheData.CheckState = CheckState.Checked;
            _chkUseSqlServerDBToCacheData.Location = new Point(8, 56);
            _chkUseSqlServerDBToCacheData.Name = "_chkUseSqlServerDBToCacheData";
            _chkUseSqlServerDBToCacheData.Size = new Size(144, 16);
            _chkUseSqlServerDBToCacheData.TabIndex = 1;
            _chkUseSqlServerDBToCacheData.Text = "Enable data caching";
            // 
            // fraUniquenessBinningOptions
            // 
            fraUniquenessBinningOptions.Controls.Add(lblPeptideUniquenessMassMode);
            fraUniquenessBinningOptions.Controls.Add(_txtUniquenessBinWidth);
            fraUniquenessBinningOptions.Controls.Add(lblUniquenessBinWidth);
            fraUniquenessBinningOptions.Controls.Add(_chkAutoComputeRangeForBinning);
            fraUniquenessBinningOptions.Controls.Add(_txtUniquenessBinEndMass);
            fraUniquenessBinningOptions.Controls.Add(lblUniquenessBinEndMass);
            fraUniquenessBinningOptions.Controls.Add(_txtUniquenessBinStartMass);
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
            _txtUniquenessBinWidth.Location = new Point(80, 24);
            _txtUniquenessBinWidth.Name = "_txtUniquenessBinWidth";
            _txtUniquenessBinWidth.Size = new Size(40, 20);
            _txtUniquenessBinWidth.TabIndex = 1;
            _txtUniquenessBinWidth.Text = "25";
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
            _chkAutoComputeRangeForBinning.Checked = true;
            _chkAutoComputeRangeForBinning.CheckState = CheckState.Checked;
            _chkAutoComputeRangeForBinning.Location = new Point(16, 56);
            _chkAutoComputeRangeForBinning.Name = "_chkAutoComputeRangeForBinning";
            _chkAutoComputeRangeForBinning.Size = new Size(184, 17);
            _chkAutoComputeRangeForBinning.TabIndex = 2;
            _chkAutoComputeRangeForBinning.Text = "Auto compute range for binning";
            // 
            // txtUniquenessBinEndMass
            // 
            _txtUniquenessBinEndMass.Location = new Point(80, 104);
            _txtUniquenessBinEndMass.Name = "_txtUniquenessBinEndMass";
            _txtUniquenessBinEndMass.Size = new Size(40, 20);
            _txtUniquenessBinEndMass.TabIndex = 6;
            _txtUniquenessBinEndMass.Text = "6000";
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
            _txtUniquenessBinStartMass.Location = new Point(80, 80);
            _txtUniquenessBinStartMass.Name = "_txtUniquenessBinStartMass";
            _txtUniquenessBinStartMass.Size = new Size(40, 20);
            _txtUniquenessBinStartMass.TabIndex = 4;
            _txtUniquenessBinStartMass.Text = "400";
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
            _cmdGenerateUniquenessStats.Location = new Point(232, 16);
            _cmdGenerateUniquenessStats.Name = "_cmdGenerateUniquenessStats";
            _cmdGenerateUniquenessStats.Size = new Size(176, 24);
            _cmdGenerateUniquenessStats.TabIndex = 5;
            _cmdGenerateUniquenessStats.Text = "&Generate Uniqueness Stats";
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
            _txtProteinScramblingLoopCount.Location = new Point(312, 40);
            _txtProteinScramblingLoopCount.MaxLength = 3;
            _txtProteinScramblingLoopCount.Name = "_txtProteinScramblingLoopCount";
            _txtProteinScramblingLoopCount.Size = new Size(32, 20);
            _txtProteinScramblingLoopCount.TabIndex = 13;
            _txtProteinScramblingLoopCount.Text = "1";
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
            _txtMaxpISequenceLength.Location = new Point(168, 70);
            _txtMaxpISequenceLength.Name = "_txtMaxpISequenceLength";
            _txtMaxpISequenceLength.Size = new Size(40, 20);
            _txtMaxpISequenceLength.TabIndex = 4;
            _txtMaxpISequenceLength.Text = "10";
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
            _chkMaxpIModeEnabled.Location = new Point(8, 48);
            _chkMaxpIModeEnabled.Name = "_chkMaxpIModeEnabled";
            _chkMaxpIModeEnabled.Size = new Size(224, 16);
            _chkMaxpIModeEnabled.TabIndex = 2;
            _chkMaxpIModeEnabled.Text = "Report maximum of all sub-sequences";
            // 
            // frapIAndHydrophobicity
            // 
            frapIAndHydrophobicity.Controls.Add(_txtMaxpISequenceLength);
            frapIAndHydrophobicity.Controls.Add(lblMaxpISequenceLength);
            frapIAndHydrophobicity.Controls.Add(_chkMaxpIModeEnabled);
            frapIAndHydrophobicity.Controls.Add(lblHydrophobicityMode);
            frapIAndHydrophobicity.Controls.Add(_cboHydrophobicityMode);
            frapIAndHydrophobicity.Controls.Add(txtpIStats);
            frapIAndHydrophobicity.Controls.Add(_txtSequenceForpI);
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
            _cboHydrophobicityMode.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboHydrophobicityMode.DropDownWidth = 70;
            _cboHydrophobicityMode.Location = new Point(128, 18);
            _cboHydrophobicityMode.Name = "_cboHydrophobicityMode";
            _cboHydrophobicityMode.Size = new Size(184, 21);
            _cboHydrophobicityMode.TabIndex = 1;
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
            _txtSequenceForpI.Location = new Point(400, 16);
            _txtSequenceForpI.Name = "_txtSequenceForpI";
            _txtSequenceForpI.Size = new Size(208, 20);
            _txtSequenceForpI.TabIndex = 6;
            _txtSequenceForpI.Text = "FKDLGEEQFK";
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
            fraDelimitedFileOptions.Controls.Add(_cboInputFileColumnDelimiter);
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
            _cboInputFileColumnDelimiter.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboInputFileColumnDelimiter.DropDownWidth = 70;
            _cboInputFileColumnDelimiter.Location = new Point(112, 56);
            _cboInputFileColumnDelimiter.Name = "_cboInputFileColumnDelimiter";
            _cboInputFileColumnDelimiter.Size = new Size(70, 21);
            _cboInputFileColumnDelimiter.TabIndex = 3;
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
            TabPageParseAndDigest.Controls.Add(_cmdParseInputFile);
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
            fraProcessingOptions.Controls.Add(_txtProteinScramblingLoopCount);
            fraProcessingOptions.Controls.Add(lblSamplingPercentageUnits);
            fraProcessingOptions.Controls.Add(lblProteinReversalSamplingPercentage);
            fraProcessingOptions.Controls.Add(_txtProteinReversalSamplingPercentage);
            fraProcessingOptions.Controls.Add(lbltxtAddnlRefAccessionSepChar);
            fraProcessingOptions.Controls.Add(_chkLookForAddnlRefInDescription);
            fraProcessingOptions.Controls.Add(_cboProteinReversalOptions);
            fraProcessingOptions.Controls.Add(lblProteinReversalOptions);
            fraProcessingOptions.Controls.Add(_chkDigestProteins);
            fraProcessingOptions.Controls.Add(lblAddnlRefSepChar);
            fraProcessingOptions.Controls.Add(txtAddnlRefAccessionSepChar);
            fraProcessingOptions.Controls.Add(txtAddnlRefSepChar);
            fraProcessingOptions.Controls.Add(_chkCreateFastaOutputFile);
            fraProcessingOptions.Location = new Point(8, 8);
            fraProcessingOptions.Name = "fraProcessingOptions";
            fraProcessingOptions.Size = new Size(360, 176);
            fraProcessingOptions.TabIndex = 0;
            fraProcessingOptions.TabStop = false;
            fraProcessingOptions.Text = "Processing Options";
            // 
            // txtProteinReversalSamplingPercentage
            // 
            _txtProteinReversalSamplingPercentage.Location = new Point(168, 40);
            _txtProteinReversalSamplingPercentage.MaxLength = 3;
            _txtProteinReversalSamplingPercentage.Name = "_txtProteinReversalSamplingPercentage";
            _txtProteinReversalSamplingPercentage.Size = new Size(32, 20);
            _txtProteinReversalSamplingPercentage.TabIndex = 3;
            _txtProteinReversalSamplingPercentage.Text = "100";
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
            _chkLookForAddnlRefInDescription.Location = new Point(16, 72);
            _chkLookForAddnlRefInDescription.Name = "_chkLookForAddnlRefInDescription";
            _chkLookForAddnlRefInDescription.Size = new Size(120, 32);
            _chkLookForAddnlRefInDescription.TabIndex = 5;
            _chkLookForAddnlRefInDescription.Text = "Look for addnl Ref in description";
            // 
            // cboProteinReversalOptions
            // 
            _cboProteinReversalOptions.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboProteinReversalOptions.Location = new Point(168, 16);
            _cboProteinReversalOptions.Name = "_cboProteinReversalOptions";
            _cboProteinReversalOptions.Size = new Size(184, 21);
            _cboProteinReversalOptions.TabIndex = 1;
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
            _chkDigestProteins.Location = new Point(16, 115);
            _chkDigestProteins.Name = "_chkDigestProteins";
            _chkDigestProteins.Size = new Size(160, 32);
            _chkDigestProteins.TabIndex = 10;
            _chkDigestProteins.Text = "In Silico digest of all proteins in input file";
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
            _chkCreateFastaOutputFile.Location = new Point(192, 128);
            _chkCreateFastaOutputFile.Name = "_chkCreateFastaOutputFile";
            _chkCreateFastaOutputFile.Size = new Size(160, 16);
            _chkCreateFastaOutputFile.TabIndex = 11;
            _chkCreateFastaOutputFile.Text = "Create FASTA Output File";
            // 
            // fraCalculationOptions
            // 
            fraCalculationOptions.Controls.Add(_cmdNETInfo);
            fraCalculationOptions.Controls.Add(chkExcludeProteinDescription);
            fraCalculationOptions.Controls.Add(chkComputeSequenceHashIgnoreILDiff);
            fraCalculationOptions.Controls.Add(chkTruncateProteinDescription);
            fraCalculationOptions.Controls.Add(_chkComputeSequenceHashValues);
            fraCalculationOptions.Controls.Add(lblMassMode);
            fraCalculationOptions.Controls.Add(_cboElementMassMode);
            fraCalculationOptions.Controls.Add(chkExcludeProteinSequence);
            fraCalculationOptions.Controls.Add(_chkComputepIandNET);
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
            _cmdNETInfo.Location = new Point(268, 87);
            _cmdNETInfo.Margin = new Padding(1);
            _cmdNETInfo.Name = "_cmdNETInfo";
            _cmdNETInfo.Size = new Size(34, 20);
            _cmdNETInfo.TabIndex = 4;
            _cmdNETInfo.Text = "Info";
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
            _chkComputeSequenceHashValues.Checked = true;
            _chkComputeSequenceHashValues.CheckState = CheckState.Checked;
            _chkComputeSequenceHashValues.Location = new Point(16, 107);
            _chkComputeSequenceHashValues.Name = "_chkComputeSequenceHashValues";
            _chkComputeSequenceHashValues.Size = new Size(164, 19);
            _chkComputeSequenceHashValues.TabIndex = 6;
            _chkComputeSequenceHashValues.Text = "Compute sequence hashes";
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
            _cboElementMassMode.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboElementMassMode.Location = new Point(88, 65);
            _cboElementMassMode.Name = "_cboElementMassMode";
            _cboElementMassMode.Size = new Size(144, 21);
            _cboElementMassMode.TabIndex = 4;
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
            _chkComputepIandNET.Location = new Point(16, 89);
            _chkComputepIandNET.Name = "_chkComputepIandNET";
            _chkComputepIandNET.Size = new Size(252, 18);
            _chkComputepIandNET.TabIndex = 3;
            _chkComputepIandNET.Text = "Compute pI and Normalized Elution Time (NET)";
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
            fraDigestionOptions.Controls.Add(_txtDigestProteinsMinimumResidueCount);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMinimumResidueCount);
            fraDigestionOptions.Controls.Add(_txtDigestProteinsMaximumMissedCleavages);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMaximumMissedCleavages);
            fraDigestionOptions.Controls.Add(_txtDigestProteinsMaximumMass);
            fraDigestionOptions.Controls.Add(lblDigestProteinsMaximumMass);
            fraDigestionOptions.Controls.Add(_txtDigestProteinsMinimumMass);
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
            _txtDigestProteinsMinimumResidueCount.Location = new Point(359, 56);
            _txtDigestProteinsMinimumResidueCount.Name = "_txtDigestProteinsMinimumResidueCount";
            _txtDigestProteinsMinimumResidueCount.Size = new Size(32, 20);
            _txtDigestProteinsMinimumResidueCount.TabIndex = 7;
            _txtDigestProteinsMinimumResidueCount.Text = "0";
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
            _txtDigestProteinsMaximumMissedCleavages.Location = new Point(359, 80);
            _txtDigestProteinsMaximumMissedCleavages.Name = "_txtDigestProteinsMaximumMissedCleavages";
            _txtDigestProteinsMaximumMissedCleavages.Size = new Size(32, 20);
            _txtDigestProteinsMaximumMissedCleavages.TabIndex = 9;
            _txtDigestProteinsMaximumMissedCleavages.Text = "3";
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
            _txtDigestProteinsMaximumMass.Location = new Point(152, 80);
            _txtDigestProteinsMaximumMass.Name = "_txtDigestProteinsMaximumMass";
            _txtDigestProteinsMaximumMass.Size = new Size(40, 20);
            _txtDigestProteinsMaximumMass.TabIndex = 5;
            _txtDigestProteinsMaximumMass.Text = "6000";
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
            _txtDigestProteinsMinimumMass.Location = new Point(152, 56);
            _txtDigestProteinsMinimumMass.Name = "_txtDigestProteinsMinimumMass";
            _txtDigestProteinsMinimumMass.Size = new Size(40, 20);
            _txtDigestProteinsMinimumMass.TabIndex = 3;
            _txtDigestProteinsMinimumMass.Text = "400";
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
            _cmdParseInputFile.Location = new Point(384, 8);
            _cmdParseInputFile.Name = "_cmdParseInputFile";
            _cmdParseInputFile.Size = new Size(112, 24);
            _cmdParseInputFile.TabIndex = 3;
            _cmdParseInputFile.Text = "&Parse and Digest";
            // 
            // TabPagePeakMatchingThresholds
            // 
            TabPagePeakMatchingThresholds.Controls.Add(_chkAutoDefineSLiCScoreTolerances);
            TabPagePeakMatchingThresholds.Controls.Add(_cmdPastePMThresholdsList);
            TabPagePeakMatchingThresholds.Controls.Add(cboPMPredefinedThresholds);
            TabPagePeakMatchingThresholds.Controls.Add(_cmdPMThresholdsAutoPopulate);
            TabPagePeakMatchingThresholds.Controls.Add(_cmdClearPMThresholdsList);
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
            _chkAutoDefineSLiCScoreTolerances.Checked = true;
            _chkAutoDefineSLiCScoreTolerances.CheckState = CheckState.Checked;
            _chkAutoDefineSLiCScoreTolerances.Location = new Point(16, 256);
            _chkAutoDefineSLiCScoreTolerances.Name = "_chkAutoDefineSLiCScoreTolerances";
            _chkAutoDefineSLiCScoreTolerances.Size = new Size(208, 16);
            _chkAutoDefineSLiCScoreTolerances.TabIndex = 3;
            _chkAutoDefineSLiCScoreTolerances.Text = "Auto Define SLiC Score Tolerances";
            // 
            // cmdPastePMThresholdsList
            // 
            _cmdPastePMThresholdsList.Location = new Point(456, 96);
            _cmdPastePMThresholdsList.Name = "_cmdPastePMThresholdsList";
            _cmdPastePMThresholdsList.Size = new Size(104, 24);
            _cmdPastePMThresholdsList.TabIndex = 6;
            _cmdPastePMThresholdsList.Text = "Paste Values";
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
            _cmdPMThresholdsAutoPopulate.Location = new Point(336, 224);
            _cmdPMThresholdsAutoPopulate.Name = "_cmdPMThresholdsAutoPopulate";
            _cmdPMThresholdsAutoPopulate.Size = new Size(104, 24);
            _cmdPMThresholdsAutoPopulate.TabIndex = 4;
            _cmdPMThresholdsAutoPopulate.Text = "Auto-Populate";
            // 
            // cmdClearPMThresholdsList
            // 
            _cmdClearPMThresholdsList.Location = new Point(456, 128);
            _cmdClearPMThresholdsList.Name = "_cmdClearPMThresholdsList";
            _cmdClearPMThresholdsList.Size = new Size(104, 24);
            _cmdClearPMThresholdsList.TabIndex = 7;
            _cmdClearPMThresholdsList.Text = "Clear List";
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
            TabPageProgress.Controls.Add(_cmdAbortProcessing);
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
            _cmdAbortProcessing.Location = new Point(10, 106);
            _cmdAbortProcessing.Name = "_cmdAbortProcessing";
            _cmdAbortProcessing.Size = new Size(121, 24);
            _cmdAbortProcessing.TabIndex = 4;
            _cmdAbortProcessing.Text = "Abort Processing";
            // 
            // mnuHelpAboutElutionTime
            // 
            _mnuHelpAboutElutionTime.Index = 1;
            _mnuHelpAboutElutionTime.Text = "About &Elution Time Prediction";
            // 
            // cboInputFileFormat
            // 
            _cboInputFileFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboInputFileFormat.Location = new Point(112, 56);
            _cboInputFileFormat.Name = "_cboInputFileFormat";
            _cboInputFileFormat.Size = new Size(112, 21);
            _cboInputFileFormat.TabIndex = 3;
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
            _cmdSelectFile.Location = new Point(8, 24);
            _cmdSelectFile.Name = "_cmdSelectFile";
            _cmdSelectFile.Size = new Size(80, 24);
            _cmdSelectFile.TabIndex = 0;
            _cmdSelectFile.Text = "&Select file";
            // 
            // fraInputFilePath
            // 
            fraInputFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            fraInputFilePath.Controls.Add(_cmdValidateFastaFile);
            fraInputFilePath.Controls.Add(_cboInputFileFormat);
            fraInputFilePath.Controls.Add(lblInputFileFormat);
            fraInputFilePath.Controls.Add(_cmdSelectFile);
            fraInputFilePath.Controls.Add(_txtProteinInputFilePath);
            fraInputFilePath.Location = new Point(12, 12);
            fraInputFilePath.Name = "fraInputFilePath";
            fraInputFilePath.Size = new Size(730, 88);
            fraInputFilePath.TabIndex = 3;
            fraInputFilePath.TabStop = false;
            fraInputFilePath.Text = "Protein Input File Path (FASTA or Tab-delimited)";
            // 
            // cmdValidateFastaFile
            // 
            _cmdValidateFastaFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _cmdValidateFastaFile.Location = new Point(595, 56);
            _cmdValidateFastaFile.Name = "_cmdValidateFastaFile";
            _cmdValidateFastaFile.Size = new Size(120, 24);
            _cmdValidateFastaFile.TabIndex = 4;
            _cmdValidateFastaFile.Text = "&Validate FASTA File";
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
            _mnuFileSelectOutputFile.Index = 1;
            _mnuFileSelectOutputFile.Text = "Select &Output File...";
            // 
            // cmdSelectOutputFile
            // 
            _cmdSelectOutputFile.Location = new Point(8, 56);
            _cmdSelectOutputFile.Name = "_cmdSelectOutputFile";
            _cmdSelectOutputFile.Size = new Size(88, 33);
            _cmdSelectOutputFile.TabIndex = 5;
            _cmdSelectOutputFile.Text = "Select / &Create File";
            // 
            // mnuFileSep1
            // 
            mnuFileSep1.Index = 2;
            mnuFileSep1.Text = "-";
            // 
            // mnuFile
            // 
            mnuFile.Index = 0;
            mnuFile.MenuItems.AddRange(new MenuItem[] { _mnuFileSelectInputFile, _mnuFileSelectOutputFile, mnuFileSep1, _mnuFileSaveDefaultOptions, mnuFileSep2, _mnuFileExit });
            mnuFile.Text = "&File";
            // 
            // mnuFileSelectInputFile
            // 
            _mnuFileSelectInputFile.Index = 0;
            _mnuFileSelectInputFile.Text = "Select &Input File...";
            // 
            // mnuFileSaveDefaultOptions
            // 
            _mnuFileSaveDefaultOptions.Index = 3;
            _mnuFileSaveDefaultOptions.Text = "Save &Default Options";
            // 
            // mnuFileSep2
            // 
            mnuFileSep2.Index = 4;
            mnuFileSep2.Text = "-";
            // 
            // mnuFileExit
            // 
            _mnuFileExit.Index = 5;
            _mnuFileExit.Text = "E&xit";
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
            _mnuEditResetOptions.Index = 3;
            _mnuEditResetOptions.Text = "&Reset options to Defaults...";
            // 
            // mnuHelp
            // 
            mnuHelp.Index = 2;
            mnuHelp.MenuItems.AddRange(new MenuItem[] { _mnuHelpAbout, _mnuHelpAboutElutionTime });
            mnuHelp.Text = "&Help";
            // 
            // mnuHelpAbout
            // 
            _mnuHelpAbout.Index = 0;
            _mnuHelpAbout.Text = "&About";
            // 
            // mnuEditSep1
            // 
            mnuEditSep1.Index = 2;
            mnuEditSep1.Text = "-";
            // 
            // mnuEditMakeUniquenessStats
            // 
            _mnuEditMakeUniquenessStats.Index = 1;
            _mnuEditMakeUniquenessStats.Text = "&Make Uniqueness Stats";
            // 
            // mnuEdit
            // 
            mnuEdit.Index = 1;
            mnuEdit.MenuItems.AddRange(new MenuItem[] { _mnuEditParseFile, _mnuEditMakeUniquenessStats, mnuEditSep1, _mnuEditResetOptions });
            mnuEdit.Text = "&Edit";
            // 
            // mnuEditParseFile
            // 
            _mnuEditParseFile.Index = 0;
            _mnuEditParseFile.Text = "&Parse File";
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
            _cboOutputFileFieldDelimiter.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboOutputFileFieldDelimiter.Location = new Point(128, 24);
            _cboOutputFileFieldDelimiter.Name = "_cboOutputFileFieldDelimiter";
            _cboOutputFileFieldDelimiter.Size = new Size(70, 21);
            _cboOutputFileFieldDelimiter.TabIndex = 1;
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
            fraOutputTextOptions.Controls.Add(_cmdSelectOutputFile);
            fraOutputTextOptions.Controls.Add(txtProteinOutputFilePath);
            fraOutputTextOptions.Controls.Add(chkIncludePrefixAndSuffixResidues);
            fraOutputTextOptions.Controls.Add(_cboOutputFileFieldDelimiter);
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

        private TextBox _txtProteinInputFilePath;

        internal TextBox txtProteinInputFilePath
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtProteinInputFilePath;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtProteinInputFilePath != null)
                {
                    _txtProteinInputFilePath.TextChanged -= txtProteinInputFilePath_TextChanged;
                }

                _txtProteinInputFilePath = value;
                if (_txtProteinInputFilePath != null)
                {
                    _txtProteinInputFilePath.TextChanged += txtProteinInputFilePath_TextChanged;
                }
            }
        }

        internal RadioButton optUseRectangleSearchRegion;
        internal RadioButton optUseEllipseSearchRegion;
        internal Label lblUniquenessCalculationsNote;
        internal Label lblProteinScramblingLoopCount;
        internal GroupBox fraPeakMatchingOptions;
        private TextBox _txtMaxPeakMatchingResultsPerFeatureToSave;

        internal TextBox txtMaxPeakMatchingResultsPerFeatureToSave
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtMaxPeakMatchingResultsPerFeatureToSave;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtMaxPeakMatchingResultsPerFeatureToSave != null)
                {
                    _txtMaxPeakMatchingResultsPerFeatureToSave.KeyPress -= txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress;
                    _txtMaxPeakMatchingResultsPerFeatureToSave.Validating -= txtMaxPeakMatchingResultsPerFeatureToSave_Validating;
                }

                _txtMaxPeakMatchingResultsPerFeatureToSave = value;
                if (_txtMaxPeakMatchingResultsPerFeatureToSave != null)
                {
                    _txtMaxPeakMatchingResultsPerFeatureToSave.KeyPress += txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress;
                    _txtMaxPeakMatchingResultsPerFeatureToSave.Validating += txtMaxPeakMatchingResultsPerFeatureToSave_Validating;
                }
            }
        }

        internal Label lblMaxPeakMatchingResultsPerFeatureToSave;
        internal CheckBox chkExportPeakMatchingResults;
        private TextBox _txtMinimumSLiCScore;

        internal TextBox txtMinimumSLiCScore
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtMinimumSLiCScore;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtMinimumSLiCScore != null)
                {
                    _txtMinimumSLiCScore.KeyPress -= txtMinimumSLiCScore_KeyPress;
                    _txtMinimumSLiCScore.Validating -= txtMinimumSLiCScore_Validating;
                }

                _txtMinimumSLiCScore = value;
                if (_txtMinimumSLiCScore != null)
                {
                    _txtMinimumSLiCScore.KeyPress += txtMinimumSLiCScore_KeyPress;
                    _txtMinimumSLiCScore.Validating += txtMinimumSLiCScore_Validating;
                }
            }
        }

        internal Label lblMinimumSLiCScore;
        private CheckBox _chkUseSLiCScoreForUniqueness;

        internal CheckBox chkUseSLiCScoreForUniqueness
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkUseSLiCScoreForUniqueness;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkUseSLiCScoreForUniqueness != null)
                {
                    _chkUseSLiCScoreForUniqueness.CheckedChanged -= chkUseSLiCScoreForUniqueness_CheckedChanged;
                }

                _chkUseSLiCScoreForUniqueness = value;
                if (_chkUseSLiCScoreForUniqueness != null)
                {
                    _chkUseSLiCScoreForUniqueness.CheckedChanged += chkUseSLiCScoreForUniqueness_CheckedChanged;
                }
            }
        }

        internal TabPage TabPageUniquenessStats;
        internal GroupBox fraSqlServerOptions;
        internal CheckBox chkSqlServerUseExistingData;
        private CheckBox _chkAllowSqlServerCaching;

        internal CheckBox chkAllowSqlServerCaching
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkAllowSqlServerCaching;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkAllowSqlServerCaching != null)
                {
                    _chkAllowSqlServerCaching.CheckedChanged -= chkAllowSqlServerCaching_CheckedChanged;
                }

                _chkAllowSqlServerCaching = value;
                if (_chkAllowSqlServerCaching != null)
                {
                    _chkAllowSqlServerCaching.CheckedChanged += chkAllowSqlServerCaching_CheckedChanged;
                }
            }
        }

        internal Label lblSqlServerPassword;
        internal Label lblSqlServerUsername;
        internal TextBox txtSqlServerPassword;
        internal TextBox txtSqlServerUsername;
        internal Label lblSqlServerDatabase;
        internal Label lblSqlServerServerName;
        private CheckBox _chkSqlServerUseIntegratedSecurity;

        internal CheckBox chkSqlServerUseIntegratedSecurity
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkSqlServerUseIntegratedSecurity;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkSqlServerUseIntegratedSecurity != null)
                {
                    _chkSqlServerUseIntegratedSecurity.CheckedChanged -= chkSqlServerUseIntegratedSecurity_CheckedChanged;
                }

                _chkSqlServerUseIntegratedSecurity = value;
                if (_chkSqlServerUseIntegratedSecurity != null)
                {
                    _chkSqlServerUseIntegratedSecurity.CheckedChanged += chkSqlServerUseIntegratedSecurity_CheckedChanged;
                }
            }
        }

        internal TextBox txtSqlServerDatabase;
        internal TextBox txtSqlServerName;
        private CheckBox _chkUseSqlServerDBToCacheData;

        internal CheckBox chkUseSqlServerDBToCacheData
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkUseSqlServerDBToCacheData;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkUseSqlServerDBToCacheData != null)
                {
                    _chkUseSqlServerDBToCacheData.CheckedChanged -= chkUseSqlServerDBToCacheData_CheckedChanged;
                }

                _chkUseSqlServerDBToCacheData = value;
                if (_chkUseSqlServerDBToCacheData != null)
                {
                    _chkUseSqlServerDBToCacheData.CheckedChanged += chkUseSqlServerDBToCacheData_CheckedChanged;
                }
            }
        }

        internal GroupBox fraUniquenessBinningOptions;
        internal Label lblPeptideUniquenessMassMode;
        private TextBox _txtUniquenessBinWidth;

        internal TextBox txtUniquenessBinWidth
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtUniquenessBinWidth;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtUniquenessBinWidth != null)
                {
                    _txtUniquenessBinWidth.KeyPress -= txtUniquenessBinWidth_KeyPress;
                }

                _txtUniquenessBinWidth = value;
                if (_txtUniquenessBinWidth != null)
                {
                    _txtUniquenessBinWidth.KeyPress += txtUniquenessBinWidth_KeyPress;
                }
            }
        }

        internal Label lblUniquenessBinWidth;
        private CheckBox _chkAutoComputeRangeForBinning;

        internal CheckBox chkAutoComputeRangeForBinning
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkAutoComputeRangeForBinning;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkAutoComputeRangeForBinning != null)
                {
                    _chkAutoComputeRangeForBinning.CheckedChanged -= chkAutoComputeRangeForBinning_CheckedChanged;
                }

                _chkAutoComputeRangeForBinning = value;
                if (_chkAutoComputeRangeForBinning != null)
                {
                    _chkAutoComputeRangeForBinning.CheckedChanged += chkAutoComputeRangeForBinning_CheckedChanged;
                }
            }
        }

        private TextBox _txtUniquenessBinEndMass;

        internal TextBox txtUniquenessBinEndMass
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtUniquenessBinEndMass;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtUniquenessBinEndMass != null)
                {
                    _txtUniquenessBinEndMass.KeyPress -= txtUniquenessBinEndMass_KeyPress;
                }

                _txtUniquenessBinEndMass = value;
                if (_txtUniquenessBinEndMass != null)
                {
                    _txtUniquenessBinEndMass.KeyPress += txtUniquenessBinEndMass_KeyPress;
                }
            }
        }

        internal Label lblUniquenessBinEndMass;
        private TextBox _txtUniquenessBinStartMass;

        internal TextBox txtUniquenessBinStartMass
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtUniquenessBinStartMass;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtUniquenessBinStartMass != null)
                {
                    _txtUniquenessBinStartMass.KeyPress -= txtUniquenessBinStartMass_KeyPress;
                }

                _txtUniquenessBinStartMass = value;
                if (_txtUniquenessBinStartMass != null)
                {
                    _txtUniquenessBinStartMass.KeyPress += txtUniquenessBinStartMass_KeyPress;
                }
            }
        }

        internal Label lblUniquenessBinStartMass;
        internal Label lblUniquenessStatsNote;
        private Button _cmdGenerateUniquenessStats;

        internal Button cmdGenerateUniquenessStats
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdGenerateUniquenessStats;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdGenerateUniquenessStats != null)
                {
                    _cmdGenerateUniquenessStats.Click -= cmdGenerateUniquenessStats_Click;
                }

                _cmdGenerateUniquenessStats = value;
                if (_cmdGenerateUniquenessStats != null)
                {
                    _cmdGenerateUniquenessStats.Click += cmdGenerateUniquenessStats_Click;
                }
            }
        }

        internal CheckBox chkAssumeInputFileIsDigested;
        private TextBox _txtProteinScramblingLoopCount;

        internal TextBox txtProteinScramblingLoopCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtProteinScramblingLoopCount;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtProteinScramblingLoopCount != null)
                {
                    _txtProteinScramblingLoopCount.KeyPress -= txtProteinScramblingLoopCount_KeyPress;
                }

                _txtProteinScramblingLoopCount = value;
                if (_txtProteinScramblingLoopCount != null)
                {
                    _txtProteinScramblingLoopCount.KeyPress += txtProteinScramblingLoopCount_KeyPress;
                }
            }
        }

        internal Label lblSamplingPercentageUnits;
        private TextBox _txtMaxpISequenceLength;

        internal TextBox txtMaxpISequenceLength
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtMaxpISequenceLength;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtMaxpISequenceLength != null)
                {
                    _txtMaxpISequenceLength.KeyDown -= txtMaxpISequenceLength_KeyDown;
                    _txtMaxpISequenceLength.KeyPress -= txtMaxpISequenceLength_KeyPress;
                    _txtMaxpISequenceLength.Validating -= txtMaxpISequenceLength_Validating;
                    _txtMaxpISequenceLength.Validated -= txtMaxpISequenceLength_Validated;
                }

                _txtMaxpISequenceLength = value;
                if (_txtMaxpISequenceLength != null)
                {
                    _txtMaxpISequenceLength.KeyDown += txtMaxpISequenceLength_KeyDown;
                    _txtMaxpISequenceLength.KeyPress += txtMaxpISequenceLength_KeyPress;
                    _txtMaxpISequenceLength.Validating += txtMaxpISequenceLength_Validating;
                    _txtMaxpISequenceLength.Validated += txtMaxpISequenceLength_Validated;
                }
            }
        }

        internal Label lblProteinReversalSamplingPercentage;
        internal Label lblMaxpISequenceLength;
        private CheckBox _chkMaxpIModeEnabled;

        internal CheckBox chkMaxpIModeEnabled
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkMaxpIModeEnabled;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkMaxpIModeEnabled != null)
                {
                    _chkMaxpIModeEnabled.CheckedChanged -= chkMaxpIModeEnabled_CheckedChanged;
                }

                _chkMaxpIModeEnabled = value;
                if (_chkMaxpIModeEnabled != null)
                {
                    _chkMaxpIModeEnabled.CheckedChanged += chkMaxpIModeEnabled_CheckedChanged;
                }
            }
        }

        internal GroupBox frapIAndHydrophobicity;
        internal Label lblHydrophobicityMode;
        private ComboBox _cboHydrophobicityMode;

        internal ComboBox cboHydrophobicityMode
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cboHydrophobicityMode;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cboHydrophobicityMode != null)
                {
                    _cboHydrophobicityMode.SelectedIndexChanged -= cboHydrophobicityMode_SelectedIndexChanged;
                }

                _cboHydrophobicityMode = value;
                if (_cboHydrophobicityMode != null)
                {
                    _cboHydrophobicityMode.SelectedIndexChanged += cboHydrophobicityMode_SelectedIndexChanged;
                }
            }
        }

        internal TextBox txtpIStats;
        private TextBox _txtSequenceForpI;

        internal TextBox txtSequenceForpI
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtSequenceForpI;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtSequenceForpI != null)
                {
                    _txtSequenceForpI.TextChanged -= txtSequenceForpI_TextChanged;
                }

                _txtSequenceForpI = value;
                if (_txtSequenceForpI != null)
                {
                    _txtSequenceForpI.TextChanged += txtSequenceForpI_TextChanged;
                }
            }
        }

        internal Label lblSequenceForpI;
        internal GroupBox fraDelimitedFileOptions;
        internal ComboBox cboInputFileColumnOrdering;
        internal Label lblInputFileColumnOrdering;
        internal TextBox txtInputFileColumnDelimiter;
        internal Label lblInputFileColumnDelimiter;
        private ComboBox _cboInputFileColumnDelimiter;

        internal ComboBox cboInputFileColumnDelimiter
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cboInputFileColumnDelimiter;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cboInputFileColumnDelimiter != null)
                {
                    _cboInputFileColumnDelimiter.SelectedIndexChanged -= cboInputFileColumnDelimiter_SelectedIndexChanged;
                }

                _cboInputFileColumnDelimiter = value;
                if (_cboInputFileColumnDelimiter != null)
                {
                    _cboInputFileColumnDelimiter.SelectedIndexChanged += cboInputFileColumnDelimiter_SelectedIndexChanged;
                }
            }
        }

        internal TabPage TabPageFileFormatOptions;
        internal TabControl tbsOptions;
        internal TabPage TabPageParseAndDigest;
        internal GroupBox fraProcessingOptions;
        private TextBox _txtProteinReversalSamplingPercentage;

        internal TextBox txtProteinReversalSamplingPercentage
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtProteinReversalSamplingPercentage;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtProteinReversalSamplingPercentage != null)
                {
                    _txtProteinReversalSamplingPercentage.KeyPress -= txtProteinReversalSamplingPercentage_KeyPress;
                }

                _txtProteinReversalSamplingPercentage = value;
                if (_txtProteinReversalSamplingPercentage != null)
                {
                    _txtProteinReversalSamplingPercentage.KeyPress += txtProteinReversalSamplingPercentage_KeyPress;
                }
            }
        }

        internal Label lbltxtAddnlRefAccessionSepChar;
        private CheckBox _chkLookForAddnlRefInDescription;

        internal CheckBox chkLookForAddnlRefInDescription
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkLookForAddnlRefInDescription;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkLookForAddnlRefInDescription != null)
                {
                    _chkLookForAddnlRefInDescription.CheckedChanged -= chkLookForAddnlRefInDescription_CheckedChanged;
                }

                _chkLookForAddnlRefInDescription = value;
                if (_chkLookForAddnlRefInDescription != null)
                {
                    _chkLookForAddnlRefInDescription.CheckedChanged += chkLookForAddnlRefInDescription_CheckedChanged;
                }
            }
        }

        private ComboBox _cboProteinReversalOptions;

        internal ComboBox cboProteinReversalOptions
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cboProteinReversalOptions;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cboProteinReversalOptions != null)
                {
                    _cboProteinReversalOptions.SelectedIndexChanged -= cboProteinReversalOptions_SelectedIndexChanged;
                }

                _cboProteinReversalOptions = value;
                if (_cboProteinReversalOptions != null)
                {
                    _cboProteinReversalOptions.SelectedIndexChanged += cboProteinReversalOptions_SelectedIndexChanged;
                }
            }
        }

        internal Label lblProteinReversalOptions;
        private CheckBox _chkDigestProteins;

        internal CheckBox chkDigestProteins
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkDigestProteins;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkDigestProteins != null)
                {
                    _chkDigestProteins.CheckedChanged -= chkDigestProteins_CheckedChanged;
                }

                _chkDigestProteins = value;
                if (_chkDigestProteins != null)
                {
                    _chkDigestProteins.CheckedChanged += chkDigestProteins_CheckedChanged;
                }
            }
        }

        internal Label lblAddnlRefSepChar;
        internal TextBox txtAddnlRefAccessionSepChar;
        internal TextBox txtAddnlRefSepChar;
        private CheckBox _chkCreateFastaOutputFile;

        internal CheckBox chkCreateFastaOutputFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkCreateFastaOutputFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkCreateFastaOutputFile != null)
                {
                    _chkCreateFastaOutputFile.CheckedChanged -= chkCreateFastaOutputFile_CheckedChanged;
                }

                _chkCreateFastaOutputFile = value;
                if (_chkCreateFastaOutputFile != null)
                {
                    _chkCreateFastaOutputFile.CheckedChanged += chkCreateFastaOutputFile_CheckedChanged;
                }
            }
        }

        internal GroupBox fraCalculationOptions;
        internal Label lblMassMode;
        private ComboBox _cboElementMassMode;

        internal ComboBox cboElementMassMode
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cboElementMassMode;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cboElementMassMode != null)
                {
                    _cboElementMassMode.SelectedIndexChanged -= cboElementMassMode_SelectedIndexChanged;
                }

                _cboElementMassMode = value;
                if (_cboElementMassMode != null)
                {
                    _cboElementMassMode.SelectedIndexChanged += cboElementMassMode_SelectedIndexChanged;
                }
            }
        }

        internal CheckBox chkExcludeProteinSequence;
        private CheckBox _chkComputepIandNET;

        internal CheckBox chkComputepIandNET
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkComputepIandNET;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkComputepIandNET != null)
                {
                    _chkComputepIandNET.CheckedChanged -= chkComputepI_CheckedChanged;
                }

                _chkComputepIandNET = value;
                if (_chkComputepIandNET != null)
                {
                    _chkComputepIandNET.CheckedChanged += chkComputepI_CheckedChanged;
                }
            }
        }

        internal CheckBox chkIncludeXResidues;
        internal CheckBox chkComputeProteinMass;
        internal GroupBox fraDigestionOptions;
        internal TextBox txtDigestProteinsMaximumpI;
        internal Label lblDigestProteinsMaximumpI;
        internal TextBox txtDigestProteinsMinimumpI;
        internal Label lblDigestProteinsMinimumpI;
        internal CheckBox chkGenerateUniqueIDValues;
        internal CheckBox chkCysPeptidesOnly;
        private TextBox _txtDigestProteinsMinimumResidueCount;

        internal TextBox txtDigestProteinsMinimumResidueCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtDigestProteinsMinimumResidueCount;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtDigestProteinsMinimumResidueCount != null)
                {
                    _txtDigestProteinsMinimumResidueCount.KeyPress -= txtDigestProteinsMinimumResidueCount_KeyPress;
                }

                _txtDigestProteinsMinimumResidueCount = value;
                if (_txtDigestProteinsMinimumResidueCount != null)
                {
                    _txtDigestProteinsMinimumResidueCount.KeyPress += txtDigestProteinsMinimumResidueCount_KeyPress;
                }
            }
        }

        internal Label lblDigestProteinsMinimumResidueCount;
        private TextBox _txtDigestProteinsMaximumMissedCleavages;

        internal TextBox txtDigestProteinsMaximumMissedCleavages
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtDigestProteinsMaximumMissedCleavages;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtDigestProteinsMaximumMissedCleavages != null)
                {
                    _txtDigestProteinsMaximumMissedCleavages.KeyPress -= txtDigestProteinsMaximumMissedCleavages_KeyPress;
                }

                _txtDigestProteinsMaximumMissedCleavages = value;
                if (_txtDigestProteinsMaximumMissedCleavages != null)
                {
                    _txtDigestProteinsMaximumMissedCleavages.KeyPress += txtDigestProteinsMaximumMissedCleavages_KeyPress;
                }
            }
        }

        internal Label lblDigestProteinsMaximumMissedCleavages;
        private TextBox _txtDigestProteinsMaximumMass;

        internal TextBox txtDigestProteinsMaximumMass
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtDigestProteinsMaximumMass;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtDigestProteinsMaximumMass != null)
                {
                    _txtDigestProteinsMaximumMass.KeyPress -= txtDigestProteinsMaximumMass_KeyPress;
                }

                _txtDigestProteinsMaximumMass = value;
                if (_txtDigestProteinsMaximumMass != null)
                {
                    _txtDigestProteinsMaximumMass.KeyPress += txtDigestProteinsMaximumMass_KeyPress;
                }
            }
        }

        internal Label lblDigestProteinsMaximumMass;
        private TextBox _txtDigestProteinsMinimumMass;

        internal TextBox txtDigestProteinsMinimumMass
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtDigestProteinsMinimumMass;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtDigestProteinsMinimumMass != null)
                {
                    _txtDigestProteinsMinimumMass.KeyPress -= txtDigestProteinsMinimumMass_KeyPress;
                }

                _txtDigestProteinsMinimumMass = value;
                if (_txtDigestProteinsMinimumMass != null)
                {
                    _txtDigestProteinsMinimumMass.KeyPress += txtDigestProteinsMinimumMass_KeyPress;
                }
            }
        }

        internal Label lblDigestProteinsMinimumMass;
        internal ComboBox cboCleavageRuleType;
        internal CheckBox chkIncludeDuplicateSequences;
        private Button _cmdParseInputFile;

        internal Button cmdParseInputFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdParseInputFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdParseInputFile != null)
                {
                    _cmdParseInputFile.Click -= cmdParseInputFile_Click;
                }

                _cmdParseInputFile = value;
                if (_cmdParseInputFile != null)
                {
                    _cmdParseInputFile.Click += cmdParseInputFile_Click;
                }
            }
        }

        internal TabPage TabPagePeakMatchingThresholds;
        private CheckBox _chkAutoDefineSLiCScoreTolerances;

        internal CheckBox chkAutoDefineSLiCScoreTolerances
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkAutoDefineSLiCScoreTolerances;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkAutoDefineSLiCScoreTolerances != null)
                {
                    _chkAutoDefineSLiCScoreTolerances.CheckedChanged -= chkAutoDefineSLiCScoreTolerances_CheckedChanged;
                }

                _chkAutoDefineSLiCScoreTolerances = value;
                if (_chkAutoDefineSLiCScoreTolerances != null)
                {
                    _chkAutoDefineSLiCScoreTolerances.CheckedChanged += chkAutoDefineSLiCScoreTolerances_CheckedChanged;
                }
            }
        }

        private Button _cmdPastePMThresholdsList;

        internal Button cmdPastePMThresholdsList
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdPastePMThresholdsList;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdPastePMThresholdsList != null)
                {
                    _cmdPastePMThresholdsList.Click -= cmdPastePMThresholdsList_Click;
                }

                _cmdPastePMThresholdsList = value;
                if (_cmdPastePMThresholdsList != null)
                {
                    _cmdPastePMThresholdsList.Click += cmdPastePMThresholdsList_Click;
                }
            }
        }

        internal ComboBox cboPMPredefinedThresholds;
        private Button _cmdPMThresholdsAutoPopulate;

        internal Button cmdPMThresholdsAutoPopulate
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdPMThresholdsAutoPopulate;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdPMThresholdsAutoPopulate != null)
                {
                    _cmdPMThresholdsAutoPopulate.Click -= cmdPMThresholdsAutoPopulate_Click;
                }

                _cmdPMThresholdsAutoPopulate = value;
                if (_cmdPMThresholdsAutoPopulate != null)
                {
                    _cmdPMThresholdsAutoPopulate.Click += cmdPMThresholdsAutoPopulate_Click;
                }
            }
        }

        private Button _cmdClearPMThresholdsList;

        internal Button cmdClearPMThresholdsList
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdClearPMThresholdsList;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdClearPMThresholdsList != null)
                {
                    _cmdClearPMThresholdsList.Click -= cmdClearPMThresholdsList_Click;
                }

                _cmdClearPMThresholdsList = value;
                if (_cmdClearPMThresholdsList != null)
                {
                    _cmdClearPMThresholdsList.Click += cmdClearPMThresholdsList_Click;
                }
            }
        }

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
        private Button _cmdAbortProcessing;

        internal Button cmdAbortProcessing
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdAbortProcessing;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdAbortProcessing != null)
                {
                    _cmdAbortProcessing.Click -= cmdAbortProcessing_Click;
                }

                _cmdAbortProcessing = value;
                if (_cmdAbortProcessing != null)
                {
                    _cmdAbortProcessing.Click += cmdAbortProcessing_Click;
                }
            }
        }

        private MenuItem _mnuHelpAboutElutionTime;

        internal MenuItem mnuHelpAboutElutionTime
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuHelpAboutElutionTime;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuHelpAboutElutionTime != null)
                {
                    _mnuHelpAboutElutionTime.Click -= mnuHelpAboutElutionTime_Click;
                }

                _mnuHelpAboutElutionTime = value;
                if (_mnuHelpAboutElutionTime != null)
                {
                    _mnuHelpAboutElutionTime.Click += mnuHelpAboutElutionTime_Click;
                }
            }
        }

        private ComboBox _cboInputFileFormat;

        internal ComboBox cboInputFileFormat
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cboInputFileFormat;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cboInputFileFormat != null)
                {
                    _cboInputFileFormat.SelectedIndexChanged -= cboInputFileFormat_SelectedIndexChanged;
                }

                _cboInputFileFormat = value;
                if (_cboInputFileFormat != null)
                {
                    _cboInputFileFormat.SelectedIndexChanged += cboInputFileFormat_SelectedIndexChanged;
                }
            }
        }

        internal Label lblInputFileFormat;
        private Button _cmdSelectFile;

        internal Button cmdSelectFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdSelectFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdSelectFile != null)
                {
                    _cmdSelectFile.Click -= cmdSelectFile_Click;
                }

                _cmdSelectFile = value;
                if (_cmdSelectFile != null)
                {
                    _cmdSelectFile.Click += cmdSelectFile_Click;
                }
            }
        }

        internal GroupBox fraInputFilePath;
        private Button _cmdValidateFastaFile;

        internal Button cmdValidateFastaFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdValidateFastaFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdValidateFastaFile != null)
                {
                    _cmdValidateFastaFile.Click -= cmdValidateFastaFile_Click;
                }

                _cmdValidateFastaFile = value;
                if (_cmdValidateFastaFile != null)
                {
                    _cmdValidateFastaFile.Click += cmdValidateFastaFile_Click;
                }
            }
        }

        internal CheckBox chkEnableLogging;
        private MenuItem _mnuFileSelectOutputFile;

        internal MenuItem mnuFileSelectOutputFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuFileSelectOutputFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuFileSelectOutputFile != null)
                {
                    _mnuFileSelectOutputFile.Click -= mnuFileSelectOutputFile_Click;
                }

                _mnuFileSelectOutputFile = value;
                if (_mnuFileSelectOutputFile != null)
                {
                    _mnuFileSelectOutputFile.Click += mnuFileSelectOutputFile_Click;
                }
            }
        }

        private Button _cmdSelectOutputFile;

        internal Button cmdSelectOutputFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdSelectOutputFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdSelectOutputFile != null)
                {
                    _cmdSelectOutputFile.Click -= cmdSelectOutputFile_Click;
                }

                _cmdSelectOutputFile = value;
                if (_cmdSelectOutputFile != null)
                {
                    _cmdSelectOutputFile.Click += cmdSelectOutputFile_Click;
                }
            }
        }

        internal MenuItem mnuFileSep1;
        internal MenuItem mnuFile;
        private MenuItem _mnuFileSelectInputFile;

        internal MenuItem mnuFileSelectInputFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuFileSelectInputFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuFileSelectInputFile != null)
                {
                    _mnuFileSelectInputFile.Click -= mnuFileSelectInputFile_Click;
                }

                _mnuFileSelectInputFile = value;
                if (_mnuFileSelectInputFile != null)
                {
                    _mnuFileSelectInputFile.Click += mnuFileSelectInputFile_Click;
                }
            }
        }

        private MenuItem _mnuFileSaveDefaultOptions;

        internal MenuItem mnuFileSaveDefaultOptions
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuFileSaveDefaultOptions;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuFileSaveDefaultOptions != null)
                {
                    _mnuFileSaveDefaultOptions.Click -= mnuFileSaveDefaultOptions_Click;
                }

                _mnuFileSaveDefaultOptions = value;
                if (_mnuFileSaveDefaultOptions != null)
                {
                    _mnuFileSaveDefaultOptions.Click += mnuFileSaveDefaultOptions_Click;
                }
            }
        }

        internal MenuItem mnuFileSep2;
        private MenuItem _mnuFileExit;

        internal MenuItem mnuFileExit
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuFileExit;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuFileExit != null)
                {
                    _mnuFileExit.Click -= mnuFileExit_Click;
                }

                _mnuFileExit = value;
                if (_mnuFileExit != null)
                {
                    _mnuFileExit.Click += mnuFileExit_Click;
                }
            }
        }

        internal TextBox txtProteinOutputFilePath;
        internal CheckBox chkIncludePrefixAndSuffixResidues;
        private MenuItem _mnuEditResetOptions;

        internal MenuItem mnuEditResetOptions
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditResetOptions;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditResetOptions != null)
                {
                    _mnuEditResetOptions.Click -= mnuEditResetOptions_Click;
                }

                _mnuEditResetOptions = value;
                if (_mnuEditResetOptions != null)
                {
                    _mnuEditResetOptions.Click += mnuEditResetOptions_Click;
                }
            }
        }

        internal MenuItem mnuHelp;
        private MenuItem _mnuHelpAbout;

        internal MenuItem mnuHelpAbout
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuHelpAbout;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuHelpAbout != null)
                {
                    _mnuHelpAbout.Click -= mnuHelpAbout_Click;
                }

                _mnuHelpAbout = value;
                if (_mnuHelpAbout != null)
                {
                    _mnuHelpAbout.Click += mnuHelpAbout_Click;
                }
            }
        }

        internal MenuItem mnuEditSep1;
        private MenuItem _mnuEditMakeUniquenessStats;

        internal MenuItem mnuEditMakeUniquenessStats
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditMakeUniquenessStats;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditMakeUniquenessStats != null)
                {
                    _mnuEditMakeUniquenessStats.Click -= mnuEditMakeUniquenessStats_Click;
                }

                _mnuEditMakeUniquenessStats = value;
                if (_mnuEditMakeUniquenessStats != null)
                {
                    _mnuEditMakeUniquenessStats.Click += mnuEditMakeUniquenessStats_Click;
                }
            }
        }

        internal MenuItem mnuEdit;
        private MenuItem _mnuEditParseFile;

        internal MenuItem mnuEditParseFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditParseFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditParseFile != null)
                {
                    _mnuEditParseFile.Click -= mnuEditParseFile_Click;
                }

                _mnuEditParseFile = value;
                if (_mnuEditParseFile != null)
                {
                    _mnuEditParseFile.Click += mnuEditParseFile_Click;
                }
            }
        }

        internal MainMenu MainMenuControl;
        internal Label lblOutputFileFieldDelimiter;
        private ComboBox _cboOutputFileFieldDelimiter;

        internal ComboBox cboOutputFileFieldDelimiter
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cboOutputFileFieldDelimiter;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cboOutputFileFieldDelimiter != null)
                {
                    _cboOutputFileFieldDelimiter.SelectedIndexChanged -= cboOutputFileFieldDelimiter_SelectedIndexChanged;
                }

                _cboOutputFileFieldDelimiter = value;
                if (_cboOutputFileFieldDelimiter != null)
                {
                    _cboOutputFileFieldDelimiter.SelectedIndexChanged += cboOutputFileFieldDelimiter_SelectedIndexChanged;
                }
            }
        }

        internal TextBox txtOutputFileFieldDelimiter;
        internal GroupBox fraOutputTextOptions;
        internal CheckBox chkTruncateProteinDescription;
        private CheckBox _chkComputeSequenceHashValues;

        internal CheckBox chkComputeSequenceHashValues
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkComputeSequenceHashValues;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkComputeSequenceHashValues != null)
                {
                    _chkComputeSequenceHashValues.CheckedChanged -= chkComputeSequenceHashValues_CheckedChanged;
                }

                _chkComputeSequenceHashValues = value;
                if (_chkComputeSequenceHashValues != null)
                {
                    _chkComputeSequenceHashValues.CheckedChanged += chkComputeSequenceHashValues_CheckedChanged;
                }
            }
        }

        internal CheckBox chkComputeSequenceHashIgnoreILDiff;
        internal CheckBox chkExcludeProteinDescription;
        private Button _cmdNETInfo;

        internal Button cmdNETInfo
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdNETInfo;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdNETInfo != null)
                {
                    _cmdNETInfo.Click -= cmdNETInfo_Click;
                }

                _cmdNETInfo = value;
                if (_cmdNETInfo != null)
                {
                    _cmdNETInfo.Click += cmdNETInfo_Click;
                }
            }
        }

        internal ComboBox cboCysTreatmentMode;
        internal Label lblCysTreatment;
        internal ComboBox cboFragmentMassMode;
        internal Label lblFragmentMassMode;
    }
}