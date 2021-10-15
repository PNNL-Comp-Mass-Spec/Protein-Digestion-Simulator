using System;
using System.Collections.Generic;

namespace ProteinDigestionSimulator
{
    /// <summary>
    /// This class can be used to calculate the mass of an amino acid sequence (peptide or protein)
    /// The protein must be in one letter abbreviation format
    /// </summary>
    public class PeptideSequence
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PeptideSequence()
        {
            if (!mSharedArraysInitialized)
            {
                mCurrentElementMode = ElementModeConstants.IsotopicMass;
                InitializeSharedData();
                mSharedArraysInitialized = true;
            }

            mResidues = string.Empty;
        }

        private const char UNKNOWN_SYMBOL_ONE_LETTER = 'X';
        private const string UNKNOWN_SYMBOL_THREE_LETTERS = "Xxx";

        private const char TERMINII_SYMBOL = '-';
        private const string TRYPTIC_RULE_RESIDUES = "KR";
        private const string TRYPTIC_EXCEPTION_RESIDUES = "P";

        public enum CTerminusGroupConstants
        {
            Hydroxyl = 0,
            Amide = 1,
            None = 2
        }

        public enum NTerminusGroupConstants
        {
            Hydrogen = 0,
            HydrogenPlusProton = 1,
            Acetyl = 2,
            PyroGlu = 3,
            Carbamyl = 4,
            PTC = 5,
            None = 6
        }

        /// <summary>
        /// Cysteine treatment constants
        /// </summary>
        public enum CysTreatmentModeConstants
        {
            Untreated = 0,
            Iodoacetamide = 1,       // +57.0215 (alkylated)
            IodoaceticAcid = 2      // +58.0055
        }

        public enum ElementModeConstants
        {
            AverageMass = 0,
            IsotopicMass = 1
        }

        private class TerminusInfo
        {
            public string Formula { get; set; }
            public double Mass { get; set; }
            public ElementModeConstants MassElementMode { get; set; }

            /// <summary>
            /// If the peptide sequence is part of a protein, the user can record the final residue of the previous peptide sequence here
            /// </summary>
            // ReSharper disable once NotAccessedField.Local
            public string PrecedingResidue { get; set; }

            /// <summary>
            /// If the peptide sequence is part of a protein, the user can record the first residue of the next peptide sequence here
            /// </summary>
            // ReSharper disable once NotAccessedField.Local
            public string FollowingResidue { get; set; }
        }

        // Variables shared across all instances of this class

        private static bool mSharedArraysInitialized;

        /// <summary>
        /// One letter symbols and corresponding mass for each
        /// </summary>
        private static Dictionary<char, double> AminoAcidMasses;

        /// <summary>
        /// One letter symbols and corresponding three letter symbol for each
        /// </summary>
        private static Dictionary<char, string> AminoAcidSymbols;

        /// <summary>
        /// One letter element symbols and corresponding mass for each
        /// </summary>
        private static Dictionary<char, double> ElementMasses;
        private static ElementModeConstants mCurrentElementMode;

        /// <summary>
        /// Mass of hydrogen
        /// </summary>
        private static double mHydrogenMass;

        /// <summary>
        /// Mass value to add for each Cys residue when CysTreatmentMode is Iodoacetamide
        /// </summary>
        /// <remarks>Auto-updated when mCurrentElementMode is updated</remarks>
        private static double mIodoacetamideMass;

        /// <summary>
        /// Mass value to add for each Cys residue when CysTreatmentMode is IodoaceticAcid
        /// </summary>
        /// <remarks>Auto-updated when mCurrentElementMode is updated</remarks>
        private static double mIodoaceticAcidMass;

        private static CleavageRule mTrypticCleavageRule;

        private string mResidues;

        /// <summary>
        /// Formula on the N-Terminus
        /// </summary>
        private readonly TerminusInfo mNTerminus = new TerminusInfo();

        /// <summary>
        /// Formula on the C-Terminus
        /// </summary>
        private readonly TerminusInfo mCTerminus = new TerminusInfo();

        private double mTotalMass;
        private ElementModeConstants mTotalMassElementMode;

        private bool mDelayUpdateResidueMass;

        /// <summary>
        /// Charge carrier mass: hydrogen minus one electron
        /// </summary>
        public static double ChargeCarrierMass { get; private set; }

        /// <summary>
        /// Cysteine treatment mode
        /// </summary>
        public CysTreatmentModeConstants CysTreatmentMode { get; set; }

        /// <summary>
        /// Element mode
        /// </summary>
        public ElementModeConstants ElementMode
        {
            get => mCurrentElementMode;
            set
            {
                mCurrentElementMode = value;
                InitializeSharedData();
                UpdateSequenceMass();
            }
        }

        /// <summary>
        /// Sequence mass
        /// </summary>
        public double Mass
        {
            get
            {
                if (mTotalMassElementMode != mCurrentElementMode)
                {
                    UpdateSequenceMass();
                }

                return mTotalMass;
            }
        }

        /// <summary>
        /// Adds new entry to AminoAcidMasses and AminoAcidSymbols
        /// </summary>
        /// <param name="symbolOneLetter"></param>
        /// <param name="symbolThreeLetter"></param>
        /// <param name="formula">
        /// Can only contain C, H, N, O, S, or P, with each element optionally having an integer after it
        /// Cannot contain any parentheses or other advanced formula features
        /// </param>
        private void AddAminoAcidStatEntry(char symbolOneLetter, string symbolThreeLetter, string formula)
        {
            if (AminoAcidMasses.ContainsKey(symbolOneLetter))
            {
                AminoAcidMasses.Remove(symbolOneLetter);
            }

            var aminoAcidMass = ComputeFormulaWeightCHNOSP(formula);
            AminoAcidMasses.Add(symbolOneLetter, aminoAcidMass);

            // Uncomment this to create a file named "AminoAcidMasses.txt" containing the amino acid symbols and masses
            // var writer = new System.IO.StreamWriter("AminoAcidMasses.txt", true);
            // writer.WriteLine(symbolOneLetter + "\t" + mass.ToString("0.000000"));
            // writer.Close();

            AminoAcidSymbols.Add(symbolOneLetter, symbolThreeLetter);
        }

        /// <summary>
        /// Remove prefix and suffix residues
        /// </summary>
        /// <remarks>This function is only applicable for sequences in one-letter notation</remarks>
        /// <param name="sequence">Sequence to examine</param>
        /// <param name="prefix">Output: prefix residue</param>
        /// <param name="suffix">Output: suffix residue</param>
        private string CheckForAndRemovePrefixAndSuffixResidues(
            string sequence,
            out string prefix,
            out string suffix)
        {
            prefix = string.Empty;
            suffix = string.Empty;

            // First look if sequence is in the form A.BCDEFG.Z or -.BCDEFG.Z or A.BCDEFG.-
            // If so, then need to strip out the preceding A and Z residues since they aren't really part of the sequence
            if (sequence.Length > 1 && sequence.IndexOf(".", StringComparison.Ordinal) >= 0)
            {
                if (sequence[1].ToString() == "." && sequence.Length > 2)
                {
                    prefix = sequence[0].ToString();
                    sequence = sequence.Substring(2);
                }

                if (sequence.Length > 2 && sequence[sequence.Length - 2].ToString() == ".")
                {
                    prefix = sequence[sequence.Length - 1].ToString();
                    sequence = sequence.Substring(0, sequence.Length - 2);
                }

                // Also check for starting with a . or ending with a .
                if (sequence.Length > 0 && sequence[0].ToString() == ".")
                {
                    sequence = sequence.Substring(1);
                }

                if (sequence.Length > 0 && sequence[sequence.Length - 1].ToString() == ".")
                {
                    sequence = sequence.Substring(0, sequence.Length - 1);
                }
            }

            return sequence;
        }

