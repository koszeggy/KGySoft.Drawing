#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IQuantizer.cs
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

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a quantizer that can be used to reduce the number of colors of an image.
    /// <br/>For built-in implementations see the <see cref="PredefinedColorsQuantizer"/> and <see cref="OptimizedPaletteQuantizer"/> classes.
    /// </summary>
    /// <seealso cref="PredefinedColorsQuantizer"/>
    /// <seealso cref="OptimizedPaletteQuantizer"/>
    /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
    /// <seealso cref="BitmapExtensions.Quantize"/>
    public interface IQuantizer
    {
        #region Methods

        /// <summary>
        /// Gets an <see cref="IQuantizingSession"/> instance that can be used to quantize the colors of the specified <see cref="IReadableBitmapData"/> instance.
        /// </summary>
        /// <param name="source">The quantizing session to be initialized will be performed on the specified <see cref="IReadableBitmapData"/> instance.</param>
        /// <returns>An <see cref="IQuantizingSession"/> instance that can be used to quantize the colors of the specified <see cref="IReadableBitmapData"/> instance.</returns>
        IQuantizingSession Initialize(IReadableBitmapData source);

        #endregion
    }
}