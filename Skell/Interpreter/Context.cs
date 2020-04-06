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

        public Skell.Data.ISkellData Get(string name)
        {
            if (!Exists(name)) {
                throw new System.NotImplementedException();
            }
            return mem[name];
        }

        public void Delete(string name) => mem.Remove(name);
    }
}