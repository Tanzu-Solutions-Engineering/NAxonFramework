using System;

namespace NAxonFramework.Common
{
    public static class ObjectUtils
    {
        public static T GetOrDefault<T>(this T instance, Func<T> defaultProvider) 
        {
            if (instance == null) 
            {
                return defaultProvider();
            }
            return instance;
        }
        public static T GetOrDefault<T>(this T instance, T defaultValue) 
        {
            if (instance == null) {
                return defaultValue;
            }
            return instance;
        }
        
    }
}