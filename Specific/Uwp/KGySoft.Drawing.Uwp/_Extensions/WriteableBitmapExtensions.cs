#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WriteableBitmapExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#nullable enable

#region Usings

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

using KGySoft.Collections;
using KGySoft.Drawing.Imaging;

using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

#endregion

namespace KGySoft.Drawing.Uwp
{
    /// <summary>
    /// Contains extension methods for the <see cref="WriteableBitmap"/> type.
    /// </summary>
    public static class WriteableBitmapExtensions
    {
        #region Methods

        #region Public Methods

        /// <summary>
        /// Gets a managed read-only accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetWritableBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this WriteableBitmap bitmap) => DoGetBitmapData(bitmap, true, WorkingColorSpace.Default);

        /// <summary>
        /// Gets a managed read-only accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data. Determines the behavior
        /// of some operations such as resizing or cloning using a specific pixel format with no transparency support.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetWritableBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this WriteableBitmap bitmap, WorkingColorSpace workingColorSpace)
            => DoGetBitmapData(bitmap, true, workingColorSpace);

        /// <summary>
        /// Gets a managed write-only accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this WriteableBitmap bitmap) => DoGetBitmapData(bitmap, false, WorkingColorSpace.Default);

        /// <summary>
        /// Gets a managed write-only accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data.
        /// Determines the <see cref="IBitmapData.WorkingColorSpace"/> property of the result.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="GetReadWriteBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IWritableBitmapData GetWritableBitmapData(this WriteableBitmap bitmap, WorkingColorSpace workingColorSpace)
            => DoGetBitmapData(bitmap, false, workingColorSpace);

        /// <summary>
        /// Gets a managed read-write accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="GetWritableBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this WriteableBitmap bitmap) => DoGetBitmapData(bitmap, false, WorkingColorSpace.Default);

        /// <summary>
        /// Gets a managed read-write accessor for a <see cref="WriteableBitmap"/> instance.
        /// <br/>See the <strong>Remarks</strong> section of the <a href="https://docs.kgysoft.net/drawing/html/M_KGySoft_Drawing_BitmapExtensions_GetReadWriteBitmapData.htm">BitmapExtensions.GetReadWriteBitmapData</a>
        /// method for details and code samples. That method is for the GDI+ <a href="https://docs.microsoft.com/en-us/dotnet/api/system.drawing.bitmap" target="_blank">Bitmap</a> type but the main principles apply for this method, too.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <param name="workingColorSpace">Specifies the preferred color space that should be used when working with the result bitmap data. Determines the behavior
        /// of some operations such as drawing another bitmap into the returned instance by the <see cref="O:KGySoft.Drawing.Imaging.BitmapDataExtensions.DrawInto">DrawInto</see> methods.
        /// <br/>See the <strong>Remarks</strong> section of the <see cref="WorkingColorSpace"/> enumeration for more details.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="GetWritableBitmapData(WriteableBitmap, WorkingColorSpace)"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, WorkingColorSpace, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this WriteableBitmap bitmap, WorkingColorSpace workingColorSpace)
            => DoGetBitmapData(bitmap, false, workingColorSpace);

        #endregion

        #region Private Methods

        private static IReadWriteBitmapData DoGetBitmapData(WriteableBitmap bitmap, bool readOnly, WorkingColorSpace workingColorSpace)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);
            if (workingColorSpace is < WorkingColorSpace.Default or > WorkingColorSpace.Srgb)
                throw new ArgumentOutOfRangeException(nameof(workingColorSpace), PublicResources.EnumOutOfRange(workingColorSpace));

            try
            {
                var size = new Size(bitmap.PixelWidth, bitmap.PixelHeight);
                IBuffer nativeBuffer = bitmap.PixelBuffer;

                // Just because it can use array pooling if referenced from a project that can target .NET Standard 2.1
                ArraySection<byte> managedBuffer = new ArraySection<byte>((int)nativeBuffer.Length, false);
                try
                {
                    nativeBuffer.CopyTo(managedBuffer.UnderlyingArray);

                    // UWP's WriteableBitmap is really simple: it always uses the premultiplied ARGB32 format
                    return BitmapDataFactory.CreateBitmapData(managedBuffer, size, size.Width << 2, KnownPixelFormat.Format32bppPArgb, workingColorSpace,
                        disposeCallback: () =>
                        {
                            if (!readOnly)
                            {
                                nativeBuffer.AsStream().Write(managedBuffer.UnderlyingArray!, 0, managedBuffer.Length);
                                bitmap.Invalidate();
                            }

                            managedBuffer.Release();
                        });
                }
                catch (Exception)
                {
                    managedBuffer.Release();
                    throw;
                }
            }
            catch (Exception e) when (e.GetType() == typeof(Exception) && e.HResult != 0)
            {
                // Converting possible non-derived Exceptions to specific ones if possible. Can happen because UWP uses lots of wrapped COM objects.
                throw Marshal.GetExceptionForHR(e.HResult) ?? e;
            }
        }

        #endregion

        #endregion
    }
}
