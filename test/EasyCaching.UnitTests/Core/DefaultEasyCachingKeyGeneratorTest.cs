namespace EasyCaching.UnitTests
{
    using System;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using EasyCaching.Core;
    using EasyCaching.Core.Internal;
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

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[]{} ,string.Empty);

            var keyBeforeHash = $"Demo:Method1:0";

            Assert.Equal(SHA1AndBase64(keyBeforeHash),key);
        }

        [Fact]
        public void Generate_CacheKey_With_No_Params_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method1";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { }, "GenKey");

            var keyBeforeHash = $"GenKey:0";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
        }

        [Fact]
        public void Generate_CacheKey_With_Int_Param_Method_Should_Succeed()
        {
            var methodName = "Method2";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { 10 } ,string.Empty);

            var keyBeforeHash = $"Demo:Method2:10";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
        }

        [Fact]
        public void Generate_CacheKey_With_Int_Param_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method2";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { 10 }, "GenKey");

            var keyBeforeHash = $"GenKey:10";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
        }

        [Fact]
        public void Generate_CacheKey_With_String_Param_Method_Should_Succeed()
        {
            var methodName = "Method3";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[]{ "str" } ,string.Empty);

            var keyBeforeHash = $"Demo:Method3:str";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
        }

        [Fact]
        public void Generate_CacheKey_With_String_Param_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method3";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { "str" }, "GenKey");

            var keyBeforeHash = $"GenKey:str";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
        }

        [Fact]
        public void Generate_CacheKey_With_DateTime_Param_Method_Should_Succeed()
        {
            var methodName = "Method4";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, null, string.Empty);

            var keyBeforeHash = $"Demo:Method4:0";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
        }

        [Fact]
        public void Generate_CacheKey_With_DateTime_Param_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method4";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, null, "GenKey");

            var keyBeforeHash = $"GenKey:0";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
        }

        [Fact]
        public void Generate_CacheKey_With_ICachable_Param_Method_Should_Succeed()
        {
            var methodName = "Method5";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { new Product() }, string.Empty);

            var keyBeforeHash = $"Demo:Method5:1000";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
        }

        [Fact]
        public void Generate_CacheKey_With_ICachable_Param_And_Prefix_Method_Should_Succeed()
        {
            var methodName = "Method5";
            MethodInfo methodInfo = typeof(Demo).GetMethod(methodName);

            var key = _keyGenerator.GetCacheKey(methodInfo, new object[] { new Product() }, "GenKey");

            var keyBeforeHash = $"GenKey:1000";

            Assert.Equal(SHA1AndBase64(keyBeforeHash), key);
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
