using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NETPrediction;
using PRISM;
using PRISM.FileProcessor;
using DBUtils = PRISMDatabaseUtils.DataTableUtils;
using PRISMWin;
using ProteinDigestionSimulator.Options;
using ProteinFileReader;

// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Started in 2004
//
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the 2-Clause BSD License; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// https://opensource.org/licenses/BSD-2-Clause
//
// Copyright 2021 Battelle Memorial Institute

namespace ProteinDigestionSimulator
{
    public partial class Main
    {
        // Ignore Spelling: al, cbo, chk, combobox, ComputepI, const, CrLf, Cys, Da, diff, Eisenberg, Engleman
        // Ignore Spelling: frm, gauging, Hopp, Hydrophobicity, Iodoacetamide, Iodoacetic, Kangas, Kostas, Kyte
        // Ignore Spelling: MaximumpI, MaxpI, MinimumpI, Petritis, Sep, Sql, tryptic

        public Main()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.

            mDefaultFastaFileOptions = new ProteinFileParser.FastaFileParseOptions { ReadonlyClass = true };

            mProteinDigestionSimulator.ErrorEvent += ProteinDigestionSimulator_ErrorEvent;
            mProteinDigestionSimulator.ProgressUpdate += ProteinDigestionSimulator_ProgressChanged;
            mProteinDigestionSimulator.ProgressComplete += ProteinDigestionSimulator_ProgressComplete;
            mProteinDigestionSimulator.ProgressReset += ProteinDigestionSimulator_ProgressReset;
            mProteinDigestionSimulator.SubtaskProgressChanged += ProteinDigestionSimulator_SubtaskProgressChanged;

            NETCalculator = new ElutionTimePredictionKangas();
            SCXNETCalculator = new SCXElutionTimePredictionKangas();

            mCleavageRuleComboboxIndexToType = new Dictionary<int, CleavageRuleConstants>();
            mTabPageIndexSaved = 0;

            InitializeControls();
        }

        private const string XML_SETTINGS_FILE_NAME = "ProteinDigestionSimulatorOptions.xml";

        private const string OUTPUT_FILE_SUFFIX = "_output.txt";                         // Note that this const starts with an underscore
        private const string PEAK_MATCHING_STATS_FILE_SUFFIX = "_PeakMatching.txt";      // Note that this const starts with an underscore

        private const string PM_THRESHOLDS_DATA_TABLE = "PeakMatchingThresholds";

        private const string COL_NAME_MASS_TOLERANCE = "MassTolerance";
        private const string COL_NAME_NET_TOLERANCE = "NETTolerance";
        private const string COL_NAME_SLIC_MASS_STDEV = "SLiCMassStDev";
        private const string COL_NAME_SLIC_NET_STDEV = "SLiCNETStDev";
        private const string COL_NAME_PM_THRESHOLD_ROW_ID = "PMThresholdRowID";

        private const double DEFAULT_SLIC_MASS_STDEV = 3d;
        private const double DEFAULT_SLIC_NET_STDEV = 0.025d;

        private const int PROGRESS_TAB_INDEX = 4;

        /// <summary>
        /// Input file format constants
        /// </summary>
        private enum InputFileFormatConstants
        {
            AutoDetermine = 0,
            FastaFile = 1,
            DelimitedText = 2
        }

        private const int PREDEFINED_PM_THRESHOLDS_COUNT = 5;

        /// <summary>
        /// Predefined peak matching threshold constants
        /// </summary>
        private enum PredefinedPMThresholdsConstants
        {
            OneMassOneNET = 0,
            OneMassThreeNET = 1,
            ThreeMassOneNET = 2,
            ThreeMassThreeNET = 3,
            FiveMassThreeNET = 4
        }

        private readonly struct PeakMatchingThresholds
        {
            public double MassTolerance { get; }
            public double NETTolerance { get; }

            public PeakMatchingThresholds(double massTolerance, double netTolerance)
            {
                MassTolerance = massTolerance;
                NETTolerance = netTolerance;
            }
        }

        private class PredefinedPMThresholds
        {
            public PeakMatching.MassToleranceConstants MassTolType { get; }
            public List<PeakMatchingThresholds> Thresholds { get; }

            public PredefinedPMThresholds(PeakMatching.MassToleranceConstants massTolType)
            {
                MassTolType = massTolType;
                Thresholds = new List<PeakMatchingThresholds>();
            }
        }

        // The following is used to lookup the default symbols for FASTA files, and should thus be treated as ReadOnly
        private readonly ProteinFileParser.FastaFileParseOptions mDefaultFastaFileOptions;

        private DataSet mPeakMatchingThresholdsDataset;
        private PredefinedPMThresholds[] mPredefinedPMThresholds;

        private bool mWorking;
        private string mCustomValidationRulesFilePath;

        private readonly ElutionTimePredictionKangas NETCalculator;

        private readonly SCXElutionTimePredictionKangas SCXNETCalculator;

        /// <summary>
        /// Keys in this dictionary are the index in combobox cboCleavageRuleType, values are the cleavage rule enum for that index
        /// </summary>
        private readonly Dictionary<int, CleavageRuleConstants> mCleavageRuleComboboxIndexToType;

        private int mTabPageIndexSaved;

        private FastaValidation.FastaValidationOptions mFastaValidationOptions;

        private ProteinFileParser mParseProteinFile;

        private readonly DigestionSimulator mProteinDigestionSimulator = new();

        private FastaValidation mFastaValidation;

        private void AbortProcessingNow()
        {
            try
            {
                mParseProteinFile?.AbortProcessingNow();

                mProteinDigestionSimulator?.AbortProcessingNow();
            }
            catch
            {
                // Ignore errors here
            }
        }

        private void AddPMThresholdRow(double massThreshold, double netThreshold, out bool existingRowFound)
        {
            AddPMThresholdRow(massThreshold, netThreshold, DEFAULT_SLIC_MASS_STDEV, DEFAULT_SLIC_NET_STDEV, out existingRowFound);
        }

        private void AddPMThresholdRow(double massThreshold, double netThreshold, double slicMassStDev, double slicNETStDev, out bool existingRowFound)
        {
            existingRowFound = false;
            foreach (DataRow myDataRow in mPeakMatchingThresholdsDataset.Tables[PM_THRESHOLDS_DATA_TABLE].Rows)
            {
                if (Math.Abs(Convert.ToDouble(myDataRow[0]) - massThreshold) < 0.000001d && Math.Abs(Convert.ToDouble(myDataRow[1]) - netThreshold) < 0.000001d)
                {
                    existingRowFound = true;
                    break;
                }
            }

            if (!existingRowFound)
            {
                var myDataRow = mPeakMatchingThresholdsDataset.Tables[PM_THRESHOLDS_DATA_TABLE].NewRow();
                myDataRow[0] = massThreshold;
                myDataRow[1] = netThreshold;
                myDataRow[2] = slicMassStDev;
                myDataRow[3] = slicNETStDev;
                mPeakMatchingThresholdsDataset.Tables[PM_THRESHOLDS_DATA_TABLE].Rows.Add(myDataRow);
            }
        }

        private void AppendEnzymeToCleavageRuleCombobox(InSilicoDigest inSilicoDigest, CleavageRuleConstants cleavageRuleId)
        {
            inSilicoDigest.GetCleavageRuleById(cleavageRuleId, out var cleavageRule);

            if (cleavageRule == null)
            {
                return;
            }

            var targetIndex = cboCleavageRuleType.Items.Count;
            cboCleavageRuleType.Items.Add(cleavageRule.Description + " (" + cleavageRule.GetDetailedRuleDescription() + ")");

            mCleavageRuleComboboxIndexToType.Add(targetIndex, cleavageRuleId);
        }

        private void AutoDefineOutputFile()
        {
            try
            {
                if (txtProteinInputFilePath.Text.Length > 0)
                {
                    txtProteinOutputFilePath.Text = AutoDefineOutputFileWork(GetProteinInputFilePath());
                }
            }
            catch
            {
                // Leave the TextBox unchanged
            }
        }

        private string AutoDefineOutputFileWork(string inputFilePath)
        {
            var inputFileName = ProteinFileParser.StripExtension(Path.GetFileName(inputFilePath), ".gz");

            string outputFileName;

            if (chkCreateFastaOutputFile.Enabled && chkCreateFastaOutputFile.Checked)
            {
                if (ProteinFileParser.IsFastaFile(inputFilePath, true))
                {
                    outputFileName = Path.GetFileNameWithoutExtension(inputFileName) + "_new.fasta";
                }
                else
                {
                    outputFileName = Path.ChangeExtension(inputFileName, ".fasta");
                }
            }
            else if (string.Equals(Path.GetExtension(inputFileName), ".txt", StringComparison.OrdinalIgnoreCase))
            {
                outputFileName = Path.GetFileNameWithoutExtension(inputFileName) + "_output.txt";
            }
            else
            {
                outputFileName = Path.ChangeExtension(inputFileName, ".txt");
            }

            if (!string.Equals(inputFilePath, txtProteinInputFilePath.Text))
            {
                txtProteinInputFilePath.Text = inputFilePath;
            }

            return Path.Combine(Path.GetDirectoryName(inputFilePath) ?? ".", outputFileName);
        }

        private void AutoPopulatePMThresholds(PredefinedPMThresholds predefinedThresholds, bool confirmReplaceExistingResults)
        {
            if (ClearPMThresholdsList(confirmReplaceExistingResults))
            {
                cboMassTolType.SelectedIndex = (int)predefinedThresholds.MassTolType;

                foreach (var threshold in predefinedThresholds.Thresholds)
                {
                    AddPMThresholdRow(threshold.MassTolerance, threshold.NETTolerance, out _);
                }
            }
        }

        private void AutoPopulatePMThresholdsByID(PredefinedPMThresholdsConstants predefinedPMThreshold, bool confirmReplaceExistingResults)
        {
            try
            {
                AutoPopulatePMThresholds(mPredefinedPMThresholds[(int)predefinedPMThreshold], confirmReplaceExistingResults);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error calling AutoPopulatePMThresholds in AutoPopulatePMThresholdsByID: " + ex.Message);
            }
        }

        private bool ClearPMThresholdsList(bool confirmReplaceExistingResults)
        {
            // Returns true if the PM_THRESHOLDS_DATA_TABLE is empty or if it was cleared
            // Returns false if the user is queried about clearing and they do not click Yes

            var success = false;
            if (mPeakMatchingThresholdsDataset.Tables[PM_THRESHOLDS_DATA_TABLE].Rows.Count > 0)
            {
                DialogResult result;

                if (confirmReplaceExistingResults)
                {
                    result = MessageBox.Show("Are you sure you want to clear the thresholds?", "Clear Thresholds", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                }
                else
                {
                    result = DialogResult.Yes;
                }

                if (result == DialogResult.Yes)
                {
                    mPeakMatchingThresholdsDataset.Tables[PM_THRESHOLDS_DATA_TABLE].Rows.Clear();
                    success = true;
                }
            }
            else
            {
                success = true;
            }

            return success;
        }

        private void ComputeSequencepI()
        {
            if (txtSequenceForpI.TextLength == 0)
            {
                return;
            }

            var sequence = txtSequenceForpI.Text;
            float lcNET = 0;
            float scxNET = 0;

            if (cboHydrophobicityMode.SelectedIndex >= 0)
            {
                mProteinDigestionSimulator.ProcessingOptions.HydrophobicityMode = (HydrophobicityTypeConstants)cboHydrophobicityMode.SelectedIndex;
            }

            mProteinDigestionSimulator.ProcessingOptions.ReportMaximumpI = chkMaxpIModeEnabled.Checked;
            mProteinDigestionSimulator.ProcessingOptions.SequenceWidthToExamineForMaximumpI = LookupMaxpISequenceLength();

            var pI = mProteinDigestionSimulator.IsoelectricPointCalculator.CalculateSequencepI(sequence);
            var hydrophobicity = mProteinDigestionSimulator.IsoelectricPointCalculator.CalculateSequenceHydrophobicity(sequence);

            // Could compute charge state: pICalculator.CalculateSequenceChargeState(sequence, pI)

            if (NETCalculator != null)
            {
                // Compute the LC-based normalized elution time
                lcNET = NETCalculator.GetElutionTime(sequence);
            }

            if (SCXNETCalculator != null)
            {
                // Compute the SCX-based normalized elution time
                scxNET = SCXNETCalculator.GetElutionTime(sequence);
            }

            var message = "pI = " + pI + Environment.NewLine +
                          "Hydrophobicity = " + hydrophobicity + Environment.NewLine +
                          "Predicted LC NET = " + lcNET.ToString("0.000") + Environment.NewLine +
                          "Predicted SCX NET = " + scxNET.ToString("0.000");
            // "Predicted charge state = " + Environment.NewLine + charge.ToString() + " at pH = " + pI.ToString();

            txtpIStats.Text = message;
        }

        private bool ConfirmFilePaths()
        {
            if (txtProteinInputFilePath.TextLength == 0)
            {
                ShowErrorMessage("Please define an input file path", "Missing Value");
                txtProteinInputFilePath.Focus();
                return false;
            }

            if (txtProteinOutputFilePath.TextLength == 0)
            {
                ShowErrorMessage("Please define an output file path", "Missing Value");
                txtProteinOutputFilePath.Focus();
                return false;
            }

            return true;
        }

        private void DefineDefaultPMThresholds()
        {
            int index;
            int massIndex, netIndex;

            mPredefinedPMThresholds = new PredefinedPMThresholds[5];

            // All of the predefined thresholds have mass tolerances in units of PPM
            for (index = 0; index <= PREDEFINED_PM_THRESHOLDS_COUNT - 1; index++)
            {
                mPredefinedPMThresholds[index] = new PredefinedPMThresholds(PeakMatching.MassToleranceConstants.PPM);
            }

            var netValues = new double[3];
            netValues[0] = 0.01d;
            netValues[1] = 0.05d;
            netValues[2] = 100d;

            var massValues = new double[5];
            massValues[0] = 0.5d;
            massValues[1] = 1d;
            massValues[2] = 5d;
            massValues[3] = 10d;
            massValues[4] = 50d;

            // OneMassOneNET
            DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds[(int)PredefinedPMThresholdsConstants.OneMassOneNET], 5d, 0.05d);

            // OneMassThreeNET
            for (netIndex = 0; netIndex < netValues.Length; netIndex++)
            {
                DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds[(int)PredefinedPMThresholdsConstants.OneMassThreeNET], 5d, netValues[netIndex]);
            }

