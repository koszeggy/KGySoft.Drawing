#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgba10x6Srgb.cs
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

using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorRgba10x6Srgb
    {
        #region Constants

        private const ushort channelMask = 0b11111111_11000000;

        #endregion

        #region Fields

        [FieldOffset(0)]private readonly ushort r;
        [FieldOffset(2)]private readonly ushort g;
        [FieldOffset(4)]private readonly ushort b;
        [FieldOffset(6)]private readonly ushort a;

        #endregion

        #region Constructors

        internal ColorRgba10x6Srgb(Color64 c)
        {
            r = (ushort)(c.R & channelMask);
            g = (ushort)(c.G & channelMask);
            b = (ushort)(c.B & channelMask);
            a = (ushort)(c.A & channelMask);
        }

        #endregion

        #region Methods

        internal Color64 ToColor64() => new Color64((ushort)(((uint)a & channelMask) | ((uint)a >> 10)),
            (ushort)(((uint)r & channelMask) | ((uint)r >> 10)),
            (ushort)(((uint)g & channelMask) | ((uint)g >> 10)),
            (ushort)(((uint)b & channelMask) | ((uint)b >> 10)));

        internal Color32 ToColor32() => new Color32((byte)(a >> 8),
            (byte)(r >> 8),
            (byte)(g >> 8),
            (byte)(b >> 8));

        #endregion
    }
}