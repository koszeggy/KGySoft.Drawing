#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgba8888Srgb.cs
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
    internal struct ColorPrgba8888Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte b;
        [FieldOffset(3)]private readonly byte a;

        #endregion

        #region Constructors

        internal ColorPrgba8888Srgb(PColor32 c)
        {
            r = c.R;
            g = c.G;
            b = c.B;
            a = c.A;
        }

        #endregion

        #region Methods

        internal PColor32 ToPColor32() => new PColor32(a, r, g, b);

        #endregion
    }
}