using Serilog;

namespace Skell
{
    public static class SkellLogger
    {
        private const string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}";

        public static void InitializeConsoleLogger(bool Debug, bool Verbose)
        {
            var config = new LoggerConfiguration();
            if (!Debug && !Verbose) {
                config = config.MinimumLevel.Information();
            } else if (Debug) {
                config = config.MinimumLevel.Debug();
            } else if (Verbose) {
                config = config.MinimumLevel.Verbose();
            }

            Log.Logger = config
                .WriteTo.Console(
                    outputTemplate: outputTemplate
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