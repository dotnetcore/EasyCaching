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
            _serializer = new DefaultJsonSerializer("json", new JsonSerializerSettings());
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
        public void ReferenceLoopHandling_Test_Should_Succeed()
        {
            var serializer = new DefaultJsonSerializer("json", new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            Employee joe = new Employee { Name = "Joe User" };
            Employee mike = new Employee { Name = "Mike Manager" };
            joe.Manager = mike;
            mike.Manager = mike;

            var joe_byte = serializer.Serialize(joe);
            var joe_obj = serializer.Deserialize<Employee>(joe_byte);


            Assert.Equal(joe.Name, joe_obj.Name);
            Assert.Equal(joe.Manager, mike);
        }

        public class Employee
        {
            public string Name { get; set; }
            public Employee Manager { get; set; }
        }

        [Fact]
        public void NullValueHandling_Test_Should_Succeed()
        {
            var serializer = new DefaultJsonSerializer("json", new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            Employee joe = new Employee { Name = "Joe User" };

            var joe_byte = serializer.Serialize(joe);
            var joe_obj = serializer.Deserialize<Employee>(joe_byte);

            Assert.Null(joe.Manager);
        }
    }
}
