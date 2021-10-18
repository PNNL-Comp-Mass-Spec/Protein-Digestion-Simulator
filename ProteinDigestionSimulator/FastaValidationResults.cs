using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using PRISM.Logging;
using DBUtils = PRISMDatabaseUtils.DataTableUtils;
using PRISMWin;
using ValidateFastaFile;

namespace ProteinDigestionSimulator
{
    public delegate void FastaValidationStartedEventHandler();

    public partial class FastaValidation
    {
        // Ignore Spelling: validator, dt, ds, chk

        public FastaValidation(string fastaFilePath) : base()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call
            InitializeForm(fastaFilePath);

            mValidationTriggerTimer = new Timer
            {
                Interval = 100,
                Enabled = true
            };

            mValidationTriggerTimer.Tick += ValidationTriggerTimer_Tick;

            mValidateFastaFile = new FastaValidator();
            RegisterEvents(mValidateFastaFile);
        }

        private const string COL_NAME_LINE = "Line";
        private const string COL_NAME_COLUMN = "Column";
        private const string COL_NAME_PROTEIN = "Protein";
        private const string COL_NAME_DESCRIPTION = "Description";
        private const string COL_NAME_CONTEXT = "Context";

        public class FastaValidationOptions
        {
            public bool Initialized { get; set; }

            public int MaximumErrorsToTrackInDetail { get; set; }
            public int MaximumResiduesPerLine { get; set; }

            public int ValidProteinNameLengthMinimum { get; set; }
            public int ValidProteinNameLengthMaximum { get; set; }
            public bool AllowAsterisksInResidues { get; set; }
            public bool CheckForDuplicateProteinNames { get; set; }
            public bool LogResultsToFile { get; set; }
            public bool SaveHashInfoFile { get; set; }

            public FixedFastaOptions FixedFastaOptions { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="fixedFastaOptions"></param>
            public FastaValidationOptions(FixedFastaOptions fixedFastaOptions)
            {
                FixedFastaOptions = fixedFastaOptions;
            }
        }

        public class FixedFastaOptions
        {
            public bool GenerateFixedFasta { get; set; }
            public bool TruncateLongProteinName { get; set; }
            public bool RenameProteinWithDuplicateNames { get; set; }
            public bool KeepDuplicateNamedProteins { get; set; }
            public bool WrapLongResidueLines { get; set; }
            public int ResiduesPerLineForWrap { get; set; }
            public bool RemoveInvalidResidues { get; set; }
            public bool SplitOutMultipleRefsForKnownAccession { get; set; }
            public bool SplitOutMultipleRefsInProteinName { get; set; }
            public bool ConsolidateDuplicateProteins { get; set; }
            // ReSharper disable once IdentifierTypo
            public bool ConsolidateDupsIgnoreILDiff { get; set; }
        }

        private DataSet mErrorsDataset;
        private DataView mErrorsDataView;

        private DataSet mWarningsDataset;
        private DataView mWarningsDataView;

        private bool mKeepDuplicateNamedProteinsLastValue = false;

        // This timer is used to cause StartValidation to be called after the form becomes visible
        private readonly Timer mValidationTriggerTimer;

        private readonly FastaValidator mValidateFastaFile;

        private string mValidatorErrorMessage;

        private readonly List<string> mValidateFastaFileErrors = new();

        private readonly List<string> mValidateFastaFileWarnings = new();

        public event FastaValidationStartedEventHandler FastaValidationStarted;

        public string CustomRulesFilePath
        {
            get => txtCustomValidationRulesFilePath.Text;
            set => txtCustomValidationRulesFilePath.Text = value;
        }

        public string FastaFilePath { get; set; }

        public float TextFontSize
        {
            get => txtResults.Font.SizeInPoints;
            set
            {
                if (value < 6f)
                {
                    value = 6f;
                }
                else if (value > 72f)
                {
                    value = 72f;
                }

                try
                {
                    txtResults.Font = new Font(txtResults.Font.FontFamily, value);

                    dgErrors.Font = new Font(txtResults.Font.FontFamily, value);
                    dgWarnings.Font = new Font(txtResults.Font.FontFamily, value);
                }
                catch
                {
                    // Ignore errors here
                }
            }
        }

        private void AppendToString(ICollection<string> results, string newText)
        {
            results.Add(newText);
        }

        private void AppendToString(ICollection<string> results, string numberDescription, long number, bool useCommaSeparator = true)
        {
            if (useCommaSeparator)
            {
                results.Add(numberDescription + number.ToString("###,###,###,###,##0"));
            }
            else
            {
                results.Add(numberDescription + number);
            }
        }

        private void AppendValidatorErrors(ICollection<string> results)
        {
            if (mValidateFastaFileErrors.Count > 0)
            {
                AppendToString(results, string.Empty);
                AppendToString(results, "Errors from the validator");

                foreach (var item in mValidateFastaFileErrors)
                {
                    AppendToString(results, item);
                }
            }

            if (mValidateFastaFileWarnings.Count > 0)
            {
                AppendToString(results, string.Empty);
                AppendToString(results, "Warnings from the validator");

                foreach (var item in mValidateFastaFileWarnings)
                {
                    AppendToString(results, item);
                }
            }
        }

        private void CopyAllResults()
        {
            Clipboard.SetDataObject(txtResults.Text + Environment.NewLine + FlattenDataView(mErrorsDataView) + Environment.NewLine + FlattenDataView(mWarningsDataView), true);
        }

        private void CopyErrorsDataView()
        {
            Clipboard.SetDataObject(FlattenDataView(mErrorsDataView), true);
        }

