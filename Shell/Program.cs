﻿using System;
using System.Linq;
using Serilog;
using Shell.Interpreter;
using CommandLine;

namespace Shell
{
    internal class Options
    {
        [Option(
            SetName = "LogLevel",
            Default = false,
            HelpText = "Log extra details necessary for debugging"
        )]
        public bool Debug { get; set; }

        [Option(
            SetName = "LogLevel",
            Default = false,
            HelpText = "Log everything"
        )]
        public bool Verbose { get; set; }

        [Option(
            Required = false,
            HelpText = "Input file to be interpreted"
        )]
        public string InputFile { get; set; }
    }
    internal class Program
    {
        private static ILogger logger;
        private static Interpreter.Interpreter interpreter;
        private static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunWithOptions)
                .WithNotParsed(e => {
                    if (e.First() is CommandLine.VersionRequestedError ||
                        e.First() is CommandLine.HelpRequestedError)
                            return;

                    foreach(var i in e)
                        Console.WriteLine("Error: " + i);

                    throw new ArgumentException("Invalid Argument(s).");
                });
        }

        private static void Exit(int exitCode)
        {
            if (exitCode == 0)
                logger.Information("Exiting normally...");
            else
                logger.Information($"Exiting with error code {exitCode}");

            Logger.TerminateConsoleLogger();
            Environment.Exit(exitCode);
        }

        private static void RunWithOptions(Options options)
        {
            Logger.InitializeConsoleLogger(options.Debug, options.Verbose);
            logger = Log.ForContext<Program>();

            interpreter = new Interpreter.Interpreter();

            if (options.InputFile == null) {
                logger.Information("No arguments specified, running in interpreter mode");
                Exit(interpreter.RunPrompt());
            } else {
                logger.Information($"Running interpreter on {options.InputFile}");
                Exit(interpreter.RunFile(options.InputFile));
            }
        }
    }
}
