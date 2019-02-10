namespace EasyCaching.UnitTests
{
    using EasyCaching.Core.Serialization;

    public class BinaryFormatterSerializerTest : BaseSerializerTest
    {      
        public BinaryFormatterSerializerTest()
        {
            _serializer = new DefaultBinaryFormatterSerializer();            
        }
    }
}