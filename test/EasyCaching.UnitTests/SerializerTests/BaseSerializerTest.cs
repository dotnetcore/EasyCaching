namespace EasyCaching.UnitTests
{
    using System;
    using EasyCaching.Core.Serialization;
    using ProtoBuf;
    using Xunit;

    public abstract class BaseSerializerTest
    {
        protected IEasyCachingSerializer _serializer;

        [Fact]
        public void Serialize_Object_Should_Succeed()
        {
            var res = _serializer.Serialize(new Model { Prop = "abc" });

            Assert.NotEmpty(res);
        }

        [Fact]
        public void Deserialize_Object_Should_Succeed()
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

            Assert.Equal("abc", desObj.Prop);
        }


        [Theory]
        [InlineData("N2L7KXa084WvelONYjkJ_traBMCCvy_UKmpUUzlrQ0EA2yNp3Iz6eSUrRG0bhaR_viswd50vDuPkY5nG43d1gbm-olT2KRMxOsVE08RfeD9lvK9lMguNG9kpIkKGZEjIf8Jv2m9fFhf8bnNa-yQH3g")]
        [InlineData("123abc")]
        public void Serialize_String_Should_Succeed(string str)
        {
            var res = _serializer.Serialize(str);

            Assert.NotEmpty(res);
        }

        [Theory]
        [InlineData("N2L7KXa084WvelONYjkJ_traBMCCvy_UKmpUUzlrQ0EA2yNp3Iz6eSUrRG0bhaR_viswd50vDuPkY5nG43d1gbm-olT2KRMxOsVE08RfeD9lvK9lMguNG9kpIkKGZEjIf8Jv2m9fFhf8bnNa-yQH3g")]
        [InlineData("123abc")]
        public void Deserialize_String_Should_Succeed(string str)
        {
            var bytes = _serializer.Serialize(str);

            var res = _serializer.Deserialize<string>(bytes);

            Assert.Equal(str, res);
        }

    }

    [Serializable]
    [ProtoContract]
    public class Model
    {
        [ProtoMember(1)]
        public string Prop { get; set; }
    }
}