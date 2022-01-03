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

using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public struct PixelFormatInfo
    {
        #region Properties

        #region Public Properties

        public byte BitsPerPixel
        {
            get => (byte)PixelFormat.ToBitsPerPixel();
            set => PixelFormat = (PixelFormat)((value & ~0xFF00) | (value << 8));
        }

        public bool HasAlpha
        {
            get => PixelFormat.HasAlpha();
            set => PixelFormat = value ? PixelFormat & ~PixelFormat.Alpha : PixelFormat | PixelFormat.Alpha;
        }

        public bool IsIndexed
        {
            get => PixelFormat.IsIndexed();
            set => PixelFormat = value ? PixelFormat & ~PixelFormat.Indexed : PixelFormat | PixelFormat.Indexed;
        }

        #endregion

        #region Internal Properties

        internal PixelFormat PixelFormat { get; private set; }

        #endregion

        #endregion

        #region Constructors

        #region Public Constructors

        public PixelFormatInfo(byte bitsPerPixel) => PixelFormat = (PixelFormat)(bitsPerPixel << 8);

        #endregion

        #region Internal Constructors

        internal PixelFormatInfo(PixelFormat pixelFormat) => PixelFormat = pixelFormat;

        #endregion

        #endregion
    }
}