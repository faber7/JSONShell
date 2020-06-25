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
            IShellData data = args.First().Item3;
            if (data is Property prop)
                data = prop.Value;
            
            if (data is Types.String str) {
                Console.Write(str.contents);
                return new Types.None();
            }
            Console.Write(data.ToString());

            return new Types.None();
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
            IShellData data = args.First().Item3;
            if (data is Property prop)
                data = prop.Value;
            
            if (data is Types.String str) {
                Console.WriteLine(str.contents);
                return new Types.None();
            }
            Console.WriteLine(data.ToString());

            return new Types.None();
        }
    }
}