#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorBgr101010.cs
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
    internal readonly struct ColorBgr101010
    {
        #region Constants

        private const int redMask = 0b00111111_11110000_00000000_00000000;
        private const int greenMask = 0x00_0F_FC_00;
        private const int blueMask = 0x00_00_03_FF;

        #endregion

        #region Fields

        private readonly uint value;

        #endregion

        #region Properties

        private uint R => (value & redMask) >> 20;
        private uint G => (value & greenMask) >> 10;
        private uint B => value & blueMask;

        #endregion

        #region Constructors

        internal ColorBgr101010(Color64 c)
        {
            Debug.Assert(c.A == UInt16.MaxValue);
            value = (uint)((c.R >> 6) << 20)
                | (uint)((c.G >> 6) << 10)
                | (uint)(c.B >> 6);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32((byte)(R >> 2), (byte)(G >> 2), (byte)(B >> 2));
        
        internal Color64 ToColor64()
        {
            uint r = R << 6;
            uint g = G << 6;
            uint b = B << 6;
            return new Color64((ushort)(r | (r >> 10)), (ushort)(g | (g >> 10)), (ushort)(b | (b >> 10)));
        }

        #endregion
    }
}