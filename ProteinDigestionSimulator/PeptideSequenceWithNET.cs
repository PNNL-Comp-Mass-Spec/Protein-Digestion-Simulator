using NETPrediction;

namespace ProteinDigestionSimulator
{
    /// <summary>
    /// This class is an extension of PeptideSequence, adding NET computation
    /// </summary>
    /// <remarks>
    /// Adds NET computation to the PeptideSequence
    /// </remarks>
    public class PeptideSequenceWithNET : PeptideSequence
    {
        // Ignore Spelling: alkylated, cysteine

        /// <summary>
        /// Constructor
        /// </summary>
        public PeptideSequenceWithNET()
        {
            NETPredictor ??= new ElutionTimePredictionKangas();

            // Disable mAutoComputeNET for now so that the call to SetSequence() below doesn't auto-call UpdateNET
            AutoComputeNET = false;

            PeptideName = string.Empty;
            SetSequence(string.Empty);
            NET = 0f;
            mPrefixResidue = PROTEIN_TERMINUS_SYMBOL;
            mSuffixResidue = PROTEIN_TERMINUS_SYMBOL;

            // Re-enable mAutoComputeNET
            AutoComputeNET = true;
        }

        public const string PROTEIN_TERMINUS_SYMBOL = "-";

        // The following is declared Shared so that it is only initialized once per program execution
        // All objects of type PeptideSequenceWithNET will use the same instance of this object
        private static NETPrediction.iPeptideElutionTime NETPredictor;

        private string mPrefixResidue;
        private string mSuffixResidue;

        /// <summary>
        /// When true, auto-compute NET when Sequence changes
        /// Set to False to speed things up a little
        /// </summary>
        public bool AutoComputeNET { get; set; }

        /// <summary>
        /// Peptide name
        /// </summary>
        public string PeptideName { get; set; }

        /// <summary>
        /// Normalized elution time
        /// </summary>
        public float NET { get; private set; }

        /// <summary>
        /// Prefix residue
        /// </summary>
        public string PrefixResidue
        {
            get => mPrefixResidue;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    mPrefixResidue = value[0].ToString();
                }
                else
                {
                    mPrefixResidue = PROTEIN_TERMINUS_SYMBOL;
                }
            }
        }

        /// <summary>
        /// Peptide sequence in 1-letter format
        /// </summary>
        public string SequenceOneLetter
        {
            get => GetSequence();
            set => SetSequence(value);
        }

        /// <summary>
        /// Sequence with prefix and suffix residues
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public string SequenceWithPrefixAndSuffix => mPrefixResidue + "." + SequenceOneLetter + "." + mSuffixResidue;

        /// <summary>
        /// Suffix residue
        /// </summary>
        public string SuffixResidue
        {
            get => mSuffixResidue;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    mSuffixResidue = value[0].ToString();
                }
                else
                {
                    mSuffixResidue = PROTEIN_TERMINUS_SYMBOL;
                }
            }
        }

        /// <summary>
        /// Define the peptide sequence
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="nTerminus"></param>
        /// <param name="cTerminus"></param>
        /// <param name="is3LetterCode"></param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues"></param>
        /// <param name="threeLetterCheckForPrefixHandSuffixOH"></param>
        public sealed override int SetSequence(
            string sequence,
            NTerminusGroupConstants nTerminus = NTerminusGroupConstants.Hydrogen,
            CTerminusGroupConstants cTerminus = CTerminusGroupConstants.Hydroxyl,
            bool is3LetterCode = false,
            bool oneLetterCheckForPrefixAndSuffixResidues = true,
            bool threeLetterCheckForPrefixHandSuffixOH = true)
        {
            var returnVal = base.SetSequence(sequence, nTerminus, cTerminus, is3LetterCode, oneLetterCheckForPrefixAndSuffixResidues, threeLetterCheckForPrefixHandSuffixOH);

            if (AutoComputeNET)
            {
                UpdateNET();
            }

            return returnVal;
        }

        /// <summary>
        /// Updates the sequence without performing any error checking
        /// Does not look for or remove prefix or suffix letters
        /// </summary>
        /// <remarks>Calls UpdateSequenceMass and UpdateNET</remarks>
        /// <param name="sequenceNoPrefixOrSuffix"></param>
        public override void SetSequenceOneLetterCharactersOnly(string sequenceNoPrefixOrSuffix)
        {
            base.SetSequenceOneLetterCharactersOnly(sequenceNoPrefixOrSuffix);

            if (AutoComputeNET)
            {
                UpdateNET();
            }
        }

        /// <summary>
        /// Update the predicted normalized elution time
        /// </summary>
        public void UpdateNET()
        {
            try
            {
                var sequence = GetSequence();

                if (CysTreatmentMode != CysTreatmentModeConstants.Untreated)
                {
                    // Change cysteine residues to lowercase so that the NET predictor will recognize them as alkylated
                    NET = NETPredictor.GetElutionTime(sequence.Replace("C", "c"));
                }
                else
                {
                    NET = NETPredictor.GetElutionTime(sequence);
                }
            }
            catch
            {
                NET = 0f;
            }
        }
    }
}
