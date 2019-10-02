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
        /// Uses the disk caching provider.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions UseDisk(this EasyCachingOptions options, Action<DiskOptions> configure, string name = EasyCachingConstValue.DefaultDiskName)
        {
            ArgumentCheck.NotNull(configure, nameof(configure));

            options.RegisterExtension(new DiskOptionsExtension(name, configure));

            return options;
        }

        public static EasyCachingOptions UseDisk(this EasyCachingOptions options, IConfiguration configuration, string name = EasyCachingConstValue.DefaultDiskName, string sectionName = EasyCachingConstValue.DiskSection)
        {
            var dbConfig = configuration.GetSection(sectionName);
            var diskOptions = new DiskOptions();
            dbConfig.Bind(diskOptions);

            void configure(DiskOptions x)
            {
                x.EnableLogging = diskOptions.EnableLogging;
                x.MaxRdSecond = diskOptions.MaxRdSecond;
                x.DBConfig = diskOptions.DBConfig;
            }
            return options.UseDisk(configure, name);
        }
    }
}
