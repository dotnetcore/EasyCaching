namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Configurations;
    using System;

    public class FakeOptions : BaseProviderOptionsWithDecorator<IEasyCachingProvider>
    {
        public const string DefaultName = "Fake";
        
        public Func<IEasyCachingProvider> ProviderFactory { get; set; }
    }
}