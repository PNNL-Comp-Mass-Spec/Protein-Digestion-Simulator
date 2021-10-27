using System;
using PRISM;
using ProteinFileReader;

namespace ProteinDigestionSimulator.Options
{
    public class DigestionSimulatorOptions : EventNotifier
    {
        // Ignore Spelling: Cysteine, Hydrophobicity, Ile, isoleucine, Leu, leucine, silico

        private int mSequenceLengthToExamineForMaximumpI;

        /// <summary>
        /// Input file path
        /// </summary>
        /// <remarks>.fasta, .fasta.gz, or tab-delimited .txt file</remarks>
        [Option("Input", "InputFile", "I",
            ArgPosition = 1, Required = true, IsInputFilePath = true,
            HelpText = "The name of the FASTA file to process (.fasta, .faa, .fasta.gz, or .faa.gz); " +
                       "alternatively, a tab-delimited text file with protein info. " +
                       "Either define this at the command line using /I or in a parameter file. " +
                       "When using /I at the command line, surround the filename with double quotes if it contains spaces")]
        public string InputFilePath { get; set; }

        /// <summary>
        /// When true, assume that the input file is a FASTA file
        /// </summary>
        [Option("AssumeFasta", "F",
            HelpText = "Optionally set this to true to indicate that the input file is a FASTA file. " +
                       "If false, the format will be assumed to be FASTA only if the filename ends with .fasta, .faa, .fasta.gz, or .faa.gz")]
        public bool AssumeFastaFile { get; set; }

        /// <summary>
        /// When true, assume that the input file is a tab-delimited text file
        /// </summary>
        /// <remarks>
        /// This property is only used by the GUI
        /// </remarks>
        public bool AssumeDelimitedFile { get; set; }

        [Option("DelimitedFileFormatCode", "DelimitedFileFormat",
            HelpText = "Specifies the column order in delimited input files; ignored if the input file is a FASTA file")]
        public DelimitedProteinFileReader.ProteinFileFormatCode DelimitedFileFormatCode { get; set; }

        [Option("DelimitedFileHasHeaderLine", "InputFileHasHeader",
            HelpText = "When true, assume the delimited input file has a header line")]
        public bool DelimitedFileHasHeaderLine { get; set; }

        /// <summary>
        /// Input file delimiter
        /// </summary>
        /// <remarks>
        /// Only used for delimited protein input files, not for FASTA files
        /// </remarks>
        private char mInputFileDelimiter;

        /// <summary>
        /// Input file delimiter
        /// </summary>
        [Option("InputFileDelimiter", "InputDelimiter", "AD",
            HelpText = "Input file delimiter (only used for delimited protein input files, not FASTA files); " +
                       "default is the tab character",
            HelpShowsDefault = false)]
        public char InputFileDelimiter
        {
            get => mInputFileDelimiter;
            set
            {
                if (value != default(char))
                {
                    mInputFileDelimiter = value;
                }
            }
        }

        [Option("ExcludeProteinDescription", "ExcludeDescription",
            HelpText = "When true, do not include protein description in the output file")]
        public bool ExcludeProteinDescription { get; set; }

        [Option("ExcludeProteinSequence", "ExcludeSequence",
            HelpText = "When true, do not include protein sequence in the output file")]
        public bool ExcludeProteinSequence { get; set; }

        [Option("TruncateProteinDescription", "TruncateDescription",
            HelpText = "When true, truncate protein descriptions longer than 7995 characters")]
        public bool TruncateProteinDescription { get; set; }

        /// <summary>
        /// Output directory path
        /// </summary>
        [Option("Output", "OutputDirectory", "O",
            HelpText = "Output directory name (or full path). " +
                       "If omitted, the output files will be created in the program directory")]
        public string OutputDirectoryPath { get; set; }

        private char mOutputFileDelimiter;

        /// <summary>
        /// Output file delimiter
        /// </summary>
        [Option("OutputFileDelimiter",
            HelpText = "Output file delimiter; default is a tab character",
            HelpShowsDefault = false)]
        public char OutputFileDelimiter
        {
            get => mOutputFileDelimiter;
            set
            {
                if (value != default(char))
                {
                    mOutputFileDelimiter = value;
                }
            }
        }

        [Option("Recurse", "S",
            HelpText = "Search in subdirectories")]
        public bool RecurseDirectories { get; set; }

        [Option("RecurseDepth",
            HelpText = "When searching in subdirectories, the maximum depth to search. " +
                       "When 0 or negative, recurse infinitely. " +
                       "When 1, only process the current directory. " +
                       "When 2, process the current directory and files in its subdirectories.")]
        public int MaxLevelsToRecurse { get; set; }

