using Shell.Types;

namespace Shell.Library
{
    public class String : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var string_ns = new Shell.Types.Namespace(typeof(String).Name);

            var lengthfn = new Shell.Types.Function(typeof(Shell.Library.Functions.String.Length).Name);
            lengthfn.AddBuiltinLambda(new Shell.Library.Functions.String.Length());
            string_ns.Set(lengthfn.name, lengthfn);

            var substrfn = new Shell.Types.Function(typeof(Shell.Library.Functions.String.Substring).Name);
            substrfn.AddBuiltinLambda(new Shell.Library.Functions.String.Substring());
            string_ns.Set(substrfn.name, substrfn);

            return string_ns;
        }
    }
}