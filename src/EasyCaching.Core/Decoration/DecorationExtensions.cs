namespace EasyCaching.Core.Decoration
{
    using Polly;
    using System;
    using System.Linq;

    public static class DecorationExtensions
    {
        public static IEasyCachingProvider CreateDecoratedProvider(
            this IProviderOptionsWithDecorator<IEasyCachingProvider> options,
            string name,
            IServiceProvider serviceProvider,
            Func<IEasyCachingProvider> cachingProviderFactory) 
        {
            if (options.ProviderDecoratorFactory == null)
            {
                return cachingProviderFactory();
            }
            else
            {
                var decorator = options.ProviderDecoratorFactory(name, serviceProvider, cachingProviderFactory);
                return new DecoratedEasyCachingProvider(name, decorator);
            }
        }
        
        public static IRedisAndEasyCachingProvider CreateDecoratedProvider(
            this IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> options,
            string name,
            IServiceProvider serviceProvider,
            Func<IRedisAndEasyCachingProvider> cachingProviderFactory) 
        {
            if (options.ProviderDecoratorFactory == null)
            {
                return cachingProviderFactory();
            }
            else
            {
                var decorator = options.ProviderDecoratorFactory(name, serviceProvider, cachingProviderFactory);
                return new DecoratedRedisAndEasyCachingProvider(name, decorator);
            }
        }
        
        public static IEasyCachingProviderDecorator<TProvider> DecorateWith<TProvider>(
            this Func<TProvider> cachingProviderFactory) where TProvider : class, IEasyCachingProviderBase
        {
            return new EasyCachingProviderDecoratorWrapper<TProvider>(cachingProviderFactory);
        }
        
        public static IEasyCachingProviderDecorator<TProvider> Fallback<TProvider>(
            this IEasyCachingProviderDecorator<TProvider> innerDecorator,
            Func<Exception, bool> exceptionFilter,
            TProvider fallbackCachingProvider) where TProvider : class, IEasyCachingProviderBase
        {
            return new EasyCachingProviderFallbackDecorator<TProvider>(
                innerDecorator, 
                exceptionFilter.IncludingInnerExceptions(), 
                fallbackCachingProvider);
        }
        
        public static IEasyCachingProviderDecorator<TProvider> CircuitBreaker<TProvider>(
            this IEasyCachingProviderDecorator<TProvider> innerDecorator,
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
                innerDecorator, initPolicy, syncExecutePolicy, asyncExecutePolicy);
        }

        private static Func<Exception, bool> IncludingInnerExceptions(this Func<Exception, bool> exceptionFilter)
        {
            return exception =>
            {
                if (exceptionFilter(exception))
                {
                    return true;
                }
                else if (exception is AggregateException aggregateException)
                {
                    return aggregateException.Flatten().InnerExceptions.Any(exceptionFilter);
                }
                else
                {
                    return HandleInnerNested(exceptionFilter, exception);
                }
            };
        }

        private static bool HandleInnerNested(Func<Exception, bool> exceptionFilter, Exception current)
        {
            if (current == null) return false;
            else if (exceptionFilter(current)) return true;
            else return HandleInnerNested(exceptionFilter, current.InnerException);
        }
    }
}