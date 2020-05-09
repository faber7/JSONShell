using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Indexable
{
    public class Contains : Skell.Types.BuiltinLambda
    {
        public Contains()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("indexable", Skell.Types.Specifier.Any));
            argList.Add(new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.Any));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var indexable1 = args.First().Item3;
            if (indexable1 is Skell.Types.Property prop)
                indexable1 = prop.value;
            var value = args.Skip(1).First().Item3;

            if (indexable1 is Skell.Types.ISkellIndexable indexable)
                return indexable.IndexOf(value);
            else
                return new Skell.Types.None();
        }
    }
}