namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.MessagePack;

    public class MessagePackSerializerTest : BaseSerializerTestTest
    {
        public MessagePackSerializerTest()
        {
            _serializer = new DefaultMessagePackSerializer();
        }
    }
}