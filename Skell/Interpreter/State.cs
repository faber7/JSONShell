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

        /// Backups of flag_return, current context, and the functions context
        /// Created and restored during ENTER_RETURNABLE_CONTEXT and EXIT_RETURNABLE_CONTEXT
        private Tuple<bool, Context<Skell.Types.ISkellType>, Context<Skell.Types.Function>> temp_state;
        /// Backup of functions context
        /// Created and restored during DISABLE_GLOBAL_FUNCTIONS and ENABLE_GLOBAL_FUNCTIONS
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
        /// Any new functions created are lost on switching back
        /// </summary>
        public void ENTER_RETURNABLE_CONTEXT(string name) {
            var cont = new Context<Skell.Types.ISkellType>("{" + name + "}");
            var new_functions = Context<Skell.Types.Function>.Copy(functions);

            logger.Verbose($"Entering a new returnable context {cont.name} from {context.name}. Creating a copy of function declarations.");

            // Create a backup of current context and function context
            temp_state = new Tuple<bool, Context<Skell.Types.ISkellType>, Context<Skell.Types.Function>>(
                flag_return,
                this.context,
                functions
            );

            flag_return = true;
            contexts.Add(cont);
            context = cont;
            functions = new_functions;
        }

        /// <summary>
        /// Safely exit the context created by ENTER_RETURNABLE_CONTEXT()
        /// </summary>
        public void EXIT_RETURNABLE_CONTEXT() {
            logger.Verbose($"Exiting from {context.name} to {temp_state.Item2.name}. Restoring previous copy of function declarations");
            flag_return = temp_state.Item1;
            contexts.Remove(context);
            context = temp_state.Item2;
            functions = temp_state.Item3;
            temp_state = null;
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