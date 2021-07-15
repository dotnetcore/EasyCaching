using EasyCaching.Core;
using EasyCaching.Core.DistributedLock;
using EasyCaching.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace EasyCaching.UnitTests.DistributedLock
{
    public class EasyCachingAbstractProviderTest
    {
        [Fact]
        public void BaseGet_Should_Not_Be_Called() =>
            CreateCachingProvider().Get(Guid.NewGuid().ToString(), Guid.NewGuid, TimeSpan.FromSeconds(10));

        [Fact]
        public Task BaseGetAsync_Should_Not_Be_Called() =>
            CreateCachingProvider().GetAsync(Guid.NewGuid().ToString(), () => Task.FromResult(Guid.NewGuid()), TimeSpan.FromSeconds(10));

        private static IEasyCachingProvider CreateCachingProvider()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x => x
                .UseInMemory(options =>
                {
                    options.MaxRdSecond = 0;
                }).UseMemoryLock()
            );
            services.AddSingleton<IEasyCachingProvider, TestCachingProvider>(x =>
            {
                var mCache = x.GetServices<IInMemoryCaching>();
                var optionsMon = x.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<InMemoryOptions>>();
                var options = optionsMon.Get(EasyCachingConstValue.DefaultInMemoryName);
                var dlf = x.GetService<IDistributedLockFactory>();
                // ILoggerFactory can be null
                var factory = x.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
                return new TestCachingProvider(EasyCachingConstValue.DefaultInMemoryName, mCache, options, dlf, factory);
            });
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>(); ;
        }
    }

    public class TestCachingProvider : DefaultInMemoryCachingProvider
    {
        public TestCachingProvider(string name, IEnumerable<IInMemoryCaching> cache, InMemoryOptions options, IDistributedLockFactory factory, ILoggerFactory loggerFactory = null) : base(name, cache, options, factory, loggerFactory)
        {
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            throw new InvalidOperationException();
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            throw new InvalidOperationException();
        }
    }
}
