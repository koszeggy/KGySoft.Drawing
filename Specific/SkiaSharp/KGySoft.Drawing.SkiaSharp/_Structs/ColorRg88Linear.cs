#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRg88Linear.cs
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

using System;
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorRg88Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;

        #endregion

        #region Constructors

        internal ColorRg88Linear(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            r = c.R.ToLinear();
            g = c.G.ToLinear();
        }

        internal ColorRg88Linear(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            r = c.R.ToLinearByte();
            g = c.G.ToLinearByte();
        }

        internal ColorRg88Linear(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            r = ColorSpaceHelper.ToByte(c.R);
            g = ColorSpaceHelper.ToByte(c.G);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(r.ToSrgb(), g.ToSrgb(), 0);

        #endregion
    }
}
