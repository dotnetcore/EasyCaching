namespace EasyCaching.Core.Decoration
{
    using Polly;
    using System;

    public static class CircuitBreakerDecorationExtensions
    {
        public static IProviderOptionsWithDecorator<IEasyCachingProvider> DecorateWithCircuitBreaker(
            this IProviderOptionsWithDecorator<IEasyCachingProvider> options,
            Func<Exception, bool> exceptionFilter,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedEasyCachingProvider(
                    name, 
                    cachingProviderFactory.WithCircuitBreaker(exceptionFilter, initParameters, executeParameters))
            );
        }
        
        public static IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> DecorateWithCircuitBreaker(
            this IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> options,
            Func<Exception, bool> exceptionFilter,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedRedisAndEasyCachingProvider(
                    name, 
                    cachingProviderFactory.WithCircuitBreaker(exceptionFilter, initParameters, executeParameters))
            );
        }

        public static IProviderOptionsWithDecorator<IHybridCachingProvider> DecorateWithCircuitBreaker(
            this IProviderOptionsWithDecorator<IHybridCachingProvider> options,
            Func<Exception, bool> exceptionFilter,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedHybridCachingProvider(
                    name, 
                    cachingProviderFactory.WithCircuitBreaker(exceptionFilter, initParameters, executeParameters))
            );
        }

        private static IEasyCachingProviderDecorator<TProvider> WithCircuitBreaker<TProvider>(
            this Func<TProvider> cachingProviderFactory,
            Func<Exception, bool> exceptionFilter,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters) where TProvider : class, IEasyCachingProviderBase
        {
            var policyBuilder = Policy
                .Handle(exceptionFilter)
                .OrInner(exceptionFilter);
            var initPolicy = initParameters.CreatePolicy(policyBuilder);
            var syncExecutePolicy = executeParameters.CreatePolicy(policyBuilder);
            var asyncExecutePolicy = executeParameters.CreatePolicyAsync(policyBuilder);

            return new EasyCachingProviderPolicyDecorator<TProvider>(
                cachingProviderFactory,
                initPolicy,
                syncExecutePolicy,
                asyncExecutePolicy);
        }
    }
}