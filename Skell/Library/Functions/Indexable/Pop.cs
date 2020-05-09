using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Types;

namespace Skell.Library.Functions.Indexable
{
    public class Pop : Skell.Types.BuiltinLambda
    {
        public Pop()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("indexable", Skell.Types.Specifier.Any));
        }

        public override ISkellReturnable Execute(List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var indexable = args.First().Item3;

            ISkellReturnable result = new Skell.Types.None();

            if (indexable is Skell.Types.Array arr) {
                var index = arr.Count() - new Number(1);
                if (arr.ListIndices().Contains(index))
                    result = arr.GetMember(index);
            }
            
            return result;
        }
    }
}