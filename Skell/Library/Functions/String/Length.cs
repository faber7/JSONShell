using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.String
{
    public class Length : Skell.Types.BuiltinLambda
    {
        public Length()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.String)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var value = args.First().Item3;
            if (value is Skell.Types.Property prop)
                value = prop.Value;

            return ((Types.String) value).Length();
        }
    }
}