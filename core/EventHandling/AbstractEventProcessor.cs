using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using CommonServiceLocator;
using Microsoft.Extensions.Logging;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;
using NAxonFramework.Monitoring;

namespace NAxonFramework.EventHandling
{
    public abstract class AbstractEventProcessor : IEventProcessor
    {
        private readonly ILogger _logger = ServiceLocator.Current.GetInstance<ILogger<AbstractEventProcessor>>();
        private readonly ConcurrentDictionary<IMessageHandlerInterceptor, object> _interceptors = new ConcurrentDictionary<IMessageHandlerInterceptor, object>();
        protected IEventHandlerInvoker EventHandlerInvoker { get; }
        private readonly RollbackConfigurationType _rollbackConfiguration;
        private readonly IErrorHandler _errorHandler;
        private readonly IMessageMonitor _messageMonitor;


        public AbstractEventProcessor(string name, IEventHandlerInvoker eventHandlerInvoker, RollbackConfigurationType rollbackConfiguration, IErrorHandler errorHandler, IMessageMonitor messageMonitor)
        {
            EventHandlerInvoker = eventHandlerInvoker ?? throw new ArgumentNullException(nameof(name));
            _rollbackConfiguration = rollbackConfiguration;
            _errorHandler = errorHandler ?? PropagatingErrorHandler.Instance;
            _messageMonitor = messageMonitor ?? NoOpMessageMonitor.Instance;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
        public IDisposable RegisterInterceptor(IMessageHandlerInterceptor interceptor)
        {
            _interceptors.TryAdd(interceptor, null);
            return Disposable.Create(() => _interceptors.TryRemove(interceptor, out _));
        }

        public override string ToString() => Name;

        protected bool CanHandle(IEventMessage eventMessage, Segment segment)
        {
            try
            {
                return EventHandlerInvoker.CanHandle(eventMessage, segment);
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(new ErrorContext(Name, e, new List<IEventMessage> {eventMessage}));
                return false;
            }
        }

        protected void ProcessInUnitOfWork(List<IEventMessage> eventMessages, IUnitOfWork unitOfWork, Segment segment)
        {
            try
            {
                unitOfWork.ExecuteWithResult(() =>
                {
                    var monitorCallback = _messageMonitor.OnMessageIngested(unitOfWork.Message);
                    return new DefaultInterceptorChain(unitOfWork, _interceptors.Keys.GetEnumerator(), MessageHandler<IEventMessage>.Create(m =>
                    {
                        try
                        {
                            EventHandlerInvoker.Handle(m, segment);
                            monitorCallback.ReportSuccess();
                            return null;
                        }
                        catch (Exception exception)
                        {
                            monitorCallback.ReportFailure(exception);
                            throw;
                        }
                    })).Proceed();
                }, _rollbackConfiguration);
            }
            catch (Exception e)
            {
                if (unitOfWork.IsRolledBack)
                {
                    _errorHandler.HandleError(new ErrorContext(Name, e, eventMessages));
                }
                else
                {
                    _logger.LogInformation($"Exception occurred while processing a message, but unit of work was committed. {e.GetType().Name}");
                }
            }
        }
        
        protected void ReportIgnored(IEventMessage eventMessage) 
        {
            _messageMonitor.OnMessageIngested(eventMessage).ReportIgnored();
        }
        public abstract void Start();

        public abstract void Shutdown();
    }
}