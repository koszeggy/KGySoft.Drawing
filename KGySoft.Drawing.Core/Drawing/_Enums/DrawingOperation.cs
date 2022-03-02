#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DrawingOperation.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
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
    /// Represents various drawing operations that can be used with the <see cref="IDrawingProgress"/> type.
    /// </summary>
    public enum DrawingOperation
    {
        /// <summary>
        /// Represents indefinite processing
        /// </summary>
        UndefinedProcessing,

        /// <summary>
        /// Represents the initialization of a quantizer
        /// </summary>
        InitializingQuantizer,

        /// <summary>
        /// Represents the initialization of a ditherer
        /// </summary>
        InitializingDitherer,

        /// <summary>
        /// Represents a palette-generating operation. Can be the part of another operation, such as quantizer initialization.
        /// </summary>
        GeneratingPalette,

        /// <summary>
        /// Represents an operation that processes pixels. An more complex async method may perform multiple processing operations one after another.
        /// </summary>
        ProcessingPixels,

        /// <summary>
        /// Represents a saving operation.
        /// </summary>
        Saving,

        /// <summary>
        /// Represents a loading operation.
        /// </summary>
        Loading,
    }
}