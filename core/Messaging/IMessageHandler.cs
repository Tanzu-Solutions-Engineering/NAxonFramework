namespace NAxonFramework.Messaging
{
    public interface IMessageHandler<T> : IMessageHandler where T : IMessage
    {
        object Handle(T message);
    }
    public interface IMessageHandler
    {
        object Handle(IMessage message);
    }
}