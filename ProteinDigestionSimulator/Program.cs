using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using PRISM;
using PRISM.FileProcessor;
using ProteinFileReader;

// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004
//
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the 2-Clause BSD License; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// https://opensource.org/licenses/BSD-2-Clause
//
// Copyright 2018 Battelle Memorial Institute

namespace ProteinDigestionSimulator
{
    /// <summary>
    /// <para>
    /// This program can be used to read a FASTA file or tab delimited file
    /// containing protein or peptide sequences, then output the data to a tab-delimited file
    /// </para>
    /// <para>
    /// It can optionally digest the input sequences using trypsin or partial trypsin rules,
    /// and can add the predicted normalized elution time (NET) values for the peptides
    /// </para>
    /// <para>
    /// Additionally, it can calculate the number of uniquely identifiable peptides, using
    /// only mass, or both mass and NET, with appropriate tolerances
    /// </para>
    /// </summary>
    /// <remarks>
    /// Example command line: /I:Yeast_2003-01-06.fasta /debug /d /p:ProteinDigestionSettings.xml
    /// </remarks>
    internal static class Program
    {
        // Ignore Spelling: silico

        public const string PROGRAM_DATE = "October 18, 2021";

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetConsoleWindow();

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private static string mInputFilePath;
        private static bool mAssumeFastaFile;
        private static bool mCreateDigestedProteinOutputFile;
        private static bool mComputeProteinMass;

        private static char mInputFileDelimiter;

        private static string mOutputDirectoryPath;              // Optional
        private static string mParameterFilePath;                // Optional
        private static string mOutputDirectoryAlternatePath;     // Optional

        private static bool mRecreateDirectoryHierarchyInAlternatePath;  // Optional

        private static bool mRecurseDirectories;
        private static int mMaxLevelsToRecurse;

        private static bool mLogMessagesToFile;
        private static string mLogFilePath = string.Empty;
        private static string mLogDirectoryPath = string.Empty;

        private static bool mShowDebugPrompts;

        private static ProteinFileParser mParseProteinFile;
        private static DateTime mLastProgressReportTime;
        private static int mLastProgressReportValue;

        [STAThread]
        private static int Main()
        {
            // Returns 0 if no error, error code if an error

            int returnCode;
            var commandLineParser = new clsParseCommandLine();

            mInputFilePath = string.Empty;
            mAssumeFastaFile = false;
            mCreateDigestedProteinOutputFile = false;
            mComputeProteinMass = false;

            mInputFileDelimiter = '\t';

            mOutputDirectoryPath = string.Empty;
            mParameterFilePath = string.Empty;

            mRecurseDirectories = false;
            mMaxLevelsToRecurse = 0;

            mLogMessagesToFile = false;
            mLogFilePath = string.Empty;
            mLogDirectoryPath = string.Empty;

            try
            {
                var proceed = false;
                if (commandLineParser.ParseCommandLine())
                {
                    if (SetOptionsUsingCommandLineParameters(commandLineParser))
                    {
                        proceed = true;
                    }
                }

                if (commandLineParser.ParameterCount + commandLineParser.NonSwitchParameterCount == 0 && !commandLineParser.NeedToShowHelp)
                {
                    ShowGUI();
                    return 0;
                }

                if (!proceed || commandLineParser.NeedToShowHelp || mInputFilePath.Length == 0)
                {
                    ShowProgramHelp();
                    return -1;
                }

                mParseProteinFile = new ProteinFileParser { ShowDebugPrompts = mShowDebugPrompts };

                mParseProteinFile.ProgressUpdate += Processing_ProgressChanged;
                mParseProteinFile.ProgressReset += Processing_ProgressReset;

                // Note: the following settings will be overridden if mParameterFilePath points to a valid parameter file that has these settings defined
                mParseProteinFile.AssumeFastaFile = mAssumeFastaFile;
                mParseProteinFile.CreateProteinOutputFile = true;
                mParseProteinFile.CreateDigestedProteinOutputFile = mCreateDigestedProteinOutputFile;
                mParseProteinFile.ComputeProteinMass = mComputeProteinMass;

                mParseProteinFile.InputFileDelimiter = mInputFileDelimiter;
                mParseProteinFile.DelimitedFileFormatCode = DelimitedProteinFileReader.ProteinFileFormatCode.ProteinName_Description_Sequence;

                mParseProteinFile.DigestionOptions.RemoveDuplicateSequences = false;

                mParseProteinFile.LogMessagesToFile = mLogMessagesToFile;
                mParseProteinFile.LogFilePath = mLogFilePath;
                mParseProteinFile.LogDirectoryPath = mLogDirectoryPath;

                if (mRecurseDirectories)
                {
                    if (mParseProteinFile.ProcessFilesAndRecurseDirectories(mInputFilePath, mOutputDirectoryPath,
                                                                            mOutputDirectoryAlternatePath, mRecreateDirectoryHierarchyInAlternatePath,
                                                                            mParameterFilePath, mMaxLevelsToRecurse))
                    {
                        returnCode = 0;
                    }
                    else
                    {
                        returnCode = (int)mParseProteinFile.ErrorCode;
                    }
                }
                else if (mParseProteinFile.ProcessFilesWildcard(mInputFilePath, mOutputDirectoryPath, mParameterFilePath))
                {
                    returnCode = 0;
                }
                else
                {
                    returnCode = (int)mParseProteinFile.ErrorCode;
                    if (returnCode != 0)
                    {
                        ShowErrorMessage("Error while processing: " + mParseProteinFile.GetErrorMessage());
                    }
                }

                DisplayProgressPercent(mLastProgressReportValue, true);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error occurred in Program->Main", ex);
                returnCode = -1;
            }

            Thread.Sleep(1500);

            return returnCode;
        }

