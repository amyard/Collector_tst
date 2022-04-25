using System.Collections.Generic;

namespace Collector.Models.FileInformation
{
    public class PathChunk
    {
        public PathChunk(List<string> filePaths, long chunkSize, bool chunkIsBiggerThanFileSizeLimit)
        {
            FilePaths = filePaths;
            ChunkIsBiggerThanFileSizeLimit = chunkIsBiggerThanFileSizeLimit;
            ChunkSize = chunkSize;
        }

        public List<string> FilePaths { get; set; }
        public bool ChunkIsBiggerThanFileSizeLimit { get; set; }
        public long ChunkSize { get; set; }
    }
}