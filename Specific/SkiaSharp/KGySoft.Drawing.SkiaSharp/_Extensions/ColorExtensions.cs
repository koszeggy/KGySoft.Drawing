#region Copyright

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
using System.Drawing;
using System.Runtime.CompilerServices;

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    public static class ColorExtensions
    {
        #region Methods

        /// <summary>
        /// Converts an <see cref="SKColor"/> struct to <see cref="Color32">KGySoft.Drawing.Imaging.Color32</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color32 ToColor32(this SKColor color) => Unsafe.As<SKColor, Color32>(ref color);

        /// <summary>
        /// Converts an <see cref="SKColor"/> struct to <see cref="Color32">KGySoft.Drawing.Imaging.Color32</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this Color32 color) => new SKColor((uint)color.ToArgb());

        /// <summary>
        /// Converts an <see cref="SKColor"/> struct to <see cref="Color">System.Drawing.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToColor(this SKColor color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

        /// <summary>
        /// Converts a <see cref="Color">System.Drawing.Color</see> struct to <see cref="SKColor"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this Color color) => new SKColor((uint)color.ToArgb());

        #endregion

        #region Internal Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Color32 ToStraight(this Color32 c) => c.A switch
        {
            Byte.MaxValue => c,
            0 => default,
            _ => new Color32(c.A,
                (byte)((c.R << 8) / c.A),
                (byte)((c.G << 8) / c.A),
                (byte)((c.B << 8) / c.A))
                //(byte)(c.R * Byte.MaxValue / c.A),
                //(byte)(c.G * Byte.MaxValue / c.A),
                //(byte)(c.B * Byte.MaxValue / c.A))
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Color32 ToPremultiplied(this Color32 c) => c.A switch
        {
            Byte.MaxValue => c,
            0 => default,
            _ => new Color32(c.A,
                (byte)((c.R * c.A) >> 8),
                (byte)((c.G * c.A) >> 8),
                (byte)((c.B * c.A) >> 8))
                //(byte)(c.R * c.A / Byte.MaxValue),
                //(byte)(c.G * c.A / Byte.MaxValue),
                //(byte)(c.B * c.A / Byte.MaxValue))
        };

        internal static Color32 ToSrgb(this Color32 c) => new Color32(c.A, c.R.ToSrgb(), c.G.ToSrgb(), c.B.ToSrgb());

        internal static Color32 ToLinear(this Color32 c) => new Color32(c.A, c.R.ToLinear(), c.G.ToLinear(), c.B.ToLinear());

        internal static byte To8Bit(this float value)
        {
            if (Single.IsNaN(value))
                return 0;

            value = value * 255f + 0.5f;
            return value < Byte.MinValue ? Byte.MinValue
                : value > Byte.MaxValue ? Byte.MaxValue
                : (byte)value;
        }

        internal static byte ToSrgb(this byte b) => (b / 255f).ToSrgb8Bit();
        internal static byte ToLinear(this byte b) => (b / 255f).ToLinear().To8Bit();

        internal static byte ToSrgb8Bit(this float value) => value switch
        {
            <= 0f => 0,
            <= 0.0031308f => (byte)((255f * value * 12.92f) + 0.5f),
            < 1f => (byte)((255f * ((1.055f * MathF.Pow(value, 1f / 2.4f)) - 0.055f)) + 0.5f),
            >= 1f => 255,
            _ => 0 // NaN
        };

        internal static float ToLinear(this float value) => value switch
        {
            <= 0f => 0f,
            <= 0.04045f => value / 12.92f,
            < 1f => MathF.Pow((value + 0.055f) / 1.055f, 2.4f),
            >= 1f => 1f,
            _ => 0 // NaN
        };

        #endregion
    }
}
