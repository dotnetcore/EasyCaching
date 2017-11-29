namespace EasyCaching.SQLite
{
    using System;
    using System.Data;
    using Microsoft.Data.Sqlite;
    using Dapper;
    using System.Linq;

    /// <summary>
    /// SQLHelper.
    /// </summary>
    public class SQLHelper
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static SQLHelper Instance = new SQLHelper();

        private SqliteConnection _conn;
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The conn.</value>
        public SqliteConnection Conn
        {
            get
            {
                if(_conn==null)
                {
                    _conn = new SqliteConnection("");
                }
                return _conn;
            }
        }

        /// <summary>
        /// Open this instance.
        /// </summary>
        public void Open()
        {
            Conn.Open();
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:EasyCaching.SQLite.SQLHelper"/> is opened.
        /// </summary>
        /// <value><c>true</c> if is opened; otherwise, <c>false</c>.</value>
        public bool IsOpened
        {
            get
            {
                return _conn != null && _conn.State != ConnectionState.Closed;
            }
        }

        /// <summary>
        /// Set the specified cacheKey, cacheValue and expiration.
        /// </summary>
        /// <returns>The set.</returns>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="cacheValue">Cache value.</param>
        /// <param name="expiration">Expiration.</param>
        public void Set(string cacheKey,string cacheValue,long expiration)
        {
            if (!IsOpened) this.Open();

            var sql = @"INSERT INTO [easycaching]
        ([cachekey]
        ,[cachevalue]
        ,[expiration])
    VALUES
        (@cachekey
        ,@cachevalue
        ,(select strftime('%s','now')) + @expiration)";
            
            Conn.Execute(sql, new
            {
                cachekey = cacheKey,
                cacheValue = cacheValue,
                expiration = expiration
            });
        }

        /// <summary>
        /// Get the specified cacheKey.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="cacheKey">Cache key.</param>
        public object Get(string cacheKey)
        {
            if (!IsOpened) this.Open();

            var sql = @"SELECT [cachevalue]
                    FROM [easycaching]
                    WHERE [cachekey] = @cachekey AND [expiration] > strftime('%s','now')";

            var res = Conn.Query<string>(sql, new
            {
                cachekey = cacheKey
            }).FirstOrDefault();

            return res == null ? null : Newtonsoft.Json.JsonConvert.DeserializeObject(res);
        }
    }
}
