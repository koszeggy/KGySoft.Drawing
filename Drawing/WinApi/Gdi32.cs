using System;
using System.Runtime.InteropServices;

namespace KGySoft.Drawing.WinApi
{
    /// <summary>
    /// Contains external methods for Gdi32.dll
    /// </summary>
    internal static class Gdi32
    {
        /// <summary>
        /// The GetObject function retrieves information for the specified graphics object.
        /// </summary>
        /// <param name="hgdiobj">A handle to the graphics object of interest. This can be a handle to one of the following: a logical bitmap, a brush, a font, a palette, a pen, or a device independent bitmap created by calling the <see cref="CreateDIBSection"/> function.</param>
        /// <param name="cbBuffer">The number of bytes of information to be written to the buffer.</param>
        /// <param name="lpvObject">A pointer to a buffer that receives the information about the specified graphics object.</param>
        /// <returns>If the lpvObject parameter is NULL, the function return value is the number of bytes required to store the information it writes to the buffer for the specified graphics object.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern int GetObject(IntPtr hgdiobj, int cbBuffer, out BITMAP lpvObject);

        /// <summary>
        /// The DeleteObject function deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system resources associated with the object. After the object is deleted, the specified handle is no longer valid.
        /// </summary>
        /// <param name="hObject">A handle to a logical pen, brush, font, bitmap, region, or palette.</param>
        /// <returns>If the function succeeds, the return value is nonzero.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// The CreateDIBSection function creates a DIB that applications can write to directly. The function gives you a pointer to the location of the bitmap bit values. You can supply a handle to a file-mapping object that the function will use to create the bitmap, or you can let the system allocate the memory for the bitmap.
        /// </summary>
        /// <param name="hdc">A handle to a device context. If the value of iUsage is DIB_PAL_COLORS, the function uses this device context's logical palette to initialize the DIB colors.</param>
        /// <param name="pbmi">A pointer to a <see cref="BITMAPINFO"/> structure that specifies various attributes of the DIB, including the bitmap dimensions and colors.</param>
        /// <param name="iUsage">The type of data contained in the bmiColors array member of the BITMAPINFO structure pointed to by pbmi (either logical palette indexes or literal RGB values). The following values are defined.
        /// <para>DIB_PAL_COLORS - The bmiColors member is an array of 16-bit indexes into the logical palette of the device context specified by hdc.</para>
        /// <para>DIB_RGB_COLORS - The BITMAPINFO structure contains an array of literal RGB values.</para>
        /// </param>
        /// <param name="ppvBits">A pointer to a variable that receives a pointer to the location of the DIB bit values.</param>
        /// <param name="hSection">A handle to a file-mapping object that the function will use to create the DIB. This parameter can be NULL.</param>
        /// <param name="dwOffset">The offset from the beginning of the file-mapping object referenced by hSection where storage for the bitmap bit values is to begin. This value is ignored if hSection is NULL. The bitmap bit values are aligned on doubleword boundaries, so dwOffset must be a multiple of the size of a DWORD.</param>
        /// <returns>If the function succeeds, the return value is a handle to the newly created DIB, and *ppvBits points to the bitmap bit values.
        /// If the function fails, the return value is NULL, and ppvBits is NULL.</returns>
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, int iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        /// <summary>
        /// This function creates a memory device context (DC) compatible with the specified device. 
        /// </summary>
        /// <param name="hdc">[in] Handle to an existing device context.
        /// If this handle is NULL, the function creates a memory device context compatible with the application's current screen. </param>
        /// <returns>The handle to a memory device context indicates success.
        /// NULL indicates failure.
        /// To get extended error information, call GetLastError.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        /// <summary>
        /// The SelectObject function selects an object into the specified device context (DC). The new object replaces the previous object of the same type.
        /// </summary>
        /// <param name="hdc">A handle to the DC.</param>
        /// <param name="hgdiobj">A handle to the object to be selected.</param>
        /// <returns>If the selected object is not a region and the function succeeds, the return value is a handle to the object being replaced.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        /// <summary>The BitBlt function performs a bit-block transfer of the color data corresponding to a rectangle of pixels from the specified source device context into a destination device context.</summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns><see langword="true"/>&#160;if the operation succeedes, <see langword="false"/>&#160;otherwise. To get extended error information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        /// <summary>The DeleteDC function deletes the specified device context (DC).</summary>
        /// <param name="hdc">A handle to the device context.</param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the function fails, the return value is <see langword="false"/>.</returns>
        /// <remarks>
        /// An application must not delete a DC whose handle was obtained by calling the GetDC function. Instead, it must call the ReleaseDC function to free the DC.
        /// </remarks>
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteDC(IntPtr hdc);

