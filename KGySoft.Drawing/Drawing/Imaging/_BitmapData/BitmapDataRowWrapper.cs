#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowWrapper.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class BitmapDataRowWrapper : IBitmapDataRowInternal
    {
        #region Fields

        private readonly IBitmapDataRow row;
        private readonly IReadableBitmapDataRow readableBitmapDataRow;
        private readonly IWritableBitmapDataRow writableBitmapDataRow;

        #endregion

        #region Properties and Indexers

        #region Properties

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

        #endregion
    }
}
