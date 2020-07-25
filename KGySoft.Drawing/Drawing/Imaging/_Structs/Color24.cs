#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Color24.cs
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

using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Drawing.Imaging
{
    [StructLayout(LayoutKind.Explicit)]
    internal readonly struct Color24
    {
        #region Fields

        [FieldOffset(0)]
        private readonly byte b;

        [FieldOffset(1)]
        private readonly byte g;

        [FieldOffset(2)]
        private readonly byte r;

        #endregion

        #region Constructors

        internal Color24(Color32 c)
        {
            r = c.R;
            g = c.G;
            b = c.B;
        }

        #endregion

        #region Methods

        internal Color32 ToColor32() => new Color32(r, g, b);

        internal int ToRgb() => ToColor32().ToRgb();

        #endregion
    }
}