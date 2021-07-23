namespace EasyCaching.Core.DistributedLock
{
    public interface IDistributedLockFactory
    {
        IDistributedLock CreateLock(string name, string key);
    }
}
