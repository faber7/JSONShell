using System.Collections.Generic;

namespace Skell.Interpreter
{
    internal class Context
    {
        private readonly Dictionary<string, Skell.Types.ISkellType> mem;

        public Context()
        {
            mem = new Dictionary<string, Skell.Types.ISkellType>();
        }

        public bool Exists(string name) => mem.ContainsKey(name);

        public void Set(string name, Skell.Types.ISkellType data) => mem[name] = data;

        public Skell.Types.ISkellType Get(string name)
        {
            if (!Exists(name)) {
                throw new System.NotImplementedException();
            }
            return mem[name];
        }

        public void Delete(string name) => mem.Remove(name);
    }
}