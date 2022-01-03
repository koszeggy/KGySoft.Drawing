#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeBitmapDataRow8I.cs
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
    internal class NativeBitmapDataRow8I : NativeBitmapDataRowIndexedBase
    {
        #region Properties

        protected override uint MaxIndex => 255;

        #endregion

        #region Methods

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe int DoGetColorIndex(int x) => ((byte*)Address)[x];

        [SecurityCritical]
        [MethodImpl(MethodImpl.AggressiveInlining)]
        public override unsafe void DoSetColorIndex(int x, int colorIndex) => ((byte*)Address)[x] = (byte)colorIndex;

        #endregion
    }
}