#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: SKColorSpaceExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using SkiaSharp;

#endregion

namespace KGySoft.Drawing.SkiaSharp
{
    internal static class SKColorSpaceExtensions
    {
        #region Fields

        private static SKColorSpace? defaultSrgb;
        private static SKColorSpace? defaultLinear;

        #endregion

        #region Properties

        private static SKColorSpace DefaultSrgb => defaultSrgb ??= SKColorSpace.CreateSrgb();
        private static SKColorSpace DefaultLinear => defaultLinear ??= SKColorSpace.CreateSrgbLinear();

        #endregion

        #region Methods

        internal static bool IsDefaultSrgb(this SKColorSpace? colorSpace)
        {
            if (colorSpace == null || colorSpace == DefaultSrgb)
                return true;

            if (colorSpace == DefaultLinear
                || !colorSpace.GetNumericalTransferFunction(out SKColorSpaceTransferFn fn)
                || !colorSpace.ToColorSpaceXyz(out SKColorSpaceXyz xyz))
            {
                return false;
            }

            return (fn == SKColorSpaceTransferFn.Srgb || fn == SKColorSpaceTransferFn.Empty)
                && (xyz == SKColorSpaceXyz.Srgb || xyz == SKColorSpaceXyz.Empty);
        }

        internal static bool IsDefaultLinear(this SKColorSpace? colorSpace)
        {
            if (colorSpace == DefaultLinear)
                return true;

            if (colorSpace == null || colorSpace == DefaultSrgb
                || !colorSpace.GetNumericalTransferFunction(out SKColorSpaceTransferFn fn)
                || !colorSpace.ToColorSpaceXyz(out SKColorSpaceXyz xyz))
            {
                return false;
            }

            return fn == SKColorSpaceTransferFn.Linear && (xyz == SKColorSpaceXyz.Srgb || xyz == SKColorSpaceXyz.Empty);
        }

        #endregion
    }
}
