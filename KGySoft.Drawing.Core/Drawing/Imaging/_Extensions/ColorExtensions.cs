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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
using System.Numerics;
#endif
using System.Runtime.CompilerServices;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains extension methods for various color types representing colors.
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

        #region Properties

#if NET5_0_OR_GREATER
        private static Vector128<byte> PackLowBytesMask => Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#elif NETCOREAPP3_0_OR_GREATER
        private static Vector128<byte> PackLowBytesMask { get; } = Vector128.Create(0, 4, 8, 12, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF);
#endif

        #endregion

        #region Methods

        #region Public Methods

        #region Conversions

        #region Color

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>A <see cref="Color32"/> instance converted from this <see cref="Color"/> instance.</returns>
        public static Color32 ToColor32(this Color color) => new Color32(color);

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="PColor32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>A <see cref="PColor32"/> instance converted from this <see cref="Color"/> instance.</returns>
        public static PColor32 ToPColor32(this Color color) => new PColor32(color);

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>A <see cref="Color64"/> instance converted from this <see cref="Color"/> instance.</returns>
        public static Color64 ToColor64(this Color color) => new Color64(color);

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="PColor64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>A <see cref="PColor64"/> instance converted from this <see cref="Color"/> instance.</returns>
        public static PColor64 ToPColor64(this Color color) => new PColor64(color);

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="Color"/> instance.</returns>
        public static ColorF ToColorF(this Color color) => new ColorF(color);

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="Color"/> instance.</returns>
        public static ColorF ToColorF(this Color color, bool adjustColorSpace) => adjustColorSpace ? new ColorF(color) : ColorF.FromColor32NoColorSpaceChange(color);

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="Color"/> instance.</returns>
        public static PColorF ToPColorF(this Color color) => new PColorF(color);

        /// <summary>
        /// Converts this <see cref="Color"/> to a <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="Color"/> instance.</returns>
        public static PColorF ToPColorF(this Color color, bool adjustColorSpace) => adjustColorSpace ? new PColorF(color) : ColorF.FromColor32NoColorSpaceChange(color).ToPColorF();

        #endregion

        #region Color32

        /// <summary>
        /// Converts this <see cref="Color32"/> to a <see cref="PColor32"/> instance.
        /// It's practically the same as calling the <see cref="ToPremultiplied(Color32)"/> method.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> to convert.</param>
        /// <returns>A <see cref="PColor32"/> instance converted from this <see cref="Color32"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor32 ToPColor32(this Color32 color) => new PColor32(color);

        /// <summary>
        /// Converts this <see cref="Color32"/> to a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> to convert.</param>
        /// <returns>A <see cref="Color64"/> instance converted from this <see cref="Color32"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color64 ToColor64(this Color32 color) => new Color64(color);

        /// <summary>
        /// Converts this <see cref="Color32"/> to a <see cref="PColor64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> to convert.</param>
        /// <returns>A <see cref="PColor64"/> instance converted from this <see cref="Color32"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor64 ToPColor64(this Color32 color) => new PColor64(color);

        /// <summary>
        /// Converts this <see cref="Color32"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> to convert.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="Color32"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF ToColorF(this Color32 color) => new ColorF(color);

        /// <summary>
        /// Converts this <see cref="Color32"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="Color32"/> instance.</returns>
        public static ColorF ToColorF(this Color32 color, bool adjustColorSpace) => adjustColorSpace ? new ColorF(color) : ColorF.FromColor32NoColorSpaceChange(color);

        /// <summary>
        /// Converts this <see cref="Color32"/> to a <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> to convert.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="Color32"/> instance.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF ToPColorF(this Color32 color) => new PColorF(color);

        /// <summary>
        /// Converts this <see cref="Color32"/> to a <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="Color32"/> instance.</returns>
        public static PColorF ToPColorF(this Color32 color, bool adjustColorSpace) => adjustColorSpace ? new PColorF(color) : ColorF.FromColor32NoColorSpaceChange(color).ToPremultiplied();

        #endregion

        #region PColor32

        /// <summary>
        /// Converts this <see cref="PColor32"/> to a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> to convert.</param>
        /// <returns>A <see cref="Color"/> instance converted from this <see cref="PColor32"/> instance.</returns>
        public static Color ToColor(this PColor32 color) => color.ToStraight().ToColor();

        /// <summary>
        /// Converts this <see cref="PColor32"/> to a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> to convert.</param>
        /// <returns>A <see cref="Color64"/> instance converted from this <see cref="PColor32"/> instance.</returns>
        public static Color64 ToColor64(this PColor32 color) => new Color64(color.ToStraight());

        /// <summary>
        /// Converts this <see cref="PColor32"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> to convert.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="PColor32"/> instance.</returns>
        public static ColorF ToColorF(this PColor32 color) => new ColorF(color.ToStraight());

        /// <summary>
        /// Converts this <see cref="PColor32"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="PColor32"/> instance.</returns>
        public static ColorF ToColorF(this PColor32 color, bool adjustColorSpace) => adjustColorSpace ? color.ToColorF() : ColorF.FromColor32NoColorSpaceChange(color.ToStraight());

        /// <summary>
        /// Converts this <see cref="PColor32"/> to a <see cref="PColor64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> to convert.</param>
        /// <returns>A <see cref="PColor64"/> instance converted from this <see cref="PColor32"/> instance.</returns>
        public static PColor64 ToPColor64(this PColor32 color) => new PColor64(color);

        /// <summary>
        /// Converts this <see cref="PColor32"/> to a <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> to convert.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="PColor32"/> instance.</returns>
        public static PColorF ToPColorF(this PColor32 color) => new PColorF(color.ToStraight());

        /// <summary>
        /// Converts this <see cref="PColor32"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="PColor32"/> instance.</returns>
        public static PColorF ToPColorF(this PColor32 color, bool adjustColorSpace) => adjustColorSpace ? color.ToPColorF() : PColorF.FromPColor32NoColorSpaceChange(color);

        #endregion

        #region Color64

        /// <summary>
        /// Converts this <see cref="Color64"/> to a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color64"/> to convert.</param>
        /// <returns>A <see cref="Color"/> instance converted from this <see cref="Color64"/> instance.</returns>
        public static Color ToColor(this Color64 color) => color.ToColor32().ToColor();

        /// <summary>
        /// Converts this <see cref="Color64"/> to a <see cref="PColor32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color64"/> to convert.</param>
        /// <returns>A <see cref="PColor32"/> instance converted from this <see cref="Color64"/> instance.</returns>
        public static PColor32 ToPColor32(this Color64 color) => new PColor32(color.ToColor32());

        /// <summary>
        /// Converts this <see cref="Color64"/> to a <see cref="PColor64"/> instance.
        /// It's practically the same as calling the <see cref="ToPremultiplied(Color64)"/> method.
        /// </summary>
        /// <param name="color">The <see cref="Color64"/> to convert.</param>
        /// <returns>A <see cref="PColor64"/> instance converted from this <see cref="Color64"/> instance.</returns>
        public static PColor64 ToPColor64(this Color64 color) => new PColor64(color);

        /// <summary>
        /// Converts this <see cref="Color64"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color64"/> to convert.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="Color64"/> instance.</returns>
        public static ColorF ToColorF(this Color64 color) => new ColorF(color);

        /// <summary>
        /// Converts this <see cref="Color64"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color64"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="Color64"/> instance.</returns>
        public static ColorF ToColorF(this Color64 color, bool adjustColorSpace) => adjustColorSpace ? new ColorF(color) : ColorF.FromColor64NoColorSpaceChange(color);

        /// <summary>
        /// Converts this <see cref="Color64"/> to a <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color64"/> to convert.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="Color64"/> instance.</returns>
        public static PColorF ToPColorF(this Color64 color) => new ColorF(color).ToPremultiplied();

        /// <summary>
        /// Converts this <see cref="Color64"/> to a <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="Color64"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="Color64"/> instance.</returns>
        public static PColorF ToPColorF(this Color64 color, bool adjustColorSpace) => adjustColorSpace ? new PColorF(color) : ColorF.FromColor64NoColorSpaceChange(color).ToPremultiplied();

        #endregion

        #region PColor64

        /// <summary>
        /// Converts this <see cref="PColor64"/> to a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor64"/> to convert.</param>
        /// <returns>A <see cref="Color"/> instance converted from this <see cref="PColor64"/> instance.</returns>
        public static Color ToColor(this PColor64 color) => color.ToColor32().ToColor();

        /// <summary>
        /// Converts this <see cref="PColor64"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor64"/> to convert.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="PColor64"/> instance.</returns>
        public static ColorF ToColorF(this PColor64 color) => new ColorF(color.ToStraight());

        /// <summary>
        /// Converts this <see cref="PColor64"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor64"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="PColor64"/> instance.</returns>
        public static ColorF ToColorF(this PColor64 color, bool adjustColorSpace) => adjustColorSpace ? color.ToColorF() : ColorF.FromColor64NoColorSpaceChange(color.ToStraight());

        /// <summary>
        /// Converts this <see cref="PColor64"/> to a <see cref="PColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor64"/> to convert.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="PColor64"/> instance.</returns>
        public static PColorF ToPColorF(this PColor64 color) => new PColorF(color.ToStraight());

        /// <summary>
        /// Converts this <see cref="PColor64"/> to a <see cref="ColorF"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColor64"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from sRGB to linear; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="ColorF"/> instance converted from this <see cref="PColor64"/> instance.</returns>
        public static PColorF ToPColorF(this PColor64 color, bool adjustColorSpace) => adjustColorSpace ? color.ToPColorF() : PColorF.FromPColor64NoColorSpaceChange(color);

        #endregion

        #region ColorF

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <returns>A <see cref="Color"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static Color ToColor(this ColorF color) => color.ToColor32().ToColor();

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="Color"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static Color ToColor(this ColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToColor32().ToColor() : color.ToColor32NoColorSpaceChange().ToColor();

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the <see cref="ColorF.ToColor32">ColorF.ToColor32</see> method for a slightly better performance.</param>
        /// <returns>A <see cref="Color32"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static Color32 ToColor32(this ColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToColor32() : color.ToColor32NoColorSpaceChange();

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="PColor32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <returns>A <see cref="PColor32"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static PColor32 ToPColor32(this ColorF color) => color.ToColor32().ToPremultiplied();

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="PColor32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="PColor32"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static PColor32 ToPColor32(this ColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToPColor32() : color.ToColor32NoColorSpaceChange().ToPremultiplied();

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the <see cref="ColorF.ToColor64">ColorF.ToColor64</see> method for a slightly better performance.</param>
        /// <returns>A <see cref="Color64"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static Color64 ToColor64(this ColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToColor64() : color.ToColor64NoColorSpaceChange();

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="PColor64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <returns>A <see cref="PColor64"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static PColor64 ToPColor64(this ColorF color) => color.ToColor64().ToPremultiplied();

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="PColor64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="PColor64"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static PColor64 ToPColor64(this ColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToPColor64() : color.ToColor64NoColorSpaceChange().ToPremultiplied();

        /// <summary>
        /// Converts this <see cref="ColorF"/> to a <see cref="PColorF"/> instance.
        /// It's practically the same as calling the <see cref="ToPremultiplied(ColorF)"/> method.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> to convert.</param>
        /// <returns>A <see cref="PColorF"/> instance converted from this <see cref="ColorF"/> instance.</returns>
        public static PColorF ToPColorF(this ColorF color) => new PColorF(color);

        #endregion

        #region PColorF

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <returns>A <see cref="Color"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static Color ToColor(this PColorF color) => color.ToStraight().ToColor32().ToColor();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="Color"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="Color"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static Color ToColor(this PColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToColor() : color.ToPColor32NoColorSpaceChange().ToStraight().ToColor();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <returns>A <see cref="Color32"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static Color32 ToColor32(this PColorF color) => color.ToStraight().ToColor32();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="Color32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="Color32"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static Color32 ToColor32(this PColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToColor32() : color.ToPColor32NoColorSpaceChange().ToStraight();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="PColor32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <returns>A <see cref="PColor32"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static PColor32 ToPColor32(this PColorF color) => color.ToStraight().ToColor32().ToPremultiplied();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="PColor32"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="PColor32"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static PColor32 ToPColor32(this PColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToPColor32() : color.ToPColor32NoColorSpaceChange();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <returns>A <see cref="Color64"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static Color64 ToColor64(this PColorF color) => color.ToStraight().ToColor64();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="Color64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="Color64"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static Color64 ToColor64(this PColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToColor64() : color.ToPColor64NoColorSpaceChange().ToStraight();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="PColor64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <returns>A <see cref="PColor64"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static PColor64 ToPColor64(this PColorF color) => color.ToStraight().ToColor64().ToPremultiplied();

        /// <summary>
        /// Converts this <see cref="PColorF"/> to a <see cref="PColor64"/> instance.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> to convert.</param>
        /// <param name="adjustColorSpace"><see langword="true"/> to adjust the color space from linear to sRGB; otherwise, <see langword="false"/>.
        /// If you would just pass a <see langword="true"/> constant to this parameter, then use the overload without this parameter for a slightly better performance.</param>
        /// <returns>A <see cref="PColor64"/> instance converted from this <see cref="PColorF"/> instance.</returns>
        public static PColor64 ToPColor64(this PColorF color, bool adjustColorSpace) => adjustColorSpace ? color.ToPColor64() : color.ToPColor64NoColorSpaceChange();

        #endregion

        #region ToPremultiplied

        /// <summary>
        /// Converts this straight <see cref="Color32"/> value to a premultiplied <see cref="PColor32"/> value.
        /// It's practically the same as calling the <see cref="ToPColor32(Color32)"/> method.
        /// </summary>
        /// <param name="color">The <see cref="Color32"/> value to convert.</param>
        /// <returns>A premultiplied <see cref="PColor32"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor32 ToPremultiplied(this Color32 color) => new PColor32(color);

        /// <summary>
        /// Converts this straight <see cref="Color64"/> value to a premultiplied <see cref="PColor64"/> value.
        /// It's practically the same as calling the <see cref="ToPColor64(Color64)"/> method.
        /// </summary>
        /// <param name="color">The <see cref="Color64"/> value to convert.</param>
        /// <returns>A premultiplied <see cref="PColor64"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor64 ToPremultiplied(this Color64 color) => new PColor64(color);

        /// <summary>
        /// Converts this straight <see cref="ColorF"/> value to a premultiplied <see cref="PColorF"/> value.
        /// It's practically the same as calling the <see cref="ToPColorF(ColorF)"/> method.
        /// </summary>
        /// <param name="color">The <see cref="ColorF"/> value to convert.</param>
        /// <returns>A premultiplied <see cref="PColorF"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF ToPremultiplied(this ColorF color) => new PColorF(color);

        #endregion

        #region ToStraight[Safe]
        
        /// <summary>
        /// Converts this premultiplied <see cref="PColor32"/> value to a straight <see cref="Color32"/> value.
        /// It's practically the same as calling the <see cref="PColor32.ToColor32"/> method.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> value to convert.</param>
        /// <returns>A straight <see cref="Color32"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color32 ToStraight(this PColor32 color) => color.ToColor32();

        /// <summary>
        /// Converts this premultiplied <see cref="PColor64"/> value to a straight <see cref="Color64"/> value.
        /// It's practically the same as calling the <see cref="PColor64.ToColor64"/> method.
        /// </summary>
        /// <param name="color">The <see cref="PColor64"/> value to convert.</param>
        /// <returns>A straight <see cref="Color64"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color64 ToStraight(this PColor64 color) => color.ToColor64();

        /// <summary>
        /// Converts this premultiplied <see cref="PColorF"/> value to a straight <see cref="ColorF"/> value.
        /// It's practically the same as calling the <see cref="PColorF.ToColorF"/> method.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> value to convert.</param>
        /// <returns>A straight <see cref="ColorF"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF ToStraight(this PColorF color) => color.ToColorF();

        /// <summary>
        /// Converts this premultiplied <see cref="PColor32"/> instance containing possibly invalid RGB values to a straight <see cref="Color32"/> value.
        /// </summary>
        /// <param name="color">The <see cref="PColor32"/> value to convert.</param>
        /// <returns>A straight <see cref="Color32"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color32 ToStraightSafe(this PColor32 color) => color.Clip().ToColor32();

        /// <summary>
        /// Converts this premultiplied <see cref="PColor64"/> instance containing possibly invalid RGB values to a straight <see cref="Color64"/> value.
        /// </summary>
        /// <param name="color">The <see cref="PColor64"/> value to convert.</param>
        /// <returns>A straight <see cref="Color64"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color64 ToStraightSafe(this PColor64 color) => color.Clip().ToColor64();

        /// <summary>
        /// Converts this premultiplied <see cref="PColorF"/> instance containing possibly invalid ARGB values to a straight <see cref="ColorF"/> value.
        /// </summary>
        /// <param name="color">The <see cref="PColorF"/> value to convert.</param>
        /// <returns>A straight <see cref="ColorF"/> value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF ToStraightSafe(this PColorF color) => color.Clip().ToColorF();

        #endregion

        #endregion

        #region Brightness

        /// <summary>
        /// Gets the brightness of a <see cref="Color32"/> instance as a <see cref="byte">byte</see> based on human perception.
        /// The <see cref="Color32.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color32"/> instance to get the brightness of.</param>
        /// <returns>A <see cref="byte">byte</see> value where 0 represents the darkest and 255 represents the brightest possible value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static byte GetBrightness(this Color32 c)
        {
            if (c.R == c.G && c.R == c.B)
                return c.R;

#if NET5_0_OR_GREATER
            // Actually it would be supported even in .NET Core 3.0 but it's not performant enough below .NET 5.0
            if (Sse2.IsSupported)
            {
                // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as bytes if supported)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                    // Reinterpreting the uint value as bytes and converting them to ints in one step is still faster than converting them separately
                    ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                    // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                    : Vector128.Create(c.B, c.G, c.R, default));

                // Multiplying the RGB components with the sRGB grayscale coefficients and returning the sum
                // NOTE: this is actually the dot product with the grayscale coefficients but Vector128.Dot is slower than the explicit Multiply + Sum
                var result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb, GLumSrgb, RLumSrgb, default));

#if NET7_0_OR_GREATER
                return (byte)Vector128.Sum(result);
#else
                return (byte)(result.GetElement(0) + result.GetElement(1) + result.GetElement(2));
#endif
            }
#endif

            return (byte)(c.R * RLumSrgb + c.G * GLumSrgb + c.B * BLumSrgb);
        }

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
            // Note: using gamma correction even for linear color space because the source is an sRGB color
            ? ColorSpaceHelper.LinearToSrgb8Bit(c.ToColorF().GetBrightness())
            : GetBrightness(c);

        /// <summary>
        /// Gets the brightness of a <see cref="Color64"/> instance as a <see cref="ushort"/> based on human perception.
        /// The <see cref="Color64.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color64"/> instance to get the brightness of.</param>
        /// <returns>A <see cref="ushort"/> value where 0 represents the darkest and 65535 represents the brightest possible value.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ushort GetBrightness(this Color64 c)
        {
            if (c.R == c.G && c.R == c.B)
                return c.R;

#if NET5_0_OR_GREATER
            // Actually it would be supported even in .NET Core 3.0 but it's not performant enough below .NET 5.0
            if (Sse2.IsSupported)
            {
                // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as ushorts if supported)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                    // Reinterpreting the ulong value as ushorts and converting them to ints in one step is still faster than converting them separately
                    ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16())
                    // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                    : Vector128.Create(c.B, c.G, c.R, default));

                // Multiplying the RGB components with the sRGB grayscale coefficients and returning the sum.
                // NOTE: this is actually the dot product with the grayscale coefficients but Vector128.Dot is slower than the explicit Multiply + Sum
                var result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb, GLumSrgb, RLumSrgb, default));

