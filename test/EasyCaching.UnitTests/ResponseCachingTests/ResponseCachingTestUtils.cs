namespace EasyCaching.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using EasyCaching.ResponseCaching;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;
    using EasyCaching.InMemory;
    using Microsoft.AspNetCore.ResponseCaching;

    public class ResponseCachingTestUtils
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

        internal static Task TestRequestDelegateWrite(HttpContext context)
        {
            var uniqueId = Guid.NewGuid().ToString();
            if (TestRequestDelegate(context, uniqueId))
            {
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
                yield return new WebHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddEasyCachingResponseCaching(responseCachingOptions =>
                        {
                            if (options != null)
                            {
                                responseCachingOptions.MaximumBodySize = options.MaximumBodySize;
                                responseCachingOptions.UseCaseSensitivePaths = options.UseCaseSensitivePaths;
                            }
                        });
                        // Test with in memory ResponseCache
                        services.AddDefaultInMemoryCache();
                    })
                    .Configure(app =>
                    {
                        configureDelegate(app);
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

    internal class DummySendFileFeature : IHttpSendFileFeature
    {
        public Task SendFileAsync(string path, long offset, long? count, CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }
    }
}
