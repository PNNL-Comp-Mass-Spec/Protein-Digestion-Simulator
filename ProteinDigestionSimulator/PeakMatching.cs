using System;
using System.Collections.Generic;

namespace ProteinDigestionSimulator
{
    public class PeakMatching
    {
        // Ignore Spelling: Da, Sql, tol

        public PeakMatching()
        {
            InitializeLocalVariables();
        }

        public const float DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER = 2f;
        private const int STDEV_SCALING_FACTOR = 2;
        private const double ONE_PART_PER_MILLION = 1000000.0d;

        public enum MessageTypeConstants
        {
            Normal = 0,
            ErrorMsg = 1,
            Warning = 2,
            Health = 3
        }

        public readonly struct FeatureInfo : IComparable<FeatureInfo>
        {
            public int FeatureID { get; }                                   // Each feature should have a unique ID
            public string FeatureName { get; }                                // Optional
            public double Mass { get; }
            public float NET { get; }

            public FeatureInfo(int featureId, string featureName, double mass, float net)
            {
                FeatureID = featureId;
                FeatureName = featureName;
                Mass = mass;
                NET = net;
            }

            public static FeatureInfo Blank()
            {
                return new FeatureInfo(0, string.Empty, 0, 0);
            }

            public int CompareTo(FeatureInfo other)
            {
                // Sort by Feature ID, ascending
                return FeatureID.CompareTo(other.FeatureID);
            }
        }

        private class PeakMatchingRawMatches : IComparable<PeakMatchingRawMatches>
        {
            public int MatchingIDIndex { get; }               // Pointer into comparison features (RowIndex in PMComparisonFeatureInfo)
            public double StandardizedSquaredDistance { get; set; }
            public double SLiCScoreNumerator { get; set; }
            public double SLiCScore { get; set; }                      // SLiC Score (Spatially Localized Confidence score)
            public double DelSLiC { get; set; }                        // Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
            public double MassErr { get; set; }                        // Observed difference (error) between comparison mass and feature mass (in Da)
            public double NETErr { get; set; }                         // Observed difference (error) between comparison NET and feature NET

            public PeakMatchingRawMatches(int matchingIdIndex)
            {
                MatchingIDIndex = matchingIdIndex;
            }

            public int CompareTo(PeakMatchingRawMatches other)
            {
                // Sort by .SLiCScore descending, and by MatchingIDIndexOriginal Ascending
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                var sLiCScoreComparison = other.SLiCScore.CompareTo(SLiCScore);
                if (sLiCScoreComparison != 0) return sLiCScoreComparison;
                return MatchingIDIndex.CompareTo(other.MatchingIDIndex);
            }
        }

        private class SearchModeOptions
        {
            public bool UseMaxSearchDistanceMultiplierAndSLiCScore { get; set; }
            public bool UseEllipseSearchRegion { get; set; }        // Only valid if UseMaxSearchDistanceMultiplierAndSLiCScore = False; if both UseMaxSearchDistanceMultiplierAndSLiCScore = False and UseEllipseSearchRegion = False, then uses a rectangle for peak matching
        }

        internal class PMFeatureInfo
        {
            protected int mFeatureCount;
            protected FeatureInfo[] mFeatures;
            protected bool mFeaturesArrayIsSorted;
            protected bool mUseFeatureIDDictionary;
            protected Dictionary<int, int> featureIDToRowIndex;

            public event SortingListEventHandler SortingList;

            public PMFeatureInfo()
            {
                mUseFeatureIDDictionary = true;                       // Set this to False to conserve memory; you must call Clear() after changing this for it to take effect
                Clear();
            }

            public virtual bool Add(ref FeatureInfo featureInfo)
            {
                return Add(featureInfo.FeatureID, featureInfo.FeatureName, featureInfo.Mass, featureInfo.NET);
            }

            public virtual bool Add(int featureID, string peptideName, double peptideMass, float peptideNET)
            {
                // Returns True if the feature was added

                if (ContainsFeature(featureID))
                {
                    return false;
                }

                // Add the feature
                if (mFeatureCount >= mFeatures.Length)
                {
                    Array.Resize(ref mFeatures, mFeatures.Length * 2);
                    // ReDim Preserve mFeatures(mFeatures.Length + MEMORY_RESERVE_CHUNK - 1)
                }

                mFeatures[mFeatureCount] = new FeatureInfo(featureID, peptideName, peptideMass, peptideNET);

                if (mUseFeatureIDDictionary)
                {
                    featureIDToRowIndex.Add(featureID, mFeatureCount);
                }

                mFeatureCount += 1;
                mFeaturesArrayIsSorted = false;

                // If we get here, all went well
                return true;
            }

            protected int BinarySearchFindFeature(int featureIDToFind)
            {
                // Looks through mFeatures() for featureIDToFind, returning the index of the item if found, or -1 if not found

                var firstIndex = 0;
                var lastIndex = mFeatureCount - 1;
                var matchingRowIndex = -1;
                if (mFeatureCount <= 0 || !SortFeatures())
                {
                    return matchingRowIndex;
                }

                try
                {
                    var midIndex = (firstIndex + lastIndex) / 2;
                    if (midIndex < firstIndex)
                        midIndex = firstIndex;
                    while (firstIndex <= lastIndex && mFeatures[midIndex].FeatureID != featureIDToFind)
                    {
                        if (featureIDToFind < mFeatures[midIndex].FeatureID)
                        {
                            // Search the lower half
                            lastIndex = midIndex - 1;
                        }
                        else if (featureIDToFind > mFeatures[midIndex].FeatureID)
                        {
                            // Search the upper half
                            firstIndex = midIndex + 1;
                        }
                        // Compute the new mid point
                        midIndex = (firstIndex + lastIndex) / 2;
                        if (midIndex < firstIndex)
                            break;
                    }

                    if (midIndex >= firstIndex && midIndex <= lastIndex)
                    {
                        if (mFeatures[midIndex].FeatureID == featureIDToFind)
                        {
                            matchingRowIndex = midIndex;
                        }
                    }
                }
                catch
                {
                    matchingRowIndex = -1;
                }

                return matchingRowIndex;
            }

