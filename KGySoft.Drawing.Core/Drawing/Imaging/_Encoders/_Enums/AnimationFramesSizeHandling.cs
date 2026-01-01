#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AnimationFramesSizeHandling.cs
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
    /// Represents the strategy to be used with frames of different sizes.
    /// </summary>
    public enum AnimationFramesSizeHandling
    {
        /// <summary>
        /// Specifies that if an input frame has a different size, then an exception should be thrown.
        /// </summary>
        ErrorIfDiffers,

        /// <summary>
        /// Specifies that smaller or larger frames should be centered. Possibly exceeding areas will be clipped.
        /// </summary>
        Center,

        /// <summary>
        /// Specifies that frames should be resized to the actual size of the animation.
        /// </summary>
        Resize,
    }
}