#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Accessors.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing
{
    // ReSharper disable InconsistentNaming
    internal static class Accessors
    {
        #region Methods

        #region Internal Methods

        #region Graphics

        internal static Image? GetBackingImage(this Graphics graphics) => graphics.GetFieldValueOrDefault<Image?>("backingImage");

        #endregion

        #region Icon
        
        internal static bool HasIconData(this Icon icon) => (typeof(Icon).GetField(null, "iconData")
            ?? typeof(Icon).GetField(null, "imageData"))?.Get(icon) != null;

        #endregion

        #region ColorPalette
        
        internal static bool TrySetEntries(this ColorPalette palette, Color[] value) => palette.TrySetFieldValue("entries", value);
        internal static void SetFlags(this ColorPalette palette, int value) => palette.TrySetFieldValue("flags", value);

        #endregion

        #endregion

        #endregion
    }
}
