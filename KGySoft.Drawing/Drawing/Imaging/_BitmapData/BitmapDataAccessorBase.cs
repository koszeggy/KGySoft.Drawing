#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataAccessorBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataAccessorBase : IReadWriteBitmapData
    {
        #region Fields

        private readonly Bitmap bitmap;
        private readonly int handle;

        private bool disposed;

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height { get; }

        public int Width { get; }

        public PixelFormat PixelFormat { get; }

        public int Stride { get; }

        public byte AlphaThreshold { get; }

        public Palette Palette { get; }

        #endregion

        #region Internal Properties

        internal Color32 BackColor { get; }

        #endregion

        #region Protected Internal Properties
        
        protected internal IntPtr Scan0 { get; }

        #endregion

        #region Explicitly Implemented Properties

        IReadableBitmapDataRow IReadableBitmapData.FirstRow => GetRow(0);
        IWritableBitmapDataRow IWritableBitmapData.FirstRow => GetRow(0);
        IReadWriteBitmapDataRow IReadWriteBitmapData.FirstRow => GetRow(0);

        Color IWritableBitmapData.BackColor => BackColor.ToColor();

        #endregion

        #endregion

        #region Indexers

        IReadWriteBitmapDataRow IReadWriteBitmapData.this[int y]
        {
            get
            {
                if ((uint)y >= Height)
                    ThrowYOutOfRange();
                return GetRow(y);
            }
        }

        IReadableBitmapDataRow IReadableBitmapData.this[int y] => ((IReadWriteBitmapData)this)[y];
        IWritableBitmapDataRow IWritableBitmapData.this[int y] => ((IReadWriteBitmapData)this)[y];

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        protected BitmapDataAccessorBase(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, Color32 backColor, byte alphaThreshold)
        {
            // Pixel format is passed only to avoid doubled retrieval but it must be the same as bitmap format.
            Debug.Assert(bitmap.PixelFormat == pixelFormat, "Unmatching pixel format");

            this.bitmap = bitmap;
            BackColor = backColor;
            AlphaThreshold = alphaThreshold;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), lockMode, pixelFormat);

            // Instead of single BitmapData property it is safer if we store all of the data because anyone could
            // mutate a BitmapData instance. If a BitmapData is needed we create a new one always with these values.
            Width = bitmapData.Width;
            Height = bitmapData.Height;
            Stride = bitmapData.Stride;
            PixelFormat = bitmapData.PixelFormat;
            Scan0 = bitmapData.Scan0;
            handle = bitmapData.Reserved; // used as a handle, required for UnlockBits

            if (bitmapData.PixelFormat.IsIndexed())
                Palette = new Palette(bitmap.Palette.Entries);
        }

        #endregion

        #region Destructor

        ~BitmapDataAccessorBase() => Dispose(false);

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        private static void ThrowYOutOfRange()
        {
#pragma warning disable CA2208
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("y", PublicResources.ArgumentOutOfRange);
#pragma warning restore CA2208
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        public Color GetPixel(int x, int y)
        {
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            return GetRow(y).GetColor(x);
        }

        public void SetPixel(int x, int y, Color color)
        {
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            GetRow(y).SetColor(x, color);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Internal Methods

        internal abstract BitmapDataRowBase GetRow(int row);

        #endregion

        #region Private Methods

        private BitmapData ToBitmapData()
            => new BitmapData
            {
                Reserved = handle,
                Width = Width,
                Height = Height,
                Stride = Stride,
                PixelFormat = PixelFormat,
                Scan0 = Scan0
            };

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;
            bitmap.UnlockBits(ToBitmapData());
            disposed = true;
        }

        #endregion

        #endregion

        #endregion
    }
}
