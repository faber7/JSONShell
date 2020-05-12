using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.String
{
    public class Substring : Skell.Types.BuiltinLambda
    {
        public Substring()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.String),
                new Tuple<string, Skell.Types.Specifier>("start", Skell.Types.Specifier.Number),
                new Tuple<string, Skell.Types.Specifier>("stop", Skell.Types.Specifier.Number)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var val = args.First().Item3;
            if (val is Skell.Types.Property prop)
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