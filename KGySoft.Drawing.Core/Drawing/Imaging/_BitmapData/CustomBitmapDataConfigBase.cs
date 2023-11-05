#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CustomBitmapDataConfigBase.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2023 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;

#endregion

namespace KGySoft.Drawing.Imaging
{
    public abstract class CustomBitmapDataConfigBase
    {
        #region Properties

        public PixelFormatInfo PixelFormat { get; set; }

        public Action? DisposeCallback { get; set; }

        /// <summary>
        /// Gets or sets whether the pixel accessor delegates are not tied to a specific buffer instance or memory location but can work with any buffer.
        /// If the pixel accessor delegates do not capture any specific buffer instance and access the bitmap data only by the <see cref="ICustomBitmapDataRow"/>
        /// parameter of the delegate, then it is safe to set this property to <see langword="true"/>.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// <para>It is always recommended to create buffer-independent accessors. If you do so, then you can set this property to <see langword="true"/>, which
        /// allows improving the quality of some operations.</para>
        /// <para>When this property is <see langword="true"/>, then an exact clone of a bitmap data with custom format preserves the original pixel format.
        /// Otherwise, the clone will have a <see cref="KnownPixelFormat"/> and the raw data of the original pixels might have a different structure.</para>
        /// <para>The value of this property affects also the quality of the quantizers created by the <see cref="PredefinedColorsQuantizer.FromBitmapData">PredefinedColorsQuantizer.FromBitmapData</see>
        /// method. If this property returns <see langword="true"/>, then the quantizer will be able to reproduce the exact colors of the original custom bitmap data.
        /// Otherwise, the quantizer may not work correctly for non-indexed bitmaps or for bitmaps with more than 8 bits-per-pixels.</para>
        /// <para>For safety reasons the default value of this property is <see langword="false"/>.</para>
        /// </remarks>
        public bool BackBufferIndependentPixelAccess { get; set; }

        #endregion

        #region Methods

        internal abstract bool CanRead();
        internal abstract bool CanWrite();

        #endregion
    }
}