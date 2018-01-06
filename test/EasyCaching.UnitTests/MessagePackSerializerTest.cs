namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.MessagePack;
    using Xunit; 

    public class MessagePackSerializerTest
    {
        private readonly DefaultMessagePackSerializer _serializer;
        //private readonly string jsonStr;

        public MessagePackSerializerTest()
        {
            _serializer = new DefaultMessagePackSerializer();
            //jsonStr = "{\"Prop\":\"abc\"}";
        }

        [Fact]
        public void Serialize_Should_Succeed()
        {
            var res = _serializer.Serialize(new Model{ Prop = "abc"});

            Assert.NotEmpty(res);
        }             

        [Fact]
        public void Deserialize_Should_Succeed()
        {
            var bytes = _serializer.Serialize(new Model { Prop = "abc" });

            var res = _serializer.Deserialize<Model>(bytes);

            Assert.Equal("abc", res.Prop);
        }
    }

    public class Model
    {
        public string Prop { get; set; }
    }
}
