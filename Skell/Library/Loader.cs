using System.Collections.Generic;

namespace Skell.Library
{
    public static class Loader
    {
        public static void Load(Skell.Interpreter.State state)
        {
            var namespaces = new List<Skell.Types.Namespace>();

            namespaces.Add((new Skell.Library.System()).AsNamespace());

            foreach (var ns in namespaces) {
                state.Namespaces.Set(ns.name, ns);
            }
        }
    }
}