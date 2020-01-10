#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IQuantizingSession.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public interface IQuantizingSession : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the palette if the number of colors are limited up to 256 colors; otherwise, can be <see langword="null"/>&#160;or empty.
        /// </summary>
        Color32[] Palette { get; }

        bool SupportsPositionalResult { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the quantized color of the specified <paramref name="origColor"/>.
        /// </summary>
        /// <param name="origColor">The original color to be quantized.</param>
        /// <returns>The quantized color.</returns>
        Color32 GetQuantizedColor(Color32 origColor);

        Color32 GetQuantizedColorByPosition(int x, int y);

        #endregion
    }
}