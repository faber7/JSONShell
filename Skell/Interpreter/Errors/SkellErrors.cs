using Serilog;

namespace Skell.Error
{
    internal class NativeParseError : System.Exception
    {
        public NativeParseError(string token)
        {
            Log.Warning($"Failed to parse \"{token}\" to native type");
        }

        public NativeParseError() : base()
        {
            throw new System.NotImplementedException();
        }

        public NativeParseError(string message, System.Exception innerException) : base(message, innerException)
        {
            throw new System.NotImplementedException();
        }
    }
}