namespace ProteinDigestionSimulator.Options
{
    public class PeakMatchingOptions
    {
        private int mMaxPeakMatchingResultsPerFeatureToSave;

        public MassBinningOptions BinningSettings { get; }

        public bool CreateSeparateOutputFileForEachThreshold { get; set; }

        public int MaxPeakMatchingResultsPerFeatureToSave
        {
            get => mMaxPeakMatchingResultsPerFeatureToSave;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                mMaxPeakMatchingResultsPerFeatureToSave = value;
            }
        }

        public bool SavePeakMatchingResults { get; set; }

        /// <summary>
        /// Use Ellipse Search Region
        /// </summary>
        /// <remarks>
        /// Only valid if mUseSLiCScoreForUniqueness = False
        /// If both mUseSLiCScoreForUniqueness = False and mUseEllipseSearchRegion = False, uses a rectangle to determine uniqueness
        /// </remarks>
        public bool UseEllipseSearchRegion { get; set; }

        public bool UseMaxSearchDistanceMultiplierAndSLiCScore { get; set; }

        public bool UseSLiCScoreForUniqueness { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PeakMatchingOptions()
        {
            BinningSettings = new MassBinningOptions();
            CreateSeparateOutputFileForEachThreshold = false;

            MaxPeakMatchingResultsPerFeatureToSave = 3;
            SavePeakMatchingResults = false;

            UseEllipseSearchRegion = true;
            UseMaxSearchDistanceMultiplierAndSLiCScore = true;
            UseSLiCScoreForUniqueness = true;
        }

        public class MassBinningOptions
        {
            /// <summary>
            /// When true, auto-determine the mass range for binning
            /// </summary>
            public bool AutoDetermineMassRange { get; set; }
            public float MassBinSizeDa { get; set; }
            public float MassMinimum { get; set; } // This is ignored if AutoDetermineMassRange = True
            public float MassMaximum { get; set; } // This is ignored if AutoDetermineMassRange = True
            public float MinimumSLiCScore { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public MassBinningOptions()
            {
                AutoDetermineMassRange = true;
                MassBinSizeDa = 25f;
                MassMinimum = 400f;
                MassMaximum = 6000f;
                MinimumSLiCScore = 0.99f;
            }

            public MassBinningOptions Clone()
            {
                return (MassBinningOptions)MemberwiseClone();
            }
        }
    }
}
