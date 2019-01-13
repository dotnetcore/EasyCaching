namespace EasyCaching.UnitTests
{
    using Dapper;
    using EasyCaching.Core;
    using EasyCaching.SQLite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Xunit;

    public class SQLiteCachingTest : BaseCachingProviderTest
    {
        private readonly ISQLiteDatabaseProvider _dbProvider;

        public SQLiteCachingTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSQLiteCache(options =>
            {
                options.DBConfig = new SQLiteDBOptions
                {
                    FileName = "s1.db",
                    CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default,
                    OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory,
                };
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _dbProvider = serviceProvider.GetService<ISQLiteDatabaseProvider>();

            var conn = _dbProvider.GetConnection();
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            conn.Execute(ConstSQL.CREATESQL);

            _provider = new DefaultSQLiteCachingProvider(_dbProvider, new TestOptionMonitorWrapper<SQLiteOptions>(new SQLiteOptions()));
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        protected override Task  GetAsync_Parallel_Should_Succeed()
        {
            return Task.FromResult(1);
                
        }

        [Fact]
        protected override void Get_Parallel_Should_Succeed()
        {

        }
    }


    public class SQLiteCachingProviderWithFactoryTest : BaseCachingProviderWithFactoryTest
    {
        public SQLiteCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSQLiteCacheWithFactory(EasyCachingConstValue.DefaultSQLiteName, options =>
            {
                options.DBConfig = new SQLiteDBOptions
                {
                    FileName = "f0.db",
                    CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default,
                    OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory,
                };

            });
            services.AddSQLiteCacheWithFactory(SECOND_PROVIDER_NAME, options =>
            {
                options.DBConfig = new SQLiteDBOptions
                {
                    FileName = "f1.db",
                    CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default,
                    OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory,
                };
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultSQLiteName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);

            var _dbProviders = serviceProvider.GetServices<ISQLiteDatabaseProvider>();
            foreach (var _dbProvider in _dbProviders)
            {
                var conn = _dbProvider.GetConnection();
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                conn.Execute(ConstSQL.CREATESQL);
            }

            _defaultTs = TimeSpan.FromSeconds(30);
        }
    }

    public class SQLiteCachingProviderUseEasyCachingTest : BaseUsingEasyCachingTest
    {
        private readonly IEasyCachingProvider _secondProvider;
        private const string SECOND_PROVIDER_NAME = "second";

        public SQLiteCachingProviderUseEasyCachingTest()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddEasyCaching(option =>
            {
                option.UseSQLite(config =>
                {
                    config.DBConfig = new SQLiteDBOptions
                    {
                        FileName = "use_0.db",
                    };
                }, EasyCachingConstValue.DefaultSQLiteName);
                option.UseSQLite(config =>
                {
                    config.DBConfig = new SQLiteDBOptions
                    {
                        FileName = "use_1.db",
                    };
                }, SECOND_PROVIDER_NAME);
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultSQLiteName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);

            var _dbProviders = serviceProvider.GetServices<ISQLiteDatabaseProvider>();
            foreach (var _dbProvider in _dbProviders)
            {
                var conn = _dbProvider.GetConnection();
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                conn.Execute(ConstSQL.CREATESQL);
            }

            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Sec_Set_Value_And_Get_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _secondProvider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _secondProvider.Get<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Sec_Set_Value_And_Get_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _secondProvider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _secondProvider.GetAsync<string>(cacheKey);
            Assert.True(val.HasValue);
            Assert.Equal(cacheValue, val.Value);
        }
    }

    public class SQLiteCachingProviderUseEasyCachingWithConfigTest : BaseUsingEasyCachingTest
    {
        public SQLiteCachingProviderUseEasyCachingWithConfigTest()
        {
            IServiceCollection services = new ServiceCollection();

            var appsettings = @"
{
    'easycaching': {
        'sqlite': {
            'MaxRdSecond': 600,
            'Order': 99,
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

            services.AddEasyCaching(option =>
            {
                option.UseSQLite(config, "mName");
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();

            var _dbProviders = serviceProvider.GetServices<ISQLiteDatabaseProvider>();
            foreach (var _dbProvider in _dbProviders)
            {
                var conn = _dbProvider.GetConnection();
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                conn.Execute(ConstSQL.CREATESQL);
            }

            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Provider_Information_Should_Be_Correct()
        {
            Assert.Equal(600, _provider.MaxRdSecond);
            Assert.Equal(99, _provider.Order);
            Assert.Equal("mName", _provider.Name);
        }
    }
}
