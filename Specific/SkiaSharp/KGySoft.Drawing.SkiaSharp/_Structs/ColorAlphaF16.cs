#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorAlphaF16.cs
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
    [StructLayout(LayoutKind.Sequential, Size = 2)]
    internal readonly struct ColorAlphaF16
    {
        #region Fields

        private readonly Half a;

        #endregion

        #region Constructors

        internal ColorAlphaF16(Color32 c) => a = (Half)ColorSpaceHelper.ToFloat(c.A);

        #endregion

        #region Methods

        internal Color32 ToColor32() => Color32.FromArgb(ColorSpaceHelper.ToByte((float)a), default);

        #endregion
    }
}