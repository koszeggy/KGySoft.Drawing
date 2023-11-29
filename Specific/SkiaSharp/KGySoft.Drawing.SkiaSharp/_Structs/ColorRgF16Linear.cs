#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgF16Linear.cs
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

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    internal readonly struct ColorRgF16Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly Half r;
        [FieldOffset(2)]private readonly Half g;

        #endregion

        #region Properties

        private float R => (float)r;
        private float G => (float)g;

        #endregion

        #region Constructors

        internal ColorRgF16Linear(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            r = (Half)c.R;
            g = (Half)c.G;
        }

        #endregion

        #region Methods

        internal ColorF ToColorF() => new ColorF(R, G, 0f);

        #endregion
    }
}