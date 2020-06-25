using System;
using System.Collections.Generic;
using io = System.IO;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions.Directory
{
    public class List : BuiltinLambda
    {
        public List()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>();
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            var system = state.Namespaces.Get("System");
            var dir = (Shell.Types.Namespace) system.Get("Directory");
            var cwd = (Shell.Types.Property) dir.Get("Current");

            var path = ((Shell.Types.String) cwd.Value).contents;
            
            if (new io.DirectoryInfo(path).Exists) {
                var list = io.Directory.GetFiles(path).Select((path) => new Shell.Types.String(path));
                return new Shell.Types.Array(list.ToArray());
            }

            return new Shell.Types.None();
        }
    }

    public class ListDir : BuiltinLambda
    {
        public ListDir()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("string", Shell.Types.Specifier.String)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            var system = state.Namespaces.Get("System");
            var dir = (Shell.Types.Namespace) system.Get("Directory");
            var cwd = (Shell.Types.Property) dir.Get("Current");

            var path = ((Shell.Types.String) args.First().Item3).contents;
            
            if (new io.DirectoryInfo(path).Exists) {
                var list = io.Directory.GetFiles(path).Select((path) => new Shell.Types.String(path));
                return new Shell.Types.Array(list.ToArray());
            }

            return new Shell.Types.None();
        }
    }
}