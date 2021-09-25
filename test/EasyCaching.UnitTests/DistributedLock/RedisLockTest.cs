namespace EasyCaching.UnitTests.DistributedLock
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Core.DistributedLock;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

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

        public RedisLockTest(ITestOutputHelper output) : base(EasyCachingConstValue.DefaultRedisName, Factory, output) { }
    }

    public class RedisLockV2Test : DistributedLockV2Test
    {
        public RedisLockV2Test(ITestOutputHelper output) : base(output)
        {
            var factories = new ServiceCollection()
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
                    }, "t2").UseRedisLock("t2"))
                    .BuildServiceProvider()
                    .GetServices<IDistributedLockFactory>();

            _lockFactory = factories.First(x => x.Name.Equals("t2"));
        }

        [Fact]
        public void MultiLock_Should_Succeed()
        {
            var factories = new ServiceCollection()
                    .AddLogging()
                    .AddEasyCaching(option =>
                    {
                        option.UseRedis(config =>
                        {
                            config.DBConfig = new RedisDBOptions
                            {
                                Endpoints =
                            {
                                new ServerEndPoint("127.0.0.1", 6380)
                            }
                            };
                        }, "t2");

                        option.UseRedisLock("t2");
                        option.UseMemoryLock();
                    })
                    .BuildServiceProvider()
                    .GetServices<IDistributedLockFactory>();

            Assert.Equal(2, factories.Count());

            var rl = factories.First(x => x.Name.Equals("t2"));
            Assert.NotNull(rl);
            var ml = factories.First(x => x.Name.Equals("MLF"));
            Assert.NotNull(ml);
        }

        [Fact]
        public void MultiLock_Should_Fail_When_Name_Not_Match()
        {
            var factories = new ServiceCollection()
                    .AddLogging()
                    .AddEasyCaching(option =>
                    {
                        option.UseRedis(config =>
                        {
                            config.DBConfig = new RedisDBOptions
                            {
                                Endpoints =
                            {
                                new ServerEndPoint("127.0.0.1", 6380)
                            }
                            };
                        }, "t2");

                        option.UseRedisLock("t2");
                        option.UseMemoryLock();
                    })
                    .BuildServiceProvider()
                    .GetServices<IDistributedLockFactory>();


            Assert.Throws<Exception>(() => factories.First(x => x.Name.Equals("t3")));
        }
    }
}
