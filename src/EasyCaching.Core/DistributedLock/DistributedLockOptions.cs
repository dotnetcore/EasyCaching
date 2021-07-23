using EasyCaching.Core.Configurations;

namespace EasyCaching.Core.DistributedLock
{
    public class DistributedLockOptions
    {
        public int MaxTtl { get; set; } = 30000;

        public int DueTime { get; set; } = 8000;

        public int Period { get; set; } = 8000;

        public static DistributedLockOptions FromProviderOptions(BaseProviderOptions options) =>
            new DistributedLockOptions
            {
                MaxTtl = options.LockMs * 3 + 1000,
                DueTime = options.LockMs,
                Period = options.LockMs
            };
    }
}
