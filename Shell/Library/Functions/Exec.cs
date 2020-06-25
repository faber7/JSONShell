using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions
{
    public class Exec : BuiltinLambda
    {
        public Exec()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("string_array", Shell.Types.Specifier.Array)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            var array = args.First().Item3;
            
            if (array is Shell.Types.Array arr && arr.IsHomogeneous(Shell.Types.Specifier.String)) {
                var proc = new Process();
                var arguments = arr.ListValues().Select((val) => val.ToString()).ToList();

                proc.StartInfo.FileName = arguments.First();
                proc.StartInfo.Arguments = string.Join(" ", arguments.Skip(1));
                proc.Start();
                proc.WaitForExit();

                return new Number(proc.ExitCode);
            }

            return new Shell.Types.None();
        }
    }
}