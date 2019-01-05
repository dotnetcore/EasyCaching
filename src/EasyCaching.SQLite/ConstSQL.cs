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
                DELETE FROM [easycaching] WHERE [cachekey] = @cachekey AND [name]=@name;
                INSERT INTO [easycaching]
                    ([name]
                    ,[cachekey]
                    ,[cachevalue]
                    ,[expiration])
                VALUES
                    (@name
                    ,@cachekey
                    ,@cachevalue
                    ,(select strftime('%s','now')) + @expiration);";

        /// <summary>
        /// The trysetsql.
        /// </summary>
        public const string TRYSETSQL = @"
                INSERT INTO [easycaching]
                    ([name]
                    ,[cachekey]
                    ,[cachevalue]
                    ,[expiration])
                SELECT @name,@cachekey,@cachevalue,(select strftime('%s','now') + @expiration)
                WHERE NOT EXISTS (SELECT 1 FROM [easycaching] WHERE [cachekey] = @cachekey AND [name]=@name AND [expiration] > strftime('%s','now'));";



        /// <summary>
        /// The getsql.
        /// </summary>
        public const string GETSQL = @"SELECT [cachevalue]
                    FROM [easycaching]
                    WHERE [cachekey] = @cachekey AND [name]=@name AND [expiration] > strftime('%s','now')";

        /// <summary>
        /// The getallsql.
        /// </summary>
        public const string GETALLSQL = @"SELECT [cachekey],[cachevalue]
                    FROM [easycaching]
                    WHERE [cachekey] IN @cachekey AND [name]=@name AND [expiration] > strftime('%s','now')";
        
        /// <summary>
        /// The getbyprefixsql.
        /// </summary>
        public const string GETBYPREFIXSQL = @"SELECT [cachekey],[cachevalue]
                    FROM [easycaching]
                    WHERE [cachekey] LIKE @cachekey AND [name]=@name AND [expiration] > strftime('%s','now')";

        /// <summary>
        /// The removesql.
        /// </summary>
        public const string REMOVESQL = @"DELETE FROM [easycaching] WHERE [cachekey] = @cachekey AND [name] = @name ";

        /// <summary>
        /// The removebyprefixsql.
        /// </summary>
        public const string REMOVEBYPREFIXSQL = @"DELETE FROM [easycaching] WHERE [cachekey] like @cachekey  AND [name]=@name";

        /// <summary>
        /// The existssql.
        /// </summary>
        public const string EXISTSSQL = @"SELECT COUNT(1)
                    FROM [easycaching]
                    WHERE [cachekey] = @cachekey AND [name]=@name AND [expiration] > strftime('%s','now')";

        /// <summary>
        /// The countallsql.
        /// </summary>
        public const string COUNTALLSQL = @"SELECT COUNT(1)
            FROM [easycaching]
            WHERE [expiration] > strftime('%s','now') AND [name]=@name";

        /// <summary>
        /// The countprefixsql.
        /// </summary>
        public const string COUNTPREFIXSQL = @"SELECT COUNT(1)
            FROM [easycaching]
            WHERE [cachekey] like @cachekey AND [name]=@name AND [expiration] > strftime('%s','now')";

        /// <summary>
        /// The flushsql.
        /// </summary>
        public const string FLUSHSQL = @"DELETE FROM [easycaching] WHERE [name]=@name";

        /// <summary>
        /// The createsql.
        /// </summary>
        public const string CREATESQL = @"CREATE TABLE IF NOT EXISTS [easycaching] (
                    [ID] INTEGER PRIMARY KEY
                    , [name] TEXT
                    , [cachekey] TEXT
                    , [cachevalue] TEXT
                    , [expiration] INTEGER)";
    }
}
