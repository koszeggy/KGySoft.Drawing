#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BitmapDataFactory.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2019 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution. If not, then this file is considered as
//  an illegal copy.
//
//  Unauthorized copying of this file, via any medium is strictly prohibited.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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

        /// <summary>
        /// Loads a managed <see cref="IReadWriteBitmapData"/> instance from the specified <paramref name="stream"/> that was saved by
        /// the <see cref="BitmapDataExtensions.Save">BitmapDataExtensions.Save</see> method.
        /// </summary>
        /// <param name="stream">The stream to load the bitmap data from.</param>
        /// <returns>A managed <see cref="IReadWriteBitmapData"/> instance loaded from the specified <paramref name="stream"/>.</returns>
        /// <remarks>
        /// <note>This method blocks the caller, and does not support cancellation or reporting progress. Use the <see cref="BeginLoad">BeginLoad</see>
        /// or <see cref="LoadAsync">LoadAsync</see> (in .NET 4.0 and above) methods for asynchronous call and to set up cancellation or for reporting progress.</note>
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
        /// In .NET 4.0 and above you can use the <see cref="LoadAsync">LoadAsync</see> method instead.
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

        #region Internal Methods

        /// <summary>
        /// Creates a native <see cref="IBitmapDataInternal"/> from a <see cref="Bitmap"/>.
        /// </summary>
        internal static IBitmapDataInternal CreateBitmapData(Bitmap bitmap, ImageLockMode lockMode, Color32 backColor = default, byte alphaThreshold = 128, Palette? palette = null)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (!lockMode.IsDefined())
                throw new ArgumentOutOfRangeException(nameof(lockMode), PublicResources.EnumOutOfRange(lockMode));
            Debug.Assert(palette == null || backColor.ToOpaque() == palette.BackColor && alphaThreshold == palette.AlphaThreshold);

            var pixelFormat = bitmap.PixelFormat;
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return new NativeBitmapData<NativeBitmapDataRow32Argb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format32bppPArgb:
                    return new NativeBitmapData<NativeBitmapDataRow32PArgb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format32bppRgb:
                    return new NativeBitmapData<NativeBitmapDataRow32Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format24bppRgb:
                    return new NativeBitmapData<NativeBitmapDataRow24Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format8bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow8I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold, palette);

                case PixelFormat.Format4bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow4I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold, palette);

                case PixelFormat.Format1bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow1I>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold, palette);

                case PixelFormat.Format64bppArgb:
                    return new NativeBitmapData<NativeBitmapDataRow64Argb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format64bppPArgb:
                    return new NativeBitmapData<NativeBitmapDataRow64PArgb>(bitmap, pixelFormat, lockMode);

                case PixelFormat.Format48bppRgb:
                    return new NativeBitmapData<NativeBitmapDataRow48Rgb>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb565:
                    return OSUtils.IsWindows
                        ? new NativeBitmapData<NativeBitmapDataRow16Rgb565>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold)
                        : new NativeBitmapData<NativeBitmapDataRow16Rgb565Via24Bpp>(bitmap, PixelFormat.Format24bppRgb, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb555:
                    return OSUtils.IsWindows
                        ? new NativeBitmapData<NativeBitmapDataRow16Rgb555>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold)
                        : new NativeBitmapData<NativeBitmapDataRow16Rgb555Via24Bpp>(bitmap, PixelFormat.Format24bppRgb, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppArgb1555:
                    return new NativeBitmapData<NativeBitmapDataRow16Argb1555>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

                case PixelFormat.Format16bppGrayScale:
                    return new NativeBitmapData<NativeBitmapDataRow16Gray>(bitmap, pixelFormat, lockMode, backColor, alphaThreshold);

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
            switch (pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow8I>(bitmap, pixelFormat, lockMode, quantizingSession);

                case PixelFormat.Format4bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow4I>(bitmap, pixelFormat, lockMode, quantizingSession);

                case PixelFormat.Format1bppIndexed:
                    return new NativeBitmapData<NativeBitmapDataRow1I>(bitmap, pixelFormat, lockMode, quantizingSession);

                default:
                    throw new InvalidOperationException(Res.InternalError($"Unexpected indexed format: {pixelFormat}"));
            }
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
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return new ManagedBitmapData<Color32, ManagedBitmapDataRow32Argb>(size, pixelFormat);

                case PixelFormat.Format32bppPArgb:
                    return new ManagedBitmapData<Color32, ManagedBitmapDataRow32PArgb>(size, pixelFormat);

                case PixelFormat.Format32bppRgb:
                    return new ManagedBitmapData<Color32, ManagedBitmapDataRow32Rgb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format24bppRgb:
                    return new ManagedBitmapData<Color24, ManagedBitmapDataRow24Rgb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format8bppIndexed:
                    return new ManagedBitmapData<byte, ManagedBitmapDataRow8I>(size, pixelFormat, backColor, alphaThreshold, palette);

                case PixelFormat.Format4bppIndexed:
                    return new ManagedBitmapData<byte, ManagedBitmapDataRow4I>(size, pixelFormat, backColor, alphaThreshold, palette);

                case PixelFormat.Format1bppIndexed:
                    return new ManagedBitmapData<byte, ManagedBitmapDataRow1I>(size, pixelFormat, backColor, alphaThreshold, palette);

                case PixelFormat.Format64bppArgb:
                    return new ManagedBitmapData<Color64, ManagedBitmapDataRow64Argb>(size, pixelFormat);

                case PixelFormat.Format64bppPArgb:
                    return new ManagedBitmapData<Color64, ManagedBitmapDataRow64PArgb>(size, pixelFormat);

                case PixelFormat.Format48bppRgb:
                    return new ManagedBitmapData<Color48, ManagedBitmapDataRow48Rgb>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb565:
                    return new ManagedBitmapData<Color16Rgb565, ManagedBitmapDataRow16Rgb565>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format16bppRgb555:
                    return new ManagedBitmapData<Color16Rgb555, ManagedBitmapDataRow16Rgb555>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format16bppArgb1555:
                    return new ManagedBitmapData<Color16Argb1555, ManagedBitmapDataRow16Argb1555>(size, pixelFormat, backColor, alphaThreshold);

                case PixelFormat.Format16bppGrayScale:
                    return new ManagedBitmapData<Color16Gray, ManagedBitmapDataRow16Gray>(size, pixelFormat, backColor, alphaThreshold);

                default:
                    throw new ArgumentOutOfRangeException(nameof(pixelFormat), Res.PixelFormatInvalid(pixelFormat));
            }
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
                if (pixelFormat.ToBitsPerPixel() > 32 && bitmapData is NativeBitmapDataBase && ColorExtensions.Max16BppValue != UInt16.MaxValue)
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
