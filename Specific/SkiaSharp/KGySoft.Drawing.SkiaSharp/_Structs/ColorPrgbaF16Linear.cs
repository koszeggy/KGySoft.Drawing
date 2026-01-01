#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorPrgbaF16Linear.cs
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
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    internal readonly struct ColorPrgbaF16Linear
    {
        #region Fields

        [FieldOffset(0)]private readonly Half r;
        [FieldOffset(2)]private readonly Half g;
        [FieldOffset(4)]private readonly Half b;
        [FieldOffset(6)]private readonly Half a;

        #endregion

        #region Properties

        private float A => (float)a;
        private float R => (float)r;
        private float G => (float)g;
        private float B => (float)b;

        #endregion

        #region Constructors

        internal ColorPrgbaF16Linear(PColorF c)
        {
            r = (Half)c.R;
            g = (Half)c.G;
            b = (Half)c.B;
            a = (Half)c.A;
        }

        #endregion

        #region Methods

        internal PColorF ToPColorF() => new PColorF(A, R, G, B);

        #endregion
    }
}