using System;
using Serilog;
using Shell.Types;

namespace Shell.Library
{
    public class Directory : IBuiltinLibrary
    {
        public Namespace AsNamespace()
        {
            var dir_ns = new Shell.Types.Namespace(typeof(Directory).Name);

            dir_ns.Set("Current", GetCWD());

            var changefn = new Shell.Types.Function(typeof(Shell.Library.Functions.Directory.Change).Name);
            changefn.AddBuiltinLambda(new Shell.Library.Functions.Directory.Change());
            dir_ns.Set(changefn.name, changefn);

            var listfn = new Shell.Types.Function(typeof(Shell.Library.Functions.Directory.List).Name);
            listfn.AddBuiltinLambda(new Shell.Library.Functions.Directory.List());
            listfn.AddBuiltinLambda(new Shell.Library.Functions.Directory.ListDir());
            dir_ns.Set(listfn.name, listfn);

            return dir_ns;
        }

        private Property GetCWD()
        {
            var cwd = Environment.CurrentDirectory;

            var prop = new Property((ncwd) =>
            {
                
            })
            {
                Value = new Shell.Types.String(cwd)
            };

            return prop;
        }
    }
}