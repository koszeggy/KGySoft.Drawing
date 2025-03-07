﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IBitmapDataAccessor.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Security;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal interface IBitmapDataAccessor<TColor, in TArg>
        where TColor : unmanaged
    {
        #region Methods

        void InitBitmapData(IBitmapDataInternal bitmapData, TArg arg = default!);
        [SecurityCritical]TColor GetColor(int x, int y);
        [SecurityCritical]void SetColor(int x, int y, TColor color);

        void InitRow(IBitmapDataRowInternal row, TArg arg = default!);
        [SecurityCritical]TColor GetColor(int x);
        [SecurityCritical]void SetColor(int x, TColor color);

        #endregion
    }

    internal interface IBitmapDataAccessor<TColor, TBaseColor, in TArg> : IBitmapDataAccessor<TColor, TArg>
        where TColor : unmanaged, IColor<TColor, TBaseColor>
        where TBaseColor : unmanaged, IColor<TBaseColor, TBaseColor>
    {
        #region Methods

        [SecurityCritical]TBaseColor GetBaseColor(int x);
        [SecurityCritical]void SetBaseColor(int x, TBaseColor color);

        #endregion
    }
}
