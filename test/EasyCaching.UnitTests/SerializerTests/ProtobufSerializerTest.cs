namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.Protobuf;

    public class ProtobufSerializerTest: BaseSerializerTest
    {
        public ProtobufSerializerTest()
        {
            _serializer = new DefaultProtobufSerializer("proto");
        }
    }
}
