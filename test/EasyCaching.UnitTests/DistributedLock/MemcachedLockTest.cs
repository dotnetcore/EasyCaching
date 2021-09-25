using EasyCaching.Core;
using EasyCaching.Core.DistributedLock;
using EasyCaching.Memcached;
using Enyim.Caching.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;
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

        public MemcachedLockTest(ITestOutputHelper output) : base(EasyCachingConstValue.DefaultMemcachedName, Factory, output) { }
    }

    public class MemcachedLockV2Test : DistributedLockV2Test
    {
        public MemcachedLockV2Test(ITestOutputHelper output) : base(output)
        {
            var factories = new ServiceCollection()
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
                    }, "t2").UseMemcachedLock("t2"))
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
                        option.UseMemcached(config =>
                        {
                            config.DBConfig = new EasyCachingMemcachedClientOptions
                            {
                                Servers =
                                {
                                    new Server { Address = "127.0.0.1", Port = 11211 }
                                }
                            };
                        }, "t2");

                        option.UseMemcachedLock("t2");
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
                        option.UseMemcached(config =>
                        {
                            config.DBConfig = new EasyCachingMemcachedClientOptions
                            {
                                Servers =
                                {
                                    new Server { Address = "127.0.0.1", Port = 11211 }
                                }
                            };
                        }, "t2");

                        option.UseMemcachedLock("t2");
                        option.UseMemoryLock();
                    })
                    .BuildServiceProvider()
                    .GetServices<IDistributedLockFactory>();


            Assert.Throws<InvalidOperationException>(() => factories.First(x => x.Name.Equals("t3")));
        }
    }
}
