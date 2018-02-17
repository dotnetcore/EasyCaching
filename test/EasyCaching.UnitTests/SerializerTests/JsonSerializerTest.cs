namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.Json;

    public class JsonSerializerTest: BaseSerializerTest
    {
        public JsonSerializerTest()
        {
            _serializer = new DefaultJsonSerializer();
        }
    }
}
