namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core;
    using System;
    using System.Threading.Tasks;
    using Polly;

    public class EasyCachingProviderDecoratorWrapper<TProvider> : IEasyCachingProviderDecorator<TProvider> 
        where TProvider : class, IEasyCachingProviderBase
    {
        private readonly Func<TProvider> _cachingProviderFactory;
        
        public EasyCachingProviderDecoratorWrapper(Func<TProvider> cachingProviderFactory)
        {
            _cachingProviderFactory = cachingProviderFactory ?? throw new ArgumentNullException(nameof(cachingProviderFactory));
        }

        public TProvider GetCachingProvider() => _cachingProviderFactory();

        public void Execute(TProvider provider, Action<TProvider> action) => action(provider);
        public T Execute<T>(TProvider provider, Func<TProvider, T> function) => function(provider);
        public Task ExecuteAsync(TProvider provider, Func<TProvider, Task> function) => function(provider);
        public Task<T> ExecuteAsync<T>(TProvider provider, Func<TProvider, Task<T>> function) => function(provider);
    }
}