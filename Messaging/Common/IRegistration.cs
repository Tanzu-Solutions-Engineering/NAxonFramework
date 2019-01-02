using System;

namespace NAxonFramework.Common
{
    public interface IRegistration : IDisposable
    {
        bool Cancel();
    }
}