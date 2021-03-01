namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Decoration.Polly;
    using System;

    public static class CircuitBreakerAndFallbackDecorationBuilders
    {
        public static IEasyCachingProvider CreateDecoratedProvider(
            Func<IEasyCachingProvider> providerFactory, IEasyCachingProvider fallbackProvider) =>
            ServiceBuilders.CreateFakeProvider(options =>
            {
                options.ProviderFactory = providerFactory;

                var circuitBreakerParameters = new CircuitBreakerParameters(
                    exceptionsAllowedBeforeBreaking: 1,
                    durationOfBreak: TimeSpan.FromMinutes(1));
                
                options
                    .DecorateWithCircuitBreaker(
                        initParameters: circuitBreakerParameters,
                        executeParameters: circuitBreakerParameters,
                        exception => exception is InvalidOperationException)
                    .DecorateWithFallback(
                        (_, __) => fallbackProvider,
                        exception => exception is InvalidOperationException);
            });
        
        public static IEasyCachingProvider CreateDecoratedProviderWithBrokenCircuit(
            Func<IEasyCachingProvider> providerFactory, IEasyCachingProvider fallbackProvider)
        {
            var provider = CreateDecoratedProvider(providerFactory, fallbackProvider);
            provider.Get<string>("CacheKey");
            return provider;
        }
    }

    public class CircuitBreakerAndFallbackCachingProviderCachingProviderDecorationTestsWithFailOnInitialization : FallbackCachingProviderCachingProviderDecorationTestsWithFailOnInitialization
    {
        protected override IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProvider(CreateProvider, fallbackProvider);
    }

    public class CircuitBreakerAndFallbackCachingProviderCachingProviderDecorationTestsWithFailOnAnyMethod : FallbackCachingProviderCachingProviderDecorationTestsWithFailOnAnyMethod
    {
        protected override IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProvider(CreateProvider, fallbackProvider);
    }

    public class CircuitBreakerWithBrokenCircuitAndFallbackCachingProviderCachingProviderDecorationTestsWithFailOnInitialization : FallbackCachingProviderCachingProviderDecorationTestsWithFailOnInitialization
    {
        protected override IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProviderWithBrokenCircuit(CreateProvider, fallbackProvider);
    }

    public class CircuitBreakerWithBrokenCircuitAndFallbackCachingProviderCachingProviderDecorationTestsWithFailOnAnyMethod : FallbackCachingProviderCachingProviderDecorationTestsWithFailOnAnyMethod
    {
        protected override IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProviderWithBrokenCircuit(CreateProvider, fallbackProvider);
    }
}