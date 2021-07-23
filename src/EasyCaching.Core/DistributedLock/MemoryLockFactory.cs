namespace EasyCaching.Core.DistributedLock
{
    public class MemoryLockFactory : IDistributedLockFactory
    {
        public IDistributedLock CreateLock(string name, string key) => new MemoryLock($"{name}/{key}");
    }
}
