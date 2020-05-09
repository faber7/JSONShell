using System;
using Serilog;
using Skell.Types;

namespace Skell.Library
{
    public class Directory : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var dir_ns = new Skell.Types.Namespace(typeof(Directory).Name);

            dir_ns.Set("Current", getCWD());

            var changefn = new Skell.Types.Function(typeof(Skell.Library.Functions.Directory.Change).Name);
            changefn.AddBuiltinLambda(new Skell.Library.Functions.Directory.Change());
            dir_ns.Set(changefn.name, changefn);

            var listfn = new Skell.Types.Function(typeof(Skell.Library.Functions.Directory.List).Name);
            listfn.AddBuiltinLambda(new Skell.Library.Functions.Directory.List());
            listfn.AddBuiltinLambda(new Skell.Library.Functions.Directory.ListDir());
            dir_ns.Set(listfn.name, listfn);

            return dir_ns;
        }

        private Property getCWD()
        {
            var cwd = Environment.CurrentDirectory;

            var prop = new Property((ncwd) => {
                Log.Verbose($"Current directory is now {ncwd}");
            });
            prop.value = new Skell.Types.String(cwd);

            return prop;
        }
    }
}