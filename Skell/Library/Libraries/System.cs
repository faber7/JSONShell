using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;

namespace Skell.Library
{
    public class System : IBuiltinLibrary
    {
        public Skell.Types.Namespace AsNamespace()
        {
            var system = new Skell.Types.Namespace(typeof(System).Name);

            var libraries = new List<Skell.Types.Namespace>();
            libraries.Add((new Skell.Library.Indexable()).AsNamespace());
            libraries.Add((new Skell.Library.Name()).AsNamespace());
            libraries.Add((new Skell.Library.Directory()).AsNamespace());

            foreach (var library in libraries) {
                library.parent = system;
                system.Set(library.name, library);
            }

            var printfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Print).Name);
            printfn.AddBuiltinLambda(new Skell.Library.Functions.Print());
            system.Set(printfn.name, printfn);

            system.Set("Path", getPath());

            var addtopathfn = new Skell.Types.Function(typeof(Skell.Library.Functions.AddToPath).Name);
            addtopathfn.AddBuiltinLambda(new Skell.Library.Functions.AddToPath());
            system.Set(addtopathfn.name, addtopathfn);

            var removefrompathfn = new Skell.Types.Function(typeof(Skell.Library.Functions.RemoveFromPath).Name);
            removefrompathfn.AddBuiltinLambda(new Skell.Library.Functions.RemoveFromPath());
            system.Set(removefrompathfn.name, removefrompathfn);

            var setpathfn = new Skell.Types.Function(typeof(Skell.Library.Functions.SetPath).Name);
            setpathfn.AddBuiltinLambda(new Skell.Library.Functions.SetPath());
            system.Set(setpathfn.name, setpathfn);

            var execfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Exec).Name);
            execfn.AddBuiltinLambda(new Skell.Library.Functions.Exec());
            system.Set(execfn.name, execfn);

            return system;
        }

        private Skell.Types.Property getPath()
        {
            var sysPath = Environment.GetEnvironmentVariable("PATH");
            var array = sysPath.Split(':').Select((str) => new Skell.Types.String(str)).ToArray();
            var skellArr = new Skell.Types.Array(array);
            var prop = new Skell.Types.Property((arr) => {
                var array = ((Skell.Types.Array) arr).ListValues().Select((str) => ((Skell.Types.String) str).contents);
                var pathstr = string.Join(':', array);
                Environment.SetEnvironmentVariable("PATH", pathstr);
                Log.Verbose($"Set $PATH: {pathstr}");
            });
            prop.value = skellArr;
            return prop;
        }
    }
}