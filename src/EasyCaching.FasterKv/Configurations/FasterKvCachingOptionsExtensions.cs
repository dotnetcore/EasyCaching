using System;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using EasyCaching.FasterKv.Configurations;
using Microsoft.Extensions.Configuration;
// ReSharper disable CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class FasterKvCachingOptionsExtensions
{
            /// <summary>
        /// Uses the FasterKv provider (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure provider settings.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseFasterKv(
            this EasyCachingOptions options,
            Action<FasterKvCachingOptions> configure,
            string name = EasyCachingConstValue.DefaultFasterKvName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new FasterKvOptionsExtension(name, configure));
            return options;
        }

        /// <summary>
        /// Uses the FasterKv provider (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>
        public static EasyCachingOptions UseFasterKv(
            this EasyCachingOptions options, 
            IConfiguration configuration, 
            string name = EasyCachingConstValue.DefaultFasterKvName, 
            string sectionName = EasyCachingConstValue.FasterKvSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var fasterKvOptions = new FasterKvCachingOptions();
            dbConfig.Bind(fasterKvOptions);

            void Configure(FasterKvCachingOptions x)
            {
                x.EnableLogging = fasterKvOptions.EnableLogging;
                x.MaxRdSecond = fasterKvOptions.MaxRdSecond;             
                x.LockMs = fasterKvOptions.LockMs;
                x.SleepMs = fasterKvOptions.SleepMs;
                x.SerializerName = fasterKvOptions.SerializerName;
                x.CacheNulls = fasterKvOptions.CacheNulls;
                x.IndexCount = fasterKvOptions.IndexCount;
                x.MemorySizeBit = fasterKvOptions.MemorySizeBit;
                x.PageSizeBit = fasterKvOptions.PageSizeBit;
                x.ReadCacheMemorySizeBit = fasterKvOptions.ReadCacheMemorySizeBit;
                x.ReadCachePageSizeBit = fasterKvOptions.ReadCachePageSizeBit;
                x.LogPath = fasterKvOptions.LogPath;
            }

            options.RegisterExtension(new FasterKvOptionsExtension(name, Configure));
            return options;
        }
}