        private void CopyWarningsDataView()
        {
            Clipboard.SetDataObject(FlattenDataView(mWarningsDataView), true);
        }

        private void CreateDefaultValidationRulesFile()
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                DefaultExt = ".xml",
                DereferenceLinks = true,
                OverwritePrompt = false,
                ValidateNames = true,
                Filter = "XML Settings Files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (txtCustomValidationRulesFilePath.Text.Length > 0)
            {
                try
                {
                    dialog.InitialDirectory = Directory.GetParent(txtCustomValidationRulesFilePath.Text).ToString();
                }
                catch
                {
                    dialog.InitialDirectory = GetApplicationDataFolderPath();
                }
            }
            else
            {
                dialog.InitialDirectory = GetApplicationDataFolderPath();
            }

            dialog.Title = "Select/Create file to save custom rule definitions";

            dialog.ShowDialog();
            if (dialog.FileName.Length > 0)
            {
                var customRuleDefinitionsFilePath = dialog.FileName;

                try
                {
                    var validateFastaFile = new FastaValidator();
                    validateFastaFile.SaveSettingsToParameterFile(customRuleDefinitionsFilePath);

                    if (txtCustomValidationRulesFilePath.TextLength == 0)
                    {
                        txtCustomValidationRulesFilePath.Text = customRuleDefinitionsFilePath;
                    }

                    MessageBox.Show("File " + customRuleDefinitionsFilePath + " now contains the default rule validation settings.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error creating/updating file: " + customRuleDefinitionsFilePath + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        /// <summary>
        /// Display the results
        /// </summary>
        /// <param name="parameterFilePath"></param>
        private void DisplayResults(string parameterFilePath)
        {
            const char sepChar = '\t';

            var results = new List<string> { "Results for file " + mValidateFastaFile.FastaFilePath };

            AppendToString(results, "Protein count = " + mValidateFastaFile.ProteinCount + sepChar + sepChar + "Residue count = ", mValidateFastaFile.ResidueCount);
            AppendToString(results, "Error count = " + mValidateFastaFile.GetErrorWarningCounts(FastaValidator.MsgTypeConstants.ErrorMsg, FastaValidator.ErrorWarningCountTypes.Total));
            AppendToString(results, "Warning count = ", mValidateFastaFile.GetErrorWarningCounts(FastaValidator.MsgTypeConstants.WarningMsg, FastaValidator.ErrorWarningCountTypes.Total));

            if (mValidateFastaFile.GetOptionSwitchValue(FastaValidator.SwitchOptions.GenerateFixedFASTAFile))
            {
                AppendToString(results, "Count of long protein names that were truncated = " + mValidateFastaFile.GetFixedFASTAFileStats(FastaValidator.FixedFASTAFileValues.TruncatedProteinNameCount));
                AppendToString(results, "Count of protein names with invalid chars removed = " + mValidateFastaFile.GetFixedFASTAFileStats(FastaValidator.FixedFASTAFileValues.ProteinNamesInvalidCharsReplaced));
                AppendToString(results, "Count of protein names with multiple refs split out = " + mValidateFastaFile.GetFixedFASTAFileStats(FastaValidator.FixedFASTAFileValues.ProteinNamesMultipleRefsRemoved));
                AppendToString(results, "Count of residue lines with invalid chars removed = " + mValidateFastaFile.GetFixedFASTAFileStats(FastaValidator.FixedFASTAFileValues.UpdatedResidueLines));

                if (mValidateFastaFile.GetOptionSwitchValue(FastaValidator.SwitchOptions.FixedFastaRenameDuplicateNameProteins))
                {
                    AppendToString(results, "Count of proteins renamed due to duplicate names = " + mValidateFastaFile.GetFixedFASTAFileStats(FastaValidator.FixedFASTAFileValues.DuplicateProteinNamesRenamedCount));
                }
                else if (mValidateFastaFile.GetOptionSwitchValue(FastaValidator.SwitchOptions.CheckForDuplicateProteinNames))
                {
                    AppendToString(results, "Count of proteins skipped due to duplicate names = " + mValidateFastaFile.GetFixedFASTAFileStats(FastaValidator.FixedFASTAFileValues.DuplicateProteinNamesSkippedCount));
                }

                if (mValidateFastaFile.GetOptionSwitchValue(FastaValidator.SwitchOptions.FixedFastaConsolidateDuplicateProteinSeqs))
                {
                    AppendToString(results, "Count of proteins removed due to duplicate sequences = " + mValidateFastaFile.GetFixedFASTAFileStats(FastaValidator.FixedFASTAFileValues.DuplicateProteinSeqsSkippedCount));
                }
            }

            if (parameterFilePath.Length > 0)
            {
                AppendToString(results, "Used validation rules from file " + parameterFilePath);
            }
            else
            {
                AppendToString(results, "Default validation rules were used.");
            }

            var outputStatsEnabled = mValidateFastaFile.GetOptionSwitchValue(FastaValidator.SwitchOptions.OutputToStatsFile);

            if (outputStatsEnabled)
            {
                AppendToString(results, "Results were logged to file: " + mValidateFastaFile.StatsFilePath);
            }

            AppendValidatorErrors(results);

            txtResults.Text = string.Join(Environment.NewLine, results);

            txtResults.SelectionStart = 0;
            txtResults.SelectionLength = 0;

            // Clear the filters
            txtFilterData.Text = string.Empty;
            if (mValidateFastaFile.GetErrorWarningCounts(FastaValidator.MsgTypeConstants.ErrorMsg, FastaValidator.ErrorWarningCountTypes.Specified) > 0)
            {
                // List all of the errors
                PopulateMsgResultsDataGrid(dgErrors, mErrorsDataset, mValidateFastaFile.FileErrorList);
            }
            else
            {
                mErrorsDataset.Tables[0].Clear();
            }

            if (mValidateFastaFile.GetErrorWarningCounts(FastaValidator.MsgTypeConstants.WarningMsg, FastaValidator.ErrorWarningCountTypes.Specified) > 0)
            {
                // List all of the warnings in the DataGrid
                PopulateMsgResultsDataGrid(dgWarnings, mWarningsDataset, mValidateFastaFile.FileWarningList);
            }
            else
            {
                mWarningsDataset.Tables[0].Clear();
            }

            PositionControls();
            FilterLists();
        }

        private void EnableDisableControls()
        {
            var enableFixedFastaOptions = chkGenerateFixedFastaFile.Checked;

            txtLongProteinNameSplitChars.Enabled = enableFixedFastaOptions;
            txtInvalidProteinNameCharsToRemove.Enabled = enableFixedFastaOptions;
            txtResiduesPerLineForWrap.Enabled = enableFixedFastaOptions && chkWrapLongResidueLines.Checked;

            chkSplitOutMultipleRefsInProteinName.Enabled = enableFixedFastaOptions;
            chkRenameDuplicateProteins.Enabled = enableFixedFastaOptions;

            if (enableFixedFastaOptions)
            {
                if (chkRenameDuplicateProteins.Checked)
                {
                    chkKeepDuplicateNamedProteins.Enabled = false;
                    chkKeepDuplicateNamedProteins.Checked = false;
                }
                else
                {
                    chkKeepDuplicateNamedProteins.Enabled = true;
                    chkKeepDuplicateNamedProteins.Checked = mKeepDuplicateNamedProteinsLastValue;
                }
            }
            else
            {
                chkKeepDuplicateNamedProteins.Enabled = false;
            }

            chkConsolidateDuplicateProteinSeqs.Enabled = enableFixedFastaOptions;
            chkConsolidateDupsIgnoreILDiff.Enabled = enableFixedFastaOptions && chkConsolidateDuplicateProteinSeqs.Checked;

            chkTruncateLongProteinNames.Enabled = enableFixedFastaOptions;
            chkSplitOutMultipleRefsForKnownAccession.Enabled = enableFixedFastaOptions;
            chkWrapLongResidueLines.Enabled = enableFixedFastaOptions;
            chkRemoveInvalidResidues.Enabled = enableFixedFastaOptions;

            chkAllowAsteriskInResidues.Enabled = txtCustomValidationRulesFilePath.TextLength == 0;
        }

        /// <summary>
        /// Copy the contents of the DataGrid to the clipboard
        /// </summary>
        /// <param name="dvDataView"></param>
        private string FlattenDataView(DataView dvDataView)
        {
            const char sepChar = '\t';

            var flattenedText = string.Empty;

            try
            {
                var columnCount = dvDataView.Table.Columns.Count;
                for (var index = 0; index < columnCount; index++)
                {
                    if (index < columnCount - 1)
                    {
                        flattenedText += dvDataView.Table.Columns[index].ColumnName + sepChar;
                    }
                    else
                    {
                        flattenedText += dvDataView.Table.Columns[index].ColumnName + Environment.NewLine;
                    }
                }

                foreach (DataRow currentRow in dvDataView.Table.Rows)
                {
                    for (var index = 0; index < columnCount; index++)
                    {
                        if (index < columnCount - 1)
                        {
                            flattenedText += currentRow[index].ToString() + sepChar;
                        }
                        else
                        {
                            flattenedText += currentRow[index] + Environment.NewLine;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                flattenedText += Environment.NewLine + Environment.NewLine + "Error copying data: " + ex.Message;
            }

            return flattenedText;
        }

        private void CopySummaryText()
        {
            var selStart = txtResults.SelectionStart;
            var selLength = txtResults.SelectionLength;

            txtResults.SelectAll();
            txtResults.Copy();

            txtResults.SelectionStart = selStart;
            txtResults.SelectionLength = selLength;
        }

        private void FilterLists()
        {
            try
            {
                var filter = string.Empty;
                if (txtFilterData.TextLength > 0)
                {
                    filter = "[" + COL_NAME_PROTEIN + "] LIKE '%" + txtFilterData.Text + "%' OR [" + COL_NAME_DESCRIPTION + "] LIKE '%" + txtFilterData.Text + "%' OR [" + COL_NAME_CONTEXT + "] LIKE '%" + txtFilterData.Text + "%'";
                }

                mErrorsDataView.RowFilter = filter;
                dgErrors.Update();

                mWarningsDataView.RowFilter = filter;
                dgWarnings.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering lists: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private string GetApplicationDataFolderPath()
        {
            var appDataFolderPath = string.Empty;
            try
            {
                appDataFolderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"PAST Toolkit\ProteinDigestionSimulator");

                if (!Directory.Exists(appDataFolderPath))
                {
                    Directory.CreateDirectory(appDataFolderPath);
                }
            }
            catch
            {
                // Ignore errors here; an exception will likely be thrown by the calling function that is trying to access this non-existent application data folder
            }

            return appDataFolderPath;
        }

        public FastaValidationOptions GetOptions()
        {
            var fixedFastaOptions = new FixedFastaOptions
            {
                GenerateFixedFasta = chkGenerateFixedFastaFile.Checked,
                TruncateLongProteinName = chkTruncateLongProteinNames.Checked,
                RenameProteinWithDuplicateNames = chkRenameDuplicateProteins.Checked,
                KeepDuplicateNamedProteins = chkKeepDuplicateNamedProteins.Checked,
                WrapLongResidueLines = chkWrapLongResidueLines.Checked,
                RemoveInvalidResidues = chkRemoveInvalidResidues.Checked,
                SplitOutMultipleRefsForKnownAccession = chkSplitOutMultipleRefsForKnownAccession.Checked,
                SplitOutMultipleRefsInProteinName = chkSplitOutMultipleRefsInProteinName.Checked,
                ConsolidateDuplicateProteins = chkConsolidateDuplicateProteinSeqs.Checked,
                ConsolidateDupsIgnoreILDiff = chkConsolidateDupsIgnoreILDiff.Checked,
                ResiduesPerLineForWrap = TextBoxUtils.ParseTextBoxValueInt(txtResiduesPerLineForWrap, string.Empty, out _, 60)
            };

            return new FastaValidationOptions(fixedFastaOptions)
            {
                MaximumErrorsToTrackInDetail = TextBoxUtils.ParseTextBoxValueInt(txtMaxFileErrorsToTrack, string.Empty, out _, 10),
                MaximumResiduesPerLine = TextBoxUtils.ParseTextBoxValueInt(txtMaximumResiduesPerLine, string.Empty, out _, 120),
                ValidProteinNameLengthMinimum = TextBoxUtils.ParseTextBoxValueInt(txtProteinNameLengthMinimum, string.Empty, out _, 3),
                ValidProteinNameLengthMaximum = TextBoxUtils.ParseTextBoxValueInt(txtProteinNameLengthMaximum, string.Empty, out _, 34),
                AllowAsterisksInResidues = chkAllowAsteriskInResidues.Checked,
                CheckForDuplicateProteinNames = chkCheckForDuplicateProteinInfo.Checked,
                LogResultsToFile = chkLogResults.Checked,
                SaveHashInfoFile = chkSaveBasicProteinHashInfoFile.Checked,
                Initialized = true
            };
        }

        private void InitializeDataGrid(DataGrid dgDataGrid, out DataSet dsDataset, out DataView dvDataView, FastaValidator.MsgTypeConstants msgType)
        {
            var msgColumnName = msgType switch
            {
                FastaValidator.MsgTypeConstants.WarningMsg => "Warnings",
                FastaValidator.MsgTypeConstants.ErrorMsg => "Errors",
                _ => "Status",
            };
            var datasetName = "ds" + msgColumnName;
            var dataTableName = "T_" + msgColumnName;

            // Create a DataTable
            var dtDataTable = new DataTable(dataTableName);

            // Add the columns to the DataTable
            DBUtils.AppendColumnIntegerToTable(dtDataTable, COL_NAME_LINE);
            DBUtils.AppendColumnIntegerToTable(dtDataTable, COL_NAME_COLUMN);
            DBUtils.AppendColumnStringToTable(dtDataTable, COL_NAME_PROTEIN);
            DBUtils.AppendColumnStringToTable(dtDataTable, COL_NAME_DESCRIPTION);
            DBUtils.AppendColumnStringToTable(dtDataTable, COL_NAME_CONTEXT);

            // Could define a primary key
            // dtDataTable.PrimaryKey = new DataColumn() {dtDataTable.Columns[COL_NAME_LINE]};

            // Instantiate the DataSet
            dsDataset = new DataSet(datasetName);

            // Add the table to the DataSet
            dsDataset.Tables.Add(dtDataTable);

            // Instantiate the DataView
            dvDataView = new DataView
            {
                Table = dsDataset.Tables[dataTableName],
                RowFilter = string.Empty
            };

            // Bind the DataSet to the DataGrid
            dgDataGrid.DataSource = dvDataView;
            dgDataGrid.ReadOnly = false;
            dgDataGrid.CaptionText = msgColumnName;

            // Update the grid's table style
            UpdateDataGridTableStyle(dgDataGrid, dataTableName);
        }

        private void InitializeForm(string fastaFilePathToValidate)
        {
            txtResults.ReadOnly = true;
            TextFontSize = 10f;

            InitializeDataGrid(dgErrors, out mErrorsDataset, out mErrorsDataView, FastaValidator.MsgTypeConstants.ErrorMsg);
            InitializeDataGrid(dgWarnings, out mWarningsDataset, out mWarningsDataView, FastaValidator.MsgTypeConstants.WarningMsg);

            SetNewFastaFile(fastaFilePathToValidate);

            EnableDisableControls();
            SetToolTips();

            ResetOptionsToDefault();
        }

        private void PopulateMsgResultsDataGrid(Control dgDataGrid, DataSet dsDataset, IEnumerable<FastaValidator.MsgInfo> itemList)
        {
            // Clear the table
            dsDataset.Tables[0].Clear();

            // Populate it with new data
            foreach (var item in itemList)
            {
                var currentRow = dsDataset.Tables[0].NewRow();
                currentRow[COL_NAME_LINE] = item.LineNumber;
                currentRow[COL_NAME_COLUMN] = item.ColNumber;
                if (string.IsNullOrEmpty(item.ProteinName))
                {
                    currentRow[COL_NAME_PROTEIN] = "N/A";
                }
                else
                {
                    currentRow[COL_NAME_PROTEIN] = item.ProteinName;
                }

                currentRow[COL_NAME_DESCRIPTION] = mValidateFastaFile.LookupMessageDescription(item.MessageCode, item.ExtraInfo);
                currentRow[COL_NAME_CONTEXT] = item.Context;

                // Add the row to the table
                dsDataset.Tables[0].Rows.Add(currentRow);
            }

            dgDataGrid.Update();
        }

        private void PositionControls()
        {
            const float MAX_RATIO = 1.5f;
            const int MENU_HEIGHT = 80;

            int desiredHeight;

            var errorToWarningsRatio = 1d;
            try
            {
                if (mErrorsDataset != null && mWarningsDataset != null)
                {
                    if (mErrorsDataset.Tables[0].Rows.Count == 0 && mWarningsDataset.Tables[0].Rows.Count == 0)
                    {
                        errorToWarningsRatio = 1d;
                    }
                    else if (mErrorsDataset.Tables[0].Rows.Count == 0)
                    {
                        errorToWarningsRatio = 1f / MAX_RATIO;
                    }
                    else if (mWarningsDataset.Tables[0].Rows.Count == 0)
                    {
                        errorToWarningsRatio = MAX_RATIO;
                    }
                    else
                    {
                        errorToWarningsRatio = mErrorsDataset.Tables[0].Rows.Count / (double)mWarningsDataset.Tables[0].Rows.Count;

                        if (errorToWarningsRatio < 1f / MAX_RATIO)
                        {
                            errorToWarningsRatio = 1f / MAX_RATIO;
                        }
                        else if (errorToWarningsRatio > MAX_RATIO)
                        {
                            errorToWarningsRatio = MAX_RATIO;
                        }
                    }
                }
            }
            catch
            {
                errorToWarningsRatio = 1d;
            }

            if (errorToWarningsRatio >= 1d)
            {
                // Errors grid should be taller
                desiredHeight = (int)Math.Round(Math.Round((Height - dgErrors.Top - MENU_HEIGHT) / (errorToWarningsRatio + 1d) * errorToWarningsRatio, 0));
            }
            else
            {
                // Errors grid should be shorter
                desiredHeight = (int)Math.Round(Math.Round((Height - dgErrors.Top - MENU_HEIGHT) / (1d / errorToWarningsRatio + 1d), 0)) - 2;
            }

            if (desiredHeight < 5)
            {
                desiredHeight = 5;
            }

            dgErrors.Height = desiredHeight;

            dgWarnings.Top = dgErrors.Top + dgErrors.Height + 10;

            desiredHeight = (int)Math.Round(Math.Round(desiredHeight / errorToWarningsRatio, 0));
            if (desiredHeight < 5)
            {
                desiredHeight = 5;
            }

            dgWarnings.Height = desiredHeight;
        }

        private void ResetOptionsToDefault()
        {
            txtMaxFileErrorsToTrack.Text = "10";

            txtMaximumResiduesPerLine.Text = "120";

            txtProteinNameLengthMinimum.Text = "3";
            txtProteinNameLengthMaximum.Text = "34";

            txtLongProteinNameSplitChars.Text = FastaValidator.DEFAULT_LONG_PROTEIN_NAME_SPLIT_CHAR.ToString();
            txtInvalidProteinNameCharsToRemove.Text = string.Empty;
            txtResiduesPerLineForWrap.Text = "60";

            chkAllowAsteriskInResidues.Checked = false;
            chkCheckForDuplicateProteinInfo.Checked = true;
            chkSaveBasicProteinHashInfoFile.Checked = false;

            chkLogResults.Checked = false;

            // Note: Leave this option unchanged
            // chkGenerateFixedFastaFile.Checked = False

            chkConsolidateDuplicateProteinSeqs.Checked = true;
            chkConsolidateDupsIgnoreILDiff.Checked = true;

            chkRemoveInvalidResidues.Checked = false;
            chkRenameDuplicateProteins.Checked = true;
            chkKeepDuplicateNamedProteins.Checked = false;
            chkSplitOutMultipleRefsForKnownAccession.Checked = false;
            chkSplitOutMultipleRefsInProteinName.Checked = false;
            chkTruncateLongProteinNames.Checked = true;
            chkWrapLongResidueLines.Checked = true;
        }

        private void SelectCustomRulesFile()
        {
            var dialog = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".xml",
                DereferenceLinks = true,
                Multiselect = false,
                ValidateNames = true,
                Filter = "XML Settings Files (*.xml)|*.xml|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (txtCustomValidationRulesFilePath.Text.Length > 0)
            {
                try
                {
                    dialog.InitialDirectory = Directory.GetParent(txtCustomValidationRulesFilePath.Text).ToString();
                }
                catch
                {
                    dialog.InitialDirectory = GetApplicationDataFolderPath();
                }
            }
            else
            {
                dialog.InitialDirectory = GetApplicationDataFolderPath();
            }

            dialog.Title = "Select custom rules file";

            dialog.ShowDialog();
            if (dialog.FileName.Length > 0)
            {
                txtCustomValidationRulesFilePath.Text = dialog.FileName;
            }
        }

        public void SetNewFastaFile(string fastaFilePathToValidate)
        {
            FastaFilePath = fastaFilePathToValidate;

            txtResults.Text = string.Empty;

            // Clear the filters
            txtFilterData.Text = string.Empty;

            mErrorsDataset.Tables[0].Clear();
            mWarningsDataset.Tables[0].Clear();
        }

        private void ShowAboutBox()
        {
            var message = new StringBuilder();

            message.AppendLine("FASTA File Validation module written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2005");
            message.AppendLine("Copyright 2018 Battelle Memorial Institute");
            message.AppendLine();
            message.AppendFormat("This is version {0} ({1})", Application.ProductVersion, Program.PROGRAM_DATE);
            message.AppendLine();
            message.AppendLine();
            message.AppendLine("E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov");
            message.AppendLine("Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics");
            message.AppendLine();
            message.AppendLine("Licensed under the 2-Clause BSD License; https://opensource.org/licenses/BSD-2-Clause");
            message.AppendLine();
            message.Append("This software is provided by the copyright holders and contributors \"as is\" and ");
            message.Append("any express or implied warranties, including, but not limited to, the implied ");
            message.Append("warranties of merchantability and fitness for a particular purpose are ");
            message.Append("disclaimed. In no event shall the copyright holder or contributors be liable ");
            message.Append("for any direct, indirect, incidental, special, exemplary, or consequential ");
            message.Append("damages (including, but not limited to, procurement of substitute goods or ");
            message.Append("services; loss of use, data, or profits; or business interruption) however ");
            message.Append("caused and on any theory of liability, whether in contract, strict liability, ");
            message.Append("or tort (including negligence or otherwise) arising in any way out of the use ");
            message.Append("of this software, even if advised of the possibility of such damage.");

            MessageBox.Show(message.ToString(), "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowHideObjectsDuringValidation(bool validating)
        {
            pbarProgress.Visible = validating;

            lblFilterData.Visible = !validating;
            txtFilterData.Visible = !validating;

            cmdValidateFastaFile.Visible = !validating;
            cmdValidateFastaFile.Enabled = !validating;

            cmdCancel.Visible = validating;
            cmdCancel.Enabled = validating;
        }

        public void SetOptions(FastaValidationOptions fastaValidationOptions)
        {
            txtMaxFileErrorsToTrack.Text = fastaValidationOptions.MaximumErrorsToTrackInDetail.ToString();
            txtMaximumResiduesPerLine.Text = fastaValidationOptions.MaximumResiduesPerLine.ToString();

            txtProteinNameLengthMinimum.Text = fastaValidationOptions.ValidProteinNameLengthMinimum.ToString();
            txtProteinNameLengthMaximum.Text = fastaValidationOptions.ValidProteinNameLengthMaximum.ToString();

            chkAllowAsteriskInResidues.Checked = fastaValidationOptions.AllowAsterisksInResidues;
            chkCheckForDuplicateProteinInfo.Checked = fastaValidationOptions.CheckForDuplicateProteinNames;
            chkLogResults.Checked = fastaValidationOptions.LogResultsToFile;
            chkSaveBasicProteinHashInfoFile.Checked = fastaValidationOptions.SaveHashInfoFile;

            chkGenerateFixedFastaFile.Checked = fastaValidationOptions.FixedFastaOptions.GenerateFixedFasta;
            chkTruncateLongProteinNames.Checked = fastaValidationOptions.FixedFastaOptions.TruncateLongProteinName;
            chkRenameDuplicateProteins.Checked = fastaValidationOptions.FixedFastaOptions.RenameProteinWithDuplicateNames;
            chkKeepDuplicateNamedProteins.Checked = fastaValidationOptions.FixedFastaOptions.KeepDuplicateNamedProteins;
            chkWrapLongResidueLines.Checked = fastaValidationOptions.FixedFastaOptions.WrapLongResidueLines;
            chkRemoveInvalidResidues.Checked = fastaValidationOptions.FixedFastaOptions.RemoveInvalidResidues;
            chkSplitOutMultipleRefsForKnownAccession.Checked = fastaValidationOptions.FixedFastaOptions.SplitOutMultipleRefsForKnownAccession;
            chkSplitOutMultipleRefsInProteinName.Checked = fastaValidationOptions.FixedFastaOptions.SplitOutMultipleRefsInProteinName;
            chkConsolidateDuplicateProteinSeqs.Checked = fastaValidationOptions.FixedFastaOptions.ConsolidateDuplicateProteins;
            chkConsolidateDupsIgnoreILDiff.Checked = fastaValidationOptions.FixedFastaOptions.ConsolidateDupsIgnoreILDiff;
            txtResiduesPerLineForWrap.Text = fastaValidationOptions.FixedFastaOptions.ResiduesPerLineForWrap.ToString();

            Application.DoEvents();
        }

        private void SetToolTips()
        {
            var toolTipControl = new ToolTip();

            toolTipControl.SetToolTip(txtLongProteinNameSplitChars, "Enter one or more characters to look for when truncating long protein names (do not separate the characters by commas).  Default character is a vertical bar.");
            toolTipControl.SetToolTip(txtInvalidProteinNameCharsToRemove, "Enter one or more characters to look and replace with an underscore (do not separate the characters by commas).  Leave blank to not replace any characters.");
            toolTipControl.SetToolTip(chkSplitOutMultipleRefsForKnownAccession, "If a protein name matches the standard IPI, GI, or JGI accession numbers, and if it contains additional reference information, then the additional information will be moved to the protein's description.");
            toolTipControl.SetToolTip(chkSaveBasicProteinHashInfoFile, "To minimize memory usage, enable this option by disable 'Check for Duplicate Proteins'");
        }

        private void StartValidation()
        {
            try
            {
                mValidatorErrorMessage = string.Empty;
                mValidateFastaFileErrors.Clear();
                mValidateFastaFileWarnings.Clear();

                FastaValidationStarted?.Invoke();

                ShowHideObjectsDuringValidation(true);
                Cursor.Current = Cursors.WaitCursor;
                Application.DoEvents();

                // Note: the following settings will be overridden if a parameter file with these settings defined is provided to .ProcessFile()
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.WarnBlankLinesBetweenProteins, true);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.AllowAsteriskInResidues, chkAllowAsteriskInResidues.Checked);

                mValidateFastaFile.MinimumProteinNameLength = TextBoxUtils.ParseTextBoxValueInt(txtProteinNameLengthMinimum, "Minimum protein name length should be a number", out _, 3);
                mValidateFastaFile.MaximumProteinNameLength = TextBoxUtils.ParseTextBoxValueInt(txtProteinNameLengthMaximum, "Maximum protein name length should be a number", out _, 34);

                if (chkGenerateFixedFastaFile.Checked && chkWrapLongResidueLines.Checked)
                {
                    mValidateFastaFile.MaximumResiduesPerLine = TextBoxUtils.ParseTextBoxValueInt(txtResiduesPerLineForWrap, "Residues per line for wrapping should be a number", out _, 60);
                }
                else
                {
                    mValidateFastaFile.MaximumResiduesPerLine = TextBoxUtils.ParseTextBoxValueInt(txtMaximumResiduesPerLine, "Maximum residues per line should be a number", out _, 120);
                }

                var parameterFilePath = txtCustomValidationRulesFilePath.Text;
                if (parameterFilePath.Length > 0)
                {
                    bool fileExists;
                    try
                    {
                        fileExists = File.Exists(parameterFilePath);
                    }
                    catch
                    {
                        fileExists = false;
                    }

                    if (!fileExists)
                    {
                        MessageBox.Show("Custom rules validation file not found: " + Environment.NewLine + parameterFilePath + Environment.NewLine + "The default validation rules will be used instead.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        parameterFilePath = string.Empty;
                    }
                    else
                    {
                        mValidateFastaFile.LoadParameterFileSettings(parameterFilePath);
                    }
                }

                if (parameterFilePath.Length == 0)
                {
                    mValidateFastaFile.SetDefaultRules();
                }

                mValidateFastaFile.MaximumFileErrorsToTrack = TextBoxUtils.ParseTextBoxValueInt(txtMaxFileErrorsToTrack, "Max file errors or warnings should be a positive number", out _, 10);

                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.CheckForDuplicateProteinNames, chkCheckForDuplicateProteinInfo.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.CheckForDuplicateProteinSequences, chkCheckForDuplicateProteinInfo.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.SaveBasicProteinHashInfoFile, chkSaveBasicProteinHashInfoFile.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.OutputToStatsFile, chkLogResults.Checked);

                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.GenerateFixedFASTAFile, chkGenerateFixedFastaFile.Checked);

                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.FixedFastaSplitOutMultipleRefsForKnownAccession, chkSplitOutMultipleRefsForKnownAccession.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.SplitOutMultipleRefsInProteinName, chkSplitOutMultipleRefsInProteinName.Checked);

                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.FixedFastaRenameDuplicateNameProteins, chkRenameDuplicateProteins.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.FixedFastaKeepDuplicateNamedProteins, chkKeepDuplicateNamedProteins.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.FixedFastaConsolidateDuplicateProteinSeqs, chkConsolidateDuplicateProteinSeqs.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.FixedFastaConsolidateDupsIgnoreILDiff, chkConsolidateDupsIgnoreILDiff.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.FixedFastaTruncateLongProteinNames, chkTruncateLongProteinNames.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.FixedFastaWrapLongResidueLines, chkWrapLongResidueLines.Checked);
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.FixedFastaRemoveInvalidResidues, chkRemoveInvalidResidues.Checked);

                mValidateFastaFile.ProteinNameFirstRefSepChars = FastaValidator.DEFAULT_PROTEIN_NAME_FIRST_REF_SEP_CHARS;
                mValidateFastaFile.ProteinNameSubsequentRefSepChars = FastaValidator.DEFAULT_PROTEIN_NAME_SUBSEQUENT_REF_SEP_CHARS;

                // Also apply chkGenerateFixedFastaFile to SaveProteinSequenceHashInfoFiles
                mValidateFastaFile.SetOptionSwitch(FastaValidator.SwitchOptions.SaveProteinSequenceHashInfoFiles, chkGenerateFixedFastaFile.Checked);

                if (txtLongProteinNameSplitChars.TextLength > 0)
                {
                    mValidateFastaFile.LongProteinNameSplitChars = txtLongProteinNameSplitChars.Text;
                }
                else
                {
                    mValidateFastaFile.LongProteinNameSplitChars = FastaValidator.DEFAULT_LONG_PROTEIN_NAME_SPLIT_CHAR.ToString();
                }

                mValidateFastaFile.ProteinNameInvalidCharsToRemove = txtInvalidProteinNameCharsToRemove.Text;

                pbarProgress.Minimum = 0;
                pbarProgress.Maximum = 100;
                pbarProgress.Value = 0;

                // Analyze the FASTA file; returns true if the analysis was successful (even if the file contains errors or warnings)
                var success = mValidateFastaFile.ProcessFile(FastaFilePath, string.Empty, string.Empty);

                cmdCancel.Enabled = false;

                if (success)
                {
                    DisplayResults(parameterFilePath);
                }
                else
                {
                    var results = new List<string> { "Error calling mValidateFastaFile.ProcessFile: " + mValidateFastaFile.GetErrorMessage() };
                    AppendValidatorErrors(results);

                    txtResults.Text = string.Join(Environment.NewLine, results);

                    if (!string.IsNullOrEmpty(mValidatorErrorMessage))
                    {
                        txtResults.AppendText(Environment.NewLine + mValidatorErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error validating FASTA file: " + FastaFilePath + Environment.NewLine + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                ShowHideObjectsDuringValidation(false);

                Cursor.Current = Cursors.Default;
                Application.DoEvents();
            }
        }

        private void UpdateDataGridTableStyle(DataGrid dgDataGrid, string targetTableName)
        {
            // Instantiate the TableStyle
            // Setting the MappingName of the table style to targetTableName will cause this style to be used with that table
            var tsTableStyle = new DataGridTableStyle
            {
                MappingName = targetTableName,
                AllowSorting = true,
                ColumnHeadersVisible = true,
                RowHeadersVisible = false,
                ReadOnly = true
            };

            DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_LINE, "Line", 80);
            DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_COLUMN, "Column", 80);
            DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_PROTEIN, "Protein", 200);
            DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_DESCRIPTION, "Value", 550);
            DataGridUtils.AppendColumnToTableStyle(tsTableStyle, COL_NAME_CONTEXT, "Context", 200);

            dgDataGrid.TableStyles.Clear();

            if (!dgDataGrid.TableStyles.Contains(tsTableStyle))
            {
                dgDataGrid.TableStyles.Add(tsTableStyle);
            }

            dgDataGrid.Refresh();
        }

        private void chkGenerateFixedFastaFile_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkConsolidateDuplicateProteinSeqs_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkRenameDuplicateProteins_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkKeepDuplicateNamedProteins_CheckedChanged(object sender, EventArgs e)
        {
            if (chkKeepDuplicateNamedProteins.Enabled)
            {
                mKeepDuplicateNamedProteinsLastValue = chkKeepDuplicateNamedProteins.Checked;
            }
        }

        private void chkWrapLongResidueLines_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            if (mValidateFastaFile != null)
            {
                cmdCancel.Enabled = false;
                mValidateFastaFile.AbortProcessingNow();
            }
        }

        private void cmdCreateDefaultValidationRulesFile_Click_1(object sender, EventArgs e)
        {
            CreateDefaultValidationRulesFile();
        }

        private void cmdSelectCustomRulesFile_Click(object sender, EventArgs e)
        {
            SelectCustomRulesFile();
        }

        private void cmdValidateFastaFile_Click(object sender, EventArgs e)
        {
            StartValidation();
        }

        private void frmFastaValidationResults_Resize(object sender, EventArgs e)
        {
            PositionControls();
        }

        private void txtCustomValidationRulesFilePath_TextChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void txtFilterData_Validated(object sender, EventArgs e)
        {
            FilterLists();
        }

        private void txtFilterData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FilterLists();
            }
        }

