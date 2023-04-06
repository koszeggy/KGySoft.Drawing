#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRg1616Srgb.cs
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
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    internal readonly struct ColorRg1616Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly ushort r;
        [FieldOffset(2)]private readonly ushort g;

        #endregion

        #region Constructors

        internal ColorRg1616Srgb(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            r = ColorSpaceHelper.ToUInt16(c.R);
            g = ColorSpaceHelper.ToUInt16(c.G);
        }

        internal ColorRg1616Srgb(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            r = c.R;
            g = c.G;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(ColorSpaceHelper.ToByte(r), ColorSpaceHelper.ToByte(g), 0);

        #endregion
    }
}