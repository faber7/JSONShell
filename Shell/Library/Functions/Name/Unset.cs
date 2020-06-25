using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Name
{
    public class Unset : Shell.Types.BuiltinLambda
    {
        public Unset()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("name", Shell.Types.Specifier.String)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var name = args.First().Item3;

            if (name is Shell.Types.String str && state.Names.Exists(str.contents)) {
                state.Names.Clear(str.contents);
            }

            return new Shell.Types.None();
        }
    }
}