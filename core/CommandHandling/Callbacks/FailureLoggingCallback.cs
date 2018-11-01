using System;
using Microsoft.Extensions.Logging;

namespace NAxonFramework.CommandHandling.Callbacks
{
    public class FailureLoggingCallback : ICommandCallback
    {
        private readonly ILogger _logger;
        private readonly ICommandCallback _delegate;

        public FailureLoggingCallback(ILogger logger, ICommandCallback @delegate)
        {
            _logger = logger;
            _delegate = @delegate;
        }

        public void OnSuccess(ICommandMessage commandMessage, object result)
        {
            _delegate.OnSuccess(commandMessage, result);
        }

        public void OnFailure(ICommandMessage commandMessage, Exception cause)
        {
            _logger.LogWarning($"Command {commandMessage.CommandName} resulted in {cause.GetType()}({cause.Message})");
            _delegate.OnFailure(commandMessage, cause);
        }
    }
}