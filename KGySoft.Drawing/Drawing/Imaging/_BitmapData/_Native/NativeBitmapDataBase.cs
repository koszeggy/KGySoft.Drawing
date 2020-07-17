#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataBase.cs
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
using System.Linq;
using System.Security;

using KGySoft.Drawing.WinApi;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal abstract class NativeBitmapDataBase : BitmapDataBase
    {
        #region Fields

        private readonly Bitmap bitmap;
        private readonly BitmapData bitmapData;

        #endregion

        #region Properties

        #region Public Properties

        public override int Height => bitmapData.Height;

        public override int Width => bitmapData.Width;

        public override PixelFormat PixelFormat => bitmapData.PixelFormat;

        public override int RowSize { get; }

        #endregion

        #region Internal Properties

        internal IntPtr Scan0 => bitmapData.Scan0;
        internal int Stride => bitmapData.Stride;

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        [SecuritySafeCritical]
        protected NativeBitmapDataBase(Bitmap bitmap, PixelFormat pixelFormat, ImageLockMode lockMode, Color32 backColor, byte alphaThreshold, Palette palette)
        {
            // It must be the same as bitmap format except if LockBits is not supported with the original pixel format (occurs on Linux).
            Debug.Assert(bitmap.PixelFormat == pixelFormat || !OSUtils.IsWindows, "Unmatching pixel format");

            // If palette is passed it must match with actual palette
            Debug.Assert(palette == null || palette.Equals(bitmap.Palette.Entries), "Unmatching palette");

            this.bitmap = bitmap;
            BackColor = pixelFormat.HasMultiLevelAlpha() ? default : backColor.ToOpaque();
            AlphaThreshold = alphaThreshold;

            bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), lockMode, pixelFormat);
            RowSize = Math.Abs(bitmapData.Stride);

            if (pixelFormat.IsIndexed())
                Palette = palette ?? new Palette(bitmap.Palette.Entries, backColor.ToColor(), alphaThreshold);
        }

        #endregion

        #region Destructor

        ~NativeBitmapDataBase() => Dispose(false);

        #endregion

        #endregion

        #region Methods

        #region Protected Methods

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
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
                base.Dispose(disposing);
            }
        }

        #endregion

        #endregion
    }
}
