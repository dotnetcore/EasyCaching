namespace EasyCaching.Decoration.Polly
{
    using EasyCaching.Core.Decoration;
    using EasyCaching.Core;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class EasyCachingProviderFallbackDecorator<TProvider> : IEasyCachingProviderDecorator<TProvider> 
        where TProvider : class, IEasyCachingProviderBase
    {
        private readonly LazyWithoutExceptionCaching<TProvider> _lazyCachingProvider;
        private readonly TProvider _fallbackCachingProvider;
        private readonly Func<Exception, bool> _exceptionFilter;

        public EasyCachingProviderFallbackDecorator(
            Func<TProvider> cachingProviderFactory,
            TProvider fallbackCachingProvider,
            Func<Exception, bool> exceptionFilter)
        {
            if (cachingProviderFactory == null) throw new ArgumentNullException(nameof(cachingProviderFactory));
            _lazyCachingProvider = new LazyWithoutExceptionCaching<TProvider>(cachingProviderFactory);
            
            _exceptionFilter = exceptionFilter ?? (_ => true);
            _fallbackCachingProvider = fallbackCachingProvider ?? throw new ArgumentNullException(nameof(fallbackCachingProvider));
        }
        
        public TProvider GetCachingProvider()
        {
            try
            {
                return _lazyCachingProvider.Value;
            }
            catch (Exception e) when (_exceptionFilter(e))
            {
                return _fallbackCachingProvider;
            }
        }

        public void Execute(TProvider provider, Action<TProvider> action)
        {
            try
            {
                action(provider);
            }
            catch (Exception e) when(_exceptionFilter(e))
            {
                action(_fallbackCachingProvider);
            }
        }

        public T Execute<T>(TProvider provider, Func<TProvider, T> function)
        {
            try
            {
                return function(provider);
            }
            catch (Exception e) when(_exceptionFilter(e))
            {
                return function(_fallbackCachingProvider);
            }
        }

        public async Task ExecuteAsync(TProvider provider, Func<TProvider, Task> function)
        {
            try
            {
                await function(provider);
            }
            catch (Exception e) when(_exceptionFilter(e))
            {
                await function(_fallbackCachingProvider);
            }
        }

        public async Task<T> ExecuteAsync<T>(TProvider provider, Func<TProvider, Task<T>> function)
        {
            try
            {
                return await function(provider);
            }
            catch (Exception e) when(_exceptionFilter(e))
            {
                return await function(_fallbackCachingProvider);
            }
        }
    }
}