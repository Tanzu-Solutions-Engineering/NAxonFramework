using System;
using System.Threading;
using System.Threading.Tasks;

namespace NAxonFramework.CommandHandling.Gateway
{
    public interface ICommandGateway
    {
        void Send<C, R>(C command, ICommandCallback<R> callback);
        Task<R> Send<C,R>(C command);
        Task<R> Send<C,R>(C command, TimeSpan timeout);
    }
}