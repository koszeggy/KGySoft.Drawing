﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorGrayF.cs
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

        internal ColorGrayF(Color32 c) => value = c.GetBrightnessLinear();

        #endregion

        #region Methods

        internal Color32 ToColor32() => Color32.FromGray(value.ToNonLinear8Bit());

        #endregion
    }
}