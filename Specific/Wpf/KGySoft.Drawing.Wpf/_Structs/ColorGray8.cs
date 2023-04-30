﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorGray8.cs
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

namespace KGySoft.Drawing.Wpf
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal readonly struct ColorGray8
    {
        #region Fields

        private readonly byte value;

        #endregion

        #region Constructors

        internal ColorGray8(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            value = c.GetBrightness();
        }

        internal ColorGray8(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            value = ColorSpaceHelper.LinearToSrgb8Bit(c.GetBrightness());
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => Color32.FromGray(value);

        #endregion
    }
}