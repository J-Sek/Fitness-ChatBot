using System;
using System.Linq;
using Fitness.ChatBot.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Fitness.ChatBot
{
    public class Startup
    {
        private readonly bool _isProduction;

        public Startup(IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
                builder.AddUserSecrets<Startup>();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.ConfigureLogging();

            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;

            var botConfig = BotConfiguration.Load(botFilePath, secretKey);

            services.AddSingleton(sp => botConfig);
            services.AddSingleton(sp => new BotServices(botConfig));

            var environment = _isProduction ? "production" : "development";
            var endpointService = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment) as EndpointService;

            var dataStore = new RavenDbBotStorage();

            services.AddSingleton(new ConversationState(dataStore));
            services.AddSingleton(new UserState(dataStore));

            services.AddBot<FitnessBot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
                options.OnTurnError = async (context, exception) =>
                {
                    Log.Error(exception, "OnTurnError");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };
            });

            return Registry.Configure(services, Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}