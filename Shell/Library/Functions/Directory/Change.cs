using System;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;
using System.IO;

namespace Shell.Library.Functions.Directory
{
    public class Change : BuiltinLambda
    {
        public Change()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("string", Shell.Types.Specifier.String)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            var path = (Shell.Types.String) args.First().Item3;
            var system = state.Namespaces.Get("System");
            var dir = (Shell.Types.Namespace) system.Get("Directory");

            var cwd = (Shell.Types.Property) dir.Get("Current");
            
            if (new DirectoryInfo(path.contents).Exists) {
                if (path.contents.StartsWith("/"))
                    cwd.Value = new Shell.Types.String(path.contents);
                else {
                    var ncwd = Path.Join(((Shell.Types.String)cwd.Value).contents, path.contents);
                    cwd.Value = new Shell.Types.String(ncwd);
                }
            }

            return new Shell.Types.None();
        }
    }
}