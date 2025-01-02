#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPsrgba8888Srgb.cs
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
    /// This is a very strange format that converts the already premultiplied sRGB values to "sRGB" again, as if the original color space was linear. Skia's description is:
    /// "A color type that linearizes just after loading, and re-encodes to sRGB just before storing, mimicking the GPU formats that work the same way"
    /// Please also note that premultiplication does not occur again in the new "sRGB" space, so alpha can be invalid until converting the color space back to real sRGB.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorPsrgba8888Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte b;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors

        internal ColorPsrgba8888Srgb(PColor32 c)
        {
            // Note that due to double sRGB conversion the alpha value can be invalid until converting back to real sRGB
            r = c.R.ToSrgb();
            g = c.G.ToSrgb();
            b = c.B.ToSrgb();
            a = c.A;
        }

        internal ColorPsrgba8888Srgb(PColor64 c)
        {
            // Note that due to double sRGB conversion the alpha value can be invalid until converting back to real sRGB
            r = c.R.ToSrgbByte();
            g = c.G.ToSrgbByte();
            b = c.B.ToSrgbByte();
            a = ColorSpaceHelper.ToByte(c.A);
        }

        #endregion

        #region Methods

        internal PColor32 ToPColor32() => new PColor32(a, r.ToLinear(), g.ToLinear(), b.ToLinear()).Clip();
        internal PColor64 ToPColor64() => new PColor64(ColorSpaceHelper.ToUInt16(a), r.ToLinearUInt16(), g.ToLinearUInt16(), b.ToLinearUInt16()).Clip();

        #endregion
    }
}
