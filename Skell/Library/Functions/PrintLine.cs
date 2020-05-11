using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions
{
    public class PrintLine : BuiltinLambda
    {
        public PrintLine()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("value", Skell.Types.Specifier.Any));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            var result = new Skell.Types.String(args.First().Item3.ToString());
            Console.WriteLine(result.contents);

            return new Skell.Types.None();
        }
    }
}