using System;

namespace NAxonFramework.Common
{
    public static class TypeExtensions
    {
        public static bool IsAttribute(this Type type)
        {
            return typeof(Attribute).IsAssignableFrom(type);
        }

        public static bool IsImplementing<T>(this Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }
    }
}