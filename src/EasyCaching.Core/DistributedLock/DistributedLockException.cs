using System;

namespace EasyCaching.Core.DistributedLock
{
    [Serializable]
    public class DistributedLockException : Exception
    {
        public DistributedLockException() : base("锁释放前请不要重复锁") { }

        public DistributedLockException(string message) : base(message) { }
    }
}
