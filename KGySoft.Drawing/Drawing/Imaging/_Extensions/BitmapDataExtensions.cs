#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadableBitmapData"/>, <see cref="IWritableBitmapData"/>
    /// and <see cref="IReadWriteBitmapData"/> types.
    /// </summary>
    public static partial class BitmapDataExtensions
    {
        #region Constants

        private const int parallelThreshold = 100;
        private const int quantizingScale = 1;
        private const int ditheringScale = 2;

        #endregion

        #region Methods

        #region Internal Methods

        internal static Size GetSize(this IBitmapData? bitmapData) => bitmapData == null ? default : new Size(bitmapData.Width, bitmapData.Height);

        internal static bool HasMultiLevelAlpha(this IBitmapData bitmapData)
        {
            PixelFormat pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasMultiLevelAlpha() || pixelFormat.IsIndexed() && bitmapData.Palette?.HasMultiLevelAlpha == true;
        }

        internal static bool IsFastPremultiplied(this IBitmapData bitmapData)
            => bitmapData.PixelFormat.IsPremultiplied()
                && bitmapData is ManagedBitmapDataBase { IsCustomPixelFormat: false } or UnmanagedBitmapDataBase { IsCustomPixelFormat: false };

        internal static bool HasAlpha(this IBitmapData bitmapData)
        {
            PixelFormat pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasAlpha() || pixelFormat.IsIndexed() && bitmapData.Palette?.HasAlpha == true;
        }

        internal static bool SupportsTransparency(this IBitmapData bitmapData)
        {
            PixelFormat pixelFormat = bitmapData.PixelFormat;
            return pixelFormat.HasAlpha() || pixelFormat.IsIndexed() && bitmapData.Palette?.HasTransparent == true;
        }

        #endregion

        #region Private Methods

        private static void Unwrap<TBitmapData>(ref TBitmapData source, ref Rectangle newRectangle)
            where TBitmapData : IBitmapData
        {
            while (true)
            {
                switch (source)
                {
                    case ClippedBitmapData clipped:
                        source = (TBitmapData)clipped.BitmapData;
                        Rectangle region = clipped.Region;
                        newRectangle.Offset(region.Location);
                        newRectangle.Intersect(region);
                        continue;
                    case BitmapDataWrapper wrapper:
                        Debug.Fail("Wrapper has been leaked out, check call stack");
                        source = (TBitmapData)wrapper.BitmapData;
                        continue;
                    default:
                        return;
                }
            }
        }

        private static void AdjustQuantizerAndDitherer(IBitmapData target, ref IQuantizer? quantizer, ref IDitherer? ditherer)
        {
            if (quantizer != null || ditherer == null)
                return;

            if (target.PixelFormat.CanBeDithered())
                quantizer = PredefinedColorsQuantizer.FromBitmapData(target);
            else
                ditherer = null;
        }

        #endregion

        #endregion
    }
}
