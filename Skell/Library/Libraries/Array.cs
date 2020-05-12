using Skell.Types;

namespace Skell.Library
{
    public class Array : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var array_ns = new Skell.Types.Namespace(typeof(Array).Name);

            var pushfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Array.Push).Name);
            pushfn.AddBuiltinLambda(new Skell.Library.Functions.Array.Push());
            array_ns.Set(pushfn.name, pushfn);

            var popfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Array.Pop).Name);
            popfn.AddBuiltinLambda(new Skell.Library.Functions.Array.Pop());
            array_ns.Set(popfn.name, popfn);

            var insertfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Array.Insert).Name);
            insertfn.AddBuiltinLambda(new Skell.Library.Functions.Array.Insert());
            array_ns.Set(insertfn.name, insertfn);

            var deletefn = new Skell.Types.Function(typeof(Skell.Library.Functions.Array.Delete).Name);
            deletefn.AddBuiltinLambda(new Skell.Library.Functions.Array.Delete());
            array_ns.Set(deletefn.name, deletefn);

            var rangefn = new Skell.Types.Function(typeof(Skell.Library.Functions.Array.Range).Name);
            rangefn.AddBuiltinLambda(new Skell.Library.Functions.Array.Range());
            array_ns.Set(rangefn.name, rangefn);

            return array_ns;
        }
    }
}