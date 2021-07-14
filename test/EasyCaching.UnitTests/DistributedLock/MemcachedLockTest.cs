using EasyCaching.Core.DistributedLock;
using EasyCaching.Memcached;
using Enyim.Caching.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.DistributedLock
{
    public class MemcachedLockTest : DistributedLockTest
    {
        private static readonly IDistributedLockFactory Factory = new ServiceCollection()
            .AddLogging()
            .AddEasyCaching(option => option.UseMemcached(config =>
                {
                    config.DBConfig = new EasyCachingMemcachedClientOptions
                    {
                        Servers =
                        {
                            new Server { Address = "127.0.0.1", Port = 11211 }
                        }
                    };
                })
                .UseMemcachedLock())
            .BuildServiceProvider()
            .GetService<IDistributedLockFactory>();

        public MemcachedLockTest(ITestOutputHelper output) : base(Factory, output) { }
    }
}
