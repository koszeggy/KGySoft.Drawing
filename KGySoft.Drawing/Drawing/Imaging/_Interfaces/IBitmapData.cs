#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
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
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// Obtain an instance by the ... extension
    /// TODO: <para>For parallel processing you can retrieve multiple rows by the indexer and process them concurrently.</para>
    /// TODO: example: Processing by coordinates
    /// TODO: example: Line by line processing by FirstRow + MoveNextRow
    /// TODO: example: Parallel processing by FirstRow + MoveNextRow
    public interface IBitmapData : IDisposable
    {
        #region Properties

        int Height { get; }

        int Width { get; }

        PixelFormat PixelFormat { get; }

        Palette Palette { get; }

        #endregion

        #region Methods

        BitmapData ToBitmapData();

        #endregion
    }
}
