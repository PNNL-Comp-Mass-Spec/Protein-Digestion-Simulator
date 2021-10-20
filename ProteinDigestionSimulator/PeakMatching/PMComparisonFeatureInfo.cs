using System;

namespace ProteinDigestionSimulator.PeakMatching
{
    internal class PMComparisonFeatureInfo : PMFeatureInfo
    {
        private readonly struct ComparisonFeatureInfoExtended
        {
            public float NETStDev { get; }
            public float DiscriminantScore { get; }

            public ComparisonFeatureInfoExtended(float netStDev, float discriminantScore)
            {
                NETStDev = netStDev;
                DiscriminantScore = discriminantScore;
            }
        }

        private ComparisonFeatureInfoExtended[] mExtendedInfo;

        public PMComparisonFeatureInfo()
        {
            Clear();
        }

        public bool Add(FeatureInfo featureInfo, float peptideNETStDev, float peptideDiscriminantScore)
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

            mExtendedInfo ??= new ComparisonFeatureInfoExtended[10000];
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

        public float GetNETStDevByRowIndex(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < mFeatureCount)
            {
                return mExtendedInfo[rowIndex].NETStDev;
            }

            return 0f;
        }
    }
}
