using System;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Model
{
    [MessageHandler(messageType: typeof(ICommandMessage))]
    public class CommandHandlerInterceptorAttribute : Attribute
    {
        public CommandHandlerInterceptorAttribute(string commandNamePattern = ".*")
        {
            CommandNamePattern = commandNamePattern;
        }

        public string CommandNamePattern { get; set; } = ".*";
    }
}