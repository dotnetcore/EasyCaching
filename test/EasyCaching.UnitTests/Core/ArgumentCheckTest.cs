namespace EasyCaching.UnitTests
{
    using EasyCaching.Core;
    using System;
    using Xunit;
    using System.Collections.Generic;

    public class ArgumentCheckTest
    {
        [Fact]
        public void NotNull_Should_Throw_ArgumentNullException_When_Argument_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => ArgumentCheck.NotNull(null, "name"));
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

        [Fact]
        public void NotNegativeOrZero_Should_Throw_ArgumentOutOfRangeException_When_Argument_Is_Negative()
        {
            var ts = new TimeSpan(0, 0, -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentCheck.NotNegativeOrZero(ts, nameof(ts)));
        }

        [Fact]
        public void NotNegativeOrZero_Should_Throw_ArgumentOutOfRangeException_When_Argument_Is_Zero()
        {
            var ts = TimeSpan.Zero;
            Assert.Throws<ArgumentOutOfRangeException>(() => ArgumentCheck.NotNegativeOrZero(ts, nameof(ts)));
        }

        [Fact]
        public void NotNegativeOrZero_Should_Not_Throw_ArgumentOutOfRangeException_When_Argument_Is_NotNegativeOrZero()
        {
            var ts = new TimeSpan(0, 0, 1);
            var ex = Record.Exception(() => ArgumentCheck.NotNegativeOrZero(ts, nameof(ts)));

            Assert.Null(ex);
        }

        [Fact]
        public void NotNullAndCountGTZero_List_Should_Throw_ArgumentNullException_When_Argument_Is_Null()
        {
            List<string> list = null;

            Assert.Throws<ArgumentNullException>(() => ArgumentCheck.NotNullAndCountGTZero(list, nameof(list)));
        }

        [Fact]
        public void NotNullAndCountGTZero_List_Should_Throw_ArgumentNullException_When_Argument_Is_Empty()
        {
            var list = new List<string>();

            Assert.Throws<ArgumentNullException>(() => ArgumentCheck.NotNullAndCountGTZero(list, nameof(list)));
        }

        [Fact]
        public void NotNullAndCountGTZero_List_Should_Not_Throw_ArgumentNullException_When_Argument_Is_NotNull_And_Empty()
        {
            var list = new List<string> { "abc" };

            var ex = Record.Exception(() => ArgumentCheck.NotNullAndCountGTZero(list, nameof(list)));

            Assert.Null(ex);
        }

        [Fact]
        public void NotNullAndCountGTZero_Dict_Should_Throw_ArgumentNullException_When_Argument_Is_Null()
        {
            Dictionary<string, string> dict = null;

            Assert.Throws<ArgumentNullException>(() => ArgumentCheck.NotNullAndCountGTZero(dict, nameof(dict)));
        }

        [Fact]
        public void NotNullAndCountGTZero_Dict_Should_Throw_ArgumentNullException_When_Argument_Is_Empty()
        {
            var dict = new Dictionary<string, string>();

            Assert.Throws<ArgumentNullException>(() => ArgumentCheck.NotNullAndCountGTZero(dict, nameof(dict)));
        }

        [Fact]
        public void NotNullAndCountGTZero_Dict_Should_Not_Throw_ArgumentNullException_When_Argument_Is_NotNull_And_Empty()
        {
            var dict = new Dictionary<string, string> { { "abc", "123" } };

            var ex = Record.Exception(() => ArgumentCheck.NotNullAndCountGTZero(dict, nameof(dict)));

            Assert.Null(ex);
        }
    }
}
