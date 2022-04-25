using System;
using System.IO;
using Collector.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace Collector
{
    public static class Startup
    {
        public static IHost AppStartup(string[] args)
        {
            IConfiguration configuration = BuildHost(new ConfigurationBuilder());
            var configDataSection = configuration.GetSection("ConfigData");

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            
            // initialize the host
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Adding the DI container for configuration
                    services.Configure<ConfigData>(configDataSection);
                    services.AddScoped(cfg => cfg.GetService<IOptions<ConfigData>>().Value);

                    services.AddScoped<ICollector, Collector>();
                    services.AddScoped<IInstantClient, InstantClient>();
                    services.AddScoped<IArchiver, Archiver>();
                })
                .UseSerilog(Log.Logger)
                .Build();

            return host;
        }

        private static IConfiguration BuildHost(ConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}