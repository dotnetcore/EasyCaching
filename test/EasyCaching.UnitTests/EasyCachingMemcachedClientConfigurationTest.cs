//namespace EasyCaching.UnitTests
//{
//    using EasyCaching.Memcached;
//    using Enyim.Caching.Configuration;
//    using Enyim.Caching.Memcached;
//    using Microsoft.Extensions.DependencyInjection;
//    using Xunit;
//    using System;

//    public class EasyCachingMemcachedClientConfigurationTest
//    {      
//        [Fact]
//        public void Not_Set_Transcoder_And_SerializationType_Should_Return_DefaultTranscoder()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultMemcached(options =>
//            {
//                options.AddServer("127.0.0.1", 11211);
//            });
//            services.AddLogging();
//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var configuration = serviceProvider.GetService<IMemcachedClientConfiguration>() as EasyCachingMemcachedClientConfiguration;

//            Assert.NotNull(configuration);
//            Assert.IsType<DefaultTranscoder>(configuration.Transcoder);
//        }

//        [Fact]
//        public void Only_Set_BinaryFormatterTranscoder_Should_Return_BinaryFormatterTranscoder()
//        {
//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultMemcached(options =>
//            {
//                options.AddServer("127.0.0.1", 11211);
//                options.Transcoder = "BinaryFormatterTranscoder";

//            });
//            services.AddLogging();
//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var configuration = serviceProvider.GetService<IMemcachedClientConfiguration>() as EasyCachingMemcachedClientConfiguration;

//            Assert.NotNull(configuration);
//            Assert.IsType<Enyim.Caching.Memcached.Transcoders.BinaryFormatterTranscoder>(configuration.Transcoder);
//        }

//        [Fact]
//        public void Set_FormatterTranscoder_And_SerializationType_Should_Succeed()
//        {
//            string formatterTranscoder = "EasyCaching.Memcached.FormatterTranscoder,EasyCaching.Memcached";


//            IServiceCollection services = new ServiceCollection();
//            services.AddDefaultMemcached(options =>
//            {
//                options.AddServer("127.0.0.1", 11211);
//                options.Transcoder = formatterTranscoder;
//                options.SerializationType = "EasyCaching.Serialization.MessagePack.DefaultMessagePackSerializer,EasyCaching.Serialization.MessagePack";
//            });
//            services.AddLogging();
//            IServiceProvider serviceProvider = services.BuildServiceProvider();
//            var configuration = serviceProvider.GetService<IMemcachedClientConfiguration>() as EasyCachingMemcachedClientConfiguration;

//            Assert.NotNull(configuration);
//            Assert.IsType(Type.GetType(formatterTranscoder),configuration.Transcoder);                       
//        }
//    }
//}
