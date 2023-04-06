#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgba8888Linear.cs
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
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorRgba8888Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte b;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors

        internal ColorRgba8888Linear(Color32 c)
        {
            r = c.R.ToLinear();
            g = c.G.ToLinear();
            b = c.B.ToLinear();
            a = c.A;
        }

        internal ColorRgba8888Linear(ColorF c)
        {
            r = ColorSpaceHelper.ToByte(c.R);
            g = ColorSpaceHelper.ToByte(c.G);
            b = ColorSpaceHelper.ToByte(c.B);
            a = ColorSpaceHelper.ToByte(c.A);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(a, r.ToSrgb(), g.ToSrgb(), b.ToSrgb());

        #endregion
    }
}
