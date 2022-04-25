using System;
using System.Collections.Generic;
using System.Linq;

namespace Collector
{
    public  static class ListExtensions
    {
        public static List<List<T>> ChunkByWithLink<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index % chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> collection, int count)
        {
            T[] bucket = null;
            int bucketCount = 0;
            foreach (var item in collection)
            {
                if (bucket is null)
                    bucket = new T[count];

                bucket[bucketCount++] = item;
                
                // the bucket is fully populated before it's yielded
                if (bucketCount != count)
                    continue;

                yield return bucket;
                bucket = null;
                bucketCount = 0;
            }
            
            // return the last bucket with all remaining elements
            if (bucket != null && bucketCount > 0)
            {
                Array.Resize(ref bucket, bucketCount);
                yield return bucket;
            }
        }
    }
}