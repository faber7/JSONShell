using Skell.Types;

namespace Skell.Library
{
    public class String : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var string_ns = new Skell.Types.Namespace(typeof(String).Name);

            var lengthfn = new Skell.Types.Function(typeof(Skell.Library.Functions.String.Length).Name);
            lengthfn.AddBuiltinLambda(new Skell.Library.Functions.String.Length());
            string_ns.Set(lengthfn.name, lengthfn);

            var substrfn = new Skell.Types.Function(typeof(Skell.Library.Functions.String.Substring).Name);
            substrfn.AddBuiltinLambda(new Skell.Library.Functions.String.Substring());
            string_ns.Set(substrfn.name, substrfn);

            return string_ns;
        }
    }
}