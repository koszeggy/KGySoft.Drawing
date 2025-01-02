#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgb48.cs
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

namespace KGySoft.Drawing.Wpf
{
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    internal readonly struct ColorRgb48
    {
        #region Fields

        [FieldOffset(0)]
        private readonly ushort r;
        [FieldOffset(2)]
        private readonly ushort g;
        [FieldOffset(4)]
        private readonly ushort b;

        #endregion

        #region Constructors

        internal ColorRgb48(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            r = c.R;
            g = c.G;
            b = c.B;
        }

        #endregion

        #region Methods

        internal Color64 ToColor64() => new Color64(r, g, b);

        #endregion
    }
}