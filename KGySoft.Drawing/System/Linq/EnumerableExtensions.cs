#if NET35
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    internal static class EnumerableExtensions
    {
        // internal usage only so omitting checks
        internal static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        {
            using (var enum1 = first.GetEnumerator())
            using (var enum2 = second.GetEnumerator())
            {
                while (enum1.MoveNext() && enum2.MoveNext())
                    yield return resultSelector.Invoke(enum1.Current, enum2.Current);
            }
        }
    }
}

#endif