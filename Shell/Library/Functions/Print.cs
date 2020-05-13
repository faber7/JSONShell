using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions
{
    public class Print : BuiltinLambda
    {
        public Print()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("value", Shell.Types.Specifier.Any)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            if (args.First().Item3 is Shell.Types.String str) {
                Console.Write(str.contents);
                return new Shell.Types.None();
            }
            Console.Write(args.First().Item3.ToString());

            return new Shell.Types.None();
        }
    }

    public class PrintLine : BuiltinLambda
    {
        public PrintLine()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("value", Shell.Types.Specifier.Any)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            if (args.First().Item3 is Shell.Types.String str) {
                Console.WriteLine(str.contents);
                return new Shell.Types.None();
            }
            Console.WriteLine(args.First().Item3.ToString());

            return new Shell.Types.None();
        }
    }
}