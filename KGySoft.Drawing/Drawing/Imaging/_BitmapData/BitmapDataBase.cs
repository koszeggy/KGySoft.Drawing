#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataBase.cs
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [DebuggerDisplay("{" + nameof(Width) + "}x{" + nameof(Height) + "} {KGySoft.Drawing." + nameof(PixelFormatExtensions) + "." + nameof(PixelFormatExtensions.ToBitsPerPixel) + "(" + nameof(PixelFormat) + ")}bpp")]
    internal abstract class BitmapDataBase : IBitmapDataInternal
    {
        #region Fields

        private Action? disposeCallback;
        private Action<Palette>? setPalette;

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public int Height { get; protected init; }
        public int Width { get; protected init; }
        public PixelFormat PixelFormat { get; }
        public Color32 BackColor { get; }
        public byte AlphaThreshold { get; }
        public Palette? Palette { get; private set; }
        public int RowSize { get; protected init; }
        public bool IsDisposed { get; private set; }
        public bool CanSetPalette => AllowSetPalette && PixelFormat.IsIndexed() && Palette != null;
        public virtual bool IsCustomPixelFormat => !PixelFormat.IsValidFormat();

        #endregion

        #region Protected Properties

        protected virtual bool AllowSetPalette => setPalette != null;

        #endregion

        #region Explicitly Implemented Interface Properties

        IReadableBitmapDataRow IReadableBitmapData.FirstRow => GetFirstRow();
        IWritableBitmapDataRow IWritableBitmapData.FirstRow => GetFirstRow();
        IReadWriteBitmapDataRow IReadWriteBitmapData.FirstRow => GetFirstRow();

        #endregion

        #endregion

        #region Indexers

        #region Public Indexers

        public IReadWriteBitmapDataRow this[int y]
        {
            [MethodImpl(MethodImpl.AggressiveInlining)]
            get
            {
                if (IsDisposed)
                    ThrowDisposed();
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

        #region Constructors

        protected BitmapDataBase(Size size, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128,
            Palette? palette = null, Action<Palette>? setPalette = null, Action? disposeCallback = null)
        {
            Debug.Assert(size.Width > 0 && size.Height > 0, "Non-empty size expected");
            Debug.Assert(pixelFormat.ToBitsPerPixel() > 0);
            Debug.Assert(palette == null || palette.BackColor == backColor.ToOpaque() && palette.AlphaThreshold == alphaThreshold);

            this.disposeCallback = disposeCallback;
            this.setPalette = setPalette;
            Width = size.Width;
            Height = size.Height;
            BackColor = pixelFormat.HasMultiLevelAlpha() ? default : backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
            PixelFormat = pixelFormat;
            if (!pixelFormat.IsIndexed())
                return;

            if (palette != null)
            {
                Debug.Assert(palette.Entries.Length <= (1 << pixelFormat.ToBitsPerPixel()), "Too many colors");
                Palette = palette;
                return;
            }

            Palette = palette ?? pixelFormat switch
            {
                PixelFormat.Format8bppIndexed => Palette.SystemDefault8BppPalette(backColor, alphaThreshold),
                PixelFormat.Format4bppIndexed => Palette.SystemDefault4BppPalette(backColor),
                PixelFormat.Format1bppIndexed => Palette.SystemDefault1BppPalette(backColor),
                _ => throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format for palette: {pixelFormat}"))
            };

            AlphaThreshold = Palette.AlphaThreshold;
        }


        #endregion

        #region Methods

        #region Static Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowDisposed() => throw new ObjectDisposedException(null, PublicResources.ObjectDisposed);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowYOutOfRange()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("y", PublicResources.ArgumentOutOfRange);
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color GetPixel(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            return DoGetRow(y).GetColor(x);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPixel(int x, int y, Color color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            DoGetRow(y).SetColor(x, color);
        }

        public abstract IBitmapDataRowInternal DoGetRow(int y);

        public bool TrySetPalette(Palette? palette)
        {
            if (!CanSetPalette || palette == null || palette.Count < Palette!.Count || palette.Count > 1 << PixelFormat.ToBitsPerPixel())
                return false;
            if (palette.BackColor == BackColor && palette.AlphaThreshold == AlphaThreshold)
                Palette = palette;
            else
                Palette = new Palette(palette, BackColor, AlphaThreshold);

            setPalette?.Invoke(palette);
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Methods

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            try
            {
                // may happen if the constructor failed and the call comes from the finalizer
                disposeCallback?.Invoke();
            }
            catch (Exception)
            {
                // From explicit dispose we throw it further but we ignore it from destructor.
                if (disposing)
                    throw;
            }
            finally
            {
                disposeCallback = null;
                setPalette = null;
                IsDisposed = true;
            }
        }

        #endregion

        #region Private Methods

        private IReadWriteBitmapDataRow GetFirstRow()
        {
            if (IsDisposed)
                ThrowDisposed();
            return DoGetRow(0);
        }

        #endregion

        #endregion

        #endregion
    }
}
