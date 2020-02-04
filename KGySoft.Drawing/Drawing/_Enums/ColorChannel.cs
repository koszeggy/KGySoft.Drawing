#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ColorChannel.cs
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

namespace KGySoft.Drawing
{
    [Flags]
    public enum ColorChannel
    {
        None = 0,
        R = 1,
        G = 1 << 1,
        B = 1 << 2,
        Rgb = R | G | B
    }
}