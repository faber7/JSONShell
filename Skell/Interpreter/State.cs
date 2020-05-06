using Serilog;
using System.Collections.Generic;
using System.Linq;

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
        /// Safely set flag_return
        /// The returned value must be passed to EXIT_RETURNABLE_STATE() even if nothing was returned
        /// </summary>
        public bool ENTER_RETURNABLE_STATE() {
            logger.Debug("Entering returnable state from current: " + flag_return);
            var ret = flag_return;
            flag_return = true;
            return ret;
        }

        /// <summary>
        /// Unsets flag_return
        /// Pass the returned value of ENTER_RETURNABLE_STATE(), even if nothing was returned
        /// </summary>
        public void EXIT_RETURNABLE_STATE(bool last) {
            logger.Debug("Exiting from current returnable state to previous: " + last);
            flag_return = last;
        }

        /// <summary>
        /// Safely switch to a new context
        /// The returned context is the current context
        /// Pass it to EXIT_CONTEXT() to switch back
        /// </summary>
        public Context ENTER_CONTEXT(string name) {
            logger.Debug($"Entering a new context from {this.context.contextName}");
            Context cont = new Context("{" + name + "}");
            contexts.Add(cont);
            var last_context = this.context;
            context = cont;
            return last_context;
        }

        /// <summary>
        /// Exit from the current context to the given context
        /// </summary>
        public void EXIT_CONTEXT(Context prevContext) {
            logger.Debug($"Exiting context {context.contextName} to {prevContext.contextName}");
            contexts.Remove(context);
            context = prevContext;
        }
    }
}