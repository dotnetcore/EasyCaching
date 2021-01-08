namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Disk;
    using Microsoft.Extensions.Configuration;

    public static class EasyCachingOptionsExtensions 
    {
        /// <summary>
        /// Uses the disk caching provider (specify the config via hard code).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure provider settings.</param>
        /// <param name="name">The name of this provider instance.</param>
        public static EasyCachingOptions UseDisk(
            this EasyCachingOptions options
            , Action<DiskOptions> configure
            , string name = EasyCachingConstValue.DefaultDiskName
            )
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new DiskOptionsExtension(name, configure));

            return options;
        }

        /// <summary>
        /// Uses the disk caching provider (read config from configuration file).
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The name of this provider instance.</param>
        /// <param name="sectionName">The section name in the configuration file.</param>        
        public static EasyCachingOptions UseDisk(
            this EasyCachingOptions options
            , IConfiguration configuration
            , string name = EasyCachingConstValue.DefaultDiskName
            , string sectionName = EasyCachingConstValue.DiskSection
            )
        {
            var dbConfig = configuration.GetSection(sectionName);
            var diskOptions = new DiskOptions();
            dbConfig.Bind(diskOptions);

            void configure(DiskOptions x)
            {
                x.EnableLogging = diskOptions.EnableLogging;
                x.MaxRdSecond = diskOptions.MaxRdSecond;
                x.LockMs = diskOptions.LockMs;
                x.SleepMs = diskOptions.SleepMs;
                x.SerializerName = diskOptions.SerializerName;
                x.CacheNulls = diskOptions.CacheNulls;
                x.DBConfig = diskOptions.DBConfig;
            }
            return options.UseDisk(configure, name);
        }
    }
}
