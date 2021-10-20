using System;
using System.Collections.Generic;
using System.Linq;
using ProteinDigestionSimulator.Options;

namespace ProteinDigestionSimulator
{
    /// <summary>
    /// This class can be used to perform an in-silico digest of an amino acid sequence
    /// Utilizes PeptideSequenceWithNET
    /// </summary>
    public class InSilicoDigest
    {
        // Ignore Spelling: alkylated, Arg, Chymotrypsin, cysteine, frag, Glu, Ile, isoelectric
        // Ignore Spelling: Leu, Lys, proline, Proteinase, silico, terminii, Thermolysin, tryptic, tryptics

        private DigestionOptions DigestionOptions { get; }

        private readonly Dictionary<CleavageRuleConstants, CleavageRule> mCleavageRules = new();

        /// <summary>
        /// General purpose object for computing mass and calling cleavage and digestion functions
        /// </summary>
        private readonly PeptideSequence mPeptideSequence;

        public event ErrorEventEventHandler ErrorEvent;

        public event ProgressResetEventHandler ProgressReset;

        public event ProgressChangedEventHandler ProgressChanged;

        private string mProgressStepDescription;

        /// <summary>
        /// Percent complete, ranges from 0 to 100, but can contain decimal percentage values
        /// </summary>
        private float mProgressPercentComplete;

        /// <summary>
        /// Dictionary mapping cleavage rule types to cleavage rule definitions
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public IReadOnlyDictionary<CleavageRuleConstants, CleavageRule> CleavageRules => mCleavageRules;

        public PeptideSequence.ElementModeConstants ElementMassMode
        {
            get => mPeptideSequence.ElementMode;
            set => mPeptideSequence.ElementMode = value;
        }

        public ComputePeptideProperties IsoelectricPointCalculator { get; }

        public string ProgressStepDescription => mProgressStepDescription;

        /// <summary>
        /// Percent complete, value between 0 and 100, but can contain decimal percentage values
        /// </summary>
        public float ProgressPercentComplete => (float)Math.Round(mProgressPercentComplete, 2);

        /// <summary>
        /// Constructor
        /// </summary>
        public InSilicoDigest(DigestionSimulatorOptions options)
        {
            DigestionOptions = options.DigestionOptions;

            mPeptideSequence = new PeptideSequence { ElementMode = options.ElementMassMode };

            IsoelectricPointCalculator = new ComputePeptideProperties(options);

            InitializeCleavageRules();
        }

        private CleavageRule AddCleavageRule(
            CleavageRuleConstants ruleId,
            string description,
            string cleavageResidues,
            string exceptionResidues,
            bool reversedCleavageDirection,
            bool allowPartialCleavage = false,
            IReadOnlyCollection<CleavageRule> additionalCleavageRules = null)
        {
            var cleavageRule = new CleavageRule(
                description,
                cleavageResidues,
                exceptionResidues,
                reversedCleavageDirection,
                allowPartialCleavage,
                additionalCleavageRules);

            mCleavageRules.Add(ruleId, cleavageRule);

            return cleavageRule;
        }

        /// <summary>
        /// Checks sequence against the rule given by ruleId
        /// </summary>
        /// <remarks>
        /// In order to check for Exception residues, sequence must be in the form "R.ABCDEFGK.L" so that the residue following the final residue of the fragment can be examined.
        /// See method InitializeCleavageRules for a list of the rules</remarks>
        /// <param name="sequence"></param>
        /// <param name="ruleId"></param>
        /// <param name="ruleMatchCount">Output: 0 if neither end matches, 1 if one end matches, 2 if both ends match</param>
        /// <returns>True if valid, False if invalid</returns>
        // ReSharper disable once UnusedMember.Global
        public bool CheckSequenceAgainstCleavageRule(string sequence, CleavageRuleConstants ruleId, out int ruleMatchCount)
        {
            if (mCleavageRules.TryGetValue(ruleId, out var cleavageRule))
            {
                if (ruleId == CleavageRuleConstants.NoRule)
                {
                    // No cleavage rule; no point in checking
                    ruleMatchCount = 2;
                    return true;
                }

                return mPeptideSequence.CheckSequenceAgainstCleavageRule(sequence, cleavageRule, out ruleMatchCount);
            }

            // No rule selected; assume True
            ruleMatchCount = 2;
            return true;
        }

