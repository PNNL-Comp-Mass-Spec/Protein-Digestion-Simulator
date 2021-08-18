using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace ProteinDigestionSimulator
{
    [DesignerGenerated()]
    public partial class FastaValidation : Form
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
            chkWrapLongResidueLines = new CheckBox();
            chkWrapLongResidueLines.CheckedChanged += new EventHandler(chkWrapLongResidueLines_CheckedChanged);
            chkSplitOutMultipleRefsForKnownAccession = new CheckBox();
            chkTruncateLongProteinNames = new CheckBox();
            txtResults = new TextBox();
            txtResults.KeyDown += new KeyEventHandler(txtResults_KeyDown);
            TabPageErrorOptions = new TabPage();
            chkSaveBasicProteinHashInfoFile = new CheckBox();
            lblProteinNameLengthUnits = new Label();
            lblProteinNameLength2 = new Label();
            txtProteinNameLengthMaximum = new TextBox();
            txtProteinNameLengthMaximum.KeyPress += new KeyPressEventHandler(txtProteinNameLengthMaximum_KeyPress);
            txtProteinNameLengthMinimum = new TextBox();
            txtProteinNameLengthMinimum.KeyPress += new KeyPressEventHandler(txtProteinNameLengthMinimum_KeyPress);
            lblProteinNameLength = new Label();
            txtMaximumResiduesPerLine = new TextBox();
            txtMaximumResiduesPerLine.KeyPress += new KeyPressEventHandler(txtMaximumResiduesPerLine_KeyPress);
            lblMaximumResiduesPerLine = new Label();
            chkAllowAsteriskInResidues = new CheckBox();
            chkCheckForDuplicateProteinInfo = new CheckBox();
            txtMaxFileErrorsToTrack = new TextBox();
            txtMaxFileErrorsToTrack.KeyPress += new KeyPressEventHandler(txtMaxFileErrorsToTrack_KeyPress1);
            lblMaxFileErrorsToTrack = new Label();
            chkLogResults = new CheckBox();
            chkConsolidateDupsIgnoreILDiff = new CheckBox();
            chkRemoveInvalidResidues = new CheckBox();
            TabPageNewFastaOptions = new TabPage();
            txtResiduesPerLineForWrap = new TextBox();
            txtResiduesPerLineForWrap.KeyPress += new KeyPressEventHandler(txtResiduesPerLineForWrap_TextChanged);
            lblResiduesPerLineForWrap = new Label();
            chkKeepDuplicateNamedProteins = new CheckBox();
            chkKeepDuplicateNamedProteins.CheckedChanged += new EventHandler(chkKeepDuplicateNamedProteins_CheckedChanged);
            chkConsolidateDuplicateProteinSeqs = new CheckBox();
            chkConsolidateDuplicateProteinSeqs.CheckedChanged += new EventHandler(chkConsolidateDuplicateProteinSeqs_CheckedChanged);
            chkRenameDuplicateProteins = new CheckBox();
            chkRenameDuplicateProteins.CheckedChanged += new EventHandler(chkRenameDuplicateProteins_CheckedChanged);
            chkSplitOutMultipleRefsInProteinName = new CheckBox();
            txtInvalidProteinNameCharsToRemove = new TextBox();
            lblInvalidProteinNameCharsToRemove = new Label();
            txtLongProteinNameSplitChars = new TextBox();
            lblLongProteinNameSplitChars = new Label();
            chkGenerateFixedFastaFile = new CheckBox();
            chkGenerateFixedFastaFile.CheckedChanged += new EventHandler(chkGenerateFixedFastaFile_CheckedChanged);
            lblCustomRulesFile = new Label();
            TabPageRuleOptions = new TabPage();
            cmdCreateDefaultValidationRulesFile = new Button();
            cmdCreateDefaultValidationRulesFile.Click += new EventHandler(cmdCreateDefaultValidationRulesFile_Click_1);
            txtCustomValidationRulesFilePath = new TextBox();
            txtCustomValidationRulesFilePath.TextChanged += new EventHandler(txtCustomValidationRulesFilePath_TextChanged);
            cmdSelectCustomRulesFile = new Button();
            cmdSelectCustomRulesFile.Click += new EventHandler(cmdSelectCustomRulesFile_Click);
            tbsOptions = new TabControl();
            mnuEditCopyAllWarnings = new MenuItem();
            mnuEditCopyAllWarnings.Click += new EventHandler(mnuEditCopyAllWarnings_Click);
            MainMenuControl = new MainMenu(components);
            mnuFile = new MenuItem();
            mnuFileExit = new MenuItem();
            mnuFileExit.Click += new EventHandler(mnuFileExit_Click);
            mnuEdit = new MenuItem();
            mnuEditCopyAllResults = new MenuItem();
            mnuEditCopyAllResults.Click += new EventHandler(mnuEditCopyAllResults_Click);
            mnuEditSep1 = new MenuItem();
            mnuEditCopySummary = new MenuItem();
            mnuEditCopySummary.Click += new EventHandler(mnuEditCopySummary_Click);
            mnuEditCopyAllErrors = new MenuItem();
            mnuEditCopyAllErrors.Click += new EventHandler(mnuEditCopyAllErrors_Click);
            mnuEditSep2 = new MenuItem();
            mnuEditFontSizeDecrease = new MenuItem();
            mnuEditFontSizeDecrease.Click += new EventHandler(mnuEditFontSizeDecrease_Click);
            mnuEditFontSizeIncrease = new MenuItem();
            mnuEditFontSizeIncrease.Click += new EventHandler(mnuEditFontSizeIncrease_Click);
            MenuItem2 = new MenuItem();
            mnuEditResetToDefaults = new MenuItem();
            mnuEditResetToDefaults.Click += new EventHandler(mnuEditResetToDefaults_Click);
            MenuItem1 = new MenuItem();
            mnuHelpAbout = new MenuItem();
            mnuHelpAbout.Click += new EventHandler(mnuHelpAbout_Click);
            dgWarnings = new DataGrid();
            lblFilterData = new Label();
            txtFilterData = new TextBox();
            txtFilterData.Validated += new EventHandler(txtFilterData_Validated);
            txtFilterData.KeyDown += new KeyEventHandler(txtFilterData_KeyDown);
            pbarProgress = new ProgressBar();
            cmdValidateFastaFile = new Button();
            cmdValidateFastaFile.Click += new EventHandler(cmdValidateFastaFile_Click);
            dgErrors = new DataGrid();
            cmdCancel = new Button();
            cmdCancel.Click += new EventHandler(cmdCancel_Click);
            TabPageErrorOptions.SuspendLayout();
            TabPageNewFastaOptions.SuspendLayout();
            TabPageRuleOptions.SuspendLayout();
            tbsOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgWarnings).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgErrors).BeginInit();
            SuspendLayout();
            // 
            // chkWrapLongResidueLines
            // 
            chkWrapLongResidueLines.Checked = true;
            chkWrapLongResidueLines.CheckState = CheckState.Checked;
            chkWrapLongResidueLines.Location = new Point(8, 57);
            chkWrapLongResidueLines.Margin = new Padding(4);
            chkWrapLongResidueLines.Name = "chkWrapLongResidueLines";
            chkWrapLongResidueLines.Size = new Size(203, 21);
            chkWrapLongResidueLines.TabIndex = 7;
            chkWrapLongResidueLines.Text = "Wrap long residue lines";
            // 
            // chkSplitOutMultipleRefsForKnownAccession
            // 
            chkSplitOutMultipleRefsForKnownAccession.Location = new Point(239, 81);
            chkSplitOutMultipleRefsForKnownAccession.Margin = new Padding(4);
            chkSplitOutMultipleRefsForKnownAccession.Name = "chkSplitOutMultipleRefsForKnownAccession";
            chkSplitOutMultipleRefsForKnownAccession.Size = new Size(256, 40);
            chkSplitOutMultipleRefsForKnownAccession.TabIndex = 9;
            chkSplitOutMultipleRefsForKnownAccession.Text = "Split out multiple references only if IPI, GI, or JGI";
            // 
            // chkTruncateLongProteinNames
            // 
            chkTruncateLongProteinNames.Checked = true;
            chkTruncateLongProteinNames.CheckState = CheckState.Checked;
            chkTruncateLongProteinNames.Location = new Point(8, 91);
            chkTruncateLongProteinNames.Margin = new Padding(4);
            chkTruncateLongProteinNames.Name = "chkTruncateLongProteinNames";
            chkTruncateLongProteinNames.Size = new Size(235, 21);
            chkTruncateLongProteinNames.TabIndex = 5;
            chkTruncateLongProteinNames.Text = "Truncate long protein names";
            // 
            // txtResults
            // 
            txtResults.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtResults.Location = new Point(584, 9);
            txtResults.Margin = new Padding(4);
            txtResults.Multiline = true;
            txtResults.Name = "txtResults";
            txtResults.ScrollBars = ScrollBars.Both;
            txtResults.Size = new Size(637, 186);
            txtResults.TabIndex = 10;
            txtResults.WordWrap = false;
            // 
            // TabPageErrorOptions
            // 
            TabPageErrorOptions.Controls.Add(chkSaveBasicProteinHashInfoFile);
            TabPageErrorOptions.Controls.Add(lblProteinNameLengthUnits);
            TabPageErrorOptions.Controls.Add(lblProteinNameLength2);
            TabPageErrorOptions.Controls.Add(txtProteinNameLengthMaximum);
            TabPageErrorOptions.Controls.Add(txtProteinNameLengthMinimum);
            TabPageErrorOptions.Controls.Add(lblProteinNameLength);
            TabPageErrorOptions.Controls.Add(txtMaximumResiduesPerLine);
            TabPageErrorOptions.Controls.Add(lblMaximumResiduesPerLine);
            TabPageErrorOptions.Controls.Add(chkAllowAsteriskInResidues);
            TabPageErrorOptions.Controls.Add(chkCheckForDuplicateProteinInfo);
            TabPageErrorOptions.Controls.Add(txtMaxFileErrorsToTrack);
            TabPageErrorOptions.Controls.Add(lblMaxFileErrorsToTrack);
            TabPageErrorOptions.Controls.Add(chkLogResults);
            TabPageErrorOptions.Location = new Point(4, 25);
            TabPageErrorOptions.Margin = new Padding(4);
            TabPageErrorOptions.Name = "TabPageErrorOptions";
            TabPageErrorOptions.Size = new Size(537, 228);
            TabPageErrorOptions.TabIndex = 0;
            TabPageErrorOptions.Text = "Error Options";
            // 
            // chkSaveBasicProteinHashInfoFile
            // 
            chkSaveBasicProteinHashInfoFile.Location = new Point(293, 119);
            chkSaveBasicProteinHashInfoFile.Margin = new Padding(4);
            chkSaveBasicProteinHashInfoFile.Name = "chkSaveBasicProteinHashInfoFile";
            chkSaveBasicProteinHashInfoFile.Size = new Size(171, 41);
            chkSaveBasicProteinHashInfoFile.TabIndex = 12;
            chkSaveBasicProteinHashInfoFile.Text = "Save basic protein hash info file";
            // 
            // lblProteinNameLengthUnits
            // 
            lblProteinNameLengthUnits.Location = new Point(407, 65);
            lblProteinNameLengthUnits.Margin = new Padding(4, 0, 4, 0);
            lblProteinNameLengthUnits.Name = "lblProteinNameLengthUnits";
            lblProteinNameLengthUnits.Size = new Size(107, 21);
            lblProteinNameLengthUnits.TabIndex = 8;
            lblProteinNameLengthUnits.Text = "characters";
            // 
            // lblProteinNameLength2
            // 
            lblProteinNameLength2.Location = new Point(311, 65);
            lblProteinNameLength2.Margin = new Padding(4, 0, 4, 0);
            lblProteinNameLength2.Name = "lblProteinNameLength2";
            lblProteinNameLength2.Size = new Size(21, 21);
            lblProteinNameLength2.TabIndex = 6;
            lblProteinNameLength2.Text = "to";
            // 
            // txtProteinNameLengthMaximum
            // 
            txtProteinNameLengthMaximum.Location = new Point(343, 62);
            txtProteinNameLengthMaximum.Margin = new Padding(4);
            txtProteinNameLengthMaximum.Name = "txtProteinNameLengthMaximum";
            txtProteinNameLengthMaximum.Size = new Size(52, 22);
            txtProteinNameLengthMaximum.TabIndex = 7;
            txtProteinNameLengthMaximum.Text = "34";
            // 
            // txtProteinNameLengthMinimum
            // 
            txtProteinNameLengthMinimum.Location = new Point(247, 62);
            txtProteinNameLengthMinimum.Margin = new Padding(4);
            txtProteinNameLengthMinimum.Name = "txtProteinNameLengthMinimum";
            txtProteinNameLengthMinimum.Size = new Size(52, 22);
            txtProteinNameLengthMinimum.TabIndex = 5;
            txtProteinNameLengthMinimum.Text = "3";
            // 
            // lblProteinNameLength
            // 
            lblProteinNameLength.Location = new Point(5, 64);
            lblProteinNameLength.Margin = new Padding(4, 0, 4, 0);
            lblProteinNameLength.Name = "lblProteinNameLength";
            lblProteinNameLength.Size = new Size(235, 21);
            lblProteinNameLength.TabIndex = 4;
            lblProteinNameLength.Text = "Valid protein name length range";
            // 
            // txtMaximumResiduesPerLine
            // 
            txtMaximumResiduesPerLine.Location = new Point(247, 32);
            txtMaximumResiduesPerLine.Margin = new Padding(4);
            txtMaximumResiduesPerLine.Name = "txtMaximumResiduesPerLine";
            txtMaximumResiduesPerLine.Size = new Size(52, 22);
            txtMaximumResiduesPerLine.TabIndex = 3;
            txtMaximumResiduesPerLine.Text = "120";
            // 
            // lblMaximumResiduesPerLine
            // 
            lblMaximumResiduesPerLine.Location = new Point(5, 34);
            lblMaximumResiduesPerLine.Margin = new Padding(4, 0, 4, 0);
            lblMaximumResiduesPerLine.Name = "lblMaximumResiduesPerLine";
            lblMaximumResiduesPerLine.Size = new Size(224, 21);
            lblMaximumResiduesPerLine.TabIndex = 2;
            lblMaximumResiduesPerLine.Text = "Maximum residues per line";
            // 
            // chkAllowAsteriskInResidues
            // 
            chkAllowAsteriskInResidues.Location = new Point(8, 91);
            chkAllowAsteriskInResidues.Margin = new Padding(4);
            chkAllowAsteriskInResidues.Name = "chkAllowAsteriskInResidues";
            chkAllowAsteriskInResidues.Size = new Size(224, 21);
            chkAllowAsteriskInResidues.TabIndex = 9;
            chkAllowAsteriskInResidues.Text = "Allow asterisks in residues";
            // 
            // chkCheckForDuplicateProteinInfo
            // 
            chkCheckForDuplicateProteinInfo.Checked = true;
            chkCheckForDuplicateProteinInfo.CheckState = CheckState.Checked;
            chkCheckForDuplicateProteinInfo.Location = new Point(8, 119);
            chkCheckForDuplicateProteinInfo.Margin = new Padding(4);
            chkCheckForDuplicateProteinInfo.Name = "chkCheckForDuplicateProteinInfo";
            chkCheckForDuplicateProteinInfo.Size = new Size(267, 41);
            chkCheckForDuplicateProteinInfo.TabIndex = 10;
            chkCheckForDuplicateProteinInfo.Text = "Check for duplicate protein names and duplicate protein sequences";
            // 
            // txtMaxFileErrorsToTrack
            // 
            txtMaxFileErrorsToTrack.Location = new Point(385, 2);
            txtMaxFileErrorsToTrack.Margin = new Padding(4);
            txtMaxFileErrorsToTrack.Name = "txtMaxFileErrorsToTrack";
            txtMaxFileErrorsToTrack.Size = new Size(52, 22);
            txtMaxFileErrorsToTrack.TabIndex = 1;
            txtMaxFileErrorsToTrack.Text = "10";
            // 
            // lblMaxFileErrorsToTrack
            // 
            lblMaxFileErrorsToTrack.Location = new Point(5, 5);
            lblMaxFileErrorsToTrack.Margin = new Padding(4, 0, 4, 0);
            lblMaxFileErrorsToTrack.Name = "lblMaxFileErrorsToTrack";
            lblMaxFileErrorsToTrack.Size = new Size(364, 21);
            lblMaxFileErrorsToTrack.TabIndex = 0;
            lblMaxFileErrorsToTrack.Text = "Maximum # of errors or warnings to track in detail";
            // 
            // chkLogResults
            // 
            chkLogResults.Location = new Point(8, 171);
            chkLogResults.Margin = new Padding(4);
            chkLogResults.Name = "chkLogResults";
            chkLogResults.Size = new Size(169, 21);
            chkLogResults.TabIndex = 11;
            chkLogResults.Text = "Log results to file";
            // 
            // chkConsolidateDupsIgnoreILDiff
            // 
            chkConsolidateDupsIgnoreILDiff.Checked = true;
            chkConsolidateDupsIgnoreILDiff.CheckState = CheckState.Checked;
            chkConsolidateDupsIgnoreILDiff.Location = new Point(239, 202);
            chkConsolidateDupsIgnoreILDiff.Margin = new Padding(4);
            chkConsolidateDupsIgnoreILDiff.Name = "chkConsolidateDupsIgnoreILDiff";
            chkConsolidateDupsIgnoreILDiff.Size = new Size(277, 22);
            chkConsolidateDupsIgnoreILDiff.TabIndex = 12;
            chkConsolidateDupsIgnoreILDiff.Text = "Ignore I/L diffs when consolidating";
            // 
            // chkRemoveInvalidResidues
            // 
            chkRemoveInvalidResidues.Location = new Point(8, 203);
            chkRemoveInvalidResidues.Margin = new Padding(4);
            chkRemoveInvalidResidues.Name = "chkRemoveInvalidResidues";
            chkRemoveInvalidResidues.Size = new Size(203, 20);
            chkRemoveInvalidResidues.TabIndex = 8;
            chkRemoveInvalidResidues.Text = "Remove invalid residues";
            // 
            // TabPageNewFastaOptions
            // 
            TabPageNewFastaOptions.Controls.Add(txtResiduesPerLineForWrap);
            TabPageNewFastaOptions.Controls.Add(lblResiduesPerLineForWrap);
            TabPageNewFastaOptions.Controls.Add(chkKeepDuplicateNamedProteins);
            TabPageNewFastaOptions.Controls.Add(chkRemoveInvalidResidues);
            TabPageNewFastaOptions.Controls.Add(chkWrapLongResidueLines);
            TabPageNewFastaOptions.Controls.Add(chkSplitOutMultipleRefsForKnownAccession);
            TabPageNewFastaOptions.Controls.Add(chkTruncateLongProteinNames);
            TabPageNewFastaOptions.Controls.Add(chkConsolidateDupsIgnoreILDiff);
            TabPageNewFastaOptions.Controls.Add(chkConsolidateDuplicateProteinSeqs);
            TabPageNewFastaOptions.Controls.Add(chkRenameDuplicateProteins);
            TabPageNewFastaOptions.Controls.Add(chkSplitOutMultipleRefsInProteinName);
            TabPageNewFastaOptions.Controls.Add(txtInvalidProteinNameCharsToRemove);
            TabPageNewFastaOptions.Controls.Add(lblInvalidProteinNameCharsToRemove);
            TabPageNewFastaOptions.Controls.Add(txtLongProteinNameSplitChars);
            TabPageNewFastaOptions.Controls.Add(lblLongProteinNameSplitChars);
            TabPageNewFastaOptions.Controls.Add(chkGenerateFixedFastaFile);
            TabPageNewFastaOptions.Location = new Point(4, 25);
            TabPageNewFastaOptions.Margin = new Padding(4);
            TabPageNewFastaOptions.Name = "TabPageNewFastaOptions";
            TabPageNewFastaOptions.Size = new Size(537, 228);
            TabPageNewFastaOptions.TabIndex = 1;
            TabPageNewFastaOptions.Text = "Fixed FASTA Options";
            // 
            // txtResiduesPerLineForWrap
            // 
            txtResiduesPerLineForWrap.Location = new Point(439, 56);
            txtResiduesPerLineForWrap.Margin = new Padding(4);
            txtResiduesPerLineForWrap.Name = "txtResiduesPerLineForWrap";
            txtResiduesPerLineForWrap.Size = new Size(52, 22);
            txtResiduesPerLineForWrap.TabIndex = 15;
            txtResiduesPerLineForWrap.Text = "60";
            // 
            // lblResiduesPerLineForWrap
            // 
            lblResiduesPerLineForWrap.Location = new Point(239, 57);
            lblResiduesPerLineForWrap.Margin = new Padding(4, 0, 4, 0);
            lblResiduesPerLineForWrap.Name = "lblResiduesPerLineForWrap";
            lblResiduesPerLineForWrap.Size = new Size(199, 21);
            lblResiduesPerLineForWrap.TabIndex = 14;
            lblResiduesPerLineForWrap.Text = "Residues per line for wrap";
            // 
            // chkKeepDuplicateNamedProteins
            // 
            chkKeepDuplicateNamedProteins.Location = new Point(8, 158);
            chkKeepDuplicateNamedProteins.Margin = new Padding(4);
            chkKeepDuplicateNamedProteins.Name = "chkKeepDuplicateNamedProteins";
            chkKeepDuplicateNamedProteins.Size = new Size(171, 39);
            chkKeepDuplicateNamedProteins.TabIndex = 13;
            chkKeepDuplicateNamedProteins.Text = "Retain duplicate named proteins";
            // 
            // chkConsolidateDuplicateProteinSeqs
            // 
            chkConsolidateDuplicateProteinSeqs.Checked = true;
            chkConsolidateDuplicateProteinSeqs.CheckState = CheckState.Checked;
            chkConsolidateDuplicateProteinSeqs.Location = new Point(239, 166);
            chkConsolidateDuplicateProteinSeqs.Margin = new Padding(4);
            chkConsolidateDuplicateProteinSeqs.Name = "chkConsolidateDuplicateProteinSeqs";
            chkConsolidateDuplicateProteinSeqs.Size = new Size(267, 22);
            chkConsolidateDuplicateProteinSeqs.TabIndex = 11;
            chkConsolidateDuplicateProteinSeqs.Text = "Consolidate duplicate proteins";
            // 
            // chkRenameDuplicateProteins
            // 
            chkRenameDuplicateProteins.Checked = true;
            chkRenameDuplicateProteins.CheckState = CheckState.Checked;
            chkRenameDuplicateProteins.Location = new Point(8, 117);
            chkRenameDuplicateProteins.Margin = new Padding(4);
            chkRenameDuplicateProteins.Name = "chkRenameDuplicateProteins";
            chkRenameDuplicateProteins.Size = new Size(171, 40);
            chkRenameDuplicateProteins.TabIndex = 6;
            chkRenameDuplicateProteins.Text = "Rename proteins with duplicate names";
            // 
            // chkSplitOutMultipleRefsInProteinName
            // 
            chkSplitOutMultipleRefsInProteinName.Location = new Point(239, 117);
            chkSplitOutMultipleRefsInProteinName.Margin = new Padding(4);
            chkSplitOutMultipleRefsInProteinName.Name = "chkSplitOutMultipleRefsInProteinName";
            chkSplitOutMultipleRefsInProteinName.Size = new Size(256, 40);
            chkSplitOutMultipleRefsInProteinName.TabIndex = 10;
            chkSplitOutMultipleRefsInProteinName.Text = "Split out multiple references in protein names (always)";
            // 
            // txtInvalidProteinNameCharsToRemove
            // 
            txtInvalidProteinNameCharsToRemove.Location = new Point(439, 30);
            txtInvalidProteinNameCharsToRemove.Margin = new Padding(4);
            txtInvalidProteinNameCharsToRemove.Name = "txtInvalidProteinNameCharsToRemove";
            txtInvalidProteinNameCharsToRemove.Size = new Size(52, 22);
            txtInvalidProteinNameCharsToRemove.TabIndex = 4;
            // 
            // lblInvalidProteinNameCharsToRemove
            // 
            lblInvalidProteinNameCharsToRemove.Location = new Point(161, 32);
            lblInvalidProteinNameCharsToRemove.Margin = new Padding(4, 0, 4, 0);
            lblInvalidProteinNameCharsToRemove.Name = "lblInvalidProteinNameCharsToRemove";
            lblInvalidProteinNameCharsToRemove.Size = new Size(277, 20);
            lblInvalidProteinNameCharsToRemove.TabIndex = 3;
            lblInvalidProteinNameCharsToRemove.Text = "Invalid protein name chars to remove";
            // 
            // txtLongProteinNameSplitChars
            // 
            txtLongProteinNameSplitChars.Location = new Point(439, 2);
            txtLongProteinNameSplitChars.Margin = new Padding(4);
            txtLongProteinNameSplitChars.Name = "txtLongProteinNameSplitChars";
            txtLongProteinNameSplitChars.Size = new Size(52, 22);
            txtLongProteinNameSplitChars.TabIndex = 2;
            txtLongProteinNameSplitChars.Text = "|";
            // 
            // lblLongProteinNameSplitChars
            // 
            lblLongProteinNameSplitChars.Location = new Point(161, 2);
            lblLongProteinNameSplitChars.Margin = new Padding(4, 0, 4, 0);
            lblLongProteinNameSplitChars.Name = "lblLongProteinNameSplitChars";
            lblLongProteinNameSplitChars.Size = new Size(277, 20);
            lblLongProteinNameSplitChars.TabIndex = 1;
            lblLongProteinNameSplitChars.Text = "Char(s) to split long protein names on";
            // 
            // chkGenerateFixedFastaFile
            // 
            chkGenerateFixedFastaFile.Location = new Point(8, 2);
            chkGenerateFixedFastaFile.Margin = new Padding(4);
            chkGenerateFixedFastaFile.Name = "chkGenerateFixedFastaFile";
            chkGenerateFixedFastaFile.Size = new Size(145, 41);
            chkGenerateFixedFastaFile.TabIndex = 0;
            chkGenerateFixedFastaFile.Text = "Generate fixed FASTA file";
            // 
            // lblCustomRulesFile
            // 
            lblCustomRulesFile.Location = new Point(5, 64);
            lblCustomRulesFile.Margin = new Padding(4, 0, 4, 0);
            lblCustomRulesFile.Name = "lblCustomRulesFile";
            lblCustomRulesFile.Size = new Size(427, 18);
            lblCustomRulesFile.TabIndex = 2;
            lblCustomRulesFile.Text = "Custom rules file (i.e. ValidateFastaFile parameter file)";
            // 
            // TabPageRuleOptions
            // 
            TabPageRuleOptions.Controls.Add(lblCustomRulesFile);
            TabPageRuleOptions.Controls.Add(cmdCreateDefaultValidationRulesFile);
            TabPageRuleOptions.Controls.Add(txtCustomValidationRulesFilePath);
            TabPageRuleOptions.Controls.Add(cmdSelectCustomRulesFile);
            TabPageRuleOptions.Location = new Point(4, 25);
            TabPageRuleOptions.Margin = new Padding(4);
            TabPageRuleOptions.Name = "TabPageRuleOptions";
            TabPageRuleOptions.Size = new Size(537, 228);
            TabPageRuleOptions.TabIndex = 2;
            TabPageRuleOptions.Text = "Rule Options";
            // 
            // cmdCreateDefaultValidationRulesFile
            // 
            cmdCreateDefaultValidationRulesFile.Location = new Point(249, 10);
            cmdCreateDefaultValidationRulesFile.Margin = new Padding(4);
            cmdCreateDefaultValidationRulesFile.Name = "cmdCreateDefaultValidationRulesFile";
            cmdCreateDefaultValidationRulesFile.Size = new Size(159, 45);
            cmdCreateDefaultValidationRulesFile.TabIndex = 1;
            cmdCreateDefaultValidationRulesFile.Text = "Create XML file with default rules";
            // 
            // txtCustomValidationRulesFilePath
            // 
            txtCustomValidationRulesFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCustomValidationRulesFilePath.Location = new Point(5, 93);
            txtCustomValidationRulesFilePath.Margin = new Padding(4);
            txtCustomValidationRulesFilePath.Name = "txtCustomValidationRulesFilePath";
            txtCustomValidationRulesFilePath.Size = new Size(511, 22);
            txtCustomValidationRulesFilePath.TabIndex = 3;
            // 
            // cmdSelectCustomRulesFile
            // 
            cmdSelectCustomRulesFile.Location = new Point(5, 10);
            cmdSelectCustomRulesFile.Margin = new Padding(4);
            cmdSelectCustomRulesFile.Name = "cmdSelectCustomRulesFile";
            cmdSelectCustomRulesFile.Size = new Size(120, 45);
            cmdSelectCustomRulesFile.TabIndex = 0;
            cmdSelectCustomRulesFile.Text = "Select Custom Rules File";
            // 
            // tbsOptions
            // 
            tbsOptions.Controls.Add(TabPageErrorOptions);
            tbsOptions.Controls.Add(TabPageNewFastaOptions);
            tbsOptions.Controls.Add(TabPageRuleOptions);
            tbsOptions.Location = new Point(7, 7);
            tbsOptions.Margin = new Padding(4);
            tbsOptions.Name = "tbsOptions";
            tbsOptions.SelectedIndex = 0;
            tbsOptions.Size = new Size(545, 257);
            tbsOptions.TabIndex = 9;
            // 
            // mnuEditCopyAllWarnings
            // 
            mnuEditCopyAllWarnings.Index = 4;
            mnuEditCopyAllWarnings.Text = "Copy &Warnings";
            // 
            // MainMenuControl
            // 
            MainMenuControl.MenuItems.AddRange(new MenuItem[] { mnuFile, mnuEdit, MenuItem1 });
            // 
            // mnuFile
            // 
            mnuFile.Index = 0;
            mnuFile.MenuItems.AddRange(new MenuItem[] { mnuFileExit });
            mnuFile.Text = "&File";
            // 
            // mnuFileExit
            // 
            mnuFileExit.Index = 0;
            mnuFileExit.Text = "E&xit";
            // 
            // mnuEdit
            // 
            mnuEdit.Index = 1;
            mnuEdit.MenuItems.AddRange(new MenuItem[] { mnuEditCopyAllResults, mnuEditSep1, mnuEditCopySummary, mnuEditCopyAllErrors, mnuEditCopyAllWarnings, mnuEditSep2, mnuEditFontSizeDecrease, mnuEditFontSizeIncrease, MenuItem2, mnuEditResetToDefaults });
            mnuEdit.Text = "&Edit";
            // 
            // mnuEditCopyAllResults
            // 
            mnuEditCopyAllResults.Index = 0;
            mnuEditCopyAllResults.Text = "Copy &All Results";
            // 
            // mnuEditSep1
            // 
            mnuEditSep1.Index = 1;
            mnuEditSep1.Text = "-";
            // 
            // mnuEditCopySummary
            // 
            mnuEditCopySummary.Index = 2;
            mnuEditCopySummary.Text = "Copy &Summary";
            // 
            // mnuEditCopyAllErrors
            // 
            mnuEditCopyAllErrors.Index = 3;
            mnuEditCopyAllErrors.Text = "Copy &Errors";
            // 
            // mnuEditSep2
            // 
            mnuEditSep2.Index = 5;
            mnuEditSep2.Text = "-";
            // 
            // mnuEditFontSizeDecrease
            // 
            mnuEditFontSizeDecrease.Index = 6;
            mnuEditFontSizeDecrease.Shortcut = Shortcut.F3;
            mnuEditFontSizeDecrease.Text = "&Decrease Font Size";
            // 
            // mnuEditFontSizeIncrease
            // 
            mnuEditFontSizeIncrease.Index = 7;
            mnuEditFontSizeIncrease.Shortcut = Shortcut.F4;
            mnuEditFontSizeIncrease.Text = "&Increase Font Size";
            // 
            // MenuItem2
            // 
            MenuItem2.Index = 8;
            MenuItem2.Text = "-";
            // 
            // mnuEditResetToDefaults
            // 
            mnuEditResetToDefaults.Index = 9;
            mnuEditResetToDefaults.Text = "&Reset options to Default";
            // 
            // MenuItem1
            // 
            MenuItem1.Index = 2;
            MenuItem1.MenuItems.AddRange(new MenuItem[] { mnuHelpAbout });
            MenuItem1.Text = "&Help";
            // 
            // mnuHelpAbout
            // 
            mnuHelpAbout.Index = 0;
            mnuHelpAbout.Text = "&About";
            // 
            // dgWarnings
            // 
            dgWarnings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgWarnings.DataMember = "";
            dgWarnings.HeaderForeColor = SystemColors.ControlText;
            dgWarnings.Location = new Point(7, 428);
            dgWarnings.Margin = new Padding(4);
            dgWarnings.Name = "dgWarnings";
            dgWarnings.Size = new Size(1220, 128);
            dgWarnings.TabIndex = 16;
            // 
            // lblFilterData
            // 
            lblFilterData.Location = new Point(733, 218);
            lblFilterData.Margin = new Padding(4, 0, 4, 0);
            lblFilterData.Name = "lblFilterData";
            lblFilterData.Size = new Size(85, 18);
            lblFilterData.TabIndex = 13;
            lblFilterData.Text = "Filter data";
            // 
            // txtFilterData
            // 
            txtFilterData.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFilterData.Location = new Point(829, 215);
            txtFilterData.Margin = new Padding(4);
            txtFilterData.Name = "txtFilterData";
            txtFilterData.Size = new Size(392, 22);
            txtFilterData.TabIndex = 14;
            // 
            // pbarProgress
            // 
            pbarProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pbarProgress.Location = new Point(829, 215);
            pbarProgress.Margin = new Padding(4);
            pbarProgress.Name = "pbarProgress";
            pbarProgress.Size = new Size(393, 25);
            pbarProgress.TabIndex = 17;
            pbarProgress.Visible = false;
            // 
            // cmdValidateFastaFile
            // 
            cmdValidateFastaFile.Location = new Point(584, 213);
            cmdValidateFastaFile.Margin = new Padding(4);
            cmdValidateFastaFile.Name = "cmdValidateFastaFile";
            cmdValidateFastaFile.Size = new Size(139, 30);
            cmdValidateFastaFile.TabIndex = 11;
            cmdValidateFastaFile.Text = "&Start Validation";
            // 
            // dgErrors
            // 
            dgErrors.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dgErrors.DataMember = "";
            dgErrors.HeaderForeColor = SystemColors.ControlText;
            dgErrors.Location = new Point(6, 272);
            dgErrors.Margin = new Padding(4);
            dgErrors.Name = "dgErrors";
            dgErrors.Size = new Size(1215, 148);
            dgErrors.TabIndex = 15;
            // 
            // cmdCancel
            // 
            cmdCancel.Location = new Point(584, 213);
            cmdCancel.Margin = new Padding(4);
            cmdCancel.Name = "cmdCancel";
            cmdCancel.Size = new Size(139, 30);
            cmdCancel.TabIndex = 12;
            cmdCancel.Text = "Cancel";
            cmdCancel.Visible = false;
            // 
            // frmFastaValidation
            // 
            AutoScaleDimensions = new SizeF(8.0f, 16.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1239, 583);
            Controls.Add(txtResults);
            Controls.Add(tbsOptions);
            Controls.Add(dgWarnings);
            Controls.Add(lblFilterData);
            Controls.Add(txtFilterData);
            Controls.Add(pbarProgress);
            Controls.Add(cmdValidateFastaFile);
            Controls.Add(dgErrors);
            Controls.Add(cmdCancel);
            Margin = new Padding(4);
            Menu = MainMenuControl;
            Name = "FastaValidation";
            Text = "FASTA File Validation";
            TabPageErrorOptions.ResumeLayout(false);
            TabPageErrorOptions.PerformLayout();
            TabPageNewFastaOptions.ResumeLayout(false);
            TabPageNewFastaOptions.PerformLayout();
            TabPageRuleOptions.ResumeLayout(false);
            TabPageRuleOptions.PerformLayout();
            tbsOptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgWarnings).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgErrors).EndInit();
            Resize += new EventHandler(frmFastaValidationResults_Resize);
            ResumeLayout(false);
            PerformLayout();
        }

        internal CheckBox chkWrapLongResidueLines;
        internal CheckBox chkSplitOutMultipleRefsForKnownAccession;
        internal CheckBox chkTruncateLongProteinNames;
        internal TextBox txtResults;
        internal TabPage TabPageErrorOptions;
        internal CheckBox chkSaveBasicProteinHashInfoFile;
        internal Label lblProteinNameLengthUnits;
        internal Label lblProteinNameLength2;
        internal TextBox txtProteinNameLengthMaximum;
        internal TextBox txtProteinNameLengthMinimum;
        internal Label lblProteinNameLength;
        internal TextBox txtMaximumResiduesPerLine;
        internal Label lblMaximumResiduesPerLine;
        internal CheckBox chkAllowAsteriskInResidues;
        internal CheckBox chkCheckForDuplicateProteinInfo;
        internal TextBox txtMaxFileErrorsToTrack;
        internal Label lblMaxFileErrorsToTrack;
        internal CheckBox chkLogResults;
        internal CheckBox chkConsolidateDupsIgnoreILDiff;
        internal CheckBox chkRemoveInvalidResidues;
        internal TabPage TabPageNewFastaOptions;
        internal CheckBox chkConsolidateDuplicateProteinSeqs;
        internal CheckBox chkRenameDuplicateProteins;
        internal CheckBox chkSplitOutMultipleRefsInProteinName;
        internal TextBox txtInvalidProteinNameCharsToRemove;
        internal Label lblInvalidProteinNameCharsToRemove;
        internal TextBox txtLongProteinNameSplitChars;
        internal Label lblLongProteinNameSplitChars;
        internal CheckBox chkGenerateFixedFastaFile;
        internal Label lblCustomRulesFile;
        internal TabPage TabPageRuleOptions;
        internal Button cmdCreateDefaultValidationRulesFile;
        internal TextBox txtCustomValidationRulesFilePath;
        internal Button cmdSelectCustomRulesFile;
        internal TabControl tbsOptions;
        internal MenuItem mnuEditCopyAllWarnings;
        internal MainMenu MainMenuControl;
        internal MenuItem mnuFile;
        internal MenuItem mnuFileExit;
        internal MenuItem mnuEdit;
        internal MenuItem mnuEditCopyAllResults;
        internal MenuItem mnuEditSep1;
        internal MenuItem mnuEditCopySummary;
        internal MenuItem mnuEditCopyAllErrors;
        internal MenuItem mnuEditSep2;
        internal MenuItem mnuEditFontSizeDecrease;
        internal MenuItem mnuEditFontSizeIncrease;
        internal MenuItem MenuItem2;
        internal MenuItem mnuEditResetToDefaults;
        internal MenuItem MenuItem1;
        internal MenuItem mnuHelpAbout;
        internal DataGrid dgWarnings;
        internal Label lblFilterData;
        internal TextBox txtFilterData;
        internal ProgressBar pbarProgress;
        internal Button cmdValidateFastaFile;
        internal DataGrid dgErrors;
        internal Button cmdCancel;
        internal CheckBox chkKeepDuplicateNamedProteins;
        internal TextBox txtResiduesPerLineForWrap;
        internal Label lblResiduesPerLineForWrap;
    }
}