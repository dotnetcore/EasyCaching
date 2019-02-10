namespace EasyCaching.SQLite
{
    using System.IO;
    using EasyCaching.Core;
    using Microsoft.Data.Sqlite;

    /// <summary>
    /// SQLite cache option.
    /// </summary>
    public class SQLiteDBOptions
    {
        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; } = "easycaching.db";

        /// <summary>
        /// Gets or sets the open mode.
        /// </summary>
        /// <value>The open mode.</value>
        public SqliteOpenMode OpenMode { get; set; } = SqliteOpenMode.ReadWriteCreate;

        /// <summary>
        /// Gets or sets the cache mode.
        /// </summary>
        /// <value>The cache mode.</value>
        public SqliteCacheMode CacheMode { get; set; } = SqliteCacheMode.Default;

        /// <summary>
        /// Gets the data source.
        /// </summary>
        /// <value>The data source.</value>
        public string DataSource
        {
            get
            {
                

                if(string.IsNullOrWhiteSpace(FilePath)&& string.IsNullOrWhiteSpace(FileName))
                {
                    return ":memory:";
                }
                else
                {
                    ArgumentCheck.NotNullOrWhiteSpace(FilePath, nameof(FilePath));
                    ArgumentCheck.NotNullOrWhiteSpace(FileName, nameof(FileName));

                    return Path.Combine(FilePath, FileName);
                }

               
            }
        }
    }
}
