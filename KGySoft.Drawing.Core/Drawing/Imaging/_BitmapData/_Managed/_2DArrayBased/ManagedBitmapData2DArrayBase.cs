#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData2DArrayBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.CompilerServices;
#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapData2DArrayBase<T, TRow> : ManagedBitmapDataBase
        where T : unmanaged
        where TRow : ManagedBitmapDataRow2DBase<T>, new()
    {
        #region Properties

        protected T[,] Buffer { get; }

        #endregion

        #region Constructors

        [SecuritySafeCritical]
        protected unsafe ManagedBitmapData2DArrayBase(T[,] buffer, in BitmapDataConfig cfg)
            : base(cfg)
        {
            Buffer = buffer;
            RowSize = buffer.GetLength(1) * sizeof(T);
        }

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override TResult DoReadRaw<TResult>(int x, int y) => GetPixelRef<TResult>(y, x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public sealed override void DoWriteRaw<TValue>(int x, int y, TValue data) => GetPixelRef<TValue>(y, x) = data;

        #endregion

        #region Internal Methods

        [SecuritySafeCritical]
#if NET6_0_OR_GREATER
        internal sealed override ref byte GetPinnableReference() => ref MemoryMarshal.GetArrayDataReference(Buffer);
#else
        internal sealed override ref byte GetPinnableReference() => ref Buffer[0, 0].As<T, byte>();
#endif

        #endregion

        #region Protected Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected ref TPixel GetPixelRef<TPixel>(int rowIndex, int offset)
            where TPixel : unmanaged
        {
#if NET6_0_OR_GREATER
            return ref GetPinnableReference().At<byte, TPixel>(rowIndex * RowSize, offset);
#else
            // we could use the same as above but under .NET 6 the GetPinnableReference already has an indexed access so it is faster this way
            return ref Buffer[rowIndex, 0].At<T, TPixel>(offset);
#endif
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected sealed override IBitmapDataRowInternal DoGetRow(int y) => new TRow
        {
            Buffer = Buffer,
            BitmapData = this,
            Index = y,
        };

        #endregion

        #endregion
    }
}