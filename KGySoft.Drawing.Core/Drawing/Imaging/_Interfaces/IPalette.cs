#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IPalette.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
    /// Represents the properties of a <see cref="Palette"/> instance that can be accessed from custom color lookup functions.
    /// </summary>
    public interface IPalette
    {
        #region Properties and Indexers

        #region Properties

        /// <summary>
        /// Gets the number of color entries in the current <see cref="IPalette"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the background color. If a lookup operation
        /// is performed with a color whose <see cref="Color32.A">Color32.A</see> field is equal to or greater than <see cref="AlphaThreshold"/>,
        /// and there is no exact match among the entries of this <see cref="IPalette"/>,
        /// then the color will be blended with this color before performing the lookup.
        /// </summary>
        Color32 BackColor { get; }

        /// <summary>
        /// If this <see cref="IPalette"/> has a transparent entry, then gets a threshold value for the <see cref="Color32.A">Color32.A</see> field,
        /// under which lookup operations will return the first transparent color in the palette.
        /// </summary>
        byte AlphaThreshold { get; }

        /// <summary>
        /// Gets whether the palette consists of grayscale entries only.
        /// </summary>
        bool IsGrayscale { get; }

        /// <summary>
        /// Gets whether the palette contains at least one entry that is not fully opaque.
        /// </summary>
        bool HasAlpha { get; }

        /// <summary>
        /// Gets the preferred color space when this <see cref="IPalette"/> instance performs blending and measuring distance when looking for a nearest color.
        /// </summary>
        /// <remarks>
        /// <note type="tip">See the <strong>Remarks</strong> section of the <see cref="Imaging.WorkingColorSpace"/> enumeration for details and
        /// image examples about using the different color spaces in various operations.</note>
        /// <para>If the value of this property is <see cref="Imaging.WorkingColorSpace.Default"/>, then the sRGB color space is used.</para>
        /// <para>If this palette uses a custom nearest color lookup, then it depends on the custom function whether it considers the value of this property.</para>
        /// <note>Please note that palette entries themselves always represent sRGB color values, regardless the value of this property.</note>
        /// </remarks>
        WorkingColorSpace WorkingColorSpace { get; }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets the color entry of this <see cref="IPalette"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the color entry to be retrieved.</param>
        /// <returns>A <see cref="Color32"/> instance representing the color entry of the <see cref="Palette"/> at the specified <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> must be equal to or greater than zero and less <see cref="Count"/>.</exception>
        Color32 this[int index] { get; }

        #endregion

        #endregion
    }
}
