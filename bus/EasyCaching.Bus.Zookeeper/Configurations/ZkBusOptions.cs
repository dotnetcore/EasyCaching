using System;
using System.Text;

namespace EasyCaching.Bus.Zookeeper
{
    public class ZkBusOptions
    {

        /// <summary>
        /// default constructor
        /// </summary>
        public ZkBusOptions()
        {
            ConnectionSpanTimeout = TimeSpan.FromSeconds(20);
            SessionSpanTimeout = TimeSpan.FromSeconds(30);
            OperatingSpanTimeout = TimeSpan.FromSeconds(60);
            ReadOnly = false;
            SessionId = 0;
            SessionPasswd = null;
            EnableEphemeralNodeRestore = true;
        }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="connectionTimeout"></param>
      /// <param name="operatingTimeout"></param>
      /// <param name="sessionTimeout"></param>
        protected ZkBusOptions(int connectionTimeout, int operatingTimeout, int sessionTimeout)
        {
            ConnectionSpanTimeout = TimeSpan.FromSeconds(connectionTimeout);
            SessionSpanTimeout = TimeSpan.FromSeconds(sessionTimeout);
            OperatingSpanTimeout = TimeSpan.FromSeconds(operatingTimeout);
            ReadOnly = false;
            SessionId = 0;
            SessionPasswd = null;
            EnableEphemeralNodeRestore = true;
        }

        /// <summary>
        /// create ZooKeeper client
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ZkBusOptions(string connectionString) : this()
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
            , int sessionTimeout) : this(connectionTimeout,operatingTimeout,sessionTimeout)
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
        /// <param name="retryCount"></param>
        /// <param name="sessionTimeout"></param>
        /// <param name="readOnly"></param>
        /// <param name="sessionId"></param>
        /// <param name="sessionPasswd"></param>
        /// <param name="healthyCheckTimes"></param>
        /// <param name="baseRoutePath"></param>
        /// <param name="enableEphemeralNodeRestore"></param>
        /// <param name="logToFile"></param>
        public ZkBusOptions(string connectionString
            , int connectionTimeout
            , int operatingTimeout
            , int retryCount
            , int sessionTimeout
            , bool readOnly
            , long sessionId
            , string sessionPasswd
            , int healthyCheckTimes
            , string baseRoutePath
            , bool enableEphemeralNodeRestore
            , bool logToFile) : this(connectionString,connectionTimeout,operatingTimeout,sessionTimeout)
        {
            ConnectionTimeout = connectionTimeout;
            OperatingTimeout = operatingTimeout;
            RetryCount = retryCount;
            SessionTimeout = sessionTimeout;
            ReadOnly = readOnly;
            SessionId = sessionId;
            SessionPasswd = sessionPasswd;
            HealthyCheckTimes = healthyCheckTimes;
            BaseRoutePath = baseRoutePath;
            EnableEphemeralNodeRestore = enableEphemeralNodeRestore;
            LogToFile = logToFile;
        }


        /// <summary>
        /// connect string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// wait zooKeeper connect time
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// execute zooKeeper handler retry waittime
        /// </summary>
        public int OperatingTimeout { get; set; } = 60;

        /// <summary>
        /// retry count
        /// </summary>
        public int RetryCount { get; set; } = 10;

        /// <summary>
        /// zookeeper session timeout
        /// </summary>
        public int SessionTimeout { get; set; } = 20;

        /// <summary>
        /// readonly
        /// </summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>
        /// session Id。
        /// </summary>
        public long SessionId { get; set; }

        /// <summary>
        /// session password
        /// </summary>
        public string SessionPasswd { get; set; }

        public byte[] SessionPasswdBytes
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SessionPasswd))
                {
                    return Encoding.UTF8.GetBytes(SessionPasswd);
                }
                return null;
            }
        }

        /// <summary>
        /// log to file options
        /// </summary>
        public bool LogToFile { get; set; } = false;

        /// <summary>
        /// healthy check count
        /// </summary>
        public int HealthyCheckTimes { get; set; } = 10;

        /// <summary>
        /// base root path
        /// </summary>
        public string BaseRoutePath { get; set; } = "/easycachebus";

        /// <summary>
        /// enable effect shortnode recover
        /// </summary>
        public bool EnableEphemeralNodeRestore { get; set; }

        #region Internal
        /// <summary>
        /// wait zooKeeper connect span time
        /// </summary>
        internal TimeSpan ConnectionSpanTimeout { get; }

        /// <summary>
        /// execute zooKeeper handler retry span waittime
        /// </summary>
        internal TimeSpan OperatingSpanTimeout { get; }

        /// <summary>
        /// zookeeper session timeout
        /// </summary>
        internal TimeSpan SessionSpanTimeout { get; } 
        #endregion
    }
}
