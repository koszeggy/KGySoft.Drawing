#if NETFRAMEWORK || NETSTANDARD2_0
namespace System
{
    internal static class MathF
    {
        #region Methods

        public static float Pow(float x, float y) => (float)Math.Pow(x, y);

        #endregion
    }
} 
#endif