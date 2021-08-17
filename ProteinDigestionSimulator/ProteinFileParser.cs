using System;
using System.Collections;
using System.Collections.Generic;

// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004
//
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://omics.pnl.gov/ or https://www.pnnl.gov/sysbio/ or https://panomics.pnnl.gov/
// -------------------------------------------------------------------------------
//
// Licensed under the 2-Clause BSD License; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// https://opensource.org/licenses/BSD-2-Clause
//
// Copyright 2018 Battelle Memorial Institute

using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using NETPrediction;
using PRISM;
using PRISM.FileProcessor;
using ProteinFileReader;
using ValidateFastaFile;

namespace ProteinDigestionSimulator
{
    /// <summary>
    /// This class will read a protein FASTA file or delimited protein info file and parse it
    /// to create a delimited protein list output file, and optionally an in-silico digested output file
    ///
    /// It can also create a FASTA file containing reversed or scrambled sequences, and these can
    /// be based on all of the proteins in the input file, or a sampling of the proteins in the input file
    /// </summary>
    public class ProteinFileParser : ProcessFilesBase
    {
        // Ignore Spelling: ComputepI, Cys, gi, hydrophobicity, Ile, Leu, pre, SepChar, silico, varchar

        public ProteinFileParser()
        {
            mFileDate = "August 6, 2021";
            InitializeLocalVariables();
        }

        public const string XML_SECTION_OPTIONS = "ProteinDigestionSimulatorOptions";
        public const string XML_SECTION_FASTA_OPTIONS = "FastaInputOptions";
        public const string XML_SECTION_PROCESSING_OPTIONS = "ProcessingOptions";
        public const string XML_SECTION_DIGESTION_OPTIONS = "DigestionOptions";
        public const string XML_SECTION_UNIQUENESS_STATS_OPTIONS = "UniquenessStatsOptions";
        private const int PROTEIN_CACHE_MEMORY_RESERVE_COUNT = 500;
        private const int SCRAMBLING_CACHE_LENGTH = 4000;
        private const string PROTEIN_PREFIX_SCRAMBLED = "Random_";
        private const string PROTEIN_PREFIX_REVERSED = "XXX.";
        private const int MAXIMUM_PROTEIN_NAME_LENGTH = 512;
        private const int MAX_ABBREVIATED_FILENAME_LENGTH = 15;

        // The value of 7995 is chosen because the maximum varchar() value in SQL Server is varchar(8000)
        // and we want to prevent truncation errors when importing protein names and descriptions into SQL Server
        private const int MAX_PROTEIN_DESCRIPTION_LENGTH = FastaValidator.MAX_PROTEIN_DESCRIPTION_LENGTH;

        // Error codes specialized for this class
        public enum ParseProteinFileErrorCodes
        {
            NoError = 0,
            ProteinFileParsingOptionsSectionNotFound = 1,
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
            None = 0,
            Reversed = 1,
            Randomized = 2
        }

        /// <summary>
        /// Column delimiter to use when processing TSV or CSV files
        /// </summary>
        /// <remarks>The GUI uses this enum</remarks>
        public enum DelimiterCharConstants
        {
            Space = 0,
            Tab = 1,
            Comma = 2,
            // ReSharper disable once UnusedMember.Global
            Other = 3
        }

        public struct AddnlRef
        {
            public string RefName;                // e.g. in gi:12334  the RefName is "gi" and the RefAccession is "1234"
            public string RefAccession;
        }

        public struct ProteinInfo
        {
            public string Name;
            public int AlternateNameCount;
            public AddnlRef[] AlternateNames;
            public string Description;
            public string Sequence;
            public string SequenceHash;                   // Only populated if ComputeSequenceHashValues=true
            public double Mass;
            public float pI;
            public float Hydrophobicity;
            public float ProteinNET;
            public float ProteinSCXNET;
        }

        private struct FilePathInfo
        {
            public string ProteinInputFilePath;
            public string OutputFileNameBaseOverride;
            public string OutputFolderPath;
            public string ProteinOutputFilePath;
            public string DigestedProteinOutputFilePath;
        }

        private struct ScramblingResidueCache
        {
            public string Cache;                          // Cache of residues parsed; when this reaches 4000 characters, then a portion of this text is appended to ResiduesToWrite
            public int CacheLength;
            public int SamplingPercentage;
            public int OutputCount;
            public string ResiduesToWrite;
        }

        private char mInputFileDelimiter;                              // Only used for delimited protein input files, not for FASTA files
        private int mInputFileProteinsProcessed;
        private int mInputFileLinesRead;
        private int mInputFileLineSkipCount;
        private char mOutputFileDelimiter;
        private int mSequenceWidthToExamineForMaximumpI;
        public InSilicoDigest.DigestionOptions DigestionOptions;
        public FastaFileParseOptions FastaFileOptions;
        private bool mObjectVariablesLoaded;
        private InSilicoDigest _mInSilicoDigest;

        private InSilicoDigest mInSilicoDigest
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mInSilicoDigest;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mInSilicoDigest != null)
                {
                    _mInSilicoDigest.ErrorEvent -= InSilicoDigest_ErrorEvent;
                    _mInSilicoDigest.ProgressChanged -= InSilicoDigest_ProgressChanged;
                    _mInSilicoDigest.ProgressReset -= InSilicoDigest_ProgressReset;
                }

