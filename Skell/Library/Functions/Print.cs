using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions
{
    public class Print : BuiltinLambda
    {
        public Print()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.Any)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            if (args.First().Item3 is Skell.Types.String str) {
                Console.Write(str.contents);
                return new Skell.Types.None();
            }
            Console.Write(args.First().ToString());

            return new Skell.Types.None();
        }
    }

    public class PrintLine : BuiltinLambda
    {
        public PrintLine()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.Any)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            if (args.First().Item3 is Skell.Types.String str) {
                Console.WriteLine(str.contents);
                return new Skell.Types.None();
            }
            Console.WriteLine(args.First().ToString());

            return new Skell.Types.None();
        }
    }
}