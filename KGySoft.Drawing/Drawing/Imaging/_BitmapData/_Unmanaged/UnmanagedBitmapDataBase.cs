#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapDataBase.cs
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
    internal abstract class UnmanagedBitmapDataBase : BitmapDataBase
    {
        #region Properties

        internal IntPtr Scan0 { get; }
        internal int Stride { get; }

        #endregion

        #region Construction and Destruction

        #region Constructors

        protected UnmanagedBitmapDataBase(IntPtr buffer, Size size, int stride, PixelFormat pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Action<Palette>? setPalette, Action? disposeCallback)
            : base(size, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback)
        {
            Debug.Assert(buffer != IntPtr.Zero);
            Debug.Assert(Math.Abs(stride) >= pixelFormat.GetByteWidth(size.Width));

            Scan0 = buffer;
            Stride = stride;
            RowSize = Math.Abs(stride);
        }

        #endregion

        #region Destructor

        ~UnmanagedBitmapDataBase() => Dispose(false);

        #endregion

        #endregion
    }
}