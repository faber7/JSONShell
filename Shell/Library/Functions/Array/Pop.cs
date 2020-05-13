using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Array
{
    public class Pop : Shell.Types.BuiltinLambda
    {
        public Pop()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("array", Shell.Types.Specifier.Array)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, Shell.Types.IShellData>> args)
        {
            var indexable = args.First().Item3;

            IShellReturnable result = new Shell.Types.None();

            if (indexable is Shell.Types.Array arr) {
                var index = arr.Count() - new Number(1);
                if (arr.ListIndices().Contains(index)) {
                    result = arr.GetMember(index);
                    arr.Delete(index);
                } else
                    Console.WriteLine("Invalid Index!");
            } else if (indexable is Shell.Types.Property prop && prop.Value is Shell.Types.Array array) {
                var index = array.Count() - new Number(1);
                if (array.ListIndices().Contains(index)) {
                    result = array.GetMember(index);
                    array.Delete(index);

                    prop.Value = new Shell.Types.Array(array.ListValues());
                } else
                    Console.WriteLine("Invalid Index!");
            }
            
            return result;
        }
    }
}