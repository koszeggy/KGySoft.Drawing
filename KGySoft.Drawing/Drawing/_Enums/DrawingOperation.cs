#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DrawingOperation.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2020 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents various drawing operations that can be used with the <see cref="IDrawingProgress"/> type.
    /// </summary>
    public enum DrawingOperation
    {
        IndefiniteProcessing,
        InitializingQuantizer,
        InitializingDitherer,
        GeneratingPalette,
        //ProcessingPixels,
        Saving,
        Loading,

        ProcessingPixels,
        RawCopy,
        StraightCopy,
        PremultipliedCopy,
        CopyWithQuantizer,
        CopyWithDithering,

        StraightDraw,
        PremultipliedDraw,
        DrawWithQuantizer,
        DrawWithDithering,

        ResizeNNStraight,
        ResizeNNPremultiplied,
        ResizeNNWithQuantizer,
        ResizeNNWithDithering,

        InitializingResize,
        ResizeStraight,
        ResizePremultiplied,
        ResizeWithQuantizer,
        ResizeWithDithering,

        ClearByIndex,
        ClearByColor,
        ClearRaw,
        ClearWithDithering,

        Quantizing,
        Dithering,
        TransformingColors,
        TransformingColorsWithDithering
    }
}