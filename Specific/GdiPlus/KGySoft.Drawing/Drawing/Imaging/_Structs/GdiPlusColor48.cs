#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GdiPlusColor48.cs
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
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    internal readonly struct GdiPlusColor48
    {
        #region Fields

        [FieldOffset(0)]private readonly ushort b;
        [FieldOffset(2)]private readonly ushort g;
        [FieldOffset(4)]private readonly ushort r;

        #endregion

        #region Constructors

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor48(Color32 c)
        {
            ushort[]? lookupTable = ColorsHelper.GetLookupTable8To16Bpp();
            if (lookupTable == null)
            {
                b = (ushort)((c.B << 8) | c.B);
                g = (ushort)((c.G << 8) | c.G);
                r = (ushort)((c.R << 8) | c.R);
                return;
            }

            b = lookupTable[c.B];
            g = lookupTable[c.G];
            r = lookupTable[c.R];
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32()
        {
            byte[]? lookupTable = ColorsHelper.GetLookupTable16To8Bpp();
            return lookupTable == null
                ? new Color32((byte)(r >> 8), (byte)(g >> 8), (byte)(b >> 8))
                : new Color32(lookupTable[r], lookupTable[g], lookupTable[b]);
        }

        #endregion
    }
}
