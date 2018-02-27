namespace EasyCaching.SQLite
{
    /// <summary>
    /// Const sql.
    /// </summary>
    public static class ConstSQL
    {
        /// <summary>
        /// The setsql.
        /// </summary>
        public const string SETSQL = @"
                DELETE FROM [easycaching] WHERE [cachekey] = @cachekey;
                INSERT INTO [easycaching]
                    ([cachekey]
                    ,[cachevalue]
                    ,[expiration])
                VALUES
                    (@cachekey
                    ,@cachevalue
                    ,(select strftime('%s','now')) + @expiration);";

        /// <summary>
        /// The getsql.
        /// </summary>
        public const string GETSQL = @"SELECT [cachevalue]
                    FROM [easycaching]
                    WHERE [cachekey] = @cachekey AND [expiration] > strftime('%s','now')";

        /// <summary>
        /// The getallsql.
        /// </summary>
        public const string GETALLSQL = @"SELECT [cachekey],[cachevalue]
                    FROM [easycaching]
                    WHERE [cachekey] IN @cachekey AND [expiration] > strftime('%s','now')";
        
        /// <summary>
        /// The getbyprefixsql.
        /// </summary>
        public const string GETBYPREFIXSQL = @"SELECT [cachekey],[cachevalue]
                    FROM [easycaching]
                    WHERE [cachekey] LIKE @cachekey AND [expiration] > strftime('%s','now')";

        /// <summary>
        /// The removesql.
        /// </summary>
        public const string REMOVESQL = @"DELETE FROM [easycaching] WHERE [cachekey] = @cachekey ";

        /// <summary>
        /// The removebyprefixsql.
        /// </summary>
        public const string REMOVEBYPREFIXSQL = @"DELETE FROM [easycaching] WHERE [cachekey] like @cachekey ";

        /// <summary>
        /// The existssql.
        /// </summary>
        public const string EXISTSSQL = @"SELECT COUNT(1)
                    FROM [easycaching]
                    WHERE [cachekey] = @cachekey AND [expiration] > strftime('%s','now')";

        /// <summary>
        /// The createsql.
        /// </summary>
        public const string CREATESQL = @"CREATE TABLE IF NOT EXISTS [easycaching] (
                    [ID] INTEGER PRIMARY KEY
                    , [cachekey] TEXT
                    , [cachevalue] TEXT
                    , [expiration] INTEGER)";
    }
}
