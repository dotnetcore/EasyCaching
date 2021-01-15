namespace EasyCaching.UnitTests
{
    using Core;
    using Core.Decoration;
    using FakeItEasy;
    using System;
    using Xunit;
    using static ServiceBuilders;

    public class DecorationTests
    {
        private const string CacheKey = "CacheKey";
        
        protected virtual IEasyCachingProvider CreateDecoratedProvider(Action<FakeOptions> additionalConfig) =>
            CreateFakeProvider(options =>
            {
                options.ProviderFactory = () => CreateFake<IEasyCachingProvider>(fake => fake
                    .CallsTo(x => x.Get<string>(CacheKey))
                    .Returns(new CacheValue<string>("1", true))
                );
                additionalConfig(options);
            });
        
        [Fact]
        public void Get_Non_Decorated_Once_Should_Return_Original_Value()
        {
            var provider = CreateDecoratedProvider(options => { });
            
            var result = provider.Get<string>(CacheKey);

            Assert.Equal("1", result.Value);
        }
        
        [Fact]
        public void Get_Decorated_Once_Should_Return_Value_With_One_Postfix()
        {
            var provider = CreateDecoratedProvider(options => DecorateGetWithPostfix(options, "2"));
            
            var result = provider.Get<string>(CacheKey);

            Assert.Equal("12", result.Value);
        }
        
        [Fact]
        public void Get_Decorated_Twice_Should_Return_Value_With_Two_Postfixes()
        {
            var provider = CreateDecoratedProvider(options =>
            {
                DecorateGetWithPostfix(options, "2");
                DecorateGetWithPostfix(options, "3");
            });
            
            var result = provider.Get<string>(CacheKey);

            Assert.Equal("123", result.Value);
        }

        private static void DecorateGetWithPostfix(FakeOptions options, string postfix)
        {
            options.Decorate((_, __, cachingProviderFactory) => () =>
            {
                var innerProvider = cachingProviderFactory();

                return CreateFake<IEasyCachingProvider>(fake => fake
                    .CallsTo(x => x.Get<string>(CacheKey))
                    .ReturnsLazily(() =>
                    {
                        var value = innerProvider.Get<string>(CacheKey);
                        return new CacheValue<string>(value.Value + postfix, hasValue: true);
                    })
                );
            });
        }
    }
}