using System;
using EasyCaching.Serialization.MemoryPack;
using MemoryPack;
using Xunit;

namespace EasyCaching.UnitTests;


public class MemoryPackSerializerTest : BaseSerializerTest
{
    public MemoryPackSerializerTest()
    {
        _serializer = new DefaultMemoryPackSerializer("mempack");
    }

    //This should be overrided becuse it is not supported by memory-pack
    protected override void DeserializeObject_should_Succeed()
    {
        Person input = new("test", "test1");
        var serialized = _serializer.Serialize<Person>(input);

        Assert.Throws<NotImplementedException>(() =>
        {
            _serializer.DeserializeObject(new System.ArraySegment<byte>(serialized));
        });
    }

    [Fact]
    public void GivenSampleRecord_ShouldSerializeAndDeserializeSuccessfuly()
    {
        Person input = new("test", "test1");
        var bytes = _serializer.Serialize(input);

        Person output = _serializer.Deserialize<Person>(bytes);

        Assert.Equal(input, output);
    }

    [Fact]
    public void GivenSampleRecord_ShouldHandleNestedObjectSuccessfuly()
    {
        NestedPerson item1 = new() { Name = "test", Lastname = "test1" };
        NestedPerson expected = new() { Name = "test2", Lastname = "test3", Inner = item1 };

        var bytes = _serializer.Serialize(expected);

        NestedPerson output = _serializer.Deserialize<NestedPerson>(bytes);

        Assert.Equal(expected, output);
    }

    [Fact]
    public void GivenSampleInput_ShouldHandleCircularRefSuccessfuly()
    {
        CircularPerson person = new CircularPerson()
        {
            Name = "test"
        };

        person.Self = person;

        var bytes = _serializer.Serialize(person);
        var output = _serializer.Deserialize<CircularPerson>(bytes);

        bool expected =
            person.Name == output.Name &&
            output.Self == output &&
            output.Name == output.Self.Name;

        Assert.True(expected);
    }
}

#region Test Models
[MemoryPackable(GenerateType.CircularReference)]
internal partial class CircularPerson
{
    [MemoryPackOrder(0)]
    public string Name { set; get; }
    [MemoryPackOrder(1)]
    public CircularPerson Self { set; get; }
}

[MemoryPackable]
internal partial record struct Person(string Name, string Lastname);

[MemoryPackable]
internal partial record class NestedPerson
{
    public string Name { set; get; }

    public string Lastname { set; get; }

    public NestedPerson Inner { set; get; }
}
#endregion
