namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.MessagePack;

    public class MessagePackSerializerTest : BaseSerializerTest
    {
        public MessagePackSerializerTest()
        {
            _serializer = new DefaultMessagePackSerializer();
        }
    }
}