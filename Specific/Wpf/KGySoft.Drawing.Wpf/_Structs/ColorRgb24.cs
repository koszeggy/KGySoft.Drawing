#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorRgb24.cs
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

using System.Runtime.InteropServices;

using KGySoft.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Wpf
{
    [StructLayout(LayoutKind.Explicit, Size = 3)]
    internal readonly struct ColorRgb24
    {
        #region Fields

        [FieldOffset(0)]
        private readonly byte r;

        [FieldOffset(1)]
        private readonly byte g;
        
        [FieldOffset(2)]
        private readonly byte b;

        #endregion

        #region Constructors

        internal ColorRgb24(Color32 c)
        {
            r = c.R;
            g = c.G;
            b = c.B;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(r, g, b);

        #endregion
    }
}