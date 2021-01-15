namespace EasyCaching.Core.Decoration
{
    using System;
    using System.Linq;

    public static class FallbackDecorationExtensions
    {
        public static IProviderOptionsWithDecorator<IEasyCachingProvider> DecorateWithFallback(
            this IProviderOptionsWithDecorator<IEasyCachingProvider> options,
            Func<Exception, bool> exceptionFilter,
            Func<string, IServiceProvider, IEasyCachingProvider> fallbackCachingProviderFactory)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedEasyCachingProvider(
                    name, 
                    cachingProviderFactory.WithFallback(
                        exceptionFilter, 
                        fallbackCachingProviderFactory(name, serviceProvider)))
            );
        }
        
        public static IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> DecorateWithFallback(
            this IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> options,
            Func<Exception, bool> exceptionFilter,
            Func<string, IServiceProvider, IRedisAndEasyCachingProvider> fallbackCachingProviderFactory)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedRedisAndEasyCachingProvider(
                    name, 
                    cachingProviderFactory.WithFallback(
                        exceptionFilter, 
                        fallbackCachingProviderFactory(name, serviceProvider)))
            );
        }
        
        public static IProviderOptionsWithDecorator<IHybridCachingProvider> DecorateWithFallback(
            this IProviderOptionsWithDecorator<IHybridCachingProvider> options,
            Func<Exception, bool> exceptionFilter,
            Func<string, IServiceProvider, IHybridCachingProvider> fallbackCachingProviderFactory)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedHybridCachingProvider(
                    name, 
                    cachingProviderFactory.WithFallback(
                        exceptionFilter, 
                        fallbackCachingProviderFactory(name, serviceProvider)))
            );
        }

        private static IEasyCachingProviderDecorator<TProvider> WithFallback<TProvider>(
            this Func<TProvider> cachingProviderFactory,
            Func<Exception, bool> exceptionFilter,
            TProvider fallbackCachingProvider) where TProvider : class, IEasyCachingProviderBase
        {
            return new EasyCachingProviderFallbackDecorator<TProvider>(
                cachingProviderFactory, 
                exceptionFilter.IncludingInnerExceptions(), 
                fallbackCachingProvider);
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