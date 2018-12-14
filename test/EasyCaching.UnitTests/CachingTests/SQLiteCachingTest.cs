namespace EasyCaching.UnitTests
{
    using Dapper;
    using EasyCaching.Core;
    using EasyCaching.SQLite;
    using Microsoft.Extensions.DependencyInjection;
    using System;
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
                    FileName = "",
                    FilePath = "",
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
    }



    public class SQLiteCachingProviderWithFactoryTest : BaseCachingProviderTest
    {
        private readonly IEasyCachingProvider _secondProvider;

        private const string SECOND_PROVIDER_NAME = "second";

        public SQLiteCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSQLiteCacheWithFactory(EasyCachingConstValue.DefaultSQLiteName, options =>
            {
                options.DBConfig = new SQLiteDBOptions
                {
                    FileName = "",
                    FilePath = "",
                    CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default,
                    OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory,
                };
               
            });
            services.AddSQLiteCacheWithFactory(SECOND_PROVIDER_NAME, options =>
            {
                options.DBConfig = new SQLiteDBOptions
                {
                    FileName = "",
                    FilePath = "",
                    CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default,
                    OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory,
                };
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultSQLiteName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);

            var _dbProvider = serviceProvider.GetService<ISQLiteDatabaseProvider>();
            var conn = _dbProvider.GetConnection();
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            conn.Execute(ConstSQL.CREATESQL);

            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Multi_Instance_Set_And_Get_Should_Succeed()
        {
            var cacheKey1 = "named-provider-1";
            var cacheKey2 = "named-provider-2";

            var value1 = Guid.NewGuid().ToString();
            var value2 = Guid.NewGuid().ToString("N");

            _provider.Set(cacheKey1, value1, _defaultTs);
            _secondProvider.Set(cacheKey2, value2, _defaultTs);

            var p1 = _provider.Get<string>(cacheKey1);
            var p2 = _provider.Get<string>(cacheKey2);

            var s1 = _secondProvider.Get<string>(cacheKey1);
            var s2 = _secondProvider.Get<string>(cacheKey2);

            Assert.Equal(value1, p1.Value);
            Assert.False(p2.HasValue);

            Assert.False(s1.HasValue);
            Assert.Equal(value2, s2.Value);
        }
    }
}
