#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataRow32Argb.cs
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

using System.Security;

namespace KGySoft.Drawing.Imaging
{
    internal sealed class BitmapDataRow32Argb : BitmapDataRowNonIndexedBase
    {
        #region Methods

        [SecurityCritical]
        public override unsafe Color32 DoGetColor32(int x) => ((Color32*)Address)[x];

        [SecurityCritical]
        public override unsafe void DoSetColor32(int x, Color32 c) => ((Color32*)Address)[x] = c;

        #endregion
    }
}