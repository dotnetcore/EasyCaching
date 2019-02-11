namespace EasyCaching.SQLServer.Configurations
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
                INSERT INTO [{0}].[{1}]
                    ([name]
                    ,[cachekey]
                    ,[cachevalue]
                    ,[expiration])
                SELECT @name,@cachekey,@cachevalue,DATEADD(second, @expiration, getutcdate())
                WHERE NOT EXISTS (SELECT 1 FROM [{0}].[{1}] WHERE [cachekey] = @cachekey AND [name]=@name AND [expiration] > getutcdate());";



        /// <summary>
        /// The getsql.
        /// </summary>
        public const string GETSQL = @"SELECT [cachevalue]
                    FROM [{0}].[{1}]
                    WHERE [cachekey] = @cachekey AND [name]=@name AND [expiration] > getutcdate()";

        /// <summary>
        /// The getallsql.
        /// </summary>
        public const string GETALLSQL = @"SELECT [cachekey],[cachevalue]
                    FROM [{0}].[{1}]
                    WHERE [cachekey] IN @cachekey AND [name]=@name AND [expiration] > getutcdate()";
        
        /// <summary>
        /// The getbyprefixsql.
        /// </summary>
        public const string GETBYPREFIXSQL = @"SELECT [cachekey],[cachevalue]
                    FROM [{0}].[{1}]
                    WHERE [cachekey] LIKE @cachekey AND [name]=@name AND [expiration] > getutcdate()";

        /// <summary>
        /// The removesql.
        /// </summary>
        public const string REMOVESQL = @"DELETE FROM [{0}].[{1}] WHERE [cachekey] = @cachekey AND [name] = @name ";

        /// <summary>
        /// The removebyprefixsql.
        /// </summary>
        public const string REMOVEBYPREFIXSQL = @"DELETE FROM [{0}].[{1}] WHERE [cachekey] like @cachekey  AND [name]=@name";

        /// <summary>
        /// The existssql.
        /// </summary>
        public const string EXISTSSQL = @"SELECT COUNT(1)
                    FROM [{0}].[{1}]
                    WHERE [cachekey] = @cachekey AND [name]=@name AND [expiration] > getutcdate()";

        /// <summary>
        /// The countallsql.
        /// </summary>
        public const string COUNTALLSQL = @"SELECT COUNT(1)
            FROM [{0}].[{1}]
            WHERE [expiration] > getutcdate() AND [name]=@name";

        /// <summary>
        /// The countprefixsql.
        /// </summary>
        public const string COUNTPREFIXSQL = @"SELECT COUNT(1)
            FROM [{0}].[{1}]
            WHERE [cachekey] like @cachekey AND [name]=@name AND [expiration] > getutcdate()";

        /// <summary>
        /// The flushsql.
        /// </summary>
        public const string FLUSHSQL = @"DELETE FROM [{0}].[{1}] WHERE [name]=@name";

        /// <summary>
        /// The sql for cleaning up db. Remove all expired entries from the db.
        /// </summary>
        public const string CLEANEXPIREDSQL = @"DELETE FROM [{0}].[{1}] WHERE [expiration] < getutcdate()";

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
                                                            [ID] INT IDENTITY(1,1) PRIMARY KEY
                                                            , [name] VARCHAR(255)
                                                            , [cachekey] VARCHAR(255)
                                                            , [cachevalue] NVARCHAR(MAX)
                                                            , [expiration] DATETIME)
                                        END";
    }
}
