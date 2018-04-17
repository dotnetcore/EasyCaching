//namespace EasyCaching.UnitTests
//{
//    using EasyCaching.Core;
//    using EasyCaching.Core.Internal;
//    using EasyCaching.HybridCache;
//    using EasyCaching.InMemory;
//    using EasyCaching.Memcached;
//    using EasyCaching.Redis;
//    using EasyCaching.SQLite;
//    using Microsoft.Extensions.DependencyInjection;
//    using System;
//    using Xunit;

//    public class CachingServiceCollectionExtensionsTest
//    {
//        [Fact]
//        public void AddDefaultInMemoryCache_Should_Get_InMemoryCachingProvider()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultInMemoryCache();

//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();

//            Assert.IsType<DefaultInMemoryCachingProvider>(cachingProvider);
//        }

//        [Fact]
//        public void AddDefaultRedisCache_Should_Get_RedisDatabaseProvider()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultRedisCache(option =>
//            {
//                option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
//                option.Password = "";
//            });

//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var dbProvider = serviceProvider.GetService<IRedisDatabaseProvider>();
//            var cachingProvider = serviceProvider.GetService<IEasyCachingProvider>();

//            Assert.IsType<RedisDatabaseProvider>(dbProvider);
//            Assert.IsType<DefaultRedisCachingProvider>(cachingProvider);
//        }

//        [Fact]
//        public void AddSQLiteCache_Should_Get_SQLiteCachingProvider()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddSQLiteCache(options =>
//            {
//                options.FileName = "";
//                options.FilePath = "";
//                options.CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default;
//                options.OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory;
//            });

//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var provider = serviceProvider.GetService<IEasyCachingProvider>();

//            Assert.IsType<DefaultSQLiteCachingProvider>(provider);
//        }

//        [Fact]
//        public void AddDefaultMemcached_Should_Get_DefaultMemcachedCachingProvider()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultMemcached(options => options.AddServer("127.0.0.1", 11211));
//            services.AddLogging();

//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var provider = serviceProvider.GetService<IEasyCachingProvider>();

//            Assert.IsType<DefaultMemcachedCachingProvider>(provider);
//        }

//        [Fact]
//        public void AddDefaultHybridCache_Should_Get_HybridCachingProvider()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultHybridCache();
//            services.AddMemoryCache();
//            services.AddDefaultInMemoryCacheForHybrid();
//            services.AddDefaultRedisCacheForHybrid(option =>
//            {
//                option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
//                option.Password = "";
//            });
//            services.AddSingleton(factory =>
//            {
//                Func<string, IEasyCachingProvider> accesor = key =>
//                {
//                    if (key.Equals(HybridCachingKeyType.LocalKey))
//                    {
//                        return factory.GetService<DefaultInMemoryCachingProvider>();
//                    }
//                    else if (key.Equals(HybridCachingKeyType.DistributedKey))
//                    {
//                        return factory.GetService<DefaultRedisCachingProvider>();
//                    }
//                    else
//                    {
//                        return null;
//                    }
//                };
//                return accesor;
//            });


//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var provider = serviceProvider.GetService<IHybridCachingProvider>();

//            Assert.IsType<HybridCachingProvider>(provider);
//        }

//        [Fact]
//        public void AddDefaultInMemoryCacheForHybrid_Should_Succeed()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultInMemoryCacheForHybrid();
//            services.AddSingleton(factory =>
//            {
//                return GetFunc(factory, 1);
//            });

//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var func = serviceProvider.GetService<Func<string, IEasyCachingProvider>>();

//            Assert.IsType<DefaultInMemoryCachingProvider>(func(HybridCachingKeyType.LocalKey));
//        }

//        [Fact]
//        public void AddDefaultRedisCacheForHybrid_Should_Succeed()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultRedisCacheForHybrid(option =>
//            {
//                option.Endpoints.Add(new ServerEndPoint("127.0.0.1", 6379));
//                option.Password = "";
//            });
//            services.AddSingleton(factory =>
//            {
//                return GetFunc(factory, 2);
//            });

//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var func = serviceProvider.GetService<Func<string, IEasyCachingProvider>>();

//            Assert.IsType<DefaultRedisCachingProvider>(func(HybridCachingKeyType.DistributedKey));
//        }

//        [Fact]
//        public void AddSQLiteCacheForHybrid_Should_Succeed()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddSQLiteCacheForHybrid(options =>
//            {
//                options.FileName = "";
//                options.FilePath = "";
//                options.CacheMode = Microsoft.Data.Sqlite.SqliteCacheMode.Default;
//                options.OpenMode = Microsoft.Data.Sqlite.SqliteOpenMode.Memory;
//            });
//            services.AddSingleton(factory =>
//            {
//                return GetFunc(factory, 3);
//            });

//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var func = serviceProvider.GetService<Func<string, IEasyCachingProvider>>();

//            Assert.IsType<DefaultSQLiteCachingProvider>(func(HybridCachingKeyType.LocalKey));
//        }

//        [Fact]
//        public void AddDefaultMemcachedForHybrid_Should_Succeed()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultMemcachedForHybrid(options => options.AddServer("127.0.0.1", 11211));
//            services.AddLogging();
//            services.AddSingleton(factory =>
//            {
//                return GetFunc(factory, 4);
//            });

//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var func = serviceProvider.GetService<Func<string, IEasyCachingProvider>>();

//            Assert.IsType<DefaultMemcachedCachingProvider>(func(HybridCachingKeyType.DistributedKey));
//        }

//        private Func<string, IEasyCachingProvider> GetFunc(IServiceProvider factory, int type)
//        {
//            Func<string, IEasyCachingProvider> accesor = key =>
//            {
//                if (key.Equals(HybridCachingKeyType.LocalKey))
//                {
//                    if (type == 1)
//                    {
//                        return factory.GetService<DefaultInMemoryCachingProvider>();
//                    }
//                    else if (type == 3)
//                    {
//                        return factory.GetService<DefaultSQLiteCachingProvider>();
//                    }
//                    else
//                    {
//                        throw new ArgumentException($"Not support type for {HybridCachingKeyType.LocalKey}");
//                    }
//                }
//                else if (key.Equals(HybridCachingKeyType.DistributedKey))
//                {
//                    if (type == 2)
//                    {
//                        return factory.GetService<DefaultRedisCachingProvider>();
//                    }
//                    else if (type == 4)
//                    {
//                        return factory.GetService<DefaultMemcachedCachingProvider>();
//                    }
//                    else
//                    {
//                        throw new ArgumentException($"Not support type for {HybridCachingKeyType.DistributedKey}");
//                    }
//                }
//                else
//                {
//                    return null;
//                }
//            };
//            return accesor;
//        }
//    }
//}

