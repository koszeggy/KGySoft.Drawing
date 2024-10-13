#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataWrapper.cs
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

using System.Drawing;
using System.Runtime.CompilerServices;
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides a wrapper for custom <see cref="IBitmapData"/> implementations that do not implement <see cref="IBitmapDataInternal"/>.
    /// </summary>
    internal sealed class BitmapDataWrapper : IBitmapDataInternal
    {
        #region BitmapDataRowWrapper class

        private sealed class BitmapDataRowWrapper : IBitmapDataRowInternal
        {
            #region Fields

            private readonly IBitmapDataRowMovable row;
            private readonly IReadableBitmapDataRow readableBitmapDataRow = null!;
            private readonly IWritableBitmapDataRow writableBitmapDataRow = null!;

            #endregion

            #region Properties and Indexers

            #region Properties

            public IBitmapData BitmapData { get; }
            public int Width => row.Width;
            public int Size => row.Size;
            public int Index => row.Index;

            #endregion

            #region Indexers

            public Color32 this[int x]
            {
                get => readableBitmapDataRow[x];
                set => writableBitmapDataRow[x] = value;
            }

            #endregion

            #endregion

            #region Constructors

            internal BitmapDataRowWrapper(BitmapDataWrapper parent, IBitmapDataRowMovable row, bool isReading, bool isWriting)
            {
                this.row = row;
                BitmapData = parent;
                if (isReading)
                    readableBitmapDataRow = (IReadableBitmapDataRow)row;
                if (isWriting)
                    writableBitmapDataRow = (IWritableBitmapDataRow)row;
            }

            #endregion

            #region Methods

            public bool MoveNextRow() => row.MoveNextRow();
            public void MoveToRow(int y) => row.MoveToRow(y);
            public Color GetColor(int x) => readableBitmapDataRow.GetColor(x);
            public void SetColor(int x, Color color) => writableBitmapDataRow.SetColor(x, color);
            public Color32 GetColor32(int x) => readableBitmapDataRow.GetColor32(x);
            public void SetColor32(int x, Color32 color) => writableBitmapDataRow.SetColor32(x, color);
            public PColor32 GetPColor32(int x) => readableBitmapDataRow.GetPColor32(x);
            public void SetPColor32(int x, PColor32 color) => writableBitmapDataRow.SetPColor32(x, color);
            public Color64 GetColor64(int x) => readableBitmapDataRow.GetColor64(x);
            public void SetColor64(int x, Color64 color) => writableBitmapDataRow.SetColor64(x, color);
            public PColor64 GetPColor64(int x) => readableBitmapDataRow.GetPColor64(x);
            public void SetPColor64(int x, PColor64 color) => writableBitmapDataRow.SetPColor64(x, color);
            public ColorF GetColorF(int x) => readableBitmapDataRow.GetColorF(x);
            public void SetColorF(int x, ColorF color) => writableBitmapDataRow.SetColorF(x, color);
            public PColorF GetPColorF(int x) => readableBitmapDataRow.GetPColorF(x);
            public void SetPColorF(int x, PColorF color) => writableBitmapDataRow.SetPColorF(x, color);
            public int GetColorIndex(int x) => readableBitmapDataRow.GetColorIndex(x);
            public void SetColorIndex(int x, int colorIndex) => writableBitmapDataRow.SetColorIndex(x, colorIndex);
            public T ReadRaw<T>(int x) where T : unmanaged => readableBitmapDataRow.ReadRaw<T>(x);
            public void WriteRaw<T>(int x, T data) where T : unmanaged => writableBitmapDataRow.WriteRaw(x, data);

            public void DoMoveToRow(int y) => row.MoveToRow(y);
            public Color32 DoGetColor32(int x) => readableBitmapDataRow.GetColor32(x);
            public void DoSetColor32(int x, Color32 c) => writableBitmapDataRow.SetColor32(x, c);
            public PColor32 DoGetPColor32(int x) => readableBitmapDataRow.GetPColor32(x);
            public void DoSetPColor32(int x, PColor32 c) => writableBitmapDataRow.SetPColor32(x, c);
            public Color64 DoGetColor64(int x) => readableBitmapDataRow.GetColor64(x);
            public void DoSetColor64(int x, Color64 c) => writableBitmapDataRow.SetColor64(x, c);
            public PColor64 DoGetPColor64(int x) => readableBitmapDataRow.GetPColor64(x);
            public void DoSetPColor64(int x, PColor64 c) => writableBitmapDataRow.SetPColor64(x, c);
            public ColorF DoGetColorF(int x) => readableBitmapDataRow.GetColorF(x);
            public void DoSetColorF(int x, ColorF c) => writableBitmapDataRow.SetColorF(x, c);
            public PColorF DoGetPColorF(int x) => readableBitmapDataRow.GetPColorF(x);
            public void DoSetPColorF(int x, PColorF c) => writableBitmapDataRow.SetPColorF(x, c);
            public int DoGetColorIndex(int x) => readableBitmapDataRow.GetColorIndex(x);
            public void DoSetColorIndex(int x, int colorIndex) => writableBitmapDataRow.SetColorIndex(x, colorIndex);
            [SecurityCritical]public T DoReadRaw<T>(int x) where T : unmanaged => readableBitmapDataRow.ReadRaw<T>(x);
            [SecurityCritical]public void DoWriteRaw<T>(int x, T data) where T : unmanaged => writableBitmapDataRow.WriteRaw(x, data);

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool isReading;
        private readonly bool isWriting;

        private volatile StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>?[]? cachedRows;
        private int hashMask; // non-volatile because always the volatile cachedRowByThreadId is accessed first

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height => BitmapData.Height;
        public int Width => BitmapData.Width;
        public Size Size => BitmapData.Size;
        public PixelFormatInfo PixelFormat => BitmapData.PixelFormat;
        public Palette? Palette => BitmapData.Palette;
        public int RowSize => BitmapData.RowSize;
        public Color32 BackColor => BitmapData.BackColor;
        public byte AlphaThreshold => BitmapData.AlphaThreshold;
        public bool IsDisposed => BitmapData.IsDisposed;
        public bool CanSetPalette => false;
        public bool IsCustomPixelFormat => BitmapData.PixelFormat.IsCustomFormat;
        public WorkingColorSpace WorkingColorSpace => BitmapData.WorkingColorSpace;

        #endregion

        #region Internal Properties

        internal IBitmapData BitmapData { get; }

        #endregion

        #region Private Properties

        private IReadableBitmapData AsReadable => (IReadableBitmapData)BitmapData;
        private IWritableBitmapData AsWritable => (IWritableBitmapData)BitmapData;
        private IReadWriteBitmapData AsReadWrite => (IReadWriteBitmapData)BitmapData;

        #endregion

        #region Explicitly Implemented Interface Properties

        IReadableBitmapDataRowMovable IReadableBitmapData.FirstRow => AsReadable.FirstRow;
        IWritableBitmapDataRowMovable IWritableBitmapData.FirstRow => AsWritable.FirstRow;
        IReadWriteBitmapDataRowMovable IReadWriteBitmapData.FirstRow => AsReadWrite.FirstRow;

        #endregion

        #endregion

        #region Indexers

        IReadableBitmapDataRow IReadableBitmapData.this[int y] => AsReadable[y];
        IWritableBitmapDataRow IWritableBitmapData.this[int y] => AsWritable[y];
        IReadWriteBitmapDataRow IReadWriteBitmapData.this[int y] => AsReadWrite[y];

        #endregion

        #endregion

        #region Constructors

        internal BitmapDataWrapper(IBitmapData bitmapData, bool isReading, bool isWriting)
        {
            Debug.Assert(bitmapData is not IBitmapDataInternal, "No wrapping is needed");

            BitmapData = bitmapData;
            this.isReading = isReading;
            this.isWriting = isWriting;
        }

        #endregion

        #region Methods
        
        #region Public Methods

        public Color GetPixel(int x, int y) => AsReadable.GetPixel(x, y);
        public void SetPixel(int x, int y, Color color) => AsWritable.SetPixel(x, y, color);
        public Color32 GetColor32(int x, int y) => AsReadable.GetColor32(x, y);
        public void SetColor32(int x, int y, Color32 color) => AsWritable.SetColor32(x, y, color);
        public PColor32 GetPColor32(int x, int y) => AsReadable.GetPColor32(x, y);
        public void SetPColor32(int x, int y, PColor32 color) => AsWritable.SetPColor32(x, y, color);
        public Color64 GetColor64(int x, int y) => AsReadable.GetColor64(x, y);
        public void SetColor64(int x, int y, Color64 color) => AsWritable.SetColor64(x, y, color);
        public PColor64 GetPColor64(int x, int y) => AsReadable.GetPColor64(x, y);
        public void SetPColor64(int x, int y, PColor64 color) => AsWritable.SetPColor64(x, y, color);
        public ColorF GetColorF(int x, int y) => AsReadable.GetColorF(x, y);
        public void SetColorF(int x, int y, ColorF color) => AsWritable.SetColorF(x, y, color);
        public PColorF GetPColorF(int x, int y) => AsReadable.GetPColorF(x, y);
        public void SetPColorF(int x, int y, PColorF color) => AsWritable.SetPColorF(x, y, color);
        public T ReadRaw<T>(int x, int y) where T : unmanaged => AsReadable.ReadRaw<T>(x, y);
        public void WriteRaw<T>(int x, int y, T data) where T : unmanaged => AsWritable.WriteRaw(x, y, data);
        public int GetColorIndex(int x, int y) => AsReadable.GetColorIndex(x, y);
        public void SetColorIndex(int x, int y, int colorIndex) => AsWritable.SetColorIndex(x, y, colorIndex);

        public Color32 DoGetColor32(int x, int y) => AsReadable.GetColor32(x, y);
        public void DoSetColor32(int x, int y, Color32 color) => AsWritable.SetColor32(x, y, color);
        public PColor32 DoGetPColor32(int x, int y) => AsReadable.GetPColor32(x, y);
        public void DoSetPColor32(int x, int y, PColor32 color) => AsWritable.SetPColor32(x, y, color);
        public Color64 DoGetColor64(int x, int y) => AsReadable.GetColor64(x, y);
        public void DoSetColor64(int x, int y, Color64 color) => AsWritable.SetColor64(x, y, color);
        public PColor64 DoGetPColor64(int x, int y) => AsReadable.GetPColor64(x, y);
        public void DoSetPColor64(int x, int y, PColor64 color) => AsWritable.SetPColor64(x, y, color);
        public ColorF DoGetColorF(int x, int y) => AsReadable.GetColorF(x, y);
        public void DoSetColorF(int x, int y, ColorF color) => AsWritable.SetColorF(x, y, color);
        public PColorF DoGetPColorF(int x, int y) => AsReadable.GetPColorF(x, y);
        public void DoSetPColorF(int x, int y, PColorF color) => AsWritable.SetPColorF(x, y, color);
        [SecurityCritical]public T DoReadRaw<T>(int x, int y) where T : unmanaged => AsReadable.ReadRaw<T>(x, y);
        [SecurityCritical]public void DoWriteRaw<T>(int x, int y, T data) where T : unmanaged => AsWritable.WriteRaw(x, y, data);
        public int DoGetColorIndex(int x, int y) => AsReadable.GetColorIndex(x, y);
        public void DoSetColorIndex(int x, int y, int colorIndex) => AsWritable.SetColorIndex(x, y, colorIndex);

        public bool TrySetPalette(Palette? palette) => false;
        public IBitmapDataRowInternal GetRowUncached(int y) => DoGetRow(y);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public IBitmapDataRowInternal GetRowCached(int y)
        {
            if (cachedRows == null)
                InitThreadIdCache();
            int threadId = EnvironmentHelper.CurrentThreadId;
            var hash = threadId & hashMask;
            StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>? cached = cachedRows![hash];
            if (cached?.Value.ThreadId == threadId)
                cached.Value.Row.DoMoveToRow(y);
            else
                cachedRows[hash] = cached = new StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>((threadId, DoGetRow(y)));
            return cached.Value.Row;
        }

        public void Dispose()
        {
            // not disposing the wrapped instance here, which is intended
        }

        #endregion

        #region Private Methods
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitThreadIdCache()
        {
            var result = new StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>[EnvironmentHelper.GetThreadBasedCacheSize()];
            hashMask = result.Length - 1;
            cachedRows = result;
        }

        private IBitmapDataRowInternal DoGetRow(int y)
            => new BitmapDataRowWrapper(this, isReading ? AsReadable.GetMovableRow(y) : AsWritable.GetMovableRow(y), isReading, isWriting);

        #endregion

        #region Explicitly Implemented Interface Methods

        IReadableBitmapDataRowMovable IReadableBitmapData.GetMovableRow(int y) => AsReadable.GetMovableRow(y);
        IWritableBitmapDataRowMovable IWritableBitmapData.GetMovableRow(int y) => AsWritable.GetMovableRow(y);
        IReadWriteBitmapDataRowMovable IReadWriteBitmapData.GetMovableRow(int y) => AsReadWrite.GetMovableRow(y);

        #endregion

        #endregion
    }
}
