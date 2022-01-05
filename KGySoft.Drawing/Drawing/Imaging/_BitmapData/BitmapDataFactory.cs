﻿#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataFactory.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2021 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security;
using KGySoft.Collections;
#if !NET35
using System.Threading.Tasks; 
#endif

using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;

#endregion

#region Suppressions

#if NET35
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - in .NET 3.5 not all members are available
#endif

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides factory methods to create <see cref="IReadWriteBitmapData"/> instances.
    /// </summary>
    public static class BitmapDataFactory
    {
        #region Constants

        private const int magicNumber = 0x54414442; // "BDAT"

        #endregion

        #region Methods

        #region Public Methods

        #region CreateBitmapData

        #region Self-Allocating
        
        /// <summary>
        /// Creates a managed <see cref="IReadWriteBitmapData"/> with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> or <paramref name="pixelFormat"/> has an invalid value.</exception>
        /// <param name="size">The size of the bitmap data to create.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section for details. The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>A managed <see cref="IReadWriteBitmapData"/> with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.</returns>
        /// <remarks>
        /// <para>All possible <see cref="PixelFormat"/>s are supported, regardless of the native <see cref="Bitmap"/> support of the current operating system.
        /// <note>When <paramref name="pixelFormat"/> specifies a wide-color format (48/64 bit or 16 bit grayscale), then the returned instance will use the full 16-bit range of the color channels.
        /// This means a different raw content to Windows' wide-color <see cref="Bitmap"/> instances, which use 13-bit channels. But this difference is transparent in most cases
        /// unless we access actual raw content by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> and <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods.</note></para>
        /// <para>The <paramref name="backColor"/> parameter has no effect if <paramref name="pixelFormat"/> has alpha gradient support and it does not affect the actual content of the returned instance.
        /// To set all pixels to a color use the <see cref="BitmapDataExtensions.Clear">Clear</see> extension method.</para>
        /// <para>If <paramref name="alphaThreshold"/> is zero, then setting a fully transparent pixel in a bitmap data with indexed or single-bit-alpha pixel format
        /// will blend the pixel to set with <paramref name="backColor"/> even if the bitmap data can handle transparent pixels.</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>1</c>, then the result color of setting a pixel of a bitmap data with indexed or single-bit-alpha pixel format
        /// will be transparent only if the color to set is completely transparent (has zero alpha).</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>255</c>, then the result color of setting a pixel of a bitmap data with indexed or single-bit-alpha pixel format
        /// will be opaque only if the color to set is completely opaque (its alpha value is <c>255</c>).</para>
        /// <para>For <see cref="PixelFormat"/>s without any alpha support the specified <paramref name="alphaThreshold"/> is used only to determine the source pixels to skip
        /// when another bitmap data is drawn into the returned instance.</para>
        /// <para>If a pixel of a bitmap data without alpha gradient support is set by the <see cref="IWritableBitmapData.SetPixel">IWritableBitmapData.SetPixel</see>/<see cref="IWritableBitmapDataRow.SetColor">IWritableBitmapDataRow.SetColor</see>
        /// methods or by the <see cref="IReadWriteBitmapDataRow.this">IReadWriteBitmapDataRow indexer</see>, and the pixel has an alpha value that is greater than <paramref name="alphaThreshold"/>,
        /// then the pixel to set will be blended with <paramref name="backColor"/>.</para>
        /// <note type="tip">
        /// <list type="bullet">
        /// <item>If <paramref name="pixelFormat"/> represents an indexed format you can use the <see cref="CreateBitmapData(Size, PixelFormat, Palette)"/> overload to specify the desired palette of the result.</item>
        /// <item>You can use the <see cref="BitmapDataExtensions.ToBitmap">ToBitmap</see> extension method to convert the created <see cref="IReadWriteBitmapData"/> to a <see cref="Bitmap"/> instance.</item>
        /// <item>To create an <see cref="IReadWriteBitmapData"/> instance from a native <see cref="Bitmap"/> use the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> extension method.</item>
        /// </list></note>
        /// </remarks>
        /// <seealso cref="CreateBitmapData(Size, PixelFormat, Palette)"/>
        /// <seealso cref="BitmapDataExtensions.ToBitmap"/>
        /// <seealso cref="BitmapExtensions.GetReadableBitmapData"/>
        /// <seealso cref="BitmapExtensions.GetWritableBitmapData"/>
        /// <seealso cref="BitmapExtensions.GetReadWriteBitmapData"/>
        public static IReadWriteBitmapData CreateBitmapData(Size size, PixelFormat pixelFormat = PixelFormat.Format32bppArgb, Color32 backColor = default, byte alphaThreshold = 128)
        {
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));

            return CreateManagedBitmapData(size, pixelFormat, backColor, alphaThreshold);
        }

        /// <summary>
        /// Creates a managed <see cref="IReadWriteBitmapData"/> with the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="palette"/>.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> or <paramref name="pixelFormat"/> has an invalid value.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> contains too many colors for the indexed format specified by <paramref name="pixelFormat"/>.</exception>
        /// <param name="size">The size of the bitmap data to create.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> specifies an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.</param>
        /// <returns>A managed <see cref="IReadWriteBitmapData"/> with the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="palette"/>.</returns>
        /// <remarks>
        /// <para>All possible <see cref="PixelFormat"/>s are supported, regardless of the native <see cref="Bitmap"/> support of the current operating system.
        /// <note>When <paramref name="pixelFormat"/> specifies a wide-color format (48/64 bit or 16 bit grayscale), then the returned instance will use the full 16-bit range of the color channels.
        /// This means a different raw content to Windows' wide-color <see cref="Bitmap"/> instances, which use 13-bit channels. But this difference is transparent in most cases
        /// unless we access actual raw content by the <see cref="IReadableBitmapDataRow.ReadRaw{T}">ReadRaw</see> and <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> methods.</note></para>
        /// <para>If <paramref name="palette"/> is not specified, then a default palette will be used for indexed formats, and a default <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/>
        /// value will be used.</para>
        /// <para><paramref name="palette"/> is ignored if <paramref name="pixelFormat"/> specifies a non-indexed format.</para>
        /// <note type="tip">
        /// <list type="bullet">
        /// <item>You can use the <see cref="BitmapDataExtensions.ToBitmap">ToBitmap</see> extension method to convert the created <see cref="IReadWriteBitmapData"/> to a <see cref="Bitmap"/> instance.</item>
        /// <item>To create an <see cref="IReadWriteBitmapData"/> instance from a native <see cref="Bitmap"/> use the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> extension method.</item>
        /// </list></note>
        /// </remarks>
        /// <seealso cref="CreateBitmapData(Size, PixelFormat, Color32, byte)"/>
        /// <seealso cref="BitmapDataExtensions.ToBitmap"/>
        /// <seealso cref="BitmapExtensions.GetReadableBitmapData"/>
        /// <seealso cref="BitmapExtensions.GetWritableBitmapData"/>
        /// <seealso cref="BitmapExtensions.GetReadWriteBitmapData"/>
        public static IReadWriteBitmapData CreateBitmapData(Size size, PixelFormat pixelFormat, Palette? palette)
        {
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));

            return CreateManagedBitmapData(size, pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette);
        }

        #endregion

        #region Unmanaged Wrapper for IntPtr

        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            if (buffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (Math.Abs(stride) < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));

            return CreateUnmanagedBitmapData(buffer, size, stride, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback);
        }

        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, PixelFormat pixelFormat, Palette? palette, Action<Palette>? setPalette = null, Action? disposeCallback = null)
        {
            if (buffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (Math.Abs(stride) < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));

            return CreateUnmanagedBitmapData(buffer, size, stride, pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette, setPalette, disposeCallback);
        }

        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            RowGetColor<IntPtr> rowGetColor, RowSetColor<IntPtr> rowSetColor, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            if (buffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (pixelFormatInfo.BitsPerPixel == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormatInfo.BitsPerPixel), 0), nameof(pixelFormatInfo));
            if (Math.Abs(stride) < pixelFormatInfo.PixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormatInfo.PixelFormat.GetByteWidth(size.Width)));
            if (pixelFormatInfo.IsIndexed)
                throw new ArgumentException(Res.ImagingNonIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColor == null)
                throw new ArgumentNullException(nameof(rowGetColor), PublicResources.ArgumentNull);
            if (rowSetColor == null)
                throw new ArgumentNullException(nameof(rowSetColor), PublicResources.ArgumentNull);
            return CreateUnmanagedCustomBitmapData(buffer, size, stride, pixelFormatInfo, rowGetColor, rowSetColor, backColor, alphaThreshold, disposeCallback);
        }

        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            RowGetColorIndex<IntPtr> rowGetColorIndex, RowSetColorIndex<IntPtr> rowSetColorIndex,
            Palette? palette = null, Action<Palette>? setPalette = null, Action? disposeCallback = null)
        {
            if (buffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (pixelFormatInfo.BitsPerPixel == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormatInfo.BitsPerPixel), 0), nameof(pixelFormatInfo));
            if (Math.Abs(stride) < pixelFormatInfo.PixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormatInfo.PixelFormat.GetByteWidth(size.Width)));
            if (!pixelFormatInfo.IsIndexed)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColorIndex == null)
                throw new ArgumentNullException(nameof(rowGetColorIndex), PublicResources.ArgumentNull);
            if (rowSetColorIndex == null)
                throw new ArgumentNullException(nameof(rowSetColorIndex), PublicResources.ArgumentNull);
            return CreateUnmanagedCustomBitmapData(buffer, size, stride, pixelFormatInfo, rowGetColorIndex, rowSetColorIndex, palette, setPalette, disposeCallback);
        }

        #endregion

        #region Managed Wrapper for 1D Arrays

        [SecuritySafeCritical]
        public static unsafe IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, PixelFormat pixelFormat = PixelFormat.Format32bppArgb, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (stride < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            int elementSize = sizeof(T);
            if (stride % elementSize != 0)
                throw new ArgumentException(Res.ImagingStrideInvalid(typeof(T), sizeof(T)), nameof(stride));
            int elementWidth = stride / elementSize;
            if (buffer.Length < elementWidth * size.Height)
                throw new ArgumentException(Res.ImagingBufferLengthTooSmall(elementWidth * size.Height), nameof(buffer));

            return CreateManagedBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback);
        }

        [SecuritySafeCritical]
        public static unsafe IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, PixelFormat pixelFormat, Palette? palette, Action<Palette>? setPalette = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (stride < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            int elementSize = sizeof(T);
            if (stride % elementSize != 0)
                throw new ArgumentException(Res.ImagingStrideInvalid(typeof(T), sizeof(T)), nameof(stride));
            int elementWidth = stride / elementSize;
            if (buffer.Length < elementWidth * size.Height)
                throw new ArgumentException(Res.ImagingBufferLengthTooSmall(elementWidth * size.Height), nameof(buffer));

            return CreateManagedBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette, setPalette, disposeCallback);
        }

        [SecuritySafeCritical]
        public static unsafe IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            RowGetColor<ArraySection<T>> rowGetColor, RowSetColor<ArraySection<T>> rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (pixelFormatInfo.BitsPerPixel == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormatInfo.BitsPerPixel), 0), nameof(pixelFormatInfo));
            if (pixelFormatInfo.IsIndexed)
                throw new ArgumentException(Res.ImagingNonIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (stride < pixelFormatInfo.PixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormatInfo.PixelFormat.GetByteWidth(size.Width)));
            int elementSize = sizeof(T);
            if (stride % elementSize != 0)
                throw new ArgumentException(Res.ImagingStrideInvalid(typeof(T), sizeof(T)), nameof(stride));
            int elementWidth = stride / elementSize;
            if (buffer.Length < elementWidth * size.Height)
                throw new ArgumentException(Res.ImagingBufferLengthTooSmall(elementWidth * size.Height), nameof(buffer));
            if (rowGetColor == null)
                throw new ArgumentNullException(nameof(rowGetColor), PublicResources.ArgumentNull);
            if (rowSetColor == null)
                throw new ArgumentNullException(nameof(rowSetColor), PublicResources.ArgumentNull);

            return CreateManagedCustomBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, pixelFormatInfo, rowGetColor, rowSetColor, backColor, alphaThreshold, disposeCallback);
        }

        [SecuritySafeCritical]
        public static unsafe IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            RowGetColorIndex<ArraySection<T>> rowGetColorIndex, RowSetColorIndex<ArraySection<T>> rowSetColorIndex,
            Palette? palette = null, Action<Palette>? setPalette = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (pixelFormatInfo.BitsPerPixel == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormatInfo.BitsPerPixel), 0), nameof(pixelFormatInfo));
            if (!pixelFormatInfo.IsIndexed)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (stride < pixelFormatInfo.PixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormatInfo.PixelFormat.GetByteWidth(size.Width)));
            int elementSize = sizeof(T);
            if (stride % elementSize != 0)
                throw new ArgumentException(Res.ImagingStrideInvalid(typeof(T), sizeof(T)), nameof(stride));
            int elementWidth = stride / elementSize;
            if (buffer.Length < elementWidth * size.Height)
                throw new ArgumentException(Res.ImagingBufferLengthTooSmall(elementWidth * size.Height), nameof(buffer));
            if (rowGetColorIndex == null)
                throw new ArgumentNullException(nameof(rowGetColorIndex), PublicResources.ArgumentNull);
            if (rowSetColorIndex == null)
                throw new ArgumentNullException(nameof(rowSetColorIndex), PublicResources.ArgumentNull);

            return CreateManagedCustomBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, pixelFormatInfo, rowGetColorIndex, rowSetColorIndex, palette, setPalette, disposeCallback);
        }


        [SecuritySafeCritical]
        public static unsafe IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormat pixelFormat = PixelFormat.Format32bppArgb, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            int stride = sizeof(T) * buffer.Width;
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);

            return CreateManagedBitmapData(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback);
        }

        [SecuritySafeCritical]
        public static unsafe IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormat pixelFormat, Palette? palette, Action<Palette>? setPalette = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            int stride = sizeof(T) * buffer.Width;
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);

            return CreateManagedBitmapData(buffer, pixelWidth, pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette, setPalette, disposeCallback);
        }

        [SecuritySafeCritical]
        public static unsafe IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormatInfo,
            RowGetColor<ArraySection<T>> rowGetColor, RowSetColor<ArraySection<T>> rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            if (pixelFormatInfo.BitsPerPixel == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormatInfo.BitsPerPixel), 0), nameof(pixelFormatInfo));
            if (pixelFormatInfo.IsIndexed)
                throw new ArgumentException(Res.ImagingNonIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            int stride = sizeof(T) * buffer.Width;
            if (stride < pixelFormatInfo.PixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (rowGetColor == null)
                throw new ArgumentNullException(nameof(rowGetColor), PublicResources.ArgumentNull);
            if (rowSetColor == null)
                throw new ArgumentNullException(nameof(rowSetColor), PublicResources.ArgumentNull);

            return CreateManagedCustomBitmapData(buffer, pixelWidth, pixelFormatInfo, rowGetColor, rowSetColor, backColor, alphaThreshold, disposeCallback);
        }

        [SecuritySafeCritical]
        public static unsafe IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormatInfo,
            RowGetColorIndex<ArraySection<T>> rowGetColorIndex, RowSetColorIndex<ArraySection<T>> rowSetColorIndex,
            Palette? palette = null, Action<Palette>? setPalette = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            if (pixelFormatInfo.BitsPerPixel == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormatInfo.BitsPerPixel), 0), nameof(pixelFormatInfo));
            if (!pixelFormatInfo.IsIndexed)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            int stride = sizeof(T) * buffer.Width;
            if (stride < pixelFormatInfo.PixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (rowGetColorIndex == null)
                throw new ArgumentNullException(nameof(rowGetColorIndex), PublicResources.ArgumentNull);
            if (rowSetColorIndex == null)
                throw new ArgumentNullException(nameof(rowSetColorIndex), PublicResources.ArgumentNull);

            return CreateManagedCustomBitmapData(buffer, pixelWidth, pixelFormatInfo, rowGetColorIndex, rowSetColorIndex, palette, setPalette, disposeCallback);
        }

        #endregion

        #endregion

        #region Load

        /// <summary>
        /// Loads a managed <see cref="IReadWriteBitmapData"/> instance from the specified <paramref name="stream"/> that was saved by
        /// the <see cref="BitmapDataExtensions.Save">BitmapDataExtensions.Save</see> method.
        /// </summary>
        /// <param name="stream">The stream to load the bitmap data from.</param>
        /// <returns>A managed <see cref="IReadWriteBitmapData"/> instance loaded from the specified <paramref name="stream"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginLoad">BeginLoad</see>
        /// or <see cref="LoadAsync">LoadAsync</see> (in .NET Framework 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
        /// </remarks>
        public static IReadWriteBitmapData Load(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return DoLoadBitmapData(AsyncContext.Null, stream)!;
        }

        /// <summary>
        /// Begins to load a managed <see cref="IReadWriteBitmapData"/> instance from the specified <paramref name="stream"/> asynchronously that was saved by
        /// the <see cref="BitmapDataExtensions.Save">BitmapDataExtensions.Save</see> method.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="stream">The stream to load the bitmap data from.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="LoadAsync">LoadAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndLoad">EndLoad</see> method.</para>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// </remarks>
        public static IAsyncResult BeginLoad(Stream stream, AsyncConfig? asyncConfig = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return AsyncContext.BeginOperation(ctx => DoLoadBitmapData(ctx, stream), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginLoad">BeginLoad</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="LoadAsync">LoadAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndLoad(IAsyncResult asyncResult)
            => AsyncContext.EndOperation<IReadWriteBitmapData>(asyncResult, nameof(BeginLoad));

#if !NET35
        /// <summary>
        /// Loads a managed <see cref="IReadWriteBitmapData"/> instance from the specified <paramref name="stream"/> asynchronously that was saved by
        /// the <see cref="BitmapDataExtensions.Save">BitmapDataExtensions.Save</see> method.
        /// </summary>
        /// <param name="stream">The stream to load the bitmap data from.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.
        /// its result can be <see langword="null"/>, if the operation was canceled and <see cref="AsyncConfigBase.ThrowIfCanceled"/> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <see cref="AsyncConfigBase.MaxDegreeOfParallelism"/> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// </remarks>
        public static Task<IReadWriteBitmapData?> LoadAsync(Stream stream, TaskConfig? asyncConfig = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return AsyncContext.DoOperationAsync(ctx => DoLoadBitmapData(ctx, stream), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Creates a native <see cref="IBitmapDataInternal"/> from a <see cref="Bitmap"/>.
        /// </summary>
        [SecuritySafeCritical]
        internal static unsafe IBitmapDataInternal CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, Color32 backColor = default, byte alphaThreshold = 128, Palette? palette = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            PixelFormat pixelFormat = bitmap.PixelFormat;

            // On Linux with libgdiplus 16bpp formats can be accessed only via 24bpp bitmap data
            PixelFormat bitmapDataPixelFormat = OSUtils.IsWindows
                ? pixelFormat
                : pixelFormat is PixelFormat.Format16bppRgb565 or PixelFormat.Format16bppRgb555 ? PixelFormat.Format24bppRgb : pixelFormat;

            Size size = bitmap.Size;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(Point.Empty, size), lockMode, bitmapDataPixelFormat);
            Action dispose = () => bitmap.UnlockBits(bitmapData);

            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Format24bppRgb:
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                    return CreateUnmanagedBitmapData(bitmapData.Scan0, size, bitmapData.Stride, pixelFormat, backColor, alphaThreshold, null, null, dispose);

                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    Debug.Assert(palette == null || palette.Equals(bitmap.Palette.Entries), "Non-null palette entries must match actual palette. Expected to be passed to re-use its cache only.");
                    palette ??= new Palette(bitmap.Palette.Entries, backColor.ToColor(), alphaThreshold);
                    return CreateUnmanagedBitmapData(bitmapData.Scan0, size, bitmapData.Stride, pixelFormat, backColor, alphaThreshold, palette, bitmap.SetPalette, dispose);

                case PixelFormat.Format64bppArgb:
                    return CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(pixelFormat),
                        (_, row, x) => ((Color64*)row)[x].ToColor32PlatformDependent(),
                        (_, row, x, c) => ((Color64*)row)[x] = c.ToColor64PlatformDependent(),
                        disposeCallback: dispose);

                case PixelFormat.Format64bppPArgb:
                    return CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(pixelFormat),
                        (_, row, x) => ((Color64*)row)[x].ToStraight32PlatformDependent(),
                        (_, row, x, c) => ((Color64*)row)[x] = c.ToPremultiplied64PlatformDependent(),
                        disposeCallback: dispose);

                case PixelFormat.Format48bppRgb:
                    return CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(pixelFormat),
                        (_, row, x) => ((Color48*)row)[x].ToColor32PlatformDependent(),
                        (bmpData, row, x, c) => ((Color48*)row)[x] = (c.A == Byte.MaxValue ? c : c.BlendWithBackground(bmpData.BackColor)).ToColor48PlatformDependent(),
                        backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb565:
                    return pixelFormat == bitmapDataPixelFormat
                        ? CreateUnmanagedBitmapData(bitmapData.Scan0, size, bitmapData.Stride, pixelFormat, backColor, alphaThreshold, null, null, dispose)
                        : CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(bitmapDataPixelFormat),
                            (_, row, x) => new Color16Rgb565(((Color24*)row)[x].ToColor32()).ToColor32(),
                            (bmpData, row, x, c) => ((Color24*)row)[x] = new Color24(new Color16Rgb565(c.A == Byte.MaxValue ? c : c.BlendWithBackground(bmpData.BackColor)).ToColor32()),
                            backColor, alphaThreshold, dispose);

                case PixelFormat.Format16bppRgb555:
                    return pixelFormat == bitmapDataPixelFormat
                        ? CreateUnmanagedBitmapData(bitmapData.Scan0, size, bitmapData.Stride, pixelFormat, backColor, alphaThreshold, null, null, dispose)
                        : CreateUnmanagedCustomBitmapData(bitmapData.Scan0, size, bitmapData.Stride, new PixelFormatInfo(bitmapDataPixelFormat),
                            (_, row, x) => new Color16Rgb555(((Color24*)row)[x].ToColor32()).ToColor32(),
                            (bmpData, row, x, c) => ((Color24*)row)[x] = new Color24(new Color16Rgb555(c.A == Byte.MaxValue ? c : c.BlendWithBackground(bmpData.BackColor)).ToColor32()),
                            backColor, alphaThreshold, dispose);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"));
            }
        }

        /// <summary>
        /// Creates a native <see cref="IBitmapDataInternal"/> by a quantizer session re-using its palette if possible.
        /// </summary>
        internal static IBitmapDataInternal CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, IQuantizingSession quantizingSession)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            var pixelFormat = bitmap.PixelFormat;
            if (!pixelFormat.IsIndexed() || quantizingSession.Palette == null)
                return CreateBitmapData(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold);

            // checking if bitmap and quantizer palette has the same entries
            if (!quantizingSession.Palette.Equals(bitmap.Palette.Entries))
                return CreateBitmapData(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold);

            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));

            // here the quantizer and the target bitmap uses the same palette
            return CreateBitmapData(bitmap, lockMode, quantizingSession.BackColor, quantizingSession.AlphaThreshold, quantizingSession.Palette);
        }

        /// <summary>
        /// Creates a managed <see cref="IBitmapDataInternal"/> with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.
        /// </summary>
        internal static IBitmapDataInternal CreateManagedBitmapData(Size size, PixelFormat pixelFormat = PixelFormat.Format32bppArgb, Color32 backColor = default, byte alphaThreshold = 128, Palette? palette = null)
        {
            if (pixelFormat.IsIndexed() && palette != null)
            {
                int maxColors = 1 << pixelFormat.ToBitsPerPixel();
                if (palette.Count > maxColors)
                    throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat), nameof(palette));
            }

            Debug.Assert(palette == null || backColor.ToOpaque() == palette.BackColor && alphaThreshold == palette.AlphaThreshold);
            return pixelFormat switch
            {
                PixelFormat.Format32bppArgb => new ManagedBitmapData<Color32, ManagedBitmapDataRow32Argb>(size, pixelFormat),
                PixelFormat.Format32bppPArgb => new ManagedBitmapData<Color32, ManagedBitmapDataRow32PArgb>(size, pixelFormat),
                PixelFormat.Format32bppRgb => new ManagedBitmapData<Color32, ManagedBitmapDataRow32Rgb>(size, pixelFormat, backColor, alphaThreshold),
                PixelFormat.Format24bppRgb => new ManagedBitmapData<Color24, ManagedBitmapDataRow24Rgb>(size, pixelFormat, backColor, alphaThreshold),
                PixelFormat.Format8bppIndexed => new ManagedBitmapData<byte, ManagedBitmapDataRow8I>(size, pixelFormat, backColor, alphaThreshold, palette),
                PixelFormat.Format4bppIndexed => new ManagedBitmapData<byte, ManagedBitmapDataRow4I>(size, pixelFormat, backColor, alphaThreshold, palette),
                PixelFormat.Format1bppIndexed => new ManagedBitmapData<byte, ManagedBitmapDataRow1I>(size, pixelFormat, backColor, alphaThreshold, palette),
                PixelFormat.Format64bppArgb => new ManagedBitmapData<Color64, ManagedBitmapDataRow64Argb>(size, pixelFormat),
                PixelFormat.Format64bppPArgb => new ManagedBitmapData<Color64, ManagedBitmapDataRow64PArgb>(size, pixelFormat),
                PixelFormat.Format48bppRgb => new ManagedBitmapData<Color48, ManagedBitmapDataRow48Rgb>(size, pixelFormat, backColor, alphaThreshold),
                PixelFormat.Format16bppRgb565 => new ManagedBitmapData<Color16Rgb565, ManagedBitmapDataRow16Rgb565>(size, pixelFormat, backColor, alphaThreshold),
                PixelFormat.Format16bppRgb555 => new ManagedBitmapData<Color16Rgb555, ManagedBitmapDataRow16Rgb555>(size, pixelFormat, backColor, alphaThreshold),
                PixelFormat.Format16bppArgb1555 => new ManagedBitmapData<Color16Argb1555, ManagedBitmapDataRow16Argb1555>(size, pixelFormat, backColor, alphaThreshold),
                PixelFormat.Format16bppGrayScale => new ManagedBitmapData<Color16Gray, ManagedBitmapDataRow16Gray>(size, pixelFormat, backColor, alphaThreshold),
                _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat))
            };
        }

        /// <summary>
        /// Creates a managed <see cref="IBitmapDataInternal"/> for a preallocated 1D array.
        /// </summary>
        internal static IBitmapDataInternal CreateManagedBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128,
            Palette? palette = null, Action<Palette>? setPalette = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            if (pixelFormat.IsIndexed() && palette != null)
            {
                int maxColors = 1 << pixelFormat.ToBitsPerPixel();
                if (palette.Count > maxColors)
                    throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat), nameof(palette));
            }

            Debug.Assert(palette == null || backColor.ToOpaque() == palette.BackColor && alphaThreshold == palette.AlphaThreshold);
            return pixelFormat switch
            {
                PixelFormat.Format32bppArgb => buffer is Array2D<Color32> buf
                    ? new ManagedBitmapDataWrapper<Color32, ManagedBitmapDataRow32Argb>(buf, pixelWidth, pixelFormat, default, default, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow32Argb<T>>(buffer, pixelWidth, pixelFormat, default, default, null, null, disposeCallback),
                PixelFormat.Format32bppPArgb => buffer is Array2D<Color32> buf
                    ? new ManagedBitmapDataWrapper<Color32, ManagedBitmapDataRow32PArgb>(buf, pixelWidth, pixelFormat, default, default, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow32PArgb<T>>(buffer, pixelWidth, pixelFormat, default, default, null, null, disposeCallback),
                PixelFormat.Format32bppRgb => buffer is Array2D<Color32> buf
                    ? new ManagedBitmapDataWrapper<Color32, ManagedBitmapDataRow32Rgb>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow32Rgb<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback),
                PixelFormat.Format24bppRgb => buffer is Array2D<Color24> buf
                    ? new ManagedBitmapDataWrapper<Color24, ManagedBitmapDataRow24Rgb>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow24Rgb<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback),
                PixelFormat.Format8bppIndexed => buffer is Array2D<byte> buf
                    ? new ManagedBitmapDataWrapper<byte, ManagedBitmapDataRow8I>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow8I<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback),
                PixelFormat.Format4bppIndexed => buffer is Array2D<byte> buf
                    ? new ManagedBitmapDataWrapper<byte, ManagedBitmapDataRow4I>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow4I<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback),
                PixelFormat.Format1bppIndexed => buffer is Array2D<byte> buf
                    ? new ManagedBitmapDataWrapper<byte, ManagedBitmapDataRow1I>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow1I<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback),
                PixelFormat.Format64bppArgb => buffer is Array2D<Color64> buf
                    ? new ManagedBitmapDataWrapper<Color64, ManagedBitmapDataRow64Argb>(buf, pixelWidth, pixelFormat, default, default, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow64Argb<T>>(buffer, pixelWidth, pixelFormat, default, default, null, null, disposeCallback),
                PixelFormat.Format64bppPArgb => buffer is Array2D<Color64> buf
                    ? new ManagedBitmapDataWrapper<Color64, ManagedBitmapDataRow64PArgb>(buf, pixelWidth, pixelFormat, default, default, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow64PArgb<T>>(buffer, pixelWidth, pixelFormat, default, default, null, null, disposeCallback),
                PixelFormat.Format48bppRgb => buffer is Array2D<Color48> buf
                    ? new ManagedBitmapDataWrapper<Color48, ManagedBitmapDataRow48Rgb>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow48Rgb<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback),
                PixelFormat.Format16bppRgb565 => buffer is Array2D<Color16Rgb565> buf
                    ? new ManagedBitmapDataWrapper<Color16Rgb565, ManagedBitmapDataRow16Rgb565>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow16Rgb565<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback),
                PixelFormat.Format16bppRgb555 => buffer is Array2D<Color16Rgb555> buf
                    ? new ManagedBitmapDataWrapper<Color16Rgb555, ManagedBitmapDataRow16Rgb555>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow16Rgb555<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback),
                PixelFormat.Format16bppArgb1555 => buffer is Array2D<Color16Argb1555> buf
                    ? new ManagedBitmapDataWrapper<Color16Argb1555, ManagedBitmapDataRow16Argb1555>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow16Argb1555<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback),
                PixelFormat.Format16bppGrayScale => buffer is Array2D<Color16Gray> buf
                    ? new ManagedBitmapDataWrapper<Color16Gray, ManagedBitmapDataRow16Gray>(buf, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback)
                    : new ManagedBitmapDataWrapper<T, ManagedBitmapDataRow16Gray<T>>(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback),
                _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat))
            };
        }

        internal static IBitmapDataInternal CreateManagedCustomBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormat,
            RowGetColor<ArraySection<T>> rowGetColor, RowSetColor<ArraySection<T>> rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            return new ManagedCustomBitmapData<T>(buffer, pixelWidth, pixelFormat.PixelFormat, rowGetColor, rowSetColor, backColor, alphaThreshold, disposeCallback);
        }

        internal static IBitmapDataInternal CreateManagedCustomBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormat,
            RowGetColorIndex<ArraySection<T>> rowGetColorIndex, RowSetColorIndex<ArraySection<T>> rowSetColorIndex,
            Palette? palette = null, Action<Palette>? setPalette = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            return new ManagedCustomBitmapDataIndexed<T>(buffer, pixelWidth, pixelFormat.PixelFormat, rowGetColorIndex, rowSetColorIndex, palette, setPalette, disposeCallback);
        }

        [SecurityCritical]
        internal static IBitmapDataInternal CreateUnmanagedBitmapData(IntPtr buffer, Size size, int stride, PixelFormat pixelFormat, Color32 backColor = default, byte alphaThreshold = 128,
            Palette? palette = null, Action<Palette>? setPalette = null, Action? disposeCallback = null)
        {
            if (pixelFormat.IsIndexed() && palette != null)
            {
                int maxColors = 1 << pixelFormat.ToBitsPerPixel();
                if (palette.Count > maxColors)
                    throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat), nameof(palette));
            }
            Debug.Assert(palette == null || backColor.ToOpaque() == palette.BackColor && alphaThreshold == palette.AlphaThreshold);

            return pixelFormat switch
            {
                PixelFormat.Format32bppArgb => new UnmanagedBitmapData<UnmanagedBitmapDataRow32Argb>(buffer, size, stride, pixelFormat, default, default, null, null, disposeCallback),
                PixelFormat.Format32bppPArgb => new UnmanagedBitmapData<UnmanagedBitmapDataRow32PArgb>(buffer, size, stride, pixelFormat, default, default, null, null, disposeCallback),
                PixelFormat.Format32bppRgb => new UnmanagedBitmapData<UnmanagedBitmapDataRow32Rgb>(buffer, size, stride, pixelFormat, backColor, default, null, null, disposeCallback),
                PixelFormat.Format24bppRgb => new UnmanagedBitmapData<UnmanagedBitmapDataRow24Rgb>(buffer, size, stride, pixelFormat, backColor, default, null, null, disposeCallback),
                PixelFormat.Format8bppIndexed => new UnmanagedBitmapData<UnmanagedBitmapDataRow8I>(buffer, size, stride, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback),
                PixelFormat.Format4bppIndexed => new UnmanagedBitmapData<UnmanagedBitmapDataRow4I>(buffer, size, stride, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback),
                PixelFormat.Format1bppIndexed => new UnmanagedBitmapData<UnmanagedBitmapDataRow1I>(buffer, size, stride, pixelFormat, backColor, alphaThreshold, palette, setPalette, disposeCallback),
                PixelFormat.Format64bppArgb => new UnmanagedBitmapData<UnmanagedBitmapDataRow64Argb>(buffer, size, stride, pixelFormat, default, default, null, null, disposeCallback),
                PixelFormat.Format64bppPArgb => new UnmanagedBitmapData<UnmanagedBitmapDataRow64PArgb>(buffer, size, stride, pixelFormat, default, default, null, null, disposeCallback),
                PixelFormat.Format48bppRgb => new UnmanagedBitmapData<UnmanagedBitmapDataRow48Rgb>(buffer, size, stride, pixelFormat, backColor, default, null, null, disposeCallback),
                PixelFormat.Format16bppRgb565 => new UnmanagedBitmapData<UnmanagedBitmapDataRow16Rgb565>(buffer, size, stride, pixelFormat, backColor, default, null, null, disposeCallback),
                PixelFormat.Format16bppRgb555 => new UnmanagedBitmapData<UnmanagedBitmapDataRow16Rgb555>(buffer, size, stride, pixelFormat, backColor, default, null, null, disposeCallback),
                PixelFormat.Format16bppArgb1555 => new UnmanagedBitmapData<UnmanagedBitmapDataRow16Argb1555>(buffer, size, stride, pixelFormat, backColor, alphaThreshold, null, null, disposeCallback),
                PixelFormat.Format16bppGrayScale => new UnmanagedBitmapData<UnmanagedBitmapDataRow16Gray>(buffer, size, stride, pixelFormat, backColor, default, palette, null, disposeCallback),
                _ => throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"))
            };
        }

        [SecurityCritical]
        internal static IBitmapDataInternal CreateUnmanagedCustomBitmapData(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormat,
            RowGetColor<IntPtr> rowGetColor, RowSetColor<IntPtr> rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            return new UnmanagedCustomBitmapData(buffer, size, stride, pixelFormat.PixelFormat, rowGetColor, rowSetColor, backColor, alphaThreshold, disposeCallback);
        }

        [SecurityCritical]
        internal static IBitmapDataInternal CreateUnmanagedCustomBitmapData(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormat,
            RowGetColorIndex<IntPtr> rowGetColorIndex, RowSetColorIndex<IntPtr> rowSetColorIndex,
            Palette? palette = null, Action<Palette>? setPalette = null, Action? disposeCallback = null)
        {
            return new UnmanagedCustomBitmapDataIndexed(buffer, size, stride, pixelFormat.PixelFormat, rowGetColorIndex, rowSetColorIndex, palette, setPalette, disposeCallback);
        }

        internal static void DoSaveBitmapData(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, Stream stream)
        {
            PixelFormat pixelFormat = bitmapData.PixelFormat;
            Debug.Assert(pixelFormat == PixelFormat.Format32bppArgb || bitmapData.RowSize >= pixelFormat.GetByteWidth(rect.Right));
            Debug.Assert(pixelFormat.IsAtByteBoundary(rect.Left));

            context.Progress?.New(DrawingOperation.Saving, rect.Height + 1);
            var writer = new BinaryWriter(stream);

            writer.Write(magicNumber);
            writer.Write(rect.Width);
            writer.Write(rect.Height);
            writer.Write((int)pixelFormat);
            writer.Write(bitmapData.BackColor.ToArgb());
            writer.Write(bitmapData.AlphaThreshold);

            Palette? palette = bitmapData.Palette;
            writer.Write(palette?.Count ?? 0);
            if (palette != null)
            {
                foreach (Color32 entry in palette.Entries)
                    writer.Write(entry.ToArgb());
            }

            context.Progress?.Increment();
            if (context.IsCancellationRequested)
                return;

            try
            {
                if (pixelFormat.ToBitsPerPixel() > 32 && bitmapData is UnmanagedBitmapDataBase && ColorExtensions.Max16BppValue != UInt16.MaxValue)
                {
                    DoSaveWidePlatformDependent(context, bitmapData, rect, writer);
                    return;
                }

                DoSaveRaw(context, bitmapData, rect, writer);
            }
            finally
            {
                stream.Flush();
            }
        }

        #endregion

        #region Private Methods

        #region Save

        private static void DoSaveWidePlatformDependent(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            PixelFormat pixelFormat = bitmapData.PixelFormat;
            int byteLength = pixelFormat.ToBitsPerPixel() >> 3;

            // using a temp 1x1 managed bitmap data for the conversion
            using IBitmapDataInternal tempData = CreateManagedBitmapData(new Size(1, 1), pixelFormat, bitmapData.BackColor, bitmapData.AlphaThreshold);
            IBitmapDataRowInternal tempRow = tempData.DoGetRow(0);
            IBitmapDataRowInternal row = bitmapData.DoGetRow(rect.Top);
            for (int y = 0; y < rect.Height; y++)
            {
                if (context.IsCancellationRequested)
                    return;

                for (int x = rect.Left; x < rect.Right; x++)
                {
                    tempRow.DoSetColor32(0, row.DoGetColor32(x));
                    for (int i = 0; i < byteLength; i++)
                        writer.Write(tempRow.DoReadRaw<byte>(i));
                }

                row.MoveNextRow();
                context.Progress?.Increment();
            }
        }

        private static void DoSaveRaw(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            PixelFormat pixelFormat = bitmapData.PixelFormat;
            int bpp = pixelFormat.ToBitsPerPixel();
            Debug.Assert(pixelFormat.IsAtByteBoundary(rect.Left));

            switch (bpp)
            {
                case 1:
                    rect.X >>= 3;
                    rect.Width = pixelFormat.IsAtByteBoundary(rect.Width) ? rect.Width >> 3 : (rect.Width >> 3) + 1;
                    DoSaveRawBytes(context, bitmapData, rect, writer);
                    return;
                case 4:
                    rect.X >>= 1;
                    rect.Width = pixelFormat.IsAtByteBoundary(rect.Width) ? rect.Width >> 1 : (rect.Width >> 1) + 1;
                    DoSaveRawBytes(context, bitmapData, rect, writer);
                    return;
                case 8:
                    DoSaveRawBytes(context, bitmapData, rect, writer);
                    return;
                case 16:
                    DoSaveRawShorts(context, bitmapData, rect, writer);
                    return;
                case 32:
                    DoSaveRawInts(context, bitmapData, rect, writer);
                    return;
                case 64:
                    DoSaveRawLongs(context, bitmapData, rect, writer);
                    return;
                default: // 24/48bpp
                    int byteSize = bpp >> 3;
                    rect.X *= byteSize;
                    rect.Width *= byteSize;
                    DoSaveRawBytes(context, bitmapData, rect, writer);
                    return;
            }
        }

        private static void DoSaveRawBytes(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.DoGetRow(rect.Top);
            for (int y = 0; y < rect.Height; y++)
            {
                if (context.IsCancellationRequested)
                    return;

                for (int x = rect.Left; x < rect.Right; x++)
                    writer.Write(row.DoReadRaw<byte>(x));

                row.MoveNextRow();
                context.Progress?.Increment();
            }
        }

        private static void DoSaveRawShorts(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.DoGetRow(rect.Top);
            for (int y = 0; y < rect.Height; y++)
            {
                if (context.IsCancellationRequested)
                    return;

                for (int x = rect.Left; x < rect.Right; x++)
                    writer.Write(row.DoReadRaw<short>(x));

                row.MoveNextRow();
                context.Progress?.Increment();
            }
        }

        private static void DoSaveRawInts(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.DoGetRow(rect.Top);
            for (int y = 0; y < rect.Height; y++)
            {
                if (context.IsCancellationRequested)
                    return;

                for (int x = rect.Left; x < rect.Right; x++)
                    writer.Write(row.DoReadRaw<int>(x));

                row.MoveNextRow();
                context.Progress?.Increment();
            }
        }

        private static void DoSaveRawLongs(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.DoGetRow(rect.Top);
            for (int y = 0; y < rect.Height; y++)
            {
                if (context.IsCancellationRequested)
                    return;

                for (int x = rect.Left; x < rect.Right; x++)
                    writer.Write(row.DoReadRaw<long>(x));

                row.MoveNextRow();
                context.Progress?.Increment();
            }
        }

        #endregion

        #region Load

        [SuppressMessage("ReSharper", "AssignmentInConditionalExpression", Justification = "Intended")]
        [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "ReSharper issue")]
        private static IReadWriteBitmapData? DoLoadBitmapData(IAsyncContext context, Stream stream)
        {
            context.Progress?.New(DrawingOperation.Loading, 1000);
            var reader = new BinaryReader(stream);

            if (reader.ReadInt32() != magicNumber)
                throw new ArgumentException(Res.ImagingNotBitmapDataStream, nameof(stream));
            var size = new Size(reader.ReadInt32(), reader.ReadInt32());
            var pixelFormat = (PixelFormat)reader.ReadInt32();
            Color32 backColor = Color32.FromArgb(reader.ReadInt32());
            byte alphaThreshold = reader.ReadByte();

            Palette? palette = null;
            int paletteLength = reader.ReadInt32();
            if (paletteLength > 0)
            {
                var entries = new Color32[paletteLength];
                for (int i = 0; i < paletteLength; i++)
                    entries[i] = Color32.FromArgb(reader.ReadInt32());
                palette = new Palette(entries, backColor, alphaThreshold);
            }

            context.Progress?.SetProgressValue((int)(stream.Position * 1000 / stream.Length));
            if (context.IsCancellationRequested)
                return null;

            IBitmapDataInternal result = CreateManagedBitmapData(size, pixelFormat, backColor, alphaThreshold, palette);
            int bpp = pixelFormat.ToBitsPerPixel();
            bool canceled = false;
            try
            {
                IBitmapDataRowInternal row = result.DoGetRow(0);
                for (int y = 0; y < result.Height; y++)
                {
                    if (canceled = context.IsCancellationRequested)
                        return null;

                    switch (bpp)
                    {
                        case 32:
                            for (int x = 0; x < result.Width; x++)
                                row.DoWriteRaw(x, reader.ReadInt32());
                            break;
                        case 16:
                            for (int x = 0; x < result.Width; x++)
                                row.DoWriteRaw(x, reader.ReadInt16());
                            break;
                        case 64:
                            for (int x = 0; x < result.Width; x++)
                                row.DoWriteRaw(x, reader.ReadInt64());
                            break;
                        default:
                            for (int x = 0; x < result.RowSize; x++)
                                row.DoWriteRaw(x, reader.ReadByte());
                            break;
                    }

                    row.MoveNextRow();
                    context.Progress?.SetProgressValue((int)(stream.Position * 1000 / stream.Length));
                }

                return (canceled = context.IsCancellationRequested) ? null : result;
            }
            finally
            {
                if (canceled)
                    result.Dispose();
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
