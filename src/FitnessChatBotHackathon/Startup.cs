using System;
using System.Collections.Concurrent;
using System.Linq;
using Fitness.ChatBot.Infrastructure;
using Fitness.ChatBot.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;

            var botConfig = BotConfiguration.Load(botFilePath, secretKey);

            services.AddSingleton(sp => botConfig);
            services.AddSingleton(sp => new BotServices(botConfig));

            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            var environment = _isProduction ? "production" : "development";
            var endpointService = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment) as EndpointService;

            var dataStore = new RavenDbBotStorage();
            services.AddSingleton<RavenDbBotStorage>(dataStore);

            services.AddSingleton(new ConversationState(dataStore));
            services.AddSingleton(new UserState(dataStore));

            var optionsCredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddSingleton<ICredentialProvider>(optionsCredentialProvider);

            services.AddBot<FitnessBot>(options =>
            {
                options.CredentialProvider = optionsCredentialProvider;
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

            app.UseMvc();
        }
    }
}