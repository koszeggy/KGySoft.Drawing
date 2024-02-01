#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedCustomBitmapDataIndexed.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a managed bitmap data wrapper with custom indexed pixel format for a 1D array (wrapped into an <see cref="Array2D{T}"/>).
    /// </summary>
    internal sealed class ManagedCustomBitmapDataIndexed<T> : ManagedBitmapData1DArrayIndexedBase<T, ManagedCustomBitmapDataIndexed<T>.Row>, ICustomBitmapData
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataRowIndexedBase<T>, ICustomBitmapDataRow<T>
        {
            #region Properties and Indexers

            #region Properties

            #region Protected Properties

            protected override uint MaxIndex => (1u << BitmapData.PixelFormat.BitsPerPixel) - 1u;

            #endregion

            #region Explicitly Implemented Interface Properties

            IBitmapData ICustomBitmapDataRow.BitmapData => BitmapData;

            #endregion

            #endregion

            #region Indexers

            ref T ICustomBitmapDataRow<T>.this[int index] => ref Row.GetElementReference(index);

            #endregion

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ((ManagedCustomBitmapDataIndexed<T>)BitmapData).rowGetColorIndex.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => ((ManagedCustomBitmapDataIndexed<T>)BitmapData).rowSetColorIndex.Invoke(this, x, colorIndex);

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public unsafe ref TValue GetRefAs<TValue>(int x) where TValue : unmanaged
            {
                if ((x + 1) * sizeof(TValue) > BitmapData.RowSize)
                    ThrowXOutOfRange();
                return ref UnsafeGetRefAs<TValue>(x);
            }

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public ref TValue UnsafeGetRefAs<TValue>(int x) where TValue : unmanaged
            {
#if NETCOREAPP3_0_OR_GREATER
                return ref Unsafe.Add(ref Unsafe.As<T, TValue>(ref Row.GetPinnableReference()), x);
#else
                unsafe
                {
                    fixed (T* pRow = Row)
                        return ref ((TValue*)pRow)[x];
                }
#endif
            }

            #endregion
        }

        #endregion

        #region Fields

        private Func<ICustomBitmapDataRow<T>, int, int> rowGetColorIndex;
        private Action<ICustomBitmapDataRow<T>, int, int> rowSetColorIndex;

        #endregion

        #region Properties

        public override bool IsCustomPixelFormat => true;
        public bool CanReadWrite { get; }
        public bool BackBufferIndependentPixelAccess { get; }

        public unsafe Func<Size, WorkingColorSpace, IBitmapDataInternal> CreateCompatibleBitmapDataFactory
        {
            [SecuritySafeCritical]
            get
            {
                Debug.Assert(BackBufferIndependentPixelAccess);
                if (IsDisposed)
                    ThrowDisposed();

                // Creating locals for all used members so self reference will not be captured.
                Func<ICustomBitmapDataRow<T>, int, int> getColorIndex = rowGetColorIndex;
                Action<ICustomBitmapDataRow<T>, int, int> setColorIndex = rowSetColorIndex;
                Palette palette = Palette!;
                PixelFormatInfo pixelFormat = PixelFormat;
                int origWidth = Width;
                int origBufferWidth = Buffer.Width;
                return (size, workingColorSpace) =>
                {
                    Debug.Assert(size.Width > 0 && size.Height > 0);
                    Array2D<T> newBuffer;

                    // original width: the original stride must be alright
                    if (size.Width == origWidth)
                        newBuffer = new Array2D<T>(size.Height, origBufferWidth);
                    else
                    {
                        // new width: assuming at least 16 byte units for custom ICustomBitmapDataRow casts
                        int stride = pixelFormat.GetByteWidth(size.Width);
                        stride += 16 - stride % 16;
                        if (16 % sizeof(T) != 0)
                            stride += sizeof(T) - stride % sizeof(T);
                        newBuffer = new Array2D<T>(size.Height, stride / sizeof(T));
                    }

                    var cfg = new CustomIndexedBitmapDataConfig
                    {
                        PixelFormat = pixelFormat,
                        RowGetColorIndex = getColorIndex as Func<ICustomBitmapDataRow, int, int>,
                        RowSetColorIndex = setColorIndex as Action<ICustomBitmapDataRow, int, int>,
                        Palette = palette.WorkingColorSpace == workingColorSpace ? palette : new Palette(palette, workingColorSpace, palette.BackColor, palette.AlphaThreshold),
                        DisposeCallback = newBuffer.Dispose
                    };

                    if (cfg.RowGetColorIndex == null)
                        cfg.RowGetColorIndexLegacy = getColorIndex;
                    if (cfg.RowSetColorIndex == null)
                        cfg.RowSetColorIndexLegacy = setColorIndex;

                    return BitmapDataFactory.CreateManagedCustomBitmapData(newBuffer, size.Width, cfg);
                };
            }
        }

        #endregion

        #region Constructors

        public ManagedCustomBitmapDataIndexed(Array2D<T> buffer, in BitmapDataConfig cfg, CustomIndexedBitmapDataConfig customConfig)
            : base(buffer, cfg)
        {
            Debug.Assert(cfg.PixelFormat.Indexed);

            rowGetColorIndex = customConfig.GetRowGetColorIndex<T>();
            rowSetColorIndex = customConfig.GetRowSetColorIndex<T>();
            CanReadWrite = customConfig.CanRead() && customConfig.CanWrite();
            BackBufferIndependentPixelAccess = customConfig.BackBufferIndependentPixelAccess;
        }

        #endregion

        #region Methods

        #region Protected Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override int DoGetColorIndex(int x, int y) => GetRowCached(y).DoGetColorIndex(x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        protected override void DoSetColorIndex(int x, int y, int colorIndex) => GetRowCached(y).DoSetColorIndex(x, colorIndex);

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            rowGetColorIndex = null!;
            rowSetColorIndex = null!;
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}
