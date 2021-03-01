namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
    using EasyCaching.Decoration.Polly;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using static ServiceBuilders;

    public class FallbackCachingProviderDecorationTestsWithFailOnFirstInitialization
    {
        private int _initAttempt;
        
        private const string CacheKey = "CacheKey";
        private const string CacheValue = "CacheValue";
        private static readonly TimeSpan Expiration = TimeSpan.FromDays(1);

        private IEasyCachingProvider CreateDecoratedProvider() =>
            CreateFakeProvider(options =>
            {
                options.ProviderFactory = CreateProvider;
                options.DecorateWithFallback(
                    (name, _) => new NullCachingProvider(name, options),
                    exception => exception is InvalidOperationException);
            });
        
        private IEasyCachingProvider CreateProvider()
        {
            _initAttempt++;
            if (_initAttempt == 1)
            {
                throw new InvalidOperationException("Exception on init");
            }
            else
            {
                return CreateEasyCachingProvider(options => options.UseInMemory());
            }
        }
        
        [Fact]
        public void Set_Should_Succeed_On_Second_Attempt()
        {
            var provider = CreateDecoratedProvider();
            
            
            provider.Set(CacheKey, CacheValue, Expiration);

            var cachedValue = provider.Get<string>(CacheKey);
            Assert.False(cachedValue.HasValue);
            Assert.Null(cachedValue.Value);
            
            
            provider.Set(CacheKey, CacheValue, Expiration);

            cachedValue = provider.Get<string>(CacheKey);
            Assert.True(cachedValue.HasValue);
            Assert.Equal(CacheValue, cachedValue.Value);
        }
        
        [Fact]
        public async Task SetAsync_Should_Succeed_On_Second_Attempt()
        {
            var provider = CreateDecoratedProvider();
            
            
            await provider.SetAsync(CacheKey, CacheValue, Expiration);

            var cachedValue = provider.Get<string>(CacheKey);
            Assert.False(cachedValue.HasValue);
            Assert.Null(cachedValue.Value);
            
            
            await provider.SetAsync(CacheKey, CacheValue, Expiration);

            cachedValue = provider.Get<string>(CacheKey);
            Assert.True(cachedValue.HasValue);
            Assert.Equal(CacheValue, cachedValue.Value);
        }
        
        [Fact]
        public void Get_With_Data_Retriever_Should_Save_Value_On_Second_Attempt()
        {
            var provider = CreateDecoratedProvider();

            
           provider.Get(
                CacheKey, 
                () => CacheValue, 
                Expiration);
            
            var cachedValue = provider.Get<string>(CacheKey);
            Assert.False(cachedValue.HasValue);
            Assert.Null(cachedValue.Value);

            
            provider.Get(
                CacheKey, 
                () => CacheValue, 
                Expiration);
            
            cachedValue = provider.Get<string>(CacheKey);
            Assert.True(cachedValue.HasValue);
            Assert.Equal(CacheValue, cachedValue.Value);
        }
        
        [Fact]
        public async Task GetAsync_With_Data_Retriever_Should_Save_Value_On_Second_Attempt()
        {
            var provider = CreateDecoratedProvider();

            
            await provider.GetAsync(
                CacheKey, 
                () => Task.FromResult(CacheValue), 
                Expiration);
            
            var cachedValue = provider.Get<string>(CacheKey);
            Assert.False(cachedValue.HasValue);
            Assert.Null(cachedValue.Value);

            
            await provider.GetAsync(
                CacheKey, 
                () => Task.FromResult(CacheValue),
                Expiration);
            
            cachedValue = provider.Get<string>(CacheKey);
            Assert.True(cachedValue.HasValue);
            Assert.Equal(CacheValue, cachedValue.Value);
        }
    }
}