#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: KnownPixelFormatExtensions.cs
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

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public static class KnownPixelFormatExtensions
    {
        #region Methods

        #region Public Methods
        
        public static int ToBitsPerPixel(this KnownPixelFormat pixelFormat) => ((int)pixelFormat >> 8) & 0xFF;
        public static bool IsValidFormat(this KnownPixelFormat pixelFormat) => pixelFormat != KnownPixelFormat.Undefined && pixelFormat.IsDefined();
        // TODO Docs: does not check validity. Use GetInfo().Indexed if needed
        public static bool IsIndexed(this KnownPixelFormat pixelFormat) => ((int)pixelFormat & PixelFormatInfo.FlagIndexed) != 0;
        public static bool HasAlpha(this KnownPixelFormat pixelFormat) => ((int)pixelFormat & PixelFormatInfo.FlagHasAlpha) != 0;
        public static PixelFormatInfo GetInfo(this KnownPixelFormat pixelFormat) => new PixelFormatInfo(pixelFormat);

        #endregion

        #region Internal Methods
        
        internal static PixelFormatInfo ToInfoInternal(this KnownPixelFormat pixelFormat) => new PixelFormatInfo((uint)pixelFormat);
        internal static int GetByteWidth(this KnownPixelFormat pixelFormat, int pixelWidth) => (pixelWidth * pixelFormat.ToBitsPerPixel() + 7) >> 3;
        internal static bool CanBeDithered(this KnownPixelFormat pixelFormat) => pixelFormat.ToInfoInternal().CanBeDithered;
        internal static bool IsAtByteBoundary(this KnownPixelFormat pixelFormat, int x) => pixelFormat.ToInfoInternal().IsAtByteBoundary(x);

        #endregion

        #endregion
    }
}