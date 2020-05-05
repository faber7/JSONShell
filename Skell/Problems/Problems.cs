using Serilog;
using System.Collections.Generic;
using System.Text;

namespace Skell.Problems
{
    abstract class Exception : System.Exception
    {
    }

    internal class UnexpectedType : Exception
    {
        public UnexpectedType(Skell.Types.ISkellType got, System.Type expected)
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

    internal class InvalidLambdaCall : Exception
    {
        public InvalidLambdaCall(
            Skell.Types.Function lambda,
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
    }
}