namespace EasyCaching.Serialization.Json.Test
{    
    using Xunit; 

    public class JsonSerializerTest
    {
        private readonly DefaultEasyCachingJsonSerializer _serializer;
        private readonly string jsonStr;

        public JsonSerializerTest()
        {
            _serializer = new DefaultEasyCachingJsonSerializer();
            jsonStr = "{\"Prop\":\"abc\"}";
        }

        [Fact]
        public void Serialize_Should_Succeed()
        {
            var res = _serializer.Serialize(new Model{ Prop = "abc"});

            string exp = jsonStr;

            Assert.Equal(exp,res);
        }

        [Fact]
        public void Deserialize_Should_Succeed()
        {
            dynamic tmp = _serializer.Deserialize(jsonStr);

            string res = tmp.Prop;
            string exp = "abc";

            Assert.Equal(exp, res);
        }

        [Fact]
        public void Deserialize_With_T_Should_Succeed()
        {
            var res = _serializer.Deserialize<Model>(jsonStr);

            string exp = "abc";

            Assert.Equal(exp, res.Prop);
        }
    }


    public class Model
    {
        public string Prop { get; set; }
    }
}
