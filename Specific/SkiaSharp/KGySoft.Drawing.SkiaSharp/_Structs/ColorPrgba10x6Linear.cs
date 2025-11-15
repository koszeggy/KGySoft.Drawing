#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgba10x6Linear.cs
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
    internal readonly struct ColorPrgba10x6Linear
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

        internal ColorPrgba10x6Linear(Color64 c)
        {
            PColor64 linear64 = new Color64(c.A,
                c.R.ToLinear(),
                c.G.ToLinear(),
                c.B.ToLinear()).ToPremultiplied();

            r = (ushort)(linear64.R & channelMask);
            g = (ushort)(linear64.G & channelMask);
            b = (ushort)(linear64.B & channelMask);
            a = (ushort)(linear64.A & channelMask);
        }

        internal ColorPrgba10x6Linear(PColorF c)
        {
            PColor64 linear64 = c.ToPColor64(false);
            r = (ushort)(linear64.R & channelMask);
            g = (ushort)(linear64.G & channelMask);
            b = (ushort)(linear64.B & channelMask);
            a = (ushort)(linear64.A & channelMask);
        }

        #endregion

        #region Methods

        internal Color64 ToColor64()
        {
            Color64 linear64 = new PColor64((ushort)(((uint)a & channelMask) | ((uint)a >> 10)),
                (ushort)(((uint)r & channelMask) | ((uint)r >> 10)),
                (ushort)(((uint)g & channelMask) | ((uint)g >> 10)),
                (ushort)(((uint)b & channelMask) | ((uint)b >> 10))).ToStraight();
            return new Color64(a, linear64.R.ToSrgb(), linear64.G.ToSrgb(), linear64.B.ToSrgb());
        }

        internal PColorF ToPColorF() => new PColor64((ushort)(((uint)a & channelMask) | ((uint)a >> 10)),
            (ushort)(((uint)r & channelMask) | ((uint)r >> 10)),
            (ushort)(((uint)g & channelMask) | ((uint)g >> 10)),
            (ushort)(((uint)b & channelMask) | ((uint)b >> 10))).ToPColorF(false);


        #endregion
    }
}