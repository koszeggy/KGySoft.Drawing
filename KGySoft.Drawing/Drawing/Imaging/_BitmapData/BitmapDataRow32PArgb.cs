#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow32PArgb.cs
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

using System.Runtime.CompilerServices;
using System.Security; 

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal sealed class BitmapDataRow32PArgb : BitmapDataRowNonIndexedBase
    {
        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe Color32 DoGetColor32(int x) => ((Color32*)Address)[x].ToStraight();

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColor32(int x, Color32 c) => ((Color32*)Address)[x] = c.ToPremultiplied();

        #endregion
    }
}
