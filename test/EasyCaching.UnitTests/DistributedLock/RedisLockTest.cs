using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using EasyCaching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.DistributedLock
{
    public class RedisLockTest : DistributedLockTest
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
                .UseRedisLock())
            .BuildServiceProvider()
            .GetService<IDistributedLockFactory>();

        public RedisLockTest(ITestOutputHelper output) : base(Factory, output) { }
    }
}
