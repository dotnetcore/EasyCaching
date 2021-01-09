namespace EasyCaching.Core.Decoration
{
    using Polly;
    using System;
    using System.Linq;

    /// <summary>
    /// Delegate for decoration of caching provider factory
    /// </summary>
    /// <param name="name">Caching provider name</param>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="cachingProviderFactory">Initial caching provider factory</param>
    /// <typeparam name="TProvider">Caching provider interface (IEasyCachingProvider, IHybridCachingProvider or IRedisAndEasyCachingProvider)</typeparam>
    /// <returns>Decorated caching provider factory</returns>
    public delegate Func<TProvider> ProviderFactoryDecorator<TProvider>(
        string name, IServiceProvider serviceProvider, Func<TProvider> cachingProviderFactory)
        where TProvider : IEasyCachingProviderBase;
    

    /// <summary>
    /// Factory for creating caching provider decorator
    /// </summary>
    /// <param name="name">Caching provider name</param>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="cachingProviderFactory">Caching provider factory</param>
    /// <typeparam name="TProvider">Caching provider interface (IEasyCachingProvider, IHybridCachingProvider or IRedisAndEasyCachingProvider)</typeparam>
    /// <returns>Caching provider decorator</returns>
    public delegate IEasyCachingProviderDecorator<TProvider> ProviderDecoratorFactory<TProvider>(
        string name, IServiceProvider serviceProvider, Func<TProvider> cachingProviderFactory)
        where TProvider : IEasyCachingProviderBase;
    
    public static class DecorationExtensions
    {
        public static TProvider CreateDecoratedProvider<TProvider>(
            this IProviderOptionsWithDecorator<TProvider> options,
            string name,
            IServiceProvider serviceProvider,
            Func<TProvider> cachingProviderFactory) where TProvider : class, IEasyCachingProviderBase
        {
            if (options.ProviderFactoryDecorator == null)
            {
                return cachingProviderFactory();
            }
            else
            {
                var decoratedProviderFactory = options.ProviderFactoryDecorator(name, serviceProvider, cachingProviderFactory);
                return decoratedProviderFactory();
            }
        }
        
        public static IProviderOptionsWithDecorator<TProvider> Decorate<TProvider>(
            this IProviderOptionsWithDecorator<TProvider> options,
            ProviderFactoryDecorator<TProvider> factoryDecorator) where TProvider : class, IEasyCachingProviderBase
        {
            if (options.ProviderFactoryDecorator == null)
            {
                options.ProviderFactoryDecorator = factoryDecorator;
            }
            else
            {
                var existingFactoryDecorator = factoryDecorator;
                options.ProviderFactoryDecorator = (name, serviceProvider, cachingProviderFactory) =>
                {
                    var factoryDecoratedWithExistingDecorator = existingFactoryDecorator(name, serviceProvider, cachingProviderFactory);
                    return factoryDecorator(name, serviceProvider, factoryDecoratedWithExistingDecorator);
                };
            }

            return options;
        }

        public static IProviderOptionsWithDecorator<IEasyCachingProvider> Decorate(
            this IProviderOptionsWithDecorator<IEasyCachingProvider> options,
            ProviderDecoratorFactory<IEasyCachingProvider> decoratorFactory)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedEasyCachingProvider(
                    name, 
                    decoratorFactory(name, serviceProvider, cachingProviderFactory))
            );
        }

        public static IProviderOptionsWithDecorator<IHybridCachingProvider> Decorate(
            this IProviderOptionsWithDecorator<IHybridCachingProvider> options,
            ProviderDecoratorFactory<IHybridCachingProvider> decoratorFactory)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedHybridCachingProvider(
                    name, 
                    decoratorFactory(name, serviceProvider, cachingProviderFactory))
            );
        }

        public static IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> Decorate(
            this IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> options,
            ProviderDecoratorFactory<IRedisAndEasyCachingProvider> decoratorFactory)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedRedisAndEasyCachingProvider(
                    name, 
                    decoratorFactory(name, serviceProvider, cachingProviderFactory))
            );
        }
        
        private static IEasyCachingProviderDecorator<TProvider> AsDecorator<TProvider>(
            this Func<TProvider> cachingProviderFactory) where TProvider : class, IEasyCachingProviderBase
        {
            return new EasyCachingProviderDecoratorWrapper<TProvider>(cachingProviderFactory);
        }

        public static IEasyCachingProviderDecorator<TProvider> WithFallback<TProvider>(
            this Func<TProvider> cachingProviderFactory,
            Func<Exception, bool> exceptionFilter,
            TProvider fallbackCachingProvider) where TProvider : class, IEasyCachingProviderBase
        {
            return cachingProviderFactory
                .AsDecorator()
                .WithFallback(exceptionFilter, fallbackCachingProvider);
        }

        public static IEasyCachingProviderDecorator<TProvider> WithFallback<TProvider>(
            this IEasyCachingProviderDecorator<TProvider> innerDecorator,
            Func<Exception, bool> exceptionFilter,
            TProvider fallbackCachingProvider) where TProvider : class, IEasyCachingProviderBase
        {
            return new EasyCachingProviderFallbackDecorator<TProvider>(
                innerDecorator, 
                exceptionFilter.IncludingInnerExceptions(), 
                fallbackCachingProvider);
        }

        public static IEasyCachingProviderDecorator<TProvider> WithCircuitBreaker<TProvider>(
            this Func<TProvider> cachingProviderFactory,
            Func<Exception, bool> exceptionFilter,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters) where TProvider : class, IEasyCachingProviderBase
        {
            return cachingProviderFactory
                .AsDecorator()
                .WithCircuitBreaker(exceptionFilter, initParameters, executeParameters);
        }

        public static IEasyCachingProviderDecorator<TProvider> WithCircuitBreaker<TProvider>(
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