using System;
using System.IO;
using Collector.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Collector
{
    public static class Startup
    {
        public static IHost AppStartup(string[] args)
        {
            IConfiguration configuration = BuildHost(new ConfigurationBuilder());
            var configDataSection = configuration.GetSection("ConfigData");

            var logFilePath = configDataSection.GetValue<string>("LogFilePath");
            var logFileName = configDataSection.GetValue<string>("LogFileName");
            
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
                .UseSerilog(new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.File(
                        path: Path.Combine(logFilePath, $"{logFileName}.{DateTime.Now:yyyyMMdd_HHmm}.txt"),
                        fileSizeLimitBytes:1_000_000,
                        rollOnFileSizeLimit: true,
                        shared: true,
                        flushToDiskInterval: TimeSpan.FromDays(1)
                    )
                    .WriteTo
                    .Console(
                        outputTemplate: "[{TimeStamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{Exception}{NewLine}",
                        theme: AnsiConsoleTheme.Literate
                    )
                    .CreateLogger())
                .Build();

            return host;
        }

        private static IConfiguration BuildHost(ConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}