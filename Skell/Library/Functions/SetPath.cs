using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions
{
    public class SetPath : BuiltinLambda
    {
        public SetPath()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("string_array", Skell.Types.Specifier.Array)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            var array = args.First().Item3;
            var system = state.Namespaces.Get(typeof(Skell.Library.System).Name);

            var arrPath = new List<Skell.Types.String>();
            
            if (array is Skell.Types.Array arr && arr.IsHomogeneous(Skell.Types.Specifier.String)) {
                foreach (var strPath in arr.ListIndices()) {
                    var path = ((Skell.Types.String) strPath).contents;
                    if (File.Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                        arrPath.Insert(arrPath.Count(), (Skell.Types.String) strPath);
                }

                var pathprop = (Skell.Types.Property) system.Get("Path");
                pathprop.Value = new Skell.Types.Array(arrPath.ToArray());

                var pathstr = string.Join(':', arrPath);
                Environment.SetEnvironmentVariable("PATH", pathstr);
            }
            
            return new Skell.Types.None();
        }
    }
}