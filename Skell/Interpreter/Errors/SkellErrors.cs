using Serilog;

namespace Skell.Error
{
    class NativeParseError : System.Exception
    {
        public NativeParseError(string token)
        {
            Log.Warning($"Failed to parse \"{token}\" to native type");
        }
    }
}