using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Array
{
    public class Push : Shell.Types.BuiltinLambda
    {
        public Push()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("array", Shell.Types.Specifier.Array),
                new Tuple<string, Shell.Types.Specifier>("value", Shell.Types.Specifier.Any)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var indexable = args.First().Item3;
            var value = args.Skip(1).First().Item3;

            if (indexable is Shell.Types.Array arr)
                arr.Insert(arr.Count(), value);
            else if (indexable is Shell.Types.Property prop && prop.Value is Shell.Types.Array array) {
                array.Insert(array.Count(), value);
                prop.Value = new Shell.Types.Array(array.ListValues());
            }
            
            return new Shell.Types.None();
        }
    }
}