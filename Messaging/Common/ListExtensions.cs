using System.Collections.Generic;

namespace NAxonFramework.Common
{
    public static class ListExtensions
    {
        public static IEnumerable<TTo> FastCast<TFrom, TTo>(this IEnumerable<TFrom> list)
        {
            return new CastedEnumerable<TTo, TFrom>(list);
        }
    }
}