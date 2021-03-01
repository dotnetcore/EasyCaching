namespace EasyCaching.UnitTests
{
    using EasyCaching.Core.Bus;
    using EasyCaching.Decoration.Polly;
    using FakeItEasy;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class PublishFallbackBusDecorationTests
    {
        private const string Topic = "topic";

        private IEasyCachingBus CreateDecoratedProvider() =>
            ServiceBuilders.CreateFakeBus(options =>
            {
                options.BusFactory = CreateBus;

                options.DecorateWithPublishFallback(
                    exceptionFilter: exception => exception is InvalidOperationException);
            });

        private IEasyCachingBus CreateBus()
        {
            var fakeCachingBus = A.Fake<IEasyCachingBus>();

            A.CallTo(() => fakeCachingBus.Publish(Topic, A<EasyCachingMessage>._))
                .Throws(new InvalidOperationException("Exception on publish"));

            A.CallTo(() => fakeCachingBus.PublishAsync(Topic, A<EasyCachingMessage>._, A<CancellationToken>._))
                .ThrowsAsync(new InvalidOperationException("Exception on publish"));

            return fakeCachingBus;
        }

        [Fact]
        public void Publish_Should_Not_Fail()
        {
            var provider = CreateDecoratedProvider();

            provider.Publish(Topic, new EasyCachingMessage());
        }

        [Fact]
        public async Task PublishAsync_Should_Not_Fail()
        {
            var provider = CreateDecoratedProvider();

            await provider.PublishAsync(Topic, new EasyCachingMessage());
        }
    }
}