            public virtual void Clear()
            {
                mFeatureCount = 0;
                if (mFeatures == null)
                {
                    mFeatures = new FeatureInfo[100000];
                }

                mFeaturesArrayIsSorted = false;
                if (mUseFeatureIDDictionary)
                {
                    if (featureIDToRowIndex == null)
                    {
                        featureIDToRowIndex = new Dictionary<int, int>();
                    }
                    else
                    {
                        featureIDToRowIndex.Clear();
                    }
                }
                else if (featureIDToRowIndex != null)
                {
                    featureIDToRowIndex.Clear();
                    featureIDToRowIndex = null;
                }
            }

            protected bool ContainsFeature(int featureID)
            {
                return ContainsFeature(featureID, out _);
            }

            protected bool ContainsFeature(int featureID, out int rowIndex)
            {
                // Returns True if the features array contains the feature
                // If found, returns the row index in rowIndex
                // Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

                if (mUseFeatureIDDictionary)
                {
                    if (featureIDToRowIndex.ContainsKey(featureID))
                    {
                        rowIndex = featureIDToRowIndex[featureID];
                    }
                    else
                    {
                        rowIndex = -1;
                    }
                }
                else
                {
                    // Perform a binary search of mFeatures for featureID
                    rowIndex = BinarySearchFindFeature(featureID);
                }

                return rowIndex >= 0;
            }

            public int Count => mFeatureCount;

            public virtual bool GetFeatureInfoByFeatureID(int featureID, out FeatureInfo featureInfo)
            {
                // Return the feature info for featureID

                var matchFound = ContainsFeature(featureID, out var rowIndex);
                if (matchFound)
                {
                    featureInfo = mFeatures[rowIndex];
                    return true;
                }

                featureInfo = FeatureInfo.Blank();
                return false;
            }

            public virtual bool GetFeatureInfoByRowIndex(int rowIndex, out FeatureInfo featureInfo)
            {
                if (rowIndex >= 0 && rowIndex < mFeatureCount)
                {
                    featureInfo = mFeatures[rowIndex];
                    return true;
                }

                featureInfo = FeatureInfo.Blank();
                return false;
            }

            public virtual double[] GetMassArrayByRowRange(int rowIndexStart, int rowIndexEnd)
            {
                double[] masses;
                if (rowIndexEnd < rowIndexStart)
                {
                    masses = new double[0];
                    return masses;
                }

                masses = new double[rowIndexEnd - rowIndexStart + 1];

                if (rowIndexEnd >= mFeatureCount)
                {
                    rowIndexEnd = mFeatureCount - 1;
                }

                var matchCount = 0;
                for (var index = rowIndexStart; index <= rowIndexEnd; index++)
                {
                    masses[matchCount] = mFeatures[index].Mass;
                    matchCount += 1;
                }

                if (masses.Length > matchCount)
                {
                    Array.Resize(ref masses, matchCount);
                }

                return masses;
            }

            public virtual double GetMassByRowIndex(int rowIndex)
            {
                if (rowIndex >= 0 && rowIndex < mFeatureCount)
                {
                    return mFeatures[rowIndex].Mass;
                }

                return 0d;
            }

            protected virtual bool SortFeatures(bool forceSort = false)
            {
                if (!mFeaturesArrayIsSorted || forceSort)
                {
                    SortingList?.Invoke();
                    Array.Sort(mFeatures, 0, mFeatureCount);

                    mFeaturesArrayIsSorted = true;
                }

                return mFeaturesArrayIsSorted;
            }

            public bool UseFeatureIDDictionary
            {
                get => mUseFeatureIDDictionary;
                set => mUseFeatureIDDictionary = value;
            }
        }

        internal class PMComparisonFeatureInfo : PMFeatureInfo
        {
            protected readonly struct ComparisonFeatureInfoExtended
            {
                public float NETStDev { get; }
                public float DiscriminantScore { get; }

                public ComparisonFeatureInfoExtended(float netStDev, float discriminantScore)
                {
                    NETStDev = netStDev;
                    DiscriminantScore = discriminantScore;
                }
            }

            protected ComparisonFeatureInfoExtended[] mExtendedInfo;

            public PMComparisonFeatureInfo()
            {
                Clear();
            }

            public bool Add(ref FeatureInfo featureInfo, float peptideNETStDev, float peptideDiscriminantScore)
            {
                return Add(featureInfo.FeatureID, featureInfo.FeatureName, featureInfo.Mass, featureInfo.NET, peptideNETStDev, peptideDiscriminantScore);
            }

            public bool Add(int featureID, string peptideName, double peptideMass, float peptideNET, float peptideNETStDev, float peptideDiscriminantScore)
            {
                // Add the base feature info
                if (!base.Add(featureID, peptideName, peptideMass, peptideNET))
                {
                    // The feature already existed, and therefore wasn't added
                    return false;
                }

                // Add the extended feature info
                if (mExtendedInfo.Length < mFeatures.Length)
                {
                    Array.Resize(ref mExtendedInfo, mFeatures.Length);
                }

                mExtendedInfo[mFeatureCount - 1] = new ComparisonFeatureInfoExtended(peptideNETStDev, peptideDiscriminantScore);

                // If we get here, all went well
                return true;
            }

