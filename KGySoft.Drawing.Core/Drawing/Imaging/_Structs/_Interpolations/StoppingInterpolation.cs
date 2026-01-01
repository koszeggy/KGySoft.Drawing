#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: StoppingInterpolation.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal readonly struct StoppingInterpolation : IInterpolation
    {
        #region Methods

        public float GetValue(float value) => value <= 0f ? 0f : value >= 1f ? 1f : value;

        #endregion
    }
}