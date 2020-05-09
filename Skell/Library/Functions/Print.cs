using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Types;

namespace Skell.Library.Functions
{
    public class Print : BuiltinLambda
    {
        public Print()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.Any));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            return new Skell.Types.String(args.First().Item3.ToString());
        }
    }
}