using System;
using System.Collections.Generic;
using System.Linq;

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
        public static bool IsImplementing(this Type type, Type @interface)
        {
            if (!@interface.IsGenericTypeDefinition)
            {
                return @interface.IsAssignableFrom(type);
            }
            else
            {
                return typeof(Dictionary<string, object>).GetInterfaces().Any(x =>
                    x.IsGenericType &&
                    x.GetGenericTypeDefinition() == @interface);
            }
        }
    }
}