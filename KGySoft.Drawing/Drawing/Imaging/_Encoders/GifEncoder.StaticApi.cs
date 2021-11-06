#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifEncoder.Encode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

using System;
using System.Drawing.Imaging;
using System.IO;

namespace KGySoft.Drawing.Imaging
{
    public partial class GifEncoder
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Encodes the specified <paramref name="imageData"/> as a GIF image and writes it into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="imageData">The image data to write. Non-indexed images will be quantized by using the <see cref="GlobalPalette"/>, or, if that is not set,
        /// by <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette"/> using no dithering.</param>
        /// <param name="stream">The stream to save the encoded image into.</param>
        /// <param name="quantizer">An optional <see cref="IQuantizer"/> instance to determine the colors of the result.
        /// If <see langword="null"/>&#160;and <paramref name="imageData"/> is not an indexed image,
        /// then <see cref="PredefinedColorsQuantizer.SystemDefault8BppPalette"/> (or "web-safe" palette) will be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="ditherer">The ditherer to be used. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="imageData"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        public static void EncodeImage(IReadableBitmapData imageData, Stream stream, IQuantizer? quantizer = null, IDitherer? ditherer = null)
        {
            if (imageData == null)
                throw new ArgumentNullException(nameof(imageData), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);

            IReadableBitmapData source = GetFirstFrame(imageData, quantizer, ditherer);

            try
            {
                Palette palette = source.Palette!;
                new GifEncoder(stream, imageData.GetSize())
                    {
                        GlobalPalette = palette,
                        BackColorIndex = (byte)(palette.HasAlpha ? palette.TransparentIndex : 0),
#if DEBUG
                        AddMetaInfo = true,
#endif
                    }
                    .AddImage(source)
                    .FinalizeEncoding();
            }
            finally
            {
                if (!ReferenceEquals(source, imageData))
                    source.Dispose();
            }
        }

        #endregion

        #region Private Methods

        private static IReadableBitmapData GetFirstFrame(IReadableBitmapData imageData, IQuantizer? quantizer, IDitherer? ditherer)
        {
            Palette? palette = imageData.Palette;
            
            // Indexed source without a quantizer and multi-level alpha: a good candidate to return the bitmap data itself
            if (quantizer == null && imageData.PixelFormat.IsIndexed() && palette?.Count.ToBitsPerPixel() <= imageData.PixelFormat.ToBitsPerPixel() && !palette.HasMultiLevelAlpha)
            {
                // no transparency: we are done
                if (!palette.HasAlpha)
                    return imageData;

                // we need to check whether the palette has multiple transparent entries
                bool multiAlpha = false;
                int transparentIndex = palette.TransparentIndex;
                for (int i = 0; i < palette.Count; i++)
                {
                    if (palette[i].A == 0 && i != transparentIndex)
                    {
                        multiAlpha = true;
                        break;
                    }
                }

                // no multiple transparent entries
                if (!multiAlpha)
                    return imageData;

                // we need to scan the image to check whether non-first transparent index is in use
                multiAlpha = false;
                IReadableBitmapDataRow row = imageData.FirstRow;
                do
                {
                    for (int x = 0; x < imageData.Width; x++)
                    {
                        int index = row.GetColorIndex(x);
                        if (index != transparentIndex && palette[index].A == 0)
                        {
                            multiAlpha = true;
                            break;
                        }
                    }
                } while (row.MoveNextRow());

                // There are no transparent pixels of different palette entries (in AddImage this will not be checked again)
                if (!multiAlpha)
                    return imageData;
            }

            return imageData.Clone(PixelFormat.Format8bppIndexed, quantizer, ditherer);
        }

        #endregion

        #endregion
    }
}