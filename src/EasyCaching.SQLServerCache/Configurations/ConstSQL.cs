namespace EasyCaching.SQLServer
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
                DELETE FROM [{0}].[{1}] WHERE [cachekey] = @cachekey AND [name]=@name;
                INSERT INTO [{0}].[{1}]
                    ([name]
                    ,[cachekey]
                    ,[cachevalue]
                    ,[expiration])
                VALUES
                    (@name
                    ,@cachekey
                    ,@cachevalue
                    ,DATEADD(second, @expiration, getutcdate()));";

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
        public const string CREATESQL = @"IF NOT EXISTS (
                                        SELECT  schema_name
                                        FROM    information_schema.schemata
                                        WHERE   schema_name = '{0}' )
                                         
                                        BEGIN
                                        EXEC sp_executesql N'CREATE SCHEMA {0}'
                                        END

                                        IF  NOT EXISTS (SELECT * FROM sys.objects 
                                        WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND type in (N'U'))
                                        BEGIN
                                        CREATE TABLE [{0}].[{1}] (
                                                            [ID] INTEGER PRIMARY KEY
                                                            , [name] TEXT
                                                            , [cachekey] TEXT
                                                            , [cachevalue] TEXT
                                                            , [expiration] DATETIME)
                                        END";
    }
}
