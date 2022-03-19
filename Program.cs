using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Collector.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Collector
{
    class Program
    {
        private static readonly List<string> _errors = new List<string>();
        private static ICollector _collector;
        private static IInstantClient _instantClient;
        private static IArchiver _archiver;
        private static ConfigData _config;
        
        static async Task Main(string[] args)
        {
            var host = Startup.AppStartup(args);

            _collector = host.Services.GetService<ICollector>();
            _instantClient = host.Services.GetService<IInstantClient>();
            _archiver = host.Services.GetService<IArchiver>();
            _config = host.Services.GetService<ConfigData>();

            try
            {
                await StartAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Environment.Exit(-1);
            }
        }

        private static async Task StartAsync()
        {
            var initialiseResult = _collector.Initialise();

            if (initialiseResult.IsFailure)
            {
                _errors.Add(initialiseResult.Error);
            }
            else
            {
                foreach (var filePath in _collector.FilePaths)
                {
                    var fileInfo = _collector.Collect(filePath);
                    var sendingResult = await _instantClient.SendAsync(fileInfo);

                    if (sendingResult.IsFailure)
                    {
                        _errors.Add(sendingResult.Error);
                        continue;
                    }

                    var archiveResult = _archiver.Archive(fileInfo);
                    
                    if(archiveResult.IsFailure)
                        _errors.Add(archiveResult.Error);
                }
            }
            
            if (_errors.Any())
            {
                var errorMessage = string.Join("\r\n", _errors);
                MailHelper.SendEmailForGlobalException(errorMessage, _config.EmailFromAddress,
                    _config.NotifyEmailAddress);
                Environment.Exit(-1);
            }
        }
    }
}