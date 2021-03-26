namespace EasyCaching.UnitTests
{
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Decoration;
    using System;

    public class FakeBusOptions : IBusOptions
    {
        public const string DefaultName = "FakeBus";
        
        public BusFactoryDecorator BusFactoryDecorator { get; set; }

        public Func<IEasyCachingBus> BusFactory { get; set; }
    }
}