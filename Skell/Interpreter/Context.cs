using System.Collections.Generic;

namespace Skell.Interpreter
{
    internal class Context
    {
        private readonly Dictionary<string, Skell.Data.ISkellData> mem;

        public Context()
        {
            mem = new Dictionary<string, Data.ISkellData>();
        }

        public bool Exists(string name) => mem.ContainsKey(name);

        public void Set(string name, Skell.Data.ISkellData data) => mem[name] = data;

        public void Delete(string name) => mem.Remove(name);
    }
}