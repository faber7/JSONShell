using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Indexable
{
    public class Contains : Shell.Types.BuiltinLambda
    {
        public Contains()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("indexable", Shell.Types.Specifier.Any),
                new Tuple<string, Shell.Types.Specifier>("value", Shell.Types.Specifier.Any)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var indexable1 = args.First().Item3;
            if (indexable1 is Shell.Types.Property prop)
                indexable1 = prop.Value;
            var value = args.Skip(1).First().Item3;

            if (indexable1 is Shell.Types.IShellIndexable indexable)
                return indexable.IndexOf(value);
            else
                return new Shell.Types.None();
        }
    }
}