#if NET7_0_OR_GREATER
                return (ushort)Vector128.Sum(result);
#else
                return (ushort)(result.GetElement(0) + result.GetElement(1) + result.GetElement(2));
#endif
            }
#endif

            return (ushort)(c.R * RLumSrgb + c.G * GLumSrgb + c.B * BLumSrgb);
        }

        /// <summary>
        /// Gets the brightness of a <see cref="Color64"/> instance as a <see cref="ushort"/> based on human perception.
        /// The <see cref="Color64.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color64"/> instance to get the brightness of.</param>
        /// <param name="colorSpace">The color space to be used for determining the brightness. If <see cref="WorkingColorSpace.Default"/>, then the sRGB color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the sRGB color space will be used as well.</param>
        /// <returns>A <see cref="ushort"/> value where 0 represents the darkest and 65535 represents the brightest possible value.</returns>
        /// <remarks>
        /// <note>This method always returns a gamma corrected result, even if <paramref name="colorSpace"/> is <see cref="WorkingColorSpace.Linear"/>.
        /// To get the brightness in the linear color space use the <see cref="GetBrightness(ColorF, WorkingColorSpace)"/> method instead.</note>
        /// </remarks>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        [CLSCompliant(false)]
        public static ushort GetBrightness(this Color64 c, WorkingColorSpace colorSpace) => colorSpace == WorkingColorSpace.Linear
            // Note: using gamma correction even for linear color space because the source is an sRGB color
            ? ColorSpaceHelper.LinearToSrgb8Bit(c.ToColorF().GetBrightness())
            : GetBrightness(c);

        /// <summary>
        /// Gets the brightness of a <see cref="ColorF"/> instance as a <see cref="float">float</see> value in the linear color space.
        /// The <see cref="ColorF.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color64"/> instance to get the brightness of.</param>
        /// <returns>A <see cref="float">float</see> value where 0 represents the darkest and 1 represents the brightest possible value.</returns>
        /// <remarks>
        /// <note>The result of this method is not gamma corrected. To get a gamma corrected <see cref="float">float</see> result,
        /// call the <see cref="ColorSpaceHelper.LinearToSrgb">ColorSpaceHelper.LinearToSrgb</see> method on the result,
        /// or use the <see cref="O:KGySoft.Drawing.Imaging.ColorExtensions.GetBrightnessF">GetBrightnessF</see> methods instead.</note>
        /// </remarks>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightness(this ColorF c)
            => c.R.Equals(c.G) && c.R.Equals(c.B)
                ? c.R
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                : Vector3.Dot(c.Rgb, new Vector3(RLumLinear, GLumLinear, BLumLinear));
#else
                : c.R * RLumLinear + c.G * GLumLinear + c.B * BLumLinear;
