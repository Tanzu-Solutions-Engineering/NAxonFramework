using System;
using System.Collections.Generic;

namespace NAxonFramework.Common
{
    public static class Generics
    {
        public static Type MakeGenericTypeFast(this Type type, params Type[] genericsParameters)
        {
            //todo: actually make it fast by caching created generic types
            return type.MakeGenericType(genericsParameters);
        }
    }
}