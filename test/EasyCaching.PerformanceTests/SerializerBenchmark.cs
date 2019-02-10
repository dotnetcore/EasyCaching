namespace EasyCaching.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using EasyCaching.Core;
    using EasyCaching.Core.Serialization;
    using EasyCaching.Serialization.Json;
    using EasyCaching.Serialization.MessagePack;
    using EasyCaching.Serialization.Protobuf;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;

    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public abstract class SerializerBenchmark
    {
        private DefaultJsonSerializer _json = new DefaultJsonSerializer(new OptionsWrapper<EasyCachingJsonSerializerOptions>(new EasyCachingJsonSerializerOptions()));
        private DefaultMessagePackSerializer _messagepack = new DefaultMessagePackSerializer();
        private DefaultProtobufSerializer _protobuf = new DefaultProtobufSerializer();
        private DefaultBinaryFormatterSerializer _binary = new DefaultBinaryFormatterSerializer();
        protected MyPoco _single;
        protected List<MyPoco> _list;
        private int _count;

        [GlobalSetup]
        public void Setup()
        {
            _count = 1000;

            _single = new MyPoco { Id = 1, Name = "123" };

            var items = new List<MyPoco>();
            for (var iter = 0; iter < _count; iter++)
            {
                items.Add(new MyPoco() { Id = iter, Name = $"name-{iter.ToString()}" });
            }

            _list = items;
        }

        [Benchmark]
        public void BinaryFormatter()
        {
            Exec(_binary);
        }

        [Benchmark]
        public void Json()
        {
            Exec(_json);
        }

        [Benchmark]
        public void MessagePack()
        {
            Exec(_messagepack);
        }

        [Benchmark]
        public void Protobuf()
        {
            Exec(_protobuf);
        }

        protected abstract void Exec(IEasyCachingSerializer serializer);
    }

    public class SerializeSingleObject2BytesBenchmark : SerializerBenchmark
    {
        protected override void Exec(IEasyCachingSerializer serializer)
        {
            var data = serializer.Serialize(_single);
            var result = serializer.Deserialize<MyPoco>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }
    }

    public class SerializeMultiObject2BytesBenchmark  : SerializerBenchmark
    {
        protected override void Exec(IEasyCachingSerializer serializer)
        {
            var data = serializer.Serialize(_list);
            var result = serializer.Deserialize<List<MyPoco>>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }
    }

    public class SerializerSingleObject2ArraySegmentBenchmark : SerializerBenchmark
    {
        protected override void Exec(IEasyCachingSerializer serializer)
        {
            var data = serializer.SerializeObject(_single);
            var result = serializer.DeserializeObject(data);
            if (result == null)
            {
                throw new Exception();
            }
        }
    }

    public class SerializerMultiObject2ArraySegmentBenchmark : SerializerBenchmark
    {
        protected override void Exec(IEasyCachingSerializer serializer)
        {
            var data = serializer.SerializeObject(_list);
            var result = serializer.DeserializeObject(data);
            if (result == null)
            {
                throw new Exception();
            }
        }
    }
}
