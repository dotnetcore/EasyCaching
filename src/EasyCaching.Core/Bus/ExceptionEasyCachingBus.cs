namespace EasyCaching.Core.Bus
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class ExceptionEasyCachingBus : IEasyCachingBus
    {
        private readonly Exception _exception;

        public ExceptionEasyCachingBus(Exception exception)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }
        
        public string Name => nameof(ExceptionEasyCachingBus);

        public void Publish(string topic, EasyCachingMessage message) => throw _exception;
        public Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default) => throw _exception;

        public void Subscribe(string topic, Action<EasyCachingMessage> action) => throw _exception;
    }
}