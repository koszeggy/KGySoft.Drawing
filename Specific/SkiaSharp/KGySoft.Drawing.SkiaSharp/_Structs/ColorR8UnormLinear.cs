#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorR8UnormLinear.cs
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
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorR8UnormLinear
    {
        #region Fields

        [FieldOffset(0)] private readonly byte r;

        #endregion

        #region Constructors

        internal ColorR8UnormLinear(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            r = c.R.ToLinear();
        }

        internal ColorR8UnormLinear(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            r = c.R.ToLinearByte();
        }

        internal ColorR8UnormLinear(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            r = ColorSpaceHelper.ToByte(c.R);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(r.ToSrgb(), 0, 0);
        internal Color64 ToColor64() => new Color64(r.ToSrgbUInt16(), 0, 0);
        internal ColorF ToColorF() => new Color32(r, 0, 0).ToColorF(false);

        #endregion
    }
}