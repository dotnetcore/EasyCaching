namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.LiteDB;
    using EasyCaching.Core.Configurations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Xunit;

    public class LiteDBCachingTest : BaseCachingProviderTest
    {
        public LiteDBCachingTest()
        {
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        protected override IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
                x.UseLiteDB(options =>
                {
                    options.DBConfig = new  LiteDBDBOptions
                    {
                        FileName = "s1.ldb"
                    };
                    additionalSetup(options);
                })
            );

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();;
        }

        [Fact]
        protected override Task GetAsync_Parallel_Should_Succeed()
        {
            return Task.FromResult(1);
        }
        
        [Fact]
        protected override void Get_Parallel_Should_Succeed()
        {
        }
    }


    public class LiteDBCachingProviderWithFactoryTest : BaseCachingProviderWithFactoryTest
    {
        public LiteDBCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
            {
                x.UseLiteDB(options =>
                {
                    options.DBConfig = new  LiteDBDBOptions
                    {
                        FileName = "f0.ldb",
        
                    };
                }).UseLiteDB(options =>
                {
                    options.DBConfig = new  LiteDBDBOptions
                    {
                        FileName = "f1.ldb",
                     
                    };
                }, SECOND_PROVIDER_NAME);
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultLiteDBName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);

            var _dbProviders = serviceProvider.GetServices<ILiteDBDatabaseProvider>();
            foreach (var _dbProvider in _dbProviders)
            {
                var conn = _dbProvider.GetConnection();
            }

            _defaultTs = TimeSpan.FromSeconds(30);
        }
    }

    public class LiteDBCachingProviderUseEasyCachingTest : BaseUsingEasyCachingTest
    {
        private readonly IEasyCachingProvider _secondProvider;
        private const string SECOND_PROVIDER_NAME = "second";

        public LiteDBCachingProviderUseEasyCachingTest()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddEasyCaching(option =>
            {
                option.UseLiteDB(config =>
                {
                    config.DBConfig = new  LiteDBDBOptions
                    {
                        FileName = "use_0.ldb",
                    };
                }, EasyCachingConstValue.DefaultLiteDBName);
                option.UseLiteDB(config =>
                {
                    config.DBConfig = new  LiteDBDBOptions
                    {
                        FileName = "use_1.ldb",
                    };
                }, SECOND_PROVIDER_NAME);
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultLiteDBName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);

            var _dbProviders = serviceProvider.GetServices<ILiteDBDatabaseProvider>();
            foreach (var _dbProvider in _dbProviders)
            {
                var conn = _dbProvider.GetConnection();
               
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

    public class LiteDBCachingProviderUseEasyCachingWithConfigTest : BaseUsingEasyCachingTest
    {
        public LiteDBCachingProviderUseEasyCachingWithConfigTest()
        {
            IServiceCollection services = new ServiceCollection();

            var appsettings = " { \"easycaching\": { \"litedb\": { \"MaxRdSecond\": 600, \"dbconfig\": { \"FileName\": \"my.ldb\" } } } }";
            var path = TestHelpers.CreateTempFile(appsettings);
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);
            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();

            services.AddEasyCaching(option => { option.UseLiteDB(config, "mName"); });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();

            var _dbProviders = serviceProvider.GetServices<ILiteDBDatabaseProvider>();
            foreach (var _dbProvider in _dbProviders)
            {
                var conn = _dbProvider.GetConnection();
            }
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Provider_Information_Should_Be_Correct()
        {
            Assert.Equal(600, _provider.MaxRdSecond);
            //Assert.Equal(99, _provider.Order);
            Assert.Equal("mName", _provider.Name);
        }
    }
}