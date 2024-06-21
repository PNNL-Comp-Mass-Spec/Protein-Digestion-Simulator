using System;
using System.Collections.Generic;

namespace ProteinDigestionSimulator.PeakMatching
{
    internal class PMFeatureInfo
    {
        protected int mFeatureCount;
        protected FeatureInfo[] mFeatures;
        private bool mFeaturesArrayIsSorted;
        private Dictionary<int, int> featureIDToRowIndex;

        public event SortingListEventHandler SortingList;

        /// <summary>
        /// Constructor
        /// </summary>
        public PMFeatureInfo()
        {
            // ReSharper disable once GrammarMistakeInComment

            // Set this to false to conserve memory; you must call Clear() after changing this for it to take effect
            UseFeatureIDDictionary = true;
            Clear();
        }

        // ReSharper disable once UnusedMember.Global
        public bool Add(FeatureInfo featureInfo)
        {
            return Add(featureInfo.FeatureID, featureInfo.FeatureName, featureInfo.Mass, featureInfo.NET);
        }

        /// <summary>
        /// Add the feature to mFeatures
        /// </summary>
        /// <param name="featureID"></param>
        /// <param name="peptideName"></param>
        /// <param name="peptideMass"></param>
        /// <param name="peptideNET"></param>
        /// <returns>True if the feature was added, false if it already exists (by featureID)</returns>
        public bool Add(int featureID, string peptideName, double peptideMass, float peptideNET)
        {
            if (ContainsFeature(featureID))
            {
                return false;
            }

            // Add the feature
            if (mFeatureCount >= mFeatures.Length)
            {
                Array.Resize(ref mFeatures, mFeatures.Length * 2);
            }

            mFeatures[mFeatureCount] = new FeatureInfo(featureID, peptideName, peptideMass, peptideNET);

            if (UseFeatureIDDictionary)
            {
                featureIDToRowIndex.Add(featureID, mFeatureCount);
            }

            mFeatureCount++;
            mFeaturesArrayIsSorted = false;

            return true;
        }

        private int BinarySearchFindFeature(int featureIDToFind)
        {
            // Looks through mFeatures[] for featureIDToFind, returning the index of the item if found, or -1 if not found

            var firstIndex = 0;
            var lastIndex = mFeatureCount - 1;
            var matchingRowIndex = -1;

            if (mFeatureCount <= 0 || !SortFeatures())
            {
                return matchingRowIndex;
            }

            try
            {
                var midIndex = lastIndex / 2;            // Note: Using Integer division
                if (midIndex < firstIndex)
                {
                    midIndex = firstIndex;
                }

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
                    {
                        break;
                    }
                }

                if (midIndex >= firstIndex && midIndex <= lastIndex && mFeatures[midIndex].FeatureID == featureIDToFind)
                {
                    matchingRowIndex = midIndex;
                }
            }
            catch
            {
                matchingRowIndex = -1;
            }

            return matchingRowIndex;
        }

        public void Clear()
        {
            mFeatureCount = 0;

            mFeatures ??= new FeatureInfo[100000];

            mFeaturesArrayIsSorted = false;

            if (UseFeatureIDDictionary)
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

            if (UseFeatureIDDictionary)
            {
                if (featureIDToRowIndex.TryGetValue(featureID, out var index))
                {
                    rowIndex = index;
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

        public bool GetFeatureInfoByFeatureID(int featureID, out FeatureInfo featureInfo)
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

        public bool GetFeatureInfoByRowIndex(int rowIndex, out FeatureInfo featureInfo)
        {
            if (rowIndex >= 0 && rowIndex < mFeatureCount)
            {
                featureInfo = mFeatures[rowIndex];
                return true;
            }

            featureInfo = FeatureInfo.Blank();
            return false;
        }

        public double[] GetMassArrayByRowRange(int rowIndexStart, int rowIndexEnd)
        {
            if (rowIndexEnd < rowIndexStart)
            {
                return Array.Empty<double>();
            }

            var masses = new double[rowIndexEnd - rowIndexStart + 1];

            if (rowIndexEnd >= mFeatureCount)
            {
                rowIndexEnd = mFeatureCount - 1;
            }

            var matchCount = 0;
            for (var index = rowIndexStart; index <= rowIndexEnd; index++)
            {
                masses[matchCount] = mFeatures[index].Mass;
                matchCount++;
            }

            if (masses.Length > matchCount)
            {
                Array.Resize(ref masses, matchCount);
            }

            return masses;
        }

        // ReSharper disable once UnusedMember.Global
        public double GetMassByRowIndex(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < mFeatureCount)
            {
                return mFeatures[rowIndex].Mass;
            }

            return 0d;
        }

        private bool SortFeatures(bool forceSort = false)
        {
            if (!mFeaturesArrayIsSorted || forceSort)
            {
                SortingList?.Invoke();
                Array.Sort(mFeatures, 0, mFeatureCount);

                mFeaturesArrayIsSorted = true;
            }

            return mFeaturesArrayIsSorted;
        }

        public bool UseFeatureIDDictionary { get; set; }
    }
}
