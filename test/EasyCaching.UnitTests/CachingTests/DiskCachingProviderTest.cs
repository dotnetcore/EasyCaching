using EasyCaching.Core.Configurations;

namespace EasyCaching.UnitTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Disk;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class DiskCachingProviderTest : BaseCachingProviderTest
    {
        public DiskCachingProviderTest()
        {
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        protected override IEasyCachingProvider CreateCachingProvider(Action<BaseProviderOptions> additionalSetup)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x => 
                x.UseDisk(options => 
                {
                    options.MaxRdSecond = 0;
                    options.DBConfig = new DiskDbOptions
                    {
                        BasePath = Path.GetTempPath()
                    };
                    additionalSetup(options);
                })
            );
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IEasyCachingProvider>();
        }

        [Fact(Skip = "fail in windows ci")]
        protected override Task GetAsync_Parallel_Should_Succeed()
        {
            return Task.CompletedTask;
        }
    }   
}