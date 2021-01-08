namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core;
    using System;
    using System.Threading.Tasks;

    public class EasyCachingProviderFallbackDecorator<TProvider> : IEasyCachingProviderDecorator<TProvider> 
        where TProvider : class, IEasyCachingProviderBase
    {
        private readonly IEasyCachingProviderDecorator<TProvider> _innerDecorator;
        private readonly TProvider _fallbackCachingProvider;
        private readonly Func<Exception, bool> _exceptionFilter;

        public EasyCachingProviderFallbackDecorator(
            IEasyCachingProviderDecorator<TProvider> innerDecorator,
            Func<Exception, bool> exceptionFilter,
            TProvider fallbackCachingProvider)
        {
            _innerDecorator = innerDecorator ?? throw new ArgumentNullException(nameof(innerDecorator));
            _exceptionFilter = exceptionFilter ?? throw new ArgumentNullException(nameof(exceptionFilter));
            _fallbackCachingProvider = fallbackCachingProvider ?? throw new ArgumentNullException(nameof(fallbackCachingProvider));
        }
        
        public TProvider GetCachingProvider()
        {
            try
            {
                return _innerDecorator.GetCachingProvider();
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
                _innerDecorator.Execute(provider, action);
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
                return _innerDecorator.Execute(provider, function);
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
                await _innerDecorator.ExecuteAsync(provider, function);
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
                return await _innerDecorator.ExecuteAsync(provider, function);
            }
            catch (Exception e) when(_exceptionFilter(e))
            {
                return await function(_fallbackCachingProvider);
            }
        }
    }
}