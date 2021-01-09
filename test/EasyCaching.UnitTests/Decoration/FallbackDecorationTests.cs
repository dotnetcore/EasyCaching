namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
    using FakeItEasy;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using static ServiceBuilders;

    public abstract class FallbackDecorationTests
    {
        protected const string CacheKey = "CacheKey";
        protected const string CacheValue = "CacheValue";
        protected static readonly TimeSpan Expiration = TimeSpan.FromDays(1);

        protected virtual IEasyCachingProvider CreateDecoratedProvider() =>
            CreateFakeProvider(options =>
            {
                options.ProviderFactory = CreateProvider;
                options.Decorate((name, _, cachingProvideFactory) => cachingProvideFactory
                    .WithFallback(
                        exception => exception is InvalidOperationException,
                        new NullCachingProvider(name, options)));
            });

        protected abstract IEasyCachingProvider CreateProvider();
        
        [Fact]
        public void Set_Should_Do_Nothing()
        {
            var provider = CreateDecoratedProvider();
            
            provider.Set(CacheKey, CacheValue, Expiration);

            var result = provider.Get<string>(CacheKey);
            Assert.False(result.HasValue);
            Assert.Null(result.Value);
        }
        
        [Fact]
        public async Task SetAsync_Should_Do_Nothing()
        {
            var provider = CreateDecoratedProvider();
            
            await provider.SetAsync(CacheKey, CacheValue, Expiration);

            var result = provider.Get<string>(CacheKey);
            Assert.False(result.HasValue);
            Assert.Null(result.Value);
        }
        
        [Fact]
        public void Get_Should_Return_Empty_Value()
        {
            var provider = CreateDecoratedProvider();

            var result = provider.Get<string>(CacheKey);
            
            Assert.False(result.HasValue);
            Assert.Null(result.Value);
        }
        
        [Fact]
        public async Task GetAsync_Should_Return_Empty_Value()
        {
            var provider = CreateDecoratedProvider();

            var result = await provider.GetAsync<string>(CacheKey);
            
            Assert.False(result.HasValue);
            Assert.Null(result.Value);
        }
        
        [Fact]
        public void Get_With_Data_Retriever_Should_Succeed()
        {
            var provider = CreateDecoratedProvider();

            var result = provider.Get(
                CacheKey, 
                () => CacheValue, 
                Expiration);
            
            Assert.True(result.HasValue);
            Assert.Equal(CacheValue, result.Value);
        }
        
        [Fact]
        public async Task GetAsync_With_Data_Retriever_Should_Succeed()
        {
            var provider = CreateDecoratedProvider();

            var result = await provider.GetAsync(
                CacheKey, 
                () => Task.FromResult(CacheValue), 
                Expiration);
            
            Assert.True(result.HasValue);
            Assert.Equal(CacheValue, result.Value);
        }
    }

    public class FallbackDecorationTestsWithFailOnInit : FallbackDecorationTests
    {
        protected sealed override IEasyCachingProvider CreateProvider() => throw new InvalidOperationException("Exception on init");

        [Fact]
        public void Get_CacheStats_Should_Succeed()
        {
            var provider = CreateDecoratedProvider();

            var result = provider.CacheStats;
            
            Assert.NotNull(result);
        }
    }

    public class FallbackDecorationTestsWithFailOnAnyMethod : FallbackDecorationTests
    {
        protected sealed override IEasyCachingProvider CreateProvider()
        {
            var fakeCachingProvider = A.Fake<IEasyCachingProvider>();
            A.CallTo(fakeCachingProvider).Throws(() => new InvalidOperationException("Exception on method"));
            return fakeCachingProvider;
        }
    }
}