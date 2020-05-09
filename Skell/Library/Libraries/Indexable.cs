using Skell.Types;

namespace Skell.Library
{
    public class Indexable : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var data_ns = new Skell.Types.Namespace(typeof(Indexable).Name);

            var lengthfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Indexable.Length).Name);
            lengthfn.AddBuiltinLambda(new Skell.Library.Functions.Indexable.Length());
            data_ns.Set(lengthfn.name, lengthfn);

            var containsfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Indexable.Contains).Name);
            containsfn.AddBuiltinLambda(new Skell.Library.Functions.Indexable.Contains());
            data_ns.Set(containsfn.name, containsfn);

            var pushfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Indexable.Push).Name);
            pushfn.AddBuiltinLambda(new Skell.Library.Functions.Indexable.Push());
            data_ns.Set(pushfn.name, pushfn);

            var popfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Indexable.Pop).Name);
            popfn.AddBuiltinLambda(new Skell.Library.Functions.Indexable.Pop());
            data_ns.Set(popfn.name, popfn);

            var keysfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Indexable.Keys).Name);
            keysfn.AddBuiltinLambda(new Skell.Library.Functions.Indexable.Keys());
            data_ns.Set(keysfn.name, keysfn);

            return data_ns;
        }
    }
}