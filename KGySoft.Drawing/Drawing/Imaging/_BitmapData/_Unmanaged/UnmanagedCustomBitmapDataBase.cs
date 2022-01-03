#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedCustomBitmapDataBase.cs
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
    internal abstract class UnmanagedCustomBitmapDataBase : NativeBitmapDataBase
    {
        #region Properties

        public override bool IsCustomPixelFormat => true;

        #endregion

        #region Constructors

        protected UnmanagedCustomBitmapDataBase(IntPtr buffer, Size size, int stride, PixelFormat pixelFormat, Color32 backColor, byte alphaThreshold, Palette? palette, Action<Palette>? setPalette, Action? disposeCallback)
            : base(buffer, size, stride, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback)
        {
        }

        #endregion
    }
}