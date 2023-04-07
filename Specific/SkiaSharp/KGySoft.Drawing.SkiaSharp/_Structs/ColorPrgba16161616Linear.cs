#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgba16161616Linear.cs
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
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorPrgba16161616Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly ushort r;
        [FieldOffset(2)]private readonly ushort g;
        [FieldOffset(4)]private readonly ushort b;
        [FieldOffset(6)]private readonly ushort a;

        #endregion

        #region Constructors

        internal ColorPrgba16161616Linear(Color32 c)
        {
            // This would be the solution without floating-point operations but it's quantizing the result too heavily
            // and it's not even faster on targets where vectorization can be used:
            //PColor32 premultiplied = new Color32(c.A, c.R.ToLinear(), c.G.ToLinear(), c.B.ToLinear()).ToPremultiplied();
            //r = premultiplied.R;
            //g = premultiplied.G;
            //b = premultiplied.B;
            //a = premultiplied.A;

            PColorF result = c.ToPColorF();
            r = ColorSpaceHelper.ToUInt16(result.R);
            g = ColorSpaceHelper.ToUInt16(result.G);
            b = ColorSpaceHelper.ToUInt16(result.B);
            a = ColorSpaceHelper.ToUInt16(c.A);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            // Cheating: the temp PColor64/Color64 instances are actually in the linear color space
            Color64 straight = new PColor64(a, r, g, b).ToStraight();
            return new Color32(ColorSpaceHelper.ToByte(a), straight.R.ToSrgbByte(), straight.G.ToSrgbByte(), straight.B.ToSrgbByte());
        }

        #endregion
    }
}
