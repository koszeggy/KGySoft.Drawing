#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AnimationMode.cs
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
    /// Represents the looping mode of an animation.
    /// </summary>
    public enum AnimationMode
    {
        /// <summary>
        /// Specifies that added frames should be played back and forth.
        /// </summary>
        PingPong = -1,

        /// <summary>
        /// Specifies that the animation should be looped.
        /// </summary>
        Repeat = 0,

        /// <summary>
        /// Specifies that the animation should be played only once.
        /// </summary>
        PlayOnce = 1,
    }
}