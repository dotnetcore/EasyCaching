namespace EasyCaching.Core.DistributedLock
{
    public interface IDistributedLockFactory
    {
        [System.Obsolete("This method will be removed in a future version. Use CreateLock(string key) instead.")]
        IDistributedLock CreateLock(string name, string key);

        IDistributedLock CreateLock(string key);

        string Name { get; }
    }
}
