using System;
using System.IO;
using Collector.Models;
using Serilog;

namespace Collector
{
    public interface IArchiver
    {
        Result Archive(FileToSend customFileInfo);
    }
    
    public class Archiver: IArchiver
    {
        private readonly string _archiveFolder;

        public Archiver(ConfigData config)
        {
            _archiveFolder = config.InstantPayCardArchiveFolder;
        }
        
        public Result Archive(FileToSend customFileInfo)
        {
            var locId = customFileInfo.InstantPyCardExportData.ExternalLocationId;
            var archivePathWithLocId = Path.Combine(_archiveFolder, locId);

            if (!Directory.Exists(archivePathWithLocId))
                Directory.CreateDirectory(archivePathWithLocId);

            try
            {
                var archiveFullPath = Path.Combine(_archiveFolder, locId, customFileInfo.FileName);
                
                File.Copy(customFileInfo.FilePath, archiveFullPath, true);
                File.Delete(customFileInfo.FilePath);
            }
            catch (Exception e)
            {
                var errorByDataMoving = string.Join(" ", new string[]
                {
                    $"Error by moving the file \"{customFileInfo.FilePath}\" to archive folder",
                    $"Error message: {e.Message}"
                });
                
                Log.Error(errorByDataMoving);
                return Result.Fail(errorByDataMoving);
            }

            return Result.Ok();
        }
    }
}