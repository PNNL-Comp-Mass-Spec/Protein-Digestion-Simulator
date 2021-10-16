using System;
using System.Collections.Generic;

namespace ProteinDigestionSimulator
{
    /// <summary>
    /// This class can be used to perform an in-silico digest of an amino acid sequence
    /// Utilizes PeptideSequenceWithNET
    /// </summary>
    public class InSilicoDigest
    {
        // Ignore Spelling: silico, Ile, Leu, Tryptics, frag, terminii
        // Ignore Spelling: Chymotrypsin, Glu, Lys, Arg, Proteinase, Thermolysin, isoelectric, alkylated

        public InSilicoDigest()
        {
            mPeptideSequence = new PeptideSequence { ElementMode = PeptideSequence.ElementModeConstants.IsotopicMass };
            InitializeCleavageRules();
            InitializepICalculator();
        }

        // Note: Good list of enzymes is at https://web.expasy.org/peptide_cutter/peptidecutter_enzymes.html
        // and https://web.expasy.org/peptide_mass/peptide-mass-doc.html

        public enum CleavageRuleConstants
        {
            NoRule = 0,
            ConventionalTrypsin = 1,
            TrypsinWithoutProlineException = 2,
            EricPartialTrypsin = 3,
            // ReSharper disable once IdentifierTypo
            TrypsinPlusFVLEY = 4,
            KROneEnd = 5,
            TerminiiOnly = 6,
            Chymotrypsin = 7,
            ChymotrypsinAndTrypsin = 8,
            GluC = 9,
            CyanBr = 10,                     // Aka CNBr
            LysC = 11,
            GluC_EOnly = 12,
            ArgC = 13,
            AspN = 14,
            // ReSharper disable once IdentifierTypo
            ProteinaseK = 15,
            PepsinA = 16,
            PepsinB = 17,
            PepsinC = 18,
            PepsinD = 19,
            AceticAcidD = 20,
            TrypsinPlusLysC = 21,
            Thermolysin = 22,
            TrypsinPlusThermolysin = 23
        }

        /// <summary>
        /// Fragment mass range mode constants
        /// </summary>
        public enum FragmentMassConstants
        {
            Monoisotopic = 0,
            MH = 1
        }

        private readonly Dictionary<CleavageRuleConstants, CleavageRule> mCleavageRules = new();

        /// <summary>
        /// General purpose object for computing mass and calling cleavage and digestion functions
        /// </summary>
        private PeptideSequence mPeptideSequence;

        private ComputePeptideProperties mpICalculator;

        public event ErrorEventEventHandler ErrorEvent;

        public event ProgressResetEventHandler ProgressReset;

        public event ProgressChangedEventHandler ProgressChanged;

        private string mProgressStepDescription;

        /// <summary>
        /// Percent complete, ranges from 0 to 100, but can contain decimal percentage values
        /// </summary>
        private float mProgressPercentComplete;

        public int CleavageRuleCount => mCleavageRules.Count;

        public IReadOnlyDictionary<CleavageRuleConstants, CleavageRule> CleavageRules => mCleavageRules;

        public PeptideSequence.ElementModeConstants ElementMassMode
        {
            get
            {
                if (mPeptideSequence == null)
                {
                    return PeptideSequence.ElementModeConstants.IsotopicMass;
                }

                return mPeptideSequence.ElementMode;
            }
            set
            {
                if (mPeptideSequence == null)
                {
                    mPeptideSequence = new PeptideSequence();
                }

                mPeptideSequence.ElementMode = value;
            }
        }

        public string ProgressStepDescription => mProgressStepDescription;

        /// <summary>
        /// Percent complete, value between 0 and 100, but can contain decimal percentage values
        /// </summary>
        public float ProgressPercentComplete => (float)Math.Round(mProgressPercentComplete, 2);

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

                var ruleMatch = mPeptideSequence.CheckSequenceAgainstCleavageRule(sequence, cleavageRule, out ruleMatchCount);
                return ruleMatch;
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
                    mPeptideSequence.SetSequence(sequence.ToUpper().Replace("X", ""));
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

