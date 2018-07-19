using System;
using System.Collections.Generic;

namespace NAxonFramework.Common
{
    public static class ComparerExtensions
    {
        public static IComparer<T> Reversed<T>(this IComparer<T> comparer) => 
            Comparer<T>.Create((x, y) => comparer.Compare(y, x));
        
        public static IComparer<T> ThenComparing<T>(this IComparer<T> comparer, IComparer<T> other) =>
            Comparer<T>.Create((x, y) =>
            {
                int res = comparer.Compare(x, y);
                return (res != 0) ? res : other.Compare(x, y);
            });
        public static IComparer<T> ThenComparing<T,U>(this IComparer<T> comparer, Func<T,U> keyExtractor, IComparer<U> keyComparator) => 
            ThenComparing(comparer, Comparing(comparer, keyExtractor, keyComparator));

        public static IComparer<T> ThenComparing<T,U>(this IComparer<T> comparer, Func<T,U> keyExtractor) => 
            ThenComparing(comparer, Comparing(comparer, keyExtractor));

        public static IComparer<T> Comparing<T, U>(this IComparer<T> comparer, Func<T, U> keyExtractor) =>
            Comparer<T>.Create((x, y) => Comparer<U>.Default.Compare(keyExtractor(x),keyExtractor(y)));
        

        public static IComparer<T> Comparing<T, U>(this IComparer<T> comparer, Func<T, U> keyExtractor, IComparer<U> keyComparator) =>
            Comparer<T>.Create((x, y) => keyComparator.Compare(keyExtractor(x),keyExtractor(y)));

        
    }

}