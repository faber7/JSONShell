using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Shell.Library
{
    public class System : IBuiltinLibrary
    {
        public Shell.Types.Namespace AsNamespace()
        {
            var system = new Shell.Types.Namespace(typeof(System).Name);

            var libraries = new List<Shell.Types.Namespace>
            {
                (new Shell.Library.Indexable()).AsNamespace(),
                (new Shell.Library.Name()).AsNamespace(),
                (new Shell.Library.Directory()).AsNamespace(),
                (new Shell.Library.Array()).AsNamespace(),
                (new Shell.Library.String()).AsNamespace()
            };

            foreach (var library in libraries) {
                library.parent = system;
                system.Set(library.name, library);
            }

            var printfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Print).Name);
            printfn.AddBuiltinLambda(new Shell.Library.Functions.Print());
            system.Set(printfn.name, printfn);
            
            var printlnnfn = new Shell.Types.Function(typeof(Shell.Library.Functions.PrintLine).Name);
            printlnnfn.AddBuiltinLambda(new Shell.Library.Functions.PrintLine());
            system.Set(printlnnfn.name, printlnnfn);

            system.Set("Path", GetPath());

            var addtopathfn = new Shell.Types.Function(typeof(Shell.Library.Functions.AddToPath).Name);
            addtopathfn.AddBuiltinLambda(new Shell.Library.Functions.AddToPath());
            system.Set(addtopathfn.name, addtopathfn);

            var removefrompathfn = new Shell.Types.Function(typeof(Shell.Library.Functions.RemoveFromPath).Name);
            removefrompathfn.AddBuiltinLambda(new Shell.Library.Functions.RemoveFromPath());
            system.Set(removefrompathfn.name, removefrompathfn);

            var setpathfn = new Shell.Types.Function(typeof(Shell.Library.Functions.SetPath).Name);
            setpathfn.AddBuiltinLambda(new Shell.Library.Functions.SetPath());
            system.Set(setpathfn.name, setpathfn);

            var execfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Exec).Name);
            execfn.AddBuiltinLambda(new Shell.Library.Functions.Exec());
            system.Set(execfn.name, execfn);

            var exitfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Exit).Name);
            exitfn.AddBuiltinLambda(new Shell.Library.Functions.Exit());
            exitfn.AddBuiltinLambda(new Shell.Library.Functions.ExitWithCode());
            system.Set(exitfn.name, exitfn);

            return system;
        }

        private Shell.Types.Property GetPath()
        {
            var sysPath = Environment.GetEnvironmentVariable("PATH");
            var array = sysPath.Split(Path.PathSeparator).Select((str) => new Shell.Types.String(str)).ToArray();
            var skellArr = new Shell.Types.Array(array);
            var prop = new Shell.Types.Property((arr) =>
            {
                var array = ((Shell.Types.Array)arr).ListValues().Select((str) => ((Shell.Types.String)str).contents);
                var pathstr = string.Join(Path.PathSeparator, array);
                Environment.SetEnvironmentVariable("PATH", pathstr);
            })
            {
                Value = skellArr
            };
            return prop;
        }
    }
}