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
        #region Nested Classes

        #region Cache8Bpp class

        private static class Cache8Bpp
        {
            #region Fields

            internal static readonly byte[] LookupTableSrgbToLinear = InitLookupTableSrgbToLinear();
            internal static readonly byte[] LookupTableLinearToSrgb = InitLookupTableLinearToSrgb();

            #endregion

            #region Methods

            private static byte[] InitLookupTableSrgbToLinear()
            {
                var result = new byte[1 << 8];
                for (int i = 0; i <= Byte.MaxValue; i++)
                    result[i] = ColorSpaceHelper.ToByte(ColorSpaceHelper.SrgbToLinear((byte)i));
                return result;
            }

            private static byte[] InitLookupTableLinearToSrgb()
            {
                var result = new byte[1 << 8];
                for (int i = 0; i <= Byte.MaxValue; i++)
                    result[i] = ColorSpaceHelper.LinearToSrgb8Bit(ColorSpaceHelper.ToFloat((byte)i));
                return result;
            }

            #endregion
        }

        #endregion

        #endregion

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

        internal static byte ToLinear(this byte b) => Cache8Bpp.LookupTableSrgbToLinear[b];
        internal static byte ToSrgb(this byte b) => Cache8Bpp.LookupTableLinearToSrgb[b];
        internal static byte ToLinearByte(this float f) => ColorSpaceHelper.ToByte(ColorSpaceHelper.SrgbToLinear(f));

        #endregion
    }
}
