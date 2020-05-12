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

            return array_ns;
        }
    }
}