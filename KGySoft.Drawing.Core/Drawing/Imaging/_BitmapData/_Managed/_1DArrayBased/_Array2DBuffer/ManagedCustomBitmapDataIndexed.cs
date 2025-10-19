#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedCustomBitmapDataIndexed.cs
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
    internal sealed class ManagedCustomBitmapDataIndexed<T> : ManagedBitmapDataArray2DIndexedBase<T, ManagedCustomBitmapDataIndexed<T>.Row>, ICustomBitmapData
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataArraySectionRowIndexedBase<T>, ICustomBitmapDataRow<T>
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

            ref T ICustomBitmapDataRow<T>.this[int index]
            {
                [SecuritySafeCritical]
                get => ref Row.GetElementReference(index);
            }

            #endregion

            #endregion

            #region Methods

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ((ManagedCustomBitmapDataIndexed<T>)BitmapData).rowGetColorIndex.Invoke(this, x);

            [SecurityCritical]
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
            public unsafe ref TValue UnsafeGetRefAs<TValue>(int x) where TValue : unmanaged
            {
                Debug.Assert(!typeof(TValue).IsPrimitive || Row.GetPinnableReference().At<T, TValue>(x).AsIntPtr() % sizeof(TValue) == 0, $"Misaligned raw {typeof(TValue).Name} access in row {Index} at position {x} - {BitmapData.PixelFormat} {Width}x{BitmapData.Height}");
                return ref Row.GetPinnableReference().At<T, TValue>(x);
            }

            #endregion
        }

        #endregion

        #region Fields

        private Func<ICustomBitmapDataRow<T>, int, int> rowGetColorIndex;
        private Action<ICustomBitmapDataRow<T>, int, int> rowSetColorIndex;

        #endregion

        #region Properties
        
        #region Public Properties

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
                    bool autoAllocate = BitmapDataFactory.PoolingStrategy == ArrayPoolingStrategy.AnyElementType || typeof(T) == typeof(byte) && BitmapDataFactory.PoolingStrategy >= ArrayPoolingStrategy.IfByteArrayBased;

                    // original width: the original stride must be alright
                    if (size.Width == origWidth)
                    {
                        newBuffer = autoAllocate
                            ? new Array2D<T>(size.Height, origBufferWidth)
                            : new Array2D<T>(new T[size.Height * origBufferWidth], size.Height, origBufferWidth);
                    }
                    else
                    {
                        // new width: assuming at least 16 byte units for custom ICustomBitmapDataRow casts
                        int stride = pixelFormat.GetByteWidth(size.Width);
                        stride += 16 - stride % 16;
                        if (16 % sizeof(T) != 0)
                            stride += sizeof(T) - stride % sizeof(T);
                        newBuffer = autoAllocate
                            ? new Array2D<T>(size.Height, stride / sizeof(T))
                            : new Array2D<T>(new T[size.Height * stride / sizeof(T)], size.Height, stride / sizeof(T));
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

        #region Protected Properties

        protected override uint MaxIndex => (1u << PixelFormat.BitsPerPixel) - 1u;

        #endregion

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

        #region Public Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x, int y) => GetRowCached(y).DoGetColorIndex(x);

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorIndex(int x, int y, int colorIndex) => GetRowCached(y).DoSetColorIndex(x, colorIndex);

        #endregion

        #region Protected Methods

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
