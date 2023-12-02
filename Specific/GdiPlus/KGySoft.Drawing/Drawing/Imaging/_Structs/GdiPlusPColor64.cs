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
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Note that this converts to and from Color32/64 instead of PColor32/64 because it's faster due to the linear premultiplication.
    /// </summary>
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
        
        #region Internal Constructors

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusPColor64(Color32 c)
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");

            if (c.A == 0)
            {
                this = default;
                return;
            }

            ushort[] lookupTable = ColorsHelper.GetLookupTableSrgb8ToLinear16Bit()!;
            if (c.A == Byte.MaxValue)
            {
                b = lookupTable[c.B];
                g = lookupTable[c.G];
                r = lookupTable[c.R];
                a = ColorsHelper.Max16BppValue;
                return;
            }

            a = ColorsHelper.ToGdiPlusUInt16(c.A);
            b = (ushort)((uint)lookupTable[c.B] * a / ColorsHelper.Max16BppValue);
            g = (ushort)((uint)lookupTable[c.G] * a / ColorsHelper.Max16BppValue);
            r = (ushort)((uint)lookupTable[c.R] * a / ColorsHelper.Max16BppValue);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusPColor64(Color64 c)
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");

            if (c.A == 0)
            {
                this = default;
                return;
            }

            ushort[] lookupTable = ColorsHelper.GetLookupTableSrgb16ToLinear16Bit();
            if (c.A == UInt16.MaxValue)
            {
                b = lookupTable[c.B];
                g = lookupTable[c.G];
                r = lookupTable[c.R];
                a = ColorsHelper.Max16BppValue;
                return;
            }

            ushort max = ColorsHelper.Max16BppValue;
            a = ColorsHelper.ToGdiPlusUInt16(c.A);
            b = (ushort)((uint)lookupTable[c.B] * a / max);
            g = (ushort)((uint)lookupTable[c.G] * a / max);
            r = (ushort)((uint)lookupTable[c.R] * a / max);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusPColor64(PColorF c)
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

        #region Private Constructors

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal GdiPlusPColor64(ushort a, ushort r, ushort g, ushort b)
        {
            this.b = b;
            this.g = g;
            this.r = r;
            this.a = a;
        }

        #endregion

        #endregion

        #region Methods

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color32 ToColor32()
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");

            if (a == 0)
                return default;

            byte[] lookupTable = ColorsHelper.GetLookupTableLinear16ToSrgb8Bit();
            GdiPlusPColor64 clipped = Clip();
            ushort max = ColorsHelper.Max16BppValue;
            if (clipped.a == max)
                return new Color32(Byte.MaxValue, lookupTable[clipped.r], lookupTable[clipped.g], lookupTable[clipped.b]);

            // Alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table.
            // We must clip here because invalid values would cause exceptions.
            return new Color32(ColorsHelper.ToByte(clipped.a),
                lookupTable[(uint)clipped.r * max / clipped.a],
                lookupTable[(uint)clipped.g * max / clipped.a],
                lookupTable[(uint)clipped.b * max / clipped.a]);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal Color64 ToColor64()
        {
            Debug.Assert(ColorsHelper.LinearWideColors, "This type is not expected to be used when wide formats are the same as KnowPixelFormats on the current platform");

            if (a == 0)
                return default;

            ushort[] lookupTable = ColorsHelper.GetLookupTableLinear16ToSrgb16Bit();
            GdiPlusPColor64 clipped = Clip();
            ushort max = ColorsHelper.Max16BppValue;
            if (clipped.a == max)
                return new Color64(UInt16.MaxValue, lookupTable[clipped.r], lookupTable[clipped.g], lookupTable[clipped.b]);

            // Alpha is always scaled linearly whereas other components may have a gamma correction in the lookup table.
            // We must clip here because invalid values would cause exceptions.
            return new Color64(ColorsHelper.ToUInt16(clipped.a),
                lookupTable[(uint)clipped.r * max / clipped.a],
                lookupTable[(uint)clipped.g * max / clipped.a],
                lookupTable[(uint)clipped.b * max / clipped.a]);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal PColorF ToPColorF()
        {
            // Not clipping here because PColorF tolerates out-of-range values

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return PColorF.FromRgba(new Vector4(r, g, b, a) * ColorsHelper.Max16BppInv);
#else      
            return new PColorF(ColorsHelper.ToFloat(a), ColorsHelper.ToFloat(r), ColorsHelper.ToFloat(g), ColorsHelper.ToFloat(b));
#endif
        }

        #endregion

        #region Private Methods

        private GdiPlusPColor64 Clip()
        {
            ushort aClipped = Math.Min(a, ColorsHelper.Max16BppValue);
            return new GdiPlusPColor64(aClipped, Math.Min(r, aClipped), Math.Min(g, aClipped), Math.Min(b, aClipped));
        }

        #endregion

        #endregion
    }
}
