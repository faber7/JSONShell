using Shell.Types;

namespace Shell.Library
{
    public class Name : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var name_ns = new Shell.Types.Namespace(typeof(Name).Name);

            var availablefn = new Shell.Types.Function(typeof(Shell.Library.Functions.Name.Available).Name);
            availablefn.AddBuiltinLambda(new Shell.Library.Functions.Name.Available());
            name_ns.Set(availablefn.name, availablefn);

            var unsetfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Name.Unset).Name);
            unsetfn.AddBuiltinLambda(new Shell.Library.Functions.Name.Unset());
            name_ns.Set(unsetfn.name, unsetfn);

            var aliasfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Name.Alias).Name);
            aliasfn.AddBuiltinLambda(new Shell.Library.Functions.Name.Alias());
            name_ns.Set(aliasfn.name, aliasfn);

            return name_ns;
        }
    }
}