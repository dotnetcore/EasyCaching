using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EasyCaching.SQLServer.Configurations
{
    public interface ISQLDatabaseProvider
    {
        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        IDbConnection GetConnection();

        string DBProviderName { get; }
    }
}
