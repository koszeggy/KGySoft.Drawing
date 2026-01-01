#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AutoStrengthMode.cs
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
    /// <summary>
    /// Represents the behavior of ditherers with configurable strength when auto strength is used.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="OrderedDitherer.ConfigureStrength">OrderedDitherer.ConfigureStrength</see> method
    /// for more details and some examples regarding dithering strength.
    /// </summary>
    public enum AutoStrengthMode
    {
        /// <summary>
        /// The auto strength mode is determined by the <see cref="IQuantizingSession.WorkingColorSpace"/> property
        /// of the corresponding quantizing session. If it returns <see cref="WorkingColorSpace.Linear"/>, then the
        /// default strategy is <see cref="Interpolated"/>; otherwise, it is <see cref="Constant"/>.
        /// </summary>
        Default,

        /// <summary>
        /// Represents an auto strength strategy where strength is calibrated to a fix value that assures that neither the black nor the white colors
        /// will suffer from overdithering. This is the default auto strength strategy when quantizing in the sRGB color space, and it usually works fine
        /// if palette entries are evenly distributed in the sRGB color space.
        /// </summary>
        Constant,

        /// <summary>
        /// Represents an auto strength strategy where strength is calibrated both for white and black colors individually so none of them suffer from
        /// overdithering. During dithering the actual applied strength will be a dynamic value for each pixel using interpolation between
        /// the white and black strengths based on the pixel brightness. This is the default auto strength strategy when quantizing in the linear color space.
        /// </summary>
        Interpolated
    }
}
