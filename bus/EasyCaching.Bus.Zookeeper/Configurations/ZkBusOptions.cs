using System;

namespace EasyCaching.Bus.Zookeeper
{
    public class ZkBusOptions
    {
        public ZkBusOptions()
        {
            this.ConnectionTimeout = 50000;
            this.OperatingTimeout = 10000;
            this.SessionTimeout = 50000;
        }

        /// <summary>
        /// create ZooKeeper client
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ZkBusOptions(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
        }

        /// <summary>
        /// create ZooKeeper client
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="connectionTimeout"></param>
        /// <param name="operatingTimeout"></param>
        /// <param name="sessionTimeout"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ZkBusOptions(string connectionString
            , int connectionTimeout
            , int operatingTimeout
            , int sessionTimeout)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            ConnectionString = connectionString;
            this.ConnectionTimeout = connectionTimeout;
            this.SessionTimeout = sessionTimeout;
            this.OperatingTimeout = operatingTimeout;
        }

        /// <summary>
        /// connect string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// readonly
        /// </summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>
        /// point user to access
        /// </summary>
        public string Digest { get; set; }

        /// <summary>
        /// log to file options
        /// </summary>
        public bool LogToFile { get; set; } = false;

        /// <summary>
        /// base root path
        /// </summary>
        public string BaseRoutePath { get; set; } = "easyCacheBus";

        /// <summary>
        /// wait zooKeeper connect time
        /// </summary>
        public int ConnectionTimeout { get; set; }

        /// <summary>
        /// execute zooKeeper handler retry  waittime
        /// </summary>
        public int OperatingTimeout { get; set; }

        /// <summary>
        /// zookeeper session timeout
        /// </summary>
        public int SessionTimeout { get; set; }
    }
}