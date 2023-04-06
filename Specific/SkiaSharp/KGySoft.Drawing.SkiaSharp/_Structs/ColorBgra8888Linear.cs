#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorBgra8888Linear.cs
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
    internal readonly struct ColorBgra8888Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly byte b;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte r;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors

        internal ColorBgra8888Linear(Color32 c)
        {
            b = c.B.ToLinear();
            g = c.G.ToLinear();
            r = c.R.ToLinear();
            a = c.A;
        }

        internal ColorBgra8888Linear(ColorF c)
        {
            b = ColorSpaceHelper.ToByte(c.B);
            g = ColorSpaceHelper.ToByte(c.G);
            r = ColorSpaceHelper.ToByte(c.R);
            a = ColorSpaceHelper.ToByte(c.A);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(a, r.ToSrgb(), g.ToSrgb(), b.ToSrgb());

        #endregion
    }
}