        /// <summary>
        /// Checks a sequence to see if it matches the cleavage rule
        /// Both the prefix residue and the residue at the end of the sequence are tested against ruleResidues and exceptionResidues
        /// </summary>
        /// <remarks>Returns True if sequence doesn't contain any periods, and thus, can't be examined</remarks>
        /// <param name="sequence"></param>
        /// <param name="cleavageRule"></param>
        /// <param name="ruleMatchCount">Output: the number of ends that matched the rule (0, 1, or 2); terminii are counted as rule matches</param>
        /// <param name="separationChar"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase">When true, will capitalize all letters in sequence; if the calling method has already capitalized them, this can be set to False for a slight speed advantage</param>
        /// <returns>True if a valid match, False if not a match</returns>
        public bool CheckSequenceAgainstCleavageRule(
            string sequence,
            CleavageRule cleavageRule,
            out int ruleMatchCount,
            string separationChar = ".",
            char terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true)
        {
            // ReSharper disable CommentTypo

            // For example, if ruleResidues = "KR", exceptionResidues = "P", and reversedCleavageDirection = False
            // Then if sequence = "R.AEQDDLANYGPGNGVLPSAGSSISMEK.L" then matchesCleavageRule = True
            // However, if sequence = "R.IGASGEHIFIIGVDK.P" then matchesCleavageRule = False since suffix = "P"
            // Finally, if sequence = "R.IGASGEHIFIIGVDKPNR.Q" then matchesCleavageRule = True since K is ignored, but the final R.Q is valid

            // ReSharper restore CommentTypo

            // Need to reset this to zero since passed ByRef
            ruleMatchCount = 0;
            var terminusCount = 0;

            // First, make sure the sequence is in the form A.BCDEFG.H or A.BCDEFG or BCDEFG.H
            // If it isn't, then we can't check it (we'll return true)

            if (string.IsNullOrEmpty(cleavageRule.CleavageResidues))
            {
                // No rules
                return true;
            }

            var periodIndex1 = sequence.IndexOf(separationChar, StringComparison.Ordinal);

            if (periodIndex1 < 0)
            {
                // No periods, can't check
                Console.WriteLine("CheckSequenceAgainstCleavageRule called with a sequence that doesn't contain prefix or suffix separation characters; unable to process: " + sequence);
                return true;
            }

            var periodIndex2 = sequence.IndexOf(separationChar, periodIndex1 + 1, StringComparison.Ordinal);

            if (ignoreCase)
            {
                sequence = sequence.ToUpper();
            }

            char prefix;
            var suffix = default(char);
            char sequenceStart;
            var sequenceEnd = default(char);

            // Find the prefix residue and starting residue
            //
            if (periodIndex1 >= 1)
            {
                prefix = sequence[periodIndex1 - 1];
                sequenceStart = sequence[periodIndex1 + 1];
            }
            else
            {
                // periodIndex1 must be 0; assume we're at the protein terminus
                prefix = terminiiSymbol;
                sequenceStart = sequence[periodIndex1 + 1];
            }

            if (periodIndex2 > periodIndex1 + 1 && periodIndex2 <= sequence.Length - 2)
            {
                suffix = sequence[periodIndex2 - 1];
                sequenceStart = sequence[periodIndex2 + 1];
            }
            else if (periodIndex1 > 1)
            {
                sequenceEnd = sequence[sequence.Length - 1];
            }

            if (cleavageRule.CleavageResidues == terminiiSymbol.ToString())
            {
                // Peptide database rules
                // See if prefix and suffix are both "" or are both terminiiSymbol
                if (prefix == terminiiSymbol && suffix == terminiiSymbol ||
                    prefix == default(char) && suffix == default(char))
                {
                    ruleMatchCount = 2;
                    // Count this as a match to the cleavage rule
                    return true;
                }

                return false;
            }

            // Test both prefix and sequenceEnd against ruleResidues
            // Make sure suffix does not match exceptionResidues
            for (var endToCheck = 0; endToCheck <= 1; endToCheck++)
            {
                var skipThisEnd = false;
                var testResidue = default(char);
                var exceptionResidue = default(char);

                if (endToCheck == 0)
                {
                    // N terminus
                    if (prefix == terminiiSymbol)
                    {
                        terminusCount++;
                        skipThisEnd = true;
                    }
                    else if (cleavageRule.ReversedCleavageDirection)
                    {
                        testResidue = sequenceStart;
                        exceptionResidue = prefix;
                    }
                    else
                    {
                        testResidue = prefix;
                        exceptionResidue = sequenceStart;
                    }
                }
                // C terminus
                else if (suffix == terminiiSymbol)
                {
                    terminusCount++;
                    skipThisEnd = true;
                }
                else if (cleavageRule.ReversedCleavageDirection)
                {
                    testResidue = suffix;
                    exceptionResidue = sequenceEnd;
                }
                else
                {
                    testResidue = sequenceEnd;
                    exceptionResidue = suffix;
                }

                if (skipThisEnd)
                {
                    continue;
                }

                var ruleMatch = ResiduesMatchCleavageRule(testResidue, exceptionResidue, cleavageRule);

                if (ruleMatch)
                {
                    ruleMatchCount++;
                }
                else
                {
                    foreach (var additionalRule in cleavageRule.AdditionalCleavageRules)
                    {
                        var altRuleMatch = ResiduesMatchCleavageRule(testResidue, exceptionResidue, additionalRule);
                        if (altRuleMatch)
                        {
                            ruleMatchCount++;
                            break;
                        }
                    }
                }
            }

            if (terminusCount == 2)
            {
                // Both ends of the peptide are terminii; label as fully matching the rules and set ruleMatchCount to 2
                ruleMatchCount = 2;
                return true;
            }

            if (terminusCount == 1)
            {
                // One end was a terminus; can either be fully cleaved or non-cleaved; never partially cleaved
                if (ruleMatchCount >= 1)
                {
                    ruleMatchCount = 2;
                    return true;
                }

                return false;
            }

            if (ruleMatchCount == 2)
            {
                return true;
            }

            return ruleMatchCount >= 1 && cleavageRule.AllowPartialCleavage;
        }

        private bool CheckSequenceAgainstCleavageRuleMatchTestResidue(char testResidue, string ruleResidues)
        {
            // Checks to see if testResidue matches one of the residues in ruleResidues
            // Used to test by Rule Residues and Exception Residues

            return ruleResidues.IndexOf(testResidue) >= 0;
        }

        /// <summary>
        /// Very simple mass computation utility; only considers elements C, H, N, O, S, and P
        /// </summary>
        /// <param name="formula">
        /// Can only contain C, H, N, O, S, or P, with each element optionally having an integer after it
        /// Cannot contain any parentheses or other advanced formula features
        /// </param>
        /// <returns>Formula mass, or 0 if any unknown symbols are encountered</returns>
        private double ComputeFormulaWeightCHNOSP(string formula)
        {
            var formulaMass = 0d;
            var lastElementIndex = -1;
            var multiplier = string.Empty;

            for (var index = 0; index < formula.Length; index++)
            {
                if (char.IsNumber(formula[index]))
                {
                    multiplier += formula[index].ToString();
                }
                else
                {
                    if (lastElementIndex >= 0)
                    {
                        formulaMass += ComputeFormulaWeightLookupMass(formula[lastElementIndex], multiplier);
                        multiplier = string.Empty;
                    }

                    lastElementIndex = index;
                }
            }

            // Handle the final element
            if (lastElementIndex >= 0)
            {
                formulaMass += ComputeFormulaWeightLookupMass(formula[lastElementIndex], multiplier);
            }

            return formulaMass;
        }

        private double ComputeFormulaWeightLookupMass(char symbol, string multiplier)
        {
            var multiplierVal = PRISM.DataUtils.StringToValueUtils.CIntSafe(multiplier, 1);

            try
            {
                return ElementMasses[symbol] * multiplierVal;
            }
            catch
            {
                // Symbol not found, or has invalid mass
                return 0d;
            }
        }

