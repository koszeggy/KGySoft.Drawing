#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataSingleArrayBased.cs
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
#else
using System.Security;
#endif

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapDataSingleArrayBased<T> : ManagedBitmapDataBase
    {
        #region Fields
        
        /// <summary>
        /// The pixel buffer backed by a single dimensional array.
        /// It is a field rather than a property so possible Dispose from derived classes allow mutating it.
        /// </summary>
        internal Array2D<T> Buffer;

        #endregion

        #region Constructors

        protected ManagedBitmapDataSingleArrayBased(Size size, PixelFormat pixelFormat, Color32 backColor, byte alphaThreshold, Palette? palette, Action<Palette>? setPalette, Action? disposeCallback)
            : base(size, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback)
        {
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