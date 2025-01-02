﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatInfo.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a pixel format in a technology-agnostic way. Can be used to specify custom pixel formats
    /// for the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">CreateBitmapData</see> method overloads
    /// with a <see cref="PixelFormatInfo"/>, <see cref="CustomBitmapDataConfig"/> or <see cref="CustomIndexedBitmapDataConfig"/> parameter.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerValue) + "}")]
    public struct PixelFormatInfo : IEquatable<PixelFormatInfo>
    {
        #region Constants

        #region Internal Constants

        // ReSharper disable InconsistentNaming - matching names with System.Drawing.Imaging.PixelFormat
        // The following constants are basically the same as in System.Drawing.Imaging.PixelFormat,
        // occasionally decorated by some custom flags.
        internal const int Format1bppIndexed = 1 | (1 << 8) | FlagIndexed | isGdiCompatible;
        internal const int Format4bppIndexed = 2 | (4 << 8) | FlagIndexed | isGdiCompatible;
        internal const int Format8bppIndexed = 3 | (8 << 8) | FlagIndexed | isGdiCompatible;
        internal const int Format16bppGrayScale = 4 | (16 << 8) | prefersColor64 | isGrayscale;
        internal const int Format16bppRgb555 = 5 | (16 << 8) | isGdiCompatible;
        internal const int Format16bppRgb565 = 6 | (16 << 8) | isGdiCompatible;
        internal const int Format16bppArgb1555 = 7 | (16 << 8) | FlagHasAlpha | isGdiCompatible | hasSingleBitAlpha;
        internal const int Format24bppRgb = 8 | (24 << 8) | isGdiCompatible;
        internal const int Format32bppRgb = 9 | (32 << 8) | isGdiCompatible;
        internal const int Format32bppArgb = 10 | (32 << 8) | FlagHasAlpha | isGdiCompatible | isCanonical;
        internal const int Format32bppPArgb = 11 | (32 << 8) | FlagHasAlpha | hasPAlpha | isGdiCompatible;
        internal const int Format48bppRgb = 12 | (48 << 8) | prefersColor64;
        internal const int Format64bppArgb = 13 | (64 << 8) | FlagHasAlpha | isCanonical | prefersColor64;
        internal const int Format64bppPArgb = 14 | (64 << 8) | FlagHasAlpha | hasPAlpha | prefersColor64;
        // skipping 15, which is used for CMYK by an unnamed value in the System formats
        internal const int Format96bppRgb = 16 | (96 << 8) | prefersColorF | isLinear;
        internal const int Format128bppRgba = 17 | (128 << 8) | FlagHasAlpha | prefersColorF | isLinear;
        internal const int Format128bppPRgba = 18 | (128 << 8) | FlagHasAlpha | hasPAlpha | prefersColorF | isLinear;
        internal const int Format8ppGrayScale = 19 | (8 << 8) | isGrayscale;
        internal const int Format32bppGrayScale = 20 | (32 << 8) | prefersColorF | isLinear | isGrayscale;
        // ReSharper restore InconsistentNaming

        // The following constants have their corresponding values in System.Drawing.Imaging.PixelFormat.
        internal const int FlagHasAlpha = 0x00040000;
        internal const int FlagIndexed = 0x00010000;

        #endregion

        #region Private Constants

        // Some more constants that have their corresponding values in System.Drawing.Imaging.PixelFormat.
        // Several flags are used just for compatibility reasons.
        private const int isGdiCompatible = 0x00020000;
        private const int hasPAlpha = 0x00080000;
        private const int prefersColor64 = 0x00100000;
        private const int isCanonical = 0x00200000;

        // Further flags are custom ones (flags 24..31 are reserved in System.Drawing.Imaging.PixelFormat)
        private const int isCustomFormat = 1 << 24;
        private const int isGrayscale = 1 << 25;
        private const int hasSingleBitAlpha = 1 << 26;
        private const int prefersColorF = 1 << 27;
        private const int isLinear = 1 << 28;

        #endregion

        #endregion

        #region Fields

        private int value;

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
            readonly get => (byte)((value >> 8) & 0xFF);
            set
            {
                if (value == BitsPerPixel)
                    return;
                if (value is 0 or > 128)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeBetween(1, 128));
                this.value = (isCustomFormat | (value & ~0xFF00) | (value << 8));
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format has an alpha channel (transparency).
        /// For <see cref="Indexed"/> formats this property can be <see langword="false"/> because alpha support is determined by the current palette.
        /// Setting this property to <see langword="false"/> resets also the <see cref="HasPremultipliedAlpha"/> and <see cref="HasSingleBitAlpha"/> properties.
        /// </summary>
        public bool HasAlpha
        {
            readonly get => (value & FlagHasAlpha) != 0;
            set
            {
                if (value == HasAlpha)
                    return;
                if (value)
                    this.value |= FlagHasAlpha;
                else
                    this.value &= ~(FlagHasAlpha | hasPAlpha | hasSingleBitAlpha);
                this.value |= isCustomFormat;
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
            readonly get => (value & FlagIndexed) != 0;
            set
            {
                if (value == Indexed)
                    return;
                if (value)
                    this.value |= FlagIndexed;
                else
                    this.value &= ~FlagIndexed;
                this.value |= isCustomFormat;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format is a grayscale one.
        /// For <see cref="Indexed"/> formats this property can be <see langword="false"/> because grayscale nature is determined by the current palette.
        /// Setting this property for non-indexed custom grayscale formats helps to auto select the preferable strategy for some operations such as dithering.
        /// </summary>
        public bool Grayscale
        {
            readonly get => (value & isGrayscale) != 0;
            set
            {
                if (value == Grayscale)
                    return;
                if (value)
                    this.value |= isGrayscale;
                else
                    this.value &= ~isGrayscale;
                this.value |= isCustomFormat;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format uses premultiplied alpha.
        /// Setting this property to <see langword="true"/> sets also the <see cref="HasAlpha"/> property.
        /// </summary>
        public bool HasPremultipliedAlpha
        {
            readonly get => (value & hasPAlpha) != 0;
            set
            {
                if (value == HasPremultipliedAlpha)
                    return;
                if (value)
                    this.value |= (hasPAlpha | FlagHasAlpha);
                else
                    this.value &= ~hasPAlpha;
                this.value |= isCustomFormat;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format supports single-bit alpha only (a pixel is either completely transparent or completely opaque).
        /// Setting this property to <see langword="true"/> sets also the <see cref="HasAlpha"/> property.
        /// It is not mandatory to set this property for custom single-bit alpha formats but it helps optimizing some drawing operations.
        /// </summary>
        public bool HasSingleBitAlpha
        {
            readonly get => (value & hasSingleBitAlpha) != 0;
            set
            {
                if (value == HasSingleBitAlpha)
                    return;
                if (value)
                    this.value |= (hasSingleBitAlpha | FlagHasAlpha);
                else
                    this.value &= ~hasSingleBitAlpha;
                this.value |= isCustomFormat;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format has linear gamma.
        /// For <see cref="Indexed"/> formats this property should be <see langword="false"/> because it can be configured
        /// at the <see cref="Palette"/> constructors and factory methods whether the palette should work in the linear color space.
        /// If the <see cref="IBitmapData.WorkingColorSpace">IBitmapData.WorkingColorSpace</see> property returns <see cref="WorkingColorSpace.Default"/>,
        /// then the value of this property may affect the selected color space of some operations.
        /// </summary>
        public bool LinearGamma
        {
            readonly get => (value & isLinear) != 0;
            set
            {
                if (value == LinearGamma)
                    return;
                if (value)
                    this.value |= isLinear;
                else
                    this.value &= ~isLinear;
                this.value |= isCustomFormat;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format prefers 64-bit colors (<see cref="Color64"/>, or <see cref="PColor64"/>
        /// if <see cref="HasPremultipliedAlpha"/> is also set) when getting or setting pixels. Some operations may consider the value of this property.
        /// Setting this property to <see langword="true"/> resets the <see cref="Prefers128BitColors"/> property.
        /// If <see cref="LinearGamma"/> is also set, then some writing operations may prefer using <see cref="ColorF"/> or <see cref="PColorF"/> types
        /// regardless of this property.
        /// </summary>
        public bool Prefers64BitColors
        {
            readonly get => (value & prefersColor64) != 0;
            set
            {
                if (value == Prefers64BitColors)
                    return;
                if (value)
                    this.value = (this.value | prefersColor64) & ~prefersColorF;
                else
                    this.value &= ~prefersColor64;
                this.value |= isCustomFormat;
            }
        }

        /// <summary>
        /// Gets or sets whether the represented pixel format prefers 128-bit colors (<see cref="ColorF"/>, or <see cref="PColorF"/>
        /// if <see cref="HasPremultipliedAlpha"/> is also set) when getting or setting pixels. Some operations may consider the value of this property.
        /// Setting this property to <see langword="true"/> resets the <see cref="Prefers64BitColors"/> property.
        /// </summary>
        public bool Prefers128BitColors
        {
            readonly get => (value & prefersColorF) != 0;
            set
            {
                if (value == Prefers128BitColors)
                    return;
                if (value)
                    this.value = (this.value | prefersColorF) & ~prefersColor64;
                else
                    this.value &= ~prefersColorF;
                this.value |= isCustomFormat;
            }
        }

        /// <summary>
        /// Gets whether the represented pixel format is a custom one. That is, when this <see cref="PixelFormatInfo"/>
        /// was not instantiated by its <see cref="PixelFormatInfo(KnownPixelFormat)">constructor</see> with a <see cref="KnownPixelFormat"/> parameter
        /// or one of its properties have been set since then.
        /// </summary>
        public readonly bool IsCustomFormat => (value & isCustomFormat) != 0;

        #endregion

        #region Internal Properties

        internal readonly bool HasMultiLevelAlpha => HasAlpha && !HasSingleBitAlpha;
        internal readonly KnownPixelFormat AsKnownPixelFormatInternal => (KnownPixelFormat)value;
        internal readonly bool IsKnownFormat => value != 0 && (value & isCustomFormat) == 0;
        internal readonly bool IsWide => (value & (prefersColor64 | prefersColorF)) != 0;

        internal readonly bool CanBeDithered
        {
            get
            {
                int bpp = BitsPerPixel;
                return bpp < 8 || !Grayscale && bpp < 24;
            }
        }

        #endregion

        #region Private Properties

        private readonly string DebuggerValue => !IsCustomFormat
            ? Enum<KnownPixelFormat>.ToString((KnownPixelFormat)value)
            : $"CustomFormat{BitsPerPixel}bpp"
            + $"{(Indexed ? $" | {nameof(Indexed)}" : null)}"
            + $"{(HasAlpha ? $" | {nameof(HasAlpha)}" : null)}"
            + $"{(HasPremultipliedAlpha ? $" | {nameof(HasPremultipliedAlpha)}" : null)}"
            + $"{(HasSingleBitAlpha ? $" | {nameof(HasSingleBitAlpha)}" : null)}"
            + $"{(Prefers64BitColors ? $" | {nameof(Prefers64BitColors)}" : null)}"
            + $"{(Prefers128BitColors ? $" | {nameof(Prefers128BitColors)}" : null)}"
            + $"{(Grayscale ? $" | {nameof(Grayscale)}" : null)}"
            + $"{(LinearGamma ? $" | {nameof(LinearGamma)}" : null)}";

        #endregion

        #endregion

        #region Operators

        /// <summary>
        /// Gets whether two <see cref="PixelFormatInfo"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="PixelFormatInfo"/> instance that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="PixelFormatInfo"/> instance that is to the right of the equality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="PixelFormatInfo"/> structures are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(PixelFormatInfo left, PixelFormatInfo right) => left.Equals(right);

        /// <summary>
        /// Gets whether two <see cref="PixelFormatInfo"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="PixelFormatInfo"/> instance that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="PixelFormatInfo"/> instance that is to the right of the inequality operator.</param>
        /// <returns><see langword="true"/> if the two <see cref="PixelFormatInfo"/> structures are different; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(PixelFormatInfo left, PixelFormatInfo right) => !left.Equals(right);

        #endregion

        #region Constructors

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelFormatInfo"/> struct.
        /// </summary>
        /// <param name="bitsPerPixel">The bits-per-pixel value of the pixel format to create. Must be between 1 and 128.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bitsPerPixel"/> must be between 1 and 128.</exception>
        public PixelFormatInfo(byte bitsPerPixel)
        {
            if (bitsPerPixel is 0 or > 128)
                throw new ArgumentOutOfRangeException(nameof(bitsPerPixel), PublicResources.ArgumentMustBeBetween(1, 128));
            value = (bitsPerPixel << 8) | isCustomFormat;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelFormatInfo"/> struct.
        /// </summary>
        /// <param name="pixelFormat">A known pixel format to initialize a <see cref="PixelFormatInfo"/> from.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> must be a valid format.</exception>
        public PixelFormatInfo(KnownPixelFormat pixelFormat) : this((uint)pixelFormat)
        {
            if (pixelFormat == KnownPixelFormat.Undefined || !pixelFormat.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
        }

        #endregion

        #region Internal Constructors

        internal PixelFormatInfo(uint value) => this.value = (int)value;

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Converts this <see cref="PixelFormatInfo"/> to a <see cref="KnownPixelFormat"/> representing a compatible pixel format.
        /// </summary>
        /// <returns>A <see cref="KnownPixelFormat"/> representing a compatible pixel format.</returns>
        public readonly KnownPixelFormat ToKnownPixelFormat()
        {
            if (!IsCustomFormat)
            {
                Debug.Assert(AsKnownPixelFormatInternal.IsDefined());
                return AsKnownPixelFormatInternal;
            }

            int bpp = BitsPerPixel;
            if (Prefers128BitColors || bpp > 64)
                return HasPremultipliedAlpha ? KnownPixelFormat.Format128bppPRgba
                    : HasAlpha ? KnownPixelFormat.Format128bppRgba
                    : Grayscale ? KnownPixelFormat.Format32bppGrayScale
                    : KnownPixelFormat.Format96bppRgb;

            if (Prefers64BitColors || bpp > 32)
                return HasPremultipliedAlpha ? KnownPixelFormat.Format64bppPArgb
                    : HasAlpha ? KnownPixelFormat.Format64bppArgb
                    : Grayscale ? KnownPixelFormat.Format16bppGrayScale
                    : KnownPixelFormat.Format48bppRgb;

            if (bpp > 8 || !Indexed)
                return HasPremultipliedAlpha ? KnownPixelFormat.Format32bppPArgb
                    : HasAlpha || Indexed ? KnownPixelFormat.Format32bppArgb
                    : Grayscale ? (bpp > 16 ? KnownPixelFormat.Format32bppGrayScale
                        : bpp > 8 ? KnownPixelFormat.Format16bppGrayScale
                        : KnownPixelFormat.Format8bppGrayScale)
                    : KnownPixelFormat.Format24bppRgb;
            
            return bpp switch
            {
                > 4 => KnownPixelFormat.Format8bppIndexed,
                > 1 => KnownPixelFormat.Format4bppIndexed,
                _ => KnownPixelFormat.Format1bppIndexed
            };
        }

        /// <summary>
        /// Determines whether the current <see cref="PixelFormatInfo"/> instance is equal to another one.
        /// </summary>
        /// <param name="other">A <see cref="PixelFormatInfo"/> structure to compare with this <see cref="PixelFormatInfo"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="PixelFormatInfo"/> instance is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public readonly bool Equals(PixelFormatInfo other) => value == other.value;

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this <see cref="PixelFormatInfo"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this <see cref="PixelFormatInfo"/> instance.</param>
        /// <returns><see langword="true"/>, if the current <see cref="PixelFormatInfo"/> instance is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override readonly bool Equals(object? obj) => obj is PixelFormatInfo other && Equals(other);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override readonly int GetHashCode() => value;

        /// <summary>
        /// Gets the string representation of this <see cref="PixelFormatInfo"/> instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this <see cref="PixelFormatInfo"/> instance.</returns>
        public override string ToString() => DebuggerValue;

        #endregion

        #region Internal Methods

        internal readonly int GetByteWidth(int pixelWidth) => (pixelWidth * BitsPerPixel + 7) >> 3;

        internal readonly bool IsAtByteBoundary(int x)
        {
            int bpp = BitsPerPixel;
            return (bpp & 7) == 0 || ((bpp * x) & 7) == 0;
        }

        internal int GetColorsLimit()
        {
            int bpp = BitsPerPixel;
            return bpp switch
            {
                32 => AsKnownPixelFormatInternal == KnownPixelFormat.Format32bppRgb ? 1 << 24 : Int32.MaxValue,
                16 => AsKnownPixelFormatInternal switch
                {
                    KnownPixelFormat.Format16bppRgb555 => 1 << 15,
                    KnownPixelFormat.Format16bppArgb1555 => (1 << 15) + 1,
                    _ => 1 << 16
                },
                _ => bpp <= 30 ? 1 << bpp : Int32.MaxValue
            };
        }

        #endregion

        #endregion
    }
}