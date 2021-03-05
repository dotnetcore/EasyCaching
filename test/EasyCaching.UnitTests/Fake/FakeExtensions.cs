namespace EasyCaching.UnitTests.Fake
{
    using EasyCaching.Core;
    using FakeItEasy;
    using FakeItEasy.Configuration;
    using System;
    using System.Threading.Tasks;

    public static class FakeExtensions
    {
        public static IReturnValueArgumentValidationConfiguration<CacheValue<T>> CallToGetWithDataRetriever<T>(this IEasyCachingProviderBase cachingProvider) =>
            A.CallTo(() => cachingProvider.Get(A<string>._, A<Func<T>>._, A<TimeSpan>._));
        
        public static void CallsDataRetriever<T>(this IReturnValueArgumentValidationConfiguration<CacheValue<T>> configuration) =>
            configuration
                .ReturnsLazily(call =>
                {
                    var dataRetriever = (Func<T>) call.Arguments[1];
                    var result = dataRetriever();
                    return new CacheValue<T>(result, result != null);
                });
        
        public static IReturnValueArgumentValidationConfiguration<Task<CacheValue<T>>> CallToGetAsyncWithDataRetriever<T>(this IEasyCachingProviderBase cachingProvider) =>
            A.CallTo(() => cachingProvider.GetAsync<T>(A<string>._, A<Func<Task<T>>>._, A<TimeSpan>._));
        
        public static void CallsDataRetriever<T>(this IReturnValueArgumentValidationConfiguration<Task<CacheValue<T>>> configuration) =>
            configuration
                .ReturnsLazily(async call =>
                {
                    var dataRetriever = (Func<Task<T>>) call.Arguments[1];
                    var result = await dataRetriever();
                    return new CacheValue<T>(result, result != null);
                });
    }
}