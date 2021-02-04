namespace EasyCaching.Core.Bus
{
    public interface IBusOptions
    {
        BusFactoryDecorator BusFactoryDecorator { get; set; }
    }
}