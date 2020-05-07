using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Skell.Interpreter
{
    internal class Context<T>
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, T> mem;
        public readonly string contextName;

        public Context(string name)
        {
            logger = Log.ForContext<Context<T>>();
            mem = new Dictionary<string, T>();
            contextName = name;
        }

        public bool Exists(string name) => mem.ContainsKey(name);

        public void Set(string name, T data)
        {
            mem[name] = data;
            logger.Debug($"In {contextName}, {name} = {data}");
        }

        public T Get(string name)
        {
            if (!Exists(name)) {
                Skell.Types.String index = new Skell.Types.String(name);
                var indicesArray = mem.Keys.Select(str => (Skell.Types.ISkellType) new Skell.Types.String(str)).ToArray();
                Skell.Types.Array indices = new Skell.Types.Array(indicesArray);
                throw new Skell.Problems.IndexOutOfRange((Skell.Types.ISkellType) index, indices);
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