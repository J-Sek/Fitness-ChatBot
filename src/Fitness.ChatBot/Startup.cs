using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fitness.ChatBot
{
    public class Startup
    {
        private ILoggerFactory _loggerFactory;
        private readonly bool _isProduction;

        public Startup(IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;

            var botConfig = BotConfiguration.Load(botFilePath, secretKey);

            services.AddSingleton(sp => botConfig);
            services.AddSingleton(sp => new BotServices(botConfig));

            var environment = _isProduction ? "production" : "development";
            var endpointService = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment) as EndpointService;

            var dataStore = new MemoryStorage(); // TODO: Set RavenDB

            services.AddSingleton(new ConversationState(dataStore));
            services.AddSingleton(new UserState(dataStore));

            services.AddBot<FitnessBot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                ILogger logger = _loggerFactory.CreateLogger<FitnessBot>();

                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}