#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Array2DExtensions.cs
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

using KGySoft.Collections;

#endregion

namespace KGySoft.Drawing
{
    internal static class Array2DExtensions
    {
        #region Methods

        internal unsafe static CastArray2D<TFrom, TTo> Cast<TFrom, TTo>(this Array2D<TFrom> array2D)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            Debug.Assert(sizeof(TFrom) * array2D.Width % sizeof(TTo) == 0, $"Wrong stride: byte width should be divisible by {sizeof(TTo)}");
            return new CastArray2D<TFrom, TTo>(array2D.Buffer, array2D.Height, sizeof(TFrom) * array2D.Width / sizeof(TTo));
        }

        #endregion
    }
}