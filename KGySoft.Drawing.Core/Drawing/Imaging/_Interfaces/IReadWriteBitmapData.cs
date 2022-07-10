#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadWriteBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
    /// Represents an <see cref="IBitmapData"/> instance with read/write access.
    /// To create an instance use the <see cref="BitmapDataFactory"/> class or the <c>GetReadWriteBitmapData</c> extension methods for various platform dependent bitmap implementations.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="N:KGySoft.Drawing"/> namespace for a list about the technologies with dedicated support.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">BitmapExtensions.GetReadWriteBitmapData</a>
    /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for all sources.
    /// </summary>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="BitmapDataFactory"/>
    // ReSharper disable once PossibleInterfaceMemberAmbiguity - intended, see new
    public interface IReadWriteBitmapData : IReadableBitmapData, IWritableBitmapData
    {
        #region Properties and Indexers

        #region Properties

        /// <summary>
        /// Gets an <see cref="IReadWriteBitmapDataRow"/> instance representing the first row of the current <see cref="IReadWriteBitmapData"/>.
        /// Subsequent rows can be accessed by calling the <see cref="IBitmapDataRow.MoveNextRow">MoveNextRow</see> method on the returned instance
        /// while it returns <see langword="true"/>. Alternatively, you can use the <see cref="this">indexer</see> to obtain any row.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadWriteBitmapData"/> has already been disposed.</exception>
        new IReadWriteBitmapDataRow FirstRow { get; }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets an <see cref="IReadWriteBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadWriteBitmapData"/>.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/?topic=html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm" target="_blank">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <returns>An <see cref="IReadWriteBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadWriteBitmapData"/>.</returns>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadWriteBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        new IReadWriteBitmapDataRow this[int y] { get; }

        #endregion

        #endregion
    }
}
