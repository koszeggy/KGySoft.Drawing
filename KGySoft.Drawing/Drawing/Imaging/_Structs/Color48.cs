#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color48.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    internal readonly struct Color48
    {
        #region Fields

        [FieldOffset(0)]
        internal readonly ushort B;
        [FieldOffset(2)]
        internal readonly ushort G;
        [FieldOffset(4)]
        internal readonly ushort R;

        #endregion

        #region Constructors

        internal Color48(ushort r, ushort g, ushort b)
        {
            B = b;
            G = g;
            R = r;
        }

        internal Color48(Color32 c)
        {
            B = (ushort)((c.B << 8) | c.B);
            G = (ushort)((c.G << 8) | c.G);
            R = (ushort)((c.R << 8) | c.R);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32((byte)(R >> 8), (byte)(G >> 8), (byte)(B >> 8));

        #endregion
    }
}