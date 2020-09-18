namespace EasyCaching.LiteDB
{
    using System.IO;
    using EasyCaching.Core;

    /// <summary>
    /// LiteDB cache option.
    /// </summary>
    public class LiteDBDBOptions
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

        public long InitialSize { get; set; } = 1024 * 1024;
        public string Password { get; set; } = null;
        public global::LiteDB.ConnectionType ConnectionType { get; set; } = global::LiteDB.ConnectionType.Direct;
    }
}
