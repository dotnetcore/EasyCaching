namespace EasyCaching.Disk
{
    using EasyCaching.Core.Configurations;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class EasyCachingOptionsExtensions : IEasyCachingOptionsExtension
    {
        public EasyCachingOptionsExtensions()
        {
        }

        public void AddServices(IServiceCollection services)
        {
            throw new System.NotImplementedException();
        }

        public void WithServices(IApplicationBuilder app)
        {
            throw new System.NotImplementedException();
        }
    }
}
