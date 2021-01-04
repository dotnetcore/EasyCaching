using EasyCaching.Core.Configurations;

namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.InMemory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Xunit;

    public class MemoryCachingProviderTest : BaseCachingProviderTest
    {
        public MemoryCachingProviderTest()
        {
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        protected override IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x => x
                .UseInMemory(options =>
                {
                    options.MaxRdSecond = 0;
                    additionalSetup(options);
                })
            );
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();;
        }

        [Fact]
        public void Default_MaxRdSecond_Should_Be_0()
        {
            Assert.Equal(0, _provider.MaxRdSecond);
        }


        [Fact]
        public void TrySet_Parallel_Should_Succeed()
        {
            var list = new List<bool>();

            Parallel.For(1, 20, x =>
            {
                list.Add(_provider.TrySet<int>("Parallel", 1, TimeSpan.FromSeconds(1)));
            });

            Assert.Equal(1, list.Count(x => x));
        }

        [Fact]
        public void Exists_After_Expiration_Should_Return_False()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            _provider.Set(cacheKey, cacheValue, TimeSpan.FromMilliseconds(200));
            System.Threading.Thread.Sleep(300);
            var flag = _provider.Exists(cacheKey);

            Assert.False(flag);
        }

        [Fact]
        public void Issues105_StackOverflowException_Test()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var cacheValue = new Dictionary<string, IList<MySettingForCaching>>()
            {
                { "ss", new List<MySettingForCaching>{ new MySettingForCaching { Name = "aa" } } }
            };

            // without data retriever
            // _provider.Set(cacheKey, cacheValue, TimeSpan.FromMilliseconds(200));

            // with data retriever
            var res = _provider.Get(cacheKey, () =>
            {
                return cacheValue;
            }, _defaultTs);

            var first = res.Value.First();

            Assert.Equal("ss", first.Key);
            Assert.Equal(1, first.Value.Count);
        }

        [Fact]
        public void Issues150_DeepClone_Object_Test()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var cacheValue = new MySettingForCaching { Name = "catcherwong" } ;

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var res = _provider.Get<MySettingForCaching>(cacheKey);

            res.Value.Name = "kobe";

            var res2 = _provider.Get<MySettingForCaching>(cacheKey);

            Assert.Equal("catcherwong", res2.Value.Name);
        }      
    }
    public class MemoryCachingProviderWithFactoryTest : BaseCachingProviderWithFactoryTest
    {
        public MemoryCachingProviderWithFactoryTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x =>
            {
                x.UseInMemory();
                x.UseInMemory(SECOND_PROVIDER_NAME);
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultInMemoryName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

    }

    public class MemoryCachingProviderUseEasyCachingTest : BaseUsingEasyCachingTest
    {
        private readonly IEasyCachingProvider _secondProvider;
        private const string SECOND_PROVIDER_NAME = "second";

        public MemoryCachingProviderUseEasyCachingTest()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddEasyCaching(option =>
            {
                option.UseInMemory(EasyCachingConstValue.DefaultInMemoryName);
                option.UseInMemory(SECOND_PROVIDER_NAME);
            });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();
            _provider = factory.GetCachingProvider(EasyCachingConstValue.DefaultInMemoryName);
            _secondProvider = factory.GetCachingProvider(SECOND_PROVIDER_NAME);
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

    public class MemoryCachingProviderUseEasyCachingWithConfigTest : BaseUsingEasyCachingTest
    {
        public MemoryCachingProviderUseEasyCachingWithConfigTest()
        {
            IServiceCollection services = new ServiceCollection();
            
            var appsettings = "{ \"easycaching\": { \"inmemory\": { \"MaxRdSecond\": 600, \"dbconfig\": {       \"SizeLimit\" :  50 } } } }";
            var path = TestHelpers.CreateTempFile(appsettings);
            var directory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(directory);
            configurationBuilder.AddJsonFile(fileName);
            var config = configurationBuilder.Build();

            services.AddEasyCaching(option => { option.UseInMemory(config, "mName"); });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
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

    public class MemoryCachingProviderDeepCloneTest
    {
        private readonly TimeSpan _defaultTs;

        private readonly IEasyCachingProvider _m1;
        private readonly IEasyCachingProvider _m2;
        private readonly IEasyCachingProvider _m3;

        public MemoryCachingProviderDeepCloneTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x => 
            {
                x.UseInMemory(options => 
                {
                    options.MaxRdSecond = 0;
                    //options.DBConfig = new InMemoryCachingOptions
                    //{
                    //    EnableWriteDeepClone = false,
                    //    EnableReadDeepClone = true,                       
                    //};
                }, "m1");

                x.UseInMemory(options =>
                {
                    options.MaxRdSecond = 0;
                    options.DBConfig = new InMemoryCachingOptions
                    {
                        EnableWriteDeepClone = true,
                        EnableReadDeepClone = true,
                    };
                }, "m2");

                x.UseInMemory(options =>
                {
                    options.MaxRdSecond = 0;
                    options.DBConfig = new InMemoryCachingOptions
                    {
                        EnableWriteDeepClone = false,
                        EnableReadDeepClone = false,
                    };
                }, "m3");
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();

            _m1 = factory.GetCachingProvider("m1");
            _m2 = factory.GetCachingProvider("m2");
            _m3 = factory.GetCachingProvider("m3");

            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Enable_Read_Write_DeepClone_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var cacheValue = new MySettingForCaching { Name = "catcherwong" };

            _m2.Set(cacheKey, cacheValue, _defaultTs);

            cacheValue.Name = "afterset";

            var res = _m2.Get<MySettingForCaching>(cacheKey);

            res.Value.Name = "kobe";

            var res2 = _m2.Get<MySettingForCaching>(cacheKey);

            Assert.Equal("catcherwong", res2.Value.Name);
        }


        [Fact]
        public void Enable_Read_And_Disable_Write_DeepClone_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var cacheValue = new MySettingForCaching { Name = "catcherwong" };

            _m1.Set(cacheKey, cacheValue, _defaultTs);

            cacheValue.Name = "afterset";

            var res = _m1.Get<MySettingForCaching>(cacheKey);

            res.Value.Name = "kobe";

            var res2 = _m1.Get<MySettingForCaching>(cacheKey);

            Assert.Equal("afterset", res2.Value.Name);
        }


        [Fact]
        public void Disable_Read_And_Disable_Write_DeepClone_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var cacheValue = new MySettingForCaching { Name = "catcherwong" };

            _m3.Set(cacheKey, cacheValue, _defaultTs);

            cacheValue.Name = "afterset";

            var res = _m3.Get<MySettingForCaching>(cacheKey);

            res.Value.Name = "kobe";

            var res2 = _m3.Get<MySettingForCaching>(cacheKey);

            Assert.Equal("kobe", res2.Value.Name);
        }        
    }

    [Serializable]
    public class MySettingForCaching
    {
        public string Name { get; set; }
    }
}