using System;
using NAxonFramework.Common;

namespace NAxonFramework.Messaging.UnitOfWork
{
    public static class UnitOfWorkExtensions
    {
        public static bool IsActive(this IUnitOfWork unitOfWork) => unitOfWork.Phase.IsStarted();
        public static bool IsRoot(this IUnitOfWork unitOfWork) => !unitOfWork.Parent.IsPresent;
        public static IUnitOfWork Root(this IUnitOfWork unitOfWork) => unitOfWork.Parent.Map(x => x.Root()).OrElse(unitOfWork);
        public static R GetResource<R>(this IUnitOfWork unitOfWork, string name) => (R)unitOfWork.Resources.GetValueOrDefault(name);
        public static R GetOrComputeResource<R>(this IUnitOfWork unitOfWork, string key, Func<string, R> mappingFunction) 
            => (R)unitOfWork.Resources.ComputeIfAbsent(key, k => mappingFunction(k));

        public static R GetOrDefaultResource<R>(this IUnitOfWork unitOfWork, string key, R defaultValue)
            => (R)unitOfWork.Resources.GetValueOrDefault(key, defaultValue);
        public static void Execute(this IUnitOfWork unitOfWork, Action task) => unitOfWork.Execute(task, RollbackConfigurationType.AnyThrowable);

        public static void Execute(this IUnitOfWork unitOfWork, Action task, RollbackConfigurationType rollbackConfiguration)
            => unitOfWork.ExecuteWithResult(() =>
            {
                task.Invoke();
                return (object)null;
            }, rollbackConfiguration);

        public static R ExecuteWithResult<R>(this IUnitOfWork unitOfWork, Func<R> task) => unitOfWork.ExecuteWithResult(task, RollbackConfigurationType.AnyThrowable);
        public static bool IsCurrent(this IUnitOfWork unitOfWork) => CurrentUnitOfWork.IsStarted && CurrentUnitOfWork.Get() == unitOfWork;
        
    }
}