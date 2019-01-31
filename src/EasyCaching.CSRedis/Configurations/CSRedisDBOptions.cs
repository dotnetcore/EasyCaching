namespace EasyCaching.CSRedis
{
    using System;
    using System.Collections.Generic;

    public class CSRedisDBOptions
    {
        /// <summary>
        /// Gets or sets the node rule.
        /// </summary>
        /// <value>The node rule.</value>
        public Func<string, string> NodeRule { get; set; } = null;

        /// <summary>
        /// Gets or sets the connection strings.
        /// </summary>
        /// <value>The connection strings.</value>
        public List<string> ConnectionStrings { get; set; }        
    }
}