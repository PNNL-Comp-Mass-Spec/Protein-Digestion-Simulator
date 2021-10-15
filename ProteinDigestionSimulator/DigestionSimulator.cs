using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using DBUtils = PRISMDatabaseUtils.DataTableUtils;
using ProteinFileReader;

// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004
//
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the 2-Clause BSD License; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// https://opensource.org/licenses/BSD-2-Clause
//
// Copyright 2018 Battelle Memorial Institute

namespace ProteinDigestionSimulator
{
    /// <summary>
    /// This class will read two FASTA files and look for overlap in protein sequence between the proteins of
    /// the first FASTA file and the second FASTA file
    /// </summary>
    public class DigestionSimulator : PRISM.FileProcessor.ProcessFilesBase
    {
        // Ignore Spelling: const, Da, pre, Sql

        public DigestionSimulator()
        {
            mFileDate = "October 1, 2021";
            InitializeLocalVariables();
        }

        public const string XML_SECTION_PEAK_MATCHING_OPTIONS = "PeakMatchingOptions";

        private const string PROTEIN_ID_COLUMN = "ProteinID";
        private const string PEPTIDE_ID_MATCH_COLUMN = "PeptideIDMatch";

        private const int ID_COUNT_DISTRIBUTION_MAX = 10;

        // Error codes specialized for this class
        public enum ErrorCodes
        {
            NoError = 0,
            ProteinDigestionSimulatorSectionNotFound = 1,
            ErrorReadingInputFile = 2,
            ProteinsNotFoundInInputFile = 4,
            ErrorIdentifyingSequences = 8,
            ErrorWritingOutputFile = 16,
            UserAbortedSearch = 32,
            UnspecifiedError = -1
        }

        private class SingleBinStats
        {
            public double MassBinStart { get; }               // Mass is >= this value
            public double MassBinEnd { get; }                 // Mass is < this value
            public int UniqueResultIDCount { get; set; }
            public int NonUniqueResultIDCount { get; set; }
            public int[] ResultIDCountDistribution { get; set; }
            public float PercentUnique { get; set; }          // UniqueResultIDs[].length / (UniqueResultIDs[].length + NonUniqueResultIDs[].length)

            public SingleBinStats(double massBinStart, double massBinEnd)
            {
                MassBinStart = massBinStart;
                MassBinEnd = massBinEnd;
                PercentUnique = 0f;
                UniqueResultIDCount = 0;
                NonUniqueResultIDCount = 0;
                ResultIDCountDistribution = new int[1];
            }
        }

        private class MassBinningOptions
        {
            public bool AutoDetermineMassRange { get; set; }
            public float MassBinSizeDa { get; set; }
            public float MassMinimum { get; set; }    // This is ignored if AutoDetermineMassRange = True
            public float MassMaximum { get; set; }    // This is ignored if AutoDetermineMassRange = True
            public float MinimumSLiCScore { get; set; }

            public MassBinningOptions()
            {
                AutoDetermineMassRange = true;
                MassBinSizeDa = 25f;
                MassMinimum = 400f;
                MassMaximum = 6000f;
                MinimumSLiCScore = 0.99f;
            }

            public MassBinningOptions Clone()
            {
                return (MassBinningOptions)MemberwiseClone();
            }
        }

        private class BinnedPeptideCountStats
        {
            public MassBinningOptions Settings { get; }
            public int BinCount { get; set; }
            public SingleBinStats[] Bins { get; set; }

            public BinnedPeptideCountStats(MassBinningOptions settings)
            {
                Settings = settings;
                BinCount = 0;
                Bins = new SingleBinStats[0];
            }
        }

        /// <summary>
        /// Protein file parser
        /// </summary>
        /// <remarks>This class is exposed as public so that we can directly access some of its properties without having to create wrapper properties in this class</remarks>
        public ProteinFileParser mProteinFileParser;

        private char mOutputFileDelimiter;
        private int mMaxPeakMatchingResultsPerFeatureToSave;

        private readonly MassBinningOptions mPeptideUniquenessBinningSettings = new();

        private string mLastErrorMessage;

        /// <summary>
        /// Comparison peptides to match against
        /// </summary>
        private PeakMatching.PMComparisonFeatureInfo mComparisonPeptideInfo;
        private ProteinCollection mProteinInfo;
        private PeakMatching.PMFeatureMatchResults mPeptideMatchResults;

        /// <summary>
        /// Holds the lists of peptides that were uniquely identified for each protein
        /// </summary>
        private DataTable mProteinToIdentifiedPeptideMappingTable;

        /// <summary>
        /// Thresholds to use for searching
        /// </summary>
        private PeakMatching.SearchThresholds[] mThresholdLevels;

        private string mSubtaskProgressStepDescription = string.Empty;
        private float mSubtaskProgressPercentComplete = 0f;

        public event ProgressChangedEventHandler SubtaskProgressChanged;

        public bool AutoDetermineMassRangeForBinning
        {
            get => mPeptideUniquenessBinningSettings.AutoDetermineMassRange;
            set => mPeptideUniquenessBinningSettings.AutoDetermineMassRange = value;
        }

        public bool CreateSeparateOutputFileForEachThreshold { get; set; }

        public bool CysPeptidesOnly { get; set; }

        /// <summary>
        /// When true, digest protein sequences
        /// </summary>
        /// <remarks>Ignored for FASTA files; they are always digested</remarks>
        public bool DigestSequences { get; set; }

        public PeptideSequence.ElementModeConstants ElementMassMode
        {
            get
            {
                if (mProteinFileParser == null)
                {
                    return PeptideSequence.ElementModeConstants.IsotopicMass;
                }

                return mProteinFileParser.ElementMassMode;
            }
            set
            {
                if (mProteinFileParser == null)
                {
                    InitializeProteinFileParser();
                }

                mProteinFileParser.ElementMassMode = value;
            }
        }

        public ErrorCodes LocalErrorCode { get; private set; }

        public int MaxPeakMatchingResultsPerFeatureToSave
        {
            get => mMaxPeakMatchingResultsPerFeatureToSave;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                mMaxPeakMatchingResultsPerFeatureToSave = value;
            }
        }

        public float MinimumSLiCScoreToBeConsideredUnique
        {
            get => mPeptideUniquenessBinningSettings.MinimumSLiCScore;
            set => mPeptideUniquenessBinningSettings.MinimumSLiCScore = value;
        }

        public float PeptideUniquenessMassBinSizeForBinning
        {
            get => mPeptideUniquenessBinningSettings.MassBinSizeDa;
            set
            {
                if (value > 0f)
                {
                    mPeptideUniquenessBinningSettings.MassBinSizeDa = value;
                }
            }
        }

        public char OutputFileDelimiter
        {
            get => mOutputFileDelimiter;
            set
            {
                if (value != default)
                {
                    mOutputFileDelimiter = value;
                }
            }
        }

        public bool SavePeakMatchingResults { get; set; }

        /// <summary>
        /// Use Ellipse Search Region
        /// </summary>
        /// <remarks>
        /// Only valid if mUseSLiCScoreForUniqueness = False
        /// If both mUseSLiCScoreForUniqueness = False and mUseEllipseSearchRegion = False, uses a rectangle to determine uniqueness
        /// </remarks>
        public bool UseEllipseSearchRegion { get; set; }

        public bool UseSLiCScoreForUniqueness { get; set; }

        public void AddSearchThresholdLevel(PeakMatching.SearchThresholds.MassToleranceConstants massToleranceType, double massTolerance, double netTolerance, bool clearExistingThresholds)
        {
            AddSearchThresholdLevel(massToleranceType, massTolerance, netTolerance, true, 0d, 0d, true, clearExistingThresholds);
        }

        public void AddSearchThresholdLevel(PeakMatching.SearchThresholds.MassToleranceConstants massToleranceType, double massTolerance, double netTolerance, bool autoDefineSLiCScoreThresholds, double slicScoreMassPPMStDev, double slicScoreNETStDev, bool slicScoreUseAMTNETStDev, bool clearExistingThresholds)
        {
            AddSearchThresholdLevel(massToleranceType, massTolerance, netTolerance, autoDefineSLiCScoreThresholds, slicScoreMassPPMStDev, slicScoreNETStDev, slicScoreUseAMTNETStDev, clearExistingThresholds, PeakMatching.DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER);
        }

        public void AddSearchThresholdLevel(PeakMatching.SearchThresholds.MassToleranceConstants massToleranceType, double massTolerance, double netTolerance, bool autoDefineSLiCScoreThresholds, double slicScoreMassPPMStDev, double slicScoreNETStDev, bool slicScoreUseAMTNETStDev, bool clearExistingThresholds, float slicScoreMaxSearchDistanceMultiplier)
        {
            if (clearExistingThresholds)
            {
                InitializeThresholdLevels(ref mThresholdLevels, 1, false);
            }
            else
            {
                InitializeThresholdLevels(ref mThresholdLevels, mThresholdLevels.Length + 1, true);
            }

            var index = mThresholdLevels.Length - 1;

            mThresholdLevels[index].AutoDefineSLiCScoreThresholds = autoDefineSLiCScoreThresholds;

            mThresholdLevels[index].SLiCScoreMaxSearchDistanceMultiplier = slicScoreMaxSearchDistanceMultiplier;
            mThresholdLevels[index].MassTolType = massToleranceType;
            mThresholdLevels[index].MassTolerance = massTolerance;
            mThresholdLevels[index].NETTolerance = netTolerance;

            mThresholdLevels[index].SLiCScoreUseAMTNETStDev = slicScoreUseAMTNETStDev;

            if (!autoDefineSLiCScoreThresholds)
            {
                mThresholdLevels[index].SLiCScoreMassPPMStDev = slicScoreMassPPMStDev;
                mThresholdLevels[index].SLiCScoreNETStDev = slicScoreNETStDev;
            }
        }

