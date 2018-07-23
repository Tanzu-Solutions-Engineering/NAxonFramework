namespace NAxonFramework.Messaging
{
    public interface IMessageHandler<T> where T : IMessage
    {
        object Handle(T message);
    }
}