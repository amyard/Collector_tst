using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Collector.Models;
using Collector.Models.Instant;
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
        private static ILogger _logger;
        
        static async Task Main(string[] args)
        {
            var host = Startup.AppStartup(args);

            _collector = host.Services.GetService<ICollector>();
            _instantClient = host.Services.GetService<IInstantClient>();
            _archiver = host.Services.GetService<IArchiver>();
            _config = host.Services.GetService<ConfigData>();
            _logger = host.Services.GetService<ILogger>();

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

            if (!initialiseResult.Success)
            {
                _errors.Add(initialiseResult.Error);
            }
            else
            {
                foreach (var chunk in _collector.Chunks)
                {
                    bool moveToArchiveFolder = true;
                    List<InstantPyCardExportData> instantPyCardExportData = _collector.Collect(chunk);
                    
                    // generate array of sub chunks if we have array with more than 10 mb data in sum
                    int chunkSize = GetAmountOfChunks(chunk.ChunkSize, instantPyCardExportData.Count);
                    IEnumerable<IEnumerable<InstantPyCardExportData>> subChunks = instantPyCardExportData.ChunkBy(chunkSize);

                    foreach (var subChunk in subChunks)
                    {
                        string jsonData = _collector.ConvertInstantDataToString(subChunk);
                        using (CancellationTokenSource ctx = new CancellationTokenSource(TimeSpan.FromSeconds(30)))

                        try
                        {
                            var sendingResult = await _instantClient.SendAsync(jsonData, ctx.Token);

                            if (!sendingResult.Success)
                            {
                                moveToArchiveFolder = false;
                                _errors.Add(sendingResult.Error);
                                continue;
                            }
                        }
                        catch (OperationCanceledException ex)
                        {
                            moveToArchiveFolder = false;
                            _errors.Add(ex.Message);
                            _logger.Error(ex, ex.Message);
                        }
                    }

                    // because we combine files in one huge file, need to move all files after sending
                    if (moveToArchiveFolder)
                    {
                        var archiveResult = _archiver.Archive(chunk);
                        
                        if(!archiveResult.Success)
                            _errors.Add(archiveResult.Error);
                    }
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

        /// <summary>
        /// Return the number on which need to split our chunks, so all files will be less then 10 MB
        /// </summary>
        /// <param name="chunkChunkSize">The size of all files in chunk</param>
        /// <param name="count">The amount of entities</param>
        /// <returns>The number on which need to split our chunls</returns>
        private static int GetAmountOfChunks(long chunkSize, int count)
        {
            return (int)(count / ((int)(chunkSize / _config.FileSizeLimitInBytes) + 1)) +1;
        }
    }
}