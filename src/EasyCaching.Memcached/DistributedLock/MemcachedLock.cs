using EasyCaching.Core.Configurations;
using EasyCaching.Core.DistributedLock;
using Enyim.Caching.Memcached;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Memcached.DistributedLock
{
    public class MemcachedLock : MemoryLock
    {
        private readonly object _syncObj = new object();
        private readonly string _key;
        private readonly EasyCachingMemcachedClient _memcachedClient;
        private readonly BaseProviderOptions _options;
        private readonly ILogger<MemcachedLock> _logger;
        private byte[] _value;
        private Timer _timer;

        public MemcachedLock(string key, EasyCachingMemcachedClient memcachedClient, BaseProviderOptions options, ILogger<MemcachedLock> logger) : base(key)
        {
            _key = key;
            _memcachedClient = memcachedClient;
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

                        if (!(ex is MemcachedClientException)) break;
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

                        if (!(ex is MemcachedClientException)) break;
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

            try
            {
                if (_memcachedClient.Remove(_key)) _logger?.LogWarning($"{_key}/Release lock fail");
                else _logger?.LogInformation($"{_key}/Release lock");
            }
            finally
            {
                base.Release();
            }
        }

        public override async ValueTask ReleaseAsync()
        {
            Interlocked.Exchange(ref _timer, null)?.Dispose();

            var value = Interlocked.Exchange(ref _value, null);
            if (value == null) return;

            try
            {
                if (await _memcachedClient.RemoveAsync(_key)) _logger?.LogWarning($"{_key}/Release lock fail");
                else _logger?.LogInformation($"{_key}/Release lock");
            }
            finally
            {
                await base.ReleaseAsync();
            }
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
            var self = (MemcachedLock)state;

            try
            {
                await self._memcachedClient.StoreAsync(StoreMode.Set, self._key, self._value, TimeSpan.FromMilliseconds(self._options.LockMs));

                self._logger?.LogDebug($"{self._key}/Ping success");
            }
            catch (Exception ex)
            {
                self._logger?.LogWarning(default, ex, $"{self._key}/Ping fail");
            }
        }

        private bool Lock(string key, byte[] value) =>
            _memcachedClient.Store(StoreMode.Add, key, value, TimeSpan.FromSeconds(_options.LockMs * 3 + 1000));

        private Task<bool> LockAsync(string key, byte[] value) =>
            _memcachedClient.StoreAsync(StoreMode.Add, key, value, TimeSpan.FromSeconds(_options.LockMs * 3 + 1000));
    }
}
