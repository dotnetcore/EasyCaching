namespace EasyCaching.Decoration.Polly
{
    using EasyCaching.Core;
    using EasyCaching.Core.Decoration;
    using System;
    using System.Linq;

    public static class CachingProviderFallbackDecorationExtensions
    {
        public static IProviderOptionsWithDecorator<IEasyCachingProvider> DecorateWithFallback(
            this IProviderOptionsWithDecorator<IEasyCachingProvider> options,
            Func<string, IServiceProvider, IEasyCachingProvider> fallbackCachingProviderFactory,
            Func<Exception, bool> exceptionFilter)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedEasyCachingProvider(
                    name, 
                    cachingProviderFactory.WithFallback(
                        fallbackCachingProviderFactory(name, serviceProvider),
                        exceptionFilter))
            );
        }
        
        public static IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> DecorateWithFallback(
            this IProviderOptionsWithDecorator<IRedisAndEasyCachingProvider> options,
            Func<string, IServiceProvider, IRedisAndEasyCachingProvider> fallbackCachingProviderFactory,
            Func<Exception, bool> exceptionFilter)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedRedisAndEasyCachingProvider(
                    name, 
                    cachingProviderFactory.WithFallback( 
                        fallbackCachingProviderFactory(name, serviceProvider),
                        exceptionFilter))
            );
        }
        
        public static IProviderOptionsWithDecorator<IHybridCachingProvider> DecorateWithFallback(
            this IProviderOptionsWithDecorator<IHybridCachingProvider> options,
            Func<string, IServiceProvider, IHybridCachingProvider> fallbackCachingProviderFactory,
            Func<Exception, bool> exceptionFilter)
        {
            return options.Decorate((name, serviceProvider, cachingProviderFactory) =>
                () => new DecoratedHybridCachingProvider(
                    name, 
                    cachingProviderFactory.WithFallback(
                        fallbackCachingProviderFactory(name, serviceProvider),
                        exceptionFilter))
            );
        }

        private static IEasyCachingProviderDecorator<TProvider> WithFallback<TProvider>(
            this Func<TProvider> cachingProviderFactory,
            TProvider fallbackCachingProvider,
            Func<Exception, bool> exceptionFilter) where TProvider : class, IEasyCachingProviderBase
        {
            return new EasyCachingProviderFallbackDecorator<TProvider>(
                cachingProviderFactory, 
                fallbackCachingProvider, 
                exceptionFilter?.IncludingInnerExceptions());
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