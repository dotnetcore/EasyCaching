using System;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCaching.Core
{
    public class NullEasyCachingBus : IEasyCachingBus
    {
        public static readonly NullEasyCachingBus Instance = new NullEasyCachingBus();

        public void Dispose() { }
               
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
