#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RGBQUAD.cs
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
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
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

        #region Constructors

        internal RGBQUAD(Color color)
        {
            rgbRed = color.R;
            rgbGreen = color.G;
            rgbBlue = color.B;
            rgbReserved = 0;
        }

        #endregion

        #region Methods

        internal bool EqualsWithColor(Color color) => color != Color.Empty && rgbRed == color.R && rgbGreen == color.G && rgbBlue == color.B;

        #endregion
    }
}
