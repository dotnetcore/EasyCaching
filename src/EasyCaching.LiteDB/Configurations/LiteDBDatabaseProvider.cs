namespace EasyCaching.LiteDB
{
    using EasyCaching.Core;
    using global::LiteDB;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// LiteDB database provider.
    /// </summary>
    public class LiteDBDatabaseProvider : ILiteDBDatabaseProvider
    {
        /// <summary>
        /// The options.
        /// </summary>
        private readonly LiteDBDBOptions _options;
        
        public LiteDBDatabaseProvider(string name , LiteDBOptions options)
        {
            this._name = name;
            this._options = options.DBConfig;
        }

        /// <summary>
        /// The conn.
        /// </summary>
        private static LiteDatabase _conn;

        private readonly string _name = EasyCachingConstValue.DefaultLiteDBName;
        public string DBProviderName => _name;

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <returns>The connection.</returns>
        public LiteDatabase GetConnection()
        {
            if(_conn == null)
            {
                ConnectionString builder =  new ConnectionString ()
                {
                   Filename= _options.DataSource,
                 //   InitialSize = _options.InitialSize,
                     Connection = _options.ConnectionType,
                    Password = _options.Password
                };

                _conn = new LiteDatabase(builder);

            }
            return _conn;
        }
    }
}
