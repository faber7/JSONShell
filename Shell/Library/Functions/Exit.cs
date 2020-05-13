using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions
{
    public class Exit : BuiltinLambda
    {
        public Exit()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            Environment.Exit(0);

            return new Shell.Types.None();
        }
    }

    public class ExitWithCode : BuiltinLambda
    {
        public ExitWithCode()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Specifier>("exitcode", Specifier.Number)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            var code = (Types.Number) args.First().Item3;

            if (code.isInt)
                Environment.Exit(code.integerValue);

            Console.WriteLine("Exit code must be an integer value!");

            return new Shell.Types.None();
        }
    }
}