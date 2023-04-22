using EasyCaching.Core;
using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using EasyCaching.Redis;
using EasyCaching.Redis.DistributedLock;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.DistributedLock
{
    public class RedisLockTest : BaseDistributedLockTest
    {
        private static readonly IDistributedLockFactory Factory = new ServiceCollection()
            .AddLogging()
            .AddEasyCaching(option => option.UseRedis(config =>
                {
                    config.DBConfig = new RedisDBOptions
                    {
                        Endpoints =
                        {
                            new ServerEndPoint("127.0.0.1", 6380)
                        }
                    };
                })
                .UseRedisLock().WithJson(EasyCachingConstValue.DefaultRedisName))
            .BuildServiceProvider()
            .GetService<RedisLockFactory>();

        public RedisLockTest(ITestOutputHelper output) : base(EasyCachingConstValue.DefaultRedisName, Factory, output) { }
    }
}
