using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic.CompilerServices;

namespace ProteinDigestionSimulator
{
    internal class ProteinCollection
    {
        public enum CleavageStateConstants
        {
            None = 0,
            Partial = 1,
            Full = 2,
            Unknown = -1
        }

        protected struct ProteinEntry
        {
            public string Name;
            public int ProteinID;
        }

        protected const int MEMORY_RESERVE_CHUNK = 100000;
        protected int mProteinCount;
        protected ProteinEntry[] mProteins;
        protected bool mProteinArrayIsSorted;
        protected int mMaxProteinIDUsed;
        private ProteinToPeptideMappingInfo _mProteinToPeptideMapping;

        protected ProteinToPeptideMappingInfo mProteinToPeptideMapping
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mProteinToPeptideMapping;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mProteinToPeptideMapping != null)
                {
                    _mProteinToPeptideMapping.SortingList -= mProteinToPeptideMapping_SortingList;
                }

                _mProteinToPeptideMapping = value;
                if (_mProteinToPeptideMapping != null)
                {
                    _mProteinToPeptideMapping.SortingList += mProteinToPeptideMapping_SortingList;
                }
            }
        }

        protected bool mUseProteinNameHashTable;
        protected Hashtable mProteinNameToRowIndex;

        public event SortingListEventHandler SortingList;

        public delegate void SortingListEventHandler();

        public event SortingMappingsEventHandler SortingMappings;

        public delegate void SortingMappingsEventHandler();

        public ProteinCollection()
        {
            mUseProteinNameHashTable = true;                       // Set this to False to conserve memory; you must call Clear() after changing this for it to take effect
            Clear();
            mProteinToPeptideMapping = new ProteinToPeptideMappingInfo();
        }

        public bool Add(string proteinName, ref int newProteinID)
        {
            // Adds the protein by name, auto-assigning the ID value
            return Add(proteinName, ref newProteinID, true);
        }

        public bool Add(string proteinName, ref int proteinID, bool autoDefineProteinID)
        {
            // Adds the protein by name
            // Uses the given protein ID if autoDefineProteinID = False or auto-assigns the ID value if autoDefineProteinID = True
            // Returns ByRef the protein ID via proteinID
            // If the protein already exists, then returns True and populates proteinID

            // First, look for an existing entry in mProteins
            // Once a protein is present in the table, its ID cannot be updated
            if (GetProteinIDByProteinName(proteinName, ref proteinID))
            {
            }
            // Protein already exists; proteinID now contains the Protein ID
            else
            {
                // Add to mProteins

                if (mProteinCount >= mProteins.Length)
                {
                    Array.Resize(ref mProteins, mProteins.Length * 2);
                    // ReDim Preserve mProteins(mProteins.Length + MEMORY_RESERVE_CHUNK - 1)
                }

                if (autoDefineProteinID)
                {
                    proteinID = mMaxProteinIDUsed + 1;
                }

                mProteins[mProteinCount].Name = proteinName;
                mProteins[mProteinCount].ProteinID = proteinID;
                if (mUseProteinNameHashTable)
                {
                    mProteinNameToRowIndex.Add(proteinName, mProteinCount);
                }

                mProteinCount += 1;
                mProteinArrayIsSorted = false;
                mMaxProteinIDUsed = Math.Max(mMaxProteinIDUsed, proteinID);
            }

            // If we get here, all went well
            return true;
        }

        public bool AddProteinToPeptideMapping(int proteinID, int peptideID)
        {
            return AddProteinToPeptideMapping(proteinID, peptideID, CleavageStateConstants.Unknown);
        }

        public bool AddProteinToPeptideMapping(int proteinID, int peptideID, CleavageStateConstants cleavageState)
        {
            return mProteinToPeptideMapping.AddProteinToPeptideMapping(proteinID, peptideID, cleavageState);
        }

        public bool AddProteinToPeptideMapping(string proteinName, int peptideID)
        {
            return AddProteinToPeptideMapping(proteinName, peptideID, CleavageStateConstants.Unknown);
        }

        public bool AddProteinToPeptideMapping(string proteinName, int peptideID, CleavageStateConstants cleavageState)
        {
            return mProteinToPeptideMapping.AddProteinToPeptideMapping(this, proteinName, peptideID, cleavageState);
        }

        private int BinarySearchFindProtein(string proteinName)
        {
            // Looks through mProteins() for proteinName, returning the index of the item if found, or -1 if not found

            int midIndex;
            int firstIndex = 0;
            int lastIndex = mProteinCount - 1;
            int matchingRowIndex = -1;
            if (mProteinCount <= 0 || !SortProteins())
            {
                return matchingRowIndex;
            }

            try
            {
                midIndex = (firstIndex + lastIndex) / 2;            // Note: Using Integer division
                if (midIndex < firstIndex)
                    midIndex = firstIndex;
                while (firstIndex <= lastIndex && (mProteins[midIndex].Name ?? "") != (proteinName ?? ""))
                {
                    if (Operators.CompareString(proteinName, mProteins[midIndex].Name, false) < 0)
                    {
                        // Search the lower half
                        lastIndex = midIndex - 1;
                    }
                    else if (Operators.CompareString(proteinName, mProteins[midIndex].Name, false) > 0)
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
                    if ((mProteins[midIndex].Name ?? "") == (proteinName ?? ""))
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
            mProteinCount = 0;
            if (mProteins is null)
            {
                mProteins = new ProteinEntry[100000];
            }

            mProteinArrayIsSorted = false;
            mMaxProteinIDUsed = 0;
            if (mUseProteinNameHashTable)
            {
                if (mProteinNameToRowIndex is null)
                {
                    mProteinNameToRowIndex = new Hashtable();
                }
                else
                {
                    mProteinNameToRowIndex.Clear();
                }
            }
            else if (mProteinNameToRowIndex is object)
            {
                mProteinNameToRowIndex.Clear();
                mProteinNameToRowIndex = null;
            }
        }

        public int Count
        {
            get
            {
                return mProteinCount;
            }
        }

        public int[] GetPeptideIDsMappedToProteinID(int proteinID)
        {
            return mProteinToPeptideMapping.GetPeptideIDsMappedToProteinID(proteinID);
        }

        public int GetPeptideCountForProteinByID(int proteinID)
        {
            return mProteinToPeptideMapping.get_PeptideCountForProteinID(proteinID);
        }

        public int[] GetProteinIDsMappedToPeptideID(int peptideID)
        {
            return mProteinToPeptideMapping.GetProteinIDsMappedToPeptideID(peptideID);
        }

        public bool GetProteinNameByProteinID(int proteinID, ref string proteinName)
        {
            // Since mProteins is sorted by Protein Name, we must fully search the array to obtain the protein name for proteinID

            bool matchFound = false;
            int index;
            proteinName = string.Empty;
            var loopTo = mProteinCount - 1;
            for (index = 0; index <= loopTo; index++)
            {
                if (mProteins[index].ProteinID == proteinID)
                {
                    proteinName = mProteins[index].Name;
                    matchFound = true;
                    break;
                }
            }

            return matchFound;
        }

        public bool GetProteinIDByProteinName(string proteinName, ref int proteinID)
        {
            // Returns True if the proteins array contains the protein
            // If found, returns ProteinID in proteinID
            // Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

            int rowIndex;
            if (mUseProteinNameHashTable)
            {
                if (mProteinNameToRowIndex.Contains(proteinName))
                {
                    rowIndex = Conversions.ToInteger(mProteinNameToRowIndex[proteinName]);
                }
                else
                {
                    rowIndex = -1;
                }
            }
            else
            {
                // Perform a binary search of mFeatures for featureID
                rowIndex = BinarySearchFindProtein(proteinName);
            }

            if (rowIndex >= 0)
            {
                proteinID = mProteins[rowIndex].ProteinID;
                return true;
            }
            else
            {
                proteinID = -1;
                return false;
            }
        }

        public bool GetProteinInfoByRowIndex(int rowIndex, out int proteinID, out string proteinName)
        {
            if (rowIndex >= 0 && rowIndex < mProteinCount)
            {
                proteinName = mProteins[rowIndex].Name;
                proteinID = mProteins[rowIndex].ProteinID;
                return true;
            }
            else
            {
                proteinName = string.Empty;
                proteinID = -1;
                return false;
            }
        }

        private bool SortProteins(bool forceSort = false)
        {
            if (!mProteinArrayIsSorted || forceSort)
            {
                SortingList?.Invoke();
                try
                {
                    var comparer = new ProteinEntryComparer();
                    Array.Sort(mProteins, 0, mProteinCount, comparer);
                }
                catch
                {
                    throw;
                }

                mProteinArrayIsSorted = true;
            }

            return mProteinArrayIsSorted;
        }

        public bool UseProteinNameHashTable
        {
            get
            {
                return mUseProteinNameHashTable;
            }

            set
            {
                mUseProteinNameHashTable = value;
            }
        }

        private void mProteinToPeptideMapping_SortingList()
        {
            SortingMappings?.Invoke();
        }

        private class ProteinEntryComparer : IComparer<ProteinEntry>
        {
            public int Compare(ProteinEntry x, ProteinEntry y)
            {
                // Sort by Protein Name, ascending

                if (Operators.CompareString(x.Name, y.Name, false) > 0)
                {
                    return 1;
                }
                else if (Operators.CompareString(x.Name, y.Name, false) < 0)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        protected class ProteinToPeptideMappingInfo
        {
            public struct ProteinToPeptideMappingEntry
            {
                public int ProteinID;
                public int PeptideID;
                public CleavageStateConstants CleavageState;
            }

            protected const int MEMORY_RESERVE_CHUNK = 100000;
            protected int mMappingCount;
            protected ProteinToPeptideMappingEntry[] mMappings;
            protected bool mMappingArrayIsSorted;

            public event SortingListEventHandler SortingList;

            public delegate void SortingListEventHandler();

            public ProteinToPeptideMappingInfo()
            {
                Clear();
            }

            public bool AddProteinToPeptideMapping(int proteinID, int peptideID)
            {
                return AddProteinToPeptideMapping(proteinID, peptideID, CleavageStateConstants.Unknown);
            }

            public bool AddProteinToPeptideMapping(int proteinID, int peptideID, CleavageStateConstants cleavageState)
            {
                // Add the mapping
                if (mMappingCount >= mMappings.Length)
                {
                    Array.Resize(ref mMappings, mMappings.Length * 2);
                    // ReDim Preserve mMappings(mMappings.Length + MEMORY_RESERVE_CHUNK - 1)
                }

                mMappings[mMappingCount].ProteinID = proteinID;
                mMappings[mMappingCount].PeptideID = peptideID;
                mMappings[mMappingCount].CleavageState = cleavageState;
                mMappingCount += 1;
                mMappingArrayIsSorted = false;

                // If we get here, all went well
                return true;
            }

            public bool AddProteinToPeptideMapping(ProteinCollection proteinInfo, string proteinName, int peptideID)
            {
                return AddProteinToPeptideMapping(proteinInfo, proteinName, peptideID, CleavageStateConstants.Unknown);
            }

            public bool AddProteinToPeptideMapping(ProteinCollection proteinInfo, string proteinName, int peptideID, CleavageStateConstants cleavageState)
            {
                var proteinID = default(int);
                if (!proteinInfo.GetProteinIDByProteinName(proteinName, ref proteinID))
                {
                    // Need to add the protein
                    if (!proteinInfo.Add(proteinName, ref proteinID))
                    {
                        return false;
                    }
                }

                return AddProteinToPeptideMapping(proteinID, peptideID, cleavageState);
            }

            private int BinarySearchFindProteinMapping(int proteinIDToFind)
            {
                // Looks through mMappings() for proteinIDToFind, returning the index of the item if found, or -1 if not found
                // Since mMappings() can contain multiple entries for a given Protein, this function returns the first entry found

                int midIndex;
                int firstIndex = 0;
                int lastIndex = mMappingCount - 1;
                int matchingRowIndex = -1;
                if (mMappingCount <= 0 || !SortMappings())
                {
                    return matchingRowIndex;
                }

                try
                {
                    midIndex = (firstIndex + lastIndex) / 2;            // Note: Using Integer division
                    if (midIndex < firstIndex)
                        midIndex = firstIndex;
                    while (firstIndex <= lastIndex && mMappings[midIndex].ProteinID != proteinIDToFind)
                    {
                        if (proteinIDToFind < mMappings[midIndex].ProteinID)
                        {
                            // Search the lower half
                            lastIndex = midIndex - 1;
                        }
                        else if (proteinIDToFind > mMappings[midIndex].ProteinID)
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
                        if (mMappings[midIndex].ProteinID == proteinIDToFind)
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
                mMappingCount = 0;
                if (mMappings is null)
                {
                    mMappings = new ProteinToPeptideMappingEntry[100000];
                }

                mMappingArrayIsSorted = false;
            }

            protected bool ContainsMapping(int proteinID, int peptideID)
            {
                // Returns True if the data table contains the mapping of proteinID to peptideID
                // Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

                int index;
                var indexFirst = default(int);
                var indexLast = default(int);
                if (GetRowIndicesForProteinID(proteinID, ref indexFirst, ref indexLast))
                {
                    var loopTo = indexLast;
                    for (index = indexFirst; index <= loopTo; index++)
                    {
                        if (mMappings[index].PeptideID == peptideID)
                        {
                            return true;
                        }
                    }
                }

                // If we get here, then the mapping wasn't found
                return false;
            }

            public int Count
            {
                get
                {
                    return mMappingCount;
                }
            }

            public int[] GetPeptideIDsMappedToProteinID(int proteinID)
            {
                // Returns all of the peptides for the given protein ID

                int[] matchingIDs;
                int index;
                var indexFirst = default(int);
                var indexLast = default(int);
                if (GetRowIndicesForProteinID(proteinID, ref indexFirst, ref indexLast))
                {
                    matchingIDs = new int[indexLast - indexFirst + 1];
                    var loopTo = indexLast;
                    for (index = indexFirst; index <= loopTo; index++)
                        matchingIDs[index - indexFirst] = mMappings[index].PeptideID;
                }
                else
                {
                    matchingIDs = new int[0];
                }

                return matchingIDs;
            }

            public int[] GetProteinIDsMappedToPeptideID(int peptideID)
            {
                // Since mMappings is sorted by Protein ID, we must fully search the array to obtain the ProteinIDs for peptideID

                int ARRAY_ALLOCATION_CHUNK = 10;
                int[] matchingIDs;
                int matchCount;
                matchingIDs = new int[ARRAY_ALLOCATION_CHUNK];
                matchCount = 0;
                for (int index = 0, loopTo = mMappingCount - 1; index <= loopTo; index++)
                {
                    if (mMappings[index].PeptideID == peptideID)
                    {
                        if (matchCount >= matchingIDs.Length)
                        {
                            Array.Resize(ref matchingIDs, matchingIDs.Length * 2);
                            // ReDim Preserve matchingIDs(matchingIDs.Length + ARRAY_ALLOCATION_CHUNK - 1)
                        }

                        matchingIDs[matchCount] = mMappings[index].ProteinID;
                        matchCount += 1;
                    }
                }

                if (matchingIDs.Length > matchCount)
                {
                    Array.Resize(ref matchingIDs, matchCount);
                }

                return matchingIDs;
            }

            private bool GetRowIndicesForProteinID(int proteinID, ref int indexFirst, ref int indexLast)
            {
                // Looks for proteinID in mMappings
                // If found, returns the range of rows that contain matches for proteinID

                // Perform a binary search of mMappings for proteinID
                indexFirst = BinarySearchFindProteinMapping(proteinID);
                if (indexFirst >= 0)
                {
                    // Match found; need to find all of the rows with proteinID
                    indexLast = indexFirst;

                    // Step backward through mMappings to find the first match for proteinID
                    while (indexFirst > 0 && mMappings[indexFirst - 1].ProteinID == proteinID)
                        indexFirst -= 1;

                    // Step forward through mMappings to find the last match for proteinID
                    while (indexLast < mMappingCount - 1 && mMappings[indexLast + 1].ProteinID == proteinID)
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

            public int get_PeptideCountForProteinID(int proteinID)
            {
                var indexFirst = default(int);
                var indexLast = default(int);
                if (GetRowIndicesForProteinID(proteinID, ref indexFirst, ref indexLast))
                {
                    return indexLast - indexFirst + 1;
                }
                else
                {
                    return 0;
                }
            }

            private bool SortMappings(bool forceSort = false)
            {
                if (!mMappingArrayIsSorted || forceSort)
                {
                    SortingList?.Invoke();
                    try
                    {
                        var comparer = new ProteinToPeptideMappingsComparer();
                        Array.Sort(mMappings, 0, mMappingCount, comparer);
                    }
                    catch
                    {
                        throw;
                    }

                    mMappingArrayIsSorted = true;
                }

                return mMappingArrayIsSorted;
            }

            private class ProteinToPeptideMappingsComparer : IComparer<ProteinToPeptideMappingEntry>
            {
                public int Compare(ProteinToPeptideMappingEntry x, ProteinToPeptideMappingEntry y)
                {
                    // Sort by ProteinID, then by PeptideID

                    if (x.ProteinID > y.ProteinID)
                    {
                        return 1;
                    }
                    else if (x.ProteinID < y.ProteinID)
                    {
                        return -1;
                    }
                    else if (x.PeptideID > y.PeptideID)
                    {
                        return 1;
                    }
                    else if (x.PeptideID < y.PeptideID)
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
    }
}