#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ClippedBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
using System.Security;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ClippedBitmapData : BitmapDataBase
    {
        #region Nested classes

        #region ClippedRowBase class

        private abstract class ClippedRowBase<TRow> : BitmapDataRowBase
            where TRow : IBitmapDataRowMovable
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

            protected override void DoMoveToIndex() => WrappedRow.MoveToRow(Index + Parent.Y);

            #endregion
        }

        #endregion

        #region ClippedRowInternal class

        private sealed class ClippedRowInternal : ClippedRowBase<IBitmapDataRowInternal>
        {
            #region Constructors

            internal ClippedRowInternal(ClippedBitmapData bitmapData, int rowIndex)
                : base(bitmapData, ((IBitmapDataInternal)bitmapData.BitmapData).GetRowUncached(rowIndex + bitmapData.Y))
            {
            }

            #endregion

            #region Methods

            [SecurityCritical]public override Color32 DoGetColor32(int x) => WrappedRow.DoGetColor32(x + Parent.X);
            [SecurityCritical]public override void DoSetColor32(int x, Color32 c) => WrappedRow.DoSetColor32(x + Parent.X, c);
            [SecurityCritical]public override PColor32 DoGetPColor32(int x) => WrappedRow.DoGetPColor32(x + Parent.X);
            [SecurityCritical]public override void DoSetPColor32(int x, PColor32 c) => WrappedRow.DoSetPColor32(x + Parent.X, c);
            [SecurityCritical]public override Color64 DoGetColor64(int x) => WrappedRow.DoGetColor64(x + Parent.X);
            [SecurityCritical]public override void DoSetColor64(int x, Color64 c) => WrappedRow.DoSetColor64(x + Parent.X, c);
            [SecurityCritical]public override PColor64 DoGetPColor64(int x) => WrappedRow.DoGetPColor64(x + Parent.X);
            [SecurityCritical]public override void DoSetPColor64(int x, PColor64 c) => WrappedRow.DoSetPColor64(x + Parent.X, c);
            [SecurityCritical]public override ColorF DoGetColorF(int x) => WrappedRow.DoGetColorF(x + Parent.X);
            [SecurityCritical]public override void DoSetColorF(int x, ColorF c) => WrappedRow.DoSetColorF(x + Parent.X, c);
            [SecurityCritical]public override PColorF DoGetPColorF(int x) => WrappedRow.DoGetPColorF(x + Parent.X);
            [SecurityCritical]public override void DoSetPColorF(int x, PColorF c) => WrappedRow.DoSetPColorF(x + Parent.X, c);
            [SecurityCritical]public override T DoReadRaw<T>(int x) => WrappedRow.DoReadRaw<T>(x);
            [SecurityCritical]public override void DoWriteRaw<T>(int x, T data) => WrappedRow.DoWriteRaw(x, data);
            [SecurityCritical]public override int DoGetColorIndex(int x) => WrappedRow.DoGetColorIndex(x + Parent.X);
            [SecurityCritical]public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.DoSetColorIndex(x + Parent.X, colorIndex);

            #endregion
        }

        #endregion

        #region ClippedRowReadWrite class

        private sealed class ClippedRowReadWrite : ClippedRowBase<IReadWriteBitmapDataRowMovable>
        {
            #region Constructors

            internal ClippedRowReadWrite(ClippedBitmapData bitmapData, int rowIndex)
                : base(bitmapData, ((IReadWriteBitmapData)bitmapData.BitmapData).GetMovableRow(rowIndex + bitmapData.Y))
            {
            }

            #endregion

            #region Methods

            [SecurityCritical]public override Color32 DoGetColor32(int x) => WrappedRow.GetColor32(x + Parent.X);
            [SecurityCritical]public override void DoSetColor32(int x, Color32 c) => WrappedRow.SetColor32(x + Parent.X, c);
            [SecurityCritical]public override PColor32 DoGetPColor32(int x) => WrappedRow.GetPColor32(x + Parent.X);
            [SecurityCritical]public override void DoSetPColor32(int x, PColor32 c) => WrappedRow.SetPColor32(x + Parent.X, c);
            [SecurityCritical]public override Color64 DoGetColor64(int x) => WrappedRow.GetColor64(x + Parent.X);
            [SecurityCritical]public override void DoSetColor64(int x, Color64 c) => WrappedRow.SetColor64(x + Parent.X, c);
            [SecurityCritical]public override PColor64 DoGetPColor64(int x) => WrappedRow.GetPColor64(x + Parent.X);
            [SecurityCritical]public override void DoSetPColor64(int x, PColor64 c) => WrappedRow.SetPColor64(x + Parent.X, c);
            [SecurityCritical]public override ColorF DoGetColorF(int x) => WrappedRow.GetColorF(x + Parent.X);
            [SecurityCritical]public override void DoSetColorF(int x, ColorF c) => WrappedRow.SetColorF(x + Parent.X, c);
            [SecurityCritical]public override PColorF DoGetPColorF(int x) => WrappedRow.GetPColorF(x + Parent.X);
            [SecurityCritical]public override void DoSetPColorF(int x, PColorF c) => WrappedRow.SetPColorF(x + Parent.X, c);
            [SecurityCritical]public override T DoReadRaw<T>(int x) => WrappedRow.ReadRaw<T>(x);
            [SecurityCritical]public override void DoWriteRaw<T>(int x, T data) => WrappedRow.WriteRaw(x, data);
            [SecurityCritical]public override int DoGetColorIndex(int x) => WrappedRow.GetColorIndex(x + Parent.X);
            [SecurityCritical]public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.SetColorIndex(x + Parent.X, colorIndex);

            #endregion
        }

        #endregion

        #region ClippedRowReadable class

        private sealed class ClippedRowReadable : ClippedRowBase<IReadableBitmapDataRowMovable>
        {
            #region Constructors

            internal ClippedRowReadable(ClippedBitmapData bitmapData, int rowIndex)
                : base(bitmapData, ((IReadableBitmapData)bitmapData.BitmapData).GetMovableRow(rowIndex + bitmapData.Y))
            {
            }

            #endregion

            #region Methods

            [SecurityCritical]public override Color32 DoGetColor32(int x) => WrappedRow.GetColor32(x + Parent.X);
            [SecurityCritical]public override PColor32 DoGetPColor32(int x) => WrappedRow.GetPColor32(x + Parent.X);
            [SecurityCritical]public override Color64 DoGetColor64(int x) => WrappedRow.GetColor64(x + Parent.X);
            [SecurityCritical]public override PColor64 DoGetPColor64(int x) => WrappedRow.GetPColor64(x + Parent.X);
            [SecurityCritical]public override ColorF DoGetColorF(int x) => WrappedRow.GetColorF(x + Parent.X);
            [SecurityCritical]public override PColorF DoGetPColorF(int x) => WrappedRow.GetPColorF(x + Parent.X);
            [SecurityCritical]public override T DoReadRaw<T>(int x) => WrappedRow.ReadRaw<T>(x);
            [SecurityCritical]public override int DoGetColorIndex(int x) => WrappedRow.GetColorIndex(x + Parent.X);
            [SecurityCritical]public override void DoSetColor32(int x, Color32 c) => throw new InvalidOperationException();
            [SecurityCritical]public override void DoWriteRaw<T>(int x, T data) => throw new InvalidOperationException();
            [SecurityCritical]public override void DoSetColorIndex(int x, int colorIndex) => throw new InvalidOperationException();

            #endregion
        }

        #endregion

        #region ClippedRowWritable class

        private sealed class ClippedRowWritable : ClippedRowBase<IWritableBitmapDataRowMovable>
        {
            #region Constructors

            internal ClippedRowWritable(ClippedBitmapData bitmapData, int rowIndex)
                : base(bitmapData, ((IWritableBitmapData)bitmapData.BitmapData).GetMovableRow(rowIndex + bitmapData.Y))
            {
            }

            #endregion

            #region Methods

            [SecurityCritical]public override void DoSetColor32(int x, Color32 c) => WrappedRow.SetColor32(x + Parent.X, c);
            [SecurityCritical]public override void DoSetPColor32(int x, PColor32 c) => WrappedRow.SetPColor32(x + Parent.X, c);
            [SecurityCritical]public override void DoSetColor64(int x, Color64 c) => WrappedRow.SetColor64(x + Parent.X, c);
            [SecurityCritical]public override void DoSetPColor64(int x, PColor64 c) => WrappedRow.SetPColor64(x + Parent.X, c);
            [SecurityCritical]public override void DoSetColorF(int x, ColorF c) => WrappedRow.SetColorF(x + Parent.X, c);
            [SecurityCritical]public override void DoSetPColorF(int x, PColorF c) => WrappedRow.SetPColorF(x + Parent.X, c);
            [SecurityCritical]public override void DoSetColorIndex(int x, int colorIndex) => WrappedRow.SetColorIndex(x + Parent.X, colorIndex);
            [SecurityCritical]public override void DoWriteRaw<T>(int x, T data) => WrappedRow.WriteRaw(x, data);
            [SecurityCritical]public override Color32 DoGetColor32(int x) => throw new InvalidOperationException();
            [SecurityCritical]public override int DoGetColorIndex(int x) => throw new InvalidOperationException();
            [SecurityCritical]public override T DoReadRaw<T>(int x) => throw new InvalidOperationException();

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private readonly Func<int, BitmapDataRowBase> createRowFactory;
        private readonly bool disposeBitmapData;

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
            : base(new BitmapDataConfig(clippingRegion.Size, source.PixelFormat, source.BackColor, source.AlphaThreshold, source.WorkingColorSpace, source.Palette))
        {
            disposeBitmapData = disposeSource;

            // source is already clipped: unwrapping to prevent tiered nesting (not calling Unwrap because other types should not be extracted here)
            if (source is ClippedBitmapData parent)
            {
                BitmapData = parent.BitmapData;
                clippingRegion.Offset(parent.X, parent.Y);
                clippingRegion = clippingRegion.IntersectSafe(parent.Region);
            }
            else
            {
                BitmapData = source;
                clippingRegion = clippingRegion.IntersectSafe(new Rectangle(Point.Empty, source.Size));
            }

            if (clippingRegion.IsEmpty())
                throw new ArgumentOutOfRangeException(nameof(clippingRegion), PublicResources.ArgumentOutOfRange);

            createRowFactory = BitmapData switch
            {
                IBitmapDataInternal _ => y => new ClippedRowInternal(this, y),
                IReadWriteBitmapData _ => y => new ClippedRowReadWrite(this, y),
                IReadableBitmapData _ => y => new ClippedRowReadable(this, y),
                IWritableBitmapData _ => y => new ClippedRowWritable(this, y),
                _ => throw new InvalidOperationException(Res.InternalError($"Unexpected bitmap data type: {source.GetType()}"))
            };

            X = clippingRegion.X;
            Y = clippingRegion.Y;
            Width = clippingRegion.Width;
            Height = clippingRegion.Height;
            int bpp = PixelFormat.BitsPerPixel;
            int maxRowSize = (Width * bpp) >> 3;
            RowSize = X > 0 
                // Any clipping from the left disables raw access because ReadRaw/WriteRaw offset depends on size of T,
                // which will fail for any T whose size is not the same as the actual pixel size
                ? 0 
                // Even one byte padding is disabled to protect the right edge of a region by default
                : Math.Min(source.RowSize, maxRowSize);

            if (RowSize < maxRowSize)
                return;

            // 1/4bpp: Adjust RowSize if needed
            // right edge: if not at byte boundary but that is the right edge of the original image, then we allow including padding
            if (!PixelFormat.IsAtByteBoundary(Width) && Width == BitmapData.Width)
                RowSize++;
        }

        #endregion

        #region Methods

        #region Public Methods

        [SecurityCritical]public override Color32 DoGetColor32(int x, int y) => GetRowCached(y).DoGetColor32(x);
        [SecurityCritical]public override void DoSetColor32(int x, int y, Color32 c) => GetRowCached(y).DoSetColor32(x, c);
        [SecurityCritical]public override PColor32 DoGetPColor32(int x, int y) => GetRowCached(y).DoGetPColor32(x);
        [SecurityCritical]public override void DoSetPColor32(int x, int y, PColor32 c) => GetRowCached(y).DoSetPColor32(x, c);
        [SecurityCritical]public override Color64 DoGetColor64(int x, int y) => GetRowCached(y).DoGetColor64(x);
        [SecurityCritical]public override void DoSetColor64(int x, int y, Color64 c) => GetRowCached(y).DoSetColor64(x, c);
        [SecurityCritical]public override PColor64 DoGetPColor64(int x, int y) => GetRowCached(y).DoGetPColor64(x);
        [SecurityCritical]public override void DoSetPColor64(int x, int y, PColor64 c) => GetRowCached(y).DoSetPColor64(x, c);
        [SecurityCritical]public override ColorF DoGetColorF(int x, int y) => GetRowCached(y).DoGetColorF(x);
        [SecurityCritical]public override void DoSetColorF(int x, int y, ColorF c) => GetRowCached(y).DoSetColorF(x, c);
        [SecurityCritical]public override PColorF DoGetPColorF(int x, int y) => GetRowCached(y).DoGetPColorF(x);
        [SecurityCritical]public override void DoSetPColorF(int x, int y, PColorF c) => GetRowCached(y).DoSetPColorF(x, c);
        [SecurityCritical]public override T DoReadRaw<T>(int x, int y) => GetRowCached(y).DoReadRaw<T>(x);
        [SecurityCritical]public override void DoWriteRaw<T>(int x, int y, T data) => GetRowCached(y).DoWriteRaw(x, data);
        [SecurityCritical]public override int DoGetColorIndex(int x, int y) => GetRowCached(y).DoGetColorIndex(x);
        [SecurityCritical]public override void DoSetColorIndex(int x, int y, int colorIndex) => GetRowCached(y).DoSetColorIndex(x, colorIndex);

        #endregion

        #region Protected Methods

        protected override IBitmapDataRowInternal DoGetRow(int y) => createRowFactory.Invoke(y);

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
