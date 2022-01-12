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

#endregion

namespace KGySoft.Drawing.Imaging
{
    [DebuggerDisplay("{" + nameof(DebuggerValue) + "}")]
    public struct PixelFormatInfo
    {
        #region Constants

        private const int isGrayscale = 1 << 24;

        #endregion

        #region Properties

        #region Public Properties

        public byte BitsPerPixel
        {
            get => (byte)PixelFormat.ToBitsPerPixel();
            set
            {
                if (value > 128)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeLessThanOrEqualTo(128));
                PixelFormat = (PixelFormat)((value & ~0xFF00) | (value << 8));
            }
        }

        public bool HasAlpha
        {
            get => PixelFormat.HasAlpha();
            set
            {
                if (value)
                {
                    PixelFormat |= PixelFormat.Alpha;
                    IsIndexed = false;
                }
                else
                    PixelFormat &= ~PixelFormat.Alpha;
            }
        }

        public bool IsIndexed
        {
            get => PixelFormat.IsIndexed();
            set
            {
                if (value)
                {
                    PixelFormat |= PixelFormat.Indexed;
                    HasAlpha = false;
                }
                else
                    PixelFormat &= ~PixelFormat.Indexed;
            }
        }

        // TODO: needed for non-indexed grayscale types for ErrorDiffusionDitherer to auto detect ByBrightness
        public bool IsGrayscale
        {
            get => ((int)PixelFormat & isGrayscale) != 0;
            set
            {
                if (value)
                    PixelFormat |= (PixelFormat)isGrayscale;
                else
                    PixelFormat &= (PixelFormat)~isGrayscale;
            }
        }

        #endregion

        #region Internal Properties

        internal PixelFormat PixelFormat { get; private set; }

        #endregion

        #region Private Properties

        private string DebuggerValue => $"{BitsPerPixel}bpp{(HasAlpha ? $"| {nameof(HasAlpha)}" : null)}{(IsIndexed ? $"| {nameof(IsIndexed)}" : null)}";

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        public PixelFormatInfo(byte bitsPerPixel) => PixelFormat = (PixelFormat)(bitsPerPixel << 8);

        #endregion

        #region Internal Constructors

        internal PixelFormatInfo(PixelFormat pixelFormat)
        {
            Debug.Assert(pixelFormat.ToBitsPerPixel() <= 128);
            PixelFormat = pixelFormat;
        }

        #endregion

        #endregion
    }
}