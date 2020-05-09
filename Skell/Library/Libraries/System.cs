using System;
using System.Collections.Generic;
using System.Linq;

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

            foreach (var library in libraries) {
                library.parent = system;
                system.Set(library.name, library);
            }

            var printfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Print).Name);
            printfn.AddBuiltinLambda(new Skell.Library.Functions.Print());
            system.Set(typeof(Skell.Library.Functions.Print).Name, printfn);

            system.Set("Path", getPath());

            var addtopathfn = new Skell.Types.Function(typeof(Skell.Library.Functions.AddToPath).Name);
            addtopathfn.AddBuiltinLambda(new Skell.Library.Functions.AddToPath());
            system.Set(typeof(Skell.Library.Functions.AddToPath).Name, addtopathfn);

            var removefrompathfn = new Skell.Types.Function(typeof(Skell.Library.Functions.RemoveFromPath).Name);
            removefrompathfn.AddBuiltinLambda(new Skell.Library.Functions.RemoveFromPath());
            system.Set(typeof(Skell.Library.Functions.RemoveFromPath).Name, removefrompathfn);

            var setpathfn = new Skell.Types.Function(typeof(Skell.Library.Functions.SetPath).Name);
            setpathfn.AddBuiltinLambda(new Skell.Library.Functions.SetPath());
            system.Set(typeof(Skell.Library.Functions.SetPath).Name, setpathfn);

            return system;
        }

        private Skell.Types.Array getPath()
        {
            var sysPath = Environment.GetEnvironmentVariable("PATH");
            var array = sysPath.Split(':').Select((str) => new Skell.Types.String(str)).ToArray();
            return new Skell.Types.Array(array);
        }
    }
}