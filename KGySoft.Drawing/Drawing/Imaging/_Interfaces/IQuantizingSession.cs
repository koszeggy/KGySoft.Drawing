#region Copyright

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
    public interface IQuantizingSession : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the palette if the number of colors are limited up to 256 colors; otherwise, can be <see langword="null"/>&#160;or empty.
        /// </summary>
        Color32[] Palette { get; }

        /// <summary>
        /// Gets the background color for this <see cref="IQuantizingSession"/>.
        /// Colors with alpha above the <see cref="AlphaThreshold"/> will be blended with this color before quantizing.
        /// <br/>See the also <strong>Remarks</strong> section if the <see cref="AlphaThreshold"/> property for details.
        /// </summary>
        Color32 BackColor { get; }

        /// <summary>
        /// If this <see cref="IQuantizingSession"/> can produce transparent pixels, then gets the alpha threshold value for quantizing colors.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <remarks>
        /// <para>If this <see cref="IQuantizingSession"/> can produce transparent pixels, and <see cref="GetQuantizedColor">GetQuantizedColor</see> is called with a color,
        /// whose <see cref="Color32.A">Color32.A</see> property is lower than the threshold, then the returned color will be transparent.</para>
        /// <para>If this <see cref="IQuantizingSession"/> cannot produce transparent pixels, or <see cref="GetQuantizedColor">GetQuantizedColor</see> is called with a color,
        /// whose <see cref="Color32.A">Color32.A</see> property is greater than or equal to the threshold, then the returned color will be will be blended with <see cref="BackColor"/> before quantizing.</para>
        /// <para>If <see cref="AlphaThreshold"/> is <c>0</c>, then the quantized color will never be transparent.</para>
        /// <para>If <see cref="AlphaThreshold"/> is <c>255</c>, then only fully opaque pixels will not be considered as transparent ones.</para>
        /// </remarks>
        byte AlphaThreshold { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the quantized color of the specified <paramref name="origColor"/>.
        /// </summary>
        /// <param name="origColor">The original color to be quantized.</param>
        /// <returns>The quantized color.</returns>
        Color32 GetQuantizedColor(Color32 origColor);

        #endregion
    }
}