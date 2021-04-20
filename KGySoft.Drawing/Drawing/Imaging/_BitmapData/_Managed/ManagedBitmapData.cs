#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapData.cs
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
using System.Drawing.Imaging;
#if NET35 || NET40 || NET45 || NETCOREAPP2_0 || NETSTANDARD2_0 || NETSTANDARD2_1
using System.Security;
#else
using System.Runtime.CompilerServices;
#endif

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class ManagedBitmapData<TColor, TRow> : ManagedBitmapDataBase
        where TColor : unmanaged
        where TRow : ManagedBitmapDataRowBase<TColor, TRow>, new()
    {
        #region Fields

        #region Internal Fields

        internal Array2D<TColor> Buffer;

        #endregion

        #region Private Fields

        private TRow? lastRow;

        #endregion

        #endregion

        #region Properties

        public override PixelFormat PixelFormat { get; }

        public override int Width { get; }

        public override int Height => Buffer.Height;

        public override int RowSize { get; }

        #endregion

        #region Constructors

        internal ManagedBitmapData(Size size, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 0, Palette? palette = null)
        {
            Debug.Assert(size.Width > 0 && size.Height > 0, "Non-empty size expected");
            Debug.Assert(pixelFormat.IsValidFormat(), "Valid format expected");
            Debug.Assert(!pixelFormat.IsIndexed() || typeof(TColor) == typeof(byte), "For indexed pixel formats byte elements are expected");

            BackColor = pixelFormat.HasMultiLevelAlpha() ? default : backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
            PixelFormat = pixelFormat;
            Width = size.Width;

            // Unlike native bitmaps our stride have 1 byte alignment so Stride = (Width * bpp + 7) / 8)
            int bpp = pixelFormat.ToBitsPerPixel();
            int byteWidth = pixelFormat.GetByteWidth(size.Width);
            RowSize = byteWidth;
            Buffer = new Array2D<TColor>(size.Height, bpp <= 8 ? byteWidth : size.Width);
            if (!pixelFormat.IsIndexed())
                return;

            if (palette != null)
            {
                Debug.Assert(palette.Entries.Length <= (1 << bpp), "Too many colors");
                Palette = palette;
                return;
            }

            // if there was no palette specified we use a default one
            Palette = pixelFormat switch
            {
                PixelFormat.Format8bppIndexed => Palette.SystemDefault8BppPalette(backColor, alphaThreshold),
                PixelFormat.Format4bppIndexed => Palette.SystemDefault4BppPalette(backColor),
                _ => Palette.SystemDefault1BppPalette(backColor)
            };

        }

        #endregion

        #region Methods

        #region Public Methods

        public override IBitmapDataRowInternal DoGetRow(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            TRow? result = lastRow;
            if (result?.Index == y)
                return result;

            // Otherwise, we create and cache the result.
            return lastRow = new TRow
            {
                Row = Buffer[y],
                BitmapData = this,
                Index = y,
            };
        }

        #endregion

        #region Internal Methods

#if NET35 || NET40 || NET45 || NETCOREAPP2_0 || NETSTANDARD2_0 || NETSTANDARD2_1
        [SecuritySafeCritical]
        internal override unsafe ref byte GetPinnableReference()
        {
            ref TColor head = ref Buffer.GetPinnableReference();
            fixed (TColor* pHead = &head)
                return ref *(byte*)pHead;
        }
#else
        internal override ref byte GetPinnableReference()
            => ref Unsafe.As<TColor, byte>(ref Buffer.GetPinnableReference());
#endif

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
                Buffer.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}
