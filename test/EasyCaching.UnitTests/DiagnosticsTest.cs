namespace EasyCaching.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using EasyCaching.Core;
    using EasyCaching.Core.Diagnostics;
    using Xunit;

    public class DiagnosticsTest
    {
        [Fact]
        public void WriteExistsCacheTest()
        {
            var statsLogged = false;

            MyCachingProvider provider = new MyCachingProvider();

            FakeDiagnosticListenerObserver diagnosticListenerObserver = new FakeDiagnosticListenerObserver(kvp =>
            {
                if (kvp.Key.Equals("WriteExistsCache"))
                {
                    Assert.NotNull(kvp.Value);

                    statsLogged = true;
                }
            });

            diagnosticListenerObserver.Enable();
            using (DiagnosticListener.AllListeners.Subscribe(diagnosticListenerObserver))
            {
                var cacheKey = "test-key";

                var res = provider.Exists(cacheKey);

                Assert.True(statsLogged);

                diagnosticListenerObserver.Disable();
            }
        }
    }

    // borrowed from https://github.com/dotnet/corefx/blob/master/src/System.Data.SqlClient/tests/FunctionalTests/FakeDiagnosticListenerObserver.cs
    public sealed class FakeDiagnosticListenerObserver : IObserver<DiagnosticListener>
    {
        private class FakeDiagnosticSourceWriteObserver : IObserver<KeyValuePair<string, object>>
        {
            private readonly Action<KeyValuePair<string, object>> _writeCallback;

            public FakeDiagnosticSourceWriteObserver(Action<KeyValuePair<string, object>> writeCallback)
            {
                _writeCallback = writeCallback;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(KeyValuePair<string, object> value)
            {
                _writeCallback(value);
            }
        }

        private readonly Action<KeyValuePair<string, object>> _writeCallback;
        private bool _writeObserverEnabled;

        public FakeDiagnosticListenerObserver(Action<KeyValuePair<string, object>> writeCallback)
        {
            _writeCallback = writeCallback;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name.Equals(EasyCachingDiagnosticListenerExtensions.DiagnosticListenerName))
            {
                value.Subscribe(new FakeDiagnosticSourceWriteObserver(_writeCallback), IsEnabled);
            }
        }

        public void Enable()
        {
            _writeObserverEnabled = true;
        }
        public void Disable()
        {
            _writeObserverEnabled = false;
        }
        private bool IsEnabled(string s)
        {
            return _writeObserverEnabled;
        }
    }
     
    public class MyCachingProvider : EasyCachingAbstractProvider
    {
        public MyCachingProvider()
        {
            this.ProviderName = "myprovider";
            this.ProviderStats = new CacheStats();
            this.ProviderType = CachingProviderType.InMemory;
            this.ProviderMaxRdSecond = 120;
            this.ProviderOrder = 1;
            this.IsDistributedProvider = false;
        }

        public override bool BaseExists(string cacheKey)
        {
            return true;
        }

        public override Task<bool> BaseExistsAsync(string cacheKey)
        {
            return Task.FromResult(false);
        }

        public override void BaseFlush()
        {

        }

        public override Task BaseFlushAsync()
        {
            return Task.CompletedTask;
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration)
        {
            return CacheValue<T>.NoValue;
        }

        public override CacheValue<T> BaseGet<T>(string cacheKey)
        {
            return CacheValue<T>.NoValue;
        }

        public override IDictionary<string, CacheValue<T>> BaseGetAll<T>(IEnumerable<string> cacheKeys)
        {
            return null;
        }

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetAllAsync<T>(IEnumerable<string> cacheKeys)
        {
            return null;
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration)
        {
            return Task.FromResult( CacheValue<T>.NoValue);
        }

        public override Task<object> BaseGetAsync(string cacheKey, Type type)
        {
            return Task.FromResult<object>(null);
        }

        public override Task<CacheValue<T>> BaseGetAsync<T>(string cacheKey)
        {
            return Task.FromResult(CacheValue<T>.NoValue);
        }

        public override IDictionary<string, CacheValue<T>> BaseGetByPrefix<T>(string prefix)
        {
            return null;
        }

        public override Task<IDictionary<string, CacheValue<T>>> BaseGetByPrefixAsync<T>(string prefix)
        {
            return Task.FromResult<IDictionary<string, CacheValue<T>>>(null);
        }

        public override int BaseGetCount(string prefix = "")
        {
            return 1;
        }

        public override void BaseRefresh<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {

        }

        public override Task BaseRefreshAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        public override void BaseRemove(string cacheKey)
        {

        }

        public override void BaseRemoveAll(IEnumerable<string> cacheKeys)
        {
         
        }

        public override Task BaseRemoveAllAsync(IEnumerable<string> cacheKeys)
        {
            return Task.CompletedTask;
        }

        public override Task BaseRemoveAsync(string cacheKey)
        {
            return Task.CompletedTask;
        }

        public override void BaseRemoveByPrefix(string prefix)
        {

        }

        public override Task BaseRemoveByPrefixAsync(string prefix)
        {
            return Task.CompletedTask;
        }

        public override void BaseSet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
        
        }

        public override void BaseSetAll<T>(IDictionary<string, T> values, TimeSpan expiration)
        {

        }

        public override Task BaseSetAllAsync<T>(IDictionary<string, T> values, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        public override Task BaseSetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Task.CompletedTask;
        }

        public override bool BaseTrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return false;
        }

        public override Task<bool> BaseTrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration)
        {
            return Task.FromResult(false);
        }
    }
}
