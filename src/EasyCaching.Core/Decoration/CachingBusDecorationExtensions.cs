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
                return cachingBusFactory();
            }
            else
            {
                var decoratedProviderFactory = options.BusFactoryDecorator(name, serviceProvider, cachingBusFactory);
                return decoratedProviderFactory();
            }
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
    }
}