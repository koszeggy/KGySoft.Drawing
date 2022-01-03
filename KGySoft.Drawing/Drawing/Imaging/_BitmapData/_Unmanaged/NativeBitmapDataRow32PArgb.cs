#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataRow32PArgb.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
    internal sealed class NativeBitmapDataRow32PArgb : NativeBitmapDataRowBase
    {
        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32(int x) => ((Color32*)Address)[x].ToStraight();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32(int x, Color32 c) => ((Color32*)Address)[x] = c.ToPremultiplied();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32Premultiplied(int x) => ((Color32*)Address)[x];

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32Premultiplied(int x, Color32 c) => ((Color32*)Address)[x] = c;

        #endregion
    }
}
