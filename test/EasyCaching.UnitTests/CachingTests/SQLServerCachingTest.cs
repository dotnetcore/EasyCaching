using EasyCaching.SQLServer;
using EasyCaching.SQLServer.Configurations;
using Microsoft.Extensions.Options;

namespace EasyCaching.UnitTests
{
    using Dapper;
    using EasyCaching.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Xunit;

    public class SQLServerCachingTest : BaseCachingProviderTest
    {
        private readonly ISQLDatabaseProvider _dbProvider;

        private const string CONNECTION_STRING = "Data Source=.\\sqlexpress;Initial Catalog=EasyCacheDB;Integrated Security=True";
        private const string SCHEMA_NAME = "Easy";
        private const string TABLE_NAME = "Cache";

        public SQLServerCachingTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSQLServerCache(options =>
            {
                options.DBConfig = new SQLDBOptions
                {
                    ConnectionString = CONNECTION_STRING,
                    SchemaName = SCHEMA_NAME,
                    TableName = TABLE_NAME
                };
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _dbProvider = serviceProvider.GetService<ISQLDatabaseProvider>();
            var optionsMon = serviceProvider.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();

            var conn = _dbProvider.GetConnection();
            if (conn.State == System.Data.ConnectionState.Closed)
            {
                conn.Open();
            }
            var op = optionsMon.CurrentValue.DBConfig;
            var sql = string.Format(ConstSQL.CREATESQL, op.SchemaName,
                op.TableName);
            conn.Execute(sql);

            _provider = new DefaultSQLServerCachingProvider(_dbProvider, new TestOptionMonitorWrapper<SQLServerOptions>(optionsMon.CurrentValue));
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


    public class SQLServerCachingProviderWithFactoryTest : BaseCachingProviderWithFactoryTest
    {
        private const string CONNECTION_STRING = "Data Source=.\\sqlexpress;Initial Catalog=EasyCacheDB;Integrated Security=True";
        private const string SCHEMA_NAME = "Easy";
        private const string TABLE_NAME = "Cache";

        public SQLServerCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSQLServerCacheWithFactory(EasyCachingConstValue.DefaultSQLServerName, options =>
            {
                options.DBConfig = new SQLDBOptions
                {
                    ConnectionString = CONNECTION_STRING,
                    SchemaName = SCHEMA_NAME,
                    TableName = TABLE_NAME
                };

            });
            services.AddSQLServerCacheWithFactory(SECOND_PROVIDER_NAME, options =>
            {
                options.DBConfig = new SQLDBOptions
                {
                    ConnectionString = CONNECTION_STRING,
                    SchemaName = SCHEMA_NAME,
                    TableName = TABLE_NAME
                };
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultSQLServerName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);

            var _dbProviders = serviceProvider.GetServices<ISQLDatabaseProvider>();
            var optionsMon = serviceProvider.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
            foreach (var _dbProvider in _dbProviders)
            {
                var conn = _dbProvider.GetConnection();
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                var options = optionsMon.Get(_dbProvider.DBProviderName);
                conn.Execute(string.Format(ConstSQL.CREATESQL, options.DBConfig.SchemaName,
                    options.DBConfig.TableName));
            }

            _defaultTs = TimeSpan.FromSeconds(30);
        }
    }

    public class SQLServerCachingProviderUseEasyCachingTest : BaseUsingEasyCachingTest
    {
        private readonly IEasyCachingProvider _secondProvider;
        private const string SECOND_PROVIDER_NAME = "second";

        private const string CONNECTION_STRING = "Data Source=.\\sqlexpress;Initial Catalog=EasyCacheDB;Integrated Security=True";
        private const string SCHEMA_NAME = "Easy";
        private const string TABLE_NAME = "Cache";

        public SQLServerCachingProviderUseEasyCachingTest()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddEasyCaching(option =>
            {
                option.UseSQLServer(config =>
                {
                    config.DBConfig = new SQLDBOptions()
                    {
                        ConnectionString = CONNECTION_STRING,
                        SchemaName = SCHEMA_NAME,
                        TableName = TABLE_NAME
                    };
                }, EasyCachingConstValue.DefaultSQLServerName);
                option.UseSQLServer(config =>
                {
                    config.DBConfig = new SQLDBOptions()
                    {
                        ConnectionString = CONNECTION_STRING,
                        SchemaName = SCHEMA_NAME,
                        TableName = TABLE_NAME
                    };
                }, SECOND_PROVIDER_NAME);
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultSQLServerName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);

            var dbProviders = serviceProvider.GetServices<ISQLDatabaseProvider>();
            var optionsMon = serviceProvider.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
            foreach (var dbProvider in dbProviders)
            {
                var conn = dbProvider.GetConnection();

                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }

                var options = optionsMon.Get(dbProvider.DBProviderName);
                var sql = string.Format(ConstSQL.CREATESQL, options.DBConfig.SchemaName,
                    options.DBConfig.TableName);
                conn.Execute(sql);
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

    public class SQLServerCachingProviderUseEasyCachingWithConfigTest : BaseUsingEasyCachingTest
    {
        public SQLServerCachingProviderUseEasyCachingWithConfigTest()
        {
            IServiceCollection services = new ServiceCollection();

            var appsettings = @"
{
    'easycaching': {
        'sqlserver': {
            'MaxRdSecond': 600,
            'Order': 99,
            'dbconfig': {            
                'connectionString':'Data Source=.\\sqlexpress;Initial Catalog=EasyCacheDB;Integrated Security=True',
                'schemaName':'Easy',
                'tableName':'Cache'
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
                option.UseSQLServer(config, "mName");
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();

            var _dbProviders = serviceProvider.GetServices<ISQLDatabaseProvider>();
            var optionsMon = serviceProvider.GetRequiredService<IOptionsMonitor<SQLServerOptions>>();
            foreach (var _dbProvider in _dbProviders)
            {
                var conn = _dbProvider.GetConnection();
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.Open();
                }
                var options = optionsMon.Get(_dbProvider.DBProviderName);
                var sql = string.Format(ConstSQL.CREATESQL, options.DBConfig.SchemaName,
                    options.DBConfig.TableName);
                conn.Execute(sql);
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
