#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Accessors.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;

using KGySoft.Reflection;

#endregion

namespace KGySoft.Drawing
{
    // ReSharper disable InconsistentNaming
    internal static class Accessors
    {
        #region Fields

        private static FieldAccessor fieldGraphic_backingImage;
        private static FieldAccessor fieldIcon_iconData;
        private static FieldAccessor fieldColorPalette_entries;

        #endregion

        #region Properties

#if NET35 || NET40 || NET45

        private static FieldAccessor Graphics_backingImage => fieldGraphic_backingImage ?? (fieldGraphic_backingImage = FieldAccessor.GetAccessor(typeof(Graphics).GetField("backingImage", BindingFlags.Instance | BindingFlags.NonPublic)));

        private static FieldAccessor Icon_iconData => fieldIcon_iconData ?? (fieldIcon_iconData = FieldAccessor.GetAccessor(typeof(Icon).GetField("iconData", BindingFlags.Instance | BindingFlags.NonPublic)));

        private static FieldAccessor ColorPalette_entries => fieldColorPalette_entries ?? (fieldColorPalette_entries = FieldAccessor.GetAccessor(typeof(ColorPalette).GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic)));

#else
#error .NET version is not set or not supported! Check accessed non-public member names for the newly added .NET version.
#endif

        #endregion

        #region Methods

        internal static Image GetBackingImage(this Graphics graphics) => (Image)Graphics_backingImage.Get(graphics);

        internal static byte[] GetIconData(this Icon icon) => (byte[])Icon_iconData.Get(icon);

        internal static void SetEntries(this ColorPalette palette, Color[] value) => ColorPalette_entries.Set(palette, value);

        #endregion
    }
}
