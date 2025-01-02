#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPsrgba8888Linear.cs
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
    /// This is a very strange format that performs the premultiplication in the linear color space first, and then converts the result to sRGB.
    /// This is exactly the opposite of the usual approach (ColorF.ToPColor32) where the color space conversion is done first and then the premultiplication.
    /// Skia's description is: "A color type that linearizes just after loading, and re-encodes to sRGB just before storing, mimicking the GPU formats that work the same way"
    /// Please also note that due to the switched order of premultiplication and color space conversion the alpha can be invalid (smaller as the RGB components) until converting the color space back to linear.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorPsrgba8888Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte b;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors

        internal ColorPsrgba8888Linear(PColorF c)
        {
            // Awkward, but intentional step: reinterpreting the PColorF as a ColorF before converting it to sRGB to prevent regular un-premultiplication.
            // The result is a strange sRGB PColor32-like format where the alpha can be invalid (smaller as the RGB components) until converting the color space back to linear.
            var srgb = ColorF.FromRgba(c.ToRgba()).ToColor32();
            r = srgb.R;
            g = srgb.G;
            b = srgb.B;
            a = srgb.A;
        }

        #endregion

        #region Methods

        internal PColorF ToPColorF()
        {
            var linear = new Color32(a, r, g, b).ToColorF();
            return PColorF.FromRgba(linear.ToRgba()).Clip();
        }

        #endregion
    }
}
