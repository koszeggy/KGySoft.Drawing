﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains extension methods for the <see cref="Color32"/> struct.
    /// </summary>
    public static class ColorExtensions
    {
        #region Constants

        internal const float RLum = 0.299f;
        internal const float GLum = 0.587f;
        internal const float BLum = 0.114f;

        #endregion

        #region Methods

        #region Public Methods

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
                : (byte)(c.R * RLum + c.G * GLum + c.B * BLum);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/>.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="Color32.A"/> is 255); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        public static Color32 Blend(this Color32 foreColor, Color32 backColor)
            => foreColor.A == Byte.MaxValue ? foreColor
                : backColor.A == Byte.MaxValue ? foreColor.BlendWithBackground(backColor)
                : foreColor.BlendWith(backColor);

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

        #endregion

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToPremultiplied(this Color32 c)
        {
            if (c.A == Byte.MaxValue)
                return c;
            if (c.A == 0)
                return default;

            return new Color32(c.A,
                (byte)(c.R * c.A / Byte.MaxValue),
                (byte)(c.G * c.A / Byte.MaxValue),
                (byte)(c.B * c.A / Byte.MaxValue));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 AsValidPremultiplied(this Color32 c)
        {
            Debug.Assert(c.A > 0 && c.A < Byte.MaxValue);
            return new Color32(c.A,
                Math.Min(c.A, c.R),
                Math.Min(c.A, c.G),
                Math.Min(c.A, c.B));
        }
        
        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 ToPremultiplied(this Color64 c)
        {
            if (c.A == UInt16.MaxValue)
                return c;
            if (c.A == 0)
                return default;

            return new Color64(c.A,
                (ushort)((uint)c.R * c.A / UInt16.MaxValue),
                (ushort)((uint)c.G * c.A / UInt16.MaxValue),
                (ushort)((uint)c.B * c.A / UInt16.MaxValue));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToStraight(this Color32 c)
        {
            if (c.A == Byte.MaxValue)
                return c;
            if (c.A == 0)
                return default;

            return new Color32(
                c.A,
                c.A == 0 ? (byte)0 : (byte)(c.R * Byte.MaxValue / c.A),
                c.A == 0 ? (byte)0 : (byte)(c.G * Byte.MaxValue / c.A),
                c.A == 0 ? (byte)0 : (byte)(c.B * Byte.MaxValue / c.A));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 ToStraightSafe(this Color32 c)
        {
            if (c.A == Byte.MaxValue)
                return c;
            if (c.A == 0)
                return default;

            return new Color32(
                c.A,
                c.A == 0 ? (byte)0 : (byte)(Math.Min(c.A, c.R) * Byte.MaxValue / c.A),
                c.A == 0 ? (byte)0 : (byte)(Math.Min(c.A, c.G) * Byte.MaxValue / c.A),
                c.A == 0 ? (byte)0 : (byte)(Math.Min(c.A, c.B) * Byte.MaxValue / c.A));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 ToStraight(this Color64 c)
        {
            if (c.A == UInt16.MaxValue)
                return c;
            if (c.A == 0)
                return default;

            return new Color64(
                c.A,
                c.A == 0 ? (ushort)0 : (ushort)((uint)c.R * UInt16.MaxValue / c.A),
                c.A == 0 ? (ushort)0 : (ushort)((uint)c.G * UInt16.MaxValue / c.A),
                c.A == 0 ? (ushort)0 : (ushort)((uint)c.B * UInt16.MaxValue / c.A));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackground(this Color32 c, Color32 backColor)
        {
            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor.ToOpaque();
            float alpha = c.A / 255f;
            float inverseAlpha = 1f - alpha;
            return new Color32(Byte.MaxValue,
                (byte)(c.R * alpha + backColor.R * inverseAlpha),
                (byte)(c.G * alpha + backColor.G * inverseAlpha),
                (byte)(c.B * alpha + backColor.B * inverseAlpha));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWith(this Color32 src, Color32 dst)
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
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithPremultiplied(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0, "Partially transparent colors are expected");

            float inverseAlphaSrc = (255 - src.A) / 255f;

            return new Color32((byte)(src.A + dst.A * inverseAlphaSrc),
                (byte)(src.R + dst.R * inverseAlphaSrc),
                (byte)(src.G + dst.G * inverseAlphaSrc),
                (byte)(src.B + dst.B * inverseAlphaSrc));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool TolerantEquals(this Color32 c1, Color32 c2, byte tolerance)
        {
            Debug.Assert(c1.A == 255 && c2.A == 255);
            if (c1 == c2)
                return true;
            return Math.Abs(c1.R - c2.R) <= tolerance && Math.Abs(c1.G - c2.G) <= tolerance && Math.Abs(c1.B - c2.B) <= tolerance;
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool TolerantEquals(this Color32 c1, Color32 c2, byte tolerance, Color32 backColor)
        {
            Debug.Assert(c1.A == 255);
            return TolerantEquals(c1, c2.BlendWithBackground(backColor), tolerance);
        }

        #endregion

        #endregion
    }
}
