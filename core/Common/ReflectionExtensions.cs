using System;
using System.Linq;
using System.Reflection;

namespace NAxonFramework.Common
{
    public static class ReflectionExtensions
    {
        public static bool HasAttribute<T>(this ICustomAttributeProvider memberInfo) where T : System.Attribute => memberInfo.GetCustomAttributes(typeof(T), true).Any();

        public static T GetCustomAttribute<T>(this ICustomAttributeProvider memberInfo) where T : System.Attribute 
        {
            return memberInfo.GetCustomAttributes(typeof(T), true).Cast<T>().FirstOrDefault();
        }
        public static Attribute GetCustomAttribute(this ICustomAttributeProvider memberInfo, Type attributeType) 
        {
            return memberInfo.GetCustomAttributes(attributeType, true).Cast<Attribute>().FirstOrDefault();
        }
        
        public static Optional<Type> ResolveGenericType(this PropertyInfo field, int genericTypeIndex) 
        {
            var genericType = field.PropertyType;
            if (genericType.GenericTypeArguments.Length <= genericTypeIndex)
            {
                return Optional<Type>.Empty;
            }
            return Optional<Type>.Of(genericType.GenericTypeArguments[genericTypeIndex]);
        }
    }
}