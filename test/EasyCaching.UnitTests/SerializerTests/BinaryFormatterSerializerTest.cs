namespace EasyCaching.UnitTests
{
    using EasyCaching.Redis;
    using Xunit; 

    public class BinaryFormatterSerializerTest : BaseSerializerTest
    {      
        public BinaryFormatterSerializerTest()
        {
            _serializer = new DefaultBinaryFormatterSerializer();            
        }
    }
}