using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Skell.Interpreter
{
    class State
    {
        private static ILogger logger;
        private readonly List<Context<Skell.Types.ISkellType>> contexts;
        public Context<Skell.Types.ISkellType> context;
        public Context<Skell.Types.Function> functions;
        public Dictionary<string, Namespace> namespaces;

        private bool flag_return;
        private bool flag_returned;
        private Context<Skell.Types.Function> temp_functions;

        public State()
        {
            logger = Log.ForContext<State>();
            var global = new Context<Skell.Types.ISkellType>("ORIGIN");
            contexts = new List<Context<Skell.Types.ISkellType>>();
            contexts.Add(global);
            context = global;
            functions = new Context<Skell.Types.Function>("FUNCTIONS");
            namespaces = new Dictionary<string, Namespace>();
        }

        public bool can_return() => flag_return;
        public bool has_returned() => flag_returned;
        public bool start_return() => flag_returned = true;
        public bool end_return() => flag_returned = false;

        /// <summary>
        /// Safely enter a new returnable context
        /// The returned values must be passed back to EXIT_RETURNABLE_CONTEXT() to switch back
        /// </summary>
        public Tuple<bool, Context<Skell.Types.ISkellType>> ENTER_RETURNABLE_CONTEXT(string name) {
            var ret = flag_return;
            var cont = new Context<Skell.Types.ISkellType>("{" + name + "}");

            logger.Verbose($"Entering a new returnable context {cont.contextName} from {context.contextName}");

            flag_return = true;
            contexts.Add(cont);
            var last_context = this.context;
            context = cont;

            return new Tuple<bool, Context<Skell.Types.ISkellType>>(ret, last_context);
        }

        /// <summary>
        /// Unsets flag_return
        /// Pass the returned value of ENTER_RETURNABLE_STATE(), even if nothing was returned
        /// </summary>
        public void EXIT_RETURNABLE_CONTEXT(Tuple<bool, Context<Skell.Types.ISkellType>> last) {
            logger.Verbose($"Exiting from {context.contextName} to {last.Item2.contextName}");
            flag_return = last.Item1;
            contexts.Remove(context);
            context = last.Item2;
        }

        /// <summary>
        /// Disable user-defined functions.
        /// Note that namespaced functions will still work,
        /// as well as any locally defined functions after this call
        /// </summary>
        public void DISABLE_GLOBAL_FUNCTIONS(bool log = false) {
            if (log) {
                logger.Verbose("Disabling all existing functions");
            }
            temp_functions = functions;
            functions = new Context<Types.Function>("TEMP_FUNCTIONS");
        }

        /// <summary>
        /// Enable user-defined functions.
        /// </summary>
        public void ENABLE_GLOBAL_FUNCTIONS(bool log = false) {
            if (log) {
                logger.Verbose("Restoring global functions");
            }
            functions = temp_functions;
            temp_functions = new Context<Types.Function>("TEMP_FUNCTIONS");
        }

        public void RegisterNamespace(Namespace ns)
        {
            namespaces.Add(ns.name, ns);
        }

        public Namespace GetNamespace(string name)
        {
            if (namespaces.ContainsKey(name)) {
                return namespaces[name];
            } else {
                throw new Skell.Problems.InvalidNamespace(
                    name,
                    namespaces.Keys.Select((a, b) => new Skell.Types.String(a)).ToArray()
                );
            }
        }
    }
}