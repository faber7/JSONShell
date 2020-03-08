using System;
using System.IO;
using System.Text;

using Interpreter;

namespace Skell
{
    class Program
    {
        private static SkellInterpreter interpreter;
        static void Main(string[] args)
        {
            if (args.Length > 1) {
                Console.WriteLine("Usage: dotnet run -- [script]");
                Environment.Exit(127);
            } else {
                interpreter = new SkellInterpreter();
                if (args.Length == 1) {
                    runFile(args[0]);
                } else {
                    runPrompt();
                }
            }
        }

        public static void runFile(String path)
        {
            byte[] input = File.ReadAllBytes(path);
            interpreter.interprete(Encoding.ASCII.GetString(input));
        }

        public static void runPrompt()
        {
            Console.WriteLine("Press Ctrl+C to exit the prompt.");

            while (true) {
                Console.Write("> ");
                interpreter.interprete(Console.In.ReadLine());
            }
        }
    }
}
