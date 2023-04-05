#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorGrayF.cs
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

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    internal struct ColorGrayF
    {
        #region Fields

        private readonly float value;

        #endregion

        #region Constructors

        internal ColorGrayF(Color32 c)
        {
            Debug.Assert(c.A == Byte.MaxValue);
            value = ColorSpaceHelper.SrgbToLinear(c.GetBrightnessF());
        }

        internal ColorGrayF(ColorF c)
        {
            Debug.Assert(c.A >= 1f);
            value = c.GetBrightness();
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => Color32.FromGray(ColorSpaceHelper.LinearToSrgb8Bit(value));

        #endregion
    }
}