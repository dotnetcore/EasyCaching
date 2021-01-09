namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
    using FakeItEasy;
    using System;

    public static class CircuitBreakerAndFallbackDecorationBuilders
    {
        public static IEasyCachingProvider CreateDecoratedProvider(Func<IEasyCachingProvider> providerFactory) =>
            CachingProviderBuilders.CreateFakeProvider(options =>
            {
                options.ProviderFactory = providerFactory;

                var circuitBreakerParameters = new CircuitBreakerParameters(
                    exceptionsAllowedBeforeBreaking: 1,
                    durationOfBreak: TimeSpan.FromMinutes(1));
                
                options.Decorate((name, _, cachingProvideFactory) => cachingProvideFactory
                    .WithCircuitBreaker(
                        exception => exception is InvalidOperationException,
                        initParameters: circuitBreakerParameters,
                        executeParameters: circuitBreakerParameters)
                    .WithFallback(
                        exception => exception is InvalidOperationException,
                        new NullCachingProvider(name, options)));
            });
        
        public static IEasyCachingProvider CreateDecoratedProviderWithBrokenCircuit(Func<IEasyCachingProvider> providerFactory)
        {
            var provider = CreateDecoratedProvider(providerFactory);
            provider.Get<string>("CacheKey");
            return provider;
        }
    }

    public class CircuitBreakerAndFallbackDecorationTestsWithFailOnInit : FallbackDecorationTestsWithFailOnInit
    {
        protected override IEasyCachingProvider CreateDecoratedProvider() =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProvider(CreateProvider);
    }

    public class CircuitBreakerAndFallbackDecorationTestsWithFailOnAnyMethod : FallbackDecorationTestsWithFailOnAnyMethod
    {
        protected override IEasyCachingProvider CreateDecoratedProvider() =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProvider(CreateProvider);
    }

    public class CircuitBreakerWithBrokenCircuitAndFallbackDecorationTestsWithFailOnInit : FallbackDecorationTestsWithFailOnInit
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