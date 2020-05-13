using System.Collections.Generic;

namespace Shell.Library
{
    public static class Loader
    {
        public static void Load(Shell.Interpreter.State state)
        {
            var namespaces = new List<Shell.Types.Namespace>
            {
                (new Shell.Library.System()).AsNamespace()
            };

            foreach (var ns in namespaces) {
                state.Namespaces.Set(ns.name, ns);
            }
        }
    }
}