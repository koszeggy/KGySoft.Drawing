#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: GifGraphicDisposalMethod.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Imaging
{
    // NOTE: The descriptions are taken from the GIF89a specification: https://www.w3.org/Graphics/GIF/spec-gif89a.txt
    /// <summary>
    /// Indicates the way in which the graphic is to be treated after being displayed.
    /// </summary>
    public enum GifGraphicDisposalMethod
    {
        /// <summary>
        /// No disposal specified. The decoder is not required to take any action.
        /// </summary>
        NotSpecified,

        /// <summary>
        /// Do not dispose. The graphic is to be left in place.
        /// </summary>
        DoNotDispose,

        /// <summary>
        /// Restore to background color. The area used by the graphic must be restored to the background color.
        /// If there is no global palette, or the corresponding image has transparent color, then the virtual screen
        /// is always cleared to transparent.
        /// </summary>
        RestoreToBackground,

        /// <summary>
        /// Restore to previous. The decoder is required to restore the area overwritten by the graphic with what was there prior to rendering the graphic.
        /// </summary>
        RestoreToPrevious,
    }
}