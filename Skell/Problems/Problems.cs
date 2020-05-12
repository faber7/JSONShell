using Serilog;
using Skell.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skell.Problems
{
    abstract class Exception : System.Exception
    {
        public new string Message;
    }

    internal class DivisionByZero : Exception
    {
        public DivisionByZero(Source src)
        {
            Message = $"Attempted to divide by zero at {src}";
        }
    }

    internal class UndefinedIdentifer : Exception
    {
        public UndefinedIdentifer(string name)
        {
            Message = $"Tried to access {name}";
        }

        public UndefinedIdentifer(Source src, string name)
        {
            Message = $"Identifier {name} at {src} does not exist";
        }

        public UndefinedIdentifer(Source src, string name, System.Type expected)
        {
            Message = $"Identifier {name} at {src} does not exist, expected {expected}";
        }
    }

    internal class UnexpectedType : Exception
    {
        public UnexpectedType(Source src, Skell.Types.ISkellInternal got, System.Type expected)
        {
            Message = $"At {src}, expected {expected}, got {got} of type {got.GetType()}";
        }

        public UnexpectedType(Source src, Skell.Types.ISkellInternal got, System.Type expected1, System.Type expected2)
        {
            Message = $"At {src}, expected {expected1} or {expected2}, got {got} of type {got.GetType()}";
        }
    }

    internal class IndexOutOfRange : Exception
    {
        public IndexOutOfRange(Source src, Skell.Types.ISkellType index, Skell.Types.ISkellIndexable obj)
        {
            Message = $"Expected an index from \n{new Skell.Types.Array(obj.ListIndices())}\n at {src}, got {index}";
        }
    }

    internal class InvalidOperation : Exception
    {
        public InvalidOperation(string operation, Skell.Types.ISkellReturnable operand)
        {
            Message = $"Operation {operation} cannot be performed on {operand}";
        }

        public InvalidOperation(string operation, Skell.Types.ISkellReturnable operand1, Skell.Types.ISkellReturnable operand2)
        {
            Message = $"Cannot perform operation {operand1} {operation} {operand2}";
        }
    }
    
    internal class InvalidDefinition : Exception
    {
        public InvalidDefinition(
            Source src,
            string name
        )
        {
            Message = $"Attempted to redefine {name} at {src}";
        }

        public InvalidDefinition(
            Source src,
            string name,
            Skell.Interpreter.State state    
        )
        {
            var from = state.Names.DefinitionOf(name);
            Message = $"Attempted to redefine the {from.GetType()} {name} at {src}";
        }

        public InvalidDefinition(Source src)
        {
            Message = "Invalid definition at {src}";
        }
    }

    internal class InvalidFunctionDefinition : Exception
    {
        public InvalidFunctionDefinition(
            Source src,
            Skell.Types.Function function,
            Skell.Types.Lambda definition
        )
        {
            StringBuilder msg = new StringBuilder();
            msg.Append($"An existing definition of Function {function.name}");
            msg.Append($" conflicts with the definition {definition} at {src}\n");
            msg.Append("The following definitions are available:\n");
            foreach (var lambda in function.definitions) {
                msg.Append($"\t {function.name}{lambda}\n");
            }

            Message = msg.ToString();
        }

        public InvalidFunctionDefinition(
            Skell.Types.Function function,
            Skell.Types.BuiltinLambda definition
        )
        {
            StringBuilder msg = new StringBuilder();
            msg.Append($"A definition of Function {function.name}");
            msg.Append($" with the arguments: {definition} already exists!\n");
            msg.Append("The following definitions are available:\n");
            foreach (var lambda in function.definitions) {
                msg.Append($"\t {function.name}{lambda}\n");
            }

            Message = msg.ToString();
        }
    }

    internal class InvalidFunctionCall : Exception
    {
        public InvalidFunctionCall(
            Source src,
            Skell.Types.Function function,
            List<Tuple<int, Skell.Types.ISkellType>> arguments
        )
        {
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
                msg.Append($"\t {function.name}{lambda}\n");
            }

            Message = msg.ToString();
        }

        public InvalidFunctionCall(
            Skell.Types.Function function,
            List<Tuple<int, Skell.Types.ISkellType>> arguments
        )
        {
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
                msg.Append($"\t {function.name}{lambda}\n");
            }

            Message = msg.ToString();
        }
    }

    internal class InvalidReturn : Exception
    {
        public InvalidReturn(
            Source src
        )
        {
            Message = $"A return is not permitted at {src}. Only functions may return a value";
        }
    }

    internal class InvalidNamespacedIdentifier : Exception
    {
        public InvalidNamespacedIdentifier(
            Source src,
            Skell.Types.Array names
        )
        {
            Message = $"Namespaced identifier at {src} is invalid, expected one of \n{names}\n";
        }
    }
}