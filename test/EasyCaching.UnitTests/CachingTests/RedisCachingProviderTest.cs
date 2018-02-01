namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
    using EasyCaching.Redis;
    using EasyCaching.Serialization.MessagePack;
    using FakeItEasy;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public class RedisCachingProviderTest : BaseCachingProviderTest
    {        
        private readonly IEasyCachingSerializer _serializer;

        public RedisCachingProviderTest()
        {
            RedisCacheOptions options = new RedisCacheOptions()
            {
                //Password = ""
            };

            options.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));

            var fakeOption = A.Fake<IOptions<RedisCacheOptions>>();

            A.CallTo(() => fakeOption.Value).Returns(options);

            var fakeDbProvider = A.Fake<RedisDatabaseProvider>(option => option.WithArgumentsForConstructor(new object[] { fakeOption }));

            _serializer = A.Fake<IEasyCachingSerializer>();

            _provider = new DefaultRedisCachingProvider(fakeDbProvider, _serializer);
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
            Assert.Equal(cacheValue, val.Value);
        }

        [Fact]
        public async Task Set_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(cacheValue);

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var val = await _provider.GetAsync<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(val);
            Assert.Equal(cacheValue, val.Value);
        }
          
        [Fact]
        public void Set_Value_Should_Call_Serialize()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            A.CallTo(() => _serializer.Serialize(cacheValue)).MustHaveHappened();
        }     

        [Fact]
        public async Task Set_Value_Async_Should_Call_Serialize()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            A.CallTo(() => _serializer.Serialize(cacheValue)).MustHaveHappened();
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

        [Fact]
        public async Task Get_Cached_Value_Async_Should_Call_Deserialize()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            await _provider.GetAsync<string>(cacheKey, null, _defaultTs);

            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).MustHaveHappened();
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
        public async Task Get_Not_Cached_Value_Async_Should_Not_Call_Deserialize()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var func = Create_Fake_Retriever_Return_String_Async();

            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

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

        [Fact]
        public async Task Get_Not_Cached_Value_Async_Should_Call_Serialize_When_Setting_Value()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var func = Create_Fake_Retriever_Return_String_Async();

            var res = await _provider.GetAsync(cacheKey, func, _defaultTs);

            A.CallTo(() => _serializer.Serialize(A<string>.Ignored)).MustHaveHappened();
        }        

        [Fact]
        public void Get_Cached_Value_Without_Retriever_Should_Call_Deserialize()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            _provider.Set(cacheKey, cacheValue, _defaultTs);
            _provider.Get<string>(cacheKey);

            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task Get_Cached_Value_Without_Retriever_Async_Should_Call_Deserialize()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            await _provider.GetAsync<string>(cacheKey);

            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).MustHaveHappened();
        }
             
        [Fact]
        public void Get_Not_Cached_Value_Without_Retriever_Should_Not_Call_Deserialize()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var res = _provider.Get<string>(cacheKey);

            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Get_Not_Cached_Value_Without_Retriever_Async_Should_Not_Call_Deserialize()
        {
            var cacheKey = Guid.NewGuid().ToString();

            var res = await _provider.GetAsync<string>(cacheKey);

            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).MustNotHaveHappened();
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
            var valBeforeRemove = _provider.Get<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            _provider.Remove(cacheKey);
            var valAfterRemove = _provider.Get(cacheKey, () => "123", _defaultTs);
            Assert.Equal("123", valAfterRemove.Value);
        }

        [Fact]
        public async Task Remove_Cached_Value_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(cacheValue);

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);
            var valBeforeRemove = await _provider.GetAsync<string>(cacheKey, null, _defaultTs);
            Assert.NotNull(valBeforeRemove);

            await _provider.RemoveAsync(cacheKey);
            var valAfterRemove = await _provider.GetAsync(cacheKey,async () => await Task.FromResult("123"), _defaultTs);
            Assert.Equal("123", valAfterRemove.Value);
        }

        [Fact]
        public void Refresh_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(cacheValue);

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var tmp = _provider.Get<string>(cacheKey);
            Assert.Equal("value", tmp.Value);

            var newValue = "NewValue";

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(newValue);

            _provider.Refresh(cacheKey, newValue, _defaultTs);

            var act = _provider.Get<string>(cacheKey);

            Assert.Equal("NewValue", act.Value);
        }

        [Fact]
        public async Task Refresh_Async_Should_Succeed()
        {
            var cacheKey = Guid.NewGuid().ToString();
            var cacheValue = "value";
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(cacheValue);

            await _provider.SetAsync(cacheKey, cacheValue, _defaultTs);

            var tmp = await _provider.GetAsync<string>(cacheKey);
            Assert.Equal("value", tmp.Value);

            var newValue = "NewValue";

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(newValue);

            await _provider.RefreshAsync(cacheKey, newValue, _defaultTs);

            var act = await _provider.GetAsync<string>(cacheKey);

            Assert.Equal("NewValue", act.Value);
        }
           

        [Fact]
        public void RemoveByPrefix_Should_Succeed()
        {
            SetCacheItem("demo:1", "1");
            SetCacheItem("demo:2", "2");
            SetCacheItem("demo:3", "3");
            SetCacheItem("demo:4", "4");

            _provider.RemoveByPrefix("demo");

            var demo1 = _provider.Get<string>("demo:1");
            var demo2 = _provider.Get<string>("demo:2");
            var demo3 = _provider.Get<string>("demo:3");
            var demo4 = _provider.Get<string>("demo:4");

            Assert.False(demo1.HasValue);
            Assert.False(demo2.HasValue);
            Assert.False(demo3.HasValue);
            Assert.False(demo4.HasValue);
        }

        [Fact]
        public async Task RemoveByPrefixAsync_Should_Succeed()
        {
            SetCacheItem("demo:1", "1");
            SetCacheItem("demo:2", "2");
            SetCacheItem("demo:3", "3");
            SetCacheItem("demo:4", "4");

            await _provider.RemoveByPrefixAsync("demo");

            var demo1 = _provider.Get<string>("demo:1");
            var demo2 = _provider.Get<string>("demo:2");
            var demo3 = _provider.Get<string>("demo:3");
            var demo4 = _provider.Get<string>("demo:4");

            Assert.False(demo1.HasValue);
            Assert.False(demo2.HasValue);
            Assert.False(demo3.HasValue);
            Assert.False(demo4.HasValue);
        }

        private void SetCacheItem(string cacheKey, string cacheValue)
        {            
            var cacheBytes = new byte[] { 0x01 };

            A.CallTo(() => _serializer.Serialize(cacheValue)).Returns(cacheBytes);
            A.CallTo(() => _serializer.Deserialize<string>(A<byte[]>.Ignored)).Returns(cacheValue);

            _provider.Set(cacheKey, cacheValue, _defaultTs);

            var val = _provider.Get<string>(cacheKey);
            Assert.Equal(cacheValue, val.Value);
        }
    }
}
