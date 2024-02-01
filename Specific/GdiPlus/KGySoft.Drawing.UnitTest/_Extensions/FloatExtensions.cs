#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FloatExtensions.cs
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

using System;
using System.Runtime.CompilerServices;

#endregion

namespace KGySoft.Drawing
{
    internal static class FloatExtensions
    {
        #region Methods

        [MethodImpl(MethodImpl.AggressiveInlining)]
        internal static byte ClipToByte(this float value)
            => value < Byte.MinValue ? Byte.MinValue
                : value > Byte.MaxValue ? Byte.MaxValue
                : (byte)value;

        #endregion
    }
}