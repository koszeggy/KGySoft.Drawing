#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataBase.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataBase : IBitmapDataInternal
    {
        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public abstract int Height { get; }

        public abstract int Width { get; }

        public abstract PixelFormat PixelFormat { get; }

        public Color32 BackColor { get; protected set; }
        
        public byte AlphaThreshold { get; protected set; }

        public Palette Palette { get; protected set; }

        public abstract int RowSize { get; }

        public virtual bool CanSetPalette => PixelFormat.IsIndexed();

        #endregion

        #region Protected Properties

        protected bool IsDisposed { get; private set; }

        #endregion

        #region Explicitly Implemented Interface Properties

        IReadableBitmapDataRow IReadableBitmapData.FirstRow => DoGetRow(0);
        IWritableBitmapDataRow IWritableBitmapData.FirstRow => DoGetRow(0);
        IReadWriteBitmapDataRow IReadWriteBitmapData.FirstRow => DoGetRow(0);

        #endregion

        #endregion

        #region Indexers

        #region Public Indexers
        
        public IReadWriteBitmapDataRow this[int y]
        {
            [MethodImpl(MethodImpl.AggressiveInlining)]
            get
            {
                if ((uint)y >= Height)
                    ThrowYOutOfRange();
                return DoGetRow(y);
            }
        }

        #endregion

        #region Explicitly Implemented Interface Indexers

        IReadableBitmapDataRow IReadableBitmapData.this[int y] => this[y];
        IWritableBitmapDataRow IWritableBitmapData.this[int y] => this[y];

        #endregion

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

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color GetPixel(int x, int y)
        {
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            return DoGetRow(y).GetColor(x);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPixel(int x, int y, Color color)
        {
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            DoGetRow(y).SetColor(x, color);
        }

        public abstract IBitmapDataRowInternal DoGetRow(int y);

        public virtual bool TrySetPalette(Palette palette)
        {
            if (palette == null || Palette == null || !PixelFormat.IsIndexed() || palette.Count < Palette.Count || palette.Count > 1 << PixelFormat.ToBitsPerPixel())
                return false;
            Palette = palette;
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing) => IsDisposed = true;

        #endregion

        #endregion

        #endregion
    }
}
