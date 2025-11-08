#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ManagedBitmapDataBase.cs
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

#endregion

using System.Security;

namespace KGySoft.Drawing.Imaging
{
    internal abstract class ManagedBitmapDataBase : BitmapDataBase
    {
        #region Properties

        internal virtual bool MayUsePooledBuffer => false;

        #endregion

        #region Constructors

        protected ManagedBitmapDataBase(in BitmapDataConfig cfg)
            : base(cfg)
        {
        }

        #endregion

        #region Methods

        [SecuritySafeCritical]internal abstract ref byte GetPinnableReference();

        #endregion
    }
}