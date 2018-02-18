namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;

    public class BinaryFormatterSerializerTest : BaseSerializerTest
    {      
        public BinaryFormatterSerializerTest()
        {
            _serializer = new DefaultBinaryFormatterSerializer();            
        }
    }
}