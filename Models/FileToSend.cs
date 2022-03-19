using System.IO;
using Collector.Models.Instant;

namespace Collector.Models
{
    public class FileToSend
    {
        public FileToSend(string filePath, InstantPyCardExportData instantPyCardExportData)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            InstantPyCardExportData = instantPyCardExportData;
        }
        public string FilePath { get; set; } 
        public string FileName { get; set; } 
        public InstantPyCardExportData InstantPyCardExportData { get; set; } 
    }
}