using Shell.Types;

namespace Shell.Library
{
    public class Indexable : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var data_ns = new Shell.Types.Namespace(typeof(Indexable).Name);

            var lengthfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Indexable.Length).Name);
            lengthfn.AddBuiltinLambda(new Shell.Library.Functions.Indexable.Length());
            data_ns.Set(lengthfn.name, lengthfn);

            var containsfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Indexable.Contains).Name);
            containsfn.AddBuiltinLambda(new Shell.Library.Functions.Indexable.Contains());
            data_ns.Set(containsfn.name, containsfn);

            var keysfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Indexable.Keys).Name);
            keysfn.AddBuiltinLambda(new Shell.Library.Functions.Indexable.Keys());
            data_ns.Set(keysfn.name, keysfn);

            return data_ns;
        }
    }
}