        /// <summary>
        /// Compute the monoisotopic mass of the sequence
        /// </summary>
        /// <param name="sequence">Residues in 1-letter notation; automatically be converted to uppercase</param>
        /// <param name="includeXResiduesInMass">When true, treat X residues as Ile/Leu (C6H11NO)</param>
        public double ComputeSequenceMass(string sequence, bool includeXResiduesInMass = true)
        {
            try
            {
                if (includeXResiduesInMass)
                {
                    mPeptideSequence.SetSequence(sequence.ToUpper());
                }
                else
                {
                    // Exclude X residues from sequence when calling .SetSequence
                    mPeptideSequence.SetSequence(sequence.ToUpper().Replace("X", string.Empty));
                }

                return mPeptideSequence.Mass;
            }
            catch (Exception ex)
            {
                ReportError("ComputeSequenceMass", ex);
                return 0d;
            }
        }

        public int CountTrypticsInSequence(string sequence)
        {
            var trypticCount = 0;
            var startSearchLoc = 1;

            try
            {
                if (sequence.Length > 0)
                {
                    while (true)
                    {
                        var fragment = mPeptideSequence.GetTrypticPeptideNext(sequence, startSearchLoc, out _, out var returnResidueEnd);
                        if (fragment.Length > 0)
                        {
                            trypticCount++;
                            startSearchLoc = returnResidueEnd + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return trypticCount;
            }
            catch (Exception ex)
            {
                ReportError("CountTrypticsInSequence", ex);
                Console.WriteLine("Tryptic count is " + trypticCount);
                Console.WriteLine("Current startSearchLoc is " + startSearchLoc);
                return 0;
            }
        }

        internal readonly struct TrypticFragment
        {
            public string Sequence { get; }
            public int StartLoc { get; }
            public int EndLoc { get; }

            public TrypticFragment(string sequence, int startLoc, int endLoc)
            {
                Sequence = sequence;
                StartLoc = startLoc;
                EndLoc = endLoc;
            }
        }

        /// <summary>
        /// Digests proteinSequence using the sequence rule given by digestionOptions.CleavageRuleID
        /// If removeDuplicateSequences = True, only returns the first occurrence of each unique sequence
        /// </summary>
        /// <param name="proteinSequence"></param>
        /// <param name="peptideFragments"></param>
        /// <param name="filterByIsoelectricPoint"></param>
        /// <param name="proteinName"></param>
        /// <returns>The number of peptides in peptideFragments</returns>
        public int DigestSequence(string proteinSequence,
                                  out List<PeptideSequenceWithNET> peptideFragments,
                                  bool filterByIsoelectricPoint,
                                  string proteinName)
        {
            var fragmentsUniqueList = new SortedSet<string>();
            peptideFragments = new List<PeptideSequenceWithNET>();

            if (string.IsNullOrWhiteSpace(proteinSequence))
            {
                return 0;
            }

            var proteinSequenceLength = proteinSequence.Length;

            try
            {
                var success = GetCleavageRuleById(DigestionOptions.CleavageRuleID, out var cleavageRule);
                if (!success)
                {
                    ReportError("DigestSequence", new Exception("Invalid cleavage rule: " + (int)DigestionOptions.CleavageRuleID));
                    return 0;
                }

                // We initially count the number of tryptic peptides in the sequence (regardless of the cleavage rule)
                // ReSharper disable once UnusedVariable
                var trypticFragmentCount = CountTrypticsInSequence(proteinSequence);

                var trypticFragCache = new List<TrypticFragment>(10);
                var searchStartLoc = 1;

                // Populate trypticFragCache[]
                //
                // Using the GetTrypticPeptideNext function to retrieve the sequence for each tryptic peptide
                // is faster than using the GetTrypticPeptideByFragmentNumber function
                while (true)
                {
                    var peptideSequence = mPeptideSequence.GetTrypticPeptideNext(proteinSequence, searchStartLoc, cleavageRule, out var residueStartLoc, out var residueEndLoc);
                    if (peptideSequence.Length > 0)
                    {
                        trypticFragCache.Add(new TrypticFragment(peptideSequence, residueStartLoc, residueEndLoc));
                        searchStartLoc = residueEndLoc + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                ResetProgress("Digesting protein " + proteinName);

                double minFragmentMass;
                double maxFragmentMass;

                if (DigestionOptions.FragmentMassMode == FragmentMassConstants.MH)
                {
                    // Adjust the thresholds down by the charge carrier mass (which is easier than computing the M+H mass of every peptide)
                    minFragmentMass = DigestionOptions.MinFragmentMass - PeptideSequence.ChargeCarrierMass;
                    maxFragmentMass = DigestionOptions.MaxFragmentMass - PeptideSequence.ChargeCarrierMass;
                }
                else
                {
                    minFragmentMass = DigestionOptions.MinFragmentMass;
                    maxFragmentMass = DigestionOptions.MaxFragmentMass;
                }

                for (var trypticIndex = 0; trypticIndex < trypticFragCache.Count; trypticIndex++)
                {
                    var peptideSequenceBase = string.Empty;
                    var peptideSequence = string.Empty;
                    var residueStartLoc = trypticFragCache[trypticIndex].StartLoc;

                    for (var index = 0; index <= DigestionOptions.MaxMissedCleavages; index++)
                    {
                        if (trypticIndex + index >= trypticFragCache.Count)
                        {
                            break;
                        }

                        int residueEndLoc;
                        if (DigestionOptions.CleavageRuleID == CleavageRuleConstants.KROneEnd)
                        {
                            // Partially tryptic cleavage rule: Add all partially tryptic fragments
                            int residueLengthStart;
                            if (index == 0)
                            {
                                residueLengthStart = DigestionOptions.MinFragmentResidueCount;
                                if (residueLengthStart < 1)
                                {
                                    residueLengthStart = 1;
                                }
                            }
                            else
                            {
                                residueLengthStart = 1;
                            }

                            for (var residueLength = residueLengthStart; residueLength <= trypticFragCache[trypticIndex + index].Sequence.Length; residueLength++)
                            {
                                if (index > 0)
                                {
                                    residueEndLoc = trypticFragCache[trypticIndex + index - 1].EndLoc + residueLength;
                                }
                                else
                                {
                                    residueEndLoc = residueStartLoc + residueLength - 1;
                                }

                                peptideSequence = peptideSequenceBase + trypticFragCache[trypticIndex + index].Sequence.Substring(0, residueLength);

                                if (peptideSequence.Length >= DigestionOptions.MinFragmentResidueCount)
                                {
                                    PossiblyAddPeptide(peptideSequence, trypticIndex, index,
                                                       residueStartLoc, residueEndLoc,
                                                       proteinSequence, proteinSequenceLength,
                                                       fragmentsUniqueList, peptideFragments,
                                                       filterByIsoelectricPoint,
                                                       minFragmentMass, maxFragmentMass);
                                }
                            }
                        }
                        else
                        {
                            // Normal cleavage rule
                            residueEndLoc = trypticFragCache[trypticIndex + index].EndLoc;
                            peptideSequence += trypticFragCache[trypticIndex + index].Sequence;
                            if (peptideSequence.Length >= DigestionOptions.MinFragmentResidueCount)
                            {
                                PossiblyAddPeptide(peptideSequence, trypticIndex, index,
                                                   residueStartLoc, residueEndLoc,
                                                   proteinSequence, proteinSequenceLength,
                                                   fragmentsUniqueList, peptideFragments,
                                                   filterByIsoelectricPoint,
                                                   minFragmentMass, maxFragmentMass);
                            }
                        }

                        peptideSequenceBase += trypticFragCache[trypticIndex + index].Sequence;
                    }

                    if (DigestionOptions.CleavageRuleID == CleavageRuleConstants.KROneEnd)
                    {
                        UpdateProgress((float)(trypticIndex / (double)(trypticFragCache.Count * 2) * 100.0d));
                    }
                }

                if (DigestionOptions.CleavageRuleID == CleavageRuleConstants.KROneEnd)
                {
                    // Partially tryptic cleavage rule: Add all partially tryptic fragments, working from the end toward the front
                    for (var trypticIndex = trypticFragCache.Count - 1; trypticIndex >= 0; --trypticIndex)
                    {
                        var peptideSequenceBase = string.Empty;

                        var residueEndLoc = trypticFragCache[trypticIndex].EndLoc;

                        for (var index = 0; index <= DigestionOptions.MaxMissedCleavages; index++)
                        {
                            if (trypticIndex - index < 0)
                            {
                                break;
                            }

                            int residueLengthStart;

                            if (index == 0)
                            {
                                residueLengthStart = DigestionOptions.MinFragmentResidueCount;
                            }
                            else
                            {
                                residueLengthStart = 1;
                            }

                            // We can limit the following for loop to the peptide length - 1 since those peptides using the full peptide will have already been added above
                            for (var residueLength = residueLengthStart; residueLength < trypticFragCache[trypticIndex - index].Sequence.Length; residueLength++)
                            {
                                int residueStartLoc;

                                if (index > 0)
                                {
                                    residueStartLoc = trypticFragCache[trypticIndex - index + 1].StartLoc - residueLength;
                                }
                                else
                                {
                                    residueStartLoc = residueEndLoc - (residueLength - 1);
                                }

                                // Grab characters from the end of trypticFragCache[]
                                var peptideSequence = trypticFragCache[trypticIndex - index].Sequence.Substring(trypticFragCache[trypticIndex - index].Sequence.Length - residueLength, residueLength) + peptideSequenceBase;

                                if (peptideSequence.Length >= DigestionOptions.MinFragmentResidueCount)
                                {
                                    PossiblyAddPeptide(peptideSequence, trypticIndex, index,
                                    residueStartLoc, residueEndLoc,
                                    proteinSequence, proteinSequenceLength,
                                    fragmentsUniqueList, peptideFragments,
                                    filterByIsoelectricPoint,
                                    minFragmentMass, maxFragmentMass);
                                }
                            }

                            peptideSequenceBase = trypticFragCache[trypticIndex - index].Sequence + peptideSequenceBase;
                        }

                        UpdateProgress((float)((trypticFragCache.Count * 2 - trypticIndex) / (double)(trypticFragCache.Count * 2) * 100d));
                    }
                }

                return peptideFragments.Count;
            }
            catch (Exception ex)
            {
                ReportError("DigestSequence", ex);
                return peptideFragments.Count;
            }
        }

        public bool GetCleavageRuleById(CleavageRuleConstants ruleId, out CleavageRule cleavageRule)
        {
            return mCleavageRules.TryGetValue(ruleId, out cleavageRule);
        }

        private void InitializeCleavageRules()
        {
            // Useful site for cleavage rule info is https://web.expasy.org/peptide_mass/peptide-mass-doc.html

            mCleavageRules.Clear();

            // ReSharper disable StringLiteralTypo
            AddCleavageRule(CleavageRuleConstants.NoRule,
                            "No cleavage rule",
                            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                            string.Empty,
                            false);

            AddCleavageRule(CleavageRuleConstants.ConventionalTrypsin,
                "Fully Tryptic",
                "KR",
                "P",
                false);

            AddCleavageRule(CleavageRuleConstants.TrypsinWithoutProlineException,
                "Fully Tryptic (no Proline Rule)",
                "KR",
                string.Empty,
                false);

            // Allows partial cleavage
            AddCleavageRule(CleavageRuleConstants.EricPartialTrypsin,
                "Eric's Partial Trypsin",
                "KRFYVEL",
                string.Empty,
                false,
                true);

            AddCleavageRule(CleavageRuleConstants.TrypsinPlusFVLEY,
                "Trypsin plus FVLEY",
                "KRFYVEL",
                string.Empty,
                false);

            // Allows partial cleavage
            AddCleavageRule(CleavageRuleConstants.KROneEnd,
                "Half (Partial) Trypsin ",
                "KR",
                "P",
                false,
                true);

            AddCleavageRule(CleavageRuleConstants.TerminiiOnly,
                "Peptide Database; terminii only",
                "-",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.Chymotrypsin,
                "Chymotrypsin",
                "FWYL",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.ChymotrypsinAndTrypsin,
                "Chymotrypsin + Trypsin",
                "FWYLKR",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.GluC,
                "Glu-C",
                "ED",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.CyanBr,
                "CyanBr",
                "M",
                string.Empty,
                false);

            var cleavageRuleLysC = AddCleavageRule(CleavageRuleConstants.LysC,
                "Lys-C",
                "K",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.GluC_EOnly,
                "Glu-C, just Glu",
                "E",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.ArgC,
                "Arg-C",
                "R",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.AspN,
                "Asp-N",
                "D",
                string.Empty,
                true);

            AddCleavageRule(CleavageRuleConstants.ProteinaseK,
                "Proteinase K",
                "AEFILTVWY",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.PepsinA,
                "PepsinA",
                "FLIWY",
                "P",
                false);

            AddCleavageRule(CleavageRuleConstants.PepsinB,
                "PepsinB",
                "FLIWY",
                "PVAG",
                false);

            AddCleavageRule(CleavageRuleConstants.PepsinC,
                "PepsinC",
                "FLWYA",
                "P",
                false);

            AddCleavageRule(CleavageRuleConstants.PepsinD,
                "PepsinD",
                "FLWYAEQ",
                string.Empty,
                false);

            AddCleavageRule(CleavageRuleConstants.AceticAcidD,
                "Acetic Acid Hydrolysis",
                "D",
                string.Empty,
                false);

            var additionalRuleLysC = new List<CleavageRule> { cleavageRuleLysC };

            AddCleavageRule(CleavageRuleConstants.TrypsinPlusLysC,
                            "Trypsin plus Lys-C",
                            "KR",
                            "P",
                            false,
                            false,
                            additionalRuleLysC);

            var cleavageRuleThermolysin = AddCleavageRule(CleavageRuleConstants.Thermolysin,
                                                          "Thermolysin",
                                                          "LFVIAM",
                                                          string.Empty,
                                                          true);

            var additionalRuleThermolysin = new List<CleavageRule> { cleavageRuleThermolysin };

            AddCleavageRule(CleavageRuleConstants.TrypsinPlusThermolysin,
                            "Trypsin plus Thermolysin",
                            "KR",
                            "P",
                            false,
                            false,
                            additionalRuleThermolysin);

            // ReSharper restore StringLiteralTypo
        }

        private void PossiblyAddPeptide(
            string peptideSequence,
            int trypticIndex,
            int missedCleavageCount,
            int residueStartLoc,
            int residueEndLoc,
            string proteinSequence,
            int proteinSequenceLength,
            ISet<string> fragmentsUniqueList,
            ICollection<PeptideSequenceWithNET> peptideFragments,
            bool filterByIsoelectricPoint,
            double minFragmentMass,
            double maxFragmentMass)
        {
            var addFragment = true;
            if (DigestionOptions.RemoveDuplicateSequences)
            {
                if (fragmentsUniqueList.Contains(peptideSequence))
                {
                    addFragment = false;
                }
                else
                {
                    fragmentsUniqueList.Add(peptideSequence);
                }
            }

            if (addFragment && DigestionOptions.AminoAcidResidueFilterChars.Count > 0)
            {
                addFragment = DigestionOptions.AminoAcidResidueFilterChars.Any(peptideSequence.Contains);
            }

            if (!addFragment)
            {
                return;
            }

            var peptideFragment = new PeptideSequenceWithNET
            {
                AutoComputeNET = false,
                CysTreatmentMode = DigestionOptions.CysTreatmentMode,
                SequenceOneLetter = peptideSequence
            };

            if (peptideFragment.Mass < minFragmentMass ||
                peptideFragment.Mass > maxFragmentMass)
            {
                return;
            }

            float isoelectricPoint = 0;

            // Possibly compute the isoelectric point for the peptide
            if (filterByIsoelectricPoint)
            {
                isoelectricPoint = IsoelectricPointCalculator.CalculateSequencepI(peptideSequence);
            }

            if (filterByIsoelectricPoint &&
                (isoelectricPoint < DigestionOptions.MinIsoelectricPoint ||
                 isoelectricPoint > DigestionOptions.MaxIsoelectricPoint))
            {
                return;
            }

            // We can now compute the NET value for the peptide
            peptideFragment.UpdateNET();

            if (DigestionOptions.IncludePrefixAndSuffixResidues)
            {
                string prefix;
                string suffix;

                if (residueStartLoc <= 1)
                {
                    prefix = PeptideSequenceWithNET.PROTEIN_TERMINUS_SYMBOL;
                }
                else
                {
                    prefix = proteinSequence.Substring(residueStartLoc - 2, 1);
                }

                if (residueEndLoc >= proteinSequenceLength)
                {
                    suffix = PeptideSequenceWithNET.PROTEIN_TERMINUS_SYMBOL;
                }
                else
                {
                    suffix = proteinSequence.Substring(residueEndLoc, 1);
                }

                peptideFragment.PrefixResidue = prefix;
                peptideFragment.SuffixResidue = suffix;
            }
            else
            {
                peptideFragment.PrefixResidue = PeptideSequenceWithNET.PROTEIN_TERMINUS_SYMBOL;
                peptideFragment.SuffixResidue = PeptideSequenceWithNET.PROTEIN_TERMINUS_SYMBOL;
            }

            if (DigestionOptions.CleavageRuleID is CleavageRuleConstants.ConventionalTrypsin or CleavageRuleConstants.TrypsinWithoutProlineException)
            {
                peptideFragment.PeptideName = "t" + (trypticIndex + 1) + "." + (missedCleavageCount + 1);
            }
            else
            {
                peptideFragment.PeptideName = residueStartLoc + "." + residueEndLoc;
            }

            peptideFragments.Add(peptideFragment);
        }

        private void ReportError(string functionName, Exception ex)
        {
            try
            {
                string errorMessage;

                if (!string.IsNullOrEmpty(functionName))
                {
                    errorMessage = "Error in " + functionName + ": " + ex.Message;
                }
                else
                {
                    errorMessage = "Error: " + ex.Message;
                }

                Console.WriteLine(errorMessage);

                Console.WriteLine(PRISM.StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));

                ErrorEvent?.Invoke(errorMessage);
            }
            catch
            {
                // Ignore errors here
            }
        }

        private void ResetProgress(string description)
        {
            UpdateProgress(description, 0f);
            ProgressReset?.Invoke();
        }

        private void UpdateProgress(float percentComplete)
        {
            UpdateProgress(ProgressStepDescription, percentComplete);
        }

        private void UpdateProgress(string description, float percentComplete)
        {
            mProgressStepDescription = string.Copy(description);
            if (percentComplete < 0f)
            {
                percentComplete = 0f;
            }
            else if (percentComplete > 100f)
            {
                percentComplete = 100f;
            }

            mProgressPercentComplete = percentComplete;

            ProgressChanged?.Invoke(description, ProgressPercentComplete);
        }

        public class PeptideSequenceWithNET : PeptideSequence
        {
            // Adds NET computation to the PeptideSequence

            public PeptideSequenceWithNET()
            {
                NETPredictor ??= new NETPrediction.ElutionTimePredictionKangas();

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
}
