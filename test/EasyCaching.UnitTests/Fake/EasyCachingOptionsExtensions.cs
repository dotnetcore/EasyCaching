namespace Microsoft.Extensions.DependencyInjection
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.UnitTests;
    using System;

    /// <summary>
    /// EasyCaching options extensions of Fake.
    /// </summary>
    public static class EasyCachingOptionsExtensions
    {
        public static EasyCachingOptions UseFakeProvider(
            this EasyCachingOptions options, 
            Action<FakeProviderOptions> configure, 
            string name = FakeProviderOptions.DefaultName)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new FakeProviderOptionsExtensions(name, configure));

            return options;
        }
        
        public static EasyCachingOptions WithFakeBus(
            this EasyCachingOptions options, 
            Action<FakeBusOptions> configure, 
            string name = FakeBusOptions.DefaultName)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new FakeBusOptionsExtensions(name, configure));

            return options;
        }
    }
}
