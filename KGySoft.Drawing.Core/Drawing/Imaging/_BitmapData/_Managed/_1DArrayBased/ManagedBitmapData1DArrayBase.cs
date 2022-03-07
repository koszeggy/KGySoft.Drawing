#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData1DArrayBase.cs
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
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapData1DArrayBase<T> : ManagedBitmapDataBase
        where T : unmanaged
    {
        #region Fields
        
        /// <summary>
        /// The pixel buffer where the underlying array is a single dimensional one.
        /// It is a field rather than a property so possible Dispose from a self-allocating derived classes allows mutating it.
        /// </summary>
        internal Array2D<T> Buffer;

        #endregion

        #region Constructors

        [SecuritySafeCritical]
        protected unsafe ManagedBitmapData1DArrayBase(Array2D<T> buffer, Size size, PixelFormatInfo pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(size, pixelFormat, backColor, alphaThreshold, palette, trySetPaletteCallback, disposeCallback)
        {
            Buffer = buffer;
            RowSize = buffer.Width * sizeof(T);
        }

        #endregion

        #region Methods

#if NETCOREAPP3_0_OR_GREATER
        internal sealed override ref byte GetPinnableReference()
            => ref Unsafe.As<T, byte>(ref Buffer.GetPinnableReference());
#else
        [SecuritySafeCritical]
        internal sealed override unsafe ref byte GetPinnableReference()
        {
            ref T head = ref Buffer.GetPinnableReference();
            fixed (T* pHead = &head)
                return ref *(byte*)pHead;
        }
#endif

        #endregion
    }
}