#endif

        /// <summary>
        /// Gets the brightness of a <see cref="ColorF"/> instance as a <see cref="float">float</see> value in the linear color space.
        /// The <see cref="ColorF.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color64"/> instance to get the brightness of.</param>
        /// <param name="colorSpace">The color space to be used for determining the brightness. If <see cref="WorkingColorSpace.Default"/>, then the linear color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the linear color space will be used as well.</param>
        /// <returns>A <see cref="float">float</see> value where 0 represents the darkest and 1 represents the brightest possible value.</returns>
        /// <remarks>
        /// <note>The result of this method is not gamma corrected. To get a gamma corrected <see cref="float">float</see> result,
        /// call the <see cref="ColorSpaceHelper.LinearToSrgb">ColorSpaceHelper.LinearToSrgb</see> method on the result,
        /// or use the <see cref="O:KGySoft.Drawing.Imaging.ColorExtensions.GetBrightnessF">GetBrightnessF</see> methods instead.</note>
        /// </remarks>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightness(this ColorF c, WorkingColorSpace colorSpace) => colorSpace == WorkingColorSpace.Srgb
            // Note: removing gamma correction even for sRGB color space because the source is a linear color
            ? ColorSpaceHelper.SrgbToLinear(c.ToSrgb().GetBrightnessSrgb())
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
        public static float GetBrightnessF(this Color32 c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as bytes if supported)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                    // Reinterpreting the uint value as bytes and converting them to ints in one step is still faster than converting them separately
                    ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte())
                    // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                    : Vector128.Create(c.B, c.G, c.R, default));

                //// SSE 4.1 dot product is a bit slower for some reason (maybe due to the optional broadcasting feature where we just put the result in the first element only)
                //if (Sse41.IsSupported)
                //    return Sse41.DotProduct(bgrF, Vector128.Create(BLumSrgb / Byte.MaxValue, GLumSrgb / Byte.MaxValue, RLumSrgb / Byte.MaxValue, default), 0b_0111_0001).ToScalar();

                Vector128<float> result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb / Byte.MaxValue, GLumSrgb / Byte.MaxValue, RLumSrgb / Byte.MaxValue, default));
                return result.GetElement(0) + result.GetElement(1) + result.GetElement(2);
            }
