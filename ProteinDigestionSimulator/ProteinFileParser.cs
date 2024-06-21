using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NETPrediction;
using PRISM;
using PRISM.FileProcessor;
using ProteinDigestionSimulator.Options;
using ProteinFileReader;
using ValidateFastaFile;

// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Started in 2004
//
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the 2-Clause BSD License; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// https://opensource.org/licenses/BSD-2-Clause
//
// Copyright 2021 Battelle Memorial Institute

namespace ProteinDigestionSimulator
{
    // ReSharper disable once GrammarMistakeInComment

    /// <summary>
    /// <para>
    /// This class will read a protein FASTA file or delimited protein info file and parse it
    /// to create a delimited protein list output file, and optionally an in-silico digested output file
    /// </para>
    /// <para>
    /// It can also create a FASTA file containing reversed or scrambled sequences, and these can
    /// be based on all of the proteins in the input file, or a sampling of the proteins in the input file
    /// </para>
    /// </summary>
    public class ProteinFileParser : ProcessFilesBase
    {
        // Ignore Spelling: addnl, ComputepI, Cys, Cysteine, fasta, gi, hydrophobicity, Ile, isoelectric, Leu, pre, SepChar, silico, varchar

        private const string PROGRAM_DATE = "November 20, 2021";

        private const int SCRAMBLING_CACHE_LENGTH = 4000;
        private const string PROTEIN_PREFIX_SCRAMBLED = "Random_";
        private const string PROTEIN_PREFIX_REVERSED = "XXX_";

        private const int MAXIMUM_PROTEIN_NAME_LENGTH = 512;

        private const int MAX_ABBREVIATED_FILENAME_LENGTH = 15;

        // The value of 7995 is chosen because the maximum varchar() value in SQL Server is varchar(8000)
        // and we want to prevent truncation errors when importing protein names and descriptions into SQL Server
        private const int MAX_PROTEIN_DESCRIPTION_LENGTH = FastaValidator.MAX_PROTEIN_DESCRIPTION_LENGTH;

        // Error codes specialized for this class
        public enum ParseProteinFileErrorCodes
        {
            NoError = 0,
            ErrorReadingInputFile = 2,
            ErrorCreatingProteinOutputFile = 4,
            ErrorCreatingDigestedProteinOutputFile = 8,
            ErrorCreatingScrambledProteinOutputFile = 16,
            ErrorWritingOutputFile = 32,
            ErrorInitializingObjectVariables = 64,
            DigestProteinSequenceError = 128,
            UnspecifiedError = -1
        }

        public enum ProteinScramblingModeConstants
        {
            /// <summary>
            /// Leave protein sequences unchanged
            /// </summary>
            None = 0,

            /// <summary>
            /// Reverse protein sequences
            /// </summary>
            Reversed = 1,

            /// <summary>
            /// Randomize (scramble) protein sequences
            /// </summary>
            Randomized = 2,

            /// <summary>
            /// Write out both forward and reverse protein sequences
            /// </summary>
            Decoy = 3
        }

        public class AddnlRef : IComparable<AddnlRef>
        {
            public string RefName { get; set; }                // e.g. in gi:12334  the RefName is "gi" and the RefAccession is "1234"
            public string RefAccession { get; set; }

            // ReSharper disable once ConvertToPrimaryConstructor
            public AddnlRef(string refName, string refAccession)
            {
                RefName = refName;
                RefAccession = refAccession;
            }

            public int CompareTo(AddnlRef other)
            {
                // IComparable implementation to allow comparison of additional protein references
                if (ReferenceEquals(this, other))
                {
                    return 0;
                }

                if (other is null)
                {
                    return 1;
                }

                var refNameComparison = string.CompareOrdinal(RefName, other.RefName);
                if (refNameComparison != 0)
                {
                    return refNameComparison;
                }

                return string.CompareOrdinal(RefAccession, other.RefAccession);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public class ProteinInfo
        {
            public string Name { get; set; }
            public int AlternateNameCount => AlternateNames.Count;
            public List<AddnlRef> AlternateNames { get; } = new();
            public string Description { get; set; }
            public string Sequence { get; set; }
            public string SequenceHash { get; set; }                   // Only populated if ComputeSequenceHashValues=true
            public double Mass { get; set; }
            // ReSharper disable once InconsistentNaming
            public float pI { get; set; }
            public float Hydrophobicity { get; set; }
            public float ProteinNET { get; set; }
            public float ProteinSCXNET { get; set; }

            /// <summary>
            /// Show the protein name
            /// </summary>
            public override string ToString()
            {
                return Name ?? string.Empty;
            }
        }

        private class FilePathInfo
        {
            public string ProteinInputFilePath { get; set; }
            public string OutputFileNameBaseOverride { get; set; }
            public string OutputFolderPath { get; set; }
            public string ProteinOutputFilePath { get; set; }
            public string DigestedProteinOutputFilePath { get; set; }

            /// <summary>
            /// Show the input file path
            /// </summary>
            public override string ToString()
            {
                return ProteinInputFilePath ?? string.Empty;
            }
        }

        private class ScramblingResidueCache
        {
            /// <summary>
            /// Cache of residues parsed
            /// </summary>
            /// <remarks>
            /// when this reaches 4000 characters, a portion of this text is appended to ResiduesToWrite
            /// </remarks>
            public string Cache { get; set; }
            public int CacheLength { get; set; }
            public int SamplingPercentage { get; }
            public int OutputCount { get; set; }
            public string ResiduesToWrite { get; set; }

            // ReSharper disable once ConvertToPrimaryConstructor
            public ScramblingResidueCache(DigestionSimulatorOptions options)
            {
                SamplingPercentage = options.ProteinScramblingSamplingPercentage switch
                {
                    <= 0 or > 100 => 100,
                    _ => options.ProteinScramblingSamplingPercentage,
                };
            }

            /// <summary>
            /// Show the cached residues
            /// </summary>
            public override string ToString()
            {
                return ResiduesToWrite ?? string.Empty;
            }
        }

        public DigestionSimulatorOptions ProcessingOptions { get; }

        public InSilicoDigest InSilicoDigester { get; }

        private readonly ElutionTimePredictionKangas mNETCalculator;
        private readonly SCXElutionTimePredictionKangas mSCXNETCalculator;

        private readonly List<ProteinInfo> mProteins = new();

        private string mFileNameAbbreviated;
        private Dictionary<string, int> mMasterSequencesDictionary;

        private int mNextUniqueIDForMasterSeqs;

        private string mSubtaskProgressStepDescription = string.Empty;
        private float mSubtaskProgressPercentComplete;

        // PercentComplete ranges from 0 to 100, but can contain decimal percentage values
        public event ProgressChangedEventHandler SubtaskProgressChanged;

        /// <summary>
        /// Summary of the result of processing
        /// </summary>
        public string ProcessingSummary { get; set; }

        public int InputFileProteinsProcessed { get; private set; }

        public int InputFileLinesRead { get; private set; }

        public int InputFileLineSkipCount { get; private set; }

        public Exception MostRecentException { get; private set; }

        public ParseProteinFileErrorCodes LocalErrorCode { get; private set; }

        public bool ParsedFileIsFastaFile { get; private set; }

        public int SequenceCountLikelyDNA { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options"></param>
        public ProteinFileParser(DigestionSimulatorOptions options)
        {
            mFileDate = PROGRAM_DATE;

            ProcessingOptions = options;
            InitializeLocalVariables();

            InSilicoDigester = new InSilicoDigest(ProcessingOptions);

            InSilicoDigester.ErrorEvent += InSilicoDigest_ErrorEvent;

            if (!ProcessingOptions.QuietMode)
            {
                InSilicoDigester.ProgressChanged += InSilicoDigest_ProgressChanged;
            }

            InSilicoDigester.ProgressReset += InSilicoDigest_ProgressReset;

            try
            {
                mNETCalculator = new ElutionTimePredictionKangas();
                mNETCalculator.ErrorEvent += NETCalculator_ErrorEvent;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error initializing LC NET Calculation class");
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorInitializingObjectVariables, ex);
            }

            try
            {
                mSCXNETCalculator = new SCXElutionTimePredictionKangas();
                mSCXNETCalculator.ErrorEvent += SCXNETCalculator_ErrorEvent;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error initializing SCX NET Calculation class");
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorInitializingObjectVariables, ex);
            }
        }

        private float ComputeSequenceHydrophobicity(string peptideSequence)
        {
            return InSilicoDigester.IsoelectricPointCalculator.CalculateSequenceHydrophobicity(peptideSequence);
        }

        private float ComputeSequencepI(string peptideSequence)
        {
            return InSilicoDigester.IsoelectricPointCalculator.CalculateSequencepI(peptideSequence);
        }

        private double ComputeSequenceMass(string peptideSequence)
        {
            return InSilicoDigester.ComputeSequenceMass(peptideSequence, ProcessingOptions.IncludeXResiduesInMass);
        }

        private float ComputeSequenceNET(string peptideSequence)
        {
            // Be sure to call InitializeObjectVariables before calling this function for the first time
            // Otherwise, mNETCalculator will be nothing
            if (mNETCalculator == null)
            {
                return 0f;
            }

            return mNETCalculator.GetElutionTime(peptideSequence);
        }

        private float ComputeSequenceSCXNET(string peptideSequence)
        {
            // Be sure to call InitializeObjectVariables before calling this function for the first time
            // Otherwise, mSCXNETCalculator will be nothing
            if (mSCXNETCalculator == null)
            {
                return 0f;
            }

            return mSCXNETCalculator.GetElutionTime(peptideSequence);
        }

