#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GdiPlusPColor64.cs
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
    internal readonly struct GdiPlusPColor64
    {
        #region Fields

        [FieldOffset(0)]private readonly ushort b;
        [FieldOffset(2)]private readonly ushort g;
        [FieldOffset(4)]private readonly ushort r;
        [FieldOffset(6)]private readonly ushort a;

        #endregion

        #region Constructors

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusPColor64(Color32 c)
        {
            if (c.A == 0)
            {
                this = default;
                return;
            }

            Debug.Assert(ColorsHelper.GetLookupTable8To16Bpp() != null);
            ushort[] lookupTable = ColorsHelper.GetLookupTable8To16Bpp()!;
            ushort max = ColorsHelper.Max16BppValue;

            if (c.A == Byte.MaxValue)
            {
                b = lookupTable[c.B];
                g = lookupTable[c.G];
                r = lookupTable[c.R];
                a = max;
                return;
            }

            a = (ushort)(ColorSpaceHelper.ToUInt16(c.A) * max / UInt16.MaxValue);
            b = (ushort)((uint)lookupTable[c.B] * a / max);
            g = (ushort)((uint)lookupTable[c.G] * a / max);
            r = (ushort)((uint)lookupTable[c.R] * a / max);
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32()
        {
            if (a == 0)
                return default;

            Debug.Assert(ColorsHelper.GetLookupTable16To8Bpp() != null);
            byte[] lookupTable = ColorsHelper.GetLookupTable16To8Bpp()!;
            ushort max = ColorsHelper.Max16BppValue;

            if (a == max)
                return new Color32(Byte.MaxValue, lookupTable[r], lookupTable[g], lookupTable[b]);

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            return new Color32(ColorSpaceHelper.ToByte((ushort)((uint)a * UInt16.MaxValue / max)),
                lookupTable[(uint)r * max / a],
                lookupTable[(uint)g * max / a],
                lookupTable[(uint)b * max / a]);
        }

        #endregion
    }
}
