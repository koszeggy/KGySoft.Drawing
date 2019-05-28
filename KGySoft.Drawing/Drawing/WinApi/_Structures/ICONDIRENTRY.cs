#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ICONDIRENTRY.cs
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

using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ICONDIRENTRY
    {
        #region Fields

        /// <summary>
        /// The icon width in pixels. 0 for 256 width.
        /// </summary>
        internal byte bWidth;

        /// <summary>
        /// The icon height in pixels. 0 for 256 height.
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
