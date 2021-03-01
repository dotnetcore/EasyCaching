namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Serialization;
    using FakeItEasy;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class DistributedCachingProviderTest : BaseCachingProviderTest
    {
        private const string DeserializationErrorMessage = "Error on deserialization";
        private FakeLogger fakeLogger;
        
        protected virtual IEasyCachingProvider CreateProviderWithErrorOnDeserialization()
        {
            IServiceCollection services = new ServiceCollection();

            AddLoggerFactory(services);

            services.AddSingleton<IEasyCachingSerializer, SerializerWithErrorOnDeserialization>();
            
            services.AddEasyCaching(options => SetupCachingProvider(
                options, 
                providerOptions =>
                {
                    providerOptions.EnableLogging = true;
                    providerOptions.SerializerName = nameof(SerializerWithErrorOnDeserialization);
                }));
            
            return services.BuildServiceProvider().GetService<IEasyCachingProvider>();
        }

        protected void AddLoggerFactory(IServiceCollection services)
        {
            fakeLogger = new FakeLogger();
            
            var loggerFactory = A.Fake<ILoggerFactory>();
            A.CallTo(() => loggerFactory.CreateLogger(A<string>.Ignored)).Returns(fakeLogger);
            services.AddSingleton(loggerFactory);
        }
        
        [Fact]
        public void Get_With_Deserialization_Error_Should_Return_No_Value()
        {
            var cachingProvider = CreateProviderWithErrorOnDeserialization();
            cachingProvider.Set("key", "value", _defaultTs);

            var result = cachingProvider.Get<string>("key");
            
            Assert.False(result.HasValue);
            Assert.Equal(null, result.Value);
            AssertCacheMissed(cachingProvider.CacheStats);
            AssertDeserializationErrorLoggedAsWarning();
        }
        
        [Fact]
        public async void GetAsync_With_Deserialization_Error_Should_Return_No_Value()
        {
            var cachingProvider = CreateProviderWithErrorOnDeserialization();
            await cachingProvider.SetAsync("key", "value", _defaultTs);

            var result = await cachingProvider.GetAsync<string>("key");
            
            Assert.False(result.HasValue);
            Assert.Equal(null, result.Value);
            AssertCacheMissed(cachingProvider.CacheStats);
            AssertDeserializationErrorLoggedAsWarning();
        }
        
        [Fact]
        public void Get_With_Data_Retriever_And_Deserialization_Error_Should_Return_No_Value()
        {
            var cachingProvider = CreateProviderWithErrorOnDeserialization();
            cachingProvider.Set("key", "value", _defaultTs);

            var result = cachingProvider.Get("key", () => "newValue", _defaultTs);
            
            Assert.True(result.HasValue);
            Assert.Equal("newValue", result.Value);
            AssertCacheMissed(cachingProvider.CacheStats);
            AssertDeserializationErrorLoggedAsWarning();
        }
        
        [Fact]
        public async void GetAsync_With_Data_Retriever_And_Deserialization_Error_Should_Return_No_Value()
        {
            var cachingProvider = CreateProviderWithErrorOnDeserialization();
            await cachingProvider.SetAsync("key", "value", _defaultTs);

            var result = await cachingProvider.GetAsync("key", () => Task.FromResult("newValue"), _defaultTs);
            
            Assert.True(result.HasValue);
            Assert.Equal("newValue", result.Value);
            AssertCacheMissed(cachingProvider.CacheStats);
            AssertDeserializationErrorLoggedAsWarning();
        }

        private void AssertCacheMissed(CacheStats cacheStats)
        {
            Assert.Equal(0, cacheStats.GetStatistic(StatsType.Hit));
            Assert.Equal(1, cacheStats.GetStatistic(StatsType.Missed));
        }

        private void AssertDeserializationErrorLoggedAsWarning()
        {
            var loggedException = fakeLogger.LoggedExceptions.Single();
            Assert.Equal(DeserializationErrorMessage, loggedException.Exception.Message);
            Assert.Equal(LogLevel.Warning, loggedException.LogLevel);
        }
        
        private class FakeLogger : ILogger
        {
            private readonly List<(LogLevel, Exception)> _loggedExceptions = new List<(LogLevel, Exception)>();

            public IEnumerable<(LogLevel LogLevel, Exception Exception)> LoggedExceptions => _loggedExceptions;
            
            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (exception != null)
                {
                    _loggedExceptions.Add((logLevel, exception));
                }
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }
        }
        
        protected class SerializerWithErrorOnDeserialization : IEasyCachingSerializer
        {
            private static readonly byte[] SerializationResult = Encoding.UTF8.GetBytes("value");
            
            public string Name => nameof(SerializerWithErrorOnDeserialization);

            public T Deserialize<T>(byte[] bytes) => throw new Exception(DeserializationErrorMessage);

            public object Deserialize(byte[] bytes, Type type) => throw new Exception(DeserializationErrorMessage);

            public object DeserializeObject(ArraySegment<byte> value) => throw new Exception(DeserializationErrorMessage);

            public byte[] Serialize<T>(T value) => SerializationResult;

            public ArraySegment<byte> SerializeObject(object obj) => new ArraySegment<byte>(SerializationResult);
        }
    }
}