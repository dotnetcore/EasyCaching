namespace EasyCaching.UnitTests
{
    using System;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using EasyCaching.Core.Interceptor;
    using Xunit;

    public class DefaultEasyCachingKeyGeneratorTest
    {
        private readonly DefaultEasyCachingKeyGenerator _keyGenerator;

        public DefaultEasyCachingKeyGeneratorTest()
        {
            _keyGenerator = new DefaultEasyCachingKeyGenerator();
        }

        [Fact]
        public void Generate_CacheKey_With_No_Params_Method_Should_Succeed()
        {
            var methodName = "Method1";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { }, string.Empty);

            Assert.Equal($"Demo:Method1:0", key);
        }

        [Fact]
        public void Generate_CacheKey_With_No_Params_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method1";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { }, "GenKey");

            Assert.Equal($"GenKey:0", key);
        }

        [Fact]
        public void Generate_CacheKey_With_Int_Param_Method_Should_Succeed()
        {
            var methodName = "Method2";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { 10 }, string.Empty);

            Assert.Equal($"Demo:Method2:10", key);
        }

        [Fact]
        public void Generate_CacheKey_With_Int_Param_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method2";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { 10 }, "GenKey");

            Assert.Equal($"GenKey:10", key);
        }

        [Fact]
        public void Generate_CacheKey_With_String_Param_Method_Should_Succeed()
        {
            var methodName = "Method3";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { "str" }, string.Empty);

            Assert.Equal($"Demo:Method3:str", key);
        }

        [Fact]
        public void Generate_CacheKey_With_String_Param_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method3";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { "str" }, "GenKey");

            Assert.Equal($"GenKey:str", key);
        }

        [Fact]
        public void Generate_CacheKey_With_DateTime_Param_Method_Should_Succeed()
        {
            var methodName = "Method4";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, null, string.Empty);

            Assert.Equal($"Demo:Method4:0", key);
        }

        [Fact]
        public void Generate_CacheKey_With_DateTime_Param_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method4";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, null, "GenKey");

            Assert.Equal($"GenKey:0", key);
        }

        [Fact]
        public void Generate_CacheKey_With_ICachable_Param_Method_Should_Succeed()
        {
            var methodName = "Method5";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { new Product() }, string.Empty);

            Assert.Equal($"Demo:Method5:1000", key);
        }

        [Fact]
        public void Generate_CacheKey_With_ICachable_Param_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method5";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { new Product() }, "GenKey");

            Assert.Equal($"GenKey:1000", key);
        }

        [Fact]
        public void Generate_CacheKeyPrefix_Without_PrefixParam_Should_Succeed()
        {
            var methodName = "Method3";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKeyPrefix(methodInfo, string.Empty);

            Assert.Equal($"Demo:Method3:", key);
        }

        [Fact]
        public void Generate_CacheKeyPrefix_With_PrefixParam_Should_Succeed()
        {
            var methodName = "Method3";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKeyPrefix(methodInfo, "prefix");

            Assert.Equal($"prefix:", key);
        }

        private string SHA1AndBase64(string str)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                return Convert.ToBase64String(data, Base64FormattingOptions.None);
            }
        }

        public class Demo
        {
            public void Method1() { }
            public void Method2(int num = 10) { }
            public void Method3(string str = "str") { }
            public void Method4(DateTime dt) { }
            public void Method5(Product pro) { }
        }

        public class Product : ICachable
        {
            public string CacheKey => Id.ToString();

            public int Id { get; set; } = 1000;
        }

    }
}