using System.Collections.Generic;

namespace ProteinDigestionSimulator.Options
{
    public class DigestionOptions
    {
        // Ignore Spelling: cysteine

        private bool mCysPeptidesOnly;

        private int mMaxMissedCleavages;

        private int mMinFragmentMass;
        private int mMaxFragmentMass;

        private int mMinFragmentResidueCount;

        public List<char> AminoAcidResidueFilterChars { get; }

        public CleavageRuleConstants CleavageRuleID { get; set; }

        /// <summary>
        /// When true, while digesting protein sequences, only keep peptides with cysteine
        /// </summary>
        /// <remarks>Setting this to true or false will auto-update AminoAcidResidueFilterChars</remarks>
        public bool CysPeptidesOnly
        {
            get => mCysPeptidesOnly;
            set
            {
                mCysPeptidesOnly = value;

                AminoAcidResidueFilterChars.Clear();
                if (mCysPeptidesOnly)
                {
                    AminoAcidResidueFilterChars.Add('C');
                }
            }
        }

        public PeptideSequence.CysTreatmentModeConstants CysTreatmentMode { get; set; }

        /// <summary>
        /// When true, digest protein sequences
        /// </summary>
        /// <remarks>Ignored for FASTA files; they are always digested</remarks>
        public bool DigestSequences { get; set; }

        public FragmentMassConstants FragmentMassMode { get; set; }

        public int MinFragmentResidueCount
        {
            get => mMinFragmentResidueCount;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                mMinFragmentResidueCount = value;
            }
        }

        public int MinFragmentMass
        {
            get => mMinFragmentMass;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                mMinFragmentMass = value;
            }
        }

        public int MaxFragmentMass
        {
            get => mMaxFragmentMass;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                mMaxFragmentMass = value;
            }
        }

        public int MaxMissedCleavages
        {
            get => mMaxMissedCleavages;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (value > 500000)
                {
                    value = 500000;
                }

                mMaxMissedCleavages = value;
            }
        }

        public float MinIsoelectricPoint { get; set; }

        public float MaxIsoelectricPoint { get; set; }

        public bool RemoveDuplicateSequences { get; set; }
        public bool IncludePrefixAndSuffixResidues { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DigestionOptions()
        {
            AminoAcidResidueFilterChars = new List<char>();

            CleavageRuleID = CleavageRuleConstants.ConventionalTrypsin;

            CysTreatmentMode = PeptideSequence.CysTreatmentModeConstants.Untreated;

            DigestSequences = false;

            FragmentMassMode = Options.FragmentMassConstants.Monoisotopic;

            IncludePrefixAndSuffixResidues = false;

            MinFragmentMass = 0;
            MaxFragmentMass = 6000;

            MinFragmentResidueCount = 4;

            MinIsoelectricPoint = 0f;
            MaxIsoelectricPoint = 100f;

            MaxMissedCleavages = 0;

            RemoveDuplicateSequences = false;
        }

        // ReSharper disable once UnusedMember.Global
        public void ValidateOptions()
        {
            if (mMaxFragmentMass < mMinFragmentMass)
            {
                mMaxFragmentMass = mMinFragmentMass;
            }

            if (MaxIsoelectricPoint < MinIsoelectricPoint)
            {
                MaxIsoelectricPoint = MinIsoelectricPoint;
            }
        }
    }
}
