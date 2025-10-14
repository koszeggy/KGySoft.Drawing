#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataFactory.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
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
using System.IO;
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif
using System.Security;
using KGySoft.Collections;
#if !NET35
using System.Threading.Tasks;
#endif

using KGySoft.CoreLibraries;
using KGySoft.Threading;

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

        #region Fields

        private static ArrayPoolingStrategy poolingStrategy = ArrayPoolingStrategy.IfByteArrayBased;

        #endregion

        #region Properties

        /// <summary>
        /// On platforms where array pooling is available, gets or sets the strategy to be used when allocating buffer for managed bitmap data
        /// instances by the self-allocating <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">CreateBitmapData</see> methods.
        /// <br/>Default value: <see cref="ArrayPoolingStrategy.IfByteArrayBased"/>.
        /// </summary>
        public static ArrayPoolingStrategy PoolingStrategy
        {
            get => poolingStrategy;
            set
            {
                if (!Enum<ArrayPoolingStrategy>.IsDefined(value))
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));
                poolingStrategy = value;
            }
        }

        #endregion

        #region Methods

        #region Public Methods

        #region CreateBitmapData

        #region Self-Allocating

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format32bppArgb"/>.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section for details. The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.</returns>
        /// <seealso cref="CreateBitmapData(Size, KnownPixelFormat, Palette)"/>
        /// <overloads>There are quite a few overloads of the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataFactory.CreateBitmapData">CreateBitmapData</see> method but they can be grouped into different categories:
        /// <list type="bullet">
        /// <item>The ones whose first parameter is <see cref="Size"/>, or the first couple of parameters are integers for width and height,
        /// are allocating the buffer for the created bitmap data by themselves, whereas the others use preallocated buffers.
        /// These overloads may use array pooling. See also the <see cref="PoolingStrategy"/> property.</item>
        /// <item>The overloads whose first parameter name is <c>buffer</c> can be used to create a bitmap data for a preallocated buffer. This buffer can be a one or two-dimensional array,
        /// an <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> or <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a>
        /// value wrapping a one dimensional array that represents a section of the array as a one or two-dimensional view, or it can be an <see cref="IntPtr"/> representing a potentially unmanaged buffer.</item>
        /// <item>If an overload has a <see cref="KnownPixelFormat"/> parameter, then it can be used to create a bitmap data using one of the pixel formats with built-in support.</item>
        /// <item>To create a bitmap data with a custom pixel format you can pick the overloads that have a <see cref="PixelFormatInfo"/> parameter along with a couple of delegates, or the ones
        /// with a <see cref="CustomBitmapDataConfig"/> or <see cref="CustomIndexedBitmapDataConfig"/> parameter for the best customization.</item>
        /// <item>For indexed pixel formats look for the overloads that have a <see cref="Palette"/> or a <see cref="CustomIndexedBitmapDataConfig"/> parameter.</item>
        /// </list></overloads>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.</exception>
        /// <exception cref="OverflowException">The dimensions of <paramref name="size"/> are too large to allocate a buffer for it.</exception>
        public static IReadWriteBitmapData CreateBitmapData(Size size, KnownPixelFormat pixelFormat = KnownPixelFormat.Format32bppArgb,
            Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(size, pixelFormat);
            return CreateManagedBitmapData(size, pixelFormat, backColor, alphaThreshold, WorkingColorSpace.Default, null);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.
        /// </summary>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section for details. The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="size"/> and <paramref name="pixelFormat"/>.</returns>
        /// <remarks>
        /// <para>This method supports predefined pixel formats. To create a bitmap data with some custom pixel format use the overloads that have <see cref="PixelFormatInfo"/> parameters.</para>
        /// <para>The <paramref name="backColor"/> parameter has no practical effect if <paramref name="pixelFormat"/> has alpha gradient support, and it does not affect the actual content of the returned instance.
        /// To set all pixels to a color use the <see cref="BitmapDataExtensions.Clear(IWritableBitmapData, Color32, IDitherer?)">Clear</see> extension method.</para>
        /// <para>If <paramref name="alphaThreshold"/> is zero, then setting a fully transparent pixel in a bitmap data with indexed or single-bit-alpha pixel format
        /// will blend the pixel to set with <paramref name="backColor"/> even if the bitmap data can handle transparent pixels.</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>1</c>, then the result color of setting a pixel of a bitmap data with indexed or single-bit-alpha pixel format
        /// will be transparent only if the color to set is completely transparent (has zero alpha).</para>
        /// <para>If <paramref name="alphaThreshold"/> is <c>255</c>, then the result color of setting a pixel of a bitmap data with indexed or single-bit-alpha pixel format
        /// will be opaque only if the color to set is completely opaque (its alpha value is <c>255</c>).</para>
        /// <para>For <see cref="KnownPixelFormat"/>s without any alpha support the specified <paramref name="alphaThreshold"/> is used only to determine the source pixels to skip
        /// when another bitmap data is drawn into the returned instance.</para>
        /// <para>If a pixel of a bitmap data without alpha gradient support is set by the <see cref="IWritableBitmapData.SetPixel">IWritableBitmapData.SetPixel</see>/<see cref="IWritableBitmapDataRow.SetColor">IWritableBitmapDataRow.SetColor</see>
        /// methods or by the <see cref="IReadWriteBitmapDataRow.this">IReadWriteBitmapDataRow indexer</see>, and the pixel has an alpha value that is greater than <paramref name="alphaThreshold"/>,
        /// then the pixel to set will be blended with <paramref name="backColor"/>.</para>
        /// <para>The <paramref name="workingColorSpace"/> parameter indicates the preferred color space when working with the result bitmap data.
        /// Blending operations performed by this library (eg. by <see cref="IWritableBitmapData.SetPixel">IWritableBitmapData.SetPixel</see> when blending is necessary as described above,
        /// or by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> extension methods) respect the value of this parameter.
        /// Blending in the linear color space produces natural results but the operation is a bit slower if the actual
        /// pixel format is not in the linear color space, and the result is different from the results of most applications including popular image processors and web browsers.
        /// See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</para>
        /// <para>This method allocates the underlying managed buffer internally. On platforms where array pooling is available, the underlying buffer may be rented from a pool.
        /// The actual behavior can be controlled by the <see cref="PoolingStrategy"/> property.</para>
        /// <note type="tip">
        /// <list type="bullet">
        /// <item>If <paramref name="pixelFormat"/> represents an indexed format you can use the <see cref="CreateBitmapData(Size, KnownPixelFormat, Palette)"/> overload to specify the desired palette of the result.</item>
        /// <item>To create an <see cref="IReadWriteBitmapData"/> instance from a platform specific bitmap type such as <a href="https://docs.microsoft.com/en-us/dotnet/api/System.Drawing.Bitmap" target="_blank">Bitmap</a>
        /// or <a href="https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap" target="_blank">WriteableBitmap</a>, use the <c>GetReadWriteBitmapData</c> extension methods for various platform dependent
        /// bitmap implementations. See the <strong>Remarks</strong> section of the <see cref="N:KGySoft.Drawing"/> namespace for a list about the technologies with dedicated support,
        /// and the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for all sources.</item>
        /// </list></note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="OverflowException">The dimensions of <paramref name="size"/> are too large to allocate a buffer for it.</exception>
        /// <seealso cref="CreateBitmapData(Size, KnownPixelFormat, Palette)"/>
        /// <seealso cref="PoolingStrategy"/>
        public static IReadWriteBitmapData CreateBitmapData(Size size, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(size, pixelFormat, workingColorSpace);
            return CreateManagedBitmapData(size, pixelFormat, backColor, alphaThreshold, workingColorSpace, null);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="palette"/>.
        /// </summary>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> represents an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="palette"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> is too large for the specified <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="OverflowException">The dimensions of <paramref name="size"/> are too large to allocate a buffer for it.</exception>
        /// <seealso cref="CreateBitmapData(Size, KnownPixelFormat, Color32, byte)"/>
        public static IReadWriteBitmapData CreateBitmapData(Size size, KnownPixelFormat pixelFormat, Palette? palette)
        {
            ValidateArguments(size, pixelFormat, WorkingColorSpace.Default, palette);
            return CreateManagedBitmapData(size, pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default, palette);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="width"/>, <paramref name="height"/> and <paramref name="pixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <param name="width">The width of the bitmap data to create in pixels.</param>
        /// <param name="height">The height of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format32bppArgb"/>.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section for details. The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance with the specified dimensions and <paramref name="pixelFormat"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is zero or negative
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.</exception>
        /// <exception cref="OverflowException">The specified <paramref name="width"/> and <paramref name="height"/> are too large to allocate a buffer for it.</exception>
        /// <seealso cref="CreateBitmapData(int, int, KnownPixelFormat, Palette)"/>
        public static IReadWriteBitmapData CreateBitmapData(int width, int height, KnownPixelFormat pixelFormat = KnownPixelFormat.Format32bppArgb,
            Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(width, height, pixelFormat);
            return CreateManagedBitmapData(new Size(width, height), pixelFormat, backColor, alphaThreshold, WorkingColorSpace.Default, null);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="width"/>, <paramref name="height"/> and <paramref name="pixelFormat"/>.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <param name="width">The width of the bitmap data to create in pixels.</param>
        /// <param name="height">The height of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section for details. The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance with the specified dimensions and <paramref name="pixelFormat"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is zero or negative
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="OverflowException">The specified <paramref name="width"/> and <paramref name="height"/> are too large to allocate a buffer for it.</exception>
        /// <seealso cref="CreateBitmapData(Size, KnownPixelFormat, Palette)"/>
        public static IReadWriteBitmapData CreateBitmapData(int width, int height, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128)
        {
            ValidateArguments(width, height, pixelFormat, workingColorSpace);
            return CreateManagedBitmapData(new Size(width, height), pixelFormat, backColor, alphaThreshold, workingColorSpace, null);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with the specified <paramref name="width"/>, <paramref name="height"/>, <paramref name="pixelFormat"/> and <paramref name="palette"/>.
        /// </summary>
        /// <param name="width">The width of the bitmap data to create in pixels.</param>
        /// <param name="height">The height of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The desired pixel format of the bitmap data to create.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> represents an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance with the specified dimensions, <paramref name="pixelFormat"/> and <paramref name="palette"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is zero or negative
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> is too large for the specified <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="OverflowException">The specified <paramref name="width"/> and <paramref name="height"/> are too large to allocate a buffer for it.</exception>
        /// <seealso cref="CreateBitmapData(Size, KnownPixelFormat, Color32, byte)"/>
        public static IReadWriteBitmapData CreateBitmapData(int width, int height, KnownPixelFormat pixelFormat, Palette? palette)
        {
            ValidateArguments(width, height, pixelFormat, WorkingColorSpace.Default, palette);
            return CreateManagedBitmapData(new Size(width, height), pixelFormat, palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default, palette);
        }

        #endregion

        #region Managed Wrapper for 1D Arrays

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance for a preallocated one dimensional array with the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.
        /// If the actual image data starts at some offset use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, KnownPixelFormat, Color32, byte, Action?)"/> overload instead.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format32bppArgb"/>.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[] buffer, Size size, int stride,
            KnownPixelFormat pixelFormat = KnownPixelFormat.Format32bppArgb, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer.AsSection(), size, stride, pixelFormat, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance for a preallocated one dimensional array with the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.
        /// If the actual image data starts at some offset use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, KnownPixelFormat, WorkingColorSpace, Color32, byte, Action?)"/> overload instead.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[] buffer, Size size, int stride, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer.AsSection(), size, stride, pixelFormat, workingColorSpace, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance for a preallocated one dimensional array with the specified parameters.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.
        /// If the actual image data starts at some offset use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, KnownPixelFormat, Palette?, Func{Palette, bool}?, Action?)"/> overload instead.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> represents an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[] buffer, Size size, int stride, KnownPixelFormat pixelFormat,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer.AsSection(), size, stride, pixelFormat, palette, trySetPaletteCallback, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format for a preallocated one dimensional array with the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.
        /// If the actual image data starts at some offset use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, PixelFormatInfo, Func{ICustomBitmapDataRow{T}, int, Color32}, Action{ICustomBitmapDataRow{T}, int, Color32}, Color32, byte, Action?)"/> overload instead.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormatInfo"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[] buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32>? rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer.AsSection(), size, stride, pixelFormatInfo, rowGetColor, rowSetColor, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format for a preallocated one dimensional array with the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.
        /// If the actual image data starts at some offset use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, PixelFormatInfo, Func{ICustomBitmapDataRow{T}, int, Color32}, Action{ICustomBitmapDataRow{T}, int, Color32}, WorkingColorSpace, Color32, byte, Action?)"/> overload instead.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormatInfo"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[] buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32>? rowSetColor,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer.AsSection(), size, stride, pixelFormatInfo, rowGetColor, rowSetColor, workingColorSpace, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format for a preallocated one dimensional array with the specified parameters.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.
        /// If the actual image data starts at some offset use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, CustomBitmapDataConfig)"/> overload instead.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. At least one getter or setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <remarks>
        /// <para>This method allows creating an <see cref="IReadWriteBitmapData"/> instance with custom pixel format. You need to specify a <see cref="PixelFormatInfo"/>
        /// and at least one delegate setter or getter delegate to be called whenever a pixel is get or set. In this overload these can be done by assigning a <see cref="CustomBitmapDataConfig"/>
        /// instance to the <paramref name="customBitmapDataConfig"/> parameter. The desired <see cref="PixelFormatInfo"/> should be set in the <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// property, whereas for the getter and setter methods you can select the ones with the color types that fit the best for your pixel format.</para>
        /// <note type="tip">It is enough to set only one getter and/or setter with the best matching color type. For example, if you set the <see cref="CustomBitmapDataConfig.RowGetColor64"/> property only,
        /// which returns the pixels as <see cref="Color64"/> values, then all of the other pixel-reading methods will use this delegate and will convert the result from <see cref="Color64"/>.</note>
        /// <para>A custom pixel format can have any <see cref="PixelFormatInfo.BitsPerPixel"/> value between 1 and 128. A typical bits-per-pixel value is a power of two; however,
        /// any other value can be used if you handle them in the provided delegates.</para>
        /// <para>The getter and setter delegates are always called with an <c>x</c> coordinate meaning the pixel offset in the corresponding row.</para>
        /// <note type="implement">It is highly recommended that the delegates do not use the <paramref name="buffer"/> directly (they don't capture the <paramref name="buffer"/> instance).
        /// Instead, they should access the actual data using their <see cref="ICustomBitmapDataRow"/> argument, which allows reading and writing raw data within the corresponding row, independently
        /// from any specific buffer instance. If they do so, then you can set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> to true in <paramref name="customBitmapDataConfig"/>,
        /// which allows some operations work with better quality and performance.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> or <paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfig.WorkingColorSpace"/> in <paramref name="customBitmapDataConfig"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <see cref="CustomBitmapDataConfigBase.PixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.
        /// <br/>-or-
        /// <be/>None of the pixel getter/setter delegates are specified in <paramref name="customBitmapDataConfig"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[] buffer, Size size, int stride, CustomBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
            => CreateBitmapData(buffer.AsSection(), size, stride, customBitmapDataConfig);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom indexed pixel format for a preallocated one dimensional array with the specified parameters.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.
        /// If the actual image data starts at some offset use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, PixelFormatInfo, Func{ICustomBitmapDataRow{T}, int, int}, Action{ICustomBitmapDataRow{T}, int, int}, Palette?, Func{Palette, bool}?, Action?)"/> overload instead.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColorIndex">A delegate that can get the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColorIndex">A delegate that can set the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="palette">Specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColorIndex"/> and <paramref name="rowSetColorIndex"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormatInfo"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormatInfo"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[] buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, int>? rowGetColorIndex, Action<ICustomBitmapDataRow<T>, int, int>? rowSetColorIndex,
            Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer.AsSection(), size, stride, pixelFormatInfo, rowGetColorIndex, rowSetColorIndex, palette, trySetPaletteCallback, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom indexed pixel format for a preallocated one dimensional array with the specified parameters.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.
        /// If the actual image data starts at some offset use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, PixelFormatInfo, Func{ICustomBitmapDataRow{T}, int, int}, Action{ICustomBitmapDataRow{T}, int, int}, Palette?, Func{Palette, bool}?, Action?)"/> overload instead.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. Either the getter or the setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <remarks>
        /// <para>This method allows creating an <see cref="IReadWriteBitmapData"/> instance with custom indexed pixel format. You need to specify a <see cref="PixelFormatInfo"/>
        /// and at one or both pixel accessor delegates to be called whenever a pixel is get or set. In this overload these can be done by assigning a <see cref="CustomIndexedBitmapDataConfig"/>
        /// instance to the <paramref name="customBitmapDataConfig"/> parameter. The desired indexed <see cref="PixelFormatInfo"/> should be set in the <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// property, whereas the getter and setter methods can be assigned to the <see cref="CustomIndexedBitmapDataConfig.RowGetColorIndex"/> and <see cref="CustomIndexedBitmapDataConfig.RowSetColorIndex"/> properties, respectively.</para>
        /// <para>An indexed custom pixel format can have any <see cref="PixelFormatInfo.BitsPerPixel"/> value between 1 and 16. A typical bits-per-pixel value is a power of two and is not greater than 8;
        /// however, any other value can be used if you handle them in the provided delegates.</para>
        /// <para>The getter and setter delegates are always called with an <c>x</c> coordinate meaning the pixel offset in the corresponding row.</para>
        /// <note type="implement">It is highly recommended that the delegates do not use the <paramref name="buffer"/> directly (they don't capture the <paramref name="buffer"/> instance).
        /// Instead, they should access the actual data using their <see cref="ICustomBitmapDataRow"/> argument, which allows reading and writing raw data within the corresponding row, independently
        /// from any specific buffer instance. If they do so, then you can set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> to true in <paramref name="customBitmapDataConfig"/>,
        /// which allows some operations work with better quality and performance.</note>
        /// <para>If the <see cref="CustomIndexedBitmapDataConfig.Palette"/> property in <paramref name="customBitmapDataConfig"/> is <see langword="null"/> , then the closest not larger system palette will be used,
        /// possibly completed with transparent entries. For example, if <see cref="PixelFormatInfo.BitsPerPixel">PixelFormatInfo.BitsPerPixel</see> is 9
        /// and <see cref="CustomIndexedBitmapDataConfig.Palette"/> is <see langword="null"/>, then a <see cref="Palette"/> with 512 colors will be created where the first 256 colors will be the same
        /// as in <see cref="Palette.SystemDefault8BppPalette(Color32, byte)">SystemDefault8BppPalette</see>.</para>
        /// <note>For that reason it is always recommended to set the <see cref="CustomIndexedBitmapDataConfig.Palette"/> property, especially if it has fewer entries than the possible allowed maximum
        /// because replacing the palette afterwards by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> extension method allows only to set a palette that has no
        /// fewer entries. It's because the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method assumes that the underlying buffer might already have pixels whose
        /// indices may turn invalid with a smaller palette.</note>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> or <paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <see cref="CustomBitmapDataConfigBase.PixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><see cref="CustomIndexedBitmapDataConfig.Palette"/> in <paramref name="customBitmapDataConfig"/> is too large for the specified <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.
        /// <br/>-or-
        /// <be/>Neither the getter nor the setter delegate is specified in <paramref name="customBitmapDataConfig"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[] buffer, Size size, int stride, CustomIndexedBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
            => CreateBitmapData(buffer.AsSection(), size, stride, customBitmapDataConfig);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format32bppArgb"/>.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <a href="https://docs.kgysoft.net/corelibraries/html/F_KGySoft_Collections_ArraySection_1_Null.htm">Null</a>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride,
            KnownPixelFormat pixelFormat = KnownPixelFormat.Format32bppArgb, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer, size, stride, pixelFormat, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <a href="https://docs.kgysoft.net/corelibraries/html/F_KGySoft_Collections_ArraySection_1_Null.htm">Null</a>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride,
            KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            int elementWidth = ValidateArguments(buffer, size, stride, pixelFormat, workingColorSpace);
            return CreateManagedBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, pixelFormat, backColor, alphaThreshold, workingColorSpace, null, null, disposeCallback);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> represents an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <a href="https://docs.kgysoft.net/corelibraries/html/F_KGySoft_Collections_ArraySection_1_Null.htm">Null</a>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, KnownPixelFormat pixelFormat,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            int elementWidth = ValidateArguments(buffer, size, stride, pixelFormat, WorkingColorSpace.Default, palette);
            return CreateManagedBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, pixelFormat, palette?.BackColor ?? default,
                palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default, palette, trySetPaletteCallback, disposeCallback);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <a href="https://docs.kgysoft.net/corelibraries/html/F_KGySoft_Collections_ArraySection_1_Null.htm">Null</a>
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormatInfo"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32>? rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer, size, stride, pixelFormatInfo, rowGetColor, rowSetColor, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <a href="https://docs.kgysoft.net/corelibraries/html/F_KGySoft_Collections_ArraySection_1_Null.htm">Null</a>
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>.
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormatInfo"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32>? rowSetColor,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            int elementWidth = ValidateArguments(buffer, size, stride, pixelFormatInfo, workingColorSpace);
            if (pixelFormatInfo.Indexed)
                throw new ArgumentException(Res.ImagingNonIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColor == null && rowSetColor == null)
                throw new ArgumentNullException(null, Res.ImagingNoPixelAccessSpecified);

            var cfg = new CustomBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                RowGetColorLegacy = rowGetColor,
                RowSetColorLegacy = rowSetColor,
                WorkingColorSpace = workingColorSpace,
                BackColor = backColor,
                AlphaThreshold = alphaThreshold,
                DisposeCallback = disposeCallback,
            };

            return CreateManagedCustomBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, cfg);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. At least one getter or setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <a href="https://docs.kgysoft.net/corelibraries/html/F_KGySoft_Collections_ArraySection_1_Null.htm">Null</a>
        /// <br/>-or-
        /// <br/><paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfig.WorkingColorSpace"/> in <paramref name="customBitmapDataConfig"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <see cref="CustomBitmapDataConfigBase.PixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.
        /// <br/>-or-
        /// <be/>None of the pixel getter/setter delegates are specified in <paramref name="customBitmapDataConfig"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, CustomBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            int elementWidth = ValidateArguments(buffer, size, stride, customBitmapDataConfig);
            return CreateManagedCustomBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, customBitmapDataConfig);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(ArraySection{T}, Size, int, CustomIndexedBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColorIndex">A delegate that can get the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColorIndex">A delegate that can set the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="palette">Specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <a href="https://docs.kgysoft.net/corelibraries/html/F_KGySoft_Collections_ArraySection_1_Null.htm">Null</a>
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColorIndex"/> and <paramref name="rowSetColorIndex"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormatInfo"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormatInfo"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, int>? rowGetColorIndex, Action<ICustomBitmapDataRow<T>, int, int>? rowSetColorIndex,
            Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            int elementWidth = ValidateArguments(buffer, size, stride, pixelFormatInfo, WorkingColorSpace.Default, palette);
            if (!pixelFormatInfo.Indexed)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColorIndex == null && rowSetColorIndex == null)
                throw new ArgumentNullException(null, Res.ImagingNoPixelAccessSpecified);

            var cfg = new CustomIndexedBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                RowGetColorIndexLegacy = rowGetColorIndex,
                RowSetColorIndexLegacy = rowSetColorIndex,
                Palette = palette,
                TrySetPaletteCallback = trySetPaletteCallback,
                DisposeCallback = disposeCallback,
            };

            return CreateManagedCustomBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, cfg);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_ArraySection_1.htm">ArraySection&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// It can be larger than it is required for the specified parameters.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. Either the getter or the setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <a href="https://docs.kgysoft.net/corelibraries/html/F_KGySoft_Collections_ArraySection_1_Null.htm">Null</a>
        /// <br/>-or-
        /// <br/><paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is too small for the specified width and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <see cref="CustomBitmapDataConfigBase.PixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/><paramref name="stride"/> is not a multiple of the size of <typeparamref name="T"/>
        /// <br/>-or-
        /// <br/><see cref="CustomIndexedBitmapDataConfig.Palette"/> in <paramref name="customBitmapDataConfig"/> is too large for the specified <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.
        /// <br/>-or-
        /// <be/>Neither the getter nor the setter delegate is specified in <paramref name="customBitmapDataConfig"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(ArraySection<T> buffer, Size size, int stride, CustomIndexedBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            int elementWidth = ValidateArguments(buffer, size, stride, customBitmapDataConfig);
            return CreateManagedCustomBitmapData(new Array2D<T>(buffer, size.Height, elementWidth), size.Width, customBitmapDataConfig);
        }

        #endregion

        #region Managed Wrapper for 2D Arrays

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance for a preallocated two-dimensional array with the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format32bppArgb"/>.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[,] buffer, int pixelWidth, KnownPixelFormat pixelFormat = KnownPixelFormat.Format32bppArgb,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer, pixelWidth, pixelFormat, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance for a preallocated two-dimensional array with the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[,] buffer, int pixelWidth, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Color32 backColor, byte alphaThreshold, Action? disposeCallback = null)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, pixelFormat, workingColorSpace);
            return CreateManagedBitmapData(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, workingColorSpace, null, null, disposeCallback);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance for a preallocated two-dimensional array with the specified parameters.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> represents an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns> 
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[,] buffer, int pixelWidth, KnownPixelFormat pixelFormat,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, pixelFormat, WorkingColorSpace.Default, palette);
            return CreateManagedBitmapData(buffer, pixelWidth, pixelFormat, palette?.BackColor ?? default,
                palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default, palette, trySetPaletteCallback, disposeCallback);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format for a preallocated two-dimensional array with the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(T[,], int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(T[,], int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(T[,], int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormatInfo"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[,] buffer, int pixelWidth, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32>? rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer, pixelWidth, pixelFormatInfo, rowGetColor, rowSetColor, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format for a preallocated two-dimensional array with the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(T[,], int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(T[,], int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(T[,], int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormatInfo"/>.
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[,] buffer, int pixelWidth, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32>? rowSetColor,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, pixelFormatInfo, workingColorSpace);
            if (pixelFormatInfo.Indexed)
                throw new ArgumentException(Res.ImagingNonIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColor == null && rowSetColor == null)
                throw new ArgumentNullException(null, Res.ImagingNoPixelAccessSpecified);

            var cfg = new CustomBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                RowGetColorLegacy = rowGetColor,
                RowSetColorLegacy = rowSetColor,
                WorkingColorSpace = workingColorSpace,
                BackColor = backColor,
                AlphaThreshold = alphaThreshold,
                DisposeCallback = disposeCallback,
            };

            return CreateManagedCustomBitmapData(buffer, pixelWidth, cfg);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format for a preallocated two-dimensional array with the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. At least one getter or setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> or <paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfig.WorkingColorSpace"/> in <paramref name="customBitmapDataConfig"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.
        /// <br/>-or-
        /// <be/>None of the pixel getter/setter delegates are specified in <paramref name="customBitmapDataConfig"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[,] buffer, int pixelWidth, CustomBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, customBitmapDataConfig);
            return CreateManagedCustomBitmapData(buffer, pixelWidth, customBitmapDataConfig);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom indexed pixel format for a preallocated two-dimensional array with the specified parameters.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(T[,], int, CustomIndexedBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColorIndex">A delegate that can get the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColorIndex">A delegate that can set the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="palette">Specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColorIndex"/> and <paramref name="rowSetColorIndex"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormatInfo"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormatInfo"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[,] buffer, int pixelWidth, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, int>? rowGetColorIndex, Action<ICustomBitmapDataRow<T>, int, int>? rowSetColorIndex,
            Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, pixelFormatInfo, WorkingColorSpace.Default, palette);
            if (!pixelFormatInfo.Indexed)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColorIndex == null && rowSetColorIndex == null)
                throw new ArgumentNullException(null, Res.ImagingNoPixelAccessSpecified);

            var cfg = new CustomIndexedBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                RowGetColorIndexLegacy = rowGetColorIndex,
                RowSetColorIndexLegacy = rowSetColorIndex,
                Palette = palette,
                TrySetPaletteCallback = trySetPaletteCallback,
                DisposeCallback = disposeCallback,
            };

            return CreateManagedCustomBitmapData(buffer, pixelWidth, cfg);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom indexed pixel format for a preallocated two-dimensional array with the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">A preallocated array to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. Either the getter or the setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> or <paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><see cref="CustomIndexedBitmapDataConfig.Palette"/> in <paramref name="customBitmapDataConfig"/> is too large for the specified <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.
        /// <br/>-or-
        /// <be/>Neither the getter nor the setter delegate is specified in <paramref name="customBitmapDataConfig"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(T[,] buffer, int pixelWidth, CustomIndexedBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, customBitmapDataConfig);
            return CreateManagedCustomBitmapData(buffer, pixelWidth, customBitmapDataConfig);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create. This parameter is optional.
        /// <br/>Default value: <see cref="KnownPixelFormat.Format32bppArgb"/>.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException">The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Collections_Array2D_1_IsNull.htm">IsNull</a> property of <paramref name="buffer"/> is <see langword="true"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, KnownPixelFormat pixelFormat = KnownPixelFormat.Format32bppArgb,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer, pixelWidth, pixelFormat, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException">The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Collections_Array2D_1_IsNull.htm">IsNull</a> property of <paramref name="buffer"/> is <see langword="true"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Color32 backColor, byte alphaThreshold, Action? disposeCallback = null)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, pixelFormat, workingColorSpace);
            return CreateManagedBitmapData(buffer, pixelWidth, pixelFormat, backColor, alphaThreshold, workingColorSpace, null, null, disposeCallback);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> represents an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns> 
        /// <exception cref="ArgumentNullException">The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Collections_Array2D_1_IsNull.htm">IsNull</a> property of <paramref name="buffer"/> is <see langword="true"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormat"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, KnownPixelFormat pixelFormat,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, pixelFormat, WorkingColorSpace.Default, palette);
            return CreateManagedBitmapData(buffer, pixelWidth, pixelFormat, palette?.BackColor ?? default,
                palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default, palette, trySetPaletteCallback, disposeCallback);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(Array2D{T}, int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(Array2D{T}, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(Array2D{T}, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException">The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Collections_Array2D_1_IsNull.htm">IsNull</a> property of <paramref name="buffer"/> is <see langword="true"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormatInfo"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32>? rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
            => CreateBitmapData(buffer, pixelWidth, pixelFormatInfo, rowGetColor, rowSetColor, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(Array2D{T}, int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(Array2D{T}, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData{T}(Array2D{T}, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException">The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Collections_Array2D_1_IsNull.htm">IsNull</a> property of <paramref name="buffer"/> is <see langword="true"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormatInfo"/>.
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow<T>, int, Color32>? rowSetColor,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, pixelFormatInfo, workingColorSpace);
            if (pixelFormatInfo.Indexed)
                throw new ArgumentException(Res.ImagingNonIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColor == null && rowSetColor == null)
                throw new ArgumentNullException(null, Res.ImagingNoPixelAccessSpecified);

            var cfg = new CustomBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                RowGetColorLegacy = rowGetColor,
                RowSetColorLegacy = rowSetColor,
                WorkingColorSpace = workingColorSpace,
                BackColor = backColor,
                AlphaThreshold = alphaThreshold,
                DisposeCallback = disposeCallback,
            };

            return CreateManagedCustomBitmapData(buffer, pixelWidth, cfg);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. At least one getter or setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException">The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Collections_Array2D_1_IsNull.htm">IsNull</a> property of <paramref name="buffer"/> is <see langword="true"/>.
        /// <br/>-or-
        /// <br/><paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfig.WorkingColorSpace"/> in <paramref name="customBitmapDataConfig"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.
        /// <br/>-or-
        /// <be/>None of the pixel getter/setter delegates are specified in <paramref name="customBitmapDataConfig"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, CustomBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, customBitmapDataConfig);
            return CreateManagedCustomBitmapData(buffer, pixelWidth, customBitmapDataConfig);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData{T}(Array2D{T}, int, CustomIndexedBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColorIndex">A delegate that can get the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColorIndex">A delegate that can set the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="palette">Specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException">The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Collections_Array2D_1_IsNull.htm">IsNull</a> property of <paramref name="buffer"/> is <see langword="true"/>.
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColorIndex"/> and <paramref name="rowSetColorIndex"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <paramref name="pixelFormatInfo"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormatInfo"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow<T>, int, int>? rowGetColorIndex, Action<ICustomBitmapDataRow<T>, int, int>? rowSetColorIndex,
            Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, pixelFormatInfo, WorkingColorSpace.Default, palette);
            if (!pixelFormatInfo.Indexed)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColorIndex == null && rowSetColorIndex == null)
                throw new ArgumentNullException(null, Res.ImagingNoPixelAccessSpecified);

            var cfg = new CustomIndexedBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                RowGetColorIndexLegacy = rowGetColorIndex,
                RowSetColorIndexLegacy = rowSetColorIndex,
                Palette = palette,
                TrySetPaletteCallback = trySetPaletteCallback,
                DisposeCallback = disposeCallback,
            };
            
            return CreateManagedCustomBitmapData(buffer, pixelWidth, cfg);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom indexed pixel format wrapping the specified <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="buffer"/>.</typeparam>
        /// <param name="buffer">An <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Collections_Array2D_1.htm">Array2D&lt;T></a> to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.</param>
        /// <param name="pixelWidth">The width of the bitmap data to create in pixels.</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. Either the getter or the setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException">The <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Collections_Array2D_1_IsNull.htm">IsNull</a> property of <paramref name="buffer"/> is <see langword="true"/>.
        /// <br/>-or-
        /// <paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="pixelWidth"/> is too large for the specified <paramref name="buffer"/> and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is empty
        /// <br/>-or-
        /// <br/><see cref="CustomIndexedBitmapDataConfig.Palette"/> in <paramref name="customBitmapDataConfig"/> is too large for the specified <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.
        /// <br/>-or-
        /// <be/>Neither the getter nor the setter delegate is specified in <paramref name="customBitmapDataConfig"/>.</exception>
        public static IReadWriteBitmapData CreateBitmapData<T>(Array2D<T> buffer, int pixelWidth, CustomIndexedBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            ValidateArguments(buffer, pixelWidth, customBitmapDataConfig);
            return CreateManagedCustomBitmapData(buffer, pixelWidth, customBitmapDataConfig);
        }

        #endregion

        #region Unmanaged Wrapper for IntPtr

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping an unmanaged <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <param name="buffer">The memory address to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// Make sure there is enough allocated memory for the specified <paramref name="size"/>, <paramref name="stride"/> and <paramref name="pixelFormat"/>;
        /// otherwise, accessing pixels may corrupt memory or throw an <see cref="AccessViolationException"/>.
        /// If it points to managed memory make sure it is pinned until the returned bitmap data is disposed.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.
        /// It can be negative for bottom-up layout (ie. when <paramref name="buffer"/> points to the first pixel of the bottom row).</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see cref="IntPtr.Zero">IntPtr.Zero</see>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>.</exception>
        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, KnownPixelFormat pixelFormat,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            => CreateBitmapData(buffer, size, stride, pixelFormat, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping an unmanaged <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// </summary>
        /// <param name="buffer">The memory address to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// Make sure there is enough allocated memory for the specified <paramref name="size"/>, <paramref name="stride"/> and <paramref name="pixelFormat"/>;
        /// otherwise, accessing pixels may corrupt memory or throw an <see cref="AccessViolationException"/>.
        /// If it points to managed memory make sure it is pinned until the returned bitmap data is disposed.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.
        /// It can be negative for bottom-up layout (ie. when <paramref name="buffer"/> points to the first pixel of the bottom row).</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see cref="IntPtr.Zero">IntPtr.Zero</see>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            ValidateArguments(buffer, size, stride, pixelFormat, workingColorSpace);
            return CreateUnmanagedBitmapData(buffer, size, stride, pixelFormat,
                backColor, alphaThreshold, workingColorSpace, null, null, disposeCallback);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance wrapping an unmanaged <paramref name="buffer"/> and using the specified parameters.
        /// </summary>
        /// <param name="buffer">The memory address to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// Make sure there is enough allocated memory for the specified <paramref name="size"/>, <paramref name="stride"/> and <paramref name="pixelFormat"/>;
        /// otherwise, accessing pixels may corrupt memory or throw an <see cref="AccessViolationException"/>.
        /// If it points to managed memory make sure it is pinned until the returned bitmap data is disposed.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.
        /// It can be negative for bottom-up layout (ie. when <paramref name="buffer"/> points to the first pixel of the bottom row).</param>
        /// <param name="pixelFormat">The pixel format in <paramref name="buffer"/> and the bitmap data to create.</param>
        /// <param name="palette">If <paramref name="pixelFormat"/> represents an indexed format, then specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see cref="IntPtr.Zero">IntPtr.Zero</see>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/><paramref name="pixelFormat"/> is not one of the valid formats
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> is too small for the specified <paramref name="size"/>, <paramref name="pixelFormat"/> and <paramref name="stride"/>
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="palette"/> is too large for the specified <paramref name="pixelFormat"/>.</exception>
        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, KnownPixelFormat pixelFormat, Palette? palette,
            Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
        {
            ValidateArguments(buffer, size, stride, pixelFormat, WorkingColorSpace.Default, palette);
            return CreateUnmanagedBitmapData(buffer, size, stride, pixelFormat,
                palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default,
                palette, trySetPaletteCallback, disposeCallback);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping an unmanaged <paramref name="buffer"/> and using the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData(IntPtr, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <param name="buffer">The memory address to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// Make sure there is enough allocated memory for the specified <paramref name="size"/>, <paramref name="stride"/> and <paramref name="pixelFormatInfo"/>;
        /// otherwise, accessing pixels may corrupt memory or throw an <see cref="AccessViolationException"/>.
        /// If it points to managed memory make sure it is pinned until the returned bitmap data is disposed.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.
        /// It can be negative for bottom-up layout (ie. when <paramref name="buffer"/> points to the first pixel of the bottom row).</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData(IntPtr, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData(IntPtr, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see cref="IntPtr.Zero">IntPtr.Zero</see>
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow, int, Color32>? rowSetColor,
            Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
            => CreateBitmapData(buffer, size, stride, pixelFormatInfo, rowGetColor, rowSetColor, WorkingColorSpace.Default, backColor, alphaThreshold, disposeCallback);

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping an unmanaged <paramref name="buffer"/> and using the specified parameters.
        /// By this overload you can specify a pair of custom getter/setter delegates using the <see cref="Color32"/> color type.
        /// If other color types fit better for the custom format or you can ensure that the delegates don't capture <paramref name="buffer"/> use the <see cref="CreateBitmapData(IntPtr, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <param name="buffer">The memory address to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// Make sure there is enough allocated memory for the specified <paramref name="size"/>, <paramref name="stride"/> and <paramref name="pixelFormatInfo"/>;
        /// otherwise, accessing pixels may corrupt memory or throw an <see cref="AccessViolationException"/>.
        /// If it points to managed memory make sure it is pinned until the returned bitmap data is disposed.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.
        /// It can be negative for bottom-up layout (ie. when <paramref name="buffer"/> points to the first pixel of the bottom row).</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColor">A delegate that can get the 32-bit color of a pixel in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData(IntPtr, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColor">A delegate that can set the color of a pixel from a <see cref="Color32"/> value in a row of the bitmap data.
        /// If <paramref name="pixelFormatInfo"/> represents a wider format it is recommended to use the <see cref="CreateBitmapData(IntPtr, Size, int, CustomBitmapDataConfig)"/> overload instead.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <param name="backColor">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.BackColor"/> value of the returned <see cref="IReadWriteBitmapData"/> instance. It does not affect the actual returned bitmap content.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details.
        /// The alpha value (<see cref="Color32.A">Color32.A</see> field) of the specified background color is ignored. This parameter is optional.
        /// <br/>Default value: The default value of the <see cref="Color32"/> type, which has the same RGB values as <see cref="Color.Black"/>.</param>
        /// <param name="alphaThreshold">For pixel formats without alpha gradient support specifies the <see cref="IBitmapData.AlphaThreshold"/> value of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/> overload for details. This parameter is optional.
        /// <br/>Default value: <c>128</c>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see cref="IntPtr.Zero">IntPtr.Zero</see>
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColor"/> and <paramref name="rowSetColor"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>
        /// <br/>-or-
        /// <br/><paramref name="workingColorSpace"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><paramref name="pixelFormatInfo"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.</exception>
        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow, int, Color32>? rowGetColor, Action<ICustomBitmapDataRow, int, Color32>? rowSetColor,
            WorkingColorSpace workingColorSpace, Color32 backColor = default, byte alphaThreshold = 128, Action? disposeCallback = null)
        {
            ValidateArguments(buffer, size, stride, pixelFormatInfo, workingColorSpace);
            if (pixelFormatInfo.Indexed)
                throw new ArgumentException(Res.ImagingNonIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColor == null && rowSetColor == null)
                throw new ArgumentNullException(null, Res.ImagingNoPixelAccessSpecified);

            var cfg = new CustomBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                RowGetColor32 = rowGetColor,
                RowSetColor32 = rowSetColor,
                WorkingColorSpace = workingColorSpace,
                BackColor = backColor,
                AlphaThreshold = alphaThreshold,
                DisposeCallback = disposeCallback,
            };

            return CreateUnmanagedCustomBitmapData(buffer, size, stride, cfg);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping an unmanaged <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <param name="buffer">The memory address to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// Make sure there is enough allocated memory for the specified <paramref name="size"/>, <paramref name="stride"/> and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>;
        /// otherwise, accessing pixels may corrupt memory or throw an <see cref="AccessViolationException"/>.
        /// If it points to managed memory make sure it is pinned until the returned bitmap data is disposed.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.
        /// It can be negative for bottom-up layout (ie. when <paramref name="buffer"/> points to the first pixel of the bottom row).</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. At least one getter or setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> or its owner object to make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see cref="IntPtr.Zero">IntPtr.Zero</see>
        /// <br/>-or-
        /// <br/><paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfig.WorkingColorSpace"/> in <paramref name="customBitmapDataConfig"/> is not one of the defined values.</exception>
        /// <exception cref="ArgumentException"><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is 0.
        /// <br/>-or-
        /// <be/>None of the pixel getter/setter delegates are specified in <paramref name="customBitmapDataConfig"/>.</exception>
        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, CustomBitmapDataConfig customBitmapDataConfig)
        {
            ValidateArguments(buffer, size, stride, customBitmapDataConfig);
            return CreateUnmanagedCustomBitmapData(buffer, size, stride, customBitmapDataConfig);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping an unmanaged <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, PixelFormatInfo, Func{ICustomBitmapDataRow{T}, int, int}, Action{ICustomBitmapDataRow{T}, int, int}, Palette?, Func{Palette, bool}?, Action?)"/> overload for details.
        /// </summary>
        /// <param name="buffer">The memory address to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// Make sure there is enough allocated memory for the specified <paramref name="size"/>, <paramref name="stride"/> and <paramref name="pixelFormatInfo"/>;
        /// otherwise, accessing pixels may corrupt memory or throw an <see cref="AccessViolationException"/>.
        /// If it points to managed memory make sure it is pinned until the returned bitmap data is disposed.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.
        /// It can be negative for bottom-up layout (ie. when <paramref name="buffer"/> points to the first pixel of the bottom row).</param>
        /// <param name="pixelFormatInfo">A <see cref="PixelFormatInfo"/> instance that describes the pixel format.</param>
        /// <param name="rowGetColorIndex">A delegate that can get the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be write-only (can be cast to <see cref="IWritableBitmapData"/>).</param>
        /// <param name="rowSetColorIndex">A delegate that can set the color index of a pixel in a row of the bitmap data.
        /// If <see langword="null"/>, then the returned instance will be read-only (can be cast to <see cref="IReadableBitmapData"/>).</param>
        /// <param name="palette">Specifies the desired <see cref="IBitmapData.Palette"/> of the returned <see cref="IReadWriteBitmapData"/> instance.
        /// It determines also the <see cref="IBitmapData.BackColor"/> and <see cref="IBitmapData.AlphaThreshold"/> properties of the result. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="trySetPaletteCallback">A delegate to be called when the palette is attempted to be replaced by the <see cref="BitmapDataExtensions.TrySetPalette">TrySetPalette</see> method.
        /// If <paramref name="buffer"/> belongs to some custom bitmap implementation, it can be used to update its original palette. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <param name="disposeCallback">A delegate to be called when the returned <see cref="IReadWriteBitmapData"/> is disposed or finalized. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see cref="IntPtr.Zero">IntPtr.Zero</see>
        /// <br/>-or-
        /// <br/>Both <paramref name="rowGetColorIndex"/> and <paramref name="rowSetColorIndex"/> are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <paramref name="pixelFormatInfo"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="palette"/> is too large for the specified <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// <br/>-or-
        /// <br/><paramref name="pixelFormatInfo"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.</exception>
        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, PixelFormatInfo pixelFormatInfo,
            Func<ICustomBitmapDataRow, int, int>? rowGetColorIndex, Action<ICustomBitmapDataRow, int, int>? rowSetColorIndex,
            Palette? palette = null, Func<Palette, bool>? trySetPaletteCallback = null, Action? disposeCallback = null)
        {
            ValidateArguments(buffer, size, stride, pixelFormatInfo, WorkingColorSpace.Default, palette);
            if (!pixelFormatInfo.Indexed)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatExpected, nameof(pixelFormatInfo));
            if (rowGetColorIndex == null && rowSetColorIndex == null)
                throw new ArgumentNullException(null, Res.ImagingNoPixelAccessSpecified);

            var cfg = new CustomIndexedBitmapDataConfig
            {
                PixelFormat = pixelFormatInfo,
                RowGetColorIndex = rowGetColorIndex,
                RowSetColorIndex = rowSetColorIndex,
                Palette = palette,
                TrySetPaletteCallback = trySetPaletteCallback,
                DisposeCallback = disposeCallback,
            };

            return CreateUnmanagedCustomBitmapData(buffer, size, stride, cfg);
        }

        /// <summary>
        /// Creates an <see cref="IReadWriteBitmapData"/> instance with a custom non-indexed pixel format wrapping an unmanaged <paramref name="buffer"/> and using the specified parameters.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="CreateBitmapData{T}(T[], Size, int, CustomIndexedBitmapDataConfig)"/> overload for details.
        /// </summary>
        /// <param name="buffer">The memory address to be used as the underlying buffer for the returned <see cref="IReadWriteBitmapData"/>.
        /// Make sure there is enough allocated memory for the specified <paramref name="size"/>, <paramref name="stride"/> and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>;
        /// otherwise, accessing pixels may corrupt memory or throw an <see cref="AccessViolationException"/>.
        /// If it points to managed memory make sure it is pinned until the returned bitmap data is disposed.</param>
        /// <param name="size">The size of the bitmap data to create in pixels.</param>
        /// <param name="stride">The size of a row in bytes. It allows to have some padding at the end of each row.
        /// It can be negative for bottom-up layout (ie. when <paramref name="buffer"/> points to the first pixel of the bottom row).</param>
        /// <param name="customBitmapDataConfig">The configuration for the custom pixel format. Either the getter or the setter delegate must be specified.
        /// If you can ensure that the delegates don't capture <paramref name="buffer"/> or its owner object to make sure you set the <see cref="CustomBitmapDataConfigBase.BackBufferIndependentPixelAccess"/> property to <see langword="true"/>.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance wrapping the specified <paramref name="buffer"/> and using the provided parameters.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see cref="IntPtr.Zero">IntPtr.Zero</see>
        /// <br/>-or-
        /// <br/><paramref name="customBitmapDataConfig"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> has a zero or negative width or height
        /// <br/>-or-
        /// <br/>The absolute value of <paramref name="stride"/> is too small for the specified width and <see cref="CustomBitmapDataConfigBase.PixelFormat"/>.</exception>
        /// <exception cref="ArgumentException"><see cref="CustomIndexedBitmapDataConfig.Palette"/> in <paramref name="customBitmapDataConfig"/> is too large for the specified <see cref="CustomBitmapDataConfigBase.PixelFormat"/>
        /// <br/>-or-
        /// <br/><see cref="CustomBitmapDataConfigBase.PixelFormat"/> in <paramref name="customBitmapDataConfig"/> is not indexed or its <see cref="PixelFormatInfo.BitsPerPixel"/> is not between 1 and 16.
        /// <br/>-or-
        /// <be/>Neither the getter nor the setter delegate is specified in <paramref name="customBitmapDataConfig"/>.</exception>
        [SecurityCritical]
        public static IReadWriteBitmapData CreateBitmapData(IntPtr buffer, Size size, int stride, CustomIndexedBitmapDataConfig customBitmapDataConfig)
        {
            ValidateArguments(buffer, size, stride, customBitmapDataConfig);
            return CreateUnmanagedCustomBitmapData(buffer, size, stride, customBitmapDataConfig);
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
            return DoLoadBitmapData(AsyncHelper.DefaultContext, stream)!;
        }

        /// <summary>
        /// Begins to load a managed <see cref="IReadWriteBitmapData"/> instance from the specified <paramref name="stream"/> asynchronously that was saved by
        /// the <see cref="BitmapDataExtensions.Save">BitmapDataExtensions.Save</see> method.
        /// </summary>
        /// <param name="stream">The stream to load the bitmap data from.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>An <see cref="IAsyncResult"/> that represents the asynchronous operation, which could still be pending.</returns>
        /// <remarks>
        /// <para>In .NET Framework 4.0 and above you can use also the <see cref="LoadAsync">LoadAsync</see> method.</para>
        /// <para>To finish the operation and to get the exception that occurred during the operation you have to call the <see cref="EndLoad">EndLoad</see> method.</para>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// </remarks>
        public static IAsyncResult BeginLoad(Stream stream, AsyncConfig? asyncConfig = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return AsyncHelper.BeginOperation(ctx => DoLoadBitmapData(ctx, stream), asyncConfig);
        }

        /// <summary>
        /// Waits for the pending asynchronous operation started by the <see cref="BeginLoad">BeginLoad</see> method to complete.
        /// In .NET Framework 4.0 and above you can use the <see cref="LoadAsync">LoadAsync</see> method instead.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance that is the result of the operation,
        /// or <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <c>asyncConfig</c> parameter was <see langword="false"/>.</returns>
        public static IReadWriteBitmapData? EndLoad(IAsyncResult asyncResult)
            => AsyncHelper.EndOperation<IReadWriteBitmapData?>(asyncResult, nameof(BeginLoad));

#if !NET35
        /// <summary>
        /// Loads a managed <see cref="IReadWriteBitmapData"/> instance from the specified <paramref name="stream"/> asynchronously that was saved by
        /// the <see cref="BitmapDataExtensions.Save">BitmapDataExtensions.Save</see> method.
        /// </summary>
        /// <param name="stream">The stream to load the bitmap data from.</param>
        /// <param name="asyncConfig">The configuration of the asynchronous operation such as cancellation, reporting progress, etc.
        /// When <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_Progress.htm">Progress</a> is set in this parameter,
        /// then this library always passes a <see cref="DrawingOperation"/> instance to the generic methods of
        /// the <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Threading_IAsyncProgress.htm">IAsyncProgress</a> interface. This parameter is optional.
        /// <br/>Default value: <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation, which could still be pending.
        /// its result can be <see langword="null"/>, if the operation was canceled and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_ThrowIfCanceled.htm">ThrowIfCanceled</a> property of the <paramref name="asyncConfig"/> parameter was <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>This method is not a blocking call, though the operation is not parallelized and the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_Threading_AsyncConfigBase_MaxDegreeOfParallelism.htm">MaxDegreeOfParallelism</a> property of the <paramref name="asyncConfig"/> parameter is ignored.</para>
        /// </remarks>
        public static Task<IReadWriteBitmapData?> LoadAsync(Stream stream, TaskConfig? asyncConfig = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            return AsyncHelper.DoOperationAsync(ctx => DoLoadBitmapData(ctx, stream), asyncConfig);
        }
#endif

        #endregion

        #endregion

        #region Internal Methods

        #region Managed

        internal static IBitmapDataInternal CreateManagedBitmapData(Size size, KnownPixelFormat pixelFormat,
            Color32 backColor, byte alphaThreshold, WorkingColorSpace workingColorSpace, Palette? palette)
        {
            #region Local Methods

#if NETFRAMEWORK || NETSTANDARD2_0
            static bool IsForcedByteArrayBuffer(Size size, KnownPixelFormat format) => false;
#else
            static bool IsForcedByteArrayBuffer(Size size, KnownPixelFormat format) =>
                poolingStrategy == ArrayPoolingStrategy.IfCanUseByteArray && !format.IsIndexed()
                    && (long)size.Height * format.GetByteWidth(size.Width) <= EnvironmentHelper.MaxByteArrayLength;
#endif

            #endregion

            Debug.Assert(palette == null || backColor.ToOpaque() == palette.BackColor && alphaThreshold == palette.AlphaThreshold);
            var cfg = new BitmapDataConfig(size, pixelFormat.ToInfoInternal(), backColor, alphaThreshold, workingColorSpace, palette);
            return pixelFormat switch
            {
                KnownPixelFormat.Format32bppArgb => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData32Argb<byte>(cfg)
                    : new ManagedBitmapData32Argb(cfg),
                KnownPixelFormat.Format32bppPArgb => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData32PArgb<byte>(cfg)
                    : new ManagedBitmapData32PArgb(cfg),
                KnownPixelFormat.Format32bppRgb => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData32Rgb<byte>(cfg)
                    : new ManagedBitmapData32Rgb(cfg),
                KnownPixelFormat.Format24bppRgb => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData24Rgb<byte>(cfg)
                    : new ManagedBitmapData24Rgb(cfg),
                KnownPixelFormat.Format8bppIndexed => new ManagedBitmapData8I(cfg),
                KnownPixelFormat.Format4bppIndexed => new ManagedBitmapData4I(cfg),
                KnownPixelFormat.Format1bppIndexed => new ManagedBitmapData1I(cfg),
                KnownPixelFormat.Format64bppArgb => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData64Argb<byte>(cfg)
                    : new ManagedBitmapData64Argb(cfg),
                KnownPixelFormat.Format64bppPArgb => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData64PArgb<byte>(cfg)
                    : new ManagedBitmapData64PArgb(cfg),
                KnownPixelFormat.Format48bppRgb => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData48Rgb<byte>(cfg)
                    : new ManagedBitmapData48Rgb(cfg),
                KnownPixelFormat.Format16bppRgb565 => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData16Rgb565<byte>(cfg)
                    : new ManagedBitmapData16Rgb565(cfg),
                KnownPixelFormat.Format16bppRgb555 => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData16Rgb555<byte>(cfg)
                    : new ManagedBitmapData16Rgb555(cfg),
                KnownPixelFormat.Format16bppArgb1555 => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData16Argb1555<byte>(cfg)
                    : new ManagedBitmapData16Argb1555(cfg),
                KnownPixelFormat.Format16bppGrayScale => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData16Gray<byte>(cfg)
                    : new ManagedBitmapData16Gray(cfg),
                KnownPixelFormat.Format128bppRgba => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData128Rgba<byte>(cfg)
                    : new ManagedBitmapData128Rgba(cfg),
                KnownPixelFormat.Format128bppPRgba => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData128PRgba<byte>(cfg)
                    : new ManagedBitmapData128PRgba(cfg),
                KnownPixelFormat.Format96bppRgb => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData96Rgb<byte>(cfg)
                    : new ManagedBitmapData96Rgb(cfg),
                KnownPixelFormat.Format8bppGrayScale => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData8Gray<byte>(cfg)
                    : new ManagedBitmapData8Gray(cfg),
                KnownPixelFormat.Format32bppGrayScale => IsForcedByteArrayBuffer(size, pixelFormat)
                    ? new ManagedBitmapData32Gray<byte>(cfg)
                    : new ManagedBitmapData32Gray(cfg),
                _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat))
            };
        }

        /// <summary>
        /// Creates a managed <see cref="IBitmapDataInternal"/> for a preallocated 1D array (wrapped into an <see cref="Array2D{T}"/> struct).
        /// </summary>
        internal static IBitmapDataInternal CreateManagedBitmapData<T>(Array2D<T> buffer, int pixelWidth, KnownPixelFormat pixelFormat,
            Color32 backColor, byte alphaThreshold, WorkingColorSpace workingColorSpace,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            where T : unmanaged
        {
            Debug.Assert(palette == null || backColor.ToOpaque() == palette.BackColor && alphaThreshold == palette.AlphaThreshold);
            var cfg = new BitmapDataConfig(new Size(pixelWidth, buffer.Height), pixelFormat.ToInfoInternal(),
                backColor, alphaThreshold, workingColorSpace, palette, trySetPaletteCallback, disposeCallback);
            return pixelFormat switch
            {
                KnownPixelFormat.Format32bppArgb => buffer is Array2D<Color32> buf
                    ? new ManagedBitmapData32Argb(buf, cfg)
                    : new ManagedBitmapData32Argb<T>(buffer.Cast<T, Color32>(), cfg),
                KnownPixelFormat.Format32bppPArgb => buffer is Array2D<PColor32> buf
                    ? new ManagedBitmapData32PArgb(buf, cfg)
                    : new ManagedBitmapData32PArgb<T>(buffer.Cast<T, PColor32>(), cfg),
                KnownPixelFormat.Format32bppRgb => buffer is Array2D<Color32> buf
                    ? new ManagedBitmapData32Rgb(buf, cfg)
                    : new ManagedBitmapData32Rgb<T>(buffer.Cast<T, Color32>(), cfg),
                KnownPixelFormat.Format24bppRgb => new ManagedBitmapData24Rgb<T>(buffer.Cast<T, Color24>(), cfg),
                KnownPixelFormat.Format8bppIndexed => buffer is Array2D<byte> buf
                    ? new ManagedBitmapData8I(buf, cfg)
                    : new ManagedBitmapData8I<T>(buffer.Cast<T, byte>(), cfg),
                KnownPixelFormat.Format4bppIndexed => buffer is Array2D<byte> buf
                    ? new ManagedBitmapData4I(buf, cfg)
                    : new ManagedBitmapData4I<T>(buffer.Cast<T, byte>(), cfg),
                KnownPixelFormat.Format1bppIndexed => buffer is Array2D<byte> buf
                    ? new ManagedBitmapData1I(buf, cfg)
                    : new ManagedBitmapData1I<T>(buffer.Cast<T, byte>(), cfg),
                KnownPixelFormat.Format64bppArgb => buffer is Array2D<Color64> buf
                    ? new ManagedBitmapData64Argb(buf, cfg)
                    : new ManagedBitmapData64Argb<T>(buffer.Cast<T, Color64>(), cfg),
                KnownPixelFormat.Format64bppPArgb => buffer is Array2D<PColor64> buf
                    ? new ManagedBitmapData64PArgb(buf, cfg)
                    : new ManagedBitmapData64PArgb<T>(buffer.Cast<T, PColor64>(), cfg),
                KnownPixelFormat.Format48bppRgb => new ManagedBitmapData48Rgb<T>(buffer.Cast<T, Color48>(), cfg),
                KnownPixelFormat.Format16bppRgb565 => new ManagedBitmapData16Rgb565<T>(buffer.Cast<T, Color16Rgb565>(), cfg),
                KnownPixelFormat.Format16bppRgb555 => new ManagedBitmapData16Rgb555<T>(buffer.Cast<T, Color16Rgb555>(), cfg),
                KnownPixelFormat.Format16bppArgb1555 => new ManagedBitmapData16Argb1555<T>(buffer.Cast<T, Color16Argb1555>(), cfg),
                KnownPixelFormat.Format16bppGrayScale => new ManagedBitmapData16Gray<T>(buffer.Cast<T, Gray16>(), cfg),
                KnownPixelFormat.Format128bppRgba => buffer is Array2D<ColorF> buf
                    ? new ManagedBitmapData128Rgba(buf, cfg)
                    : new ManagedBitmapData128Rgba<T>(buffer.Cast<T, ColorF>(), cfg),
                KnownPixelFormat.Format128bppPRgba => buffer is Array2D<PColorF> buf
                    ? new ManagedBitmapData128PRgba(buf, cfg)
                    : new ManagedBitmapData128PRgba<T>(buffer.Cast<T, PColorF>(), cfg),
                KnownPixelFormat.Format96bppRgb => new ManagedBitmapData96Rgb<T>(buffer.Cast<T, RgbF>(), cfg),
                KnownPixelFormat.Format8bppGrayScale => new ManagedBitmapData8Gray<T>(buffer.Cast<T, Gray8>(), cfg),
                KnownPixelFormat.Format32bppGrayScale => new ManagedBitmapData32Gray<T>(buffer.Cast<T, GrayF>(), cfg),
                _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat))
            };
        }

        internal static IBitmapDataInternal CreateManagedCustomBitmapData<T>(Array2D<T> buffer, int pixelWidth, CustomBitmapDataConfig customConfig)
            where T : unmanaged
        {
            var commonConfig = new BitmapDataConfig(new Size(pixelWidth, buffer.Height), customConfig.PixelFormat,
                customConfig.BackColor, customConfig.AlphaThreshold, customConfig.WorkingColorSpace, 
                null, null, customConfig.DisposeCallback);
            return new ManagedCustomBitmapData<T>(buffer, commonConfig, customConfig);
        }

        internal static IBitmapDataInternal CreateManagedCustomBitmapData<T>(Array2D<T> buffer, int pixelWidth, CustomIndexedBitmapDataConfig customConfig)
            where T : unmanaged
        {
            Palette? palette = customConfig.Palette;
            var commonConfig = new BitmapDataConfig(new Size(pixelWidth, buffer.Height), customConfig.PixelFormat,
                palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default,
                palette, customConfig.TrySetPaletteCallback, customConfig.DisposeCallback);
            return new ManagedCustomBitmapDataIndexed<T>(buffer, commonConfig, customConfig);
        }

        /// <summary>
        /// Creates a managed <see cref="IBitmapDataInternal"/> for a preallocated 2D array.
        /// </summary>
        internal static IBitmapDataInternal CreateManagedBitmapData<T>(T[,] buffer, int pixelWidth, KnownPixelFormat pixelFormat,
            Color32 backColor, byte alphaThreshold, WorkingColorSpace workingColorSpace,
            Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
            where T : unmanaged
        {
            Debug.Assert(palette == null || backColor.ToOpaque() == palette.BackColor && alphaThreshold == palette.AlphaThreshold);
            var cfg = new BitmapDataConfig(new Size(pixelWidth, buffer.GetLength(0)), pixelFormat.ToInfoInternal(),
                backColor, alphaThreshold, workingColorSpace, palette, trySetPaletteCallback, disposeCallback);
            return pixelFormat switch
            {
                KnownPixelFormat.Format32bppArgb => new ManagedBitmapData32Argb2D<T>(buffer, cfg),
                KnownPixelFormat.Format32bppPArgb => new ManagedBitmapData32PArgb2D<T>(buffer, cfg),
                KnownPixelFormat.Format32bppRgb => new ManagedBitmapData32Rgb2D<T>(buffer, cfg),
                KnownPixelFormat.Format24bppRgb => new ManagedBitmapData24Rgb2D<T>(buffer, cfg),
                KnownPixelFormat.Format8bppIndexed => new ManagedBitmapData8I2D<T>(buffer, cfg),
                KnownPixelFormat.Format4bppIndexed => new ManagedBitmapData4I2D<T>(buffer, cfg),
                KnownPixelFormat.Format1bppIndexed => new ManagedBitmapData1I2D<T>(buffer, cfg),
                KnownPixelFormat.Format64bppArgb => new ManagedBitmapData64Argb2D<T>(buffer, cfg),
                KnownPixelFormat.Format64bppPArgb => new ManagedBitmapData64PArgb2D<T>(buffer, cfg),
                KnownPixelFormat.Format48bppRgb => new ManagedBitmapData48Rgb2D<T>(buffer, cfg),
                KnownPixelFormat.Format16bppRgb565 => new ManagedBitmapData16Rgb565_2D<T>(buffer, cfg),
                KnownPixelFormat.Format16bppRgb555 => new ManagedBitmapData16Rgb555_2D<T>(buffer, cfg),
                KnownPixelFormat.Format16bppArgb1555 => new ManagedBitmapData16Argb1555_2D<T>(buffer, cfg),
                KnownPixelFormat.Format16bppGrayScale => new ManagedBitmapData16Gray2D<T>(buffer, cfg),
                KnownPixelFormat.Format128bppRgba => new ManagedBitmapData128Rgba2D<T>(buffer, cfg),
                KnownPixelFormat.Format128bppPRgba => new ManagedBitmapData128PRgba2D<T>(buffer, cfg),
                KnownPixelFormat.Format96bppRgb => new ManagedBitmapData96Rgb2D<T>(buffer, cfg),
                KnownPixelFormat.Format8bppGrayScale => new ManagedBitmapData8Gray2D<T>(buffer, cfg),
                KnownPixelFormat.Format32bppGrayScale => new ManagedBitmapData32Gray2D<T>(buffer, cfg),
                _ => throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat))
            };
        }

        internal static IBitmapDataInternal CreateManagedCustomBitmapData<T>(T[,] buffer, int pixelWidth, CustomBitmapDataConfig customConfig)
            where T : unmanaged
        {
            var commonConfig = new BitmapDataConfig(new Size(pixelWidth, buffer.GetLength(0)), customConfig.PixelFormat,
                customConfig.BackColor, customConfig.AlphaThreshold, customConfig.WorkingColorSpace, 
                null, null, customConfig.DisposeCallback);
            return new ManagedCustomBitmapData2D<T>(buffer, commonConfig, customConfig);
        }

        internal static IBitmapDataInternal CreateManagedCustomBitmapData<T>(T[,] buffer, int pixelWidth, CustomIndexedBitmapDataConfig customConfig)
            where T : unmanaged
        {
            Palette? palette = customConfig.Palette;
            var commonConfig = new BitmapDataConfig(new Size(pixelWidth, buffer.GetLength(0)), customConfig.PixelFormat,
                palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default,
                palette, customConfig.TrySetPaletteCallback, customConfig.DisposeCallback);
            return new ManagedCustomBitmapDataIndexed2D<T>(buffer, commonConfig, customConfig);
        }

        #endregion

        #region Unmanaged

        [SecurityCritical]
        internal static IBitmapDataInternal CreateUnmanagedBitmapData(IntPtr buffer, Size size, int stride, KnownPixelFormat pixelFormat,
            Color32 backColor, byte alphaThreshold, WorkingColorSpace workingColorSpace,
             Palette? palette, Func<Palette, bool>? trySetPaletteCallback, Action? disposeCallback)
        {
            Debug.Assert(palette == null || backColor.ToOpaque() == palette.BackColor && alphaThreshold == palette.AlphaThreshold);
            var cfg = new BitmapDataConfig(size, pixelFormat.ToInfoInternal(), backColor, alphaThreshold, workingColorSpace, palette, trySetPaletteCallback, disposeCallback);

            return pixelFormat switch
            {
                KnownPixelFormat.Format32bppArgb => new UnmanagedBitmapData32Argb(buffer, stride, cfg),
                KnownPixelFormat.Format32bppPArgb => new UnmanagedBitmapData32PArgb(buffer, stride, cfg),
                KnownPixelFormat.Format32bppRgb => new UnmanagedBitmapData32Rgb(buffer, stride, cfg),
                KnownPixelFormat.Format24bppRgb => new UnmanagedBitmapData24Rgb(buffer, stride, cfg),
                KnownPixelFormat.Format8bppIndexed => new UnmanagedBitmapData8I(buffer, stride, cfg),
                KnownPixelFormat.Format4bppIndexed => new UnmanagedBitmapData4I(buffer, stride, cfg),
                KnownPixelFormat.Format1bppIndexed => new UnmanagedBitmapData1I(buffer, stride, cfg),
                KnownPixelFormat.Format64bppArgb => new UnmanagedBitmapData64Argb(buffer, stride, cfg),
                KnownPixelFormat.Format64bppPArgb => new UnmanagedBitmapData64PArgb(buffer, stride, cfg),
                KnownPixelFormat.Format48bppRgb => new UnmanagedBitmapData48Rgb(buffer, stride, cfg),
                KnownPixelFormat.Format16bppRgb565 => new UnmanagedBitmapData16Rgb565(buffer, stride, cfg),
                KnownPixelFormat.Format16bppRgb555 => new UnmanagedBitmapData16Rgb555(buffer, stride, cfg),
                KnownPixelFormat.Format16bppArgb1555 => new UnmanagedBitmapData16Argb1555(buffer, stride, cfg),
                KnownPixelFormat.Format16bppGrayScale => new UnmanagedBitmapData16Gray(buffer, stride, cfg),
                KnownPixelFormat.Format128bppRgba => new UnmanagedBitmapData128Rgba(buffer, stride, cfg),
                KnownPixelFormat.Format128bppPRgba => new UnmanagedBitmapData128PRgba(buffer, stride, cfg),
                KnownPixelFormat.Format96bppRgb => new UnmanagedBitmapData96Rgb(buffer, stride, cfg),
                KnownPixelFormat.Format8bppGrayScale => new UnmanagedBitmapData8Gray(buffer, stride, cfg),
                KnownPixelFormat.Format32bppGrayScale => new UnmanagedBitmapData32Gray(buffer, stride, cfg),
                _ => throw new InvalidOperationException(Res.InternalError($"Unexpected pixel format {pixelFormat}"))
            };
        }

        [SecurityCritical]
        internal static IBitmapDataInternal CreateUnmanagedCustomBitmapData(IntPtr buffer, Size size, int stride, CustomBitmapDataConfig customConfig)
        {
            var commonConfig = new BitmapDataConfig(size, customConfig.PixelFormat,
                customConfig.BackColor, customConfig.AlphaThreshold, customConfig.WorkingColorSpace,
                null, null, customConfig.DisposeCallback);
            return new UnmanagedCustomBitmapData(buffer, stride, commonConfig, customConfig);
        }

        [SecurityCritical]
        internal static IBitmapDataInternal CreateUnmanagedCustomBitmapData(IntPtr buffer, Size size, int stride, CustomIndexedBitmapDataConfig customConfig)
        {
            Palette? palette = customConfig.Palette;
            var commonConfig = new BitmapDataConfig(size, customConfig.PixelFormat,
                palette?.BackColor ?? default, palette?.AlphaThreshold ?? 128, palette?.WorkingColorSpace ?? WorkingColorSpace.Default,
                palette, customConfig.TrySetPaletteCallback, customConfig.DisposeCallback);
            return new UnmanagedCustomBitmapDataIndexed(buffer, stride, commonConfig, customConfig);
        }

        internal static void DoSaveBitmapData(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, Stream stream)
        {
            KnownPixelFormat pixelFormat = bitmapData.GetKnownPixelFormat();

            context.Progress?.New(DrawingOperation.Saving, rect.Height + 1);
            var writer = new BinaryWriter(stream);

            writer.Write(magicNumber);
            writer.Write(rect.Width);
            writer.Write(rect.Height);
            writer.Write((int)pixelFormat);
            writer.Write(bitmapData.BackColor.Value);
            writer.Write(bitmapData.AlphaThreshold);

            // preventing saving too large palette of custom pixel formats
            Palette? palette = pixelFormat.IsIndexed() && bitmapData.Palette?.Count <= 1 << pixelFormat.ToBitsPerPixel() ? bitmapData.Palette : null;
            writer.Write(palette?.Count ?? 0);
            if (palette != null)
            {
                foreach (Color32 entry in palette.Entries)
                    writer.Write(entry.Value);
            }

            context.Progress?.Increment();
            if (context.IsCancellationRequested)
                return;

            try
            {
                if ((bitmapData is ManagedBitmapDataBase { IsCustomPixelFormat: false } or UnmanagedBitmapDataBase { IsCustomPixelFormat: false })
                    && bitmapData.RowSize >= pixelFormat.GetByteWidth(rect.Right) && pixelFormat.IsAtByteBoundary(rect.Left))
                {
                    DoSaveRaw(context, bitmapData, pixelFormat, rect, writer);
                    return;
                }

                DoSaveCustom(context, bitmapData, pixelFormat, rect, writer);
            }
            finally
            {
                stream.Flush();
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Validation

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it is called ValidateArguments")]
        private static void ValidateArguments(Size size, KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default, Palette? palette = null)
        {
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (!pixelFormat.IsIndexed() || palette == null)
                return;
            int maxColors = 1 << pixelFormat.ToBitsPerPixel();
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat.ToBitsPerPixel()), nameof(palette));
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it is called ValidateArguments")]
        private static void ValidateArguments(int width, int height, KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace = WorkingColorSpace.Default, Palette? palette = null)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException(nameof(width), PublicResources.ArgumentOutOfRange);
            if (height < 1)
                throw new ArgumentOutOfRangeException(nameof(height), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (!pixelFormat.IsIndexed() || palette == null)
                return;
            int maxColors = 1 << pixelFormat.ToBitsPerPixel();
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat.ToBitsPerPixel()), nameof(palette));
        }

        [SecuritySafeCritical]
        private static unsafe int ValidateArguments<T>(ArraySection<T> buffer, Size size, int stride,
            KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace, Palette? palette = null) where T : unmanaged
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
            int pixelSize = pixelFormat.ToBitsPerPixel() >> 3;
            if (pixelSize > 1 && stride % pixelSize != 0)
                throw new ArgumentException(Res.ImagingStrideFormatInvalid(pixelFormat, pixelSize), nameof(stride));
            if (!pixelFormat.IsIndexed() || palette == null)
                return elementWidth;
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            int maxColors = 1 << pixelFormat.ToBitsPerPixel();
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat.ToBitsPerPixel()), nameof(palette));

            return elementWidth;
        }

        [SecuritySafeCritical]
        private static unsafe int ValidateArguments<T>(ArraySection<T> buffer, Size size, int stride,
            PixelFormatInfo pixelFormat, WorkingColorSpace workingColorSpace, Palette? palette = null) where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormat.BitsPerPixel), 0), nameof(pixelFormat));
            if (pixelFormat.Indexed && bpp > 16)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatTooLarge, nameof(pixelFormat));
            if (stride < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            int elementSize = sizeof(T);
            if (stride % elementSize != 0)
                throw new ArgumentException(Res.ImagingStrideInvalid(typeof(T), sizeof(T)), nameof(stride));
            int elementWidth = stride / elementSize;
            if (buffer.Length < elementWidth * size.Height)
                throw new ArgumentException(Res.ImagingBufferLengthTooSmall(elementWidth * size.Height), nameof(buffer));
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (palette == null)
                return elementWidth;

            int maxColors = 1 << bpp;
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, bpp), nameof(palette));

            return elementWidth;
        }

        [SecuritySafeCritical]
        private static unsafe int ValidateArguments<T>(ArraySection<T> buffer, Size size, int stride, CustomBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (customBitmapDataConfig == null)
                throw new ArgumentNullException(nameof(customBitmapDataConfig), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            PixelFormatInfo pixelFormat = customBitmapDataConfig.PixelFormat;
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan($"{nameof(customBitmapDataConfig.PixelFormat)}.{nameof(customBitmapDataConfig.PixelFormat.BitsPerPixel)}", 0), nameof(customBitmapDataConfig));
            if (pixelFormat.Indexed)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingNonIndexedPixelFormatExpected), nameof(customBitmapDataConfig));
            if (stride < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            int elementSize = sizeof(T);
            if (stride % elementSize != 0)
                throw new ArgumentException(Res.ImagingStrideInvalid(typeof(T), sizeof(T)), nameof(stride));
            int elementWidth = stride / elementSize;
            if (buffer.Length < elementWidth * size.Height)
                throw new ArgumentException(Res.ImagingBufferLengthTooSmall(elementWidth * size.Height), nameof(buffer));
            if (customBitmapDataConfig.WorkingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.WorkingColorSpace), PublicResources.EnumOutOfRange<WorkingColorSpace>()), nameof(customBitmapDataConfig));
            if ((customBitmapDataConfig.RowGetColor32 ?? customBitmapDataConfig.RowSetColor32 ?? customBitmapDataConfig.RowGetPColor32 ?? customBitmapDataConfig.RowSetPColor32
                ?? customBitmapDataConfig.RowGetColor64 ?? customBitmapDataConfig.RowSetColor64 ?? customBitmapDataConfig.RowGetPColor64 ?? customBitmapDataConfig.RowSetPColor64
                ?? customBitmapDataConfig.RowGetColorF ?? customBitmapDataConfig.RowSetColorF ?? customBitmapDataConfig.RowGetPColorF ?? (Delegate?)customBitmapDataConfig.RowSetPColorF) == null)
                throw new ArgumentException(Res.ImagingNoPixelAccessSpecified, nameof(customBitmapDataConfig));
            
            return elementWidth;
        }

        [SecuritySafeCritical]
        private static unsafe int ValidateArguments<T>(ArraySection<T> buffer, Size size, int stride, CustomIndexedBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (customBitmapDataConfig == null)
                throw new ArgumentNullException(nameof(customBitmapDataConfig), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            PixelFormatInfo pixelFormat = customBitmapDataConfig.PixelFormat;
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan($"{nameof(customBitmapDataConfig.PixelFormat)}.{nameof(customBitmapDataConfig.PixelFormat.BitsPerPixel)}", 0), nameof(customBitmapDataConfig));
            if (!pixelFormat.Indexed)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingIndexedPixelFormatExpected), nameof(customBitmapDataConfig));
            if (bpp > 16)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingIndexedPixelFormatTooLarge), nameof(customBitmapDataConfig));
            if (stride < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            int elementSize = sizeof(T);
            if (stride % elementSize != 0)
                throw new ArgumentException(Res.ImagingStrideInvalid(typeof(T), sizeof(T)), nameof(stride));
            int elementWidth = stride / elementSize;
            if (buffer.Length < elementWidth * size.Height)
                throw new ArgumentException(Res.ImagingBufferLengthTooSmall(elementWidth * size.Height), nameof(buffer));
            if (customBitmapDataConfig.RowGetColorIndex == null && customBitmapDataConfig.RowSetColorIndex == null)
                throw new ArgumentException(Res.ImagingNoPixelAccessSpecified, nameof(customBitmapDataConfig));
            if (customBitmapDataConfig.Palette == null)
                return elementWidth;

            int maxColors = 1 << bpp;
            if (customBitmapDataConfig.Palette.Count > maxColors)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.Palette), Res.ImagingPaletteTooLarge(maxColors, bpp)), nameof(customBitmapDataConfig));

            return elementWidth;
        }

        [SecuritySafeCritical]
        private static unsafe void ValidateArguments<T>(T[,] buffer, int pixelWidth, KnownPixelFormat pixelFormat,
            WorkingColorSpace workingColorSpace, Palette? palette = null) where T : unmanaged
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            int stride = sizeof(T) * buffer.GetLength(1);
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (!pixelFormat.IsIndexed() || palette == null)
                return;
            int maxColors = 1 << pixelFormat.ToBitsPerPixel();
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat.ToBitsPerPixel()), nameof(palette));
        }

        [SecuritySafeCritical]
        private static unsafe void ValidateArguments<T>(T[,] buffer, int pixelWidth, PixelFormatInfo pixelFormat,
            WorkingColorSpace workingColorSpace, Palette? palette = null) where T : unmanaged
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormat.BitsPerPixel), 0), nameof(pixelFormat));
            if (pixelFormat.Indexed && bpp > 16)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatTooLarge, nameof(pixelFormat));
            int stride = sizeof(T) * buffer.GetLength(1);
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (palette == null)
                return;
            int maxColors = 1 << bpp;
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, bpp), nameof(palette));
        }

        [SecuritySafeCritical]
        private static unsafe void ValidateArguments<T>(T[,] buffer, int pixelWidth, CustomBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (customBitmapDataConfig == null)
                throw new ArgumentNullException(nameof(customBitmapDataConfig), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            PixelFormatInfo pixelFormat = customBitmapDataConfig.PixelFormat;
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan($"{nameof(customBitmapDataConfig.PixelFormat)}.{nameof(customBitmapDataConfig.PixelFormat.BitsPerPixel)}", 0), nameof(customBitmapDataConfig));
            if (pixelFormat.Indexed)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingNonIndexedPixelFormatExpected), nameof(customBitmapDataConfig));
            int stride = sizeof(T) * buffer.GetLength(1);
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (customBitmapDataConfig.WorkingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.WorkingColorSpace), PublicResources.EnumOutOfRange<WorkingColorSpace>()), nameof(customBitmapDataConfig));
            if ((customBitmapDataConfig.RowGetColor32 ?? customBitmapDataConfig.RowSetColor32 ?? customBitmapDataConfig.RowGetPColor32 ?? customBitmapDataConfig.RowSetPColor32
                ?? customBitmapDataConfig.RowGetColor64 ?? customBitmapDataConfig.RowSetColor64 ?? customBitmapDataConfig.RowGetPColor64 ?? customBitmapDataConfig.RowSetPColor64
                ?? customBitmapDataConfig.RowGetColorF ?? customBitmapDataConfig.RowSetColorF ?? customBitmapDataConfig.RowGetPColorF ?? (Delegate?)customBitmapDataConfig.RowSetPColorF) == null)
                throw new ArgumentException(Res.ImagingNoPixelAccessSpecified, nameof(customBitmapDataConfig));
        }

        [SecuritySafeCritical]
        private static unsafe void ValidateArguments<T>(T[,] buffer, int pixelWidth, CustomIndexedBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (customBitmapDataConfig == null)
                throw new ArgumentNullException(nameof(customBitmapDataConfig), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            PixelFormatInfo pixelFormat = customBitmapDataConfig.PixelFormat;
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan($"{nameof(customBitmapDataConfig.PixelFormat)}.{nameof(customBitmapDataConfig.PixelFormat.BitsPerPixel)}", 0), nameof(customBitmapDataConfig));
            if (!pixelFormat.Indexed)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingIndexedPixelFormatExpected), nameof(customBitmapDataConfig));
            if (bpp > 16)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingIndexedPixelFormatTooLarge), nameof(customBitmapDataConfig));
            int stride = sizeof(T) * buffer.GetLength(1);
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (customBitmapDataConfig.RowGetColorIndex == null && customBitmapDataConfig.RowSetColorIndex == null)
                throw new ArgumentException(Res.ImagingNoPixelAccessSpecified, nameof(customBitmapDataConfig));
            if (customBitmapDataConfig.Palette == null)
                return;

            int maxColors = 1 << bpp;
            if (customBitmapDataConfig.Palette.Count > maxColors)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.Palette), Res.ImagingPaletteTooLarge(maxColors, bpp)), nameof(customBitmapDataConfig));
        }

        [SecuritySafeCritical]
        private static unsafe void ValidateArguments<T>(Array2D<T> buffer, int pixelWidth,
            KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace, Palette? palette = null) where T : unmanaged
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
            int pixelSize = pixelFormat.ToBitsPerPixel() >> 3;
            if (pixelSize > 1 && stride % pixelSize != 0)
                throw new ArgumentException(Res.ImagingPixelWidthInvalid(typeof(T), pixelFormat, sizeof(T), pixelSize), nameof(pixelWidth));
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (!pixelFormat.IsIndexed() || palette == null)
                return;
            int maxColors = 1 << pixelFormat.ToBitsPerPixel();
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat.ToBitsPerPixel()), nameof(palette));
        }

        [SecuritySafeCritical]
        private static unsafe void ValidateArguments<T>(Array2D<T> buffer, int pixelWidth,
            PixelFormatInfo pixelFormat, WorkingColorSpace workingColorSpace, Palette? palette = null) where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormat.BitsPerPixel), 0), nameof(pixelFormat));
            if (pixelFormat.Indexed && bpp > 16)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatTooLarge, nameof(pixelFormat));
            int stride = sizeof(T) * buffer.Width;
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (palette == null)
                return;
            int maxColors = 1 << bpp;
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, bpp), nameof(palette));
        }

        [SecuritySafeCritical]
        private static unsafe void ValidateArguments<T>(Array2D<T> buffer, int pixelWidth, CustomBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (customBitmapDataConfig == null)
                throw new ArgumentNullException(nameof(customBitmapDataConfig), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            PixelFormatInfo pixelFormat = customBitmapDataConfig.PixelFormat;
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan($"{nameof(customBitmapDataConfig.PixelFormat)}.{nameof(customBitmapDataConfig.PixelFormat.BitsPerPixel)}", 0), nameof(customBitmapDataConfig));
            if (pixelFormat.Indexed)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingNonIndexedPixelFormatExpected), nameof(customBitmapDataConfig));
            int stride = sizeof(T) * buffer.Width;
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (customBitmapDataConfig.WorkingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.WorkingColorSpace), PublicResources.EnumOutOfRange<WorkingColorSpace>()), nameof(customBitmapDataConfig));
            if ((customBitmapDataConfig.RowGetColor32 ?? customBitmapDataConfig.RowSetColor32 ?? customBitmapDataConfig.RowGetPColor32 ?? customBitmapDataConfig.RowSetPColor32
                ?? customBitmapDataConfig.RowGetColor64 ?? customBitmapDataConfig.RowSetColor64 ?? customBitmapDataConfig.RowGetPColor64 ?? customBitmapDataConfig.RowSetPColor64
                ?? customBitmapDataConfig.RowGetColorF ?? customBitmapDataConfig.RowSetColorF ?? customBitmapDataConfig.RowGetPColorF ?? (Delegate?)customBitmapDataConfig.RowSetPColorF) == null)
                throw new ArgumentException(Res.ImagingNoPixelAccessSpecified, nameof(customBitmapDataConfig));
        }

        [SecuritySafeCritical]
        private static unsafe void ValidateArguments<T>(Array2D<T> buffer, int pixelWidth, CustomIndexedBitmapDataConfig customBitmapDataConfig)
            where T : unmanaged
        {
            if (buffer.IsNull)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (customBitmapDataConfig == null)
                throw new ArgumentNullException(nameof(customBitmapDataConfig), PublicResources.ArgumentNull);
            if (buffer.Length == 0)
                throw new ArgumentException(PublicResources.ArgumentEmpty, nameof(buffer));
            PixelFormatInfo pixelFormat = customBitmapDataConfig.PixelFormat;
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan($"{nameof(customBitmapDataConfig.PixelFormat)}.{nameof(customBitmapDataConfig.PixelFormat.BitsPerPixel)}", 0), nameof(customBitmapDataConfig));
            if (!pixelFormat.Indexed)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingIndexedPixelFormatExpected), nameof(customBitmapDataConfig));
            if (bpp > 16)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingIndexedPixelFormatTooLarge), nameof(customBitmapDataConfig));
            int stride = sizeof(T) * buffer.Width;
            if (stride < pixelFormat.GetByteWidth(pixelWidth))
                throw new ArgumentOutOfRangeException(nameof(pixelWidth), Res.ImagingWidthTooLarge);
            if (customBitmapDataConfig.RowGetColorIndex == null && customBitmapDataConfig.RowSetColorIndex == null)
                throw new ArgumentException(Res.ImagingNoPixelAccessSpecified, nameof(customBitmapDataConfig));
            if (customBitmapDataConfig.Palette == null)
                return;

            int maxColors = 1 << bpp;
            if (customBitmapDataConfig.Palette.Count > maxColors)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.Palette), Res.ImagingPaletteTooLarge(maxColors, bpp)), nameof(customBitmapDataConfig));
        }


        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it is called ValidateArguments")]
        private static void ValidateArguments(IntPtr buffer, Size size, int stride,
            KnownPixelFormat pixelFormat, WorkingColorSpace workingColorSpace, Palette? palette = null)
        {
            if (buffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            if (!pixelFormat.IsValidFormat())
                throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            if (stride.Abs() < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (!pixelFormat.IsIndexed() || palette == null)
                return;
            int maxColors = 1 << pixelFormat.ToBitsPerPixel();
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, pixelFormat.ToBitsPerPixel()), nameof(palette));
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it is called ValidateArguments")]
        private static void ValidateArguments(IntPtr buffer, Size size, int stride,
            PixelFormatInfo pixelFormat, WorkingColorSpace workingColorSpace, Palette? palette = null)
        {
            if (buffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan(nameof(pixelFormat.BitsPerPixel), 0), nameof(pixelFormat));
            if (pixelFormat.Indexed && bpp > 16)
                throw new ArgumentException(Res.ImagingIndexedPixelFormatTooLarge, nameof(pixelFormat));
            if (stride.Abs() < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));
            if (palette == null)
                return;
            int maxColors = 1 << bpp;
            if (palette.Count > maxColors)
                throw new ArgumentException(Res.ImagingPaletteTooLarge(maxColors, bpp), nameof(palette));
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it is called ValidateArguments")]
        private static void ValidateArguments(IntPtr buffer, Size size, int stride, CustomBitmapDataConfig customBitmapDataConfig)
        {
            if (buffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (customBitmapDataConfig == null)
                throw new ArgumentNullException(nameof(customBitmapDataConfig), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            PixelFormatInfo pixelFormat = customBitmapDataConfig.PixelFormat;
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan($"{nameof(customBitmapDataConfig.PixelFormat)}.{nameof(customBitmapDataConfig.PixelFormat.BitsPerPixel)}", 0), nameof(customBitmapDataConfig));
            if (pixelFormat.Indexed)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingNonIndexedPixelFormatExpected), nameof(customBitmapDataConfig));
            if (stride.Abs() < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            if (customBitmapDataConfig.WorkingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.WorkingColorSpace), PublicResources.EnumOutOfRange<WorkingColorSpace>()), nameof(customBitmapDataConfig));
            if ((customBitmapDataConfig.RowGetColor32 ?? customBitmapDataConfig.RowSetColor32 ?? customBitmapDataConfig.RowGetPColor32 ?? customBitmapDataConfig.RowSetPColor32
                ?? customBitmapDataConfig.RowGetColor64 ?? customBitmapDataConfig.RowSetColor64 ?? customBitmapDataConfig.RowGetPColor64 ?? customBitmapDataConfig.RowSetPColor64
                ?? customBitmapDataConfig.RowGetColorF ?? customBitmapDataConfig.RowSetColorF ?? customBitmapDataConfig.RowGetPColorF ?? (Delegate?)customBitmapDataConfig.RowSetPColorF) == null)
                throw new ArgumentException(Res.ImagingNoPixelAccessSpecified, nameof(customBitmapDataConfig));
        }

        [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local", Justification = "That's why it is called ValidateArguments")]
        private static void ValidateArguments(IntPtr buffer, Size size, int stride, CustomIndexedBitmapDataConfig customBitmapDataConfig)
        {
            if (buffer == IntPtr.Zero)
                throw new ArgumentNullException(nameof(buffer), PublicResources.ArgumentNull);
            if (customBitmapDataConfig == null)
                throw new ArgumentNullException(nameof(customBitmapDataConfig), PublicResources.ArgumentNull);
            if (size.Width < 1 || size.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(size), PublicResources.ArgumentOutOfRange);
            PixelFormatInfo pixelFormat = customBitmapDataConfig.PixelFormat;
            int bpp = pixelFormat.BitsPerPixel;
            if (bpp == 0)
                throw new ArgumentException(PublicResources.PropertyMustBeGreaterThan($"{nameof(customBitmapDataConfig.PixelFormat)}.{nameof(customBitmapDataConfig.PixelFormat.BitsPerPixel)}", 0), nameof(customBitmapDataConfig));
            if (!pixelFormat.Indexed)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingIndexedPixelFormatExpected), nameof(customBitmapDataConfig));
            if (bpp > 16)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.PixelFormat), Res.ImagingIndexedPixelFormatTooLarge), nameof(customBitmapDataConfig));
            if (stride.Abs() < pixelFormat.GetByteWidth(size.Width))
                throw new ArgumentOutOfRangeException(nameof(stride), Res.ImagingStrideTooSmall(pixelFormat.GetByteWidth(size.Width)));
            if (customBitmapDataConfig.RowGetColorIndex == null && customBitmapDataConfig.RowSetColorIndex == null)
                throw new ArgumentException(Res.ImagingNoPixelAccessSpecified, nameof(customBitmapDataConfig));
            if (customBitmapDataConfig.Palette == null)
                return;

            int maxColors = 1 << bpp;
            if (customBitmapDataConfig.Palette.Count > maxColors)
                throw new ArgumentException(PublicResources.PropertyMessage(nameof(customBitmapDataConfig.Palette), Res.ImagingPaletteTooLarge(maxColors, bpp)), nameof(customBitmapDataConfig));
        }

        #endregion

        #region Save

        [SecuritySafeCritical]
        private static void DoSaveCustom(IAsyncContext context, IBitmapDataInternal bitmapData, KnownPixelFormat pixelFormat, Rectangle rect, BinaryWriter writer)
        {
            if (pixelFormat.IsIndexed())
            {
                DoSaveCustomIndexed(context, bitmapData, pixelFormat, rect, writer);
                return;
            }

            // using a temp 1x1 managed bitmap data for the conversion
            IBitmapDataRowInternal row = bitmapData.GetRowCached(rect.Top);
            int byteLength = pixelFormat.ToBitsPerPixel() >> 3;
#if NETCOREAPP3_0_OR_GREATER
            Span<byte> buffer = stackalloc byte[byteLength];
            IntPtr address;
            unsafe { address = (IntPtr)Unsafe.AsPointer(ref buffer[0]); }
            using IBitmapDataInternal tempData = CreateUnmanagedBitmapData(address, new Size(1, 1), byteLength, pixelFormat,
                bitmapData.BackColor, bitmapData.AlphaThreshold, bitmapData.WorkingColorSpace, bitmapData.Palette, null, null);
#else
            var buffer = new byte[byteLength];
            using IBitmapDataInternal tempData = CreateManagedBitmapData(new Array2D<byte>(buffer, 1, byteLength), 1, pixelFormat,
                bitmapData.BackColor, bitmapData.AlphaThreshold, bitmapData.WorkingColorSpace, bitmapData.Palette, null, null);
#endif
            IBitmapDataRowInternal tempRow = tempData.GetRowCached(0);
            for (int y = 0; y < rect.Height; y++)
            {
                if (context.IsCancellationRequested)
                    return;

                switch (pixelFormat)
                {
                    case KnownPixelFormat.Format128bppRgba:
                    case KnownPixelFormat.Format96bppRgb:
                    case KnownPixelFormat.Format32bppGrayScale:
                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            tempRow.DoSetColorF(0, row.DoGetColorF(x));
                            writer.Write(buffer);
                        }
                        break;

                    case KnownPixelFormat.Format128bppPRgba:
                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            tempRow.DoSetPColorF(0, row.DoGetPColorF(x));
                            writer.Write(buffer);
                        }
                        break;

                    case KnownPixelFormat.Format64bppArgb:
                    case KnownPixelFormat.Format48bppRgb:
                    case KnownPixelFormat.Format16bppGrayScale:
                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            tempRow.DoSetColor64(0, row.DoGetColor64(x));
                            writer.Write(buffer);
                        }
                        break;

                    case KnownPixelFormat.Format64bppPArgb:
                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            tempRow.DoSetPColor64(0, row.DoGetPColor64(x));
                            writer.Write(buffer);
                        }
                        break;

                    case KnownPixelFormat.Format32bppPArgb:
                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            tempRow.DoSetPColor32(0, row.DoGetPColor32(x));
                            writer.Write(buffer);
                        }
                        break;

                    default:
                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            tempRow.DoSetColor32(0, row.DoGetColor32(x));
                            writer.Write(buffer);
                        }
                        break;
                }

                row.MoveNextRow();
                context.Progress?.Increment();
            }
        }

        [SecuritySafeCritical]
        private static void DoSaveCustomIndexed(IAsyncContext context, IBitmapDataInternal bitmapData, KnownPixelFormat pixelFormat, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.GetRowCached(rect.Top);
            for (int y = 0; y < rect.Height; y++)
            {
                if (context.IsCancellationRequested)
                    return;

                switch (pixelFormat)
                {
                    case KnownPixelFormat.Format1bppIndexed:
                        byte bits = 0;
                        int x;
                        for (x = 0; x < rect.Width; x++)
                        {
                            if (row.DoGetColorIndex(rect.Left + x) != 0)
                                bits |= (byte)(128 >> (x & 7));

                            if ((x & 7) == 7)
                            {
                                writer.Write(bits);
                                bits = 0;
                            }
                        }

                        // columns are not multiple of 8: writing last byte
                        if ((x & 7) != 0)
                            writer.Write(bits);
                        break;

                    case KnownPixelFormat.Format4bppIndexed:
                        bits = 0;
                        for (x = 0; x < rect.Width; x++)
                        {
                            int colorIndex = row.DoGetColorIndex(rect.Left + x);
                            if ((x & 1) == 0)
                                bits = (byte)(colorIndex << 4);
                            else
                                writer.Write((byte)(bits | colorIndex));
                        }

                        // odd columns: writing last byte
                        if ((x & 1) != 0)
                            writer.Write(bits);
                        break;

                    case KnownPixelFormat.Format8bppIndexed:
                        for (x = rect.Left; x < rect.Right; x++)
                            writer.Write((byte)row.DoGetColorIndex(x));
                        break;

                    default:
                        throw new InvalidOperationException(Res.InternalError($"Unexpected indexed format: {pixelFormat}"));
                }

                row.MoveNextRow();
                context.Progress?.Increment();
            }
        }

        private static void DoSaveRaw(IAsyncContext context, IBitmapDataInternal bitmapData, KnownPixelFormat pixelFormat, Rectangle rect, BinaryWriter writer)
        {
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
                case 96:
                    rect.X *= 3;
                    rect.Width *= 3;
                    DoSaveRawInts(context, bitmapData, rect, writer);
                    return;
                case 128:
                    rect.X <<= 1;
                    rect.Width <<= 1;
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

        [SecuritySafeCritical]
        private static void DoSaveRawBytes(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.GetRowCached(rect.Top);
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

        [SecuritySafeCritical]
        private static void DoSaveRawShorts(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.GetRowCached(rect.Top);
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

        [SecuritySafeCritical]
        private static void DoSaveRawInts(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.GetRowCached(rect.Top);
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

        [SecuritySafeCritical]
        private static void DoSaveRawLongs(IAsyncContext context, IBitmapDataInternal bitmapData, Rectangle rect, BinaryWriter writer)
        {
            IBitmapDataRowInternal row = bitmapData.GetRowCached(rect.Top);
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
        [SecuritySafeCritical]
        private static IReadWriteBitmapData? DoLoadBitmapData(IAsyncContext context, Stream stream)
        {
            context.Progress?.New(DrawingOperation.Loading, 1000);
            var reader = new BinaryReader(stream);

            if (reader.ReadInt32() != magicNumber)
                throw new ArgumentException(Res.ImagingNotBitmapDataStream, nameof(stream));
            var size = new Size(reader.ReadInt32(), reader.ReadInt32());
            var pixelFormat = (KnownPixelFormat)reader.ReadInt32();
            Color32 backColor = new Color32(reader.ReadUInt32());
            byte alphaThreshold = reader.ReadByte();

            Palette? palette = null;
            int paletteLength = reader.ReadInt32();
            if (paletteLength > 0)
            {
                var entries = new Color32[paletteLength];
                for (int i = 0; i < paletteLength; i++)
                    entries[i] = new Color32(reader.ReadUInt32());

                // useLinearBlending: unfortunately, cannot be added to BDAT without breaking compatibility
                palette = new Palette(entries, backColor, alphaThreshold, default, null);
            }

            context.Progress?.SetProgressValue((int)(stream.Position * 1000 / stream.Length));
            if (context.IsCancellationRequested)
                return null;

            // workingColorSpace: unfortunately, cannot be added to BDAT without breaking compatibility
            IBitmapDataInternal result = CreateManagedBitmapData(size, pixelFormat, backColor, alphaThreshold, WorkingColorSpace.Default, palette);
            int bpp = pixelFormat.ToBitsPerPixel();
            bool canceled = false;
            try
            {
                IBitmapDataRowInternal row = result.GetRowCached(0);
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
                        case 128:
                            for (int x = 0; x < result.Width; x++)
                            {
                                row.DoWriteRaw(x << 1, reader.ReadInt64());
                                row.DoWriteRaw((x << 1) + 1, reader.ReadInt64());
                            }

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
