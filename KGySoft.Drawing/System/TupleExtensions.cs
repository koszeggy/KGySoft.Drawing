#if NET35 || NET40 || NET45

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class TupleExtensions
    {
        internal static void Deconstruct<T1, T2>(this Tuple<T1, T2> tuple, out T1 item1, out T2 item2)
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
        }
    }
}

#endif