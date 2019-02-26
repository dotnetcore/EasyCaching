namespace EasyCaching.Core
{
    using EasyCaching.Core.Bus;
    using EasyCaching.Core.Diagnostics;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class EasyCachingAbstractBus : IEasyCachingBus
    {
        protected static readonly DiagnosticListener s_diagnosticListener =
                    new DiagnosticListener(EasyCachingDiagnosticListenerExtensions.DiagnosticListenerName);

        public abstract void BasePublish(string topic, EasyCachingMessage message);
        public abstract Task BasePublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken));
        public abstract void BaseSubscribe(string topic, Action<EasyCachingMessage> action);

        public void Publish(string topic, EasyCachingMessage message)
        {
            s_diagnosticListener.WritePublishMessage(topic, message);
            BasePublish(topic, message);
        }

        public async Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            s_diagnosticListener.WritePublishMessage(topic, message);
            await BasePublishAsync(topic, message, cancellationToken);
        }

        public void Subscribe(string topic, Action<EasyCachingMessage> action)
        {
            BaseSubscribe(topic, action);
        }

        public virtual void LogMessage(EasyCachingMessage message)
        {
            s_diagnosticListener.WriteSubscribeMessage(message);
        }
    }
}
