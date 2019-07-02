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
            IServiceCollection services = new ServiceCollection();
            services.AddEasyCaching(x => x.UseDisk(options => 
            {
                options.MaxRdSecond = 0;
                options.DBConfig = new DiskDbOptions
                {
                    BasePath = Path.GetTempPath()
                };

             }));
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            _provider = serviceProvider.GetService<IEasyCachingProvider>();
            _defaultTs = TimeSpan.FromSeconds(30);
        }

        [Fact(Skip = "")]
        protected override void Refresh_Should_Succeed()
        {
            
        }

        [Fact(Skip = "")]
        protected override Task Refresh_Async_Should_Succeed()
        {
            return Task.CompletedTask;
        }

        [Fact(Skip = "")]
        protected override void Refresh_Value_Type_Object_Should_Succeed()
        {

        }

        [Fact(Skip = "")]
        protected override Task Refresh_Value_Type_Object_Async_Should_Succeed()
        {
            return Task.CompletedTask;
        }
    }   
}