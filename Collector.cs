using System.IO;
using System.Linq;
using Collector.Models;
using Collector.Models.Instant;
using Newtonsoft.Json;
using Serilog;

namespace Collector
{
    public interface ICollector
    {
        string[] FilePaths { get; }
        FileToSend Collect(string filePath);
        Result Initialise();
    }
    
    public class Collector: ICollector
    {
        private readonly string _folderPath;
        public string[] FilePaths { get; private set; }

        // constructor
        public Collector(ConfigData config)
        {
            _folderPath = config.InstantPayCardDataFolder;
        }

        public Result Initialise()
        {    
            if (!Directory.Exists(_folderPath))
                Directory.CreateDirectory(_folderPath);

            string[] folderPaths = Directory.GetDirectories(_folderPath);

            if (folderPaths.Length == 0)
            {
                string errorMessage = $"Process Error: There aren't any inner folders in base folder \"{_folderPath}\"";
                Log.Error(errorMessage);
                return Result.Fail(errorMessage);
            }

            FilePaths = folderPaths
                .SelectMany(folderPath => Directory.GetFiles(folderPath).Where(p => p.EndsWith(".json")))
                .ToArray();
            
            return Result.Ok();
        }

        public FileToSend Collect(string filePath)
        {
            return new FileToSend(filePath, Map(filePath));
        }

        private InstantPyCardExportData Map(string filePath)
        {
            var fileContent = File.ReadAllText(filePath);
            var collectedData = JsonConvert.DeserializeObject<InstantPayCardImportData>(fileContent);
            var dataToSend = Mapper.MapImportedDataToExport(collectedData);
            return dataToSend;
        }
    }
}