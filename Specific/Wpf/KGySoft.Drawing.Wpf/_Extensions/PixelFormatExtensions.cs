#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PixelFormatExtensions.cs
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

using System.Windows.Media;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    internal static class PixelFormatExtensions
    {
        #region Methods

        internal static KnownPixelFormat AsKnownPixelFormat(this PixelFormat pixelFormat)
            => pixelFormat == PixelFormats.Bgra32 ? KnownPixelFormat.Format32bppArgb
             : pixelFormat == PixelFormats.Pbgra32 ? KnownPixelFormat.Format32bppPArgb
             : pixelFormat == PixelFormats.Bgr32 ? KnownPixelFormat.Format32bppRgb
             : pixelFormat == PixelFormats.Bgr24 ? KnownPixelFormat.Format24bppRgb
             : pixelFormat == PixelFormats.Indexed1 ? KnownPixelFormat.Format1bppIndexed
             : pixelFormat == PixelFormats.Indexed4 ? KnownPixelFormat.Format4bppIndexed
             : pixelFormat == PixelFormats.Indexed8 ? KnownPixelFormat.Format8bppIndexed
             : pixelFormat == PixelFormats.Bgr555 ? KnownPixelFormat.Format16bppRgb555
             : pixelFormat == PixelFormats.Bgr565 ? KnownPixelFormat.Format16bppRgb565
             : pixelFormat == PixelFormats.Gray16 ? KnownPixelFormat.Format16bppGrayScale
             : default;

        #endregion
    }
}