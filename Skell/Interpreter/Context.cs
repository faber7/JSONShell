using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Skell.Interpreter
{
    internal class Context<T>
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, T> mem;
        public readonly string name;

        public Context(string name)
        {
            logger = Log.ForContext<Context<T>>();
            mem = new Dictionary<string, T>();
            this.name = name;
        }

        public Dictionary<string, T> pairs => mem;

        public static Context<T> Copy(Context<T> context)
        {
            var ct = new Context<T>(context.name);
            foreach (var pair in context.pairs) {
                ct.Set(pair.Key, pair.Value);
            }
            return ct;
        }

        public bool Exists(string name) => mem.ContainsKey(name);

        public void Set(string name, T data)
        {
            mem[name] = data;
            logger.Debug($"In {this.name}, {name} = {data}");
        }

        public T Get(string name) => mem[name];

        public void Delete(string name)
        {
            logger.Debug($"In {this.name}, {name} was {mem[name]}, now removed");
            mem.Remove(name);
        }
    }
}