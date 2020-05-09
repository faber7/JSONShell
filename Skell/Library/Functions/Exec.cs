using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions
{
    public class Exec : BuiltinLambda
    {
        public Exec()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("string_array", Skell.Types.Specifier.Array));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            var array = args.First().Item3;
            
            if (array is Skell.Types.Array arr && arr.IsHomogeneous(Skell.Types.Specifier.String)) {
                var proc = new Process();
                var arguments = arr.ListValues().Select((val) => val.ToString()).ToList();

                proc.StartInfo.FileName = arguments.First();
                proc.StartInfo.Arguments = string.Join(" ", arguments.Skip(1));
                proc.Start();
                proc.WaitForExit();

                return new Number(proc.ExitCode);
            }

            return new Skell.Types.None();
        }
    }
}