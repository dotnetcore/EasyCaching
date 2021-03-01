namespace EasyCaching.UnitTests
{
    using EasyCaching.Core.Bus;
    using EasyCaching.Decoration.Polly;
    using FakeItEasy;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class RetryBusDecorationTests
    {
        private const string Topic = "topic";

        private int attemptsCount = 0;

        private IEasyCachingBus CreateDecoratedProvider() =>
            ServiceBuilders.CreateFakeBus(options =>
            {
                options.BusFactory = CreateBus;

                options.DecorateWithRetry(
                    retryCount: 1,
                    exceptionFilter: exception => exception is InvalidOperationException);
            });

        private IEasyCachingBus CreateBus()
        {
            var fakeCachingBus = A.Fake<IEasyCachingBus>();

            A.CallTo(() => fakeCachingBus.Publish(Topic, A<EasyCachingMessage>._))
                .Throws(() =>
                {
                    attemptsCount++;
                    return new InvalidOperationException("Exception on publish");
                });

            A.CallTo(() => fakeCachingBus.PublishAsync(Topic, A<EasyCachingMessage>._, A<CancellationToken>._))
                .ThrowsAsync(() =>
                {
                    attemptsCount++;
                    return new InvalidOperationException("Exception on publish");
                });

            return fakeCachingBus;
        }

        [Fact]
        public void Publish_Should_Not_Fail()
        {
            var provider = CreateDecoratedProvider();

            var exception = Assert.Throws<InvalidOperationException>(() => provider.Publish(Topic, new EasyCachingMessage()));
            
            Assert.Equal("Exception on publish", exception.Message);
            Assert.Equal(2, attemptsCount);
        }

        [Fact]
        public async Task PublishAsync_Should_Not_Fail()
        {
            var provider = CreateDecoratedProvider();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.PublishAsync(Topic, new EasyCachingMessage()));
            
            Assert.Equal("Exception on publish", exception.Message);
            Assert.Equal(2, attemptsCount);
        }
    }
}