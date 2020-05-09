using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Indexable
{
    public class Length : Skell.Types.BuiltinLambda
    {
        public Length()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.Any));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var value = args.First().Item3;

            if (value is Skell.Types.ISkellIndexable indexable)
                return indexable.Count();
            else
                return new Skell.Types.Number(-1);
        }
    }
}