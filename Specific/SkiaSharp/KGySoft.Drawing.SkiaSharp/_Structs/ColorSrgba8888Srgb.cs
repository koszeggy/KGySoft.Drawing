#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorSrgba8888Srgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    /// <summary>
    /// This is a very strange format that converts the already sRGB values to "sRGB" again, as if the original color space was linear. Skia's description is:
    /// "A color type that linearizes just after loading, and re-encodes to sRGB just before storing, mimicking the GPU formats that work the same way"
    /// Note that there is no ColorSrgba8888Linear type, because that would be the same as the existing ColorRgba8888Srgb, which is actually used when the specified color space is linear.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorSrgba8888Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte b;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors

        internal ColorSrgba8888Srgb(Color32 c)
        {
            r = c.R.ToSrgb();
            g = c.G.ToSrgb();
            b = c.B.ToSrgb();
            a = c.A;
        }

        internal ColorSrgba8888Srgb(Color64 c)
        {
            r = c.R.ToSrgbByte();
            g = c.G.ToSrgbByte();
            b = c.B.ToSrgbByte();
            a = ColorSpaceHelper.ToByte(c.A);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(a, r.ToLinear(), g.ToLinear(), b.ToLinear());
        internal Color64 ToColor64() => new Color64(ColorSpaceHelper.ToUInt16(a), r.ToLinearUInt16(), g.ToLinearUInt16(), b.ToLinearUInt16());

        #endregion
    }
}
