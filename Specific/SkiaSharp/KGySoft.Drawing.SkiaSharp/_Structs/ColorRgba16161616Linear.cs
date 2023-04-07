#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgba16161616Linear.cs
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
    internal readonly struct ColorRgba16161616Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly ushort r;
        [FieldOffset(2)]private readonly ushort g;
        [FieldOffset(4)]private readonly ushort b;
        [FieldOffset(6)]private readonly ushort a;

        #endregion

        #region Constructors

        internal ColorRgba16161616Linear(Color32 c)
            : this(new Color64(c))
        {
        }

        internal ColorRgba16161616Linear(Color64 c)
        {
            r = c.R.ToLinear();
            g = c.G.ToLinear();
            b = c.B.ToLinear();
            a = c.A;
        }

        internal ColorRgba16161616Linear(ColorF c)
        {
            r = ColorSpaceHelper.ToUInt16(c.R);
            g = ColorSpaceHelper.ToUInt16(c.G);
            b = ColorSpaceHelper.ToUInt16(c.B);
            a = ColorSpaceHelper.ToUInt16(c.A);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(a), r.ToSrgbByte(), g.ToSrgbByte(), b.ToSrgbByte());

        #endregion
    }
}