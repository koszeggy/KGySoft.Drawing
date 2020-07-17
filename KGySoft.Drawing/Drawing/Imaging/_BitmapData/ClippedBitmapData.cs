#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ClippedBitmapData.cs
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

using System;
using System.Drawing;
using System.Drawing.Imaging;

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
                Index = wrappedRow.Index - bitmapData.OffsetY;
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

            internal ClippedRowInternal(ClippedBitmapData bitmapData, int rowIndex) : base(bitmapData, ((IBitmapDataInternal)bitmapData.BitmapData).GetRow(rowIndex + bitmapData.OffsetY))
            {
            }

            #endregion

            #region Methods

            public override Color32 DoGetColor32(int x) => WrappedRow.DoGetColor32(x + Parent.OffsetX);
            public override void DoSetColor32(int x, Color32 c) => WrappedRow.DoSetColor32(x + Parent.OffsetX, c);
            public override T DoReadRaw<T>(int x) => WrappedRow.DoReadRaw<T>(x);
            public override void DoWriteRaw<T>(int x, T data) => WrappedRow.DoWriteRaw(x, data);
            public override int DoGetColorIndex(int x) => WrappedRow.DoGetColorIndex(x + Parent.OffsetX);
            public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.DoSetColorIndex(x + Parent.OffsetX, colorIndex);

            #endregion
        }

        #endregion

        #region ClippedRowReadWrite class

        private sealed class ClippedRowReadWrite : ClippedRowBase<IReadWriteBitmapDataRow>
        {
            #region Constructors

            internal ClippedRowReadWrite(ClippedBitmapData bitmapData, int rowIndex) : base(bitmapData, ((IReadWriteBitmapData)bitmapData.BitmapData)[rowIndex + bitmapData.OffsetY])
            {
            }

            #endregion

            #region Methods

            public override Color32 DoGetColor32(int x) => WrappedRow[x + Parent.OffsetX];
            public override void DoSetColor32(int x, Color32 c) => WrappedRow[x + Parent.OffsetX] = c;
            public override T DoReadRaw<T>(int x) => WrappedRow.ReadRaw<T>(x);
            public override void DoWriteRaw<T>(int x, T data) => WrappedRow.WriteRaw(x, data);
            public override int DoGetColorIndex(int x) => WrappedRow.GetColorIndex(x + Parent.OffsetX);
            public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.SetColorIndex(x + Parent.OffsetX, colorIndex);

            #endregion
        }

        #endregion

        #region ClippedRowReadable class

        private sealed class ClippedRowReadable : ClippedRowBase<IReadableBitmapDataRow>
        {
            #region Constructors

            internal ClippedRowReadable(ClippedBitmapData bitmapData, int rowIndex) : base(bitmapData, ((IReadableBitmapData)bitmapData.BitmapData)[rowIndex + bitmapData.OffsetY])
            {
            }

            #endregion

            #region Methods

            public override Color32 DoGetColor32(int x) => WrappedRow[x + Parent.OffsetX];
            public override T DoReadRaw<T>(int x) => WrappedRow.ReadRaw<T>(x);
            public override int DoGetColorIndex(int x) => WrappedRow.GetColorIndex(x + Parent.OffsetX);
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

            internal ClippedRowWritable(ClippedBitmapData bitmapData, int rowIndex) : base(bitmapData, ((IWritableBitmapData)bitmapData.BitmapData)[rowIndex + bitmapData.OffsetY])
            {
            }

            #endregion

            #region Methods

            public override void DoSetColor32(int x, Color32 c) => WrappedRow[x + Parent.OffsetX] = c;
            public override void DoWriteRaw<T>(int x, T data) => WrappedRow.WriteRaw(x, data);
            public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.SetColorIndex(x + Parent.OffsetX, colorIndex);
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

        private Rectangle region;
        private IBitmapDataRowInternal lastRow;

        #endregion

        #region Properties

        #region Public Properties

        public override int Height => region.Height;
        public override int Width => region.Width;
        public override PixelFormat PixelFormat { get; }
        public override int RowSize { get; }

        #endregion

        #region Internal Properties

        internal IBitmapData BitmapData { get; }
        internal Rectangle Region => region;

        #endregion

        #region Private Properties

        private int OffsetY => region.Y;
        private int OffsetX => region.X;

        #endregion

        #endregion

        #region Constructors

        internal ClippedBitmapData(IBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            region = clippingRegion;

            // source is already clipped: unwrapping to prevent tiered nesting (not calling Unwrap because other types should not be extracted here)
            if (source is ClippedBitmapData parent)
            {
                BitmapData = parent.BitmapData;
                region.Offset(parent.region.Location);
                region.Intersect(parent.region);
            }
            else
            {
                BitmapData = source;
                region.Intersect(new Rectangle(Point.Empty, source.GetSize()));
            }

            if (region.IsEmpty)
                throw new ArgumentOutOfRangeException(nameof(clippingRegion), PublicResources.ArgumentOutOfRange);

            bitmapDataType = BitmapData switch
            {
                IBitmapDataInternal _ => BitmapDataType.Internal,
                IReadWriteBitmapData _ => BitmapDataType.ReadWrite,
                IReadableBitmapData _ => BitmapDataType.Readable,
                IWritableBitmapData _ => BitmapDataType.Writable,
                _ => BitmapDataType.None
            };

            PixelFormat = BitmapData.PixelFormat;
            BackColor = BitmapData.BackColor;
            AlphaThreshold = BitmapData.AlphaThreshold;
            Palette = BitmapData.Palette;
            int bpp = PixelFormat.ToBitsPerPixel();

            RowSize = region.Left > 0 
                // Any clipping from the left disables raw access because ReadRaw/WriteRaw offset depends on size of T,
                // which will fail for any T whose size is not the same as the actual pixel size
                ? 0 
                // Even one byte padding is disabled to protect the right edge of a region by default
                : (region.Width * bpp) >> 3;

            if (bpp >= 8 || RowSize == 0)
                return;

            // 1/4bpp: Adjust RowSize if needed
            int alignmentMask = bpp == 1 ? 7 : 1;

            // right edge: if not at byte boundary but that is the right edge of the original image, then we allow including padding
            if ((region.Width & alignmentMask) != 0 && region.Right == BitmapData.Width)
                RowSize++;
        }

        #endregion

        #region Methods

        public override IBitmapDataRowInternal GetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            IBitmapDataRowInternal result = lastRow;
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
    }
}
