using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Name
{
    public class Unset : Skell.Types.BuiltinLambda
    {
        public Unset()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("name", Skell.Types.Specifier.String));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var name = args.First().Item3;

            if (name is Skell.Types.String str && state.Names.Exists(str.contents)) {
                state.Names.Clear(str.contents);
            }

            return new Skell.Types.None();
        }
    }
}