            public new void Clear()
            {
                base.Clear();
                if (mExtendedInfo == null)
                {
                    mExtendedInfo = new ComparisonFeatureInfoExtended[10000];
                }
            }

            public bool GetFeatureInfoByFeatureID(int featureID, out FeatureInfo featureInfo, out float netStDev, out float discriminantScore)
            {
                // Return the feature info for featureID

                var matchFound = ContainsFeature(featureID, out var rowIndex);
                if (matchFound)
                {
                    featureInfo = mFeatures[rowIndex];
                    netStDev = mExtendedInfo[rowIndex].NETStDev;
                    discriminantScore = mExtendedInfo[rowIndex].DiscriminantScore;
                    return true;
                }

                featureInfo = FeatureInfo.Blank();
                netStDev = 0f;
                discriminantScore = 0f;
                return false;
            }

            public bool GetFeatureInfoByRowIndex(int rowIndex, out FeatureInfo featureInfo, out float netStDev, out float discriminantScore)
            {
                if (rowIndex >= 0 && rowIndex < mFeatureCount)
                {
                    featureInfo = mFeatures[rowIndex];
                    netStDev = mExtendedInfo[rowIndex].NETStDev;
                    discriminantScore = mExtendedInfo[rowIndex].DiscriminantScore;
                    return true;
                }

                featureInfo = FeatureInfo.Blank();
                netStDev = 0f;
                discriminantScore = 0f;
                return false;
            }

            public virtual float GetNETStDevByRowIndex(int rowIndex)
            {
                if (rowIndex >= 0 && rowIndex < mFeatureCount)
                {
                    return mExtendedInfo[rowIndex].NETStDev;
                }

                return 0f;
            }
        }

        internal class PMFeatureMatchResults
        {
            public readonly struct PeakMatchingResult
            {
                public int MatchingID { get; }                // ID of the comparison feature (this is the real ID, and not a RowIndex)
                public double SLiCScore { get; }                  // SLiC Score (Spatially Localized Confidence score)
                public double DelSLiC { get; }                    // Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
                public double MassErr { get; }                    // Observed difference (error) between comparison mass and feature mass (in Da)
                public double NETErr { get; }                     // Observed difference (error) between comparison NET and feature NET
                public int MultiAMTHitCount { get; }          // The number of Unique mass tag hits for each UMC; only applies to AMT's

                public PeakMatchingResult(int matchingId, double sliCScore, double delSLiC, double massErr, double netErr, int multiAmtHitCount)
                {
                    MatchingID = matchingId;
                    SLiCScore = sliCScore;
                    DelSLiC = delSLiC;
                    MassErr = massErr;
                    NETErr = netErr;
                    MultiAMTHitCount = multiAmtHitCount;
                }
            }

            protected readonly struct PeakMatchingResults : IComparable<PeakMatchingResults>
            {
                public int FeatureID { get; }
                public PeakMatchingResult Details { get; }

                public PeakMatchingResults(int featureId, PeakMatchingResult details)
                {
                    FeatureID = featureId;
                    Details = details;
                }

                public int CompareTo(PeakMatchingResults other)
                {
                    // Sort by .FeatureID ascending, and by .Details.MatchingID ascending
                    var featureIdComparison = FeatureID.CompareTo(other.FeatureID);
                    if (featureIdComparison != 0) return featureIdComparison;
                    return Details.MatchingID.CompareTo(other.Details.MatchingID);
                }
            }

            protected readonly List<PeakMatchingResults> mPMResults = new List<PeakMatchingResults>();
            protected bool mPMResultsIsSorted;

            public event SortingListEventHandler SortingList;

            public PMFeatureMatchResults()
            {
                Clear();
            }

            public bool AddMatch(int featureID, ref PeakMatchingResult matchResultInfo)
            {
                return AddMatch(featureID, matchResultInfo.MatchingID, matchResultInfo.SLiCScore, matchResultInfo.DelSLiC, matchResultInfo.MassErr, matchResultInfo.NETErr, matchResultInfo.MultiAMTHitCount);
            }

            public bool AddMatch(int featureID, int matchingID, double slicScore, double delSLiC, double massErr, double netErr, int multiAMTHitCount)
            {
                // Add the match
                var details = new PeakMatchingResult(matchingID, slicScore, delSLiC, massErr, netErr, multiAMTHitCount);
                mPMResults.Add(new PeakMatchingResults(featureID, details));
                mPMResultsIsSorted = false;

                // If we get here, all went well
                return true;
            }

            private int BinarySearchPMResults(int featureIDToFind)
            {
                // Looks through mPMResults() for featureIDToFind, returning the index of the item if found, or -1 if not found
                // Since mPMResults() can contain multiple entries for a given Feature, this function returns the first entry found

                var firstIndex = 0;
                var lastIndex = mPMResults.Count - 1;
                var matchingRowIndex = -1;
                if (mPMResults.Count <= 0 || !SortPMResults())
                {
                    return matchingRowIndex;
                }

                try
                {
                    var midIndex = (firstIndex + lastIndex) / 2;
                    if (midIndex < firstIndex)
                        midIndex = firstIndex;
                    while (firstIndex <= lastIndex && mPMResults[midIndex].FeatureID != featureIDToFind)
                    {
                        if (featureIDToFind < mPMResults[midIndex].FeatureID)
                        {
                            // Search the lower half
                            lastIndex = midIndex - 1;
                        }
                        else if (featureIDToFind > mPMResults[midIndex].FeatureID)
                        {
                            // Search the upper half
                            firstIndex = midIndex + 1;
                        }
                        // Compute the new mid point
                        midIndex = (firstIndex + lastIndex) / 2;
                        if (midIndex < firstIndex)
                            break;
                    }

                    if (midIndex >= firstIndex && midIndex <= lastIndex)
                    {
                        if (mPMResults[midIndex].FeatureID == featureIDToFind)
                        {
                            matchingRowIndex = midIndex;
                        }
                    }
                }
                catch
                {
                    matchingRowIndex = -1;
                }

                return matchingRowIndex;
            }

