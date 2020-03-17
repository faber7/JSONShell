using Serilog;

namespace Skell
{
    public class SkellLogger
    {
        public static void InitializeConsoleLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}"
                )
                .CreateLogger();
        }

        public static void TerminateConsoleLogger()
        {
            Log.CloseAndFlush();
        }

        public static void Inform(string location, string information)
        {
            Log.Information($"{location}: {information}");
        }

        public static void Verbose(string location, string information)
        {
            Log.Verbose($"{location}: {information}");
        }
    }
}