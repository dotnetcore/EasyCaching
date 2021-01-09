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
        public static EasyCachingOptions UseFake(
            this EasyCachingOptions options, 
            Action<FakeOptions> configure, 
            string name = FakeOptions.DefaultName)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            options.RegisterExtension(new FakeOptionsExtensions(name, configure));

            return options;
        }
    }
}
