namespace EasyCaching.Disk
{
    using System;
    using ProtoBuf;
    using MessagePack;
    using MemoryPack;

    [Serializable]
    [ProtoContract]
    [MessagePackObject]
    [MemoryPackable(GenerateType.CircularReference)]
    public partial class DiskCacheValue
    {
        [MemoryPackConstructor]
        public DiskCacheValue()
        {
        }

        [SerializationConstructor]
        public DiskCacheValue(byte[] val, long time)
        {
            Value = val;
            Expiration = time;
        }

        [MemoryPackOrder(0)]
        [ProtoMember(1)]
        [Key(0)]
        public byte[] Value { get; set; }

        [MemoryPackOrder(1)]
        [ProtoMember(2)]
        [Key(1)]
        public long Expiration { get; set; }
    }
}
