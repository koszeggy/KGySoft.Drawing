#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgbaF32Srgb.cs
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

using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal readonly struct ColorRgbaF32Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly float r;
        [FieldOffset(4)]private readonly float g;
        [FieldOffset(8)]private readonly float b;
        [FieldOffset(12)]private readonly float a;

        #endregion

        #region Constructors

        internal ColorRgbaF32Srgb(Color32 c)
        {
            r = ColorSpaceHelper.ToFloat(c.R);
            g = ColorSpaceHelper.ToFloat(c.G);
            b = ColorSpaceHelper.ToFloat(c.B);
            a = ColorSpaceHelper.ToFloat(c.A);
        }

        internal ColorRgbaF32Srgb(Color64 c)
        {
            r = ColorSpaceHelper.ToFloat(c.R);
            g = ColorSpaceHelper.ToFloat(c.G);
            b = ColorSpaceHelper.ToFloat(c.B);
            a = ColorSpaceHelper.ToFloat(c.A);
        }

        internal ColorRgbaF32Srgb(ColorF c)
        {
            r = ColorSpaceHelper.LinearToSrgb(c.R);
            g = ColorSpaceHelper.LinearToSrgb(c.G);
            b = ColorSpaceHelper.LinearToSrgb(c.B);
            a = c.A;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(a),
            ColorSpaceHelper.ToByte(r),
            ColorSpaceHelper.ToByte(g),
            ColorSpaceHelper.ToByte(b));

        #endregion
    }
}