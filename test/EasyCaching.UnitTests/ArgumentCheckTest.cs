namespace EasyCaching.UnitTests
{
    using EasyCaching.Core.Internal;
    using System;
    using Xunit;

    public class ArgumentCheckTest
    {
        [Fact]
        public void NotNull_Should_Throw_ArgumentNullException_When_Argument_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(()=>ArgumentCheck.NotNull(null,"name"));
        }

        [Theory]
        [InlineData(10)]
        [InlineData("10")]
        public void NotNull_Should_Not_Throw_ArgumentNullException_When_Argument_Is_Not_Null(object obj)
        {
            var ex = Record.Exception(() => ArgumentCheck.NotNull(obj, "name"));

            Assert.Null(ex);                    
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void NotNullOrWhiteSpace_Should_Throw_ArgumentNullException_When_Argument_Is_NullOrWhiteSpace(string str)
        {
            Assert.Throws<ArgumentNullException>(() => ArgumentCheck.NotNullOrWhiteSpace(str, "name"));
        }

        [Theory]
        [InlineData("1")]
        [InlineData("  1")]
        public void NotNullOrWhiteSpace_Should_Not_Throw_ArgumentNullException_When_Argument_Is_NotNullOrWhiteSpace(string str)
        {
            var ex = Record.Exception(() => ArgumentCheck.NotNullOrWhiteSpace(str, "name"));

            Assert.Null(ex);
        }
    }
}
