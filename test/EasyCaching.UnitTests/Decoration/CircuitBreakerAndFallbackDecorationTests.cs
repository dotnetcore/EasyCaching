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
                        exception => exception is InvalidOperationException,
                        initParameters: circuitBreakerParameters,
                        executeParameters: circuitBreakerParameters)
                    .DecorateWithFallback(
                        exception => exception is InvalidOperationException,
                        (_, __) => fallbackProvider);
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