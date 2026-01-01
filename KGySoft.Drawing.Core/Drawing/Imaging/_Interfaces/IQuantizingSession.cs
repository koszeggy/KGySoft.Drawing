#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IQuantizingSession.cs
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
        /// Gets the palette containing the colors of the quantized result. Expected to be called if the target of the quantization
        /// is indexed. Typical indexed pixel formats contain no more than 256 colors; however, it is allowed to return a bigger <see cref="Imaging.Palette"/>.
        /// If the corresponding <see cref="IQuantizer"/> returns a non-indexed pixel format in its <see cref="IQuantizer.PixelFormatHint"/> property, then this property can return <see langword="null"/>.
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
        /// </summary>
        /// <remarks>
        /// <para>If this <see cref="IQuantizingSession"/> can produce transparent pixels, and <see cref="GetQuantizedColor">GetQuantizedColor</see> is called with a color,
        /// whose <see cref="Color32.A">Color32.A</see> field is lower than the threshold, then the returned color will be transparent.</para>
        /// <para>If this <see cref="IQuantizingSession"/> cannot produce transparent pixels, or <see cref="GetQuantizedColor">GetQuantizedColor</see> is called with a color,
        /// whose <see cref="Color32.A">Color32.A</see> field is greater than or equal to the threshold, then the returned color will be blended with <see cref="BackColor"/> before quantizing.</para>
        /// <para>If <see cref="AlphaThreshold"/> is 0, then the quantized color will never be transparent.</para>
        /// <para>If <see cref="AlphaThreshold"/> is 255, then only fully opaque colors will not be considered transparent.</para>
        /// </remarks>
        byte AlphaThreshold { get; }

        /// <summary>
        /// Gets whether this <see cref="IQuantizingSession"/> works with grayscale colors.
        /// Its value may help to optimize the processing in some cases, but it is allowed to return always <see langword="false"/>.
        /// <br/>Default value if not implemented: <see langword="false"/>. (Only in .NET Core 3.0/.NET Standard 2.1 and above. In earlier targeted frameworks this member must be implemented.)
        /// </summary>
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
        bool IsGrayscale { get; }
#else
        bool IsGrayscale => false;
#endif

        /// <summary>
        /// Gets the preferred working color space of this <see cref="IQuantizingSession"/> instance.
        /// If this quantizing session is used by a ditherer, then <see cref="IDitheringSession"/> implementations may also respect the value of this property.
        /// <br/>Default value if not implemented: <see cref="Imaging.WorkingColorSpace.Default"/>. (Only in .NET Core 3.0/.NET Standard 2.1 and above. In earlier targeted frameworks this member must be implemented.)
        /// </summary>
        /// <remarks>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Imaging.WorkingColorSpace"/> enumeration for details and
        /// image examples about using the different color spaces in various operations.</note>
        /// </remarks>
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
        WorkingColorSpace WorkingColorSpace { get; }
#else
        WorkingColorSpace WorkingColorSpace => default;
#endif

        #endregion

        #region Methods

        /// <summary>
        /// Gets the quantized color of the specified <paramref name="origColor"/>. If <see cref="Palette"/> property has non-<see langword="null"/> return value,
        /// then the result color must be a valid <see cref="Imaging.Palette"/> entry.
        /// <br/>See also the <strong>Remarks</strong> section of the <see cref="AlphaThreshold"/> property for details.
        /// </summary>
        /// <param name="origColor">The original color to be quantized.</param>
        /// <returns>The quantized color.</returns>
        Color32 GetQuantizedColor(Color32 origColor);

        #endregion
    }
}