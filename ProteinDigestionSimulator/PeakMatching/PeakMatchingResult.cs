namespace ProteinDigestionSimulator.PeakMatching
{
    public class PeakMatchingResult
    {
        // Ignore Spelling: Da

        /// <summary>
        /// ID of the comparison feature (this is the real ID, and not a RowIndex)
        /// </summary>
        public int MatchingID { get; }

        /// <summary>
        /// SLiC Score (Spatially Localized Confidence score)
        /// </summary>
        public double SLiCScore { get; }

        /// <summary>
        /// Similar to DelCN, difference in SLiC score between top match and match with score value one less than this score
        /// </summary>
        public double DelSLiC { get; }

        /// <summary>
        /// Observed difference (error) between comparison mass and feature mass (in Da)
        /// </summary>
        public double MassErr { get; }

        /// <summary>
        /// Observed difference (error) between comparison NET and feature NET
        /// </summary>
        public double NETErr { get; }

        /// <summary>
        /// The number of Unique mass tag hits for each UMC; only applies to AMT's
        /// </summary>
        public int MultiAMTHitCount { get; }

        /// <summary>
        /// Parameter-less constructor
        /// </summary>
        public PeakMatchingResult() : this(0, 0, 0, 0, 0, 0)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="matchingId"></param>
        /// <param name="sliCScore"></param>
        /// <param name="delSLiC"></param>
        /// <param name="massErr"></param>
        /// <param name="netErr"></param>
        /// <param name="multiAmtHitCount"></param>
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
}
