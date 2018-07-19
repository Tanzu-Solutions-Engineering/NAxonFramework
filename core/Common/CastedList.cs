using System.Collections;
using System.Collections.Generic;

namespace NAxonFramework.Common
{
    public class CastedEnumerable<TTo, TFrom> : IEnumerable<TTo>
{
    public IEnumerable<TFrom> BaseList;

    public CastedEnumerable(IEnumerable<TFrom> baseList)
    {
        BaseList = baseList;
    }

    // IEnumerable
    IEnumerator IEnumerable.GetEnumerator() { return BaseList.GetEnumerator(); }

    // IEnumerable<>
    public IEnumerator<TTo> GetEnumerator() { return new CastedEnumerator<TTo, TFrom>(BaseList.GetEnumerator()); }

}

public class CastedEnumerator<TTo, TFrom> : IEnumerator<TTo>
{
    public IEnumerator<TFrom> BaseEnumerator;

    public CastedEnumerator(IEnumerator<TFrom> baseEnumerator)
    {
        BaseEnumerator = baseEnumerator;
    }

    // IDisposable
    public void Dispose() { BaseEnumerator.Dispose(); }

    // IEnumerator
    object IEnumerator.Current { get { return BaseEnumerator.Current; } }
    public bool MoveNext() { return BaseEnumerator.MoveNext(); }
    public void Reset() { BaseEnumerator.Reset(); }

    // IEnumerator<>
    public TTo Current { get { return (TTo)(object)BaseEnumerator.Current; } }
}

public static class ListExtensions
{
    public static IEnumerable<TTo> FastCast<TFrom, TTo>(this IEnumerable<TFrom> list)
    {
        return new CastedEnumerable<TTo, TFrom>(list);
    }
}
}