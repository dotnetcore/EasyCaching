namespace EasyCaching.Disk
{
    using System;
    using MessagePack;

    public class DiskCacheValue
    {
        [SerializationConstructor]
        public DiskCacheValue(byte[] val, int second)
        {
            Value = val;
            Expiration = DateTimeOffset.UtcNow.AddSeconds(second);
        }

        [Key(0)]
        public byte[] Value { get; private set; }

        [Key(1)]
        public DateTimeOffset Expiration { get; private set; }
    }
}
