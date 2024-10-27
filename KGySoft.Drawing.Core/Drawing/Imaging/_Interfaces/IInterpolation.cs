#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IInterpolation.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal interface IInterpolation
    {
        #region Methods

        float GetValue(float value);

        #endregion
    }
}