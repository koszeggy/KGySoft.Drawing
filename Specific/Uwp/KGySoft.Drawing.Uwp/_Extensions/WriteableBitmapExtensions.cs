#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: WriteableBitmapExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2022 - All Rights Reserved
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
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <returns>An <see cref="IReadableBitmapData"/> instance, which provides fast read-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData"/>
        /// <seealso cref="GetWritableBitmapData"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, Color32, byte)"/>
        public static IReadableBitmapData GetReadableBitmapData(this WriteableBitmap bitmap) => DoGetBitmapData(bitmap, true);

        /// <summary>
        /// Gets a managed read-write accessor for a <see cref="WriteableBitmap"/> instance.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <returns>An <see cref="IReadWriteBitmapData"/> instance, which provides fast read-write access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData"/>
        /// <seealso cref="GetWritableBitmapData"/>
        /// <seealso cref="BitmapDataFactory.CreateBitmapData(Size, KnownPixelFormat, Color32, byte)"/>
        public static IReadWriteBitmapData GetReadWriteBitmapData(this WriteableBitmap bitmap) => DoGetBitmapData(bitmap, false);

        /// <summary>
        /// Gets a managed write-only accessor for a <see cref="WriteableBitmap"/> instance.
        /// </summary>
        /// <param name="bitmap">A <see cref="WriteableBitmap"/> instance, whose data is about to be accessed.</param>
        /// <returns>An <see cref="IWritableBitmapData"/> instance, which provides fast write-only access to the actual data of the specified <paramref name="bitmap"/>.</returns>
        /// <seealso cref="GetReadableBitmapData"/>
        /// <seealso cref="GetReadWriteBitmapData"/>
        public static IWritableBitmapData GetWritableBitmapData(this WriteableBitmap bitmap) => DoGetBitmapData(bitmap, false);

        #endregion

        #region Private Methods

        private static IReadWriteBitmapData DoGetBitmapData(WriteableBitmap bitmap, bool readOnly)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap), PublicResources.ArgumentNull);

            var size = new Size(bitmap.PixelWidth, bitmap.PixelHeight);

            // Alert: this is a COM object so all of its members use "remoting".
            IBuffer nativeBuffer = bitmap.PixelBuffer;

            // Just because it can use array pooling if referenced from a project that can target .NET Standard 2.1
            ArraySection<byte> managedBuffer = new ArraySection<byte>((int)nativeBuffer.Length, false);
            try
            {
                nativeBuffer.CopyTo(managedBuffer.UnderlyingArray);

                // UWP's WriteableBitmap is really simple: it always uses the premultiplied ARGB32 format
                return BitmapDataFactory.CreateBitmapData(managedBuffer, size, size.Width << 2, KnownPixelFormat.Format32bppPArgb,
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

        #endregion

        #endregion
    }
}
