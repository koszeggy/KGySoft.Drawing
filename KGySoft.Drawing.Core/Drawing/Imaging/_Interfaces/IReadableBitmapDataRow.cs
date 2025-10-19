#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadableBitmapDataRow.cs
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
using System.Drawing;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides fast read-only access to a single row of an <see cref="IReadableBitmapData"/>.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for details and examples.
    /// </summary>
    /// <seealso cref="IWritableBitmapDataRow"/>
    /// <seealso cref="IReadWriteBitmapDataRow"/>
    /// <seealso cref="IReadableBitmapData"/>
    public interface IReadableBitmapDataRow : IBitmapDataRow
    {
        #region Indexers

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate as a <see cref="Color32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color32"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.
        /// This member is practically the same as the <see cref="GetColor32">GetColor32</see> method.</para>
        /// <para>To retrieve the color in other color formats use the <c>GetColor...</c>/<c>GetPColor...</c> methods.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="GetColor32"/>
        /// <seealso cref="GetColorIndex"/>
        /// <seealso cref="ReadRaw{T}"/>
        Color32 this[int x] { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate as a <see cref="Color"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.
        /// The result of the <see cref="this">indexer</see> and the <see cref="GetColor32">GetColor32</see> method represent the same range of colors as <see cref="Color"/>
        /// and have a slightly better performance than this method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="GetColor32"/>
        /// <seealso cref="GetColorIndex"/>
        /// <seealso cref="ReadRaw{T}"/>
        Color GetColor(int x);

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate as a <see cref="Color32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color32"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.
        /// This method is practically the same as the <see cref="this">indexer</see>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="GetColorIndex"/>
        /// <seealso cref="ReadRaw{T}"/>
        Color32 GetColor32(int x);

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate as a <see cref="PColor32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="PColor32"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="ReadRaw{T}"/>
        PColor32 GetPColor32(int x);

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate as a <see cref="Color64"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color64"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 16 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="ReadRaw{T}"/>
        Color64 GetColor64(int x);

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate as a <see cref="PColor64"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="PColor64"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a premultiplied color with 16 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="ReadRaw{T}"/>
        PColor64 GetPColor64(int x);

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate as a <see cref="ColorF"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="ColorF"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 32 bits per channel in the linear color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="ReadRaw{T}"/>
        ColorF GetColorF(int x);

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate as a <see cref="PColorF"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="PColorF"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a premultiplied color with 32 bits per channel in the linear color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="ReadRaw{T}"/>
        PColorF GetPColorF(int x);

        /// <summary>
        /// If the owner <see cref="IReadableBitmapData"/> is an indexed one, then gets the color index of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// </summary>
        /// <param name="x">The x-coordinate of the color index to retrieve.</param>
        /// <returns>A palette index that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>This method can be used only if <see cref="PixelFormatInfo.Indexed"/> is set in the <see cref="IBitmapData.PixelFormat"/> of the parent <see cref="IReadableBitmapData"/>.
        /// Otherwise, this method throws an <see cref="InvalidOperationException"/>.</para>
        /// <para>To get the actual color of the pixel at the <paramref name="x"/> coordinate you can use the <c>GetColor...</c>/<c>GetPColor...</c>
        /// methods or the <see cref="this">indexer</see>, or you can call the <see cref="Palette.GetColor">Palette.GetColor</see> method with the return value of this method
        /// on the <see cref="Palette"/> instance returned by the <see cref="IBitmapData.Palette"/> property of the parent <see cref="IReadableBitmapData"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="IReadableBitmapDataRow"/> does not belong to a row of an indexed <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="GetColor"/>
        /// <seealso cref="ReadRaw{T}"/>
        int GetColorIndex(int x);

        /// <summary>
        /// Gets the underlying raw value within the current <see cref="IReadableBitmapDataRow"/> at the specified <paramref name="x"/> coordinate.
        /// <div style="display: none;"><br/>See the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_Imaging_IReadableBitmapDataRow_ReadRaw__1.htm">online help</a> for an example.</div>
        /// </summary>
        /// <typeparam name="T">The type of the value to return. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value within the row to retrieve. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <returns>The raw value within the current <see cref="IReadableBitmapDataRow"/> at the specified <paramref name="x"/> coordinate.</returns>
        /// <remarks>
        /// <para>This method returns the actual raw underlying data as arbitrary unmanaged value type (a value type is unmanaged if contains no managed references).
        /// <typeparamref name="T"/> can have any size so this method can access multiple pixels or individual color channels.</para>
        /// <para>To determine the row width in bytes use the <see cref="IBitmapData.RowSize"/> property of the parent <see cref="IReadableBitmapData"/> instance.</para>
        /// <para>To determine the actual pixel size use the <see cref="IBitmapData.PixelFormat"/> property of the parent <see cref="IReadableBitmapData"/> instance.</para>
        /// <note type="caution">If <typeparamref name="T"/> is a primitive type of size greater than 1 byte (for example <see cref="int">int</see>, <see cref="float">float</see>, etc.),
        /// then the address of the result should be aligned to the size of <typeparamref name="T"/>. On most platforms misalignment affects only performance,
        /// but depending on the architecture, an unexpected result may be returned, or even a <see cref="DataMisalignedException"/> can be thrown.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to access the premultiplied color values of a bitmap data with premultiplied pixel format:
        /// <code lang="C#"><![CDATA[
        /// using IReadWriteBitmapData bitmapData = BitmapDataFactory.CreateBitmapData(1, 1, KnownPixelFormat.Format32bppPArgb);
        /// 
        /// // setting a white pixel with 50% alpha:
        /// bitmapData.SetPixel(0, 0, Color.FromArgb(128, 255, 255, 255));
        ///
        /// // reading the raw premultiplied color value:
        /// Console.WriteLine(bitmapData[0].ReadRaw<Color32>(0)); // 80808080 [A=128; R=128; G=128; B=128]
        ///
        /// // but reading it by the indexer (or by GetPixel/GetColor) transforms the color back:
        /// Console.WriteLine(bitmapData[0][0]); // 80FFFFFF [A=128; R=255; G=255; B=255]]]></code>
        /// <note type="tip">See also the example at the <strong>Examples</strong> section of the <see cref="IWritableBitmapDataRow.WriteRaw{T}">IWritableBitmapDataRow.WriteRaw</see> method.</note>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or the memory location of the value (considering the size of <typeparamref name="T"/>)
        /// at least partially exceeds the bounds of the current row.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="GetColor"/>
        /// <seealso cref="GetColorIndex"/>
        /// <seealso cref="IWritableBitmapDataRow.WriteRaw{T}"/>
        T ReadRaw<T>(int x) where T : unmanaged;

        #endregion
    }
}