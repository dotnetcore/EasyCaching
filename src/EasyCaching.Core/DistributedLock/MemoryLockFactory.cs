namespace EasyCaching.Core.DistributedLock
{
    public class MemoryLockFactory : IDistributedLockFactory
    {
        public string Name => "MLF";

        public IDistributedLock CreateLock(string name, string key) => new MemoryLock($"{name}/{key}");

        public IDistributedLock CreateLock(string key) => new MemoryLock($"{key}");
    }
}
