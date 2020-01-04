using Serilog;

namespace SharpSync.Common
{
    public static class Setup
    {
        public static void Logger(bool verbose)
        {
            LoggerConfiguration lcfg = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "\r[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                ;

            if (verbose)
                lcfg.MinimumLevel.Verbose();
            else
                lcfg.MinimumLevel.Information();

            Log.Logger = lcfg.CreateLogger();
        }
    }
}
