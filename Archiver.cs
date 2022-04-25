using System;
using System.IO;
using System.Text;
using Collector.Models;
using Collector.Models.FileInformation;
using Serilog;

namespace Collector
{
    public interface IArchiver
    {
        Result Archive(PathChunk chunk);
    }
    
    public class Archiver: IArchiver
    {
        private readonly string _archiveFolder;

        public Archiver(ConfigData config)
        {
            _archiveFolder = config.InstantPayCardArchiveFolder;
        }
        
        public Result Archive(PathChunk chunk)
        {
            StringBuilder sb = new StringBuilder();
            
            chunk.FilePaths.ForEach(filePath =>
            {
                var fileInfo = new FileInfo(filePath);
                var archivePathWithLocId = Path.Combine(_archiveFolder, fileInfo.Directory.Name);

                if (!Directory.Exists(archivePathWithLocId))
                    Directory.CreateDirectory(archivePathWithLocId);

                try
                {
                    var archiveFullPath = Path.Combine(archivePathWithLocId, fileInfo.Name);
                
                    File.Copy(filePath, archiveFullPath, true);
                    File.Delete(filePath);
                    
                    Log.Information($"File '{filePath}' was moved to archive folder.");
                }
                catch (Exception ex)
                {
                    var errorByDataMoving = $"Error by moving file '{filePath}' to archive folder.";
                    Log.Error(ex, errorByDataMoving);
                    sb.AppendLine(errorByDataMoving);
                }
            });
            
            if (sb.Length > 0)
                return Result.Fail(sb.ToString());
            
            return Result.Ok();
        }
    }
}