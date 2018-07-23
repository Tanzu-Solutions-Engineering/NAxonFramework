using System.Collections.Generic;
using System.Linq;

namespace NAxonFramework.Common
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T>(this ICollection<T> collection) => collection.Count == 0;
        public static bool IsEmpty<T>(this IReadOnlyCollection<T> collection) => collection.Count == 0;
    }
}