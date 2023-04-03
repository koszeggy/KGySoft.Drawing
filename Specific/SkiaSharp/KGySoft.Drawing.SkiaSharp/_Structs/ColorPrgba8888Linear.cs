#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgba8888Linear.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
    internal readonly struct ColorPrgba8888Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte b;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors

        internal ColorPrgba8888Linear(Color32 c)
        {
            // This would be the solution without floating-point operations but it's quantizing the result
            // and it's not even faster on targets where vectorization can be used:
            //PColor32 premultiplied = new Color32(c.A, c.R.ToLinear(), c.G.ToLinear(), c.B.ToLinear()).ToPremultiplied();
            //r = premultiplied.R;
            //g = premultiplied.G;
            //b = premultiplied.B;
            //a = premultiplied.A;

            PColorF result = c.ToPColorF();
            r = ColorSpaceHelper.ToByte(result.R);
            g = ColorSpaceHelper.ToByte(result.G);
            b = ColorSpaceHelper.ToByte(result.B);
            a = c.A;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            Color32 straight = new PColor32(a, r, g, b).ToStraight();
            return new Color32(a, straight.R.ToSrgb(), straight.G.ToSrgb(), straight.B.ToSrgb());
        }

        #endregion
    }
}