        private void CreateDecoyFastaFile(string forwardSequenceFastaFile, FileInfo reverseSequenceFastaFile)
        {
            try
            {
                var decoyFastaFilePath = reverseSequenceFastaFile.FullName;

                var sourceReversedProteinsTempFile = new FileInfo(reverseSequenceFastaFile.FullName + ".SourceReverseProteins");
                if (sourceReversedProteinsTempFile.Exists)
                {
                    sourceReversedProteinsTempFile.Delete();
                }

                // Rename the reversed sequences file
                reverseSequenceFastaFile.MoveTo(sourceReversedProteinsTempFile.FullName);

                // Duplicate the forward sequences file
                var forwardProteinsFile = new FileInfo(forwardSequenceFastaFile);

                forwardProteinsFile.CopyTo(decoyFastaFilePath);

                // Append the reverse protein sequences to the decoy FASTA file
                using var decoyFileWriter = new StreamWriter(new FileStream(decoyFastaFilePath, FileMode.Append, FileAccess.Write, FileShare.Read));

                var decoyFileReader = new FastaFileReader();
                decoyFileReader.OpenFile(sourceReversedProteinsTempFile.FullName);

                while (decoyFileReader.ReadNextProteinEntry())
                {
                    var headerLine = string.Format("{0}{1}{2}{3}",
                        FastaFileReader.PROTEIN_LINE_START_CHAR, decoyFileReader.ProteinName,
                        FastaFileReader.PROTEIN_LINE_ACCESSION_TERMINATOR, decoyFileReader.ProteinDescription);

                    WriteFastaProteinAndResidues(decoyFileWriter, headerLine, decoyFileReader.ProteinSequence);
                }

                decoyFileReader.CloseFile();

                sourceReversedProteinsTempFile.Delete();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error in CreateDecoyFastaFile: " + ex.Message);
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorWritingOutputFile, ex);
            }
        }

        /// <summary>
        /// Digest the protein sequence using the given cleavage options
        /// </summary>
        /// <param name="peptideSequence"></param>
        /// <param name="peptideFragments"></param>
        /// <param name="proteinName"></param>
        /// <returns>The number of digested peptides in peptideFragments</returns>
        public int DigestProteinSequence(
            string peptideSequence,
            out List<PeptideSequenceWithNET> peptideFragments,
            string proteinName = "")
        {
            try
            {
                if (InSilicoDigester.ElementMassMode != ProcessingOptions.ElementMassMode)
                {
                    InSilicoDigester.ElementMassMode = ProcessingOptions.ElementMassMode;
                }

                return InSilicoDigester.DigestSequence(peptideSequence, out peptideFragments, ProcessingOptions.ComputepI, proteinName);
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.DigestProteinSequenceError, ex);
                peptideFragments = new List<PeptideSequenceWithNET>();
                return 0;
            }
        }

