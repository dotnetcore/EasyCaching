namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using EasyCaching.ResponseCaching;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.ObjectPool;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Net.Http.Headers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipelines;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class ResponseCachingTestUtils
    {
        //borrowed from https://github.com/aspnet/ResponseCaching/blob/dev/test/Microsoft.AspNetCore.ResponseCaching.Tests/TestUtils.cs

        private static bool TestRequestDelegate(HttpContext context, string guid)
        {
            var headers = context.Response.GetTypedHeaders();

            var expires = context.Request.Query["Expires"];
            if (!string.IsNullOrEmpty(expires))
            {
                headers.Expires = DateTimeOffset.Now.AddSeconds(int.Parse(expires));
            }

            if (headers.CacheControl == null)
            {
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = string.IsNullOrEmpty(expires) ? TimeSpan.FromSeconds(10) : (TimeSpan?)null
                };
            }
            else
            {
                headers.CacheControl.Public = true;
                headers.CacheControl.MaxAge = string.IsNullOrEmpty(expires) ? TimeSpan.FromSeconds(10) : (TimeSpan?)null;
            }
            headers.Date = DateTimeOffset.UtcNow;
            headers.Headers["X-Value"] = guid;

            if (context.Request.Method != "HEAD")
            {
                return true;
            }
            return false;
        }

        internal static async Task TestRequestDelegateWriteAsync(HttpContext context)
        {
            var uniqueId = Guid.NewGuid().ToString();
            if (TestRequestDelegate(context, uniqueId))
            {
                await context.Response.WriteAsync(uniqueId);
            }
        }

        internal static async Task TestRequestDelegateSendFileAsync(HttpContext context)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestDocument.txt");
            var uniqueId = Guid.NewGuid().ToString();
            if (TestRequestDelegate(context, uniqueId))
            {
                await context.Response.SendFileAsync(path, 0, null);
                await context.Response.WriteAsync(uniqueId);
            }
        }

        internal static Task TestRequestDelegateWrite(HttpContext context)
        {
            var uniqueId = Guid.NewGuid().ToString();
            if (TestRequestDelegate(context, uniqueId))
            {
                var feature = context.Features.Get<IHttpBodyControlFeature>();
                if (feature != null)
                {
                    feature.AllowSynchronousIO = true;
                }
                context.Response.Write(uniqueId);
            }
            return Task.CompletedTask;
        }


        internal static IEnumerable<IWebHostBuilder> CreateBuildersWithResponseCaching(
            Action<IApplicationBuilder> configureDelegate = null,
            ResponseCachingOptions options = null,
            Action<HttpContext> contextAction = null)
        {
            return CreateBuildersWithResponseCaching(configureDelegate, options, new RequestDelegate[]
                {
                    context =>
                    {
                        contextAction?.Invoke(context);
                        return TestRequestDelegateWrite(context);
                    },
                    context =>
                    {
                        contextAction?.Invoke(context);
                        return TestRequestDelegateWriteAsync(context);
                    },
                    context =>
                    {
                        contextAction?.Invoke(context);
                        return TestRequestDelegateSendFileAsync(context);
                    },
                });
        }

        private static IEnumerable<IWebHostBuilder> CreateBuildersWithResponseCaching(
            Action<IApplicationBuilder> configureDelegate = null,
            ResponseCachingOptions options = null,
            IEnumerable<RequestDelegate> requestDelegates = null)
        {
            if (configureDelegate == null)
            {
                configureDelegate = app => { };
            }
            if (requestDelegates == null)
            {
                requestDelegates = new RequestDelegate[]
                {
                    TestRequestDelegateWriteAsync,
                    TestRequestDelegateWrite
                };
            }

            foreach (var requestDelegate in requestDelegates)
            {
                // Test with in memory ResponseCache
                yield return new WebHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddEasyCaching(c => c.UseInMemory("resp"));
                        services.AddEasyCachingResponseCaching(responseCachingOptions =>
                        {
                            if (options != null)
                            {
                                responseCachingOptions.MaximumBodySize = options.MaximumBodySize;
                                responseCachingOptions.UseCaseSensitivePaths = options.UseCaseSensitivePaths;
                                //responseCachingOptions.SystemClock = options.SystemClock;
                            }
                        }, "resp");                        
                    })
                    .Configure(app =>
                    {
                        configureDelegate(app);
                        //app.UseResponseCaching();
                        app.UseEasyCachingResponseCaching();
                        app.Run(requestDelegate);
                    });
            }
        }
      
        public static HttpRequestMessage CreateRequest(string method, string requestUri)
        {
            return new HttpRequestMessage(new HttpMethod(method), requestUri);
        }
    }

    internal static class HttpResponseWritingExtensions
    {
        internal static void Write(this HttpResponse response, string text)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            byte[] data = Encoding.UTF8.GetBytes(text);
            response.Body.Write(data, 0, data.Length);
        }
    }


    internal class TestResponseCachingPolicyProvider : IResponseCachingPolicyProvider
    {
        public bool AllowCacheLookupValue { get; set; } = false;
        public bool AllowCacheStorageValue { get; set; } = false;
        public bool AttemptResponseCachingValue { get; set; } = false;
        public bool IsCachedEntryFreshValue { get; set; } = true;
        public bool IsResponseCacheableValue { get; set; } = true;

        public bool AllowCacheLookup(ResponseCachingContext context) => AllowCacheLookupValue;

        public bool AllowCacheStorage(ResponseCachingContext context) => AllowCacheStorageValue;

        public bool AttemptResponseCaching(ResponseCachingContext context) => AttemptResponseCachingValue;

        public bool IsCachedEntryFresh(ResponseCachingContext context) => IsCachedEntryFreshValue;

        public bool IsResponseCacheable(ResponseCachingContext context) => IsResponseCacheableValue;
    }

    internal class TestResponseCachingKeyProvider : IResponseCachingKeyProvider
    {
        private readonly string _baseKey;
        private readonly StringValues _varyKey;

        public TestResponseCachingKeyProvider(string lookupBaseKey = null, StringValues? lookupVaryKey = null)
        {
            _baseKey = lookupBaseKey;
            if (lookupVaryKey.HasValue)
            {
                _varyKey = lookupVaryKey.Value;
            }
        }

        public IEnumerable<string> CreateLookupVaryByKeys(ResponseCachingContext context)
        {
            foreach (var varyKey in _varyKey)
            {
                yield return _baseKey + varyKey;
            }
        }

        public string CreateBaseKey(ResponseCachingContext context)
        {
            return _baseKey;
        }

        public string CreateStorageVaryByKey(ResponseCachingContext context)
        {
            throw new NotImplementedException();
        }
    }

    internal class TestResponseCache : IResponseCache
    {
        private readonly IDictionary<string, IResponseCacheEntry> _storage = new Dictionary<string, IResponseCacheEntry>();
        public int GetCount { get; private set; }
        public int SetCount { get; private set; }

        public IResponseCacheEntry Get(string key)
        {
            GetCount++;
            try
            {
                return _storage[key];
            }
            catch
            {
                return null;
            }
        }

        public Task<IResponseCacheEntry> GetAsync(string key)
        {
            return Task.FromResult(Get(key));
        }

        public void Set(string key, IResponseCacheEntry entry, TimeSpan validFor)
        {
            SetCount++;
            _storage[key] = entry;
        }

        public Task SetAsync(string key, IResponseCacheEntry entry, TimeSpan validFor)
        {
            Set(key, entry, validFor);
            return Task.CompletedTask;
        }
    }
}