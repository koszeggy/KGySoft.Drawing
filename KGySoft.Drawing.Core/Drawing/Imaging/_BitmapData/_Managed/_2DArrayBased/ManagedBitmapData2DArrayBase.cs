#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData2DArrayBase.cs
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
#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#elif NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapData2DArrayBase<T> : ManagedBitmapDataBase
        where T : unmanaged
    {
        #region Properties

        internal T[,] Buffer { get; }

        #endregion

        #region Constructors

        [SecuritySafeCritical]
        protected unsafe ManagedBitmapData2DArrayBase(T[,] buffer, Size size, PixelFormatInfo pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            : base(size, pixelFormat, backColor, alphaThreshold, palette, trySetPaletteCallback, disposeCallback)
        {
            Buffer = buffer;
            RowSize = buffer.GetLength(1) * sizeof(T);
        }

        #endregion

        #region Methods

#if NET6_0_OR_GREATER
        internal sealed override ref byte GetPinnableReference() => ref MemoryMarshal.GetArrayDataReference(Buffer);
#elif NETCOREAPP3_0_OR_GREATER
        internal sealed override ref byte GetPinnableReference() => ref Unsafe.As<T, byte>(ref Buffer[0, 0]);
#else
        [SecuritySafeCritical]
        internal sealed override unsafe ref byte GetPinnableReference()
        {
            ref T head = ref Buffer[0, 0];
            fixed (T* pHead = &head)
                return ref *(byte*)pHead;
        }
#endif

        #endregion
    }
}