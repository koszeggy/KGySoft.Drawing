namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents the scaling mode when an image needs to be resized.
    /// </summary>
    public enum ScalingMode
    {
        Auto,

        /// <summary>
        /// Represents no scaling. When source and target sizes are different, then clipping might occur
        /// </summary>
        NoScaling,

        /// <summary>
        /// Represents the nearest neighbor scaling mode, which always selects the closest pixel when scaling.
        /// </summary>
        NearestNeighbor,

        /// <summary>
        /// Represents box scaling mode. When downscaling, the pixels will average.
        /// When upscaling, the result is identical to Nearest neighbor.
        /// </summary>
        Box,

        Bilinear,

        Bicubic,

        Lanczos2,

        Lanczos3,

        Spline,

        CatmullRom,

        MitchellNetravali,

        Robidoux
    }
}
