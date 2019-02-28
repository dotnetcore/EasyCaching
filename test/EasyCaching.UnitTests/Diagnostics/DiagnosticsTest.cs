namespace EasyCaching.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
                if (kvp.Key.Equals("EasyCaching.WriteExistsCacheBefore"))
                {
                    Assert.NotNull(kvp.Value);

                    statsLogged = true;
                }
                else if (kvp.Key.Equals("EasyCaching.WriteExistsCacheAfter"))
                {
                    Assert.NotNull(kvp.Value);

                    statsLogged = true;
                }
            });

            diagnosticListenerObserver.Enable();
            using (DiagnosticListener.AllListeners.Subscribe(diagnosticListenerObserver))
            {
                var cacheKey = "WriteExistsCache-key";

                var res = provider.Exists(cacheKey);

                Assert.True(statsLogged);

                diagnosticListenerObserver.Disable();
            }
        }

        [Fact]
        public void WriteSetCacheTest()
        {
            var statsLogged = false;

            MyCachingProvider provider = new MyCachingProvider();

            FakeDiagnosticListenerObserver diagnosticListenerObserver = new FakeDiagnosticListenerObserver(kvp =>
            {
                if (kvp.Key.Equals("EasyCaching.WriteSetCacheBefore"))
                {
                    Assert.NotNull(kvp.Value);

                    statsLogged = true;
                }
            });

            diagnosticListenerObserver.Enable();
            using (DiagnosticListener.AllListeners.Subscribe(diagnosticListenerObserver))
            {
                var cacheKey = "WriteSetCache-key";

                provider.Set(cacheKey, "aa", TimeSpan.FromSeconds(10));

                Assert.True(statsLogged);

                diagnosticListenerObserver.Disable();
            }
        }

        [Fact]
        public void WriteFlushCacheTest()
        {
            var statsLogged = false;

            MyCachingProvider provider = new MyCachingProvider();

            FakeDiagnosticListenerObserver diagnosticListenerObserver = new FakeDiagnosticListenerObserver(kvp =>
            {
                if (kvp.Key.Equals("EasyCaching.WriteFlushCacheBefore"))
                {
                    Assert.NotNull(kvp.Value);

                    statsLogged = true;
                }
            });

            diagnosticListenerObserver.Enable();
            using (DiagnosticListener.AllListeners.Subscribe(diagnosticListenerObserver))
            {
                provider.Flush();

                Assert.True(statsLogged);

                diagnosticListenerObserver.Disable();
            }
        }

        [Fact]
        public void WriteRemoveCacheTest()
        {
            var statsLogged = false;

            MyCachingProvider provider = new MyCachingProvider();

            FakeDiagnosticListenerObserver diagnosticListenerObserver = new FakeDiagnosticListenerObserver(kvp =>
            {
                if (kvp.Key.Equals("EasyCaching.WriteRemoveCacheBefore"))
                {
                    Assert.NotNull(kvp.Value);

                    statsLogged = true;
                }
            });

            diagnosticListenerObserver.Enable();
            using (DiagnosticListener.AllListeners.Subscribe(diagnosticListenerObserver))
            {
                var cacheKey = "WriteRemoveCache-key";

                provider.Remove(cacheKey);

                Assert.True(statsLogged);

                diagnosticListenerObserver.Disable();
            }
        }

        [Fact]
        public void WriteGetCacheTest()
        {
            var statsLogged = false;

            MyCachingProvider provider = new MyCachingProvider();

            FakeDiagnosticListenerObserver diagnosticListenerObserver = new FakeDiagnosticListenerObserver(kvp =>
            {
                if (kvp.Key.Equals("EasyCaching.WriteGetCacheBefore"))
                {
                    Assert.NotNull(kvp.Value);

                    statsLogged = true;
                }
            });

            diagnosticListenerObserver.Enable();
            using (DiagnosticListener.AllListeners.Subscribe(diagnosticListenerObserver))
            {
                var cacheKey = "WriteGetCache-key";

                provider.Get<string>(cacheKey);

                Assert.True(statsLogged);

                diagnosticListenerObserver.Disable();
            }
        }
    }
}
