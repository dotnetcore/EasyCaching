namespace EasyCaching.Core.Bus
{
    using Decoration;

    public interface IBusOptions
    {
        BusFactoryDecorator BusFactoryDecorator { get; set; }

        void DecorateWithRetryAndPublishFallback(int retryCount);
    }
}