        [Option("AlternateOutputDirectory", "A",
            HelpText = "Optionally provide a directory path to write results to when searching in subdirectories")]
        public string OutputDirectoryAlternatePath { get; set; }

        [Option("RecreateDirStructure", "R",
            HelpText = "When searching in subdirectories, if an alternate output directory has been defined, " +
                       "this can be set to true to re-create the input directory hierarchy in the alternate output directory")]
        public bool RecreateDirectoryHierarchyInAlternatePath { get; set; }

        public bool LogEnabled { get; set; }

        [Option("LogFile", "Log", "L",
            HelpText = "If specified, write to a log file. Can optionally provide a log file path", ArgExistsProperty = nameof(LogEnabled))]
        public string LogFilePath { get; set; }

        [Option("ComputeNET",
            HelpText = "When true, compute predicted elution time values for digested peptides and/or for entries in a tab-delimited text output file")]
        public bool ComputeNET { get; set; }

        [Option("ComputepI", "ComputeIsoelectricPoint",
            HelpText = "When true, compute pI values for digested peptides and/or for entries in a tab-delimited text output file")]
        public bool ComputepI { get; set; }

        [Option("ComputeSCX",
            HelpText = "When true, compute predicted SCX values for digested peptides and/or for entries in a tab-delimited text output file")]
        public bool ComputeSCXNET { get; set; }

        [Option("HydrophobicityMode", "Hydrophobicity",
            HelpText = "Hydrophobicity mode to use when ComputepI is true")]
        public HydrophobicityTypeConstants HydrophobicityMode { get; set; }

        [Option("ReportMaximumpI", "ReportMaxpI",
            HelpText = "When ComputepI is true, set this to true to examine the protein residues in chunks of SequenceLengthMaximumpI, " +
                       "compute the pI for each chunk, then report the largest pI")]
        public bool ReportMaximumpI { get; set; }

        [Option("SequenceLengthMaximumpI", "SequenceLengthMaxpI",
            HelpText = "When ReportMaxpI is true, use this to specify the number of residues to group together for computing localized pI")]
        public int SequenceLengthToExamineForMaximumpI
        {
            get => mSequenceLengthToExamineForMaximumpI;
            set
            {
                if (value < 1)
                {
                    mSequenceLengthToExamineForMaximumpI = 1;
                }

                mSequenceLengthToExamineForMaximumpI = value;
            }
        }

        [Option("ComputeProteinMass", "ComputeMass", "Mass", "M",
            HelpText = "When true, compute mass values for digested peptides and/or for entries in a tab-delimited text output file")]
        public bool ComputeProteinMass { get; set; }

        [Option("ElementMassMode", "MassMode",
            HelpText = "Mass mode to use when computing mass values")]
        public PeptideSequence.ElementModeConstants ElementMassMode { get; set; }

        [Option("IncludeXResiduesInMass", "IncludeXResidues",
            HelpText = "When true, include X residues when computing protein mass (using the mass of Ile/Leu)")]
        public bool IncludeXResiduesInMass { get; set; }

        [Option("DigestProteins", "Digest", "D",
            HelpText = "When true, in-silico digest the proteins in the input file")]
        public bool CreateDigestedProteinOutputFile { get; set; }

        [Option("DigestionEnzyme", "CleavageRuleID",
            HelpText = "Enzyme to use for protein digestion")]
        public CleavageRuleConstants DigestionEnzyme
        {
            get => DigestionOptions.CleavageRuleID;
            set => DigestionOptions.CleavageRuleID = value;
        }

        [Option("MaxMissedCleavages",
            HelpText = "Maximum number of missed cleavages when digesting proteins")]
        public int MaxMissedCleavages
        {
            get => DigestionOptions.MaxMissedCleavages;
            set => DigestionOptions.MaxMissedCleavages = value;
        }

        [Option("MinimumFragmentResidueCount", "MinimumResidueCount",
            HelpText = "Minimum length of peptides to allow when digesting proteins")]
        public int MinFragmentResidueCount
        {
            get => DigestionOptions.MinFragmentResidueCount;
            set => DigestionOptions.MinFragmentResidueCount = value;
        }

        [Option("MinimumFragmentMass",
            HelpText = "Maximum fragment mass to allow when digesting proteins")]
        public int MinFragmentMass
        {
            get => DigestionOptions.MinFragmentMass;
            set => DigestionOptions.MinFragmentMass = value;
        }

        [Option("MaximumFragmentMass",
            HelpText = "Maximum fragment mass to allow when digesting proteins")]
        public int MaxFragmentMass
        {
            get => DigestionOptions.MaxFragmentMass;
            set => DigestionOptions.MaxFragmentMass = value;
        }

