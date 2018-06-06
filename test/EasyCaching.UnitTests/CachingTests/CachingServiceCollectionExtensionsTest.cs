namespace EasyCaching.UnitTests
{    
    using EasyCaching.InMemory;
    using EasyCaching.SQLite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System.IO;
    using Xunit;

    public class CachingServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddCacheExtensions_Should_Get_Configuration_Succeed()
        {

            var appsettings = @"
{
    'easycaching': {
        'inmemory': {
            'CachingProviderType': 1,
            'MaxRdSecond': 600,
            'Order': 99,
        },
         'sqlite': {
            'CachingProviderType': 3,
            'MaxRdSecond': 120,
            'Order': 2,
            'dbconfig': {
                'FileName': 'my.db'
            }
        }
    }
}";
            var path = TestHelpers.CreateTempFile(appsettings);
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);
            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddDefaultInMemoryCache(config);
            services.AddSQLiteCache(config);

            var serviceProvider = services.BuildServiceProvider();
            var memoryOptions = serviceProvider.GetService<IOptionsMonitor<InMemoryOptions>>();
            Assert.NotNull(memoryOptions);
            Assert.Equal(600, memoryOptions.CurrentValue.MaxRdSecond);

            var sqliteOptions = serviceProvider.GetService<IOptionsMonitor<SQLiteOptions>>();
            Assert.NotNull(sqliteOptions);
            Assert.Equal(120, sqliteOptions.CurrentValue.MaxRdSecond);
            Assert.Equal("my.db", sqliteOptions.CurrentValue.DBConfig.FileName);
        }
    }
}

