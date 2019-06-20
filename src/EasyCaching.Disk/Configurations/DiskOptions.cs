namespace EasyCaching.Disk
{
    using EasyCaching.Core.Configurations;

    public class DiskOptions : BaseProviderOptions
    {
        public DiskOptions()
        {
        }

        public DiskDbOptions DBConfig { get; set; } = new DiskDbOptions();
    }
}
