using CSRedis;
using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.CSRedis.DistributedLock
{
    public class CSRedisLock : MemoryLock
    {
        private readonly object _syncObj = new object();
        private readonly string _key;
        private readonly CSRedisClient _client;
        private readonly BaseProviderOptions _options;
        private readonly ILogger<CSRedisLock> _logger;
        private byte[] _value;
        private Timer _timer;

        public CSRedisLock(string key, CSRedisClient client, BaseProviderOptions options, ILogger<CSRedisLock> logger) : base(key)
        {
            _key = key;
            _client = client;
            _options = options;
            _logger = logger;
        }

        public override bool Lock(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            if (base.Lock(millisecondsTimeout, cancellationToken))
            {
                GetNewGuid();

                do
                {
                    try
                    {
                        if (Lock(_key, _value))
                        {
                            StartPing();

                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(default, ex, ex.Message);

                        if (!(ex is RedisClientException)) break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        _value = null;

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    Thread.Sleep(Math.Max(0, Math.Min(100, millisecondsTimeout - (int)sw.ElapsedMilliseconds)));
                } while (sw.ElapsedMilliseconds < millisecondsTimeout);

                _logger?.LogWarning($"{_key}/Wait fail");

                base.Release();
            }

            _value = null;
            return false;
        }

        public override async ValueTask<bool> LockAsync(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            if (await base.LockAsync(millisecondsTimeout, cancellationToken))
            {
                GetNewGuid();

                do
                {
                    try
                    {
                        if (await LockAsync(_key, _value))
                        {
                            StartPing();

                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(default, ex, ex.Message);

                        if (!(ex is RedisClientException)) break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        _value = null;

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(Math.Max(0, Math.Min(100, millisecondsTimeout - (int)sw.ElapsedMilliseconds)), cancellationToken);
                } while (sw.ElapsedMilliseconds < millisecondsTimeout);

                _logger?.LogWarning($"{_key}/Wait fail");

                await base.ReleaseAsync();
            }

            _value = null;
            return false;
        }

        /// <summary>http://doc.redisfans.com/string/set.html#id2</summary>
        public override void Release()
        {
            Interlocked.Exchange(ref _timer, null)?.Dispose();

            var value = Interlocked.Exchange(ref _value, null);
            if (value == null) return;

            if (GetThenDel(_key, value) < 0) _logger?.LogWarning($"{_key}/Release lock fail");
            else _logger?.LogInformation($"{_key}/Release lock");

            base.Release();
        }

        public override async ValueTask ReleaseAsync()
        {
            Interlocked.Exchange(ref _timer, null)?.Dispose();

            var value = Interlocked.Exchange(ref _value, null);
            if (value == null) return;

            if (await GetThenDelAsync(_key, value) < 0) _logger?.LogWarning($"{_key}/Release lock fail");
            else _logger?.LogInformation($"{_key}/Release lock");

            await base.ReleaseAsync();
        }

        private void GetNewGuid()
        {
            lock (_syncObj)
            {
                if (_value != null) throw new DistributedLockException();

                var id = Guid.NewGuid();

                _value = id.ToByteArray();

                _logger?.LogDebug($"{_key}/NewGuid: {id:D}");
            }
        }

        private void StartPing()
        {
            _logger?.LogInformation($"{_key}/Wait success, start ping");

            _timer = new Timer(Ping, this, _options.LockMs / 3, _options.LockMs / 3);
        }

        private static async void Ping(object state)
        {
            var self = (CSRedisLock)state;

            try
            {
                await self._client.SetAsync(self._key, self._value, TimeSpan.FromMilliseconds(self._options.LockMs));

                self._logger?.LogDebug($"{self._key}/Ping success");
            }
            catch (Exception ex)
            {
                self._logger?.LogWarning(default, ex, $"{self._key}/Ping fail");
            }
        }

        private bool Lock(string key, byte[] value) =>
            _client.Set(key, value, TimeSpan.FromMilliseconds(_options.LockMs * 3 + 1000), RedisExistence.Nx);

        private Task<bool> LockAsync(string key, byte[] value) =>
            _client.SetAsync(key, value, TimeSpan.FromMilliseconds(_options.LockMs * 3 + 1000), RedisExistence.Nx);

        private long GetThenDel(string key, byte[] val) =>
            (long)_client.Eval(@"if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1]);
end
return -1;", key, val);

        private async Task<long> GetThenDelAsync(string key, byte[] val) =>
            (long)await _client.EvalAsync(@"if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1]);
end
return -1;", key, val);
    }
}
