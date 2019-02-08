using System;
using System.Collections.Generic;
using System.Text;
using EasyCaching.Core;
using EasyCaching.Core.Configurations;

namespace EasyCaching.SQLServer.Configurations
{
    public class SQLServerOptions : BaseProviderOptions
    {
        public SQLServerOptions()
        {
            base.CachingProviderType = CachingProviderType.SQLServer;
        }

        public SQLDBOptions DBConfig { get; set; } = new SQLDBOptions();
    }
}
