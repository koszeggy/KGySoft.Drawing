#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorAlpha8.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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

namespace KGySoft.Drawing.SkiaSharp
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal readonly struct ColorAlpha8
    {
        #region Fields

        private readonly byte a;

        #endregion

        #region Constructors

        internal ColorAlpha8(Color32 c) => a = c.A;

        #endregion

        #region Methods

        internal Color32 ToColor32() => Color32.FromArgb(a, default);

        #endregion
    }
}