using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004
//
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://omics.pnl.gov/ or https://www.pnnl.gov/sysbio/ or https://panomics.pnnl.gov/
// -------------------------------------------------------------------------------
//
// Licensed under the 2-Clause BSD License; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// https://opensource.org/licenses/BSD-2-Clause
//
// Copyright 2018 Battelle Memorial Institute

using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using DBUtils = PRISMDatabaseUtils.DataTableUtils;
using ProteinFileReader;

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
            mFileDate = "August 6, 2021";
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
            public float PercentUnique { get; set; }              // UniqueResultIDs().length / (UniqueResultIDs().length + NonUniqueResultIDs().length)

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
                return (MassBinningOptions) MemberwiseClone();
            }
        }

        private class BinnedPeptideCountStats
        {
            public MassBinningOptions Settings { get; set; }
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
        private readonly MassBinningOptions mPeptideUniquenessBinningSettings = new MassBinningOptions();
        private ErrorCodes mLocalErrorCode;
        private string mLastErrorMessage;
        private PeakMatching mPeakMatching; // TODO: This should just be a local variable (fix ProgressContinues event)

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

        /// <summary>
        ///
        /// </summary>
        /// <param name="taskDescription"></param>
        /// <param name="percentComplete">Ranges from 0 to 100, but can contain decimal percentage values</param>
        public event SubtaskProgressChangedEventHandler SubtaskProgressChanged;

        public delegate void SubtaskProgressChangedEventHandler(string taskDescription, float percentComplete);

        public bool AutoDetermineMassRangeForBinning
        {
            get
            {
                return mPeptideUniquenessBinningSettings.AutoDetermineMassRange;
            }

            set
            {
                mPeptideUniquenessBinningSettings.AutoDetermineMassRange = value;
            }
        }

        public bool CreateSeparateOutputFileForEachThreshold { get; set; }
        public bool CysPeptidesOnly { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
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
                else
                {
                    return mProteinFileParser.ElementMassMode;
                }
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

        public ErrorCodes LocalErrorCode
        {
            get
            {
                return mLocalErrorCode;
            }
        }

        public int MaxPeakMatchingResultsPerFeatureToSave
        {
            get
            {
                return mMaxPeakMatchingResultsPerFeatureToSave;
            }

            set
            {
                if (value < 1)
                    value = 1;
                mMaxPeakMatchingResultsPerFeatureToSave = value;
            }
        }

        public float MinimumSLiCScoreToBeConsideredUnique
        {
            get
            {
                return mPeptideUniquenessBinningSettings.MinimumSLiCScore;
            }

            set
            {
                mPeptideUniquenessBinningSettings.MinimumSLiCScore = value;
            }
        }

        public float PeptideUniquenessMassBinSizeForBinning
        {
            get
            {
                return mPeptideUniquenessBinningSettings.MassBinSizeDa;
            }

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
            get
            {
                return mOutputFileDelimiter;
            }

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
        /// <returns></returns>
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
            AddSearchThresholdLevel(massToleranceType, massTolerance, netTolerance, true, 0d, 0d, true, clearExistingThresholds, PeakMatching.DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER);
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

            int index = mThresholdLevels.Length - 1;
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
        /// <param name="uniqueSeqID"></param>
        /// <param name="peptideMass"></param>
        /// <param name="peptideNET"></param>
        /// <param name="peptideNETStDev"></param>
        /// <param name="peptideDiscriminantScore"></param>
        /// <param name="proteinName"></param>
        /// <param name="cleavageStateInProtein"></param>
        /// <param name="peptideName"></param>
        /// <remarks>
        /// Assures that the peptide is present in mComparisonPeptideInfo and that the protein and protein/peptide mapping is present in mProteinInfo
        /// Assumes that uniqueSeqID is truly unique for the given peptide
        /// </remarks>
        private void AddOrUpdatePeptide(int uniqueSeqID, double peptideMass, float peptideNET, float peptideNETStDev, float peptideDiscriminantScore, string proteinName, ProteinCollection.CleavageStateConstants cleavageStateInProtein, string peptideName)
        {
            int proteinID;
            mComparisonPeptideInfo.Add(uniqueSeqID, peptideName, peptideMass, peptideNET, peptideNETStDev, peptideDiscriminantScore);

            // Determine the ProteinID for proteinName
            if (proteinName != null && proteinName.Length > 0)
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
            var proteinID = default(int);

            // Note: proteinID is auto-assigned
            if (!mProteinInfo.Add(proteinName, ref proteinID))
            {
                proteinID = -1;
            }

            return proteinID;
        }

        private bool ExportPeakMatchingResults(PeakMatching.SearchThresholds thresholds, int thresholdIndex, int comparisonFeatureCount, PeakMatching.PMFeatureMatchResults peptideMatchResults, string outputFolderPath, string outputFilenameBase, StreamWriter pmResultsWriter)
        {
            string workingFilenameBase;
            string outputFilePath;
            string linePrefix;
            string lineOut;
            int matchIndex;
            int cachedMatchCount;
            int cachedMatchCountFeatureID;
            var currentFeatureID = default(int);
            var matchResultInfo = default(PeakMatching.PMFeatureMatchResults.PeakMatchingResult);
            DateTime lastFlushTime;
            bool success;
            try
            {
                ResetProgress("Exporting peak matching results");
                if (CreateSeparateOutputFileForEachThreshold)
                {
                    // Create one file for each search threshold level
                    workingFilenameBase = outputFilenameBase + "_PMResults" + (thresholdIndex + 1).ToString() + ".txt";
                    outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);
                    pmResultsWriter = new StreamWriter(outputFilePath);

                    // Write the threshold values to the output file
                    pmResultsWriter.WriteLine("Comparison feature count: " + comparisonFeatureCount.ToString());
                    ExportThresholds(pmResultsWriter, thresholdIndex, thresholds);
                    pmResultsWriter.WriteLine();
                    ExportPeakMatchingResultsWriteHeaders(pmResultsWriter, CreateSeparateOutputFileForEachThreshold);
                }

                cachedMatchCount = 0;
                cachedMatchCountFeatureID = -1;
                lastFlushTime = DateTime.UtcNow;

                // ToDo: Grab chunks of data from the server if caching into SqlServer (change this to a while loop)
                for (matchIndex = 0; matchIndex < peptideMatchResults.Count; matchIndex++)
                {
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
                            cachedMatchCount = peptideMatchResults.get_MatchCountForFeatureID(currentFeatureID);
                            cachedMatchCountFeatureID = currentFeatureID;
                        }

                        lineOut = linePrefix + currentFeatureID.ToString() + mOutputFileDelimiter + cachedMatchCount.ToString() + mOutputFileDelimiter + matchResultInfo.MultiAMTHitCount.ToString() + mOutputFileDelimiter + matchResultInfo.MatchingID.ToString() + mOutputFileDelimiter + Math.Round(matchResultInfo.MassErr, 6).ToString() + mOutputFileDelimiter + Math.Round(matchResultInfo.NETErr, 4).ToString() + mOutputFileDelimiter + Math.Round(matchResultInfo.SLiCScore, 4).ToString() + mOutputFileDelimiter + Math.Round(matchResultInfo.DelSLiC, 4).ToString();
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
            catch (Exception ex)
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

            lineOut += "Unique_Sequence_ID" + mOutputFileDelimiter + "Match_Count" + mOutputFileDelimiter + "Multi_AMT_Hit_Count" + mOutputFileDelimiter + "Matching_ID_Index" + mOutputFileDelimiter + "Mass_Err";
            lineOut += mOutputFileDelimiter + "NET_Err";
            lineOut += mOutputFileDelimiter + "SLiC_Score" + mOutputFileDelimiter + "Del_SLiC";
            pmResultsWriter.WriteLine(lineOut);
        }

        private bool ExportPeptideUniquenessResults(int thresholdIndex, BinnedPeptideCountStats binResults, TextWriter peptideUniquenessWriter)
        {
            string lineOut;
            int binIndex;
            int index;
            bool success;
            try
            {
                for (binIndex = 0; binIndex < binResults.BinCount; binIndex++)
                {
                    if (CreateSeparateOutputFileForEachThreshold)
                    {
                        lineOut = string.Empty;
                    }
                    else
                    {
                        lineOut = (thresholdIndex + 1).ToString() + mOutputFileDelimiter;
                    }

                    int peptideCountTotal = binResults.Bins[binIndex].NonUniqueResultIDCount + binResults.Bins[binIndex].UniqueResultIDCount;
                    lineOut += Math.Round(binResults.Bins[binIndex].MassBinStart, 2).ToString() + mOutputFileDelimiter + Math.Round(binResults.Bins[binIndex].PercentUnique, 3).ToString() + mOutputFileDelimiter + peptideCountTotal.ToString();
                    for (index = 1; index <= Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave); index++)
                        lineOut += mOutputFileDelimiter + binResults.Bins[binIndex].ResultIDCountDistribution[index].ToString();
                    peptideUniquenessWriter.WriteLine(lineOut);
                }

                peptideUniquenessWriter.Flush();
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }

        private bool ExportProteinStats(int thresholdIndex, TextWriter proteinStatsWriter)
        {
            try
            {
                var lastFlushTime = DateTime.UtcNow;
                for (int proteinIndex = 0; proteinIndex < mProteinInfo.Count; proteinIndex++)
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

                    int proteinID;
                    string proteinName = string.Empty;
                    if (mProteinInfo.GetProteinInfoByRowIndex(proteinIndex, out proteinID, out proteinName))
                    {
                        lineOut += proteinName + mOutputFileDelimiter + proteinID.ToString() + mOutputFileDelimiter + mProteinInfo.GetPeptideCountForProteinByID(proteinID).ToString() + mOutputFileDelimiter + GetPeptidesUniquelyIdentifiedCountByProteinID(proteinID).ToString();
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
            catch (Exception ex)
            {
                return false;
            }
        }

        private string ExtractServerFromConnectionString(string connectionString)
        {
            const string DATA_SOURCE_TEXT = "data source";
            int charLoc, charLoc2;
            string serverName = string.Empty;
            try
            {
                charLoc = connectionString.IndexOf(DATA_SOURCE_TEXT, StringComparison.OrdinalIgnoreCase);
                if (charLoc >= 0)
                {
                    charLoc += DATA_SOURCE_TEXT.Length;
                    charLoc2 = connectionString.ToLower().IndexOf(';', charLoc + 1);
                    if (charLoc2 <= charLoc)
                    {
                        charLoc2 = connectionString.Length - 1;
                    }

                    serverName = connectionString.Substring(charLoc + 1, charLoc2 - charLoc - 1);
                }
            }
            catch (Exception ex)
            {
            }

            return serverName;
        }

        private bool FeatureContainsUniqueMatch(PeakMatching.FeatureInfo featureInfo, PeakMatching.PMFeatureMatchResults peptideMatchResults, ref int matchCount, bool usingSLiCScoreForUniqueness, float minimumSLiCScore)
        {
            bool uniqueMatch;
            PeakMatching.PMFeatureMatchResults.PeakMatchingResult[] matchResults = null;
            int matchIndex;
            uniqueMatch = false;
            if (peptideMatchResults.GetMatchInfoByFeatureID(featureInfo.FeatureID, ref matchResults, ref matchCount))
            {
                if (matchCount > 0)
                {
                    // The current feature has 1 or more matches

                    if (usingSLiCScoreForUniqueness)
                    {
                        // See if any of the matches have a SLiC Score >= the minimum SLiC Score
                        uniqueMatch = false;
                        for (matchIndex = 0; matchIndex < matchCount; matchIndex++)
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
                    else
                    {
                        uniqueMatch = false;
                    }
                }
            }
            else
            {
                matchCount = 0;
            }

            return uniqueMatch;
        }

        private void ExportThresholds(TextWriter writer, int thresholdIndex, PeakMatching.SearchThresholds searchThresholds)
        {
            string delimiter;
            string lineOut;
            delimiter = "; ";

            // Write the thresholds
            lineOut = "Threshold Index: " + (thresholdIndex + 1).ToString();
            lineOut += delimiter + "Mass Tolerance: +- ";
            switch (searchThresholds.MassTolType)
            {
                case PeakMatching.SearchThresholds.MassToleranceConstants.Absolute:
                    lineOut += Math.Round(searchThresholds.MassTolerance, 5).ToString() + " Da";
                    break;
                case PeakMatching.SearchThresholds.MassToleranceConstants.PPM:
                    lineOut += Math.Round(searchThresholds.MassTolerance, 2).ToString() + " ppm";
                    break;
                default:
                    lineOut += "Unknown mass tolerance mode";
                    break;
            }

            lineOut += delimiter + "NET Tolerance: +- " + Math.Round(searchThresholds.NETTolerance, 4).ToString();
            if (UseSLiCScoreForUniqueness)
            {
                lineOut += delimiter + "Minimum SLiC Score: " + Math.Round(mPeptideUniquenessBinningSettings.MinimumSLiCScore, 3).ToString() + "; " + "Max search distance multiplier: " + Math.Round(searchThresholds.SLiCScoreMaxSearchDistanceMultiplier, 1).ToString();
            }
            else if (UseEllipseSearchRegion)
            {
                lineOut += delimiter + "Minimum SLiC Score: N/A; using ellipse to find matching features";
            }
            else
            {
                lineOut += delimiter + "Minimum SLiC Score: N/A; using rectangle to find matching features";
            }

            writer.WriteLine(lineOut);
        }

        public bool GenerateUniquenessStats(string proteinInputFilePath, string outputFolderPath, string outputFilenameBase)
        {
            var progressStepCount = default(int);
            var progressStep = default(int);
            int thresholdIndex;
            PeakMatching.PMFeatureInfo featuresToIdentify;
            bool success;
            bool searchAborted = false;
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
                LogMessage("Caching data in memory", MessageTypeConstants.Normal);
                LogMessage("Caching peak matching results in memory", MessageTypeConstants.Normal);

                // ----------------------------------------------------
                // Load the proteins and digest them, or simply load the peptides
                // ----------------------------------------------------
                LogMessage("Load proteins or peptides from " + Path.GetFileName(proteinInputFilePath), MessageTypeConstants.Normal);
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
                    featuresToIdentify = mComparisonPeptideInfo;
                    try
                    {
                        // ----------------------------------------------------
                        // Compare featuresToIdentify to mComparisonPeptideInfo for each threshold level
                        // ----------------------------------------------------

                        // ----------------------------------------------------
                        // Validate mThresholdLevels to assure at least one threshold exists
                        // ----------------------------------------------------

                        if (mThresholdLevels.Length <= 0)
                        {
                            InitializeThresholdLevels(ref mThresholdLevels, 1, false);
                        }

                        for (thresholdIndex = 0; thresholdIndex < mThresholdLevels.Length; thresholdIndex++)
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

                        LogMessage("Uniqueness Stats processing starting, Threshold Count = " + mThresholdLevels.Length.ToString(), MessageTypeConstants.Normal);

                        if (!PeakMatching.FillRangeSearchObject(ref rangeSearch, mComparisonPeptideInfo))
                        {
                            success = false;
                        }
                        else
                        {
                            // ----------------------------------------------------
                            // Initialize the peak matching class
                            // ----------------------------------------------------

                            mPeakMatching = new PeakMatching()
                            {
                                MaxPeakMatchingResultsPerFeatureToSave = mMaxPeakMatchingResultsPerFeatureToSave,
                                UseMaxSearchDistanceMultiplierAndSLiCScore = UseSLiCScoreForUniqueness,
                                UseEllipseSearchRegion = UseEllipseSearchRegion
                            };

                            mPeakMatching.LogEvent += mPeakMatching_LogEvent;
                            mPeakMatching.ProgressContinues += mPeakMatching_ProgressContinues;

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

                            for (thresholdIndex = 0; thresholdIndex < mThresholdLevels.Length; thresholdIndex++)
                            {
                                UpdateProgress("Generating uniqueness stats, threshold " + (thresholdIndex + 1).ToString() + " / " + mThresholdLevels.Length.ToString());
                                UpdateSubtaskProgress("Finding matching peptides for given search thresholds", 0f);

                                // Perform the actual peak matching
                                LogMessage("Threshold " + (thresholdIndex + 1).ToString() + ", IdentifySequences", MessageTypeConstants.Normal);
                                success = mPeakMatching.IdentifySequences(mThresholdLevels[thresholdIndex], ref featuresToIdentify, mComparisonPeptideInfo, out var featureMatchResults, ref rangeSearch);
                                mPeptideMatchResults = featureMatchResults;
                                mPeptideMatchResults.SortingList += mPeptideMatchResults_SortingList;

                                if (!success)
                                    break;
                                if (SavePeakMatchingResults)
                                {
                                    // Write out the raw peak matching results
                                    success = ExportPeakMatchingResults(mThresholdLevels[thresholdIndex], thresholdIndex, mComparisonPeptideInfo.Count, mPeptideMatchResults, outputFolderPath, outputFilenameBase, pmResultsWriter);
                                    if (!success)
                                        break;
                                }

                                progressStep += 1;
                                UpdateProgress((float)(progressStep / (double)progressStepCount * 100d));

                                // Summarize the results by peptide
                                LogMessage("Threshold " + (thresholdIndex + 1).ToString() + ", SummarizeResultsByPeptide", MessageTypeConstants.Normal);
                                success = SummarizeResultsByPeptide(mThresholdLevels[thresholdIndex], thresholdIndex, featuresToIdentify, mPeptideMatchResults, outputFolderPath, outputFilenameBase, peptideUniquenessWriter);
                                if (!success)
                                    break;
                                progressStep += 1;
                                UpdateProgress((float)(progressStep / (double)progressStepCount * 100d));

                                // Summarize the results by protein
                                LogMessage("Threshold " + (thresholdIndex + 1).ToString() + ", SummarizeResultsByProtein", MessageTypeConstants.Normal);
                                success = SummarizeResultsByProtein(mThresholdLevels[thresholdIndex], thresholdIndex, featuresToIdentify, mPeptideMatchResults, outputFolderPath, outputFilenameBase, proteinStatsWriter);
                                if (!success)
                                    break;
                                progressStep += 1;
                                UpdateProgress((float)(progressStep / (double)progressStepCount * 100d));
                                if (!success || AbortProcessing)
                                {
                                    success = false;
                                    break;
                                }

                                success = true;
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
                        if (pmResultsWriter != null)
                            pmResultsWriter.Close();
                        if (peptideUniquenessWriter != null)
                            peptideUniquenessWriter.Close();
                        if (proteinStatsWriter != null)
                            proteinStatsWriter.Close();

                        if (mPeakMatching != null)
                        {
                            mPeakMatching.LogEvent -= mPeakMatching_LogEvent;
                            mPeakMatching.ProgressContinues -= mPeakMatching_ProgressContinues;
                        }

                        mPeakMatching = null;
                    }
                    catch (Exception ex)
                    {
                        // Ignore any errors closing files
                    }
                }
            }

            if (!searchAborted && success)
            {
                LogMessage("Uniqueness Stats processing complete", MessageTypeConstants.Normal);
            }

            return success;
        }

        public override IList<string> GetDefaultExtensionsToParse()
        {
            var extensionsToParse = new List<string>() { ".fasta", ".txt" };
            return extensionsToParse;
        }

        public override string GetErrorMessage()
        {
            // Returns "" if no error

            string errorMessage;
            if (ErrorCode == ProcessFilesErrorCodes.LocalizedError || ErrorCode == ProcessFilesErrorCodes.NoError)
            {
                switch (mLocalErrorCode)
                {
                    case ErrorCodes.NoError:
                        errorMessage = "";
                        break;
                    case ErrorCodes.ProteinDigestionSimulatorSectionNotFound:
                        errorMessage = "The section " + ProteinFileParser.XML_SECTION_OPTIONS + " was not found in the parameter file";
                        break;
                    case ErrorCodes.ErrorReadingInputFile:
                        errorMessage = "Error reading input file";
                        break;
                    case ErrorCodes.ProteinsNotFoundInInputFile:
                        errorMessage = "No proteins were found in the input file (make sure the Column Order is correct on the File Format Options tab)";
                        break;
                    case ErrorCodes.ErrorIdentifyingSequences:
                        errorMessage = "Error identifying sequences";
                        break;
                    case ErrorCodes.ErrorWritingOutputFile:
                        errorMessage = "Error writing to one of the output files";
                        break;
                    case ErrorCodes.UserAbortedSearch:
                        errorMessage = "User aborted search";
                        break;
                    case ErrorCodes.UnspecifiedError:
                        errorMessage = "Unspecified localized error";
                        break;
                    default:
                        // This shouldn't happen
                        errorMessage = "Unknown error state";
                        break;
                }
            }
            else
            {
                errorMessage = GetBaseClassErrorMessage();
            }

            if (mLastErrorMessage != null && mLastErrorMessage.Length > 0)
            {
                errorMessage += ControlChars.NewLine + mLastErrorMessage;
            }

            return errorMessage;
        }

        private int GetNextUniqueSequenceID(string sequence, Hashtable masterSequences, ref int nextUniqueIDForMasterSeqs)
        {
            int uniqueSeqID;
            try
            {
                if (masterSequences.ContainsKey(sequence))
                {
                    uniqueSeqID = Conversions.ToInteger(masterSequences[sequence]);
                }
                else
                {
                    masterSequences.Add(sequence, nextUniqueIDForMasterSeqs);
                    uniqueSeqID = nextUniqueIDForMasterSeqs;
                }

                nextUniqueIDForMasterSeqs += 1;
            }
            catch (Exception ex)
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
            DataRow[] dataRows;
            dataRows = mProteinToIdentifiedPeptideMappingTable.Select(PROTEIN_ID_COLUMN + " = " + proteinID.ToString());
            if (dataRows != null)
            {
                return dataRows.Length;
            }
            else
            {
                return 0;
            }
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

        private void InitializePeptideAndProteinResultsFiles(string outputFolderPath, string outputFilenameBase, IList<PeakMatching.SearchThresholds> thresholds, out StreamWriter peptideUniquenessWriter, out StreamWriter proteinStatsWriter)
        {
            // Initialize the output file so that the peptide and protein summary results for all thresholds can be saved in the same file

            string workingFilenameBase;
            string outputFilePath;
            int thresholdIndex;

            // ----------------------------------------------------
            // Create the peptide uniqueness stats files
            // ----------------------------------------------------

            workingFilenameBase = outputFilenameBase + "_PeptideStatsBinned.txt";
            outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);
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

        private void InitializePMResultsFile(string outputFolderPath, string outputFilenameBase, IList<PeakMatching.SearchThresholds> thresholds, int comparisonFeaturesCount, out StreamWriter pmResultsWriter)
        {
            // Initialize the output file so that the peak matching results for all thresholds can be saved in the same file

            string workingFilenameBase;
            string outputFilePath;
            int thresholdIndex;

            // Combine all of the data together in one output file
            workingFilenameBase = outputFilenameBase + "_PMResults.txt";
            outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);
            pmResultsWriter = new StreamWriter(outputFilePath);
            pmResultsWriter.WriteLine("Comparison feature count: " + comparisonFeaturesCount.ToString());
            for (thresholdIndex = 0; thresholdIndex < thresholds.Count; thresholdIndex++)
                ExportThresholds(pmResultsWriter, thresholdIndex, thresholds[thresholdIndex]);
            pmResultsWriter.WriteLine();
            ExportPeakMatchingResultsWriteHeaders(pmResultsWriter, CreateSeparateOutputFileForEachThreshold);
        }

        private void InitializeLocalVariables()
        {
            DigestSequences = false;
            CysPeptidesOnly = false;
            mOutputFileDelimiter = ControlChars.Tab;
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
            mLocalErrorCode = ErrorCodes.NoError;
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
                mProteinToIdentifiedPeptideMappingTable.PrimaryKey = new DataColumn[] { mProteinToIdentifiedPeptideMappingTable.Columns[PROTEIN_ID_COLUMN], mProteinToIdentifiedPeptideMappingTable.Columns[PEPTIDE_ID_MATCH_COLUMN] };
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
        //    int newFeatureID;
        //    int index;
        //    int indexEnd;

        //    System.Data.DataRow newRow;
        //    ProgressFormNET.frmProgress progressForm = new ProgressFormNET.frmProgress();
        //    DateTime startTime;

        //    indexEnd = System.Convert.ToInt32(MAX_FEATURE_COUNT * 1.5);
        //    progressForm.InitializeProgressForm("Populating dataset table", 0, indexEnd, true);
        //    progressForm.Visible = true;
        //    Application.DoEvents();

        //    startTime = System.DateTime.UtcNow;

        //    for (index = 0; index <= indexEnd; index++)
        //    {
        //        newFeatureID = randomGenerator.Next(0, MAX_FEATURE_COUNT);

        //        // Look for existing entry in table
        //        if (!dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME).Rows.Contains(newFeatureID))
        //        {
        //            newRow = dsDataset.Tables(PEPTIDE_INFO_TABLE_NAME).NewRow;
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
                mProteinFileParser.ErrorEvent += mProteinFileParser_ErrorEvent;
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
                levels = 1;
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
                if (parameterFilePath == null || parameterFilePath.Length == 0)
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
            bool digestionEnabled;
            try
            {
                if (mComparisonPeptideInfo != null)
                {
                    mComparisonPeptideInfo = null;
                }

                mComparisonPeptideInfo = new PeakMatching.PMComparisonFeatureInfo();
                if (mProteinInfo != null)
                {
                    mProteinInfo.SortingList -= mProteinInfo_SortingList;
                    mProteinInfo.SortingMappings -= mProteinInfo_SortingMappings;
                    mProteinInfo = null;
                }

                mProteinInfo = new ProteinCollection();
                mProteinInfo.SortingList += mProteinInfo_SortingList;
                mProteinInfo.SortingMappings += mProteinInfo_SortingMappings;

                // Possibly initialize the ProteinFileParser object
                if (mProteinFileParser == null)
                {
                    InitializeProteinFileParser();
                }

                // Note that ProteinFileParser is exposed as public so that options can be set directly in it

                if (ProteinFileParser.IsFastaFile(proteinInputFilePath) || mProteinFileParser.AssumeFastaFile)
                {
                    LogMessage("Input file format = FASTA", MessageTypeConstants.Normal);
                    digestionEnabled = true;
                }
                else
                {
                    delimitedFileFormatCode = mProteinFileParser.DelimitedFileFormatCode;
                    LogMessage("Input file format = " + delimitedFileFormatCode.ToString(), MessageTypeConstants.Normal);
                    digestionEnabled = DigestSequences;
                }

                switch (delimitedFileFormatCode)
                {
                    case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID:
                    case DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence:
                        break;
                    // Reading peptides from a delimited file; digestionEnabled is typically False, but could be true
                    case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET:
                    case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore:
                    // Reading peptides from a delimited file; digestionEnabled is typically False, but could be true
                    case DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET:
                        break;
                    default:
                        // Force digest Sequences to true
                        digestionEnabled = true;
                        break;
                }

                LogMessage("Digest sequences = " + digestionEnabled.ToString(), MessageTypeConstants.Normal);
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
                    LogMessage("Loaded " + mComparisonPeptideInfo.Count + " peptides corresponding to " + mProteinInfo.Count + " proteins", MessageTypeConstants.Normal);
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
            InSilicoDigest.PeptideSequenceWithNET newPeptide;
            bool delimitedFileHasMassAndNET;
            bool success;
            bool inputPeptideFound;
            int inputFileLinesRead;
            var inputFileLineSkipCount = default(int);
            string skipMessage;
            try
            {
                delimitedFileReader = new DelimitedProteinFileReader()
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
                newPeptide = new InSilicoDigest.PeptideSequenceWithNET();
                switch (delimitedFileReader.DelimitedFileFormatCode)
                {
                    case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET:
                    case DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_PeptideSequence_UniqueID_Mass_NET_NETStDev_DiscriminantScore:
                    case DelimitedProteinFileReader.ProteinFileFormatCode.UniqueID_Sequence_Mass_NET:
                        // Do not use the computed mass and NET
                        delimitedFileHasMassAndNET = true;
                        break;
                    default:
                        delimitedFileHasMassAndNET = false;
                        break;
                }

                // Always auto compute the NET and Mass in the newPeptide class
                // However, if delimitedFileHasMassAndNET = True and valid Mass and NET values were read from the text file,
                // they are passed to AddOrUpdatePeptide rather than the computed values
                newPeptide.AutoComputeNET = true;
                do
                {
                    inputPeptideFound = delimitedFileReader.ReadNextProteinEntry();
                    inputFileLinesRead = delimitedFileReader.LinesRead;
                    inputFileLineSkipCount += delimitedFileReader.LineSkipCount;
                    if (inputPeptideFound)
                    {
                        newPeptide.SequenceOneLetter = delimitedFileReader.ProteinSequence;
                        if (!delimitedFileHasMassAndNET || Math.Abs(delimitedFileReader.PeptideMass - 0d) < float.Epsilon && Math.Abs(delimitedFileReader.PeptideNET - 0f) < float.Epsilon)
                        {
                            AddOrUpdatePeptide(delimitedFileReader.EntryUniqueID, newPeptide.Mass, newPeptide.NET, 0f, 0f, delimitedFileReader.ProteinName, ProteinCollection.CleavageStateConstants.Unknown, string.Empty);
                        }
                        else
                        {
                            AddOrUpdatePeptide(delimitedFileReader.EntryUniqueID, delimitedFileReader.PeptideMass, delimitedFileReader.PeptideNET, delimitedFileReader.PeptideNETStDev, delimitedFileReader.PeptideDiscriminantScore, delimitedFileReader.ProteinName, ProteinCollection.CleavageStateConstants.Unknown, string.Empty);

                            // ToDo: Possibly enable this here if the input file contained NETStDev values: SLiCScoreUseAMTNETStDev = True
                        }

                        UpdateProgress(delimitedFileReader.PercentFileProcessed());
                        if (AbortProcessing)
                            break;
                    }
                }
                while (inputPeptideFound);
                if (inputFileLineSkipCount > 0)
                {
                    skipMessage = "Note that " + inputFileLineSkipCount.ToString() + " out of " + inputFileLinesRead.ToString() + " lines were skipped in the input file because they did not match the column order defined on the File Format Options Tab (" + mProteinFileParser.DelimitedFileFormatCode.ToString() + ")";
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
            bool generateUniqueSequenceID;
            bool isFastaFile;
            Hashtable masterSequences = null;
            List<InSilicoDigest.PeptideSequenceWithNET> digestedPeptides = null;
            try
            {
                if (mProteinFileParser.GenerateUniqueIDValuesForPeptides)
                {
                    // Need to generate unique sequence ID values
                    generateUniqueSequenceID = true;

                    // Initialize masterSequences
                    masterSequences = new Hashtable();
                }
                else
                {
                    generateUniqueSequenceID = false;
                }

                int nextUniqueIDForMasterSeqs = 1;
                if (ProteinFileParser.IsFastaFile(proteinInputFilePath) || mProteinFileParser.AssumeFastaFile)
                {
                    isFastaFile = true;
                }
                else
                {
                    isFastaFile = false;
                }

                // Disable mass calculation
                mProteinFileParser.ComputeProteinMass = false;
                mProteinFileParser.CreateProteinOutputFile = false;
                if (CysPeptidesOnly)
                {
                    mProteinFileParser.DigestionOptions.AminoAcidResidueFilterChars = new char[] { 'C' };
                }
                else
                {
                    mProteinFileParser.DigestionOptions.AminoAcidResidueFilterChars = new char[] { };
                }

                // Load the proteins in the input file into memory
                success = mProteinFileParser.ParseProteinFile(proteinInputFilePath, "");
                string skipMessage = string.Empty;
                if (mProteinFileParser.InputFileLineSkipCount > 0 && !isFastaFile)
                {
                    skipMessage = "Note that " + mProteinFileParser.InputFileLineSkipCount.ToString() + " out of " + mProteinFileParser.InputFileLinesRead.ToString() + " lines were skipped in the input file because they did not match the column order" + " defined on the File Format Options Tab (" + mProteinFileParser.DelimitedFileFormatCode.ToString() + ")";
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
                    for (int proteinIndex = 0; proteinIndex < mProteinFileParser.GetProteinCountCached(); proteinIndex++)
                    {
                        var proteinOrPeptide = mProteinFileParser.GetCachedProtein(proteinIndex);
                        int peptideCount = mProteinFileParser.GetDigestedPeptidesForCachedProtein(proteinIndex, out digestedPeptides, mProteinFileParser.DigestionOptions);
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
                                    nextUniqueIDForMasterSeqs += 1;
                                }

                                AddOrUpdatePeptide(uniqueSeqID, digestedPeptide.Mass, digestedPeptide.NET, 0f, 0f, proteinOrPeptide.Name, ProteinCollection.CleavageStateConstants.Unknown, digestedPeptide.PeptideName);
                            }
                        }

                        UpdateProgress((float)((proteinIndex + 1) / (double)mProteinFileParser.GetProteinCountCached() * 100d));
                        if (AbortProcessing)
                            break;
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

        private void InitializeBinningRanges(PeakMatching.SearchThresholds thresholds, PeakMatching.PMFeatureInfo featuresToIdentify, PeakMatching.PMFeatureMatchResults peptideMatchResults, BinnedPeptideCountStats peptideStatsBinned)
        {
            float featureMass;
            int matchIndex;
            var currentFeatureID = default(int);
            var featureInfo = new PeakMatching.FeatureInfo();
            var matchResultInfo = default(PeakMatching.PMFeatureMatchResults.PeakMatchingResult);
            int IntegerMass, remainder;
            if (peptideStatsBinned.Settings.AutoDetermineMassRange)
            {
                // Examine peptideMatchResults to determine the minimum and maximum masses for features with matches

                // First, set the ranges to out-of-range values
                peptideStatsBinned.Settings.MassMinimum = (float)double.MaxValue;
                peptideStatsBinned.Settings.MassMaximum = (float)double.MinValue;

                // Now examine .PeptideMatchResults()
                for (matchIndex = 0; matchIndex < peptideMatchResults.Count; matchIndex++)
                {
                    peptideMatchResults.GetMatchInfoByRowIndex(matchIndex, ref currentFeatureID, ref matchResultInfo);
                    featuresToIdentify.GetFeatureInfoByFeatureID(currentFeatureID, out featureInfo);
                    featureMass = (float)featureInfo.Mass;
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

                if (Math.Abs(peptideStatsBinned.Settings.MassMinimum - double.MaxValue) < double.Epsilon || Math.Abs(peptideStatsBinned.Settings.MassMaximum - double.MinValue) < double.Epsilon)
                {
                    // No matches were found; set these to defaults
                    peptideStatsBinned.Settings.MassMinimum = 400f;
                    peptideStatsBinned.Settings.MassMaximum = 6000f;
                }

                IntegerMass = (int)Math.Round(Math.Round(peptideStatsBinned.Settings.MassMinimum, 0));
                Math.DivRem(IntegerMass, 100, out remainder);
                IntegerMass -= remainder;
                peptideStatsBinned.Settings.MassMinimum = IntegerMass;
                IntegerMass = (int)Math.Round(Math.Round(peptideStatsBinned.Settings.MassMaximum, 0));
                Math.DivRem(IntegerMass, 100, out remainder);
                IntegerMass += 100 - remainder;
                peptideStatsBinned.Settings.MassMaximum = IntegerMass;
            }

            // Determine BinCount; do not allow more than 1,000,000 bins
            if (Math.Abs(peptideStatsBinned.Settings.MassBinSizeDa - 0f) < 0.00001d)
                peptideStatsBinned.Settings.MassBinSizeDa = 1f;
            do
            {
                try
                {
                    peptideStatsBinned.BinCount = (int)Math.Round(Math.Ceiling(peptideStatsBinned.Settings.MassMaximum - peptideStatsBinned.Settings.MassMinimum) / peptideStatsBinned.Settings.MassBinSizeDa);
                    if (peptideStatsBinned.BinCount > 1000000)
                    {
                        peptideStatsBinned.Settings.MassBinSizeDa *= 10f;
                    }
                }
                catch (Exception ex)
                {
                    peptideStatsBinned.BinCount = 1000000000;
                }
            }
            while (peptideStatsBinned.BinCount > 1000000);
        }

        private int MassToBinIndex(double ThisMass, double StartMass, double MassResolution)
        {
            double WorkingMass;

            // First subtract StartMass from ThisMass
            // For example, if StartMass is 500 and ThisMass is 500.28, then WorkingMass = 0.28
            // Or, if StartMass is 500 and ThisMass is 530.83, then WorkingMass = 30.83
            WorkingMass = ThisMass - StartMass;

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

            FileInfo file;
            string inputFilePathFull;
            var success = default(bool);
            if (resetErrorCode)
            {
                SetLocalErrorCode(ErrorCodes.NoError);
            }

            if (!LoadParameterFileSettings(parameterFilePath))
            {
                string statusMessage = "Parameter file load error: " + parameterFilePath;
                ShowErrorMessage(statusMessage);
                if (ErrorCode == ProcessFilesErrorCodes.NoError)
                {
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidParameterFile);
                }

                return false;
            }

            try
            {
                if (inputFilePath == null || inputFilePath.Length == 0)
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
                            file = new FileInfo(inputFilePath);
                            inputFilePathFull = file.FullName;
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

        private bool SummarizeResultsByPeptide(PeakMatching.SearchThresholds thresholds, int thresholdIndex, PeakMatching.PMFeatureInfo featuresToIdentify, PeakMatching.PMFeatureMatchResults peptideMatchResults, string outputFolderPath, string outputFilenameBase, StreamWriter peptideUniquenessWriter)
        {
            string workingFilenameBase;
            string outputFilePath;
            int peptideIndex;
            int featuresToIdentifyCount;
            var featureInfo = new PeakMatching.FeatureInfo();
            var matchCount = default(int);
            int binIndex;
            int peptideSkipCount;
            int total;
            int maxMatchCount;
            bool success;
            try
            {
                // Copy the binning settings
                var peptideStatsBinned = new BinnedPeptideCountStats(mPeptideUniquenessBinningSettings.Clone());

                // Define the minimum and maximum mass ranges, plus the number of bins required
                InitializeBinningRanges(thresholds, featuresToIdentify, peptideMatchResults, peptideStatsBinned);
                featuresToIdentifyCount = featuresToIdentify.Count;
                ResetProgress("Summarizing results by peptide");

                // --------------------------------------
                // Compute the stats for thresholds
                // --------------------------------------

                // Define the maximum match count that will be tracked
                maxMatchCount = Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave);

                // Reserve memory for the bins, store the bin ranges for each bin, and reset the ResultIDs arrays
                InitializeBinnedStats(peptideStatsBinned, peptideStatsBinned.BinCount);

                // ToDo: When using Sql Server, switch this to use a SP that performs the same function as this For Loop, sorting the results in a table in the DB, but using Bulk Update queries

                LogMessage("SummarizeResultsByPeptide starting, total feature count = " + featuresToIdentifyCount.ToString(), MessageTypeConstants.Normal);
                peptideSkipCount = 0;
                for (peptideIndex = 0; peptideIndex < featuresToIdentifyCount; peptideIndex++)
                {
                    if (featuresToIdentify.GetFeatureInfoByRowIndex(peptideIndex, out featureInfo))
                    {
                        binIndex = MassToBinIndex(featureInfo.Mass, peptideStatsBinned.Settings.MassMinimum, peptideStatsBinned.Settings.MassBinSizeDa);
                        if (binIndex < 0 || binIndex > peptideStatsBinned.BinCount - 1)
                        {
                            // Peptide mass is out-of-range, ignore the result
                            peptideSkipCount += 1;
                        }
                        else if (FeatureContainsUniqueMatch(featureInfo, peptideMatchResults, ref matchCount, UseSLiCScoreForUniqueness, peptideStatsBinned.Settings.MinimumSLiCScore))
                        {
                            peptideStatsBinned.Bins[binIndex].UniqueResultIDCount += 1;
                            peptideStatsBinned.Bins[binIndex].ResultIDCountDistribution[1] += 1;
                        }
                        else if (matchCount > 0)
                        {
                            // Feature has 1 or more matches, but they're not unique
                            peptideStatsBinned.Bins[binIndex].NonUniqueResultIDCount += 1;
                            if (matchCount < maxMatchCount)
                            {
                                peptideStatsBinned.Bins[binIndex].ResultIDCountDistribution[matchCount] += 1;
                            }
                            else
                            {
                                peptideStatsBinned.Bins[binIndex].ResultIDCountDistribution[maxMatchCount] += 1;
                            }
                        }
                    }

                    if (peptideIndex % 100 == 0)
                    {
                        UpdateProgress((float)(peptideIndex / (double)featuresToIdentifyCount * 100d));
                        if (AbortProcessing)
                            break;
                    }

                    if (peptideIndex % 100000 == 0 && peptideIndex > 0)
                    {
                        LogMessage("SummarizeResultsByPeptide, peptideIndex = " + peptideIndex.ToString(), MessageTypeConstants.Normal);
                    }
                }

                for (binIndex = 0; binIndex < peptideStatsBinned.BinCount; binIndex++)
                {
                    total = peptideStatsBinned.Bins[binIndex].UniqueResultIDCount + peptideStatsBinned.Bins[binIndex].NonUniqueResultIDCount;
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
                    LogMessage("Skipped " + peptideSkipCount.ToString() + " peptides since their masses were outside the defined bin range", MessageTypeConstants.Warning);
                }

                // Write out the peptide results for this threshold level
                if (CreateSeparateOutputFileForEachThreshold)
                {
                    workingFilenameBase = outputFilenameBase + "_PeptideStatsBinned" + (thresholdIndex + 1).ToString() + ".txt";
                    outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);
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

                LogMessage("SummarizeResultsByPeptide complete", MessageTypeConstants.Normal);
                UpdateProgress(100f);
            }
            catch (Exception ex)
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

            lineOut += "Bin_Start_Mass" + mOutputFileDelimiter + "Percent_Unique" + mOutputFileDelimiter + "Peptide_Count_Total";
            for (index = 1; index < Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave); index++)
                lineOut += mOutputFileDelimiter + "MatchCount_" + index.ToString();
            lineOut += mOutputFileDelimiter + "MatchCount_" + Math.Min(ID_COUNT_DISTRIBUTION_MAX, mMaxPeakMatchingResultsPerFeatureToSave).ToString() + "_OrMore";
            peptideUniquenessWriter.WriteLine(lineOut);
            peptideUniquenessWriter.Flush();
        }

        private bool SummarizeResultsByProtein(PeakMatching.SearchThresholds thresholds, int thresholdIndex, PeakMatching.PMFeatureInfo featuresToIdentify, PeakMatching.PMFeatureMatchResults peptideMatchResults, string outputFolderPath, string outputFilenameBase, StreamWriter proteinStatsWriter)
        {
            string workingFilenameBase;
            string outputFilePath;
            int peptideIndex;
            int featuresToIdentifyCount;
            int proteinIndex;
            var matchCount = default(int);
            int[] proteinIDs;
            DataRow newDataRow;
            bool success;
            var featureInfo = new PeakMatching.FeatureInfo();
            try
            {
                featuresToIdentifyCount = featuresToIdentify.Count;
                ResetProgress("Summarizing results by protein");

                // Compute the stats for thresholds

                // Compute number of unique peptides seen for each protein and write out
                // This will allow one to plot Unique peptide vs protein mass or total peptide count vs. protein mass

                // Initialize mProteinToIdentifiedPeptideMappingTable
                mProteinToIdentifiedPeptideMappingTable.Clear();
                LogMessage("SummarizeResultsByProtein starting, total feature count = " + featuresToIdentifyCount.ToString(), MessageTypeConstants.Normal);
                for (peptideIndex = 0; peptideIndex < featuresToIdentifyCount; peptideIndex++)
                {
                    if (featuresToIdentify.GetFeatureInfoByRowIndex(peptideIndex, out featureInfo))
                    {
                        if (FeatureContainsUniqueMatch(featureInfo, peptideMatchResults, ref matchCount, UseSLiCScoreForUniqueness, mPeptideUniquenessBinningSettings.MinimumSLiCScore))
                        {
                            proteinIDs = mProteinInfo.GetProteinIDsMappedToPeptideID(featureInfo.FeatureID);
                            for (proteinIndex = 0; proteinIndex < proteinIDs.Length; proteinIndex++)
                            {
                                if (!mProteinToIdentifiedPeptideMappingTable.Rows.Contains(new object[] { proteinIDs[proteinIndex], featureInfo.FeatureID }))
                                {
                                    newDataRow = mProteinToIdentifiedPeptideMappingTable.NewRow();
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
                            break;
                    }

                    if (peptideIndex % 100000 == 0 && peptideIndex > 0)
                    {
                        LogMessage("SummarizeResultsByProtein, peptideIndex = " + peptideIndex.ToString(), MessageTypeConstants.Normal);
                    }
                }

                // Write out the protein results for this threshold level
                if (CreateSeparateOutputFileForEachThreshold)
                {
                    workingFilenameBase = outputFilenameBase + "_ProteinStatsBinned" + (thresholdIndex + 1).ToString() + ".txt";
                    outputFilePath = Path.Combine(outputFolderPath, workingFilenameBase);
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
                LogMessage("SummarizeResultsByProtein complete", MessageTypeConstants.Normal);
            }
            catch (Exception ex)
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

            lineOut += "Protein_Name" + mOutputFileDelimiter + "Protein_ID" + mOutputFileDelimiter + "Peptide_Count_Total" + mOutputFileDelimiter + "Peptide_Count_Uniquely_Identifiable";
            proteinStatsWriter.WriteLine(lineOut);
            proteinStatsWriter.Flush();
        }

        private void SetLocalErrorCode(ErrorCodes newErrorCode)
        {
            SetLocalErrorCode(newErrorCode, false);
        }

        private void SetLocalErrorCode(ErrorCodes newErrorCode, Exception ex)
        {
            SetLocalErrorCode(newErrorCode, false);
            LogMessage(newErrorCode.ToString() + ": " + ex.Message, MessageTypeConstants.ErrorMsg);
            mLastErrorMessage = ex.Message;
        }

        private void SetLocalErrorCode(ErrorCodes newErrorCode, bool leaveExistingErrorCodeUnchanged)
        {
            if (leaveExistingErrorCodeUnchanged && mLocalErrorCode != ErrorCodes.NoError)
            {
            }
            // An error code is already defined; do not change it
            else
            {
                mLocalErrorCode = newErrorCode;
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
            bool descriptionChanged = false;
            if ((description ?? "") != (mSubtaskProgressStepDescription ?? ""))
            {
                descriptionChanged = true;
            }

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
                    LogMessage(mSubtaskProgressStepDescription.Replace(ControlChars.NewLine, "; "));
                }
                else
                {
                    LogMessage(mSubtaskProgressStepDescription + " (" + mSubtaskProgressPercentComplete.ToString("0.0") + "% complete)".Replace(ControlChars.NewLine, "; "));
                }
            }

            SubtaskProgressChanged?.Invoke(description, ProgressPercentComplete);
        }

        private int sortingListCount = 0;
        private DateTime sortingListLastPostTime = DateTime.UtcNow;

        private void mProteinInfo_SortingList()
        {
            sortingListCount += 1;
            if (DateTime.UtcNow.Subtract(sortingListLastPostTime).TotalSeconds >= 10d)
            {
                base.LogMessage("Sorting protein list (SortCount = " + sortingListCount + ")", MessageTypeConstants.Normal);
                sortingListLastPostTime = DateTime.UtcNow;
            }
        }

        private int sortingMappingsCount = 0;
        private DateTime sortingMappingsLastPostTime = DateTime.UtcNow;

        private void mProteinInfo_SortingMappings()
        {
            sortingMappingsCount += 1;
            if (DateTime.UtcNow.Subtract(sortingMappingsLastPostTime).TotalSeconds >= 10d)
            {
                base.LogMessage("Sorting protein to peptide mapping info (SortCount = " + sortingMappingsCount + ")", MessageTypeConstants.Normal);
                sortingMappingsLastPostTime = DateTime.UtcNow;
            }
        }

        private int resultsSortingListCount = 0;
        private DateTime resultsSortingListLastPostTime = DateTime.UtcNow;

        private void mPeptideMatchResults_SortingList()
        {
            resultsSortingListCount += 1;
            if (DateTime.UtcNow.Subtract(resultsSortingListLastPostTime).TotalSeconds >= 10d)
            {
                base.LogMessage("Sorting peptide match results list (SortCount = " + resultsSortingListCount + ")", MessageTypeConstants.Normal);
                resultsSortingListLastPostTime = DateTime.UtcNow;
            }
        }

        private void mPeakMatching_LogEvent(string Message, PeakMatching.MessageTypeConstants EventType)
        {
            switch (EventType)
            {
                case PeakMatching.MessageTypeConstants.Normal:
                    LogMessage(Message, MessageTypeConstants.Normal);
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

        private void mPeakMatching_ProgressContinues()
        {
            UpdateSubtaskProgress(mPeakMatching.ProgressPct);
        }

        private void mProteinFileParser_ErrorEvent(string message, Exception ex)
        {
            ShowErrorMessage("Error in mProteinFileParser: " + message);
        }
    }
}