using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.String
{
    public class Length : Shell.Types.BuiltinLambda
    {
        public Length()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("value", Shell.Types.Specifier.String)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var value = args.First().Item3;
            if (value is Shell.Types.Property prop)
                value = prop.Value;

            return ((Types.String) value).Length();
        }
    }
}