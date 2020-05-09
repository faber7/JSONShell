using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Name
{
    public class Available : Skell.Types.BuiltinLambda
    {
        public Available()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("name", Skell.Types.Specifier.String));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var name = args.First().Item3;

            if (name is Skell.Types.String str)
                return new Skell.Types.Boolean(state.Names.Available(str.contents));
            
            return new Skell.Types.None();
        }
    }
}