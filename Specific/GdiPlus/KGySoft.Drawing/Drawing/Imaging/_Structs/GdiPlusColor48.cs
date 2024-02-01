#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GdiPlusColor48.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
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
        
        #region Internal Constructors

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor48(Color32 c)
        {
            Debug.Assert(ColorsHelper.LinearWideColors);
            Debug.Assert(c.A == Byte.MaxValue);
            ushort[] lookupTable = ColorsHelper.GetLookupTableSrgb8ToLinear16Bit()!;

            b = lookupTable[c.B];
            g = lookupTable[c.G];
            r = lookupTable[c.R];
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor48(Color64 c)
        {
            Debug.Assert(ColorsHelper.LinearWideColors);
            Debug.Assert(c.A == UInt16.MaxValue);
            ushort[] lookupTable = ColorsHelper.GetLookupTableSrgb16ToLinear16Bit();

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            b = lookupTable[c.B];
            g = lookupTable[c.G];
            r = lookupTable[c.R];
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor48(ColorF c)
        {
            Debug.Assert(ColorsHelper.LinearWideColors);
            Debug.Assert(c.A >= 1f);

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            Vector4 scaled = c.Clip().ToRgba() * ColorsHelper.Max16BppValueF + new Vector4(0.5f);
            b = (ushort)scaled.Z;
            g = (ushort)scaled.Y;
            r = (ushort)scaled.X;
#else
            b = ColorsHelper.ToGdiPlusUInt16(c.B);
            g = ColorsHelper.ToGdiPlusUInt16(c.G);
            r = ColorsHelper.ToGdiPlusUInt16(c.R);
#endif
        }

        #endregion

        #region Private Constructors

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor48(ushort r, ushort g, ushort b)
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");
            this.b = b;
            this.g = g;
            this.r = r;
        }

        #endregion

        #endregion

        #region Methods

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32()
        {
            Debug.Assert(ColorsHelper.LinearWideColors);
            byte[] lookupTable = ColorsHelper.GetLookupTableLinear16ToSrgb8Bit()!;

            GdiPlusColor48 clipped = Clip();
            return new Color32(lookupTable[clipped.r],
                lookupTable[clipped.g],
                lookupTable[clipped.b]);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color64 ToColor64()
        {
            Debug.Assert(ColorsHelper.LinearWideColors);

            ushort[] lookupTable = ColorsHelper.GetLookupTableLinear16ToSrgb16Bit();

            // Alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table.
            // We must clip here because invalid values would cause exceptions.
            GdiPlusColor48 clipped = Clip();
            return new Color64(lookupTable[clipped.r],
                lookupTable[clipped.g],
                lookupTable[clipped.b]);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal ColorF ToColorF()
        {
            // Not clipping here because ColorF tolerates out-of-range values

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return ColorF.FromRgba(new Vector4(r, g, b, ColorsHelper.Max16BppValue) * ColorsHelper.Max16BppInv);
#else      
            return new ColorF(ColorsHelper.ToFloat(r), ColorsHelper.ToFloat(g), ColorsHelper.ToFloat(b));
#endif
        }

        #endregion

        #region Private Methods

        private GdiPlusColor48 Clip()
        {
            ushort max = ColorsHelper.Max16BppValue;
            return new GdiPlusColor48(Math.Min(r, max), Math.Min(g, max), Math.Min(b, max));
        }

        #endregion

        #endregion
    }
}
