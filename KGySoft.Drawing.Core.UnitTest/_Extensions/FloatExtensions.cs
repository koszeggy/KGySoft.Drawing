#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: FloatExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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

        internal static float Dec(this float value)
        {
#if NETCOREAPP3_0_OR_GREATER
            uint asUInt = Unsafe.As<float, uint>(ref value) - 1;
            return Unsafe.As<uint, float>(ref asUInt);
#else
            uint asUInt = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0) - 1;
            return BitConverter.ToSingle(BitConverter.GetBytes(asUInt), 0);
#endif
        }

        internal static float Inc(this float value)
        {
#if NETCOREAPP3_0_OR_GREATER
            uint asUInt = Unsafe.As<float, uint>(ref value) + 1;
            return Unsafe.As<uint, float>(ref asUInt);
#else
            uint asUInt = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0) + 1;
            return BitConverter.ToSingle(BitConverter.GetBytes(asUInt), 0);
#endif
        }

        #endregion
    }
}