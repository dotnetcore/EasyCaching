namespace EasyCaching.UnitTests
{
    using EasyCaching.Redis;
    using Xunit; 

    public class BinaryFormatterSerializerTest : BaseSerializerTestTest
    {      
        public BinaryFormatterSerializerTest()
        {
            _serializer = new DefaultBinaryFormatterSerializer();            
        }
    }
}