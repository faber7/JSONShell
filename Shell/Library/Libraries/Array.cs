using Shell.Types;

namespace Shell.Library
{
    public class Array : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var array_ns = new Shell.Types.Namespace(typeof(Array).Name);

            var pushfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Array.Push).Name);
            pushfn.AddBuiltinLambda(new Shell.Library.Functions.Array.Push());
            array_ns.Set(pushfn.name, pushfn);

            var popfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Array.Pop).Name);
            popfn.AddBuiltinLambda(new Shell.Library.Functions.Array.Pop());
            array_ns.Set(popfn.name, popfn);

            var insertfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Array.Insert).Name);
            insertfn.AddBuiltinLambda(new Shell.Library.Functions.Array.Insert());
            array_ns.Set(insertfn.name, insertfn);

            var deletefn = new Shell.Types.Function(typeof(Shell.Library.Functions.Array.Delete).Name);
            deletefn.AddBuiltinLambda(new Shell.Library.Functions.Array.Delete());
            array_ns.Set(deletefn.name, deletefn);

            var rangefn = new Shell.Types.Function(typeof(Shell.Library.Functions.Array.Range).Name);
            rangefn.AddBuiltinLambda(new Shell.Library.Functions.Array.Range());
            array_ns.Set(rangefn.name, rangefn);

            return array_ns;
        }
    }
}