using EasyCaching.Core.DistributedLock;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Xunit;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.DistributedLock
{
    public class MemoryLockTest : DistributedLockTest
    {
        public static IDistributedLockFactory Factory = new MemoryLockFactory();

        public MemoryLockTest(ITestOutputHelper output) : base(Factory, output) { }

        [Fact]
        public async Task Concurrency_Test()
        {
            ThreadPool.SetMinThreads(1000, 1000);

            var random = new Random();
            var key = Guid.NewGuid().ToString("N");
            var counter = 0;

            using var @lock = Factory.CreateLock("test", Guid.NewGuid().ToString());

            await Task.WhenAll(Enumerable.Range(0, 100)
                .Select(_ => Task.Run(async () =>
                {
                    for (var index = 0; index < 1000; index++)
                    {
                        var value = Interlocked.Increment(ref counter);

                        try
                        {
                            Assert.True(await @lock.LockAsync(15000, CancellationToken.None), value.ToString());

                            await Task.Delay(random.Next(0, 10));

                            await @lock.ReleaseAsync().ConfigureAwait(false);
                        }
                        catch (DistributedLockException)
                        {
                        }
                    }
                })));
        }
    }
}