using System.Collections.Generic;

namespace Skell.Library
{
    public class System : IBuiltinLibrary
    {
        public Skell.Types.Namespace AsNamespace()
        {
            var system = new Skell.Types.Namespace(typeof(System).Name);

            var libraries = new List<Skell.Types.Namespace>();
            libraries.Add((new Skell.Library.Data()).AsNamespace());

            foreach (var library in libraries) {
                library.parent = system;
                system.Set(library.name, library);
            }

            return system;
        }
    }
}