        [Option("FragmentMassMode",
            HelpText = "Fragment mass mode to use when digesting proteins")]
        public FragmentMassConstants FragmentMassMode
        {
            get => DigestionOptions.FragmentMassMode;
            set => DigestionOptions.FragmentMassMode = value;
        }

        [Option("MinimumIsoelectricPoint", "MinimumpI",
            HelpText = "Minimum pI value to allow when digesting proteins; only used if ComputepI is true")]
        public float MinIsoelectricPoint
        {
            get => DigestionOptions.MinIsoelectricPoint;
            set => DigestionOptions.MinIsoelectricPoint = value;
        }

        [Option("MaximumIsoelectricPoint", "MaximumpI",
            HelpText = "Maximum pI value to allow when digesting proteins; only used if ComputepI is true")]
        public float MaxIsoelectricPoint
        {
            get => DigestionOptions.MaxIsoelectricPoint;
            set => DigestionOptions.MaxIsoelectricPoint = value;
        }

        [Option("CysTreatmentMode",
            HelpText = "Cysteine treatment mode to use when digesting proteins")]
        public PeptideSequence.CysTreatmentModeConstants CysTreatmentMode
        {
            get => DigestionOptions.CysTreatmentMode;
            set => DigestionOptions.CysTreatmentMode = value;
        }

        [Option("CysPeptidesOnly",
            HelpText = "If true, only report peptides with at least one cysteine residue when digesting proteins")]
        public bool CysPeptidesOnly
        {
            get => DigestionOptions.CysPeptidesOnly;
            set => DigestionOptions.CysPeptidesOnly = value;
        }

        [Option("RemoveDuplicateSequences", "RemoveDuplicates",
            HelpText = "If true, remove duplicate sequences when digesting proteins")]
        public bool RemoveDuplicateSequences
        {
            get => DigestionOptions.RemoveDuplicateSequences;
            set => DigestionOptions.RemoveDuplicateSequences = value;
        }

        [Option("IncludePrefixAndSuffixResidues", "IncludePrefixAndSuffix",
            HelpText = "If true, include prefix and suffix residues in the output file when digesting proteins")]
        public bool IncludePrefixAndSuffixResidues
        {
            get => DigestionOptions.IncludePrefixAndSuffixResidues;
            set => DigestionOptions.IncludePrefixAndSuffixResidues = value;
        }

        /// <summary>
        /// True to create a FASTA output file; false for a tab-delimited text file
        /// </summary>
        /// <remarks>
        /// <para>
        /// Set this to true if the input file is a tab-delimited file and you want to convert it to a FASTA file
        /// </para>
        /// <para>
        /// This is only valid if CreateDigestedProteinOutputFile is False
        /// </para>
        /// </remarks>
        [Option("CreateFastaOutputFile", "FastaOutputFile", "FastaOutput",
            HelpText = "When true, write the output file as a FASTA file; only valid if DigestProteins is false. " +
                       "An error will occur if the input file is a FASTA file")]
        public bool CreateFastaOutputFile { get; set; }

        [Option("CreateProteinOutputFile", "ProteinOutputFile", "ProteinOutput",
            HelpText = "When true, writes the proteins to a file; when false, caches the results in memory")]
        public bool CreateProteinOutputFile { get; set; }

        [Option("ComputeHashValues", "Hash", "ComputeSequenceHashValues",
            HelpText = "When true, compute sequence hash values for digested peptides and/or for entries in a tab-delimited text output file")]
        public bool ComputeSequenceHashValues { get; set; }

        [Option("SequenceHashIgnoreILDiff", "IgnoreILDiff", "ComputeSequenceHashIgnoreILDiff",
            HelpText = "When true and computing sequence hash values, treat isoleucine and leucine as the same residue")]
        public bool ComputeSequenceHashIgnoreILDiff { get; set; }

        [Option("GenerateUniqueIDs",
            HelpText = "When true, assign UniqueID values to the digested peptides (requires more memory); " +
                       "only valid if CreateDigestedProteinOutputFile is True")]
        public bool GenerateUniqueIDValuesForPeptides { get; set; }

        [Option("ScrambleMode", "ProteinScramblingMode",
            HelpText = "When creating a protein output file (and thus not digesting), " +
                       "optionally use this to specify that reversed or scrambled sequences be written")]
        public ProteinFileParser.ProteinScramblingModeConstants ProteinScramblingMode { get; set; }

        [Option("ScrambleSamplingPercentage", "ScrambleSamplePercent",
            HelpText = "When creating a reversed or scrambled output file, " +
                       "set this to a value less than 100 to only include a portion of the residues from the input file in the output file")]
        public int ProteinScramblingSamplingPercentage { get; set; }

