#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: KnownPixelFormatExtensions.cs
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

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Contains extension methods for the <see cref="KnownPixelFormat"/> type.
    /// </summary>
    public static class KnownPixelFormatExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets the bits per pixel (BPP) value of a <see cref="KnownPixelFormat"/> value without checking
        /// whether <paramref name="pixelFormat"/> represents a valid value.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to convert.</param>
        /// <returns>The bits per pixel (BPP) value of a <see cref="KnownPixelFormat"/> value.</returns>
        public static int ToBitsPerPixel(this KnownPixelFormat pixelFormat) => ((int)pixelFormat >> 8) & 0xFF;

        /// <summary>
        /// Gets whether this <see cref="KnownPixelFormat"/> instance represents a valid format.
        /// The valid format values are the named ones, exception with the default <see cref="KnownPixelFormat.Undefined"/> value.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to be checked.</param>
        /// <returns><see langword="true"/>, if this <see cref="KnownPixelFormat"/> instance represents a valid format; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidFormat(this KnownPixelFormat pixelFormat) => pixelFormat != KnownPixelFormat.Undefined && pixelFormat.IsDefined();

        /// <summary>
        /// Gets whether this <see cref="KnownPixelFormat"/> instance represents an indexed format without checking
        /// whether <paramref name="pixelFormat"/> represents a valid value.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to be checked.</param>
        /// <returns><see langword="true"/>, if this <see cref="KnownPixelFormat"/> instance represents an indexed format; otherwise, <see langword="false"/>.</returns>
        public static bool IsIndexed(this KnownPixelFormat pixelFormat) => ((int)pixelFormat & PixelFormatInfo.FlagIndexed) != 0;

        /// <summary>
        /// Gets whether this <see cref="KnownPixelFormat"/> instance represents a format with alpha (transparency) without checking
        /// whether <paramref name="pixelFormat"/> represents a valid value.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to be checked.</param>
        /// <returns><see langword="true"/>, if this <see cref="KnownPixelFormat"/> instance represents a format with alpha; otherwise, <see langword="false"/>.</returns>
        public static bool HasAlpha(this KnownPixelFormat pixelFormat) => ((int)pixelFormat & PixelFormatInfo.FlagHasAlpha) != 0;

        /// <summary>
        /// Gets a <see cref="PixelFormatInfo"/> for this <paramref name="pixelFormat"/>.
        /// </summary>
        /// <param name="pixelFormat">The pixel format to retrieve a <see cref="PixelFormatInfo"/> for.</param>
        /// <returns>A <see cref="PixelFormatInfo"/> representing the specified <paramref name="pixelFormat"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelFormat"/> must be a valid format.</exception>
        public static PixelFormatInfo GetInfo(this KnownPixelFormat pixelFormat) => new PixelFormatInfo(pixelFormat);

        #endregion

        #region Internal Methods
        
        internal static PixelFormatInfo ToInfoInternal(this KnownPixelFormat pixelFormat) => new PixelFormatInfo((uint)pixelFormat);
        internal static int GetByteWidth(this KnownPixelFormat pixelFormat, int pixelWidth) => (pixelWidth * pixelFormat.ToBitsPerPixel() + 7) >> 3;
        internal static bool CanBeDithered(this KnownPixelFormat pixelFormat) => pixelFormat.ToInfoInternal().CanBeDithered;
        internal static bool IsAtByteBoundary(this KnownPixelFormat pixelFormat, int x) => pixelFormat.ToInfoInternal().IsAtByteBoundary(x);
        internal static bool IsGrayscale(this KnownPixelFormat pixelFormat) => pixelFormat.ToInfoInternal().Grayscale;

        #endregion

        #endregion
    }
}