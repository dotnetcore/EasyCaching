namespace EasyCaching.Core.Decoration
{
    using EasyCaching.Core.Bus;
    using System;

    /// <summary>
    /// Delegate for decoration of caching bus factory
    /// </summary>
    /// <param name="name">Bus name</param>
    /// <param name="serviceProvider">Service provider for dependency injection</param>
    /// <param name="busFactory">Initial caching bus factory</param>
    /// <returns>Decorated caching provider factory</returns>
    public delegate Func<IEasyCachingBus> BusFactoryDecorator(
        string name, IServiceProvider serviceProvider, Func<IEasyCachingBus> busFactory);
    
    public static class CachingBusDecorationExtensions
    {
        public static IEasyCachingBus CreateDecoratedBus(
            this IBusOptions options,
            string name,
            IServiceProvider serviceProvider,
            Func<IEasyCachingBus> cachingBusFactory)
        {
            if (options.BusFactoryDecorator == null)
            {
                options
                    .DecorateWithRetry(3, exceptionFilter: null)
                    .DecorateWithPublishFallback(exceptionFilter: null);
            }
            
            var decoratedProviderFactory = options.BusFactoryDecorator(name, serviceProvider, cachingBusFactory);
            return decoratedProviderFactory();
        }
        
        public static IBusOptions Decorate(this IBusOptions options, BusFactoryDecorator factoryDecorator)
        {
            if (options.BusFactoryDecorator == null)
            {
                options.BusFactoryDecorator = factoryDecorator;
            }
            else
            {
                var existingFactoryDecorator = options.BusFactoryDecorator;
                options.BusFactoryDecorator = (name, serviceProvider, cachingBusFactory) =>
                {
                    var factoryDecoratedWithExistingDecorator = existingFactoryDecorator(name, serviceProvider, cachingBusFactory);
                    return factoryDecorator(name, serviceProvider, factoryDecoratedWithExistingDecorator);
                };
            }

            return options;
        }

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