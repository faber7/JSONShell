using Serilog;
using System.Collections.Generic;

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
}