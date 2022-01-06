#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataWrapper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;

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

            private readonly IBitmapDataRow row;
            [AllowNull]private readonly IReadableBitmapDataRow readableBitmapDataRow;
            [AllowNull]private readonly IWritableBitmapDataRow writableBitmapDataRow;

            #endregion

            #region Properties and Indexers

            #region Properties

            public int Index => row.Index;
            public int Width => row.Width;
            public int Size => row.Size;

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

            internal BitmapDataRowWrapper(IBitmapDataRow row, bool isReading, bool isWriting)
            {
                this.row = row;
                if (isReading)
                    readableBitmapDataRow = (IReadableBitmapDataRow)row;
                if (isWriting)
                    writableBitmapDataRow = (IWritableBitmapDataRow)row;
            }

            #endregion

            #region Methods

            public bool MoveNextRow() => row.MoveNextRow();
            public Color GetColor(int x) => readableBitmapDataRow.GetColor(x);
            public int GetColorIndex(int x) => readableBitmapDataRow.GetColorIndex(x);
            public T ReadRaw<T>(int x) where T : unmanaged => readableBitmapDataRow.ReadRaw<T>(x);
            public void SetColor(int x, Color color) => writableBitmapDataRow.SetColor(x, color);
            public void SetColorIndex(int x, int colorIndex) => writableBitmapDataRow.SetColorIndex(x, colorIndex);
            public void WriteRaw<T>(int x, T data) where T : unmanaged => writableBitmapDataRow.WriteRaw(x, data);
            public Color32 DoGetColor32(int x) => readableBitmapDataRow[x];
            public void DoSetColor32(int x, Color32 c) => writableBitmapDataRow[x] = c;
            public T DoReadRaw<T>(int x) where T : unmanaged => readableBitmapDataRow.ReadRaw<T>(x);
            public void DoWriteRaw<T>(int x, T data) where T : unmanaged => writableBitmapDataRow.WriteRaw(x, data);
            public int DoGetColorIndex(int x) => readableBitmapDataRow.GetColorIndex(x);
            public void DoSetColorIndex(int x, int colorIndex) => writableBitmapDataRow.SetColorIndex(x, colorIndex);
            public Color32 DoGetColor32Premultiplied(int x) => DoGetColor32(x).ToPremultiplied();
            public void DoSetColor32Premultiplied(int x, Color32 c) => DoSetColor32(x, c.ToStraight());

            #endregion
        }

        #endregion

        #region Fields

        private readonly bool isReading;
        private readonly bool isWriting;

        private IBitmapDataRowInternal? lastRow;

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height => BitmapData.Height;
        public int Width => BitmapData.Width;
        public PixelFormat PixelFormat => BitmapData.PixelFormat;
        public Palette? Palette => BitmapData.Palette;
        public int RowSize => BitmapData.RowSize;
        public Color32 BackColor => BitmapData.BackColor;
        public byte AlphaThreshold => BitmapData.AlphaThreshold;
        public bool IsDisposed => BitmapData.IsDisposed;
        public bool CanSetPalette => false;
        public bool IsCustomPixelFormat => !BitmapData.PixelFormat.IsValidFormat();

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

        IReadableBitmapDataRow IReadableBitmapData.FirstRow => AsReadable.FirstRow;
        IWritableBitmapDataRow IWritableBitmapData.FirstRow => AsWritable.FirstRow;
        IReadWriteBitmapDataRow IReadWriteBitmapData.FirstRow => AsReadWrite.FirstRow;

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

        public void Dispose()
        {
            // not disposing the wrapped instance here, which is intended
        }

        public Color GetPixel(int x, int y) => AsReadable.GetPixel(x, y);
        public void SetPixel(int x, int y, Color color) => AsWritable.SetPixel(x, y, color);

        public IBitmapDataRowInternal DoGetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            IBitmapDataRowInternal? result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new BitmapDataRowWrapper(isReading ? AsReadable[y] : AsWritable[y], isReading, isWriting);
        }

        public bool TrySetPalette(Palette? palette) => false;

        #endregion
    }
}
