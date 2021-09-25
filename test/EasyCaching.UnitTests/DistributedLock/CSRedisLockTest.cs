namespace EasyCaching.UnitTests.DistributedLock
{
    using EasyCaching.Core;
    using EasyCaching.Core.DistributedLock;
    using EasyCaching.CSRedis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class CSRedisLockTest : DistributedLockTest
    {
        private static readonly IDistributedLockFactory Factory = new ServiceCollection()
            .AddLogging()
            .AddEasyCaching(option => option.UseCSRedis(config =>
                {
                    config.DBConfig = new CSRedisDBOptions
                    {
                        ConnectionStrings = new System.Collections.Generic.List<string>
                        {
                            "127.0.0.1:6388,defaultDatabase=7,poolsize=10"
                        }
                    };
                })
                .UseCSRedisLock())
            .BuildServiceProvider()
            .GetService<IDistributedLockFactory>();

        public CSRedisLockTest(ITestOutputHelper output) : base(EasyCachingConstValue.DefaultCSRedisName, Factory, output) 
        {
        }
    }

    public class CSRedisLockV2Test : DistributedLockV2Test
    {
        public CSRedisLockV2Test(ITestOutputHelper output) : base(output)
        {
            var factories = new ServiceCollection()
                    .AddLogging()
                    .AddEasyCaching(option => option.UseCSRedis(config =>
                    {
                        config.DBConfig = new CSRedisDBOptions
                        {
                            ConnectionStrings = new System.Collections.Generic.List<string>
                                {
                                    "127.0.0.1:6388,defaultDatabase=8,poolsize=10"
                                }
                        };
                    }, "t2").UseCSRedisLock("t2"))
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
                        option.UseCSRedis(config =>
                        {
                            config.DBConfig = new CSRedisDBOptions
                            {
                                ConnectionStrings = new System.Collections.Generic.List<string>
                                {
                                    "127.0.0.1:6388,defaultDatabase=8,poolsize=10"
                                }
                            };
                        }, "t2");

                        option.UseCSRedisLock("t2");
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
                        option.UseCSRedis(config =>
                        {
                            config.DBConfig = new CSRedisDBOptions
                            {
                                ConnectionStrings = new System.Collections.Generic.List<string>
                                {
                                    "127.0.0.1:6388,defaultDatabase=8,poolsize=10"
                                }
                            };
                        }, "t2");

                        option.UseCSRedisLock("t2");
                        option.UseMemoryLock();
                    })
                    .BuildServiceProvider()
                    .GetServices<IDistributedLockFactory>();


            Assert.Throws<InvalidOperationException>(() => factories.First(x => x.Name.Equals("t3")));
        }
    }
}
