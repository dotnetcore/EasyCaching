namespace EasyCaching.Decoration.Polly
{
    using EasyCaching.Core.Decoration;
    using EasyCaching.Core;
    using global::Polly;
    using System;
    using System.Threading.Tasks;

    public class EasyCachingProviderPolicyDecorator<TProvider> : IEasyCachingProviderDecorator<TProvider> 
        where TProvider : class, IEasyCachingProviderBase
    {
        private LazyWithoutExceptionCaching<TProvider> _lazyCachingProvider;
        
        private readonly Policy _initPolicy;
        private readonly Policy _syncExecutePolicy;
        private readonly AsyncPolicy _asyncExecutePolicy;
        
        public EasyCachingProviderPolicyDecorator(
            Func<TProvider> cachingProviderFactory,
            Policy initPolicy,
            Policy syncExecutePolicy,
            AsyncPolicy asyncExecutePolicy)
        {
            if (cachingProviderFactory == null) throw new ArgumentNullException(nameof(cachingProviderFactory));
            _lazyCachingProvider = new LazyWithoutExceptionCaching<TProvider>(cachingProviderFactory);
            
            _initPolicy = initPolicy ?? throw new ArgumentNullException(nameof(initPolicy));
            _syncExecutePolicy = syncExecutePolicy ?? throw new ArgumentNullException(nameof(syncExecutePolicy));
            _asyncExecutePolicy = asyncExecutePolicy ?? throw new ArgumentNullException(nameof(asyncExecutePolicy));
        }

        public TProvider GetCachingProvider()
        {
            // Micro optimization to bypass policy when provider was initialized
            return _lazyCachingProvider.Initialized 
                ? _lazyCachingProvider.Value
                : _initPolicy.Execute(() => _lazyCachingProvider.Value) ;
        }

        public void Execute(TProvider provider, Action<TProvider> action)
        {
            _syncExecutePolicy.Execute(() => action(provider));
        }

        public T Execute<T>(TProvider provider, Func<TProvider, T> function)
        {
            return _syncExecutePolicy.Execute(() => function(provider));
        }

        public Task ExecuteAsync(TProvider provider, Func<TProvider, Task> function)
        {
            return _asyncExecutePolicy.ExecuteAsync(() => function(provider));
        }

        public Task<T> ExecuteAsync<T>(TProvider provider, Func<TProvider, Task<T>> function)
        {
            return _asyncExecutePolicy.ExecuteAsync(() => function(provider));
        }
    }
}