namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.Json;
    using FakeItEasy;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Xunit;

    public class JsonSerializerTest : BaseSerializerTest
    {
        public JsonSerializerTest()
        {
            var options = A.Fake<IOptions<EasyCachingJsonSerializerOptions>>();

            A.CallTo(() => options.Value).Returns(new EasyCachingJsonSerializerOptions());

            _serializer = new DefaultJsonSerializer(options);
        }

        [Fact]
        public void Isuue_50_Test()
        {
            Employee joe = new Employee { Name = "Joe User" };
            Employee mike = new Employee { Name = "Mike Manager" };
            joe.Manager = mike;
            mike.Manager = mike;

            Assert.Throws<JsonSerializationException>(() => _serializer.Serialize(joe));
        }

        [Fact]
        public void Options_Test_Should_Succeed()
        {
            var options = A.Fake<IOptions<EasyCachingJsonSerializerOptions>>();

            A.CallTo(() => options.Value).Returns(new EasyCachingJsonSerializerOptions()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            var  serializer = new DefaultJsonSerializer(options);

            Employee joe = new Employee { Name = "Joe User" };
            Employee mike = new Employee { Name = "Mike Manager" };
            joe.Manager = mike;
            mike.Manager = mike;

            var joe_byte = serializer.Serialize(joe);
            var joe_obj = serializer.Deserialize<Employee>(joe_byte);


            Assert.Equal(joe.Name, joe_obj.Name);

        }

        public class Employee
        {
            public string Name { get; set; }
            public Employee Manager { get; set; }
        }
    }
}
