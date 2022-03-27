#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color16As24.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a 16 bpp 565 or 555 color obtained as a 24bpp bitmap data.
    /// Used on non-Windows systems where libgdiplus does not support 16bpp pixel formats directly
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    internal readonly struct Color16As24
    {
        #region Fields

        [FieldOffset(0)]private readonly byte b;
        [FieldOffset(1)]private readonly byte g;
        [FieldOffset(2)]private readonly byte r;

        #endregion

        #region Constructors

        internal Color16As24(Color32 c, bool is565)
        {
            b = (byte)((c.B >> 3) << 3);
            g = (byte)(is565 ? ((c.B >> 2) << 2) : ((c.B >> 3) << 3));
            r = (byte)((c.R >> 3) << 3);
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(r, g, b);

        #endregion
    }
}