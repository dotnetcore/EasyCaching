using EasyCaching.Core.DistributedLock;
using Xunit.Abstractions;

namespace EasyCaching.UnitTests.DistributedLock
{
    public abstract class MemorylLockTest : DistributedLockTest
    {
        public static IDistributedLockFactory Factory = new MemoryLockFactory();

        protected MemorylLockTest(ITestOutputHelper output) : base(Factory, output) { }
    }
}