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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using KGySoft.CoreLibraries;

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
        /// If <see langword="null"/>&#160;and <paramref name="imageData"/> is not an indexed image or the image contains different alpha pixels,
        /// then <see cref="OptimizedPaletteQuantizer.Wu"/> quantizer will be used. This parameter is optional.
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

            IReadableBitmapData source = quantizer == null && imageData.PixelFormat.IsIndexed() && !HasMultipleTransparentIndices(imageData)
                    ? imageData
                    : imageData.Clone(PixelFormat.Format8bppIndexed, quantizer ?? OptimizedPaletteQuantizer.Wu(), ditherer);

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

        /// <summary>
        /// Encodes the frames of the specified <paramref name="options"/> as an animated GIF image and writes it into the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="options">A <see cref="AnimGifConfig"/> instance describing the parameters of the encoding.</param>
        /// <param name="stream">The stream to save the encoded animation into.</param>
        /// <exception cref="ArgumentNullException"><paramref name="options"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="options"/> is invalid.</exception>
        public static void EncodeAnimation(AnimGifConfig options, Stream stream)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            if (options.AnimationMode < AnimationMode.PingPong || (int)options.AnimationMode > UInt16.MaxValue)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(options.AnimationMode), PublicResources.ArgumentOutOfRange), nameof(options));
            if (!options.SizeHandling.IsDefined())
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(options.SizeHandling), PublicResources.EnumOutOfRange(options.SizeHandling)), nameof(options));

            using var enumerator = new FramesEnumerator(options);
            using GifEncoder encoder = enumerator.CreateEncoder(stream);
            while (enumerator.MoveNext())
                encoder.AddImage(enumerator.Frame!, enumerator.Location, enumerator.Delay, enumerator.DisposalMethod);
        }

        #endregion

        #region Private Methods

        private static bool HasMultipleTransparentIndices(IReadableBitmapData imageData)
        {
            Debug.Assert(imageData.PixelFormat.IsIndexed());
            Palette? palette = imageData.Palette;

            // There is no palette ir it is too large: returning true to force a quantization
            if (palette == null || palette.Count > 256)
                return true;

            // no transparency: we are done
            if (!palette.HasAlpha)
                return false;

            // we need to check whether the palette has multiple transparent entries (or entries with partial transparency)
            bool multiAlpha = false;
            int transparentIndex = palette.TransparentIndex;
            for (int i = 0; i < palette.Count; i++)
            {
                if (palette[i].A < Byte.MaxValue && i != transparentIndex)
                {
                    multiAlpha = true;
                    break;
                }
            }

            // no multiple transparent entries
            if (!multiAlpha)
                return false;

            // we need to scan the image to check whether alpha pixels other than transparent index is in use
            IReadableBitmapDataRow row = imageData.FirstRow;
            do
            {
                for (int x = 0; x < imageData.Width; x++)
                {
                    int index = row.GetColorIndex(x);
                    if (index != transparentIndex && palette[index].A < Byte.MaxValue)
                        return true;
                }
            } while (row.MoveNextRow());

            return false;
        }

        private static Rectangle GetContentArea(IReadableBitmapData imageData)
        {
            Rectangle result = new Rectangle(Point.Empty, imageData.GetSize());
            if (!imageData.HasAlpha())
                return result;

            IReadableBitmapDataRow row = imageData.FirstRow;
            do
            {
                for (int x = 0; x < result.Width; x++)
                {
                    if (row[x].A != 0)
                        goto continueBottom;
                }

                result.Y += 1;
                result.Height -= 1;
            } while (row.MoveNextRow());

        continueBottom:
            // fully transparent image: returning 1x1 at the center
            if (result.Height == 0)
                return new Rectangle(imageData.Width >> 1, imageData.Height >> 1, 1, 1);

            for (int y = result.Bottom - 1; y >= result.Top; y--)
            {
                row = imageData[y];
                for (int x = 0; x < result.Width; x++)
                {
                    if (row[x].A != 0)
                        goto continueLeft;
                }

                result.Height -= 1;
            }

        continueLeft:
            Debug.Assert(result.Height > 0);
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = result.Top; y < result.Bottom; y++)
                {
                    if (imageData[y][x].A != 0)
                        goto continueRight;
                }

                result.X += 1;
                result.Width -= 1;
            }

        continueRight:
            Debug.Assert(result.Width > 0);
            for (int x = result.Right - 1; x >= result.Left; x--)
            {
                for (int y = result.Top; y < result.Bottom; y++)
                {
                    if (imageData[y][x].A != 0)
                        return result;
                }

                result.Width -= 1;
            }

            throw new InvalidOperationException(Res.InternalError("Empty result is not expected at this point"));
        }

        #endregion

        #endregion
    }
}