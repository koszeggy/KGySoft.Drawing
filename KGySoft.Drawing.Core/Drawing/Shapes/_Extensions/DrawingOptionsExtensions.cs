#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DrawingOptionsExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing.Shapes
{
    internal static class DrawingOptionsExtensions
    {
        #region Methods

        internal static float PixelOffset(this DrawingOptions? options) => options?.DrawPathPixelOffset is Shapes.PixelOffset.Half ? 0.5f : 0f;

        #endregion
    }
}