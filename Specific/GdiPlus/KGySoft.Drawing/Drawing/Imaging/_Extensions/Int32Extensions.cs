#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Int32Extensions.cs
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

using System;
#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal static class Int32Extensions
    {
        #region Methods

#if NET7_0_OR_GREATER
        [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "PixelFormat is an enum so it is supported on every platform.")]
#endif
        internal static PixelFormat ToPixelFormat(this int bpp) => bpp switch
        {
            1 => PixelFormat.Format1bppIndexed,
            4 => PixelFormat.Format4bppIndexed,
            8 => PixelFormat.Format8bppIndexed,
            16 => PixelFormat.Format16bppRgb565,
            24 => PixelFormat.Format24bppRgb,
            32 => PixelFormat.Format32bppArgb,
            48 => PixelFormat.Format48bppRgb,
            64 => PixelFormat.Format64bppArgb,
            _ => throw new ArgumentOutOfRangeException(nameof(bpp), PublicResources.ArgumentOutOfRange)
        };

        #endregion
    }
}