using Serilog;
using Skell.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skell.Problems
{
    abstract class Exception : System.Exception
    {
    }

    internal class UndefinedIdentifer : Exception
    {
        public UndefinedIdentifer(string name)
        {
            Log.Error($"Tried to access {name}");
        }

        public UndefinedIdentifer(Source src, string name)
        {
            Log.Error($"Identifier {name} at {src} does not exist");
        }

        public UndefinedIdentifer(Source src, string name, System.Type expected)
        {
            Log.Error($"Identifier {name} at {src} does not exist, expected {expected}");
        }
    }

    internal class UnexpectedType : Exception
    {
        public UnexpectedType(Source src, Skell.Types.ISkellInternal got, System.Type expected)
        {
            Log.Error($"Expected {expected}, got {got} of type {got.GetType()}");
        }

        public UnexpectedType(Source src, Skell.Types.ISkellInternal got, System.Type expected1, System.Type expected2)
        {
            Log.Error($"Expected {expected1} or {expected2}, got {got} of type {got.GetType()}");
        }
    }

    internal class IndexOutOfRange : Exception
    {
        public IndexOutOfRange(Source src, Skell.Types.ISkellType index, Skell.Types.ISkellIndexable obj)
        {
            
            Log.Error($"Expected an index from {obj.ListIndices()} at {src}, got {index}");
        }
    }

    internal class InvalidOperation : Exception
    {
        public InvalidOperation(string operation, Skell.Types.ISkellReturnable operand)
        {
            Log.Error($"Operation {operation} cannot be performed on {operand}");
        }

        public InvalidOperation(string operation, Skell.Types.ISkellReturnable operand1, Skell.Types.ISkellReturnable operand2)
        {
            Log.Error($"Cannot perform operation {operand1} {operation} {operand2}");
        }
    }

    internal class UnaccessibleLHS : Exception
    {
        public UnaccessibleLHS(Source src)
        {
            Log.Error($"Attempted to declare an unaccessible expression at {src}");
        }
    }

    internal class InvalidDefinition : Exception
    {
        public InvalidDefinition(
            Source src,
            string name
        ) {
            Log.Error($"Attempted to redefine {name} at {src}");
        }

        public InvalidDefinition(
            Source src,
            string name,
            Skell.Interpreter.State state
            
        ) {
            var from = state.Names.DefinitionOf(name);
            Log.Error($"Attempted to redefine the {from.GetType()} {name} at {src}");
        }

        public InvalidDefinition(Source src)
        {
            Log.Error($"Invalid definition at {src}");
        }
    }

    internal class InvalidFunctionDefinition : Exception
    {
        public InvalidFunctionDefinition(
            Source src,
            Skell.Types.Function function,
            Skell.Types.Lambda definition
        ) {
            StringBuilder msg = new StringBuilder();
            msg.Append($"A definition of Function {function.name}");
            msg.Append($" with the arguments: {definition.argString} already exists!\n");
            msg.Append("The following definitions are available:\n");
            foreach (var lambda in function.definitions) {
                msg.Append($"\t {function.name}{lambda.argString}\n");
            }

            Log.Information(msg.ToString());
        }

        public InvalidFunctionDefinition(
            Skell.Types.Function function,
            Skell.Types.BuiltinLambda definition
        ) {
            StringBuilder msg = new StringBuilder();
            msg.Append($"A definition of Function {function.name}");
            msg.Append($" with the arguments: {definition.argString} already exists!\n");
            msg.Append("The following definitions are available:\n");
            foreach (var lambda in function.definitions) {
                msg.Append($"\t {function.name}{lambda.argString}\n");
            }

            Log.Information(msg.ToString());
        }
    }

    internal class InvalidFunctionCall : Exception
    {
        public InvalidFunctionCall(
            Source src,
            Skell.Types.Function function,
            List<Tuple<int, Skell.Types.ISkellType>> arguments
        ) {
            StringBuilder msg = new StringBuilder();
            msg.Append($"At {src},\n");
            msg.Append($"Could not match a definition of Function {function.name} with the following arguments:\n");
            foreach (var arg in arguments) {
                msg.Append($"\t {arg.Item1}. {arg.Item2} as {arg.Item2.GetType().Name}\n");
            }
            if (function.definitions.Count == 0) {
                msg.Append("No definitions exist");
            } else {
                msg.Append("The following definitions are available:\n");
            }
            foreach (var lambda in function.definitions) {
                msg.Append($"\t {function.name}{lambda.argString}\n");
            }

            Log.Information(msg.ToString());
        }

        public InvalidFunctionCall(
            Skell.Types.Function function,
            List<Tuple<int, Skell.Types.ISkellType>> arguments
        ) {
            StringBuilder msg = new StringBuilder();
            msg.Append($"Could not match a definition of Function {function.name} with the following arguments:\n");
            foreach (var arg in arguments) {
                msg.Append($"\t {arg.Item1}. {arg.Item2} as {arg.Item2.GetType().Name}\n");
            }
            if (function.definitions.Count == 0) {
                msg.Append("No definitions exist");
            } else {
                msg.Append("The following definitions are available:\n");
            }
            foreach (var lambda in function.definitions) {
                msg.Append($"\t {function.name}{lambda.argString}\n");
            }

            Log.Information(msg.ToString());
        }
    }

    internal class InvalidNamespace : Exception
    {
        public InvalidNamespace(
            Source src,
            string ns,
            Skell.Types.Array namespaces
        )
        {
            Log.Error($"Namespace \"{ns}\" not found. Valid namespaces are {namespaces}");
        }
    }

    internal class InvalidNamespacedIdentifier : Exception
    {
        public InvalidNamespacedIdentifier(
            Source src,
            Skell.Types.Array names
        )
        {
            Log.Error($"Namespaced identifier at {src} is invalid, expected one of {names}");
        }
    }
}