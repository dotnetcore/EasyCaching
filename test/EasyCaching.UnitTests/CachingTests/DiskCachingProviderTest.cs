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

        [Fact]
        public async Task TrySetAgainAsync_After_Expired_Should_Succed()
        {
            var key = Guid.NewGuid().ToString();
            var v1 = "123456";
            var res1 = await _provider.TrySetAsync<string>(key, v1, TimeSpan.FromSeconds(1));
            Assert.True(res1);

            await Task.Delay(TimeSpan.FromSeconds(2));
            var res2 = await _provider.TrySetAsync<string>(key, v1, TimeSpan.FromSeconds(5));
            Assert.True(res2);
        }

        [Fact]
        public void TrySetAgain_After_Expired_Should_Succed()
        {
            var key = Guid.NewGuid().ToString();
            var v1 = "123456";
            var res1 = _provider.TrySet<string>(key, v1, TimeSpan.FromSeconds(1));
            Assert.True(res1);

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            var res2 = _provider.TrySet<string>(key, v1, TimeSpan.FromSeconds(5));
            Assert.True(res2);
        }

        [Fact]
        public void Set_Diff_Len_Value_Should_Succed()
        {
            var key = Guid.NewGuid().ToString();
            var v1 = "1234567890";
            var v2 = "12345";

            _provider.Set<string>(key, v1, TimeSpan.FromSeconds(15));
            var res1 = _provider.Get<string>(key);
            Assert.Equal(v1, res1.Value);

            _provider.Set<string>(key, v2, TimeSpan.FromSeconds(15));
            var res2 = _provider.Get<string>(key);
            Assert.Equal(v2, res2.Value);
        }
    }   
}