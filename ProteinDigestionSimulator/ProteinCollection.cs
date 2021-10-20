using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Global

namespace ProteinDigestionSimulator
{
    internal class ProteinCollection
    {
        public enum CleavageStateConstants
        {
            // ReSharper disable UnusedMember.Global
            None = 0,
            Partial = 1,
            Full = 2,
            Unknown = -1
            // ReSharper restore UnusedMember.Global
        }

        protected class ProteinEntry : IComparable<ProteinEntry>
        {
            public string Name { get; }
            public int ProteinID { get; }

            public ProteinEntry(string name, int proteinId)
            {
                Name = name;
                ProteinID = proteinId;
            }

            public int CompareTo(ProteinEntry other)
            {
                // Sort by Protein Name, ascending
                if (ReferenceEquals(this, other))
                {
                    return 0;
                }

                if (other is null)
                {
                    return 1;
                }

                return string.CompareOrdinal(Name, other.Name);
            }
        }

        private int mProteinCount;
        private ProteinEntry[] mProteins;
        private bool mProteinArrayIsSorted;

        private int mMaxProteinIDUsed;

        private readonly ProteinToPeptideMappingInfo mProteinToPeptideMapping;

        /// <summary>
        /// Mapping from protein to index in mProteins
        /// </summary>
        private Dictionary<string, int> mProteinNameToRowIndex;

        public event SortingListEventHandler SortingList;
        public event SortingMappingsEventHandler SortingMappings;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProteinCollection()
        {
            // Set this to False to conserve memory; you must call Clear() after changing this for it to take effect
            UseProteinNameDictionary = true;
            Clear();
            mProteinToPeptideMapping = new ProteinToPeptideMappingInfo();
            mProteinToPeptideMapping.SortingList += ProteinToPeptideMapping_SortingList;
        }

        public bool Add(string proteinName, out int newProteinID)
        {
            // Adds the protein by name, auto-assigning the ID value
            return Add(proteinName, out newProteinID, true);
        }

