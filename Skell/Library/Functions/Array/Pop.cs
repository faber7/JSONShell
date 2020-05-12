using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Array
{
    public class Pop : Skell.Types.BuiltinLambda
    {
        public Pop()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("array", Skell.Types.Specifier.Array)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, Skell.Types.ISkellType>> args)
        {
            var indexable = args.First().Item3;

            ISkellReturnable result = new Skell.Types.None();

            if (indexable is Skell.Types.Array arr) {
                var index = arr.Count() - new Number(1);
                if (arr.ListIndices().Contains(index)) {
                    result = arr.GetMember(index);
                    arr.Delete(index);
                } else
                    Console.WriteLine("Invalid Index!");
            } else if (indexable is Skell.Types.Property prop && prop.Value is Skell.Types.Array array) {
                var index = array.Count() - new Number(1);
                if (array.ListIndices().Contains(index)) {
                    result = array.GetMember(index);
                    array.Delete(index);

                    prop.Value = new Skell.Types.Array(array.ListValues());
                } else
                    Console.WriteLine("Invalid Index!");
            }
            
            return result;
        }
    }
}