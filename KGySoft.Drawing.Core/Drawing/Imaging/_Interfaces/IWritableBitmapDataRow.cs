#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IWritableBitmapDataRow.cs
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
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides a fast write-only access to a single row of an <see cref="IWritableBitmapData"/>.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a>method for details and examples.
    /// </summary>
    /// <seealso cref="IReadableBitmapDataRow"/>
    /// <seealso cref="IReadWriteBitmapDataRow"/>
    /// <seealso cref="IWritableBitmapData"/>
    public interface IWritableBitmapDataRow : IBitmapDataRow
    {
        #region Indexers

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate from a <see cref="Color32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <value>A <see cref="Color32"/> instance that represents the color of the specified pixel.</value>
        /// <remarks>
        /// <para>The <paramref name="value"/> represents a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.
        /// This member is practically the same as the <see cref="SetColor32">SetColor32</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="SetColor32"/>
        /// <seealso cref="SetColorIndex"/>
        /// <seealso cref="WriteRaw{T}"/>
        Color32 this[int x] { set; }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate from a <see cref="Color"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="Color"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.
        /// The <see cref="this">indexer</see> and the <see cref="SetColor32">SetColor32</see> method work with the same range of colors and have a slightly better performance than this method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="SetColor32"/>
        /// <seealso cref="SetColorIndex"/>
        /// <seealso cref="WriteRaw{T}"/>
        void SetColor(int x, Color color);

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate from a <see cref="Color32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="Color32"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.
        /// This method is practically the same as the <see cref="this">indexer</see>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="SetColorIndex"/>
        /// <seealso cref="WriteRaw{T}"/>
        void SetColor32(int x, Color32 color);

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate from a <see cref="PColor32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="PColor32"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="WriteRaw{T}"/>
        void SetPColor32(int x, PColor32 color);

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate from a <see cref="Color64"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="Color64"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a non-premultiplied color with 16 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="WriteRaw{T}"/>
        void SetColor64(int x, Color64 color);

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate from a <see cref="PColor64"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="PColor64"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a premultiplied color with 16 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="WriteRaw{T}"/>
        void SetPColor64(int x, PColor64 color);

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate from a <see cref="ColorF"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="ColorF"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a non-premultiplied color with 32 bits per channel in the linear color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="WriteRaw{T}"/>
        void SetColorF(int x, ColorF color);

        /// <summary>
        /// Sets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate from a <see cref="PColorF"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="PColorF"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a premultiplied color with 32 bits per channel in the linear color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="WriteRaw{T}"/>
        void SetPColorF(int x, PColorF color);

        /// <summary>
        /// If the owner <see cref="IWritableBitmapData"/> has an indexed pixel format, then sets the color index of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// </summary>
        /// <param name="x">The x-coordinate of the color index to set.</param>
        /// <param name="colorIndex">A palette index that represents the color to be set.</param>
        /// <remarks>
        /// <para>This method can be used only if <see cref="PixelFormatInfo.Indexed"/> is set in the <see cref="IBitmapData.PixelFormat"/> of the parent <see cref="IWritableBitmapData"/>.
        /// Otherwise, this method throws an <see cref="InvalidOperationException"/>.</para>
        /// <para>To set the actual color of the pixel at the <paramref name="x"/> coordinate you can use the <c>SetColor...</c>/<c>SetPColor...</c>
        /// methods or the <see cref="this">indexer</see>.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IWritableBitmapData"/>.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="IWritableBitmapDataRow"/> does not belong to a row of an indexed <see cref="IWritableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="SetColor"/>
        /// <seealso cref="WriteRaw{T}"/>
        void SetColorIndex(int x, int colorIndex);

        /// <summary>
        /// Sets the underlying raw value within the current <see cref="IWritableBitmapDataRow"/> at the specified <paramref name="x"/> coordinate.
        /// </summary>
        /// <typeparam name="T">The type of the value to write. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value within the row to write. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <param name="data">The raw value to write.</param>
        /// <remarks>
        /// <para>This method writes the actual raw underlying data. <typeparamref name="T"/> can have any size so you by using this method you can write multiple pixels as well as individual color channels.</para>
        /// <para>To determine the row width in bytes use the <see cref="IBitmapData.RowSize"/> property of the parent <see cref="IReadableBitmapData"/> instance.</para>
        /// <para>To determine the actual pixel size use the <see cref="IBitmapData.PixelFormat"/> property of the parent <see cref="IWritableBitmapData"/> instance.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to write multiple pixels by a single <see cref="WriteRaw{T}">WriteRaw</see> call:
        /// <note>This example requires to reference the <a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a> package. When targeting .NET 7 or later it can be executed on Windows only.</note>
        /// <code lang="C#"><![CDATA[
        /// using (Bitmap bmp4bppIndexed = new Bitmap(8, 1, PixelFormat.Format4bppIndexed))
        /// using (IReadWriteBitmapData bitmapData = bmp4bppIndexed.GetReadWriteBitmapData())
        /// {
        ///     IReadWriteBitmapDataRow row = bitmapData[0];
        ///
        ///     // Writing as uint writes 8 pixels at once in case of a 4 BPP indexed bitmap:
        ///     row.WriteRaw<uint>(0, 0x12345678);
        ///
        ///     // because of little endianness and 4 BPP pixel order the color indices will be printed
        ///     // in the following order: 7, 8, 5, 6, 3, 4, 1, 2
        ///     for (int x = 0; x < bitmapData.Width; x++)
        ///         Console.WriteLine(row.GetColorIndex(x));
        /// }]]></code>
        /// <note type="tip">See also the example at the <strong>Examples</strong> section of the <see cref="IReadableBitmapDataRow.ReadRaw{T}">IReadableBitmapDataRow.ReadRaw</see> method.</note>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or the memory location of the value (considering the size of <typeparamref name="T"/>)
        /// at least partially exceeds the bounds of the current row.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="SetColor"/>
        /// <seealso cref="SetColorIndex"/>
        /// <seealso cref="IReadableBitmapDataRow.ReadRaw{T}"/>
        void WriteRaw<T>(int x, T data) where T : unmanaged;

        #endregion
    }
}