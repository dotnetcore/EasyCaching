namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
    using FakeItEasy;
    using Polly.CircuitBreaker;
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class CircuitBreakerDecorationTests
    {
        private const string CacheKey = "CacheKey";
        private const string CacheValue = "CacheValue";
        private static readonly TimeSpan Expiration = TimeSpan.FromDays(1);

        protected IEasyCachingProvider CreateDecoratedProvider() =>
            ServiceBuilders.CreateFakeProvider(options =>
            {
                options.ProviderFactory = CreateProvider;

                var circuitBreakerParameters = new CircuitBreakerParameters(
                    exceptionsAllowedBeforeBreaking: 1,
                    durationOfBreak: TimeSpan.FromMinutes(1));
                
                options.DecorateWithCircuitBreaker(
                    exception => exception is InvalidOperationException,
                    initParameters: circuitBreakerParameters,
                    executeParameters: circuitBreakerParameters);
            });

        protected abstract IEasyCachingProvider CreateProvider();
        
        [Fact]
        public void Set_Should_Break_Circuit()
        {
            var provider = CreateDecoratedProvider();

            AssertCircuitIsBroken(() => provider.Set(CacheKey, CacheValue, Expiration));
        }
        
        [Fact]
        public async Task SetAsync_Should_Break_Circuit()
        {
            var provider = CreateDecoratedProvider();
            
            await AssertCircuitIsBroken(() => provider.SetAsync(CacheKey, CacheValue, Expiration));
        }
        
        [Fact]
        public void Get_Should_Break_Circuit()
        {
            var provider = CreateDecoratedProvider();

            AssertCircuitIsBroken(() => provider.Get<string>(CacheKey));
        }
        
        [Fact]
        public async Task GetAsync_Should_Break_Circuit()
        {
            var provider = CreateDecoratedProvider();
            
            await AssertCircuitIsBroken(() => provider.GetAsync<string>(CacheKey));
        }

        protected void AssertCircuitIsBroken(Action action)
        {
            Assert.Throws<InvalidOperationException>(action);
            Assert.Throws<BrokenCircuitException>(action);
        }

        protected async Task AssertCircuitIsBroken(Func<Task> action)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(action);
            await Assert.ThrowsAsync<BrokenCircuitException>(action);
        }
    }

    public class CircuitBreakerDecorationTestsWithFailOnInit : CircuitBreakerDecorationTests
    {
        protected override IEasyCachingProvider CreateProvider() => throw new InvalidOperationException("Exception on init");

        [Fact]
        public void Get_CacheStats_Should_Break_Circuit()
        {
            var provider = CreateDecoratedProvider();

            AssertCircuitIsBroken(() =>
            {
                var cacheStats = provider.CacheStats;
            });
        }
    }

    public class CircuitBreakerDecorationTestsWithFailOnAnyMethod : CircuitBreakerDecorationTests
    {
        protected override IEasyCachingProvider CreateProvider()
        {
            var fakeCachingProvider = A.Fake<IEasyCachingProvider>();
            A.CallTo(fakeCachingProvider).Throws(() => new InvalidOperationException("Exception on method"));
            return fakeCachingProvider;
        }
    }
}