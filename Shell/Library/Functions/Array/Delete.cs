using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Array
{
    public class Delete : Shell.Types.BuiltinLambda
    {
        public Delete()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("array", Shell.Types.Specifier.Array),
                new Tuple<string, Shell.Types.Specifier>("index", Shell.Types.Specifier.Number)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var indexable = args.First().Item3;
            var index = (Types.Number) args.Last().Item3;
            Shell.Types.Array arr = (Shell.Types.Array) indexable;
            if (indexable is Shell.Types.Property prop)
                arr = (Shell.Types.Array) prop.Value;

            if (!index.isInt) {
                Console.WriteLine("Index must be an integer value!");
                return new Shell.Types.Null();
            }

            Types.IShellData value = new Shell.Types.Null();

            if (arr.ListIndices().Contains(index)) {
                value = arr.GetMember(index);
                arr.Delete(index);
                if (indexable is Shell.Types.Property prop1)
                    prop1.Value = new Shell.Types.Array(arr.ListValues());
            } else {
                Console.WriteLine("Invalid index!");
            }
            
            return value;
        }
    }
}