using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Array
{
    public class Insert : Skell.Types.BuiltinLambda
    {
        public Insert()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("array", Skell.Types.Specifier.Array),
                new Tuple<string, Skell.Types.Specifier>("index", Skell.Types.Specifier.Number),
                new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.Any)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var indexable = args.First().Item3;
            var index = (Types.Number) args.Skip(1).First().Item3;
            var value = args.Last().Item3;

            if (!index.isInt) {
                Console.WriteLine("Index must be an integer value!");
                return new Skell.Types.Null();
            }

            if (indexable is Skell.Types.Array arr) {
                arr.Insert(index, value);
                return value;
            } else if (indexable is Skell.Types.Property prop && prop.Value is Skell.Types.Array array) {
                array.Insert(index, value);
                prop.Value = new Skell.Types.Array(array.ListValues());
                return value;
            }
            
            return new Skell.Types.Null();
        }
    }
}