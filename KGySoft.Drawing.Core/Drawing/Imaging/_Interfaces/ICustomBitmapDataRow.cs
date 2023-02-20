#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICustomBitmapDataRow.cs
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
using System.Security;

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

        /// <summary>
        /// Gets the index of the current row. Can fall between zero and <see cref="IBitmapData.Height">Height</see> of the owner <see cref="BitmapData"/> (exclusive upper bound).
        /// </summary>
        int Index { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a reference to a value interpreted as <typeparamref name="T"/> within the current row at the specified <paramref name="x"/> index.
        /// </summary>
        /// <typeparam name="T">The type of the value to return a reference for. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value within the row to retrieve. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <returns>A reference to a value interpreted as <typeparamref name="T"/> within the current row at the specified <paramref name="x"/> index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is not valid for <see cref="IBitmapData.RowSize"/> and <typeparamref name="T"/>.</exception>
        ref T GetRefAs<T>(int x) where T : unmanaged;

        /// <summary>
        /// Gets a reference to a value interpreted as <typeparamref name="T"/> within the current row at the specified <paramref name="x"/> index.
        /// This method is similar to <see cref="GetRefAs{T}"/> but it does not check whether <paramref name="x"/> is valid for <see cref="IBitmapData.RowSize"/> and the size of <typeparamref name="T"/>.
        /// It may provide a better performance but if <paramref name="x"/> is invalid, then memory can be either corrupted or an <see cref="AccessViolationException"/> can be thrown.
        /// </summary>
        /// <typeparam name="T">The type of the value to return a reference for. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value within the row to retrieve. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <returns>A reference to a value interpreted as <typeparamref name="T"/> within the current row at the specified <paramref name="x"/> index.</returns>
        [SecurityCritical]
        ref T UnsafeGetRefAs<T>(int x) where T : unmanaged;
        
        #endregion
    }
}