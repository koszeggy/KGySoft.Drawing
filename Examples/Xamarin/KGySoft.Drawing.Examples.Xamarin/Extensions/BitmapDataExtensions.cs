#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.cs
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
using System.Threading.Tasks;

using KGySoft.Drawing.Imaging;
using KGySoft.Threading;

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.Examples.Xamarin.Extensions
{
    internal static class BitmapDataExtensions
    {
        #region Methods

        #region Internal Methods
        
        internal static Task<SKBitmap?> ToSKBitmapAsync(this IReadableBitmapData bitmapData, IQuantizer? quantizer, IDitherer? ditherer, TaskConfig? asyncConfig)
        {
            if (bitmapData == null)
                throw new ArgumentNullException(nameof(bitmapData));

            return AsyncHelper.DoOperationAsync(ctx => ToSKBitmap(ctx, bitmapData, quantizer, ditherer), asyncConfig);
        }

        #endregion

        #region Private Methods

        private static SKBitmap? ToSKBitmap(IAsyncContext context, IReadableBitmapData source, IQuantizer? quantizer, IDitherer? ditherer)
        {
            if (context.IsCancellationRequested)
                return null;

            var result = new SKBitmap(new SKImageInfo(source.Width, source.Height));
            try
            {
                using IReadWriteBitmapData target = result.GetReadWriteBitmapData(source.WorkingColorSpace);
                source.CopyTo(target, context, new Rectangle(Point.Empty, source.Size), Point.Empty, quantizer, ditherer);
            }
            finally
            {
                if (context.IsCancellationRequested)
                {
                    result.Dispose();
                    result = null;
                }
            }

            return result;
        }

        #endregion

        #endregion
    }
}