        private void txtMaxFileErrorsToTrack_KeyPress1(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtMaxFileErrorsToTrack, e);
        }

        private void txtMaximumResiduesPerLine_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtMaximumResiduesPerLine, e);
        }

        private void txtProteinNameLengthMinimum_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtProteinNameLengthMinimum, e);
        }

        private void txtProteinNameLengthMaximum_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtProteinNameLengthMaximum, e);
        }

        private void txtResiduesPerLineForWrap_TextChanged(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtResiduesPerLineForWrap, e);
        }

        private void txtResults_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.A)
                {
                    txtResults.SelectAll();
                }
            }
        }

        /// <summary>
        /// This timer is used to cause StartValidation to be called after the form becomes visible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidationTriggerTimer_Tick(object sender, EventArgs e)
        {
            mValidationTriggerTimer.Enabled = false;
            try
            {
                // Check whether the FASTA file is over 250 MB in size
                // If it is, auto-disable the check for duplicate proteins (to avoid using too much memory)
                var fastaFile = new FileInfo(FastaFilePath);
                if (fastaFile.Exists)
                {
                    if (fastaFile.Length > 250 * 1024 * 1024)
                    {
                        chkCheckForDuplicateProteinInfo.Checked = false;
                    }

                    StartValidation();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error examining the FASTA file size: " + ex.Message);
            }
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuEditCopySummary_Click(object sender, EventArgs e)
        {
            CopySummaryText();
        }

        private void mnuEditCopyAllResults_Click(object sender, EventArgs e)
        {
            CopyAllResults();
        }

        private void mnuEditCopyAllErrors_Click(object sender, EventArgs e)
        {
            CopyErrorsDataView();
        }

        private void mnuEditCopyAllWarnings_Click(object sender, EventArgs e)
        {
            CopyWarningsDataView();
        }

        private void mnuEditFontSizeDecrease_Click(object sender, EventArgs e)
        {
            if (TextFontSize > 14f)
            {
                TextFontSize -= 2f;
            }
            else
            {
                TextFontSize--;
            }
        }

        private void mnuEditFontSizeIncrease_Click(object sender, EventArgs e)
        {
            if (TextFontSize >= 14f)
            {
                TextFontSize += 2f;
            }
            else
            {
                TextFontSize++;
            }
        }

        private void mnuEditResetToDefaults_Click(object sender, EventArgs e)
        {
            ResetOptionsToDefault();
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            ShowAboutBox();
        }

        private void RegisterEvents(IEventNotifier obj)
        {
            obj.StatusEvent += MessageEventHandler;
            obj.ErrorEvent += ErrorEventHandler;
            obj.ProgressUpdate += ProgressUpdateHandler;
            obj.WarningEvent += WarningEventHandler;
        }

        private void ErrorEventHandler(string message, Exception ex)
        {
            mValidatorErrorMessage = message;

            if (ex != null && !message.Contains(ex.Message))
            {
                mValidateFastaFileErrors.Add(message + ": " + ex.Message);
            }
            else
            {
                mValidateFastaFileErrors.Add(message);
            }
        }

        private void MessageEventHandler(string message)
        {
            // Uncomment to debug:
            // Console.WriteLine(message)
        }

        private void ProgressUpdateHandler(string taskDescription, float percentComplete)
        {
            try
            {
                pbarProgress.Value = (int)Math.Round(percentComplete);
                Application.DoEvents();
            }
            catch
            {
                // Ignore errors here
            }
        }

        private void WarningEventHandler(string message)
        {
            mValidateFastaFileWarnings.Add(message);
        }
    }
}
