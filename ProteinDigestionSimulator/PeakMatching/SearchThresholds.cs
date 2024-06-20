using System;

namespace ProteinDigestionSimulator.PeakMatching
{
    public enum MassToleranceConstants
    {
        /// <summary>
        /// Parts per million
        /// </summary>
        PPM = 0,

        /// <summary>
        /// Absolute mass (Da)
        /// </summary>
        Absolute = 1
    }

    public class SearchThresholds
    {
        // Ignore Spelling: Da

        public const float DEFAULT_SLIC_MAX_SEARCH_DISTANCE_MULTIPLIER = 2f;

        private const int STDEV_SCALING_FACTOR = 2;
        private const double ONE_PART_PER_MILLION = 1000000.0d;

        // The following defines how the SLiC scores (aka match scores) are computed
        private class SLiCScoreOptions
        {
            public double MassPPMStDev { get; set; }                  // Default 3
            public double NETStDev { get; set; }                      // Default 0.025
            public bool UseAMTNETStDev { get; set; }
            public float MaxSearchDistanceMultiplier { get; set; }   // Default 2
        }

        // Note that these tolerances are half-widths, i.e. tolerance +- comparison value
        public class SearchTolerances
        {
            public double MWTolAbsBroad { get; set; }
            public double MWTolAbsFinal { get; set; }

            public double NETTolBroad { get; set; }
            public double NETTolFinal { get; set; }
        }

        private double mMassTolerance;          // Mass search tolerance, +- this value; TolType defines if this is PPM or Da
        private double mNETTolerance;           // NET search tolerance, +- this value
        private float mSLiCScoreMaxSearchDistanceMultiplier;

        private readonly SLiCScoreOptions mSLiCScoreOptions = new();

        public bool AutoDefineSLiCScoreThresholds { get; set; }

        public SearchTolerances ComputedSearchTolerances { get; } = new();

        public MassToleranceConstants MassTolType { get; set; }

        public double MassTolerance
        {
            get => mMassTolerance;
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
            get => mNETTolerance;
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
            get => mSLiCScoreOptions.MassPPMStDev;
            set
            {
                if (value < 0d)
                {
                    value = 0d;
                }

                mSLiCScoreOptions.MassPPMStDev = value;
            }
        }

        public double SLiCScoreNETStDev
        {
            get => mSLiCScoreOptions.NETStDev;
            set
            {
                if (value < 0d)
                {
                    value = 0d;
                }

                mSLiCScoreOptions.NETStDev = value;
            }
        }

        public bool SLiCScoreUseAMTNETStDev
        {
            get => mSLiCScoreOptions.UseAMTNETStDev;
            set => mSLiCScoreOptions.UseAMTNETStDev = value;
        }

        public float SLiCScoreMaxSearchDistanceMultiplier
        {
            get => mSLiCScoreMaxSearchDistanceMultiplier;
            set
            {
                if (value < 1f)
                {
                    value = 1f;
                }

                mSLiCScoreMaxSearchDistanceMultiplier = value;
                if (AutoDefineSLiCScoreThresholds)
                {
                    InitializeSLiCScoreOptions(true);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchThresholds()
        {
            InitializeLocalVariables();
        }

        public void DefinePeakMatchingTolerances(ref double referenceMass)
        {
            // Thresholds are all half-widths; i.e. tolerance +- comparison value

            double massTolerancePPM;

            switch (MassTolType)
            {
                case MassToleranceConstants.PPM:
                    ComputedSearchTolerances.MWTolAbsFinal = PPMToMass(mMassTolerance, referenceMass);
                    massTolerancePPM = mMassTolerance;
                    break;

                case MassToleranceConstants.Absolute:
                    ComputedSearchTolerances.MWTolAbsFinal = mMassTolerance;
                    massTolerancePPM = referenceMass > 0d
                        ? MassToPPM(mMassTolerance, referenceMass)
                        : mSLiCScoreOptions.MassPPMStDev;
                    break;

                default:
                    Console.WriteLine("Programming error in DefinePeakMatchingTolerances; Unknown MassToleranceType: " + MassTolType);
                    massTolerancePPM = 0;
                    break;
            }

            double massToleranceToUse;
            if (massTolerancePPM < mSLiCScoreOptions.MassPPMStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR)
            {
                massToleranceToUse = mSLiCScoreOptions.MassPPMStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR;
            }
            else
            {
                massToleranceToUse = massTolerancePPM;
            }

            ComputedSearchTolerances.NETTolBroad = mSLiCScoreOptions.NETStDev * mSLiCScoreOptions.MaxSearchDistanceMultiplier * STDEV_SCALING_FACTOR;
            if (ComputedSearchTolerances.NETTolBroad < mNETTolerance)
            {
                ComputedSearchTolerances.NETTolBroad = mNETTolerance;
            }

            ComputedSearchTolerances.NETTolFinal = mNETTolerance;

            // Convert from PPM to Absolute mass
            ComputedSearchTolerances.MWTolAbsBroad = PPMToMass(massToleranceToUse, referenceMass);
        }

        public SearchTolerances GetComputedSearchTolerances(double referenceMass)
        {
            DefinePeakMatchingTolerances(ref referenceMass);
            return ComputedSearchTolerances;
        }

        private void InitializeSLiCScoreOptions(bool computeUsingSearchThresholds)
        {
            if (computeUsingSearchThresholds)
            {
                // Define the Mass StDev (in ppm) using the narrow mass tolerance divided by 2 = STDEV_SCALING_FACTOR
                mSLiCScoreOptions.MassPPMStDev = MassTolType switch
                {
                    MassToleranceConstants.Absolute => MassToPPM(mMassTolerance, 1000d) / STDEV_SCALING_FACTOR,
                    MassToleranceConstants.PPM => mMassTolerance / STDEV_SCALING_FACTOR,
                    _ => 3    // Unknown type
                };

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
