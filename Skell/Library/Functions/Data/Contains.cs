using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Types;

namespace Skell.Library.Functions.Data
{
    public class Contains : Skell.Types.BuiltinLambda
    {
        public Contains()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("container", Skell.Types.Specifier.Any));
            argList.Add(new Tuple<string, Skell.Types.Specifier>("content", Skell.Types.Specifier.Any));
        }

        public override ISkellReturnable Execute(List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var container = args.First().Item3;
            var content = args.Skip(1).First().Item3;

            if (container is Skell.Types.ISkellIndexable indexable)
                return indexable.IndexOf(content);
            else
                return new Skell.Types.None();
        }
    }
}