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

            if (!index.isInt) {
                Console.WriteLine("Index must be an integer value!");
                return new Skell.Types.Null();
            }

            Types.ISkellType value = new Skell.Types.Null();
            if (indexable is Skell.Types.Array arr) {
                value = arr.GetMember(index);
                arr.Delete(index);
            } else if (indexable is Skell.Types.Property prop && prop.Value is Skell.Types.Array array) {
                value = array.GetMember(index);
                array.Delete(index);
                prop.Value = new Skell.Types.Array(array.ListValues());
            }
            
            return value;
        }
    }
}