#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ScalingMode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents the scaling mode when an image needs to be resized.
    /// </summary>
    public enum ScalingMode
    {
        /// <summary>
        /// Represents an automatically selected scaling mode based on source/target sizes providing the best quality.
        /// In practice, the <see cref="MitchellNetravali"/> scaling mode is selected if either the width or height is enlarged,
        /// and the <see cref="Bicubic"/> scaling mode, if there is shrinking in both dimensions.
        /// If source and target sizes are the same, then <see cref="NoScaling"/> is selected.
        /// </summary>
        Auto,

        /// <summary>
        /// Represents no scaling. When source and target sizes are different, then clipping might occur.
        /// </summary>
        NoScaling,

        /// <summary>
        /// Represents the nearest neighbor scaling mode, which always selects the closest pixel when scaling.
        /// Apart from <see cref="NoScaling"/> this is the fastest scaling mode but provides the poorest quality.
        /// </summary>
        NearestNeighbor,

        /// <summary>
        /// Represents box scaling mode. When downscaling, the pixels will average.
        /// When upscaling, the result is identical to <see cref="NearestNeighbor"/>.
        /// </summary>
        Box,

        /// <summary>
        /// Represents a 2-dimensional linear interpolation scaling mode. It is among the faster scaling techniques, but
        /// it provides an acceptable quality only if the difference between the source and target size does not exceed 25%.
        /// </summary>
        Bilinear,

        /// <summary>
        /// Represents the bicubic interpolation. It both preserves sharpness and reduces artifacts quite well.
        /// This is the default scaling mode when downscaling images.
        /// </summary>
        Bicubic,

        /// <summary>
        /// Represents the resampling algorithm suggested by Kornél Lánczos using a kernel radius of 2 pixels.
        /// It both preserves sharpness and reduces artifacts quite well. When downscaling, acts also as a low-pass filter.
        /// The result is usually similar to the bicubic filter.
        /// </summary>
        Lanczos2,

        /// <summary>
        /// Represents the resampling algorithm suggested by Kornél Lánczos using a kernel radius of 3 pixels.
        /// It is among the slower filters, and it tends to increase the sharpness of the original image.
        /// </summary>
        Lanczos3,

        /// <summary>
        /// Represents the spline cubic interpolation. It provides smooth results, but it tends to overly blur the details.
        /// </summary>
        Spline,

        /// <summary>
        /// Represents the Catmull-Rom cubic interpolation. Similarly to the bicubic interpolation
        /// it preserves sharpness quite well without producing strong artifacts.
        /// </summary>
        CatmullRom,

        /// <summary>
        /// Represents the Mitchell-Netravali cubic interpolation. Usually it produces very good results.
        /// This is the default scaling mode when upscaling images.
        /// </summary>
        MitchellNetravali,

        /// <summary>
        /// Represents the Robidoux cubic interpolation. It produces a very similar result to the Mitchell-Netravali interpolation.
        /// </summary>
        Robidoux
    }
}
