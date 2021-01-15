namespace EasyCaching.Disk
{
    using Core;
    using EasyCaching.Core.Configurations;

    public class DiskOptions : BaseProviderOptionsWithDecorator<IEasyCachingProvider>
    {
        public DiskDbOptions DBConfig { get; set; } = new DiskDbOptions();
    }
}