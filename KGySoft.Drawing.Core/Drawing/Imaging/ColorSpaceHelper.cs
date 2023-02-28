#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorSpaceHelper.cs
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
    /// <summary>
    /// A helper class containing low-level conversion methods for <see cref="byte">byte</see> and <see cref="float">float</see> types
    /// to convert color components using the sRGB and linear color spaces.
    /// </summary>
    public static class ColorSpaceHelper
    {
        #region Nested Classes

        #region ByteToLinearCache class

        private static class ByteToLinearCache
        {
            #region Fields

            internal static readonly float[] LookupTable = InitLookupTable();

            #endregion

            #region Methods

            private static float[] InitLookupTable()
            {
                var result = new float[1 << 8];
                for (int i = 0; i <= Byte.MaxValue; i++)
                    result[i] = SrgbToLinear(i / (float)Byte.MaxValue);

                return result;
            }

            #endregion
        }

        #endregion

        #region UInt16ToLinearCache class

        private static class UInt16ToLinearCache
        {
            #region Fields

            internal static readonly float[] LookupTable = InitLookupTable();

            #endregion

            #region Methods

            private static float[] InitLookupTable()
            {
                var result = new float[1 << 16];
                for (int i = 0; i <= UInt16.MaxValue; i++)
                    result[i] = SrgbToLinear(i / (float)UInt16.MaxValue);

                return result;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Converts a <see cref="byte">byte</see> to a floating-point value between 0 and 1 without changing the color space.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A floating-point value between 0 and 1.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float ToFloat(byte value) => (float)value / Byte.MaxValue;

        /// <summary>
        /// Converts a <see cref="ushort"/> value to a floating-point value between 0 and 1 without changing the color space.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A floating-point value between 0 and 1.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float ToFloat(ushort value) => (float)value / UInt16.MaxValue;

        /// <summary>
        /// Converts a floating-point value ranging from 0 to 1 to a <see cref="byte">byte</see> without changing the color space.
        /// Out-of-range values are allowed in which case the result will be clipped
        /// to <see cref="Byte.MinValue">Byte.MinValue</see> or <see cref="Byte.MaxValue">Byte.MaxValue</see>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte ToByte(float value)
        {
            // Not using Math.Clamp because that does not convert NaN
            value = value * Byte.MaxValue + 0.5f;
            return value < Byte.MinValue ? Byte.MinValue
                : value > Byte.MaxValue ? Byte.MaxValue
                : (byte)value; // including NaN, which will be 0
        }

        /// <summary>
        /// Converts a 16-bit color channel value to an 8-bit value representing the same intensity without changing the color space.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte ToByte(ushort value) => (byte)(value >> 8);

        /// <summary>
        /// Converts a floating-point value ranging from 0 to 1 to a <see cref="ushort"/> without changing the color space.
        /// Out-of-range values are allowed in which case the result will be clipped
        /// to <see cref="UInt16.MinValue">UInt16.MinValue</see> or <see cref="UInt16.MaxValue">UInt16.MaxValue</see>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ushort ToUInt16(float value)
        {
            // Not using Math.Clamp because that does not convert NaN
            value = value * UInt16.MaxValue + 0.5f;
            return value < UInt16.MinValue ? UInt16.MinValue
                : value > UInt16.MaxValue ? UInt16.MaxValue
                : (ushort)value; // including NaN, which will be 0
        }

        /// <summary>
        /// Converts an 8-bit color channel value to a 16-bit value representing the same intensity without changing the color space.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ushort ToUInt16(byte value) => (ushort)(value * 257); // same as (ushort)((value << 8) | value)

        /// <summary>
        /// Converts a <see cref="byte">byte</see> value representing an sRGB color component to a floating-point value between 0 and 1
        /// representing an RGB color component in the linear color space.
        /// </summary>
        /// <param name="value">The <see cref="byte">byte</see> value to convert.</param>
        /// <returns>A floating-point value between 0 and 1 representing an RGB color component in the linear color space.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float SrgbToLinear(byte value) => ByteToLinearCache.LookupTable[value];

        /// <summary>
        /// Converts a <see cref="byte">byte</see> value representing an sRGB color component to a floating-point value between 0 and 1
        /// representing an RGB color component in the linear color space.
        /// </summary>
        /// <param name="value">The <see cref="byte">byte</see> value to convert.</param>
        /// <returns>A floating-point value between 0 and 1 representing an RGB color component in the linear color space.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float SrgbToLinear(ushort value) => UInt16ToLinearCache.LookupTable[value];

        /// <summary>
        /// Converts a floating-point value representing an sRGB color component to a value representing an RGB color component in the linear color space.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A floating-point value between 0 and 1 representing an RGB color component in the linear color space.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float SrgbToLinear(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => 0f,
            <= 0.04045f => value / 12.92f,
            < 1f => MathF.Pow((value + 0.055f) / 1.055f, 2.4f),
            >= 1f => 1f,
            _ => 0 // NaN
        };

        /// <summary>
        /// Converts a floating-point value representing a color component in the linear color space
        /// to a value representing an sRGB color component.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A floating-point value between 0 and 1 representing an sRGB color component.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float LinearToSrgb(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => 0f,
            <= 0.0031308f => value * 12.92f,
            < 1f => (1.055f * MathF.Pow(value, 1f / 2.4f)) - 0.055f,
            >= 1f => 1f,
            _ => 0 // NaN
        };

        /// <summary>
        /// Converts a floating-point value representing a color component in the linear color space
        /// to a <see cref="byte">byte</see> value representing an sRGB color component.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A <see cref="byte">byte</see> value representing an sRGB color component.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte LinearToSrgb8Bit(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => Byte.MinValue,
            <= 0.0031308f => (byte)((Byte.MaxValue * value * 12.92f) + 0.5f),
            < 1f => (byte)((Byte.MaxValue * ((1.055f * MathF.Pow(value, 1f / 2.4f)) - 0.055f)) + 0.5f),
            >= 1f => Byte.MaxValue,
            _ => 0 // NaN
        };

        /// <summary>
        /// Converts a floating-point value representing a color component in the linear color space
        /// to a <see cref="ushort"/> value representing an sRGB color component.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A <see cref="ushort"/> value representing an sRGB color component.</returns>
        [CLSCompliant(false)]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ushort LinearToSrgb16Bit(float value) => value switch
        {
            // formula is taken from here: https://en.wikipedia.org/wiki/SRGB
            <= 0f => UInt16.MinValue,
            <= 0.0031308f => (ushort)((UInt16.MaxValue * value * 12.92f) + 0.5f),
            < 1f => (ushort)((UInt16.MaxValue * ((1.055f * MathF.Pow(value, 1f / 2.4f)) - 0.055f)) + 0.5f),
            >= 1f => UInt16.MaxValue,
            _ => 0 // NaN
        };

        #endregion
    }
}
