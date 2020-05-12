using System;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;
using System.IO;

namespace Skell.Library.Functions.Directory
{
    public class Change : BuiltinLambda
    {
        public Change()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("string", Skell.Types.Specifier.String)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            var path = (Skell.Types.String) args.First().Item3;
            var system = state.Namespaces.Get("System");
            var dir = (Skell.Types.Namespace) system.Get("Directory");

            var cwd = (Skell.Types.Property) dir.Get("Current");
            
            if (File.Exists(path.contents) && File.GetAttributes(path.contents).HasFlag(FileAttributes.Directory)) {
                if (path.contents.StartsWith("/"))
                    cwd.Value = new Skell.Types.String(path.contents);
                else {
                    var ncwd = Path.Join(((Skell.Types.String)cwd.Value).contents, path.contents);
                    cwd.Value = new Skell.Types.String(ncwd);
                }
            }

            return new Skell.Types.None();
        }
    }
}