                _mInSilicoDigest = value;
                if (_mInSilicoDigest != null)
                {
                    _mInSilicoDigest.ErrorEvent += InSilicoDigest_ErrorEvent;
                    _mInSilicoDigest.ProgressChanged += InSilicoDigest_ProgressChanged;
                    _mInSilicoDigest.ProgressReset += InSilicoDigest_ProgressReset;
                }
            }
        }

        private ComputePeptideProperties mpICalculator;
        private ElutionTimePredictionKangas _mNETCalculator;

        private ElutionTimePredictionKangas mNETCalculator
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mNETCalculator;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mNETCalculator != null)
                {
                    _mNETCalculator.ErrorEvent -= NETCalculator_ErrorEvent;
                }

                _mNETCalculator = value;
                if (_mNETCalculator != null)
                {
                    _mNETCalculator.ErrorEvent += NETCalculator_ErrorEvent;
                }
            }
        }

        private SCXElutionTimePredictionKangas _mSCXNETCalculator;

        private SCXElutionTimePredictionKangas mSCXNETCalculator
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _mSCXNETCalculator;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_mSCXNETCalculator != null)
                {
                    _mSCXNETCalculator.ErrorEvent -= SCXNETCalculator_ErrorEvent;
                }

                _mSCXNETCalculator = value;
                if (_mSCXNETCalculator != null)
                {
                    _mSCXNETCalculator.ErrorEvent += SCXNETCalculator_ErrorEvent;
                }
            }
        }

        private int mProteinCount;
        private ProteinInfo[] mProteins;
        private bool mParsedFileIsFastaFile;
        private string mFileNameAbbreviated;
        private Hashtable mMasterSequencesHashTable;
        private int mNextUniqueIDForMasterSeqs;
        private ParseProteinFileErrorCodes mLocalErrorCode;
        private string mSubtaskProgressStepDescription = string.Empty;
        private float mSubtaskProgressPercentComplete = 0f;

        // PercentComplete ranges from 0 to 100, but can contain decimal percentage values
        public event SubtaskProgressChangedEventHandler SubtaskProgressChanged;

        public delegate void SubtaskProgressChangedEventHandler(string taskDescription, float percentComplete);

        public bool AssumeDelimitedFile { get; set; }
        public bool AssumeFastaFile { get; set; }
        public bool ComputeNET { get; set; }
        public bool ComputepI { get; set; }
        public bool ComputeProteinMass { get; set; }
        public bool ComputeSequenceHashValues { get; set; }
        public bool ComputeSequenceHashIgnoreILDiff { get; set; }
        public bool ComputeSCXNET { get; set; }

        /// <summary>
        /// True to create a FASTA output file; false for a tab-delimited text file
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Only valid if mCreateDigestedProteinOutputFile is False</remarks>
        public bool CreateFastaOutputFile { get; set; }

        /// <summary>
        /// When True, then writes the proteins to a file; When false, then caches the results in memory
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Use DigestProteinSequence to obtained digested peptides instead of proteins</remarks>
        public bool CreateProteinOutputFile { get; set; }

        /// <summary>
        /// True to in-silico digest the proteins
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Only valid if CreateProteinOutputFile is True</remarks>
        public bool CreateDigestedProteinOutputFile { get; set; }

        /// <summary>
        /// When true, do not include protein description in the output file
        /// </summary>
        /// <returns></returns>
        public bool ExcludeProteinDescription { get; set; }

        /// <summary>
        /// When true, do not include protein sequence in the output file
        /// </summary>
        /// <returns></returns>
        public bool ExcludeProteinSequence { get; set; }

        /// <summary>
        /// When true, assign UniqueID values to the digested peptides (requires more memory)
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Only valid if CreateDigestedProteinOutputFile is True</remarks>
        public bool GenerateUniqueIDValuesForPeptides { get; set; }

        /// <summary>
        /// When true, include X residues when computing protein mass (using the mass of Ile/Leu)
        /// </summary>
        /// <returns></returns>
        public bool IncludeXResiduesInMass { get; set; }

        /// <summary>
        /// Summary of the result of processing
        /// </summary>
        /// <returns></returns>
        public string ProcessingSummary { get; set; }

        /// <summary>
        /// When true, report the maximum pI
        /// </summary>
        /// <returns></returns>
        public bool ReportMaximumpI { get; set; }
        public bool ShowDebugPrompts { get; set; }
        public bool TruncateProteinDescription { get; set; }
        public DelimitedProteinFileReader.ProteinFileFormatCode DelimitedFileFormatCode { get; set; }

        public PeptideSequence.ElementModeConstants ElementMassMode
        {
            get
            {
                if (mInSilicoDigest is null)
                {
                    return PeptideSequence.ElementModeConstants.IsotopicMass;
                }
                else
                {
                    return mInSilicoDigest.ElementMassMode;
                }
            }

            set
            {
                if (mInSilicoDigest is null)
                {
                    InitializeObjectVariables();
                }

                mInSilicoDigest.ElementMassMode = value;
            }
        }

        public ComputePeptideProperties.HydrophobicityTypeConstants HydrophobicityType { get; set; }

        public char InputFileDelimiter
        {
            get
            {
                return mInputFileDelimiter;
            }

            set
            {
                if (value != default)
                {
                    mInputFileDelimiter = value;
                }
            }
        }

        public int InputFileProteinsProcessed
        {
            get
            {
                return mInputFileProteinsProcessed;
            }
        }

        public int InputFileLinesRead
        {
            get
            {
                return mInputFileLinesRead;
            }
        }

        public int InputFileLineSkipCount
        {
            get
            {
                return mInputFileLineSkipCount;
            }
        }

        public ParseProteinFileErrorCodes LocalErrorCode
        {
            get
            {
                return mLocalErrorCode;
            }
        }

        public char OutputFileDelimiter
        {
            get
            {
                return mOutputFileDelimiter;
            }

            set
            {
                if (value != default)
                {
                    mOutputFileDelimiter = value;
                }
            }
        }

        public bool ParsedFileIsFastaFile
        {
            get
            {
                return mParsedFileIsFastaFile;
            }
        }

        public int ProteinScramblingLoopCount { get; set; }
        public ProteinScramblingModeConstants ProteinScramblingMode { get; set; }
        public int ProteinScramblingSamplingPercentage { get; set; }

        public int SequenceWidthToExamineForMaximumpI
        {
            get
            {
                return mSequenceWidthToExamineForMaximumpI;
            }

            set
            {
                if (value < 1)
                    mSequenceWidthToExamineForMaximumpI = 1;
                mSequenceWidthToExamineForMaximumpI = value;
            }
        }

        private float ComputeSequenceHydrophobicity(string peptideSequence)
        {
            // Be sure to call InitializeObjectVariables before calling this function for the first time
            // Otherwise, mpICalculator will be nothing
            if (mpICalculator is null)
            {
                return 0f;
            }
            else
            {
                return mpICalculator.CalculateSequenceHydrophobicity(peptideSequence);
            }
        }

        private float ComputeSequencepI(string peptideSequence)
        {
            // Be sure to call InitializeObjectVariables before calling this function for the first time
            // Otherwise, mpICalculator will be nothing
            if (mpICalculator is null)
            {
                return 0f;
            }
            else
            {
                return mpICalculator.CalculateSequencepI(peptideSequence);
            }
        }

        private double ComputeSequenceMass(string peptideSequence)
        {
            // Be sure to call InitializeObjectVariables before calling this function for the first time
            // Otherwise, mInSilicoDigest will be nothing
            if (mInSilicoDigest is null)
            {
                return 0d;
            }
            else
            {
                return mInSilicoDigest.ComputeSequenceMass(peptideSequence, IncludeXResiduesInMass);
            }
        }

        private float ComputeSequenceNET(string peptideSequence)
        {
            // Be sure to call InitializeObjectVariables before calling this function for the first time
            // Otherwise, mNETCalculator will be nothing
            if (mNETCalculator is null)
            {
                return 0f;
            }
            else
            {
                return mNETCalculator.GetElutionTime(peptideSequence);
            }
        }

        private float ComputeSequenceSCXNET(string peptideSequence)
        {
            // Be sure to call InitializeObjectVariables before calling this function for the first time
            // Otherwise, mSCXNETCalculator will be nothing
            if (mSCXNETCalculator is null)
            {
                return 0f;
            }
            else
            {
                return mSCXNETCalculator.GetElutionTime(peptideSequence);
            }
        }

        /// <summary>
        /// Digest the protein sequence using the given cleavage options
        /// </summary>
        /// <param name="peptideSequence"></param>
        /// <param name="peptideFragments"></param>
        /// <param name="options"></param>
        /// <param name="proteinName"></param>
        /// <returns>The number of digested peptides in peptideFragments</returns>
        public int DigestProteinSequence(string peptideSequence, out List<InSilicoDigest.PeptideSequenceWithNET> peptideFragments, InSilicoDigest.DigestionOptions options, string proteinName = "")
        {
            // Make sure the object variables are initialized
            if (!InitializeObjectVariables())
            {
                peptideFragments = new List<InSilicoDigest.PeptideSequenceWithNET>();
                return 0;
            }

            try
            {
                int peptideCount = mInSilicoDigest.DigestSequence(peptideSequence, out peptideFragments, options, ComputepI, proteinName);
                return peptideCount;
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.DigestProteinSequenceError);
                peptideFragments = new List<InSilicoDigest.PeptideSequenceWithNET>();
                return 0;
            }
        }

        private int ExtractAlternateProteinNamesFromDescription(ref string description, ref AddnlRef[] alternateNames)
        {
            // Searches in description for additional protein Ref names
            // description is passed ByRef since the additional protein references will be removed from it

            int alternateNameCount = 0;
            alternateNames = new AddnlRef[1];
            try
            {
                var proteinNames = description.Split(FastaFileOptions.AddnlRefSepChar);
                if (proteinNames.Length > 0)
                {
                    alternateNames = new AddnlRef[proteinNames.Length];
                    for (int index = 0, loopTo = proteinNames.Length - 1; index <= loopTo; index++)
                    {
                        int charIndex = proteinNames[index].IndexOf(FastaFileOptions.AddnlRefAccessionSepChar);
                        if (charIndex > 0)
                        {
                            if (index == proteinNames.Length - 1)
                            {
                                // Need to find the next space after charIndex and truncate proteinNames() at that location
                                int spaceIndex = proteinNames[index].IndexOf(' ', charIndex);
                                if (spaceIndex >= 0)
                                {
                                    proteinNames[index] = proteinNames[index].Substring(0, spaceIndex);
                                }
                            }

                            if (charIndex >= proteinNames[index].Length - 1)
                            {
                                // No accession after the colon; invalid entry so discard this entry and stop parsing
                                Array.Resize(ref alternateNames, index);
                                break;
                            }

                            alternateNames[index].RefName = proteinNames[index].Substring(0, charIndex);
                            alternateNames[index].RefAccession = proteinNames[index].Substring(charIndex + 1);
                            alternateNameCount += 1;
                            charIndex = description.IndexOf(proteinNames[index], StringComparison.Ordinal);
                            if (charIndex >= 0)
                            {
                                if (charIndex + proteinNames[index].Length + 1 < description.Length)
                                {
                                    description = description.Substring(charIndex + proteinNames[index].Length + 1);
                                }
                                else
                                {
                                    description = string.Empty;
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
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error parsing out additional Ref Names from the Protein Description: " + ex.Message);
            }

            return alternateNameCount;
        }

        private double FractionLowercase(string proteinSequence)
        {
            int lowerCount = 0;
            int upperCount = 0;
            for (int index = 0, loopTo = proteinSequence.Length - 1; index <= loopTo; index++)
            {
                if (char.IsLower(proteinSequence[index]))
                {
                    lowerCount += 1;
                }
                else if (char.IsUpper(proteinSequence[index]))
                {
                    upperCount += 1;
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
            var extensionsToParse = new List<string>() { ".fasta", ".fasta.gz", ".txt" };
            return extensionsToParse;
        }

        public override string GetErrorMessage()
        {
            // Returns "" if no error

            string errorMessage;
            if (ErrorCode == ProcessFilesErrorCodes.LocalizedError || ErrorCode == ProcessFilesErrorCodes.NoError)
            {
                switch (mLocalErrorCode)
                {
                    case ParseProteinFileErrorCodes.NoError:
                        {
                            errorMessage = "";
                            break;
                        }

                    case ParseProteinFileErrorCodes.ProteinFileParsingOptionsSectionNotFound:
                        {
                            errorMessage = "The section " + XML_SECTION_OPTIONS + " was not found in the parameter file";
                            break;
                        }

                    case ParseProteinFileErrorCodes.ErrorReadingInputFile:
                        {
                            errorMessage = "Error reading input file";
                            break;
                        }

                    case ParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile:
                        {
                            errorMessage = "Error creating parsed proteins output file";
                            break;
                        }

                    case ParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile:
                        {
                            errorMessage = "Error creating digested proteins output file";
                            break;
                        }

                    case ParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile:
                        {
                            errorMessage = "Error creating scrambled proteins output file";
                            break;
                        }

                    case ParseProteinFileErrorCodes.ErrorWritingOutputFile:
                        {
                            errorMessage = "Error writing to one of the output files";
                            break;
                        }

                    case ParseProteinFileErrorCodes.ErrorInitializingObjectVariables:
                        {
                            errorMessage = "Error initializing In Silico Digester class";
                            break;
                        }

                    case ParseProteinFileErrorCodes.DigestProteinSequenceError:
                        {
                            errorMessage = "Error in DigestProteinSequence function";
                            break;
                        }

                    case ParseProteinFileErrorCodes.UnspecifiedError:
                        {
                            errorMessage = "Unspecified localized error";
                            break;
                        }

                    default:
                        {
                            // This shouldn't happen
                            errorMessage = "Unknown error state";
                            break;
                        }
                }
            }
            else
            {
                errorMessage = GetBaseClassErrorMessage();
            }

            return errorMessage;
        }

        public int GetProteinCountCached()
        {
            return mProteinCount;
        }

        public ProteinInfo GetCachedProtein(int index)
        {
            if (index < mProteinCount)
            {
                return mProteins[index];
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="digestedPeptides"></param>
        /// <param name="options"></param>
        /// <returns>The number of peptides in digestedPeptides</returns>
        public int GetDigestedPeptidesForCachedProtein(int index, out List<InSilicoDigest.PeptideSequenceWithNET> digestedPeptides, InSilicoDigest.DigestionOptions options)
        {
            if (index < mProteinCount)
            {
                return DigestProteinSequence(mProteins[index].Sequence, out digestedPeptides, options, mProteins[index].Name);
            }
            else
            {
                digestedPeptides = new List<InSilicoDigest.PeptideSequenceWithNET>();
                return 0;
            }
        }

        private void InitializeLocalVariables()
        {
            mLocalErrorCode = ParseProteinFileErrorCodes.NoError;
            ShowDebugPrompts = false;
            AssumeDelimitedFile = false;
            AssumeFastaFile = false;
            mInputFileDelimiter = ControlChars.Tab;
            DelimitedFileFormatCode = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence;
            mInputFileProteinsProcessed = 0;
            mInputFileLinesRead = 0;
            mInputFileLineSkipCount = 0;
            mOutputFileDelimiter = ControlChars.Tab;
            ExcludeProteinSequence = false;
            ComputeProteinMass = true;
            ComputepI = true;
            ComputeNET = true;
            ComputeSCXNET = true;
            ComputeSequenceHashValues = false;
            ComputeSequenceHashIgnoreILDiff = true;
            TruncateProteinDescription = true;
            IncludeXResiduesInMass = false;
            ProteinScramblingMode = ProteinScramblingModeConstants.None;
            ProteinScramblingSamplingPercentage = 100;
            ProteinScramblingLoopCount = 1;
            CreateFastaOutputFile = false;
            CreateProteinOutputFile = false;
            CreateDigestedProteinOutputFile = false;
            GenerateUniqueIDValuesForPeptides = true;
            DigestionOptions = new InSilicoDigest.DigestionOptions();
            FastaFileOptions = new FastaFileParseOptions();
            ProcessingSummary = string.Empty;
            mProteinCount = 0;
            mProteins = new ProteinInfo[1];
            mFileNameAbbreviated = string.Empty;
            HydrophobicityType = ComputePeptideProperties.HydrophobicityTypeConstants.HW;
            ReportMaximumpI = false;
            mSequenceWidthToExamineForMaximumpI = 10;
        }

        /// <summary>
        /// Examines the file's extension and returns true if it ends in .fasta or .fasta.gz
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="notifyErrorsWithMessageBox"></param>
        /// <returns></returns>
        public static bool IsFastaFile(string filePath, bool notifyErrorsWithMessageBox = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return false;
                }

                if (Path.GetExtension(StripExtension(filePath, ".gz")).Equals(".fasta", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    return false;
                }
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

        private bool InitializeObjectVariables()
        {
            string errorMessage = string.Empty;
            if (!mObjectVariablesLoaded)
            {
                // Need to initialize the object variables

                try
                {
                    mInSilicoDigest = new InSilicoDigest();
                }
                catch (Exception ex)
                {
                    errorMessage = "Error initializing InSilicoDigest class";
                    SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorInitializingObjectVariables);
                }

                try
                {
                    mpICalculator = new ComputePeptideProperties();
                }
                catch (Exception ex)
                {
                    errorMessage = "Error initializing pI Calculation class";
                    ShowErrorMessage(errorMessage);
                    SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorInitializingObjectVariables);
                }

                try
                {
                    mNETCalculator = new ElutionTimePredictionKangas();
                }
                catch (Exception ex)
                {
                    errorMessage = "Error initializing LC NET Calculation class";
                    ShowErrorMessage(errorMessage);
                    SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorInitializingObjectVariables);
                }

                try
                {
                    mSCXNETCalculator = new SCXElutionTimePredictionKangas();
                }
                catch (Exception ex)
                {
                    errorMessage = "Error initializing SCX NET Calculation class";
                    ShowErrorMessage(errorMessage);
                    SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorInitializingObjectVariables);
                }

                if (errorMessage.Length > 0)
                {
                    ShowErrorMessage(errorMessage);
                    mObjectVariablesLoaded = false;
                }
                else
                {
                    mObjectVariablesLoaded = true;
                }
            }

            if (mInSilicoDigest is object)
            {
                if (mpICalculator is object)
                {
                    mInSilicoDigest.InitializepICalculator(ref mpICalculator);
                }
            }

            return mObjectVariablesLoaded;
        }

        private bool InitializeScrambledOutput(FilePathInfo pathInfo, ScramblingResidueCache residueCache, ProteinScramblingModeConstants scramblingMode, out StreamWriter scrambledFileWriter, out Random randomNumberGenerator)
        {
            bool success;
            int randomNumberSeed;
            string suffix;
            string scrambledFastaOutputFilePath;

            // Wait to allow the timer to advance.
            Thread.Sleep(1);
            randomNumberSeed = Environment.TickCount;
            randomNumberGenerator = new Random(randomNumberSeed);
            if (scramblingMode == ProteinScramblingModeConstants.Reversed)
            {
                // Reversed FASTA file
                suffix = "_reversed";
                if (residueCache.SamplingPercentage < 100)
                {
                    suffix += "_" + residueCache.SamplingPercentage.ToString() + "pct";
                }
            }
            else
            {
                // Scrambled FASTA file
                suffix = "_scrambled_seed" + randomNumberSeed.ToString();
                if (residueCache.SamplingPercentage < 100)
                {
                    suffix += "_" + residueCache.SamplingPercentage.ToString() + "pct";
                }
            }

            scrambledFastaOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) + suffix + ".fasta");

            // Define the abbreviated name of the input file; used in the protein names
            mFileNameAbbreviated = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath);
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
            try
            {
                // Open the scrambled protein output FASTA file (if required)
                scrambledFileWriter = new StreamWriter(scrambledFastaOutputFilePath);
                success = true;
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorCreatingScrambledProteinOutputFile);
                success = false;
                scrambledFileWriter = null;
            }

            return success;
        }

        public bool LoadParameterFileSettings(string parameterFilePath)
        {
            var settingsFile = new XmlSettingsFileAccessor();
            bool cysPeptidesOnly;
            try
            {
                if (parameterFilePath is null || parameterFilePath.Length == 0)
                {
                    // No parameter file specified; nothing to load
                    return true;
                }

                if (!File.Exists(parameterFilePath))
                {
                    // See if parameterFilePath points to a file in the same directory as the application
                    parameterFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.GetFileName(parameterFilePath));
                    if (!File.Exists(parameterFilePath))
                    {
                        SetBaseClassErrorCode(ProcessFilesErrorCodes.ParameterFileNotFound);
                        return false;
                    }
                }

                // Pass False to .LoadSettings() here to turn off case sensitive matching
                if (settingsFile.LoadSettings(parameterFilePath, false))
                {
                    if (!settingsFile.SectionPresent(XML_SECTION_OPTIONS))
                    {
                        ShowErrorMessage("The node '<section name=\"" + XML_SECTION_OPTIONS + "\"> was not found in the parameter file: " + parameterFilePath);
                        SetLocalErrorCode(ParseProteinFileErrorCodes.ProteinFileParsingOptionsSectionNotFound);
                        return false;
                    }
                    else
                    {
                        // ComparisonFastaFile = settingsFile.GetParam(XML_SECTION_OPTIONS, "ComparisonFastaFile", ComparisonFastaFile)

                        int inputFileColumnDelimiterIndex = settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnDelimiterIndex", (int)DelimiterCharConstants.Tab);
                        string customInputFileColumnDelimiter = settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnDelimiter", Conversions.ToString(ControlChars.Tab));
                        InputFileDelimiter = LookupColumnDelimiterChar(inputFileColumnDelimiterIndex, customInputFileColumnDelimiter, InputFileDelimiter);
                        DelimitedFileFormatCode = (DelimitedProteinFileReader.ProteinFileFormatCode)Conversions.ToInteger(settingsFile.GetParam(XML_SECTION_OPTIONS, "InputFileColumnOrdering", (int)DelimitedFileFormatCode));
                        int outputFileFieldDelimiterIndex = settingsFile.GetParam(XML_SECTION_OPTIONS, "OutputFileFieldDelimiterIndex", (int)DelimiterCharConstants.Tab);
                        string outputFileFieldDelimiter = settingsFile.GetParam(XML_SECTION_OPTIONS, "OutputFileFieldDelimiter", Conversions.ToString(ControlChars.Tab));
                        OutputFileDelimiter = LookupColumnDelimiterChar(outputFileFieldDelimiterIndex, outputFileFieldDelimiter, OutputFileDelimiter);
                        DigestionOptions.IncludePrefixAndSuffixResidues = settingsFile.GetParam(XML_SECTION_OPTIONS, "IncludePrefixAndSuffixResidues", DigestionOptions.IncludePrefixAndSuffixResidues);
                        FastaFileOptions.LookForAddnlRefInDescription = settingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "LookForAddnlRefInDescription", FastaFileOptions.LookForAddnlRefInDescription);
                        FastaFileOptions.AddnlRefSepChar = settingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "AddnlRefSepChar", FastaFileOptions.AddnlRefSepChar.ToString())[0];
                        FastaFileOptions.AddnlRefAccessionSepChar = settingsFile.GetParam(XML_SECTION_FASTA_OPTIONS, "AddnlRefAccessionSepChar", FastaFileOptions.AddnlRefAccessionSepChar.ToString())[0];
                        ExcludeProteinSequence = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ExcludeProteinSequence", ExcludeProteinSequence);
                        ComputeProteinMass = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeProteinMass", ComputeProteinMass);
                        IncludeXResiduesInMass = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "IncludeXResidues", IncludeXResiduesInMass);
                        ElementMassMode = (PeptideSequence.ElementModeConstants)Conversions.ToInteger(settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ElementMassMode", (int)ElementMassMode));
                        ComputepI = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputepI", ComputepI);
                        ComputeNET = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeNET", ComputeNET);
                        ComputeSCXNET = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ComputeSCX", ComputeSCXNET);
                        CreateDigestedProteinOutputFile = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "DigestProteins", CreateDigestedProteinOutputFile);
                        ProteinScramblingMode = (ProteinScramblingModeConstants)Conversions.ToInteger(settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ProteinReversalIndex", (int)ProteinScramblingMode));
                        ProteinScramblingLoopCount = settingsFile.GetParam(XML_SECTION_PROCESSING_OPTIONS, "ProteinScramblingLoopCount", ProteinScramblingLoopCount);
                        DigestionOptions.CleavageRuleID = (InSilicoDigest.CleavageRuleConstants)Conversions.ToInteger(settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "CleavageRuleTypeIndex", (int)DigestionOptions.CleavageRuleID));
                        DigestionOptions.RemoveDuplicateSequences = !settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "IncludeDuplicateSequences", !DigestionOptions.RemoveDuplicateSequences);
                        cysPeptidesOnly = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "CysPeptidesOnly", false);
                        if (cysPeptidesOnly)
                        {
                            DigestionOptions.AminoAcidResidueFilterChars = new char[] { 'C' };
                        }
                        else
                        {
                            DigestionOptions.AminoAcidResidueFilterChars = new char[] { };
                        }

                        DigestionOptions.MinFragmentMass = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMinimumMass", DigestionOptions.MinFragmentMass);
                        DigestionOptions.MaxFragmentMass = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMaximumMass", DigestionOptions.MaxFragmentMass);
                        DigestionOptions.MinFragmentResidueCount = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMinimumResidueCount", DigestionOptions.MinFragmentResidueCount);
                        DigestionOptions.MaxMissedCleavages = settingsFile.GetParam(XML_SECTION_DIGESTION_OPTIONS, "DigestProteinsMaximumMissedCleavages", DigestionOptions.MaxMissedCleavages);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error in LoadParameterFileSettings: " + ex.Message);
                return false;
            }

            return true;
        }

        public static char LookupColumnDelimiterChar(int delimiterIndex, string customDelimiter, char defaultDelimiter)
        {
            string delimiter;
            switch (delimiterIndex)
            {
                case (int)DelimiterCharConstants.Space:
                    {
                        delimiter = " ";
                        break;
                    }

                case (int)DelimiterCharConstants.Tab:
                    {
                        delimiter = Conversions.ToString(ControlChars.Tab);
                        break;
                    }

                case (int)DelimiterCharConstants.Comma:
                    {
                        delimiter = ",";
                        break;
                    }

                default:
                    {
                        // Includes DelimiterCharConstants.Other
                        delimiter = string.Copy(customDelimiter);
                        break;
                    }
            }

            if (delimiter is null || delimiter.Length == 0)
            {
                delimiter = string.Copy(Conversions.ToString(defaultDelimiter));
            }

            try
            {
                return delimiter[0];
            }
            catch (Exception ex)
            {
                return ControlChars.Tab;
            }
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
        /// <returns></returns>
        public bool ParseProteinFile(string proteinInputFilePath, string outputFolderPath, string outputFileNameBaseOverride)
        {
            ProteinFileReaderBaseClass proteinFileReader = null;
            StreamWriter proteinFileWriter = null;
            StreamWriter digestFileWriter = null;
            StreamWriter scrambledFileWriter = null;
            bool success;
            bool inputProteinFound;
            var headerChecked = default(bool);
            bool allowLookForAddnlRefInDescription;
            var lookForAddnlRefInDescription = default(bool);
            SortedSet<string> addnlRefMasterNames;
            AddnlRef[] addnlRefsToOutput = null;
            var generateUniqueSequenceID = default(bool);
            Random randomNumberGenerator = null;
            ProteinScramblingModeConstants scramblingMode;
            var residueCache = new ScramblingResidueCache();
            var startTime = default(DateTime);
            ProcessingSummary = string.Empty;
            FilePathInfo pathInfo;
            pathInfo.ProteinInputFilePath = proteinInputFilePath;
            pathInfo.OutputFolderPath = outputFolderPath;
            pathInfo.OutputFileNameBaseOverride = outputFileNameBaseOverride;
            pathInfo.ProteinOutputFilePath = string.Empty;
            pathInfo.DigestedProteinOutputFilePath = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(pathInfo.ProteinInputFilePath))
                {
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath);
                    return false;
                }

                // Make sure the object variables are initialized
                if (!InitializeObjectVariables())
                {
                    return false;
                }

                success = ParseProteinFileCreateOutputFile(ref pathInfo, out proteinFileReader);
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile);
                success = false;
            }

            // Abort processing if we couldn't successfully open the input file
            if (!success)
                return false;
            try
            {
                // Set the options for mpICalculator
                // Note that this will also update the pICalculator object in mInSilicoDigest
                if (mpICalculator is object)
                {
                    mpICalculator.HydrophobicityType = HydrophobicityType;
                    mpICalculator.ReportMaximumpI = ReportMaximumpI;
                    mpICalculator.SequenceWidthToExamineForMaximumpI = mSequenceWidthToExamineForMaximumpI;
                }

                if (CreateProteinOutputFile)
                {
                    try
                    {
                        // Open the protein output file (if required)
                        // This will cause an error if the input file is the same as the output file
                        proteinFileWriter = new StreamWriter(pathInfo.ProteinOutputFilePath);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorCreatingProteinOutputFile);
                        success = false;
                    }

                    if (!success)
                        return false;
                }

                if (mMasterSequencesHashTable is null)
                {
                    mMasterSequencesHashTable = new Hashtable();
                }
                else
                {
                    mMasterSequencesHashTable.Clear();
                }

                mNextUniqueIDForMasterSeqs = 1;
                if (CreateProteinOutputFile && CreateDigestedProteinOutputFile && !CreateFastaOutputFile)
                {
                    success = ParseProteinFileCreateDigestedProteinOutputFile(pathInfo.DigestedProteinOutputFilePath, ref digestFileWriter);
                    if (!success)
                        return false;
                    if (GenerateUniqueIDValuesForPeptides)
                    {
                        // Initialize mMasterSequencesHashTable
                        generateUniqueSequenceID = true;
                    }
                }

                int loopCount = 1;
                if (ProteinScramblingMode == ProteinScramblingModeConstants.Randomized)
                {
                    loopCount = ProteinScramblingLoopCount;
                    if (loopCount < 1)
                        loopCount = 1;
                    if (loopCount > 10000)
                        loopCount = 10000;
                    allowLookForAddnlRefInDescription = false;
                }
                else
                {
                    allowLookForAddnlRefInDescription = FastaFileOptions.LookForAddnlRefInDescription;
                }

                var outLine = new StringBuilder();
                for (int loopIndex = 1, loopTo = loopCount; loopIndex <= loopTo; loopIndex++)
                {
                    // Attempt to open the input file
                    if (!proteinFileReader.OpenFile(pathInfo.ProteinInputFilePath))
                    {
                        SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile);
                        success = false;
                        break;
                    }

                    if (CreateProteinOutputFile)
                    {
                        scramblingMode = ProteinScramblingMode;
                        residueCache.SamplingPercentage = ProteinScramblingSamplingPercentage;
                        if (residueCache.SamplingPercentage <= 0)
                            residueCache.SamplingPercentage = 100;
                        if (residueCache.SamplingPercentage > 100)
                            residueCache.SamplingPercentage = 100;
                        residueCache.Cache = string.Empty;
                        residueCache.CacheLength = SCRAMBLING_CACHE_LENGTH;
                        residueCache.OutputCount = 0;
                        residueCache.ResiduesToWrite = string.Empty;
                        if (scramblingMode != ProteinScramblingModeConstants.None)
                        {
                            success = InitializeScrambledOutput(pathInfo, residueCache, scramblingMode, out scrambledFileWriter, out randomNumberGenerator);
                            if (!success)
                                break;
                        }
                    }
                    else
                    {
                        scramblingMode = ProteinScramblingModeConstants.None;
                    }

                    startTime = DateTime.UtcNow;
                    if (CreateProteinOutputFile && mParsedFileIsFastaFile && allowLookForAddnlRefInDescription)
                    {
                        // Need to pre-scan the FASTA file to find all of the possible additional reference values

                        addnlRefMasterNames = new SortedSet<string>(StringComparer.CurrentCultureIgnoreCase);
                        PreScanProteinFileForAddnlRefsInDescription(pathInfo.ProteinInputFilePath, addnlRefMasterNames);
                        if (addnlRefMasterNames.Count > 0)
                        {
                            // Need to extract out the key names from addnlRefMasterNames and sort them alphabetically
                            addnlRefsToOutput = new AddnlRef[addnlRefMasterNames.Count];
                            lookForAddnlRefInDescription = true;
                            int index = 0;
                            foreach (var addnlRef in addnlRefMasterNames)
                            {
                                addnlRefsToOutput[index].RefName = string.Copy(addnlRef);
                                index += 1;
                            }

                            var iAddnlRefComparer = new AddnlRefComparer();
                            Array.Sort(addnlRefsToOutput, iAddnlRefComparer);
                        }
                        else
                        {
                            addnlRefsToOutput = new AddnlRef[1];
                            lookForAddnlRefInDescription = false;
                        }
                    }

                    ResetProgress("Parsing protein input file");
                    if (CreateProteinOutputFile && !CreateFastaOutputFile)
                    {
                        outLine.Clear();

                        // Write the header line to the output file
                        outLine.Append("ProteinName" + mOutputFileDelimiter);
                        if (lookForAddnlRefInDescription)
                        {
                            for (int index = 0, loopTo1 = addnlRefsToOutput.Length - 1; index <= loopTo1; index++)
                                outLine.Append(addnlRefsToOutput[index].RefName + mOutputFileDelimiter);
                        }

                        // Include Description in the header line, even if we are excluding the description for all proteins
                        outLine.Append("Description");
                        if (ComputeSequenceHashValues)
                        {
                            outLine.Append(mOutputFileDelimiter + "SequenceHash");
                        }

                        if (!ExcludeProteinSequence)
                        {
                            outLine.Append(mOutputFileDelimiter + "Sequence");
                        }

                        if (ComputeProteinMass)
                        {
                            if (ElementMassMode == PeptideSequence.ElementModeConstants.AverageMass)
                            {
                                outLine.Append(mOutputFileDelimiter + "Average Mass");
                            }
                            else
                            {
                                outLine.Append(mOutputFileDelimiter + "Mass");
                            }
                        }

                        if (ComputepI)
                        {
                            outLine.Append(mOutputFileDelimiter + "pI" + mOutputFileDelimiter + "Hydrophobicity");
                        }

                        if (ComputeNET)
                        {
                            outLine.Append(mOutputFileDelimiter + "LC_NET");
                        }

                        if (ComputeSCXNET)
                        {
                            outLine.Append(mOutputFileDelimiter + "SCX_NET");
                        }

                        proteinFileWriter.WriteLine(outLine.ToString());
                    }

                    // Read each protein in the input file and process appropriately
                    mProteinCount = 0;
                    mProteins = new ProteinInfo[501];
                    mInputFileProteinsProcessed = 0;
                    mInputFileLineSkipCount = 0;
                    mInputFileLinesRead = 0;
                    do
                    {
                        inputProteinFound = proteinFileReader.ReadNextProteinEntry();
                        mInputFileLineSkipCount += proteinFileReader.LineSkipCount;
                        if (!inputProteinFound)
                            continue;
                        if (!headerChecked)
                        {
                            if (!mParsedFileIsFastaFile)
                            {
                                headerChecked = true;

                                // This may be a header line; possibly skip it
                                if (proteinFileReader.ProteinName.ToLower().StartsWith("protein"))
                                {
                                    if (proteinFileReader.ProteinDescription.ToLower().Contains("description") && proteinFileReader.ProteinSequence.ToLower().Contains("sequence"))
                                    {
                                        // Skip this entry since it's a header line, for example:
                                        // ProteinName    Description    Sequence
                                        continue;
                                    }
                                }

                                if (proteinFileReader.ProteinName.ToLower().Contains("protein") && FractionLowercase(proteinFileReader.ProteinSequence) > 0.2d)
                                {
                                    // Skip this entry since it's a header line, for example:
                                    // FirstProtein    ProteinDesc    Sequence
                                    continue;
                                }
                            }
                        }

                        mInputFileProteinsProcessed += 1;
                        mInputFileLinesRead = proteinFileReader.LinesRead;
                        ParseProteinFileStoreProtein(proteinFileReader, lookForAddnlRefInDescription);
                        if (CreateProteinOutputFile)
                        {
                            if (loopIndex == 1)
                            {
                                if (CreateFastaOutputFile)
                                {
                                    ParseProteinFileWriteFasta(proteinFileWriter, outLine);
                                }
                                else
                                {
                                    ParseProteinFileWriteTextDelimited(proteinFileWriter, outLine, lookForAddnlRefInDescription, ref addnlRefsToOutput);
                                }
                            }

                            if (loopIndex == 1 && digestFileWriter is object)
                            {
                                ParseProteinFileWriteDigested(digestFileWriter, outLine, generateUniqueSequenceID);
                            }

                            if (scramblingMode != ProteinScramblingModeConstants.None)
                            {
                                WriteScrambledFasta(scrambledFileWriter, ref randomNumberGenerator, mProteins[mProteinCount], scramblingMode, ref residueCache);
                            }
                        }
                        else
                        {
                            // Cache the proteins in memory
                            mProteinCount += 1;
                            if (mProteinCount >= mProteins.Length)
                            {
                                Array.Resize(ref mProteins, mProteins.Length + PROTEIN_CACHE_MEMORY_RESERVE_COUNT + 1);
                            }
                        }

                        float percentProcessed = (loopIndex - 1) / (float)loopCount * 100.0f + proteinFileReader.PercentFileProcessed() / loopCount;
                        UpdateProgress(percentProcessed);
                        if (AbortProcessing)
                            break;
                    }
                    while (inputProteinFound);
                    if (CreateProteinOutputFile && scramblingMode != ProteinScramblingModeConstants.None)
                    {
                        // Write out anything remaining in the cache

                        string proteinNamePrefix;
                        if (scramblingMode == ProteinScramblingModeConstants.Reversed)
                        {
                            proteinNamePrefix = PROTEIN_PREFIX_REVERSED;
                        }
                        else
                        {
                            proteinNamePrefix = PROTEIN_PREFIX_SCRAMBLED;
                        }

                        WriteFastaAppendToCache(scrambledFileWriter, ref residueCache, proteinNamePrefix, true);
                    }

                    if (mProteinCount > 0)
                    {
                        Array.Resize(ref mProteins, mProteinCount);
                    }
                    else
                    {
                        mProteins = new ProteinInfo[1];
                    }

                    proteinFileReader.CloseFile();
                    if (proteinFileWriter is object)
                    {
                        proteinFileWriter.Close();
                    }

                    if (digestFileWriter is object)
                    {
                        digestFileWriter.Close();
                    }

                    if (scrambledFileWriter is object)
                    {
                        scrambledFileWriter.Close();
                    }
                }

                if (ShowDebugPrompts)
                {
                    string statusMessage = string.Format("{0}{1}Elapsed time: {2:F2} seconds", Path.GetFileName(pathInfo.ProteinInputFilePath), ControlChars.NewLine, DateTime.UtcNow.Subtract(startTime).TotalSeconds);
                    MessageBox.Show(statusMessage, "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                string message = "Done: Processed " + mInputFileProteinsProcessed.ToString("###,##0") + " proteins (" + mInputFileLinesRead.ToString("###,###,##0") + " lines)";
                if (mInputFileLineSkipCount > 0)
                {
                    message += ControlChars.NewLine + "Note that " + mInputFileLineSkipCount.ToString("###,##0") + " lines were skipped in the input file due to having an unexpected format. ";
                    if (mParsedFileIsFastaFile)
                    {
                        message += "This is an unexpected error for FASTA files.";
                    }
                    else
                    {
                        message += "Make sure that " + DelimitedFileFormatCode.ToString() + " is the appropriate format for this file (see the File Format Options tab).";
                    }
                }

                if (loopCount > 1)
                {
                    message += ControlChars.NewLine + "Created " + loopCount.ToString() + " replicates of the scrambled output file";
                }

                ProcessingSummary = message;
                OnStatusEvent(message);
                success = true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error in ParseProteinFile: " + ex.Message);
                if (CreateProteinOutputFile || CreateDigestedProteinOutputFile)
                {
                    SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorWritingOutputFile);
                    success = false;
                }
                else
                {
                    SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile);
                    success = false;
                }
            }
            finally
            {
                if (proteinFileWriter is object)
                {
                    proteinFileWriter.Close();
                }

                if (digestFileWriter is object)
                {
                    digestFileWriter.Close();
                }

                if (scrambledFileWriter is object)
                {
                    scrambledFileWriter.Close();
                }
            }

            return success;
        }

        private void ParseProteinFileStoreProtein(ProteinFileReaderBaseClass proteinFileReader, bool lookForAddnlRefInDescription)
        {
            mProteins[mProteinCount].Name = proteinFileReader.ProteinName;
            mProteins[mProteinCount].Description = proteinFileReader.ProteinDescription;
            if (TruncateProteinDescription && mProteins[mProteinCount].Description.Length > MAX_PROTEIN_DESCRIPTION_LENGTH)
            {
                mProteins[mProteinCount].Description = mProteins[mProteinCount].Description.Substring(0, MAX_PROTEIN_DESCRIPTION_LENGTH - 3) + "...";
            }

            if (lookForAddnlRefInDescription)
            {
                // Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
                mProteins[mProteinCount].AlternateNameCount = ExtractAlternateProteinNamesFromDescription(ref mProteins[mProteinCount].Description, ref mProteins[mProteinCount].AlternateNames);
            }
            else
            {
                mProteins[mProteinCount].AlternateNameCount = 0;
                mProteins[mProteinCount].AlternateNames = new AddnlRef[1];
            }

            string sequence = proteinFileReader.ProteinSequence;
            mProteins[mProteinCount].Sequence = sequence;
            if (ComputeSequenceHashIgnoreILDiff)
            {
                mProteins[mProteinCount].SequenceHash = HashUtilities.ComputeStringHashSha1(sequence.Replace('L', 'I')).ToUpper();
            }
            else
            {
                mProteins[mProteinCount].SequenceHash = HashUtilities.ComputeStringHashSha1(sequence).ToUpper();
            }

            if (ComputeProteinMass)
            {
                mProteins[mProteinCount].Mass = ComputeSequenceMass(sequence);
            }
            else
            {
                mProteins[mProteinCount].Mass = 0d;
            }

            if (ComputepI)
            {
                mProteins[mProteinCount].pI = ComputeSequencepI(sequence);
                mProteins[mProteinCount].Hydrophobicity = ComputeSequenceHydrophobicity(sequence);
            }
            else
            {
                mProteins[mProteinCount].pI = 0f;
                mProteins[mProteinCount].Hydrophobicity = 0f;
            }

            if (ComputeNET)
            {
                mProteins[mProteinCount].ProteinNET = ComputeSequenceNET(sequence);
            }

            if (ComputeSCXNET)
            {
                mProteins[mProteinCount].ProteinSCXNET = ComputeSequenceSCXNET(sequence);
            }
        }

        private void ParseProteinFileWriteDigested(TextWriter digestFileWriter, StringBuilder outLine, bool generateUniqueSequenceID)
        {
            List<InSilicoDigest.PeptideSequenceWithNET> peptideFragments = null;
            DigestProteinSequence(mProteins[mProteinCount].Sequence, out peptideFragments, DigestionOptions, mProteins[mProteinCount].Name);
            foreach (var peptideFragment in peptideFragments)
            {
                int uniqueSeqID;
                if (generateUniqueSequenceID)
                {
                    try
                    {
                        if (mMasterSequencesHashTable.ContainsKey(peptideFragment.SequenceOneLetter))
                        {
                            uniqueSeqID = Conversions.ToInteger(mMasterSequencesHashTable[peptideFragment.SequenceOneLetter]);
                        }
                        else
                        {
                            mMasterSequencesHashTable.Add(peptideFragment.SequenceOneLetter, mNextUniqueIDForMasterSeqs);
                            uniqueSeqID = mNextUniqueIDForMasterSeqs;
                        }

                        mNextUniqueIDForMasterSeqs += 1;
                    }
                    catch (Exception ex)
                    {
                        uniqueSeqID = 0;
                    }
                }
                else
                {
                    uniqueSeqID = 0;
                }

                string baseSequence = peptideFragment.SequenceOneLetter;
                outLine.Clear();
                if (!ExcludeProteinSequence)
                {
                    outLine.Append(mProteins[mProteinCount].Name + mOutputFileDelimiter);
                    if (DigestionOptions.IncludePrefixAndSuffixResidues)
                    {
                        outLine.Append(peptideFragment.PrefixResidue + "." + baseSequence + "." + peptideFragment.SuffixResidue + mOutputFileDelimiter);
                    }
                    else
                    {
                        outLine.Append(baseSequence + mOutputFileDelimiter);
                    }
                }

                outLine.Append(uniqueSeqID.ToString() + mOutputFileDelimiter + peptideFragment.Mass + mOutputFileDelimiter + Math.Round(peptideFragment.NET, 4).ToString() + mOutputFileDelimiter + peptideFragment.PeptideName);
                if (ComputepI)
                {
                    float pI = ComputeSequencepI(baseSequence);
                    float hydrophobicity = ComputeSequenceHydrophobicity(baseSequence);
                    outLine.Append(mOutputFileDelimiter + pI.ToString("0.000") + mOutputFileDelimiter + hydrophobicity.ToString("0.0000"));
                }

                if (ComputeNET)
                {
                    float lcNET = ComputeSequenceNET(baseSequence);
                    outLine.Append(mOutputFileDelimiter + lcNET.ToString("0.0000"));
                }

                if (ComputeSCXNET)
                {
                    float scxNET = ComputeSequenceSCXNET(baseSequence);
                    outLine.Append(mOutputFileDelimiter + scxNET.ToString("0.0000"));
                }

                digestFileWriter.WriteLine(outLine.ToString());
            }
        }

        private void ParseProteinFileWriteFasta(TextWriter proteinFileWriter, StringBuilder outLine)
        {
            // Write the entry to the output FASTA file

            if (mProteins[mProteinCount].Name == "ProteinName" && mProteins[mProteinCount].Description == "Description" && mProteins[mProteinCount].Sequence == "Sequence")
            {
                // Skip this entry; it's an artifact from converting from a FASTA file to a text file, then back to a FASTA file
                return;
            }

            outLine.Clear();
            outLine.Append(FastaFileOptions.ProteinLineStartChar + mProteins[mProteinCount].Name);
            if (!ExcludeProteinDescription)
            {
                outLine.Append(FastaFileOptions.ProteinLineAccessionEndChar + mProteins[mProteinCount].Description);
            }

            proteinFileWriter.WriteLine(outLine.ToString());
            if (!ExcludeProteinSequence)
            {
                int index = 0;
                while (index < mProteins[mProteinCount].Sequence.Length)
                {
                    int length = Math.Min(60, mProteins[mProteinCount].Sequence.Length - index);
                    proteinFileWriter.WriteLine(mProteins[mProteinCount].Sequence.Substring(index, length));
                    index += 60;
                }
            }
        }

        private void ParseProteinFileWriteTextDelimited(TextWriter proteinFileWriter, StringBuilder outLine, bool lookForAddnlRefInDescription, ref AddnlRef[] addnlRefsToOutput)
        {
            // Write the entry to the protein output file, and possibly digest it

            outLine.Clear();
            if (lookForAddnlRefInDescription)
            {
                // Reset the Accession numbers in addnlRefsToOutput
                for (int index = 0, loopTo = addnlRefsToOutput.Length - 1; index <= loopTo; index++)
                    addnlRefsToOutput[index].RefAccession = string.Empty;

                // Update the accession numbers in addnlRefsToOutput
                for (int index = 0, loopTo1 = mProteins[mProteinCount].AlternateNameCount - 1; index <= loopTo1; index++)
                {
                    for (int compareIndex = 0, loopTo2 = addnlRefsToOutput.Length - 1; compareIndex <= loopTo2; compareIndex++)
                    {
                        if ((addnlRefsToOutput[compareIndex].RefName.ToUpper() ?? "") == (mProteins[mProteinCount].AlternateNames[index].RefName.ToUpper() ?? ""))
                        {
                            addnlRefsToOutput[compareIndex].RefAccession = mProteins[mProteinCount].AlternateNames[index].RefAccession;
                            break;
                        }
                    }
                }

                outLine.Append(mProteins[mProteinCount].Name + mOutputFileDelimiter);
                for (int index = 0, loopTo3 = addnlRefsToOutput.Length - 1; index <= loopTo3; index++)
                    outLine.Append(addnlRefsToOutput[index].RefAccession + mOutputFileDelimiter);
                if (!ExcludeProteinDescription)
                {
                    outLine.Append(mProteins[mProteinCount].Description);
                }
            }
            else
            {
                outLine.Append(mProteins[mProteinCount].Name + mOutputFileDelimiter);
                if (!ExcludeProteinDescription)
                {
                    outLine.Append(mProteins[mProteinCount].Description);
                }
            }

            if (ComputeSequenceHashValues)
            {
                outLine.Append(mOutputFileDelimiter + mProteins[mProteinCount].SequenceHash);
            }

            if (!ExcludeProteinSequence)
            {
                outLine.Append(mOutputFileDelimiter + mProteins[mProteinCount].Sequence);
            }

            if (ComputeProteinMass)
            {
                outLine.Append(mOutputFileDelimiter + Math.Round(mProteins[mProteinCount].Mass, 5).ToString());
            }

            if (ComputepI)
            {
                outLine.Append(mOutputFileDelimiter + mProteins[mProteinCount].pI.ToString("0.000") + mOutputFileDelimiter + mProteins[mProteinCount].Hydrophobicity.ToString("0.0000"));
            }

            if (ComputeNET)
            {
                outLine.Append(mOutputFileDelimiter + mProteins[mProteinCount].ProteinNET.ToString("0.0000"));
            }

            if (ComputeSCXNET)
            {
                outLine.Append(mOutputFileDelimiter + mProteins[mProteinCount].ProteinSCXNET.ToString("0.0000"));
            }

            proteinFileWriter.WriteLine(outLine.ToString());
        }

        private bool ParseProteinFileCreateOutputFile(ref FilePathInfo pathInfo, out ProteinFileReaderBaseClass proteinFileReader)
        {
            string outputFileName;
            bool success;
            if (AssumeFastaFile || IsFastaFile(pathInfo.ProteinInputFilePath, true))
            {
                if (AssumeDelimitedFile)
                {
                    mParsedFileIsFastaFile = false;
                }
                else
                {
                    mParsedFileIsFastaFile = true;
                }
            }
            else
            {
                mParsedFileIsFastaFile = false;
            }

            if (CreateDigestedProteinOutputFile)
            {
                // Make sure mCreateFastaOutputFile is false
                CreateFastaOutputFile = false;
            }

            if (!string.IsNullOrEmpty(pathInfo.OutputFileNameBaseOverride))
            {
                if (Path.HasExtension(pathInfo.OutputFileNameBaseOverride))
                {
                    outputFileName = string.Copy(pathInfo.OutputFileNameBaseOverride);
                    if (CreateFastaOutputFile)
                    {
                        if (!Path.GetExtension(outputFileName).Equals(".fasta", StringComparison.OrdinalIgnoreCase))
                        {
                            outputFileName += ".fasta";
                        }
                    }
                    else if (Path.GetExtension(outputFileName).Length > 4)
                    {
                        outputFileName += ".txt";
                    }
                }
                else if (CreateFastaOutputFile)
                {
                    outputFileName = pathInfo.OutputFileNameBaseOverride + ".fasta";
                }
                else
                {
                    outputFileName = pathInfo.OutputFileNameBaseOverride + ".txt";
                }
            }
            else
            {
                outputFileName = string.Empty;
            }

            if (mParsedFileIsFastaFile)
            {
                var reader = new FastaFileReader();
                proteinFileReader = reader;
            }
            else
            {
                var reader = new DelimitedProteinFileReader()
                {
                    Delimiter = mInputFileDelimiter,
                    DelimitedFileFormatCode = DelimitedFileFormatCode
                };
                proteinFileReader = reader;
            }

            // Verify that the input file exists
            if (!File.Exists(pathInfo.ProteinInputFilePath))
            {
                SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidInputFilePath);
                success = false;
                return success;
            }

            if (mParsedFileIsFastaFile)
            {
                if (outputFileName.Length == 0)
                {
                    outputFileName = Path.GetFileName(pathInfo.ProteinInputFilePath);
                    if (Path.GetExtension(outputFileName).Equals(".fasta", StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    // Nothing special to do; will replace the extension below

                    else if (outputFileName.EndsWith(".fasta.gz", StringComparison.OrdinalIgnoreCase))
                    {
                        // Remove .gz from outputFileName
                        outputFileName = StripExtension(outputFileName, ".gz");
                    }
                    else
                    {
                        // .Fasta appears somewhere in the middle
                        // Remove the text .Fasta, then add the extension .txt (unless it already ends in .txt)
                        int charIndex = outputFileName.ToLower().LastIndexOf(".fasta", StringComparison.Ordinal);
                        if (charIndex > 0)
                        {
                            if (charIndex < outputFileName.Length)
                            {
                                outputFileName = outputFileName.Substring(0, charIndex) + outputFileName.Substring(charIndex + 6);
                            }
                            else
                            {
                                outputFileName = outputFileName.Substring(0, charIndex);
                            }
                        }
                        else
                        {
                            // This shouldn't happen
                        }
                    }

                    if (CreateFastaOutputFile)
                    {
                        outputFileName = Path.GetFileNameWithoutExtension(outputFileName) + "_new.fasta";
                    }
                    else
                    {
                        outputFileName = Path.ChangeExtension(outputFileName, ".txt");
                    }
                }
            }
            else if (outputFileName.Length == 0)
            {
                if (CreateFastaOutputFile)
                {
                    outputFileName = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) + ".fasta";
                }
                else
                {
                    outputFileName = Path.GetFileNameWithoutExtension(pathInfo.ProteinInputFilePath) + "_parsed.txt";
                }
            }

            // Make sure the output file isn't the same as the input file
            if ((Path.GetFileName(pathInfo.ProteinInputFilePath).ToLower() ?? "") == (Path.GetFileName(outputFileName).ToLower() ?? ""))
            {
                outputFileName = Path.GetFileNameWithoutExtension(outputFileName) + "_new" + Path.GetExtension(outputFileName);
            }

            // Define the full path to the parsed proteins output file
            pathInfo.ProteinOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, outputFileName);
            if (outputFileName.EndsWith("_parsed.txt"))
            {
                outputFileName = outputFileName.Substring(0, outputFileName.Length - "_parsed.txt".Length) + "_digested";
            }
            else
            {
                outputFileName = Path.GetFileNameWithoutExtension(outputFileName) + "_digested";
            }

            outputFileName += "_Mass" + Math.Round((decimal)DigestionOptions.MinFragmentMass, 0).ToString() + "to" + Math.Round((decimal)DigestionOptions.MaxFragmentMass, 0).ToString();
            if (ComputepI && (DigestionOptions.MinIsoelectricPoint > 0f || DigestionOptions.MaxIsoelectricPoint < 14f))
            {
                outputFileName += "_pI" + Math.Round(DigestionOptions.MinIsoelectricPoint, 1).ToString() + "to" + Math.Round(DigestionOptions.MaxIsoelectricPoint, 2).ToString();
            }

            outputFileName += ".txt";

            // Define the full path to the digested proteins output file
            pathInfo.DigestedProteinOutputFilePath = Path.Combine(pathInfo.OutputFolderPath, outputFileName);
            success = true;
            return success;
        }

        private bool ParseProteinFileCreateDigestedProteinOutputFile(string digestedProteinOutputFilePath, ref StreamWriter digestFileWriter)
        {
            try
            {
                // Create the digested protein output file
                digestFileWriter = new StreamWriter(digestedProteinOutputFilePath);
                string lineOut;
                if (!ExcludeProteinSequence)
                {
                    lineOut = "Protein_Name" + mOutputFileDelimiter + "Sequence" + mOutputFileDelimiter;
                }
                else
                {
                    lineOut = string.Empty;
                }

                lineOut += "Unique_ID" + mOutputFileDelimiter + "Monoisotopic_Mass" + mOutputFileDelimiter + "Predicted_NET" + mOutputFileDelimiter + "Tryptic_Name";
                if (ComputepI)
                {
                    lineOut += mOutputFileDelimiter + "pI" + mOutputFileDelimiter + "Hydrophobicity";
                }

                if (ComputeNET)
                {
                    lineOut += mOutputFileDelimiter + "LC_NET";
                }

                if (ComputeSCXNET)
                {
                    lineOut += mOutputFileDelimiter + "SCX_NET";
                }

                digestFileWriter.WriteLine(lineOut);
                return true;
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorCreatingDigestedProteinOutputFile);
                return false;
            }
        }

        private void PreScanProteinFileForAddnlRefsInDescription(string proteinInputFilePath, ISet<string> addnlRefMasterNames)
        {
            FastaFileReader reader = null;
            ProteinInfo protein;
            int index;
            bool success;
            bool inputProteinFound;
            try
            {
                reader = new FastaFileReader();

                // Attempt to open the input file
                if (!reader.OpenFile(proteinInputFilePath))
                {
                    success = false;
                    return;
                }

                success = true;
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile);
                success = false;
            }

            // Abort processing if we couldn't successfully open the input file
            if (!success)
                return;
            try
            {
                ResetProgress("Pre-reading FASTA file; looking for possible additional reference names");

                // Read each protein in the output file and process appropriately
                do
                {
                    inputProteinFound = reader.ReadNextProteinEntry();
                    if (inputProteinFound)
                    {
                        protein.Name = reader.ProteinName;
                        protein.Description = reader.ProteinDescription;

                        // Look for additional protein names in .Description, delimited by FastaFileOptions.AddnlRefSepChar
                        protein.AlternateNames = new AddnlRef[1];
                        protein.AlternateNameCount = ExtractAlternateProteinNamesFromDescription(ref protein.Description, ref protein.AlternateNames);

                        // Make sure each of the names in .AlternateNames() is in addnlRefMasterNames
                        var loopTo = protein.AlternateNameCount - 1;
                        for (index = 0; index <= loopTo; index++)
                        {
                            if (!addnlRefMasterNames.Contains(protein.AlternateNames[index].RefName))
                            {
                                addnlRefMasterNames.Add(protein.AlternateNames[index].RefName);
                            }
                        }

                        UpdateProgress(reader.PercentFileProcessed());
                        if (AbortProcessing)
                            break;
                    }
                }
                while (inputProteinFound);
                reader.CloseFile();
            }
            catch (Exception ex)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.ErrorReadingInputFile);
            }

            return;
        }

        /// <summary>
        /// If filePath ends with extension, remove it
        /// Supports multi-part extensions like .fasta.gz
        /// </summary>
        /// <param name="filePath">File name or path</param>
        /// <param name="extension">Extension, with or without the leading period</param>
        /// <returns></returns>
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
            var sepChars = new char[] { ' ', ',', ';', ':', '_', '-', '|', '/' };
            if (maximumLength < 1)
                maximumLength = 1;
            if (proteinName is null)
            {
                proteinName = string.Empty;
            }
            else if (proteinName.Length > maximumLength)
            {
                // Truncate protein name to maximum length
                proteinName = proteinName.Substring(0, maximumLength);

                // Make sure the protein name doesn't end in a space, dash, underscore, semicolon, colon, etc.
                proteinName = proteinName.TrimEnd(sepChars);
            }

            return proteinName;
        }

        private void WriteFastaAppendToCache(TextWriter scrambledFileWriter, ref ScramblingResidueCache residueCache, string proteinNamePrefix, bool flushResiduesToWrite)
        {
            int residueCount;
            int residuesToAppend;
            if (residueCache.Cache.Length > 0)
            {
                residueCount = (int)Math.Round(Math.Round(residueCache.Cache.Length * residueCache.SamplingPercentage / 100.0d, 0));
                if (residueCount < 1)
                    residueCount = 1;
                if (residueCount > residueCache.Cache.Length)
                    residueCount = residueCache.Cache.Length;
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
                        residuesToAppend = residueCache.CacheLength - residueCache.ResiduesToWrite.Length;
                        residueCache.ResiduesToWrite += residueCache.Cache.Substring(0, residuesToAppend);
                        residueCache.Cache = residueCache.Cache.Substring(residuesToAppend);
                        residueCount -= residuesToAppend;
                    }

                    if (residueCache.ResiduesToWrite.Length >= residueCache.CacheLength)
                    {
                        // Write out .ResiduesToWrite
                        WriteFastaEmptyCache(scrambledFileWriter, ref residueCache, proteinNamePrefix, residueCache.SamplingPercentage);
                    }
                }
            }

            if (flushResiduesToWrite && residueCache.ResiduesToWrite.Length > 0)
            {
                WriteFastaEmptyCache(scrambledFileWriter, ref residueCache, proteinNamePrefix, residueCache.SamplingPercentage);
            }
        }

        private void WriteFastaEmptyCache(TextWriter scrambledFileWriter, ref ScramblingResidueCache residueCache, string proteinNamePrefix, int samplingPercentage)
        {
            string proteinName;
            string headerLine;
            if (residueCache.ResiduesToWrite.Length > 0)
            {
                residueCache.OutputCount += 1;
                proteinName = proteinNamePrefix + mFileNameAbbreviated;
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
                    proteinName += "_" + samplingPercentage.ToString() + "pct" + "_";
                }
                else
                {
                    proteinName += proteinName + "_";
                }

                proteinName += residueCache.OutputCount.ToString();
                headerLine = FastaFileOptions.ProteinLineStartChar + proteinName + FastaFileOptions.ProteinLineAccessionEndChar + proteinName;
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

        private void WriteScrambledFasta(TextWriter scrambledFileWriter, ref Random randomNumberGenerator, ProteinInfo protein, ProteinScramblingModeConstants scramblingMode, ref ScramblingResidueCache residueCache)
        {
            string sequence;
            string scrambledSequence;
            string headerLine;
            string proteinNamePrefix;
            string proteinName;
            int index;
            int residueCount;
            if (scramblingMode == ProteinScramblingModeConstants.Reversed)
            {
                proteinNamePrefix = PROTEIN_PREFIX_REVERSED;
                scrambledSequence = Strings.StrReverse(protein.Sequence);
            }
            else
            {
                proteinNamePrefix = PROTEIN_PREFIX_SCRAMBLED;
                scrambledSequence = string.Empty;
                sequence = string.Copy(protein.Sequence);
                residueCount = sequence.Length;
                while (residueCount > 0)
                {
                    if (residueCount != sequence.Length)
                    {
                        ShowErrorMessage("Assertion failed in WriteScrambledFasta: residueCount should equal sequence.Length: " + residueCount + " vs. " + sequence.Length);
                    }

                    // Randomly extract residues from sequence
                    index = randomNumberGenerator.Next(residueCount);
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

                    residueCount -= 1;
                }
            }

            if (residueCache.SamplingPercentage >= 100)
            {
                proteinName = ValidateProteinName(proteinNamePrefix + protein.Name, MAXIMUM_PROTEIN_NAME_LENGTH);
                headerLine = FastaFileOptions.ProteinLineStartChar + proteinName + FastaFileOptions.ProteinLineAccessionEndChar + protein.Description;
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
                        WriteFastaAppendToCache(scrambledFileWriter, ref residueCache, proteinNamePrefix, false);
                    }
                }
            }
        }

        /// <summary>
        /// Main processing function -- Calls ParseProteinFile
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <param name="outputFolderPath"></param>
        /// <param name="parameterFilePath"></param>
        /// <param name="resetErrorCode"></param>
        /// <returns></returns>
        public override bool ProcessFile(string inputFilePath, string outputFolderPath, string parameterFilePath, bool resetErrorCode)
        {
            // Returns True if success, False if failure

            FileInfo inputFile;
            string inputFilePathFull;
            string statusMessage;
            var success = default(bool);
            if (resetErrorCode)
            {
                SetLocalErrorCode(ParseProteinFileErrorCodes.NoError);
            }

            if (!LoadParameterFileSettings(parameterFilePath))
            {
                statusMessage = "Parameter file load error: " + parameterFilePath;
                ShowErrorMessage(statusMessage);
                if (ErrorCode == ProcessFilesErrorCodes.NoError)
                {
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.InvalidParameterFile);
                }

                return false;
            }

            try
            {
                if (inputFilePath is null || inputFilePath.Length == 0)
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
                            inputFile = new FileInfo(inputFilePath);
                            inputFilePathFull = inputFile.FullName;
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

        private void SetLocalErrorCode(ParseProteinFileErrorCodes newErrorCode)
        {
            SetLocalErrorCode(newErrorCode, false);
        }

        private void SetLocalErrorCode(ParseProteinFileErrorCodes newErrorCode, bool leaveExistingErrorCodeUnchanged)
        {
            if (leaveExistingErrorCodeUnchanged && mLocalErrorCode != ParseProteinFileErrorCodes.NoError)
            {
            }
            // An error code is already defined; do not change it
            else
            {
                mLocalErrorCode = newErrorCode;
                if (newErrorCode == ParseProteinFileErrorCodes.NoError)
                {
                    if (ErrorCode == ProcessFilesErrorCodes.LocalizedError)
                    {
                        SetBaseClassErrorCode(ProcessFilesErrorCodes.NoError);
                    }
                }
                else
                {
                    SetBaseClassErrorCode(ProcessFilesErrorCodes.LocalizedError);
                }
            }
        }

        protected void UpdateSubtaskProgress(string description)
        {
            UpdateProgress(description, mProgressPercentComplete);
        }

        protected void UpdateSubtaskProgress(float percentComplete)
        {
            UpdateProgress(ProgressStepDescription, percentComplete);
        }

        protected void UpdateSubtaskProgress(string description, float percentComplete)
        {
            bool descriptionChanged = (description ?? "") != (mSubtaskProgressStepDescription ?? "");
            mSubtaskProgressStepDescription = string.Copy(description);
            if (percentComplete < 0f)
            {
                percentComplete = 0f;
            }
            else if (percentComplete > 100f)
            {
                percentComplete = 100f;
            }

            mSubtaskProgressPercentComplete = percentComplete;
            if (descriptionChanged)
            {
                if (Math.Abs(mSubtaskProgressPercentComplete - 0f) < float.Epsilon)
                {
                    LogMessage(mSubtaskProgressStepDescription.Replace(ControlChars.NewLine, "; "));
                }
                else
                {
                    LogMessage(mSubtaskProgressStepDescription + " (" + mSubtaskProgressPercentComplete.ToString("0.0") + "% complete)".Replace(ControlChars.NewLine, "; "));
                }
            }

            SubtaskProgressChanged?.Invoke(description, percentComplete);
        }

        // IComparer class to allow comparison of additional protein references
        private class AddnlRefComparer : IComparer<AddnlRef>
        {
            public int Compare(AddnlRef x, AddnlRef y)
            {
                if (Operators.CompareString(x.RefName, y.RefName, false) > 0)
                {
                    return 1;
                }
                else if (Operators.CompareString(x.RefName, y.RefName, false) < 0)
                {
                    return -1;
                }
                else if (Operators.CompareString(x.RefAccession, y.RefAccession, false) > 0)
                {
                    return 1;
                }
                else if (Operators.CompareString(x.RefAccession, y.RefAccession, false) < 0)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        // Options class
        public class FastaFileParseOptions
        {
            public FastaFileParseOptions()
            {
                ProteinLineStartChar = '>';
                ProteinLineAccessionEndChar = ' ';
                mLookForAddnlRefInDescription = false;
                mAddnlRefSepChar = '|';
                mAddnlRefAccessionSepChar = ':';
            }

            private bool mReadonly;
            private bool mLookForAddnlRefInDescription;
            private char mAddnlRefSepChar;
            private char mAddnlRefAccessionSepChar;

            public bool ReadonlyClass
            {
                get
                {
                    return mReadonly;
                }

                set
                {
                    if (!mReadonly)
                    {
                        mReadonly = value;
                    }
                }
            }

            public char ProteinLineStartChar { get; private set; } = '>';
            public char ProteinLineAccessionEndChar { get; private set; } = ' ';

            public bool LookForAddnlRefInDescription
            {
                get
                {
                    return mLookForAddnlRefInDescription;
                }

                set
                {
                    if (!mReadonly)
                    {
                        mLookForAddnlRefInDescription = value;
                    }
                }
            }

            public char AddnlRefSepChar
            {
                get
                {
                    return mAddnlRefSepChar;
                }

                set
                {
                    if (value != default && !mReadonly)
                    {
                        mAddnlRefSepChar = value;
                    }
                }
            }

            public char AddnlRefAccessionSepChar
            {
                get
                {
                    return mAddnlRefAccessionSepChar;
                }

                set
                {
                    if (value != default && !mReadonly)
                    {
                        mAddnlRefAccessionSepChar = value;
                    }
                }
            }
        }

        private void InSilicoDigest_ErrorEvent(string message)
        {
            ShowErrorMessage("Error in mInSilicoDigest: " + message);
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