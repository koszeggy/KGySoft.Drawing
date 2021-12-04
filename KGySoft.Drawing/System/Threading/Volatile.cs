#if NET35 || NET40

// ReSharper disable once CheckNamespace
namespace System.Threading
{
    internal static class Volatile
    {
        #region Methods

        internal static bool Read(ref bool location)
        {
            bool value = location;
            Thread.MemoryBarrier();
            return value;
        }

        // ReSharper disable once RedundantAssignment - false alarm, it is a ref parameter
        public static void Write(ref bool location, bool value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        #endregion
    }
}

#endif