        /// <summary>
        /// Add/update a peptide
        /// </summary>
        /// <remarks>
        /// Assures that the peptide is present in mComparisonPeptideInfo and that the protein and protein/peptide mapping is present in mProteinInfo
        /// Assumes that uniqueSeqID is truly unique for the given peptide
        /// </remarks>
        /// <param name="uniqueSeqID"></param>
        /// <param name="peptideMass"></param>
        /// <param name="peptideNET"></param>
        /// <param name="peptideNETStDev"></param>
        /// <param name="peptideDiscriminantScore"></param>
        /// <param name="proteinName"></param>
        /// <param name="cleavageStateInProtein"></param>
        /// <param name="peptideName"></param>
        private void AddOrUpdatePeptide(
            int uniqueSeqID,
            double peptideMass,
            float peptideNET,
            float peptideNETStDev,
            float peptideDiscriminantScore,
            string proteinName,
            ProteinCollection.CleavageStateConstants cleavageStateInProtein,
            string peptideName)
        {
            int proteinID;

            mComparisonPeptideInfo.Add(uniqueSeqID, peptideName, peptideMass, peptideNET, peptideNETStDev, peptideDiscriminantScore);

            // Determine the ProteinID for proteinName
            if (!string.IsNullOrEmpty(proteinName))
            {
                // Lookup the index for the given protein
                proteinID = AddOrUpdateProtein(proteinName);
            }
            else
            {
                proteinID = -1;
            }

            if (proteinID >= 0)
            {
                // Add the protein to the peptide to protein mapping table, if necessary
                mProteinInfo.AddProteinToPeptideMapping(proteinID, uniqueSeqID, cleavageStateInProtein);
            }
        }

        /// <summary>
        /// Add/update a protein
        /// </summary>
        /// <param name="proteinName"></param>
        /// <returns>The index of the protein in mProteinInfo, or -1 if not found and unable to add it</returns>
        private int AddOrUpdateProtein(string proteinName)
        {
            // Note: proteinID is auto-assigned
            if (!mProteinInfo.Add(proteinName, out var proteinID))
            {
                proteinID = -1;
            }

            return proteinID;
        }

