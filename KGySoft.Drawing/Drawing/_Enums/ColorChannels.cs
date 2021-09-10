#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorChannels.cs
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

using System;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents the RGB color channels when manipulating images.
    /// </summary>
    [Flags]
    public enum ColorChannels
    {
        /// <summary>
        /// Represents none of the color channels.
        /// </summary>
        None = 0,

        /// <summary>
        /// Represents the Red color channel.
        /// </summary>
        R = 1,

        /// <summary>
        /// Represents the Green color channel.
        /// </summary>
        G = 1 << 1,

        /// <summary>
        /// Represents the Blue color channel.
        /// </summary>
        B = 1 << 2,

        /// <summary>
        /// Represents the R, G and B color channels.
        /// </summary>
        Rgb = R | G | B
    }
}