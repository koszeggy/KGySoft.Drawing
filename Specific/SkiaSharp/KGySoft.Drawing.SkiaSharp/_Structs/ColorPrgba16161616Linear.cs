﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgba16161616Linear.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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

        internal ColorPrgba16161616Linear(PColorF c)
        {
            PColor64 linear64 = c.ToPColor64(false);
            r = linear64.R;
            g = linear64.G;
            b = linear64.B;
            a = linear64.A;
        }

        #endregion

        #region Methods

        internal Color64 ToColor64()
        {
            Color64 linear64 = new PColor64(a, r, g, b).ToStraight();
            return new Color64(a, linear64.R.ToSrgb(), linear64.G.ToSrgb(), linear64.B.ToSrgb());
        }

        internal PColorF ToPColorF() => new PColor64(a, r, g, b).ToPColorF(false);

        #endregion
    }
}