        private bool ExportPeakMatchingResults(
            PeakMatching.SearchThresholds thresholds,
            int thresholdIndex,
            int comparisonFeatureCount,
            PeakMatching.PMFeatureMatchResults peptideMatchResults,
            string outputFolderPath,
            string outputFilenameBase,
            StreamWriter pmResultsWriter)
        {
            var currentFeatureID = default(int);
            var matchResultInfo = default(PeakMatching.PMFeatureMatchResults.PeakMatchingResult);

            bool success;

            try
            {
                ResetProgress("Exporting peak matching results");

                if (CreateSeparateOutputFileForEachThreshold)
                {
                    // Create one file for each search threshold level
                    var workingFilenameBase = outputFilenameBase + "_PMResults" + (thresholdIndex + 1) + ".txt";
                    var outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);

                    pmResultsWriter = new StreamWriter(outputFilePath);

                    // Write the threshold values to the output file
                    pmResultsWriter.WriteLine("Comparison feature count: " + comparisonFeatureCount);
                    ExportThresholds(pmResultsWriter, thresholdIndex, thresholds);

                    pmResultsWriter.WriteLine();

                    ExportPeakMatchingResultsWriteHeaders(pmResultsWriter, CreateSeparateOutputFileForEachThreshold);
                }

                var cachedMatchCount = 0;
                var cachedMatchCountFeatureID = -1;

                var lastFlushTime = DateTime.UtcNow;

                // ToDo: Grab chunks of data from the server if caching into SqlServer (change this to a while loop)
                int matchIndex;
                for (matchIndex = 0; matchIndex < peptideMatchResults.Count; matchIndex++)
                {
                    string linePrefix;
                    if (CreateSeparateOutputFileForEachThreshold)
                    {
                        linePrefix = string.Empty;
                    }
                    else
                    {
                        linePrefix = (thresholdIndex + 1).ToString() + mOutputFileDelimiter;
                    }

                    if (peptideMatchResults.GetMatchInfoByRowIndex(matchIndex, ref currentFeatureID, ref matchResultInfo))
                    {
                        if (currentFeatureID != cachedMatchCountFeatureID)
                        {
                            cachedMatchCount = peptideMatchResults.GetMatchCountForFeatureID(currentFeatureID);
                            cachedMatchCountFeatureID = currentFeatureID;
                        }

                        var lineOut = linePrefix +
                                      currentFeatureID + mOutputFileDelimiter +
                                      cachedMatchCount + mOutputFileDelimiter +
                                      matchResultInfo.MultiAMTHitCount + mOutputFileDelimiter +
                                      matchResultInfo.MatchingID + mOutputFileDelimiter +
                                      Math.Round(matchResultInfo.MassErr, 6) + mOutputFileDelimiter +
                                      Math.Round(matchResultInfo.NETErr, 4) + mOutputFileDelimiter +
                                      Math.Round(matchResultInfo.SLiCScore, 4) + mOutputFileDelimiter +
                                      Math.Round(matchResultInfo.DelSLiC, 4);
                        pmResultsWriter.WriteLine(lineOut);
                    }

                    if (matchIndex % 100 == 0)
                    {
                        UpdateProgress((float)(matchIndex / (double)peptideMatchResults.Count * 100d));

                        if (DateTime.UtcNow.Subtract(lastFlushTime).TotalSeconds > 30d)
                        {
                            pmResultsWriter.Flush();
                        }
                    }
                }

                if (CreateSeparateOutputFileForEachThreshold)
                {
                    pmResultsWriter.Close();
                }

                UpdateProgress(100f);

                success = true;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        private void ExportPeakMatchingResultsWriteHeaders(TextWriter pmResultsWriter, bool createSeparateOutputFiles)
        {
            string lineOut;

            // Write the column headers
            if (createSeparateOutputFiles)
            {
                lineOut = string.Empty;
            }
            else
            {
                lineOut = "Threshold_Index" + mOutputFileDelimiter;
            }

            lineOut += "Unique_Sequence_ID" +
                       mOutputFileDelimiter + "Match_Count" +
                       mOutputFileDelimiter + "Multi_AMT_Hit_Count" +
                       mOutputFileDelimiter + "Matching_ID_Index" +
                       mOutputFileDelimiter + "Mass_Err";
            lineOut += mOutputFileDelimiter + "NET_Err";

            lineOut += mOutputFileDelimiter + "SLiC_Score" +
                       mOutputFileDelimiter + "Del_SLiC";
            pmResultsWriter.WriteLine(lineOut);
        }

        private bool ExportPeptideUniquenessResults(int thresholdIndex, BinnedPeptideCountStats binResults, TextWriter peptideUniquenessWriter)
        {
            bool success;

            try
            {
                for (var binIndex = 0; binIndex < binResults.BinCount; binIndex++)
                {
                    string lineOut;
                    if (CreateSeparateOutputFileForEachThreshold)
                    {
                        lineOut = string.Empty;
                    }
                    else
                    {
                        lineOut = (thresholdIndex + 1).ToString() + mOutputFileDelimiter;
                    }

                    var peptideCountTotal = binResults.Bins[binIndex].NonUniqueResultIDCount + binResults.Bins[binIndex].UniqueResultIDCount;

                    lineOut += Math.Round(binResults.Bins[binIndex].MassBinStart, 2).ToString() + mOutputFileDelimiter +
                               Math.Round(binResults.Bins[binIndex].PercentUnique, 3) + mOutputFileDelimiter +
                               peptideCountTotal;

                    for (var index = 1; index <= Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave); index++)
                    {
                        lineOut += mOutputFileDelimiter + binResults.Bins[binIndex].ResultIDCountDistribution[index].ToString();
                    }

                    peptideUniquenessWriter.WriteLine(lineOut);
                }

                peptideUniquenessWriter.Flush();
                success = true;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        private bool ExportProteinStats(
            int thresholdIndex,
            TextWriter proteinStatsWriter)
        {
            try
            {
                var lastFlushTime = DateTime.UtcNow;
                for (var proteinIndex = 0; proteinIndex < mProteinInfo.Count; proteinIndex++)
                {
                    string lineOut;
                    if (CreateSeparateOutputFileForEachThreshold)
                    {
                        lineOut = string.Empty;
                    }
                    else
                    {
                        lineOut = (thresholdIndex + 1).ToString() + mOutputFileDelimiter;
                    }

                    if (mProteinInfo.GetProteinInfoByRowIndex(proteinIndex, out var proteinID, out var proteinName))
                    {
                        lineOut += proteinName +
                                   mOutputFileDelimiter + proteinID +
                                   mOutputFileDelimiter + mProteinInfo.GetPeptideCountForProteinByID(proteinID) +
                                   mOutputFileDelimiter + GetPeptidesUniquelyIdentifiedCountByProteinID(proteinID);

                        proteinStatsWriter.WriteLine(lineOut);
                    }

                    if (proteinIndex % 100 == 0 && DateTime.UtcNow.Subtract(lastFlushTime).TotalSeconds > 30d)
                    {
                        proteinStatsWriter.Flush();
                    }
                }

                proteinStatsWriter.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool FeatureContainsUniqueMatch(
            PeakMatching.FeatureInfo featureInfo,
            PeakMatching.PMFeatureMatchResults peptideMatchResults,
            ref int matchCount,
            bool usingSLiCScoreForUniqueness,
            float minimumSLiCScore)
        {
            PeakMatching.PMFeatureMatchResults.PeakMatchingResult[] matchResults = null;

            var uniqueMatch = false;

            if (peptideMatchResults.GetMatchInfoByFeatureID(featureInfo.FeatureID, ref matchResults, ref matchCount))
            {
                if (matchCount > 0)
                {
                    // The current feature has 1 or more matches

                    if (usingSLiCScoreForUniqueness)
                    {
                        // See if any of the matches have a SLiC Score >= the minimum SLiC Score
                        for (var matchIndex = 0; matchIndex < matchCount; matchIndex++)
                        {
                            if (Math.Round(matchResults[matchIndex].SLiCScore, 4) >= Math.Round(minimumSLiCScore, 4))
                            {
                                uniqueMatch = true;
                                break;
                            }
                        }
                    }
                    // Not using SLiC score; when we performed the peak matching, we used an ellipse or rectangle to find matching features
                    // This feature is unique if it only has one match
                    else if (matchCount == 1)
                    {
                        uniqueMatch = true;
                    }
                }
            }
            else
            {
                matchCount = 0;
            }

            return uniqueMatch;
        }

        private void ExportThresholds(
            TextWriter writer,
            int thresholdIndex,
            PeakMatching.SearchThresholds searchThresholds)
        {
            const string delimiter = "; ";

            // Write the thresholds
            var outline = new StringBuilder();

            outline.AppendFormat("Threshold Index: {0}{1}", thresholdIndex + 1, delimiter);
            outline.Append("Mass Tolerance: +- ");

            switch (searchThresholds.MassTolType)
            {
                case PeakMatching.SearchThresholds.MassToleranceConstants.Absolute:
                    outline.AppendFormat("{0} Da", Math.Round(searchThresholds.MassTolerance, 5));
                    break;
                case PeakMatching.SearchThresholds.MassToleranceConstants.PPM:
                    outline.AppendFormat("{0} ppm", Math.Round(searchThresholds.MassTolerance, 2));
                    break;
                default:
                    outline.Append("Unknown mass tolerance mode");
                    break;
            }

            outline.AppendFormat("{0}NET Tolerance: +- {1}", delimiter, Math.Round(searchThresholds.NETTolerance, 4));

            if (UseSLiCScoreForUniqueness)
            {
                outline.AppendFormat("{0}Minimum SLiC Score: {1}", delimiter, Math.Round(mPeptideUniquenessBinningSettings.MinimumSLiCScore, 3));
                outline.AppendFormat("; Max search distance multiplier: {0}", Math.Round(searchThresholds.SLiCScoreMaxSearchDistanceMultiplier, 1));
            }
            else if (UseEllipseSearchRegion)
            {
                outline.AppendFormat("{0}Minimum SLiC Score: N/A; using ellipse to find matching features", delimiter);
            }
            else
            {
                outline.AppendFormat("{0}Minimum SLiC Score: N/A; using rectangle to find matching features", delimiter);
            }

            writer.WriteLine(outline.ToString());
        }

        public bool GenerateUniquenessStats(string proteinInputFilePath, string outputFolderPath, string outputFilenameBase)
        {
            var progressStepCount = default(int);
            var progressStep = default(int);

            bool success;

            SearchRange rangeSearch = null;

            StreamWriter pmResultsWriter = null;
            StreamWriter peptideUniquenessWriter = null;
            StreamWriter proteinStatsWriter = null;

            try
            {
                if (Path.HasExtension(outputFilenameBase))
                {
                    // Remove the extension
                    outputFilenameBase = Path.ChangeExtension(outputFilenameBase, null);
                }

                // ----------------------------------------------------
                // Initialize the progress bar
                // ----------------------------------------------------
                progressStep = 0;
                progressStepCount = 1 + mThresholdLevels.Length * 3;

                LogMessage("Caching data in memory");
                LogMessage("Caching peak matching results in memory");

                // ----------------------------------------------------
                // Load the proteins and digest them, or simply load the peptides
                // ----------------------------------------------------
                LogMessage("Load proteins or peptides from " + Path.GetFileName(proteinInputFilePath));
                success = LoadProteinsOrPeptides(proteinInputFilePath);

                // ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True

                progressStep = 1;
                UpdateProgress((float)(progressStep / (double)progressStepCount * 100d));
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile, ex);
                success = false;
            }

            if (success && mComparisonPeptideInfo.Count == 0)
            {
                SetLocalErrorCode(ErrorCodes.ProteinsNotFoundInInputFile, true);
                success = false;
            }
            else if (!success || AbortProcessing)
            {
                success = false;
            }
            else
            {
                try
                {
                    // ----------------------------------------------------
                    // Search mComparisonPeptideInfo against itself
                    // Since mComparisonPeptideInfo is class type PMComparisonFeatureInfo, which is a
                    // derived class of PMFeatureInfo, we can simply link the two objects
                    // This way, we don't use double the memory necessary
                    // ----------------------------------------------------

                    UpdateProgress("Initializing structures");

                    // Initialize featuresToIdentify by linking to mComparisonPeptideInfo
                    PeakMatching.PMFeatureInfo featuresToIdentify = mComparisonPeptideInfo;

                    try
                    {
                        // ----------------------------------------------------
                        // Compare featuresToIdentify to mComparisonPeptideInfo for each threshold level
                        // ----------------------------------------------------

                        // ----------------------------------------------------
                        // Validate mThresholdLevels to assure at least one threshold exists
                        // ----------------------------------------------------

                        if (mThresholdLevels.Length == 0)
                        {
                            InitializeThresholdLevels(ref mThresholdLevels, 1, false);
                        }

                        for (var thresholdIndex = 0; thresholdIndex < mThresholdLevels.Length; thresholdIndex++)
                        {
                            if (mThresholdLevels[thresholdIndex] == null)
                            {
                                if (thresholdIndex == 0)
                                {
                                    // Need at least one set of thresholds
                                    mThresholdLevels[thresholdIndex] = new PeakMatching.SearchThresholds();
                                }
                                else
                                {
                                    Array.Resize(ref mThresholdLevels, thresholdIndex);
                                    break;
                                }
                            }
                        }

                        // ----------------------------------------------------
                        // Initialize rangeSearch
                        // ----------------------------------------------------

                        LogMessage("Uniqueness Stats processing starting, Threshold Count = " + mThresholdLevels.Length);

                        if (!PeakMatching.FillRangeSearchObject(ref rangeSearch, mComparisonPeptideInfo))
                        {
                            success = false;
                        }
                        else
                        {
                            // ----------------------------------------------------
                            // Initialize the peak matching class
                            // ----------------------------------------------------

                            var peakMatching = new PeakMatching
                            {
                                MaxPeakMatchingResultsPerFeatureToSave = mMaxPeakMatchingResultsPerFeatureToSave,
                                UseMaxSearchDistanceMultiplierAndSLiCScore = UseSLiCScoreForUniqueness,
                                UseEllipseSearchRegion = UseEllipseSearchRegion
                            };

                            peakMatching.LogEvent += PeakMatching_LogEvent;
                            peakMatching.ProgressChanged += PeakMatching_ProgressContinues;

                            // ----------------------------------------------------
                            // Initialize the output files if combining all results
                            // in a single file for each type of result
                            // ----------------------------------------------------

                            if (!CreateSeparateOutputFileForEachThreshold)
                            {
                                InitializePeptideAndProteinResultsFiles(outputFolderPath, outputFilenameBase, mThresholdLevels, out peptideUniquenessWriter, out proteinStatsWriter);

                                if (SavePeakMatchingResults)
                                {
                                    InitializePMResultsFile(outputFolderPath, outputFilenameBase, mThresholdLevels, mComparisonPeptideInfo.Count, out pmResultsWriter);
                                }
                            }

                            for (var thresholdIndex = 0; thresholdIndex < mThresholdLevels.Length; thresholdIndex++)
                            {
                                UpdateProgress("Generating uniqueness stats, threshold " + (thresholdIndex + 1) + " / " + mThresholdLevels.Length);

                                UpdateSubtaskProgress("Finding matching peptides for given search thresholds", 0f);

                                // Perform the actual peak matching
                                LogMessage("Threshold " + (thresholdIndex + 1) + ", IdentifySequences");
                                success = peakMatching.IdentifySequences(mThresholdLevels[thresholdIndex], ref featuresToIdentify, mComparisonPeptideInfo, out var featureMatchResults, ref rangeSearch);
                                mPeptideMatchResults = featureMatchResults;
                                mPeptideMatchResults.SortingList += PeptideMatchResults_SortingList;

                                if (!success)
                                {
                                    break;
                                }

                                if (SavePeakMatchingResults)
                                {
                                    // Write out the raw peak matching results
                                    success = ExportPeakMatchingResults(mThresholdLevels[thresholdIndex], thresholdIndex, mComparisonPeptideInfo.Count, mPeptideMatchResults, outputFolderPath, outputFilenameBase, pmResultsWriter);
                                    if (!success)
                                    {
                                        break;
                                    }
                                }

                                progressStep++;
                                UpdateProgress((float)(progressStep / (double)progressStepCount * 100d));

                                // Summarize the results by peptide
                                LogMessage("Threshold " + (thresholdIndex + 1) + ", SummarizeResultsByPeptide");
                                success = SummarizeResultsByPeptide(mThresholdLevels[thresholdIndex], thresholdIndex, featuresToIdentify, mPeptideMatchResults, outputFolderPath, outputFilenameBase, peptideUniquenessWriter);
                                if (!success)
                                {
                                    break;
                                }

                                progressStep++;
                                UpdateProgress((float)(progressStep / (double)progressStepCount * 100d));

                                // Summarize the results by protein
                                LogMessage("Threshold " + (thresholdIndex + 1) + ", SummarizeResultsByProtein");
                                success = SummarizeResultsByProtein(mThresholdLevels[thresholdIndex], thresholdIndex, featuresToIdentify, mPeptideMatchResults, outputFolderPath, outputFilenameBase, proteinStatsWriter);
                                if (!success)
                                {
                                    break;
                                }

                                progressStep++;
                                UpdateProgress((float)(progressStep / (double)progressStepCount * 100d));
                                if (AbortProcessing)
                                {
                                    success = false;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SetLocalErrorCode(ErrorCodes.ErrorIdentifyingSequences, ex);
                        success = false;
                    }

                    UpdateProgress(100f);
                }
                catch (Exception ex)
                {
                    SetLocalErrorCode(ErrorCodes.ErrorIdentifyingSequences, ex);
                    success = false;
                }
                finally
                {
                    try
                    {
                        pmResultsWriter?.Close();

                        peptideUniquenessWriter?.Close();

                        proteinStatsWriter?.Close();
                    }
                    catch
                    {
                        // Ignore any errors closing files
                    }
                }
            }

            if (success)
            {
                LogMessage("Uniqueness Stats processing complete");
            }

            return success;
        }

        public override IList<string> GetDefaultExtensionsToParse()
        {
            var extensionsToParse = new List<string> { ".fasta", ".txt" };
            return extensionsToParse;
        }

        public override string GetErrorMessage()
        {
            // Returns "" if no error

            string errorMessage;

            if (ErrorCode == ProcessFilesErrorCodes.LocalizedError ||
                ErrorCode == ProcessFilesErrorCodes.NoError)
            {
                errorMessage = LocalErrorCode switch
                {
                    ErrorCodes.NoError => "",
                    ErrorCodes.ProteinDigestionSimulatorSectionNotFound => "The section " + ProteinFileParser.XML_SECTION_OPTIONS + " was not found in the parameter file",
                    ErrorCodes.ErrorReadingInputFile => "Error reading input file",
                    ErrorCodes.ProteinsNotFoundInInputFile => "No proteins were found in the input file (make sure the Column Order is correct on the File Format Options tab)",
                    ErrorCodes.ErrorIdentifyingSequences => "Error identifying sequences",
                    ErrorCodes.ErrorWritingOutputFile => "Error writing to one of the output files",
                    ErrorCodes.UserAbortedSearch => "User aborted search",
                    ErrorCodes.UnspecifiedError => "Unspecified localized error",
                    _ => "Unknown error state",// This shouldn't happen
                };
            }
            else
            {
                errorMessage = GetBaseClassErrorMessage();
            }

            if (!string.IsNullOrEmpty(mLastErrorMessage))
            {
                errorMessage += Environment.NewLine + mLastErrorMessage;
            }

            return errorMessage;
        }

        private int GetNextUniqueSequenceID(string sequence, Dictionary<string, int> masterSequences, ref int nextUniqueIDForMasterSeqs)
        {
            int uniqueSeqID;

            try
            {
                if (masterSequences.ContainsKey(sequence))
                {
                    uniqueSeqID = masterSequences[sequence];
                }
                else
                {
                    masterSequences.Add(sequence, nextUniqueIDForMasterSeqs);
                    uniqueSeqID = nextUniqueIDForMasterSeqs;
                }

                nextUniqueIDForMasterSeqs++;
            }
            catch
            {
                uniqueSeqID = 0;
            }

            return uniqueSeqID;
        }

        public void GetPeptideUniquenessMassRangeForBinning(out float massMinimum, out float massMaximum)
        {
            massMinimum = mPeptideUniquenessBinningSettings.MassMinimum;
            massMaximum = mPeptideUniquenessBinningSettings.MassMaximum;
        }

        private int GetPeptidesUniquelyIdentifiedCountByProteinID(int proteinID)
        {
            var dataRows = mProteinToIdentifiedPeptideMappingTable.Select(PROTEIN_ID_COLUMN + " = " + proteinID);
            return dataRows.Length;
        }

        private void InitializeBinnedStats(BinnedPeptideCountStats statsBinned, int binCount)
        {
            int binIndex;

            statsBinned.BinCount = binCount;
            statsBinned.Bins = new SingleBinStats[statsBinned.BinCount];

            for (binIndex = 0; binIndex < statsBinned.BinCount; binIndex++)
            {
                var stats = new SingleBinStats(
                    statsBinned.Settings.MassMinimum + statsBinned.Settings.MassBinSizeDa * binIndex,
                    statsBinned.Settings.MassMinimum + statsBinned.Settings.MassBinSizeDa * (binIndex + 1))
                {
                    PercentUnique = 0f,
                    UniqueResultIDCount = 0,
                    NonUniqueResultIDCount = 0,
                    ResultIDCountDistribution = new int[Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave) + 1]
                };

                statsBinned.Bins[binIndex] = stats;
            }
        }

        private void InitializePeptideAndProteinResultsFiles(
            string outputFolderPath,
            string outputFilenameBase,
            IList<PeakMatching.SearchThresholds> thresholds,
            out StreamWriter peptideUniquenessWriter,
            out StreamWriter proteinStatsWriter)
        {
            // Initialize the output file so that the peptide and protein summary results for all thresholds can be saved in the same file

            int thresholdIndex;

            // ----------------------------------------------------
            // Create the peptide uniqueness stats files
            // ----------------------------------------------------

            var workingFilenameBase = outputFilenameBase + "_PeptideStatsBinned.txt";
            var outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);

            peptideUniquenessWriter = new StreamWriter(outputFilePath);

            // ----------------------------------------------------
            // Create the protein stats file
            // ----------------------------------------------------

            workingFilenameBase = outputFilenameBase + "_ProteinStats.txt";
            outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);

            proteinStatsWriter = new StreamWriter(outputFilePath);

            // ----------------------------------------------------
            // Write the thresholds
            // ----------------------------------------------------

            for (thresholdIndex = 0; thresholdIndex < thresholds.Count; thresholdIndex++)
            {
                ExportThresholds(peptideUniquenessWriter, thresholdIndex, thresholds[thresholdIndex]);
                ExportThresholds(proteinStatsWriter, thresholdIndex, thresholds[thresholdIndex]);
            }

            peptideUniquenessWriter.WriteLine();
            SummarizeResultsByPeptideWriteHeaders(peptideUniquenessWriter, CreateSeparateOutputFileForEachThreshold);
            proteinStatsWriter.WriteLine();
            SummarizeResultsByProteinWriteHeaders(proteinStatsWriter, CreateSeparateOutputFileForEachThreshold);
        }

        private void InitializePMResultsFile(
            string outputFolderPath,
            string outputFilenameBase,
            IList<PeakMatching.SearchThresholds> thresholds,
            int comparisonFeaturesCount,
            out StreamWriter pmResultsWriter)
        {
            // Initialize the output file so that the peak matching results for all thresholds can be saved in the same file

            int thresholdIndex;

            // Combine all of the data together in one output file
            var workingFilenameBase = outputFilenameBase + "_PMResults.txt";
            var outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);

            pmResultsWriter = new StreamWriter(outputFilePath);

            pmResultsWriter.WriteLine("Comparison feature count: " + comparisonFeaturesCount);

            for (thresholdIndex = 0; thresholdIndex < thresholds.Count; thresholdIndex++)
            {
                ExportThresholds(pmResultsWriter, thresholdIndex, thresholds[thresholdIndex]);
            }

            pmResultsWriter.WriteLine();

            ExportPeakMatchingResultsWriteHeaders(pmResultsWriter, CreateSeparateOutputFileForEachThreshold);
        }

        private void InitializeLocalVariables()
        {
            DigestSequences = false;
            CysPeptidesOnly = false;
            mOutputFileDelimiter = '\t';

            SavePeakMatchingResults = false;
            mMaxPeakMatchingResultsPerFeatureToSave = 3;

            mPeptideUniquenessBinningSettings.AutoDetermineMassRange = true;
            mPeptideUniquenessBinningSettings.MassBinSizeDa = 25f;
            mPeptideUniquenessBinningSettings.MassMinimum = 400f;
            mPeptideUniquenessBinningSettings.MassMaximum = 6000f;
            mPeptideUniquenessBinningSettings.MinimumSLiCScore = 0.99f;

            UseSLiCScoreForUniqueness = true;
            UseEllipseSearchRegion = true;

            CreateSeparateOutputFileForEachThreshold = false;

            LocalErrorCode = ErrorCodes.NoError;
            mLastErrorMessage = string.Empty;

            InitializeProteinToIdentifiedPeptideMappingTable();

            InitializeThresholdLevels(ref mThresholdLevels, 1, false);
            InitializeProteinFileParser();

            // mUseSqlServerDBToCacheData = false;
            // mUseSqlServerForMatchResults = false;

            // mUseBulkInsert = false;

            // mSqlServerConnectionString = DBUtils.DEFAULT_CONNECTION_STRING_NO_PROVIDER;
            // mTableNameFeaturesToIdentify = PeakMatching.PMFeatureInfo.DEFAULT_FEATURE_INFO_TABLE_NAME;
            // mTableNameComparisonPeptides = PeakMatching.PMComparisonFeatureInfo.DEFAULT_COMPARISON_FEATURE_INFO_TABLE_NAME;

            // mTableNameProteinInfo = ProteinInfoCollection.DEFAULT_PROTEIN_INFO_TABLE_NAME;
            // mTableNameProteinToPeptideMap = ProteinInfoCollection.DEFAULT_PROTEIN_TO_PEPTIDE_MAP_TABLE_NAME;
        }

        private void InitializeProteinToIdentifiedPeptideMappingTable()
        {
            if (mProteinToIdentifiedPeptideMappingTable == null)
            {
                mProteinToIdentifiedPeptideMappingTable = new DataTable();

                // ---------------------
                // Protein stats uniquely identified peptides table
                // ---------------------
                DBUtils.AppendColumnIntegerToTable(mProteinToIdentifiedPeptideMappingTable, PROTEIN_ID_COLUMN);
                DBUtils.AppendColumnStringToTable(mProteinToIdentifiedPeptideMappingTable, PEPTIDE_ID_MATCH_COLUMN);

                // Define the PROTEIN_ID_COLUMN AND PEPTIDE_ID_COLUMN columns to be the primary key
                mProteinToIdentifiedPeptideMappingTable.PrimaryKey = new[]
                {
                    mProteinToIdentifiedPeptideMappingTable.Columns[PROTEIN_ID_COLUMN],
                    mProteinToIdentifiedPeptideMappingTable.Columns[PEPTIDE_ID_MATCH_COLUMN]
                };
            }
            else
            {
                mProteinToIdentifiedPeptideMappingTable.Clear();
            }
        }

        //// The following was created to test the speed and performance when dealing with large dataset tables
        //private void FillWithLotsOfData(ref System.Data.DataSet targetDataset)
        //{
        //    const int MAX_FEATURE_COUNT = 300000;

        //    Random randomGenerator = new Random();

        //    ProgressFormNET.frmProgress progressForm = new ProgressFormNET.frmProgress();

        //    var indexEnd = System.Convert.ToInt32(MAX_FEATURE_COUNT * 1.5);
        //    progressForm.InitializeProgressForm("Populating dataset table", 0, indexEnd, true);
        //    progressForm.Visible = true;
        //    Application.DoEvents();

        //    var startTime = System.DateTime.UtcNow;

        //    for (var index = 0; index <= indexEnd; index++)
        //    {
        //        var newFeatureID = randomGenerator.Next(0, MAX_FEATURE_COUNT);

        //        // Look for existing entry in table
        //        if (!dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME).Rows.Contains(newFeatureID))
        //        {
        //            var newRow = dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME).NewRow;
        //            newRow(COMPARISON_FEATURE_ID_COLUMN) = newFeatureID;
        //            newRow(FEATURE_NAME_COLUMN) = "Feature" + newFeatureID.ToString();
        //            newRow(MASS_COLUMN) = newFeatureID / (double)System.Convert.ToSingle(MAX_FEATURE_COUNT) * 1000;
        //            newRow(NET_COLUMN) = randomGenerator.Next(0, 1000) / 1000.0;
        //            newRow(NET_STDEV_COLUMN) = randomGenerator.Next(0, 1000) / 10000.0;
        //            newRow(DISCRIMINANT_SCORE_COLUMN) = randomGenerator.Next(0, 1000) / 1000.0;
        //            dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME).Rows.Add(newRow);
        //        }

        //        if (index % 100 == 0)
        //        {
        //            progressForm.UpdateProgressBar(index);
        //            Application.DoEvents();

        //            if (progressForm.KeyPressAbortProcess)
        //                break;
        //        }
        //    }

        //    MessageBox.Show("Elapsed time: " + Math.Round(System.DateTime.UtcNow.Subtract(startTime).TotalSeconds, 2).ToString() + " seconds", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //}

        public void InitializeProteinFileParser(bool resetToDefaults = false)
        {
            if (mProteinFileParser == null)
            {
                mProteinFileParser = new ProteinFileParser();
                mProteinFileParser.ErrorEvent += ProteinFileParser_ErrorEvent;
                resetToDefaults = true;
            }

            if (resetToDefaults)
            {
                mProteinFileParser.ComputeProteinMass = true;
                mProteinFileParser.ElementMassMode = PeptideSequence.ElementModeConstants.IsotopicMass;

                mProteinFileParser.CreateDigestedProteinOutputFile = false;
                mProteinFileParser.CreateProteinOutputFile = false;

                mProteinFileParser.DelimitedFileFormatCode = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence;

                mProteinFileParser.DigestionOptions.CleavageRuleID = InSilicoDigest.CleavageRuleConstants.ConventionalTrypsin;
                mProteinFileParser.DigestionOptions.MaxMissedCleavages = 1;

                mProteinFileParser.DigestionOptions.MinFragmentResidueCount = 0;
                mProteinFileParser.DigestionOptions.MinFragmentMass = 600;
                mProteinFileParser.DigestionOptions.MaxFragmentMass = 3000;

                mProteinFileParser.DigestionOptions.RemoveDuplicateSequences = true;

                mProteinFileParser.DigestionOptions.IncludePrefixAndSuffixResidues = false;
            }
        }

        private void InitializeThresholdLevels(ref PeakMatching.SearchThresholds[] thresholds, int levels, bool preserveData)
        {
            int index;

            if (levels < 1)
            {
                levels = 1;
            }

            if (preserveData && thresholds.Length > 0)
            {
                Array.Resize(ref thresholds, levels);
            }
            else
            {
                thresholds = new PeakMatching.SearchThresholds[levels];
            }

            for (index = 0; index < levels; index++)
            {
                if (thresholds[index] == null)
                {
                    thresholds[index] = new PeakMatching.SearchThresholds();
                }
                else if (preserveData && index == levels - 1)
                {
                    // Initialize this level to defaults
                    thresholds[index].ResetToDefaults();
                }
            }
        }

        private bool LoadParameterFileSettings(string parameterFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(parameterFilePath))
                {
                    // No parameter file specified; nothing to load
                    return true;
                }

                if (!File.Exists(parameterFilePath))
                {
                    // See if parameterFilePath points to a file in the same directory as the application
                    parameterFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(parameterFilePath));
                    if (!File.Exists(parameterFilePath))
                    {
                        SetBaseClassErrorCode(ProcessFilesErrorCodes.ParameterFileNotFound);
                        return false;
                    }
                }

                mProteinFileParser.LoadParameterFileSettings(parameterFilePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in LoadParameterFileSettings", ex);
            }

            return true;
        }

