﻿#if NETFRAMEWORK || NETSTANDARD2_0
namespace System
{
    internal static class MathF
    {
        #region Methods

        public static float Round(float x) => (float)Math.Round(x);

        #endregion
    }
} 
#endif