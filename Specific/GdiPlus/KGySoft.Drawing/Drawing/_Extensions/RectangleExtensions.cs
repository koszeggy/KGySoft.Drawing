#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RectangleExtensions.cs
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

using System.Drawing;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing
{
    internal static class RectangleExtensions
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static void Normalize(this ref RectangleF rect)
        {
            if (rect.Width < 0f)
            {
                rect.X += rect.Width;
                rect.Width = -rect.Width;
            }

            if (rect.Height < 0f)
            {
                rect.Y += rect.Height;
                rect.Height = -rect.Height;
            }
        }

        #endregion
    }
}