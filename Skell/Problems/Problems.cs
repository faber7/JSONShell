using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Skell.Problems
{
    abstract class Exception : System.Exception
    {
    }

    internal class UnexpectedType : Exception
    {
        public UnexpectedType(Skell.Types.ISkellInternal got, System.Type expected)
        {
            Log.Error($"Expected {expected}, got {got} of type {got.GetType()}");
        }
    }

    internal class IndexOutOfRange : Exception
    {
        public IndexOutOfRange(Skell.Types.ISkellType index, Skell.Types.Array indices)
        {
            Log.Error($"Expected an index from {indices}, got {index}");
        }
    }

    internal class InvalidOperation : Exception
    {
        public InvalidOperation(string operation, string operandType)
        {
            Log.Error($"Operation {operation} cannot be performed on operands of type {operandType}");
        }

        public InvalidOperation(string operation, object operand)
        {
            Log.Error($"Operation {operation} cannot be performed on {operand}");
        }

        public InvalidOperation(string operation, object operand1, object operand2)
        {
            Log.Error($"Cannot perform operation {operand1} {operation} {operand2}");
        }
    }

    internal class UnaccessibleLHS : Exception
    {
        public UnaccessibleLHS(Skell.Generated.SkellParser.PrimaryContext context)
        {
            Log.Error($"Attempted to declare an unaccessible expression at line {context.Start.Line}");
        }
    }

    internal class InvalidDefinition : Exception
    {
        public InvalidDefinition(
            string name, 
            Skell.Types.ISkellInternal existing
        ) {
            string from, to;
            if (existing.GetType() == typeof(Skell.Types.Function)) {
                from = "function";
                to = "value";
            } else {
                from = "value";
                to = "function";
            }
            Log.Error($"Attempted to redefine the {from} {name} as a {to}");
        }

        public InvalidDefinition(Skell.Generated.SkellParser.DeclarationContext context)
        {
            Log.Error($"Attempted to define a function as a value at line {context.Start.Line}");
        }
    }

    internal class InvalidFunctionDefinition : Exception
    {
        public InvalidFunctionDefinition(
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
    }

    internal class InvalidFunctionCall : Exception
    {
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
}