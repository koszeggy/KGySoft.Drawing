#region Copyright

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
using System.Windows.Media;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    /// <summary>
    /// Contains extension methods for the <see cref="Color"/> type.
    /// </summary>
    public static class ColorExtensions
    {
        #region Constants

        private const float rLum = 0.299f;
        private const float gLum = 0.587f;
        private const float bLum = 0.114f;

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="Color32">KGySoft.Drawing.Imaging.Color32</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color32 ToColor32(this Color color) => new Color32(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="Color32">KGySoft.Drawing.Imaging.Color32</see> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this Color32 color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="Color">System.Windows.Media.Color</see> struct to <see cref="System.Drawing.Color">System.Drawing.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static System.Drawing.Color ToDrawingColor(this Color color) => System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Converts a <see cref="System.Drawing.Color">System.Drawing.Color</see> struct to <see cref="Color">System.Windows.Media.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToMediaColor(this System.Drawing.Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        #endregion

        #region Internal Methods

        internal static float GetBrightnessLinear(this Color32 color) => ToLinear((color.R * rLum + color.G * gLum + color.B * bLum) / 255f);

        internal static byte To8Bit(this float value)
        {
            if (Single.IsNaN(value))
                return 0;

            value = value * 255f + 0.5f;
            return value < Byte.MinValue ? Byte.MinValue
                : value > Byte.MaxValue ? Byte.MaxValue
                : (byte)value;
        }

        internal static byte ToNonLinear8Bit(this float value) => value switch
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

        #endregion
    }
}
