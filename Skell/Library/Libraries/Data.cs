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

            return data_ns;
        }
    }
}