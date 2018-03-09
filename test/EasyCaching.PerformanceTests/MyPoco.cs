namespace EasyCaching.PerformanceTests
{    
    using ProtoBuf;
    using System; 

    [Serializable]
    [ProtoContract]
    public class MyPoco
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }
    }
}
