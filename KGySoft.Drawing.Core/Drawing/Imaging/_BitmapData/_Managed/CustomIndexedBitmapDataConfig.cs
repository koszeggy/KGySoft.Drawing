#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomIndexedBitmapDataConfig.cs
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
    public sealed class CustomIndexedBitmapDataConfig<T> : CustomIndexedBitmapDataConfigBase
        where T : unmanaged
    {
        #region Properties

        public Func<ICustomBitmapDataRow<T>, int, int>? RowGetColorIndex { get; set; }

        public Action<ICustomBitmapDataRow<T>, int, int>? RowSetColorIndex { get; set; }

        #endregion
    }
}