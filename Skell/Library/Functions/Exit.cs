using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions
{
    public class Exit : BuiltinLambda
    {
        public Exit()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            Environment.Exit(0);

            return new Skell.Types.None();
        }
    }

    public class ExitWithCode : BuiltinLambda
    {
        public ExitWithCode()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Specifier>("exitcode", Specifier.Number)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            var code = (Types.Number) args.First().Item3;

            if (code.isInt)
                Environment.Exit(code.integerValue);

            Console.WriteLine("Exit code must be an integer value!");

            return new Skell.Types.None();
        }
    }
}