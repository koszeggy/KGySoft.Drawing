#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRg88Srgb.cs
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

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct ColorRg88Srgb
    {
        #region Fields

        [FieldOffset(0)]private readonly byte r;
        [FieldOffset(1)]private readonly byte g;

        #endregion

        #region Constructors

        internal ColorRg88Srgb(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            r = c.R;
            g = c.G;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(r, g, 0);

        #endregion
    }
}
