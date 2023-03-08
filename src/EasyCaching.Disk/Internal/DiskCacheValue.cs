namespace EasyCaching.Disk
{
    public class DiskCacheValue
    {
        public DiskCacheValue()
        {
        }

        public DiskCacheValue(byte[] val, long time)
        {
            Value = val;
            Expiration = time;
        }

        public byte[] Value { get; set; }

        public long Expiration { get; set; }
    }
}
