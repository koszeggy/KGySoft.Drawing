﻿#region Copyright

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
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
            Debug.Assert(ColorsHelper.GetLookupTable8To16Bpp() != null);
            ushort[] lookupTable = ColorsHelper.GetLookupTable8To16Bpp()!;
            ushort max = ColorsHelper.Max16BppValue;

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            b = lookupTable[c.B];
            g = lookupTable[c.G];
            r = lookupTable[c.R];
            a = c.A == Byte.MaxValue ? max : (ushort)((uint)ColorSpaceHelper.ToUInt16(c.A) * max / UInt16.MaxValue);
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32()
        {
            Debug.Assert(ColorsHelper.GetLookupTable16To8Bpp() != null);
            byte[] lookupTable = ColorsHelper.GetLookupTable16To8Bpp()!;
            ushort max = ColorsHelper.Max16BppValue;

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            return new Color32(a == max ? Byte.MaxValue : ColorSpaceHelper.ToByte((ushort)((uint)a * UInt16.MaxValue / max)),
                lookupTable[r],
                lookupTable[g],
                lookupTable[b]);
        }

        #endregion
    }
}
