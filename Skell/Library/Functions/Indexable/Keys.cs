using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Types;

namespace Skell.Library.Functions.Indexable
{
    public class Keys : Skell.Types.BuiltinLambda
    {
        public Keys()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("indexable", Skell.Types.Specifier.Any));
        }

        public override ISkellReturnable Execute(List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var obj = args.First().Item3;

            if (obj is Skell.Types.ISkellIndexable indexable)
                return new Skell.Types.Array(indexable.ListIndices());
            
            return new Skell.Types.None();
        }
    }
}