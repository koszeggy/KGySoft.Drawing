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
    internal static class KnownPixelFormatExtensions
    {
        #region Methods

        internal static PixelFormatInfo ToInfo(this KnownPixelFormat pixelFormat) => new PixelFormatInfo((int)pixelFormat);
        internal static bool IsValidFormat(this KnownPixelFormat pixelFormat) => pixelFormat != KnownPixelFormat.Undefined && pixelFormat.IsDefined();
        internal static bool HasAlpha(this KnownPixelFormat pixelFormat) => ((int)pixelFormat & PixelFormatInfo.FlagHasAlpha) != 0;
        internal static bool IsIndexed(this KnownPixelFormat pixelFormat) => ((int)pixelFormat & PixelFormatInfo.FlagIndexed) != 0;
        internal static int ToBitsPerPixel(this KnownPixelFormat pixelFormat) => ((int)pixelFormat >> 8) & 0xFF;
        internal static int GetByteWidth(this KnownPixelFormat pixelFormat, int pixelWidth) =>  (pixelWidth * pixelFormat.ToBitsPerPixel() + 7) >> 3;
        internal static bool CanBeDithered(this KnownPixelFormat pixelFormat) => pixelFormat.ToInfo().CanBeDithered;
        internal static bool IsAtByteBoundary(this KnownPixelFormat pixelFormat, int x) => pixelFormat.ToInfo().IsAtByteBoundary(x);

        #endregion
    }
}