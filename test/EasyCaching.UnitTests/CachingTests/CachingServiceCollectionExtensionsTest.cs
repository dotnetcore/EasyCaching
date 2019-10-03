namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
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
            var appsettings = " { \"easycaching\": { \"inmemory\": { \"CachingProviderType\": 1, \"MaxRdSecond\": 600, \"Order\": 99, \"DBConfig\":{ \"ExpirationScanFrequency\": 120, \"SizeLimit\" : 100 } }, \"sqlite\": { \"CachingProviderType\": 3, \"MaxRdSecond\": 120, \"Order\": 2, \"dbconfig\": { \"FileName\": \"my.db\" } } } }";
            var path = TestHelpers.CreateTempFile(appsettings);
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);
            string MemoryName = "memory1";
            string SQLiteName = "sqlite1";

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);
            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddEasyCaching(x => x.UseInMemory(config, MemoryName).UseSQLite(config, SQLiteName));

            var serviceProvider = services.BuildServiceProvider();
            var memoryOptions = serviceProvider.GetService<IOptionsMonitor<InMemoryOptions>>().Get(MemoryName);
            Assert.NotNull(memoryOptions);
            Assert.Equal(600, memoryOptions.MaxRdSecond);
            Assert.Equal(120, memoryOptions.DBConfig.ExpirationScanFrequency);
            Assert.Equal(100, memoryOptions.DBConfig.SizeLimit);

            var sqliteOptions = serviceProvider.GetService<IOptionsMonitor<SQLiteOptions>>().Get(SQLiteName);
            Assert.NotNull(sqliteOptions);
            Assert.Equal(120, sqliteOptions.MaxRdSecond);
            Assert.Equal("my.db", sqliteOptions.DBConfig.FileName);
        }
    }
}