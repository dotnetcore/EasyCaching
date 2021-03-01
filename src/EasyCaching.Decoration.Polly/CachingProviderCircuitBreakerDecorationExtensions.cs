namespace EasyCaching.Decoration.Polly
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
    using Polly;
    using System;

    public static class CachingProviderCircuitBreakerDecorationExtensions
    {
        public static IProviderOptionsWithDecorator<IEasyCachingProvider> DecorateWithCircuitBreaker(
            this IProviderOptionsWithDecorator<IEasyCachingProvider> options,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters,
            Func<Exception, bool> exceptionFilter)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedEasyCachingProvider(
                    name, 
                    cachingProviderFactory.WithCircuitBreaker(initParameters, executeParameters, exceptionFilter))
            );
        }
        
        public static IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> DecorateWithCircuitBreaker(
            this IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> options,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters,
            Func<Exception, bool> exceptionFilter)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedRedisAndEasyCachingProvider(
                    name, 
                    cachingProviderFactory.WithCircuitBreaker(initParameters, executeParameters, exceptionFilter))
            );
        }

        public static IProviderOptionsWithDecorator<IHybridCachingProvider> DecorateWithCircuitBreaker(
            this IProviderOptionsWithDecorator<IHybridCachingProvider> options,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters,
            Func<Exception, bool> exceptionFilter)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedHybridCachingProvider(
                    name, 
                    cachingProviderFactory.WithCircuitBreaker(initParameters, executeParameters, exceptionFilter))
            );
        }

        private static IEasyCachingProviderDecorator<TProvider> WithCircuitBreaker<TProvider>(
            this Func<TProvider> cachingProviderFactory,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters,
            Func<Exception, bool> exceptionFilter) where TProvider : class, IEasyCachingProviderBase
        {
            var policyBuilder = exceptionFilter.GetHandleExceptionPolicyBuilder();
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