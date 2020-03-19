using System;
using System.IO;
using System.Text;
using System.Linq;
using Serilog;
using Skell.Interpreter;
using CommandLine;

namespace Skell
{
    class Options
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
    class Program
    {
        private static ILogger logger;
        private static SkellInterpreter interpreter;
        static void Main(string[] args)
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

        static void exit(int exitCode)
        {
            if (exitCode == 0) {
                logger.Information("Exiting normally...");
            } else {
                logger.Information($"Exiting with error code {exitCode}");
            }
            SkellLogger.TerminateConsoleLogger();
            Environment.Exit(exitCode);
        }

        static void RunWithOptions(Options options)
        {
            SkellLogger.InitializeConsoleLogger(options.Debug, options.Verbose);
            logger = Log.ForContext<Program>();

            interpreter = new SkellInterpreter();
            
            if (options.InputFile == null) {
                logger.Information("No arguments specified, running in interpreter mode");
                runPrompt();
            } else {
                logger.Information($"Running interpreter on {options.InputFile}");
                runFile(options.InputFile);
            }
        }

        public static void runFile(String path)
        {
            byte[] input = File.ReadAllBytes(path);
            interpreter.interprete(Encoding.ASCII.GetString(input));
        }

        public static void runPrompt()
        {
            Console.WriteLine("Press Ctrl+D to exit the prompt.");

            string input;
            Console.Write("> ");
            while ((input = Console.In.ReadLine()) != null) {
                interpreter.interprete(input);
                Console.Write("> ");
            }
            exit(0);
        }
    }
}
