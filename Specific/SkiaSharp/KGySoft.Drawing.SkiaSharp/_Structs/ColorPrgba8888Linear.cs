#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgba8888Linear.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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

        internal ColorPrgba8888Linear(PColorF c)
        {
            // This would be the solution without floating-point operations but it's quantizing the result too heavily
            // and it's not even faster on targets where vectorization can be used:
            //PColor32 premultiplied = new Color32(c.A, c.R.ToLinear(), c.G.ToLinear(), c.B.ToLinear()).ToPremultiplied();
            //r = premultiplied.R;
            //g = premultiplied.G;
            //b = premultiplied.B;
            //a = premultiplied.A;

            PColor32 linear32 = c.ToPColor32(false);
            r = linear32.R;
            g = linear32.G;
            b = linear32.B;
            a = linear32.A;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32()
        {
            Color32 linear32 = new PColor32(a, r, g, b).ToStraight();
            return new Color32(a, linear32.R.ToSrgb(), linear32.G.ToSrgb(), linear32.B.ToSrgb());
        }

        internal Color64 ToColor64()
        {
            Color64 linear64 = new PColor32(a, r, g, b).ToPColor64().ToStraight();
            return new Color64(ColorSpaceHelper.ToUInt16(a), linear64.R.ToSrgb(), linear64.G.ToSrgb(), linear64.B.ToSrgb());
        }

        internal PColorF ToPColorF() => new PColor32(a, r, g, b).ToPColorF(false);

        #endregion
    }
}
