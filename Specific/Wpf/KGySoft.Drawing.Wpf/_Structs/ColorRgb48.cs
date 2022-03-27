#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgb48.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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

        internal ColorRgb48(Color32 c)
        {
            r = (ushort)((c.R << 8) | c.R);
            g = (ushort)((c.G << 8) | c.G);
            b = (ushort)((c.B << 8) | c.B);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32((byte)(r >> 8), (byte)(g >> 8), (byte)(b >> 8));

        #endregion
    }
}