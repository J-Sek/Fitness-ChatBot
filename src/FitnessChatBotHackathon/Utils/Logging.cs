using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fitness.ChatBot.Utils
{
    public static class Logging
    {
        public static void ConfigureLogging(this IServiceCollection _)
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Information()
//                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
//                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                .WriteTo.Console()
# if DEBUG
                .WriteTo.Seq("http://localhost:5341")
# endif
                .CreateLogger();
        }
    }
}