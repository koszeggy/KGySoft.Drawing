#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RECT.cs
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
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.WinApi
{
    // ReSharper disable once InconsistentNaming
    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        #region Fields

        internal int Left;
        internal int Top;
        internal int Right;
        internal int Bottom;

        #endregion

        #region Methods

        internal Rectangle ToRectangle() => Rectangle.FromLTRB(Left, Top, Right, Bottom);

        #endregion
    }
}
