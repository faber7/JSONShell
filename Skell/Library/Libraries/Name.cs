using Skell.Types;

namespace Skell.Library
{
    public class Name : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var name_ns = new Skell.Types.Namespace(typeof(Name).Name);

            var availablefn = new Skell.Types.Function(typeof(Skell.Library.Functions.Name.Available).Name);
            availablefn.AddBuiltinLambda(new Skell.Library.Functions.Name.Available());
            name_ns.Set(availablefn.name, availablefn);

            var unsetfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Name.Unset).Name);
            unsetfn.AddBuiltinLambda(new Skell.Library.Functions.Name.Unset());
            name_ns.Set(unsetfn.name, unsetfn);

            return name_ns;
        }
    }
}