        private bool LoadProteinsOrPeptides(string proteinInputFilePath)
        {
            var delimitedFileFormatCode = default(DelimitedProteinFileReader.ProteinFileFormatCode);

            bool success;

            try
            {
                mComparisonPeptideInfo = new PeakMatching.PMComparisonFeatureInfo();
                if (mProteinInfo != null)
                {
                    mProteinInfo.SortingList -= ProteinInfo_SortingList;
                    mProteinInfo.SortingMappings -= ProteinInfo_SortingMappings;
                    mProteinInfo = null;
                }

                mProteinInfo = new ProteinCollection();
                mProteinInfo.SortingList += ProteinInfo_SortingList;
                mProteinInfo.SortingMappings += ProteinInfo_SortingMappings;

                // Possibly initialize the ProteinFileParser object
                if (mProteinFileParser == null)
                {
                    InitializeProteinFileParser();
                }

                // Note that ProteinFileParser is exposed as public so that options can be set directly in it

                bool digestionEnabled;
                if (ProteinFileParser.IsFastaFile(proteinInputFilePath) || mProteinFileParser.AssumeFastaFile)
                {
                    LogMessage("Input file format = FASTA");
                    digestionEnabled = true;
                }
                else
                {
                    delimitedFileFormatCode = mProteinFileParser.DelimitedFileFormatCode;
                    LogMessage("Input file format = " + delimitedFileFormatCode);
                    digestionEnabled = DigestSequences;
                }

                switch (delimitedFileFormatCode)
                {
                    case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID:
                    case DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence:
                        // Reading peptides from a delimited file; digestionEnabled is typically False, but could be true
                        break;
                    case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET:
                    case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore:
                    case DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET:
                        // Reading peptides from a delimited file; digestionEnabled is typically False, but could be true
                        break;
                    default:
                        // Force digest Sequences to true
                        digestionEnabled = true;
                        break;
                }

                LogMessage("Digest sequences = " + digestionEnabled);

                if (digestionEnabled)
                {
                    success = LoadProteinsOrPeptidesWork(proteinInputFilePath);
                }
                else
                {
                    // We can assume peptides in the input file are already digested and thus do not need to pre-cache the entire file in memory
                    // Load the data directly using this class instead

                    // Load each peptide using ProteinFileReader.DelimitedFileReader
                    success = LoadPeptidesFromDelimitedFile(proteinInputFilePath);
                    // ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True
                }

                if (success)
                {
                    LogMessage("Loaded " + mComparisonPeptideInfo.Count + " peptides corresponding to " + mProteinInfo.Count + " proteins");
                }
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile, ex);
                success = false;
            }

