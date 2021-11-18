namespace EasyCaching.Redis
{
	using StackExchange.Redis;
	using System.Collections.Concurrent;

	internal class ConnectionMultiplexerProvider
	{
		private readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connectionMultiplexers = new();

		private ConnectionMultiplexer GetConnectionMultiplexer(string connectionString) =>
			_connectionMultiplexers.GetOrAdd(connectionString, _ => ConnectionMultiplexer.Connect(connectionString));
		
		public ConnectionMultiplexer GetConnectionMultiplexer(BaseRedisOptions options)
		{
			if (string.IsNullOrWhiteSpace(options.Configuration))
			{
				var configurationOptions = new ConfigurationOptions
				{
					ConnectTimeout = options.ConnectionTimeout,
					User = options.Username,
					Password = options.Password,
					Ssl = options.IsSsl,
					SslHost = options.SslHost,
					AllowAdmin = options.AllowAdmin,
					DefaultDatabase = options.Database,
					AbortOnConnectFail = options.AbortOnConnectFail,
				};

				foreach (var endpoint in options.Endpoints)
				{
					configurationOptions.EndPoints.Add(endpoint.Host, endpoint.Port);
				}

				return GetConnectionMultiplexer(configurationOptions.ToString());
			}
			else
			{
				return GetConnectionMultiplexer(options.Configuration);
			}
		}
	}
}