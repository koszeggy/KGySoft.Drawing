﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRowSingleBitAlphaBase.cs
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
    internal abstract class BitmapDataRowSingleBitAlphaBase : BitmapDataRowNoAlphaBase
    {
        #region Fields

        internal byte AlphaThreshold;

        #endregion
    }
}