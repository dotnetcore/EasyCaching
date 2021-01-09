namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
    using FakeItEasy;
    using System;

    public static class CircuitBreakerAndFallbackDecorationBuilders
    {
        public static IEasyCachingProvider CreateDecoratedProvider(Func<IEasyCachingProvider> providerFactory) =>
            ServiceBuilders.CreateFakeProvider(options =>
            {
                options.ProviderFactory = providerFactory;

                var circuitBreakerParameters = new CircuitBreakerParameters(
                    exceptionsAllowedBeforeBreaking: 1,
                    durationOfBreak: TimeSpan.FromMinutes(1));
                
                options
                    .DecorateWithCircuitBreaker(
                        exception => exception is InvalidOperationException,
                        initParameters: circuitBreakerParameters,
                        executeParameters: circuitBreakerParameters)
                    .DecorateWithFallback(
                        exception => exception is InvalidOperationException,
                        (name, _) => new NullCachingProvider(name, options));
            });
        
        public static IEasyCachingProvider CreateDecoratedProviderWithBrokenCircuit(Func<IEasyCachingProvider> providerFactory)
        {
            var provider = CreateDecoratedProvider(providerFactory);
            provider.Get<string>("CacheKey");
            return provider;
        }
    }

    public class CircuitBreakerAndFallbackDecorationTestsWithFailOnInitialization : FallbackDecorationTestsWithFailOnInitialization
    {
        protected override IEasyCachingProvider CreateDecoratedProvider() =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProvider(CreateProvider);
    }

    public class CircuitBreakerAndFallbackDecorationTestsWithFailOnAnyMethod : FallbackDecorationTestsWithFailOnAnyMethod
    {
        protected override IEasyCachingProvider CreateDecoratedProvider() =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProvider(CreateProvider);
    }

    public class CircuitBreakerWithBrokenCircuitAndFallbackDecorationTestsWithFailOnInitialization : FallbackDecorationTestsWithFailOnInitialization
    {
        protected override IEasyCachingProvider CreateDecoratedProvider() =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProviderWithBrokenCircuit(CreateProvider);
    }

    public class CircuitBreakerWithBrokenCircuitAndFallbackDecorationTestsWithFailOnAnyMethod : FallbackDecorationTestsWithFailOnAnyMethod
    {
        protected override IEasyCachingProvider CreateDecoratedProvider() =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProviderWithBrokenCircuit(CreateProvider);
    }
}