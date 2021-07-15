using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Core.DistributedLock
{
    public class DistributedLock : MemoryLock
    {
        private readonly IDistributedLockProvider _provider;
        private readonly object _syncObj = new object();
        private readonly DistributedLockOptions _options;
        private readonly ILogger _logger;

        private byte[] _value;
        private Timer _timer;

        public DistributedLock(string key, IDistributedLockProvider provider, DistributedLockOptions options, ILoggerFactory loggerFactory = null) : base(key)
        {
            _provider = provider;
            _options = options;
            _logger = loggerFactory?.CreateLogger(GetType().FullName);
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
                        if (_provider.Add(Key, _value, _options.MaxTtl))
                        {
                            StartPing();

                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(default, ex, ex.Message);

                        if (!_provider.CanRetry(ex)) break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        _value = null;

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    Thread.Sleep(Math.Max(0, Math.Min(100, millisecondsTimeout - (int)sw.ElapsedMilliseconds)));
                } while (sw.ElapsedMilliseconds < millisecondsTimeout);

                _logger?.LogWarning($"{Key}/Wait fail");

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
                        if (await _provider.AddAsync(Key, _value, _options.MaxTtl))
                        {
                            StartPing();

                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(default, ex, ex.Message);

                        if (!_provider.CanRetry(ex)) break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        _value = null;

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(Math.Max(0, Math.Min(100, millisecondsTimeout - (int)sw.ElapsedMilliseconds)), cancellationToken);
                } while (sw.ElapsedMilliseconds < millisecondsTimeout);

                _logger?.LogWarning($"{Key}/Wait fail");

                await base.ReleaseAsync();
            }

            _value = null;
            return false;
        }

        public override void Release()
        {
            Interlocked.Exchange(ref _timer, null)?.Dispose();

            var value = Interlocked.Exchange(ref _value, null);
            if (value == null) return;

            try
            {
                if (_provider.Delete(Key, value)) _logger?.LogInformation($"{Key}/Release lock");
                else _logger?.LogWarning($"{Key}/Release lock fail");
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
                if (await _provider.DeleteAsync(Key, value)) _logger?.LogInformation($"{Key}/Release lock");
                else _logger?.LogWarning($"{Key}/Release lock fail");
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

                _logger?.LogDebug($"{Key}/NewGuid: {id:D}");
            }
        }

        private void StartPing()
        {
            _logger?.LogInformation($"{Key}/Wait success, start ping");

            _timer = new Timer(Ping, this, _options.DueTime, _options.Period);
        }

        private static async void Ping(object state)
        {
            var self = (DistributedLock)state;

            try
            {
                await self._provider.SetAsync(self.Key, self._value, self._options.MaxTtl);

                self._logger?.LogDebug($"{self.Key}/Ping success");
            }
            catch (Exception ex)
            {
                self._logger?.LogWarning(default, ex, $"{self.Key}/Ping fail");
            }
        }
    }
}