        [Option("ScrambledFileCount", "ScrambleCount",
            HelpText = "When creating a reversed or scrambled output file, " +
                       "set this to a value greater than 1 to create multiple scrambled versions of the input file")]
        public int ProteinScramblingLoopCount { get; set; }

        [Option("QuietMode", "Q",
            HelpText = "When true, do not show protein names while digesting proteins")]
        public bool QuietMode { get; set; }

        /// <summary>
        /// In-silico digestion options
        /// </summary>
        public DigestionOptions DigestionOptions { get; }

        /// <summary>
        /// FASTA file parsing options
        /// </summary>
        public ProteinFileParser.FastaFileParseOptions FastaFileOptions { get; }

        /// <summary>
        /// Peak matching options when computing peptide uniqueness
        /// </summary>
        public PeakMatchingOptions PeakMatchingOptions { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public DigestionSimulatorOptions()
        {
            DigestionOptions = new DigestionOptions();

            DigestionEnzyme = CleavageRuleConstants.ConventionalTrypsin;
            MaxMissedCleavages = 3;
            MinFragmentResidueCount = 0;

            MinFragmentMass = 400;
            MaxFragmentMass = 6000;

            FragmentMassMode = FragmentMassConstants.Monoisotopic;

            MinIsoelectricPoint = 0;
            MaxIsoelectricPoint = 14;

            CysTreatmentMode = PeptideSequence.CysTreatmentModeConstants.Untreated;
            CysPeptidesOnly = false;

            RemoveDuplicateSequences = false;
            IncludePrefixAndSuffixResidues = false;

            FastaFileOptions = new ProteinFileParser.FastaFileParseOptions();

            PeakMatchingOptions = new PeakMatchingOptions();

            InputFilePath = string.Empty;
            AssumeFastaFile = false;
            AssumeDelimitedFile = false;

            DelimitedFileFormatCode = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence;
            DelimitedFileHasHeaderLine = true;
            InputFileDelimiter = '\t';

            ExcludeProteinDescription = false;
            ExcludeProteinSequence = false;
            TruncateProteinDescription = true;

            OutputDirectoryPath = string.Empty;

            OutputFileDelimiter = '\t';

            RecurseDirectories = false;
            MaxLevelsToRecurse = 0;

            OutputDirectoryAlternatePath = string.Empty;
            RecreateDirectoryHierarchyInAlternatePath = false;

            LogEnabled = false;
            LogFilePath = string.Empty;

            ComputeNET = true;
            ComputepI = true;
            ComputeSCXNET = true;

            HydrophobicityMode = HydrophobicityTypeConstants.HW;

            ReportMaximumpI = false;
            SequenceLengthToExamineForMaximumpI = 10;

            ComputeProteinMass = true;
            ElementMassMode = PeptideSequence.ElementModeConstants.IsotopicMass;
            IncludeXResiduesInMass = false;

            CreateDigestedProteinOutputFile = false;
            CreateFastaOutputFile = false;
            CreateProteinOutputFile = false;

            ComputeSequenceHashValues = false;
            ComputeSequenceHashIgnoreILDiff = true;
            GenerateUniqueIDValuesForPeptides = true;

            ProteinScramblingMode = ProteinFileParser.ProteinScramblingModeConstants.None;
            ProteinScramblingSamplingPercentage = 100;
            ProteinScramblingLoopCount = 1;

            QuietMode = false;
        }

        public bool Validate()
        {
            if (CreateDigestedProteinOutputFile && !CreateProteinOutputFile)
            {
                OnDebugEvent("Auto-enabling CreateProteinOutputFile since CreateDigestedProteinOutputFile is true");
                Console.WriteLine();
                CreateProteinOutputFile = true;
            }

            if (CreateDigestedProteinOutputFile && CreateFastaOutputFile)
            {
                OnDebugEvent("Auto-disabling CreateFastaOutputFile since CreateDigestedProteinOutputFile is true");
                Console.WriteLine();
                CreateFastaOutputFile = false;
            }

            if (CreateFastaOutputFile && CreateProteinOutputFile)
            {
                OnDebugEvent("Auto-disabling CreateProteinOutputFile since CreateFastaOutputFile is true");
                Console.WriteLine();
                CreateProteinOutputFile = false;
            }

            if (!CreateFastaOutputFile && !CreateProteinOutputFile && !CreateDigestedProteinOutputFile)
            {
                OnDebugEvent("Auto-enabling CreateProteinOutputFile since the output file mode was not defined");
                Console.WriteLine();
                CreateProteinOutputFile = true;
            }

            return true;
        }
    }
}
