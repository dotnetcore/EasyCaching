namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.MessagePack;
    using MessagePack.Resolvers;
    using System;
    using Xunit;

    public class MessagePackSerializerTest : BaseSerializerTest
    {
        public MessagePackSerializerTest()
        {
            _serializer = new DefaultMessagePackSerializer("msgpack", new EasyCachingMsgPackSerializerOptions { });       
        }
    }

    public class MessagePackSerializerTest2 //: BaseSerializerTest
    {
        DefaultMessagePackSerializer _serializer;

        public MessagePackSerializerTest2()
        {
           // CompositeResolver.RegisterAndSetAsDefault(
           //    // This can solve DateTime time zone problem
           //    NativeDateTimeResolver.Instance,
           //    ContractlessStandardResolver.Instance
           //);

            // due to messagepack api change
            var reslover = CompositeResolver.Create(new MessagePack.IFormatterResolver[] { ContractlessStandardResolver.Instance });

            _serializer = new DefaultMessagePackSerializer("msgpack", new EasyCachingMsgPackSerializerOptions { EnableCustomResolver = true, CustomResolvers = reslover });
        }

        [Fact]
        public void Issue_174_DateTime_Test()
        {
            var dt1 = DateTime.Parse("2019-11-07 10:30:30");

            var s1 = _serializer.Serialize(new DT { Dt = dt1 });

            Assert.NotEmpty(s1);

            var res1 = _serializer.Deserialize<DT>(s1);

            Assert.Equal(dt1, res1.Dt);

            var dt = DateTime.Parse("2019-11-07 10:30:30").ToUniversalTime();

            var s = _serializer.Serialize(new DT { Dt = dt });

            Assert.NotEmpty(s);

            var res = _serializer.Deserialize<DT>(s);

            Assert.Equal(dt, res.Dt);
        }     

        public class DT
        {
            public DateTime Dt { get; set; }
        }
    }
}