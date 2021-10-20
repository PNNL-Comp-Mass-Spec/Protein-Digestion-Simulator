using System;
using System.Collections.Generic;

namespace ProteinDigestionSimulator.PeakMatching
{
    internal class PMFeatureMatchResults
    {
        // Ignore Spelling: Da

        private readonly struct PeakMatchingResults : IComparable<PeakMatchingResults>
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
                if (featureIdComparison != 0)
                {
                    return featureIdComparison;
                }

                return Details.MatchingID.CompareTo(other.Details.MatchingID);
            }
        }

        private readonly List<PeakMatchingResults> mPMResults = new();
        private bool mPMResultsIsSorted;

        public event SortingListEventHandler SortingList;

        public PMFeatureMatchResults()
        {
            Clear();
        }

        public bool AddMatch(int featureID, ref PeakMatchingResult matchResultInfo)
        {
            return AddMatch(featureID, matchResultInfo.MatchingID,
                            matchResultInfo.SLiCScore, matchResultInfo.DelSLiC,
                            matchResultInfo.MassErr, matchResultInfo.NETErr,
                            matchResultInfo.MultiAMTHitCount);
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
            // Looks through mPMResults[] for featureIDToFind, returning the index of the item if found, or -1 if not found
            // Since mPMResults[] can contain multiple entries for a given Feature, this function returns the first entry found

            var firstIndex = 0;
            var lastIndex = mPMResults.Count - 1;

            var matchingRowIndex = -1;

            if (mPMResults.Count == 0 || !SortPMResults())
            {
                return matchingRowIndex;
            }

            try
            {
                var midIndex = lastIndex / 2;
                if (midIndex < firstIndex)
                {
                    midIndex = firstIndex;
                }

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
                    {
                        break;
                    }
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

        private void Clear()
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
                    {
                        matchResults[index - indexFirst] = mPMResults[index].Details;
                    }

                    matchesFound = true;
                }
            }
            catch
            {
                matchesFound = false;
            }

            return matchesFound;
        }

        public bool GetMatchInfoByRowIndex(int rowIndex, out int featureID, out PeakMatchingResult matchResultInfo)
        {
            // Populates featureID and matchResultInfo with the peak matching results for the given row index

            try
            {
                if (rowIndex < mPMResults.Count)
                {
                    featureID = mPMResults[rowIndex].FeatureID;
                    matchResultInfo = mPMResults[rowIndex].Details;

                    return true;
                }
            }
            catch
            {
                // Ignore errors here
            }

            featureID = 0;
            matchResultInfo = new PeakMatchingResult();
            return false;
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
                {
                    indexFirst--;
                }

                // Step forward through mPMResults to find the last match for featureID
                while (indexLast < mPMResults.Count - 1 && mPMResults[indexLast + 1].FeatureID == featureID)
                {
                    indexLast++;
                }

                return true;
            }

            indexFirst = -1;
            indexLast = -1;
            return false;
        }

        public int GetMatchCountForFeatureID(int featureID)
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
}
