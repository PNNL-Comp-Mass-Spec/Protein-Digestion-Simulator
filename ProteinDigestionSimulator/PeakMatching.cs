﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.CompilerServices;

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

        public struct FeatureInfo
        {
            public int FeatureID;                                   // Each feature should have a unique ID
            public string FeatureName;                                // Optional
            public double Mass;
            public float NET;
        }

        private struct PeakMatchingRawMatches
        {
            public int MatchingIDIndex;               // Pointer into comparison features (RowIndex in PMComparisonFeatureInfo)
            public double StandardizedSquaredDistance;
            public double SLiCScoreNumerator;
            public double SLiCScore;                      // SLiC Score (Spatially Localized Confidence score)
            public double DelSLiC;                        // Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
            public double MassErr;                        // Observed difference (error) between comparison mass and feature mass (in Da)
            public double NETErr;                         // Observed difference (error) between comparison NET and feature NET
        }

        private struct SearchModeOptions
        {
            public bool UseMaxSearchDistanceMultiplierAndSLiCScore;
            public bool UseEllipseSearchRegion;        // Only valid if UseMaxSearchDistanceMultiplierAndSLiCScore = False; if both UseMaxSearchDistanceMultiplierAndSLiCScore = False and UseEllipseSearchRegion = False, then uses a rectangle for peak matching
        }

        internal class PMFeatureInfo
        {
            protected const int MEMORY_RESERVE_CHUNK = 100000;
            protected int mFeatureCount;
            protected FeatureInfo[] mFeatures;
            protected bool mFeaturesArrayIsSorted;
            protected bool mUseFeatureIDHashTable;
            protected Hashtable featureIDToRowIndex;

            public event SortingListEventHandler SortingList;

            public delegate void SortingListEventHandler();

            public PMFeatureInfo()
            {
                mUseFeatureIDHashTable = true;                       // Set this to False to conserve memory; you must call Clear() after changing this for it to take effect
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
                else
                {
                    // Add the feature
                    if (mFeatureCount >= mFeatures.Length)
                    {
                        Array.Resize(ref mFeatures, mFeatures.Length * 2);
                        // ReDim Preserve mFeatures(mFeatures.Length + MEMORY_RESERVE_CHUNK - 1)
                    }

                    mFeatures[mFeatureCount].FeatureID = featureID;
                    mFeatures[mFeatureCount].FeatureName = peptideName;
                    mFeatures[mFeatureCount].Mass = peptideMass;
                    mFeatures[mFeatureCount].NET = peptideNET;
                    if (mUseFeatureIDHashTable)
                    {
                        featureIDToRowIndex.Add(featureID, mFeatureCount);
                    }

                    mFeatureCount += 1;
                    mFeaturesArrayIsSorted = false;

                    // If we get here, all went well
                    return true;
                }
            }

            protected int BinarySearchFindFeature(int featureIDToFind)
            {
                // Looks through mFeatures() for featureIDToFind, returning the index of the item if found, or -1 if not found

                int midIndex;
                int firstIndex = 0;
                int lastIndex = mFeatureCount - 1;
                int matchingRowIndex = -1;
                if (mFeatureCount <= 0 || !SortFeatures())
                {
                    return matchingRowIndex;
                }

                try
                {
                    midIndex = (firstIndex + lastIndex) / 2;            // Note: Using Integer division
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
                catch (Exception ex)
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
                if (mUseFeatureIDHashTable)
                {
                    if (featureIDToRowIndex == null)
                    {
                        featureIDToRowIndex = new Hashtable();
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
                int argrowIndex = 0;
                return ContainsFeature(featureID, out argrowIndex);
            }

            protected bool ContainsFeature(int featureID, out int rowIndex)
            {
                // Returns True if the features array contains the feature
                // If found, returns the row index in rowIndex
                // Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

                if (mUseFeatureIDHashTable)
                {
                    if (featureIDToRowIndex.Contains(featureID))
                    {
                        rowIndex = Conversions.ToInteger(featureIDToRowIndex[featureID]);
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

                if (rowIndex >= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int Count
            {
                get
                {
                    return mFeatureCount;
                }
            }

            public virtual bool GetFeatureInfoByFeatureID(int featureID, ref FeatureInfo featureInfo)
            {
                // Return the feature info for featureID

                int rowIndex;
                bool matchFound = ContainsFeature(featureID, out rowIndex);
                if (matchFound)
                {
                    featureInfo = mFeatures[rowIndex];
                    return true;
                }
                else
                {
                    featureInfo.FeatureID = 0;
                    featureInfo.FeatureName = string.Empty;
                    featureInfo.Mass = 0d;
                    featureInfo.NET = 0f;
                    return false;
                }
            }

            public virtual bool GetFeatureInfoByRowIndex(int rowIndex, ref FeatureInfo featureInfo)
            {
                if (rowIndex >= 0 && rowIndex < mFeatureCount)
                {
                    featureInfo = mFeatures[rowIndex];
                    return true;
                }
                else
                {
                    featureInfo.FeatureID = 0;
                    featureInfo.FeatureName = string.Empty;
                    featureInfo.Mass = 0d;
                    featureInfo.NET = 0f;
                    return false;
                }
            }

            public virtual double[] GetMassArrayByRowRange(int rowIndexStart, int rowIndexEnd)
            {
                double[] masses;
                int matchCount;
                if (rowIndexEnd < rowIndexStart)
                {
                    masses = new double[0];
                    return masses;
                }

                masses = new double[rowIndexEnd - rowIndexStart + 1];
                int index;
                try
                {
                    if (rowIndexEnd >= mFeatureCount)
                    {
                        rowIndexEnd = mFeatureCount - 1;
                    }

                    matchCount = 0;
                    for (index = rowIndexStart; index <= rowIndexEnd; index++)
                    {
                        masses[matchCount] = mFeatures[index].Mass;
                        matchCount += 1;
                    }

                    if (masses.Length > matchCount)
                    {
                        Array.Resize(ref masses, matchCount);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

                return masses;
            }

            public virtual double GetMassByRowIndex(int rowIndex)
            {
                if (rowIndex >= 0 && rowIndex < mFeatureCount)
                {
                    return mFeatures[rowIndex].Mass;
                }
                else
                {
                    return 0d;
                }
            }

            protected virtual bool SortFeatures(bool forceSort = false)
            {
                if (!mFeaturesArrayIsSorted || forceSort)
                {
                    SortingList?.Invoke();
                    try
                    {
                        var comparer = new FeatureInfoComparer();
                        Array.Sort(mFeatures, 0, mFeatureCount, comparer);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    mFeaturesArrayIsSorted = true;
                }

                return mFeaturesArrayIsSorted;
            }

            public bool UseFeatureIDHashTable
            {
                get
                {
                    return mUseFeatureIDHashTable;
                }

                set
                {
                    mUseFeatureIDHashTable = value;
                }
            }

            private class FeatureInfoComparer : IComparer<FeatureInfo>
            {
                public int Compare(FeatureInfo x, FeatureInfo y)
                {
                    // Sort by Feature ID, ascending

                    if (x.FeatureID > y.FeatureID)
                    {
                        return 1;
                    }
                    else if (x.FeatureID < y.FeatureID)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        internal class PMComparisonFeatureInfo : PMFeatureInfo
        {
            protected struct ComparisonFeatureInfoExtended
            {
                public float NETStDev;
                public float DiscriminantScore;
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
                else
                {
                    // Add the extended feature info
                    if (mExtendedInfo.Length < mFeatures.Length)
                    {
                        Array.Resize(ref mExtendedInfo, mFeatures.Length);
                    }

                    mExtendedInfo[mFeatureCount - 1].NETStDev = peptideNETStDev;
                    mExtendedInfo[mFeatureCount - 1].DiscriminantScore = peptideDiscriminantScore;

                    // If we get here, all went well
                    return true;
                }
            }

            public new void Clear()
            {
                base.Clear();
                if (mExtendedInfo == null)
                {
                    mExtendedInfo = new ComparisonFeatureInfoExtended[100000];
                }
            }

            public bool GetFeatureInfoByFeatureID(int featureID, ref FeatureInfo featureInfo, ref float netStDev, ref float discriminantScore)
            {
                // Return the feature info for featureID

                int rowIndex;
                bool matchFound = ContainsFeature(featureID, out rowIndex);
                if (matchFound)
                {
                    featureInfo = mFeatures[rowIndex];
                    netStDev = mExtendedInfo[rowIndex].NETStDev;
                    discriminantScore = mExtendedInfo[rowIndex].DiscriminantScore;
                    return true;
                }
                else
                {
                    featureInfo.FeatureID = 0;
                    featureInfo.FeatureName = string.Empty;
                    featureInfo.Mass = 0d;
                    featureInfo.NET = 0f;
                    netStDev = 0f;
                    discriminantScore = 0f;
                    return false;
                }
            }

            public bool GetFeatureInfoByRowIndex(int rowIndex, ref FeatureInfo featureInfo, ref float netStDev, ref float discriminantScore)
            {
                if (rowIndex >= 0 && rowIndex < mFeatureCount)
                {
                    featureInfo = mFeatures[rowIndex];
                    netStDev = mExtendedInfo[rowIndex].NETStDev;
                    discriminantScore = mExtendedInfo[rowIndex].DiscriminantScore;
                    return true;
                }
                else
                {
                    featureInfo.FeatureID = 0;
                    featureInfo.FeatureName = string.Empty;
                    featureInfo.Mass = 0d;
                    featureInfo.NET = 0f;
                    netStDev = 0f;
                    discriminantScore = 0f;
                    return false;
                }
            }

            public virtual float GetNETStDevByRowIndex(int rowIndex)
            {
                if (rowIndex >= 0 && rowIndex < mFeatureCount)
                {
                    return mExtendedInfo[rowIndex].NETStDev;
                }
                else
                {
                    return 0f;
                }
            }
        }

        internal class PMFeatureMatchResults
        {
            public struct PeakMatchingResult
            {
                public int MatchingID;                // ID of the comparison feature (this is the real ID, and not a RowIndex)
                public double SLiCScore;                  // SLiC Score (Spatially Localized Confidence score)
                public double DelSLiC;                    // Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
                public double MassErr;                    // Observed difference (error) between comparison mass and feature mass (in Da)
                public double NETErr;                     // Observed difference (error) between comparison NET and feature NET
                public int MultiAMTHitCount;          // The number of Unique mass tag hits for each UMC; only applies to AMT's
            }

            protected struct PeakMatchingResults
            {
                public int FeatureID;
                public PeakMatchingResult Details;
            }

            protected const int MEMORY_RESERVE_CHUNK = 100000;
            protected int mPMResultsCount;
            protected PeakMatchingResults[] mPMResults;
            protected bool mPMResultsIsSorted;

            public event SortingListEventHandler SortingList;

            public delegate void SortingListEventHandler();

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
                if (mPMResultsCount >= mPMResults.Length)
                {
                    Array.Resize(ref mPMResults, mPMResults.Length * 2);
                }

                mPMResults[mPMResultsCount].FeatureID = featureID;
                mPMResults[mPMResultsCount].Details.MatchingID = matchingID;
                mPMResults[mPMResultsCount].Details.SLiCScore = slicScore;
                mPMResults[mPMResultsCount].Details.DelSLiC = delSLiC;
                mPMResults[mPMResultsCount].Details.MassErr = massErr;
                mPMResults[mPMResultsCount].Details.NETErr = netErr;
                mPMResults[mPMResultsCount].Details.MultiAMTHitCount = multiAMTHitCount;
                mPMResultsCount += 1;
                mPMResultsIsSorted = false;

                // If we get here, all went well
                return true;
            }

            private int BinarySearchPMResults(int featureIDToFind)
            {
                // Looks through mPMResults() for featureIDToFind, returning the index of the item if found, or -1 if not found
                // Since mPMResults() can contain multiple entries for a given Feature, this function returns the first entry found

                int midIndex;
                int firstIndex = 0;
                int lastIndex = mPMResultsCount - 1;
                int matchingRowIndex = -1;
                if (mPMResultsCount <= 0 || !SortPMResults())
                {
                    return matchingRowIndex;
                }

                try
                {
                    midIndex = (firstIndex + lastIndex) / 2;            // Note: Using Integer division
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
                catch (Exception ex)
                {
                    matchingRowIndex = -1;
                }

                return matchingRowIndex;
            }

            public virtual void Clear()
            {
                mPMResultsCount = 0;
                if (mPMResults == null)
                {
                    mPMResults = new PeakMatchingResults[100000];
                }

                mPMResultsIsSorted = false;
            }

            public int Count
            {
                get
                {
                    return mPMResultsCount;
                }
            }

            public bool GetMatchInfoByFeatureID(int featureID, ref PeakMatchingResult[] matchResults, ref int matchCount)
            {
                // Returns all of the matches for the given feature ID row index
                // Returns false if the feature has no matches
                // Note that this function never shrinks matchResults; it only expands it if needed

                bool matchesFound = false;
                try
                {
                    int index;
                    var indexFirst = default(int);
                    var indexLast = default(int);
                    if (GetRowIndicesForFeatureID(featureID, ref indexFirst, ref indexLast))
                    {
                        matchCount = indexLast - indexFirst + 1;
                        if (matchResults == null || matchCount > matchResults.Length)
                        {
                            matchResults = new PeakMatchingResult[matchCount];
                        }

                        for (index = indexFirst; index <= indexLast; index++)
                            matchResults[index - indexFirst] = mPMResults[index].Details;
                        matchesFound = true;
                    }
                }
                catch (Exception ex)
                {
                    matchesFound = false;
                }

                return matchesFound;
            }

            public bool GetMatchInfoByRowIndex(int rowIndex, ref int featureID, ref PeakMatchingResult matchResultInfo)
            {
                // Populates featureID and matchResultInfo with the peak matching results for the given row index

                bool matchFound = false;
                try
                {
                    if (rowIndex < mPMResultsCount)
                    {
                        featureID = mPMResults[rowIndex].FeatureID;
                        matchResultInfo = mPMResults[rowIndex].Details;
                        matchFound = true;
                    }
                }
                catch (Exception ex)
                {
                    matchFound = false;
                }

                return matchFound;
            }

            private bool GetRowIndicesForFeatureID(int featureID, ref int indexFirst, ref int indexLast)
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
                    while (indexLast < mPMResultsCount - 1 && mPMResults[indexLast + 1].FeatureID == featureID)
                        indexLast += 1;
                    return true;
                }
                else
                {
                    indexFirst = -1;
                    indexLast = -1;
                    return false;
                }
            }

            public int get_MatchCountForFeatureID(int featureID)
            {
                var indexFirst = default(int);
                var indexLast = default(int);
                if (GetRowIndicesForFeatureID(featureID, ref indexFirst, ref indexLast))
                {
                    return indexLast - indexFirst + 1;
                }
                else
                {
                    return 0;
                }
            }

            private bool SortPMResults(bool forceSort = false)
            {
                if (!mPMResultsIsSorted || forceSort)
                {
                    SortingList?.Invoke();
                    try
                    {
                        var comparer = new PeakMatchingResultsComparer();
                        Array.Sort(mPMResults, 0, mPMResultsCount, comparer);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    mPMResultsIsSorted = true;
                }

                return mPMResultsIsSorted;
            }

            private class PeakMatchingResultsComparer : IComparer<PeakMatchingResults>
            {
                public int Compare(PeakMatchingResults x, PeakMatchingResults y)
                {
                    // Sort by .SLiCScore descending, and by MatchingIDIndexOriginal ascending

                    if (x.FeatureID > y.FeatureID)
                    {
                        return 1;
                    }
                    else if (x.FeatureID < y.FeatureID)
                    {
                        return -1;
                    }
                    else if (x.Details.MatchingID > y.Details.MatchingID)
                    {
                        return 1;
                    }
                    else if (x.Details.MatchingID < y.Details.MatchingID)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        private int mMaxPeakMatchingResultsPerFeatureToSave;
        private SearchModeOptions mSearchModeOptions;
        private string mProgressDescription;
        private float mProgressPercent;
        private bool mAbortProcessing;

        public event ProgressContinuesEventHandler ProgressContinues;

        public delegate void ProgressContinuesEventHandler();

        public event LogEventEventHandler LogEvent;

        public delegate void LogEventEventHandler(string Message, MessageTypeConstants EventType);

        public int MaxPeakMatchingResultsPerFeatureToSave
        {
            get
            {
                return mMaxPeakMatchingResultsPerFeatureToSave;
            }

            set
            {
                if (value < 0)
                    value = 0;
                mMaxPeakMatchingResultsPerFeatureToSave = value;
            }
        }

        public string ProgressDescription
        {
            get
            {
                return mProgressDescription;
            }
        }

        public float ProgressPct
        {
            get
            {
                return mProgressPercent;
            }
        }

        public bool UseMaxSearchDistanceMultiplierAndSLiCScore
        {
            get
            {
                return mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore;
            }

            set
            {
                mSearchModeOptions.UseMaxSearchDistanceMultiplierAndSLiCScore = value;
            }
        }

        public bool UseEllipseSearchRegion
        {
            get
            {
                return mSearchModeOptions.UseEllipseSearchRegion;
            }

            set
            {
                mSearchModeOptions.UseEllipseSearchRegion = value;
            }
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

        private void ComputeSLiCScores(ref FeatureInfo featureToIdentify, ref PMFeatureMatchResults featureMatchResults, ref PeakMatchingRawMatches[] rawMatches, ref PMComparisonFeatureInfo comparisonFeatures, ref SearchThresholds searchThresholds, SearchThresholds.SearchTolerances computedTolerances)
        {
            int index;
            int newMatchCount;
            double massStDevPPM;
            double massStDevAbs;
            double netStDevCombined;
            double numeratorSum;
            var comparisonFeatureInfo = new FeatureInfo();
            string message;

            // Compute the match scores (aka SLiC scores)

            massStDevPPM = searchThresholds.SLiCScoreMassPPMStDev;
            if (massStDevPPM <= 0d)
                massStDevPPM = 3d;
            massStDevAbs = searchThresholds.PPMToMass(massStDevPPM, featureToIdentify.Mass);
            if (massStDevAbs <= 0d)
            {
                message = "Assertion failed in ComputeSLiCScores; massStDevAbs is <= 0, which isn't allowed; will assume 0.003";
                Console.WriteLine(message);
                PostLogEntry(message, MessageTypeConstants.ErrorMsg);
                massStDevAbs = 0.003d;
            }

            // Compute the standardized squared distance and the numerator sum
            numeratorSum = 0d;
            for (index = 0; index < rawMatches.Length; index++)
            {
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
            for (index = 0; index < rawMatches.Length; index++)
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

            if (rawMatches.Length > 1)
            {
                // Sort by SLiCScore descending
                var iPeakMatchingRawMatchesComparer = new PeakMatchingRawMatchesComparer();
                Array.Sort(rawMatches, iPeakMatchingRawMatchesComparer);
            }

            if (rawMatches.Length > 0)
            {
                // Compute the DelSLiC value
                // If there is only one match, then the DelSLiC value is 1
                // If there is more than one match, then the highest scoring match gets a DelSLiC value,
                // computed by subtracting the next lower scoring value from the highest scoring value; all
                // other matches get a DelSLiC score of 0
                // This allows one to quickly identify the features with a single match (DelSLiC = 1) or with a match
                // distinct from other matches (DelSLiC > threshold)

                if (rawMatches.Length > 1)
                {
                    rawMatches[0].DelSLiC = rawMatches[0].SLiCScore - rawMatches[1].SLiCScore;
                    for (index = 1; index < rawMatches.Length; index++)
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
                newMatchCount = 0;
                for (index = 0; index < rawMatches.Length; index++)
                {
                    if (TestPointInEllipse(rawMatches[index].NETErr, rawMatches[index].MassErr, computedTolerances.NETTolFinal, computedTolerances.MWTolAbsFinal))
                    {
                        rawMatches[newMatchCount] = rawMatches[index];
                        newMatchCount += 1;
                    }
                }

                if (newMatchCount == 0)
                {
                    rawMatches = new PeakMatchingRawMatches[0];
                }
                else if (newMatchCount < rawMatches.Length)
                {
                    Array.Resize(ref rawMatches, newMatchCount);
                }

                // Add new match results to featureMatchResults
                // Record, at most, mMaxPeakMatchingResultsPerFeatureToSave entries
                for (index = 0; index < Math.Min(mMaxPeakMatchingResultsPerFeatureToSave, rawMatches.Length); index++)
                {
                    comparisonFeatures.GetFeatureInfoByRowIndex(rawMatches[index].MatchingIDIndex, ref comparisonFeatureInfo);
                    featureMatchResults.AddMatch(featureToIdentify.FeatureID, comparisonFeatureInfo.FeatureID, rawMatches[index].SLiCScore, rawMatches[index].DelSLiC, rawMatches[index].MassErr, rawMatches[index].NETErr, rawMatches.Length);
                }
            }
        }

        internal static bool FillRangeSearchObject(ref SearchRange rangeSearch, ref PMComparisonFeatureInfo comparisonFeatures)
        {
            // Initialize the range searching class

            const int LOAD_BLOCK_SIZE = 50000;
            int index;
            int comparisonFeatureCount;
            bool success;
            try
            {
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

                    // for (index = 0; i < comparisonFeatures.Count; i++)
                    // {
                    //     rangeSearch.FillWithDataAddPoint(comparisonFeatures.GetMassByRowIndex(index));
                    // }

                    comparisonFeatureCount = comparisonFeatures.Count;
                    index = 0;
                    while (index < comparisonFeatureCount)
                    {
                        rangeSearch.FillWithDataAddBlock(comparisonFeatures.GetMassArrayByRowRange(index, index + LOAD_BLOCK_SIZE - 1));
                        index += LOAD_BLOCK_SIZE;
                    }

                    success = rangeSearch.FinalizeDataFill();
                }
            }
            catch (Exception ex)
            {
                throw;
                success = false;
            }

            return success;
        }

        internal bool IdentifySequences(SearchThresholds searchThresholds, ref PMFeatureInfo featuresToIdentify, ref PMComparisonFeatureInfo comparisonFeatures, ref PMFeatureMatchResults featureMatchResults, [Optional, DefaultParameterValue(null)] ref SearchRange rangeSearch)
        {
            // Returns True if success, False if the search is canceled
            // Will return true even if none of the features match any of the comparison features
            //
            // If rangeSearch is Nothing or if rangeSearch contains a different number of entries than comparisonFeatures,
            // then will auto-populate it; otherwise, assumes it is populated

            // Note that featureMatchResults will only contain info on the features in featuresToIdentify that matched entries in comparisonFeatures

            int featureIndex;
            int featureCount;
            int matchIndex;
            int comparisonFeaturesOriginalRowIndex;
            var currentFeatureToIdentify = new FeatureInfo();
            var currentComparisonFeature = new FeatureInfo();
            double massTol;
            double netTol;
            double netDiff;
            int matchInd1, matchInd2;
            SearchThresholds.SearchTolerances computedTolerances;

            // The following hold the matches using the broad search tolerances (if .UseMaxSearchDistanceMultiplierAndSLiCScore = True, otherwise, simply holds the matches)
            int rawMatchCount;
            PeakMatchingRawMatches[] rawMatches;    // Pointers into comparisonFeatures; list of peptides that match within both mass and NET tolerance
            bool storeMatch;
            bool success;
            string message;
            if (featureMatchResults == null)
            {
                // if (mUseSqlServerForMatchResults)
                //     featureMatchResults = new PMFeatureMatchResults(mSqlServerConnectionString, mTableNameFeatureMatchResults);
                // else
                featureMatchResults = new PMFeatureMatchResults();
            }
            else
            {
                // Clear any existing results
                featureMatchResults.Clear();
            }

            if (rangeSearch == null || rangeSearch.DataCount != comparisonFeatures.Count)
            {
                success = FillRangeSearchObject(ref rangeSearch, ref comparisonFeatures);
            }
            else
            {
                success = true;
            }

            if (!success)
                return false;
            try
            {
                featureCount = featuresToIdentify.Count;
                UpdateProgress("Finding matching peptides for given search thresholds", 0f);
                mAbortProcessing = false;
                PostLogEntry("IdentifySequences starting, total feature count = " + featureCount.ToString(), MessageTypeConstants.Normal);
                for (featureIndex = 0; featureIndex < featureCount; featureIndex++)
                {
                    // Use rangeSearch to search for matches to each peptide in comparisonFeatures

                    if (featuresToIdentify.GetFeatureInfoByRowIndex(featureIndex, ref currentFeatureToIdentify))
                    {
                        // By Calling .ComputedSearchTolerances() with a mass, the tolerances will be auto re-computed
                        computedTolerances = searchThresholds.get_ComputedSearchTolerances(currentFeatureToIdentify.Mass);
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

                        matchInd1 = 0;
                        matchInd2 = -1;
                        if (rangeSearch.FindValueRange(currentFeatureToIdentify.Mass, massTol, ref matchInd1, ref matchInd2))
                        {
                            rawMatchCount = 0;
                            rawMatches = new PeakMatchingRawMatches[matchInd2 - matchInd1 + 1];
                            for (matchIndex = matchInd1; matchIndex <= matchInd2; matchIndex++)
                            {
                                comparisonFeaturesOriginalRowIndex = rangeSearch.get_OriginalIndex(matchIndex);
                                if (comparisonFeatures.GetFeatureInfoByRowIndex(comparisonFeaturesOriginalRowIndex, ref currentComparisonFeature))
                                {
                                    netDiff = currentFeatureToIdentify.NET - currentComparisonFeature.NET;
                                    if (Math.Abs(netDiff) <= netTol)
                                    {
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
                                            rawMatches[rawMatchCount].MatchingIDIndex = comparisonFeaturesOriginalRowIndex;
                                            rawMatches[rawMatchCount].SLiCScore = -1;
                                            rawMatches[rawMatchCount].MassErr = currentFeatureToIdentify.Mass - currentComparisonFeature.Mass;
                                            rawMatches[rawMatchCount].NETErr = netDiff;
                                            rawMatchCount += 1;
                                        }
                                    }
                                }
                            }

                            if (rawMatchCount > 0)
                            {
                                // Store the FeatureIDIndex in featureMatchResults
                                if (rawMatchCount < rawMatches.Length)
                                {
                                    // Shrink rawMatches
                                    Array.Resize(ref rawMatches, rawMatchCount);
                                }

                                // Compute the SLiC Scores and store the results
                                ComputeSLiCScores(ref currentFeatureToIdentify, ref featureMatchResults, ref rawMatches, ref comparisonFeatures, ref searchThresholds, computedTolerances);
                            }
                        }
                    }
                    else
                    {
                        message = "Programming error in IdentifySequences: Feature not found in featuresToIdentify using feature index: " + featureIndex.ToString();
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
                        PostLogEntry("IdentifySequences, featureIndex = " + featureIndex.ToString(), MessageTypeConstants.Health);
                    }
                }

                UpdateProgress("IdentifySequences complete", 100f);
                PostLogEntry("IdentifySequences complete", MessageTypeConstants.Normal);
                success = !mAbortProcessing;
            }
            catch (Exception ex)
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
                if (Math.Pow(pointX, 2d) / Math.Pow(xTol, 2d) + Math.Pow(pointY, 2d) / Math.Pow(yTol, 2d) <= 1d)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
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
            mProgressPercent = progressPercent;
            ProgressContinues?.Invoke();
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="description">Progress description</param>
        /// <param name="progressPercent">Value between 0 and 100</param>
        private void UpdateProgress(string description, float progressPercent)
        {
            mProgressDescription = description;
            mProgressPercent = progressPercent;
            ProgressContinues?.Invoke();
        }

        private class PeakMatchingRawMatchesComparer : IComparer<PeakMatchingRawMatches>
        {
            public int Compare(PeakMatchingRawMatches x, PeakMatchingRawMatches y)
            {
                // Sort by .SLiCScore descending, and by MatchingIDIndexOriginal Ascending

                if (x.SLiCScore > y.SLiCScore)
                {
                    return -1;
                }
                else if (x.SLiCScore < y.SLiCScore)
                {
                    return 1;
                }
                else if (x.MatchingIDIndex > y.MatchingIDIndex)
                {
                    return 1;
                }
                else if (x.MatchingIDIndex < y.MatchingIDIndex)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
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
            private struct SLiCScoreOptions
            {
                public double MassPPMStDev;                  // Default 3
                public double NETStDev;                      // Default 0.025
                public bool UseAMTNETStDev;
                public float MaxSearchDistanceMultiplier;   // Default 2
            }

            // Note that all of these tolerances are half-widths, i.e. tolerance +- comparison value
            public struct SearchTolerances
            {
                public double MWTolAbsBroad;
                public double MWTolAbsFinal;
                public double NETTolBroad;
                public double NETTolFinal;
            }

            private double mMassTolerance;          // Mass search tolerance, +- this value; TolType defines if this is PPM or Da
            private double mNETTolerance;           // NET search tolerance, +- this value
            private float mSLiCScoreMaxSearchDistanceMultiplier;
            private SLiCScoreOptions mSLiCScoreOptions;

            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            private SearchTolerances mComputedSearchTolerances = new SearchTolerances();

            public bool AutoDefineSLiCScoreThresholds { get; set; }

            public SearchTolerances ComputedSearchTolerances
            {
                get
                {
                    return mComputedSearchTolerances;
                }
            }

            public SearchTolerances get_ComputedSearchTolerances(double referenceMass)
            {
                DefinePeakMatchingTolerances(ref referenceMass);
                return mComputedSearchTolerances;
            }

            public MassToleranceConstants MassTolType { get; set; }

            public double MassTolerance
            {
                get
                {
                    return mMassTolerance;
                }

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
                get
                {
                    return mNETTolerance;
                }

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
                get
                {
                    return mSLiCScoreOptions.MassPPMStDev;
                }

                set
                {
                    if (value < 0d)
                        value = 0d;
                    mSLiCScoreOptions.MassPPMStDev = value;
                }
            }

            public double SLiCScoreNETStDev
            {
                get
                {
                    return mSLiCScoreOptions.NETStDev;
                }

                set
                {
                    if (value < 0d)
                        value = 0d;
                    mSLiCScoreOptions.NETStDev = value;
                }
            }

            public bool SLiCScoreUseAMTNETStDev
            {
                get
                {
                    return mSLiCScoreOptions.UseAMTNETStDev;
                }

                set
                {
                    mSLiCScoreOptions.UseAMTNETStDev = value;
                }
            }

            public float SLiCScoreMaxSearchDistanceMultiplier
            {
                get
                {
                    return mSLiCScoreMaxSearchDistanceMultiplier;
                }

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
                        mComputedSearchTolerances.MWTolAbsFinal = PPMToMass(mMassTolerance, referenceMass);
                        MWTolPPMBroad = mMassTolerance;
                        break;
                    case MassToleranceConstants.Absolute:
                        mComputedSearchTolerances.MWTolAbsFinal = mMassTolerance;
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
                        Console.WriteLine("Programming error in DefinePeakMatchingTolerances; Unknown MassToleranceType: " + MassTolType.ToString());
                        break;
                }

                if (MWTolPPMBroad < mSLiCScoreOptions.MassPPMStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR)
                {
                    MWTolPPMBroad = mSLiCScoreOptions.MassPPMStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR;
                }

                mComputedSearchTolerances.NETTolBroad = mSLiCScoreOptions.NETStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR;
                if (mComputedSearchTolerances.NETTolBroad < mNETTolerance)
                {
                    mComputedSearchTolerances.NETTolBroad = mNETTolerance;
                }

                mComputedSearchTolerances.NETTolFinal = mNETTolerance;

                // Convert from PPM to Absolute mass
                mComputedSearchTolerances.MWTolAbsBroad = PPMToMass(MWTolPPMBroad, referenceMass);
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