using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Array
{
    public class Range : Shell.Types.BuiltinLambda
    {
        public Range()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("start", Shell.Types.Specifier.Number),
                new Tuple<string, Shell.Types.Specifier>("stop", Shell.Types.Specifier.Number)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var start = (Types.Number) args.First().Item3;
            var stop = (Types.Number) args.Last().Item3;

            if (!start.isInt || !stop.isInt) {
                Console.WriteLine("Start and stop indices must be integers!");
                return new Types.Null();
            }

            IEnumerable<int> range;
            if (start.integerValue < stop.integerValue)
                range = Enumerable.Range(start.integerValue, 1 + stop.integerValue - start.integerValue);
            else
                range = Enumerable.Range(stop.integerValue, 1 + start.integerValue - stop.integerValue).Reverse();
            
            return new Shell.Types.Array(range.Select((i) => new Shell.Types.Number(i)).ToArray());
        }
    }
}