        /// <summary>
        /// Converts a sequence from 1 letter to 3 letter codes or vice versa
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="oneLetterTo3Letter"></param>
        /// <param name="addSpaceEvery10Residues"></param>
        /// <param name="separateResiduesWithDash"></param>
        public string ConvertAminoAcidSequenceSymbols(
            string sequence,
            bool oneLetterTo3Letter,
            bool addSpaceEvery10Residues = false,
            bool separateResiduesWithDash = false)
        {
            var peptide = new PeptideSequence();
            var prefix = string.Empty;
            var suffix = string.Empty;

            try
            {
                if (oneLetterTo3Letter)
                {
                    sequence = CheckForAndRemovePrefixAndSuffixResidues(sequence, out prefix, out suffix);
                }

                peptide.SetSequence(sequence, NTerminusGroupConstants.None, CTerminusGroupConstants.None, !oneLetterTo3Letter);

                var sequenceOut = peptide.GetSequence(oneLetterTo3Letter, addSpaceEvery10Residues, separateResiduesWithDash);

                if (oneLetterTo3Letter && prefix.Length > 0 || suffix.Length > 0)
                {
                    peptide.SetSequence(prefix, NTerminusGroupConstants.None, CTerminusGroupConstants.None);
                    sequenceOut = peptide.GetSequence(true) + "." + sequenceOut;

                    peptide.SetSequence(suffix, NTerminusGroupConstants.None, CTerminusGroupConstants.None);
                    sequenceOut += "." + peptide.GetSequence(true);
                }

                return sequenceOut;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert between 1 letter and 3 letter symbol
        /// </summary>
        /// <param name="symbolToParse">Amino acid symbol to parse</param>
        /// <param name="oneLetterTo3Letter">
        /// When true, converting 1 letter codes to 3 letter codes
        /// Otherwise, converting 3 letter codes to 1 letter codes
        /// </param>
        /// <returns>Converted amino acid symbol if a valid amino acid symbol, otherwise an empty string</returns>
        private string GetAminoAcidSymbolConversion(string symbolToParse, bool oneLetterTo3Letter)
        {
            if (oneLetterTo3Letter)
            {
                // 1 letter to 3 letter
                var symbol = symbolToParse.Substring(0, 1).ToUpper()[0];
                if (AminoAcidSymbols.ContainsKey(symbol))
                {
                    return AminoAcidSymbols[symbol];
                }

                return string.Empty;
            }

            // 3 letter to 1 letter
            symbolToParse = symbolToParse.ToUpper();
            foreach (var entry in AminoAcidSymbols)
            {
                if (entry.Value.ToUpper() == symbolToParse)
                {
                    return entry.Key.ToString();
                }
            }

            // If we get here, the value wasn't found
            return string.Empty;
        }

        /// <summary>
        /// Returns the given residue in the current sequence
        /// </summary>
        /// <param name="residueNumber"></param>
        /// <param name="use3LetterCode"></param>
        public string GetResidue(int residueNumber, bool use3LetterCode = false)
        {
            if (mResidues != null && residueNumber > 0 && residueNumber <= mResidues.Length)
            {
                if (use3LetterCode)
                {
                    return GetAminoAcidSymbolConversion(mResidues[residueNumber - 1].ToString(), true);
                }

                return mResidues[residueNumber - 1].ToString();
            }

            return string.Empty;
        }

        public int GetResidueCount()
        {
            if (mResidues == null)
            {
                return 0;
            }

            return mResidues.Length;
        }

        /// <summary>
        /// Returns the number of occurrences of the given residue in the loaded sequence, or -1 if an error
        /// </summary>
        /// <param name="residueSymbol"></param>
        /// <param name="use3LetterCode"></param>
        public int GetResidueCountSpecificResidue(string residueSymbol, bool use3LetterCode = false)
        {
            char searchResidue1Letter;

            if (string.IsNullOrEmpty(mResidues))
            {
                return 0;
            }

            try
            {
                if (use3LetterCode)
                {
                    searchResidue1Letter = GetAminoAcidSymbolConversion(residueSymbol, false)[0];
                }
                else
                {
                    searchResidue1Letter = residueSymbol[0];
                }
            }
            catch
            {
                return -1;
            }

            var residueIndex = -1;
            var residueCount = 0;
            while (true)
            {
                residueIndex = mResidues.IndexOf(searchResidue1Letter, residueIndex + 1);
                if (residueIndex >= 0)
                {
                    residueCount++;
                }
                else
                {
                    break;
                }
            }

            return residueCount;
        }

        /// <summary>
        /// Construct the peptide sequence using Residues[] and the N and C Terminus info
        /// </summary>
        /// <param name="use3LetterCode"></param>
        /// <param name="addSpaceEvery10Residues"></param>
        /// <param name="separateResiduesWithDash"></param>
        /// <param name="includeNAndCTerminii"></param>
        public string GetSequence(
            bool use3LetterCode = false,
            bool addSpaceEvery10Residues = false,
            bool separateResiduesWithDash = false,
            bool includeNAndCTerminii = false)
        {
            string sequence;
            var dashAdd = string.Empty;

            if (mResidues == null)
            {
                return string.Empty;
            }

            if (!use3LetterCode && !addSpaceEvery10Residues && !separateResiduesWithDash)
            {
                // Simply return the sequence, possibly with the N and C terminii
                sequence = mResidues;
            }
            else
            {
                if (separateResiduesWithDash)
                {
                    dashAdd = "-";
                }
                else
                {
                    dashAdd = string.Empty;
                }

                sequence = string.Empty;
                var lastIndex = mResidues.Length - 1;
                for (var index = 0; index <= lastIndex; index++)
                {
                    if (use3LetterCode)
                    {
                        var symbol3Letter = GetAminoAcidSymbolConversion(mResidues[index].ToString(), true);

                        if (string.IsNullOrWhiteSpace(symbol3Letter))
                        {
                            symbol3Letter = UNKNOWN_SYMBOL_THREE_LETTERS;
                        }

                        sequence += symbol3Letter;
                    }
                    else
                    {
                        sequence += mResidues[index].ToString();
                    }

                    if (index < lastIndex)
                    {
                        if (addSpaceEvery10Residues)
                        {
                            if ((index + 1) % 10 == 0)
                            {
                                sequence += " ";
                            }
                            else
                            {
                                sequence += dashAdd;
                            }
                        }
                        else
                        {
                            sequence += dashAdd;
                        }
                    }
                }
            }

            if (includeNAndCTerminii)
            {
                sequence = mNTerminus.Formula + dashAdd + sequence + dashAdd + mCTerminus.Formula;
            }

            return sequence;
        }

        public double GetSequenceMass()
        {
            return Mass;
        }

        /// <summary>
        /// Get the tryptic name for the peptide, within the context of the protein residues
        /// </summary>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="cleavageRule"></param>
        /// <param name="returnResidueStart">Output: residue in the protein where the peptide starts</param>
        /// <param name="returnResidueEnd">Output: residue in the protein where the peptide ends</param>
        /// <param name="icr2LSCompatible"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="proteinSearchStartLoc"></param>
        public string GetTrypticName(
            string proteinResidues,
            string peptideResidues,
            CleavageRule cleavageRule,
            out int returnResidueStart,
            out int returnResidueEnd,
            bool icr2LSCompatible = false,
            char terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 1)
        {
            // Examines peptideResidues to see where they exist in proteinResidues
            // Constructs a name string based on their position and based on whether the fragment is truly tryptic
            // In addition, returns the position of the first and last residue in returnResidueStart and returnResidueEnd
            // The tryptic name in the following format
            // t1  indicates tryptic peptide 1
            // t2 represents tryptic peptide 2, etc.
            // t1.2  indicates tryptic peptide 1, plus one more tryptic peptide, i.e. t1 and t2
            // t5.2  indicates tryptic peptide 5, plus one more tryptic peptide, i.e. t5 and t6
            // t5.3  indicates tryptic peptide 5, plus two more tryptic peptides, i.e. t5, t6, and t7
            // 40.52  means that the residues are not tryptic, and simply range from residue 40 to 52
            // If the peptide residues are not present in proteinResidues, returns ""
            // Since a peptide can occur multiple times in a protein, one can set proteinSearchStartLoc to a value larger than 1 to ignore previous hits

            // If icr2LSCompatible is True, then the values returned when a peptide is not tryptic are modified to
            // range from the starting residue, to the ending residue +1
            // returnResidueEnd is always equal to the position of the final residue, regardless of icr2LSCompatible

            // ReSharper disable CommentTypo
            // For example, if proteinResidues = "IGKANR"
            // Then when peptideResidues = "IGK", the TrypticName is t1
            // Then when peptideResidues = "ANR", the TrypticName is t2
            // Then when peptideResidues = "IGKANR", the TrypticName is t1.2
            // Then when peptideResidues = "IG", the TrypticName is 1.2
            // Then when peptideResidues = "KANR", the TrypticName is 3.6
            // Then when peptideResidues = "NR", the TrypticName is 5.6

            // However, if icr2LSCompatible = True, then the last three are changed to:
            // Then when peptideResidues = "IG", the TrypticName is 1.3
            // Then when peptideResidues = "KANR", the TrypticName is 3.7
            // Then when peptideResidues = "NR", the TrypticName is 5.7

            // ReSharper restore CommentTypo

            if (ignoreCase)
            {
                proteinResidues = proteinResidues.ToUpper();
                peptideResidues = peptideResidues.ToUpper();
            }

            // startLoc and endLoc track residue numbers, ranging from 1 to length of the protein
            int startLoc;

            if (proteinSearchStartLoc <= 1)
            {
                startLoc = proteinResidues.IndexOf(peptideResidues, StringComparison.Ordinal) + 1;
            }
            else
            {
                startLoc = proteinResidues.Substring(proteinSearchStartLoc + 1).IndexOf(peptideResidues, StringComparison.Ordinal) + 1;
                if (startLoc > 0)
                {
                    startLoc = startLoc + proteinSearchStartLoc - 1;
                }
            }

            var peptideResiduesLength = peptideResidues.Length;
            if (startLoc > 0 && proteinResidues.Length > 0 && peptideResiduesLength > 0)
            {
                var endLoc = startLoc + peptideResiduesLength - 1;
                char prefix;
                char suffix;

                // Determine if the residue is tryptic
                // Use CheckSequenceAgainstCleavageRule() for this
                if (startLoc > 1)
                {
                    prefix = proteinResidues[startLoc - 2];
                }
                else
                {
                    prefix = terminiiSymbol;
                }

                if (endLoc == proteinResidues.Length)
                {
                    suffix = terminiiSymbol;
                }
                else
                {
                    suffix = proteinResidues[endLoc];
                }

                // We can set ignoreCase to false when calling CheckSequenceAgainstCleavageRule
                // since proteinResidues and peptideResidues are already uppercase
                var matchesCleavageRule = CheckSequenceAgainstCleavageRule(
                    prefix + "." + peptideResidues + "." + suffix, cleavageRule, out _, ".", terminiiSymbol, false);

                string trypticName;

                if (matchesCleavageRule)
                {
                    // Construct trypticName

                    int trypticResidueNumber;

                    // Determine which tryptic residue peptideResidues is
                    if (startLoc == 1)
                    {
                        trypticResidueNumber = 1;
                    }
                    else
                    {
                        trypticResidueNumber = 0;
                        var proteinResiduesBeforeStartLoc = proteinResidues.Substring(0, startLoc - 1);
                        var residueFollowingSearchResidues = peptideResidues[0];

                        var ruleResidueNumForProtein = 0;
                        do
                        {
                            ruleResidueNumForProtein = GetTrypticNameFindNextCleavageLoc(proteinResiduesBeforeStartLoc, residueFollowingSearchResidues.ToString(), ruleResidueNumForProtein + 1, cleavageRule, terminiiSymbol);
                            if (ruleResidueNumForProtein > 0)
                            {
                                trypticResidueNumber++;
                            }
                        }
                        while (ruleResidueNumForProtein > 0 && ruleResidueNumForProtein + 1 < startLoc);
                        trypticResidueNumber++;
                    }

                    // Determine number of K or R residues in peptideResidues
                    // Ignore K or R residues followed by Proline
                    var ruleResidueMatchCount = 0;
                    var ruleResidueNumForPeptide = 0;
                    do
                    {
                        ruleResidueNumForPeptide = GetTrypticNameFindNextCleavageLoc(peptideResidues, suffix.ToString(), ruleResidueNumForPeptide + 1, cleavageRule, terminiiSymbol);
                        if (ruleResidueNumForPeptide > 0)
                        {
                            ruleResidueMatchCount++;
                        }
                    }
                    while (ruleResidueNumForPeptide > 0 && ruleResidueNumForPeptide < peptideResiduesLength);

                    trypticName = "t" + trypticResidueNumber;
                    if (ruleResidueMatchCount > 1)
                    {
                        trypticName += "." + ruleResidueMatchCount;
                    }
                }
                else if (icr2LSCompatible)
                {
                    trypticName = startLoc + "." + (endLoc + 1);
                }
                else
                {
                    trypticName = startLoc + "." + endLoc;
                }

                returnResidueStart = startLoc;
                returnResidueEnd = endLoc;
                return trypticName;
            }

            // Residues not found
            returnResidueStart = 0;
            returnResidueEnd = 0;
            return string.Empty;
        }

        /// <summary>
        /// Examines peptideResidues to see where they exist in proteinResidues
        /// Looks for all possible matches, returning them as a comma separated list
        /// </summary>
        /// <remarks>See GetTrypticName for additional information</remarks>
        /// <param name="proteinResidues"></param>
        /// <param name="peptideResidues"></param>
        /// <param name="cleavageRule"></param>
        /// <param name="returnMatchCount">Output: number of times peptideResidues is found in proteinResidues</param>
        /// <param name="returnResidueStart">Output: residue in the protein where the peptide starts</param>
        /// <param name="returnResidueEnd">Output: residue in the protein where the peptide ends</param>
        /// <param name="icr2LSCompatible"></param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <param name="proteinSearchStartLoc"></param>
        /// <param name="listDelimiter"></param>
        /// <returns>Comma separated list of tryptic names</returns>
        public string GetTrypticNameMultipleMatches(
            string proteinResidues,
            string peptideResidues,
            CleavageRule cleavageRule,
            out int returnMatchCount,
            out int returnResidueStart,
            out int returnResidueEnd,
            bool icr2LSCompatible = false,
            char terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true,
            int proteinSearchStartLoc = 1,
            string listDelimiter = ", ")
        {
            returnMatchCount = 0;
            returnResidueStart = 0;
            returnResidueEnd = 0;

            var currentSearchLoc = proteinSearchStartLoc;
            var nameList = string.Empty;

            while (true)
            {
                var currentName = GetTrypticName(proteinResidues, peptideResidues, cleavageRule, out var currentResidueStart, out var currentResidueEnd, icr2LSCompatible, terminiiSymbol, ignoreCase, currentSearchLoc);

                if (currentName.Length > 0)
                {
                    if (nameList.Length > 0)
                    {
                        nameList += listDelimiter;
                    }

                    nameList += currentName;
                    currentSearchLoc = currentResidueEnd + 1;
                    returnMatchCount++;

                    if (returnMatchCount == 1)
                    {
                        returnResidueStart = currentResidueStart;
                    }

                    returnResidueEnd = currentResidueEnd;

                    if (currentSearchLoc > proteinResidues.Length)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return nameList;
        }

        /// <summary>
        /// Finds the location of the next cleavage point in searchResidues using the given cleavage rule
        /// </summary>
        /// <remarks>
        /// Assumes searchResidues are already upper case
        /// </remarks>
        /// <param name="searchResidues"></param>
        /// <param name="residueFollowingSearchResidues">Residue following the last residue in searchResidues</param>
        /// <param name="startResidueNum">Starting residue number (value between 1 and length of searchResidues)</param>
        /// <param name="cleavageRule"></param>
        /// <param name="terminiiSymbol"></param>
        /// <returns>Residue number of the next cleavage location (value between 1 and length of searchResidues), or 0 if no match</returns>
        private int GetTrypticNameFindNextCleavageLoc(
            string searchResidues,
            string residueFollowingSearchResidues,
            int startResidueNum,
            CleavageRule cleavageRule,
            char terminiiSymbol = TERMINII_SYMBOL)
        {
            // ReSharper disable CommentTypo

            // For example, if searchResidues = "IGASGEHIFIIGVDKPNR" and residueFollowingSearchResidues = "Q"
            // and the protein it is part of is: TNSANFRIGASGEHIFIIGVDKPNRQPDS
            // and cleavageRule has CleavageResidues = "KR and ExceptionResidues = "P"
            // The K in IGASGEHIFIIGVDKPNR is ignored because the following residue is P,
            // while the R in IGASGEHIFIIGVDKPNR is OK because residueFollowingSearchResidues is Q

            // It is the calling function's responsibility to assign the correct residue to residueFollowingSearchResidues
            // If no match is found, but residueFollowingSearchResidues is "-", the cleavage location returned is Len(searchResidues) + 1

            // ReSharper restore CommentTypo

            var exceptionSuffixResidueCount = cleavageRule.ExceptionResidues.Length;

            var minCleavedResidueNum = -1;
            for (var charIndexInCleavageResidues = 0; charIndexInCleavageResidues < cleavageRule.CleavageResidues.Length; charIndexInCleavageResidues++)
            {
                var matchFound = FindNextCleavageResidue(cleavageRule, charIndexInCleavageResidues, searchResidues, startResidueNum, out var cleavedResidueNum);

                if (matchFound)
                {
                    if (exceptionSuffixResidueCount > 0)
                    {
                        var matchFoundToExceptionResidue = IsMatchToExceptionResidue(cleavageRule, searchResidues, residueFollowingSearchResidues, cleavedResidueNum, out var exceptionCharIndexInSearchResidues);

                        if (matchFoundToExceptionResidue)
                        {
                            // Exception char matched; can't count this as the cleavage point
                            matchFound = false;
                            var recursiveCheck = false;
                            var newStartResidueNum = default(int);

                            if (cleavageRule.ReversedCleavageDirection)
                            {
                                if (cleavedResidueNum + 1 < searchResidues.Length)
                                {
                                    recursiveCheck = true;
                                    newStartResidueNum = cleavedResidueNum + 2;
                                }
                            }
                            else if (exceptionCharIndexInSearchResidues < searchResidues.Length)
                            {
                                recursiveCheck = true;
                                newStartResidueNum = exceptionCharIndexInSearchResidues;
                            }

                            if (recursiveCheck)
                            {
                                // Recursively call this function to find the next cleavage position, using an updated startResidue position

                                var residueNumViaRecursiveSearch = GetTrypticNameFindNextCleavageLoc(searchResidues, residueFollowingSearchResidues, newStartResidueNum, cleavageRule, terminiiSymbol);

                                if (residueNumViaRecursiveSearch > 0)
                                {
                                    // Found a residue further along that is a valid cleavage point
                                    cleavedResidueNum = residueNumViaRecursiveSearch;
                                    if (cleavedResidueNum >= startResidueNum)
                                    {
                                        matchFound = true;
                                    }
                                }
                                else
                                {
                                    cleavedResidueNum = 0;
                                }
                            }
                            else
                            {
                                cleavedResidueNum = 0;
                            }
                        }
                    }
                }

                if (matchFound)
                {
                    if (minCleavedResidueNum < 0)
                    {
                        minCleavedResidueNum = cleavedResidueNum;
                    }
                    else if (cleavedResidueNum < minCleavedResidueNum)
                    {
                        minCleavedResidueNum = cleavedResidueNum;
                    }
                }
            }

            if (minCleavedResidueNum < 0 && residueFollowingSearchResidues == terminiiSymbol.ToString())
            {
                minCleavedResidueNum = searchResidues.Length + 1;
            }

            foreach (var additionalRule in cleavageRule.AdditionalCleavageRules)
            {
                var additionalRuleResidueNum = GetTrypticNameFindNextCleavageLoc(searchResidues, residueFollowingSearchResidues, startResidueNum, additionalRule, terminiiSymbol);
                if (additionalRuleResidueNum >= 0 && additionalRuleResidueNum < minCleavedResidueNum)
                {
                    minCleavedResidueNum = additionalRuleResidueNum;
                }
            }

            if (minCleavedResidueNum < 0)
            {
                return 0;
            }

            return minCleavedResidueNum;
        }

        public string GetTrypticPeptideNext(
            string proteinResidues,
            int searchStartLoc,
            out int returnResidueStart,
            out int returnResidueEnd)
        {
            return GetTrypticPeptideNext(proteinResidues, searchStartLoc, mTrypticCleavageRule, out returnResidueStart, out returnResidueEnd);
        }

        /// <summary>
        /// Finds the next tryptic peptide in proteinResidues, starting the search as searchStartLoc
        /// </summary>
        /// <remarks>
        /// Useful when obtaining all of the tryptic peptides for a protein, since this function will operate
        /// much faster than repeatedly calling GetTrypticPeptideByFragmentNumber()
        /// </remarks>
        /// <param name="proteinResidues"></param>
        /// <param name="searchStartLoc"></param>
        /// <param name="cleavageRule">Cleavage rule</param>
        /// <param name="returnResidueStart">Output: residue in the protein where the peptide starts</param>
        /// <param name="returnResidueEnd">Output: residue in the protein where the peptide ends</param>
        /// <param name="terminiiSymbol"></param>
        /// <returns>The next tryptic peptide in proteinResidues</returns>
        public string GetTrypticPeptideNext(
            string proteinResidues,
            int searchStartLoc,
            CleavageRule cleavageRule,
            out int returnResidueStart,
            out int returnResidueEnd,
            char terminiiSymbol = TERMINII_SYMBOL)
        {
            int proteinResiduesLength;
            if (searchStartLoc < 1)
            {
                searchStartLoc = 1;
            }

            if (proteinResidues == null)
            {
                proteinResiduesLength = 0;
            }
            else
            {
                proteinResiduesLength = proteinResidues.Length;
            }

            if (searchStartLoc > proteinResiduesLength)
            {
                returnResidueStart = 0;
                returnResidueEnd = 0;
                return string.Empty;
            }

            var ruleResidueNum = GetTrypticNameFindNextCleavageLoc(proteinResidues, terminiiSymbol.ToString(), searchStartLoc, cleavageRule, terminiiSymbol);
            if (ruleResidueNum > 0)
            {
                returnResidueStart = searchStartLoc;
                if (ruleResidueNum > proteinResiduesLength)
                {
                    returnResidueEnd = proteinResiduesLength;
                }
                else
                {
                    returnResidueEnd = ruleResidueNum;
                }

                return proteinResidues.Substring(returnResidueStart - 1, returnResidueEnd - returnResidueStart + 1);
            }

            returnResidueStart = 1;
            returnResidueEnd = proteinResiduesLength;
            return proteinResidues;
        }

        /// <summary>
        /// Finds the desired tryptic peptide from proteinResidues
        /// </summary>
        /// <remarks>
        /// For example, if proteinResidues = "IGKANRMTFGL"
        /// when desiredPeptideNumber = 1, returns "IGK"
        /// when desiredPeptideNumber = 2, returns "ANR"
        /// when desiredPeptideNumber = 3, returns "MTFGL"
        /// </remarks>
        /// <param name="proteinResidues"></param>
        /// <param name="desiredPeptideNumber"></param>
        /// <param name="cleavageRule"></param>
        /// <param name="returnResidueStart">Output: residue in the protein where the peptide starts</param>
        /// <param name="returnResidueEnd">Output: residue in the protein where the peptide ends</param>
        /// <param name="terminiiSymbol"></param>
        /// <param name="ignoreCase"></param>
        /// <returns>The desired tryptic peptide from proteinResidues</returns>
        public string GetTrypticPeptideByFragmentNumber(
            string proteinResidues,
            int desiredPeptideNumber,
            CleavageRule cleavageRule,
            out int returnResidueStart,
            out int returnResidueEnd,
            char terminiiSymbol = TERMINII_SYMBOL,
            bool ignoreCase = true)
        {
            string matchingFragment;

            if (desiredPeptideNumber < 1)
            {
                returnResidueStart = 0;
                returnResidueEnd = 0;
                return string.Empty;
            }

            if (ignoreCase)
            {
                proteinResidues = proteinResidues.ToUpper();
            }

            var proteinResiduesLength = proteinResidues.Length;

            // startLoc tracks residue number, ranging from 1 to length of the protein
            var startLoc = 1;
            var prevStartLoc = 0;

            int ruleResidueNum = 0;
            var currentTrypticPeptideNumber = 0;
            while (currentTrypticPeptideNumber < desiredPeptideNumber)
            {
                ruleResidueNum = GetTrypticNameFindNextCleavageLoc(proteinResidues, terminiiSymbol.ToString(), startLoc, cleavageRule, terminiiSymbol);
                if (ruleResidueNum > 0)
                {
                    currentTrypticPeptideNumber++;
                    prevStartLoc = startLoc;
                    startLoc = ruleResidueNum + 1;

                    if (prevStartLoc > proteinResiduesLength)
                    {
                        // User requested a peptide number that doesn't exist
                        returnResidueStart = 0;
                        returnResidueEnd = 0;
                        return string.Empty;
                    }
                }
                else
                {
                    // This code should never be reached
                    Console.WriteLine("Unexpected code point reached in GetTrypticPeptideByFragmentNumber");
                    break;
                }
            }

            if (currentTrypticPeptideNumber > 0 && prevStartLoc > 0)
            {
                if (prevStartLoc > proteinResidues.Length)
                {
                    // User requested a peptide number that is too high
                    returnResidueStart = 0;
                    returnResidueEnd = 0;
                    matchingFragment = string.Empty;
                }
                else
                {
                    // Match found, find the extent of this peptide
                    returnResidueStart = prevStartLoc;
                    if (ruleResidueNum > proteinResiduesLength)
                    {
                        returnResidueEnd = proteinResiduesLength;
                    }
                    else
                    {
                        returnResidueEnd = ruleResidueNum;
                    }

                    matchingFragment = proteinResidues.Substring(prevStartLoc - 1, ruleResidueNum - prevStartLoc + 1);
                }
            }
            else
            {
                returnResidueStart = 1;
                returnResidueEnd = proteinResiduesLength;
                matchingFragment = proteinResidues;
            }

            return matchingFragment;
        }

        private bool FindNextCleavageResidue(
            CleavageRule cleavageRule,
            int charIndexInCleavageResidues,
            string searchResidues,
            int startResidueNum,
            out int cleavageResidueNum)
        {
            if (cleavageRule.ReversedCleavageDirection)
            {
                // Cleave before the matched residue
                // Note that the CharLoc value we're storing is the location just before the cleavage point
                cleavageResidueNum = searchResidues.IndexOf(cleavageRule.CleavageResidues[charIndexInCleavageResidues], startResidueNum);
            }
            else
            {
                cleavageResidueNum = searchResidues.IndexOf(cleavageRule.CleavageResidues[charIndexInCleavageResidues], startResidueNum - 1) + 1;
            }

            return cleavageResidueNum >= 1 && cleavageResidueNum >= startResidueNum;
        }

        private bool IsMatchToExceptionResidue(
            CleavageRule cleavageRule,
            string searchResidues,
            string residueFollowingSearchResidues,
            int cleavedResidueIndex,
            out int exceptionCharIndexInSearchResidues)
        {
            char exceptionResidueToCheck;

            if (cleavageRule.ReversedCleavageDirection)
            {
                // Make sure the residue before the matched residue does not match exceptionResidues
                // We already subtracted 1 from charLoc so charLoc is already the correct character number
                // Note that the logic in FindNextCleavageResidue assures that charLoc is > 0
                exceptionCharIndexInSearchResidues = cleavedResidueIndex;
                exceptionResidueToCheck = searchResidues[exceptionCharIndexInSearchResidues - 1];
            }
            // Make sure suffixResidue does not match exceptionResidues
            else if (cleavedResidueIndex < searchResidues.Length)
            {
                exceptionCharIndexInSearchResidues = cleavedResidueIndex + 1;
                exceptionResidueToCheck = searchResidues[exceptionCharIndexInSearchResidues - 1];
            }
            else
            {
                // Matched the last residue in searchResidues
                exceptionCharIndexInSearchResidues = searchResidues.Length + 1;
                exceptionResidueToCheck = residueFollowingSearchResidues[0];
            }

            return cleavageRule.ExceptionResidues.IndexOf(exceptionResidueToCheck) >= 0;
        }

        /// <summary>
        /// Removing the leading H, if present
        /// </summary>
        /// <remarks>This is only applicable for sequences in 3 letter notation</remarks>
        /// <param name="workingSequence">Amino acids, in 3 letter notation</param>
        private void RemoveLeadingH(ref string workingSequence)
        {
            if (workingSequence.Length >= 4 && string.Equals(workingSequence.Substring(0, 1), "H", StringComparison.OrdinalIgnoreCase))
            {
                // If next character is not a character, remove the H and the next character
                if (!char.IsLetter(workingSequence[1]))
                {
                    workingSequence = workingSequence.Substring(2);
                }
                // Otherwise, see if next three characters are letters
                else if (char.IsLetter(workingSequence[1]) && char.IsLetter(workingSequence[2]) && char.IsLetter(workingSequence[3]))
                {
                    // Formula starts with 4 characters and the first is H, see if the first 3 characters are a valid amino acid code

                    var oneLetterSymbol = GetAminoAcidSymbolConversion(workingSequence.Substring(1, 3), false);

                    if (oneLetterSymbol.Length > 0)
                    {
                        // Starts with H then a valid 3 letter abbreviation, so remove the initial H
                        workingSequence = workingSequence.Substring(1);
                    }
                }
            }
        }

        /// <summary>
        /// Removing the trailing OH, if present
        /// </summary>
        /// <remarks>This is only applicable for sequences in 3 letter notation</remarks>
        /// <param name="workingSequence">Amino acids, in 3 letter notation</param>
        private void RemoveTrailingOH(ref string workingSequence)
        {
            var stringLength = workingSequence.Length;
            if (stringLength >= 5 && string.Equals(workingSequence.Substring(stringLength - 2, 2), "OH", StringComparison.OrdinalIgnoreCase))
            {
                // If previous character is not a character, then remove the OH (and the character preceding)
                if (!char.IsLetter(workingSequence[stringLength - 3]))
                {
                    workingSequence = workingSequence.Substring(0, stringLength - 3);
                }
                // Otherwise, see if previous three characters are letters
                else if (char.IsLetter(workingSequence[stringLength - 2]) && char.IsLetter(workingSequence[stringLength - 3]) && char.IsLetter(workingSequence[stringLength - 4]))
                {
                    // Formula ends with 3 characters and the last two are OH, see if the last 3 characters are a valid amino acid code

                    var oneLetterSymbol = GetAminoAcidSymbolConversion(workingSequence.Substring(stringLength - 4, 3), false);
                    if (oneLetterSymbol.Length > 0)
                    {
                        // Ends with a valid 3 letter abbreviation then OH, so remove the OH
                        workingSequence = workingSequence.Substring(0, stringLength - 2);
                    }
                }
            }
        }

        private bool ResiduesMatchCleavageRule(char testResidue, char exceptionResidue, CleavageRule cleavageRule)
        {
            if (CheckSequenceAgainstCleavageRuleMatchTestResidue(testResidue, cleavageRule.CleavageResidues))
            {
                // Match found
                // See if exceptionResidue matches any of the exception residues
                if (!CheckSequenceAgainstCleavageRuleMatchTestResidue(exceptionResidue, cleavageRule.ExceptionResidues))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Define the C-terminus using an empirical formula
        /// </summary>
        /// <remarks>
        /// Typical C terminus groups
        /// Free Acid = OH
        /// Amide = NH2
        /// </remarks>
        /// <param name="formula">Can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features</param>
        /// <param name="followingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>0 if success; 1 if error</returns>
        private int SetCTerminus(string formula, string followingResidue = "", bool use3LetterCode = false)
        {
            int returnVal;

            mCTerminus.Formula = formula;
            mCTerminus.Mass = ComputeFormulaWeightCHNOSP(mCTerminus.Formula);
            mCTerminus.MassElementMode = mCurrentElementMode;
            if (mCTerminus.Mass < 0d)
            {
                mCTerminus.Mass = 0d;
                returnVal = 1;
            }
            else
            {
                returnVal = 0;
            }

            mCTerminus.PrecedingResidue = string.Empty;
            if (use3LetterCode && followingResidue.Length > 0)
            {
                mCTerminus.FollowingResidue = GetAminoAcidSymbolConversion(followingResidue, false);
            }
            else
            {
                mCTerminus.FollowingResidue = followingResidue;
            }

            if (!mDelayUpdateResidueMass)
            {
                UpdateSequenceMass();
            }

            return returnVal;
        }

        /// <summary>
        /// Define the C-terminus using an enum in CTerminusGroupConstants
        /// </summary>
        /// <param name="cTerminusGroup"></param>
        /// <param name="followingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>0 if success; 1 if error</returns>
        public int SetCTerminusGroup(CTerminusGroupConstants cTerminusGroup, string followingResidue = "", bool use3LetterCode = false)
        {
            int errorCode;

            switch (cTerminusGroup)
            {
                case CTerminusGroupConstants.Hydroxyl:
                    errorCode = SetCTerminus("OH", followingResidue, use3LetterCode);
                    break;
                case CTerminusGroupConstants.Amide:
                    errorCode = SetCTerminus("NH2", followingResidue, use3LetterCode);
                    break;
                case CTerminusGroupConstants.None:
                    errorCode = SetCTerminus(string.Empty, followingResidue, use3LetterCode);
                    break;
                default:
                    errorCode = 1;
                    break;
            }

            return errorCode;
        }

        /// <summary>
        /// Define the N-terminus using an empirical formula
        /// </summary>
        /// <remarks>
        /// Typical N terminus groups
        /// Hydrogen = H
        /// Acetyl = C2OH3
        /// PyroGlu = C5O2NH6
        /// Carbamyl = CONH2
        /// PTC = C7H6NS
        /// </remarks>
        /// <param name="formula">Can only contain C, H, N, O, S, or P, and cannot contain any parentheses or other advanced formula features</param>
        /// <param name="precedingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>0 if success; 1 if error</returns>
        private int SetNTerminus(string formula, string precedingResidue = "", bool use3LetterCode = false)
        {
            int returnVal;

            mNTerminus.Formula = formula;
            mNTerminus.Mass = ComputeFormulaWeightCHNOSP(mNTerminus.Formula);
            mNTerminus.MassElementMode = mCurrentElementMode;
            if (mNTerminus.Mass < 0d)
            {
                mNTerminus.Mass = 0d;
                returnVal = 1;
            }
            else
            {
                returnVal = 0;
            }

            mNTerminus.PrecedingResidue = string.Empty;
            if (use3LetterCode && precedingResidue.Length > 0)
            {
                mNTerminus.PrecedingResidue = GetAminoAcidSymbolConversion(precedingResidue, false);
            }
            else
            {
                mNTerminus.PrecedingResidue = precedingResidue;
            }

            mNTerminus.FollowingResidue = string.Empty;

            if (!mDelayUpdateResidueMass)
            {
                UpdateSequenceMass();
            }

            return returnVal;
        }

        /// <summary>
        /// Define the N-terminus using an enum in NTerminusGroupConstants
        /// </summary>
        /// <param name="nTerminusGroup"></param>
        /// <param name="precedingResidue"></param>
        /// <param name="use3LetterCode"></param>
        /// <returns>0 if success; 1 if error</returns>
        public int SetNTerminusGroup(NTerminusGroupConstants nTerminusGroup, string precedingResidue = "", bool use3LetterCode = false)
        {
            int errorCode;

            switch (nTerminusGroup)
            {
                case NTerminusGroupConstants.Hydrogen:
                    errorCode = SetNTerminus("H", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupConstants.HydrogenPlusProton:
                    errorCode = SetNTerminus("HH", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupConstants.Acetyl:
                    errorCode = SetNTerminus("C2OH3", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupConstants.PyroGlu:
                    errorCode = SetNTerminus("C5O2NH6", precedingResidue, use3LetterCode);
                    break;
                // ReSharper disable once StringLiteralTypo
                case NTerminusGroupConstants.Carbamyl:
                    errorCode = SetNTerminus("CONH2", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupConstants.PTC:
                    errorCode = SetNTerminus("C7H6NS", precedingResidue, use3LetterCode);
                    break;
                case NTerminusGroupConstants.None:
                    errorCode = SetNTerminus("", precedingResidue, use3LetterCode);
                    break;
                default:
                    errorCode = 1;
                    break;
            }

            return errorCode;
        }

        /// <summary>
        /// Define the peptide sequence
        /// </summary>
        /// <remarks>Calls UpdateSequenceMass</remarks>
        /// <param name="sequence">Peptide or protein amino acid symbols</param>
        /// <param name="nTerminus">N-terminus type</param>
        /// <param name="cTerminus">C-terminus type</param>
        /// <param name="is3LetterCode">When true, sequence uses 3 letter amino acid symbols</param>
        /// <param name="oneLetterCheckForPrefixAndSuffixResidues">
        /// When true, if the sequence has prefix and suffix letters, they are removed
        /// For example: R.ABCDEFGK.R will be stored as ABCDEFGK
        /// </param>
        /// <param name="threeLetterCheckForPrefixHandSuffixOH">
        /// When true, if the sequence starts with H or OH, remove those letters
        /// For example: Arg.AlaCysAspPheGlyLys.Arg will be stored as AlaCysAspPheGlyLys
        /// </param>
        /// <returns>
        /// 0 if success or 1 if an error
        /// Will return 0 if the sequence is blank or if it contains no valid residues
        /// </returns>
        public virtual int SetSequence(
            string sequence,
            NTerminusGroupConstants nTerminus = NTerminusGroupConstants.Hydrogen,
            CTerminusGroupConstants cTerminus = CTerminusGroupConstants.Hydroxyl,
            bool is3LetterCode = false,
            bool oneLetterCheckForPrefixAndSuffixResidues = true,
            bool threeLetterCheckForPrefixHandSuffixOH = true)
        {
            mResidues = string.Empty;

            sequence = sequence.Trim();
            var sequenceStrLength = sequence.Length;
            if (sequenceStrLength == 0)
            {
                UpdateSequenceMass();
                return 0;
            }

            if (!is3LetterCode)
            {
                // Sequence is 1 letter codes

                if (oneLetterCheckForPrefixAndSuffixResidues)
                {
                    sequence = CheckForAndRemovePrefixAndSuffixResidues(sequence, out _, out _);
                    sequenceStrLength = sequence.Length;
                }

                // Now parse the string of 1 letter characters
                for (var index = 0; index < sequenceStrLength; index++)
                {
                    if (char.IsLetter(sequence[index]))
                    {
                        // Character found
                        mResidues += sequence[index].ToString();
                    }
                }
            }
            else
            {
                // Sequence is 3 letter codes
                if (threeLetterCheckForPrefixHandSuffixOH)
                {
                    // Look for a leading H or trailing OH, provided those don't match any of the amino acids
                    RemoveLeadingH(ref sequence);
                    RemoveTrailingOH(ref sequence);

                    // Recompute sequence length
                    sequenceStrLength = sequence.Length;
                }

                var index = 0;
                while (index <= sequenceStrLength - 3)
                {
                    if (char.IsLetter(sequence[index]))
                    {
                        if (char.IsLetter(sequence[index + 1]) && char.IsLetter(sequence[index + 2]))
                        {
                            var oneLetterSymbol = GetAminoAcidSymbolConversion(sequence.Substring(index, 3), false);

                            if (oneLetterSymbol.Length == 0)
                            {
                                // 3 letter symbol not found
                                // Add anyway, but mark as X
                                oneLetterSymbol = UNKNOWN_SYMBOL_ONE_LETTER.ToString();
                            }

                            mResidues += oneLetterSymbol;
                            index += 3;
                        }
                        else
                        {
                            // First letter is a character, but next two are not; ignore it
                            index++;
                        }
                    }
                    else
                    {
                        // Ignore anything else
                        index++;
                    }
                }
            }

            // By calling SetNTerminus and SetCTerminus, the UpdateSequenceMass() method will also be called
            // We don't want to compute the mass yet
            mDelayUpdateResidueMass = true;
            SetNTerminusGroup(nTerminus);
            SetCTerminusGroup(cTerminus);

            mDelayUpdateResidueMass = false;
            UpdateSequenceMass();

            return 0;
        }

        /// <summary>
        /// Updates the sequence without performing any error checking
        /// Does not look for or remove prefix or suffix letters
        /// </summary>
        /// <remarks>Calls UpdateSequenceMass</remarks>
        /// <param name="sequenceNoPrefixOrSuffix"></param>
        public virtual void SetSequenceOneLetterCharactersOnly(string sequenceNoPrefixOrSuffix)
        {
            mResidues = sequenceNoPrefixOrSuffix;
            UpdateSequenceMass();
        }

        /// <summary>
        /// Computes mass for each residue in mResidues
        /// Only process one letter amino acid abbreviations
        /// </summary>
        private void UpdateSequenceMass()
        {
            double runningTotal;
            var protonatedNTerminus = default(bool);

            if (mDelayUpdateResidueMass)
            {
                return;
            }

            if (mResidues.Length == 0)
            {
                runningTotal = 0d;
            }
            else
            {
                // The N-terminus ions are the basis for the running total
                ValidateTerminusMasses();

                runningTotal = mNTerminus.Mass;
                if (string.Equals(mNTerminus.Formula, "HH", StringComparison.OrdinalIgnoreCase))
                {
                    // ntgNTerminusGroupConstants.HydrogenPlusProton; since we add back in the proton below when computing the fragment masses,
                    // we need to subtract it out here
                    // However, we need to subtract out mHydrogenMass, and not mChargeCarrierMass since the current
                    // formula's mass was computed using two hydrogen atoms, and not one hydrogen and one charge carrier
                    protonatedNTerminus = true;
                    runningTotal -= mHydrogenMass;
                }

                foreach (var oneLetterSymbol in mResidues)
                {
                    try
                    {
                        runningTotal += AminoAcidMasses[oneLetterSymbol];

                        if (oneLetterSymbol == 'C' && CysTreatmentMode != CysTreatmentModeConstants.Untreated)
                        {
                            switch (CysTreatmentMode)
                            {
                                case CysTreatmentModeConstants.Iodoacetamide:
                                    runningTotal += mIodoacetamideMass;
                                    break;
                                case CysTreatmentModeConstants.IodoaceticAcid:
                                    runningTotal += mIodoaceticAcidMass;
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Skip this residue
                        Console.WriteLine("Error parsing Residue symbols in UpdateSequenceMass; " + ex.Message);
                    }
                }

                runningTotal += mCTerminus.Mass;
                if (protonatedNTerminus)
                {
                    runningTotal += ChargeCarrierMass;
                }
            }

            mTotalMassElementMode = mCurrentElementMode;
            mTotalMass = runningTotal;
        }

        private void UpdateStandardMasses()
        {
            const double DEFAULT_CHARGE_CARRIER_MASS_AVG = 1.00739d;
            const double DEFAULT_CHARGE_CARRIER_MASS_MONOISOTOPIC = 1.00727649d;

            if (mCurrentElementMode == ElementModeConstants.AverageMass)
            {
                ChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_AVG;
            }
            else
            {
                ChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_MONOISOTOPIC;
            }

            // Update Hydrogen mass
            mHydrogenMass = ComputeFormulaWeightCHNOSP("H");

            mIodoacetamideMass = ComputeFormulaWeightCHNOSP("C2H3NO");
            mIodoaceticAcidMass = ComputeFormulaWeightCHNOSP("C2H2O2");
        }

        private void ValidateTerminusMasses()
        {
            if (mNTerminus.MassElementMode != mCurrentElementMode)
            {
                mNTerminus.Mass = ComputeFormulaWeightCHNOSP(mNTerminus.Formula);
                mNTerminus.MassElementMode = mCurrentElementMode;
            }

            if (mCTerminus.MassElementMode != mCurrentElementMode)
            {
                mCTerminus.Mass = ComputeFormulaWeightCHNOSP(mCTerminus.Formula);
                mCTerminus.MassElementMode = mCurrentElementMode;
            }
        }

        private void InitializeSharedData()
        {
            if (ElementMasses == null)
            {
                ElementMasses = new Dictionary<char, double>();
            }
            else
            {
                ElementMasses.Clear();
            }

            if (mCurrentElementMode == ElementModeConstants.IsotopicMass)
            {
                // Isotopic masses
                ElementMasses.Add('C', 12.0d);
                ElementMasses.Add('H', 1.0078246d);
                ElementMasses.Add('N', 14.003074d);
                ElementMasses.Add('O', 15.994915d);
                ElementMasses.Add('S', 31.972072d);
                ElementMasses.Add('P', 30.973763d);
            }
            else
            {
                // Average masses
                ElementMasses.Add('C', 12.0107d);
                ElementMasses.Add('H', 1.00794d);
                ElementMasses.Add('N', 14.00674d);
                ElementMasses.Add('O', 15.9994d);
                ElementMasses.Add('S', 32.066d);
                ElementMasses.Add('P', 30.973761d);
            }

            if (AminoAcidMasses == null)
            {
                AminoAcidMasses = new Dictionary<char, double>();
            }
            else
            {
                AminoAcidMasses.Clear();
            }

            if (AminoAcidSymbols == null)
            {
                AminoAcidSymbols = new Dictionary<char, string>();
            }
            else
            {
                AminoAcidSymbols.Clear();
            }

            AddAminoAcidStatEntry('A', "Ala", "C3H5NO");
            AddAminoAcidStatEntry('B', "Bbb", "C4H6N2O2");       // N or D
            AddAminoAcidStatEntry('C', "Cys", "C3H5NOS");
            AddAminoAcidStatEntry('D', "Asp", "C4H5NO3");
            AddAminoAcidStatEntry('E', "Glu", "C5H7NO3");
            AddAminoAcidStatEntry('F', "Phe", "C9H9NO");
            AddAminoAcidStatEntry('G', "Gly", "C2H3NO");
            AddAminoAcidStatEntry('H', "His", "C6H7N3O");
            AddAminoAcidStatEntry('I', "Ile", "C6H11NO");
            AddAminoAcidStatEntry('J', "Jjj", "C6H11NO");        // Unknown; use mass of Ile/Leu
            AddAminoAcidStatEntry('K', "Lys", "C6H12N2O");
            AddAminoAcidStatEntry('L', "Leu", "C6H11NO");
            AddAminoAcidStatEntry('M', "Met", "C5H9NOS");
            AddAminoAcidStatEntry('N', "Asn", "C4H6N2O2");
            AddAminoAcidStatEntry('O', "Orn", "C5H10N2O");
            AddAminoAcidStatEntry('P', "Pro", "C5H7NO");
            AddAminoAcidStatEntry('Q', "Gln", "C5H8N2O2");
            AddAminoAcidStatEntry('R', "Arg", "C6H12N4O");
            AddAminoAcidStatEntry('S', "Ser", "C3H5NO2");
            AddAminoAcidStatEntry('T', "Thr", "C4H7NO2");
            AddAminoAcidStatEntry('U', "Gla", "C6H7NO5");
            AddAminoAcidStatEntry('V', "Val", "C5H9NO");
            AddAminoAcidStatEntry('W', "Trp", "C11H10N2O");
            AddAminoAcidStatEntry(UNKNOWN_SYMBOL_ONE_LETTER, UNKNOWN_SYMBOL_THREE_LETTERS, "C6H11NO");       // Unknown; use mass of Ile/Leu
            AddAminoAcidStatEntry('Y', "Tyr", "C9H9NO2");
            AddAminoAcidStatEntry('Z', "Zzz", "C5H8N2O2");       // Q or E (note that these are 0.984 Da apart)

            UpdateStandardMasses();

            mTrypticCleavageRule = new CleavageRule("Fully Tryptic", TRYPTIC_RULE_RESIDUES, TRYPTIC_EXCEPTION_RESIDUES, false);
        }
    }
}
