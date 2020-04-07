using System.Collections.Generic;
using Serilog;

namespace Skell.Interpreter
{
    internal class Context
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, Skell.Types.ISkellType> mem;
        public readonly string contextName;

        public Context(string name)
        {
            logger = Log.ForContext<Context>();
            mem = new Dictionary<string, Skell.Types.ISkellType>();
            contextName = name;
        }

        public bool Exists(string name) => mem.ContainsKey(name);

        public void Set(string name, Skell.Types.ISkellType data)
        {
            mem[name] = data;
            logger.Debug($"In {contextName}, {name} is now {data}");
        }

        public Skell.Types.ISkellType Get(string name)
        {
            logger.Verbose($"In {contextName}, accessing {name}");
            if (!Exists(name)) {
                throw new System.NotImplementedException();
            }
            return mem[name];
        }

        public void Delete(string name) 
        {
            logger.Debug($"In {contextName}, {name} was {mem[name]}, now removed");
            mem.Remove(name);
        }
    }
}