using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCaching.SQLServer.Configurations
{
    public class SQLDBOptions
    {
        public string ConnectionString { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public TimeSpan ExpirationScanFrequency { get; set; } = TimeSpan.FromMinutes(1);
    }
}
