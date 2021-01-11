namespace EasyCaching.Core.Decoration
{
    using System;
    using System.Threading.Tasks;

    public interface IEasyCachingProviderDecorator<TProvider> where TProvider : IEasyCachingProviderBase
    {
        TProvider GetCachingProvider();
        void Execute(TProvider provider, Action<TProvider> action);
        T Execute<T>(TProvider provider, Func<TProvider, T> function);
        Task ExecuteAsync(TProvider provider, Func<TProvider, Task> function);
        Task<T> ExecuteAsync<T>(TProvider provider, Func<TProvider, Task<T>> function);
    }

    public static class EasyCachingProviderDecoratorExtensions
    {
        public static void Execute<TProvider> (this IEasyCachingProviderDecorator<TProvider> decorator, Action<TProvider> action)
            where TProvider : class, IEasyCachingProviderBase
        {
            var provider = decorator.GetCachingProvider();
            decorator.Execute(provider, action);
        }

        public static T Execute<TProvider, T>(this IEasyCachingProviderDecorator<TProvider> decorator, Func<TProvider, T> function)
            where TProvider : class, IEasyCachingProviderBase
        {
            var provider = decorator.GetCachingProvider();
            return decorator.Execute(provider, function);
        }

        public static async Task ExecuteAsync<TProvider>(this IEasyCachingProviderDecorator<TProvider> decorator, Func<TProvider, Task> function)
            where TProvider : class, IEasyCachingProviderBase
        {
            var provider = decorator.GetCachingProvider();
            await decorator.ExecuteAsync(provider, function);
        }

        public static async Task<T> ExecuteAsync<TProvider, T>(this IEasyCachingProviderDecorator<TProvider> decorator, Func<TProvider, Task<T>> function)
            where TProvider : class, IEasyCachingProviderBase
        {
            var provider = decorator.GetCachingProvider();
            return await decorator.ExecuteAsync(provider, function);
        }
    }
}