#endif

            return c.R * RLumSrgb / Byte.MaxValue + c.G * GLumSrgb / Byte.MaxValue + c.B * BLumSrgb / Byte.MaxValue;
        }

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
        /// Gets the brightness of a <see cref="Color64"/> instance as a <see cref="float">float</see> value based on human perception.
        /// The <see cref="Color64.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color64"/> instance to get the brightness of.</param>
        /// <returns>A <see cref="float">float</see> value where 0 represents the darkest and 1 represents the brightest possible value.</returns>
        /// <remarks>
        /// <note>This method always returns a gamma corrected result.
        /// To get the brightness in the linear color space use the <see cref="GetBrightness(ColorF)"/> method instead.</note>
        /// </remarks>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF(this Color64 c)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Sse2.IsSupported)
            {
                // Converting the [A]RGB values to float (order is BGRA because we reinterpret the original value as ushorts if supported)
                Vector128<float> bgrF = Sse2.ConvertToVector128Single(Sse41.IsSupported
                    // Reinterpreting the ulong value as ushorts and converting them to ints in one step is still faster than converting them separately
                    ? Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16())
                    // Cannot do the conversion in one step. Sparing one conversion because A is actually not needed here.
                    : Vector128.Create(c.B, c.G, c.R, default));

                //// SSE 4.1 dot product is a bit slower for some reason (maybe due to the optional broadcasting feature where we just put the result in the first element only)
                //if (Sse41.IsSupported)
                //    return Sse41.DotProduct(bgrF, Vector128.Create(BLumSrgb / UInt16.MaxValue, GLumSrgb / UInt16.MaxValue, RLumSrgb / UInt16.MaxValue, default), 0b_0111_0001).ToScalar();

                Vector128<float> result = Sse.Multiply(bgrF, Vector128.Create(BLumSrgb / UInt16.MaxValue, GLumSrgb / UInt16.MaxValue, RLumSrgb / UInt16.MaxValue, default));
                return result.GetElement(0) + result.GetElement(1) + result.GetElement(2);
            }
