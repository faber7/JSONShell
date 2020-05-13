using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.String
{
    public class Substring : Shell.Types.BuiltinLambda
    {
        public Substring()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("value", Shell.Types.Specifier.String),
                new Tuple<string, Shell.Types.Specifier>("start", Shell.Types.Specifier.Number),
                new Tuple<string, Shell.Types.Specifier>("stop", Shell.Types.Specifier.Number)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var val = args.First().Item3;
            if (val is Shell.Types.Property prop)
                val = prop.Value;
            
            var value = (Types.String) val;
            var start = (Types.Number) args.Skip(1).First().Item3;
            var stop = (Types.Number) args.Last().Item3;

            if (!start.isInt || !stop.isInt) {
                Console.WriteLine("Start and stop indices must be integers!");
                return new Types.Null();
            }

            return value.Substring(start, stop);
        }
    }
}