        public int DigestSequence(string proteinSequence,
                                  out List<PeptideSequenceWithNET> peptideFragments,
                                  DigestionOptions digestionOptions,
                                  bool filterByIsoelectricPoint)
        {
            return DigestSequence(proteinSequence, out peptideFragments, digestionOptions, filterByIsoelectricPoint, "");
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
        /// <param name="digestionOptions"></param>
        /// <param name="filterByIsoelectricPoint"></param>
        /// <param name="proteinName"></param>
        /// <returns>The number of peptides in peptideFragments</returns>
        public int DigestSequence(string proteinSequence,
                                  out List<PeptideSequenceWithNET> peptideFragments,
                                  DigestionOptions digestionOptions,
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
                var success = GetCleavageRuleById(digestionOptions.CleavageRuleID, out var cleavageRule);
                if (!success)
                {
                    ReportError("DigestSequence", new Exception("Invalid cleavage rule: " + (int)digestionOptions.CleavageRuleID));
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

                if (digestionOptions.FragmentMassMode == FragmentMassConstants.MH)
                {
                    // Adjust the thresholds down by the charge carrier mass (which is easier than computing the M+H mass of every peptide)
                    minFragmentMass = digestionOptions.MinFragmentMass - PeptideSequence.ChargeCarrierMass;
                    maxFragmentMass = digestionOptions.MaxFragmentMass - PeptideSequence.ChargeCarrierMass;
                }
                else
                {
                    minFragmentMass = digestionOptions.MinFragmentMass;
                    maxFragmentMass = digestionOptions.MaxFragmentMass;
                }

                for (var trypticIndex = 0; trypticIndex < trypticFragCache.Count; trypticIndex++)
                {
                    var peptideSequenceBase = string.Empty;
                    var peptideSequence = string.Empty;
                    var residueStartLoc = trypticFragCache[trypticIndex].StartLoc;

                    for (var index = 0; index <= digestionOptions.MaxMissedCleavages; index++)
                    {
                        if (trypticIndex + index >= trypticFragCache.Count)
                        {
                            break;
                        }

                        int residueEndLoc;
                        if (digestionOptions.CleavageRuleID == CleavageRuleConstants.KROneEnd)
                        {
                            // Partially tryptic cleavage rule: Add all partially tryptic fragments
                            int residueLengthStart;
                            if (index == 0)
                            {
                                residueLengthStart = digestionOptions.MinFragmentResidueCount;
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

                                if (peptideSequence.Length >= digestionOptions.MinFragmentResidueCount)
                                {
                                    PossiblyAddPeptide(peptideSequence, trypticIndex, index,
                                                       residueStartLoc, residueEndLoc,
                                                       ref proteinSequence, proteinSequenceLength,
                                                       fragmentsUniqueList, peptideFragments,
                                                       digestionOptions, filterByIsoelectricPoint,
                                                       minFragmentMass, maxFragmentMass);
                                }
                            }
                        }
                        else
                        {
                            // Normal cleavage rule
                            residueEndLoc = trypticFragCache[trypticIndex + index].EndLoc;
                            peptideSequence += trypticFragCache[trypticIndex + index].Sequence;
                            if (peptideSequence.Length >= digestionOptions.MinFragmentResidueCount)
                            {
                                PossiblyAddPeptide(peptideSequence, trypticIndex, index,
                                                   residueStartLoc, residueEndLoc,
                                                   ref proteinSequence, proteinSequenceLength,
                                                   fragmentsUniqueList, peptideFragments,
                                                   digestionOptions, filterByIsoelectricPoint,
                                                   minFragmentMass, maxFragmentMass);
                            }
                        }

                        peptideSequenceBase += trypticFragCache[trypticIndex + index].Sequence;
                    }

                    if (digestionOptions.CleavageRuleID == CleavageRuleConstants.KROneEnd)
                    {
                        UpdateProgress((float)(trypticIndex / (double)(trypticFragCache.Count * 2) * 100.0d));
                    }
                }

                if (digestionOptions.CleavageRuleID == CleavageRuleConstants.KROneEnd)
                {
                    // Partially tryptic cleavage rule: Add all partially tryptic fragments, working from the end toward the front
                    for (var trypticIndex = trypticFragCache.Count - 1; trypticIndex >= 0; --trypticIndex)
                    {
                        var peptideSequenceBase = string.Empty;

                        var residueEndLoc = trypticFragCache[trypticIndex].EndLoc;

                        for (var index = 0; index <= digestionOptions.MaxMissedCleavages; index++)
                        {
                            if (trypticIndex - index < 0)
                            {
                                break;
                            }

                            int residueLengthStart;

                            if (index == 0)
                            {
                                residueLengthStart = digestionOptions.MinFragmentResidueCount;
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

                                if (peptideSequence.Length >= digestionOptions.MinFragmentResidueCount)
                                {
                                    PossiblyAddPeptide(peptideSequence, trypticIndex, index,
                                    residueStartLoc, residueEndLoc,
                                    ref proteinSequence, proteinSequenceLength,
                                    fragmentsUniqueList, peptideFragments,
                                    digestionOptions, filterByIsoelectricPoint,
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

        public void InitializepICalculator()
        {
            InitializepICalculator(new ComputePeptideProperties());
        }

        public void InitializepICalculator(ComputePeptideProperties pICalculator)
        {
            if (mpICalculator != null)
            {
                if (ReferenceEquals(mpICalculator, pICalculator))
                {
                    // Classes are the same instance of the object; no need to update anything
                    return;
                }

                mpICalculator = null;
            }

            mpICalculator = pICalculator;
        }

        public void InitializepICalculator(
            ComputePeptideProperties.HydrophobicityTypeConstants hydrophobicityType,
            bool reportMaximumpI,
            int sequenceWidthToExamineForMaximumpI)
        {
            if (mpICalculator == null)
            {
                mpICalculator = new ComputePeptideProperties();
            }

            mpICalculator.HydrophobicityType = hydrophobicityType;
            mpICalculator.ReportMaximumpI = reportMaximumpI;
            mpICalculator.SequenceWidthToExamineForMaximumpI = sequenceWidthToExamineForMaximumpI;
        }

        private void PossiblyAddPeptide(
            string peptideSequence,
            int trypticIndex,
            int missedCleavageCount,
            int residueStartLoc,
            int residueEndLoc,
            ref string proteinSequence,
            int proteinSequenceLength,
            ISet<string> fragmentsUniqueList,
            ICollection<PeptideSequenceWithNET> peptideFragments,
            DigestionOptions digestionOptions,
            bool filterByIsoelectricPoint,
            double minFragmentMass,
            double maxFragmentMass)
        {
            // Note: proteinSequence is passed ByRef for speed purposes since passing a reference of a large string is easier than passing it ByVal
            // It is not modified by this function

            var addFragment = true;
            if (digestionOptions.RemoveDuplicateSequences)
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

            if (addFragment && digestionOptions.AminoAcidResidueFilterChars.Length > 0)
            {
                if (peptideSequence.IndexOfAny(digestionOptions.AminoAcidResidueFilterChars) < 0)
                {
                    addFragment = false;
                }
            }

            if (!addFragment)
            {
                return;
            }

            var peptideFragment = new PeptideSequenceWithNET
            {
                AutoComputeNET = false,
                CysTreatmentMode = digestionOptions.CysTreatmentMode,
                SequenceOneLetter = peptideSequence
            };

            if (peptideFragment.Mass < minFragmentMass ||
                peptideFragment.Mass > maxFragmentMass)
            {
                return;
            }

            var isoelectricPoint = default(float);

            // Possibly compute the isoelectric point for the peptide
            if (filterByIsoelectricPoint)
            {
                isoelectricPoint = mpICalculator.CalculateSequencepI(peptideSequence);
            }

            if (filterByIsoelectricPoint &&
                (isoelectricPoint < digestionOptions.MinIsoelectricPoint ||
                 isoelectricPoint > digestionOptions.MaxIsoelectricPoint))
            {
                return;
            }

            // We can now compute the NET value for the peptide
            peptideFragment.UpdateNET();

            if (digestionOptions.IncludePrefixAndSuffixResidues)
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

            if (digestionOptions.CleavageRuleID is CleavageRuleConstants.ConventionalTrypsin or CleavageRuleConstants.TrypsinWithoutProlineException)
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

        private void ResetProgress()
        {
            ProgressReset?.Invoke();
        }

        private void ResetProgress(string description)
        {
            UpdateProgress(description, 0f);
            ProgressReset?.Invoke();
        }

        private void UpdateProgress(string description)
        {
            UpdateProgress(description, mProgressPercentComplete);
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
                if (NETPredictor == null)
                {
                    NETPredictor = new NETPrediction.ElutionTimePredictionKangas();
                }

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
            public override int SetSequence(
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

        public class DigestionOptions
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public DigestionOptions()
            {
                mMaxMissedCleavages = 0;
                CleavageRuleID = CleavageRuleConstants.ConventionalTrypsin;
                mMinFragmentResidueCount = 4;

                CysTreatmentMode = PeptideSequence.CysTreatmentModeConstants.Untreated;

                FragmentMassMode = FragmentMassConstants.Monoisotopic;

                mMinFragmentMass = 0;
                mMaxFragmentMass = 6000;

                MinIsoelectricPoint = 0f;
                MaxIsoelectricPoint = 100f;

                RemoveDuplicateSequences = false;
                IncludePrefixAndSuffixResidues = false;
                AminoAcidResidueFilterChars = Array.Empty<char>();
            }

            private int mMaxMissedCleavages;
            private int mMinFragmentResidueCount;
            private int mMinFragmentMass;
            private int mMaxFragmentMass;

            public char[] AminoAcidResidueFilterChars { get; set; }

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

            public CleavageRuleConstants CleavageRuleID { get; set; }

            public PeptideSequence.CysTreatmentModeConstants CysTreatmentMode { get; set; }

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

            public float MinIsoelectricPoint { get; set; }

            public float MaxIsoelectricPoint { get; set; }

            public bool RemoveDuplicateSequences { get; set; }
            public bool IncludePrefixAndSuffixResidues { get; set; }

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
}
