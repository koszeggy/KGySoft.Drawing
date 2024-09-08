﻿#if NETFRAMEWORK || NETSTANDARD2_0
// ReSharper disable once CheckNamespace
namespace System
{
    internal static class MathF
    {
        #region Methods

        public static float Round(float x) => (float)Math.Round(x);
        public static float Round(float x, MidpointRounding mode) => (float)Math.Round(x, mode);
        
        public static float Floor(float x) => (float)Math.Floor(x);

        public static float Pow(float x, float y) => (float)Math.Pow(x, y);

        #endregion
    }
} 
#endif