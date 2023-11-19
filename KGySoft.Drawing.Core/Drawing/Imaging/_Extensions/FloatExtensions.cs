#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FloatExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class FloatExtensions
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static byte ClipToByte(this float value) => value switch
        {
            >= Byte.MaxValue => Byte.MaxValue,
            >= Byte.MinValue => (byte)value,
            _ => Byte.MinValue
        };

#if NET35 || NET40 || NET45 || NETSTANDARD2_0
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ushort ClipToUInt16(this float value) => value switch
        {
            >= UInt16.MaxValue => UInt16.MaxValue,
            >= UInt16.MinValue => (byte)value,
            _ => UInt16.MinValue
        };

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static byte ClipToByte(this float value, byte max)
            => value >= max ? max
                : value >= 0 ? (byte)value
                : (byte)0;
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

        #endregion
    }
}