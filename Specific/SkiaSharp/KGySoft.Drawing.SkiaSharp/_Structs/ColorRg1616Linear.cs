#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRg1616Linear.cs
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

using System;
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    internal readonly struct ColorRg1616Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly ushort r;
        [FieldOffset(2)]private readonly ushort g;

        #endregion

        #region Constructors

        internal ColorRg1616Linear(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            r = c.R.ToLinear();
            g = c.G.ToLinear();
        }

        internal ColorRg1616Linear(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            r = ColorSpaceHelper.ToUInt16(c.R);
            g = ColorSpaceHelper.ToUInt16(c.G);
        }

        #endregion

        #region Methods

        internal Color64 ToColor64() => new Color64(r.ToSrgb(), g.ToSrgb(), 0);
        internal ColorF ToColorF() => new Color64(r, g, 0).ToColorF(false);

        #endregion
    }
}