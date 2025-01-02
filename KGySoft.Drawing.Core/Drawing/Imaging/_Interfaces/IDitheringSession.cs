#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IDitheringSession.cs
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

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a disposable dithering session returned by the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method
    /// that is used to dither the result of a quantizing session for a specific <see cref="IReadableBitmapData"/> source.
    /// </summary>
    /// <seealso cref="IDitherer" />
    public interface IDitheringSession : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets whether this ditherer allows only sequential processing (line by line). Even returning <see langword="true"/> does
        /// not guarantee that every pixel will be queried. It just enforces that queried rows are processed sequentially.
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, then the <see cref="GetDitheredColor">GetDitheredColor</see> method will be called sequentially for each queried pixels.
        /// If <see langword="false"/>, then the <see cref="GetDitheredColor">GetDitheredColor</see> method can be called concurrently for any pixels.
        /// </value>
        bool IsSequential { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the dithered color of the specified <paramref name="origColor"/> that may depend on the specified coordinates.
        /// The returned color should be quantized by the <see cref="IQuantizingSession"/> passed to the <see cref="IDitherer.Initialize">IDitherer.Initialize</see> method,
        /// which was used to create this <see cref="IDitheringSession"/> instance.
        /// </summary>
        /// <param name="origColor">The original color to be dithered.</param>
        /// <param name="x">The x-coordinate of the pixel to be dithered represented by the specified <paramref name="origColor"/>.</param>
        /// <param name="y">The y-coordinate of the pixel to be dithered represented by the specified <paramref name="origColor"/>.</param>
        /// <returns>The dithered color of the specified <paramref name="origColor"/> that may depend on the specified coordinates.</returns>
        Color32 GetDitheredColor(Color32 origColor, int x, int y);

        #endregion
    }
}