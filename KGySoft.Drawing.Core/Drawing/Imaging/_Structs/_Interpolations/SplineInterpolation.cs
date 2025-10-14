#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SplineInterpolation.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    internal struct SplineInterpolation : IFixedSizeInterpolation
    {
        #region Properties

        public float Radius => 2f;

        #endregion

        #region Methods

        public float GetValue(float value) => Interpolation.CubicInterpolation(value, 1f, 0f);

        #endregion
    }
}