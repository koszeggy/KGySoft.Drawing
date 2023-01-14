#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataConfig.cs
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
using System.Drawing;

using KGySoft.CoreLibraries;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal readonly struct BitmapDataConfig
    {
        #region Fields

        internal readonly Size Size;
        internal readonly PixelFormatInfo PixelFormat;
        internal readonly Color32 BackColor;
        internal readonly byte AlphaThreshold;
        internal readonly BlendingMode BlendingMode;
        internal readonly Palette? Palette;
        internal readonly Func<Palette, bool>? TrySetPaletteCallback;
        internal readonly Action? DisposeCallback;

        #endregion

        #region Constructors

        internal BitmapDataConfig(Size size, PixelFormatInfo pixelFormat,
            Color32 backColor = default, byte alphaThreshold = 128, BlendingMode blendingMode = BlendingMode.Default,
            Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
        {
            Size = size;
            PixelFormat = pixelFormat;
            BackColor = backColor;
            AlphaThreshold = alphaThreshold;
            BlendingMode = blendingMode;
            Palette = palette;
            TrySetPaletteCallback = trySetPaletteCallback;
            DisposeCallback = disposeCallback;
        }

        #endregion
    }
}