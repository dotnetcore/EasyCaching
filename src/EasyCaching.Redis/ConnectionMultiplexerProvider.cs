namespace EasyCaching.Redis
{
	using StackExchange.Redis;
	using System.Collections.Concurrent;

	internal class ConnectionMultiplexerProvider
	{
		private readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connectionMultiplexers = new();

		public ConnectionMultiplexer GetConnectionMultiplexer(string connectionString) =>
			_connectionMultiplexers.GetOrAdd(connectionString, _ => ConnectionMultiplexer.Connect(connectionString));
	}
}