#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadWriteBitmapData.cs
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

namespace KGySoft.Drawing.Imaging
{
    /// Obtain an instance by the ... extension
    /// TODO: <para>For parallel processing you can retrieve multiple rows by the indexer and process them concurrently.</para>
    /// TODO: example: Processing by coordinates
    /// TODO: example: Line by line processing by FirstRow + MoveNextRow
    /// TODO: example: Parallel processing by FirstRow + MoveNextRow
    public interface IReadWriteBitmapData : IReadableBitmapData, IWritableBitmapData
    {
        #region Properties and Indexers

        #region Properties

        new IReadWriteBitmapDataRow FirstRow { get; }

        #endregion

        #region Indexers

        new IReadWriteBitmapDataRow this[int y] { get; }

        #endregion

        #endregion
    }
}
