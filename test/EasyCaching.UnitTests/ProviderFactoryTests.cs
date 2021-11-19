namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.Core.Configurations;
    using EasyCaching.Redis;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using Xunit;

    public class ProviderFactoryTests
    {
        private readonly IEasyCachingProvider _e1;
        private readonly IEasyCachingProvider _e2;

        public ProviderFactoryTests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(option =>
            {
                option.UseRedis(options =>
                {
                    options.ConnectionString = "127.0.0.1:6380,allowAdmin=true,defaultDatabase=12";
                }, "redis");

                option.UseInMemory(_ =>
                {
                }, "inMemory");
            });


            IServiceProvider serviceProvider = services.BuildServiceProvider();

            var factory = serviceProvider.GetService<IEasyCachingProviderFactory>();

            _e1 = factory.GetCachingProvider("redis");
            _e2 = factory.GetCachingProvider("inMemory");
        }

        [Fact]
        public void ProviderName_Should_Be_Same()
        {        
            Assert.Equal("redis", _e1.Name);
            Assert.Equal("inMemory", _e2.Name);
        }
    }
}