            return success;
        }

        private bool LoadPeptidesFromDelimitedFile(string proteinInputFilePath)
        {
            // Assumes the peptides in the input file are already digested and that they already have unique ID values generated
            // They could optionally have Mass and NET values defined

            DelimitedProteinFileReader delimitedFileReader = null;

            bool success;

            var inputFileLineSkipCount = default(int);

            try
            {
                delimitedFileReader = new DelimitedProteinFileReader
                {
                    Delimiter = mProteinFileParser.InputFileDelimiter,
                    DelimitedFileFormatCode = mProteinFileParser.DelimitedFileFormatCode
                };

                // Verify that the input file exists
                if (!File.Exists(proteinInputFilePath))
                {
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath);
                    return false;
                }

                // Attempt to open the input file
                if (!delimitedFileReader.OpenFile(proteinInputFilePath))
                {
                    SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile);
                    return false;
                }

                ResetProgress("Parsing digested input file");

                // Read each peptide in the input file and add to mComparisonPeptideInfo

                // Also need to initialize newPeptide
                var newPeptide = new InSilicoDigest.PeptideSequenceWithNET();

                var delimitedFileHasMassAndNET = delimitedFileReader.DelimitedFileFormatCode switch
                {
                    DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET or DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore or DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET => true,
                    _ => false,
                };

                // Always auto compute the NET and Mass in the newPeptide class
                // However, if delimitedFileHasMassAndNET = True and valid Mass and NET values were read from the text file,
                // they are passed to AddOrUpdatePeptide rather than the computed values
                newPeptide.AutoComputeNET = true;
                int inputFileLinesRead = 0;
                var inputPeptideFound = true; // set to true for loop entry

                while (inputPeptideFound)
                {
                    inputPeptideFound = delimitedFileReader.ReadNextProteinEntry();

                    inputFileLinesRead = delimitedFileReader.LinesRead;
                    inputFileLineSkipCount += delimitedFileReader.LineSkipCount;

                    if (!inputPeptideFound)
                    {
                        continue;
                    }

                    newPeptide.SequenceOneLetter = delimitedFileReader.ProteinSequence;

                    if (!delimitedFileHasMassAndNET ||
                        Math.Abs(delimitedFileReader.PeptideMass - 0d) < float.Epsilon &&
                        Math.Abs(delimitedFileReader.PeptideNET - 0f) < float.Epsilon)
                    {
                        AddOrUpdatePeptide(delimitedFileReader.EntryUniqueID, newPeptide.Mass, newPeptide.NET, 0f, 0f, delimitedFileReader.ProteinName, ProteinCollection.CleavageStateConstants.Unknown, string.Empty);
                    }
                    else
                    {
                        AddOrUpdatePeptide(delimitedFileReader.EntryUniqueID, delimitedFileReader.PeptideMass, delimitedFileReader.PeptideNET,
                            delimitedFileReader.PeptideNETStDev, delimitedFileReader.PeptideDiscriminantScore,
                            delimitedFileReader.ProteinName, ProteinCollection.CleavageStateConstants.Unknown, string.Empty);

                        // ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True
                    }

                    UpdateProgress(delimitedFileReader.PercentFileProcessed());
                    if (AbortProcessing)
                    {
                        break;
                    }
                }

                if (inputFileLineSkipCount > 0)
                {
                    var skipMessage = "Note that " + inputFileLineSkipCount + " out of " + inputFileLinesRead + " lines were skipped in the input file because they did not match the column order defined on the File Format Options Tab (" + mProteinFileParser.DelimitedFileFormatCode + ")";
                    LogMessage(skipMessage, MessageTypeConstants.Warning);
                    mLastErrorMessage = string.Copy(skipMessage);
                }

                success = !AbortProcessing;
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile, ex);
                success = false;
            }
            finally
            {
                delimitedFileReader?.CloseFile();
            }

            return success;
        }

        private bool LoadProteinsOrPeptidesWork(string proteinInputFilePath)
        {
            bool success;

            Dictionary<string, int> masterSequences = null;

            try
            {
                bool generateUniqueSequenceID;
                if (mProteinFileParser.GenerateUniqueIDValuesForPeptides)
                {
                    // Need to generate unique sequence ID values
                    generateUniqueSequenceID = true;

                    // Initialize masterSequences
                    masterSequences = new Dictionary<string, int>();
                }
                else
                {
                    generateUniqueSequenceID = false;
                }

                var nextUniqueIDForMasterSeqs = 1;

                var isFastaFile = ProteinFileParser.IsFastaFile(proteinInputFilePath) || mProteinFileParser.AssumeFastaFile;

                // Disable mass calculation
                mProteinFileParser.ComputeProteinMass = false;
                mProteinFileParser.CreateProteinOutputFile = false;

                if (CysPeptidesOnly)
                {
                    mProteinFileParser.DigestionOptions.AminoAcidResidueFilterChars = new[] { 'C' };
                }
                else
                {
                    mProteinFileParser.DigestionOptions.AminoAcidResidueFilterChars = new char[] { };
                }

                // Load the proteins in the input file into memory
                success = mProteinFileParser.ParseProteinFile(proteinInputFilePath, "");

                var skipMessage = string.Empty;
                if (mProteinFileParser.InputFileLineSkipCount > 0 && !isFastaFile)
                {
                    skipMessage = "Note that " + mProteinFileParser.InputFileLineSkipCount +
                                  " out of " + mProteinFileParser.InputFileLinesRead +
                                  " lines were skipped in the input file because they did not match the column order" +
                                  " defined on the File Format Options Tab (" + mProteinFileParser.DelimitedFileFormatCode + ")";
                    LogMessage(skipMessage, MessageTypeConstants.Warning);
                    mLastErrorMessage = string.Copy(skipMessage);
                }

                if (mProteinFileParser.GetProteinCountCached() <= 0)
                {
                    if (success)
                    {
                        // File was successfully loaded, but no proteins were found
                        // This could easily happen for delimited files that only have a header line, or that don't have a format that matches what the user specified
                        SetLocalErrorCode(ErrorCodes.ProteinsNotFoundInInputFile);
                        mLastErrorMessage = string.Empty;
                        success = false;
                    }
                    else
                    {
                        SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile);
                        mLastErrorMessage = "Error using ParseProteinFile to read the proteins from " + Path.GetFileName(proteinInputFilePath);
                    }

                    if (mProteinFileParser.InputFileLineSkipCount > 0 && !isFastaFile)
                    {
                        if (mLastErrorMessage.Length > 0)
                        {
                            mLastErrorMessage += ". ";
                        }

                        mLastErrorMessage += skipMessage;
                    }
                }
                else if (success)
                {
                    // Re-enable mass calculation
                    mProteinFileParser.ComputeProteinMass = true;

                    ResetProgress("Digesting proteins in input file");

                    for (var proteinIndex = 0; proteinIndex < mProteinFileParser.GetProteinCountCached(); proteinIndex++)
                    {
                        var proteinOrPeptide = mProteinFileParser.GetCachedProtein(proteinIndex);

                        var peptideCount = mProteinFileParser.GetDigestedPeptidesForCachedProtein(proteinIndex, out var digestedPeptides, mProteinFileParser.DigestionOptions);

                        if (peptideCount > 0)
                        {
                            foreach (var digestedPeptide in digestedPeptides)
                            {
                                int uniqueSeqID;

                                if (generateUniqueSequenceID)
                                {
                                    uniqueSeqID = GetNextUniqueSequenceID(digestedPeptide.SequenceOneLetter, masterSequences, ref nextUniqueIDForMasterSeqs);
                                }
                                else
                                {
                                    // Must assume each sequence is unique; probably an incorrect assumption
                                    uniqueSeqID = nextUniqueIDForMasterSeqs;
                                    nextUniqueIDForMasterSeqs++;
                                }

                                AddOrUpdatePeptide(uniqueSeqID,
                                                   digestedPeptide.Mass, digestedPeptide.NET, 0, 0,
                                                   proteinOrPeptide.Name,
                                                   ProteinCollection.CleavageStateConstants.Unknown,
                                                   digestedPeptide.PeptideName);
                            }
                        }

                        UpdateProgress((float)((proteinIndex + 1) / (double)mProteinFileParser.GetProteinCountCached() * 100d));

                        if (AbortProcessing)
                        {
                            break;
                        }
                    }

                    success = !AbortProcessing;
                }
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ErrorCodes.ErrorReadingInputFile, ex);
                success = false;
            }

            return success;
        }

        private void InitializeBinningRanges(
            PeakMatching.PMFeatureInfo featuresToIdentify,
            PeakMatching.PMFeatureMatchResults peptideMatchResults,
            BinnedPeptideCountStats peptideStatsBinned)
        {
            var currentFeatureID = default(int);

            var matchResultInfo = default(PeakMatching.PMFeatureMatchResults.PeakMatchingResult);

            if (peptideStatsBinned.Settings.AutoDetermineMassRange)
            {
                // Examine peptideMatchResults to determine the minimum and maximum masses for features with matches

                // First, set the ranges to out-of-range values
                peptideStatsBinned.Settings.MassMinimum = (float)double.MaxValue;
                peptideStatsBinned.Settings.MassMaximum = (float)double.MinValue;

                // Now examine .PeptideMatchResults()
                int matchIndex;
                for (matchIndex = 0; matchIndex < peptideMatchResults.Count; matchIndex++)
                {
                    peptideMatchResults.GetMatchInfoByRowIndex(matchIndex, ref currentFeatureID, ref matchResultInfo);

                    featuresToIdentify.GetFeatureInfoByFeatureID(currentFeatureID, out var featureInfo);
                    var featureMass = (float)featureInfo.Mass;

                    if (featureMass > peptideStatsBinned.Settings.MassMaximum)
                    {
                        peptideStatsBinned.Settings.MassMaximum = featureMass;
                    }

                    if (featureMass < peptideStatsBinned.Settings.MassMinimum)
                    {
                        peptideStatsBinned.Settings.MassMinimum = featureMass;
                    }
                }

                // Round the minimum and maximum masses to the nearest 100

                if (Math.Abs(peptideStatsBinned.Settings.MassMinimum - double.MaxValue) < double.Epsilon ||
                    Math.Abs(peptideStatsBinned.Settings.MassMaximum - double.MinValue) < double.Epsilon)
                {
                    // No matches were found; set these to defaults
                    peptideStatsBinned.Settings.MassMinimum = 400f;
                    peptideStatsBinned.Settings.MassMaximum = 6000f;
                }

                var IntegerMass = (int)Math.Round(Math.Round(peptideStatsBinned.Settings.MassMinimum, 0));
                Math.DivRem(IntegerMass, 100, out var remainder);
                IntegerMass -= remainder;

                peptideStatsBinned.Settings.MassMinimum = IntegerMass;

                IntegerMass = (int)Math.Round(Math.Round(peptideStatsBinned.Settings.MassMaximum, 0));
                Math.DivRem(IntegerMass, 100, out remainder);
                IntegerMass += 100 - remainder;

                peptideStatsBinned.Settings.MassMaximum = IntegerMass;
            }

            // Determine BinCount; do not allow more than 1,000,000 bins
            if (Math.Abs(peptideStatsBinned.Settings.MassBinSizeDa - 0f) < 0.00001d)
            {
                peptideStatsBinned.Settings.MassBinSizeDa = 1f;
            }

            peptideStatsBinned.BinCount = 1000000000;
            while (peptideStatsBinned.BinCount > 1000000)
            {
                try
                {
                    peptideStatsBinned.BinCount = (int)Math.Round(Math.Ceiling(peptideStatsBinned.Settings.MassMaximum - peptideStatsBinned.Settings.MassMinimum) / peptideStatsBinned.Settings.MassBinSizeDa);
                    if (peptideStatsBinned.BinCount > 1000000)
                    {
                        peptideStatsBinned.Settings.MassBinSizeDa *= 10f;
                    }
                }
                catch
                {
                    peptideStatsBinned.BinCount = 1000000000;
                }
            }
        }

        private int MassToBinIndex(double ThisMass, double StartMass, double MassResolution)
        {
            // First subtract StartMass from ThisMass
            // For example, if StartMass is 500 and ThisMass is 500.28, then WorkingMass = 0.28
            // Or, if StartMass is 500 and ThisMass is 530.83, then WorkingMass = 30.83
            var WorkingMass = ThisMass - StartMass;

            // Now, dividing WorkingMass by MassResolution and rounding to the nearest integer
            // actually gives the bin
            // For example, given WorkingMass = 0.28 and MassResolution = 0.1, Bin = CInt(2.8) = 3
            // Or, given WorkingMass = 30.83 and MassResolution = 0.1, Bin = CInt(308.3) = 308
            return (int)Math.Round(Math.Floor(WorkingMass / MassResolution));
        }

        // Main processing function
        public override bool ProcessFile(string inputFilePath, string outputFolderPath, string parameterFilePath, bool resetErrorCode)
        {
            // Returns True if success, False if failure

            var success = default(bool);

            if (resetErrorCode)
            {
                SetLocalErrorCode(ErrorCodes.NoError);
            }

            if (!LoadParameterFileSettings(parameterFilePath))
            {
                var statusMessage = "Parameter file load error: " + parameterFilePath;
                ShowErrorMessage(statusMessage);
                if (ErrorCode == ProcessFilesErrorCodes.NoError)
                {
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidParameterFile);
                }

                return false;
            }

            try
            {
                if (string.IsNullOrEmpty(inputFilePath))
                {
                    Console.WriteLine("Input file name is empty");
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Parsing " + Path.GetFileName(inputFilePath));

                    if (!CleanupFilePaths(ref inputFilePath, ref outputFolderPath))
                    {
                        SetBaseClassErrorCode(ProcessFilesErrorCodes.FilePathError);
                    }
                    else
                    {
                        try
                        {
                            // Obtain the full path to the input file
                            var file = new FileInfo(inputFilePath);
                            var inputFilePathFull = file.FullName;

                            success = GenerateUniquenessStats(inputFilePathFull, outputFolderPath, Path.GetFileNameWithoutExtension(inputFilePath));
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error calling ParseProteinFile", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ProcessFile", ex);
            }

            return success;
        }

        protected new void ResetProgress()
        {
            AbortProcessing = false;
            base.ResetProgress();
        }

        protected new void ResetProgress(string description)
        {
            AbortProcessing = false;
            base.ResetProgress(description);
        }

        public bool SetPeptideUniquenessMassRangeForBinning(float massMinimum, float massMaximum)
        {
            // Returns True if the minimum and maximum mass specified were valid

            if (massMinimum < massMaximum && massMinimum >= 0f)
            {
                mPeptideUniquenessBinningSettings.MassMinimum = massMinimum;
                mPeptideUniquenessBinningSettings.MassMaximum = massMaximum;
                return true;
            }

            return false;
        }

        private bool SummarizeResultsByPeptide(
            PeakMatching.SearchThresholds thresholds,
            int thresholdIndex,
            PeakMatching.PMFeatureInfo featuresToIdentify,
            PeakMatching.PMFeatureMatchResults peptideMatchResults,
            string outputFolderPath,
            string outputFilenameBase,
            StreamWriter peptideUniquenessWriter)
        {
            var matchCount = default(int);

            bool success;

            try
            {
                // Copy the binning settings
                var peptideStatsBinned = new BinnedPeptideCountStats(mPeptideUniquenessBinningSettings.Clone());

                // Define the minimum and maximum mass ranges, plus the number of bins required
                InitializeBinningRanges(featuresToIdentify, peptideMatchResults, peptideStatsBinned);

                var featuresToIdentifyCount = featuresToIdentify.Count;

                ResetProgress("Summarizing results by peptide");

                // --------------------------------------
                // Compute the stats for thresholds
                // --------------------------------------

                // Define the maximum match count that will be tracked
                var maxMatchCount = Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave);

                // Reserve memory for the bins, store the bin ranges for each bin, and reset the ResultIDs arrays
                InitializeBinnedStats(peptideStatsBinned, peptideStatsBinned.BinCount);

                // ToDo: When using Sql Server, switch this to use a SP that performs the same function as this For Loop, sorting the results in a table in the DB, but using Bulk Update queries

                LogMessage("SummarizeResultsByPeptide starting, total feature count = " + featuresToIdentifyCount);

                var peptideSkipCount = 0;
                int binIndex;
                int peptideIndex;
                for (peptideIndex = 0; peptideIndex < featuresToIdentifyCount; peptideIndex++)
                {
                    if (featuresToIdentify.GetFeatureInfoByRowIndex(peptideIndex, out var featureInfo))
                    {
                        binIndex = MassToBinIndex(featureInfo.Mass, peptideStatsBinned.Settings.MassMinimum, peptideStatsBinned.Settings.MassBinSizeDa);

                        if (binIndex < 0 || binIndex > peptideStatsBinned.BinCount - 1)
                        {
                            // Peptide mass is out-of-range, ignore the result
                            peptideSkipCount++;
                        }
                        else if (FeatureContainsUniqueMatch(featureInfo, peptideMatchResults, ref matchCount, UseSLiCScoreForUniqueness, peptideStatsBinned.Settings.MinimumSLiCScore))
                        {
                            peptideStatsBinned.Bins[binIndex].UniqueResultIDCount++;
                            peptideStatsBinned.Bins[binIndex].ResultIDCountDistribution[1]++;
                        }
                        else if (matchCount > 0)
                        {
                            // Feature has 1 or more matches, but they're not unique
                            peptideStatsBinned.Bins[binIndex].NonUniqueResultIDCount++;
                            if (matchCount < maxMatchCount)
                            {
                                peptideStatsBinned.Bins[binIndex].ResultIDCountDistribution[matchCount]++;
                            }
                            else
                            {
                                peptideStatsBinned.Bins[binIndex].ResultIDCountDistribution[maxMatchCount]++;
                            }
                        }
                    }

                    if (peptideIndex % 100 == 0)
                    {
                        UpdateProgress((float)(peptideIndex / (double)featuresToIdentifyCount * 100d));
                        if (AbortProcessing)
                        {
                            break;
                        }
                    }

                    if (peptideIndex % 100000 == 0 && peptideIndex > 0)
                    {
                        LogMessage("SummarizeResultsByPeptide, peptideIndex = " + peptideIndex);
                    }
                }

                for (binIndex = 0; binIndex < peptideStatsBinned.BinCount; binIndex++)
                {
                    var total = peptideStatsBinned.Bins[binIndex].UniqueResultIDCount + peptideStatsBinned.Bins[binIndex].NonUniqueResultIDCount;
                    if (total > 0)
                    {
                        peptideStatsBinned.Bins[binIndex].PercentUnique = (float)(peptideStatsBinned.Bins[binIndex].UniqueResultIDCount / (double)total * 100d);
                    }
                    else
                    {
                        peptideStatsBinned.Bins[binIndex].PercentUnique = 0f;
                    }
                }

                if (peptideSkipCount > 0)
                {
                    LogMessage("Skipped " + peptideSkipCount + " peptides since their masses were outside the defined bin range", MessageTypeConstants.Warning);
                }

                // Write out the peptide results for this threshold level
                if (CreateSeparateOutputFileForEachThreshold)
                {
                    var workingFilenameBase = outputFilenameBase + "_PeptideStatsBinned" + (thresholdIndex + 1) + ".txt";
                    var outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);

                    peptideUniquenessWriter = new StreamWriter(outputFilePath);

                    ExportThresholds(peptideUniquenessWriter, thresholdIndex, thresholds);
                    peptideUniquenessWriter.WriteLine();

                    SummarizeResultsByPeptideWriteHeaders(peptideUniquenessWriter, CreateSeparateOutputFileForEachThreshold);
                }

                success = ExportPeptideUniquenessResults(thresholdIndex, peptideStatsBinned, peptideUniquenessWriter);

                if (CreateSeparateOutputFileForEachThreshold)
                {
                    peptideUniquenessWriter.Close();
                }

                LogMessage("SummarizeResultsByPeptide complete");

                UpdateProgress(100f);
            }
            catch
            {
                success = false;
            }

            return success;
        }

        private void SummarizeResultsByPeptideWriteHeaders(TextWriter peptideUniquenessWriter, bool createSeparateOutputFiles)
        {
            string lineOut;
            int index;

            // Write the column headers
            if (createSeparateOutputFiles)
            {
                lineOut = string.Empty;
            }
            else
            {
                lineOut = "Threshold_Index" + mOutputFileDelimiter;
            }

            lineOut += "Bin_Start_Mass" + mOutputFileDelimiter +
                       "Percent_Unique" + mOutputFileDelimiter +
                       "Peptide_Count_Total";

            for (index = 1; index < Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave); index++)
            {
                lineOut += mOutputFileDelimiter + "MatchCount_" + index;
            }

            lineOut += mOutputFileDelimiter + "MatchCount_" + Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave) + "_OrMore";

            peptideUniquenessWriter.WriteLine(lineOut);
            peptideUniquenessWriter.Flush();
        }

        private bool SummarizeResultsByProtein(
            PeakMatching.SearchThresholds thresholds,
            int thresholdIndex,
            PeakMatching.PMFeatureInfo featuresToIdentify,
            PeakMatching.PMFeatureMatchResults peptideMatchResults,
            string outputFolderPath, string outputFilenameBase,
            StreamWriter proteinStatsWriter)
        {
            var matchCount = default(int);

            bool success;

            try
            {
                var featuresToIdentifyCount = featuresToIdentify.Count;
                ResetProgress("Summarizing results by protein");

                // Compute the stats for thresholds

                // Compute number of unique peptides seen for each protein and write out
                // This will allow one to plot Unique peptide vs protein mass or total peptide count vs. protein mass

                // Initialize mProteinToIdentifiedPeptideMappingTable
                mProteinToIdentifiedPeptideMappingTable.Clear();

                LogMessage("SummarizeResultsByProtein starting, total feature count = " + featuresToIdentifyCount);

                int peptideIndex;
                for (peptideIndex = 0; peptideIndex < featuresToIdentifyCount; peptideIndex++)
                {
                    if (featuresToIdentify.GetFeatureInfoByRowIndex(peptideIndex, out var featureInfo))
                    {
                        if (FeatureContainsUniqueMatch(featureInfo, peptideMatchResults, ref matchCount, UseSLiCScoreForUniqueness, mPeptideUniquenessBinningSettings.MinimumSLiCScore))
                        {
                            var proteinIDs = mProteinInfo.GetProteinIDsMappedToPeptideID(featureInfo.FeatureID);
                            int proteinIndex;

                            for (proteinIndex = 0; proteinIndex < proteinIDs.Length; proteinIndex++)
                            {
                                if (!mProteinToIdentifiedPeptideMappingTable.Rows.Contains(new object[] { proteinIDs[proteinIndex], featureInfo.FeatureID }))
                                {
                                    var newDataRow = mProteinToIdentifiedPeptideMappingTable.NewRow();
                                    newDataRow[PROTEIN_ID_COLUMN] = proteinIDs[proteinIndex];
                                    newDataRow[PEPTIDE_ID_MATCH_COLUMN] = featureInfo.FeatureID;
                                    mProteinToIdentifiedPeptideMappingTable.Rows.Add(newDataRow);
                                }
                            }
                        }
                    }

                    if (peptideIndex % 100 == 0)
                    {
                        UpdateProgress((float)(peptideIndex / (double)featuresToIdentifyCount * 100d));
                        if (AbortProcessing)
                        {
                            break;
                        }
                    }

                    if (peptideIndex % 100000 == 0 && peptideIndex > 0)
                    {
                        LogMessage("SummarizeResultsByProtein, peptideIndex = " + peptideIndex);
                    }
                }

                // Write out the protein results for this threshold level
                if (CreateSeparateOutputFileForEachThreshold)
                {
                    var workingFilenameBase = outputFilenameBase + "_ProteinStatsBinned" + (thresholdIndex + 1) + ".txt";
                    var outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);

                    proteinStatsWriter = new StreamWriter(outputFilePath);

                    ExportThresholds(proteinStatsWriter, thresholdIndex, thresholds);
                    proteinStatsWriter.WriteLine();

                    SummarizeResultsByProteinWriteHeaders(proteinStatsWriter, CreateSeparateOutputFileForEachThreshold);
                }

                success = ExportProteinStats(thresholdIndex, proteinStatsWriter);

                if (CreateSeparateOutputFileForEachThreshold)
                {
                    proteinStatsWriter.Close();
                }

                UpdateProgress(100f);

                LogMessage("SummarizeResultsByProtein complete");
            }
            catch
            {
                success = false;
            }

            return success;
        }

        private void SummarizeResultsByProteinWriteHeaders(TextWriter proteinStatsWriter, bool createSeparateOutputFiles)
        {
            string lineOut;

            // Write the column headers
            if (createSeparateOutputFiles)
            {
                lineOut = string.Empty;
            }
            else
            {
                lineOut = "Threshold_Index" + mOutputFileDelimiter;
            }

            lineOut += "Protein_Name" +
                       mOutputFileDelimiter + "Protein_ID" +
                       mOutputFileDelimiter + "Peptide_Count_Total" +
                       mOutputFileDelimiter + "Peptide_Count_Uniquely_Identifiable";
            proteinStatsWriter.WriteLine(lineOut);
            proteinStatsWriter.Flush();
        }

        private void SetLocalErrorCode(ErrorCodes newErrorCode, Exception ex)
        {
            SetLocalErrorCode(newErrorCode);

            LogMessage(newErrorCode + ": " + ex.Message, MessageTypeConstants.ErrorMsg);
            mLastErrorMessage = ex.Message;
        }

        private void SetLocalErrorCode(ErrorCodes newErrorCode, bool leaveExistingErrorCodeUnchanged = false)
        {
            if (leaveExistingErrorCodeUnchanged && LocalErrorCode != ErrorCodes.NoError)
            {
                // An error code is already defined; do not change it
            }
            else
            {
                LocalErrorCode = newErrorCode;
                mLastErrorMessage = string.Empty;
            }
        }

        protected void UpdateSubtaskProgress(string description)
        {
            UpdateProgress(description, mProgressPercentComplete);
        }

        protected void UpdateSubtaskProgress(float percentComplete)
        {
            UpdateProgress(ProgressStepDescription, percentComplete);
        }

        protected void UpdateSubtaskProgress(string description, float percentComplete)
        {
            var descriptionChanged = (description ?? "") != (mSubtaskProgressStepDescription ?? "");

            mSubtaskProgressStepDescription = string.Copy(description);
            if (percentComplete < 0f)
            {
                percentComplete = 0f;
            }
            else if (percentComplete > 100f)
            {
                percentComplete = 100f;
            }

            mSubtaskProgressPercentComplete = percentComplete;

            if (descriptionChanged)
            {
                if (Math.Abs(mSubtaskProgressPercentComplete - 0f) < float.Epsilon)
                {
                    LogMessage(mSubtaskProgressStepDescription.Replace(Environment.NewLine, "; "));
                }
                else
                {
                    LogMessage(mSubtaskProgressStepDescription + " (" + mSubtaskProgressPercentComplete.ToString("0.0") + "% complete)".Replace(Environment.NewLine, "; "));
                }
            }

            SubtaskProgressChanged?.Invoke(description, ProgressPercentComplete);
        }

        private int mSortingListCount = 0;
        private DateTime mSortingListLastPostTime = DateTime.UtcNow;

        private void ProteinInfo_SortingList()
        {
            mSortingListCount++;
            if (DateTime.UtcNow.Subtract(mSortingListLastPostTime).TotalSeconds >= 10d)
            {
                LogMessage("Sorting protein list (SortCount = " + mSortingListCount + ")");
                mSortingListLastPostTime = DateTime.UtcNow;
            }
        }

        private int mSortingMappingsCount = 0;
        private DateTime mSortingMappingsLastPostTime = DateTime.UtcNow;

        private void ProteinInfo_SortingMappings()
        {
            mSortingMappingsCount++;
            if (DateTime.UtcNow.Subtract(mSortingMappingsLastPostTime).TotalSeconds >= 10d)
            {
                LogMessage("Sorting protein to peptide mapping info (SortCount = " + mSortingMappingsCount + ")");
                mSortingMappingsLastPostTime = DateTime.UtcNow;
            }
        }

        private int mResultsSortingListCount = 0;
        private DateTime mResultsSortingListLastPostTime = DateTime.UtcNow;

        private void PeptideMatchResults_SortingList()
        {
            mResultsSortingListCount++;
            if (DateTime.UtcNow.Subtract(mResultsSortingListLastPostTime).TotalSeconds >= 10d)
            {
                LogMessage("Sorting peptide match results list (SortCount = " + mResultsSortingListCount + ")");
                mResultsSortingListLastPostTime = DateTime.UtcNow;
            }
        }

        private void PeakMatching_LogEvent(string Message, PeakMatching.MessageTypeConstants EventType)
        {
            switch (EventType)
            {
                case PeakMatching.MessageTypeConstants.Normal:
                    LogMessage(Message);
                    break;
                case PeakMatching.MessageTypeConstants.Warning:
                    LogMessage(Message, MessageTypeConstants.Warning);
                    break;
                case PeakMatching.MessageTypeConstants.ErrorMsg:
                    LogMessage(Message, MessageTypeConstants.ErrorMsg);
                    break;
                case PeakMatching.MessageTypeConstants.Health:
                    // Don't log this type of message
                    break;
            }
        }

        private void PeakMatching_ProgressContinues(string taskDescription, float percentComplete)
        {
            UpdateSubtaskProgress(percentComplete);
        }

        private void ProteinFileParser_ErrorEvent(string message, Exception ex)
        {
            ShowErrorMessage("Error in mProteinFileParser: " + message);
        }
    }
}
