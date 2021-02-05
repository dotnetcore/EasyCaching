namespace EasyCaching.UnitTests
{
    using Core.Bus;
    using System;

    public class FakeBusOptions : IBusOptions
    {
        public const string DefaultName = "FakeBus";
        
        public BusFactoryDecorator BusFactoryDecorator { get; set; }
        
        public Func<IEasyCachingBus> BusFactory { get; set; }
    }
}