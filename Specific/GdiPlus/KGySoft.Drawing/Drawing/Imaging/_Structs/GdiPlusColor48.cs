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
            Debug.Assert(ColorsHelper.GetLookupTableSrgb8ToLinear16Bit() != null);
            ushort[] lookupTable = ColorsHelper.GetLookupTableSrgb8ToLinear16Bit()!;

            b = lookupTable[c.B];
            g = lookupTable[c.G];
            r = lookupTable[c.R];
        }

        #endregion

        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32()
        {
            Debug.Assert(ColorsHelper.GetLookupTableLinear16ToSrgb8Bit() != null);
            byte[] lookupTable = ColorsHelper.GetLookupTableLinear16ToSrgb8Bit()!;
            return new Color32(lookupTable[r], lookupTable[g], lookupTable[b]);
        }

        #endregion
    }
}
