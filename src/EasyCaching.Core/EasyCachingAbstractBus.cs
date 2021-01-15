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

        protected Action<EasyCachingMessage> _handler;

        protected string BusName { get; set; }

        public string Name => this.BusName;

        public void Publish(string topic, EasyCachingMessage message)
        {
            var operationId = s_diagnosticListener.WritePublishMessageBefore(new BeforePublishMessageRequestEventData(topic, message));
            Exception e = null;
            try
            {
                BasePublish(topic, message);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WritePublishMessageError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WritePublishMessageAfter(operationId);
                }
            }
        }

        public async Task PublishAsync(string topic, EasyCachingMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            var operationId = s_diagnosticListener.WritePublishMessageBefore(new BeforePublishMessageRequestEventData(topic, message));
            Exception e = null;
            try
            {
                await BasePublishAsync(topic, message, cancellationToken);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WritePublishMessageError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WritePublishMessageAfter(operationId);
                }
            }
        }

        public void Subscribe(string topic, Action<EasyCachingMessage> action)
        {
            _handler = action;
            BaseSubscribe(topic, action);
        }

        public virtual void BaseOnMessage(EasyCachingMessage message)
        {
            var operationId = s_diagnosticListener.WriteSubscribeMessageBefore(new BeforeSubscribeMessageRequestEventData(message));
            Exception e = null;
            try
            {
                _handler?.Invoke(message);
            }
            catch (Exception ex)
            {
                e = ex;
                throw;
            }
            finally
            {
                if (e != null)
                {
                    s_diagnosticListener.WritePublishMessageError(operationId, e);
                }
                else
                {
                    s_diagnosticListener.WritePublishMessageAfter(operationId);
                }
            }
        }
    }
}
