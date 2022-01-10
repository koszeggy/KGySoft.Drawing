#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICustomBitmapDataRow.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
    /// Represents a low-level custom accessor to a bitmap data row.
    /// </summary>
    public interface ICustomBitmapDataRow
    {
        #region Properties

        /// <summary>
        /// Gets the corresponding <see cref="IBitmapData"/> of this row.
        /// </summary>
        IBitmapData BitmapData { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a reference to a value interpreted as <typeparamref name="T"/> within the current row at the specified <paramref name="x"/> index.
        /// </summary>
        /// <typeparam name="T">The type of the value to return a reference for. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value within the row to retrieve. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <returns>A reference to a value interpreted as <typeparamref name="T"/> within the current row at the specified <paramref name="x"/> index.</returns>
        ref T GetRefAs<T>(int x) where T : unmanaged;

        ref T UnsafeGetRefAs<T>(int x) where T : unmanaged;
        
        #endregion
    }

    /// <summary>
    /// Represents a low-level custom accessor to a bitmap data row.
    /// </summary>
    /// <typeparam name="T">The element type of the underlying custom buffer.</typeparam>
    public interface ICustomBitmapDataRow<T> : ICustomBitmapDataRow where T : unmanaged
    {
        #region Indexers

        /// <summary>
        /// Gets a reference to the actual underlying buffer element at the specified index.
        /// </summary>
        /// <param name="index">The element index of the value withing the current row to obtain.</param>
        /// <returns>A reference to the actual underlying buffer element at the specified index.</returns>
        ref T this[int index] { get; }

        #endregion
    }
}