#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgbaF16Srgb.cs
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
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorRgbaF16Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly Half r;
        [FieldOffset(2)]private readonly Half g;
        [FieldOffset(4)]private readonly Half b;
        [FieldOffset(6)]private readonly Half a;

        #endregion

        #region Properties

        private float A => (float)a;
        private float R => (float)r;
        private float G => (float)g;
        private float B => (float)b;

        #endregion

        #region Constructors

        internal ColorRgbaF16Srgb(Color32 c)
        {
            r = (Half)ColorSpaceHelper.ToFloat(c.R);
            g = (Half)ColorSpaceHelper.ToFloat(c.G);
            b = (Half)ColorSpaceHelper.ToFloat(c.B);
            a = (Half)ColorSpaceHelper.ToFloat(c.A);
        }

        internal ColorRgbaF16Srgb(Color64 c)
        {
            r = (Half)ColorSpaceHelper.ToFloat(c.R);
            g = (Half)ColorSpaceHelper.ToFloat(c.G);
            b = (Half)ColorSpaceHelper.ToFloat(c.B);
            a = (Half)ColorSpaceHelper.ToFloat(c.A);
        }

        internal ColorRgbaF16Srgb(ColorF c)
        {
            r = (Half)ColorSpaceHelper.LinearToSrgb(c.R);
            g = (Half)ColorSpaceHelper.LinearToSrgb(c.G);
            b = (Half)ColorSpaceHelper.LinearToSrgb(c.B);
            a = (Half)c.A;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(A),
            ColorSpaceHelper.ToByte(R),
            ColorSpaceHelper.ToByte(G),
            ColorSpaceHelper.ToByte(B));

        #endregion
    }
}