            public virtual void Clear()
            {
                mPMResults.Clear();
                mPMResultsIsSorted = false;
            }

            public int Count => mPMResults.Count;

            public bool GetMatchInfoByFeatureID(int featureID, ref PeakMatchingResult[] matchResults, ref int matchCount)
            {
                // Returns all of the matches for the given feature ID row index
                // Returns false if the feature has no matches
                // Note that this function never shrinks matchResults; it only expands it if needed

                var matchesFound = false;
                try
                {
                    if (GetRowIndicesForFeatureID(featureID, out var indexFirst, out var indexLast))
                    {
                        matchCount = indexLast - indexFirst + 1;
                        if (matchResults == null || matchCount > matchResults.Length)
                        {
                            matchResults = new PeakMatchingResult[matchCount];
                        }

                        for (var index = indexFirst; index <= indexLast; index++)
                            matchResults[index - indexFirst] = mPMResults[index].Details;
                        matchesFound = true;
                    }
                }
                catch
                {
                    matchesFound = false;
                }

                return matchesFound;
            }

            public bool GetMatchInfoByRowIndex(int rowIndex, ref int featureID, ref PeakMatchingResult matchResultInfo)
            {
                // Populates featureID and matchResultInfo with the peak matching results for the given row index

                var matchFound = false;
                try
                {
                    if (rowIndex < mPMResults.Count)
                    {
                        featureID = mPMResults[rowIndex].FeatureID;
                        matchResultInfo = mPMResults[rowIndex].Details;
                        matchFound = true;
                    }
                }
                catch
                {
                    matchFound = false;
                }

                return matchFound;
            }

            private bool GetRowIndicesForFeatureID(int featureID, out int indexFirst, out int indexLast)
            {
                // Looks for featureID in mPMResults
                // If found, returns the range of rows that contain matches for featureID

                // Perform a binary search of mPMResults for featureID
                indexFirst = BinarySearchPMResults(featureID);
                if (indexFirst >= 0)
                {
                    // Match found; need to find all of the rows with featureID
                    indexLast = indexFirst;

                    // Step backward through mPMResults to find the first match for featureID
                    while (indexFirst > 0 && mPMResults[indexFirst - 1].FeatureID == featureID)
                        indexFirst -= 1;

                    // Step forward through mPMResults to find the last match for featureID
                    while (indexLast < mPMResults.Count - 1 && mPMResults[indexLast + 1].FeatureID == featureID)
                        indexLast += 1;
                    return true;
                }

                indexFirst = -1;
                indexLast = -1;
                return false;
            }

            public int get_MatchCountForFeatureID(int featureID)
            {
                if (GetRowIndicesForFeatureID(featureID, out var indexFirst, out var indexLast))
                {
                    return indexLast - indexFirst + 1;
                }

                return 0;
            }

            private bool SortPMResults(bool forceSort = false)
            {
                if (!mPMResultsIsSorted || forceSort)
                {
                    SortingList?.Invoke();
                    mPMResults.Sort();

                    mPMResultsIsSorted = true;
                }

                return mPMResultsIsSorted;
            }
        }

        private int mMaxPeakMatchingResultsPerFeatureToSave;
        private readonly SearchModeOptions mSearchModeOptions = new SearchModeOptions();
        private bool mAbortProcessing;

        public event ProgressContinuesEventHandler ProgressContinues;
        public event LogEventEventHandler LogEvent;

        public int MaxPeakMatchingResultsPerFeatureToSave
        {
            get => mMaxPeakMatchingResultsPerFeatureToSave;
            set
            {
                if (value < 0)
                    value = 0;
                mMaxPeakMatchingResultsPerFeatureToSave = value;
            }
        }

        public string ProgressDescription { get; private set; }

        public float ProgressPct { get; private set; }

        public bool UseMaxSearchDistanceMultiplierAndSLiCScore
        {
            get => mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore;
            set => mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore = value;
        }

        public bool UseEllipseSearchRegion
        {
            get => mSearchModeOptions.UseEllipseSearchRegion;
            set => mSearchModeOptions.UseEllipseSearchRegion = value;
        }

        // public string SqlServerConnectionString
        // {
        //     get => mSqlServerConnectionString;
        //     set => mSqlServerConnectionString = value;
        // }

        // public string SqlServerTableNameFeatureMatchResults
        // {
        //     get => mTableNameFeatureMatchResults;
        //     set => mTableNameFeatureMatchResults = value;
        // }

        // public bool UseSqlServerDBToCacheData
        // {
        //     get => mUseSqlServerDBToCacheData;
        //     set => mUseSqlServerDBToCacheData = value;
        // }

        // public bool UseSqlServerForMatchResults
        // {
        //     get => mUseSqlServerForMatchResults;
        //     set => mUseSqlServerForMatchResults = value;
        // }

        public void AbortProcessingNow()
        {
            mAbortProcessing = true;
        }

