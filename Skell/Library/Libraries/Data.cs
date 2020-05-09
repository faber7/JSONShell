using Skell.Types;

namespace Skell.Library
{
    public class Data : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var data_ns = new Skell.Types.Namespace(typeof(Data).Name);

            var lengthfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Data.Length).Name);
            lengthfn.AddBuiltinLambda(new Skell.Library.Functions.Data.Length());
            data_ns.Set(lengthfn.name, lengthfn);

            var containsfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Data.Contains).Name);
            containsfn.AddBuiltinLambda(new Skell.Library.Functions.Data.Contains());
            data_ns.Set(containsfn.name, containsfn);

            return data_ns;
        }
    }
}