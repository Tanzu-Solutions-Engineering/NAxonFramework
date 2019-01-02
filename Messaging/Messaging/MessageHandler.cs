using System;

namespace NAxonFramework.Messaging
{
    public class MessageHandler<T> : IMessageHandler<T> where T : IMessage
    {
        private readonly Func<T, object> _handle;

        private MessageHandler(Func<T, object> handle)
        {
            _handle = handle;
        }

        public static IMessageHandler Create(Func<T, object> handle)
        {
            return new MessageHandler<T>(handle);
        }

        public object Handle(T message)
        {
            return _handle(message);
        }

        public object Handle(IMessage message)
        {
            return _handle((T)message);
        }
    }
}