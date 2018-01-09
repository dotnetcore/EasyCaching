namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Redis;
    using FakeItEasy;
    using Microsoft.Extensions.Options;
    using System;
    using Xunit;

    public class RedisCachingProviderTest
    {
        private readonly DefaultRedisCachingProvider _provider;
        private readonly TimeSpan _defaultTs;
        private readonly IEasyCachingSerializer _serializer;

        public RedisCachingProviderTest()
        {
            RedisCacheOptions options = new RedisCacheOptions()
            {
                //Password = ""
            };

            options.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));

            var fakeOption = A.Fake<IOptions<RedisCacheOptions>>();
            _serializer = A.Fake<IEasyCachingSerializer>();
                
            A.CallTo(() => fakeOption.Value).Returns(options);

            _provider = new DefaultRedisCachingProvider(fakeOption, _serializer);
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact]
        public void Set_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(cacheValue);

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(val);
            Assert.Equal(cacheValue, val);
        }

        [Fact]
        public void Set_Object_Type_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            object cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<object>(A<byte[]>.Ignored)).Returns(cacheValue);

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<object>(cacheKey, null, _defaultTs);
            Assert.NotNull(val);
            Assert.Equal(cacheValue, val);
        }

        [Fact]
        public void Set_Value_Should_Call_Serialize()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            A.CallTo(() => _serializer.Serialize(cacheValue)).MustHaveHappened();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Set_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cachekey)
        {
            var cacheValue = "value";

            Assert.Throws<ArgumentNullException>(() => _provider.Set(cachekey, cacheValue, _defaultTs));
        }

        [Theory]
        [InlineData(null)]
        public void Set_Value_Should_Throw_ArgumentNullException_When_CacheValue_IsNullOrWhiteSpace(string cacheValue)
        {
            var cacheKey = Guid.NewGuid().ToString();

            Assert.Throws<ArgumentNullException>(() => _provider.Set(cacheKey, cacheValue, _defaultTs));
        }


        [Fact]
        public void Get_Cached_Value_Should_Call_Deserialize()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            _provider.Get<string>(cacheKey, null, _defaultTs);

            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).MustHaveHappened();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Get_Cached_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cachekey)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Get(cachekey, null, _defaultTs));
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Retriever()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            A.CallTo(() => func.Invoke()).MustHaveHappened();
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Not_Call_Deserialize()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public void Get_Not_Cached_Value_Should_Call_Serialize_When_Setting_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var func = Create_Fake_Retriever_Return_String();

            var res = _provider.Get(cacheKey, func, _defaultTs);

            A.CallTo(() => _serializer.Serialize(A<string>.Ignored)).MustHaveHappened();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Remove_Cached_Value_Should_Throw_ArgumentNullException_When_CacheKey_IsNullOrWhiteSpace(string cacheKey)
        {
            Assert.Throws<ArgumentNullException>(() => _provider.Remove(cacheKey));
        }

        [Fact]
        public void Remove_Cached_Value_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(cacheValue);

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            var valBeforeRemove = _provider.Get(cacheKey, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            _provider.Remove(cacheKey);
            var valAfterRemove = _provider.Get(cacheKey, () => "123", _defaultTs);
            Assert.Equal("123", valAfterRemove);
        }

        private Func<string> Create_Fake_Retriever_Return_String()
        {
            var func = A.Fake<Func<string>>();

            A.CallTo(() => func.Invoke()).Returns("123");

            return func;
        }
    }
}
