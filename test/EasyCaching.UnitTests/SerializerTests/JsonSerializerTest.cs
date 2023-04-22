namespace EasyCaching.UnitTests
{
    using EasyCaching.Serialization.Json;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
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

#if NET6_0
        [Fact]
        public void DateOnly_Iss338_Test_Should_Succeed()
        {
            var serializer = new DefaultJsonSerializer("json", new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter>() { new DateOnlyJsonConverter()}
            });

            var dateTime = DateTime.Parse("2021-12-21 12:12:12");
            var date = DateOnly.FromDateTime(dateTime);

            DateOnlyModel m = new DateOnlyModel { Name = "Joe User", Date = date };

            var @byte = serializer.Serialize(m);
            var @obj = serializer.Deserialize<DateOnlyModel>(@byte);

            Assert.Equal(date, obj.Date);
        }

        public class DateOnlyModel
        {
            public string Name { get; set; }

            public DateOnly Date { get; set; }
        }


        public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
        {
            public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString("O"));
            }

            public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue,
                JsonSerializer serializer)
            {
                //return DateOnly.FromDateTime(reader.ReadAsDateTime().Value);
                return DateOnly.ParseExact((string)reader.Value, "yyyy-MM-dd");
            }
        }
#endif
    }
}
