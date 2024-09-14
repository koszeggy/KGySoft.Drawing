#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WorkingColorSpaceExtensions.cs
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
    internal static class WorkingColorSpaceExtensions
    {
        #region Methods

        internal static KnownPixelFormat GetPreferredFirstPassPixelFormat(this WorkingColorSpace quantizerWorkingColorSpace)
            => quantizerWorkingColorSpace == WorkingColorSpace.Linear ? KnownPixelFormat.Format32bppArgb : KnownPixelFormat.Format32bppPArgb;

        #endregion
    }
}