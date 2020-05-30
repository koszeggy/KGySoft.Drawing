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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;

using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataAccessorBase : IBitmapDataInternal
    {
        #region Fields

        private readonly Bitmap bitmap;
        private readonly BitmapData bitmapData;

        private bool disposed;

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height => bitmapData.Height;

        public int Width => bitmapData.Width;

        public PixelFormat PixelFormat => bitmapData.PixelFormat;

        public Color32 BackColor { get; }
        
        public byte AlphaThreshold { get; }

        public Palette Palette { get; }

        public int RowSize { get; }

        #endregion

        #region Internal Properties

        internal IntPtr Scan0 => bitmapData.Scan0;
        internal int Stride => bitmapData.Stride;

        #endregion

        #region Explicitly Implemented Properties

        IReadableBitmapDataRow IReadableBitmapData.FirstRow
        {
            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            get => GetRow(0);
        }

        IWritableBitmapDataRow IWritableBitmapData.FirstRow
        {
            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            get => GetRow(0);
        }

        IReadWriteBitmapDataRow IReadWriteBitmapData.FirstRow
        {
            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
            get => GetRow(0);
        }

        #endregion

        #endregion

        #region Indexers

        IReadWriteBitmapDataRow IReadWriteBitmapData.this[int y]
        {
            [SecuritySafeCritical]
            [MethodImpl(MethodImpl.AggressiveInlining)]
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

        [SecuritySafeCritical]
        protected BitmapDataAccessorBase(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, Color32 backColor, byte alphaThreshold, Palette palette)
        {
            // It must be the same as bitmap format except if LockBits is not supported with the original pixel format (occurs on Linux).
            Debug.Assert(bitmap.PixelFormat == pixelFormat || !OSUtils.IsWindows, "Unmatching pixel format");

            // If palette is passed it must match with actual palette
            Debug.Assert(palette == null || bitmap.Palette.Entries.Length == palette.Entries.Length
                && bitmap.Palette.Entries.Zip(palette.Entries, (c1, c2) => new Color32(c1) == c2).All(b => b), "Unmatching palette");

            this.bitmap = bitmap;
            BackColor = backColor;
            AlphaThreshold = alphaThreshold;

            bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), lockMode, pixelFormat);
            RowSize = Math.Abs(bitmapData.Stride);

            if (bitmapData.PixelFormat.IsIndexed())
                Palette = palette ?? new Palette(bitmap.Palette.Entries, backColor.ToColor(), alphaThreshold);
        }

        #endregion

        #region Destructor

        ~BitmapDataAccessorBase() => Dispose(false);

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
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

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color GetPixel(int x, int y)
        {
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            return GetRow(y).GetColor(x);
        }

        [SecuritySafeCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPixel(int x, int y, Color color)
        {
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            GetRow(y).SetColor(x, color);
        }

        [SecurityCritical]
        public abstract IBitmapDataRowInternal GetRow(int row);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        [SecuritySafeCritical]
        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            try
            {
                // On Linux this may throw an exception after LockBits failed.
                bitmap.UnlockBits(bitmapData);
            }
            catch (Exception)
            {
                // From explicit dispose we throw it further but we ignore it from destructor.
                if (disposing)
                    throw;
            }
            finally
            {
                disposed = true;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
