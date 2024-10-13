#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedCustomBitmapData.cs
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
    internal sealed class UnmanagedCustomBitmapData : UnmanagedBitmapDataBase<UnmanagedCustomBitmapData.Row>, ICustomBitmapData
    {
        #region Row class

        internal sealed class Row : UnmanagedBitmapDataRowBase, ICustomBitmapDataRow
        {
            #region Properties
            
            IBitmapData ICustomBitmapDataRow.BitmapData => BitmapData;

            #endregion

            #region Methods

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color32 DoGetColor32(int x) => ((UnmanagedCustomBitmapData)BitmapData).rowGetColor32.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor32(int x, Color32 c) => ((UnmanagedCustomBitmapData)BitmapData).rowSetColor32.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor32 DoGetPColor32(int x) => ((UnmanagedCustomBitmapData)BitmapData).rowGetPColor32.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor32(int x, PColor32 c) => ((UnmanagedCustomBitmapData)BitmapData).rowSetPColor32.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override Color64 DoGetColor64(int x) => ((UnmanagedCustomBitmapData)BitmapData).rowGetColor64.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColor64(int x, Color64 c) => ((UnmanagedCustomBitmapData)BitmapData).rowSetColor64.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColor64 DoGetPColor64(int x) => ((UnmanagedCustomBitmapData)BitmapData).rowGetPColor64.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColor64(int x, PColor64 c) => ((UnmanagedCustomBitmapData)BitmapData).rowSetPColor64.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override ColorF DoGetColorF(int x) => ((UnmanagedCustomBitmapData)BitmapData).rowGetColorF.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetColorF(int x, ColorF c) => ((UnmanagedCustomBitmapData)BitmapData).rowSetColorF.Invoke(this, x, c);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override PColorF DoGetPColorF(int x) => ((UnmanagedCustomBitmapData)BitmapData).rowGetPColorF.Invoke(this, x);

            [MethodImpl(MethodImpl.AggressiveInlining)]
            public override void DoSetPColorF(int x, PColorF c) => ((UnmanagedCustomBitmapData)BitmapData).rowSetPColorF.Invoke(this, x, c);

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

        private Func<ICustomBitmapDataRow, int, Color32> rowGetColor32;
        private Action<ICustomBitmapDataRow, int, Color32> rowSetColor32;
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

        public Func<Size, WorkingColorSpace, IBitmapDataInternal> CreateCompatibleBitmapDataFactory
        {
            get
            {
                Debug.Assert(BackBufferIndependentPixelAccess);
                if (IsDisposed)
                    ThrowDisposed();

                // Creating locals for all used members so self reference will not be captured.
                Func<ICustomBitmapDataRow, int, Color32> getColor32 = rowGetColor32;
                Func<ICustomBitmapDataRow, int, PColor32> getPColor32 = rowGetPColor32;
                Func<ICustomBitmapDataRow, int, Color64> getColor64 = rowGetColor64;
                Func<ICustomBitmapDataRow, int, PColor64> getPColor64 = rowGetPColor64;
                Func<ICustomBitmapDataRow, int, ColorF> getColorF = rowGetColorF;
                Func<ICustomBitmapDataRow, int, PColorF> getPColorF = rowGetPColorF;
                Action<ICustomBitmapDataRow, int, Color32> setColor32 = rowSetColor32;
                Action<ICustomBitmapDataRow, int, PColor32> setPColor32 = rowSetPColor32;
                Action<ICustomBitmapDataRow, int, Color64> setColor64 = rowSetColor64;
                Action<ICustomBitmapDataRow, int, PColor64> setPColor64 = rowSetPColor64;
                Action<ICustomBitmapDataRow, int, ColorF> setColorF = rowSetColorF;
                Action<ICustomBitmapDataRow, int, PColorF> setPColorF = rowSetPColorF;
                Color32 backColor = BackColor; 
                byte alphaThreshold = AlphaThreshold;
                PixelFormatInfo pixelFormat = PixelFormat;
                int origWidth = Width;
                int origStride = RowSize;
                return (size, workingColorSpace) =>
                {
                    Debug.Assert(size.Width > 0 && size.Height > 0);
                    Array2D<byte> newBuffer;

                    // original width: the original stride must be alright
                    if (size.Width == origWidth)
                        newBuffer = new Array2D<byte>(size.Height, origStride);
                    else
                    {
                        // new width: assuming at least 16 byte units for custom ICustomBitmapDataRow casts
                        int stride = pixelFormat.GetByteWidth(size.Width);
                        stride += 16 - stride % 16;
                        newBuffer = new Array2D<byte>(size.Height, stride);
                    }

                    var cfg = new CustomBitmapDataConfig
                    {
                        PixelFormat = pixelFormat,
                        BackColor = backColor,
                        AlphaThreshold = alphaThreshold,
                        WorkingColorSpace = workingColorSpace,
                        RowGetColor32 = getColor32,
                        RowGetPColor32 = getPColor32,
                        RowGetColor64 = getColor64,
                        RowGetPColor64 = getPColor64,
                        RowGetColorF = getColorF,
                        RowGetPColorF = getPColorF,
                        RowSetColor32 = setColor32,
                        RowSetPColor32 = setPColor32,
                        RowSetColor64 = setColor64,
                        RowSetPColor64 = setPColor64,
                        RowSetColorF = setColorF,
                        RowSetPColorF = setPColorF,
                        DisposeCallback = newBuffer.Dispose
                    };

                    return BitmapDataFactory.CreateManagedCustomBitmapData(newBuffer, size.Width, cfg);
                };
            }
        }

        #endregion

        #region Constructors

        internal UnmanagedCustomBitmapData(IntPtr buffer, int stride, in BitmapDataConfig cfg, CustomBitmapDataConfig customConfig)
            : base(buffer, stride, cfg)
        {
            Debug.Assert(!cfg.PixelFormat.Indexed);

            rowGetColor32 = customConfig.GetRowGetColor32();
            rowSetColor32 = customConfig.GetRowSetColor32();
            rowGetPColor32 = customConfig.GetRowGetPColor32();
            rowSetPColor32 = customConfig.GetRowSetPColor32();
            rowGetColor64 = customConfig.GetRowGetColor64();
            rowSetColor64 = customConfig.GetRowSetColor64();
            rowGetPColor64 = customConfig.GetRowGetPColor64();
            rowSetPColor64 = customConfig.GetRowSetPColor64();
            rowGetColorF = customConfig.GetRowGetColorF();
            rowSetColorF = customConfig.GetRowSetColorF();
            rowGetPColorF = customConfig.GetRowGetPColorF();
            rowSetPColorF = customConfig.GetRowSetPColorF();
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