        /// <summary>
        /// Searches in the protein description for additional protein names
        /// </summary>
        /// <param name="protein"></param>
        private void ExtractAlternateProteinNamesFromDescription(ProteinInfo protein)
        {
            protein.AlternateNames.Clear();

            try
            {
                var proteinNames = protein.Description.Split(ProcessingOptions.FastaFileOptions.AddnlRefSepChar);

                if (proteinNames.Length == 0)
                    return;

                for (var index = 0; index < proteinNames.Length; index++)
                {
                    var charIndex = proteinNames[index].IndexOf(ProcessingOptions.FastaFileOptions.AddnlRefAccessionSepChar);

                    if (charIndex > 0)
                    {
                        if (index == proteinNames.Length - 1)
                        {
                            // Need to find the next space after charIndex and truncate proteinNames[] at that location
                            var spaceIndex = proteinNames[index].IndexOf(' ', charIndex);
                            if (spaceIndex >= 0)
                            {
                                proteinNames[index] = proteinNames[index].Substring(0, spaceIndex);
                            }
                        }

                        if (charIndex >= proteinNames[index].Length - 1)
                        {
                            // No accession after the colon; invalid entry so discard this entry and stop parsing
                            break;
                        }

                        protein.AlternateNames.Add(new AddnlRef(proteinNames[index].Substring(0, charIndex), proteinNames[index].Substring(charIndex + 1)));
                        charIndex = protein.Description.IndexOf(proteinNames[index], StringComparison.Ordinal);
                        if (charIndex >= 0)
                        {
                            if (charIndex + proteinNames[index].Length + 1 < protein.Description.Length)
                            {
                                protein.Description = protein.Description.Substring(charIndex + proteinNames[index].Length + 1);
                            }
                            else
                            {
                                protein.Description = string.Empty;
                            }
                        }
                        else
                        {
                            ShowErrorMessage("This code in ExtractAlternateProteinNamesFromDescription should never be reached");
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error parsing out additional Ref Names from the Protein Description: " + ex.Message);
            }
        }

        private double FractionLowercase(string proteinSequence)
        {
            var lowerCount = 0;
            var upperCount = 0;

            foreach (var residue in proteinSequence)
            {
                if (char.IsLower(residue))
                {
                    lowerCount++;
                }
                else if (char.IsUpper(residue))
                {
                    upperCount++;
                }
            }

            if (lowerCount + upperCount == 0)
            {
                return 0d;
            }

            return lowerCount / (double)(lowerCount + upperCount);
        }

        public override IList<string> GetDefaultExtensionsToParse()
        {
            return new List<string> { ".fasta", ".fasta.gz", ".faa", ".faa.gz", ".txt" };
        }

        public override string GetErrorMessage()
        {
            if (ErrorCode is not (ProcessFilesErrorCodes.LocalizedError or ProcessFilesErrorCodes.NoError))
            {
                return GetBaseClassErrorMessage();
            }

            var errorMessage = LocalErrorCode switch
            {
                ParseProteinFileErrorCodes.NoError => string.Empty,
                ParseProteinFileErrorCodes.ErrorReadingInputFile => "Error reading input file",
                ParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile => "Error creating parsed proteins output file",
                ParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile => "Error creating digested proteins output file",
                ParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile => "Error creating scrambled proteins output file",
                ParseProteinFileErrorCodes.ErrorWritingOutputFile => "Error writing to one of the output files",
                ParseProteinFileErrorCodes.ErrorInitializingObjectVariables => "Error initializing In Silico Digester class",
                ParseProteinFileErrorCodes.DigestProteinSequenceError => "Error in DigestProteinSequence function",
                ParseProteinFileErrorCodes.UnspecifiedError => "Unspecified localized error",
                _ => "Unknown error state", // This shouldn't happen
            };

            return MostRecentException == null
                ? errorMessage
                : string.Format("{0}: \n{1}", errorMessage, MostRecentException.Message);
        }

        public int GetProteinCountCached()
        {
            return mProteins.Count;
        }

        public ProteinInfo GetCachedProtein(int index)
        {
            if (index < mProteins.Count)
            {
                return mProteins[index];
            }

            return default;
        }

        /// <summary>
        /// Get the digested peptides for the given protein (by protein index)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="digestedPeptides"></param>
        /// <returns>The number of peptides in digestedPeptides</returns>
        public int GetDigestedPeptidesForCachedProtein(
            int index,
            out List<PeptideSequenceWithNET> digestedPeptides)
        {
            if (index < mProteins.Count)
            {
                return DigestProteinSequence(mProteins[index].Sequence, out digestedPeptides, mProteins[index].Name);
            }

            digestedPeptides = new List<PeptideSequenceWithNET>();
            return 0;
        }

        private void InitializeLocalVariables()
        {
            LocalErrorCode = ParseProteinFileErrorCodes.NoError;

            InputFileProteinsProcessed = 0;
            InputFileLinesRead = 0;
            InputFileLineSkipCount = 0;

            ProcessingSummary = string.Empty;

            mFileNameAbbreviated = string.Empty;

            ProcessingSummary = string.Empty;

            mFileNameAbbreviated = string.Empty;
        }

        // ReSharper disable once GrammarMistakeInComment

        /// <summary>
        /// Return true if all of the characters in sequence are A, T, C, or G
        /// </summary>
        /// <param name="sequence"></param>
        private bool IsLikelyDNA(string sequence)
        {
            var aminoAcidCount = 0;
            var dnaCount = 0;

            foreach (var character in sequence.Where(char.IsLetter))
            {
                if (character is 'A' or 'T' or 'C' or 'G')
                {
                    dnaCount++;
                }
                else
                {
                    aminoAcidCount++;
                }
            }

            return dnaCount > 0 && aminoAcidCount == 0;
        }

        /// <summary>
        /// Examines the file's extension and returns true if it ends in .fasta, .fasta.gz, .faa, or .faa.gz
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="notifyErrorsWithMessageBox"></param>
        public static bool IsFastaFile(string filePath, bool notifyErrorsWithMessageBox = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                var extensionToCheck = Path.GetExtension(StripExtension(filePath, ".gz"));

                return extensionToCheck.Equals(".fasta", StringComparison.OrdinalIgnoreCase) ||
                       extensionToCheck.Equals(".faa", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                if (notifyErrorsWithMessageBox)
                {
                    MessageBox.Show("Error looking for suffix .fasta or .fasta.gz: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    ConsoleMsgUtils.ShowError("Error in IsFastaFile: " + ex.Message);
                }

                return false;
            }
        }

        private bool InitializeScrambledOutput(
            FilePathInfo pathInfo,
            ScramblingResidueCache residueCache,
            ProteinScramblingModeConstants scramblingMode,
            out FileInfo scrambledSequenceFastaFile,
            out StreamWriter scrambledFileWriter,
            out Random randomNumberGenerator)
        {
            bool success;

            // Wait to allow the timer to advance.
            Thread.Sleep(1);
            var randomNumberSeed = Environment.TickCount;

            randomNumberGenerator = new Random(randomNumberSeed);

            var scrambleModeName = scramblingMode switch
            {
                ProteinScramblingModeConstants.Reversed => "reversed",
                ProteinScramblingModeConstants.Decoy => "decoy",
                ProteinScramblingModeConstants.Randomized => "scrambled",
                _ => throw new InvalidEnumArgumentException(nameof(scramblingMode))
            };

            var suffix = "_" + scrambleModeName;

            if (scramblingMode == ProteinScramblingModeConstants.Reversed)
            {
                // Reversed FASTA file, suffix _reversed
                if (residueCache.SamplingPercentage < 100)
                {
                    suffix += "_" + residueCache.SamplingPercentage + "pct";
                }
            }
            else if (scramblingMode == ProteinScramblingModeConstants.Decoy)
            {
                // Decoy FASTA file, suffix _decoy
                if (residueCache.SamplingPercentage < 100)
                {
                    suffix += "_" + residueCache.SamplingPercentage + "pct";
                }
            }
            else
            {
                // Scrambled FASTA file, with suffix _scrambled_seed
                suffix += "_seed" + randomNumberSeed;
                if (residueCache.SamplingPercentage < 100)
                {
                    suffix += "_" + residueCache.SamplingPercentage + "pct";
                }
            }

            var scrambledFastaOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) + suffix + ".fasta");

            // Define the abbreviated name of the input file; used in the protein names
            mFileNameAbbreviated = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) ?? "InputFile";

            if (mFileNameAbbreviated.Length > MAX_ABBREVIATED_FILENAME_LENGTH)
            {
                mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, MAX_ABBREVIATED_FILENAME_LENGTH);

                if (mFileNameAbbreviated.Substring(mFileNameAbbreviated.Length - 1, 1) == "_")
                {
                    mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.Length - 1);
                }
                else if (mFileNameAbbreviated.LastIndexOf('-') > MAX_ABBREVIATED_FILENAME_LENGTH / 3d)
                {
                    mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.LastIndexOf('-'));
                }
                else if (mFileNameAbbreviated.LastIndexOf('_') > MAX_ABBREVIATED_FILENAME_LENGTH / 3d)
                {
                    mFileNameAbbreviated = mFileNameAbbreviated.Substring(0, mFileNameAbbreviated.LastIndexOf('_'));
                }
            }

            // Make sure there aren't any spaces in the abbreviated filename
            mFileNameAbbreviated = mFileNameAbbreviated.Replace(" ", "_");

            scrambledSequenceFastaFile = new FileInfo(scrambledFastaOutputFilePath);

            try
            {
                // Create the scrambled protein output FASTA file
                scrambledFileWriter = new StreamWriter(new FileStream(scrambledSequenceFastaFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read));
                success = true;
            }
            catch (Exception ex)
            {
                OnWarningEvent("Error creating {0} output FASTA file {1}: {2}", scrambleModeName, scrambledSequenceFastaFile.FullName, ex.Message);
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile, ex);
                success = false;
                scrambledFileWriter = null;
            }

            return success;
        }

        /// <summary>
        /// Checks headerLine to see if it starts with the given column names, separated by tabs
        /// </summary>
        /// <param name="headerLine"></param>
        /// <param name="columnNames"></param>
        /// <returns>True if the given header names are found, otherwise false</returns>
        private bool MatchesOrStartsWith(string headerLine, params string[] columnNames)
        {
            var tabDelimitedHeaders = string.Join("\t", columnNames);

            return headerLine.Equals(tabDelimitedHeaders, StringComparison.OrdinalIgnoreCase) ||
                   headerLine.StartsWith(tabDelimitedHeaders + "\t", StringComparison.CurrentCultureIgnoreCase);
        }

        public bool ParseProteinFile(string proteinInputFilePath, string outputFolderPath)
        {
            return ParseProteinFile(proteinInputFilePath, outputFolderPath, string.Empty);
        }

        /// <summary>
        /// Process the protein FASTA file or tab-delimited text file
        /// </summary>
        /// <param name="proteinInputFilePath"></param>
        /// <param name="outputFolderPath"></param>
        /// <param name="outputFileNameBaseOverride">Name for the protein output filename (auto-defined if empty)</param>
        public bool ParseProteinFile(
            string proteinInputFilePath,
            string outputFolderPath,
            string outputFileNameBaseOverride)
        {
            MostRecentException = null;
            ProteinFileReaderBaseClass proteinFileReader = null;

            StreamWriter proteinFileWriter = null;
            StreamWriter digestFileWriter = null;
            StreamWriter scrambledFileWriter = null;

            bool success;
            var headerChecked = false;

            var lookForAddnlRefInDescription = false;

            var addnlRefsToOutput = new List<AddnlRef>();

            var generateUniqueSequenceID = false;

            Random randomNumberGenerator = null;

            var residueCache = new ScramblingResidueCache(ProcessingOptions);

            ProcessingSummary = string.Empty;

            var pathInfo = new FilePathInfo
            {
                ProteinInputFilePath = proteinInputFilePath,
                OutputFolderPath = outputFolderPath,
                OutputFileNameBaseOverride = outputFileNameBaseOverride,
                ProteinOutputFilePath = string.Empty,
                DigestedProteinOutputFilePath = string.Empty
            };

            try
            {
                if (string.IsNullOrWhiteSpace(pathInfo.ProteinInputFilePath))
                {
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath);
                    return false;
                }

                success = ParseProteinFileGetReaderAndDefineOutputFile(pathInfo, out proteinFileReader);
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile, ex);
                success = false;
            }

            // Abort processing if we couldn't successfully open the input file
            if (!success)
            {
                return false;
            }

            try
            {
                bool createProteinOutputFile;

                if (ProcessingOptions.ProteinScramblingMode == ProteinScramblingModeConstants.Decoy)
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (ParsedFileIsFastaFile)
                    {
                        // Since the input file is a FASTA file, and we're creating a decoy output file, do not create a new forward-sequence protein output file
                        createProteinOutputFile = false;
                    }
                    else
                    {
                        createProteinOutputFile = true;
                    }
                }
                else
                {
                    createProteinOutputFile = ProcessingOptions.CreateProteinOutputFile;
                }

                if (createProteinOutputFile)
                {
                    try
                    {
                        // Open the protein output file (if required)
                        // This will cause an error if the input file is the same as the output file
                        proteinFileWriter = new StreamWriter(new FileStream(pathInfo.ProteinOutputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read));
                    }
                    catch (Exception ex)
                    {
                        OnWarningEvent("Error creating protein output file {0}: {1}", pathInfo.ProteinOutputFilePath, ex.Message);
                        SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile, ex);
                        success = false;
                    }

                    if (!success)
                    {
                        return false;
                    }
                }

                if (mMasterSequencesDictionary == null)
                {
                    mMasterSequencesDictionary = new Dictionary<string, int>();
                }
                else
                {
                    mMasterSequencesDictionary.Clear();
                }

                mNextUniqueIDForMasterSeqs = 1;
                SequenceCountLikelyDNA = 0;
                var totalFragmentCount = 0;

                var outputFileDelimiter = ProcessingOptions.OutputFileDelimiter;

                if (createProteinOutputFile &&
                    ProcessingOptions.CreateDigestedProteinOutputFile &&
                    !ProcessingOptions.CreateFastaOutputFile)
                {
                    success = ParseProteinFileCreateDigestedProteinOutputFile(pathInfo.DigestedProteinOutputFilePath, ref digestFileWriter, outputFileDelimiter);
                    if (!success)
                    {
                        return false;
                    }

                    if (ProcessingOptions.GenerateUniqueIDValuesForPeptides)
                    {
                        // Initialize mMasterSequencesDictionary
                        generateUniqueSequenceID = true;
                    }
                }

                var loopCount = 1;
                bool allowLookForAddnlRefInDescription;
                if (ProcessingOptions.ProteinScramblingMode == ProteinScramblingModeConstants.Randomized)
                {
                    loopCount = ProcessingOptions.ProteinScramblingLoopCount;
                    if (loopCount < 1)
                    {
                        loopCount = 1;
                    }

                    if (loopCount > 10000)
                    {
                        loopCount = 10000;
                    }

                    allowLookForAddnlRefInDescription = false;
                }
                else
                {
                    allowLookForAddnlRefInDescription = ProcessingOptions.FastaFileOptions.LookForAddnlRefInDescription;
                }

                var outLine = new StringBuilder();

                for (var loopIndex = 1; loopIndex <= loopCount; loopIndex++)
                {
                    // Attempt to open the input file
                    if (!proteinFileReader.OpenFile(pathInfo.ProteinInputFilePath))
                    {
                        SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile);
                        break;
                    }

                    ProteinScramblingModeConstants scramblingMode;
                    FileInfo scrambledSequenceFastaFile;

                    if (createProteinOutputFile || ProcessingOptions.ProteinScramblingMode != ProteinScramblingModeConstants.None)
                    {
                        scramblingMode = ProcessingOptions.ProteinScramblingMode;

                        residueCache.Cache = string.Empty;
                        residueCache.CacheLength = SCRAMBLING_CACHE_LENGTH;
                        residueCache.OutputCount = 0;
                        residueCache.ResiduesToWrite = string.Empty;

                        if (scramblingMode != ProteinScramblingModeConstants.None)
                        {
                            success = InitializeScrambledOutput(
                                pathInfo, residueCache, scramblingMode,
                                out scrambledSequenceFastaFile,
                                out scrambledFileWriter,
                                out randomNumberGenerator);

                            if (!success)
                            {
                                break;
                            }
                        }
                        else
                        {
                            scrambledSequenceFastaFile = null;
                        }
                    }
                    else
                    {
                        scramblingMode = ProteinScramblingModeConstants.None;
                        scrambledSequenceFastaFile = null;
                    }

                    addnlRefsToOutput.Clear();

                    if (createProteinOutputFile && ParsedFileIsFastaFile && allowLookForAddnlRefInDescription)
                    {
                        // ReSharper disable once GrammarMistakeInComment

                        // Need to pre-scan the FASTA file to find all of the possible additional reference values

                        var addnlRefMasterNames = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
                        PreScanProteinFileForAddnlRefsInDescription(pathInfo.ProteinInputFilePath, addnlRefMasterNames);

                        if (addnlRefMasterNames.Count > 0)
                        {
                            // Need to extract out the key names from addnlRefMasterNames and sort them alphabetically
                            lookForAddnlRefInDescription = true;

                            var index = 0;
                            foreach (var addnlRef in addnlRefMasterNames)
                            {
                                addnlRefsToOutput[index].RefName = addnlRef;
                                index++;
                            }
                        }
                        else
                        {
                            lookForAddnlRefInDescription = false;
                        }
                    }

                    ResetProgress("Parsing protein input file");

                    if (createProteinOutputFile && !ProcessingOptions.CreateFastaOutputFile)
                    {
                        outLine.Clear();

                        // Write the header line to the output file
                        outLine.AppendFormat("{0}{1}", "ProteinName", outputFileDelimiter);

                        if (lookForAddnlRefInDescription)
                        {
                            foreach (var addnlRef in (from item in addnlRefsToOutput orderby item.RefName select item))
                            {
                                outLine.AppendFormat("{0}{1}", addnlRef.RefName, outputFileDelimiter);
                            }
                        }

                        // Include Description in the header line, even if we are excluding the description for all proteins
                        outLine.Append("Description");

                        if (ProcessingOptions.ComputeSequenceHashValues)
                        {
                            outLine.AppendFormat("{0}{1}", outputFileDelimiter, "SequenceHash");
                        }

                        if (!ProcessingOptions.ExcludeProteinSequence)
                        {
                            outLine.AppendFormat("{0}{1}", outputFileDelimiter, "Sequence");
                        }

                        if (ProcessingOptions.ComputeProteinMass)
                        {
                            if (ProcessingOptions.ElementMassMode == PeptideSequence.ElementModeConstants.AverageMass)
                            {
                                outLine.AppendFormat("{0}{1}", outputFileDelimiter, "Average Mass");
                            }
                            else
                            {
                                outLine.AppendFormat("{0}{1}", outputFileDelimiter, "Mass");
                            }
                        }

                        if (ProcessingOptions.ComputepI)
                        {
                            outLine.AppendFormat("{0}{1}{0}{2}", outputFileDelimiter, "pI", "Hydrophobicity");
                        }

                        if (ProcessingOptions.ComputeNET)
                        {
                            outLine.AppendFormat("{0}{1}", outputFileDelimiter, "LC_NET");
                        }

                        if (ProcessingOptions.ComputeSCXNET)
                        {
                            outLine.AppendFormat("{0}{1}", outputFileDelimiter, "SCX_NET");
                        }

                        proteinFileWriter.WriteLine(outLine.ToString());
                    }

                    // Read each protein in the input file and process appropriately
                    mProteins.Clear();
                    mProteins.Capacity = 501;

                    InputFileProteinsProcessed = 0;
                    InputFileLineSkipCount = 0;
                    InputFileLinesRead = 0;

                    do
                    {
                        var inputProteinFound = proteinFileReader.ReadNextProteinEntry();
                        InputFileLineSkipCount += proteinFileReader.LineSkipCount;

                        if (!inputProteinFound)
                        {
                            break;
                        }

                        if (!headerChecked && !ParsedFileIsFastaFile)
                        {
                            headerChecked = true;

                            // This may be a header line; possibly skip it
                            if (proteinFileReader.ProteinName.StartsWith("protein", StringComparison.OrdinalIgnoreCase))
                            {
                                if (proteinFileReader.ProteinDescription.IndexOf("description", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                    proteinFileReader.ProteinSequence.IndexOf("sequence", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    // Skip this entry since it's a header line, for example:
                                    // ProteinName    Description    Sequence
                                    continue;
                                }
                            }

                            if (proteinFileReader.ProteinName.IndexOf("protein", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                FractionLowercase(proteinFileReader.ProteinSequence) > 0.2d)
                            {
                                // Skip this entry since it's a header line, for example:
                                // FirstProtein    ProteinDesc    Sequence
                                continue;
                            }
                        }

                        InputFileProteinsProcessed++;
                        InputFileLinesRead = proteinFileReader.LinesRead;

                        var protein = ParseProteinFileStoreProtein(proteinFileReader, lookForAddnlRefInDescription);

                        if (loopIndex == 1 && IsLikelyDNA(protein.Sequence))
                        {
                            OnWarningEvent("Protein {0} appears to be a DNA sequence; this program is designed for amino acid based FASTA files", protein.Name);
                            SequenceCountLikelyDNA++;
                        }

                        if (createProteinOutputFile || scramblingMode != ProteinScramblingModeConstants.None)
                        {
                            if (createProteinOutputFile && loopIndex == 1)
                            {
                                if (ProcessingOptions.CreateFastaOutputFile)
                                {
                                    ParseProteinFileWriteFasta(proteinFileWriter, protein, outLine);
                                }
                                else
                                {
                                    ParseProteinFileWriteTextDelimited(proteinFileWriter, outputFileDelimiter, protein, outLine, lookForAddnlRefInDescription, addnlRefsToOutput);
                                }
                            }

                            if (loopIndex == 1 && digestFileWriter != null)
                            {
                                var peptideFragmentCount = ParseProteinFileWriteDigested(digestFileWriter, outputFileDelimiter, protein, outLine, generateUniqueSequenceID);

                                totalFragmentCount += peptideFragmentCount;
                            }

                            if (scramblingMode != ProteinScramblingModeConstants.None)
                            {
                                WriteScrambledFasta(scrambledFileWriter, randomNumberGenerator, protein, scramblingMode, residueCache);
                            }
                        }
                        else
                        {
                            // Cache the proteins in memory
                            mProteins.Add(protein);
                        }

                        var percentProcessed = (loopIndex - 1) / (float)loopCount * 100.0f + proteinFileReader.PercentFileProcessed() / loopCount;
                        UpdateProgress(percentProcessed);
                    }
                    while (!AbortProcessing);

                    if (createProteinOutputFile && scramblingMode != ProteinScramblingModeConstants.None)
                    {
                        // Write out anything remaining in the cache

                        string proteinNamePrefix;
                        if (scramblingMode is ProteinScramblingModeConstants.Reversed or ProteinScramblingModeConstants.Decoy)
                        {
                            proteinNamePrefix = PROTEIN_PREFIX_REVERSED;
                        }
                        else
                        {
                            proteinNamePrefix = PROTEIN_PREFIX_SCRAMBLED;
                        }

                        WriteFastaAppendToCache(scrambledFileWriter, residueCache, proteinNamePrefix, true);
                    }

                    if (mProteins.Count > 0)
                    {
                        mProteins.Capacity = mProteins.Count;
                    }

                    proteinFileReader.CloseFile();

                    proteinFileWriter?.Close();

                    if (loopIndex == 1 && digestFileWriter != null)
                    {
                        if (totalFragmentCount == 0 && SequenceCountLikelyDNA > 0)
                        {
                            digestFileWriter.WriteLine(
                                "Warning:{0}Proteins in the input file are likely DNA sequences; " +
                                "this program is designed for amino acid based proteins",
                                outputFileDelimiter);
                        }

                        digestFileWriter.Close();
                    }

                    scrambledFileWriter?.Close();

                    if (scramblingMode == ProteinScramblingModeConstants.Decoy)
                    {
                        // Create the decoy .fasta file by combining the original proteins with the reversed proteins
                        if (ParsedFileIsFastaFile)
                        {
                            CreateDecoyFastaFile(pathInfo.ProteinInputFilePath, scrambledSequenceFastaFile);
                        }
                        else
                        {
                            CreateDecoyFastaFile(pathInfo.ProteinOutputFilePath, scrambledSequenceFastaFile);
                        }
                    }
                }

                var message = new StringBuilder();

                var errorMessage = GetErrorMessage();

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    message.Append(errorMessage);
                    message.AppendLine();
                }
                else
                {
                    message.AppendFormat("Done: Processed {0:###,##0} {1} ({2:###,###,##0} lines)",
                        InputFileProteinsProcessed,
                        InputFileProteinsProcessed == 1 ? "protein" : "proteins",
                        InputFileLinesRead);
                }

                if (InputFileLineSkipCount > 0)
                {
                    message.AppendLine();
                    message.AppendFormat("Note that {0:###,##0} {1} skipped in the input file due to having an unexpected format.",
                        InputFileLineSkipCount,
                        InputFileLineSkipCount == 1 ? "line was" : "lines were");

                    if (ParsedFileIsFastaFile)
                    {
                        message.Append(" This is an unexpected error for FASTA files.");
                    }
                    else
                    {
                        message.AppendFormat(
                            " Make sure that {0} is the appropriate format for this file " +
                            "(see the File Format Options tab in the GUI or command line argument DelimitedFileFormat).",
                            ProcessingOptions.DelimitedFileFormatCode);

                        if (ProcessingOptions.InputFileDelimiter == '\t')
                            message.Append(" Also confirm that the file is tab-delimited.");
                        else
                            message.AppendFormat(" Also confirm that data in the file is delimited with a {0}", ProcessingOptions.InputFileDelimiter);
                    }
                }

                if (SequenceCountLikelyDNA > 0)
                {
                    message.AppendLine();
                    message.AppendLine();

                    if (SequenceCountLikelyDNA == 1)
                        message.Append("The protein in the input file appears to be a DNA sequence");
                    else
                        message.AppendFormat("{0} proteins in the input file appear to be DNA sequences", SequenceCountLikelyDNA);

                    message.Append("; this program is designed for amino acid based proteins");
                }

                if (loopCount > 1)
                {
                    message.AppendLine();
                    message.AppendFormat("Created {0} replicates of the scrambled output file", loopCount);
                }

                ProcessingSummary = message.ToString();
                Console.WriteLine();
                OnStatusEvent(message.ToString());

                success = true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error in ParseProteinFile: " + ex.Message);
                if (ProcessingOptions.CreateProteinOutputFile || ProcessingOptions.CreateDigestedProteinOutputFile || ProcessingOptions.ProteinScramblingMode != ProteinScramblingModeConstants.None)
                {
                    SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorWritingOutputFile, ex);
                    success = false;
                }
                else
                {
                    SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile, ex);
                    success = false;
                }
            }
            finally
            {
                proteinFileWriter?.Close();

                digestFileWriter?.Close();

                // ReSharper disable once ConstantConditionalAccessQualifier
                scrambledFileWriter?.Close();
            }

            return success;
        }

        private ProteinInfo ParseProteinFileStoreProtein(
            ProteinFileReaderBaseClass proteinFileReader,
            bool lookForAddnlRefInDescription)
        {
            var protein = new ProteinInfo
            {
                Name = proteinFileReader.ProteinName,
                Description = proteinFileReader.ProteinDescription
            };

            if (ProcessingOptions.TruncateProteinDescription && protein.Description.Length > MAX_PROTEIN_DESCRIPTION_LENGTH)
            {
                protein.Description = protein.Description.Substring(0, MAX_PROTEIN_DESCRIPTION_LENGTH - 3) + "...";
            }

            if (lookForAddnlRefInDescription)
            {
                // Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
                ExtractAlternateProteinNamesFromDescription(protein);
            }
            else
            {
                protein.AlternateNames.Clear();
            }

            var sequence = proteinFileReader.ProteinSequence;
            protein.Sequence = sequence;

            if (ProcessingOptions.ComputeSequenceHashIgnoreILDiff)
            {
                protein.SequenceHash = HashUtilities.ComputeStringHashSha1(sequence.Replace('L', 'I')).ToUpper();
            }
            else
            {
                protein.SequenceHash = HashUtilities.ComputeStringHashSha1(sequence).ToUpper();
            }

            if (ProcessingOptions.ComputeProteinMass)
            {
                protein.Mass = ComputeSequenceMass(sequence);
            }
            else
            {
                protein.Mass = 0d;
            }

            if (ProcessingOptions.ComputepI)
            {
                protein.pI = ComputeSequencepI(sequence);
                protein.Hydrophobicity = ComputeSequenceHydrophobicity(sequence);
            }
            else
            {
                protein.pI = 0f;
                protein.Hydrophobicity = 0f;
            }

            if (ProcessingOptions.ComputeNET)
            {
                protein.ProteinNET = ComputeSequenceNET(sequence);
            }

            if (ProcessingOptions.ComputeSCXNET)
            {
                protein.ProteinSCXNET = ComputeSequenceSCXNET(sequence);
            }

            return protein;
        }

        /// <summary>
        /// Digest the protein sequence and append the fragments to the writer
        /// </summary>
        /// <param name="digestFileWriter"></param>
        /// <param name="outputFileDelimiter"></param>
        /// <param name="protein"></param>
        /// <param name="outLine"></param>
        /// <param name="generateUniqueSequenceID"></param>
        /// <returns>The number of digested peptides written</returns>
        private int ParseProteinFileWriteDigested(
            TextWriter digestFileWriter,
            char outputFileDelimiter,
            ProteinInfo protein,
            StringBuilder outLine,
            bool generateUniqueSequenceID)
        {
            var peptideFragmentCount = DigestProteinSequence(protein.Sequence, out var peptideFragments, protein.Name);

            foreach (var peptideFragment in peptideFragments)
            {
                int uniqueSeqID;

                if (generateUniqueSequenceID)
                {
                    try
                    {
                        if (mMasterSequencesDictionary.TryGetValue(peptideFragment.SequenceOneLetter, out var sequenceID))
                        {
                            uniqueSeqID = sequenceID;
                        }
                        else
                        {
                            mMasterSequencesDictionary.Add(peptideFragment.SequenceOneLetter, mNextUniqueIDForMasterSeqs);
                            uniqueSeqID = mNextUniqueIDForMasterSeqs;
                        }

                        mNextUniqueIDForMasterSeqs++;
                    }
                    catch
                    {
                        uniqueSeqID = 0;
                    }
                }
                else
                {
                    uniqueSeqID = 0;
                }

                var baseSequence = peptideFragment.SequenceOneLetter;
                outLine.Clear();

                if (!ProcessingOptions.ExcludeProteinSequence)
                {
                    outLine.AppendFormat("{0}{1}", protein.Name, outputFileDelimiter);

                    if (ProcessingOptions.DigestionOptions.IncludePrefixAndSuffixResidues)
                    {
                        outLine.AppendFormat("{0}.{1}.{2}{3}", peptideFragment.PrefixResidue, baseSequence, peptideFragment.SuffixResidue, outputFileDelimiter);
                    }
                    else
                    {
                        outLine.AppendFormat("{0}{1}", baseSequence, outputFileDelimiter);
                    }
                }

                outLine.AppendFormat("{1}{0}{2}{0}{3}{0}{4}",
                    outputFileDelimiter, uniqueSeqID.ToString(), peptideFragment.Mass, Math.Round(peptideFragment.NET, 4), peptideFragment.PeptideName);

                if (ProcessingOptions.ComputepI)
                {
                    var pI = ComputeSequencepI(baseSequence);
                    var hydrophobicity = ComputeSequenceHydrophobicity(baseSequence);

                    outLine.AppendFormat("{0}{1:0.000}{0}{2:0.0000}", outputFileDelimiter, pI, hydrophobicity);
                }

                if (ProcessingOptions.ComputeNET)
                {
                    var lcNET = ComputeSequenceNET(baseSequence);

                    outLine.AppendFormat("{0}{1:0.0000}", outputFileDelimiter, lcNET);
                }

                if (ProcessingOptions.ComputeSCXNET)
                {
                    var scxNET = ComputeSequenceSCXNET(baseSequence);

                    outLine.AppendFormat("{0}{1:0.0000}", outputFileDelimiter, scxNET);
                }

                digestFileWriter.WriteLine(outLine.ToString());
            }

            return peptideFragmentCount;
        }

        private void ParseProteinFileWriteFasta(TextWriter proteinFileWriter, ProteinInfo protein, StringBuilder outLine)
        {
            // Write the entry to the output FASTA file

            if (protein.Name == "ProteinName" &&
                protein.Description == "Description" &&
                protein.Sequence == "Sequence")
            {
                // Skip this entry; it's an artifact from converting from a FASTA file to a text file, then back to a FASTA file
                return;
            }

            outLine.Clear();
            outLine.AppendFormat("{0}{1}", FastaFileReader.PROTEIN_LINE_START_CHAR, protein.Name);
            if (!ProcessingOptions.ExcludeProteinDescription)
            {
                outLine.AppendFormat("{0}{1}", FastaFileReader.PROTEIN_LINE_ACCESSION_TERMINATOR, protein.Description);
            }

            proteinFileWriter.WriteLine(outLine.ToString());

            if (!ProcessingOptions.ExcludeProteinSequence)
            {
                var index = 0;
                while (index < protein.Sequence.Length)
                {
                    var length = Math.Min(60, protein.Sequence.Length - index);
                    proteinFileWriter.WriteLine(protein.Sequence.Substring(index, length));
                    index += 60;
                }
            }
        }

        private void ParseProteinFileWriteTextDelimited(
            TextWriter proteinFileWriter,
            char outputFileDelimiter,
            ProteinInfo protein,
            StringBuilder outLine,
            bool lookForAddnlRefInDescription,
            List<AddnlRef> addnlRefsToOutput)
        {
            // Write the entry to the protein output file, and possibly digest it

            outLine.Clear();

            if (lookForAddnlRefInDescription)
            {
                // Reset the Accession numbers in addnlRefsToOutput
                foreach (var addnlRef in addnlRefsToOutput)
                {
                    addnlRef.RefAccession = string.Empty;
                }

                // Update the accession numbers in addnlRefsToOutput
                for (var index = 0; index < protein.AlternateNameCount; index++)
                {
                    foreach (var addnlRef in addnlRefsToOutput)
                    {
                        if (string.Equals(addnlRef.RefName, protein.AlternateNames[index].RefName, StringComparison.OrdinalIgnoreCase))
                        {
                            addnlRef.RefAccession = protein.AlternateNames[index].RefAccession;
                            break;
                        }
                    }
                }

                outLine.AppendFormat("{0}{1}", protein.Name, outputFileDelimiter);
                foreach (var addnlRef in (from item in addnlRefsToOutput orderby item.RefName select item))
                {
                    outLine.AppendFormat("{0}{1}", addnlRef.RefAccession, outputFileDelimiter);
                }

                if (!ProcessingOptions.ExcludeProteinDescription)
                {
                    outLine.Append(protein.Description);
                }
            }
            else
            {
                outLine.AppendFormat("{0}{1}", protein.Name, outputFileDelimiter);
                if (!ProcessingOptions.ExcludeProteinDescription)
                {
                    outLine.Append(protein.Description);
                }
            }

            if (ProcessingOptions.ComputeSequenceHashValues)
            {
                outLine.AppendFormat("{0}{1}", outputFileDelimiter, protein.SequenceHash);
            }

            if (!ProcessingOptions.ExcludeProteinSequence)
            {
                outLine.AppendFormat("{0}{1}", outputFileDelimiter, protein.Sequence);
            }

            if (ProcessingOptions.ComputeProteinMass)
            {
                outLine.AppendFormat("{0}{1}", outputFileDelimiter, Math.Round(protein.Mass, 5).ToString(CultureInfo.InvariantCulture));
            }

            if (ProcessingOptions.ComputepI)
            {
                outLine.AppendFormat("{0}{1:0.000}{0}{2:0.0000}", outputFileDelimiter, protein.pI, protein.Hydrophobicity);
            }

            if (ProcessingOptions.ComputeNET)
            {
                outLine.AppendFormat("{0}{1:0.0000}", outputFileDelimiter, protein.ProteinNET);
            }

            if (ProcessingOptions.ComputeSCXNET)
            {
                outLine.AppendFormat("{0}{1:0.0000}", outputFileDelimiter, protein.ProteinSCXNET);
            }

            proteinFileWriter.WriteLine(outLine.ToString());
        }

        private bool ParseProteinFileGetReaderAndDefineOutputFile(
            FilePathInfo pathInfo,
            out ProteinFileReaderBaseClass proteinFileReader)
        {
            if (ProcessingOptions.AssumeFastaFile)
            {
                ParsedFileIsFastaFile = true;
            }
            else if (ProcessingOptions.AssumeDelimitedFile)
            {
                ParsedFileIsFastaFile = false;
            }
            else
            {
                ParsedFileIsFastaFile = IsFastaFile(pathInfo.ProteinInputFilePath);
            }

            if (ProcessingOptions.CreateDigestedProteinOutputFile)
            {
                // Make sure CreateFastaOutputFile is false
                ProcessingOptions.CreateFastaOutputFile = false;
            }
            else if (!ParsedFileIsFastaFile && ProcessingOptions.ProteinScramblingMode == ProteinScramblingModeConstants.Decoy)
            {
                // Force the output file format to be a FASTA file
                ProcessingOptions.CreateFastaOutputFile = true;
            }

            string outputFilePath;

            if (!string.IsNullOrEmpty(pathInfo.OutputFileNameBaseOverride))
            {
                outputFilePath = pathInfo.OutputFileNameBaseOverride;

                if (Path.HasExtension(pathInfo.OutputFileNameBaseOverride))
                {
                    var fileExtension = Path.GetExtension(pathInfo.OutputFileNameBaseOverride);

                    if (ProcessingOptions.CreateFastaOutputFile)
                    {
                        if (fileExtension.Equals(".fasta", StringComparison.OrdinalIgnoreCase))
                        {
                            outputFilePath = pathInfo.OutputFileNameBaseOverride + ".fasta";
                        }
                    }
                    else if (!fileExtension.Equals(".txt") && !fileExtension.Equals(".tsv"))
                    {
                        outputFilePath = pathInfo.OutputFileNameBaseOverride + ".txt";
                    }
                }
                else if (ProcessingOptions.CreateFastaOutputFile)
                {
                    outputFilePath = pathInfo.OutputFileNameBaseOverride + ".fasta";
                }
                else
                {
                    outputFilePath = pathInfo.OutputFileNameBaseOverride + ".txt";
                }
            }
            else
            {
                outputFilePath = string.Empty;
            }

            if (ParsedFileIsFastaFile)
            {
                proteinFileReader = new FastaFileReader();
            }
            else
            {
                proteinFileReader = new DelimitedProteinFileReader
                {
                    Delimiter = ProcessingOptions.InputFileDelimiter,
                    DelimitedFileFormatCode = ProcessingOptions.DelimitedFileFormatCode,
                    SkipFirstLine = ProcessingOptions.DelimitedFileHasHeaderLine
                };
            }

            // Verify that the input file exists
            if (!File.Exists(pathInfo.ProteinInputFilePath))
            {
                SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath);

                return false;
            }

            if (ParsedFileIsFastaFile)
            {
                if (outputFilePath.Length == 0)
                {
                    outputFilePath = Path.GetFileName(pathInfo.ProteinInputFilePath);

                    var fileExtension = Path.GetExtension(outputFilePath);

                    if (fileExtension.Equals(".fasta", StringComparison.OrdinalIgnoreCase) ||
                        fileExtension.Equals(".faa", StringComparison.OrdinalIgnoreCase))
                    {
                        // Nothing special to do; will replace the extension below
                    }
                    else if (outputFilePath.EndsWith(".fasta.gz", StringComparison.OrdinalIgnoreCase) ||
                             outputFilePath.EndsWith(".faa.gz", StringComparison.OrdinalIgnoreCase))
                    {
                        // Remove .gz from outputFilePath
                        outputFilePath = StripExtension(outputFilePath, ".gz");
                    }
                    else
                    {
                        // .Fasta appears somewhere in the middle
                        // Remove the text .Fasta, then add the extension .txt (unless it already ends in .txt)
                        var charIndex = outputFilePath.ToLower().LastIndexOf(".fasta", StringComparison.Ordinal);
                        if (charIndex > 0)
                        {
                            if (charIndex < outputFilePath.Length)
                            {
                                outputFilePath = outputFilePath.Substring(0, charIndex) + outputFilePath.Substring(charIndex + 6);
                            }
                            else
                            {
                                outputFilePath = outputFilePath.Substring(0, charIndex);
                            }
                        }
                    }

                    if (ProcessingOptions.CreateFastaOutputFile)
                    {
                        outputFilePath = Path.GetFileNameWithoutExtension(outputFilePath) + "_new.fasta";
                    }
                    else
                    {
                        outputFilePath = Path.ChangeExtension(outputFilePath, ".txt");
                    }
                }
            }
            else if (outputFilePath.Length == 0)
            {
                if (ProcessingOptions.CreateFastaOutputFile)
                {
                    outputFilePath = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) + ".fasta";
                }
                else
                {
                    outputFilePath = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) + "_parsed.txt";
                }
            }

            if (proteinFileReader is DelimitedProteinFileReader { Delimiter: '\t' } delimitedReader && !string.IsNullOrWhiteSpace(pathInfo.ProteinInputFilePath))
            {
                // Read the first line of the input file and auto-update DelimitedFileFormatCode if we find known headers
                using var reader = new StreamReader(new FileStream(pathInfo.ProteinInputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                if (!reader.EndOfStream)
                {
                    var headerLine = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(headerLine))
                    {
                        DelimitedProteinFileReader.ProteinFileFormatCode fileFormat;

                        if (MatchesOrStartsWith(headerLine, "ProteinName", "Sequence") ||
                            MatchesOrStartsWith(headerLine, "Protein", "Sequence"))
                        {
                            fileFormat = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Sequence;
                        }
                        else if (MatchesOrStartsWith(headerLine, "ProteinName", "Description", "Sequence") ||
                                 MatchesOrStartsWith(headerLine, "Protein", "Description", "Sequence"))
                        {
                            fileFormat = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence;
                        }
                        else if (MatchesOrStartsWith(headerLine, "ProteinName", "Description", "SequenceHash", "Sequence") ||
                                 MatchesOrStartsWith(headerLine, "Protein", "Description", "SequenceHash", "Sequence"))
                        {
                            fileFormat = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Hash_Sequence;
                        }
                        else
                        {
                            fileFormat = delimitedReader.DelimitedFileFormatCode;
                        }

                        if (delimitedReader.DelimitedFileFormatCode != fileFormat)
                        {
                            OnWarningEvent("Auto changing the delimited protein file format to {0} based on the actual header line", fileFormat);

                            delimitedReader.DelimitedFileFormatCode = fileFormat;
                        }
                    }
                }
            }

            // Make sure the output file isn't the same as the input file
            if (string.Equals(Path.GetFileName(pathInfo.ProteinInputFilePath), Path.GetFileName(outputFilePath), StringComparison.OrdinalIgnoreCase))
            {
                outputFilePath = Path.GetFileNameWithoutExtension(outputFilePath) + "_new" + Path.GetExtension(outputFilePath);
            }

            // Define the full path to the parsed proteins output file
            pathInfo.ProteinOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, outputFilePath);

            if (outputFilePath.EndsWith("_parsed.txt"))
            {
                outputFilePath = outputFilePath.Substring(0, outputFilePath.Length - "_parsed.txt".Length) + "_digested";
            }
            else
            {
                outputFilePath = Path.GetFileNameWithoutExtension(outputFilePath) + "_digested";
            }

            outputFilePath += "_Mass" + Math.Round((decimal)ProcessingOptions.DigestionOptions.MinFragmentMass, 0) + "to" + Math.Round((decimal)ProcessingOptions.DigestionOptions.MaxFragmentMass, 0);

            if (ProcessingOptions.ComputepI && (ProcessingOptions.DigestionOptions.MinIsoelectricPoint > 0f || ProcessingOptions.DigestionOptions.MaxIsoelectricPoint < 14f))
            {
                outputFilePath += "_pI" + Math.Round(ProcessingOptions.DigestionOptions.MinIsoelectricPoint, 1) +
                                  "to" + Math.Round(ProcessingOptions.DigestionOptions.MaxIsoelectricPoint, 2);
            }

            outputFilePath += ".txt";

            // Define the full path to the digested proteins output file
            pathInfo.DigestedProteinOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, outputFilePath);

            return true;
        }

        private bool ParseProteinFileCreateDigestedProteinOutputFile(string digestedProteinOutputFilePath, ref StreamWriter digestFileWriter, char outputFileDelimiter)
        {
            try
            {
                // Create the digested protein output file
                digestFileWriter = new StreamWriter(digestedProteinOutputFilePath);

                string lineOut;
                if (!ProcessingOptions.ExcludeProteinSequence)
                {
                    lineOut = "Protein_Name" + outputFileDelimiter + "Sequence" + outputFileDelimiter;
                }
                else
                {
                    lineOut = string.Empty;
                }

                lineOut += "Unique_ID" + outputFileDelimiter + "Monoisotopic_Mass" + outputFileDelimiter + "Predicted_NET" + outputFileDelimiter + "Tryptic_Name";

                if (ProcessingOptions.ComputepI)
                {
                    lineOut += outputFileDelimiter + "pI" + outputFileDelimiter + "Hydrophobicity";
                }

                if (ProcessingOptions.ComputeNET)
                {
                    lineOut += outputFileDelimiter + "LC_NET";
                }

                if (ProcessingOptions.ComputeSCXNET)
                {
                    lineOut += outputFileDelimiter + "SCX_NET";
                }

                digestFileWriter.WriteLine(lineOut);

                return true;
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile, ex);
                return false;
            }
        }

        private void PreScanProteinFileForAddnlRefsInDescription(
            string proteinInputFilePath,
            ISet<string> addnlRefMasterNames)
        {
            FastaFileReader reader = null;
            var protein = new ProteinInfo();

            bool success;

            try
            {
                reader = new FastaFileReader();

                // Attempt to open the input file
                if (!reader.OpenFile(proteinInputFilePath))
                {
                    return;
                }

                success = true;
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile, ex);
                success = false;
            }

            // Abort processing if we couldn't successfully open the input file
            if (!success)
            {
                return;
            }

            try
            {
                ResetProgress("Pre-reading FASTA file; looking for possible additional reference names");

                // Read each protein in the output file and process appropriately

                do
                {
                    var inputProteinFound = reader.ReadNextProteinEntry();

                    if (!inputProteinFound)
                    {
                        break;
                    }

                    protein.Name = reader.ProteinName;
                    protein.Description = reader.ProteinDescription;

                    // Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
                    ExtractAlternateProteinNamesFromDescription(protein);

                    // Make sure each of the names in .AlternateNames[] is in addnlRefMasterNames
                    int index;
                    for (index = 0; index < protein.AlternateNameCount; index++)
                    {
                        addnlRefMasterNames.Add(protein.AlternateNames[index].RefName);
                    }

                    UpdateProgress(reader.PercentFileProcessed());
                }
                while (!AbortProcessing);

                reader.CloseFile();
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile, ex);
            }
        }

        public void ShowProcessingOptions()
        {
            var digestPeptides = ProcessingOptions.CreateProteinOutputFile &&
                                 ProcessingOptions.CreateDigestedProteinOutputFile &&
                                 !ProcessingOptions.CreateFastaOutputFile;

            Console.WriteLine("{0,-31} {1}", "Create FASTA output file:", ProcessingOptions.CreateFastaOutputFile);
            if (ProcessingOptions.ProteinScramblingMode == ProteinScramblingModeConstants.None)
            {
                Console.WriteLine("{0,-31} {1}", "Create protein .txt file:", ProcessingOptions.CreateProteinOutputFile);
            }
            else
            {
                Console.WriteLine("{0,-31} {1}", "Reverse/Scrambled mode:", ProcessingOptions.ProteinScramblingMode);
            }

            Console.WriteLine("{0,-31} {1}", "Digest input file:", digestPeptides);
            Console.WriteLine();
            Console.WriteLine("{0,-31} {1}", "Exclude protein sequence:", ProcessingOptions.ExcludeProteinSequence);
            Console.WriteLine("{0,-31} {1}", "Exclude protein description:", ProcessingOptions.ExcludeProteinDescription);
            Console.WriteLine("{0,-31} {1}", "Compute protein mass:", ProcessingOptions.ComputeProteinMass);
            Console.WriteLine("{0,-31} {1}", "Element mass mode:", ProcessingOptions.ElementMassMode);
            Console.WriteLine("{0,-31} {1}", "Include X residues in mass:", ProcessingOptions.IncludeXResiduesInMass);
            Console.WriteLine("{0,-31} {1}", "Compute NET:", ProcessingOptions.ComputeNET);
            Console.WriteLine("{0,-31} {1}", "Compute isoelectric point (pI):", ProcessingOptions.ComputepI);
            Console.WriteLine("{0,-31} {1}", "Compute sequence hash values:", ProcessingOptions.ComputeSequenceHashValues);
            Console.WriteLine("{0,-31} {1}", "Hash ignores I/L differences:", ProcessingOptions.ComputeSequenceHashIgnoreILDiff);
            Console.WriteLine("{0,-31} {1}", "Truncate protein description:", ProcessingOptions.TruncateProteinDescription);
            Console.WriteLine();

            if (digestPeptides)
            {
                Console.WriteLine("{0,-31} {1}", "Digestion enzyme:", ProcessingOptions.DigestionEnzyme);
                Console.WriteLine("{0,-31} {1}", "Minimum fragment mass:", ProcessingOptions.DigestionOptions.MinFragmentMass);
                Console.WriteLine("{0,-31} {1}", "Maximum fragment mass:", ProcessingOptions.DigestionOptions.MaxFragmentMass);
                Console.WriteLine("{0,-31} {1}", "Minimum fragment length:", ProcessingOptions.DigestionOptions.MinFragmentResidueCount);
                Console.WriteLine("{0,-31} {1}", "Maximum missed cleavages:", ProcessingOptions.DigestionOptions.MaxMissedCleavages);
                Console.WriteLine("{0,-31} {1}", "Minimum pI:", ProcessingOptions.DigestionOptions.MinIsoelectricPoint);
                Console.WriteLine("{0,-31} {1}", "Maximum pI:", ProcessingOptions.DigestionOptions.MaxIsoelectricPoint);
                Console.WriteLine("{0,-31} {1}", "Generate unique ID values:", ProcessingOptions.GenerateUniqueIDValuesForPeptides);
                Console.WriteLine("{0,-31} {1}", "Remove duplicate sequences:", ProcessingOptions.DigestionOptions.RemoveDuplicateSequences);
                Console.WriteLine("{0,-31} {1}", "Only include cys peptides:", ProcessingOptions.DigestionOptions.CysPeptidesOnly);
                Console.WriteLine("{0,-31} {1}", "Cysteine treatment mode:", ProcessingOptions.DigestionOptions.CysTreatmentMode);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// If filePath ends with extension, remove it
        /// Supports multi-part extensions like .fasta.gz
        /// </summary>
        /// <param name="filePath">File name or path</param>
        /// <param name="extension">Extension, with or without the leading period</param>
        public static string StripExtension(string filePath, string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return filePath;
            }

            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            if (!filePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                return filePath;
            }

            return filePath.Substring(0, filePath.Length - extension.Length);
        }

        private string ValidateProteinName(string proteinName, int maximumLength)
        {
            var sepChars = new[] { ' ', ',', ';', ':', '_', '-', '|', '/' };

            if (maximumLength < 1)
            {
                maximumLength = 1;
            }

            if (proteinName == null)
            {
                return string.Empty;
            }

            if (proteinName.Length <= maximumLength)
            {
                return proteinName;
            }

            // Truncate protein name to maximum length
            var truncatedName = proteinName.Substring(0, maximumLength);

            // Make sure the protein name doesn't end in a space, dash, underscore, semicolon, colon, etc.
            return truncatedName.TrimEnd(sepChars);
        }

        private void WriteFastaAppendToCache(TextWriter scrambledFileWriter, ScramblingResidueCache residueCache, string proteinNamePrefix, bool flushResiduesToWrite)
        {
            if (residueCache.Cache.Length > 0)
            {
                var residueCount = (int)Math.Round(Math.Round(residueCache.Cache.Length * residueCache.SamplingPercentage / 100.0d, 0));
                if (residueCount < 1)
                {
                    residueCount = 1;
                }

                if (residueCount > residueCache.Cache.Length)
                {
                    residueCount = residueCache.Cache.Length;
                }

                while (residueCount > 0)
                {
                    if (residueCache.ResiduesToWrite.Length + residueCount <= residueCache.CacheLength)
                    {
                        residueCache.ResiduesToWrite += residueCache.Cache.Substring(0, residueCount);
                        residueCache.Cache = string.Empty;
                        residueCount = 0;
                    }
                    else
                    {
                        var residuesToAppend = residueCache.CacheLength - residueCache.ResiduesToWrite.Length;
                        residueCache.ResiduesToWrite += residueCache.Cache.Substring(0, residuesToAppend);
                        residueCache.Cache = residueCache.Cache.Substring(residuesToAppend);
                        residueCount -= residuesToAppend;
                    }

                    if (residueCache.ResiduesToWrite.Length >= residueCache.CacheLength)
                    {
                        // Write out .ResiduesToWrite
                        WriteFastaEmptyCache(scrambledFileWriter, residueCache, proteinNamePrefix, residueCache.SamplingPercentage);
                    }
                }
            }

            if (flushResiduesToWrite && residueCache.ResiduesToWrite.Length > 0)
            {
                WriteFastaEmptyCache(scrambledFileWriter, residueCache, proteinNamePrefix, residueCache.SamplingPercentage);
            }
        }

        private void WriteFastaEmptyCache(TextWriter scrambledFileWriter, ScramblingResidueCache residueCache, string proteinNamePrefix, int samplingPercentage)
        {
            if (residueCache.ResiduesToWrite.Length > 0)
            {
                residueCache.OutputCount++;

                var proteinName = proteinNamePrefix + mFileNameAbbreviated;

                if (samplingPercentage < 100)
                {
                    proteinName = ValidateProteinName(proteinName, MAXIMUM_PROTEIN_NAME_LENGTH - 7 - Math.Max(5, residueCache.OutputCount.ToString().Length));
                }
                else
                {
                    proteinName = ValidateProteinName(proteinName, MAXIMUM_PROTEIN_NAME_LENGTH - 1 - Math.Max(5, residueCache.OutputCount.ToString().Length));
                }

                if (samplingPercentage < 100)
                {
                    proteinName += "_" + samplingPercentage + "pct_";
                }
                else
                {
                    proteinName += proteinName + "_";
                }

                proteinName += residueCache.OutputCount.ToString();

                var headerLine = string.Format("{0}{1}{2}{3}",
                    FastaFileReader.PROTEIN_LINE_START_CHAR, proteinName,
                    FastaFileReader.PROTEIN_LINE_ACCESSION_TERMINATOR, proteinName);

                WriteFastaProteinAndResidues(scrambledFileWriter, headerLine, residueCache.ResiduesToWrite);
                residueCache.ResiduesToWrite = string.Empty;
            }
        }

        private void WriteFastaProteinAndResidues(TextWriter scrambledFileWriter, string headerLine, string sequence)
        {
            scrambledFileWriter.WriteLine(headerLine);
            while (sequence.Length > 0)
            {
                if (sequence.Length >= 60)
                {
                    scrambledFileWriter.WriteLine(sequence.Substring(0, 60));
                    sequence = sequence.Substring(60);
                }
                else
                {
                    scrambledFileWriter.WriteLine(sequence);
                    sequence = string.Empty;
                }
            }
        }

        private void WriteScrambledFasta(
            TextWriter scrambledFileWriter,
            Random randomNumberGenerator,
            ProteinInfo protein,
            ProteinScramblingModeConstants scramblingMode,
            ScramblingResidueCache residueCache)
        {
            string scrambledSequence;
            string proteinNamePrefix;
            int residueCount;

            if (scramblingMode is ProteinScramblingModeConstants.Reversed or ProteinScramblingModeConstants.Decoy)
            {
                proteinNamePrefix = PROTEIN_PREFIX_REVERSED;
                var seqArray = protein.Sequence.ToCharArray();
                Array.Reverse(seqArray);
                scrambledSequence = new string(seqArray);
            }
            else
            {
                proteinNamePrefix = PROTEIN_PREFIX_SCRAMBLED;

                scrambledSequence = string.Empty;

                var sequence = string.Copy(protein.Sequence);
                residueCount = sequence.Length;

                while (residueCount > 0)
                {
                    if (residueCount != sequence.Length)
                    {
                        ShowErrorMessage("Assertion failed in WriteScrambledFasta: residueCount should equal sequence.Length: " + residueCount + " vs. " + sequence.Length);
                    }

                    // Randomly extract residues from sequence
                    var index = randomNumberGenerator.Next(residueCount);

                    scrambledSequence += sequence.Substring(index, 1);

                    if (index > 0)
                    {
                        if (index < sequence.Length - 1)
                        {
                            sequence = sequence.Substring(0, index) + sequence.Substring(index + 1);
                        }
                        else
                        {
                            sequence = sequence.Substring(0, index);
                        }
                    }
                    else
                    {
                        sequence = sequence.Substring(index + 1);
                    }

                    residueCount--;
                }
            }

            if (residueCache.SamplingPercentage >= 100)
            {
                var proteinName = ValidateProteinName(proteinNamePrefix + protein.Name, MAXIMUM_PROTEIN_NAME_LENGTH);
                var headerLine = string.Format("{0}{1}{2}{3}",
                    FastaFileReader.PROTEIN_LINE_START_CHAR, proteinName,
                    FastaFileReader.PROTEIN_LINE_ACCESSION_TERMINATOR, protein.Description);

                WriteFastaProteinAndResidues(scrambledFileWriter, headerLine, scrambledSequence);
            }
            else
            {
                // Writing a sampling of the residues to the output file

                while (scrambledSequence.Length > 0)
                {
                    // Append to the cache
                    if (residueCache.Cache.Length + scrambledSequence.Length <= residueCache.CacheLength)
                    {
                        residueCache.Cache += scrambledSequence;
                        scrambledSequence = string.Empty;
                    }
                    else
                    {
                        residueCount = residueCache.CacheLength - residueCache.Cache.Length;
                        residueCache.Cache += scrambledSequence.Substring(0, residueCount);
                        scrambledSequence = scrambledSequence.Substring(residueCount);
                    }

                    if (residueCache.Cache.Length >= residueCache.CacheLength)
                    {
                        // Write out a portion of the cache
                        WriteFastaAppendToCache(scrambledFileWriter, residueCache, proteinNamePrefix, false);
                    }
                }
            }
        }

        /// <summary>
        /// Main processing function -- Calls ParseProteinFile
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <param name="outputFolderPath"></param>
        /// <param name="parameterFilePath">Ignored, since options are loaded from a Key=Value parameter file by the CommandLineParser</param>
        /// <param name="resetErrorCode"></param>
        public override bool ProcessFile(string inputFilePath, string outputFolderPath, string parameterFilePath, bool resetErrorCode)
        {
            // Returns True if success, False if failure

            var success = false;

            if (resetErrorCode)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.NoError);
                MostRecentException = null;
            }

            try
            {
                if (string.IsNullOrEmpty(inputFilePath))
                {
                    ShowErrorMessage("Input file name is empty");
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath);
                }
                else
                {
                    Console.WriteLine();
                    ShowMessage("Parsing " + Path.GetFileName(inputFilePath));

                    if (!CleanupFilePaths(ref inputFilePath, ref outputFolderPath))
                    {
                        SetBaseClassErrorCode(ProcessFilesErrorCodes.FilePathError);
                    }
                    else
                    {
                        try
                        {
                            // Obtain the full path to the input file
                            var inputFile = new FileInfo(inputFilePath);
                            var inputFilePathFull = inputFile.FullName;

                            success = ParseProteinFile(inputFilePathFull, outputFolderPath);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Error calling ParseProteinFile", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in ProcessFile", ex);
            }

            return success;
        }

        private void SetLocalErrorCode(ParseProteinFileErrorCodes newErrorCode, Exception ex = null, bool leaveExistingErrorCodeUnchanged = false)
        {
            if (leaveExistingErrorCodeUnchanged && LocalErrorCode != ParseProteinFileErrorCodes.NoError)
            {
                // An error code is already defined; do not change it
            }
            else
            {
                LocalErrorCode = newErrorCode;

                if (newErrorCode == ParseProteinFileErrorCodes.NoError)
                {
                    if (ErrorCode == ProcessFilesErrorCodes.LocalizedError)
                    {
                        SetBaseClassErrorCode(ProcessFilesErrorCodes.NoError);
                        MostRecentException = null;
                    }
                }
                else
                {
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.LocalizedError);
                    if (ex != null)
                        MostRecentException = ex;
                }
            }
        }

        private void UpdateSubtaskProgress(string description, float percentComplete)
        {
            var descriptionChanged = (description ?? string.Empty) != (mSubtaskProgressStepDescription ?? string.Empty);

            if (descriptionChanged)
            {
                mSubtaskProgressStepDescription = string.Copy(description ?? string.Empty);
            }

            mSubtaskProgressPercentComplete = percentComplete switch
            {
                < 0f => 0f,
                > 100f => 100f,
                _ => percentComplete
            };

            if (descriptionChanged)
            {
                if (Math.Abs(mSubtaskProgressPercentComplete - 0f) < float.Epsilon)
                {
                    LogMessage(mSubtaskProgressStepDescription.Replace(Environment.NewLine, "; "));
                }
                else
                {
                    LogMessage(mSubtaskProgressStepDescription + " (" + mSubtaskProgressPercentComplete.ToString("0.0") + "% complete)".Replace(Environment.NewLine, "; "));
                }
            }

            SubtaskProgressChanged?.Invoke(description, mSubtaskProgressPercentComplete);
        }

        /// <summary>
        /// ProteinFileParser options class
        /// </summary>
        public class FastaFileParseOptions
        {
            private bool mReadonly;

            private bool mLookForAddnlRefInDescription;

            private char mAddnlRefSepChar;
            private char mAddnlRefAccessionSepChar;

            public bool ReadonlyClass
            {
                get => mReadonly;
                set
                {
                    if (!mReadonly)
                    {
                        mReadonly = value;
                    }
                }
            }

            public bool LookForAddnlRefInDescription
            {
                get => mLookForAddnlRefInDescription;
                set
                {
                    if (!mReadonly)
                    {
                        mLookForAddnlRefInDescription = value;
                    }
                }
            }

            // ReSharper disable CommentTypo

            /// <summary>
            /// <para>
            /// Character between alternate protein accession names, as defined in a protein description
            /// </para>
            /// <para>
            /// Example with three alternate names:
            /// SWISS-PROT:O95793-1|TREMBL:Q59F99|ENSEMBL:ENSP00000346163
            /// </para>
            /// </summary>
            /// <remarks>Default is a vertical bar</remarks>
            public char AddnlRefSepChar
            {
                get => mAddnlRefSepChar;
                set
                {
                    if (value != default && !mReadonly)
                    {
                        mAddnlRefSepChar = value;
                    }
                }
            }

            // ReSharper restore CommentTypo

            /// <summary>
            /// Character between the accession and the value
            /// </summary>
            /// <remarks>Default is a colon</remarks>
            public char AddnlRefAccessionSepChar
            {
                get => mAddnlRefAccessionSepChar;
                set
                {
                    if (value != default && !mReadonly)
                    {
                        mAddnlRefAccessionSepChar = value;
                    }
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public FastaFileParseOptions()
            {
                mLookForAddnlRefInDescription = false;
                mAddnlRefSepChar = '|';
                mAddnlRefAccessionSepChar = ':';
            }
        }

        private void InSilicoDigest_ErrorEvent(string message)
        {
            ShowErrorMessage("Error in InSilicoDigester: " + message);
        }

        private void InSilicoDigest_ProgressChanged(string taskDescription, float percentComplete)
        {
            UpdateSubtaskProgress(taskDescription, percentComplete);
        }

        private void InSilicoDigest_ProgressReset()
        {
            // Don't do anything with this event
        }

        private void NETCalculator_ErrorEvent(string message)
        {
            ShowErrorMessage(message);
        }

        private void SCXNETCalculator_ErrorEvent(string message)
        {
            ShowErrorMessage(message);
        }
    }
}
