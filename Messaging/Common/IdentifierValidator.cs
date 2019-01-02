using System;
using System.Collections.Generic;

namespace NAxonFramework.Common
{
    public static class IdentifierValidator
    {
        private static readonly HashSet<Type> _whiteList = new HashSet<Type>();

        public static bool IsValidIdentifier(this Type type)
        {
            if(type == null) throw new ArgumentNullException(nameof(type));
            if (type.GetMethod(nameof(ToString))?.DeclaringType == typeof(object))
            {
                return false;
            }

            _whiteList.Add(type);
            return true;
        }
        
    }
}