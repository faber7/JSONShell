using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions
{
    public class AddToPath : BuiltinLambda
    {
        public AddToPath()
        {
            argList = new List<Tuple<string, Shell.Types.Specifier>>
            {
                new Tuple<string, Shell.Types.Specifier>("path", Shell.Types.Specifier.String)
            };
        }

        public override IShellReturnable Execute(State state, List<Tuple<int, string, IShellData>> args)
        {
            var path = args.First().Item3;
            var system = state.Namespaces.Get(typeof(Shell.Library.System).Name);

            var arr = (Shell.Types.Property) system.Get("Path");
            var origarr = (Shell.Types.Array) arr.Value;
            var arrPath = new Shell.Types.Array(origarr.ListValues());

            if (path is Shell.Types.String strPath) 
                if (File.Exists(strPath.contents) && File.GetAttributes(strPath.contents).HasFlag(FileAttributes.Directory)) {
                    arrPath.Insert(arrPath.Count(), strPath);
                    arr.Value = new Shell.Types.Array(arrPath.ListValues());
                }
            
            return new Shell.Types.None();
        }
    }
}