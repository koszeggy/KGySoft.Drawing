#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ClippedBitmapData.cs
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

using System;
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ClippedBitmapData : BitmapDataBase
    {
        #region Nested types

        #region Enumerations

        private enum BitmapDataType { None, Internal, ReadWrite, Readable, Writable }

        #endregion

        #region Nested classes

        #region ClippedRowBase class

        private abstract class ClippedRowBase<TRow> : BitmapDataRowBase
            where TRow : IBitmapDataRow
        {
            #region Fields

            protected readonly ClippedBitmapData Parent;
            protected readonly TRow WrappedRow;

            #endregion

            #region Constructors

            protected ClippedRowBase(ClippedBitmapData bitmapData, TRow wrappedRow)
            {
                BitmapData = Parent = bitmapData;
                WrappedRow = wrappedRow;
                Index = wrappedRow.Index - bitmapData.Y;
            }

            #endregion

            #region Methods

            public override bool MoveNextRow() => base.MoveNextRow() && WrappedRow.MoveNextRow();

            #endregion
        }

        #endregion

        #region ClippedRowInternal class

        private sealed class ClippedRowInternal : ClippedRowBase<IBitmapDataRowInternal>
        {
            #region Constructors

            internal ClippedRowInternal(ClippedBitmapData bitmapData, int rowIndex) : base(bitmapData, ((IBitmapDataInternal)bitmapData.BitmapData).DoGetRow(rowIndex + bitmapData.Y))
            {
            }

            #endregion

            #region Methods

            public override Color32 DoGetColor32(int x) => WrappedRow.DoGetColor32(x + Parent.X);
            public override void DoSetColor32(int x, Color32 c) => WrappedRow.DoSetColor32(x + Parent.X, c);
            public override T DoReadRaw<T>(int x) => WrappedRow.DoReadRaw<T>(x);
            public override void DoWriteRaw<T>(int x, T data) => WrappedRow.DoWriteRaw(x, data);
            public override int DoGetColorIndex(int x) => WrappedRow.DoGetColorIndex(x + Parent.X);
            public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.DoSetColorIndex(x + Parent.X, colorIndex);
            public override Color32 DoGetColor32Premultiplied(int x) => WrappedRow.DoGetColor32Premultiplied(x + Parent.X);
            public override void DoSetColor32Premultiplied(int x, Color32 c) => WrappedRow.DoSetColor32Premultiplied(x + Parent.X, c);

            #endregion
        }

        #endregion

        #region ClippedRowReadWrite class

        private sealed class ClippedRowReadWrite : ClippedRowBase<IReadWriteBitmapDataRow>
        {
            #region Constructors

            internal ClippedRowReadWrite(ClippedBitmapData bitmapData, int rowIndex) : base(bitmapData, ((IReadWriteBitmapData)bitmapData.BitmapData)[rowIndex + bitmapData.Y])
            {
            }

            #endregion

            #region Methods

            public override Color32 DoGetColor32(int x) => WrappedRow[x + Parent.X];
            public override void DoSetColor32(int x, Color32 c) => WrappedRow[x + Parent.X] = c;
            public override T DoReadRaw<T>(int x) => WrappedRow.ReadRaw<T>(x);
            public override void DoWriteRaw<T>(int x, T data) => WrappedRow.WriteRaw(x, data);
            public override int DoGetColorIndex(int x) => WrappedRow.GetColorIndex(x + Parent.X);
            public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.SetColorIndex(x + Parent.X, colorIndex);

            #endregion
        }

        #endregion

        #region ClippedRowReadable class

        private sealed class ClippedRowReadable : ClippedRowBase<IReadableBitmapDataRow>
        {
            #region Constructors

            internal ClippedRowReadable(ClippedBitmapData bitmapData, int rowIndex) : base(bitmapData, ((IReadableBitmapData)bitmapData.BitmapData)[rowIndex + bitmapData.Y])
            {
            }

            #endregion

            #region Methods

            public override Color32 DoGetColor32(int x) => WrappedRow[x + Parent.X];
            public override T DoReadRaw<T>(int x) => WrappedRow.ReadRaw<T>(x);
            public override int DoGetColorIndex(int x) => WrappedRow.GetColorIndex(x + Parent.X);
            public override void DoSetColor32(int x, Color32 c) => throw new InvalidOperationException();
            public override void DoWriteRaw<T>(int x, T data) => throw new InvalidOperationException();
            public override void DoSetColorIndex(int x, int colorIndex) => throw new InvalidOperationException();

            #endregion
        }

        #endregion

        #region ClippedRowWritable class

        private sealed class ClippedRowWritable : ClippedRowBase<IWritableBitmapDataRow>
        {
            #region Constructors

            internal ClippedRowWritable(ClippedBitmapData bitmapData, int rowIndex) : base(bitmapData, ((IWritableBitmapData)bitmapData.BitmapData)[rowIndex + bitmapData.Y])
            {
            }

            #endregion

            #region Methods

            public override void DoSetColor32(int x, Color32 c) => WrappedRow[x + Parent.X] = c;
            public override void DoWriteRaw<T>(int x, T data) => WrappedRow.WriteRaw(x, data);
            public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.SetColorIndex(x + Parent.X, colorIndex);
            public override Color32 DoGetColor32(int x) => throw new InvalidOperationException();
            public override T DoReadRaw<T>(int x) => throw new InvalidOperationException();
            public override int DoGetColorIndex(int x) => throw new InvalidOperationException();

            #endregion
        }

        #endregion

        #endregion

        #endregion

        #region Fields

        private readonly BitmapDataType bitmapDataType;
        private readonly bool disposeBitmapData;

        private IBitmapDataRowInternal? lastRow;

        #endregion

        #region Properties

        #region Internal Properties

        private int X { get; }
        private int Y { get; }
        internal Rectangle Region => new Rectangle(X, Y, Width, Height);
        internal IBitmapData BitmapData { get; }

        #endregion

        #region Protected Properties

        protected override bool AllowSetPalette => false;

        #endregion

        #endregion

        #region Constructors

        internal ClippedBitmapData(IBitmapData source, Rectangle clippingRegion, bool disposeSource)
            : base(clippingRegion.Size, source.PixelFormat, source.BackColor, source.AlphaThreshold, source.Palette, null, null)
        {
            disposeBitmapData = disposeSource;

            // source is already clipped: unwrapping to prevent tiered nesting (not calling Unwrap because other types should not be extracted here)
            if (source is ClippedBitmapData parent)
            {
                BitmapData = parent.BitmapData;
                clippingRegion.Offset(parent.X, parent.Y);
                clippingRegion.Intersect(parent.Region);
            }
            else
            {
                BitmapData = source;
                clippingRegion.Intersect(new Rectangle(Point.Empty, source.GetSize()));
            }

            if (clippingRegion.IsEmpty)
                throw new ArgumentOutOfRangeException(nameof(clippingRegion), PublicResources.ArgumentOutOfRange);

            bitmapDataType = BitmapData switch
            {
                IBitmapDataInternal _ => BitmapDataType.Internal,
                IReadWriteBitmapData _ => BitmapDataType.ReadWrite,
                IReadableBitmapData _ => BitmapDataType.Readable,
                IWritableBitmapData _ => BitmapDataType.Writable,
                _ => BitmapDataType.None
            };

            X = clippingRegion.X;
            Y = clippingRegion.Y;
            Width = clippingRegion.Width;
            Height = clippingRegion.Height;
            int bpp = PixelFormat.ToBitsPerPixel();
            int maxRowSize = (Width * bpp) >> 3;
            RowSize = X > 0 
                // Any clipping from the left disables raw access because ReadRaw/WriteRaw offset depends on size of T,
                // which will fail for any T whose size is not the same as the actual pixel size
                ? 0 
                // Even one byte padding is disabled to protect the right edge of a region by default
                : Math.Min(source.RowSize, maxRowSize);

            if (bpp >= 8 || RowSize < maxRowSize)
                return;

            // 1/4bpp: Adjust RowSize if needed
            // right edge: if not at byte boundary but that is the right edge of the original image, then we allow including padding
            if (!PixelFormat.IsAtByteBoundary(Width) && Width == BitmapData.Width)
                RowSize++;
        }

        #endregion

        #region Methods

        #region Public Methods
        
        public override IBitmapDataRowInternal DoGetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            IBitmapDataRowInternal? result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = bitmapDataType switch
            {
                BitmapDataType.Internal => new ClippedRowInternal(this, y),
                BitmapDataType.ReadWrite => new ClippedRowReadWrite(this, y),
                BitmapDataType.Readable => new ClippedRowReadable(this, y),
                BitmapDataType.Writable => new ClippedRowWritable(this, y),
                _ => throw new InvalidOperationException(Res.InternalError($"Unexpected row access on type: {BitmapData.GetType()}")),
            };
        }

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing && disposeBitmapData)
                BitmapData.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}