        private static void DisplayProgressPercent(int percentComplete, bool addCarriageReturn)
        {
            if (addCarriageReturn)
            {
                Console.WriteLine();
            }

            if (percentComplete > 100)
            {
                percentComplete = 100;
            }

            Console.Write("Processing: " + percentComplete + "% ");
            if (addCarriageReturn)
            {
                Console.WriteLine();
            }
        }

        private static string GetAppVersion()
        {
            return ProcessFilesOrDirectoriesBase.GetAppVersion(PROGRAM_DATE);
        }

        private static bool SetOptionsUsingCommandLineParameters(clsParseCommandLine commandLineParser)
        {
            // Returns True if no problems; otherwise, returns false

            var validParameters = new List<string> { "I", "F", "D", "M", "AD", "O", "P", "S", "A", "R", "DEBUG" };

            try
            {
                // Make sure no invalid parameters are present
                if (commandLineParser.InvalidParametersPresent(validParameters))
                {
                    ConsoleMsgUtils.ShowErrors("Invalid command line parameters", commandLineParser.InvalidParameters(validParameters).ConvertAll(x => "/" + x));
                    return false;
                }

                // Query commandLineParser to see if various parameters are present
                if (commandLineParser.RetrieveValueForParameter("I", out var value))
                {
                    mInputFilePath = value;
                }
                else if (commandLineParser.NonSwitchParameterCount > 0)
                {
                    mInputFilePath = commandLineParser.RetrieveNonSwitchParameter(0);
                }

                if (commandLineParser.RetrieveValueForParameter("F", out value))
                {
                    mAssumeFastaFile = true;
                }

                if (commandLineParser.RetrieveValueForParameter("D", out value))
                {
                    mCreateDigestedProteinOutputFile = true;
                }

                if (commandLineParser.RetrieveValueForParameter("M", out value))
                {
                    mComputeProteinMass = true;
                }

                if (commandLineParser.RetrieveValueForParameter("AD", out value))
                {
                    mInputFileDelimiter = value[0];
                }

                if (commandLineParser.RetrieveValueForParameter("O", out value))
                {
                    mOutputDirectoryPath = value;
                }

                if (commandLineParser.RetrieveValueForParameter("P", out value))
                {
                    mParameterFilePath = value;
                }

                if (commandLineParser.RetrieveValueForParameter("S", out value))
                {
                    mRecurseDirectories = true;
                    if (int.TryParse(value, out var valueInt))
                    {
                        mMaxLevelsToRecurse = valueInt;
                    }
                }

                if (commandLineParser.RetrieveValueForParameter("A", out value))
                {
                    mOutputDirectoryAlternatePath = value;
                }

                if (commandLineParser.RetrieveValueForParameter("R", out value))
                {
                    mRecreateDirectoryHierarchyInAlternatePath = true;
                }

                //if (commandLineParser.RetrieveValueForParameter("L", out value))
                //{
                //    mLogMessagesToFile = true;
                //    if (!string.IsNullOrEmpty(value))
                //        mLogFilePath = value;
                //}

                //if (commandLineParser.RetrieveValueForParameter("LogDir", out value))
                //{
                //    mLogMessagesToFile = true;
                //    if (!string.IsNullOrEmpty(value))
                //        mLogDirectoryPath = value;
                //}

                if (commandLineParser.RetrieveValueForParameter("DEBUG", out value))
                {
                    mShowDebugPrompts = true;
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error parsing the command line parameters: " + ex.Message, ex);
            }

            return false;
        }

        private static void ShowErrorMessage(string message, Exception ex = null)
        {
            ConsoleMsgUtils.ShowError(message, ex);

            WriteToErrorStream(message);
        }

        public static void ShowGUI()
        {
            var hWndConsole = IntPtr.Zero;

            try
            {
                if (!SystemInfo.IsLinux)
                {
                    // Hide the console
                    hWndConsole = GetConsoleWindow();
                    ShowWindow(hWndConsole, SW_HIDE);
                }

                Application.EnableVisualStyles();
                Application.DoEvents();

                var formMain = new Main();
                formMain.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Exception with the GUI", ex);
            }

            if (hWndConsole != IntPtr.Zero && !SystemInfo.IsLinux)
            {
                ShowWindow(hWndConsole, SW_SHOW);
            }
        }

        private static void ShowProgramHelp()
        {
            try
            {
                Console.WriteLine(WrapParagraph(
                    "This program can be used to read a FASTA file or tab delimited file containing protein or peptide sequences, then output " +
                    "the data to a tab-delimited file.  It can optionally digest the input sequences using trypsin or partial trypsin rules, " +
                    "and can add the predicted normalized elution time (NET) values for the peptides.Additionally, it can calculate the " +
                    "number of uniquely identifiable peptides, using only mass, or both mass and NET, with appropriate tolerances."));
                Console.WriteLine();
                Console.WriteLine("Program syntax:");
                Console.WriteLine(WrapParagraph(
                    Path.GetFileName(ProcessFilesOrDirectoriesBase.GetAppPath()) +
                    " /I:SourceFastaOrTextFile [/F] [/D] [/M] [/AD:AlternateDelimiter] " +
                    "[/O:OutputDirectoryPath] [/P:ParameterFilePath] [/S:[MaxLevel]] " +
                    "[/A:AlternateOutputDirectoryPath] [/R] [/Q]"));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("The input file path can contain the wildcard character * and should point to a FASTA file or tab-delimited text file."));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("Use /F to indicate that the input file is a FASTA file.  If /F is not used, the format will be assumed to be FASTA only if the filename ends with .fasta or .fasta.gz"));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("Use /D to indicate that an in-silico digestion of the proteins should be performed.  Digestion options must be specified in the Parameter file."));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("Use /M to indicate that protein mass should be computed."));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("Use /AD to specify a delimiter other than the Tab character (not applicable for FASTA files)."));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("The output directory path is optional.  If omitted, the output files will be created in the same directory as the input file."));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("The parameter file path is optional.  If included, it should point to a valid XML parameter file."));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("Use /S to process all valid files in the input directory and subdirectories. Include a number after /S (like /S:2) to limit the level of subdirectories to examine."));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("When using /S, you can redirect the output of the results using /A."));
                Console.WriteLine();
                Console.WriteLine(WrapParagraph("When using /S, you can use /R to re-create the input directory hierarchy in the alternate output directory (if defined)."));
                Console.WriteLine();
                Console.WriteLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2004");
                Console.WriteLine("Version: " + GetAppVersion());
                Console.WriteLine();
                Console.WriteLine("E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov");
                Console.WriteLine("Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics");
                Console.WriteLine();
                Console.WriteLine(WrapParagraph(Disclaimer.GetKangasPetritisDisclaimerText(false)));

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error displaying the program syntax", ex);
            }
        }

        private static string WrapParagraph(string message, int wrapWidth = 80)
        {
            return CommandLineParser<clsParseCommandLine>.WrapParagraph(message, wrapWidth);
        }

        private static void WriteToErrorStream(string errorMessage)
        {
            try
            {
                using var errorStream = new StreamWriter(Console.OpenStandardError());
                errorStream.WriteLine(errorMessage);
            }
            catch
            {
                // Ignore errors here
            }
        }

        private static void Processing_ProgressChanged(string taskDescription, float percentComplete)
        {
            const int PERCENT_REPORT_INTERVAL = 25;
            const int PROGRESS_DOT_INTERVAL_MSEC = 250;

            if (percentComplete >= mLastProgressReportValue)
            {
                if (mLastProgressReportValue > 0)
                {
                    Console.WriteLine();
                }

                DisplayProgressPercent(mLastProgressReportValue, false);
                mLastProgressReportValue += PERCENT_REPORT_INTERVAL;
                mLastProgressReportTime = DateTime.UtcNow;
            }
            else if (DateTime.UtcNow.Subtract(mLastProgressReportTime).TotalMilliseconds > PROGRESS_DOT_INTERVAL_MSEC)
            {
                mLastProgressReportTime = DateTime.UtcNow;
                Console.Write(".");
            }
        }

        private static void Processing_ProgressReset()
        {
            mLastProgressReportTime = DateTime.UtcNow;
            mLastProgressReportValue = 0;
        }
    }
}