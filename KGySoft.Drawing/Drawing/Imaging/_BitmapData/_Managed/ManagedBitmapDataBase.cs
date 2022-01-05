#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataBase.cs
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

using System;
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapDataBase : BitmapDataBase
    {
        #region Methods

        internal abstract ref byte GetPinnableReference();

        #endregion

        #region Constructors

        protected ManagedBitmapDataBase(Size size, PixelFormat pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Action<Palette>? setPalette, Action? disposeCallback)
            : base(size, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback)
        {
        }

        #endregion
    }
}