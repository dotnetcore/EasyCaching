namespace EasyCaching.Core.Bus
{
    using Decoration;
    using Polly;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class EasyCachingBusPolicyDecorator : IEasyCachingBus
    {
        public static IEasyCachingBus WithRetry(
            string name,
            Func<IEasyCachingBus> busFactory,
            int retryCount)
        {
            var retryAsyncPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)));

            var retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)));

            var fallbackPolicy = Policy.Handle<Exception>().Fallback(() => { });

            var fallbackAsyncPolicy = Policy.Handle<Exception>().FallbackAsync(cancellationToken => Task.CompletedTask);

            var syncExecutePolicy = Policy.Wrap(fallbackPolicy, retryPolicy);
            var asyncExecutePolicy = Policy.WrapAsync(fallbackAsyncPolicy, retryAsyncPolicy);
            
            return new EasyCachingBusPolicyDecorator(
                name, 
                busFactory, 
                initPolicy: Policy.NoOp(),
                syncExecutePolicy: syncExecutePolicy,
                asyncExecutePolicy: asyncExecutePolicy);
        }

        private readonly LazyWithoutExceptionCaching<IEasyCachingBus> _lazyBusProvider;
        private readonly Policy _initPolicy;
        private readonly Policy _syncExecutePolicy;
        private readonly AsyncPolicy _asyncExecutePolicy;
        
        public EasyCachingBusPolicyDecorator(
            string name,
            Func<IEasyCachingBus> busFactory,
            Policy initPolicy,
            Policy syncExecutePolicy,
            AsyncPolicy asyncExecutePolicy)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            
            if (busFactory == null) throw new ArgumentNullException(nameof(busFactory));
            _lazyBusProvider = new LazyWithoutExceptionCaching<IEasyCachingBus>(busFactory);
            
            _initPolicy = initPolicy;
            _syncExecutePolicy = syncExecutePolicy;
            _asyncExecutePolicy = asyncExecutePolicy;
        }

        private IEasyCachingBus Bus =>
            // Micro optimization to bypass policy when provider was initialized
            _lazyBusProvider.Initialized
                ? _lazyBusProvider.Value
                : _initPolicy.Execute(() => _lazyBusProvider.Value);

        public string Name { get; }

        public void Publish(string topic, EasyCachingMessage message)
        {
            _syncExecutePolicy.Execute(() => Bus.Publish(topic, message));
        }

        public Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _asyncExecutePolicy.ExecuteAsync(() => Bus.PublishAsync(topic, message, cancellationToken));
        }

        public void Subscribe(string topic, Action<EasyCachingMessage> action)
        {
            Bus.Subscribe(topic, action);
        }
    }
}