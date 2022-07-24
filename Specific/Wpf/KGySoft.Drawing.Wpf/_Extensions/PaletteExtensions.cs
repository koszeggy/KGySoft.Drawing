#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PaletteExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Linq;
using System.Windows.Media.Imaging;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    internal static class PaletteExtensions
    {
        #region Methods

        internal static BitmapPalette? ToBitmapPalette(this Palette? palette)
            => palette == null ? null : new BitmapPalette(palette.GetEntries().Select(c => ColorExtensions.ToMediaColor(c)).ToArray());

        #endregion
    }
}