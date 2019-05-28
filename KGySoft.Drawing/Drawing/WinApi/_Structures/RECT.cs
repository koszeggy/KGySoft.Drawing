#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: RECT.cs
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
