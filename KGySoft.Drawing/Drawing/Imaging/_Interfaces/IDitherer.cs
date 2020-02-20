#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IDitherer.cs
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
    /// Represents a ditherer that can be used to dither the result of a quantizing session.
    /// <br/>For built-in implementations see the <see cref="OrderedDitherer"/>, <see cref="ErrorDiffusionDitherer"/>,
    /// <see cref="RandomNoiseDitherer"/> and <see cref="InterleavedGradientNoiseDitherer"/> classes.
    /// </summary>
    /// <seealso cref="OrderedDitherer"/>
    /// <seealso cref="ErrorDiffusionDitherer"/>
    /// <seealso cref="RandomNoiseDitherer"/>
    /// <seealso cref="InterleavedGradientNoiseDitherer"/>
    /// <seealso cref="ImageExtensions.ConvertPixelFormat(Image, PixelFormat, IQuantizer, IDitherer)"/>
    /// <seealso cref="BitmapExtensions.Dither"/>
    public interface IDitherer
    {
        #region Methods

        /// <summary>
        /// Gets an <see cref="IDitheringSession"/> instance that can be used to dither the result of the specified <see cref="IQuantizingSession"/>
        /// applied to the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The dithering session to be initialized will be performed on the specified <see cref="IReadableBitmapData"/> instance.</param>
        /// <param name="quantizingSession">The <see cref="IQuantizingSession"/> to which the dithering should be applied.</param>
        /// <returns>An <see cref="IDitheringSession"/> instance that can be used to dither the result of the specified <see cref="IQuantizingSession"/>
        /// applied to the specified <paramref name="source"/>.</returns>
        IDitheringSession Initialize(IReadableBitmapData source, IQuantizingSession quantizingSession);

        #endregion
    }
}