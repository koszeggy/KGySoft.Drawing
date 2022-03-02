#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IQuantizingSessionInternal.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    // TODO: move this in IQuantizingSession in next major version change
    internal interface IQuantizingSessionInternal : IQuantizingSession
    {
        #region Properties

        /// <summary>
        /// Gets whether this <see cref="IQuantizingSession"/> works with grayscale colors.
        /// Its value may help to optimize the processing in some cases but it is allowed to return always <see langword="false"/>.
        /// </summary>
        bool IsGrayscale { get; }

        #endregion
    }
}