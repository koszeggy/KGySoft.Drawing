#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IconInfo.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides information about an <see cref="Icon"/> image.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{" + nameof(Size) + "} {" + nameof(BitsPerPixel) + "} BPP")]
    public sealed class IconInfo
    {
        #region Properties

        /// <summary>
        /// Gets the size of the icon image.
        /// </summary>
        public Size Size { get; internal set; }

        /// <summary>
        /// Gets the bits per pixel (BPP) value of the icon image.
        /// </summary>
        public int BitsPerPixel { get; internal set; }

        /// <summary>
        /// Gets whether the icon image is compressed.
        /// </summary>
        public bool IsCompressed { get; internal set; }


        /// <summary>
        /// Gets the palette of the icon image, or <see langword="null"/>, if it has no palette.
        /// </summary>
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays",
            Justification = "It's always a copy")]
        public Color[]? Palette { get; internal set; }

        #endregion
    }
}