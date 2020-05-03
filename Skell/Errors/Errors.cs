using Serilog;
using System.Collections.Generic;
using System.Text;

namespace Skell.Error
{
    internal class NativeParseFailure : System.Exception
    {
        public NativeParseFailure(string token)
        {
            Log.Error($"Failed to parse \"{token}\" to native type");
        }

        public NativeParseFailure() : base() {}
        public NativeParseFailure(string message, System.Exception innerException) : base(message, innerException) {}
    }

    internal class UnexpectedType : System.Exception
    {
        public UnexpectedType(Skell.Types.ISkellType got, System.Type expected)
        {
            Log.Error($"Expected {expected}, got {got} of type {got.GetType()}");
        }

        public UnexpectedType() : base() {}
        public UnexpectedType(string message) : base(message) {}
        public UnexpectedType(string message, System.Exception innerException) : base(message, innerException) {}
    }

    internal class IndexOutOfRange : System.Exception
    {
        public IndexOutOfRange(Skell.Types.ISkellType index, Skell.Types.Array indices)
        {
            Log.Error($"Expected an index from {indices}, got {index}");
        }

        public IndexOutOfRange() : base() {}
        public IndexOutOfRange(string message) : base(message) {}
        public IndexOutOfRange(string message, System.Exception innerException) : base(message, innerException) {}
    }

    internal class InvalidOperation : System.Exception
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

        public InvalidOperation() : base() {}
        public InvalidOperation(string message) : base(message) {}
        public InvalidOperation(string message, System.Exception innerException) : base(message, innerException) {}
    }

    internal class InvalidLambdaCall : System.Exception
    {
        public InvalidLambdaCall(
            Skell.Types.Lambda lambda,
            Dictionary<string, Skell.Types.ISkellType> arguments
        ) {
            StringBuilder msg = new StringBuilder();
            msg.Append($"Lambda {lambda.name} was called with the following arguments:\n");
            foreach (KeyValuePair<string, Skell.Types.ISkellType> pair in arguments) {
                msg.Append($"\t {pair.Value} as {pair.Key}\n");
            }
            if (lambda.argsList.Count == 0) {
                msg.Append("Expected no arguments");
            } else {
                msg.Append("Expected list of arguments:\n");
            }
            foreach (KeyValuePair<string, Antlr4.Runtime.IToken> pair in lambda.argsList) {
                msg.Append($"\t {pair.Key} of {pair.Value.Text}\n");
            }

            Log.Information(msg.ToString());
        }

        public InvalidLambdaCall() : base() {}
        public InvalidLambdaCall(string message) : base(message) {}
        public InvalidLambdaCall(string message, System.Exception innerException) : base(message, innerException) {}
    }
}