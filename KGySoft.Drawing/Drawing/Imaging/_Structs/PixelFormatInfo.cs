#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatInfo.cs
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a pixel format in a technology-agnostic way. Can be used to specify custom pixel formats
    /// for the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">CreateBitmapData</see> method overloads
    /// with a <see cref="PixelFormatInfo"/> parameter.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerValue) + "}")]
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags", Justification = "PixelFormat consists of flags even if not marked by [Flags] - TODO: remove in next version with self PixelFormat")]
    public struct PixelFormatInfo
    {
        #region Constants

        private const int isCustom = 1 << 24;
        private const int isGrayscale = 1 << 25;
        private const int hasSingleBitAlpha = 1 << 26;

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the bits per pixel value of this <see cref="PixelFormatInfo"/>.
        /// Supported range is 1..128 (or 1..16 for <see cref="Indexed"/> formats). Typical values are powers of two but any value is supported.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When setting this property, <paramref name="value"/> must be between 1 and 128.</exception>
        public byte BitsPerPixel
        {
            get => (byte)PixelFormat.ToBitsPerPixel();
            set
            {
                if (value == BitsPerPixel)
                    return;
                if (value is 0 or > 128)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeBetween(1, 128));
                PixelFormat = (PixelFormat)(isCustom | (value & ~0xFF00) | (value << 8));
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format has an alpha channel (transparency).
        /// For <see cref="Indexed"/> formats this property can be <see langword="false"/> because alpha support is determined by the current palette.
        /// </summary>
        public bool HasAlpha
        {
            get => PixelFormat.HasAlpha();
            set
            {
                if (value == HasAlpha)
                    return;
                if (value)
                    PixelFormat |= PixelFormat.Alpha;
                else
                    PixelFormat &= ~PixelFormat.Alpha;

                PixelFormat |= (PixelFormat)isCustom;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format is an indexed one.
        /// An indexed format is not expected to have more than 16 <see cref="BitsPerPixel"/> (up to 65536 color entries).
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, then pixel data represent <see cref="Palette"/> entries instead of direct colors.
        /// <br/>If <see langword="false"/>, then pixel data represent specific colors.
        /// </value>
        public bool Indexed
        {
            get => PixelFormat.IsIndexed();
            set
            {
                if (value == Indexed)
                    return;
                if (value)
                    PixelFormat |= PixelFormat.Indexed;
                else
                    PixelFormat &= ~PixelFormat.Indexed;

                PixelFormat |= (PixelFormat)isCustom;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format is a grayscale one.
        /// For <see cref="Indexed"/> formats this property can be <see langword="false"/> because grayscale nature is determined by the current palette.
        /// Setting this property for non-indexed custom grayscale formats helps to auto select the preferable strategy for some operations such as dithering.
        /// </summary>
        public bool Grayscale
        {
            get => ((int)PixelFormat & isGrayscale) != 0;
            set
            {
                if (value == Grayscale)
                    return;
                if (value)
                    PixelFormat |= (PixelFormat)isGrayscale;
                else
                    PixelFormat &= (PixelFormat)~isGrayscale;
                PixelFormat |= (PixelFormat)isCustom;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format uses premultiplied alpha.
        /// Setting this property to <see langword="true"/>&#160;sets also the <see cref="HasAlpha"/> property.
        /// </summary>
        public bool HasPremultipliedAlpha
        {
            get => PixelFormat.IsPremultiplied();
            set
            {
                if (value == HasPremultipliedAlpha)
                    return;
                if (value)
                {
                    PixelFormat |= PixelFormat.PAlpha;
                    HasAlpha = true;
                }
                else
                    PixelFormat &= ~PixelFormat.PAlpha;

                PixelFormat |= (PixelFormat)isCustom;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format supports single-bit alpha only (a pixel is either completely transparent or completely opaque).
        /// Setting this property to <see langword="true"/>&#160;sets also the <see cref="HasAlpha"/> property.
        /// It is not mandatory to set this property for custom single-bit alpha formats but it helps optimizing some drawing operations.
        /// </summary>
        public bool HasSingleBitAlpha
        {
            get => ((int)PixelFormat & hasSingleBitAlpha) != 0;
            set
            {
                if (value == HasSingleBitAlpha)
                    return;
                if (value)
                {
                    PixelFormat |= (PixelFormat)hasSingleBitAlpha;
                    HasAlpha = true;
                }
                else
                    PixelFormat &= (PixelFormat)~hasSingleBitAlpha;

                PixelFormat |= (PixelFormat)isCustom;
            }
        }

        #endregion

        #region Internal Properties

        internal PixelFormat PixelFormat { get; private set; }

        internal bool HasMultiLevelAlpha => PixelFormat != PixelFormat.Format16bppArgb1555 // TODO: remove this clause when removing Drawing.Common
            && HasAlpha && !HasSingleBitAlpha;

        #endregion

        #region Private Properties

        private string DebuggerValue => PixelFormat.IsValidFormat()
            ? Enum<PixelFormat>.ToString(PixelFormat)
            : $"{BitsPerPixel}bpp"
            + $"{(Indexed ? $" | {nameof(Indexed)}" : null)}"
            + $"{(HasAlpha ? $" | {nameof(HasAlpha)}" : null)}"
            + $"{(HasPremultipliedAlpha ? $" | {nameof(HasPremultipliedAlpha)}" : null)}"
            + $"{(HasSingleBitAlpha ? $" | {nameof(HasSingleBitAlpha)}" : null)}"
            + $"{(Grayscale ? $" | {nameof(Grayscale)}" : null)}";

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelFormatInfo"/> struct.
        /// </summary>
        /// <param name="bitsPerPixel">The bits-per-pixel value of the pixel format to create. Must be between 1 and 128.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitsPerPixel"/> must be between 1 and 128.</exception>
        public PixelFormatInfo(byte bitsPerPixel) => PixelFormat = (PixelFormat)(bitsPerPixel << 8);

        #endregion

        #region Internal Constructors

        internal PixelFormatInfo(PixelFormat pixelFormat)
        {
            // TODO: In next major change make it public and accept known pixel formats only.
            //       For now it must accept any pixel format because IBitmapData.PixelFormat is not PixelFormatInfo yet.
            Debug.Assert(pixelFormat.ToBitsPerPixel() is > 0 and <= 128);
            PixelFormat = pixelFormat;
        }

        #endregion

        #endregion
    }
}