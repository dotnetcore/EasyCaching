using EasyCaching.Serialization.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace EasyCaching.UnitTests.SerializerTests
{
    public class SystemTestJsonSerializerTest : BaseSerializerTest
    {
        public SystemTestJsonSerializerTest()
        {
            _serializer = new DefaultJsonSerializer("json", new System.Text.Json.JsonSerializerOptions());
        }

        [Fact]
        public void Isuue_50_Test()
        {
            Employee joe = new Employee { Name = "Joe User" };
            Employee mike = new Employee { Name = "Mike Manager" };
            joe.Manager = mike;
            mike.Manager = mike;

            Assert.Throws<JsonException>(() => _serializer.Serialize(joe));
        }

        [Fact]
        public void ReferenceLoopHandling_Test_Should_Succeed()
        {
            var serializer = new DefaultJsonSerializer("json", new JsonSerializerOptions()
            {
                ReferenceHandler = ReferenceHandler.Preserve
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
            var serializer = new DefaultJsonSerializer("json", new JsonSerializerOptions
            {
                IgnoreNullValues = true
            });

            Employee joe = new Employee { Name = "Joe User" };

            var joe_byte = serializer.Serialize(joe);
            var joe_obj = serializer.Deserialize<Employee>(joe_byte);

            Assert.Null(joe.Manager);
        }
    }
}