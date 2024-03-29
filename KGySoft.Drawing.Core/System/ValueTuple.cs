﻿#if NETFRAMEWORK && !NET47_OR_GREATER
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable FieldCanBeMadeReadOnly.Global
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System
{
    internal struct ValueTuple
    {
        internal static int CombineHashCodes(int h1, int h2)
        {
            uint num = (uint)((h1 << 5) | (h1 >> 27));
            return ((int)num + h1) ^ h2;
        }

        internal static int CombineHashCodes(int h1, int h2, int h3) => CombineHashCodes(CombineHashCodes(h1, h2), h3);
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4) => CombineHashCodes(CombineHashCodes(CombineHashCodes(h1, h2), h3), h4);
        internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5) => CombineHashCodes(CombineHashCodes(CombineHashCodes(CombineHashCodes(h1, h2), h3), h4), h5);
    }

    [Serializable]
    internal struct ValueTuple<T1, T2> : IEquatable<ValueTuple<T1, T2>>
    {
        public T1 Item1;
        public T2 Item2;

        public ValueTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public bool Equals(ValueTuple<T1, T2> other)
            => EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2);

        public override bool Equals(object obj) => obj is ValueTuple<T1, T2> tuple && Equals(tuple);

        public override int GetHashCode()
            => ValueTuple.CombineHashCodes(EqualityComparer<T1>.Default.GetHashCode(Item1),
                EqualityComparer<T2>.Default.GetHashCode(Item2));

        public override string ToString() => $"({Item1}, {Item2})";

        public static bool operator ==(ValueTuple<T1, T2> left, ValueTuple<T1, T2> right) => left.Equals(right);
        public static bool operator !=(ValueTuple<T1, T2> left, ValueTuple<T1, T2> right) => !left.Equals(right);
    }

    [Serializable]
    internal struct ValueTuple<T1, T2, T3> : IEquatable<ValueTuple<T1, T2, T3>>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;

        public ValueTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public bool Equals(ValueTuple<T1, T2, T3> other)
            => EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3);

        public override bool Equals(object obj) => obj is ValueTuple<T1, T2, T3> tuple && Equals(tuple);

        public override int GetHashCode()
            => ValueTuple.CombineHashCodes(EqualityComparer<T1>.Default.GetHashCode(Item1),
                EqualityComparer<T2>.Default.GetHashCode(Item2),
                EqualityComparer<T3>.Default.GetHashCode(Item3));

        public override string ToString() => $"({Item1}, {Item2}, {Item3})";

        public static bool operator ==(ValueTuple<T1, T2, T3> left, ValueTuple<T1, T2, T3> right) => left.Equals(right);
        public static bool operator !=(ValueTuple<T1, T2, T3> left, ValueTuple<T1, T2, T3> right) => !left.Equals(right);
    }

    [Serializable]
    internal struct ValueTuple<T1, T2, T3, T4> : IEquatable<ValueTuple<T1, T2, T3, T4>>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        public bool Equals(ValueTuple<T1, T2, T3, T4> other)
            => EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
                && EqualityComparer<T4>.Default.Equals(Item4, other.Item4);

        public override bool Equals(object obj) => obj is ValueTuple<T1, T2, T3, T4> tuple && Equals(tuple);

        public override int GetHashCode()
            => ValueTuple.CombineHashCodes(EqualityComparer<T1>.Default.GetHashCode(Item1),
                EqualityComparer<T2>.Default.GetHashCode(Item2),
                EqualityComparer<T3>.Default.GetHashCode(Item3),
                EqualityComparer<T4>.Default.GetHashCode(Item4));

        public override string ToString() => $"({Item1}, {Item2}, {Item3}, {Item4})";

        public static bool operator ==(ValueTuple<T1, T2, T3, T4> left, ValueTuple<T1, T2, T3, T4> right) => left.Equals(right);
        public static bool operator !=(ValueTuple<T1, T2, T3, T4> left, ValueTuple<T1, T2, T3, T4> right) => !left.Equals(right);
    }

    [Serializable]
    internal struct ValueTuple<T1, T2, T3, T4, T5> : IEquatable<ValueTuple<T1, T2, T3, T4, T5>>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;

        public ValueTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }

        public bool Equals(ValueTuple<T1, T2, T3, T4, T5> other)
            => EqualityComparer<T1>.Default.Equals(Item1, other.Item1)
                && EqualityComparer<T2>.Default.Equals(Item2, other.Item2)
                && EqualityComparer<T3>.Default.Equals(Item3, other.Item3)
                && EqualityComparer<T4>.Default.Equals(Item4, other.Item4)
                && EqualityComparer<T5>.Default.Equals(Item5, other.Item5);

        public override bool Equals(object obj) => obj is ValueTuple<T1, T2, T3, T4, T5> tuple && Equals(tuple);

        public override int GetHashCode()
            => ValueTuple.CombineHashCodes(EqualityComparer<T1>.Default.GetHashCode(Item1),
                EqualityComparer<T2>.Default.GetHashCode(Item2),
                EqualityComparer<T3>.Default.GetHashCode(Item3),
                EqualityComparer<T4>.Default.GetHashCode(Item4),
                EqualityComparer<T5>.Default.GetHashCode(Item5));

        public override string ToString() => $"({Item1}, {Item2}, {Item3}, {Item4}, {Item5})";

        public static bool operator ==(ValueTuple<T1, T2, T3, T4, T5> left, ValueTuple<T1, T2, T3, T4, T5> right) => left.Equals(right);
        public static bool operator !=(ValueTuple<T1, T2, T3, T4, T5> left, ValueTuple<T1, T2, T3, T4, T5> right) => !left.Equals(right);
    }
}
#endif
