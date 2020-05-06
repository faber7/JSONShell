using System.Collections.Generic;
using System.Linq;
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
            logger.Debug($"In {contextName}, {name} = {data}");
        }

        public Skell.Types.ISkellType Get(string name)
        {
            if (!Exists(name)) {
                Skell.Types.String index = new Skell.Types.String(name);
                var indicesArray = mem.Keys.Select(str => new Skell.Types.String(str)).ToArray();
                Skell.Types.Array indices = new Skell.Types.Array(indicesArray);
                throw new Skell.Problems.IndexOutOfRange(index, indices);
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