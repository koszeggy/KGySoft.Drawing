#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [DebuggerDisplay("{" + nameof(Width) + "}x{" + nameof(Height) + "} {" + nameof(PixelFormat) + "." + nameof(PixelFormatInfo.BitsPerPixel) + "}bpp")]
    internal abstract class BitmapDataBase : IBitmapDataInternal
    {
        #region Fields

        private Action? disposeCallback;
        private Func<Palette, bool>? trySetPaletteCallback;

        // This cache is exposed only for the indexers, which return interface types without MoveNext/MoveToRow methods
        // Non-volatile field because it's even better if the threads see their lastly set instance
        private IBitmapDataRowInternal? cachedRowByIndex;

        // This cache is not exposed to public access, only to the internal GetCachedRowByThreadId method.
        // Its consumers must always use the result in a local scope where no context switch is possible between threads.
        // The array contains up to 8 entries even if the CPU has more cores. Possible hijack: Parallel.For(Get/SetPixel) on a custom bitmap data.
        private volatile StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>?[]? cachedRowByThreadId;
        private int hashMask; // non-volatile because always the volatile cachedRowByThreadId is accessed first

        #endregion

        #region Properties and Indexers

        #region Properties

        #region Public Properties

        public Size Size => new Size(Width, Height);
        public bool IsDisposed { get; private set; }
        public bool CanSetPalette => PixelFormat.Indexed && Palette != null && AllowSetPalette;
        public virtual bool IsCustomPixelFormat => PixelFormat.IsCustomFormat;
        public WorkingColorSpace WorkingColorSpace { get; }

        #endregion

        #region Internal Properties

        internal int Height { get; private protected set; }
        internal int Width { get; private protected set; }
        internal Color32 BackColor { get; }
        internal byte AlphaThreshold { get; }
        internal Palette? Palette { get; private set; }
        internal PixelFormatInfo PixelFormat { get; }
        internal int RowSize { get; private protected set; }
        internal bool LinearWorkingColorSpace { get; }

        #endregion

        #region Protected Properties

        protected virtual bool AllowSetPalette => true;

        #endregion

        #region Explicitly Implemented Interface Properties

        IReadableBitmapDataRowMovable IReadableBitmapData.FirstRow => GetFirstRow();
        IWritableBitmapDataRowMovable IWritableBitmapData.FirstRow => GetFirstRow();
        IReadWriteBitmapDataRowMovable IReadWriteBitmapData.FirstRow => GetFirstRow();

        // The following properties are implemented explicitly so their underlying actual property
        // can be accessed faster internally than implicit interface implementations, which are always virtual members.
        int IBitmapData.Height => Height;
        int IBitmapData.Width => Width;
        byte IBitmapData.AlphaThreshold => AlphaThreshold;
        Color32 IBitmapData.BackColor => BackColor;
        Palette? IBitmapData.Palette => Palette;
        PixelFormatInfo IBitmapData.PixelFormat => PixelFormat;
        int IBitmapData.RowSize => RowSize;

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
                return GetCachedRowByIndex(y);
            }
        }

        #endregion

        #region Explicitly Implemented Interface Indexers

        IReadableBitmapDataRow IReadableBitmapData.this[int y] => this[y];
        IWritableBitmapDataRow IWritableBitmapData.this[int y] => this[y];

        #endregion

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        protected BitmapDataBase(in BitmapDataConfig cfg)
        {
            #region Local Methods

            static Palette ExpandPalette(Palette palette, int bpp)
            {
                var entries = new Color32[1 << bpp];
                palette.Entries.CopyTo(entries, 0);
                return new Palette(entries, palette.BackColor, palette.AlphaThreshold, palette.WorkingColorSpace, null);
            }

            #endregion

            Debug.Assert(cfg.Size.Width > 0 && cfg.Size.Height > 0, "Non-empty size expected");
            Debug.Assert(cfg.PixelFormat.BitsPerPixel is > 0 and <= 128);
            Debug.Assert(cfg.Palette == null || cfg.Palette.BackColor == cfg.BackColor.ToOpaque()
                && cfg.Palette.AlphaThreshold == cfg.AlphaThreshold && (cfg.Palette.WorkingColorSpace == cfg.WorkingColorSpace || cfg.WorkingColorSpace == WorkingColorSpace.Default));

            disposeCallback = cfg.DisposeCallback;
            trySetPaletteCallback = cfg.TrySetPaletteCallback;
            Width = cfg.Size.Width;
            Height = cfg.Size.Height;
            BackColor = cfg.BackColor.ToOpaque();
            AlphaThreshold = cfg.AlphaThreshold;
            PixelFormat = cfg.PixelFormat;
            WorkingColorSpace = cfg.WorkingColorSpace;
            LinearWorkingColorSpace = this.GetPreferredColorSpace() == WorkingColorSpace.Linear;
            if (!cfg.PixelFormat.Indexed)
                return;

            int bpp = cfg.PixelFormat.BitsPerPixel;
            if (cfg.Palette != null)
            {
                if (cfg.Palette.Count > 1 << bpp)
                    throw new ArgumentException(Res.ImagingPaletteTooLarge(1 << bpp, bpp), nameof(cfg.Palette).ToLowerInvariant());
                Palette = cfg.Palette;
                LinearWorkingColorSpace = Palette.WorkingColorSpace == WorkingColorSpace.Linear;
            }
            else
                Palette = bpp switch
                {
                    > 8 => ExpandPalette(Palette.SystemDefault8BppPalette(WorkingColorSpace, cfg.BackColor, cfg.AlphaThreshold), bpp),
                    8 => Palette.SystemDefault8BppPalette(WorkingColorSpace, cfg.BackColor, cfg.AlphaThreshold),
                    > 4 => ExpandPalette(Palette.SystemDefault4BppPalette(WorkingColorSpace, cfg.BackColor), bpp),
                    4 => Palette.SystemDefault4BppPalette(WorkingColorSpace, cfg.BackColor),
                    > 1 => ExpandPalette(Palette.SystemDefault1BppPalette(WorkingColorSpace, cfg.BackColor), bpp),
                    _ => Palette.SystemDefault1BppPalette(WorkingColorSpace, cfg.BackColor)
                };

            AlphaThreshold = Palette!.AlphaThreshold;
        }

        #endregion

        #region Destructor

        ~BitmapDataBase() => Dispose(false);

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        #region Protected Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected static void ThrowDisposed() => throw new ObjectDisposedException(null, PublicResources.ObjectDisposed);

        #endregion

        #region Private Methods

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowYOutOfRange()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("y", PublicResources.ArgumentOutOfRange);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowXOutOfRange()
        {
            // ReSharper disable once NotResolvedInText
            throw new ArgumentOutOfRangeException("x", PublicResources.ArgumentOutOfRange);
        }

        #endregion

        #endregion

        #region Instance Methods

        #region Public Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color GetPixel(int x, int y) => GetColor32(x, y).ToColor();

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPixel(int x, int y, Color color) => SetColor32(x, y, new Color32(color));

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color32 GetColor32(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            return DoGetColor32(x, y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetColor32(int x, int y, Color32 color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            DoSetColor32(x, y, color);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public PColor32 GetPColor32(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            return DoGetPColor32(x, y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPColor32(int x, int y, PColor32 color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            DoSetPColor32(x, y, color);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public Color64 GetColor64(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            return DoGetColor64(x, y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetColor64(int x, int y, Color64 color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            DoSetColor64(x, y, color);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public PColor64 GetPColor64(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            return DoGetPColor64(x, y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPColor64(int x, int y, PColor64 color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            DoSetPColor64(x, y, color);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public ColorF GetColorF(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            return DoGetColorF(x, y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetColorF(int x, int y, ColorF color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            DoSetColorF(x, y, color);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public PColorF GetPColorF(int x, int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            return DoGetPColorF(x, y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public void SetPColorF(int x, int y, PColorF color)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            if ((uint)x >= Width)
                ThrowXOutOfRange();
            DoSetPColorF(x, y, color);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public IBitmapDataRowInternal GetRowUncached(int y) => DoGetRow(y);

        [MethodImpl(MethodImpl.AggressiveInlining)]
        public IBitmapDataRowInternal GetRowCached(int y) => GetCachedRowByThreadId(y);

        public bool TrySetPalette(Palette? palette)
        {
            if (!CanSetPalette || palette == null || palette.Count < Palette!.Count || palette.Count > 1 << PixelFormat.BitsPerPixel)
                return false;

            if (trySetPaletteCallback?.Invoke(palette) == false)
                return false;

            // Inheriting only the color entries from the palette because back color, alpha and working color space are read-only
            if (palette.BackColor == BackColor && palette.AlphaThreshold == AlphaThreshold && palette.WorkingColorSpace == WorkingColorSpace)
                Palette = palette;
            else
                Palette = new Palette(palette, WorkingColorSpace, BackColor, AlphaThreshold);

            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Protected Methods

        protected abstract IBitmapDataRowInternal DoGetRow(int y);
        protected abstract Color32 DoGetColor32(int x, int y);
        protected abstract void DoSetColor32(int x, int y, Color32 c);
        protected virtual PColor32 DoGetPColor32(int x, int y) => DoGetColor32(x, y).ToPColor32();
        protected virtual void DoSetPColor32(int x, int y, PColor32 c) => DoSetColor32(x, y, c.ToColor32());
        protected virtual Color64 DoGetColor64(int x, int y) => DoGetColor32(x, y).ToColor64();
        protected virtual void DoSetColor64(int x, int y, Color64 c) => DoSetColor32(x, y, c.ToColor32());
        protected virtual PColor64 DoGetPColor64(int x, int y) => DoGetColor32(x, y).ToPColor64();
        protected virtual void DoSetPColor64(int x, int y, PColor64 c) => DoSetColor32(x, y, c.ToColor32());
        protected virtual ColorF DoGetColorF(int x, int y) => DoGetColor32(x, y).ToColorF();
        protected virtual void DoSetColorF(int x, int y, ColorF c) => DoSetColor32(x, y, c.ToColor32());
        protected virtual PColorF DoGetPColorF(int x, int y) => DoGetColor32(x, y).ToPColorF();
        protected virtual void DoSetPColorF(int x, int y, PColorF c) => DoSetColor32(x, y, c.ToColor32());

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
                trySetPaletteCallback = null;
                cachedRowByIndex = null;
                cachedRowByThreadId = null;
                IsDisposed = true;
            }
        }

        #endregion

        #region Private Methods

        private IBitmapDataRowInternal GetFirstRow()
        {
            if (IsDisposed)
                ThrowDisposed();
            return DoGetRow(0);
        }

        private IBitmapDataRowInternal GetMovableRow(int y)
        {
            if (IsDisposed)
                ThrowDisposed();
            if ((uint)y >= Height)
                ThrowYOutOfRange();
            return DoGetRow(y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private IBitmapDataRowInternal GetCachedRowByIndex(int y)
        {
            // If the same row is accessed repeatedly we return the cached last row.
            // Note: this caching is exposed only to the indexer, which returns an immutable interface where Index cannot be changed
            IBitmapDataRowInternal? cached = cachedRowByIndex;
            if (cached?.Index == y)
                return cached;

            // Otherwise, we create and cache the result.
            return cachedRowByIndex = DoGetRow(y);
        }

        [MethodImpl(MethodImpl.AggressiveInlining)]
        private IBitmapDataRowInternal GetCachedRowByThreadId(int y)
        {
            if (cachedRowByThreadId == null)
                InitThreadIdCache();
            int threadId = EnvironmentHelper.CurrentThreadId;
            var hash = threadId & hashMask;
            StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>? cached = cachedRowByThreadId![hash];
            if (cached?.Value.ThreadId == threadId)
            {
                if (cached.Value.Row.Index != y)
                    cached.Value.Row.DoMoveToRow(y);
            }
            else
                cachedRowByThreadId[hash] = cached = new StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>((threadId, DoGetRow(y)));
            return cached.Value.Row;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitThreadIdCache()
        {
            var result = new StrongBox<(int ThreadId, IBitmapDataRowInternal Row)>[Math.Max(8, ((uint)Environment.ProcessorCount << 1).RoundUpToPowerOf2())];
            hashMask = result.Length - 1;
            cachedRowByThreadId = result;
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        IReadableBitmapDataRowMovable IReadableBitmapData.GetMovableRow(int y) => GetMovableRow(y);
        IWritableBitmapDataRowMovable IWritableBitmapData.GetMovableRow(int y) => GetMovableRow(y);
        IReadWriteBitmapDataRowMovable IReadWriteBitmapData.GetMovableRow(int y) => GetMovableRow(y);

        #endregion

        #endregion

        #endregion
    }

}
