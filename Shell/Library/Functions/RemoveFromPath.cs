using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Shell.Interpreter;
using Shell.Types;

namespace Shell.Library.Functions
{
    public class RemoveFromPath : BuiltinLambda
    {
        public RemoveFromPath()
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
            var arrPath = (Shell.Types.Array) arr.Value;

            if (path is Shell.Types.String strPath) 
                if (arrPath.IndexOf(strPath) is Shell.Types.IShellData index) {
                    arrPath.Delete(index);
                    arr.Value = new Shell.Types.Array(arrPath.ListValues());
                }
            
            return new Shell.Types.None();
        }
    }
}