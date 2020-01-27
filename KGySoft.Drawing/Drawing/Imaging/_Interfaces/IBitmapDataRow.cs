﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataRow.cs
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
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public interface IBitmapDataRow
    {
        #region Properties

        //IntPtr Address { get; }

        int Index { get; }

        #endregion

        #region Methods

        bool MoveNextRow();

        #endregion
    }
}