using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Collector.Models;
using Collector.Models.FileInformation;
using Collector.Models.Instant;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Collector
{
    public interface ICollector
    {
        List<FileInfo> FileDataPaths { get; }
        public List<PathChunk> Chunks { get; }
        List<InstantPyCardExportData> Collect(PathChunk chunk);
        string ConvertInstantDataToString(IEnumerable<InstantPyCardExportData> data);
        Result Initialise();
    }
    
    public class Collector: ICollector
    {
        private readonly string _folderPath;
        private readonly long _fileSizeLimit;
        public List<FileInfo> FileDataPaths { get; private set; } = new List<FileInfo>();
        public List<PathChunk> Chunks { get; private set; } = new List<PathChunk>();

        // constructor
        public Collector(ConfigData config)
        {
            _folderPath = config.InstantPayCardDataFolder;
            _fileSizeLimit = config.FileSizeLimitInBytes;
        }

        public Result Initialise()
        {
            string[] folderPaths = Directory.GetDirectories(_folderPath);

            if (folderPaths.Length == 0)
            {
                string errorMessage = $"Process Error: There aren't any inner folders in base folder \"{_folderPath}\"";
                Log.Error(errorMessage);
                return Result.Ok(errorMessage);
            }

            FileDataPaths = folderPaths
                .SelectMany(folderPath => Directory.GetFiles(folderPath)
                                                        .Where(p => p.EndsWith(".json")),
                                                (folderPath, fullPath) => new FileInfo(fullPath))
                .OrderBy(x => x.Length)
                .ToList();
            
            // upload chunks
            GenerateChunks(FileDataPaths);

            if (FileDataPaths.Count == 0)
            {
                const string noFilesForSending = "We don't have any .json files for sending.";
                Log.Information(noFilesForSending);
                return Result.Ok(noFilesForSending);
            }
            
            return Result.Ok();
        }
        
        /// <summary>
        /// Generate list of entities from list of file paths
        /// </summary>
        /// <param name="chunk">The chunk with list of file paths.</param>
        /// <returns>The list of entities.</returns>
        public List<InstantPyCardExportData> Collect(PathChunk chunk)
        {
            return chunk.FilePaths.SelectMany(x => Map(x)).ToList();
        }
        
        /// <summary>
        /// Generate serialized string from list of entities
        /// </summary>
        /// <param name="data">List of entities.</param>
        /// <returns>The serialized string from list of entities.</returns>
        public string ConvertInstantDataToString(IEnumerable<InstantPyCardExportData> data)
        {
            var camelSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            return JsonConvert.SerializeObject(data, camelSettings);
        }

        /// <summary>
        /// Take data from file paths and generate List of entities
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>The list of entities.</returns>
        private static List<InstantPyCardExportData> Map(string filePath)
        {
            var fileContent = File.ReadAllText(filePath, Encoding.UTF8);
            var collectedData = JsonConvert.DeserializeObject<List<EmployeeData>>(fileContent);
            
            return collectedData.Select(Mapper.MapImportedDataToExport).ToList();
        }

        private void GenerateChunks(List<FileInfo> fileDataPaths)
        {
            List<string> resultArray = new List<string>();
            long sumOfCollectedDataArraay = 0;

            for (int i = 0; i < fileDataPaths.Count; i++)
            {
                // if fileSize is bigger than 10 Mb
                if (fileDataPaths[i].Length >= _fileSizeLimit && resultArray.Count <= 1)
                {
                    // collect all filePaths which are bigger than 10 Mb in one list to split it later to chinks = filePaths+1 
                    if (Chunks.Any(x => x.ChunkIsBiggerThanFileSizeLimit))
                    {
                        // update existed chunk
                        var chunk = Chunks.FirstOrDefault(x => x.ChunkIsBiggerThanFileSizeLimit);
                        chunk.ChunkSize += fileDataPaths[i].Length;
                        chunk.FilePaths.Add(fileDataPaths[i].FullName);
                    }
                    else
                    {
                        // create new chunk
                        Chunks.Add(new PathChunk(new List<string>() {fileDataPaths[i].FullName }, fileDataPaths[i].Length, true));
                    }
                    continue;
                }
                
                // iterate through each value and find values which are less than 10 Mb
                if ((fileDataPaths[i].Length + sumOfCollectedDataArraay) <= _fileSizeLimit)
                {
                    sumOfCollectedDataArraay += fileDataPaths[i].Length;
                    resultArray.Add(fileDataPaths[i].FullName);
                    continue;
                }
                
                // if we don't have more indexes and finish iteration
                Chunks.Add(new PathChunk(resultArray, sumOfCollectedDataArraay, false));
                resultArray = new List<string>() {fileDataPaths[i].FullName};
                sumOfCollectedDataArraay = fileDataPaths[i].Length;
            }
            
            // when we finish with iteration , need to add last values in array to chunk
            if (resultArray.Count > 0)
                Chunks.Add(new PathChunk(resultArray, sumOfCollectedDataArraay, false));
        }
    }
}
