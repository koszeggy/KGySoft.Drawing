#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataWrapper.cs
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

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal class BitmapDataWrapper : IBitmapDataInternal
    {
        #region Fields

        private readonly IBitmapData bitmapData;
        private readonly IReadableBitmapData readableBitmapData;
        private readonly IWritableBitmapData writableBitmapData;
        private readonly bool isReading;

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height => bitmapData.Height;
        public int Width => bitmapData.Width;
        public PixelFormat PixelFormat => bitmapData.PixelFormat;
        public Palette Palette => bitmapData.Palette;
        public int RowSize => bitmapData.RowSize;
        public Color32 BackColor => bitmapData.BackColor;
        public byte AlphaThreshold => bitmapData.AlphaThreshold;

        #endregion

        #region Explicitly Implemented Interface Properties

        IReadableBitmapDataRow IReadableBitmapData.FirstRow => readableBitmapData.FirstRow;
        IWritableBitmapDataRow IWritableBitmapData.FirstRow => writableBitmapData.FirstRow;
        IReadWriteBitmapDataRow IReadWriteBitmapData.FirstRow => default;

        #endregion

        #endregion

        #region Indexers

        IReadableBitmapDataRow IReadableBitmapData.this[int y] => readableBitmapData[y];
        IWritableBitmapDataRow IWritableBitmapData.this[int y] => writableBitmapData[y];
        IReadWriteBitmapDataRow IReadWriteBitmapData.this[int y] => default;

        #endregion

        #endregion

        #region Constructors

        internal BitmapDataWrapper(IBitmapData bitmapData, bool isReading)
        {
            Debug.Assert(!(bitmapData is IBitmapDataInternal), "No wrapping is needed");
            this.bitmapData = bitmapData;
            this.isReading = isReading;
            if (isReading)
                readableBitmapData = (IReadableBitmapData)bitmapData;
            else
                writableBitmapData = (IWritableBitmapData)bitmapData;
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // not disposing the wrapped instance here, which is intended
        }

        public Color GetPixel(int x, int y) => readableBitmapData.GetPixel(x, y);
        public void SetPixel(int x, int y, Color color) => writableBitmapData.SetPixel(x, y, color);

        public IBitmapDataRowInternal GetRow(int y) => isReading
            ? new BitmapDataRowWrapper(readableBitmapData[y], true)
            : new BitmapDataRowWrapper(writableBitmapData[y], false);

        #endregion
    }
}