#endif

            return c.R * RLumSrgb / UInt16.MaxValue + c.G * GLumSrgb / UInt16.MaxValue + c.B * BLumSrgb / UInt16.MaxValue;
        }

        /// <summary>
        /// Gets the brightness of a <see cref="Color64"/> instance as a <see cref="float">float</see> value based on human perception.
        /// The <see cref="Color64.A"/> component of the specified value is ignored.
        /// </summary>
        /// <param name="c">The <see cref="Color64"/> instance to get the brightness of.</param>
        /// <param name="colorSpace">The color space to be used for determining the brightness. If <see cref="WorkingColorSpace.Default"/>, then the sRGB color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the sRGB color space will be used as well.</param>
        /// <returns>A <see cref="float">float</see> value where 0 represents the darkest and 1 represents the brightest possible value.</returns>
        /// <remarks>
        /// <note>This method always returns a gamma corrected result, even if <paramref name="colorSpace"/> is <see cref="WorkingColorSpace.Linear"/>.
        /// To get the brightness in the linear color space use the <see cref="GetBrightness(ColorF, WorkingColorSpace)"/> method instead.</note>
        /// </remarks>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static float GetBrightnessF(this Color64 c, WorkingColorSpace colorSpace) => colorSpace == WorkingColorSpace.Linear
            // Note: using gamma correction even for linear color space because we need a result based of human perception
            ? ColorSpaceHelper.LinearToSrgb(c.ToColorF().GetBrightness())
            : GetBrightnessF(c);

        #endregion

        #region Blending
        
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
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the sRGB color space.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="Color64.A"/> is 65535); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color64 Blend(this Color64 foreColor, Color64 backColor)
            => foreColor.A == UInt16.MaxValue ? foreColor
                : backColor.A == UInt16.MaxValue ? foreColor.BlendWithBackgroundSrgb(backColor)
                : foreColor.A == 0 ? backColor
                : backColor.A == 0 ? foreColor
                : foreColor.BlendWithSrgb(backColor);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the specified <paramref name="colorSpace"/>.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="Color64.A"/> is 65535); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <param name="colorSpace">The color space to be used for the blending. If <see cref="WorkingColorSpace.Default"/>, then the sRGB color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the sRGB color space will be used as well.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static Color64 Blend(this Color64 foreColor, Color64 backColor, WorkingColorSpace colorSpace)
            => foreColor.A == UInt16.MaxValue ? foreColor
                : backColor.A == UInt16.MaxValue ? foreColor.BlendWithBackground(backColor, colorSpace)
                : foreColor.A == 0 ? backColor
                : backColor.A == 0 ? foreColor
                : foreColor.BlendWith(backColor, colorSpace);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the linear color space.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="ColorF.A"/> is greater than or equal to 1); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF Blend(this ColorF foreColor, ColorF backColor)
            => foreColor.A >= 1f ? foreColor
                : backColor.A >= 1f ? foreColor.BlendWithBackgroundLinear(backColor)
                : foreColor.A <= 0f ? backColor
                : backColor.A <= 0f ? foreColor
                : foreColor.BlendWithLinear(backColor);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the specified <paramref name="colorSpace"/>.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="ColorF.A"/> is greater than or equal to 1); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <param name="colorSpace">The color space to be used for the blending. If <see cref="WorkingColorSpace.Default"/>, then the linear color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the linear color space will be used as well.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static ColorF Blend(this ColorF foreColor, ColorF backColor, WorkingColorSpace colorSpace)
            => foreColor.A >= 1f ? foreColor
                : backColor.A >= 1f ? foreColor.BlendWithBackground(backColor, colorSpace)
                : foreColor.A <= 0f ? backColor
                : backColor.A <= 0f ? foreColor
                : foreColor.BlendWith(backColor, colorSpace);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the sRGB color space.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="PColor32.A"/> is 255); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor32 Blend(this PColor32 foreColor, PColor32 backColor)
            => foreColor.A == Byte.MaxValue ? foreColor
                : foreColor.A == 0 ? backColor
                : backColor.A == 0 ? foreColor
                : foreColor.BlendWithSrgb(backColor);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the specified <paramref name="colorSpace"/>.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="PColor32.A"/> is 255); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <param name="colorSpace">The color space to be used for the blending. If <see cref="WorkingColorSpace.Default"/>, then the sRGB color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the sRGB color space will be used as well.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor32 Blend(this PColor32 foreColor, PColor32 backColor, WorkingColorSpace colorSpace)
            => foreColor.A == Byte.MaxValue ? foreColor
                : foreColor.A == 0 ? backColor
                : backColor.A == 0 ? foreColor
                : foreColor.BlendWith(backColor, colorSpace);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the sRGB color space.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="PColor64.A"/> is 65535); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor64 Blend(this PColor64 foreColor, PColor64 backColor)
            => foreColor.A == UInt16.MaxValue ? foreColor
                : foreColor.A == 0 ? backColor
                : backColor.A == 0 ? foreColor
                : foreColor.BlendWithSrgb(backColor);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the specified <paramref name="colorSpace"/>.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="Color64.A"/> is 65535); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <param name="colorSpace">The color space to be used for the blending. If <see cref="WorkingColorSpace.Default"/>, then the sRGB color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the sRGB color space will be used as well.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColor64 Blend(this PColor64 foreColor, PColor64 backColor, WorkingColorSpace colorSpace)
            => foreColor.A == UInt16.MaxValue ? foreColor
                : foreColor.A == 0 ? backColor
                : backColor.A == 0 ? foreColor
                : foreColor.BlendWith(backColor, colorSpace);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the linear color space.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="PColorF.A"/> is greater than or equal to 1); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF Blend(this PColorF foreColor, PColorF backColor)
            => foreColor.A >= 1f ? foreColor
                : foreColor.A <= 0f ? backColor
                : backColor.A <= 0f ? foreColor
                : foreColor.BlendWithLinear(backColor);

        /// <summary>
        /// Blends the specified <paramref name="foreColor"/> and <paramref name="backColor"/> in the specified <paramref name="colorSpace"/>.
        /// It returns <paramref name="foreColor"/> if it has no transparency (that is, when <see cref="PColorF.A"/> is greater than or equal to 1); otherwise, the result of the blending.
        /// </summary>
        /// <param name="foreColor">The covering color to blend with <paramref name="backColor"/>.</param>
        /// <param name="backColor">The background color to be covered with <paramref name="foreColor"/>.</param>
        /// <param name="colorSpace">The color space to be used for the blending. If <see cref="WorkingColorSpace.Default"/>, then the linear color space will be used.
        /// For performance reasons this method does not validate this parameter. For undefined values the linear color space will be used as well.</param>
        /// <returns><paramref name="foreColor"/> if it has no transparency; otherwise, the result of the blending.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static PColorF Blend(this PColorF foreColor, PColorF backColor, WorkingColorSpace colorSpace)
            => foreColor.A >= 1f ? foreColor
                : foreColor.A <= 0f ? backColor
                : backColor.A <= 0f ? foreColor
                : foreColor.BlendWith(backColor, colorSpace);

        #endregion

        #region Tolerant Equality

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
        /// Gets whether two <see cref="Color64"/> instances are equal using a specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="c1">The first color to compare.</param>
        /// <param name="c2">The second color to compare.</param>
        /// <param name="tolerance">The allowed tolerance for ARGB components.</param>
        /// <param name="alphaThreshold">Specifies a threshold under which colors are considered transparent. If both colors have lower <see cref="Color64.A"/> value than the threshold, then they are considered equal.
        /// If only one of the specified colors has lower <see cref="Color64.A"/> value than the threshold, then the colors are considered different.
        /// If both colors' <see cref="Color64.A"/> value are equal to or greater than this value, then <paramref name="tolerance"/> is applied to the <see cref="Color64.A"/> value, too. This parameter is optional.
        /// <br/>Default value: 0.</param>
        /// <returns><see langword="true"/>, if the colors are considered equal with the specified <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        [CLSCompliant(false)]
        public static bool TolerantEquals(this Color64 c1, Color64 c2, ushort tolerance, ushort alphaThreshold = 0)
        {
            if (c1 == c2 || c1.A < alphaThreshold && c2.A < alphaThreshold)
                return true;
            if ((c1.A < alphaThreshold) ^ (c2.A < alphaThreshold))
                return false;
            return Math.Abs(c1.R - c2.R) <= tolerance && Math.Abs(c1.G - c2.G) <= tolerance && Math.Abs(c1.B - c2.B) <= tolerance && Math.Abs(c1.A - c2.A) <= tolerance;
        }

        /// <summary>
        /// Gets whether two <see cref="ColorF"/> instances are equal using a specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="c1">The first color to compare.</param>
        /// <param name="c2">The second color to compare.</param>
        /// <param name="tolerance">The allowed tolerance for ARGB components. For performance reasons this parameter is not validated. This parameter is optional.
        /// <br/>Default value: <c>0.000001</c> (10<sup>-6</sup>).</param>
        /// <param name="alphaThreshold">Specifies a threshold under which colors are considered transparent. If both colors have lower <see cref="ColorF.A"/> value than the threshold, then they are considered equal.
        /// If only one of the specified colors has lower <see cref="ColorF.A"/> value than the threshold, then the colors are considered different.
        /// If both colors' <see cref="ColorF.A"/> value are equal to or greater than this value, then <paramref name="tolerance"/> is applied to the <see cref="ColorF.A"/> value, too.
        /// For performance reasons this parameter is not validated. This parameter is optional.
        /// <br/>Default value: 0.</param>
        /// <returns><see langword="true"/>, if the colors are considered equal with the specified <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool TolerantEquals(this ColorF c1, ColorF c2, float tolerance = 1e-6f, float alphaThreshold = 0f)
        {
            if (c1 == c2 || c1.A < alphaThreshold && c2.A < alphaThreshold)
                return true;
            if ((c1.A < alphaThreshold) ^ (c2.A < alphaThreshold))
                return false;
            return c1.R.TolerantEquals(c2.R, tolerance)
                && c1.G.TolerantEquals(c2.G, tolerance)
                && c1.B.TolerantEquals(c2.B, tolerance)
                && c1.A.TolerantEquals(c2.A, tolerance);
        }

        /// <summary>
        /// Gets whether two <see cref="PColor32"/> instances are equal using a specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="c1">The first color to compare.</param>
        /// <param name="c2">The second color to compare.</param>
        /// <param name="tolerance">The allowed tolerance for ARGB components.</param>
        /// <param name="alphaThreshold">Specifies a threshold under which colors are considered transparent. If both colors have lower <see cref="PColor32.A"/> value than the threshold, then they are considered equal.
        /// If only one of the specified colors has lower <see cref="PColor32.A"/> value than the threshold, then the colors are considered different.
        /// If both colors' <see cref="PColor32.A"/> value are equal to or greater than this value, then <paramref name="tolerance"/> is applied to the <see cref="PColor32.A"/> value, too. This parameter is optional.
        /// <br/>Default value: 0.</param>
        /// <returns><see langword="true"/>, if the colors are considered equal with the specified <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter", Justification = "False alarm, the 'hiding' method is internal so 3rd party consumers call always this method.")]
        public static bool TolerantEquals(this PColor32 c1, PColor32 c2, byte tolerance, byte alphaThreshold = 0)
        {
            if (c1 == c2 || c1.A < alphaThreshold && c2.A < alphaThreshold)
                return true;
            if ((c1.A < alphaThreshold) ^ (c2.A < alphaThreshold))
                return false;
            return Math.Abs(c1.R - c2.R) <= tolerance && Math.Abs(c1.G - c2.G) <= tolerance && Math.Abs(c1.B - c2.B) <= tolerance && Math.Abs(c1.A - c2.A) <= tolerance;
        }

        /// <summary>
        /// Gets whether two <see cref="PColor64"/> instances are equal using a specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="c1">The first color to compare.</param>
        /// <param name="c2">The second color to compare.</param>
        /// <param name="tolerance">The allowed tolerance for ARGB components.</param>
        /// <param name="alphaThreshold">Specifies a threshold under which colors are considered transparent. If both colors have lower <see cref="PColor64.A"/> value than the threshold, then they are considered equal.
        /// If only one of the specified colors has lower <see cref="PColor64.A"/> value than the threshold, then the colors are considered different.
        /// If both colors' <see cref="PColor64.A"/> value are equal to or greater than this value, then <paramref name="tolerance"/> is applied to the <see cref="PColor64.A"/> value, too. This parameter is optional.
        /// <br/>Default value: 0.</param>
        /// <returns><see langword="true"/>, if the colors are considered equal with the specified <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        [CLSCompliant(false)]
        public static bool TolerantEquals(this PColor64 c1, PColor64 c2, ushort tolerance, ushort alphaThreshold = 0)
        {
            if (c1 == c2 || c1.A < alphaThreshold && c2.A < alphaThreshold)
                return true;
            if ((c1.A < alphaThreshold) ^ (c2.A < alphaThreshold))
                return false;
            return Math.Abs(c1.R - c2.R) <= tolerance && Math.Abs(c1.G - c2.G) <= tolerance && Math.Abs(c1.B - c2.B) <= tolerance && Math.Abs(c1.A - c2.A) <= tolerance;
        }

        /// <summary>
        /// Gets whether two <see cref="PColorF"/> instances are equal using a specified <paramref name="tolerance"/>.
        /// </summary>
        /// <param name="c1">The first color to compare.</param>
        /// <param name="c2">The second color to compare.</param>
        /// <param name="tolerance">The allowed tolerance for ARGB components. For performance reasons this parameter is not validated. This parameter is optional.
        /// <br/>Default value: <c>0.000001</c> (10<sup>-6</sup>).</param>
        /// <param name="alphaThreshold">Specifies a threshold under which colors are considered transparent. If both colors have lower <see cref="PColorF.A"/> value than the threshold, then they are considered equal.
        /// If only one of the specified colors has lower <see cref="PColorF.A"/> value than the threshold, then the colors are considered different.
        /// If both colors' <see cref="PColorF.A"/> value are equal to or greater than this value, then <paramref name="tolerance"/> is applied to the <see cref="PColorF.A"/> value, too.
        /// For performance reasons this parameter is not validated. This parameter is optional.
        /// <br/>Default value: 0.</param>
        /// <returns><see langword="true"/>, if the colors are considered equal with the specified <paramref name="tolerance"/>; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public static bool TolerantEquals(this PColorF c1, PColorF c2, float tolerance = 1e-6f, float alphaThreshold = 0f)
        {
            if (c1 == c2 || c1.A < alphaThreshold && c2.A < alphaThreshold)
                return true;
            if ((c1.A < alphaThreshold) ^ (c2.A < alphaThreshold))
                return false;
            return c1.R.TolerantEquals(c2.R, tolerance)
                && c1.G.TolerantEquals(c2.G, tolerance)
                && c1.B.TolerantEquals(c2.B, tolerance)
                && c1.A.TolerantEquals(c2.A, tolerance);
        }

        #endregion

        #endregion

        #region Internal Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackground(this Color32 c, Color32 backColor, bool linear)
            => linear ? c.BlendWithBackgroundLinear(backColor) : c.BlendWithBackgroundSrgb(backColor);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackground(this Color32 c, Color32 backColor, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Linear ? c.BlendWithBackgroundLinear(backColor) : c.BlendWithBackgroundSrgb(backColor);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithBackground(this Color64 c, Color64 backColor, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Linear ? c.BlendWithBackgroundLinear(backColor) : c.BlendWithBackgroundSrgb(backColor);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithBackground(this Color64 c, Color64 backColor, bool linear)
            => linear ? c.BlendWithBackgroundLinear(backColor) : c.BlendWithBackgroundSrgb(backColor);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithBackground(this ColorF c, ColorF backColor, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Srgb ? c.BlendWithBackgroundSrgb(backColor) : c.BlendWithBackgroundLinear(backColor);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithBackground(this ColorF c, ColorF backColor, bool linear)
            => linear ? c.BlendWithBackgroundLinear(backColor) : c.BlendWithBackgroundSrgb(backColor);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithBackgroundSrgb(this Color32 c, Color32 backColor)
        {
            Debug.Assert(c.A != Byte.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == Byte.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            int inverseAlpha = Byte.MaxValue - c.A;

#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization if possible. Without division everything can be performed on integers.
            // And the bit-shifting version at least always produces the exact same results regardless of acceleration.
            if (Sse41.IsSupported)
            {
                // Order is BGRA because we reinterpret the original value as bytes before converting to int
                // bgraI32 = (int)c.RGB * (int)c.A
                Vector128<int> bgraI32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsByte()),
                    Sse41.ConvertToVector128Int32(Vector128.Create(c.A)));

                // resultI32 = (int)backColor.RGB * inverseAlpha
                Vector128<int> resultI32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(backColor.Value).AsByte()),
                    Vector128.Create(inverseAlpha));

                // resultI32 = (bgraI32 + resultI32) >> 8 | a:0xFF
                resultI32 = Sse2.ShiftRightLogical(Sse2.Add(bgraI32, resultI32), 8).WithElement(3, Byte.MaxValue);

                // Initializing directly from uint by packing the bytes of resultI32
                return new Color32(Ssse3.Shuffle(resultI32.AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            // The non-accelerated version. Division, eg. r:(byte)((c.R * c.A + backColor.R * inverseAlpha) / 255) would be more accurate
            // but unlike for premultiplication we use the faster bit shifting because there is no inverse operation for blending.
            return new Color32(Byte.MaxValue,
                (byte)((c.R * c.A + backColor.R * inverseAlpha) >> 8),
                (byte)((c.G * c.A + backColor.G * inverseAlpha) >> 8),
                (byte)((c.B * c.A + backColor.B * inverseAlpha) >> 8));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithBackgroundSrgb(this Color64 c, Color64 backColor)
        {
            Debug.Assert(c.A != UInt16.MaxValue, "Partially transparent fore color is expected. Call Blend for better performance.");
            Debug.Assert(backColor.A == UInt16.MaxValue, "Totally opaque back color is expected.");

            // The blending is applied only to the color and not the resulting alpha, which will always be opaque
            if (c.A == 0)
                return backColor;
            uint inverseAlpha = (uint)UInt16.MaxValue - c.A;

#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization if possible. Without division everything can be performed on integers.
            // And the bit-shifting version at least always produces the exact same results regardless of acceleration.
            if (Sse41.IsSupported)
            {
                // Order is BGRA because we reinterpret the original value as ushorts before converting to int
                // bgraU32 = (uint)c.RGB * (uint)c.A
                Vector128<uint> bgraU32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(c.Value).AsUInt16()).AsUInt32(),
                    Sse41.ConvertToVector128Int32(Vector128.Create(c.A)).AsUInt32());

                // resultU32 = (uint)backColor.RGB * inverseAlpha
                Vector128<uint> resultU32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(backColor.Value).AsUInt16()).AsUInt32(),
                    Vector128.Create(inverseAlpha));

                // resultU32 = (bgraU32 + result) >> 16 | a:0xFFFF;
                resultU32 = Sse2.ShiftRightLogical(Sse2.Add(bgraU32, resultU32), 16).WithElement(3, UInt16.MaxValue);

                // Initializing directly from ulong by packing the words of resultU32
                return new Color64(Sse41.PackUnsignedSaturate(resultU32.AsInt32(), resultU32.AsInt32()).AsUInt64().ToScalar());
            }
#endif

            // The non-accelerated version. Division, eg. r:(ushort)(((uint)c.R * c.A + backColor.R * inverseAlpha) / 65535) would be more accurate
            // but unlike for premultiplication we use the faster bit shifting because there is no inverse operation for blending.
            return new Color64(UInt16.MaxValue,
                (ushort)(((uint)c.R * c.A + backColor.R * inverseAlpha) >> 16),
                (ushort)(((uint)c.G * c.A + backColor.G * inverseAlpha) >> 16),
                (ushort)(((uint)c.B * c.A + backColor.B * inverseAlpha) >> 16));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithBackgroundSrgb(this ColorF c, ColorF backColor)
            => c.A <= 0f ? backColor : c.ToSrgb().BlendWithBackgroundLinear(backColor).ToLinear();

        internal static Color32 BlendWithBackgroundLinear(this Color32 c, Color32 backColor)
            => c.A == 0 ? backColor : c.ToColorF().BlendWithBackgroundLinear(backColor.ToColorF()).ToColor32();

        internal static Color64 BlendWithBackgroundLinear(this Color64 c, Color64 backColor)
            => c.A == 0 ? backColor : c.ToColorF().BlendWithBackgroundLinear(backColor.ToColorF()).ToColor64();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithBackgroundLinear(this ColorF c, ColorF backColor)
        {
            if (c.A <= 0)
                return backColor;

#if NETCOREAPP3_0_OR_GREATER
            // Using native vectorization if possible.
            if (Sse.IsSupported)
            {
                // rgbaResultF = c.RGBA * c.A
                Vector128<float> rgbaResultF = Sse.Multiply(c.RgbaV128, Vector128.Create(c.A));

                // rgbaResultF += backColor.RGBA * (1f - c.A)
                rgbaResultF = Sse.Add(rgbaResultF, Sse.Multiply(backColor.RgbaV128, Vector128.Create(1f - c.A)));

                return new ColorF(rgbaResultF.WithElement(3, 1f));
            }
#endif

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            // The possibly still accelerated auto vectorization
            return new ColorF(new Vector4(c.Rgb * c.A + backColor.Rgb * (1f - c.A), 1f));
#else
            // The non-accelerated version
            float inverseAlpha = 1f - c.A;
            return new ColorF(1f,
                c.R * c.A + backColor.R * inverseAlpha,
                c.G * c.A + backColor.G * inverseAlpha,
                c.B * c.A + backColor.B * inverseAlpha);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWith(this Color32 src, Color32 dst, bool linear)
            => linear ? src.BlendWithLinear(dst) : src.BlendWithSrgb(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWith(this Color32 src, Color32 dst, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Linear ? src.BlendWithLinear(dst) : src.BlendWithSrgb(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWith(this Color64 src, Color64 dst, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Linear ? src.BlendWithLinear(dst) : src.BlendWithSrgb(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWith(this ColorF src, ColorF dst, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Srgb ? src.BlendWithSrgb(dst) : src.BlendWithLinear(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 BlendWith(this PColor32 src, PColor32 dst, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Linear ? src.BlendWithLinear(dst) : src.BlendWithSrgb(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 BlendWith(this PColor64 src, PColor64 dst, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Linear ? src.BlendWithLinear(dst) : src.BlendWithSrgb(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 BlendWith(this PColor64 src, PColor64 dst, bool isLinear)
            => isLinear ? src.BlendWithLinear(dst) : src.BlendWithSrgb(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColorF BlendWith(this PColorF src, PColorF dst, WorkingColorSpace colorSpace)
            => colorSpace == WorkingColorSpace.Srgb ? src.BlendWithSrgb(dst) : src.BlendWithLinear(dst);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithSrgb(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");
            float inverseAlphaSrc;
            float alphaOut;

#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization if possible. We cannot spare the floating point operations due to the division by output alpha.
            if (Sse41.IsSupported)
            {
                // Doing the byte -> int conversion by SSE 4.1 is faster for some reason even if there is only one conversion.
                // In fact, it's so much faster that the SSE2 compatible solution would be even slower than the fallback version, especially with the workaround for https://github.com/dotnet/runtime/issues/83387
                // srcAF = (float)src.A / 255f
                // dstAF = (float)dst.A / 255f
                Vector128<float> srcAF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(src.A))), Vector128.Create(1f / 255f));
                Vector128<float> dstAF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(dst.A))), Vector128.Create(1f / 255f));
                inverseAlphaSrc = 1f - srcAF.ToScalar();
                alphaOut = srcAF.ToScalar() + dstAF.ToScalar() * inverseAlphaSrc;

                // Order is BGR[A] because we reinterpret the original value as bytes before converting to float
                // srcBgrxF = (float)src.RGB * srcAF
                Vector128<float> srcBgrxF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(src.Value).AsByte())), srcAF);

                // dstBgrxF = (float)dst.RGB * dstAF * (1f - srcAF)
                Vector128<float> dstBgrxF = Sse.Multiply(Sse.Multiply(
                    Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(dst.Value).AsByte())), dstAF), Vector128.Create(inverseAlphaSrc));

                // bgraI32 = {rgb: (srcBgrxF + dstBgrxF) / alphaOut, a: (alphaOut * 255)} as int
                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(
                    Sse.Divide(Sse.Add(srcBgrxF, dstBgrxF), Vector128.Create(alphaOut)).WithElement(3, alphaOut * Byte.MaxValue));

                // Initializing directly from uint by packing the bytes of bgraI32
                return new Color32(Ssse3.Shuffle(bgraI32.AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            // The non-accelerated version. Bit-shifting, eg. r:(byte)((src.R * src.A + ((dst.R * dst.A * inverseAlphaSrc) >> 8)) / alphaOut)
            // would not be faster because it still contains a division
            float alphaSrc = src.A / 255f;
            float alphaDst = dst.A / 255f;
            inverseAlphaSrc = 1f - alphaSrc;
            alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;

            return new Color32((byte)(alphaOut * Byte.MaxValue),
                (byte)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (byte)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) * alphaOutRecip));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithSrgb(this Color64 src, Color64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0 && dst.A != 65535, "Partially transparent colors are expected");
            float inverseAlphaSrc;
            float alphaOut;

#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization if possible. We cannot spare the floating point operations due to the division by output alpha.
            if (Sse41.IsSupported)
            {
                // Doing the ushort -> int conversion by SSE 4.1 is faster for some reason even if there is only one conversion.
                // In fact, it's so much faster that the SSE2 compatible solution would be even slower than the fallback version, especially with the workaround for https://github.com/dotnet/runtime/issues/83387
                // srcAF = (float)src.A / 65535f
                // dstAF = (float)dst.A / 65535f
                Vector128<float> srcAF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(src.A))), Vector128.Create(1f / 65535f));
                Vector128<float> dstAF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.Create(dst.A))), Vector128.Create(1f / 65535f));
                inverseAlphaSrc = 1f - srcAF.ToScalar();
                alphaOut = srcAF.ToScalar() + dstAF.ToScalar() * inverseAlphaSrc;

                // Order is BGR[A] because we reinterpret the original value as ushorts before converting to float
                // srcBgrxF = (float)src.RGB * srcAF
                Vector128<float> srcBgrxF = Sse.Multiply(Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(src.Value).AsUInt16())), srcAF);

                // dstBgrxF = (float)dst.RGB * dstAF * (1f - srcAF)
                Vector128<float> dstBgrxF = Sse.Multiply(Sse.Multiply(
                    Sse2.ConvertToVector128Single(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(dst.Value).AsUInt16())), dstAF), Vector128.Create(inverseAlphaSrc));

                // bgraI32 = {rgb: (srcBgrxF + dstBgrxF) / alphaOut, a: (alphaOut * 65535)} as int
                Vector128<int> bgraI32 = Sse2.ConvertToVector128Int32(
                    Sse.Divide(Sse.Add(srcBgrxF, dstBgrxF), Vector128.Create(alphaOut)).WithElement(3, alphaOut * UInt16.MaxValue));

                // Initializing directly from ulong by packing the words of bgraI32
                return new Color64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).AsUInt64().ToScalar());
            }
