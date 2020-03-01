#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadableBitmapDataRow.cs
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

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;

#endregion

namespace KGySoft.Drawing.Imaging
{
    /// <summary>
    /// Provides a fast read-only access to a single row of the actual data of a <see cref="Bitmap"/>. The owner <see cref="Bitmap"/> can have any <see cref="PixelFormat"/>.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for details and examples.
    /// </summary>
    /// <seealso cref="IWritableBitmapDataRow"/>
    /// <seealso cref="IReadWriteBitmapDataRow"/>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="BitmapExtensions.GetReadableBitmapData"/>
    public interface IReadableBitmapDataRow : IBitmapDataRow
    {
        #region Indexers

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color32"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>To return a <see cref="Color"/> structure you can use also the <see cref="GetColor">GetColor</see> method but this member has a slightly better performance.</para>
        /// <para>The returned value represents a straight (non-premultiplied) color with gamma correction γ = 2.2,
        /// regardless of the underlying <see cref="PixelFormat"/>. To access the actual <see cref="PixelFormat"/>-dependent raw value
        /// use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// <note>See the <strong>Examples</strong> section of the <see cref="BitmapExtensions.GetReadWriteBitmapData">GetReadWriteBitmapData</see> method for examples.</note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="GetColor"/>
        /// <seealso cref="GetColorIndex"/>
        /// <seealso cref="ReadRaw{T}"/>
        Color32 this[int x] { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the color of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>If you don't really need to retrieve a 20 byte wide <see cref="Color"/> structure (16 bytes on 32-bit targets), then you can use the
        /// <see cref="this">indexer</see> for a slightly better performance, which returns a more compact 4-byte <see cref="Color32"/> structure.</para>
        /// <para>The returned value represents a straight (non-premultiplied) color with gamma correction γ = 2.2,
        /// regardless of the underlying <see cref="PixelFormat"/>. To access the actual <see cref="PixelFormat"/>-dependent raw value
        /// use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="GetColorIndex"/>
        /// <seealso cref="ReadRaw{T}"/>
        Color GetColor(int x);

        /// <summary>
        /// If the owner <see cref="Bitmap"/> is an indexed one, then gets the color index of the pixel in the current row at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details.
        /// </summary>
        /// <param name="x">The x-coordinate of the color index to retrieve.</param>
        /// <returns>A palette index that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>This method can be used only if the <see cref="IBitmapData.PixelFormat"/> property of the parent <see cref="IReadableBitmapData"/> returns an indexed format
        /// (which are <see cref="PixelFormat.Format8bppIndexed"/>, <see cref="PixelFormat.Format4bppIndexed"/> and <see cref="PixelFormat.Format1bppIndexed"/>).
        /// Otherwise, this method throws an <see cref="InvalidOperationException"/>.</para>
        /// <para>To get the actual color of the pixel at the <paramref name="x"/> coordinate you can use the <see cref="GetColor">GetColor</see> method,
        /// the <see cref="this">indexer</see>, or you can call the <see cref="Palette.GetColor">Palette.GetColor</see> method with the return value of this method
        /// on the <see cref="Palette"/> instance returned by the <see cref="IBitmapData.Palette"/> property of the parent <see cref="IReadableBitmapData"/>.</para>
        /// <note>For information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to the <see cref="IBitmapData.Width"/> of the parent <see cref="IReadableBitmapData"/>.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="IReadableBitmapDataRow"/> does not belong to a row of an indexed <see cref="Bitmap"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="GetColor"/>
        /// <seealso cref="ReadRaw{T}"/>
        int GetColorIndex(int x);

        /// <summary>
        /// Gets the underlying raw value within the current <see cref="IReadableBitmapDataRow"/> at the specified <paramref name="x"/> coordinate.
        /// <br/>See the <strong>Remarks</strong> section for details and an example.
        /// </summary>
        /// <typeparam name="T">The type of the value to return. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value within the row to retrieve. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <returns>The raw value within the current <see cref="IReadableBitmapDataRow"/> at the specified <paramref name="x"/> coordinate.</returns>
        /// <remarks>
        /// <para>This method returns the actual raw underlying data as arbitrary unmanaged value type (a value type is unmanaged if contains no managed references).
        /// <typeparamref name="T"/> can have any size so you using this method can access multiple pixels or individual color channels.</para>
        /// <para>To determine the actual pixel size use the <see cref="IBitmapData.PixelFormat"/> property of the parent <see cref="IReadableBitmapData"/> instance.</para>
        /// <note>Please note that on some non-Windows platforms the <see cref="IBitmapData.PixelFormat">IBitmapData.PixelFormat</see> property may return a different format than
        /// the <see cref="Image.PixelFormat">Image.PixelFormat</see> property of the original <see cref="Bitmap"/>.
        /// <br/>For details and further information about the possible usable <see cref="PixelFormat"/>s on different platforms see the <strong>Remarks</strong> section of the <see cref="ImageExtensions.ConvertPixelFormat(Image,PixelFormat,Color,byte)">ConvertPixelFormat</see> method.</note>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to access the premultiplied color values of a <see cref="Bitmap"/> with premultiplied pixel format:
        /// <code lang="C#"><![CDATA[
        /// using (Bitmap bmpPremultiplied = new Bitmap(1, 1, PixelFormat.Format32bppPArgb))
        /// using (IReadWriteBitmapData bitmapData = bmpPremultiplied.GetReadWriteBitmapData())
        /// {
        ///     // setting a white pixel with 50% alpha:
        ///     bitmapData.SetPixel(0, 0, Color.FromArgb(128, 255, 255, 255));
        ///
        ///     // reading the raw premultiplied color value:
        ///     Console.WriteLine(bitmapData[0].ReadRaw<Color32>(0)); // 80808080 [A=128; R=128; G=128; B=128]
        ///
        ///     // but reading it by the indexer (or by GetPixel/GetColor) transforms the color back:
        ///     Console.WriteLine(bitmapData[0][0]); // 80FFFFFF [A=128; R=255; G=255; B=255]
        /// }]]></code>
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