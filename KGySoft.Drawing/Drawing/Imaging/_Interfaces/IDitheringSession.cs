#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IDitheringSession.cs
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
    public interface IDitheringSession : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets whether this ditherer allows only sequential processing (line by line).
        /// </summary>
        /// <value>
        /// If <see langword="true"/>, then the <see cref="GetDitheredColor">GetDitheredColor</see> method will be called sequentially for each pixels.
        /// If <see langword="false"/>, then the <see cref="GetDitheredColor">GetDitheredColor</see> method can be called concurrently for any pixels.
        /// </value>
        bool IsSequential { get; }

        #endregion

        #region Methods

        Color32 GetDitheredColor(Color32 origColor, int x, int y);

        #endregion
    }
}