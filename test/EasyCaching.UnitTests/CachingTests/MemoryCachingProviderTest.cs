namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.InMemory;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public class MemoryCachingProviderTest : BaseCachingProviderTest
    {
        public MemoryCachingProviderTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddDefaultInMemoryCache();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(30);
        }
    }
}
