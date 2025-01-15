#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IReadableBitmapData.cs
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
    /// Represents a readable <see cref="IBitmapData"/> instance.
    /// To create an instance use the <see cref="BitmapDataFactory"/> class or the <c>GetReadableBitmapData</c> extension methods for various platform dependent bitmap implementations.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="N:KGySoft.Drawing"/> namespace for a list about the technologies with dedicated support.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
    /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for all sources.
    /// </summary>
    /// <seealso cref="IWritableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    /// <seealso cref="BitmapDataFactory"/>
    public interface IReadableBitmapData : IBitmapData
    {
        #region Properties and Indexers

        #region Properties

        /// <summary>
        /// Gets an <see cref="IReadableBitmapDataRowMovable"/> instance representing the first row of the current <see cref="IReadableBitmapData"/>.
        /// Subsequent rows can be accessed by calling the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method on the returned instance
        /// while it returns <see langword="true"/>. Alternatively, you can use the <see cref="this">indexer</see> or the <see cref="GetMovableRow">GetMovableRow</see> method to obtain any row.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="GetMovableRow"/>
        IReadableBitmapDataRowMovable FirstRow { get; }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets an <see cref="IReadableBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadableBitmapData"/>.
        /// When obtaining the same row repeatedly, then a cached instance is returned. To get a movable row use the <see cref="GetMovableRow">GetMovableRow</see> method instead.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <param name="y">The y-coordinate of the row to obtain.</param>
        /// <returns>An <see cref="IReadableBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadableBitmapData"/>.</returns>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        IReadableBitmapDataRow this[int y] { get; }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates as a <see cref="Color"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.
        /// The result of the <see cref="GetColor32">GetColor32</see> method represents the same range of colors as <see cref="Color"/> and has a slightly better performance than this method.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by the <see cref="IReadableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value you can use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="GetColor32"/>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.GetColor"/>
        Color GetPixel(int x, int y);

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates as a <see cref="Color32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color32"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by the <see cref="IReadableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value you can use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.GetColor32"/>
        Color32 GetColor32(int x, int y);

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates as a <see cref="PColor32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="PColor32"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by the <see cref="IReadableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value you can use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.GetPColor32"/>
        PColor32 GetPColor32(int x, int y);

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates as a <see cref="Color64"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="Color64"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 16 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by the <see cref="IReadableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value you can use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.GetColor64"/>
        Color64 GetColor64(int x, int y);

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates as a <see cref="PColor64"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="PColor64"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a premultiplied color with 16 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by the <see cref="IReadableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value you can use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.GetPColor64"/>
        PColor64 GetPColor64(int x, int y);

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates as a <see cref="ColorF"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="ColorF"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a non-premultiplied color with 32 bits per channel in the linear color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by the <see cref="IReadableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value you can use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.GetColorF"/>
        ColorF GetColorF(int x, int y);

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates as a <see cref="PColorF"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to retrieve.</param>
        /// <param name="y">The y-coordinate of the pixel to retrieve.</param>
        /// <returns>A <see cref="PColorF"/> instance that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>The returned value is a premultiplied color with 32 bits per channel in the linear color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// getting the pixels by the <see cref="IReadableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value you can use the <see cref="ReadRaw{T}">ReadRaw</see> method.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.GetPColorF"/>
        PColorF GetPColorF(int x, int y);

        /// <summary>
        /// If this <see cref="IReadableBitmapData"/> is an indexed one, then gets the color index of the pixel in the current row of the specified pixel.
        /// </summary>
        /// <param name="x">The x-coordinate of the color index to retrieve.</param>
        /// <param name="y">The y-coordinate of the color index to retrieve.</param>
        /// <returns>A palette index that represents the color of the specified pixel.</returns>
        /// <remarks>
        /// <para>This method can be used only if <see cref="PixelFormatInfo.Indexed"/> is set in the <see cref="IBitmapData.PixelFormat"/> of this <see cref="IReadableBitmapData"/>.
        /// Otherwise, this method throws an <see cref="InvalidOperationException"/>.</para>
        /// <para>To get the actual color of the pixel at the specified coordinates you can use the <c>GetColor...</c>/<c>GetPColor...</c>
        /// methods, or you can call the <see cref="Palette.GetColor">Palette.GetColor</see> method with the return value of this method
        /// on the <see cref="Palette"/> instance returned by the <see cref="IBitmapData.Palette"/> property of this <see cref="IReadableBitmapData"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="IReadableBitmapData"/> does not have an indexed pixel format.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.GetColorIndex"/>
        int GetColorIndex(int x, int y);

        /// <summary>
        /// Gets the underlying raw value within the current <see cref="IReadableBitmapData"/> at the specified coordinates.
        /// </summary>
        /// <typeparam name="T">The type of the value to return. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value to retrieve. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <param name="y">The y-coordinate of the value to retrieve.</param>
        /// <returns>The raw value within the current <see cref="IReadableBitmapData"/> at the specified coordinates.</returns>
        /// <remarks>
        /// <para>This method returns the actual raw underlying data as arbitrary unmanaged value type (a value type is unmanaged if contains no managed references).
        /// <typeparamref name="T"/> can have any size so this method can access multiple pixels or individual color channels.</para>
        /// <para>To determine the row width in bytes use the <see cref="IBitmapData.RowSize"/> property of the parent <see cref="IReadableBitmapData"/> instance.</para>
        /// <para>To determine the actual pixel size use the <see cref="IBitmapData.PixelFormat"/> property of the parent <see cref="IReadableBitmapData"/> instance.</para>
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
        /// Console.WriteLine(bitmapData.ReadRaw<Color32>(0, 0)); // 80808080 [A=128; R=128; G=128; B=128]
        ///
        /// // but reading it by the GetColor32 method transforms the color back:
        /// Console.WriteLine(bitmapData.GetColor32(0, 0); // 80FFFFFF [A=128; R=255; G=255; B=255]]]></code>
        /// <note type="tip">See also the example at the <strong>Examples</strong> section of the <see cref="IWritableBitmapData.WriteRaw{T}">IWritableBitmapData.WriteRaw</see> method.</note>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or the calculated offset of the value (considering the size of <typeparamref name="T"/>)
        /// at least partially exceeds the bounds of a row.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IReadableBitmapDataRow.ReadRaw{T}"/>
        /// <seealso cref="IWritableBitmapData.WriteRaw{T}"/>
        T ReadRaw<T>(int x, int y) where T : unmanaged;

        /// <summary>
        /// Gets an <see cref="IReadableBitmapDataRowMovable"/> instance representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadableBitmapData"/>.
        /// Unlike the <see cref="this">indexer</see>, this method always allocates a new instance.
        /// </summary>
        /// <param name="y">The y-coordinate of the row to obtain.</param>
        /// <returns>An <see cref="IReadableBitmapDataRowMovable"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IReadableBitmapData"/>.</returns>
        /// <exception cref="ObjectDisposedException">This <see cref="IReadableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="FirstRow"/>
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
        IReadableBitmapDataRowMovable GetMovableRow(int y);
#else
        IReadableBitmapDataRowMovable GetMovableRow(int y)
        {
            IReadableBitmapDataRowMovable result = FirstRow;
            result.MoveToRow(y);
            return result;
        }
#endif

        #endregion
    }
}
