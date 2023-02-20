#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RGBQUAD.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Drawing;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// The RGBQUAD structure describes a color consisting of relative intensities of red, green, and blue.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct RGBQUAD
    {
        #region Fields

        /// <summary>
        /// The intensity of blue in the color.
        /// </summary>
        internal byte rgbBlue;

        /// <summary>
        /// The intensity of green in the color.
        /// </summary>
        internal byte rgbGreen;

        /// <summary>
        /// The intensity of red in the color.
        /// </summary>
        internal byte rgbRed;

        /// <summary>
        /// This member is reserved and must be zero.
        /// </summary>
        internal byte rgbReserved;

        #endregion

        #region Methods
        
        internal Color ToColor() => Color.FromArgb(rgbRed, rgbGreen, rgbBlue);

        #endregion
    }
}