        /// <summary>
        /// The GetEnhMetaFileBits function retrieves the contents of the specified enhanced-format metafile and copies them into a buffer.
        /// </summary>
        /// <param name="hemf">A handle to the enhanced metafile.</param>
        /// <param name="cbBuffer">The size, in bytes, of the buffer to receive the data.</param>
        /// <param name="lpbBuffer">A pointer to a buffer that receives the metafile data. The buffer must be sufficiently large to contain the data. If lpbBuffer is NULL, the function returns the size necessary to hold the data.</param>
        /// <returns>If the function succeeds and the buffer pointer is NULL, the return value is the size of the enhanced metafile, in bytes.
        /// If the function succeeds and the buffer pointer is a valid pointer, the return value is the number of bytes copied to the buffer.
        /// If the function fails, the return value is zero.</returns>
        /// <remarks>After the enhanced-metafile bits are retrieved, they can be used to create a memory-based metafile by calling the SetEnhMetaFileBits function.
        /// The GetEnhMetaFileBits function does not invalidate the enhanced-metafile handle. The application must call the DeleteEnhMetaFile function to delete the handle when it is no longer needed.
        /// The metafile contents retrieved by this function are in the enhanced format. To retrieve the metafile contents in the Windows format, use the GetWinMetaFileBits function.</remarks>
        [DllImport("gdi32.dll")]
        internal static extern uint GetEnhMetaFileBits(IntPtr hemf, uint cbBuffer, [Out]byte[] lpbBuffer);

        /// <summary>
        /// The GetWinMetaFileBits function converts the enhanced-format records from a metafile into Windows-format records and stores the converted records in the specified buffer.
        /// </summary>
        /// <param name="hemf">A handle to the enhanced metafile.</param>
        /// <param name="cbBuffer">The size, in bytes, of the buffer into which the converted records are to be copied.</param>
        /// <param name="lpbBuffer">A pointer to the buffer that receives the converted records. If lpbBuffer is NULL, GetWinMetaFileBits returns the number of bytes required to store the converted metafile records.</param>
        /// <param name="fnMapMode">The mapping mode to use in the converted metafile.</param>
        /// <param name="hdcRef">A handle to the reference device context.</param>
        /// <returns>If the function succeeds and the buffer pointer is NULL, the return value is the number of bytes required to store the converted records; if the function succeeds and the buffer pointer is a valid pointer, the return value is the size of the metafile data in bytes.
        /// If the function fails, the return value is zero.</returns>
        /// <remarks>This function converts an enhanced metafile into a Windows-format metafile so that its picture can be displayed in an application that recognizes the older format.
        /// The system uses the reference device context to determine the resolution of the converted metafile.
        /// The GetWinMetaFileBits function does not invalidate the enhanced metafile handle. An application should call the DeleteEnhMetaFile function to release the handle when it is no longer needed.
        /// To create a scalable Windows-format metafile, specify MM_ANISOTROPIC as the fnMapMode parameter.
        /// The upper-left corner of the metafile picture is always mapped to the origin of the reference device.</remarks>
        [DllImport("gdi32.dll")]
        internal static extern uint GetWinMetaFileBits(IntPtr hemf, uint cbBuffer, [Out]byte[] lpbBuffer, MappingModes fnMapMode, IntPtr hdcRef);

        /// <summary>
        /// The GetMetaFileBitsEx function retrieves the contents of a Windows-format metafile and copies them into the specified buffer.
        /// </summary>
        /// <param name="hmf">A handle to a Windows-format metafile.</param>
        /// <param name="nSize">The size, in bytes, of the buffer to receive the data.</param>
        /// <param name="lpvData">A pointer to a buffer that receives the metafile data. The buffer must be sufficiently large to contain the data. If lpvData is NULL, the function returns the number of bytes required to hold the data.</param>
        /// <returns>If the function succeeds and the buffer pointer is NULL, the return value is the number of bytes required for the buffer; if the function succeeds and the buffer pointer is a valid pointer, the return value is the number of bytes copied.
        /// If the function fails, the return value is zero.</returns>
        /// <remarks>
        /// Note: This function is provided only for compatibility with Windows-format metafiles. Enhanced-format metafiles provide superior functionality and are recommended for new applications. The corresponding function for an enhanced-format metafile is GetEnhMetaFileBits.
        /// After the Windows-metafile bits are retrieved, they can be used to create a memory-based metafile by calling the SetMetaFileBitsEx function.
        /// The GetMetaFileBitsEx function does not invalidate the metafile handle. An application must delete this handle by calling the DeleteMetaFile function.
        /// To convert a Windows-format metafile into an enhanced-format metafile, use the SetWinMetaFileBits function.</remarks>
        [DllImport("gdi32.dll")]
        internal static extern uint GetMetaFileBitsEx(IntPtr hmf, uint nSize, [Out]byte[] lpvData);

        /// <summary>
        /// The DeleteEnhMetaFile function deletes an enhanced-format metafile or an enhanced-format metafile handle.
        /// </summary>
        /// <param name="hemf">A handle to an enhanced metafile.</param>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.</returns>
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteEnhMetaFile(IntPtr hemf);

        /// <summary>
        /// Creates a bitmap compatible with the device that is associated with the specified device context.
        /// </summary>
        /// <param name="hdc">A handle to a device context.</param>
        /// <param name="nWidth">The bitmap width, in pixels.</param>
        /// <param name="nHeight">The bitmap height, in pixels.</param>
        /// <returns>If the function succeeds, the return value is a handle to the compatible bitmap (DDB).
        /// If the function fails, the return value is <see cref="System.IntPtr.Zero"/>.</returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
    }
}
