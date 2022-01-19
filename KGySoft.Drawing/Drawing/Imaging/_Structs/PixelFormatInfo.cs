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
using System.Drawing.Imaging;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [DebuggerDisplay("{" + nameof(DebuggerValue) + "}")]
    public struct PixelFormatInfo
    {
        #region Constants

        private const int isCustom = 1 << 24;
        private const int isGrayscale = 1 << 25;
        private const int hasSingleBitAlpha = 1 << 26;

        #endregion

        #region Properties

        #region Public Properties

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

        public bool HasAlpha
        {
            get => PixelFormat.HasAlpha();
            set
            {
                if (value == HasAlpha)
                    return;
                if (value)
                {
                    PixelFormat |= PixelFormat.Alpha;
                    Indexed = false;
                }
                else
                    PixelFormat &= ~PixelFormat.Alpha;

                PixelFormat |= (PixelFormat)isCustom;
            }
        }

        public bool Indexed
        {
            get => PixelFormat.IsIndexed();
            set
            {
                if (value == Indexed)
                    return;
                if (value)
                {
                    PixelFormat |= PixelFormat.Indexed;
                    HasAlpha = false;
                }
                else
                    PixelFormat &= ~PixelFormat.Indexed;

                PixelFormat |= (PixelFormat)isCustom;
            }
        }

        // TODO: needed for non-indexed grayscale types for ErrorDiffusionDitherer to auto detect ByBrightness
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

        // actually not used effectively but affects the result of GetClosestKnownPixelFormat
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

        // may help optimize drawing operations for non-indexed formats
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

        public PixelFormatInfo(byte bitsPerPixel) => PixelFormat = (PixelFormat)(bitsPerPixel << 8);

        #endregion

        #region Internal Constructors

        internal PixelFormatInfo(PixelFormat pixelFormat)
        {
            // TODO: make public and accept known pixel formats only. For now it must accept any pixel format because IBitmapData.PixelFormat is not PixelFormatInfo yet.
            Debug.Assert(pixelFormat.ToBitsPerPixel() is > 0 and <= 128);
            PixelFormat = pixelFormat;
        }

        #endregion

        #endregion
    }
}