            // ThreeMassOneNET
            for (massIndex = 0; massIndex <= 2; massIndex++)
            {
                DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds[(int)PredefinedPMThresholdsConstants.ThreeMassOneNET], massValues[massIndex], 0.05d);
            }

            // ThreeMassThreeNET
            for (netIndex = 0; netIndex < netValues.Length; netIndex++)
            {
                for (massIndex = 0; massIndex <= 2; massIndex++)
                {
                    DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds[(int)PredefinedPMThresholdsConstants.ThreeMassThreeNET], massValues[massIndex], netValues[netIndex]);
                }
            }

            // FiveMassThreeNET
            for (netIndex = 0; netIndex < netValues.Length; netIndex++)
            {
                for (massIndex = 0; massIndex < massValues.Length; massIndex++)
                {
                    DefineDefaultPMThresholdAppendItem(mPredefinedPMThresholds[(int)PredefinedPMThresholdsConstants.FiveMassThreeNET], massValues[massIndex], netValues[netIndex]);
                }
            }
        }

        private void DefineDefaultPMThresholdAppendItem(PredefinedPMThresholds pmThreshold, double massTolerance, double netTolerance)
        {
            pmThreshold.Thresholds.Add(new PeakMatchingThresholds(massTolerance, netTolerance));
        }

        private void EnableDisableControls()
        {
            bool enableDelimitedFileOptions;

            var inputFilePath = GetProteinInputFilePath();
            var sourceIsFasta = ProteinFileParser.IsFastaFile(inputFilePath, true);

            if (cboInputFileFormat.SelectedIndex == (int)InputFileFormatConstants.DelimitedText)
            {
                enableDelimitedFileOptions = true;
            }
            else if (cboInputFileFormat.SelectedIndex == (int)InputFileFormatConstants.FastaFile ||
                     txtProteinInputFilePath.TextLength == 0 ||
                     sourceIsFasta)
            {
                // FASTA file (or blank)
                enableDelimitedFileOptions = false;
            }
            else
            {
                enableDelimitedFileOptions = true;
            }

            cboInputFileColumnDelimiter.Enabled = enableDelimitedFileOptions;
            lblInputFileColumnDelimiter.Enabled = enableDelimitedFileOptions;
            chkAssumeInputFileIsDigested.Enabled = enableDelimitedFileOptions;

            txtInputFileColumnDelimiter.Enabled = cboInputFileColumnDelimiter.SelectedIndex == (int)ProteinFileParser.DelimiterCharConstants.Other && enableDelimitedFileOptions;

            var enableDigestionOptions = chkDigestProteins.Checked;
            if (enableDigestionOptions)
            {
                cmdParseInputFile.Text = "&Parse and Digest";
            }
            else
            {
                cmdParseInputFile.Text = "&Parse File";
            }

            cmdValidateFastaFile.Enabled = cboInputFileFormat.SelectedIndex == (int)InputFileFormatConstants.FastaFile || sourceIsFasta;

            chkCreateFastaOutputFile.Enabled = !enableDigestionOptions;

            chkComputeSequenceHashIgnoreILDiff.Enabled = chkComputeSequenceHashValues.Checked;

            fraDigestionOptions.Enabled = enableDigestionOptions;
            chkIncludePrefixAndSuffixResidues.Enabled = enableDigestionOptions;

            txtOutputFileFieldDelimiter.Enabled = cboOutputFileFieldDelimiter.SelectedIndex == (int)ProteinFileParser.DelimiterCharConstants.Other;

            enableDelimitedFileOptions = chkLookForAddnlRefInDescription.Checked;
            txtAddnlRefSepChar.Enabled = enableDelimitedFileOptions;
            txtAddnlRefAccessionSepChar.Enabled = enableDelimitedFileOptions;

            txtUniquenessBinStartMass.Enabled = !chkAutoComputeRangeForBinning.Checked;
            txtUniquenessBinEndMass.Enabled = txtUniquenessBinStartMass.Enabled;

            var allowSqlServerCaching = chkAllowSqlServerCaching.Checked;
            chkUseSqlServerDBToCacheData.Enabled = allowSqlServerCaching;

            txtSqlServerDatabase.Enabled = chkUseSqlServerDBToCacheData.Checked && allowSqlServerCaching;
            txtSqlServerName.Enabled = txtSqlServerDatabase.Enabled;
            chkSqlServerUseIntegratedSecurity.Enabled = txtSqlServerDatabase.Enabled;

            chkSqlServerUseExistingData.Enabled = chkSqlServerUseIntegratedSecurity.Checked && allowSqlServerCaching;

            txtSqlServerUsername.Enabled = chkUseSqlServerDBToCacheData.Checked && !chkSqlServerUseIntegratedSecurity.Checked && allowSqlServerCaching;
            txtSqlServerPassword.Enabled = txtSqlServerUsername.Enabled;

            txtProteinReversalSamplingPercentage.Enabled = cboProteinReversalOptions.SelectedIndex > 0;

            txtProteinScramblingLoopCount.Enabled = cboProteinReversalOptions.SelectedIndex == 2;

            txtMinimumSLiCScore.Enabled = chkUseSLiCScoreForUniqueness.Checked;
            optUseEllipseSearchRegion.Enabled = !chkUseSLiCScoreForUniqueness.Checked;
            optUseRectangleSearchRegion.Enabled = optUseEllipseSearchRegion.Enabled;

            txtMaxpISequenceLength.Enabled = chkMaxpIModeEnabled.Checked;

            txtDigestProteinsMinimumpI.Enabled = enableDigestionOptions && chkComputepIandNET.Checked;
            txtDigestProteinsMaximumpI.Enabled = enableDigestionOptions && chkComputepIandNET.Checked;
        }

        private string FormatPercentComplete(float percentComplete)
        {
            return percentComplete.ToString("0.0") + "% complete";
        }

        private void GenerateUniquenessStats()
        {
            if (!mWorking && ConfirmFilePaths())
            {
                try
                {
                    if (mPeakMatchingThresholdsDataset.Tables[PM_THRESHOLDS_DATA_TABLE].Rows.Count == 0)
                    {
                        ShowErrorMessage("Please define one or more peak matching thresholds before proceeding.");
                        return;
                    }

                    if (chkEnableLogging.Checked)
                    {
                        mProteinDigestionSimulator.LogMessagesToFile = true;

                        var appFolderPath = ProcessFilesOrDirectoriesBase.GetAppDataDirectoryPath("ProteinDigestionSimulator");
                        var logFilePath = Path.Combine(appFolderPath, "ProteinDigestionSimulatorLog.txt");
                        mProteinDigestionSimulator.LogFilePath = logFilePath;
                    }

                    var success = InitializeProteinFileParserGeneralOptions(mProteinDigestionSimulator.ProcessingOptions);
                    if (!success)
                    {
                        return;
                    }

                    var outputFilePath = txtProteinOutputFilePath.Text;

                    if (!Path.IsPathRooted(outputFilePath))
                    {
                        outputFilePath = Path.Combine(GetMyDocsFolderPath(), outputFilePath);
                    }

                    if (Directory.Exists(outputFilePath))
                    {
                        // outputFilePath points to a folder and not a file
                        outputFilePath = Path.Combine(outputFilePath, Path.GetFileNameWithoutExtension(GetProteinInputFilePath()) + PEAK_MATCHING_STATS_FILE_SUFFIX);
                    }
                    else
                    {
                        // Replace _output.txt" in outputFilePath with PEAK_MATCHING_STATS_FILE_SUFFIX
                        var charIndex = outputFilePath.IndexOf(OUTPUT_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase);
                        if (charIndex > 0)
                        {
                            outputFilePath = outputFilePath.Substring(0, charIndex) + PEAK_MATCHING_STATS_FILE_SUFFIX;
                        }
                        else
                        {
                            outputFilePath = Path.Combine(Path.GetDirectoryName(outputFilePath) ?? ".", Path.GetFileNameWithoutExtension(outputFilePath) + PEAK_MATCHING_STATS_FILE_SUFFIX);
                        }
                    }

                    // Check input file size and possibly warn user to enable/disable SQL Server DB Usage
                    if (chkAllowSqlServerCaching.Checked)
                    {
                        if (!ValidateSqlServerCachingOptionsForInputFile(
                            GetProteinInputFilePath(),
                            chkAssumeInputFileIsDigested.Checked,
                            mProteinDigestionSimulator.ProteinFileParser))
                        {
                            return;
                        }
                    }

                    PeakMatching.MassToleranceConstants massToleranceType;
                    if (cboMassTolType.SelectedIndex >= 0)
                    {
                        massToleranceType = (PeakMatching.MassToleranceConstants)cboMassTolType.SelectedIndex;
                    }
                    else
                    {
                        massToleranceType = PeakMatching.MassToleranceConstants.PPM;
                    }

                    var autoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked;

                    const bool clearExistingThresholds = true;
                    foreach (DataRow myDataRow in mPeakMatchingThresholdsDataset.Tables[PM_THRESHOLDS_DATA_TABLE].Rows)
                    {
                        if (autoDefineSLiCScoreThresholds)
                        {
                            mProteinDigestionSimulator.AddSearchThresholdLevel(massToleranceType, Convert.ToDouble(myDataRow[0]), Convert.ToDouble(myDataRow[1]), clearExistingThresholds);
                        }
                        else
                        {
                            mProteinDigestionSimulator.AddSearchThresholdLevel(massToleranceType, Convert.ToDouble(myDataRow[0]), Convert.ToDouble(myDataRow[1]), false, Convert.ToDouble(myDataRow[2]), Convert.ToDouble(myDataRow[3]), true, clearExistingThresholds);
                        }
                    }

                    mProteinDigestionSimulator.ProcessingOptions.DigestionOptions.DigestSequences = !chkAssumeInputFileIsDigested.Checked;
                    mProteinDigestionSimulator.ProcessingOptions.DigestionOptions.CysPeptidesOnly = chkCysPeptidesOnly.Checked;

                    if (cboElementMassMode.SelectedIndex >= 0)
                    {
                        mProteinDigestionSimulator.ProcessingOptions.ElementMassMode = (PeptideSequence.ElementModeConstants)cboElementMassMode.SelectedIndex;
                    }

                    mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.BinningSettings.AutoDetermineMassRange = chkAutoComputeRangeForBinning.Checked;

                    mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.BinningSettings.MassBinSizeDa = ParseTextBoxValueInt(txtUniquenessBinWidth, lblUniquenessBinWidth.Text + " must be an integer value", out var invalidValue);
                    if (invalidValue)
                    {
                        return;
                    }

                    if (!mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.BinningSettings.AutoDetermineMassRange)
                    {
                        var binStartMass = ParseTextBoxValueInt(txtUniquenessBinStartMass, "Uniqueness binning start mass must be an integer value", out invalidValue);
                        if (invalidValue)
                        {
                            return;
                        }

                        var binEndMass = ParseTextBoxValueInt(txtUniquenessBinEndMass, "Uniqueness binning end mass must be an integer value", out invalidValue);
                        if (invalidValue)
                        {
                            return;
                        }

                        if (!mProteinDigestionSimulator.SetPeptideUniquenessMassRangeForBinning(binStartMass, binEndMass))
                        {
                            mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.BinningSettings.AutoDetermineMassRange = true;
                        }
                    }

                    mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.BinningSettings.MinimumSLiCScore = ParseTextBoxValueSng(txtMinimumSLiCScore, lblMinimumSLiCScore.Text + " must be a value", out invalidValue);
                    if (invalidValue)
                    {
                        return;
                    }

                    mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.MaxPeakMatchingResultsPerFeatureToSave = ParseTextBoxValueInt(txtMaxPeakMatchingResultsPerFeatureToSave, lblMaxPeakMatchingResultsPerFeatureToSave.Text + " must be an integer value", out invalidValue);
                    if (invalidValue)
                    {
                        return;
                    }

                    mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.SavePeakMatchingResults = chkExportPeakMatchingResults.Checked;
                    mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.UseSLiCScoreForUniqueness = chkUseSLiCScoreForUniqueness.Checked;
                    mProteinDigestionSimulator.ProcessingOptions.PeakMatchingOptions.UseEllipseSearchRegion = optUseEllipseSearchRegion.Checked;             // Only applicable if mProteinDigestionSimulator.UseSLiCScoreForUniqueness = True

                    Cursor.Current = Cursors.WaitCursor;
                    mWorking = true;
                    cmdGenerateUniquenessStats.Enabled = false;

                    ResetProgress();
                    SwitchToProgressTab();

                    success = mProteinDigestionSimulator.GenerateUniquenessStats(GetProteinInputFilePath(), Path.GetDirectoryName(outputFilePath), Path.GetFileNameWithoutExtension(outputFilePath));

                    Cursor.Current = Cursors.Default;

                    if (success)
                    {
                        MessageBox.Show("Uniqueness stats calculation complete ", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SwitchFromProgressTab();
                    }
                    else
                    {
                        ShowErrorMessage("Unable to Generate Uniqueness Stats: " + mProteinDigestionSimulator.GetErrorMessage());
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Error in frmMain->GenerateUniquenessStats: " + ex.Message);
                }
                finally
                {
                    mWorking = false;
                    cmdGenerateUniquenessStats.Enabled = true;
                }
            }
        }

        private string GetMyDocsFolderPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        private string GetProteinInputFilePath()
        {
            return txtProteinInputFilePath.Text.Trim('"', ' ');
        }

        private CleavageRuleConstants GetSelectedCleavageRule()
        {
            var selectedIndex = cboCleavageRuleType.SelectedIndex;

            if (selectedIndex < 0 || !mCleavageRuleComboboxIndexToType.TryGetValue(selectedIndex, out var selectedCleavageRule))
            {
                return CleavageRuleConstants.ConventionalTrypsin;
            }

            return selectedCleavageRule;
        }

        private string GetSettingsFilePath()
        {
            return ProcessFilesOrDirectoriesBase.GetSettingsFilePathLocal("ProteinDigestionSimulator", XML_SETTINGS_FILE_NAME);
        }

        private void IniFileLoadOptions()
        {
            const string OptionsSection = ProteinFileParser.XML_SECTION_OPTIONS;
            const string FASTAOptions = ProteinFileParser.XML_SECTION_FASTA_OPTIONS;
            const string ProcessingOptions = ProteinFileParser.XML_SECTION_PROCESSING_OPTIONS;
            const string DigestionOptions = ProteinFileParser.XML_SECTION_DIGESTION_OPTIONS;
            const string UniquenessStatsOptions = ProteinFileParser.XML_SECTION_UNIQUENESS_STATS_OPTIONS;
            const string PMOptions = DigestionSimulator.XML_SECTION_PEAK_MATCHING_OPTIONS;

            const int MAX_AUTO_WINDOW_HEIGHT = 775;

            var xmlSettings = new XmlSettingsFileAccessor();

            var columnDelimiters = new[] { '\t', ',' };

            ResetToDefaults(false);
            var settingsFilePath = GetSettingsFilePath();

            try
            {
                // Pass False to .LoadSettings() here to turn off case sensitive matching
                xmlSettings.LoadSettings(settingsFilePath, false);
                ProcessFilesOrDirectoriesBase.CreateSettingsFileIfMissing(settingsFilePath);

                if (!File.Exists(settingsFilePath))
                {
                    ShowErrorMessage("Parameter file not Found: " + settingsFilePath);
                    return;
                }

                try
                {
                    txtProteinInputFilePath.Text = xmlSettings.GetParam(OptionsSection, "InputFilePath", GetProteinInputFilePath());

                    cboInputFileFormat.SelectedIndex = xmlSettings.GetParam(OptionsSection, "InputFileFormatIndex", cboInputFileFormat.SelectedIndex);
                    cboInputFileColumnDelimiter.SelectedIndex = xmlSettings.GetParam(OptionsSection, "InputFileColumnDelimiterIndex", cboInputFileColumnDelimiter.SelectedIndex);
                    txtInputFileColumnDelimiter.Text = xmlSettings.GetParam(OptionsSection, "InputFileColumnDelimiter", txtInputFileColumnDelimiter.Text);

                    cboInputFileColumnOrdering.SelectedIndex = xmlSettings.GetParam(OptionsSection, "InputFileColumnOrdering", cboInputFileColumnOrdering.SelectedIndex);

                    cboOutputFileFieldDelimiter.SelectedIndex = xmlSettings.GetParam(OptionsSection, "OutputFileFieldDelimiterIndex", cboOutputFileFieldDelimiter.SelectedIndex);
                    txtOutputFileFieldDelimiter.Text = xmlSettings.GetParam(OptionsSection, "OutputFileFieldDelimiter", txtOutputFileFieldDelimiter.Text);

                    chkIncludePrefixAndSuffixResidues.Checked = xmlSettings.GetParam(OptionsSection, "IncludePrefixAndSuffixResidues", chkIncludePrefixAndSuffixResidues.Checked);
                    chkEnableLogging.Checked = xmlSettings.GetParam(OptionsSection, "EnableLogging", chkEnableLogging.Checked);

                    mCustomValidationRulesFilePath = xmlSettings.GetParam(OptionsSection, "CustomValidationRulesFilePath", string.Empty);

                    Width = xmlSettings.GetParam(OptionsSection, "WindowWidth", Width);
                    var windowHeight = xmlSettings.GetParam(OptionsSection, "WindowHeight", Height);
                    if (windowHeight > MAX_AUTO_WINDOW_HEIGHT)
                    {
                        windowHeight = MAX_AUTO_WINDOW_HEIGHT;
                    }

                    Height = windowHeight;

                    chkLookForAddnlRefInDescription.Checked = xmlSettings.GetParam(FASTAOptions, "LookForAddnlRefInDescription", chkLookForAddnlRefInDescription.Checked);
                    txtAddnlRefSepChar.Text = xmlSettings.GetParam(FASTAOptions, "AddnlRefSepChar", txtAddnlRefSepChar.Text);
                    txtAddnlRefAccessionSepChar.Text = xmlSettings.GetParam(FASTAOptions, "AddnlRefAccessionSepChar", txtAddnlRefAccessionSepChar.Text);

                    chkExcludeProteinSequence.Checked = xmlSettings.GetParam(ProcessingOptions, "ExcludeProteinSequence", chkExcludeProteinSequence.Checked);
                    chkComputeProteinMass.Checked = xmlSettings.GetParam(ProcessingOptions, "ComputeProteinMass", chkComputeProteinMass.Checked);
                    cboElementMassMode.SelectedIndex = xmlSettings.GetParam(ProcessingOptions, "ElementMassMode", cboElementMassMode.SelectedIndex);

                    // In the GUI, chkComputepI controls computing pI, NET, and SCX
                    // Running from the command line, you can toggle those options separately using "ComputepI" and "ComputeSCX"
                    chkComputepIandNET.Checked = xmlSettings.GetParam(ProcessingOptions, "ComputepI", chkComputepIandNET.Checked);
                    chkIncludeXResidues.Checked = xmlSettings.GetParam(ProcessingOptions, "IncludeXResidues", chkIncludeXResidues.Checked);

                    chkComputeSequenceHashValues.Checked = xmlSettings.GetParam(ProcessingOptions, "ComputeSequenceHashValues", chkComputeSequenceHashValues.Checked);
                    chkComputeSequenceHashIgnoreILDiff.Checked = xmlSettings.GetParam(ProcessingOptions, "ComputeSequenceHashIgnoreILDiff", chkComputeSequenceHashIgnoreILDiff.Checked);
                    chkTruncateProteinDescription.Checked = xmlSettings.GetParam(ProcessingOptions, "TruncateProteinDescription", chkTruncateProteinDescription.Checked);
                    chkExcludeProteinDescription.Checked = xmlSettings.GetParam(ProcessingOptions, "ExcludeProteinDescription", chkExcludeProteinDescription.Checked);

                    chkDigestProteins.Checked = xmlSettings.GetParam(ProcessingOptions, "DigestProteins", chkDigestProteins.Checked);
                    cboProteinReversalOptions.SelectedIndex = xmlSettings.GetParam(ProcessingOptions, "ProteinReversalIndex", cboProteinReversalOptions.SelectedIndex);
                    txtProteinScramblingLoopCount.Text = xmlSettings.GetParam(ProcessingOptions, "ProteinScramblingLoopCount", txtProteinScramblingLoopCount.Text);

                    try
                    {
                        cboHydrophobicityMode.SelectedIndex = xmlSettings.GetParam(ProcessingOptions, "HydrophobicityMode", cboHydrophobicityMode.SelectedIndex);
                    }
                    catch
                    {
                        // Ignore errors setting the selected index
                    }

                    chkMaxpIModeEnabled.Checked = xmlSettings.GetParam(ProcessingOptions, "MaxpIModeEnabled", chkMaxpIModeEnabled.Checked);
                    txtMaxpISequenceLength.Text = xmlSettings.GetParam(ProcessingOptions, "MaxpISequenceLength", LookupMaxpISequenceLength()).ToString();

                    var cleavageRuleName = xmlSettings.GetParam(DigestionOptions, "CleavageRuleName", string.Empty);

                    if (!string.IsNullOrWhiteSpace(cleavageRuleName))
                    {
                        SetSelectedCleavageRule(cleavageRuleName);
                    }
                    else
                    {
                        var legacyCleavageRuleIndexSetting = xmlSettings.GetParam(DigestionOptions, "CleavageRuleTypeIndex", -1);
                        if (legacyCleavageRuleIndexSetting >= 0)
                        {
                            try
                            {
                                var cleavageRule = (CleavageRuleConstants)legacyCleavageRuleIndexSetting;
                                SetSelectedCleavageRule(cleavageRule);
                            }
                            catch
                            {
                                // Ignore errors here
                            }
                        }
                    }

                    chkIncludeDuplicateSequences.Checked = xmlSettings.GetParam(DigestionOptions, "IncludeDuplicateSequences", chkIncludeDuplicateSequences.Checked);
                    chkCysPeptidesOnly.Checked = xmlSettings.GetParam(DigestionOptions, "CysPeptidesOnly", chkCysPeptidesOnly.Checked);

                    txtDigestProteinsMinimumMass.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMinimumMass", txtDigestProteinsMinimumMass.Text);
                    txtDigestProteinsMaximumMass.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMaximumMass", txtDigestProteinsMaximumMass.Text);
                    txtDigestProteinsMinimumResidueCount.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMinimumResidueCount", txtDigestProteinsMinimumResidueCount.Text);
                    txtDigestProteinsMaximumMissedCleavages.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMaximumMissedCleavages", txtDigestProteinsMaximumMissedCleavages.Text);

                    txtDigestProteinsMinimumpI.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMinimumpI", txtDigestProteinsMinimumpI.Text);
                    txtDigestProteinsMaximumpI.Text = xmlSettings.GetParam(DigestionOptions, "DigestProteinsMaximumpI", txtDigestProteinsMaximumpI.Text);

                    cboFragmentMassMode.SelectedIndex = xmlSettings.GetParam(DigestionOptions, "FragmentMassModeIndex", cboFragmentMassMode.SelectedIndex);
                    cboCysTreatmentMode.SelectedIndex = xmlSettings.GetParam(DigestionOptions, "CysTreatmentModeIndex", cboCysTreatmentMode.SelectedIndex);

                    // Load Uniqueness Options
                    chkAssumeInputFileIsDigested.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "AssumeInputFileIsDigested", chkAssumeInputFileIsDigested.Checked);

                    txtUniquenessBinWidth.Text = xmlSettings.GetParam(UniquenessStatsOptions, "UniquenessBinWidth", txtUniquenessBinWidth.Text);
                    chkAutoComputeRangeForBinning.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "AutoComputeRangeForBinning", chkAutoComputeRangeForBinning.Checked);
                    txtUniquenessBinStartMass.Text = xmlSettings.GetParam(UniquenessStatsOptions, "UniquenessBinStartMass", txtUniquenessBinStartMass.Text);
                    txtUniquenessBinEndMass.Text = xmlSettings.GetParam(UniquenessStatsOptions, "UniquenessBinEndMass", txtUniquenessBinEndMass.Text);

                    txtMaxPeakMatchingResultsPerFeatureToSave.Text = xmlSettings.GetParam(UniquenessStatsOptions, "MaxPeakMatchingResultsPerFeatureToSave", txtMaxPeakMatchingResultsPerFeatureToSave.Text);
                    chkUseSLiCScoreForUniqueness.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "UseSLiCScoreForUniqueness", chkUseSLiCScoreForUniqueness.Checked);
                    txtMinimumSLiCScore.Text = xmlSettings.GetParam(UniquenessStatsOptions, "MinimumSLiCScore", txtMinimumSLiCScore.Text);
                    var radioButtonChecked = xmlSettings.GetParam(UniquenessStatsOptions, "UseEllipseSearchRegion", true);
                    if (radioButtonChecked)
                    {
                        optUseEllipseSearchRegion.Checked = radioButtonChecked;
                    }
                    else
                    {
                        optUseRectangleSearchRegion.Checked = radioButtonChecked;
                    }

                    // chkAllowSqlServerCaching.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "AllowSqlServerCaching", chkAllowSqlServerCaching.Checked);
                    // chkUseSqlServerDBToCacheData.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "UseSqlServerDBToCacheData", chkUseSqlServerDBToCacheData.Checked);
                    // txtSqlServerName.Text = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerName", txtSqlServerName.Text);
                    // txtSqlServerDatabase.Text = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerDatabase", txtSqlServerDatabase.Text);
                    // chkSqlServerUseIntegratedSecurity.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerUseIntegratedSecurity", chkSqlServerUseIntegratedSecurity.Checked);

                    // chkSqlServerUseExistingData.Checked = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerUseExistingData", chkSqlServerUseExistingData.Checked);

                    // txtSqlServerUsername.Text = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerUsername", txtSqlServerUsername.Text);
                    // txtSqlServerPassword.Text = xmlSettings.GetParam(UniquenessStatsOptions, "SqlServerPassword", txtSqlServerPassword.Text);

                    // Load the peak matching thresholds
                    cboMassTolType.SelectedIndex = xmlSettings.GetParam(PMOptions, "MassToleranceType", cboMassTolType.SelectedIndex);
                    chkAutoDefineSLiCScoreTolerances.Checked = xmlSettings.GetParam(PMOptions, "AutoDefineSLiCScoreThresholds", chkAutoDefineSLiCScoreTolerances.Checked);
                    var autoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked;

                    // See if any peak matching data is present
                    // If it is, clear the table and load it; if not, leave the table unchanged

                    var thresholdData = xmlSettings.GetParam(PMOptions, "ThresholdData", string.Empty, out var valueNotPresent);

                    if (!valueNotPresent && !string.IsNullOrEmpty(thresholdData))
                    {
                        var thresholds = thresholdData.Split(';');

                        if (thresholds.Length > 0)
                        {
                            ClearPMThresholdsList(false);

                            foreach (var threshold in thresholds)
                            {
                                var thresholdDetails = threshold.Split(columnDelimiters);

                                if (thresholdDetails.Length > 2 && !autoDefineSLiCScoreThresholds)
                                {
                                    if (double.TryParse(thresholdDetails[0], out _) && double.TryParse(thresholdDetails[1], out _) &&
                                        double.TryParse(thresholdDetails[2], out _) && double.TryParse(thresholdDetails[3], out _))
                                    {
                                        AddPMThresholdRow(double.Parse(thresholdDetails[0]), double.Parse(thresholdDetails[1]),
                                            double.Parse(thresholdDetails[2]), double.Parse(thresholdDetails[3]), out _);
                                    }
                                }
                                else if (thresholdDetails.Length >= 2)
                                {
                                    if (double.TryParse(thresholdDetails[0], out _) && double.TryParse(thresholdDetails[1], out _))
                                    {
                                        AddPMThresholdRow(double.Parse(thresholdDetails[0]), double.Parse(thresholdDetails[1]), out _);
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    ShowErrorMessage("Invalid parameter in settings file: " + Path.GetFileName(settingsFilePath));
                }
            }
            catch
            {
                ShowErrorMessage("Error loading settings from file: " + settingsFilePath);
            }
        }

        private void IniFileSaveOptions(bool showFilePath, bool saveWindowDimensionsOnly = false)
        {
            const string OptionsSection = ProteinFileParser.XML_SECTION_OPTIONS;
            const string FASTAOptions = ProteinFileParser.XML_SECTION_FASTA_OPTIONS;
            const string ProcessingOptions = ProteinFileParser.XML_SECTION_PROCESSING_OPTIONS;
            const string DigestionOptions = ProteinFileParser.XML_SECTION_DIGESTION_OPTIONS;
            const string UniquenessStatsOptions = ProteinFileParser.XML_SECTION_UNIQUENESS_STATS_OPTIONS;
            const string PMOptions = DigestionSimulator.XML_SECTION_PEAK_MATCHING_OPTIONS;

            var xmlSettings = new XmlSettingsFileAccessor();
            var settingsFilePath = GetSettingsFilePath();
            try
            {
                var settingsFile = new FileInfo(settingsFilePath);
                if (!settingsFile.Exists)
                {
                    saveWindowDimensionsOnly = false;
                }
            }
            catch
            {
                // Ignore errors here
            }

            try
            {
                // Pass True to .LoadSettings() to turn on case sensitive matching
                xmlSettings.LoadSettings(settingsFilePath, true);
                try
                {
                    if (!saveWindowDimensionsOnly)
                    {
                        xmlSettings.SetParam(OptionsSection, "InputFilePath", GetProteinInputFilePath());
                        xmlSettings.SetParam(OptionsSection, "InputFileFormatIndex", cboInputFileFormat.SelectedIndex);
                        xmlSettings.SetParam(OptionsSection, "InputFileColumnDelimiterIndex", cboInputFileColumnDelimiter.SelectedIndex);
                        xmlSettings.SetParam(OptionsSection, "InputFileColumnDelimiter", txtInputFileColumnDelimiter.Text);

                        xmlSettings.SetParam(OptionsSection, "InputFileColumnOrdering", cboInputFileColumnOrdering.SelectedIndex);

                        xmlSettings.SetParam(OptionsSection, "OutputFileFieldDelimiterIndex", cboOutputFileFieldDelimiter.SelectedIndex);
                        xmlSettings.SetParam(OptionsSection, "OutputFileFieldDelimiter", txtOutputFileFieldDelimiter.Text);

                        xmlSettings.SetParam(OptionsSection, "IncludePrefixAndSuffixResidues", chkIncludePrefixAndSuffixResidues.Checked);
                        xmlSettings.SetParam(OptionsSection, "EnableLogging", chkEnableLogging.Checked);

                        xmlSettings.SetParam(OptionsSection, "CustomValidationRulesFilePath", mCustomValidationRulesFilePath);
                    }

                    xmlSettings.SetParam(OptionsSection, "WindowWidth", Width);
                    xmlSettings.SetParam(OptionsSection, "WindowHeight", Height);

                    if (!saveWindowDimensionsOnly)
                    {
                        xmlSettings.SetParam(FASTAOptions, "LookForAddnlRefInDescription", chkLookForAddnlRefInDescription.Checked);
                        xmlSettings.SetParam(FASTAOptions, "AddnlRefSepChar", txtAddnlRefSepChar.Text);
                        xmlSettings.SetParam(FASTAOptions, "AddnlRefAccessionSepChar", txtAddnlRefAccessionSepChar.Text);

                        xmlSettings.SetParam(ProcessingOptions, "ExcludeProteinSequence", chkExcludeProteinSequence.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "ComputeProteinMass", chkComputeProteinMass.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "ComputepI", chkComputepIandNET.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "ComputeNET", chkComputepIandNET.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "ComputeSCX", chkComputepIandNET.Checked);

                        xmlSettings.SetParam(ProcessingOptions, "IncludeXResidues", chkIncludeXResidues.Checked);

                        xmlSettings.SetParam(ProcessingOptions, "ComputeSequenceHashValues", chkComputeSequenceHashValues.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "ComputeSequenceHashIgnoreILDiff", chkComputeSequenceHashIgnoreILDiff.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "TruncateProteinDescription", chkTruncateProteinDescription.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "ExcludeProteinDescription", chkExcludeProteinDescription.Checked);

                        xmlSettings.SetParam(ProcessingOptions, "DigestProteins", chkDigestProteins.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "ProteinReversalIndex", cboProteinReversalOptions.SelectedIndex);
                        xmlSettings.SetParam(ProcessingOptions, "ProteinScramblingLoopCount", txtProteinScramblingLoopCount.Text);
                        xmlSettings.SetParam(ProcessingOptions, "ElementMassMode", cboElementMassMode.SelectedIndex);

                        xmlSettings.SetParam(ProcessingOptions, "HydrophobicityMode", cboHydrophobicityMode.SelectedIndex);
                        xmlSettings.SetParam(ProcessingOptions, "MaxpIModeEnabled", chkMaxpIModeEnabled.Checked);
                        xmlSettings.SetParam(ProcessingOptions, "MaxpISequenceLength", LookupMaxpISequenceLength());

                        xmlSettings.SetParam(DigestionOptions, "CleavageRuleName", GetSelectedCleavageRule().ToString());

                        xmlSettings.SetParam(DigestionOptions, "IncludeDuplicateSequences", chkIncludeDuplicateSequences.Checked);
                        xmlSettings.SetParam(DigestionOptions, "CysPeptidesOnly", chkCysPeptidesOnly.Checked);

                        xmlSettings.SetParam(DigestionOptions, "DigestProteinsMinimumMass", txtDigestProteinsMinimumMass.Text);
                        xmlSettings.SetParam(DigestionOptions, "DigestProteinsMaximumMass", txtDigestProteinsMaximumMass.Text);
                        xmlSettings.SetParam(DigestionOptions, "DigestProteinsMinimumResidueCount", txtDigestProteinsMinimumResidueCount.Text);
                        xmlSettings.SetParam(DigestionOptions, "DigestProteinsMaximumMissedCleavages", txtDigestProteinsMaximumMissedCleavages.Text);

                        xmlSettings.SetParam(DigestionOptions, "DigestProteinsMinimumpI", txtDigestProteinsMinimumpI.Text);
                        xmlSettings.SetParam(DigestionOptions, "DigestProteinsMaximumpI", txtDigestProteinsMaximumpI.Text);

                        xmlSettings.SetParam(DigestionOptions, "FragmentMassModeIndex", cboFragmentMassMode.SelectedIndex);
                        xmlSettings.SetParam(DigestionOptions, "CysTreatmentModeIndex", cboCysTreatmentMode.SelectedIndex);

                        // Load Uniqueness Options
                        xmlSettings.SetParam(UniquenessStatsOptions, "AssumeInputFileIsDigested", chkAssumeInputFileIsDigested.Checked);

                        xmlSettings.SetParam(UniquenessStatsOptions, "UniquenessBinWidth", txtUniquenessBinWidth.Text);
                        xmlSettings.SetParam(UniquenessStatsOptions, "AutoComputeRangeForBinning", chkAutoComputeRangeForBinning.Checked);
                        xmlSettings.SetParam(UniquenessStatsOptions, "UniquenessBinStartMass", txtUniquenessBinStartMass.Text);
                        xmlSettings.SetParam(UniquenessStatsOptions, "UniquenessBinEndMass", txtUniquenessBinEndMass.Text);

                        xmlSettings.SetParam(UniquenessStatsOptions, "MaxPeakMatchingResultsPerFeatureToSave", txtMaxPeakMatchingResultsPerFeatureToSave.Text);
                        xmlSettings.SetParam(UniquenessStatsOptions, "UseSLiCScoreForUniqueness", chkUseSLiCScoreForUniqueness.Checked);
                        xmlSettings.SetParam(UniquenessStatsOptions, "MinimumSLiCScore", txtMinimumSLiCScore.Text);
                        xmlSettings.SetParam(UniquenessStatsOptions, "UseEllipseSearchRegion", optUseEllipseSearchRegion.Checked);

                        // xmlSettings.SetParam(UniquenessStatsOptions, "AllowSqlServerCaching", chkAllowSqlServerCaching.Checked);
                        // xmlSettings.SetParam(UniquenessStatsOptions, "UseSqlServerDBToCacheData", chkUseSqlServerDBToCacheData.Checked);
                        // xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerName", txtSqlServerName.Text);
                        // xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerDatabase", txtSqlServerDatabase.Text);
                        // xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerUseIntegratedSecurity", chkSqlServerUseIntegratedSecurity.Checked);
                        // xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerUseExistingData", chkSqlServerUseExistingData.Checked);
                        // xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerUsername", txtSqlServerUsername.Text);
                        // xmlSettings.SetParam(UniquenessStatsOptions, "SqlServerPassword", txtSqlServerPassword.Text);

                        // Save the peak matching thresholds
                        xmlSettings.SetParam(PMOptions, "MassToleranceType", cboMassTolType.SelectedIndex.ToString());

                        var autoDefineSLiCScoreThresholds = chkAutoDefineSLiCScoreTolerances.Checked;
                        xmlSettings.SetParam(PMOptions, "AutoDefineSLiCScoreThresholds", autoDefineSLiCScoreThresholds.ToString());

                        var thresholdData = string.Empty;
                        foreach (DataRow myDataRow in mPeakMatchingThresholdsDataset.Tables[PM_THRESHOLDS_DATA_TABLE].Rows)
                        {
                            if (thresholdData.Length > 0)
                            {
                                thresholdData += "; ";
                            }

                            if (autoDefineSLiCScoreThresholds)
                            {
                                thresholdData += myDataRow[0] + "," + myDataRow[1];
                            }
                            else
                            {
                                thresholdData += myDataRow[0] + "," + myDataRow[1] + "," + myDataRow[2] + "," + myDataRow[3];
                            }
                        }

                        xmlSettings.SetParam(PMOptions, "ThresholdData", thresholdData);
                    }
                }
                catch
                {
                    ShowErrorMessage("Error storing parameter in settings file: " + Path.GetFileName(settingsFilePath));
                }

                xmlSettings.SaveSettings();

                if (showFilePath)
                {
                    MessageBox.Show("Saved settings to file " + settingsFilePath, "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                ShowErrorMessage("Error saving settings to file: " + settingsFilePath);
            }
        }

        private void InitializeControls()
        {
            DefineDefaultPMThresholds();

            Text = "Protein Digestion Simulator";
            lblUniquenessCalculationsNote.Text = "The Protein Digestion Simulator uses an elution time prediction algorithm " +
                                                 "developed by Lars Kangas and Kostas Petritis. See Help->About Elution Time Prediction for more info. " +
                                                 "Note that you can provide custom time values for peptides by separately " +
                                                 "generating a tab or comma delimited text file with information corresponding " +
                                                 "to one of the options in the 'Column Order' list on the 'File Format' option tab, " +
                                                 "then checking 'Assume Input file is Already Digested' on this tab.";

            PopulateComboBoxes();
            InitializePeakMatchingDataGrid();

            IniFileLoadOptions();
            SetToolTips();

            ShowSplashScreen();

            EnableDisableControls();

            ResetProgress();
        }

        private void InitializePeakMatchingDataGrid()
        {
            // Make the Peak Matching Thresholds DATA_TABLE
            var pmThresholds = new DataTable(PM_THRESHOLDS_DATA_TABLE);

            // Add the columns to the data table
            DBUtils.AppendColumnDoubleToTable(pmThresholds, COL_NAME_MASS_TOLERANCE);
            DBUtils.AppendColumnDoubleToTable(pmThresholds, COL_NAME_NET_TOLERANCE);
            DBUtils.AppendColumnDoubleToTable(pmThresholds, COL_NAME_SLIC_MASS_STDEV, DEFAULT_SLIC_MASS_STDEV);
            DBUtils.AppendColumnDoubleToTable(pmThresholds, COL_NAME_SLIC_NET_STDEV, DEFAULT_SLIC_NET_STDEV);
            DBUtils.AppendColumnIntegerToTable(pmThresholds, COL_NAME_PM_THRESHOLD_ROW_ID, 0, true, true);

            var PrimaryKeyColumn = new[] { pmThresholds.Columns[COL_NAME_PM_THRESHOLD_ROW_ID] };
            pmThresholds.PrimaryKey = PrimaryKeyColumn;

            // Instantiate the dataset
            mPeakMatchingThresholdsDataset = new DataSet(PM_THRESHOLDS_DATA_TABLE);

            // Add the new System.Data.DataTable to the DataSet.
            mPeakMatchingThresholdsDataset.Tables.Add(pmThresholds);

            // Bind the DataSet to the DataGrid
            dgPeakMatchingThresholds.DataSource = mPeakMatchingThresholdsDataset;
            dgPeakMatchingThresholds.DataMember = PM_THRESHOLDS_DATA_TABLE;

            // Update the grid's table style
            UpdateDataGridTableStyle();

            // Populate the table
            AutoPopulatePMThresholdsByID(PredefinedPMThresholdsConstants.OneMassOneNET, false);
        }

        private void UpdateDataGridTableStyle()
        {
            // Define the PM Thresholds table style
            // Setting the MappingName of the table style to PM_THRESHOLDS_DATA_TABLE will cause this style to be used with that table
            var tsPMThresholdsTableStyle = new DataGridTableStyle
            {
                MappingName = PM_THRESHOLDS_DATA_TABLE,
                AllowSorting = true,
                ColumnHeadersVisible = true,
                RowHeadersVisible = true,
                ReadOnly = false
            };

            DataGridUtils.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_MASS_TOLERANCE, "Mass Tolerance", 90);
            DataGridUtils.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_NET_TOLERANCE, "NET Tolerance", 90);

            if (chkAutoDefineSLiCScoreTolerances.Checked)
            {
                dgPeakMatchingThresholds.Width = 250;
            }
            else
            {
                dgPeakMatchingThresholds.Width = 425;
                DataGridUtils.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_MASS_STDEV, "SLiC Mass StDev", 90);
                DataGridUtils.AppendColumnToTableStyle(tsPMThresholdsTableStyle, COL_NAME_SLIC_NET_STDEV, "SLiC NET StDev", 90);
            }

            cmdPastePMThresholdsList.Left = dgPeakMatchingThresholds.Left + dgPeakMatchingThresholds.Width + 15;
            cmdClearPMThresholdsList.Left = cmdPastePMThresholdsList.Left;

            dgPeakMatchingThresholds.TableStyles.Clear();

            if (!dgPeakMatchingThresholds.TableStyles.Contains(tsPMThresholdsTableStyle))
            {
                dgPeakMatchingThresholds.TableStyles.Add(tsPMThresholdsTableStyle);
            }

            dgPeakMatchingThresholds.Refresh();
        }

        private bool InitializeProteinFileParserGeneralOptions(DigestionSimulatorOptions options)
        {
            // Returns true if all values were valid

            switch (cboInputFileFormat.SelectedIndex)
            {
                case (int)InputFileFormatConstants.FastaFile:
                    options.AssumeFastaFile = true;
                    break;
                case (int)InputFileFormatConstants.DelimitedText:
                    options.AssumeDelimitedFile = true;
                    break;
                default:
                    options.AssumeFastaFile = false;
                    break;
            }

            if (cboInputFileColumnOrdering.SelectedIndex >= 0)
            {
                options.DelimitedFileFormatCode = (DelimitedProteinFileReader.ProteinFileFormatCode)cboInputFileColumnOrdering.SelectedIndex;
            }

            options.InputFileDelimiter = LookupColumnDelimiter(cboInputFileColumnDelimiter, txtInputFileColumnDelimiter, '\t');
            options.OutputFileDelimiter = LookupColumnDelimiter(cboOutputFileFieldDelimiter, txtOutputFileFieldDelimiter, '\t');

            options.FastaFileOptions.LookForAddnlRefInDescription = chkLookForAddnlRefInDescription.Checked;

            ValidateTextBox(txtAddnlRefSepChar, mDefaultFastaFileOptions.AddnlRefSepChar.ToString());
            ValidateTextBox(txtAddnlRefAccessionSepChar, mDefaultFastaFileOptions.AddnlRefAccessionSepChar.ToString());

            options.FastaFileOptions.AddnlRefSepChar = txtAddnlRefSepChar.Text[0];
            options.FastaFileOptions.AddnlRefAccessionSepChar = txtAddnlRefAccessionSepChar.Text[0];

            options.ExcludeProteinSequence = chkExcludeProteinSequence.Checked;
            options.ComputeProteinMass = chkComputeProteinMass.Checked;
            options.ComputepI = chkComputepIandNET.Checked;
            options.ComputeNET = chkComputepIandNET.Checked;
            options.ComputeSCXNET = chkComputepIandNET.Checked;

            options.ComputeSequenceHashValues = chkComputeSequenceHashValues.Checked;
            options.ComputeSequenceHashIgnoreILDiff = chkComputeSequenceHashIgnoreILDiff.Checked;
            options.TruncateProteinDescription = chkTruncateProteinDescription.Checked;
            options.ExcludeProteinDescription = chkExcludeProteinDescription.Checked;

            if (cboHydrophobicityMode.SelectedIndex >= 0)
            {
                options.HydrophobicityMode = (HydrophobicityTypeConstants)cboHydrophobicityMode.SelectedIndex;
            }

            options.ReportMaximumpI = chkMaxpIModeEnabled.Checked;
            options.SequenceWidthToExamineForMaximumpI = LookupMaxpISequenceLength();

            options.IncludeXResiduesInMass = chkIncludeXResidues.Checked;

            options.GenerateUniqueIDValuesForPeptides = chkGenerateUniqueIDValues.Checked;

            if (cboCleavageRuleType.SelectedIndex >= 0)
            {
                options.DigestionOptions.CleavageRuleID = GetSelectedCleavageRule();
            }

            options.DigestionOptions.IncludePrefixAndSuffixResidues = chkIncludePrefixAndSuffixResidues.Checked;

            options.DigestionOptions.MinFragmentMass = ParseTextBoxValueInt(txtDigestProteinsMinimumMass, lblDigestProteinsMinimumMass.Text + " must be an integer value", out var invalidValue);
            if (invalidValue)
            {
                return false;
            }

            options.DigestionOptions.MaxFragmentMass = ParseTextBoxValueInt(txtDigestProteinsMaximumMass, lblDigestProteinsMaximumMass.Text + " must be an integer value", out invalidValue);
            if (invalidValue)
            {
                return false;
            }

            options.DigestionOptions.MaxMissedCleavages = ParseTextBoxValueInt(txtDigestProteinsMaximumMissedCleavages, lblDigestProteinsMaximumMissedCleavages.Text + " must be an integer value", out invalidValue);
            if (invalidValue)
            {
                return false;
            }

            options.DigestionOptions.MinFragmentResidueCount = ParseTextBoxValueInt(txtDigestProteinsMinimumResidueCount, lblDigestProteinsMinimumResidueCount.Text + " must be an integer value", out invalidValue);
            if (invalidValue)
            {
                return false;
            }

            options.DigestionOptions.MinIsoelectricPoint = ParseTextBoxValueSng(txtDigestProteinsMinimumpI, lblDigestProteinsMinimumpI.Text + " must be a decimal value", out invalidValue);
            if (invalidValue)
            {
                return false;
            }

            options.DigestionOptions.MaxIsoelectricPoint = ParseTextBoxValueSng(txtDigestProteinsMaximumpI, lblDigestProteinsMaximumpI.Text + " must be a decimal value", out invalidValue);
            if (invalidValue)
            {
                return false;
            }

            if (cboCysTreatmentMode.SelectedIndex >= 0)
            {
                options.DigestionOptions.CysTreatmentMode = (PeptideSequence.CysTreatmentModeConstants)cboCysTreatmentMode.SelectedIndex;
            }

            if (cboFragmentMassMode.SelectedIndex >= 0)
            {
                options.DigestionOptions.FragmentMassMode = (FragmentMassConstants)cboFragmentMassMode.SelectedIndex;
            }

            options.DigestionOptions.RemoveDuplicateSequences = !chkIncludeDuplicateSequences.Checked;

            options.DigestionOptions.CysPeptidesOnly = chkCysPeptidesOnly.Checked;

            return true;
        }

        private char LookupColumnDelimiter(ListControl delimiterCombobox, Control delimiterTextBox, char defaultDelimiter)
        {
            try
            {
                return ProteinFileParser.LookupColumnDelimiterChar(delimiterCombobox.SelectedIndex, delimiterTextBox.Text, defaultDelimiter);
            }
            catch
            {
                return '\t';
            }
        }

        private int LookupMaxpISequenceLength()
        {
            int length;

            try
            {
                length = TextBoxUtils.ParseTextBoxValueInt(txtMaxpISequenceLength, string.Empty, out var invalidValue, 10);
                if (invalidValue)
                {
                    txtMaxpISequenceLength.Text = length.ToString();
                }
            }
            catch
            {
                length = 10;
            }

            if (length < 1)
            {
                length = 1;
            }

            return length;
        }

        private void SetToolTips()
        {
            var toolTipControl = new ToolTip();

            toolTipControl.SetToolTip(cmdParseInputFile, "Parse proteins in input file to create output file(s).");
            toolTipControl.SetToolTip(cboInputFileColumnDelimiter, "Character separating columns in a delimited text input file.");
            toolTipControl.SetToolTip(txtInputFileColumnDelimiter, "Custom character separating columns in a delimited text input file.");
            toolTipControl.SetToolTip(txtOutputFileFieldDelimiter, "Character separating the fields in the output file.");

            toolTipControl.SetToolTip(txtAddnlRefSepChar, "Character separating additional protein accession entries in a protein's description in a FASTA file.");
            toolTipControl.SetToolTip(txtAddnlRefAccessionSepChar, "Character separating source name and accession number for additional protein accession entries in a FASTA file.");

            toolTipControl.SetToolTip(chkGenerateUniqueIDValues, "Set this to false to use less memory when digesting huge protein input files.");
            toolTipControl.SetToolTip(txtProteinReversalSamplingPercentage, "Set this to a value less than 100 to only include a portion of the residues from the input file in the output file.");
            toolTipControl.SetToolTip(txtProteinScramblingLoopCount, "Set this to a value greater than 1 to create multiple scrambled versions of the input file.");

            toolTipControl.SetToolTip(optUseEllipseSearchRegion, "This setting only takes effect if 'Use SLiC Score when gauging uniqueness' is false.");
            toolTipControl.SetToolTip(optUseRectangleSearchRegion, "This setting only takes effect if 'Use SLiC Score when gauging uniqueness' is false.");

            toolTipControl.SetToolTip(lblPeptideUniquenessMassMode, "Current mass mode; to change go to the 'Parse and Digest File Options' tab");

            toolTipControl.SetToolTip(chkExcludeProteinSequence, "Enabling this setting will prevent protein sequences from being written to the output file; useful when processing extremely large files.");
            toolTipControl.SetToolTip(chkTruncateProteinDescription, "Truncate description (if over 7995 chars)");

            toolTipControl.SetToolTip(chkEnableLogging, "Logs status and error messages to file ProteinDigestionSimulatorLog*.txt in the program directory.");
        }

        private void ParseProteinInputFile()
        {
            if (!mWorking && ConfirmFilePaths())
            {
                try
                {
                    if (mParseProteinFile == null)
                    {
                        mParseProteinFile = new ProteinFileParser(mProteinDigestionSimulator.ProcessingOptions);
                        mParseProteinFile.ErrorEvent += ParseProteinFile_ErrorEvent;
                        mParseProteinFile.ProgressUpdate += ParseProteinFile_ProgressChanged;
                        mParseProteinFile.ProgressComplete += ParseProteinFile_ProgressComplete;
                        mParseProteinFile.ProgressReset += ParseProteinFile_ProgressReset;
                        mParseProteinFile.SubtaskProgressChanged += ParseProteinFile_SubtaskProgressChanged;
                    }

                    // Update options based on GUI control values
                    var success = InitializeProteinFileParserGeneralOptions(mProteinDigestionSimulator.ProcessingOptions);
                    if (!success)
                    {
                        return;
                    }

                    mProteinDigestionSimulator.ProcessingOptions.CreateProteinOutputFile = true;

                    if (cboProteinReversalOptions.SelectedIndex >= 0)
                    {
                        mProteinDigestionSimulator.ProcessingOptions.ProteinScramblingMode = (ProteinFileParser.ProteinScramblingModeConstants)cboProteinReversalOptions.SelectedIndex;
                    }

                    mProteinDigestionSimulator.ProcessingOptions.ProteinScramblingSamplingPercentage = TextBoxUtils.ParseTextBoxValueInt(txtProteinReversalSamplingPercentage, string.Empty, out _, 100);
                    mProteinDigestionSimulator.ProcessingOptions.ProteinScramblingLoopCount = TextBoxUtils.ParseTextBoxValueInt(txtProteinScramblingLoopCount, string.Empty, out _, 1);
                    mProteinDigestionSimulator.ProcessingOptions.CreateDigestedProteinOutputFile = chkDigestProteins.Checked;
                    mProteinDigestionSimulator.ProcessingOptions.CreateFastaOutputFile = chkCreateFastaOutputFile.Checked;

                    if (cboElementMassMode.SelectedIndex >= 0)
                    {
                        mProteinDigestionSimulator.ProcessingOptions.ElementMassMode = (PeptideSequence.ElementModeConstants)cboElementMassMode.SelectedIndex;
                    }

                    Cursor.Current = Cursors.WaitCursor;
                    mWorking = true;
                    cmdParseInputFile.Enabled = false;

                    ResetProgress();
                    SwitchToProgressTab();

                    var outputFolderPath = string.Empty;
                    var outputFileNameBaseOverride = string.Empty;

                    if (txtProteinOutputFilePath.TextLength > 0)
                    {
                        outputFolderPath = Path.GetDirectoryName(txtProteinOutputFilePath.Text);
                        outputFileNameBaseOverride = Path.GetFileNameWithoutExtension(txtProteinOutputFilePath.Text);
                    }

                    success = mParseProteinFile.ParseProteinFile(GetProteinInputFilePath(), outputFolderPath, outputFileNameBaseOverride);

                    Cursor.Current = Cursors.Default;

                    if (success)
                    {
                        MessageBox.Show(mParseProteinFile.ProcessingSummary, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SwitchFromProgressTab();
                    }
                    else
                    {
                        ShowErrorMessage("Error parsing protein file: " + mParseProteinFile.GetErrorMessage());
                    }
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Error in frmMain->ParseProteinInputFile: " + ex.Message);
                }
                finally
                {
                    mWorking = false;
                    cmdParseInputFile.Enabled = true;
                    if (mParseProteinFile != null)
                    {
                        mParseProteinFile.CloseLogFileNow();
                        mParseProteinFile.ErrorEvent -= ParseProteinFile_ErrorEvent;
                        mParseProteinFile.ProgressUpdate -= ParseProteinFile_ProgressChanged;
                        mParseProteinFile.ProgressComplete -= ParseProteinFile_ProgressComplete;
                        mParseProteinFile.ProgressReset -= ParseProteinFile_ProgressReset;
                        mParseProteinFile.SubtaskProgressChanged -= ParseProteinFile_SubtaskProgressChanged;
                    }

                    mParseProteinFile = null;
                }
            }
        }

        private int ParseTextBoxValueInt(Control thisTextBox, string messageIfError, out bool invalidValue, int valueIfError = 0)
        {
            invalidValue = false;

            try
            {
                return int.Parse(thisTextBox.Text);
            }
            catch
            {
                ShowErrorMessage(messageIfError);
                invalidValue = true;
                return valueIfError;
            }
        }

        private float ParseTextBoxValueSng(Control thisTextBox, string messageIfError, out bool invalidValue, float valueIfError = 0f)
        {
            invalidValue = false;

            try
            {
                return float.Parse(thisTextBox.Text);
            }
            catch
            {
                ShowErrorMessage(messageIfError);
                invalidValue = true;
                return valueIfError;
            }
        }

        private void PastePMThresholdsValues(bool clearList)
        {
            var lineDelimiters = new[] { '\r', '\n' };
            var columnDelimiters = new[] { '\t', ',' };

            // Examine the clipboard contents
            var clipboardObject = Clipboard.GetDataObject();

            if (clipboardObject != null)
            {
                if (clipboardObject.GetDataPresent(DataFormats.StringFormat, true))
                {
                    var clipboardData = clipboardObject.GetData(DataFormats.StringFormat, true).ToString();

                    // Split clipboardData on carriage return or line feed characters
                    // Lines that end in CrLf will give two separate lines; one with the text, and one blank; that's OK
                    var dataLines = clipboardData.Split(lineDelimiters, 1000);

                    if (dataLines.Length > 0)
                    {
                        if (clearList)
                        {
                            if (!ClearPMThresholdsList(true))
                            {
                                return;
                            }
                        }

                        var rowsAlreadyPresent = 0;
                        var rowsSkipped = 0;

                        foreach (var line in dataLines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var dataColumns = line.Split(columnDelimiters, 5);
                                if (dataColumns.Length >= 2)
                                {
                                    try
                                    {
                                        var massThreshold = double.Parse(dataColumns[0]);
                                        var netThreshold = double.Parse(dataColumns[1]);

                                        if (massThreshold < 0 || netThreshold < 0)
                                        {
                                            continue;
                                        }

                                        var useSLiC = !chkAutoDefineSLiCScoreTolerances.Checked && dataColumns.Length >= 4;

                                        double slicMassStDev = 0;
                                        double slicNETStDev = 0;

                                        if (useSLiC)
                                        {
                                            try
                                            {
                                                slicMassStDev = double.Parse(dataColumns[2]);
                                                slicNETStDev = double.Parse(dataColumns[3]);
                                            }
                                            catch
                                            {
                                                useSLiC = false;
                                            }
                                        }

                                        bool existingRowFound;
                                        if (useSLiC)
                                        {
                                            AddPMThresholdRow(massThreshold, netThreshold, slicMassStDev, slicNETStDev, out existingRowFound);
                                        }
                                        else
                                        {
                                            AddPMThresholdRow(massThreshold, netThreshold, out existingRowFound);
                                        }

                                        if (existingRowFound)
                                        {
                                            rowsAlreadyPresent++;
                                        }
                                    }
                                    catch
                                    {
                                        // Skip this row
                                        rowsSkipped++;
                                    }
                                }
                                else
                                {
                                    rowsSkipped++;
                                }
                            }
                        }

                        if (rowsAlreadyPresent > 0)
                        {
                            string errorMessage;
                            if (rowsAlreadyPresent == 1)
                            {
                                errorMessage = "1 row of thresholds was";
                            }
                            else
                            {
                                errorMessage = rowsAlreadyPresent + " rows of thresholds were";
                            }

                            ShowErrorMessage(errorMessage + " already present in the table; duplicate rows are not allowed.", "Warning");
                        }

                        if (rowsSkipped > 0)
                        {
                            string errorMessage;
                            if (rowsSkipped == 1)
                            {
                                errorMessage = "1 row was skipped because it";
                            }
                            else
                            {
                                errorMessage = rowsSkipped + " rows were skipped because they";
                            }

                            ShowErrorMessage(errorMessage + " didn't contain two columns of numeric data.", "Warning");
                        }
                    }
                }
            }
        }

        private void PopulateComboBoxes()
        {
            const string NET_UNITS = "NET";

            try
            {
                cboInputFileFormat.Items.Clear();
                cboInputFileFormat.Items.Insert((int)InputFileFormatConstants.AutoDetermine, "Auto-determine");
                cboInputFileFormat.Items.Insert((int)InputFileFormatConstants.FastaFile, "FASTA file");
                cboInputFileFormat.Items.Insert((int)InputFileFormatConstants.DelimitedText, "Delimited text");
                cboInputFileFormat.SelectedIndex = (int)InputFileFormatConstants.AutoDetermine;

                cboInputFileColumnDelimiter.Items.Clear();
                cboInputFileColumnDelimiter.Items.Insert((int)ProteinFileParser.DelimiterCharConstants.Space, "Space");
                cboInputFileColumnDelimiter.Items.Insert((int)ProteinFileParser.DelimiterCharConstants.Tab, "Tab");
                cboInputFileColumnDelimiter.Items.Insert((int)ProteinFileParser.DelimiterCharConstants.Comma, "Comma");
                cboInputFileColumnDelimiter.Items.Insert((int)ProteinFileParser.DelimiterCharConstants.Other, "Other");
                cboInputFileColumnDelimiter.SelectedIndex = (int)ProteinFileParser.DelimiterCharConstants.Tab;

                cboOutputFileFieldDelimiter.Items.Clear();
                for (var index = 0; index < cboInputFileColumnDelimiter.Items.Count; index++)
                {
                    cboOutputFileFieldDelimiter.Items.Insert(index, cboInputFileColumnDelimiter.Items[index]);
                }

                cboOutputFileFieldDelimiter.SelectedIndex = (int)ProteinFileParser.DelimiterCharConstants.Space;

                cboInputFileColumnOrdering.Items.Clear();
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.SequenceOnly, "Sequence Only");
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Sequence, "ProteinName and Sequence");
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence, "ProteinName, Description, Sequence");
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence, "UniqueID and Seq");
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID, "ProteinName, Seq, UniqueID");
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET, "ProteinName, Seq, UniqueID, Mass, NET");
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore, "ProteinName, Seq, UniqueID, Mass, NET, NETStDev, DiscriminantScore");
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET, "UniqueID, Seq, Mass, NET");
                cboInputFileColumnOrdering.Items.Insert((int)DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Hash_Sequence, "ProteinName, Description, Hash, Sequence");
                cboInputFileColumnOrdering.SelectedIndex = (int)DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence;

                cboElementMassMode.Items.Clear();
                cboElementMassMode.Items.Insert((int)PeptideSequence.ElementModeConstants.AverageMass, "Average");
                cboElementMassMode.Items.Insert((int)PeptideSequence.ElementModeConstants.IsotopicMass, "Monoisotopic");
                cboElementMassMode.SelectedIndex = (int)PeptideSequence.ElementModeConstants.IsotopicMass;

                cboProteinReversalOptions.Items.Clear();
                cboProteinReversalOptions.Items.Insert((int)ProteinFileParser.ProteinScramblingModeConstants.None, "Normal output");
                cboProteinReversalOptions.Items.Insert((int)ProteinFileParser.ProteinScramblingModeConstants.Reversed, "Reverse ORF sequences");
                cboProteinReversalOptions.Items.Insert((int)ProteinFileParser.ProteinScramblingModeConstants.Randomized, "Randomized ORF sequences");
                cboProteinReversalOptions.SelectedIndex = (int)ProteinFileParser.ProteinScramblingModeConstants.None;

                var inSilicoDigest = mProteinDigestionSimulator.InSilicoDigester;

                cboCleavageRuleType.Items.Clear();
                mCleavageRuleComboboxIndexToType.Clear();

                // Add Trypsin rules first
                AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, CleavageRuleConstants.NoRule);
                AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, CleavageRuleConstants.ConventionalTrypsin);
                AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, CleavageRuleConstants.TrypsinWithoutProlineException);
                AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, CleavageRuleConstants.KROneEnd);
                AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, CleavageRuleConstants.TrypsinPlusFVLEY);
                AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, CleavageRuleConstants.TrypsinPlusLysC);
                AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, CleavageRuleConstants.TrypsinPlusThermolysin);

                // Add the remaining enzymes based on the description, but skip CleavageRuleConstants.EricPartialTrypsin

                // Keys in this dictionary are cleavage rule enums, values are the rule description
                var additionalRulesToAppend = new Dictionary<CleavageRuleConstants, string>();

                foreach (var cleavageRule in inSilicoDigest.CleavageRules)
                {
                    var cleavageRuleId = cleavageRule.Key;
                    if (mCleavageRuleComboboxIndexToType.ContainsValue(cleavageRuleId))
                    {
                        continue;
                    }

                    additionalRulesToAppend.Add(cleavageRuleId, cleavageRule.Value.Description);
                }

                foreach (var ruleToAdd in additionalRulesToAppend.OrderBy(x => x.Value).Select(x => x.Key))
                {
                    if (ruleToAdd == CleavageRuleConstants.EricPartialTrypsin)
                    {
                        continue;
                    }

                    AppendEnzymeToCleavageRuleCombobox(inSilicoDigest, ruleToAdd);
                }

                // Select the fully tryptic enzyme rule
                SetSelectedCleavageRule(CleavageRuleConstants.ConventionalTrypsin);

                cboMassTolType.Items.Clear();
                cboMassTolType.Items.Insert((int)PeakMatching.MassToleranceConstants.PPM, "PPM");
                cboMassTolType.Items.Insert((int)PeakMatching.MassToleranceConstants.Absolute, "Absolute (Da)");
                cboMassTolType.SelectedIndex = (int)PeakMatching.MassToleranceConstants.PPM;

                cboPMPredefinedThresholds.Items.Clear();
                cboPMPredefinedThresholds.Items.Insert((int)PredefinedPMThresholdsConstants.OneMassOneNET, "5 ppm; 0.05 " + NET_UNITS);
                cboPMPredefinedThresholds.Items.Insert((int)PredefinedPMThresholdsConstants.OneMassThreeNET, "5 ppm; 0.01, 0.05, 100 " + NET_UNITS);
                cboPMPredefinedThresholds.Items.Insert((int)PredefinedPMThresholdsConstants.ThreeMassOneNET, "0.5, 1, & 5 ppm; 0.05 " + NET_UNITS);
                cboPMPredefinedThresholds.Items.Insert((int)PredefinedPMThresholdsConstants.ThreeMassThreeNET, "0.5, 1, 5 ppm; 0.01, 0.05, & 100 " + NET_UNITS);
                cboPMPredefinedThresholds.Items.Insert((int)PredefinedPMThresholdsConstants.FiveMassThreeNET, "0.5, 1, 5, 10, & 50 ppm; 0.01, 0.05, & 100 " + NET_UNITS);
                cboPMPredefinedThresholds.SelectedIndex = (int)PredefinedPMThresholdsConstants.OneMassOneNET;

                cboHydrophobicityMode.Items.Clear();
                cboHydrophobicityMode.Items.Insert((int)HydrophobicityTypeConstants.HW, "Hopp and Woods");
                cboHydrophobicityMode.Items.Insert((int)HydrophobicityTypeConstants.KD, "Kyte and Doolittle");
                cboHydrophobicityMode.Items.Insert((int)HydrophobicityTypeConstants.Eisenberg, "Eisenberg");
                cboHydrophobicityMode.Items.Insert((int)HydrophobicityTypeConstants.GES, "Engleman et. al.");
                cboHydrophobicityMode.Items.Insert((int)HydrophobicityTypeConstants.MeekPH7p4, "Meek pH 7.4");
                cboHydrophobicityMode.Items.Insert((int)HydrophobicityTypeConstants.MeekPH2p1, "Meek pH 2.1");
                cboHydrophobicityMode.SelectedIndex = (int)HydrophobicityTypeConstants.HW;

                cboCysTreatmentMode.Items.Clear();
                cboCysTreatmentMode.Items.Insert((int)PeptideSequence.CysTreatmentModeConstants.Untreated, "Untreated");
                cboCysTreatmentMode.Items.Insert((int)PeptideSequence.CysTreatmentModeConstants.Iodoacetamide, "Iodoacetamide (+57.02)");
                cboCysTreatmentMode.Items.Insert((int)PeptideSequence.CysTreatmentModeConstants.IodoaceticAcid, "Iodoacetic Acid (+58.01)");
                cboCysTreatmentMode.SelectedIndex = (int)PeptideSequence.CysTreatmentModeConstants.Untreated;

                cboFragmentMassMode.Items.Clear();
                cboFragmentMassMode.Items.Insert((int)FragmentMassConstants.Monoisotopic, "Monoisotopic");
                cboFragmentMassMode.Items.Insert((int)FragmentMassConstants.MH, "M+H");
                cboFragmentMassMode.SelectedIndex = (int)FragmentMassConstants.Monoisotopic;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error initializing the combo boxes: " + ex.Message);
            }
        }

        private void ResetProgress()
        {
            lblProgressDescription.Text = string.Empty;
            lblProgress.Text = FormatPercentComplete(0f);
            pbarProgress.Value = 0;
            pbarProgress.Visible = true;

            lblSubtaskProgressDescription.Text = string.Empty;
            lblSubtaskProgress.Text = FormatPercentComplete(0f);

            lblErrorMessage.Text = string.Empty;

            Application.DoEvents();
        }

        private void ResetToDefaults(bool confirm)
        {
            if (confirm)
            {
                var response = MessageBox.Show("Are you sure you want to reset all settings to their default values?", "Reset to Defaults", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (response != DialogResult.Yes)
                {
                    return;
                }
            }

            cboInputFileFormat.SelectedIndex = (int)InputFileFormatConstants.AutoDetermine;
            cboInputFileColumnDelimiter.SelectedIndex = (int)ProteinFileParser.DelimiterCharConstants.Tab;
            txtInputFileColumnDelimiter.Text = ";";

            cboOutputFileFieldDelimiter.SelectedIndex = (int)ProteinFileParser.DelimiterCharConstants.Tab;
            txtOutputFileFieldDelimiter.Text = ";";

            chkEnableLogging.Checked = false;

            chkIncludePrefixAndSuffixResidues.Checked = false;

            chkLookForAddnlRefInDescription.Checked = false;
            txtAddnlRefSepChar.Text = mDefaultFastaFileOptions.AddnlRefSepChar.ToString();                      // "|"
            txtAddnlRefAccessionSepChar.Text = mDefaultFastaFileOptions.AddnlRefAccessionSepChar.ToString();    // ":"

            chkExcludeProteinSequence.Checked = false;
            chkComputeProteinMass.Checked = false;
            cboElementMassMode.SelectedIndex = (int)PeptideSequence.ElementModeConstants.IsotopicMass;

            chkComputepIandNET.Checked = false;
            chkIncludeXResidues.Checked = false;

            chkComputeSequenceHashValues.Checked = true;
            chkComputeSequenceHashIgnoreILDiff.Checked = true;
            chkTruncateProteinDescription.Checked = true;
            chkExcludeProteinDescription.Checked = false;

            cboHydrophobicityMode.SelectedIndex = (int)HydrophobicityTypeConstants.HW;
            chkMaxpIModeEnabled.Checked = false;
            txtMaxpISequenceLength.Text = "10";

            chkDigestProteins.Checked = false;
            cboProteinReversalOptions.SelectedIndex = (int)ProteinFileParser.ProteinScramblingModeConstants.None;
            txtProteinReversalSamplingPercentage.Text = "100";
            txtProteinScramblingLoopCount.Text = "1";

            SetSelectedCleavageRule(CleavageRuleConstants.ConventionalTrypsin);

            chkIncludeDuplicateSequences.Checked = false;
            chkCysPeptidesOnly.Checked = false;
            chkGenerateUniqueIDValues.Checked = true;

            txtDigestProteinsMinimumMass.Text = "400";
            txtDigestProteinsMaximumMass.Text = "6000";
            txtDigestProteinsMinimumResidueCount.Text = "0";
            txtDigestProteinsMaximumMissedCleavages.Text = "3";

            txtDigestProteinsMinimumpI.Text = "0";
            txtDigestProteinsMaximumpI.Text = "14";

            // Load Uniqueness Options
            chkAssumeInputFileIsDigested.Checked = true;

            txtUniquenessBinWidth.Text = "25";
            chkAutoComputeRangeForBinning.Checked = true;
            txtUniquenessBinStartMass.Text = "400";
            txtUniquenessBinEndMass.Text = "4000";

            txtMaxPeakMatchingResultsPerFeatureToSave.Text = "3";
            chkUseSLiCScoreForUniqueness.Checked = true;
            txtMinimumSLiCScore.Text = "0.95";
            optUseEllipseSearchRegion.Checked = true;

            chkUseSqlServerDBToCacheData.Checked = false;
            txtSqlServerName.Text = SystemInformation.ComputerName;
            txtSqlServerDatabase.Text = "TempDB";
            chkSqlServerUseIntegratedSecurity.Checked = true;
            chkSqlServerUseExistingData.Checked = false;

            txtSqlServerUsername.Text = "user";
            txtSqlServerPassword.Text = string.Empty;

            Width = 960;
            Height = 780;

            mCustomValidationRulesFilePath = string.Empty;

            var settingsFilePath = GetSettingsFilePath();
            ProcessFilesOrDirectoriesBase.CreateSettingsFileIfMissing(settingsFilePath);
        }

        private void SelectInputFile()
        {
            var currentExtension = string.Empty;
            if (txtProteinInputFilePath.TextLength > 0)
            {
                try
                {
                    currentExtension = Path.GetExtension(GetProteinInputFilePath());
                }
                catch
                {
                    // Ignore errors here
                }
            }

            var openFile = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                DefaultExt = ".txt",
                DereferenceLinks = true,
                Multiselect = false,
                ValidateNames = true,
                Filter = "FASTA files (*.fasta)|*.fasta|FASTA files (*.fasta.gz)|*.fasta.gz|Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (cboInputFileFormat.SelectedIndex == (int)InputFileFormatConstants.DelimitedText)
            {
                openFile.FilterIndex = 3;
            }
            else if (string.Equals(currentExtension, ".txt", StringComparison.OrdinalIgnoreCase))
            {
                openFile.FilterIndex = 3;
            }
            else if (string.Equals(currentExtension, ".gz", StringComparison.OrdinalIgnoreCase))
            {
                openFile.FilterIndex = 2;
            }
            else
            {
                openFile.FilterIndex = 1;
            }

            if (GetProteinInputFilePath().Length > 0)
            {
                try
                {
                    openFile.InitialDirectory = Directory.GetParent(GetProteinInputFilePath())?.FullName;
                }
                catch
                {
                    openFile.InitialDirectory = GetMyDocsFolderPath();
                }
            }
            else
            {
                openFile.InitialDirectory = GetMyDocsFolderPath();
            }

            openFile.Title = "Select input file";

            openFile.ShowDialog();
            if (openFile.FileName.Length > 0)
            {
                txtProteinInputFilePath.Text = openFile.FileName;
            }
        }

        private void SelectOutputFile()
        {
            var saveFile = new SaveFileDialog
            {
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                DefaultExt = ".txt",
                DereferenceLinks = true,
                OverwritePrompt = true,
                ValidateNames = true,
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (txtProteinOutputFilePath.Text.Length > 0)
            {
                try
                {
                    saveFile.InitialDirectory = Directory.GetParent(txtProteinOutputFilePath.Text)?.ToString();
                }
                catch
                {
                    saveFile.InitialDirectory = GetMyDocsFolderPath();
                }
            }
            else
            {
                saveFile.InitialDirectory = GetMyDocsFolderPath();
            }

            saveFile.Title = "Select/Create output file";

            saveFile.ShowDialog();
            if (saveFile.FileName.Length > 0)
            {
                txtProteinOutputFilePath.Text = saveFile.FileName;
            }
        }

        private void SetSelectedCleavageRule(string cleavageRuleName)
        {
            if (Enum.TryParse(cleavageRuleName, true, out CleavageRuleConstants cleavageRule))
            {
                SetSelectedCleavageRule(cleavageRule);
            }
        }

        private void SetSelectedCleavageRule(CleavageRuleConstants cleavageRule)
        {
            var query = mCleavageRuleComboboxIndexToType.Where(x => x.Value == cleavageRule).Select(x => x.Key);

            foreach (var item in query.Take(1))
            {
                cboCleavageRuleType.SelectedIndex = item;
            }
        }

        private void ShowAboutBox()
        {
            var message = new StringBuilder();

            message.AppendLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)");
            message.AppendLine("Copyright 2021 Battelle Memorial Institute");
            message.AppendLine();
            message.AppendFormat("This is version {0} ({1})", Application.ProductVersion, Program.PROGRAM_DATE);
            message.AppendLine();
            message.AppendLine();
            message.AppendLine("E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov");
            message.AppendLine("Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics");
            message.AppendLine();
            message.AppendLine(Disclaimer.GetKangasPetritisDisclaimerText());
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

        private void ShowElutionTimeInfo()
        {
            MessageBox.Show(NETCalculator.ProgramDescription, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowErrorMessage(string message, string caption = "Error")
        {
            MessageBoxIcon messageIcon;

            if (caption.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                messageIcon = MessageBoxIcon.Exclamation;
            }
            else
            {
                messageIcon = MessageBoxIcon.Information;
            }

            MessageBox.Show(message, caption, MessageBoxButtons.OK, messageIcon);
        }

        private void ShowSplashScreen()
        {
            // See if the user has been shown the splash screen sometime in the last 6 months (SPLASH_INTERVAL_DAYS)
            // Keep track of the last splash screen display date using the program user settings (stored in the user's AppData\Local\PNNL\... folder)

            const int SPLASH_INTERVAL_DAYS = 182;

            var currentDateTime = DateTime.Now;

            var lastSplashDate = Properties.Settings.Default.LastSplashDate;

            if (currentDateTime.Subtract(lastSplashDate).TotalDays >= SPLASH_INTERVAL_DAYS)
            {
                lastSplashDate = currentDateTime;
                Properties.Settings.Default.LastSplashDate = lastSplashDate;
                Properties.Settings.Default.Save();

                var splashForm = new Disclaimer();
                splashForm.ShowDialog();
            }
        }

        private void SwitchToProgressTab()
        {
            mTabPageIndexSaved = tbsOptions.SelectedIndex;

            tbsOptions.SelectedIndex = PROGRESS_TAB_INDEX;
            Application.DoEvents();
        }

        private void SwitchFromProgressTab()
        {
            // Wait 500 msec, then switch from the progress tab back to the tab that was visible before we started, but only if the current tab is the progress tab

            if (tbsOptions.SelectedIndex == PROGRESS_TAB_INDEX)
            {
                tbsOptions.SelectedIndex = mTabPageIndexSaved;
                Application.DoEvents();
            }
        }

        private void UpdatePeptideUniquenessMassMode()
        {
            if (cboElementMassMode.SelectedIndex == (int)PeptideSequence.ElementModeConstants.AverageMass)
            {
                lblPeptideUniquenessMassMode.Text = "Using average masses";
            }
            else
            {
                lblPeptideUniquenessMassMode.Text = "Using monoisotopic masses";
            }
        }

        private void ValidateFastaFile(string fastaFilePath)
        {
            try
            {
                // Make sure an existing file has been chosen
                if (string.IsNullOrEmpty(fastaFilePath))
                {
                    return;
                }

                if (!File.Exists(fastaFilePath))
                {
                    ShowErrorMessage("File not found: " + fastaFilePath);
                }
                else
                {
                    if (mFastaValidation == null)
                    {
                        mFastaValidation = new FastaValidation(fastaFilePath);
                        mFastaValidation.FastaValidationStarted += FastaValidation_FastaValidationStarted;
                    }
                    else
                    {
                        mFastaValidation.SetNewFastaFile(fastaFilePath);
                    }

                    try
                    {
                        if (!string.IsNullOrEmpty(mCustomValidationRulesFilePath))
                        {
                            if (File.Exists(mCustomValidationRulesFilePath))
                            {
                                mFastaValidation.CustomRulesFilePath = mCustomValidationRulesFilePath;
                            }
                            else
                            {
                                mCustomValidationRulesFilePath = string.Empty;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessage("Error trying to validate or set the custom validation rules file path: " + mCustomValidationRulesFilePath + "; " + ex.Message);
                    }

                    if (mFastaValidationOptions?.Initialized == true)
                    {
                        mFastaValidation.SetOptions(mFastaValidationOptions);
                    }

                    mFastaValidation.ShowDialog();

                    // Note that mFastaValidation.GetOptions() will be called when event FastaValidationStarted fires
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error occurred in frmFastaValidation: " + ex.Message);
            }
            finally
            {
                if (mFastaValidation != null)
                {
                    mCustomValidationRulesFilePath = mFastaValidation.CustomRulesFilePath;
                }
            }
        }

        private bool ValidateSqlServerCachingOptionsForInputFile(string inputFilePath, bool assumeDigested, ProteinFileParser proteinFileParser)
        {
            // Returns True if the user OK's or updates the current Sql Server caching options
            // Returns False if the user cancels processing
            // Assumes that inputFilePath exists, and thus does not have a Try-Catch block

            const int SAMPLING_LINE_COUNT = 10000;

            var totalLineCount = 0;

            var suggestEnableSqlServer = false;
            var suggestDisableSqlServer = false;

            var isFastaFile = ProteinFileParser.IsFastaFile(inputFilePath, true) || proteinFileParser.ProcessingOptions.AssumeFastaFile;

            // Lookup the file size
            var inputFile = new FileInfo(inputFilePath);
            var fileSizeKB = (int)Math.Round(inputFile.Length / 1024.0d);

            if (isFastaFile)
            {
                if (proteinFileParser.ProcessingOptions.DigestionOptions.CleavageRuleID is CleavageRuleConstants.KROneEnd or CleavageRuleConstants.NoRule)
                {
                    suggestEnableSqlServer = true;
                }
                else if (fileSizeKB > 500)
                {
                    suggestEnableSqlServer = true;
                }
                else
                {
                    suggestDisableSqlServer = true;
                }
            }
            else
            {
                // Assume a delimited text file
                // Estimate the total line count by reading the first SAMPLING_LINE_COUNT lines
                try
                {
                    using var reader = new StreamReader(inputFilePath);

                    var bytesRead = 0;
                    var lineCount = 0;
                    while (!reader.EndOfStream && lineCount < SAMPLING_LINE_COUNT)
                    {
                        var dataLine = reader.ReadLine();
                        lineCount++;

                        if (dataLine != null)
                        {
                            bytesRead += dataLine.Length + 2;
                        }
                    }

                    if (lineCount < SAMPLING_LINE_COUNT || bytesRead == 0)
                    {
                        totalLineCount = lineCount;
                    }
                    else
                    {
                        totalLineCount = (int)Math.Round(lineCount * fileSizeKB / (bytesRead / 1024d));
                    }
                }
                catch
                {
                    // Error reading input file
                }

                if (assumeDigested)
                {
                    if (totalLineCount > 50000)
                    {
                        suggestEnableSqlServer = true;
                    }
                    else
                    {
                        suggestDisableSqlServer = true;
                    }
                }
                else if (totalLineCount > 1000)
                {
                    suggestEnableSqlServer = true;
                }
                else
                {
                    suggestDisableSqlServer = true;
                }
            }

            if (suggestEnableSqlServer && !chkUseSqlServerDBToCacheData.Checked)
            {
                var response = MessageBox.Show("Warning, memory usage could be quite large.  Enable Sql Server caching using Server " + txtSqlServerName.Text + "?  If no, then will continue using memory caching.", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (response == DialogResult.Yes)
                {
                    chkUseSqlServerDBToCacheData.Checked = true;
                }

                return response != DialogResult.Cancel;
            }

            if (suggestDisableSqlServer && chkUseSqlServerDBToCacheData.Checked)
            {
                var response = MessageBox.Show("Memory usage is expected to be minimal.  Continue caching data using Server " + txtSqlServerName.Text + "?  If no, then will switch to using memory caching.", "Note", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (response == DialogResult.No)
                {
                    chkUseSqlServerDBToCacheData.Checked = false;
                }

                return response != DialogResult.Cancel;
            }

            return true;
        }

        private void ValidateTextBox(TextBoxBase thisTextBox, string defaultText)
        {
            if (thisTextBox.TextLength == 0)
            {
                thisTextBox.Text = defaultText;
            }
        }

        private void cboElementMassMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePeptideUniquenessMassMode();
        }

        private void cboHydrophobicityMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComputeSequencepI();
        }

        private void cboInputFileColumnDelimiter_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void cboInputFileFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void cboProteinReversalOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkAllowSqlServerCaching_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkAutoDefineSLiCScoreTolerances_CheckedChanged(object sender, EventArgs e)
        {
            UpdateDataGridTableStyle();
        }

        private void chkComputepI_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkComputeSequenceHashValues_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkDigestProteins_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
            AutoDefineOutputFile();
        }

        private void chkUseSLiCScoreForUniqueness_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkUseSqlServerDBToCacheData_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkSqlServerUseIntegratedSecurity_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void cboOutputFileFieldDelimiter_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkAutoComputeRangeForBinning_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void cmdAbortProcessing_Click(object sender, EventArgs e)
        {
            AbortProcessingNow();
        }

        private void cmdClearPMThresholdsList_Click(object sender, EventArgs e)
        {
            ClearPMThresholdsList(true);
        }

        private void chkCreateFastaOutputFile_CheckedChanged(object sender, EventArgs e)
        {
            AutoDefineOutputFile();
        }

        private void chkLookForAddnlRefInDescription_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
        }

        private void chkMaxpIModeEnabled_CheckedChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
            ComputeSequencepI();
        }

        private void cmdGenerateUniquenessStats_Click(object sender, EventArgs e)
        {
            GenerateUniquenessStats();
        }

        private void cmdNETInfo_Click(object sender, EventArgs e)
        {
            ShowElutionTimeInfo();
        }

        private void cmdParseInputFile_Click(object sender, EventArgs e)
        {
            ParseProteinInputFile();
        }

        private void cmdPastePMThresholdsList_Click(object sender, EventArgs e)
        {
            PastePMThresholdsValues(false);
        }

        private void cmdPMThresholdsAutoPopulate_Click(object sender, EventArgs e)
        {
            if (cboPMPredefinedThresholds.SelectedIndex >= 0)
            {
                AutoPopulatePMThresholdsByID((PredefinedPMThresholdsConstants)cboPMPredefinedThresholds.SelectedIndex, true);
            }
        }

        private void cmdSelectFile_Click(object sender, EventArgs e)
        {
            SelectInputFile();
        }

        private void cmdSelectOutputFile_Click(object sender, EventArgs e)
        {
            SelectOutputFile();
        }

        private void cmdValidateFastaFile_Click(object sender, EventArgs e)
        {
            ValidateFastaFile(GetProteinInputFilePath());
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            // Note that InitializeControls() is called in Main()
        }

        private void frmMain_Closing(object sender, CancelEventArgs e)
        {
            IniFileSaveOptions(false, true);
        }

        private void txtDigestProteinsMinimumMass_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtDigestProteinsMinimumMass, e);
        }

        private void txtDigestProteinsMaximumMissedCleavages_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtDigestProteinsMaximumMissedCleavages, e);
        }

        private void txtDigestProteinsMinimumResidueCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtDigestProteinsMinimumResidueCount, e);
        }

        private void txtDigestProteinsMaximumMass_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtDigestProteinsMaximumMass, e);
        }

        private void txtMaxPeakMatchingResultsPerFeatureToSave_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtMaxPeakMatchingResultsPerFeatureToSave, e);
        }

        private void txtMaxPeakMatchingResultsPerFeatureToSave_Validating(object sender, CancelEventArgs e)
        {
            if (txtMaxPeakMatchingResultsPerFeatureToSave.Text.Trim() == "0")
            {
                txtMaxPeakMatchingResultsPerFeatureToSave.Text = "1";
            }

            TextBoxUtils.ValidateTextBoxInt(txtMaxPeakMatchingResultsPerFeatureToSave, 1, 100, 3);
        }

        private void txtMaxpISequenceLength_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && chkMaxpIModeEnabled.Checked)
            {
                ComputeSequencepI();
            }
        }

        private void txtMaxpISequenceLength_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtMaxpISequenceLength, e);
        }

        private void txtMaxpISequenceLength_Validating(object sender, CancelEventArgs e)
        {
            TextBoxUtils.ValidateTextBoxInt(txtMaxpISequenceLength, 1, 10000, 10);
        }

        private void txtMaxpISequenceLength_Validated(object sender, EventArgs e)
        {
            ComputeSequencepI();
        }

        private void txtMinimumSLiCScore_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtMinimumSLiCScore, e, true, true);
        }

        private void txtMinimumSLiCScore_Validating(object sender, CancelEventArgs e)
        {
            TextBoxUtils.ValidateTextBoxFloat(txtMinimumSLiCScore, 0f, 1f, 0.95f);
        }

        private void txtProteinInputFilePath_TextChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
            AutoDefineOutputFile();
        }

        private void txtProteinReversalSamplingPercentage_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtProteinReversalSamplingPercentage, e);
        }

        private void txtProteinScramblingLoopCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtProteinScramblingLoopCount, e);
        }

        // ReSharper disable once IdentifierTypo
        private void txtSequenceForpI_TextChanged(object sender, EventArgs e)
        {
            ComputeSequencepI();
        }

        private void txtUniquenessBinWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtUniquenessBinWidth, e);
        }

        private void txtUniquenessBinStartMass_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtUniquenessBinStartMass, e);
        }

        private void txtUniquenessBinEndMass_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBoxUtils.TextBoxKeyPressHandler(txtUniquenessBinEndMass, e);
        }

        private void mnuFileSelectInputFile_Click(object sender, EventArgs e)
        {
            SelectInputFile();
        }

        private void mnuFileSelectOutputFile_Click(object sender, EventArgs e)
        {
            SelectOutputFile();
        }

        private void mnuFileSaveDefaultOptions_Click(object sender, EventArgs e)
        {
            IniFileSaveOptions(true);
        }

        private void mnuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuEditMakeUniquenessStats_Click(object sender, EventArgs e)
        {
            GenerateUniquenessStats();
        }

        private void mnuEditParseFile_Click(object sender, EventArgs e)
        {
            ParseProteinInputFile();
        }

        private void mnuEditResetOptions_Click(object sender, EventArgs e)
        {
            ResetToDefaults(true);
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e)
        {
            ShowAboutBox();
        }

        private void mnuHelpAboutElutionTime_Click(object sender, EventArgs e)
        {
            ShowElutionTimeInfo();
        }

        private void FastaValidation_FastaValidationStarted()
        {
            mFastaValidationOptions = mFastaValidation.GetOptions();
        }

        private void ParseProteinFile_ErrorEvent(string message, Exception ex)
        {
            lblErrorMessage.Text = "Error in mParseProteinFile: " + message;
            Application.DoEvents();
        }

        private void ParseProteinFile_ProgressChanged(string taskDescription, float percentComplete)
        {
            lblProgressDescription.Text = taskDescription;
            lblProgress.Text = FormatPercentComplete(percentComplete);
            pbarProgress.Value = (int)Math.Round(percentComplete);
            Application.DoEvents();
        }

        private void ParseProteinFile_ProgressComplete()
        {
            lblProgressDescription.Text = "Processing complete";
            lblProgress.Text = FormatPercentComplete(100f);
            pbarProgress.Value = 100;

            lblSubtaskProgress.Text = string.Empty;
            lblSubtaskProgressDescription.Text = string.Empty;

            Application.DoEvents();
        }

        private void ParseProteinFile_ProgressReset()
        {
            ResetProgress();
        }

        private void ParseProteinFile_SubtaskProgressChanged(string taskDescription, float percentComplete)
        {
            lblSubtaskProgressDescription.Text = taskDescription;
            lblSubtaskProgress.Text = FormatPercentComplete(percentComplete);
            Application.DoEvents();
        }

        private void ProteinDigestionSimulator_ErrorEvent(string message, Exception ex)
        {
            lblErrorMessage.Text = "Error in mProteinDigestionSimulator: " + message;
            Application.DoEvents();
        }

        private void ProteinDigestionSimulator_ProgressChanged(string taskDescription, float percentComplete)
        {
            lblProgressDescription.Text = taskDescription;
            lblProgress.Text = FormatPercentComplete(percentComplete);
            pbarProgress.Value = (int)Math.Round(percentComplete);
            Application.DoEvents();
        }

        private void ProteinDigestionSimulator_ProgressComplete()
        {
            lblProgressDescription.Text = "Processing complete";
            lblProgress.Text = FormatPercentComplete(100f);
            pbarProgress.Value = 100;

            lblSubtaskProgress.Text = string.Empty;
            lblSubtaskProgressDescription.Text = string.Empty;

            Application.DoEvents();
        }

        private void ProteinDigestionSimulator_ProgressReset()
        {
            ResetProgress();
        }

        private void ProteinDigestionSimulator_SubtaskProgressChanged(string taskDescription, float percentComplete)
        {
            lblSubtaskProgressDescription.Text = taskDescription;
            lblSubtaskProgress.Text = FormatPercentComplete(percentComplete);
            Application.DoEvents();
        }
    }
}