        private void ComputeSLiCScores(ref FeatureInfo featureToIdentify, ref PMFeatureMatchResults featureMatchResults, List<PeakMatchingRawMatches> rawMatches, PMComparisonFeatureInfo comparisonFeatures, ref SearchThresholds searchThresholds, SearchThresholds.SearchTolerances computedTolerances)
        {
            int index;
            string message;

            // Compute the match scores (aka SLiC scores)

            var massStDevPPM = searchThresholds.SLiCScoreMassPPMStDev;
            if (massStDevPPM <= 0d)
                massStDevPPM = 3d;
            var massStDevAbs = searchThresholds.PPMToMass(massStDevPPM, featureToIdentify.Mass);
            if (massStDevAbs <= 0d)
            {
                message = "Assertion failed in ComputeSLiCScores; massStDevAbs is <= 0, which isn't allowed; will assume 0.003";
                Console.WriteLine(message);
                PostLogEntry(message, MessageTypeConstants.ErrorMsg);
                massStDevAbs = 0.003d;
            }

            // Compute the standardized squared distance and the numerator sum
            var numeratorSum = 0d;
            for (index = 0; index < rawMatches.Count; index++)
            {
                double netStDevCombined;
                if (searchThresholds.SLiCScoreUseAMTNETStDev)
                {
                    // The NET StDev is computed by combining the default NETStDev value with the Comparison Features' specific NETStDev
                    // The combining is done by "adding in quadrature", which means to square each number, add together, and take the square root
                    netStDevCombined = Math.Sqrt(Math.Pow(searchThresholds.SLiCScoreNETStDev, 2d) + Math.Pow(comparisonFeatures.GetNETStDevByRowIndex(rawMatches[index].MatchingIDIndex), 2d));
                }
                else
                {
                    // Simply use the default NETStDev value
                    netStDevCombined = searchThresholds.SLiCScoreNETStDev;
                }

                if (netStDevCombined <= 0d)
                {
                    message = "Assertion failed in ComputeSLiCScores; netStDevCombined is <= 0, which isn't allowed; will assume 0.025";
                    Console.WriteLine(message);
                    PostLogEntry(message, MessageTypeConstants.ErrorMsg);
                    netStDevCombined = 0.025d;
                }

                rawMatches[index].StandardizedSquaredDistance = Math.Pow(rawMatches[index].MassErr, 2d) / Math.Pow(massStDevAbs, 2d) + Math.Pow(rawMatches[index].NETErr, 2d) / Math.Pow(netStDevCombined, 2d);
                rawMatches[index].SLiCScoreNumerator = 1d / (massStDevAbs * netStDevCombined) * Math.Exp(-rawMatches[index].StandardizedSquaredDistance / 2d);
                numeratorSum += rawMatches[index].SLiCScoreNumerator;
            }

            // Compute the match score for each match
            for (index = 0; index < rawMatches.Count; index++)
            {
                if (numeratorSum > 0d)
                {
                    rawMatches[index].SLiCScore = Math.Round(rawMatches[index].SLiCScoreNumerator / numeratorSum, 5);
                }
                else
                {
                    rawMatches[index].SLiCScore = 0d;
                }
            }

            if (rawMatches.Count > 1)
            {
                // Sort by SLiCScore descending
                rawMatches.Sort();
            }

            if (rawMatches.Count > 0)
            {
                // Compute the DelSLiC value
                // If there is only one match, then the DelSLiC value is 1
                // If there is more than one match, then the highest scoring match gets a DelSLiC value,
                // computed by subtracting the next lower scoring value from the highest scoring value; all
                // other matches get a DelSLiC score of 0
                // This allows one to quickly identify the features with a single match (DelSLiC = 1) or with a match
                // distinct from other matches (DelSLiC > threshold)

                if (rawMatches.Count > 1)
                {
                    rawMatches[0].DelSLiC = rawMatches[0].SLiCScore - rawMatches[1].SLiCScore;
                    for (index = 1; index < rawMatches.Count; index++)
                        rawMatches[index].DelSLiC = 0d;
                }
                else
                {
                    rawMatches[0].DelSLiC = 1d;
                }

                // Now filter the list using the tighter tolerances:
                // Since we're shrinking the array, we can copy in place
                //
                // When testing whether to keep the match or not, we're testing whether the match is in the ellipse bounded by MWTolAbsFinal and NETTolFinal
                // Note that these are half-widths of the ellipse
                var newMatches = new List<PeakMatchingRawMatches>();
                for (index = 0; index < rawMatches.Count; index++)
                {
                    if (TestPointInEllipse(rawMatches[index].NETErr, rawMatches[index].MassErr, computedTolerances.NETTolFinal, computedTolerances.MWTolAbsFinal))
                    {
                        newMatches.Add(rawMatches[index]);
                    }
                }

                rawMatches.Clear();
                rawMatches.AddRange(newMatches);

                // Add new match results to featureMatchResults
                // Record, at most, mMaxPeakMatchingResultsPerFeatureToSave entries
                for (index = 0; index < Math.Min(mMaxPeakMatchingResultsPerFeatureToSave, rawMatches.Count); index++)
                {
                    comparisonFeatures.GetFeatureInfoByRowIndex(rawMatches[index].MatchingIDIndex, out var comparisonFeatureInfo);
                    featureMatchResults.AddMatch(featureToIdentify.FeatureID, comparisonFeatureInfo.FeatureID, rawMatches[index].SLiCScore, rawMatches[index].DelSLiC, rawMatches[index].MassErr, rawMatches[index].NETErr, rawMatches.Count);
                }
            }
        }

