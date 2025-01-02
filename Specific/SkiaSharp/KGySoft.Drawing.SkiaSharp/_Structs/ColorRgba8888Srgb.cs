#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgba8888Srgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// It's normally used for <see cref="SKColorType.Rgba8888"/> when the color space is sRGB,
    /// but it's used also for <see cref="SKColorType.Srgba8888"/> when the color space is linear.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorRgba8888Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte b;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors

        internal ColorRgba8888Srgb(Color32 c)
        {
            r = c.R;
            g = c.G;
            b = c.B;
            a = c.A;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(a, r, g, b);

        #endregion
    }
}
