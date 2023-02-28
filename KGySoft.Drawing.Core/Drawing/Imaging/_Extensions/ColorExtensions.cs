﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensions.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains extension methods for the <see cref="Color"/> and <see cref="Color32"/> types.
    /// </summary>
    public static class ColorExtensions
    {
        #region Constants

        // In sRGB color space using the coefficients used also by the Y'UV color space (used by PAL/SECAM/NTSC systems)
        // because for gamma compressed RGB values it approximates perceptual brightness quite well so no linear conversion is needed.
        private const float RLumSrgb = 0.299f;
        private const float GLumSrgb = 0.587f;
        private const float BLumSrgb = 0.114f;

        // In the linear color space using the coefficients recommended by the ITU-R BT.709 standard.
        // The values were taken from here: https://en.wikipedia.org/wiki/Grayscale
        private const float RLumLinear = 0.2126f;
        private const float GLumLinear = 0.7152f;
        private const float BLumLinear = 0.0722f;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>A <see cref="Color32"/> instance that represents the original <see cref="Color"/> instance.</returns>
        public static Color32 ToColor32(this Color color) => new Color32(color);

        /// <summary>
        /// Gets the brightness of a <see cref="Color32"/> instance as a <see cref="byte">byte</see> based on human perception.
        /// The <see cref="Color32.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color32"/> instance to get the brightness of.</param>
        /// <returns>A <see cref="byte">byte</see> value where 0 represents the darkest and 255 represents the brightest possible value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness(this Color32 c)
            => c.R == c.G && c.R == c.B
                ? c.R
                : (byte)(c.R * RLumSrgb + c.G * GLumSrgb + c.B * BLumSrgb);

        /// <summary>
        /// Gets the brightness of a <see cref="Color32"/> instance as a <see cref="byte">byte</see> based on human perception.
        /// The <see cref="Color32.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color32"/> instance to get the brightness of.</param>
        /// <param name="colorSpace">The color space to be used for determining the brightness. If <see cref="WorkingColorSpace.Default"/>, then the sRGB color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the sRGB color space will be used as well.</param>
        /// <returns>A <see cref="byte">byte</see> value where 0 represents the darkest and 255 represents the brightest possible value.</returns>
        /// <remarks>
        /// <note>This method always returns a gamma corrected result, even if <paramref name="colorSpace"/> is <see cref="WorkingColorSpace.Linear"/>.
        /// To get the brightness in the linear color space use the <see cref="GetBrightness(ColorF, WorkingColorSpace)"/> method instead.</note>
        /// </remarks>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness(this Color32 c, WorkingColorSpace colorSpace) => colorSpace == WorkingColorSpace.Linear
            // Note: using gamma correction even for linear color space because we the source is an sRGB color
            ? ColorSpaceHelper.LinearToSrgb8Bit(c.ToColorF().GetBrightness())
            : GetBrightness(c);

        /// <summary>
        /// Gets the brightness of a <see cref="Color32"/> instance as a <see cref="float">float</see> value based on human perception.
        /// The <see cref="Color32.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color32"/> instance to get the brightness of.</param>
        /// <returns>A <see cref="float">float</see> value where 0 represents the darkest and 1 represents the brightest possible value.</returns>
        /// <remarks>
        /// <note>This method always returns a gamma corrected result.
        /// To get the brightness in the linear color space use the <see cref="GetBrightness(ColorF)"/> method instead.</note>
        /// </remarks>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF(this Color32 c) => c.R * RLumSrgb + c.G * GLumSrgb + c.B * BLumSrgb / Byte.MaxValue;

        /// <summary>
        /// Gets the brightness of a <see cref="Color32"/> instance as a <see cref="float">float</see> value based on human perception.
        /// The <see cref="Color32.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color32"/> instance to get the brightness of.</param>
        /// <param name="colorSpace">The color space to be used for determining the brightness. If <see cref="WorkingColorSpace.Default"/>, then the sRGB color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the sRGB color space will be used as well.</param>
        /// <returns>A <see cref="float">float</see> value where 0 represents the darkest and 1 represents the brightest possible value.</returns>
        /// <remarks>
        /// <note>This method always returns a gamma corrected result, even if <paramref name="colorSpace"/> is <see cref="WorkingColorSpace.Linear"/>.
        /// To get the brightness in the linear color space use the <see cref="GetBrightness(ColorF, WorkingColorSpace)"/> method instead.</note>
        /// </remarks>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF(this Color32 c, WorkingColorSpace colorSpace) => colorSpace == WorkingColorSpace.Linear
            // Note: using gamma correction even for linear color space because we need a result based of human perception
            ? ColorSpaceHelper.LinearToSrgb(c.ToColorF().GetBrightness())
            : GetBrightnessF(c);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the sRGB color space.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="Color32.A"/> is 255); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color32 Blend(this Color32 foreColor, Color32 backColor)
            => foreColor.A == Byte.MaxValue ? foreColor
                : backColor.A == Byte.MaxValue ? foreColor.BlendWithBackgroundSrgb(backColor)
                : foreColor.A == 0 ? backColor
                : backColor.A == 0 ? foreColor
                : foreColor.BlendWithSrgb(backColor);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the specified <paramref name="colorSpace"/>.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="Color32.A"/> is 255); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <param name="colorSpace">The color space to be used for the blending. If <see cref="WorkingColorSpace.Default"/>, then the sRGB color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the sRGB color space will be used as well.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color32 Blend(this Color32 foreColor, Color32 backColor, WorkingColorSpace colorSpace)
            => foreColor.A == Byte.MaxValue ? foreColor
                : backColor.A == Byte.MaxValue ? foreColor.BlendWithBackground(backColor, colorSpace)
                : foreColor.A == 0 ? backColor
                : backColor.A == 0 ? foreColor
                : foreColor.BlendWith(backColor, colorSpace);

        /// <summary>
        /// Gets whether two <see cref="Color32"/> instances are equal using a specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="c1">The first color to compare.</param>
        /// <param name="c2">The second color to compare.</param>
        /// <param name="tolerance">The allowed tolerance for ARGB components.</param>
        /// <param name="alphaThreshold">Specifies a threshold under which colors are considered transparent. If both colors have lower <see cref="Color32.A"/> value than the threshold, then they are considered equal.
        /// If only one of the specified colors has lower <see cref="Color32.A"/> value than the threshold, then the colors are considered different.
        /// If both colors' <see cref="Color32.A"/> value are equal to or greater than this value, then <paramref name="tolerance"/> is applied to the <see cref="Color32.A"/> value, too. This parameter is optional.
        /// <br/>Default value: 0.</param>
        /// <returns><see langword="true"/>, if the colors are considered equal with the specified <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter", Justification = "False alarm, the 'hiding' method is internal so 3rd party consumers call always this method.")]
        public static bool TolerantEquals(this Color32 c1, Color32 c2, byte tolerance, byte alphaThreshold = 0)
        {
            if (c1 == c2 || c1.A < alphaThreshold && c2.A < alphaThreshold)
                return true;
            if ((c1.A < alphaThreshold) ^ (c2.A < alphaThreshold))
                return false;
            return Math.Abs(c1.R - c2.R) <= tolerance && Math.Abs(c1.G - c2.G) <= tolerance && Math.Abs(c1.B - c2.B) <= tolerance && Math.Abs(c1.A - c2.A) <= tolerance;
        }

        /// <summary>
        /// Converts this straight <see cref="Color32"/> value to a premultiplied <see cref="PColor32"/> value.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> value to convert.</param>
        /// <returns>A premultiplied <see cref="PColor32"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor32 ToPremultiplied(this Color32 color) => new PColor32(color);

        /// <summary>
        /// Converts this premultiplied <see cref="PColor32"/> value to a straight <see cref="Color32"/> value.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> value to convert.</param>
        /// <returns>A straight <see cref="Color32"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color32 ToStraight(this PColor32 color) => color.ToColor32();

        /// <summary>
        /// Converts this premultiplied <see cref="PColor32"/> instance containing possibly invalid RGB values to a straight <see cref="Color32"/> value.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> value to convert.</param>
        /// <returns>A straight <see cref="Color32"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color32 ToStraightSafe(this PColor32 color) => color.Clip().ToColor32();

        #endregion

        #region Internal Methods

        internal static ColorF ToColorF(this Color32 c) => new ColorF(c);
        internal static PColorF ToPColorF(this Color32 c) => new PColorF(c);

        internal static PColor64 ToPremultiplied(this Color64 c) => new PColor64(c);
        internal static Color64 ToStraight(this PColor64 c) => c.ToColor64();
        internal static Color64 ToStraightSafe(this PColor64 c) => c.Clip().ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackground(this Color32 c, Color32 backColor, bool linear)
            => linear ? c.BlendWithBackgroundLinear(backColor) : c.BlendWithBackgroundSrgb(backColor);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackground(this Color32 c, Color32 backColor, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Linear ? c.BlendWithBackgroundLinear(backColor) : c.BlendWithBackgroundSrgb(backColor);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackgroundSrgb(this Color32 c, Color32 backColor)
        {
            Debug.Assert(c.A != 255, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == 255, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            int inverseAlpha = 255 - c.A;
            return new Color32(Byte.MaxValue,
                (byte)((c.R * c.A + backColor.R * inverseAlpha) / 255),
                (byte)((c.G * c.A + backColor.G * inverseAlpha) / 255),
                (byte)((c.B * c.A + backColor.B * inverseAlpha) / 255));
        }

        internal static Color32 BlendWithBackgroundLinear(this Color32 c, Color32 backColor)
            => c.A == 0 ? backColor : c.ToColorF().BlendWithBackground(backColor.ToColorF()).ToColor32();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWith(this Color32 src, Color32 dst, bool linear)
            => linear ? src.BlendWithLinear(dst) : src.BlendWithSrgb(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWith(this Color32 src, Color32 dst, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Linear ? src.BlendWithLinear(dst) : src.BlendWithSrgb(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithSrgb(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");

            float alphaSrc = src.A / 255f;
            float alphaDst = dst.A / 255f;
            float inverseAlphaSrc = 1f - alphaSrc;
            float alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;

            return new Color32((byte)(alphaOut * 255),
                (byte)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) / alphaOut),
                (byte)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) / alphaOut),
                (byte)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) / alphaOut));

            // This would be the floating point free version but in practice it's not faster at all (at least on my computer):
            //int inverseAlphaSrc = 255 - src.A;
            //int alphaOut = src.A + ((dst.A * inverseAlphaSrc) >> 8);

            //return new Color32((byte)alphaOut,
            //    (byte)((src.R * src.A + ((dst.R * dst.A * inverseAlphaSrc) >> 8)) / alphaOut),
            //    (byte)((src.G * src.A + ((dst.G * dst.A * inverseAlphaSrc) >> 8)) / alphaOut),
            //    (byte)((src.B * src.A + ((dst.B * dst.A * inverseAlphaSrc) >> 8)) / alphaOut));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithLinear(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");
            return src.ToColorF().BlendWith(dst.ToColorF()).ToColor32();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 BlendWithPremultipliedSrgb(this PColor32 src, PColor32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0, "Partially transparent colors are expected");
            int inverseAlphaSrc = 255 - src.A;
            return new PColor32(dst.A == Byte.MaxValue ? Byte.MaxValue : (byte)(src.A + ((dst.A * inverseAlphaSrc) >> 8)),
                (byte)(src.R + ((dst.R * inverseAlphaSrc) >> 8)),
                (byte)(src.G + ((dst.G * inverseAlphaSrc) >> 8)),
                (byte)(src.B + ((dst.B * inverseAlphaSrc) >> 8)));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithBackground(this ColorF c, ColorF backColor)
        {
            if (c.A <= 0)
                return backColor;
            float inverseAlpha = 1f - c.A;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(new Vector4(c.Rgb * c.A + backColor.Rgb * inverseAlpha, 1f));
#else
            return new ColorF(1f,
                c.R * c.A + backColor.R * inverseAlpha,
                c.G * c.A + backColor.G * inverseAlpha,
                c.B * c.A + backColor.B * inverseAlpha);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWith(this ColorF src, ColorF dst)
        {
            float inverseAlphaSrc = 1f - src.A;
            float alphaOut = src.A + dst.A * inverseAlphaSrc;

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new ColorF(new Vector4((src.Rgb * src.A + dst.Rgb * inverseAlphaSrc) / alphaOut, alphaOut));
#else
            return new ColorF(alphaOut,
                (src.R * src.A + dst.R * dst.A * inverseAlphaSrc) / alphaOut,
                (src.G * src.A + dst.G * dst.A * inverseAlphaSrc) / alphaOut,
                (src.B * src.A + dst.B * dst.A * inverseAlphaSrc) / alphaOut);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColorF BlendWith(this PColorF src, PColorF dst)
        {
            float inverseAlphaSrc = 1f - src.A;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return new PColorF(new Vector4(src.Rgb + dst.Rgb * inverseAlphaSrc, dst.A >= 1f ? 1f : src.A + dst.A * inverseAlphaSrc));
#else
            return new PColorF(dst.A >= 1f ? 1f : src.A + dst.A * inverseAlphaSrc,
                src.R + dst.R * inverseAlphaSrc,
                src.G + dst.G * inverseAlphaSrc,
                src.B + dst.B * inverseAlphaSrc);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ushort GetBrightness(this Color64 c)
            => c.R == c.G && c.R == c.B
                ? c.R
                : (ushort)(c.R * RLumSrgb + c.G * GLumSrgb + c.B * BLumSrgb);

        // Note: either do not make this public or 
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static float GetBrightness(this ColorF c)
            => c.R.Equals(c.G) && c.R.Equals(c.B)
                ? c.R
                : c.R * RLumLinear + c.G * GLumLinear + c.B * BLumLinear;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool TolerantEquals(this Color32 c1, Color32 c2, byte tolerance)
        {
            Debug.Assert(c1.A == 255 && c2.A == 255);
            if (c1 == c2)
                return true;
            return Math.Abs(c1.R - c2.R) <= tolerance && Math.Abs(c1.G - c2.G) <= tolerance && Math.Abs(c1.B - c2.B) <= tolerance;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool TolerantEquals(this Color32 c1, Color32 c2, byte tolerance, Color32 backColor, WorkingColorSpace colorSpace)
        {
            Debug.Assert(c1.A == 255 && backColor.A == 255);
            return TolerantEquals(c1, c2.Blend(backColor, colorSpace), tolerance);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "4B is confusable with 48")]
        internal static int Get4bppColorIndex(byte nibbles, int x) => (x & 1) == 0
            ? nibbles >> 4
            : nibbles & 0b00001111;

        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "4B is confusable with 48")]
        internal static void Set4bppColorIndex(ref byte nibbles, int x, int colorIndex)
        {
            if ((x & 1) == 0)
            {
                nibbles &= 0b00001111;
                nibbles |= (byte)(colorIndex << 4);
            }
            else
            {
                nibbles &= 0b11110000;
                nibbles |= (byte)colorIndex;
            }
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "1B is confusable with 18")]
        internal static int Get1bppColorIndex(byte bits, int x)
        {
            int mask = 128 >> (x & 7);
            return (bits & mask) != 0 ? 1 : 0;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "1B is confusable with 18")]
        internal static void Set1bppColorIndex(ref byte bits, int x, int colorIndex)
        {
            int mask = 128 >> (x & 7);
            if (colorIndex == 0)
                bits &= (byte)~mask;
            else
                bits |= (byte)mask;
        }

        #endregion

        #endregion
    }
}
