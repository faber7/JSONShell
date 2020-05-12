using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Array
{
    public class Range : Skell.Types.BuiltinLambda
    {
        public Range()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("start", Skell.Types.Specifier.Number),
                new Tuple<string, Skell.Types.Specifier>("stop", Skell.Types.Specifier.Number)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
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
            
            return new Skell.Types.Array(range.Select((i) => new Skell.Types.Number(i)).ToArray());
        }
    }
}