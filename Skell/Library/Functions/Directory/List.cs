using System;
using System.Collections.Generic;
using io = System.IO;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions.Directory
{
    public class List : BuiltinLambda
    {
        public List()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            var system = state.Namespaces.Get("System");
            var dir = (Skell.Types.Namespace) system.Get("Directory");
            var cwd = (Skell.Types.Property) dir.Get("Current");

            var path = ((Skell.Types.String) cwd.Value).contents;
            
            if (io.File.Exists(path) && io.File.GetAttributes(path).HasFlag(io.FileAttributes.Directory)) {
                var list = io.Directory.GetFiles(path).Select((path) => new Skell.Types.String(path));
                return new Skell.Types.Array(list.ToArray());
            }

            return new Skell.Types.None();
        }
    }

    public class ListDir : BuiltinLambda
    {
        public ListDir()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>
            {
                new Tuple<string, Skell.Types.Specifier>("string", Skell.Types.Specifier.String)
            };
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            var system = state.Namespaces.Get("System");
            var dir = (Skell.Types.Namespace) system.Get("Directory");
            var cwd = (Skell.Types.Property) dir.Get("Current");

            var path = ((Skell.Types.String) args.First().Item3).contents;
            
            if (io.File.Exists(path) && io.File.GetAttributes(path).HasFlag(io.FileAttributes.Directory)) {
                var list = io.Directory.GetFiles(path).Select((path) => new Skell.Types.String(path));
                return new Skell.Types.Array(list.ToArray());
            }

            return new Skell.Types.None();
        }
    }
}