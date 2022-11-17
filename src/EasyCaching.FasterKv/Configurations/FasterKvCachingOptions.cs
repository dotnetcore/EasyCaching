using System;
using System.IO;
using EasyCaching.Core.Configurations;
using FASTER.core;

namespace EasyCaching.FasterKv.Configurations
{
    /// <summary>
    /// FasterKvCachingOptions
    /// for details, see https://microsoft.github.io/FASTER/docs/fasterkv-basics/#fasterkvsettings
    /// </summary>
    public class FasterKvCachingOptions : BaseProviderOptions
    {
        /// <summary>
        /// FasterKv index count, Must be power of 2
        /// </summary>
        /// <para>For example: 1024(2^10) 2048(2^11) 65536(2^16) 131072(2^17)</para>
        /// <para>Each index is 64 bits. So this define 131072 keys. Used 1024Kb memory</para>
        public long IndexCount { get; set; } = 131072;

        /// <summary>
        /// FasterKv used memory size (default: 16MB)
        /// </summary>
        public int MemorySizeBit { get; set; } = 24;

        /// <summary>
        /// FasterKv page size (default: 1MB) 
        /// </summary>
        public int PageSizeBit { get; set; } = 20;

        /// <summary>
        /// FasterKv read cache used memory size (default: 16MB)
        /// </summary>
        public int ReadCacheMemorySizeBit { get; set; } = 24;

        /// <summary>
        /// FasterKv read cache page size (default: 16MB)
        /// </summary>
        public int ReadCachePageSizeBit { get; set; } = 20;
        
        /// <summary>
        /// FasterKv commit logs path
        /// </summary>
        public string LogPath { get; set; } =
#if (NET6_0 || NET7_0)
            Path.Combine(Environment.CurrentDirectory, $"EasyCaching-FasterKv-{Environment.ProcessId}");
#else
            Path.Combine(Environment.CurrentDirectory, $"EasyCaching-FasterKv-{System.Diagnostics.Process.GetCurrentProcess().Id}");
#endif
            
        
        /// <summary>
        /// Set Custom Store
        /// </summary>
        public FasterKV<SpanByte, SpanByte>? CustomStore { get; set; }

        internal LogSettings GetLogSettings(string name)
        {
            return new LogSettings
            {
                LogDevice = Devices.CreateLogDevice(Path.Combine(LogPath, name),
                    preallocateFile: true,
                    deleteOnClose: true),
                PageSizeBits = PageSizeBit,
                MemorySizeBits = MemorySizeBit,
                ReadCacheSettings = new ReadCacheSettings
                {
                    MemorySizeBits = ReadCacheMemorySizeBit,
                    PageSizeBits = ReadCachePageSizeBit,
                }
            };
        }
    }
}