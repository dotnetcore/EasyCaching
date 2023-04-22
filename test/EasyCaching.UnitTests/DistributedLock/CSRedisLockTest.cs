using EasyCaching.Core;
using EasyCaching.Core.DistributedLock;
using EasyCaching.CSRedis;
using EasyCaching.CSRedis.DistributedLock;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.DistributedLock
{
    public class CSRedisLockTest : BaseDistributedLockTest
    {
        private static readonly IDistributedLockFactory Factory = new ServiceCollection()
            .AddLogging()
            .AddEasyCaching(option => option.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=13,poolsize=10"
                        }
                    };
                })
                .UseCSRedisLock().WithJson(EasyCachingConstValue.DefaultCSRedisName))
            .BuildServiceProvider()
            .GetService<CSRedisLockFactory>();

        public CSRedisLockTest(ITestOutputHelper output) : base(EasyCachingConstValue.DefaultCSRedisName, Factory, output) { }
    }
}
