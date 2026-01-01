#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MirroringInterpolation.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal readonly struct MirroringInterpolation : IInterpolation
    {
        #region Methods

        public float GetValue(float value)
        {
            value = Math.Abs(value) % 2f;
            return value > 1f ? 2f - value : value;
        }

        #endregion
    }
}