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

            return data_ns;
        }
    }
}