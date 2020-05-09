using System.Collections.Generic;

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

            return system;
        }
    }
}