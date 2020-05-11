using Serilog;
using System;
using System.Collections.Generic;

namespace Skell.Interpreter
{
    public class State
    {
        private static ILogger logger;

        // These variables track the state of the interpreter

        private readonly List<Context<Skell.Types.ISkellType>> all_variables;
        internal Context<Skell.Types.ISkellType> current_variables;
        internal Context<Skell.Types.Function> current_functions;
        internal Dictionary<string, Skell.Types.Namespace> all_namespaces;
        private bool flag_return;
        private bool flag_returned;

        // These are backup variables, only used with the 4 special functions in this class

        private Tuple<bool, Context<Skell.Types.ISkellType>, Context<Skell.Types.Function>> temp_state;
        private Context<Skell.Types.Function> temp_functions;

        // These variables act as the public api for handling
        // requests related to variables, functions, namespaces, and names
        public readonly VariableHandler Variables;
        public readonly FunctionHandler Functions;
        public readonly NamespaceHandler Namespaces;

        // More or less intended for debugging purposes
        public readonly NameHandler Names;

        public State()
        {
            logger = Log.ForContext<State>();
            var global = new Context<Skell.Types.ISkellType>("ORIGIN");
            all_variables = new List<Context<Skell.Types.ISkellType>>();
            all_variables.Add(global);
            current_variables = global;
            current_functions = new Context<Skell.Types.Function>("FUNCTIONS");
            all_namespaces = new Dictionary<string, Skell.Types.Namespace>();

            this.Variables = new VariableHandler(this);
            this.Functions = new FunctionHandler(this);
            this.Namespaces = new NamespaceHandler(this);
            this.Names = new NameHandler(this);
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
            var new_functions = Context<Skell.Types.Function>.Copy(current_functions);

            logger.Verbose($"Entering a new returnable context {cont.name} from {current_variables.name}. Creating a copy of function declarations.");

            // Create a backup of current context and function context
            temp_state = new Tuple<bool, Context<Skell.Types.ISkellType>, Context<Skell.Types.Function>>(
                flag_return,
                this.current_variables,
                current_functions
            );

            flag_return = true;
            all_variables.Add(cont);
            current_variables = cont;
            current_functions = new_functions;
        }

        /// <summary>
        /// Safely exit the context created by ENTER_RETURNABLE_CONTEXT()
        /// </summary>
        public void EXIT_RETURNABLE_CONTEXT() {
            logger.Verbose($"Exiting from {current_variables.name} to {temp_state.Item2.name}. Restoring previous copy of function declarations");
            flag_return = temp_state.Item1;
            all_variables.Remove(current_variables);
            current_variables = temp_state.Item2;
            current_functions = temp_state.Item3;
            temp_state = null;
        }

        /// <summary>
        /// Disable user-defined functions.
        /// Note that namespaced functions will still work,
        /// as well as any locally defined functions after this call
        /// </summary>
        public void DISABLE_GLOBAL_FUNCTIONS(bool log = false) {
            if (log)
                logger.Verbose("Disabling all existing functions");

            temp_functions = current_functions;
            current_functions = new Context<Types.Function>("TEMP_FUNCTIONS");
        }

        /// <summary>
        /// Enable user-defined functions.
        /// </summary>
        public void ENABLE_GLOBAL_FUNCTIONS(bool log = false) {
            if (log)
                logger.Verbose("Restoring global functions");

            current_functions = temp_functions;
            temp_functions = new Context<Types.Function>("TEMP_FUNCTIONS");
        }

        public class NameHandler
        {
            private string error_not_found = "Name not found";

            private ILogger logger;
            private VariableHandler variables;
            private FunctionHandler functions;
            private NamespaceHandler namespaces;

            public NameHandler(State s)
            {
                logger = Log.ForContext<NameHandler>();
                variables = s.Variables;
                functions = s.Functions;
                namespaces = s.Namespaces;
            }

            /// <summary>
            /// Use to check whether a name can be used for defining something
            /// </summary>
            /// <remark>
            /// Not a specific check, only for checking if a name is open across every type
            /// </remark>
            public bool Available(string s)
                => !Exists(s);

            /// <summary>
            /// Use this before the other functions
            /// </summary>
            public bool Exists(string s)
                => (variables.Exists(s) || functions.Exists(s) || namespaces.Exists(s));

            /// <summary>
            /// Use to check if the name is defined as the expected type
            /// </summary>
            public bool DefinedAs(string s, System.Type type)
            {
                if (Available(s))
                    return false;

                if (variables.Exists(s) && variables.Get(s).GetType() == type)
                    return true;
                else if (functions.Exists(s) && functions.Get(s).GetType() == type)
                    return true;
                else if (namespaces.Exists(s) && namespaces.Get(s).GetType() == type)
                    return true;

                return false;
            }

            /// <summary>
            /// Use to check if the name is defined as a variable
            /// </summary>
            public bool DefinedAsVariable(string s)
            {
                if (Available(s))
                    return false;

                if (variables.Exists(s) && variables.Get(s) is Skell.Types.ISkellType)
                    return true;

                return false;
            }

            /// <summary>
            /// Use to check if the name is defined as an indexable
            /// </summary>
            public bool DefinedAsIndexable(string s)
            {
                if (Available(s))
                    return false;

                if (variables.Exists(s) && variables.Get(s) is Skell.Types.ISkellIndexable)
                    return true;

                return false;
            }

            /// <summary>
            /// Use with DefinedAs to get the value of a name.
            /// </summary>
            public Skell.Types.ISkellNamedType DefinitionOf(string s)
            {
                if (Available(s)) {
                    logger.Error("Attempted to read a name that was invalid");
                    throw new System.Exception(error_not_found);
                }
                
                if (variables.Exists(s))
                    return variables.Get(s);
                else if (functions.Exists(s))
                    return functions.Get(s);
                else
                    return namespaces.Get(s);
            }

            /// <summary>
            /// Opens up a name for use
            /// </summary>
            public void Clear(string s)
            {
                if (variables.Exists(s))
                    variables.Delete(s);
                else if (functions.Exists(s))
                    functions.Delete(s);
                else if (namespaces.Exists(s))
                    namespaces.Delete(s);
            }

        }

        public class VariableHandler
        {
            private State state;

            public VariableHandler(State s)
            {
                state = s;
            }

            public bool Exists(string name) => state.current_variables.Exists(name);

            public Skell.Types.ISkellType Get(string name) => state.current_variables.Get(name);

            public void Set(string name, Skell.Types.ISkellType value) => state.current_variables.Set(name, value);
            
            public void Delete(string name) => state.current_variables.Delete(name);
        }

        public class FunctionHandler
        {
            private State state;

            public FunctionHandler(State s)
            {
                state = s;
            }

            public bool Exists(string name) => state.current_functions.Exists(name);

            public Skell.Types.Function Get(string name) => state.current_functions.Get(name);

            public void Set(string name, Skell.Types.Function value) => state.current_functions.Set(name, value);
            
            public void Delete(string name) => state.current_functions.Delete(name);
        }

        public class NamespaceHandler
        {
            private State state;

            internal NamespaceHandler(State s)
            {
                state = s;
            }

            public void Register(Skell.Types.Namespace ns) => state.all_namespaces.Add(ns.name, ns);

            public bool Exists(string name) => state.all_namespaces.ContainsKey(name);

            public Skell.Types.Namespace Get(string name) => state.all_namespaces[name];

            public void Set(string name, Skell.Types.Namespace ns) => state.all_namespaces[name] = ns;

            public void Delete(string name) => state.all_namespaces.Remove(name);
        }
    }
}