#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataBase.cs
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
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class NativeBitmapDataBase : BitmapDataBase
    {
        #region Fields

        private Action? disposeCallback;
        private Action<Palette>? setPalette;

        #endregion

        #region Properties

        internal IntPtr Scan0 { get; }
        internal int Stride { get; }

        #endregion

        #region Construction and Destruction

        #region Constructors

        protected NativeBitmapDataBase(IntPtr buffer, Size size, int stride, PixelFormat pixelFormat, Color32 backColor, byte alphaThreshold,
            Palette? palette, Action<Palette>? setPalette, Action? disposeCallback)
        {
            Debug.Assert(buffer != IntPtr.Zero);
            Debug.Assert(size.Width > 0 && size.Height > 0);
            Debug.Assert(pixelFormat.ToBitsPerPixel() > 0);
            Debug.Assert(palette == null || palette.BackColor == backColor.ToOpaque() && palette.AlphaThreshold == alphaThreshold);

            this.disposeCallback = disposeCallback;
            this.setPalette = setPalette;
            Width = size.Width;
            Height = size.Height;
            PixelFormat = pixelFormat;
            Scan0 = buffer;
            Stride = stride;
            BackColor = pixelFormat.HasMultiLevelAlpha() ? default : backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;
            RowSize = Math.Abs(stride);

            if (pixelFormat.IsIndexed())
            {
                Palette = palette ?? pixelFormat switch
                {
                    PixelFormat.Format8bppIndexed => Palette.SystemDefault8BppPalette(backColor, alphaThreshold),
                    PixelFormat.Format4bppIndexed => Palette.SystemDefault4BppPalette(backColor),
                    PixelFormat.Format1bppIndexed => Palette.SystemDefault1BppPalette(backColor),
                    _ => throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format for palette: {pixelFormat}"))
                };

                AlphaThreshold = Palette.AlphaThreshold;
            }
        }

        #endregion

        #region Destructor

        ~NativeBitmapDataBase() => Dispose(false);

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        public sealed override bool CanSetPalette => base.CanSetPalette && setPalette != null;

        public sealed override bool TrySetPalette(Palette? palette)
        {
            if (setPalette == null || !base.TrySetPalette(palette))
                return false;
            setPalette.Invoke(palette!);
            return true;
        }

        #endregion

        #region Protectd Methods

        protected override void Dispose(bool disposing)
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
                base.Dispose(disposing);
            }
        }

        #endregion

        #endregion
    }
}