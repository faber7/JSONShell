using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Indexable
{
    public class Keys : Shell.Types.BuiltinLambda
    {
        public Keys()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("indexable", Shell.Types.Specifier.Any)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var obj = args.First().Item3;
            if (obj is Shell.Types.Property prop)
                obj = prop.Value;

            if (obj is Shell.Types.IShellIndexable indexable)
                return new Shell.Types.Array(indexable.ListIndices());
            
            return new Shell.Types.None();
        }
    }
}