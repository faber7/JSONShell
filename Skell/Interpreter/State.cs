using Serilog;
using System;
using System.Collections.Generic;

namespace Skell.Interpreter
{
    class State
    {
        private static ILogger logger;
        private readonly List<Context> contexts;
        public Context context;
        public Context functions;
        private bool flag_return;
        private bool flag_returned;

        public State()
        {
            logger = Log.ForContext<State>();
            Context global = new Context("GLOBAL");
            contexts = new List<Context>();
            contexts.Add(global);
            context = global;
            functions = new Context("FUNCTIONS");
        }

        public bool can_return() => flag_return;
        public bool has_returned() => flag_returned;
        public bool start_return() => flag_returned = true;
        public bool end_return() => flag_returned = false;

        /// <summary>
        /// Safely enter a new returnable context
        /// The returned values must be passed back to EXIT_RETURNABLE_CONTEXT() to switch back
        /// </summary>
        public Tuple<bool, Context> ENTER_RETURNABLE_CONTEXT(string name) {
            var ret = flag_return;
            Context cont = new Context("{" + name + "}");

            logger.Verbose($"Entering a new returnable context {cont.contextName} from {context.contextName}");

            flag_return = true;
            contexts.Add(cont);
            var last_context = this.context;
            context = cont;

            return new Tuple<bool, Context>(ret, last_context);
        }

        /// <summary>
        /// Unsets flag_return
        /// Pass the returned value of ENTER_RETURNABLE_STATE(), even if nothing was returned
        /// </summary>
        public void EXIT_RETURNABLE_CONTEXT(Tuple<bool, Context> last) {
            logger.Verbose($"Exiting from {context.contextName} to {last.Item2.contextName}");
            flag_return = last.Item1;
            contexts.Remove(context);
            context = last.Item2;
        }
    }
}