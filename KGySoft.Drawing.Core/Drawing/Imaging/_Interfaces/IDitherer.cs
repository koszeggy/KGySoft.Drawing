#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IDitherer.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
    /// Represents a ditherer that can be used to dither the result of a quantizing session.
    /// <br/>For built-in implementations see the <see cref="OrderedDitherer"/>, <see cref="ErrorDiffusionDitherer"/>,
    /// <see cref="RandomNoiseDitherer"/> and <see cref="InterleavedGradientNoiseDitherer"/> classes.
    /// </summary>
    /// <seealso cref="OrderedDitherer"/>
    /// <seealso cref="ErrorDiffusionDitherer"/>
    /// <seealso cref="RandomNoiseDitherer"/>
    /// <seealso cref="InterleavedGradientNoiseDitherer"/>
    /// <seealso cref="BitmapDataExtensions.Clone(IReadableBitmapData, KnownPixelFormat, IQuantizer?, IDitherer?)"/>
    /// <seealso cref="BitmapDataExtensions.Dither(IReadWriteBitmapData, IQuantizer, IDitherer)"/>
    public interface IDitherer
    {
        #region Properties

        /// <summary>
        /// Gets whether <see cref="Initialize">Initialize</see> relies on the actual content of the source to be dithered.
        /// </summary>
        /// <remarks>
        /// <para>This property should return <see langword="true"/>, if <see cref="Initialize">Initialize</see> method relies on the exact content to be dithered
        /// in order to make the <see cref="IDitheringSession.GetDitheredColor">GetDitheredColor</see> method work properly.</para>
        /// <para>If this property returns <see langword="false"/>, then <see cref="Initialize">Initialize</see> will be called with an <see cref="IReadableBitmapData"/>
        /// instance, whose <see cref="IBitmapData.Height"/> and <see cref="IBitmapData.Width"/> properties can be used but the actual content might be different
        /// from the one will be used when calling the <see cref="IDitheringSession.GetDitheredColor">GetDitheredColor</see> method.</para>
        /// <para>The return value of this property may affect the performance of some drawing operations where returning <see langword="true"/> forces the source and
        /// target images to be blended together before dithering the result, while returning <see langword="false"/> allows the ditherer to be initialized
        /// with the source image and let the <see cref="IDitheringSession.GetDitheredColor">GetDitheredColor</see> method to be called with colors that are blended on-the-fly during the dithering.</para>
        /// </remarks>
        bool InitializeReliesOnContent { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an <see cref="IDitheringSession"/> instance that can be used to dither the result of the specified <see cref="IQuantizingSession"/>
        /// applied to the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The dithering session to be initialized will be performed on the specified <see cref="IReadableBitmapData"/> instance.</param>
        /// <param name="quantizingSession">The <see cref="IQuantizingSession"/> to which the dithering should be applied.</param>
        /// <param name="asyncContext">Contains information for asynchronous processing about the current operation. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IDitheringSession"/> instance that can be used to dither the result of the specified <see cref="IQuantizingSession"/>
        /// applied to the specified <paramref name="source"/>.</returns>
        IDitheringSession Initialize(IReadableBitmapData source, IQuantizingSession quantizingSession, IAsyncContext? asyncContext = null);

        #endregion
    }
}