namespace EasyCaching.Decoration.Polly
{
    using global::Polly;
    using EasyCaching.Core.Decoration;
    using EasyCaching.Core.Bus;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class EasyCachingBusPolicyDecorator : IEasyCachingBus
    {
        private static readonly Func<int, TimeSpan> DefaultSleepDurationProvider =
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1));
        
        public static IEasyCachingBus WithRetry(
            string name,
            Func<IEasyCachingBus> busFactory,
            int retryCount,
            Func<Exception, bool> exceptionFilter,
            Func<int, TimeSpan> sleepDurationProvider = null)
        {
            var policyBuilder = exceptionFilter.GetHandleExceptionPolicyBuilder();
            
            var retryPolicy = policyBuilder.WaitAndRetry(retryCount, sleepDurationProvider ?? DefaultSleepDurationProvider);
            var retryAsyncPolicy = policyBuilder.WaitAndRetryAsync(retryCount, sleepDurationProvider ?? DefaultSleepDurationProvider);
            
            return new EasyCachingBusPolicyDecorator(
                name, 
                busFactory, 
                syncExecutePolicy: retryPolicy,
                asyncExecutePolicy: retryAsyncPolicy);
        }
        
        public static IEasyCachingBus WithPublishFallback(
            string name,
            Func<IEasyCachingBus> busFactory,
            Func<Exception, bool> exceptionFilter)
        {
            var policyBuilder = exceptionFilter.GetHandleExceptionPolicyBuilder();
            
            var fallbackPolicy = policyBuilder.Fallback(() => { });
            var fallbackAsyncPolicy = policyBuilder.FallbackAsync(cancellationToken => Task.CompletedTask);
            
            return new EasyCachingBusPolicyDecorator(
                name, 
                busFactory, 
                syncExecutePolicy: fallbackPolicy,
                asyncExecutePolicy: fallbackAsyncPolicy);
        }
        
        public static IEasyCachingBus WithCircuitBreaker(
            string name,
            Func<IEasyCachingBus> busFactory,
            ICircuitBreakerParameters initParameters,
            ICircuitBreakerParameters executeParameters,
            TimeSpan subscribeRetryInterval,
            Func<Exception, bool> exceptionFilter)
        {
            var policyBuilder = exceptionFilter.GetHandleExceptionPolicyBuilder();
            var policyBuilderWithResult = exceptionFilter.GetHandleExceptionPolicyBuilder<IEasyCachingBus>();

            var initFallbackPolicy = policyBuilderWithResult.Fallback(
                fallbackAction: (result, _, __) => new ExceptionEasyCachingBus(result.Exception),
                onFallback: (_, __) => { });
            var initCircuitBreakerPolicy = initParameters.CreatePolicy(policyBuilderWithResult);
            var initPolicy = Policy.Wrap(initFallbackPolicy, initCircuitBreakerPolicy);

            var syncExecutePolicy = executeParameters.CreatePolicy(policyBuilder);
            var asyncExecutePolicy = executeParameters.CreatePolicyAsync(policyBuilder);
            var subscribePolicy = policyBuilder.WaitAndRetryForeverAsync(_ => subscribeRetryInterval);
            
            return new EasyCachingBusPolicyDecorator(
                name, 
                busFactory,
                initPolicy: initPolicy,
                syncExecutePolicy: syncExecutePolicy,
                asyncExecutePolicy: asyncExecutePolicy,
                subscribePolicy: subscribePolicy);
        }

        private readonly LazyWithoutExceptionCaching<IEasyCachingBus> _lazyBusProvider;
        private readonly Policy<IEasyCachingBus> _initPolicy;
        private readonly Policy _syncExecutePolicy;
        private readonly AsyncPolicy _asyncExecutePolicy;
        private readonly AsyncPolicy _subscribePolicy;
        
        public EasyCachingBusPolicyDecorator(
            string name,
            Func<IEasyCachingBus> busFactory,
            Policy<IEasyCachingBus> initPolicy = null,
            Policy syncExecutePolicy = null,
            AsyncPolicy asyncExecutePolicy = null,
            AsyncPolicy subscribePolicy = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            
            if (busFactory == null) throw new ArgumentNullException(nameof(busFactory));
            _lazyBusProvider = new LazyWithoutExceptionCaching<IEasyCachingBus>(busFactory);
            
            _initPolicy = initPolicy;
            _syncExecutePolicy = syncExecutePolicy;
            _asyncExecutePolicy = asyncExecutePolicy;
            _subscribePolicy = subscribePolicy;
        }

        private IEasyCachingBus Bus =>
            _initPolicy == null || _lazyBusProvider.Initialized
                ? _lazyBusProvider.Value
                : _initPolicy.Execute(() => _lazyBusProvider.Value);

        public string Name { get; }

        public void Publish(string topic, EasyCachingMessage message)
        {
            if (_syncExecutePolicy == null)
            {
                Bus.Publish(topic, message);
            }
            else
            {
                _syncExecutePolicy.Execute(() => Bus.Publish(topic, message));
            }
        }

        public Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _asyncExecutePolicy == null 
                ? Bus.PublishAsync(topic, message, cancellationToken) 
                : _asyncExecutePolicy.ExecuteAsync(() => Bus.PublishAsync(topic, message, cancellationToken));
        }

        public void Subscribe(string topic, Action<EasyCachingMessage> action)
        {
            if (_subscribePolicy == null)
            {
                Bus.Subscribe(topic, action);
            }
            else
            {
                _subscribePolicy.ExecuteAsync(() =>
                {
                    Bus.Subscribe(topic, action);
                    return Task.CompletedTask;
                });
            }
        }
    }
}