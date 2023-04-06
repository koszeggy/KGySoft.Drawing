#region Copyright

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
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    internal readonly struct ColorPrgbaF32Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly float r;
        [FieldOffset(4)]private readonly float g;
        [FieldOffset(8)]private readonly float b;
        [FieldOffset(12)]private readonly float a;

        #endregion

        #region Constructors

        internal ColorPrgbaF32Srgb(Color32 c)
        {
            if (c.A == Byte.MinValue)
            {
                this = default;
                return;
            }

            a = ColorSpaceHelper.ToFloat(c.A);
            if (c.A == Byte.MaxValue)
            {
                r = ColorSpaceHelper.ToFloat(c.R);
                g = ColorSpaceHelper.ToFloat(c.G);
                b = ColorSpaceHelper.ToFloat(c.B);
                return;
            }

            r = ColorSpaceHelper.ToFloat(c.R) * a;
            g = ColorSpaceHelper.ToFloat(c.G) * a;
            b = ColorSpaceHelper.ToFloat(c.B) * a;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            if (a >= 1f)
            {
                return new Color32(ColorSpaceHelper.ToByte(a),
                    ColorSpaceHelper.ToByte(r),
                    ColorSpaceHelper.ToByte(g),
                    ColorSpaceHelper.ToByte(b));
            }

            if (a > 0f)
            {
                return new Color32(ColorSpaceHelper.ToByte(a),
                    ColorSpaceHelper.ToByte(r / a),
                    ColorSpaceHelper.ToByte(g / a),
                    ColorSpaceHelper.ToByte(b / a));
            }

            return default;
        }

        #endregion
    }
}