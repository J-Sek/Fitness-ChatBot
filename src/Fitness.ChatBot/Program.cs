using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Fitness.ChatBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddAzureWebAppDiagnostics();

#if DEBUG
                    logging.AddConsole();
#endif
                })
                // .UseApplicationInsights()
                .UseStartup<Startup>()
                .Build();
    }
}