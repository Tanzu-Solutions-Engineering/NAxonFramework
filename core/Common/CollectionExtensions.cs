using System.Collections.Generic;
using System.Linq;

namespace NAxonFramework.Common
{
    public static class CollectionExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            switch (enumerable)
            {
                case ICollection<T> collection:
                    return collection.Count == 0;
                case IReadOnlyCollection<T> readOnlyCollection:
                    return readOnlyCollection.Count == 0;
                default:
                    return !enumerable.Any();
            }
        }
    }
}