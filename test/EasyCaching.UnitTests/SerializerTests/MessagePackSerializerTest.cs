namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.MessagePack;
    using Xunit; 

    public class MessagePackSerializerTest
    {
        private readonly DefaultMessagePackSerializer _serializer;        

        public MessagePackSerializerTest()
        {
            _serializer = new DefaultMessagePackSerializer();            
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

        [Fact]
        public void SerializeObject_should_Succeed()
        {
            object obj = new Model { Prop = "abc" };

            var bytes = _serializer.SerializeObject(obj);

            Assert.NotEmpty(bytes);            
        }
        
        [Fact]
        public void DeserializeObject_should_Succeed()
        {
            object obj = new Model { Prop = "abc" };

            var bytes = _serializer.SerializeObject(obj);

            var desObj = _serializer.DeserializeObject(bytes) as Model;

            Assert.Equal("abc",desObj.Prop);   
        }
    }

    public class Model
    {
        public string Prop { get; set; }
    }
}
