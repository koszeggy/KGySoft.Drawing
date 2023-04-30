﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgbaF32Srgb.cs
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
    internal readonly struct ColorPrgbaF16Srgb
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

        internal ColorPrgbaF16Srgb(Color32 c)
        {
            if (c.A == Byte.MinValue)
            {
                this = default;
                return;
            }

            float aF = ColorSpaceHelper.ToFloat(c.A);
            a = (Half)aF;
            if (c.A == Byte.MaxValue)
            {
                r = (Half)ColorSpaceHelper.ToFloat(c.R);
                g = (Half)ColorSpaceHelper.ToFloat(c.G);
                b = (Half)ColorSpaceHelper.ToFloat(c.B);
                return;
            }

            r = (Half)(ColorSpaceHelper.ToFloat(c.R) * aF);
            g = (Half)(ColorSpaceHelper.ToFloat(c.G) * aF);
            b = (Half)(ColorSpaceHelper.ToFloat(c.B) * aF); 
        }

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            if (A >= 1f)
            {
                return new Color32(ColorSpaceHelper.ToByte(A),
                    ColorSpaceHelper.ToByte(R),
                    ColorSpaceHelper.ToByte(G),
                    ColorSpaceHelper.ToByte(B));
            }

            if (A >= 0f)
            {
                return new Color32(ColorSpaceHelper.ToByte(A),
                    ColorSpaceHelper.ToByte(R / A),
                    ColorSpaceHelper.ToByte(G / A),
                    ColorSpaceHelper.ToByte(B / A));
            }

            return default;
        }

        #endregion
    }
}