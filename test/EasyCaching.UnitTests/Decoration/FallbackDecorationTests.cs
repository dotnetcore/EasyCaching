namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
    using FakeItEasy;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using static ServiceBuilders;

    public abstract class FallbackDecorationTests
    {
        private const string CacheKey = "CacheKey";
        private const string CacheValue = "CacheValue";
        private static readonly TimeSpan Expiration = TimeSpan.FromDays(1);

        protected virtual IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CreateFakeProvider(options =>
            {
                options.ProviderFactory = CreateProvider;
                options.DecorateWithFallback(
                    exception => exception is InvalidOperationException,
                    (_, __) => fallbackProvider);
            });

        protected abstract IEasyCachingProvider CreateProvider();
        
        [Fact]
        public void Set_Should_Call_Fallback()
        {
            var fallback = A.Fake<IEasyCachingProvider>();
            var provider = CreateDecoratedProvider(fallback);
            
            provider.Set(CacheKey, CacheValue, Expiration);

            A.CallTo(() => fallback.Set(CacheKey, CacheValue, Expiration)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public async Task SetAsync_Should_Call_Fallback()
        {
            var fallback = A.Fake<IEasyCachingProvider>();
            var provider = CreateDecoratedProvider(fallback);
            
            await provider.SetAsync(CacheKey, CacheValue, Expiration);
            
            A.CallTo(() => fallback.SetAsync(CacheKey, CacheValue, Expiration)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void Get_Should_Call_Fallback()
        {
            var fallback = CreateFake<IEasyCachingProvider>(fake => fake
                .CallsTo(x => x.Get<string>(CacheKey))
                .Returns(new CacheValue<string>(CacheValue, true))
            );
            var provider = CreateDecoratedProvider(fallback);

            var result = provider.Get<string>(CacheKey);
            
            Assert.True(result.HasValue);
            Assert.Equal(CacheValue, result.Value);
        }
        
        [Fact]
        public async Task GetAsync_Should_Call_Fallback()
        {
            var fallback = CreateFake<IEasyCachingProvider>(fake => fake
                .CallsTo(x => x.GetAsync<string>(CacheKey))
                .Returns(new CacheValue<string>(CacheValue, true)));
            var provider = CreateDecoratedProvider(fallback);

            var result = await provider.GetAsync<string>(CacheKey);
            
            Assert.True(result.HasValue);
            Assert.Equal(CacheValue, result.Value);
        }
        
        [Fact]
        public void Get_With_Data_Retriever_Should_Call_Fallback()
        {
            var fallback = CreateFake<IEasyCachingProvider>(fake => fake
                .CallsTo(x => x.Get(CacheKey, A<Func<string>>.Ignored, Expiration))
                .Returns(new CacheValue<string>(CacheValue, true))
            );
            var provider = CreateDecoratedProvider(fallback);

            var result = provider.Get(
                CacheKey, 
                () => string.Empty, 
                Expiration);
            
            Assert.True(result.HasValue);
            Assert.Equal(CacheValue, result.Value);
        }
        
        [Fact]
        public async Task GetAsync_With_Data_Retriever_Should_Call_Fallback()
        {
            var fallback = CreateFake<IEasyCachingProvider>(fake => fake
                .CallsTo(x => x.GetAsync(CacheKey, A<Func<Task<string>>>.Ignored, Expiration))
                .Returns(Task.FromResult(new CacheValue<string>(CacheValue, true)))
            );
            var provider = CreateDecoratedProvider(fallback);

            var result = await provider.GetAsync(
                CacheKey, 
                () => Task.FromResult(string.Empty), 
                Expiration);
            
            Assert.True(result.HasValue);
            Assert.Equal(CacheValue, result.Value);
        }
    }

    public class FallbackDecorationTestsWithFailOnInitialization : FallbackDecorationTests
    {
        protected sealed override IEasyCachingProvider CreateProvider() => throw new InvalidOperationException("Exception on init");

        [Fact]
        public void Get_IsDistributedCache_Should_Call_Fallback()
        {
            var fallback = CreateFake<IEasyCachingProvider>(fake => fake
                .CallsTo(x => x.IsDistributedCache)
                .Returns(true)
            );
            var provider = CreateDecoratedProvider(fallback);

            Assert.True(provider.IsDistributedCache);
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