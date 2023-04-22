using EasyCaching.Core;
using EasyCaching.Core.DistributedLock;
using EasyCaching.Memcached;
using EasyCaching.Memcached.DistributedLock;
using Enyim.Caching.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.DistributedLock
{
    public class MemcachedLockTest : BaseDistributedLockTest
    {
        private static readonly IDistributedLockFactory Factory = new ServiceCollection()
            .AddLogging()
            .AddEasyCaching(option =>
            {
                option.WithMessagePack("msg");
                option.UseMemcached(config =>
                {
                    config.SerializerName = "msg";
                    config.DBConfig = new EasyCachingMemcachedClientOptions
                    {
                        Servers =
                        {
                            new Server { Address = "127.0.0.1", Port = 11211 }
                        }
                    };
                });
                option.UseMemcachedLock();
            })
            .BuildServiceProvider()
            .GetService<MemcachedLockFactory>();

        public MemcachedLockTest(ITestOutputHelper output) : base(EasyCachingConstValue.DefaultMemcachedName, Factory, output) { }
    }
}
