#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomBitmapDataConfigBase.cs
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
    public abstract class CustomBitmapDataConfigBase
    {
        #region Properties

        public PixelFormatInfo PixelFormat { get; set; }

        public Action? DisposeCallback { get; set; }

        #endregion
    }
}