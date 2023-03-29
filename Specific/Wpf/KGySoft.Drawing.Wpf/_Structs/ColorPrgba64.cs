#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgba64.cs
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

namespace KGySoft.Drawing.Wpf
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorPrgba64
    {
        #region Fields

        [FieldOffset(0)]
        private readonly ushort r;
        [FieldOffset(2)]
        private readonly ushort g;
        [FieldOffset(4)]
        private readonly ushort b;
        [FieldOffset(6)]
        private readonly ushort a;

        #endregion

        #region Constructors

        internal ColorPrgba64(Color32 c)
        {
            if (c.A == 0)
            {
                this = default;
                return;
            }

            var pc64 = c.ToPColor64();
            r = pc64.R;
            g = pc64.G;
            b = pc64.B;
            a = pc64.A;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => a == 0 ? default : new PColor64(a, r, g, b).ToColor32();

        #endregion
    }
}