﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: UnmanagedBitmapDataRow32PArgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System.Runtime.CompilerServices;
using System.Security; 

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class UnmanagedBitmapDataRow32PArgb : UnmanagedBitmapDataRowBase
    {
        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32(int x) => ((Color32*)Row)[x].ToStraight();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32(int x, Color32 c) => ((Color32*)Row)[x] = c.ToPremultiplied();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32Premultiplied(int x) => ((Color32*)Row)[x];

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32Premultiplied(int x, Color32 c) => ((Color32*)Row)[x] = c;

        #endregion
    }
}