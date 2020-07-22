#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataExtensions.ReadWrite.cs
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
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    partial class BitmapDataExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Clips the specified <paramref name="source"/> using the specified <paramref name="clippingRegion"/>.
        /// Unlike the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.Clone">Clone</see> methods, this one returns a wrapper,
        /// providing access only to the specified region of the original <paramref name="source"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="source">The source bitmap data to be clipped.</param>
        /// <param name="clippingRegion">A <see cref="Rectangle"/> that specifies a region within the <paramref name="source"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> that provides access only to the specified region withing the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="clippingRegion"/> has no overlapping region with source bounds.</exception>
        /// <remarks>
        /// <para>The <see cref="IBitmapData.RowSize"/> property of the returned instance can be 0, indicating that the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>/<see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see>
        /// methods cannot be used. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the left edge of the clipping is not on byte boundary.</para>
        /// <para>Even if <see cref="IBitmapData.RowSize"/> property of the returned instance is a nonzero value it can happen that it is too low to access all columns
        /// by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see>/<see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods. It can occur with indexed <see cref="IBitmapData.PixelFormat"/>s if the right edge of the clipping is not on byte boundary.</para>
        /// </remarks>
        public static IReadWriteBitmapData Clip(this IReadWriteBitmapData source, Rectangle clippingRegion)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), PublicResources.ArgumentNull);
            return clippingRegion.Location.IsEmpty && clippingRegion.Size == source.GetSize()
                ? source
                : new ClippedBitmapData(source, clippingRegion);
        }

        #endregion

        #endregion
    }
}
