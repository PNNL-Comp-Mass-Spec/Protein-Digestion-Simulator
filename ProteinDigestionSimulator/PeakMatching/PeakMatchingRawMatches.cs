using System;

namespace ProteinDigestionSimulator.PeakMatching
{
    internal class PeakMatchingRawMatches : IComparable<PeakMatchingRawMatches>
    {
        // Ignore Spelling: Da

        /// <summary>
        /// Pointer into comparison features (RowIndex in PMComparisonFeatureInfo)
        /// </summary>
        public int MatchingIDIndex { get; }
        public double StandardizedSquaredDistance { get; set; }
        public double SLiCScoreNumerator { get; set; }

        /// <summary>
        /// SLiC Score (Spatially Localized Confidence score)
        /// </summary>
        public double SLiCScore { get; set; }

        /// <summary>
        /// Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
        /// </summary>
        public double DelSLiC { get; set; }

        /// <summary>
        /// Observed difference (error) between comparison mass and feature mass (in Da)
        /// </summary>
        public double MassErr { get; set; }

        /// <summary>
        /// Observed difference (error) between comparison NET and feature NET
        /// </summary>
        public double NETErr { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matchingIdIndex"></param>
        public PeakMatchingRawMatches(int matchingIdIndex)
        {
            MatchingIDIndex = matchingIdIndex;
        }

        public int CompareTo(PeakMatchingRawMatches other)
        {
            // Sort by .SLiCScore descending, and by MatchingIDIndexOriginal Ascending
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (other is null)
            {
                return 1;
            }

            var sLiCScoreComparison = other.SLiCScore.CompareTo(SLiCScore);
            if (sLiCScoreComparison != 0)
            {
                return sLiCScoreComparison;
            }

            return MatchingIDIndex.CompareTo(other.MatchingIDIndex);
        }
    }
}
