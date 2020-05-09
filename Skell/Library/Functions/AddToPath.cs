using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Skell.Interpreter;
using Skell.Types;

namespace Skell.Library.Functions
{
    public class AddToPath : BuiltinLambda
    {
        public AddToPath()
        {
            argList = new List<Tuple<string, Skell.Types.Specifier>>();
            argList.Add(new Tuple<string, Skell.Types.Specifier>("path", Skell.Types.Specifier.String));
        }

        public override ISkellReturnable Execute(State state, List<Tuple<int, string, ISkellType>> args)
        {
            var path = args.First().Item3;
            var system = state.Namespaces.Get(typeof(Skell.Library.System).Name);

            if (path is Skell.Types.String strPath && system.Get("Path") is Skell.Types.Array arrPath) 
                if (File.Exists(strPath.contents) && File.GetAttributes(strPath.contents).HasFlag(FileAttributes.Directory)) {
                    arrPath.Insert(arrPath.Count(), strPath);
                    var pathstr = string.Join(':', arrPath);
                    Environment.SetEnvironmentVariable("PATH", pathstr);
                }
            
            return new Skell.Types.None();
        }
    }
}