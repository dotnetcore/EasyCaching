namespace EasyCaching.Memcached
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Enyim.Caching.Configuration;
    using Enyim.Caching.Memcached;
    using Enyim.Caching.Memcached.Protocol.Binary;
    using Enyim.Reflection;
    using Microsoft.Extensions.Logging;

    public class EasyCachingMemcachedClientConfiguration : IMemcachedClientConfiguration
    {
        // these are lazy initialized in the getters
        private Type nodeLocator;
        private ITranscoder _transcoder;
        private IMemcachedKeyTransformer _keyTransformer;
        private ILogger<EasyCachingMemcachedClientConfiguration> _logger;
        private string _name;        

        public EasyCachingMemcachedClientConfiguration(
            string name,
            ILoggerFactory loggerFactory,
            MemcachedOptions optionsAccessor,
            IEnumerable<EasyCachingTranscoder> transcoders = null,
            IMemcachedKeyTransformer keyTransformer = null)
        {
            this._name = name;
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _logger = loggerFactory.CreateLogger<EasyCachingMemcachedClientConfiguration>();

            var options = optionsAccessor.DBConfig;

            ConfigureServers(options);

            SocketPool = new SocketPoolConfiguration();
            if (options.SocketPool != null)
            {
                options.SocketPool.CheckPoolSize();
                options.SocketPool.CheckTimeout();

                SocketPool.MinPoolSize = options.SocketPool.MinPoolSize;
                _logger.LogInformation($"{nameof(SocketPool.MinPoolSize)}: {SocketPool.MinPoolSize}");

                SocketPool.MaxPoolSize = options.SocketPool.MaxPoolSize;
                _logger.LogInformation($"{nameof(SocketPool.MaxPoolSize)}: {SocketPool.MaxPoolSize}");

                SocketPool.ConnectionTimeout = options.SocketPool.ConnectionTimeout;
                _logger.LogInformation($"{nameof(SocketPool.ConnectionTimeout)}: {SocketPool.ConnectionTimeout}");

                SocketPool.ReceiveTimeout = options.SocketPool.ReceiveTimeout;
                _logger.LogInformation($"{nameof(SocketPool.ReceiveTimeout)}: {SocketPool.ReceiveTimeout}");

                SocketPool.DeadTimeout = options.SocketPool.DeadTimeout;
                _logger.LogInformation($"{nameof(SocketPool.DeadTimeout)}: {SocketPool.DeadTimeout}");

                SocketPool.QueueTimeout = options.SocketPool.QueueTimeout;
                _logger.LogInformation($"{nameof(SocketPool.QueueTimeout)}: {SocketPool.QueueTimeout}");

                SocketPool.InitPoolTimeout = options.SocketPool.InitPoolTimeout;
            }

            Protocol = options.Protocol;

            if (options.Authentication != null && !string.IsNullOrEmpty(options.Authentication.Type))
            {
                try
                {
                    var authenticationType = Type.GetType(options.Authentication.Type);
                    if (authenticationType != null)
                    {
                        _logger.LogDebug($"Authentication type is {authenticationType}.");

                        Authentication = new AuthenticationConfiguration();
                        Authentication.Type = authenticationType;
                        foreach (var parameter in options.Authentication.Parameters)
                        {
                            Authentication.Parameters[parameter.Key] = parameter.Value;
                            _logger.LogDebug($"Authentication {parameter.Key} is '{parameter.Value}'.");
                        }
                    }
                    else
                    {
                        _logger.LogError($"Unable to load authentication type {options.Authentication.Type}.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(), ex, $"Unable to load authentication type {options.Authentication.Type}.");
                }
            }

            if (keyTransformer != null)
            {
                this.KeyTransformer = keyTransformer;
                _logger.LogDebug($"Use KeyTransformer Type : '{keyTransformer.ToString()}'");
            }

            if (NodeLocator == null)
            {
                NodeLocator = options.Servers.Count > 1 ? typeof(DefaultNodeLocator) : typeof(SingleNodeLocator);
            }

            if (transcoders != null)
            {
                EasyCachingTranscoder coder = null;

                if (string.IsNullOrWhiteSpace(optionsAccessor.SerializerName))
                {
                    coder = transcoders.FirstOrDefault(x => x.Name.Equals(_name));
                }
                else
                {
                    coder = transcoders.FirstOrDefault(x => x.Name.Equals(optionsAccessor.SerializerName));
                }
                
                if (coder != null)
                {
                    this._transcoder = coder;
                    _logger.LogDebug($"Use Transcoder Type : '{coder.ToString()}'");
                }
            }

            if (options.NodeLocatorFactory != null)
            {
                NodeLocatorFactory = options.NodeLocatorFactory;
            }
        }

        private void ConfigureServers(EasyCachingMemcachedClientOptions options)
        {
            Servers = new List<EndPoint>();
            foreach (var server in options.Servers)
            {
                if (!IPAddress.TryParse(server.Address, out var address))
                {
                    address = Dns.GetHostAddresses(server.Address)
                        .FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);

                    if (address == null)
                    {
                        _logger.LogError($"Could not resolve host '{server.Address}'.");
                    }
                    else
                    {
                        _logger.LogInformation($"Memcached server address - {address}");
                    }
                }
                else
                {
                    _logger.LogInformation($"Memcached server address - {server.Address }:{server.Port}");
                }

                Servers.Add(new IPEndPoint(address, server.Port));
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this._name;

        /// <summary>
        /// Gets a list of <see cref="T:IPEndPoint"/> each representing a Memcached server in the pool.
        /// </summary>
        public IList<EndPoint> Servers { get; private set; }

        /// <summary>
        /// Gets the configuration of the socket pool.
        /// </summary>
        public ISocketPoolConfiguration SocketPool { get; private set; }

        /// <summary>
        /// Gets the authentication settings.
        /// </summary>
        public IAuthenticationConfiguration Authentication { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.IMemcachedKeyTransformer"/> which will be used to convert item keys for Memcached.
        /// </summary>
        public IMemcachedKeyTransformer KeyTransformer
        {
            get { return this._keyTransformer ?? (this._keyTransformer = new DefaultKeyTransformer()); }
            set { this._keyTransformer = value; }
        }

        /// <summary>
        /// Gets or sets the Type of the <see cref="T:Enyim.Caching.Memcached.IMemcachedNodeLocator"/> which will be used to assign items to Memcached nodes.
        /// </summary>
        /// <remarks>If both <see cref="M:NodeLocator"/> and  <see cref="M:NodeLocatorFactory"/> are assigned then the latter takes precedence.</remarks>
        public Type NodeLocator
        {
            get { return this.nodeLocator; }
            set
            {
                ConfigurationHelper.CheckForInterface(value, typeof(IMemcachedNodeLocator));
                this.nodeLocator = value;
            }
        }

        /// <summary>
        /// Gets or sets the NodeLocatorFactory instance which will be used to create a new IMemcachedNodeLocator instances.
        /// </summary>
        /// <remarks>If both <see cref="M:NodeLocator"/> and  <see cref="M:NodeLocatorFactory"/> are assigned then the latter takes precedence.</remarks>
        public IProviderFactory<IMemcachedNodeLocator> NodeLocatorFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:Enyim.Caching.Memcached.ITranscoder"/> which will be used serialize or deserialize items.
        /// </summary>
        public ITranscoder Transcoder
        {
            get { return _transcoder ?? (_transcoder = new DefaultTranscoder()); }
            set { _transcoder = value; }
        }

        /// <summary>
        /// Gets or sets the type of the communication between client and server.
        /// </summary>
        public MemcachedProtocol Protocol { get; set; }

        #region [ interface                     ]

        IList<EndPoint> IMemcachedClientConfiguration.Servers
        {
            get { return this.Servers; }
        }

        ISocketPoolConfiguration IMemcachedClientConfiguration.SocketPool
        {
            get { return this.SocketPool; }
        }

        IAuthenticationConfiguration IMemcachedClientConfiguration.Authentication
        {
            get { return this.Authentication; }
        }

        IMemcachedKeyTransformer IMemcachedClientConfiguration.CreateKeyTransformer()
        {
            return this.KeyTransformer;
        }

        IMemcachedNodeLocator IMemcachedClientConfiguration.CreateNodeLocator()
        {
            var f = this.NodeLocatorFactory;
            if (f != null) return f.Create();

            return this.NodeLocator == null
                    ? new SingleNodeLocator()
                    : (IMemcachedNodeLocator)FastActivator.Create(this.NodeLocator);
        }

        ITranscoder IMemcachedClientConfiguration.CreateTranscoder()
        {
            return this.Transcoder;
        }

        IServerPool IMemcachedClientConfiguration.CreatePool()
        {
            switch (this.Protocol)
            {
                case MemcachedProtocol.Text: return new DefaultServerPool(this, new Enyim.Caching.Memcached.Protocol.Text.TextOperationFactory(), _logger);
                case MemcachedProtocol.Binary: return new BinaryPool(this, _logger);
            }

            throw new ArgumentOutOfRangeException("Unknown protocol: " + (int)this.Protocol);
        }

        #endregion
    }
}
