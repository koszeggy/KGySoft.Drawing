#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IWritableBitmapData.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2024 - All Rights Reserved
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
    /// Represents a writable <see cref="IBitmapData"/> instance.
    /// To create an instance use the <see cref="BitmapDataFactory"/> class or the <c>GetWritableBitmapData</c> extension methods for various platform dependent bitmap implementations.
    /// <br/>See the <strong>Remarks</strong> section of the <see cref="N:KGySoft.Drawing"/> namespace for a list about the technologies with dedicated support.
    /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
    /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for all sources.
    /// </summary>
    /// <seealso cref="IReadableBitmapData"/>
    /// <seealso cref="IReadWriteBitmapData"/>
    /// <seealso cref="BitmapDataFactory"/>
    public interface IWritableBitmapData : IBitmapData
    {
        #region Properties and Indexers

        #region Properties

        /// <summary>
        /// Gets an <see cref="IWritableBitmapDataRowMovable"/> instance representing the first row of the current <see cref="IWritableBitmapData"/>.
        /// Subsequent rows can be accessed by calling the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method on the returned instance
        /// while it returns <see langword="true"/>. Alternatively, you can use the <see cref="this">indexer</see> or the <see cref="GetMovableRow">GetMovableRow</see> method to obtain any row.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="GetMovableRow"/>
        IWritableBitmapDataRowMovable FirstRow { get; }

        #endregion

        #region Indexers

        /// <summary>
        /// Gets an <see cref="IWritableBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IWritableBitmapData"/>.
        /// When obtaining the same row repeatedly, then a cached instance is returned. To get a movable row use the <see cref="GetMovableRow">GetMovableRow</see> method instead.
        /// <br/>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for examples.
        /// </summary>
        /// <param name="y">The y-coordinate of the row to obtain.</param>
        /// <returns>An <see cref="IWritableBitmapDataRow"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IWritableBitmapData"/>.</returns>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        IWritableBitmapDataRow this[int y] { get; }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates from a <see cref="Color"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="Color"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.
        /// The <see cref="SetColor32">SetColor32</see> method works with the same range of colors as this one and has a slightly better performance.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// setting the pixels by the <see cref="IWritableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="SetColor32"/>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IWritableBitmapDataRow.SetColor"/>
        void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates from a <see cref="Color32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="Color32"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a non-premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// setting the pixels by the <see cref="IWritableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IReadWriteBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IWritableBitmapDataRow.SetColor32"/>
        void SetColor32(int x, int y, Color32 color);

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates from a <see cref="PColor32"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="PColor32"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a premultiplied color with 8 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// setting the pixels by the <see cref="IWritableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IWritableBitmapDataRow.SetPColor32"/>
        void SetPColor32(int x, int y, PColor32 color);

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates from a <see cref="Color64"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="Color64"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a non-premultiplied color with 16 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// setting the pixels by the <see cref="IWritableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IWritableBitmapDataRow.SetColor64"/>
        void SetColor64(int x, int y, Color64 color);

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates from a <see cref="PColor64"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="PColor64"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a premultiplied color with 16 bits per channel in the sRGB color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// setting the pixels by the <see cref="IWritableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IWritableBitmapDataRow.SetPColor64"/>
        void SetPColor64(int x, int y, PColor64 color);

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates from a <see cref="ColorF"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="ColorF"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a non-premultiplied color with 32 bits per channel in the linear color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// setting the pixels by the <see cref="IWritableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IWritableBitmapDataRow.SetColorF"/>
        void SetColorF(int x, int y, ColorF color);

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates from a <see cref="PColorF"/> value.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel to set.</param>
        /// <param name="y">The y-coordinate of the pixel to set.</param>
        /// <param name="color">A <see cref="PColorF"/> value that represents the color to assign to the specified pixel.</param>
        /// <remarks>
        /// <para>The <paramref name="color"/> parameter represents a premultiplied color with 32 bits per channel in the linear color space, regardless of the underlying <see cref="IBitmapData.PixelFormat"/>.</para>
        /// <para>Line by line processing is also possible by obtaining the first row by the <see cref="FirstRow"/> property,
        /// setting the pixels by the <see cref="IWritableBitmapDataRowMovable"/> members and then moving to the next line by the <see cref="IBitmapDataRowMovable.MoveNextRow">MoveNextRow</see> method.</para>
        /// <para>To access the actual <see cref="IBitmapData.PixelFormat"/>-dependent raw value
        /// obtain a row and use the <see cref="IWritableBitmapDataRow.WriteRaw{T}">WriteRaw</see> method.</para>
        /// <para>If the color to be set cannot be represented precisely by the owner <see cref="IWritableBitmapData"/>, then it will be quantized to a supported color value.</para>
        /// <note>See the <strong>Examples</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">GetReadWriteBitmapData</a> method for an example.</note>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IWritableBitmapDataRow.SetPColorF"/>
        void SetPColorF(int x, int y, PColorF color);

        /// <summary>
        /// If this <see cref="IWritableBitmapData"/> has an indexed pixel format, then sets the color index of the pixel in the current row at the specified pixel.
        /// </summary>
        /// <param name="x">The x-coordinate of the color index to set.</param>
        /// <param name="y">The y-coordinate of the color index to set.</param>
        /// <param name="colorIndex">A palette index that represents the color to be set.</param>
        /// <remarks>
        /// <para>This method can be used only if <see cref="PixelFormatInfo.Indexed"/> is set in the <see cref="IBitmapData.PixelFormat"/> of this <see cref="IWritableBitmapData"/>.
        /// Otherwise, this method throws an <see cref="InvalidOperationException"/>.</para>
        /// <para>To set the actual color of the pixel at the specified coordinates you can use the <c>SetColor...</c>/<c>SetPColor...</c> methods.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Width"/>.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <exception cref="InvalidOperationException">This <see cref="IWritableBitmapData"/> does not have an indexed pixel format.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="IWritableBitmapDataRow.SetColorIndex"/>
        void SetColorIndex(int x, int y, int colorIndex);

        /// <summary>
        /// Sets the underlying raw value within the current <see cref="IWritableBitmapData"/> at the specified coordinates.
        /// </summary>
        /// <typeparam name="T">The type of the value to write. Must be a value type without managed references.</typeparam>
        /// <param name="x">The x-coordinate of the value to write. The valid range depends on the size of <typeparamref name="T"/>.</param>
        /// <param name="y">The y-coordinate of the value to write.</param>
        /// <param name="data">The raw value to write.</param>
        /// <remarks>
        /// <para>This method writes the actual raw underlying data. <typeparamref name="T"/> can have any size so you by using this method you can write multiple pixels as well as individual color channels.</para>
        /// <para>To determine the row width in bytes use the <see cref="IBitmapData.RowSize"/> property of this <see cref="IWritableBitmapData"/> instance.</para>
        /// <para>To determine the actual pixel size use the <see cref="IBitmapData.PixelFormat"/> property of this <see cref="IWritableBitmapData"/> instance.</para>
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to write multiple pixels by a single <see cref="WriteRaw{T}">WriteRaw</see> call:
        /// <note>This example requires to reference the <a href="https://www.nuget.org/packages/KGySoft.Drawing/" target="_blank">KGySoft.Drawing</a> package. When targeting .NET 7 or later it can be executed on Windows only.</note>
        /// <code lang="C#"><![CDATA[
        /// using (Bitmap bmp4bppIndexed = new Bitmap(8, 1, PixelFormat.Format4bppIndexed))
        /// using (IReadWriteBitmapData bitmapData = bmp4bppIndexed.GetReadWriteBitmapData())
        /// {
        ///     // Writing as uint writes 8 pixels at once in case of a 4 BPP indexed bitmap:
        ///     bitmapData.WriteRaw<uint>(0, 0, 0x12345678);
        ///
        ///     // because of little endianness and 4 BPP pixel order the color indices will be printed
        ///     // in the following order: 7, 8, 5, 6, 3, 4, 1, 2
        ///     for (int x = 0; x < bitmapData.Width; x++)
        ///         Console.WriteLine(bitmapData.GetColorIndex(x, 0));
        /// }]]></code>
        /// <note type="tip">See also the example at the <strong>Examples</strong> section of the <see cref="IReadableBitmapData.ReadRaw{T}">IReadableBitmapData.ReadRaw</see> method.</note>
        /// </example>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is less than zero or the calculated offset of the value (considering the size of <typeparamref name="T"/>)
        /// at least partially exceeds the bounds of a row.
        /// <br/>-or-
        /// <br/><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="FirstRow"/>
        /// <seealso cref="this"/>
        /// <seealso cref="SetColorIndex"/>
        /// <seealso cref="IWritableBitmapDataRow.WriteRaw{T}"/>
        /// <seealso cref="IReadableBitmapData.ReadRaw{T}"/>
        void WriteRaw<T>(int x, int y, T data) where T : unmanaged;

        /// <summary>
        /// Gets an <see cref="IWritableBitmapDataRowMovable"/> instance representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IWritableBitmapData"/>.
        /// Unlike the <see cref="this">indexer</see>, this method always allocates a new instance.
        /// </summary>
        /// <param name="y">The y-coordinate of the row to obtain.</param>
        /// <returns>An <see cref="IWritableBitmapDataRowMovable"/> representing the row of the specified <paramref name="y"/> coordinate in the current <see cref="IWritableBitmapData"/>.</returns>
        /// <exception cref="ObjectDisposedException">This <see cref="IWritableBitmapData"/> has already been disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="y"/> is less than zero or is greater than or equal to <see cref="IBitmapData.Height"/>.</exception>
        /// <seealso cref="this"/>
        /// <seealso cref="FirstRow"/>
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
        IWritableBitmapDataRowMovable GetMovableRow(int y);
#else
        IWritableBitmapDataRowMovable GetMovableRow(int y)
        {
            IWritableBitmapDataRowMovable result = FirstRow;
            result.MoveToRow(y);
            return result;
        }
#endif

        #endregion
    }
}
