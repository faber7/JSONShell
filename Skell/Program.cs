using System;
using System.IO;
using System.Text;
using System.Linq;
using Serilog;
using Skell.Interpreter;
using CommandLine;

namespace Skell
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
        private static SkellInterpreter interpreter;
        private static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunWithOptions)
                .WithNotParsed(e => {
                    if (e.First() is CommandLine.VersionRequestedError ||
                        e.First() is CommandLine.HelpRequestedError) {
                            return;
                    }
                    foreach(var i in e) {
                        Console.WriteLine("Error: " + i);
                    }
                    throw new ArgumentException("Invalid Argument(s).");
                });
        }

        private static void Exit(int exitCode)
        {
            if (exitCode == 0) {
                logger.Information("Exiting normally...");
            } else {
                logger.Information($"Exiting with error code {exitCode}");
            }
            SkellLogger.TerminateConsoleLogger();
            Environment.Exit(exitCode);
        }

        private static void RunWithOptions(Options options)
        {
            SkellLogger.InitializeConsoleLogger(options.Debug, options.Verbose);
            logger = Log.ForContext<Program>();

            interpreter = new SkellInterpreter();

            if (options.InputFile == null) {
                logger.Information("No arguments specified, running in interpreter mode");
                RunPrompt();
            } else {
                logger.Information($"Running interpreter on {options.InputFile}");
                RunFile(options.InputFile);
            }
        }

        public static void RunFile(String path)
        {
            byte[] input = File.ReadAllBytes(path);
            interpreter.Interprete(Encoding.ASCII.GetString(input));
        }

        public static void RunPrompt()
        {
            Console.WriteLine("Press Ctrl+D to exit the prompt.");

            string input;
            Console.Write("> ");
            while ((input = Console.In.ReadLine()) != null) {
                interpreter.Interprete(input + '\n');
                Console.Write("> ");
            }
            Exit(0);
        }
    }
}