#endif

            // The non-accelerated version. Bit-shifting, eg. r:(ushort)(((ulong)src.R * src.A + (((ulong)dst.R * dst.A * inverseAlphaSrc) >> 16)) / alphaOut)
            // would not be faster because it still contains a division
            float alphaSrc = src.A / 65535f;
            float alphaDst = dst.A / 65535f;
            inverseAlphaSrc = 1f - alphaSrc;
            alphaOut = alphaSrc + alphaDst * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;

            return new Color64((ushort)(alphaOut * UInt16.MaxValue),
                (ushort)((src.R * alphaSrc + dst.R * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (ushort)((src.G * alphaSrc + dst.G * alphaDst * inverseAlphaSrc) * alphaOutRecip),
                (ushort)((src.B * alphaSrc + dst.B * alphaDst * inverseAlphaSrc) * alphaOutRecip));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithSrgb(this ColorF src, ColorF dst)
        {
            Debug.Assert(src.A is > 0f and < 1f && dst.A is > 0f and < 1f, "Partially transparent colors are expected");
            return src.ToSrgb().BlendWithLinear(dst).ToLinear();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 BlendWithSrgb(this PColor32 src, PColor32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0, "Partially transparent colors are expected");
            int inverseAlphaSrc = Byte.MaxValue - src.A;

#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization if possible.
            if (Sse41.IsSupported)
            {
                // Order is BGRA because we reinterpret the original value as bytes before converting to int
                // bgraI32 = (int)dst.ARGB * inverseAlphaSrc
                Vector128<int> bgraI32 = Sse41.MultiplyLow(
                    Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(dst.Value).AsByte()), Vector128.Create(inverseAlphaSrc));

                // bgraI32 >>= 8
                bgraI32 = Sse2.ShiftRightLogical(bgraI32, 8);

                // bgraI32 += (int)src.ARGB
                bgraI32 = Sse2.Add(bgraI32, Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(src.Value).AsByte()));

                // Due to the bit shifting (division by 256 instead of 255) we must adjust the alpha because it could be 254 even if dst is completely opaque
                if (dst.A == Byte.MaxValue)
                    bgraI32 = bgraI32.AsByte().WithElement(12, Byte.MaxValue).AsInt32();

                // Initializing directly from uint by packing the bytes of bgraI32
                return new PColor32(Ssse3.Shuffle(bgraI32.AsByte(), PackLowBytesMask).AsUInt32().ToScalar());
            }
#endif

            // The non-accelerated version. Division, eg. r:(byte)(src.R + (dst.R * inverseAlphaSrc) / 255) would be more accurate
            // but unlike for premultiplication we use the faster bit shifting because there is no inverse operation for blending.
            return new PColor32(dst.A == Byte.MaxValue ? Byte.MaxValue : (byte)(src.A + ((dst.A * inverseAlphaSrc) >> 8)),
                (byte)(src.R + ((dst.R * inverseAlphaSrc) >> 8)),
                (byte)(src.G + ((dst.G * inverseAlphaSrc) >> 8)),
                (byte)(src.B + ((dst.B * inverseAlphaSrc) >> 8)));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 BlendWithSrgb(this PColor64 src, PColor64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0, "Partially transparent colors are expected");
            int inverseAlphaSrc = UInt16.MaxValue - src.A;

#if NETCOREAPP3_0_OR_GREATER
            // Using vectorization if possible.
            if (Sse41.IsSupported)
            {
                // Order is BGRA because we reinterpret the original value as ushorts before converting to int
                // bgraI32 = (int)dst.ARGB * inverseAlphaSrc
                Vector128<int> bgraI32 = Sse41.MultiplyLow(Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(dst.Value).AsUInt16()), Vector128.Create(inverseAlphaSrc));

                // bgraI32 >>= 16
                bgraI32 = Sse2.ShiftRightLogical(bgraI32, 16);

                // bgraI32 += (int)src.ARGB
                bgraI32 = Sse2.Add(bgraI32, Sse41.ConvertToVector128Int32(Vector128.CreateScalarUnsafe(src.Value).AsUInt16()));

                // Due to the bit shifting (division by 65536 instead of 65535) we must adjust the alpha because it could be 65534 even if dst is completely opaque
                if (dst.A == UInt16.MaxValue)
                    bgraI32 = bgraI32.AsUInt16().WithElement(6, UInt16.MaxValue).AsInt32();

                // Initializing directly from ulong by packing the bytes of bgraI32
                return new PColor64(Sse41.PackUnsignedSaturate(bgraI32, bgraI32).AsUInt64().ToScalar());
            }
