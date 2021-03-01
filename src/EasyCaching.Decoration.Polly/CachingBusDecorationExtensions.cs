namespace EasyCaching.Decoration.Polly
{
    using EasyCaching.Core.Decoration;
    using EasyCaching.Core.Bus;
    using System;
    
    public static class CachingBusDecorationExtensions
    {
        public static IBusOptions DecorateWithRetry(
            this IBusOptions options, 
            int retryCount,
            Func<Exception, bool> exceptionFilter,
            Func<int, TimeSpan> sleepDurationProvider = null) =>
            options.Decorate((name, serviceProvider, cachingBusFactory) =>
                () => EasyCachingBusPolicyDecorator.WithRetry(
                    name,
                    cachingBusFactory,
                    retryCount,
                    exceptionFilter,
                    sleepDurationProvider));

        public static IBusOptions DecorateWithPublishFallback(
            this IBusOptions options, Func<Exception, bool> exceptionFilter) =>
            options.Decorate((name, serviceProvider, cachingBusFactory) =>
                () => EasyCachingBusPolicyDecorator.WithPublishFallback(
                    name, 
                    cachingBusFactory, 
                    exceptionFilter));

        public static IBusOptions DecorateWithCircuitBreaker(
            this IBusOptions options, 
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters,
            TimeSpan subscribeRetryInterval,
            Func<Exception, bool> exceptionFilter) =>
            options.Decorate((name, serviceProvider, cachingBusFactory) =>
                () => EasyCachingBusPolicyDecorator.WithCircuitBreaker(
                    name, 
                    cachingBusFactory, 
                    initParameters,
                    executeParameters,
                    subscribeRetryInterval,
                    exceptionFilter));

    }
}