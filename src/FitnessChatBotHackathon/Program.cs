using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

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
//                    Rv.WithRaven(session =>
//                    {
//                        session.Store(new RavenDBSelfTest());
//                        session.SaveChanges();
//                    });
                })
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }

    internal class RavenDBSelfTest
    {
        public string Id => $"{Created:O}";
        public DateTime Created { get; set; } = DateTime.Now;
    }
}