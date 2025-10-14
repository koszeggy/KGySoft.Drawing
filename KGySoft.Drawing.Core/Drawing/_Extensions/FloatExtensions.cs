#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FloatExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Runtime.CompilerServices;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing
{
    internal static class FloatExtensions
    {
        #region Constants

        private const float oneDegreeInRadian = MathF.PI / 180f;
        private const float oneRadianInDegree = 1f / oneDegreeInRadian;

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static byte ClipToByte(this float value) => value switch
        {
            >= Byte.MaxValue => Byte.MaxValue,
            >= Byte.MinValue => (byte)value,
            _ => Byte.MinValue
        };

#if NET35 || NET40
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ushort ClipToUInt16(this float value) => value switch
        {
            >= UInt16.MaxValue => UInt16.MaxValue,
            >= UInt16.MinValue => (ushort)value,
            _ => UInt16.MinValue
        };

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static byte ClipToByte(this float value, byte max)
            => value >= max ? max
                : value >= 0 ? (byte)value
                : (byte)0;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ushort ClipToUInt16(this float value, ushort max)
            => value >= max ? max
                : value >= 0 ? (ushort)value
                : (ushort)0;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float Sqr(this float value) => value * value;
#endif

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float Clip(this float value, float min, float max)
            // Unlike Math.Clamp/Min/Max this returns min for NaN
            => value >= max ? max
                : value >= min ? value
                : min;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float ClipF(this float value)
            // Unlike Math.Clamp/Min/Max this returns min for NaN
            => value >= 1f ? 1f
                : value >= 0f ? value
                : 0f;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float RoundTo(this float value, float smallestUnit)
            // Always rounding halves up, even for negative values
            => MathF.Floor(value / smallestUnit + 0.5f) * smallestUnit;

        internal static float ToRadian(this float degree) => degree * oneDegreeInRadian;
        internal static float ToDegree(this float radians) => radians * oneRadianInDegree;

        /// <summary>
        /// Gets the sine cardinal of <paramref name="x"/>, which is Sin(PI * x) / (PI * x)
        /// </summary>
        internal static float SinC(this float x)
        {
            if (x.TolerantIsZero(1e-4f))
                return 1f;
            x *= MathF.PI;
            return MathF.Sin(x) / x;
        }

        #endregion
    }
}