#endif

            // The non-accelerated version. Division, eg. r:(ushort)(src.R + dst.R * inverseAlphaSrc / 65535) would be more accurate
            // but unlike for premultiplication we use the faster bit shifting because there is no inverse operation for blending.
            return new PColor64(dst.A == UInt16.MaxValue ? UInt16.MaxValue : (ushort)(src.A + ((dst.A * inverseAlphaSrc) >> 16)),
                (ushort)(src.R + ((dst.R * inverseAlphaSrc) >> 16)),
                (ushort)(src.G + ((dst.G * inverseAlphaSrc) >> 16)),
                (ushort)(src.B + ((dst.B * inverseAlphaSrc) >> 16)));
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColorF BlendWithSrgb(this PColorF src, PColorF dst)
        {
            Debug.Assert(src.A is > 0f and < 1f && dst.A is > 0f and < 1f, "Partially transparent colors are expected");
            return src.ToStraight().ToSrgb().BlendWithLinear(dst.ToStraight().ToSrgb()).ToLinear().ToPremultiplied();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color32 BlendWithLinear(this Color32 src, Color32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");
            return src.ToColorF().BlendWithLinear(dst.ToColorF()).ToColor32();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static Color64 BlendWithLinear(this Color64 src, Color64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0 && dst.A != 65535, "Partially transparent colors are expected");
            return src.ToColorF().BlendWithLinear(dst.ToColorF()).ToColor64();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static ColorF BlendWithLinear(this ColorF src, ColorF dst)
        {
            float inverseAlphaSrc = 1f - src.A;
            float alphaOut = src.A + dst.A * inverseAlphaSrc;

#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
#if NETCOREAPP3_0_OR_GREATER
            // Using native vectorization if possible.
            if (Sse.IsSupported)
            {
                // rgbaResultF = src.RGBA * src.A
                Vector128<float> rgbaResultF = Sse.Multiply(src.RgbaV128, Vector128.Create(src.A));

                // rgbaResultF += dst.RGBA * dst.A * inverseAlphaSrc
                rgbaResultF = Sse.Add(rgbaResultF, Sse.Multiply(dst.RgbaV128, Vector128.Create(dst.A * inverseAlphaSrc)));

                // (rgbaResultF /=  alphaOut) with A:alphaOut
                return new ColorF(Sse.Divide(rgbaResultF, Vector128.Create(alphaOut)).WithElement(3, alphaOut));
            }
#endif
            // The possibly still accelerated auto vectorization
            return new ColorF(new Vector4((src.Rgb * src.A + dst.Rgb * (dst.A * inverseAlphaSrc)) / alphaOut, alphaOut));
#else
            // The non-accelerated version
            float alphaDst = dst.A * inverseAlphaSrc;
            float alphaOutRecip = 1f / alphaOut;
            return new ColorF(alphaOut,
                (src.R * src.A + dst.R * alphaDst) * alphaOutRecip,
                (src.G * src.A + dst.G * alphaDst) * alphaOutRecip,
                (src.B * src.A + dst.B * alphaDst) * alphaOutRecip);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor32 BlendWithLinear(this PColor32 src, PColor32 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 255 && dst.A != 0 && dst.A != 255, "Partially transparent colors are expected");
            return src.ToColorF().BlendWithLinear(dst.ToColorF()).ToPColor32();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColor64 BlendWithLinear(this PColor64 src, PColor64 dst)
        {
            Debug.Assert(src.A != 0 && src.A != 65535 && dst.A != 0 && dst.A != 65535, "Partially transparent colors are expected");
            return src.ToColorF().BlendWithLinear(dst.ToColorF()).ToPColor64();
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static PColorF BlendWithLinear(this PColorF src, PColorF dst)
        {
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            // The possibly accelerated auto vectorization.
            // Note: not using native vectorization here because according to the performance tests
            //       this can be perfectly optimized by auto vectorization
            return new PColorF(src.Rgba + dst.Rgba * (1f - src.A));
#else
            // The non-accelerated version
            float inverseAlphaSrc = 1f - src.A;
            return new PColorF(dst.A >= 1f ? 1f : src.A + dst.A * inverseAlphaSrc,
                src.R + dst.R * inverseAlphaSrc,
                src.G + dst.G * inverseAlphaSrc,
                src.B + dst.B * inverseAlphaSrc);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static bool TolerantEquals(this Color32 c1, Color32 c2, byte tolerance)
        {
            Debug.Assert(c1.A == 255 && c2.A == 255, "If alpha can be nonzero use the public overload instead");
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

        #region Private Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private static float GetBrightnessSrgb(this ColorF c)
            => c.R.Equals(c.G) && c.R.Equals(c.B)
                ? c.R
#if NETCOREAPP || NET46_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                : Vector3.Dot(c.Rgb, new Vector3(RLumSrgb, GLumSrgb, BLumSrgb));
#else
                : c.R * RLumSrgb + c.G * GLumSrgb + c.B * BLumSrgb;
#endif

        #endregion

        #endregion
    }
}
