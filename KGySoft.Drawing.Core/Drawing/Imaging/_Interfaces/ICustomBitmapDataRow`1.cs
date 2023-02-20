#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICustomBitmapDataRow`1.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Represents a low-level custom accessor to a bitmap data row of specific element type.
    /// </summary>
    /// <typeparam name="T">The element type of the underlying custom buffer.</typeparam>
    public interface ICustomBitmapDataRow<T> : ICustomBitmapDataRow where T : unmanaged
    {
        #region Indexers

        /// <summary>
        /// Gets a reference to the actual underlying buffer element at the specified index.
        /// To reinterpret the element type of the underlying buffer use the <see cref="ICustomBitmapDataRow.GetRefAs{T}"/> method instead.
        /// </summary>
        /// <param name="index">The element index of the value withing the current row to obtain.</param>
        /// <returns>A reference to the actual underlying buffer element at the specified index.</returns>
        ref T this[int index] { get; }

        #endregion
    }
}