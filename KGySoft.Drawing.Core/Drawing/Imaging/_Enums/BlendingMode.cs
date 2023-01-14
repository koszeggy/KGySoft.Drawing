#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BlendingMode.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
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
    /// Represents the preferred blending mode of an <see cref="IBitmapData"/> instance.
    /// </summary>
    public enum BlendingMode
    {
        /// <summary>
        /// Represents the default blending mode most optimal for the current operation.
        /// When setting pixels it may depend on the color type, or when performing other operations
        /// such as drawing or performing other transformations it may depend on the pixel format.
        /// </summary>
        Default,

        /// <summary>
        /// Indicates that the bitmap data prefers blending in the linear color space.
        /// Using a quantizer may override this value.
        /// </summary>
        Linear,

        /// <summary>
        /// Indicates that the bitmap data prefers blending in the sRGB color space.
        /// Using a quantizer may override this value.
        /// </summary>
        Srgb
    }
}