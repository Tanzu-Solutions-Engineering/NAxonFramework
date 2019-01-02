using System;
using Microsoft.Extensions.Logging;

namespace NAxonFramework.CommandHandling.Callbacks
{
    public class LoggingCallback : ICommandCallback<object>
    {
        private readonly ILogger _logger;

        public LoggingCallback(ILogger logger)
        {
            _logger = logger;
        }

        public void OnSuccess(ICommandMessage commandMessage, object result)
        {
            _logger.LogInformation($"Command executed successfully: {commandMessage.CommandName}");
        }

        public void OnFailure(ICommandMessage commandMessage, Exception cause)
        {
            _logger.LogWarning($"Command resulted in exception:  {commandMessage.CommandName}", cause);
        }
    }
}