        internal static bool FillRangeSearchObject(ref SearchRange rangeSearch, PMComparisonFeatureInfo comparisonFeatures)
        {
            // Initialize the range searching class

            const int LOAD_BLOCK_SIZE = 50000;
            bool success;
            if (rangeSearch == null)
            {
                rangeSearch = new SearchRange();
            }
            else
            {
                rangeSearch.ClearData();
            }

            if (comparisonFeatures.Count <= 0)
            {
                // No comparison features to search against
                success = false;
            }
            else
            {
                rangeSearch.InitializeDataFillDouble(comparisonFeatures.Count);

                var index = 0;
                // for (index = 0; i < comparisonFeatures.Count; i++)
                // {
                //     rangeSearch.FillWithDataAddPoint(comparisonFeatures.GetMassByRowIndex(index));
                // }

                var comparisonFeatureCount = comparisonFeatures.Count;
                while (index < comparisonFeatureCount)
                {
                    rangeSearch.FillWithDataAddBlock(comparisonFeatures.GetMassArrayByRowRange(index, index + LOAD_BLOCK_SIZE - 1));
                    index += LOAD_BLOCK_SIZE;
                }

                success = rangeSearch.FinalizeDataFill();
            }

            return success;
        }

        internal bool IdentifySequences(SearchThresholds searchThresholds, ref PMFeatureInfo featuresToIdentify, PMComparisonFeatureInfo comparisonFeatures, out PMFeatureMatchResults featureMatchResults, ref SearchRange rangeSearch)
        {
            // Returns True if success, False if the search is canceled
            // Will return true even if none of the features match any of the comparison features
            //
            // If rangeSearch is Nothing or if rangeSearch contains a different number of entries than comparisonFeatures,
            // then will auto-populate it; otherwise, assumes it is populated

            // Note that featureMatchResults will only contain info on the features in featuresToIdentify that matched entries in comparisonFeatures

            bool success;

            // if (mUseSqlServerForMatchResults)
            //     featureMatchResults = new PMFeatureMatchResults(mSqlServerConnectionString, mTableNameFeatureMatchResults);
            // else
            featureMatchResults = new PMFeatureMatchResults();

            if (rangeSearch == null || rangeSearch.DataCount != comparisonFeatures.Count)
            {
                success = FillRangeSearchObject(ref rangeSearch, comparisonFeatures);
            }
            else
            {
                success = true;
            }

            if (!success)
                return false;
            try
            {
                var featureCount = featuresToIdentify.Count;
                UpdateProgress("Finding matching peptides for given search thresholds", 0f);
                mAbortProcessing = false;
                PostLogEntry("IdentifySequences starting, total feature count = " + featureCount, MessageTypeConstants.Normal);
                int featureIndex;
                for (featureIndex = 0; featureIndex < featureCount; featureIndex++)
                {
                    // Use rangeSearch to search for matches to each peptide in comparisonFeatures

                    if (featuresToIdentify.GetFeatureInfoByRowIndex(featureIndex, out var currentFeatureToIdentify))
                    {
                        // By Calling .ComputedSearchTolerances() with a mass, the tolerances will be auto re-computed
                        var computedTolerances = searchThresholds.get_ComputedSearchTolerances(currentFeatureToIdentify.Mass);
                        double netTol;
                        double massTol;
                        if (mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore)
                        {
                            massTol = computedTolerances.MWTolAbsBroad;
                            netTol = computedTolerances.NETTolBroad;
                        }
                        else
                        {
                            massTol = computedTolerances.MWTolAbsFinal;
                            netTol = computedTolerances.NETTolFinal;
                        }

                        var matchInd1 = 0;
                        var matchInd2 = -1;
                        if (rangeSearch.FindValueRange(currentFeatureToIdentify.Mass, massTol, ref matchInd1, ref matchInd2))
                        {
                            // The following hold the matches using the broad search tolerances (if .UseMaxSearchDistanceMultiplierAndSLiCScore = True, otherwise, simply holds the matches)
                            // Pointers into comparisonFeatures; list of peptides that match within both mass and NET tolerance
                            var rawMatches = new List<PeakMatchingRawMatches>();
                            int matchIndex;
                            for (matchIndex = matchInd1; matchIndex <= matchInd2; matchIndex++)
                            {
                                var comparisonFeaturesOriginalRowIndex = rangeSearch.get_OriginalIndex(matchIndex);
                                if (comparisonFeatures.GetFeatureInfoByRowIndex(comparisonFeaturesOriginalRowIndex, out var currentComparisonFeature))
                                {
                                    double netDiff = currentFeatureToIdentify.NET - currentComparisonFeature.NET;
                                    if (Math.Abs(netDiff) <= netTol)
                                    {
                                        bool storeMatch;
                                        if (mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore)
                                        {
                                            // Store this match
                                            storeMatch = true;
                                        }
                                        // The match is within a rectangle defined by computedTolerances.MWTolAbsBroad and computedTolerances.NETTolBroad
                                        else if (mSearchModeOptions.UseEllipseSearchRegion)
                                        {
                                            // Only keep the match if it's within the ellipse defined by the search tolerances
                                            // Note that the search tolerances we send to TestPointInEllipse should be half-widths (i.e. tolerance +- comparison value), not full widths
                                            storeMatch = TestPointInEllipse(netDiff, currentFeatureToIdentify.Mass - currentComparisonFeature.Mass, netTol, massTol);
                                        }
                                        else
                                        {
                                            storeMatch = true;
                                        }

                                        if (storeMatch)
                                        {
                                            var rawMatch =  new PeakMatchingRawMatches(comparisonFeaturesOriginalRowIndex)
                                            {
                                                SLiCScore = -1,
                                                MassErr = currentFeatureToIdentify.Mass -
                                                          currentComparisonFeature.Mass,
                                                NETErr = netDiff
                                            };


                                            rawMatches.Add(rawMatch);
                                        }
                                    }
                                }
                            }

                            if (rawMatches.Count > 0)
                            {
                                rawMatches.Capacity = rawMatches.Count;
                                // Store the FeatureIDIndex in featureMatchResults
                                // Compute the SLiC Scores and store the results
                                ComputeSLiCScores(ref currentFeatureToIdentify, ref featureMatchResults, rawMatches, comparisonFeatures, ref searchThresholds, computedTolerances);
                            }
                        }
                    }
                    else
                    {
                        var message = "Programming error in IdentifySequences: Feature not found in featuresToIdentify using feature index: " + featureIndex;
                        Console.WriteLine(message);
                        PostLogEntry(message, MessageTypeConstants.ErrorMsg);
                    }

                    if (featureIndex % 100 == 0)
                    {
                        UpdateProgress((float)(featureIndex / (double)featureCount * 100d));
                        if (mAbortProcessing)
                            break;
                    }

                    if (featureIndex % 10000 == 0 && featureIndex > 0)
                    {
                        PostLogEntry("IdentifySequences, featureIndex = " + featureIndex, MessageTypeConstants.Health);
                    }
                }

                UpdateProgress("IdentifySequences complete", 100f);
                PostLogEntry("IdentifySequences complete", MessageTypeConstants.Normal);
                success = !mAbortProcessing;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        private void InitializeLocalVariables()
        {
            mMaxPeakMatchingResultsPerFeatureToSave = 20;
            mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore = true;
            mSearchModeOptions.UseEllipseSearchRegion = true;

            // mUseSqlServerDBToCacheData = false
            // mUseSqlServerForMatchResults = false
            // mSqlServerConnectionString = SharedADONetFunctions.DEFAULT_CONNECTION_STRING_NO_PROVIDER;
            // mTableNameFeatureMatchResults = PMFeatureMatchResults.DEFAULT_FEATURE_MATCH_RESULTS_TABLE_NAME;
        }

        private void PostLogEntry(string message, MessageTypeConstants EntryType)
        {
            LogEvent?.Invoke(message, EntryType);
        }

        private bool TestPointInEllipse(double pointX, double pointY, double xTol, double yTol)
        {
            // The equation for the points along the edge of an ellipse is x^2/a^2 + y^2/b^2 = 1 where a and b are
            // the half-widths of the ellipse and x and y are the coordinates of each point on the ellipse's perimeter
            //
            // This function takes x, y, a, and b as inputs and computes the result of this equation
            // If the result is <= 1, then the point at x,y is inside the ellipse

            try
            {
                return Math.Pow(pointX, 2d) / Math.Pow(xTol, 2d) + Math.Pow(pointY, 2d) / Math.Pow(yTol, 2d) <= 1d;
            }
            catch
            {
                // Error; return false
                return false;
            }
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="progressPercent">Value between 0 and 100</param>
        private void UpdateProgress(float progressPercent)
        {
            ProgressPct = progressPercent;
            ProgressContinues?.Invoke();
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="description">Progress description</param>
        /// <param name="progressPercent">Value between 0 and 100</param>
        private void UpdateProgress(string description, float progressPercent)
        {
            ProgressDescription = description;
            ProgressPct = progressPercent;
            ProgressContinues?.Invoke();
        }

        public class SearchThresholds
        {
            public SearchThresholds()
            {
                InitializeLocalVariables();
            }

            public enum MassToleranceConstants
            {
                PPM = 0,             // parts per million
                Absolute = 1        // absolute (Da)
            }

            // The following defines how the SLiC scores (aka match scores) are computed
            private class SLiCScoreOptions
            {
                public double MassPPMStDev { get; set; }                  // Default 3
                public double NETStDev { get; set; }                      // Default 0.025
                public bool UseAMTNETStDev { get; set; }
                public float MaxSearchDistanceMultiplier { get; set; }   // Default 2
            }

            // Note that all of these tolerances are half-widths, i.e. tolerance +- comparison value
            public class SearchTolerances
            {
                public double MWTolAbsBroad { get; set; }
                public double MWTolAbsFinal { get; set; }
                public double NETTolBroad { get; set; }
                public double NETTolFinal { get; set; }
            }

            private double mMassTolerance;          // Mass search tolerance, +- this value; TolType defines if this is PPM or Da
            private double mNETTolerance;           // NET search tolerance, +- this value
            private float mSLiCScoreMaxSearchDistanceMultiplier;
            private readonly SLiCScoreOptions mSLiCScoreOptions = new SLiCScoreOptions();

            public bool AutoDefineSLiCScoreThresholds { get; set; }

            public SearchTolerances ComputedSearchTolerances { get; } = new SearchTolerances();

            public SearchTolerances get_ComputedSearchTolerances(double referenceMass)
            {
                DefinePeakMatchingTolerances(ref referenceMass);
                return ComputedSearchTolerances;
            }

            public MassToleranceConstants MassTolType { get; set; }

            public double MassTolerance
            {
                get => mMassTolerance;
                set
                {
                    mMassTolerance = value;
                    if (AutoDefineSLiCScoreThresholds)
                    {
                        InitializeSLiCScoreOptions(true);
                    }
                }
            }

            public double NETTolerance
            {
                get => mNETTolerance;
                set
                {
                    mNETTolerance = value;
                    if (AutoDefineSLiCScoreThresholds)
                    {
                        InitializeSLiCScoreOptions(true);
                    }
                }
            }

            public double SLiCScoreMassPPMStDev
            {
                get => mSLiCScoreOptions.MassPPMStDev;
                set
                {
                    if (value < 0d)
                        value = 0d;
                    mSLiCScoreOptions.MassPPMStDev = value;
                }
            }

            public double SLiCScoreNETStDev
            {
                get => mSLiCScoreOptions.NETStDev;
                set
                {
                    if (value < 0d)
                        value = 0d;
                    mSLiCScoreOptions.NETStDev = value;
                }
            }

            public bool SLiCScoreUseAMTNETStDev
            {
                get => mSLiCScoreOptions.UseAMTNETStDev;
                set => mSLiCScoreOptions.UseAMTNETStDev = value;
            }

            public float SLiCScoreMaxSearchDistanceMultiplier
            {
                get => mSLiCScoreMaxSearchDistanceMultiplier;
                set
                {
                    if (value < 1f)
                        value = 1f;
                    mSLiCScoreMaxSearchDistanceMultiplier = value;
                    if (AutoDefineSLiCScoreThresholds)
                    {
                        InitializeSLiCScoreOptions(true);
                    }
                }
            }

            public void DefinePeakMatchingTolerances(ref double referenceMass)
            {
                // Thresholds are all half-widths; i.e. tolerance +- comparison value

                var MWTolPPMBroad = default(double);
                switch (MassTolType)
                {
                    case MassToleranceConstants.PPM:
                        ComputedSearchTolerances.MWTolAbsFinal = PPMToMass(mMassTolerance, referenceMass);
                        MWTolPPMBroad = mMassTolerance;
                        break;
                    case MassToleranceConstants.Absolute:
                        ComputedSearchTolerances.MWTolAbsFinal = mMassTolerance;
                        if (referenceMass > 0d)
                        {
                            MWTolPPMBroad = MassToPPM(mMassTolerance, referenceMass);
                        }
                        else
                        {
                            MWTolPPMBroad = mSLiCScoreOptions.MassPPMStDev;
                        }

                        break;
                    default:
                        Console.WriteLine("Programming error in DefinePeakMatchingTolerances; Unknown MassToleranceType: " + MassTolType);
                        break;
                }

                if (MWTolPPMBroad < mSLiCScoreOptions.MassPPMStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR)
                {
                    MWTolPPMBroad = mSLiCScoreOptions.MassPPMStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR;
                }

                ComputedSearchTolerances.NETTolBroad = mSLiCScoreOptions.NETStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR;
                if (ComputedSearchTolerances.NETTolBroad < mNETTolerance)
                {
                    ComputedSearchTolerances.NETTolBroad = mNETTolerance;
                }

                ComputedSearchTolerances.NETTolFinal = mNETTolerance;

                // Convert from PPM to Absolute mass
                ComputedSearchTolerances.MWTolAbsBroad = PPMToMass(MWTolPPMBroad, referenceMass);
            }

            private void InitializeSLiCScoreOptions(bool computeUsingSearchThresholds)
            {
                if (computeUsingSearchThresholds)
                {
                    // Define the Mass StDev (in ppm) using the narrow mass tolerance divided by 2 = STDEV_SCALING_FACTOR
                    switch (MassTolType)
                    {
                        case MassToleranceConstants.Absolute:
                            mSLiCScoreOptions.MassPPMStDev = MassToPPM(mMassTolerance, 1000d) / STDEV_SCALING_FACTOR;
                            break;
                        case MassToleranceConstants.PPM:
                            mSLiCScoreOptions.MassPPMStDev = mMassTolerance / STDEV_SCALING_FACTOR;
                            break;
                        default:
                            // Unknown type
                            mSLiCScoreOptions.MassPPMStDev = 3d;
                            break;
                    }

                    // Define the Net StDev using the narrow NET tolerance divided by 2 = STDEV_SCALING_FACTOR
                    mSLiCScoreOptions.NETStDev = mNETTolerance / STDEV_SCALING_FACTOR;
                }
                else
                {
                    mSLiCScoreOptions.MassPPMStDev = 3d;
                    mSLiCScoreOptions.NETStDev = 0.025d;
                }

                mSLiCScoreOptions.UseAMTNETStDev = false;
                mSLiCScoreOptions.MaxSearchDistanceMultiplier = mSLiCScoreMaxSearchDistanceMultiplier;
                if (mSLiCScoreOptions.MaxSearchDistanceMultiplier < 1f)
                {
                    mSLiCScoreOptions.MaxSearchDistanceMultiplier = DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER;
                }
            }

            private void InitializeLocalVariables()
            {
                AutoDefineSLiCScoreThresholds = true;
                MassTolType = MassToleranceConstants.PPM;
                mMassTolerance = 5d;
                mNETTolerance = 0.05d;
                mSLiCScoreMaxSearchDistanceMultiplier = DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER;
                InitializeSLiCScoreOptions(AutoDefineSLiCScoreThresholds);
            }

            public double MassToPPM(double MassToConvert, double ReferenceMZ)
            {
                // Converts MassToConvert to ppm, which is dependent on ReferenceMZ
                return MassToConvert * ONE_PART_PER_MILLION / ReferenceMZ;
            }

            public double PPMToMass(double PPMToConvert, double ReferenceMZ)
            {
                // Converts PPMToConvert to a mass value, which is dependent on ReferenceMZ
                return PPMToConvert / ONE_PART_PER_MILLION * ReferenceMZ;
            }

            public void ResetToDefaults()
            {
                InitializeLocalVariables();
            }
        }
    }
}