using System.Threading.Tasks;

namespace NAxonFramework.Common.Lock
{
    public interface ILockFactory
    {
        Task<ILock> ObtainLock(string identifier);
    }
}