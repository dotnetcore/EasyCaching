namespace EasyCaching.InMemory
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;
    using System;

    /// <summary>
    /// EasyCaching options extensions of InMemory.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        /// <summary>
        /// Uses the in memory.
        /// </summary>
        /// <returns>The in memory.</returns>
        /// <param name="options">Options.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions UseInMemory(this EasyCachingOptions options, string name = EasyCachingConstValue.DefaultInMemoryName)
        {
            var option = new InMemoryOptions();

            void configure(InMemoryOptions x)
            {
                x.EnableLogging = option.EnableLogging;
                x.MaxRdSecond = option.MaxRdSecond;
                x.DBConfig = option.DBConfig;
            }

            return options.UseInMemory(configure, name);
        }

        /// <summary>
        /// Uses the in memory.
        /// </summary>
        /// <returns>The in memory.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configure">Configure.</param>
        /// <param name="name">Name.</param>
        public static EasyCachingOptions UseInMemory(this EasyCachingOptions options, Action<InMemoryOptions> configure, string name = EasyCachingConstValue.DefaultInMemoryName)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new InMemoryOptionsExtension(name, configure));

            return options;
        }

        /// <summary>
        /// Uses the in memory.
        /// </summary>
        /// <returns>The in memory.</returns>
        /// <param name="options">Options.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="name">Name.</param>
        /// <param name="sectionName">SectionName.</param>
        public static EasyCachingOptions UseInMemory(this EasyCachingOptions options, IConfiguration configuration, string name = EasyCachingConstValue.DefaultInMemoryName, string sectionName = EasyCachingConstValue.InMemorySection)
        {
            var dbConfig = configuration.GetSection(sectionName);
            var memoryOptions = new InMemoryOptions();
            dbConfig.Bind(memoryOptions);

            void configure(InMemoryOptions x)
            {
                x.EnableLogging = memoryOptions.EnableLogging;
                x.MaxRdSecond = memoryOptions.MaxRdSecond;
                x.DBConfig = memoryOptions.DBConfig;
            }
            return options.UseInMemory(configure,name);
        }
    }
}
