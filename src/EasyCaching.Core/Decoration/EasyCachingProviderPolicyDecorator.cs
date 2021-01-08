namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core;
    using System;
    using System.Threading.Tasks;
    using Polly;

    public class EasyCachingProviderPolicyDecorator<TProvider> : IEasyCachingProviderDecorator<TProvider> 
        where TProvider : class, IEasyCachingProviderBase
    {
        private readonly IEasyCachingProviderDecorator<TProvider> _innerDecorator;
        private readonly object _cachingProviderCreationLock = new object();
        private TProvider _cachingProvider;
        
        private readonly Policy _initPolicy;
        private readonly Policy _syncExecutePolicy;
        private readonly AsyncPolicy _asyncExecutePolicy;
        
        public EasyCachingProviderPolicyDecorator(
            IEasyCachingProviderDecorator<TProvider> innerDecorator,
            Policy initPolicy,
            Policy syncExecutePolicy,
            AsyncPolicy asyncExecutePolicy)
        {
            _innerDecorator = innerDecorator ?? throw new ArgumentNullException(nameof(innerDecorator));
            _initPolicy = initPolicy ?? throw new ArgumentNullException(nameof(initPolicy));
            _syncExecutePolicy = syncExecutePolicy ?? throw new ArgumentNullException(nameof(syncExecutePolicy));
            _asyncExecutePolicy = asyncExecutePolicy ?? throw new ArgumentNullException(nameof(asyncExecutePolicy));
        }

        public TProvider GetCachingProvider()
        {
            if (_cachingProvider == null)
            {
                _cachingProvider = _initPolicy.Execute(() =>
                {
                    lock (_cachingProviderCreationLock)
                    {
                        return _cachingProvider ?? _innerDecorator.GetCachingProvider();
                    }
                });
            }

            return _cachingProvider;
        }

        public void Execute(TProvider provider, Action<TProvider> action)
        {
            _syncExecutePolicy.Execute(() =>
                _innerDecorator.Execute(provider, action));
        }

        public T Execute<T>(TProvider provider, Func<TProvider, T> function)
        {
            return _syncExecutePolicy.Execute(() => 
                _innerDecorator.Execute(provider, function));
        }

        public Task ExecuteAsync(TProvider provider, Func<TProvider, Task> function)
        {
            return _asyncExecutePolicy.ExecuteAsync(() => 
                _innerDecorator.Execute(provider, function));
        }

        public Task<T> ExecuteAsync<T>(TProvider provider, Func<TProvider, Task<T>> function)
        {
            return _asyncExecutePolicy.ExecuteAsync(() => 
                _innerDecorator.Execute(provider, function));
        }
    }
}