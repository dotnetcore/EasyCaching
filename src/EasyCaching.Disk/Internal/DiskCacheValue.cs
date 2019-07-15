namespace EasyCaching.Disk
{
    using System;
    using MessagePack;

    [MessagePackObject]
    public class DiskCacheValue
    {
        [SerializationConstructor]
        public DiskCacheValue(byte[] val, DateTimeOffset time)
        {
            Value = val;
            Expiration = time;
        }


        [Key(0)]
        public byte[] Value { get; private set; }

        [Key(1)]
        public DateTimeOffset Expiration { get; private set; }
    }
}
