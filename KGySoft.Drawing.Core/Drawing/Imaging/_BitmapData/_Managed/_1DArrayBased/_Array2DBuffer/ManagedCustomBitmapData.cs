#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedCustomBitmapData.cs
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
    /// Represents a managed bitmap data wrapper with custom pixel format for a 1D array (wrapped into an <see cref="Array2D{T}"/>).
    /// </summary>
    internal sealed class ManagedCustomBitmapData<T> : ManagedBitmapDataArray2DBase<T, ManagedCustomBitmapData<T>.Row>, ICustomBitmapData
        where T : unmanaged
    {
        #region Row class

        internal sealed class Row : ManagedBitmapDataArraySectionRowBase<T>, ICustomBitmapDataRow<T>
        {
            #region Properties and Indexers

            #region Properties

            IBitmapData ICustomBitmapDataRow.BitmapData => BitmapData;

            #endregion

            #region Indexers

            ref T ICustomBitmapDataRow<T>.this[int index] => ref Row.GetElementReference(index);

            #endregion

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => ((ManagedCustomBitmapData<T>)BitmapData).rowGetColor32.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => ((ManagedCustomBitmapData<T>)BitmapData).rowSetColor32.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor32 DoGetPColor32(int x) => ((ManagedCustomBitmapData<T>)BitmapData).rowGetPColor32.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor32(int x, PColor32 c) => ((ManagedCustomBitmapData<T>)BitmapData).rowSetPColor32.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => ((ManagedCustomBitmapData<T>)BitmapData).rowGetColor64.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => ((ManagedCustomBitmapData<T>)BitmapData).rowSetColor64.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => ((ManagedCustomBitmapData<T>)BitmapData).rowGetPColor64.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => ((ManagedCustomBitmapData<T>)BitmapData).rowSetPColor64.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => ((ManagedCustomBitmapData<T>)BitmapData).rowGetColorF.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => ((ManagedCustomBitmapData<T>)BitmapData).rowSetColorF.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => ((ManagedCustomBitmapData<T>)BitmapData).rowGetPColorF.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => ((ManagedCustomBitmapData<T>)BitmapData).rowSetPColorF.Invoke(this, x, c);

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

        private Func<ICustomBitmapDataRow<T>, int, Color32> rowGetColor32;
        private Action<ICustomBitmapDataRow<T>, int, Color32> rowSetColor32;
        private Func<ICustomBitmapDataRow, int, PColor32> rowGetPColor32;
        private Action<ICustomBitmapDataRow, int, PColor32> rowSetPColor32;
        private Func<ICustomBitmapDataRow, int, Color64> rowGetColor64;
        private Action<ICustomBitmapDataRow, int, Color64> rowSetColor64;
        private Func<ICustomBitmapDataRow, int, PColor64> rowGetPColor64;
        private Action<ICustomBitmapDataRow, int, PColor64> rowSetPColor64;
        private Func<ICustomBitmapDataRow, int, ColorF> rowGetColorF;
        private Action<ICustomBitmapDataRow, int, ColorF> rowSetColorF;
        private Func<ICustomBitmapDataRow, int, PColorF> rowGetPColorF;
        private Action<ICustomBitmapDataRow, int, PColorF> rowSetPColorF;

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
                Func<ICustomBitmapDataRow<T>, int, Color32> getColor32 = rowGetColor32;
                Func<ICustomBitmapDataRow, int, PColor32> getPColor32 = rowGetPColor32;
                Func<ICustomBitmapDataRow, int, Color64> getColor64 = rowGetColor64;
                Func<ICustomBitmapDataRow, int, PColor64> getPColor64 = rowGetPColor64;
                Func<ICustomBitmapDataRow, int, ColorF> getColorF = rowGetColorF;
                Func<ICustomBitmapDataRow, int, PColorF> getPColorF = rowGetPColorF;
                Action<ICustomBitmapDataRow<T>, int, Color32> setColor32 = rowSetColor32;
                Action<ICustomBitmapDataRow, int, PColor32> setPColor32 = rowSetPColor32;
                Action<ICustomBitmapDataRow, int, Color64> setColor64 = rowSetColor64;
                Action<ICustomBitmapDataRow, int, PColor64> setPColor64 = rowSetPColor64;
                Action<ICustomBitmapDataRow, int, ColorF> setColorF = rowSetColorF;
                Action<ICustomBitmapDataRow, int, PColorF> setPColorF = rowSetPColorF;
                Color32 backColor = BackColor;
                byte alphaThreshold = AlphaThreshold;
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

                    var cfg = new CustomBitmapDataConfig
                    {
                        PixelFormat = pixelFormat,
                        BackColor = backColor,
                        AlphaThreshold = alphaThreshold,
                        WorkingColorSpace = workingColorSpace,
                        RowGetColor32 = getColor32 as Func<ICustomBitmapDataRow, int, Color32>,
                        RowGetPColor32 = getPColor32,
                        RowGetColor64 = getColor64,
                        RowGetPColor64 = getPColor64,
                        RowGetColorF = getColorF,
                        RowGetPColorF = getPColorF,
                        RowSetColor32 = setColor32 as Action<ICustomBitmapDataRow, int, Color32>,
                        RowSetPColor32 = setPColor32,
                        RowSetColor64 = setColor64,
                        RowSetPColor64 = setPColor64,
                        RowSetColorF = setColorF,
                        RowSetPColorF = setPColorF,
                        DisposeCallback = newBuffer.Dispose
                    };

                    if (cfg.RowGetColor32 == null)
                        cfg.RowGetColorLegacy = getColor32;
                    if (cfg.RowSetColor32 == null)
                        cfg.RowSetColorLegacy = setColor32;

                    return BitmapDataFactory.CreateManagedCustomBitmapData(newBuffer, size.Width, cfg);
                };
            }
        }

        #endregion

        #region Constructors

        public ManagedCustomBitmapData(Array2D<T> buffer, in BitmapDataConfig cfg, CustomBitmapDataConfig customConfig)
            : base(buffer, cfg)
        {
            Debug.Assert(!cfg.PixelFormat.Indexed);

            rowGetColor32 = customConfig.GetRowGetColor32<T>();
            rowSetColor32 = customConfig.GetRowSetColor32<T>();
            rowGetPColor32 = customConfig.GetRowGetPColor32<T>();
            rowSetPColor32 = customConfig.GetRowSetPColor32<T>();
            rowGetColor64 = customConfig.GetRowGetColor64<T>();
            rowSetColor64 = customConfig.GetRowSetColor64<T>();
            rowGetPColor64 = customConfig.GetRowGetPColor64<T>();
            rowSetPColor64 = customConfig.GetRowSetPColor64<T>();
            rowGetColorF = customConfig.GetRowGetColorF<T>();
            rowSetColorF = customConfig.GetRowSetColorF<T>();
            rowGetPColorF = customConfig.GetRowGetPColorF<T>();
            rowSetPColorF = customConfig.GetRowSetPColorF<T>();
            CanReadWrite = customConfig.CanRead() && customConfig.CanWrite();
            BackBufferIndependentPixelAccess = customConfig.BackBufferIndependentPixelAccess;
        }

        #endregion

        #region Methods

        #region Public Methods
        
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color32 DoGetColor32(int x, int y) => GetRowCached(y).DoGetColor32(x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor32(int x, int y, Color32 c) => GetRowCached(y).DoSetColor32(x, c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor32 DoGetPColor32(int x, int y) => GetRowCached(y).DoGetPColor32(x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor32(int x, int y, PColor32 c) => GetRowCached(y).DoSetPColor32(x, c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override Color64 DoGetColor64(int x, int y) => GetRowCached(y).DoGetColor64(x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColor64(int x, int y, Color64 c) => GetRowCached(y).DoSetColor64(x, c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColor64 DoGetPColor64(int x, int y) => GetRowCached(y).DoGetPColor64(x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColor64(int x, int y, PColor64 c) => GetRowCached(y).DoSetPColor64(x, c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override ColorF DoGetColorF(int x, int y) => GetRowCached(y).DoGetColorF(x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetColorF(int x, int y, ColorF c) => GetRowCached(y).DoSetColorF(x, c);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override PColorF DoGetPColorF(int x, int y) => GetRowCached(y).DoGetPColorF(x);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override void DoSetPColorF(int x, int y, PColorF c) => GetRowCached(y).DoSetPColorF(x, c);

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            rowGetColor32 = null!;
            rowSetColor32 = null!;
            rowGetPColor32 = null!;
            rowSetPColor32 = null!;
            rowGetColor64 = null!;
            rowSetColor64 = null!;
            rowGetPColor64 = null!;
            rowSetPColor64 = null!;
            rowGetColorF = null!;
            rowSetColorF = null!;
            rowGetPColorF = null!;
            rowSetPColorF = null!;
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}
