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
        private readonly string _key;
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

            _provider = new DefaultRedisCachingProvider(fakeOption,_serializer);
            _key = Guid.NewGuid().ToString();
            _defaultTs = TimeSpan.FromSeconds(5);
        }

        [Fact]
        public void Set_Value_Should_Succeed()
        {
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(cacheBytes)).Returns(cacheValue);

            _provider.Set(_key, cacheValue, _defaultTs);

            var val = _provider.Get<string>(_key, null, _defaultTs);
            Assert.NotNull(val);
            Assert.Equal(cacheValue, val);
        }

        [Fact]
        public void Remove_Cached_Value_Should_Succeed()
        {
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(cacheBytes)).Returns(cacheValue);

            _provider.Set(_key, cacheValue, _defaultTs);
            var valBeforeRemove = _provider.Get(_key, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            _provider.Remove(_key);
            var valAfterRemove = _provider.Get(_key, () => "123", _defaultTs);
            Assert.Equal("123", valAfterRemove);
        }
    }
}
