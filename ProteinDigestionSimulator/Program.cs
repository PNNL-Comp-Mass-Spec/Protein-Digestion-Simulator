using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using PRISM;
using PRISM.FileProcessor;
using PRISM.Logging;
using ProteinDigestionSimulator.Options;

// ReSharper disable LocalizableElement

// -------------------------------------------------------------------------------
// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
// Started in 2004
//
// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics
// -------------------------------------------------------------------------------
//
// Licensed under the 2-Clause BSD License; you may not use this file except
// in compliance with the License.  You may obtain a copy of the License at
// https://opensource.org/licenses/BSD-2-Clause
//
// Copyright 2021 Battelle Memorial Institute

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
        // Ignore Spelling: conf, silico

        public const string PROGRAM_DATE = "July 19, 2024";

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetConsoleWindow();

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private static DateTime mLastProgressReportTime;
        private static int mLastProgressReportValue;

        /// <summary>
        /// Program entry method
        /// </summary>
        /// <param name="args"></param>
        /// <returns>0 if no error, error code if an error</returns>
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                if (args?.Length == 0)
                {
                    ShowGUI();
                    return 0;
                }

                var programName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
                var exePath = AppUtils.GetAppPath();
                var exeName = Path.GetFileName(exePath);

                var parser = new CommandLineParser<DigestionSimulatorOptions>(programName, GetAppVersion())
                {
                    ProgramInfo = "This program can be used to read a FASTA file or tab delimited file containing protein or peptide sequences, then output " +
                                  "the data to a tab-delimited file. It can optionally digest the input sequences using trypsin or partial trypsin rules, " +
                                  "and can add the predicted normalized elution time (NET) values for the peptides. Additionally, it can calculate the " +
                                  "number of uniquely identifiable peptides, using only mass, or both mass and NET, with appropriate tolerances.",
                    ContactInfo = "Program written by Matthew Monroe for PNNL (Richland, WA)" + Environment.NewLine +
                                  "E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov" + Environment.NewLine +
                                  "Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://www.pnnl.gov/integrative-omics"
                };

                parser.UsageExamples.Add(exeName + " SourceFile.fasta");
                parser.UsageExamples.Add(exeName + " SourceFile.fasta /Digest");
                parser.UsageExamples.Add(exeName + " SourceFile.fasta /O:OutputDirectoryPath");
                parser.UsageExamples.Add(exeName + " SourceFile.fasta /P:ParameterFilePath");

                // The default argument name for parameter files is /ParamFile or -ParamFile
                // Also allow /Conf or /P
                parser.AddParamFileKey("Conf");
                parser.AddParamFileKey("P");

                var result = parser.ParseArgs(args);
                var options = result.ParsedResults;
                RegisterEvents(options);

                if (!result.Success || !options.Validate())
                {
                    if (parser.CreateParamFileProvided)
                    {
                        return 0;
                    }

                    // Delay for 750 msec in case the user double-clicked this file from within Windows Explorer (or started the program via a shortcut)
                    Thread.Sleep(750);
                    return -1;
                }

                var proteinFileParser = new ProteinFileParser(options)
                {
                    ArchiveOldLogFiles = false,
                    LogMessagesToFile = options.LogEnabled,
                    LogFilePath = options.LogFilePath
                };

                proteinFileParser.ShowProcessingOptions();

                proteinFileParser.ProgressUpdate += Processing_ProgressChanged;
                proteinFileParser.ProgressReset += Processing_ProgressReset;

                if (options.RecurseDirectories)
                {
                    return ProcessFilesAndRecurse(proteinFileParser, options);
                }

                // Do not provide a parameter file here, since the CommandLineParser will have already loaded options from a Key=Value parameter file
                if (proteinFileParser.ProcessFilesWildcard(options.InputFilePath, options.OutputDirectoryPath))
                {
                    DisplayProgressPercent(mLastProgressReportValue, true);
                    return 0;
                }

                int returnCode;

                if (proteinFileParser.ErrorCode != ProcessFilesBase.ProcessFilesErrorCodes.NoError)
                {
                    returnCode = (int)proteinFileParser.ErrorCode;
                    ShowErrorMessage("Error while processing: " + proteinFileParser.GetErrorMessage());
                }
                else
                {
                    returnCode = -1;
                    ShowErrorMessage("Unknown error while processing");
                }

                Thread.Sleep(750);
                return returnCode;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error occurred in Program->Main", ex);
                Thread.Sleep(1500);
                return -1;
            }
        }

        private static int ProcessFilesAndRecurse(ProcessFilesBase proteinFileParser, DigestionSimulatorOptions options)
        {
            // Send an empty string for the parameter file path since the CommandLineParser will have already loaded options from a Key=Value parameter file

            if (proteinFileParser.ProcessFilesAndRecurseDirectories(
                options.InputFilePath,
                options.OutputDirectoryPath,
                options.OutputDirectoryAlternatePath,
                options.RecreateDirectoryHierarchyInAlternatePath,
                string.Empty,
                options.MaxLevelsToRecurse))
            {
                return 0;
            }

            int errorCode;

            if (proteinFileParser.ErrorCode != ProcessFilesBase.ProcessFilesErrorCodes.NoError)
            {
                errorCode = (int)proteinFileParser.ErrorCode;
                ShowErrorMessage("Error while processing: " + proteinFileParser.GetErrorMessage());
            }
            else
            {
                errorCode = -1;
                ShowErrorMessage("Unknown error while processing");
            }

            Thread.Sleep(750);
            return errorCode;
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
            return AppUtils.GetAppVersion(PROGRAM_DATE);
        }

        private static string GetExecutableName()
        {
            return Path.GetFileName(AppUtils.GetAppPath());
        }

        private static void OnDebugEvent(string message)
        {
            ConsoleMsgUtils.ShowDebug(message);
        }

        private static void OnErrorEvent(string message, Exception ex)
        {
            ShowErrorMessage(message, ex);
        }

        private static void OnStatusEvent(string message)
        {
            Console.WriteLine(message);
        }

        private static void OnWarningEvent(string message)
        {
            ConsoleMsgUtils.ShowWarning(message);
        }

        /// <summary>
        /// Use this method to chain events between classes
        /// </summary>
        /// <param name="sourceClass"></param>
        private static void RegisterEvents(IEventNotifier sourceClass)
        {
            sourceClass.DebugEvent += OnDebugEvent;
            sourceClass.StatusEvent += OnStatusEvent;
            sourceClass.ErrorEvent += OnErrorEvent;
            sourceClass.WarningEvent += OnWarningEvent;
            // Ignore: sourceClass.ProgressUpdate += OnProgressUpdate;
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

                try
                {
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.DoEvents();
                }
                catch (Exception ex)
                {
                    ConsoleMsgUtils.ShowWarning("Exception enabling visual styles: " + ex.Message);
                }

                var formMain = new Main();
                formMain.ShowDialog();
            }
            catch (Exception ex)
            {
                if (SystemInfo.IsLinux)
                {
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.InnerException != null && ex.InnerException.Message.Contains("X-Server required"))
                        {
                            ConsoleMsgUtils.ShowWarning("Unable to show the GUI, most likely since the console is not running under X-Windows: " +
                                                        ex.InnerException.InnerException.Message);
                        }
                        else
                        {
                            ConsoleMsgUtils.ShowWarning("Unable to show the GUI, most likely since the console is not running under X-Windows: " +
                                                        ex.InnerException.Message);
                        }
                    }
                    else
                    {
                        ConsoleMsgUtils.ShowWarning("Unable to show the GUI: " + ex.Message);
                        ConsoleMsgUtils.ShowDebug(StackTraceFormatter.GetExceptionStackTraceMultiLine(ex));
                    }
                }
                else
                {
                    ShowErrorMessage("Exception with the GUI", ex);
                }

                Console.WriteLine();
                Console.WriteLine("The Protein Digestion Simulator can be run as a console application (no GUI).");
                Console.WriteLine("To see supported arguments, use the --help argument:");
                Console.WriteLine();
                Console.WriteLine("{0}{1} --help", SystemInfo.IsLinux ? "mono " : string.Empty, GetExecutableName());
                Console.WriteLine();
            }

            if (hWndConsole != IntPtr.Zero && !SystemInfo.IsLinux)
            {
                ShowWindow(hWndConsole, SW_SHOW);
            }
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