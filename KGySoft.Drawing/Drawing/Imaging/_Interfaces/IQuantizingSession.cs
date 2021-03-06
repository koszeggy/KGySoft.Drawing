﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IQuantizingSession.cs
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

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a disposable quantizing session returned by the <see cref="IQuantizer.Initialize">IQuantizer.Initialize</see> method
    /// that is used to quantize (reduce the colors) of a specific <see cref="IReadableBitmapData"/> source.
    /// </summary>
    /// <seealso cref="IQuantizer" />
    public interface IQuantizingSession : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the palette containing the colors of the quantized result. Expected to be called if the result of the quantization
        /// is an indexed image and in that case should not contain more than 256 colors.
        /// If this <see cref="IQuantizingSession"/> is not limited to use up to 256 colors, then this property can return <see langword="null"/>.
        /// </summary>
        Palette? Palette { get; }

        /// <summary>
        /// Gets the background color for this <see cref="IQuantizingSession"/>.
        /// When the <see cref="GetQuantizedColor">GetQuantizedColor</see> is called with a color with alpha,
        /// which is considered opaque, then it will be blended with this color before quantizing.
        /// <br/>See also the <strong>Remarks</strong> section of the <see cref="AlphaThreshold"/> property for details.
        /// </summary>
        Color32 BackColor { get; }

        /// <summary>
        /// If this <see cref="IQuantizingSession"/> can produce transparent pixels, then gets the alpha threshold value
        /// that can be used to determine whether a color with alpha should be considered transparent or should be blended with <see cref="BackColor"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <remarks>
        /// <para>If this <see cref="IQuantizingSession"/> can produce transparent pixels, and <see cref="GetQuantizedColor">GetQuantizedColor</see> is called with a color,
        /// whose <see cref="Color32.A">Color32.A</see> field is lower than the threshold, then the returned color will be transparent.</para>
        /// <para>If this <see cref="IQuantizingSession"/> cannot produce transparent pixels, or <see cref="GetQuantizedColor">GetQuantizedColor</see> is called with a color,
        /// whose <see cref="Color32.A">Color32.A</see> field is greater than or equal to the threshold, then the returned color will be will be blended with <see cref="BackColor"/> before quantizing.</para>
        /// <para>If <see cref="AlphaThreshold"/> is 0, then the quantized color will never be transparent.</para>
        /// <para>If <see cref="AlphaThreshold"/> is 255, then only fully opaque colors will not be considered transparent.</para>
        /// </remarks>
        byte AlphaThreshold { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the quantized color of the specified <paramref name="origColor"/>. If <see cref="Palette"/> property has non-<see langword="null"/>&#160;return value,
        /// then the result color must be a valid <see cref="Imaging.Palette"/> entry.
        /// <br/>See the also <strong>Remarks</strong> section of the <see cref="AlphaThreshold"/> property for details.
        /// </summary>
        /// <param name="origColor">The original color to be quantized.</param>
        /// <returns>The quantized color.</returns>
        Color32 GetQuantizedColor(Color32 origColor);

        #endregion
    }
}