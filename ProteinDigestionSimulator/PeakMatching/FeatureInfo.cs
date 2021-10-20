using System;

namespace ProteinDigestionSimulator.PeakMatching
{
    public class FeatureInfo : IComparable<FeatureInfo>
    {
        // Ignore Spelling: Da

        /// <summary>
        /// Feature ID
        /// </summary>
        /// <remarks>
        /// Each feature should have a unique ID
        /// </remarks>
        public int FeatureID { get; }

        /// <summary>
        /// Feature name (optional)
        /// </summary>
        public string FeatureName { get; }

        /// <summary>
        /// Mass
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// Normalized elution time
        /// </summary>
        public float NET { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="featureId"></param>
        /// <param name="featureName"></param>
        /// <param name="mass"></param>
        /// <param name="net"></param>
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

        /// <summary>
        /// Show feature ID, feature name, mass, and NET
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}: {1,-10}, {2:F3} Da, {3:F2} NET", FeatureID, FeatureName, Mass, NET);
        }
    }
}
