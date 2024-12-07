#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedCustomBitmapDataIndexed.cs
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
    internal sealed class UnmanagedCustomBitmapDataIndexed : UnmanagedBitmapDataIndexedBase<UnmanagedCustomBitmapDataIndexed.Row>, ICustomBitmapData
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowIndexedBase, ICustomBitmapDataRow
        {
            #region Properties

            #region Protected Properties

            protected override uint MaxIndex => (1u << BitmapData.PixelFormat.BitsPerPixel) - 1u;

            #endregion

            #region Explicitly Implemented Interface Properties

            IBitmapData ICustomBitmapDataRow.BitmapData => BitmapData;

            #endregion

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override int DoGetColorIndex(int x) => ((UnmanagedCustomBitmapDataIndexed)BitmapData).rowGetColorIndex.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorIndex(int x, int colorIndex) => ((UnmanagedCustomBitmapDataIndexed)BitmapData).rowSetColorIndex.Invoke(this, x, colorIndex);

            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public unsafe ref T GetRefAs<T>(int x) where T : unmanaged
            {
                if ((x + 1) * sizeof(T) > BitmapData.RowSize)
                    ThrowXOutOfRange();
                return ref UnsafeGetRefAs<T>(x);
            }

            [SecurityCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            public unsafe ref T UnsafeGetRefAs<T>(int x) where T : unmanaged => ref ((T*)Row)[x];

            #endregion
        }

        #endregion

        #region Fields

        private Func<ICustomBitmapDataRow, int, int> rowGetColorIndex;
        private Action<ICustomBitmapDataRow, int, int> rowSetColorIndex;

        #endregion

        #region Properties

        #region Public Properties

        public override bool IsCustomPixelFormat => true;
        public bool CanReadWrite { get; }
        public bool BackBufferIndependentPixelAccess { get; }

        public Func<Size, WorkingColorSpace, IBitmapDataInternal> CreateCompatibleBitmapDataFactory
        {
            get
            {
                Debug.Assert(BackBufferIndependentPixelAccess);
                if (IsDisposed)
                    ThrowDisposed();

                // Creating locals for all used members so self reference will not be captured.
                Func<ICustomBitmapDataRow, int, int> getColorIndex = rowGetColorIndex;
                Action<ICustomBitmapDataRow, int, int> setColorIndex = rowSetColorIndex;
                Palette palette = Palette!;
                PixelFormatInfo pixelFormat = PixelFormat;
                int origWidth = Width;
                int origStride = RowSize;
                return (size, workingColorSpace) =>
                {
                    Debug.Assert(size.Width > 0 && size.Height > 0);
                    Array2D<byte> newBufferByte = default;
                    Array2D<ulong> newBufferUInt64 = default;
                    int stride = size.Width == origWidth ? origStride : pixelFormat.GetByteWidth(size.Width);

                    // new width: assuming at least 16 byte units for custom ICustomBitmapDataRow casts
                    if (size.Width != origWidth)
                        stride += 16 - stride % 16;

                    long byteLength = (long)stride * size.Height;
                    if (byteLength / sizeof(ulong) > EnvironmentHelper.MaxByteArrayLength)
                        throw new NotSupportedException(Res.ImagingUnmanagedBufferTooLarge);
                    bool useByteBuffer = byteLength <= EnvironmentHelper.MaxByteArrayLength;
                    bool autoAllocate = useByteBuffer && BitmapDataFactory.PoolingStrategy >= ArrayPoolingStrategy.IfByteArrayBased || BitmapDataFactory.PoolingStrategy == ArrayPoolingStrategy.AnyElementType;

                    if (useByteBuffer)
                    {
                        newBufferByte = autoAllocate
                            ? new Array2D<byte>(size.Height, stride)
                            : new Array2D<byte>(new byte[byteLength], size.Height, stride);
                    }
                    else
                    {
                        newBufferUInt64 = autoAllocate
                            ? new Array2D<ulong>(size.Height, stride / sizeof(ulong))
                            : new Array2D<ulong>(new ulong[byteLength / sizeof(ulong)], size.Height, stride / sizeof(ulong));
                    }

                    var cfg = new CustomIndexedBitmapDataConfig
                    {
                        PixelFormat = pixelFormat,
                        RowGetColorIndex = getColorIndex,
                        RowSetColorIndex = setColorIndex,
                        Palette = palette.WorkingColorSpace == workingColorSpace ? palette : new Palette(palette, workingColorSpace, palette.BackColor, palette.AlphaThreshold),
                        DisposeCallback = useByteBuffer ? newBufferByte.Dispose : newBufferUInt64.Dispose,
                    };

                    return useByteBuffer
                        ? BitmapDataFactory.CreateManagedCustomBitmapData(newBufferByte, size.Width, cfg)
                        : BitmapDataFactory.CreateManagedCustomBitmapData(newBufferUInt64, size.Width, cfg);
                };
            }
        }

        #endregion

        #region Protected Properties

        protected override uint MaxIndex => (1u << PixelFormat.BitsPerPixel) - 1u;

        #endregion

        #endregion

        #region Constructors

        [SecurityCritical]
        internal UnmanagedCustomBitmapDataIndexed(IntPtr buffer, int stride, in BitmapDataConfig cfg, CustomIndexedBitmapDataConfig customConfig)
            : base(buffer, stride, cfg)
        {
            Debug.Assert(cfg.PixelFormat.Indexed);

            rowGetColorIndex = customConfig.GetRowGetColorIndex();
            rowSetColorIndex = customConfig.GetRowSetColorIndex();
            CanReadWrite = customConfig.CanRead() && customConfig.CanWrite();
            BackBufferIndependentPixelAccess = customConfig.BackBufferIndependentPixelAccess;
        }

        #endregion

        #region Methods

        #region Public Methods
        
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override int DoGetColorIndex(int x, int y) => GetRowCached(y).DoGetColorIndex(x);

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
