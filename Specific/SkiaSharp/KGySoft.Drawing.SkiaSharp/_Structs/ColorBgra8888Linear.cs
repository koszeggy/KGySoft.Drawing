#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorBgra8888Linear.cs
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

using System.Runtime.CompilerServices;
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

        internal ColorBgra8888Linear(Color64 c)
        {
            b = c.B.ToLinearByte();
            g = c.G.ToLinearByte();
            r = c.R.ToLinearByte();
            a = ColorSpaceHelper.ToByte(c.A);
        }

        internal ColorBgra8888Linear(ColorF c) => this = Unsafe.As<Color32, ColorBgra8888Linear>(ref Unsafe.AsRef(c.ToColor32(false)));

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(a, r.ToSrgb(), g.ToSrgb(), b.ToSrgb());
        internal Color64 ToColor64() => new Color64(ColorSpaceHelper.ToUInt16(a), r.ToSrgbUInt16(), g.ToSrgbUInt16(), b.ToSrgbUInt16());
        internal ColorF ToColorF() => Unsafe.As<ColorBgra8888Linear, Color32>(ref Unsafe.AsRef(this)).ToColorF(false);

        #endregion
    }
}
