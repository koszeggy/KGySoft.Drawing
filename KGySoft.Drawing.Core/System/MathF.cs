#if NETFRAMEWORK || NETSTANDARD2_0

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace System
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility")]
    internal static class MathF
    {
        #region Constants

        internal const float PI = (float)Math.PI;

        #endregion

        #region Methods

        internal static float Round(float x) => (float)Math.Round(x);
        internal static float Round(float x, MidpointRounding mode) => (float)Math.Round(x, mode);
        internal static float Floor(float x) => (float)Math.Floor(x);
        internal static float Pow(float x, float y) => (float)Math.Pow(x, y);
        internal static float Sin(float x) => (float)Math.Sin(x);
        internal static float Cos(float x) => (float)Math.Cos(x);
        internal static float Sqrt(float x) => (float)Math.Sqrt(x);
        internal static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        internal static float Asin(float x) => (float)Math.Asin(x);
        internal static float IEEERemainder(float x, float y) => (float)Math.IEEERemainder(x, y);
        internal static float Tan(float a) => (float)Math.Tan(a);
        internal static float Ceiling(float a) => (float)Math.Ceiling(a);

        #endregion

    }
} 
#endif