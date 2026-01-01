#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SizeExtension.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;

#endregion

namespace KGySoft.Drawing
{
    internal static class SizeExtension
    {
        #region Methods

        /// <summary>
        /// Gets whether the Size has zero Width OR Height.
        /// </summary>
        internal static bool IsEmpty(this Size size) => size.Width == 0 || size.Height == 0;

        #endregion
    }
}