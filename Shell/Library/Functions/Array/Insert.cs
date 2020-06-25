using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Array
{
    public class Insert : Shell.Types.BuiltinLambda
    {
        public Insert()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("array", Shell.Types.Specifier.Array),
                new Tuple<string, Shell.Types.Specifier>("index", Shell.Types.Specifier.Number),
                new Tuple<string, Shell.Types.Specifier>("value", Shell.Types.Specifier.Any)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var indexable = args.First().Item3;
            var index = (Types.Number) args.Skip(1).First().Item3;
            var value = args.Last().Item3;
            Shell.Types.Array arr = (Shell.Types.Array) indexable;
            if (indexable is Shell.Types.Property prop)
                arr = (Shell.Types.Array) prop.Value;

            if (!index.isInt) {
                Console.WriteLine("Index must be an integer value!");
                return new Shell.Types.Null();
            }

            if (arr.ListIndices().Contains(index) || index.integerValue == arr.Count().integerValue) {
                arr.Insert(index, value);
                if (indexable is Shell.Types.Property prop1)
                    prop1.Value = new Shell.Types.Array(arr.ListValues());
            } else {
                Console.WriteLine("Invalid index!");
            }
            
            return new Shell.Types.Null();
        }
    }
}