namespace EasyCaching.UnitTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyCaching.Core.Bus;

    public class FakeBus : IEasyCachingBus
    {
        public string Name => nameof(FakeBus);

        public void Publish(string topic, EasyCachingMessage message)
        {

        }

        public Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public void Subscribe(string topic, Action<EasyCachingMessage> action)
        {

        }
    }
}
