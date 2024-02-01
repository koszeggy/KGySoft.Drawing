#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorExtensions.cs
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
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

#region Suppressions

#if NET8_0_OR_GREATER
#pragma warning disable CS9193 // Argument should be a variable because it is passed to a 'ref readonly' parameter - false alarm
#endif

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// Provides extension methods for the <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> type.
    /// </summary>
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

        #region Cache16Bpp class

        private static class Cache16Bpp
        {
            #region Fields

            internal static readonly ushort[] LookupTableSrgbToLinear = InitLookupTableSrgbToLinear();
            internal static readonly ushort[] LookupTableLinearToSrgb = InitLookupTableLinearToSrgb();

            #endregion

            #region Methods

            private static ushort[] InitLookupTableSrgbToLinear()
            {
                var result = new ushort[1 << 16];
                for (int i = 0; i <= UInt16.MaxValue; i++)
                    result[i] = ColorSpaceHelper.ToUInt16(ColorSpaceHelper.SrgbToLinear((ushort)i));
                return result;
            }

            private static ushort[] InitLookupTableLinearToSrgb()
            {
                var result = new ushort[1 << 16];
                for (int i = 0; i <= UInt16.MaxValue; i++)
                    result[i] = ColorSpaceHelper.LinearToSrgb16Bit(ColorSpaceHelper.ToFloat((ushort)i));
                return result;
            }

            #endregion
        }

        #endregion

        #endregion

        #region Constants

        internal const SKColorType MaxColorType = SKColorType.Bgr101010x;
        internal const SKAlphaType MaxAlphaType = SKAlphaType.Unpremul;

        #endregion

        #region Methods

        #region Public Methods
        // ReSharper disable CommentTypo - SKPM

        #region To SKColor

        /// <summary>
        /// Converts a <see cref="Color">System.Drawing.Color</see> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this Color color) => new SKColor((uint)color.ToArgb());

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this SKPMColor color) => SKPMColor.UnPreMultiply(color);

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then the internal SkiaSharp conversion is used. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default) => sourceColorSpace switch
        {
            WorkingColorSpace.Linear => Unsafe.As<SKColorF, ColorF>(ref color).ToSKColor(),
            WorkingColorSpace.Srgb => Unsafe.As<SKColorF, ColorF>(ref color).ToColor32(false).ToSKColor(),
            _ => (SKColor)color,
        };

        /// <summary>
        /// Converts a <see cref="Color32"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this Color32 color) => new SKColor((uint)color.ToArgb());

        /// <summary>
        /// Converts a <see cref="PColor32"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this PColor32 color) => color.ToColor32().ToSKColor();

        /// <summary>
        /// Converts a <see cref="Color64"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this Color64 color) => color.ToColor32().ToSKColor();

        /// <summary>
        /// Converts a <see cref="PColor64"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this PColor64 color) => color.ToColor32().ToSKColor();

        /// <summary>
        /// Converts a <see cref="ColorF"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this ColorF color) => color.ToColor32().ToSKColor();

        /// <summary>
        /// Converts a <see cref="PColorF"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColor ToSKColor(this PColorF color) => color.ToColor32().ToSKColor();

        #endregion

        #region From SKColor

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <see cref="Color">System.Drawing.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToColor(this SKColor color) => Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color32 ToColor32(this SKColor color) => Unsafe.As<SKColor, Color32>(ref color);

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <see cref="PColor32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor32 ToPColor32(this SKColor color) => color.ToColor32().ToPColor32();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <see cref="Color64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color64 ToColor64(this SKColor color) => color.ToColor32().ToColor64();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <see cref="PColor64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor64 ToPColor64(this SKColor color) => color.ToColor32().ToPColor64();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <see cref="ColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static ColorF ToColorF(this SKColor color) => color.ToColor32().ToColorF();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <see cref="PColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColorF ToPColorF(this SKColor color) => color.ToColor32().ToPColorF();

        #endregion

        #region ToSKPMColor
        // ReSharper disable InconsistentNaming - Unfortunately SkiaSharp has this unconvenctional naming of the SKPMColor type.
        // ReSharper disable IdentifierTypo - SKPM

        /// <summary>
        /// Converts a <see cref="Color">System.Drawing.Color</see> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this Color color) => color.ToPColor32().ToSKPMColor();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this SKColor color) => SKPMColor.PreMultiply(color);

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then the internal SkiaSharp conversion is used. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default)
            => sourceColorSpace is WorkingColorSpace.Srgb or WorkingColorSpace.Linear
                ? Unsafe.As<SKColorF, ColorF>(ref color).ToPColor32(sourceColorSpace is WorkingColorSpace.Linear).ToSKPMColor()
                : SKPMColor.PreMultiply((SKColor)color);

        /// <summary>
        /// Converts a <see cref="Color32"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this Color32 color) => color.ToPColor32().ToSKPMColor();

        /// <summary>
        /// Converts a <see cref="PColor32"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this PColor32 color)
            // cannot just use ToArgb because the byte order of SKPMColor is platform dependent
            => new SKPMColor((uint)color.A << SKImageInfo.PlatformColorAlphaShift
                | (uint)color.R << SKImageInfo.PlatformColorRedShift
                | (uint)color.G << SKImageInfo.PlatformColorGreenShift
                | (uint)color.B << SKImageInfo.PlatformColorBlueShift);

        /// <summary>
        /// Converts a <see cref="Color64"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this Color64 color) => color.ToPColor32().ToSKPMColor();

        /// <summary>
        /// Converts a <see cref="PColor64"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this PColor64 color) => color.ToPColor32().ToSKPMColor();

        /// <summary>
        /// Converts a <see cref="ColorF"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this ColorF color) => color.ToPColor32().ToSKPMColor();

        /// <summary>
        /// Converts a <see cref="PColorF"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKPMColor ToSKPMColor(this PColorF color) => color.ToPColor32().ToSKPMColor();

        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming
        #endregion

        #region From SKPMColor

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <see cref="Color">System.Drawing.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToColor(this SKPMColor color) => color.ToPColor32().ToColor();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color32 ToColor32(this SKPMColor color) => color.ToPColor32().ToColor32();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <see cref="PColor32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor32 ToPColor32(this SKPMColor color) => new PColor32(color.Alpha, color.Red, color.Green, color.Blue);

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <see cref="Color64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color64 ToColor64(this SKPMColor color) => color.ToPColor32().ToColor64();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <see cref="PColor64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor64 ToPColor64(this SKPMColor color) => color.ToPColor32().ToPColor64();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <see cref="ColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static ColorF ToColorF(this SKPMColor color) => color.ToPColor32().ToColorF();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <see cref="PColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColorF ToPColorF(this SKPMColor color) => color.ToPColor32().ToPColorF();

        #endregion

        #region To SKColorF

        /// <summary>
        /// Converts a <see cref="Color">System.Drawing.Color</see> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then it's the same as the color space of the <see cref="Color">System.Drawing.Color</see> type, which is sRGB. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this Color color, WorkingColorSpace targetColorSpace) => color.ToColor32().ToSKColorF(targetColorSpace);

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolor">SKColor</a> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then the internal SkiaSharp conversion is used. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this SKColor color, WorkingColorSpace targetColorSpace) => targetColorSpace is WorkingColorSpace.Linear or WorkingColorSpace.Srgb
            ? color.ToColor32().ToSKColorF(targetColorSpace)
            : color;

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skpmcolor">SKPMColor</a> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then the internal SkiaSharp conversion is used. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this SKPMColor color, WorkingColorSpace targetColorSpace) => targetColorSpace is WorkingColorSpace.Linear or WorkingColorSpace.Srgb
            ? color.ToColor32().ToSKColorF(targetColorSpace)
            : SKPMColor.UnPreMultiply(color);

        /// <summary>
        /// Converts a <see cref="Color32"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then it's the same as the color space of the <see cref="Color32"/> type, which is sRGB. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this Color32 color, WorkingColorSpace targetColorSpace)
            => Unsafe.As<ColorF, SKColorF>(ref Unsafe.AsRef(color.ToColorF(targetColorSpace == WorkingColorSpace.Linear)));

        /// <summary>
        /// Converts a <see cref="PColor32"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then it's the same as the color space of the <see cref="PColor32"/> type, which is sRGB. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this PColor32 color, WorkingColorSpace targetColorSpace) => color.ToColor32().ToSKColorF(targetColorSpace);

        /// <summary>
        /// Converts a <see cref="Color64"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then it's the same as the color space of the <see cref="Color64"/> type, which is sRGB. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this Color64 color, WorkingColorSpace targetColorSpace)
            => Unsafe.As<ColorF, SKColorF>(ref Unsafe.AsRef(color.ToColorF(targetColorSpace == WorkingColorSpace.Linear)));

        /// <summary>
        /// Converts a <see cref="PColor64"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then it's the same as the color space of the <see cref="PColor64"/> type, which is sRGB. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this PColor64 color, WorkingColorSpace targetColorSpace) => color.ToColor64().ToSKColorF(targetColorSpace);

        /// <summary>
        /// Converts a <see cref="ColorF"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then it's the same as the color space of the <see cref="ColorF"/> type, which is linear. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this ColorF color, WorkingColorSpace targetColorSpace)
        {
            Vector4 result = color.ToRgba();
            if (targetColorSpace == WorkingColorSpace.Srgb)
                result = ColorSpaceHelper.LinearToSrgbVectorRgba(result);
            return Unsafe.As<Vector4, SKColorF>(ref result);
        }

        /// <summary>
        /// Converts a <see cref="PColorF"/> struct to <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="targetColorSpace">The color space of the result.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then it's the same as the color space of the <see cref="PColorF"/> type, which is linear. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static SKColorF ToSKColorF(this PColorF color, WorkingColorSpace targetColorSpace) => color.ToColorF().ToSKColorF(targetColorSpace);

        #endregion

        #region From SKColorF

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <see cref="Color">System.Drawing.Color</see>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then <paramref name="color"/> is interpreted as an sRGB color. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color ToColor(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default) => color.ToColor32(sourceColorSpace).ToColor();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then <paramref name="color"/> is interpreted as an sRGB color. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color32 ToColor32(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default)
            => Unsafe.As<SKColorF, ColorF>(ref color).ToColor32(sourceColorSpace == WorkingColorSpace.Linear);

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <see cref="PColor32"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then <paramref name="color"/> is interpreted as an sRGB color. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor32 ToPColor32(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default)
            => color.ToColor32(sourceColorSpace).ToPColor32();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <see cref="Color64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then <paramref name="color"/> is interpreted as an sRGB color. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static Color64 ToColor64(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default)
            => Unsafe.As<SKColorF, ColorF>(ref color).ToColor64(sourceColorSpace == WorkingColorSpace.Linear);

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <see cref="PColor64"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then <paramref name="color"/> is interpreted as an sRGB color. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColor64 ToPColor64(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default)
            => color.ToColor64(sourceColorSpace).ToPColor64();

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <see cref="ColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then <paramref name="color"/> is interpreted as a linear color. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static ColorF ToColorF(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default)
            => sourceColorSpace != WorkingColorSpace.Srgb
                ? Unsafe.As<SKColorF, ColorF>(ref color)
                : ColorF.FromRgba(ColorSpaceHelper.SrgbToLinearVectorRgba(Unsafe.As<SKColorF, ColorF>(ref color).ToRgba()));

        /// <summary>
        /// Converts an <a href="https://learn.microsoft.com/en-us/dotnet/api/skiasharp.skcolorf">SKColorF</a> struct to <see cref="ColorF"/>.
        /// </summary>
        /// <param name="color">The source color.</param>
        /// <param name="sourceColorSpace">The color space of the source <paramref name="color"/>.
        /// If <see cref="WorkingColorSpace.Default"/> or any undefined value, then <paramref name="color"/> is interpreted as a linear color. This parameter is optional.
        /// <br/>Default value: <see cref="WorkingColorSpace.Default"/>.</param>
        /// <returns>The result of the conversion.</returns>
        public static PColorF ToPColorF(this SKColorF color, WorkingColorSpace sourceColorSpace = WorkingColorSpace.Default)
            => color.ToColorF(sourceColorSpace).ToPColorF();

        #endregion

        // ReSharper restore CommentTypo
        #endregion

        #region Internal Methods

        internal static byte ToLinear(this byte b) => Cache8Bpp.LookupTableSrgbToLinear[b];
        internal static byte ToLinearByte(this ushort b) => ColorSpaceHelper.ToByte(Cache16Bpp.LookupTableSrgbToLinear[b]);
        internal static byte ToLinearByte(this float f) => ColorSpaceHelper.ToByte(ColorSpaceHelper.SrgbToLinear(f));
        internal static byte ToSrgb(this byte b) => Cache8Bpp.LookupTableLinearToSrgb[b];
        internal static byte ToSrgbByte(this ushort b) => ColorSpaceHelper.ToByte(Cache16Bpp.LookupTableLinearToSrgb[b]);

        internal static ushort ToLinear(this ushort b) => Cache16Bpp.LookupTableSrgbToLinear[b];
        internal static ushort ToSrgb(this ushort b) => Cache16Bpp.LookupTableLinearToSrgb[b];
        internal static ushort ToSrgbUInt16(this byte b) => Cache16Bpp.LookupTableLinearToSrgb[ColorSpaceHelper.ToUInt16(b)];
        internal static ColorF ToSrgb(this ColorF c) => ColorF.FromRgba(ColorSpaceHelper.LinearToSrgbVectorRgba(c.ToRgba()));
        internal static ColorF ToLinear(this ColorF c) => ColorF.FromRgba(ColorSpaceHelper.SrgbToLinearVectorRgba(c.ToRgba()));

        #endregion

        #endregion
    }
}
