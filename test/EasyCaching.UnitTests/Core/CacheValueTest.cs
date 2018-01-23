namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using Xunit;

    public class CacheValueTest
    {
        [Fact]
        public void Create_CacheValue_Should_Succeed()
        {
            CacheValue<string> value = new CacheValue<string>("value", true);

            Assert.True(value.HasValue);
            Assert.Equal("value", value.Value);
        }

        [Fact]
        public void Null_CacheValue_Should_Succeed()
        {
            CacheValue<string> value = CacheValue<string>.Null;

            Assert.True(value.HasValue);
            Assert.Equal(default(string), value.Value);
        }

        [Fact]
        public void NoValue_CacheValue_Should_Succeed()
        {
            CacheValue<string> value = CacheValue<string>.NoValue;

            Assert.False(value.HasValue);
            Assert.Equal(default(string), value.Value);
        }

        [Fact]
        public void ToString_Should_Get_Cached_Value()
        {
            CacheValue<string> value = new CacheValue<string>("value", true);

            Assert.Equal("value", value.ToString());
        }

        [Fact]
        public void ToString_Should_Get_Null_Value()
        {
            CacheValue<string> value = CacheValue<string>.Null;

            Assert.Equal("<null>", value.ToString());
        }
    }
}
