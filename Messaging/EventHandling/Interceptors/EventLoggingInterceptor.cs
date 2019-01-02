using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.UnitOfWork;

namespace NAxonFramework.EventHandling.Interceptors
{
    public class EventLoggingInterceptor : IMessageDispatchInterceptor
    {
        private ILogger _logger;

        public EventLoggingInterceptor() : this(nameof(EventLoggingInterceptor))
        {
        }

        public EventLoggingInterceptor(string loggerName)
        {
            _logger = CommonServiceLocator.ServiceLocator.Current.GetInstance<ILoggerFactory>().CreateLogger(loggerName);
        }

        public Func<int, IMessage, IMessage> Handle<T>(IList<T> messages) where T : IMessage
        {
            var sb = new StringBuilder($"Events published: [{string.Join(", ", messages.Select(x => x.PayloadType.Name))}]");
            CurrentUnitOfWork.IfStarted(unitOfWork => 
            {
                var message = unitOfWork.Message;
                if (message == null) 
                {
                    sb.Append(" while processing an operation not tied to an incoming message");
                } 
                else 
                {
                    sb.Append($" while processing a [{message.PayloadType.Name}]");
                }
                var executionResult = unitOfWork.ExecutionResult;
                if (executionResult != null) 
                {
                    if (executionResult.IsFaulted && executionResult.Exception != null) 
                    {
                        var exception = executionResult.Exception.GetBaseException();
                        exception = exception is ExecutionException ? exception.InnerException : exception; 
                        sb.Append($" which failed with a [{exception.GetType().Name}]");
                    } 
                    else if (executionResult.Status == TaskStatus.RanToCompletion)
                    {
                        sb.Append($" which yielded a [{executionResult.Result.GetType().Name}] return value");
                    }
                }
            });
            _logger.LogInformation(sb.ToString());
             
            return (i, m) => m;
        }
    }
}