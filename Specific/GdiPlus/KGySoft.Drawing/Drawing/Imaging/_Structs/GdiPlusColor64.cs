#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GdiPlusColor64.cs
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

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
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
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");
            this.b = b;
            this.g = g;
            this.r = r;
            this.a = a;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor64(Color32 c)
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");
            ushort[] lookupTable = ColorsHelper.GetLookupTableSrgb8ToLinear16Bit()!;

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            b = lookupTable[c.B];
            g = lookupTable[c.G];
            r = lookupTable[c.R];
            a = ColorsHelper.ToGdiPlusUInt16(c.A);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor64(Color64 c)
        {
            Debug.Assert( ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");
            ushort[] lookupTable = ColorsHelper.GetLookupTableSrgb16ToLinear16Bit();

            // alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table
            b = lookupTable[c.B];
            g = lookupTable[c.G];
            r = lookupTable[c.R];
            a = ColorsHelper.ToGdiPlusUInt16(c.A);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusColor64(ColorF c)
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            Vector4 scaled = c.Clip().ToRgba() * ColorsHelper.Max16BppValueF + new Vector4(0.5f);
            b = (ushort)scaled.Z;
            g = (ushort)scaled.Y;
            r = (ushort)scaled.X;
            a = (ushort)scaled.W;
#else
            b = ColorsHelper.ToGdiPlusUInt16(c.B);
            g = ColorsHelper.ToGdiPlusUInt16(c.G);
            r = ColorsHelper.ToGdiPlusUInt16(c.R);
            a = ColorsHelper.ToGdiPlusUInt16(c.A);
#endif
        }

        #endregion

        #region Methods
        
        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32()
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");
            byte[] lookupTable = ColorsHelper.GetLookupTableLinear16ToSrgb8Bit();

            // Alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table.
            // We must clip here because invalid values would cause exceptions.
            GdiPlusColor64 clipped = Clip();
            return new Color32(ColorsHelper.ToByte(clipped.a),
                lookupTable[clipped.r],
                lookupTable[clipped.g],
                lookupTable[clipped.b]);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color64 ToColor64()
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");
            ushort[] lookupTable = ColorsHelper.GetLookupTableLinear16ToSrgb16Bit();

            // Alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table.
            // We must clip here because invalid values would cause exceptions.
            GdiPlusColor64 clipped = Clip();
            return new Color64(ColorsHelper.ToUInt16(clipped.a),
                lookupTable[clipped.r],
                lookupTable[clipped.g],
                lookupTable[clipped.b]);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal ColorF ToColorF()
        {
            // Not clipping here because ColorF tolerates out-of-range values

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return ColorF.FromRgba(new Vector4(r, g, b, a) * ColorsHelper.Max16BppInv);
#else      
            return new ColorF(ColorsHelper.ToFloat(a), ColorsHelper.ToFloat(r), ColorsHelper.ToFloat(g), ColorsHelper.ToFloat(b));
#endif
        }

        #endregion

        #region Private Methods

        private GdiPlusColor64 Clip()
        {
            ushort max = ColorsHelper.Max16BppValue;
            return new GdiPlusColor64(Math.Min(a, max), Math.Min(r, max), Math.Min(g, max), Math.Min(b, max));
        }

        #endregion

        #endregion
    }
}
