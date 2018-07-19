using System;

namespace NAxonFramework.Common
{
    public static class ObjectExtensions
    {
        public static void IfPresent<T>(this T target, Action<T> action)
        {
            if (target != null)
                action(target);
        }
        
    }
}