        public bool Add(string proteinName, out int proteinID, bool autoDefineProteinID)
        {
            // Adds the protein by name
            // Uses the given protein ID if autoDefineProteinID = False or auto-assigns the ID value if autoDefineProteinID = True
            // Returns ByRef the protein ID via proteinID
            // If the protein already exists, then returns True and populates proteinID

            // First, look for an existing entry in mProteins
            // Once a protein is present in the table, its ID cannot be updated
            if (GetProteinIDByProteinName(proteinName, out proteinID))
            {
                // Protein already exists; proteinID now contains the Protein ID
            }
            else
            {
                // Add to mProteins

                if (mProteinCount >= mProteins.Length)
                {
                    Array.Resize(ref mProteins, mProteins.Length * 2);
                    //Array.Resize(ref mProteins, mProteins.Length + MEMORY_RESERVE_CHUNK - 1);
                }

                if (autoDefineProteinID)
                {
                    proteinID = mMaxProteinIDUsed + 1;
                }

                mProteins[mProteinCount] = new ProteinEntry(proteinName, proteinID);

                if (UseProteinNameDictionary)
                {
                    mProteinNameToRowIndex.Add(proteinName, mProteinCount);
                }

                mProteinCount++;
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
            // Looks through mProteins[] for proteinName, returning the index of the item if found, or -1 if not found

            var firstIndex = 0;
            var lastIndex = mProteinCount - 1;

            var matchingRowIndex = -1;

            if (mProteinCount <= 0 || !SortProteins())
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

                while (firstIndex <= lastIndex && (mProteins[midIndex].Name ?? string.Empty) != (proteinName ?? string.Empty))
                {
                    if (string.CompareOrdinal(proteinName, mProteins[midIndex].Name) < 0)
                    {
                        // Search the lower half
                        lastIndex = midIndex - 1;
                    }
                    else if (string.CompareOrdinal(proteinName, mProteins[midIndex].Name) > 0)
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
                    if ((mProteins[midIndex].Name ?? string.Empty) == (proteinName ?? string.Empty))
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

        public void Clear()
        {
            mProteinCount = 0;

            mProteins ??= new ProteinEntry[100000];

            mProteinArrayIsSorted = false;
            mMaxProteinIDUsed = 0;

            if (UseProteinNameDictionary)
            {
                if (mProteinNameToRowIndex == null)
                {
                    mProteinNameToRowIndex = new Dictionary<string, int>();
                }
                else
                {
                    mProteinNameToRowIndex.Clear();
                }
            }
            else if (mProteinNameToRowIndex != null)
            {
                mProteinNameToRowIndex.Clear();
                mProteinNameToRowIndex = null;
            }
        }

        public int Count => mProteinCount;

        public int[] GetPeptideIDsMappedToProteinID(int proteinID)
        {
            return mProteinToPeptideMapping.GetPeptideIDsMappedToProteinID(proteinID);
        }

        public int GetPeptideCountForProteinByID(int proteinID)
        {
            return mProteinToPeptideMapping.GetPeptideCountForProteinID(proteinID);
        }

        public int[] GetProteinIDsMappedToPeptideID(int peptideID)
        {
            return mProteinToPeptideMapping.GetProteinIDsMappedToPeptideID(peptideID);
        }

        public bool GetProteinNameByProteinID(int proteinID, out string proteinName)
        {
            // Since mProteins is sorted by Protein Name, we must fully search the array to obtain the protein name for proteinID

            var matchFound = false;

            int index;

            proteinName = string.Empty;

            for (index = 0; index < mProteinCount; index++)
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

        public bool GetProteinIDByProteinName(string proteinName, out int proteinID)
        {
            // Returns True if the proteins array contains the protein
            // If found, returns ProteinID in proteinID
            // Note that the data will be sorted if necessary, which could lead to slow execution if this function is called repeatedly, while adding new data between calls

            int rowIndex;

            if (UseProteinNameDictionary)
            {
                if (mProteinNameToRowIndex.ContainsKey(proteinName))
                {
                    rowIndex = mProteinNameToRowIndex[proteinName];
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

            proteinID = -1;
            return false;
        }

        public bool GetProteinInfoByRowIndex(int rowIndex, out int proteinID, out string proteinName)
        {
            if (rowIndex >= 0 && rowIndex < mProteinCount)
            {
                proteinName = mProteins[rowIndex].Name;
                proteinID = mProteins[rowIndex].ProteinID;
                return true;
            }

            proteinName = string.Empty;
            proteinID = -1;
            return false;
        }

        private bool SortProteins(bool forceSort = false)
        {
            if (!mProteinArrayIsSorted || forceSort)
            {
                SortingList?.Invoke();
                Array.Sort(mProteins, 0, mProteinCount);

                mProteinArrayIsSorted = true;
            }

            return mProteinArrayIsSorted;
        }

        /// <summary>
        /// When true, store protein names in a dictionary
        /// </summary>
        /// <remarks>
        /// Set this to False to conserve memory; you must call Clear() after changing this for it to take effect
        /// </remarks>
        public bool UseProteinNameDictionary { get; set; }

        private void ProteinToPeptideMapping_SortingList()
        {
            SortingMappings?.Invoke();
        }

        private class ProteinToPeptideMappingInfo
        {
            public class ProteinToPeptideMappingEntry : IComparable<ProteinToPeptideMappingEntry>
            {
                public readonly int ProteinID;

                public readonly int PeptideID;

                // ReSharper disable once MemberCanBePrivate.Local
                // ReSharper disable once NotAccessedField.Local
                public readonly CleavageStateConstants CleavageState;

                public ProteinToPeptideMappingEntry(int proteinId, int peptideId, CleavageStateConstants cleavageState)
                {
                    ProteinID = proteinId;
                    PeptideID = peptideId;
                    CleavageState = cleavageState;
                }

                public int CompareTo(ProteinToPeptideMappingEntry other)
                {
                    if (ReferenceEquals(this, other))
                    {
                        return 0;
                    }

                    if (other is null)
                    {
                        return 1;
                    }
                    // Sort by ProteinID, then by PeptideID
                    var proteinIdComparison = ProteinID.CompareTo(other.ProteinID);
                    if (proteinIdComparison != 0)
                    {
                        return proteinIdComparison;
                    }

                    return PeptideID.CompareTo(other.PeptideID);
                }
            }

            private int mMappingCount;
            private ProteinToPeptideMappingEntry[] mMappings;
            private bool mMappingArrayIsSorted;

            public event SortingListEventHandler SortingList;

            public ProteinToPeptideMappingInfo()
            {
                Clear();
            }

            public bool AddProteinToPeptideMapping(int proteinID, int peptideID, CleavageStateConstants cleavageState = CleavageStateConstants.Unknown)
            {
                // Add the mapping
                if (mMappingCount >= mMappings.Length)
                {
                    Array.Resize(ref mMappings, mMappings.Length * 2);
                    //Array.Resize(ref mMappings, mMappings.Length + MEMORY_RESERVE_CHUNK - 1);
                }

                mMappings[mMappingCount] = new ProteinToPeptideMappingEntry(proteinID, peptideID, cleavageState);
                mMappingCount++;
                mMappingArrayIsSorted = false;

                // If we get here, all went well
                return true;
            }

            // ReSharper disable once UnusedMember.Local
            public bool AddProteinToPeptideMapping(ProteinCollection proteinInfo, string proteinName, int peptideID)
            {
                return AddProteinToPeptideMapping(proteinInfo, proteinName, peptideID, CleavageStateConstants.Unknown);
            }

            public bool AddProteinToPeptideMapping(ProteinCollection proteinInfo, string proteinName, int peptideID, CleavageStateConstants cleavageState)
            {
                if (!proteinInfo.GetProteinIDByProteinName(proteinName, out var proteinID))
                {
                    // Need to add the protein
                    if (!proteinInfo.Add(proteinName, out proteinID))
                    {
                        return false;
                    }
                }

                return AddProteinToPeptideMapping(proteinID, peptideID, cleavageState);
            }

            private int BinarySearchFindProteinMapping(int proteinIDToFind)
            {
                // Looks through mMappings[] for proteinIDToFind, returning the index of the item if found, or -1 if not found
                // Since mMappings[] can contain multiple entries for a given Protein, this function returns the first entry found

                var firstIndex = 0;
                var lastIndex = mMappingCount - 1;

                var matchingRowIndex = -1;

                if (mMappingCount <= 0 || !SortMappings())
                {
                    return matchingRowIndex;
                }

                try
                {
                    var midIndex = lastIndex / 2; // Note: Using Integer division
                    if (midIndex < firstIndex)
                    {
                        midIndex = firstIndex;
                    }

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
                        {
                            break;
                        }
                    }

                    if (midIndex >= firstIndex && midIndex <= lastIndex)
                    {
                        if (mMappings[midIndex].ProteinID == proteinIDToFind)
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
                mMappingCount = 0;

                mMappings ??= new ProteinToPeptideMappingEntry[100000];

                mMappingArrayIsSorted = false;
            }

            public int[] GetPeptideIDsMappedToProteinID(int proteinID)
            {
                // Returns all of the peptides for the given protein ID

                int[] matchingIDs;

                if (GetRowIndicesForProteinID(proteinID, out var indexFirst, out var indexLast))
                {
                    matchingIDs = new int[indexLast - indexFirst + 1];

                    for (var index = indexFirst; index <= indexLast; index++)
                    {
                        matchingIDs[index - indexFirst] = mMappings[index].PeptideID;
                    }
                }
                else
                {
                    matchingIDs = Array.Empty<int>();
                }

                return matchingIDs;
            }

            public int[] GetProteinIDsMappedToPeptideID(int peptideID)
            {
                // Since mMappings is sorted by Protein ID, we must fully search the array to obtain the ProteinIDs for peptideID

                const int ARRAY_ALLOCATION_CHUNK = 10;

                var matchingIDs = new int[ARRAY_ALLOCATION_CHUNK];
                var matchCount = 0;

                for (var index = 0; index < mMappingCount; index++)
                {
                    if (mMappings[index].PeptideID == peptideID)
                    {
                        if (matchCount >= matchingIDs.Length)
                        {
                            Array.Resize(ref matchingIDs, matchingIDs.Length * 2);
                            //Array.Resize(ref matchingIDs, matchingIDs.Length + ARRAY_ALLOCATION_CHUNK - 1);
                        }

                        matchingIDs[matchCount] = mMappings[index].ProteinID;
                        matchCount++;
                    }
                }

                if (matchingIDs.Length > matchCount)
                {
                    Array.Resize(ref matchingIDs, matchCount);
                }

                return matchingIDs;
            }

            private bool GetRowIndicesForProteinID(int proteinID, out int indexFirst, out int indexLast)
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
                    {
                        indexFirst--;
                    }

                    // Step forward through mMappings to find the last match for proteinID
                    while (indexLast < mMappingCount - 1 && mMappings[indexLast + 1].ProteinID == proteinID)
                    {
                        indexLast++;
                    }

                    return true;
                }

                indexFirst = -1;
                indexLast = -1;
                return false;
            }

            public int GetPeptideCountForProteinID(int proteinID)
            {
                if (GetRowIndicesForProteinID(proteinID, out var indexFirst, out var indexLast))
                {
                    return indexLast - indexFirst + 1;
                }

                return 0;
            }

            private bool SortMappings(bool forceSort = false)
            {
                if (!mMappingArrayIsSorted || forceSort)
                {
                    SortingList?.Invoke();
                    Array.Sort(mMappings, 0, mMappingCount);

                    mMappingArrayIsSorted = true;
                }

                return mMappingArrayIsSorted;
            }
        }
    }
}