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
            _chkWrapLongResidueLines = new CheckBox();
            _chkWrapLongResidueLines.CheckedChanged += new EventHandler(chkWrapLongResidueLines_CheckedChanged);
            chkSplitOutMultipleRefsForKnownAccession = new CheckBox();
            chkTruncateLongProteinNames = new CheckBox();
            _txtResults = new TextBox();
            _txtResults.KeyDown += new KeyEventHandler(txtResults_KeyDown);
            TabPageErrorOptions = new TabPage();
            chkSaveBasicProteinHashInfoFile = new CheckBox();
            lblProteinNameLengthUnits = new Label();
            lblProteinNameLength2 = new Label();
            _txtProteinNameLengthMaximum = new TextBox();
            _txtProteinNameLengthMaximum.KeyPress += new KeyPressEventHandler(txtProteinNameLengthMaximum_KeyPress);
            _txtProteinNameLengthMinimum = new TextBox();
            _txtProteinNameLengthMinimum.KeyPress += new KeyPressEventHandler(txtProteinNameLengthMinimum_KeyPress);
            lblProteinNameLength = new Label();
            _txtMaximumResiduesPerLine = new TextBox();
            _txtMaximumResiduesPerLine.KeyPress += new KeyPressEventHandler(txtMaximumResiduesPerLine_KeyPress);
            lblMaximumResiduesPerLine = new Label();
            chkAllowAsteriskInResidues = new CheckBox();
            chkCheckForDuplicateProteinInfo = new CheckBox();
            _txtMaxFileErrorsToTrack = new TextBox();
            _txtMaxFileErrorsToTrack.KeyPress += new KeyPressEventHandler(txtMaxFileErrorsToTrack_KeyPress1);
            lblMaxFileErrorsToTrack = new Label();
            chkLogResults = new CheckBox();
            chkConsolidateDupsIgnoreILDiff = new CheckBox();
            chkRemoveInvalidResidues = new CheckBox();
            TabPageNewFastaOptions = new TabPage();
            _txtResiduesPerLineForWrap = new TextBox();
            _txtResiduesPerLineForWrap.KeyPress += new KeyPressEventHandler(txtResiduesPerLineForWrap_TextChanged);
            lblResiduesPerLineForWrap = new Label();
            _chkKeepDuplicateNamedProteins = new CheckBox();
            _chkKeepDuplicateNamedProteins.CheckedChanged += new EventHandler(chkKeepDuplicateNamedProteins_CheckedChanged);
            _chkConsolidateDuplicateProteinSeqs = new CheckBox();
            _chkConsolidateDuplicateProteinSeqs.CheckedChanged += new EventHandler(chkConsolidateDuplicateProteinSeqs_CheckedChanged);
            _chkRenameDuplicateProteins = new CheckBox();
            _chkRenameDuplicateProteins.CheckedChanged += new EventHandler(chkRenameDuplicateProteins_CheckedChanged);
            chkSplitOutMultipleRefsInProteinName = new CheckBox();
            txtInvalidProteinNameCharsToRemove = new TextBox();
            lblInvalidProteinNameCharsToRemove = new Label();
            txtLongProteinNameSplitChars = new TextBox();
            lblLongProteinNameSplitChars = new Label();
            _chkGenerateFixedFastaFile = new CheckBox();
            _chkGenerateFixedFastaFile.CheckedChanged += new EventHandler(chkGenerateFixedFastaFile_CheckedChanged);
            lblCustomRulesFile = new Label();
            TabPageRuleOptions = new TabPage();
            _cmdCreateDefaultValidationRulesFile = new Button();
            _cmdCreateDefaultValidationRulesFile.Click += new EventHandler(cmdCreateDefaultValidationRulesFile_Click_1);
            _txtCustomValidationRulesFilePath = new TextBox();
            _txtCustomValidationRulesFilePath.TextChanged += new EventHandler(txtCustomValidationRulesFilePath_TextChanged);
            _cmdSelectCustomRulesFile = new Button();
            _cmdSelectCustomRulesFile.Click += new EventHandler(cmdSelectCustomRulesFile_Click);
            tbsOptions = new TabControl();
            _mnuEditCopyAllWarnings = new MenuItem();
            _mnuEditCopyAllWarnings.Click += new EventHandler(mnuEditCopyAllWarnings_Click);
            MainMenuControl = new MainMenu(components);
            mnuFile = new MenuItem();
            _mnuFileExit = new MenuItem();
            _mnuFileExit.Click += new EventHandler(mnuFileExit_Click);
            mnuEdit = new MenuItem();
            _mnuEditCopyAllResults = new MenuItem();
            _mnuEditCopyAllResults.Click += new EventHandler(mnuEditCopyAllResults_Click);
            mnuEditSep1 = new MenuItem();
            _mnuEditCopySummary = new MenuItem();
            _mnuEditCopySummary.Click += new EventHandler(mnuEditCopySummary_Click);
            _mnuEditCopyAllErrors = new MenuItem();
            _mnuEditCopyAllErrors.Click += new EventHandler(mnuEditCopyAllErrors_Click);
            mnuEditSep2 = new MenuItem();
            _mnuEditFontSizeDecrease = new MenuItem();
            _mnuEditFontSizeDecrease.Click += new EventHandler(mnuEditFontSizeDecrease_Click);
            _mnuEditFontSizeIncrease = new MenuItem();
            _mnuEditFontSizeIncrease.Click += new EventHandler(mnuEditFontSizeIncrease_Click);
            MenuItem2 = new MenuItem();
            _mnuEditResetToDefaults = new MenuItem();
            _mnuEditResetToDefaults.Click += new EventHandler(mnuEditResetToDefaults_Click);
            MenuItem1 = new MenuItem();
            _mnuHelpAbout = new MenuItem();
            _mnuHelpAbout.Click += new EventHandler(mnuHelpAbout_Click);
            dgWarnings = new DataGrid();
            lblFilterData = new Label();
            _txtFilterData = new TextBox();
            _txtFilterData.Validated += new EventHandler(txtFilterData_Validated);
            _txtFilterData.KeyDown += new KeyEventHandler(txtFilterData_KeyDown);
            pbarProgress = new ProgressBar();
            _cmdValidateFastaFile = new Button();
            _cmdValidateFastaFile.Click += new EventHandler(cmdValidateFastaFile_Click);
            dgErrors = new DataGrid();
            _cmdCancel = new Button();
            _cmdCancel.Click += new EventHandler(cmdCancel_Click);
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
            _chkWrapLongResidueLines.Checked = true;
            _chkWrapLongResidueLines.CheckState = CheckState.Checked;
            _chkWrapLongResidueLines.Location = new Point(8, 57);
            _chkWrapLongResidueLines.Margin = new Padding(4);
            _chkWrapLongResidueLines.Name = "_chkWrapLongResidueLines";
            _chkWrapLongResidueLines.Size = new Size(203, 21);
            _chkWrapLongResidueLines.TabIndex = 7;
            _chkWrapLongResidueLines.Text = "Wrap long residue lines";
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
            _txtResults.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtResults.Location = new Point(584, 9);
            _txtResults.Margin = new Padding(4);
            _txtResults.Multiline = true;
            _txtResults.Name = "_txtResults";
            _txtResults.ScrollBars = ScrollBars.Both;
            _txtResults.Size = new Size(637, 186);
            _txtResults.TabIndex = 10;
            _txtResults.WordWrap = false;
            // 
            // TabPageErrorOptions
            // 
            TabPageErrorOptions.Controls.Add(chkSaveBasicProteinHashInfoFile);
            TabPageErrorOptions.Controls.Add(lblProteinNameLengthUnits);
            TabPageErrorOptions.Controls.Add(lblProteinNameLength2);
            TabPageErrorOptions.Controls.Add(_txtProteinNameLengthMaximum);
            TabPageErrorOptions.Controls.Add(_txtProteinNameLengthMinimum);
            TabPageErrorOptions.Controls.Add(lblProteinNameLength);
            TabPageErrorOptions.Controls.Add(_txtMaximumResiduesPerLine);
            TabPageErrorOptions.Controls.Add(lblMaximumResiduesPerLine);
            TabPageErrorOptions.Controls.Add(chkAllowAsteriskInResidues);
            TabPageErrorOptions.Controls.Add(chkCheckForDuplicateProteinInfo);
            TabPageErrorOptions.Controls.Add(_txtMaxFileErrorsToTrack);
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
            _txtProteinNameLengthMaximum.Location = new Point(343, 62);
            _txtProteinNameLengthMaximum.Margin = new Padding(4);
            _txtProteinNameLengthMaximum.Name = "_txtProteinNameLengthMaximum";
            _txtProteinNameLengthMaximum.Size = new Size(52, 22);
            _txtProteinNameLengthMaximum.TabIndex = 7;
            _txtProteinNameLengthMaximum.Text = "34";
            // 
            // txtProteinNameLengthMinimum
            // 
            _txtProteinNameLengthMinimum.Location = new Point(247, 62);
            _txtProteinNameLengthMinimum.Margin = new Padding(4);
            _txtProteinNameLengthMinimum.Name = "_txtProteinNameLengthMinimum";
            _txtProteinNameLengthMinimum.Size = new Size(52, 22);
            _txtProteinNameLengthMinimum.TabIndex = 5;
            _txtProteinNameLengthMinimum.Text = "3";
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
            _txtMaximumResiduesPerLine.Location = new Point(247, 32);
            _txtMaximumResiduesPerLine.Margin = new Padding(4);
            _txtMaximumResiduesPerLine.Name = "_txtMaximumResiduesPerLine";
            _txtMaximumResiduesPerLine.Size = new Size(52, 22);
            _txtMaximumResiduesPerLine.TabIndex = 3;
            _txtMaximumResiduesPerLine.Text = "120";
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
            _txtMaxFileErrorsToTrack.Location = new Point(385, 2);
            _txtMaxFileErrorsToTrack.Margin = new Padding(4);
            _txtMaxFileErrorsToTrack.Name = "_txtMaxFileErrorsToTrack";
            _txtMaxFileErrorsToTrack.Size = new Size(52, 22);
            _txtMaxFileErrorsToTrack.TabIndex = 1;
            _txtMaxFileErrorsToTrack.Text = "10";
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
            TabPageNewFastaOptions.Controls.Add(_txtResiduesPerLineForWrap);
            TabPageNewFastaOptions.Controls.Add(lblResiduesPerLineForWrap);
            TabPageNewFastaOptions.Controls.Add(_chkKeepDuplicateNamedProteins);
            TabPageNewFastaOptions.Controls.Add(chkRemoveInvalidResidues);
            TabPageNewFastaOptions.Controls.Add(_chkWrapLongResidueLines);
            TabPageNewFastaOptions.Controls.Add(chkSplitOutMultipleRefsForKnownAccession);
            TabPageNewFastaOptions.Controls.Add(chkTruncateLongProteinNames);
            TabPageNewFastaOptions.Controls.Add(chkConsolidateDupsIgnoreILDiff);
            TabPageNewFastaOptions.Controls.Add(_chkConsolidateDuplicateProteinSeqs);
            TabPageNewFastaOptions.Controls.Add(_chkRenameDuplicateProteins);
            TabPageNewFastaOptions.Controls.Add(chkSplitOutMultipleRefsInProteinName);
            TabPageNewFastaOptions.Controls.Add(txtInvalidProteinNameCharsToRemove);
            TabPageNewFastaOptions.Controls.Add(lblInvalidProteinNameCharsToRemove);
            TabPageNewFastaOptions.Controls.Add(txtLongProteinNameSplitChars);
            TabPageNewFastaOptions.Controls.Add(lblLongProteinNameSplitChars);
            TabPageNewFastaOptions.Controls.Add(_chkGenerateFixedFastaFile);
            TabPageNewFastaOptions.Location = new Point(4, 25);
            TabPageNewFastaOptions.Margin = new Padding(4);
            TabPageNewFastaOptions.Name = "TabPageNewFastaOptions";
            TabPageNewFastaOptions.Size = new Size(537, 228);
            TabPageNewFastaOptions.TabIndex = 1;
            TabPageNewFastaOptions.Text = "Fixed FASTA Options";
            // 
            // txtResiduesPerLineForWrap
            // 
            _txtResiduesPerLineForWrap.Location = new Point(439, 56);
            _txtResiduesPerLineForWrap.Margin = new Padding(4);
            _txtResiduesPerLineForWrap.Name = "_txtResiduesPerLineForWrap";
            _txtResiduesPerLineForWrap.Size = new Size(52, 22);
            _txtResiduesPerLineForWrap.TabIndex = 15;
            _txtResiduesPerLineForWrap.Text = "60";
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
            _chkKeepDuplicateNamedProteins.Location = new Point(8, 158);
            _chkKeepDuplicateNamedProteins.Margin = new Padding(4);
            _chkKeepDuplicateNamedProteins.Name = "_chkKeepDuplicateNamedProteins";
            _chkKeepDuplicateNamedProteins.Size = new Size(171, 39);
            _chkKeepDuplicateNamedProteins.TabIndex = 13;
            _chkKeepDuplicateNamedProteins.Text = "Retain duplicate named proteins";
            // 
            // chkConsolidateDuplicateProteinSeqs
            // 
            _chkConsolidateDuplicateProteinSeqs.Checked = true;
            _chkConsolidateDuplicateProteinSeqs.CheckState = CheckState.Checked;
            _chkConsolidateDuplicateProteinSeqs.Location = new Point(239, 166);
            _chkConsolidateDuplicateProteinSeqs.Margin = new Padding(4);
            _chkConsolidateDuplicateProteinSeqs.Name = "_chkConsolidateDuplicateProteinSeqs";
            _chkConsolidateDuplicateProteinSeqs.Size = new Size(267, 22);
            _chkConsolidateDuplicateProteinSeqs.TabIndex = 11;
            _chkConsolidateDuplicateProteinSeqs.Text = "Consolidate duplicate proteins";
            // 
            // chkRenameDuplicateProteins
            // 
            _chkRenameDuplicateProteins.Checked = true;
            _chkRenameDuplicateProteins.CheckState = CheckState.Checked;
            _chkRenameDuplicateProteins.Location = new Point(8, 117);
            _chkRenameDuplicateProteins.Margin = new Padding(4);
            _chkRenameDuplicateProteins.Name = "_chkRenameDuplicateProteins";
            _chkRenameDuplicateProteins.Size = new Size(171, 40);
            _chkRenameDuplicateProteins.TabIndex = 6;
            _chkRenameDuplicateProteins.Text = "Rename proteins with duplicate names";
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
            _chkGenerateFixedFastaFile.Location = new Point(8, 2);
            _chkGenerateFixedFastaFile.Margin = new Padding(4);
            _chkGenerateFixedFastaFile.Name = "_chkGenerateFixedFastaFile";
            _chkGenerateFixedFastaFile.Size = new Size(145, 41);
            _chkGenerateFixedFastaFile.TabIndex = 0;
            _chkGenerateFixedFastaFile.Text = "Generate fixed FASTA file";
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
            TabPageRuleOptions.Controls.Add(_cmdCreateDefaultValidationRulesFile);
            TabPageRuleOptions.Controls.Add(_txtCustomValidationRulesFilePath);
            TabPageRuleOptions.Controls.Add(_cmdSelectCustomRulesFile);
            TabPageRuleOptions.Location = new Point(4, 25);
            TabPageRuleOptions.Margin = new Padding(4);
            TabPageRuleOptions.Name = "TabPageRuleOptions";
            TabPageRuleOptions.Size = new Size(537, 228);
            TabPageRuleOptions.TabIndex = 2;
            TabPageRuleOptions.Text = "Rule Options";
            // 
            // cmdCreateDefaultValidationRulesFile
            // 
            _cmdCreateDefaultValidationRulesFile.Location = new Point(249, 10);
            _cmdCreateDefaultValidationRulesFile.Margin = new Padding(4);
            _cmdCreateDefaultValidationRulesFile.Name = "_cmdCreateDefaultValidationRulesFile";
            _cmdCreateDefaultValidationRulesFile.Size = new Size(159, 45);
            _cmdCreateDefaultValidationRulesFile.TabIndex = 1;
            _cmdCreateDefaultValidationRulesFile.Text = "Create XML file with default rules";
            // 
            // txtCustomValidationRulesFilePath
            // 
            _txtCustomValidationRulesFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtCustomValidationRulesFilePath.Location = new Point(5, 93);
            _txtCustomValidationRulesFilePath.Margin = new Padding(4);
            _txtCustomValidationRulesFilePath.Name = "_txtCustomValidationRulesFilePath";
            _txtCustomValidationRulesFilePath.Size = new Size(511, 22);
            _txtCustomValidationRulesFilePath.TabIndex = 3;
            // 
            // cmdSelectCustomRulesFile
            // 
            _cmdSelectCustomRulesFile.Location = new Point(5, 10);
            _cmdSelectCustomRulesFile.Margin = new Padding(4);
            _cmdSelectCustomRulesFile.Name = "_cmdSelectCustomRulesFile";
            _cmdSelectCustomRulesFile.Size = new Size(120, 45);
            _cmdSelectCustomRulesFile.TabIndex = 0;
            _cmdSelectCustomRulesFile.Text = "Select Custom Rules File";
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
            _mnuEditCopyAllWarnings.Index = 4;
            _mnuEditCopyAllWarnings.Text = "Copy &Warnings";
            // 
            // MainMenuControl
            // 
            MainMenuControl.MenuItems.AddRange(new MenuItem[] { mnuFile, mnuEdit, MenuItem1 });
            // 
            // mnuFile
            // 
            mnuFile.Index = 0;
            mnuFile.MenuItems.AddRange(new MenuItem[] { _mnuFileExit });
            mnuFile.Text = "&File";
            // 
            // mnuFileExit
            // 
            _mnuFileExit.Index = 0;
            _mnuFileExit.Text = "E&xit";
            // 
            // mnuEdit
            // 
            mnuEdit.Index = 1;
            mnuEdit.MenuItems.AddRange(new MenuItem[] { _mnuEditCopyAllResults, mnuEditSep1, _mnuEditCopySummary, _mnuEditCopyAllErrors, _mnuEditCopyAllWarnings, mnuEditSep2, _mnuEditFontSizeDecrease, _mnuEditFontSizeIncrease, MenuItem2, _mnuEditResetToDefaults });
            mnuEdit.Text = "&Edit";
            // 
            // mnuEditCopyAllResults
            // 
            _mnuEditCopyAllResults.Index = 0;
            _mnuEditCopyAllResults.Text = "Copy &All Results";
            // 
            // mnuEditSep1
            // 
            mnuEditSep1.Index = 1;
            mnuEditSep1.Text = "-";
            // 
            // mnuEditCopySummary
            // 
            _mnuEditCopySummary.Index = 2;
            _mnuEditCopySummary.Text = "Copy &Summary";
            // 
            // mnuEditCopyAllErrors
            // 
            _mnuEditCopyAllErrors.Index = 3;
            _mnuEditCopyAllErrors.Text = "Copy &Errors";
            // 
            // mnuEditSep2
            // 
            mnuEditSep2.Index = 5;
            mnuEditSep2.Text = "-";
            // 
            // mnuEditFontSizeDecrease
            // 
            _mnuEditFontSizeDecrease.Index = 6;
            _mnuEditFontSizeDecrease.Shortcut = Shortcut.F3;
            _mnuEditFontSizeDecrease.Text = "&Decrease Font Size";
            // 
            // mnuEditFontSizeIncrease
            // 
            _mnuEditFontSizeIncrease.Index = 7;
            _mnuEditFontSizeIncrease.Shortcut = Shortcut.F4;
            _mnuEditFontSizeIncrease.Text = "&Increase Font Size";
            // 
            // MenuItem2
            // 
            MenuItem2.Index = 8;
            MenuItem2.Text = "-";
            // 
            // mnuEditResetToDefaults
            // 
            _mnuEditResetToDefaults.Index = 9;
            _mnuEditResetToDefaults.Text = "&Reset options to Default";
            // 
            // MenuItem1
            // 
            MenuItem1.Index = 2;
            MenuItem1.MenuItems.AddRange(new MenuItem[] { _mnuHelpAbout });
            MenuItem1.Text = "&Help";
            // 
            // mnuHelpAbout
            // 
            _mnuHelpAbout.Index = 0;
            _mnuHelpAbout.Text = "&About";
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
            _txtFilterData.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtFilterData.Location = new Point(829, 215);
            _txtFilterData.Margin = new Padding(4);
            _txtFilterData.Name = "_txtFilterData";
            _txtFilterData.Size = new Size(392, 22);
            _txtFilterData.TabIndex = 14;
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
            _cmdValidateFastaFile.Location = new Point(584, 213);
            _cmdValidateFastaFile.Margin = new Padding(4);
            _cmdValidateFastaFile.Name = "_cmdValidateFastaFile";
            _cmdValidateFastaFile.Size = new Size(139, 30);
            _cmdValidateFastaFile.TabIndex = 11;
            _cmdValidateFastaFile.Text = "&Start Validation";
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
            _cmdCancel.Location = new Point(584, 213);
            _cmdCancel.Margin = new Padding(4);
            _cmdCancel.Name = "_cmdCancel";
            _cmdCancel.Size = new Size(139, 30);
            _cmdCancel.TabIndex = 12;
            _cmdCancel.Text = "Cancel";
            _cmdCancel.Visible = false;
            // 
            // frmFastaValidation
            // 
            AutoScaleDimensions = new SizeF(8.0f, 16.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1239, 583);
            Controls.Add(_txtResults);
            Controls.Add(tbsOptions);
            Controls.Add(dgWarnings);
            Controls.Add(lblFilterData);
            Controls.Add(_txtFilterData);
            Controls.Add(pbarProgress);
            Controls.Add(_cmdValidateFastaFile);
            Controls.Add(dgErrors);
            Controls.Add(_cmdCancel);
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

        private CheckBox _chkWrapLongResidueLines;

        internal CheckBox chkWrapLongResidueLines
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkWrapLongResidueLines;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkWrapLongResidueLines != null)
                {
                    _chkWrapLongResidueLines.CheckedChanged -= chkWrapLongResidueLines_CheckedChanged;
                }

                _chkWrapLongResidueLines = value;
                if (_chkWrapLongResidueLines != null)
                {
                    _chkWrapLongResidueLines.CheckedChanged += chkWrapLongResidueLines_CheckedChanged;
                }
            }
        }

        internal CheckBox chkSplitOutMultipleRefsForKnownAccession;
        internal CheckBox chkTruncateLongProteinNames;
        private TextBox _txtResults;

        internal TextBox txtResults
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtResults;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtResults != null)
                {
                    _txtResults.KeyDown -= txtResults_KeyDown;
                }

                _txtResults = value;
                if (_txtResults != null)
                {
                    _txtResults.KeyDown += txtResults_KeyDown;
                }
            }
        }

        internal TabPage TabPageErrorOptions;
        internal CheckBox chkSaveBasicProteinHashInfoFile;
        internal Label lblProteinNameLengthUnits;
        internal Label lblProteinNameLength2;
        private TextBox _txtProteinNameLengthMaximum;

        internal TextBox txtProteinNameLengthMaximum
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtProteinNameLengthMaximum;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtProteinNameLengthMaximum != null)
                {
                    _txtProteinNameLengthMaximum.KeyPress -= txtProteinNameLengthMaximum_KeyPress;
                }

                _txtProteinNameLengthMaximum = value;
                if (_txtProteinNameLengthMaximum != null)
                {
                    _txtProteinNameLengthMaximum.KeyPress += txtProteinNameLengthMaximum_KeyPress;
                }
            }
        }

        private TextBox _txtProteinNameLengthMinimum;

        internal TextBox txtProteinNameLengthMinimum
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtProteinNameLengthMinimum;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtProteinNameLengthMinimum != null)
                {
                    _txtProteinNameLengthMinimum.KeyPress -= txtProteinNameLengthMinimum_KeyPress;
                }

                _txtProteinNameLengthMinimum = value;
                if (_txtProteinNameLengthMinimum != null)
                {
                    _txtProteinNameLengthMinimum.KeyPress += txtProteinNameLengthMinimum_KeyPress;
                }
            }
        }

        internal Label lblProteinNameLength;
        private TextBox _txtMaximumResiduesPerLine;

        internal TextBox txtMaximumResiduesPerLine
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtMaximumResiduesPerLine;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtMaximumResiduesPerLine != null)
                {
                    _txtMaximumResiduesPerLine.KeyPress -= txtMaximumResiduesPerLine_KeyPress;
                }

                _txtMaximumResiduesPerLine = value;
                if (_txtMaximumResiduesPerLine != null)
                {
                    _txtMaximumResiduesPerLine.KeyPress += txtMaximumResiduesPerLine_KeyPress;
                }
            }
        }

        internal Label lblMaximumResiduesPerLine;
        internal CheckBox chkAllowAsteriskInResidues;
        internal CheckBox chkCheckForDuplicateProteinInfo;
        private TextBox _txtMaxFileErrorsToTrack;

        internal TextBox txtMaxFileErrorsToTrack
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtMaxFileErrorsToTrack;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtMaxFileErrorsToTrack != null)
                {
                    _txtMaxFileErrorsToTrack.KeyPress -= txtMaxFileErrorsToTrack_KeyPress1;
                }

                _txtMaxFileErrorsToTrack = value;
                if (_txtMaxFileErrorsToTrack != null)
                {
                    _txtMaxFileErrorsToTrack.KeyPress += txtMaxFileErrorsToTrack_KeyPress1;
                }
            }
        }

        internal Label lblMaxFileErrorsToTrack;
        internal CheckBox chkLogResults;
        internal CheckBox chkConsolidateDupsIgnoreILDiff;
        internal CheckBox chkRemoveInvalidResidues;
        internal TabPage TabPageNewFastaOptions;
        private CheckBox _chkConsolidateDuplicateProteinSeqs;

        internal CheckBox chkConsolidateDuplicateProteinSeqs
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkConsolidateDuplicateProteinSeqs;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkConsolidateDuplicateProteinSeqs != null)
                {
                    _chkConsolidateDuplicateProteinSeqs.CheckedChanged -= chkConsolidateDuplicateProteinSeqs_CheckedChanged;
                }

                _chkConsolidateDuplicateProteinSeqs = value;
                if (_chkConsolidateDuplicateProteinSeqs != null)
                {
                    _chkConsolidateDuplicateProteinSeqs.CheckedChanged += chkConsolidateDuplicateProteinSeqs_CheckedChanged;
                }
            }
        }

        private CheckBox _chkRenameDuplicateProteins;

        internal CheckBox chkRenameDuplicateProteins
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkRenameDuplicateProteins;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkRenameDuplicateProteins != null)
                {
                    _chkRenameDuplicateProteins.CheckedChanged -= chkRenameDuplicateProteins_CheckedChanged;
                }

                _chkRenameDuplicateProteins = value;
                if (_chkRenameDuplicateProteins != null)
                {
                    _chkRenameDuplicateProteins.CheckedChanged += chkRenameDuplicateProteins_CheckedChanged;
                }
            }
        }

        internal CheckBox chkSplitOutMultipleRefsInProteinName;
        internal TextBox txtInvalidProteinNameCharsToRemove;
        internal Label lblInvalidProteinNameCharsToRemove;
        internal TextBox txtLongProteinNameSplitChars;
        internal Label lblLongProteinNameSplitChars;
        private CheckBox _chkGenerateFixedFastaFile;

        internal CheckBox chkGenerateFixedFastaFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkGenerateFixedFastaFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkGenerateFixedFastaFile != null)
                {
                    _chkGenerateFixedFastaFile.CheckedChanged -= chkGenerateFixedFastaFile_CheckedChanged;
                }

                _chkGenerateFixedFastaFile = value;
                if (_chkGenerateFixedFastaFile != null)
                {
                    _chkGenerateFixedFastaFile.CheckedChanged += chkGenerateFixedFastaFile_CheckedChanged;
                }
            }
        }

        internal Label lblCustomRulesFile;
        internal TabPage TabPageRuleOptions;
        private Button _cmdCreateDefaultValidationRulesFile;

        internal Button cmdCreateDefaultValidationRulesFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdCreateDefaultValidationRulesFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdCreateDefaultValidationRulesFile != null)
                {
                    _cmdCreateDefaultValidationRulesFile.Click -= cmdCreateDefaultValidationRulesFile_Click_1;
                }

                _cmdCreateDefaultValidationRulesFile = value;
                if (_cmdCreateDefaultValidationRulesFile != null)
                {
                    _cmdCreateDefaultValidationRulesFile.Click += cmdCreateDefaultValidationRulesFile_Click_1;
                }
            }
        }

        private TextBox _txtCustomValidationRulesFilePath;

        internal TextBox txtCustomValidationRulesFilePath
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtCustomValidationRulesFilePath;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtCustomValidationRulesFilePath != null)
                {
                    _txtCustomValidationRulesFilePath.TextChanged -= txtCustomValidationRulesFilePath_TextChanged;
                }

                _txtCustomValidationRulesFilePath = value;
                if (_txtCustomValidationRulesFilePath != null)
                {
                    _txtCustomValidationRulesFilePath.TextChanged += txtCustomValidationRulesFilePath_TextChanged;
                }
            }
        }

        private Button _cmdSelectCustomRulesFile;

        internal Button cmdSelectCustomRulesFile
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdSelectCustomRulesFile;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdSelectCustomRulesFile != null)
                {
                    _cmdSelectCustomRulesFile.Click -= cmdSelectCustomRulesFile_Click;
                }

                _cmdSelectCustomRulesFile = value;
                if (_cmdSelectCustomRulesFile != null)
                {
                    _cmdSelectCustomRulesFile.Click += cmdSelectCustomRulesFile_Click;
                }
            }
        }

        internal TabControl tbsOptions;
        private MenuItem _mnuEditCopyAllWarnings;

        internal MenuItem mnuEditCopyAllWarnings
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditCopyAllWarnings;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditCopyAllWarnings != null)
                {
                    _mnuEditCopyAllWarnings.Click -= mnuEditCopyAllWarnings_Click;
                }

                _mnuEditCopyAllWarnings = value;
                if (_mnuEditCopyAllWarnings != null)
                {
                    _mnuEditCopyAllWarnings.Click += mnuEditCopyAllWarnings_Click;
                }
            }
        }

        internal MainMenu MainMenuControl;
        internal MenuItem mnuFile;
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

        internal MenuItem mnuEdit;
        private MenuItem _mnuEditCopyAllResults;

        internal MenuItem mnuEditCopyAllResults
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditCopyAllResults;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditCopyAllResults != null)
                {
                    _mnuEditCopyAllResults.Click -= mnuEditCopyAllResults_Click;
                }

                _mnuEditCopyAllResults = value;
                if (_mnuEditCopyAllResults != null)
                {
                    _mnuEditCopyAllResults.Click += mnuEditCopyAllResults_Click;
                }
            }
        }

        internal MenuItem mnuEditSep1;
        private MenuItem _mnuEditCopySummary;

        internal MenuItem mnuEditCopySummary
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditCopySummary;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditCopySummary != null)
                {
                    _mnuEditCopySummary.Click -= mnuEditCopySummary_Click;
                }

                _mnuEditCopySummary = value;
                if (_mnuEditCopySummary != null)
                {
                    _mnuEditCopySummary.Click += mnuEditCopySummary_Click;
                }
            }
        }

        private MenuItem _mnuEditCopyAllErrors;

        internal MenuItem mnuEditCopyAllErrors
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditCopyAllErrors;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditCopyAllErrors != null)
                {
                    _mnuEditCopyAllErrors.Click -= mnuEditCopyAllErrors_Click;
                }

                _mnuEditCopyAllErrors = value;
                if (_mnuEditCopyAllErrors != null)
                {
                    _mnuEditCopyAllErrors.Click += mnuEditCopyAllErrors_Click;
                }
            }
        }

        internal MenuItem mnuEditSep2;
        private MenuItem _mnuEditFontSizeDecrease;

        internal MenuItem mnuEditFontSizeDecrease
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditFontSizeDecrease;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditFontSizeDecrease != null)
                {
                    _mnuEditFontSizeDecrease.Click -= mnuEditFontSizeDecrease_Click;
                }

                _mnuEditFontSizeDecrease = value;
                if (_mnuEditFontSizeDecrease != null)
                {
                    _mnuEditFontSizeDecrease.Click += mnuEditFontSizeDecrease_Click;
                }
            }
        }

        private MenuItem _mnuEditFontSizeIncrease;

        internal MenuItem mnuEditFontSizeIncrease
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditFontSizeIncrease;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditFontSizeIncrease != null)
                {
                    _mnuEditFontSizeIncrease.Click -= mnuEditFontSizeIncrease_Click;
                }

                _mnuEditFontSizeIncrease = value;
                if (_mnuEditFontSizeIncrease != null)
                {
                    _mnuEditFontSizeIncrease.Click += mnuEditFontSizeIncrease_Click;
                }
            }
        }

        internal MenuItem MenuItem2;
        private MenuItem _mnuEditResetToDefaults;

        internal MenuItem mnuEditResetToDefaults
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mnuEditResetToDefaults;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mnuEditResetToDefaults != null)
                {
                    _mnuEditResetToDefaults.Click -= mnuEditResetToDefaults_Click;
                }

                _mnuEditResetToDefaults = value;
                if (_mnuEditResetToDefaults != null)
                {
                    _mnuEditResetToDefaults.Click += mnuEditResetToDefaults_Click;
                }
            }
        }

        internal MenuItem MenuItem1;
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

        internal DataGrid dgWarnings;
        internal Label lblFilterData;
        private TextBox _txtFilterData;

        internal TextBox txtFilterData
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtFilterData;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtFilterData != null)
                {
                    _txtFilterData.Validated -= txtFilterData_Validated;
                    _txtFilterData.KeyDown -= txtFilterData_KeyDown;
                }

                _txtFilterData = value;
                if (_txtFilterData != null)
                {
                    _txtFilterData.Validated += txtFilterData_Validated;
                    _txtFilterData.KeyDown += txtFilterData_KeyDown;
                }
            }
        }

        internal ProgressBar pbarProgress;
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

        internal DataGrid dgErrors;
        private Button _cmdCancel;

        internal Button cmdCancel
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdCancel;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdCancel != null)
                {
                    _cmdCancel.Click -= cmdCancel_Click;
                }

                _cmdCancel = value;
                if (_cmdCancel != null)
                {
                    _cmdCancel.Click += cmdCancel_Click;
                }
            }
        }

        private CheckBox _chkKeepDuplicateNamedProteins;

        internal CheckBox chkKeepDuplicateNamedProteins
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _chkKeepDuplicateNamedProteins;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_chkKeepDuplicateNamedProteins != null)
                {
                    _chkKeepDuplicateNamedProteins.CheckedChanged -= chkKeepDuplicateNamedProteins_CheckedChanged;
                }

                _chkKeepDuplicateNamedProteins = value;
                if (_chkKeepDuplicateNamedProteins != null)
                {
                    _chkKeepDuplicateNamedProteins.CheckedChanged += chkKeepDuplicateNamedProteins_CheckedChanged;
                }
            }
        }

        private TextBox _txtResiduesPerLineForWrap;

        internal TextBox txtResiduesPerLineForWrap
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtResiduesPerLineForWrap;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtResiduesPerLineForWrap != null)
                {
                    _txtResiduesPerLineForWrap.KeyPress -= txtResiduesPerLineForWrap_TextChanged;
                }

                _txtResiduesPerLineForWrap = value;
                if (_txtResiduesPerLineForWrap != null)
                {
                    _txtResiduesPerLineForWrap.KeyPress += txtResiduesPerLineForWrap_TextChanged;
                }
            }
        }

        internal Label lblResiduesPerLineForWrap;
    }
}