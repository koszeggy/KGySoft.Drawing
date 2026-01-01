#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorR8UnormSrgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorR8UnormSrgb
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;

        #endregion

        #region Constructors

        internal ColorR8UnormSrgb(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            r = c.R;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(r, 0, 0);

        #endregion
    }
}
