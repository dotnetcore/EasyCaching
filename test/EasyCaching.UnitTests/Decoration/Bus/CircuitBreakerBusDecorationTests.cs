namespace EasyCaching.UnitTests
{
    using EasyCaching.Core.Bus;
    using EasyCaching.Decoration.Polly;
    using FakeItEasy;
    using Polly.CircuitBreaker;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public abstract class CircuitBreakerBusDecorationTests
    {
        protected const string Topic = "topic";

        protected IEasyCachingBus CreateDecoratedProvider() =>
            ServiceBuilders.CreateFakeBus(options =>
            {
                options.BusFactory = CreateBus;

                var circuitBreakerParameters = new CircuitBreakerParameters(
                    exceptionsAllowedBeforeBreaking: 1,
                    durationOfBreak: TimeSpan.FromMinutes(1));
                
                options.DecorateWithCircuitBreaker(
                    initParameters: circuitBreakerParameters,
                    executeParameters: circuitBreakerParameters,
                    subscribeRetryInterval: TimeSpan.FromMilliseconds(1),
                    exceptionFilter: exception => exception is InvalidOperationException);
            });

        protected abstract IEasyCachingBus CreateBus();
        
        [Fact]
        public void Subscribe_Should_Not_Fail()
        {
            var provider = CreateDecoratedProvider();

            provider.Subscribe(Topic, message => { });
        }
        
        [Fact]
        public void Publish_Should_Break_Circuit()
        {
            var provider = CreateDecoratedProvider();
            
            AssertCircuitIsBroken(() => provider.Publish(Topic, new EasyCachingMessage()));
        }
        
        [Fact]
        public async Task PublishAsync_Should_Break_Circuit()
        {
            var provider = CreateDecoratedProvider();
            
            await AssertCircuitIsBroken(() => provider.PublishAsync(Topic, new EasyCachingMessage()));
        }
        
        private void AssertCircuitIsBroken(Action action)
        {
            Assert.Throws<InvalidOperationException>(action);
            Assert.Throws<BrokenCircuitException>(action);
        }

        private async Task AssertCircuitIsBroken(Func<Task> action)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(action);
            await Assert.ThrowsAsync<BrokenCircuitException>(action);
        }
    }

    public class CircuitBreakerBusDecorationTestsWithFailOnInit : CircuitBreakerBusDecorationTests
    {
        protected override IEasyCachingBus CreateBus() => throw new InvalidOperationException("Exception on init");
    }

    public class CircuitBreakerBusDecorationTestsWithFailOnAnyMethod : CircuitBreakerBusDecorationTests
    {
        private int _subscriptionAttemptsCount = 0;
        
        protected override IEasyCachingBus CreateBus()
        {
            var fakeCachingBus = A.Fake<IEasyCachingBus>();
            
            A.CallTo(() => fakeCachingBus.Subscribe(Topic, A<Action<EasyCachingMessage>>._)).Invokes(_ =>
            {
                _subscriptionAttemptsCount++;
                if (_subscriptionAttemptsCount == 1)
                {
                    throw new InvalidOperationException("Exception on subscribe");
                }
            });
            
            A.CallTo(() => fakeCachingBus.Publish(Topic, A<EasyCachingMessage>._))
                .Throws(new InvalidOperationException("Exception on publish"));
            
            A.CallTo(() => fakeCachingBus.PublishAsync(Topic, A<EasyCachingMessage>._, A<CancellationToken>._))
                .ThrowsAsync(new InvalidOperationException("Exception on publish"));
            
            return fakeCachingBus;
        }
        
        [Fact]
        public void Subscribe_Should_Make_Second_Attempt()
        {
            var provider = CreateDecoratedProvider();

            provider.Subscribe(Topic, message => { });
            Thread.Sleep(100);
            
            Assert.Equal(2, _subscriptionAttemptsCount);
        }
    }
}