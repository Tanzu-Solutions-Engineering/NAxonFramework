using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace NAxonFramework.Common
{
    public static class EnumerableExtensions
    {
        public static Optional<T> FindFirst<T>(this IEnumerable<T> source)
        {
            var result = source.FirstOrDefault();
            return result == null ? Optional<T>.Empty : Optional<T>.Of(result);
        }
    }
}