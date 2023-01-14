#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICustomBitmapData.cs
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
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal interface ICustomBitmapData : IBitmapData
    {
        #region Properties

        /// <summary>
        /// Gets a factory to create a compatible bitmap data of the specified size that can be used by quantizers.
        /// </summary>
        Func<Size, IBitmapDataInternal>  CreateCompatibleBitmapDataFactory { get; }

        #endregion
    }
}