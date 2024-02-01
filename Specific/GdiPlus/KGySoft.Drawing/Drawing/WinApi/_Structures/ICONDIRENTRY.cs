#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICONDIRENTRY.cs
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

using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    // ReSharper disable once InconsistentNaming
    [StructLayout(LayoutKind.Sequential)]
    internal struct ICONDIRENTRY
    {
        #region Fields

        /// <summary>
        /// The icon width in pixels. 0 for >=256 width or PNG compressed icons.
        /// </summary>
        internal byte bWidth;

        /// <summary>
        /// The icon height in pixels. 0 for >=256 height or PNG compressed icons.
        /// </summary>
        internal byte bHeight;

        /// <summary>
        /// Number of colors in the image. 0 for 256 or more colors.
        /// </summary>
        internal byte bColorCount;

        /// <summary>
        /// Reserved byte. Always 0.
        /// </summary>
        internal byte bReserved;

        /// <summary>
        /// Color planes. Always 1.
        /// </summary>
        internal ushort wPlanes;

        /// <summary>
        /// Bits per pixel.
        /// </summary>
        internal ushort wBitCount;

        /// <summary>
        /// Length of the image in bytes including header and palette size.
        /// </summary>
        internal uint dwBytesInRes;

        /// <summary>
        /// Start offset of the image insize of the icon in bytes.
        /// </summary>
        internal uint dwImageOffset;

        #endregion
    }
}
