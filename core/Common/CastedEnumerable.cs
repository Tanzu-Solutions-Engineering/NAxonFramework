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
}