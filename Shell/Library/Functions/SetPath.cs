using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions
{
    public class SetPath : BuiltinLambda
    {
        public SetPath()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("string_array", Shell.Types.Specifier.Array)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            var array = args.First().Item3;
            var system = state.Namespaces.Get(typeof(Shell.Library.System).Name);

            var arrPath = new List<Shell.Types.String>();
            
            if (array is Shell.Types.Array arr && arr.IsHomogeneous(Shell.Types.Specifier.String)) {
                foreach (var strPath in arr.ListIndices()) {
                    var path = ((Shell.Types.String) strPath).contents;
                    if (new DirectoryInfo(path).Exists)
                        arrPath.Insert(arrPath.Count(), (Shell.Types.String) strPath);
                }

                var pathprop = (Shell.Types.Property) system.Get("Path");
                pathprop.Value = new Shell.Types.Array(arrPath.ToArray());

                var pathstr = string.Join(':', arrPath);
                Environment.SetEnvironmentVariable("PATH", pathstr);
            }
            
            return new Shell.Types.None();
        }
    }
}