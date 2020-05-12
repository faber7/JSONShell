using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Array
{
    public class Delete : Skell.Types.BuiltinLambda
    {
        public Delete()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("array", Skell.Types.Specifier.Array),
                new Tuple<string, Skell.Types.Specifier>("index", Skell.Types.Specifier.Number)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var indexable = args.First().Item3;
            var index = (Types.Number) args.Last().Item3;
            Skell.Types.Array arr = (Skell.Types.Array) indexable;
            if (indexable is Skell.Types.Property prop)
                arr = (Skell.Types.Array) prop.Value;

            if (!index.isInt) {
                Console.WriteLine("Index must be an integer value!");
                return new Skell.Types.Null();
            }

            Types.ISkellType value = new Skell.Types.Null();

            if (arr.ListIndices().Contains(index)) {
                value = arr.GetMember(index);
                arr.Delete(index);
                if (indexable is Skell.Types.Property prop1)
                    prop1.Value = new Skell.Types.Array(arr.ListValues());
            } else {
                Console.WriteLine("Invalid index!");
            }
            
            return value;
        }
    }
}