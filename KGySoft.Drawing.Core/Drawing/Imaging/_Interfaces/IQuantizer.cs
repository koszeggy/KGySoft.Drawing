#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IQuantizer.cs
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

using KGySoft.Threading;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a quantizer that can be used to reduce the number of colors of an image.
    /// <br/>For built-in implementations see the <see cref="PredefinedColorsQuantizer"/> and <see cref="OptimizedPaletteQuantizer"/> classes.
    /// </summary>
    /// <seealso cref="PredefinedColorsQuantizer"/>
    /// <seealso cref="OptimizedPaletteQuantizer"/>
    /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer?, IDitherer?)"/>
    /// <seealso cref="BitmapDataExtensions.Quantize(IReadWriteBitmapData, IQuantizer)"/>
    public interface IQuantizer
    {
        #region Properties

        /// <summary>
        /// Gets a <see cref="KnownPixelFormat"/> that is compatible with this <see cref="IQuantizer"/>.
        /// It is recommended to return the format with the lowest bits-per-pixel value that is still compatible with this instance.
        /// </summary>
        KnownPixelFormat PixelFormatHint { get; }

        /// <summary>
        /// Gets whether <see cref="Initialize">Initialize</see> relies on the actual content of the source to be quantized.
        /// </summary>
        /// <remarks>
        /// <para>This property should return <see langword="true"/>, if <see cref="Initialize">Initialize</see> method relies on the exact content to be quantized
        /// in order to generate a palette and make the <see cref="IQuantizingSession.GetQuantizedColor">GetQuantizedColor</see> method work properly.</para>
        /// <para>If this property returns <see langword="false"/>, then <see cref="Initialize">Initialize</see> will be called with an <see cref="IReadableBitmapData"/>
        /// instance, whose <see cref="IBitmapData.Height"/> and <see cref="IBitmapData.Width"/> properties can be used but the actual content might be different
        /// from the one will be used when calling the <see cref="IQuantizingSession.GetQuantizedColor">GetQuantizedColor</see> method.</para>
        /// <para>The return value of this property may affect the performance of some drawing operations where returning <see langword="true"/> forces the source and
        /// target images to be blended together before quantizing the result, while returning <see langword="false"/> allows the quantizer to be initialized
        /// with the source image and let the <see cref="IQuantizingSession.GetQuantizedColor">GetQuantizedColor</see> method to be called with colors that are blended on-the-fly during the quantization.</para>
        /// </remarks>
        bool InitializeReliesOnContent { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an <see cref="IQuantizingSession"/> instance that can be used to quantize the colors of the specified <see cref="IReadableBitmapData"/> instance.
        /// </summary>
        /// <param name="source">The quantizing session to be initialized will be performed on the specified <see cref="IReadableBitmapData"/> instance.</param>
        /// <param name="asyncContext">Contains information for asynchronous processing about the current operation. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IQuantizingSession"/> instance that can be used to quantize the colors of the specified <see cref="IReadableBitmapData"/> instance.</returns>
        IQuantizingSession Initialize(IReadableBitmapData source, IAsyncContext? asyncContext = null);

        #endregion
    }
}