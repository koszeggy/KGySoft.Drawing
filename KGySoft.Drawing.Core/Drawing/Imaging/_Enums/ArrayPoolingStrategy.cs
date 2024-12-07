#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: PoolingStrategy.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
    /// Represents the possible values for the <see cref="BitmapDataFactory.PoolingStrategy">BitmapDataFactory.PoolingStrategy</see> property, which specifies
    /// the array pooling strategies for the self-allocating <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">CreateBitmapData</see>
    /// overloads in the <see cref="BitmapDataFactory"/> class.
    /// </summary>
    public enum ArrayPoolingStrategy
    {
        /// <summary>
        /// Indicates that no array pooling occurs when creating <see cref="IBitmapData"/> instances by the self-allocating factory methods.
        /// This strategy will always use the best matching element type for the underlying buffer.
        /// </summary>
        Never,

        /// <summary>
        /// Specifies that array pooling occurs only if the natural buffer element type of the created <see cref="IBitmapData"/> instance is byte anyway.
        /// This is the case for indexed pixel formats in the <see cref="KnownPixelFormat"/> enumaration.
        /// </summary>
        IfByteArrayBased,

        /// <summary>
        /// Specifies that array pooling always occurs if the created <see cref="IBitmapData"/> instance can use a byte array as its buffer.
        /// This is true for all pixel formats as long as the size of the underlying buffer does not exceed the maximum size of a byte array.
        /// Please note that the default array pool may not cache large arrays, and that accessing the reinterpreted elements may be
        /// slightly slower than using the best matching element type.
        /// </summary>
        IfCanUseByteArray,

        /// <summary>
        /// Uses array pooling (if available on the current platform) for any element type. This may consume more memory than the other strategies,
        /// but there is no performance penalty for reinterpreting the elements.
        /// </summary>
        AnyElementType
    }
}
