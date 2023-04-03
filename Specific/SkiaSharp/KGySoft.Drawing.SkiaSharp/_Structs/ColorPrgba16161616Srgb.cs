#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgba16161616Srgb.cs
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
    internal struct ColorPrgba16161616Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly ushort r;
        [FieldOffset(2)]private readonly ushort g;
        [FieldOffset(4)]private readonly ushort b;
        [FieldOffset(6)]private readonly ushort a;

        #endregion

        #region Constructors

        internal ColorPrgba16161616Srgb(Color32 c)
        {
            var pc32 = new PColor64(c);
            r = pc32.R;
            g = pc32.G;
            b = pc32.B;
            a = pc32.A;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new PColor64(a, r, g, b).ToColor32();

        #endregion
    }
}