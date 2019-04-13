using System;
using Fitness.ChatBot.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StructureMap;

namespace Fitness.ChatBot
{
    public class Registry
    {
        public static IServiceProvider Configure(IServiceCollection services, IConfiguration settings)
        {
            var container = new Container();

            container.Configure(config =>
            {
                config.Scan(_ =>
                {
                    _.AssemblyContainingType(typeof(Startup));
                    _.WithDefaultConventions();
                    _.AddAllTypesOf<IBotCommand>();
                });

                config.For<ILogger>().Use(_ => Log.ForContext("service", "DataService", false));

                config.Populate(services);
            });

            return container.GetInstance<IServiceProvider>();
        }
    }
}