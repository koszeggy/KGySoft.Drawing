#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WorkingColorSpaceExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class WorkingColorSpaceExtensions
    {
        #region Methods

        internal static WorkingColorSpace GetValueOrLinear(this WorkingColorSpace value)
            => value == WorkingColorSpace.Srgb ? WorkingColorSpace.Srgb : WorkingColorSpace.Linear;

        #endregion
    }
}