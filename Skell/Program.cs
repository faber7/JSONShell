using System;
using System.IO;
using System.Text;
using Serilog;
using Interpreter;

namespace Skell
{
    class Program
    {
        private static ILogger logger;
        private static SkellInterpreter interpreter;
        static void Main(string[] args)
        {
            SkellLogger.InitializeConsoleLogger();
            logger = Log.ForContext<Program>();
            if (args.Length > 1) {
                logger.Information($"Incorrect number of arguments : {args.Length}");
                exit(127);
            } else {
                interpreter = new SkellInterpreter();
                if (args.Length == 1) {
                    logger.Information($"Running interpreter on {args[0]}");
                    runFile(args[0]);
                } else {
                    logger.Information("No arguments specified, running in interpreter mode");
                    runPrompt();
                }
            }
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
