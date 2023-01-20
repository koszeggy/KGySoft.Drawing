#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GdiPlusColor64.cs
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
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct GdiPlusColor64
    {
        #region Fields

        [FieldOffset(0)]private readonly ushort b;
        [FieldOffset(2)]private readonly ushort g;
        [FieldOffset(4)]private readonly ushort r;
        [FieldOffset(6)]private readonly ushort a;

        #endregion

        #region Constructors

        internal GdiPlusColor64(ushort a, ushort r, ushort g, ushort b)
        {
            this.b = b;
            this.g = g;
            this.r = r;
            this.a = a;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor64(Color32 c)
        {
            ushort[]? lookupTable = ColorsHelper.GetLookupTable8To16Bpp();
            if (lookupTable == null)
            {
                b = (ushort)((c.B << 8) | c.B);
                g = (ushort)((c.G << 8) | c.G);
                r = (ushort)((c.R << 8) | c.R);
                a = (ushort)((c.A << 8) | c.A);
                return;
            }

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            b = lookupTable[c.B];
            g = lookupTable[c.G];
            r = lookupTable[c.R];
            ushort max = ColorsHelper.Max16BppValue;
            a = c.A == Byte.MaxValue ? max
                : max == UInt16.MaxValue ? (ushort)((c.A << 8) | c.A)
                : (ushort)(((c.A << 8) | c.A) * max / UInt16.MaxValue);
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32()
        {
            byte[]? lookupTable = ColorsHelper.GetLookupTable16To8Bpp();

            if (lookupTable == null)
                return new Color32((byte)(a >> 8), (byte)(r >> 8), (byte)(g >> 8), (byte)(b >> 8));

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            ushort max = ColorsHelper.Max16BppValue;
            return new Color32(a == max ? Byte.MaxValue : max == UInt16.MaxValue ? (byte)(a >> 8) : (byte)((a * UInt16.MaxValue / max) >> 8),
                lookupTable[r],
                lookupTable[g],
                lookupTable[b]);
        }

        internal GdiPlusColor64 ToStraight()
        {
            if (a == 0)
                return default;

            ushort max = ColorsHelper.Max16BppValue;
            return new GdiPlusColor64(a,
                a == 0 ? (ushort)0 : (ushort)Math.Min(max, (uint)r * max / a),
                a == 0 ? (ushort)0 : (ushort)Math.Min(max, (uint)g * max / a),
                a == 0 ? (ushort)0 : (ushort)Math.Min(max, (uint)b * max / a));
        }

        internal GdiPlusColor64 ToPremultiplied()
        {
            if (a == 0)
                return default;
            ushort max = ColorsHelper.Max16BppValue;
            if (a == max)
                return this;
            return new GdiPlusColor64(a,
                (ushort)(r * a / max),
                (ushort)(g * a / max),
                (ushort)(b * a / max));
        }

        #endregion
    }
}
