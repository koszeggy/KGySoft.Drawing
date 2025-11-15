#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgba10x6Linear.cs
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Following the naming in SKColorType")]
    internal readonly struct ColorRgba10x6Linear
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

        internal ColorRgba10x6Linear(Color64 c)
        {
            r = (ushort)(c.R.ToLinear() & channelMask);
            g = (ushort)(c.G.ToLinear() & channelMask);
            b = (ushort)(c.B.ToLinear() & channelMask);
            a = (ushort)(c.A & channelMask);
        }

        internal ColorRgba10x6Linear(ColorF c)
        {
            Color64 linear64 = c.ToColor64(false);
            r = (ushort)(linear64.R & channelMask);
            g = (ushort)(linear64.G & channelMask);
            b = (ushort)(linear64.B & channelMask);
            a = (ushort)(linear64.A & channelMask);
        }

        #endregion

        #region Methods

        internal Color64 ToColor64() => new Color64(((ushort)(((uint)a & channelMask) | ((uint)a >> 10))),
            ((ushort)(((uint)r & channelMask) | ((uint)r >> 10))).ToSrgb(),
            ((ushort)(((uint)g & channelMask) | ((uint)g >> 10))).ToSrgb(),
            ((ushort)(((uint)b & channelMask) | ((uint)b >> 10))).ToSrgb());

        internal ColorF ToColorF() => new Color64((ushort)(((uint)a & channelMask) | ((uint)a >> 10)),
            (ushort)(((uint)r & channelMask) | ((uint)r >> 10)),
            (ushort)(((uint)g & channelMask) | ((uint)g >> 10)),
            (ushort)(((uint)b & channelMask) | ((uint)b >> 10))).ToColorF(false);


        #endregion
    }
}