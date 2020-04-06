using System.Collections.Generic;
using Serilog;

namespace Skell.Interpreter
{
    internal class Context
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, Skell.Types.ISkellType> mem;

        public Context()
        {
            logger = Log.ForContext<Context>();
            mem = new Dictionary<string, Skell.Types.ISkellType>();
        }

        public bool Exists(string name) => mem.ContainsKey(name);

        public void Set(string name, Skell.Types.ISkellType data)
        {
            mem[name] = data;
            logger.Debug($"{name} is now {data}");
        }

        public Skell.Types.ISkellType Get(string name)
        {
            if (!Exists(name)) {
                throw new System.NotImplementedException();
            }
            logger.Verbose($"Accessing {name}");
            return mem[name];
        }

        public void Delete(string name) 
        {
            logger.Debug($"{name} was {mem[name]}, now removed");
            mem.Remove(name);
        }
    }
}