namespace EasyCaching.PerformanceTests
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Attributes.Columns;
    using BenchmarkDotNet.Running;
    using EasyCaching.Core;
    using EasyCaching.Serialization.Json;
    using EasyCaching.Serialization.MessagePack;
    using EasyCaching.Serialization.Protobuf;
    using System;
    using System.Collections.Generic;

    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SerializerBenchmark
    {
        private DefaultJsonSerializer _json = new DefaultJsonSerializer();
        private DefaultMessagePackSerializer _messagepack = new DefaultMessagePackSerializer();
        private DefaultProtobufSerializer _protobuf = new DefaultProtobufSerializer();
        private DefaultBinaryFormatterSerializer _binary = new DefaultBinaryFormatterSerializer();
        private MyPoco _single;
        private List<MyPoco> _list;
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

        #region  single
        [Benchmark]
        public void JsonSerializer_Single()
        {
            var data = _json.Serialize(_single);
            var result = _json.Deserialize<MyPoco>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }

        [Benchmark]
        public void MsgSerializer_Single()
        {
            var data = _messagepack.Serialize(_single);
            var result = _messagepack.Deserialize<MyPoco>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }

        [Benchmark]
        public void ProSerializer_Single()
        {
            var data = _protobuf.Serialize(_single);
            var result = _protobuf.Deserialize<MyPoco>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }

        [Benchmark]
        public void BinSerializer_Single()
        {
            var data = _binary.Serialize(_single);
            var result = _binary.Deserialize<MyPoco>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }
        #endregion

        #region  multi
        [Benchmark]
        public void JsonSerializer_Multi()
        {
            var data = _json.Serialize(_list);
            var result = _json.Deserialize<List<MyPoco>>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }

        [Benchmark]
        public void MsgSerializer_Multi()
        {
            var data = _messagepack.Serialize(_list);
            var result = _messagepack.Deserialize<List<MyPoco>>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }

        [Benchmark]
        public void ProSerializer_Multi()
        {
            var data = _protobuf.Serialize(_list);
            var result = _protobuf.Deserialize<List<MyPoco>>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }

        [Benchmark]
        public void BinSerializer_Multi()
        {
            var data = _binary.Serialize(_list);
            var result = _binary.Deserialize<List<MyPoco>>(data);
            if (result == null)
            {
                throw new Exception();
            }
        }
        #endregion
    }
}
