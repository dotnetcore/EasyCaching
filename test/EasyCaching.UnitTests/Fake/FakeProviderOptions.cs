namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Configurations;
    using System;

    public class FakeProviderOptions : BaseProviderOptionsWithDecorator<IEasyCachingProvider>
    {
        public const string DefaultName = "FakeProvider";
        
        public Func<IEasyCachingProvider> ProviderFactory { get; set; }
    }
}