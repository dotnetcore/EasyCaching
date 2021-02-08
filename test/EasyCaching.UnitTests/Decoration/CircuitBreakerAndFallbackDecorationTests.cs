namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
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

    public class CircuitBreakerAndFallbackDecorationTestsWithFailOnInitialization : FallbackDecorationTestsWithFailOnInitialization
    {
        protected override IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProvider(CreateProvider, fallbackProvider);
    }

    public class CircuitBreakerAndFallbackDecorationTestsWithFailOnAnyMethod : FallbackDecorationTestsWithFailOnAnyMethod
    {
        protected override IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProvider(CreateProvider, fallbackProvider);
    }

    public class CircuitBreakerWithBrokenCircuitAndFallbackDecorationTestsWithFailOnInitialization : FallbackDecorationTestsWithFailOnInitialization
    {
        protected override IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProviderWithBrokenCircuit(CreateProvider, fallbackProvider);
    }

    public class CircuitBreakerWithBrokenCircuitAndFallbackDecorationTestsWithFailOnAnyMethod : FallbackDecorationTestsWithFailOnAnyMethod
    {
        protected override IEasyCachingProvider CreateDecoratedProvider(IEasyCachingProvider fallbackProvider) =>
            CircuitBreakerAndFallbackDecorationBuilders.CreateDecoratedProviderWithBrokenCircuit(CreateProvider, fallbackProvider);
    }
}