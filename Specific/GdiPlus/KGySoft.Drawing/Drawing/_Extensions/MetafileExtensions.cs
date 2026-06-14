#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MetafileExtensions.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
#if NET
using System.Runtime.Versioning;
#endif
using System.Security;

using KGySoft.CoreLibraries;
using KGySoft.Drawing.WinApi;
using KGySoft.Serialization.Binary;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Contains extension methods for the <see cref="Metafile"/> type.
    /// </summary>
    /// <remarks>
    /// <note>When targeting .NET 7.0 or later versions this class is supported on Windows only.</note>
    /// </remarks>
#if NET7_0_OR_GREATER
    [SupportedOSPlatform("windows")] 
#endif
    public static class MetafileExtensions
    {
        #region WmfHeader struct

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable", Justification = "Serialized structure")]
        private struct WmfHeader
        {
            #region Fields

            /// <summary>
            /// Magic number (always 9AC6CDD7h)
            /// </summary>
            private readonly uint key;

            /// <summary>
            /// Metafile HANDLE number (always 0)
            /// </summary>
            private readonly ushort handle;

            /// <summary>
            /// Left coordinate in metafile units
            /// </summary>
            private readonly short left;

            /// <summary>
            /// Top coordinate in metafile units
            /// </summary>
            private readonly short top;

            /// <summary>
            /// Right coordinate in metafile units
            /// </summary>
            private readonly short right;

            /// <summary>
            /// Bottom coordinate in metafile units
            /// </summary>
            private readonly short bottom;

            /// <summary>
            /// Number of metafile units per inch
            /// </summary>
            private readonly ushort inch;

            /// <summary>
            /// Reserved (always 0)
            /// </summary>
            private readonly uint reserved;

            /// <summary>
            /// Checksum value for previous 10 WORDs
            /// </summary>
            private readonly ushort checksum;

            #endregion

            #region Constructors

            internal WmfHeader(short width, short height, ushort dpi)
                : this()
            {
                key = 0x9AC6CDD7;
                right = width;
                bottom = height;
                inch = dpi;
                checksum = (ushort)(key & 0x0000FFFF);
                checksum ^= (ushort)(key >> 16);
                checksum ^= (ushort)right;
                checksum ^= (ushort)bottom;
                checksum ^= inch;
            }

            #endregion
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Creates a <see cref="Bitmap"/> of a <see cref="Metafile"/> instance specified in the <paramref name="metafile"/> parameter.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> to convert.</param>
        /// <param name="requestedSize">The requested size of the result <see cref="Bitmap"/>. This overload does not maintain aspect ratio.</param>
        /// <param name="antiAliased"><see langword="true"/> to create an anti-aliased result; otherwise, <see langword="false"/>. This parameter is optional.
        /// <br/>Default value: <see langword="false"/>.</param>
        /// <returns>A <see cref="Bitmap"/> instance of the requested size.</returns>
        /// <remarks>
        /// <para>If the source <paramref name="metafile"/> contains already anti-aliased records, setting <paramref name="antiAliased"/> to <see langword="true"/>
        /// will not improve the result. It also does not really help if the <paramref name="metafile"/> contains only bitmap drawing records.</para>
        /// <para>If the <paramref name="requestedSize"/> is very large, the conversion may omit anti-aliasing.</para>
        /// </remarks>
        public static Bitmap ToBitmap(this Metafile metafile, Size requestedSize, bool antiAliased = false) => ToBitmap(metafile, requestedSize, antiAliased, false);

        /// <summary>
        /// Creates a <see cref="Bitmap"/> of a <see cref="Metafile"/> instance specified in the <paramref name="metafile"/> parameter.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> to convert.</param>
        /// <param name="requestedSize">The requested size of the result <see cref="Bitmap"/>.</param>
        /// <param name="antiAliased"><see langword="true"/> to create an anti-aliased result; otherwise, <see langword="false"/>.</param>
        /// <param name="keepAspectRatio"><see langword="true"/> to keep aspect ratio of the source <paramref name="metafile"/>; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="Bitmap"/> instance of the requested size.</returns>
        /// <remarks>
        /// <para>If the source <paramref name="metafile"/> contains already anti-aliased records, setting <paramref name="antiAliased"/> to <see langword="true"/>
        /// will not improve the result. It also does not really help if the <paramref name="metafile"/> contains only bitmap drawing records.</para>
        /// <para>If the <paramref name="requestedSize"/> is very large, the conversion may omit anti-aliasing.</para>
        /// </remarks>
        public static Bitmap ToBitmap(this Metafile metafile, Size requestedSize, bool antiAliased, bool keepAspectRatio)
        {
            if (metafile == null)
                throw new ArgumentNullException(nameof(metafile), PublicResources.ArgumentNull);

            if (requestedSize.Width < 1 || requestedSize.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(requestedSize), PublicResources.ArgumentOutOfRange);

            Size sourceSize = metafile.Size;
            Rectangle targetRectangle = new Rectangle(Point.Empty, requestedSize);

            if (keepAspectRatio && requestedSize != sourceSize)
            {
                float ratio = Math.Min((float)requestedSize.Width / sourceSize.Width, (float)requestedSize.Height / sourceSize.Height);
                targetRectangle.Size = new Size((int)(sourceSize.Width * ratio), (int)(sourceSize.Height * ratio));
                targetRectangle.Location = new Point((requestedSize.Width >> 1) - (targetRectangle.Width >> 1), (requestedSize.Height >> 1) - (targetRectangle.Height >> 1));
            }

            // NOTE: not using the image drawing constructor here, because it uses bilinear interpolation,
            //       which may cause ugly black edges for bitmap drawing records in case of legacy GDI metafile types.
            var result = new Bitmap(requestedSize.Width, requestedSize.Height);
            if (antiAliased)
            {
                try
                {
                    var doubledSize = new Size(targetRectangle.Width << 1, targetRectangle.Height << 1);
                    using Bitmap bmpDouble = new Bitmap(doubledSize.Width, doubledSize.Height);
                    using (Graphics g = Graphics.FromImage(bmpDouble))
                    {
                        // Interpolation mode must always be NN here. Matters when the metafile contains image drawing records, and the metafile type is WMF or EmfOnly.
                        // In this case the enlarged result with interpolation may cause ugly black contours at transparent edges.
                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g.DrawImage(metafile, new Rectangle(Point.Empty, doubledSize));
                    }

                    // Using DrawInto instead of Graphics.DrawImage here to prevent blocking other Graphics operations in the process during the slower interpolated resizing.
                    // The same kind of blocking occurs also when drawing the metafile, but unless the metafile contains interpolated image drawing records,
                    // the drawing operation tends to be fast enough.
                    bmpDouble.DrawInto(result, targetRectangle);
                    return result;
                }
                catch (Exception e) when (!e.IsCriticalGdi())
                {
                    // trying it again without antialiasing
                }
            }

            using (Graphics g = Graphics.FromImage(result))
            {
                // NN is not because we don't use anti-aliasing here, as for metafiles it makes a difference only when the metafile contains image drawing records,
                // and the metafile type is WMF or EmfOnly. In such case using other interpolation mode could cause ugly black edges if the requestedSize makes the original image zoom in.
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(metafile, targetRectangle);
            }

            return result;
        }

        /// <summary>
        /// Saves a <see cref="Metafile"/> instance into a <see cref="Stream"/>.
        /// Actual format is selected by the raw format of the metafile.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save.</param>
        /// <param name="stream">The <see cref="Stream"/> into the metafile should be saved.</param>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static void Save(this Metafile metafile, Stream stream) => Save(metafile, stream, false);

        /// <summary>
        /// Saves a <see cref="Metafile"/> instance into a <see cref="Stream"/> using the required format.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save.</param>
        /// <param name="stream">The <see cref="Stream"/> into the metafile should be saved.</param>
        /// <param name="forceWmfFormat">When <see langword="true"/>, forces to use the Windows Metafile Format (WMF), even if
        /// the <paramref name="metafile"/> itself is encoded by Enhanced Metafile Format (EMF). When <see langword="false"/>, uses the appropriate format automatically.</param>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
        [SecuritySafeCritical]
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static void Save(this Metafile metafile, Stream stream, bool forceWmfFormat)
        {
            if (metafile == null)
                throw new ArgumentNullException(nameof(metafile), PublicResources.ArgumentNull);
            if (stream == null)
                throw new ArgumentNullException(nameof(stream), PublicResources.ArgumentNull);
            if (!OSUtils.IsWindows)
                throw new PlatformNotSupportedException(DrawingRes.RequiresWindows);

            bool isWmf = metafile.RawFormat.Guid == ImageFormat.Wmf.Guid
                || (metafile.RawFormat.Guid == ImageFormat.Emf.Guid
                    ? false
                    : throw new ArgumentException(DrawingRes.MetafileExtensionsUnsupportedFormat, nameof(metafile))); // a deserialized instance may have a PNG raw format

            if (isWmf || forceWmfFormat)
                WriteWmfHeader(metafile, stream);

            // making a clone, otherwise, it will not be usable after saving
            metafile = (Metafile)metafile.Clone();
            IntPtr handle = metafile.GetHenhmetafile();
            try
            {
                byte[] buffer = isWmf ? Gdi32.GetWmfContent(handle)
                        : (forceWmfFormat ? Gdi32.GetWmfContentFromEmf(handle) : Gdi32.GetEmfContent(handle));
                stream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                if (isWmf)
                    Gdi32.DeleteMetaFile(handle);
                else
                    Gdi32.DeleteEnhMetaFile(handle);
                metafile.Dispose();
            }
        }

        /// <summary>
        /// Saves the specified <paramref name="metafile"/> as an EMF (Enhanced Metafile) using Windows API.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save. It must have <see cref="ImageFormat.Emf"/> raw format.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metafile"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Image.RawFormat"/> of the specified <paramref name="metafile"/> is not the <see cref="ImageFormat.Emf"/> format.</exception>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static void SaveAsEmf(this Metafile metafile, Stream stream)
        {
            if (metafile == null)
                throw new ArgumentNullException(nameof(metafile), PublicResources.ArgumentNull);
            if (metafile.RawFormat.Guid != ImageFormat.Emf.Guid)
                throw new ArgumentException(DrawingRes.MetafileExtensionsCannotBeSavedAsEmf, nameof(metafile));
            Save(metafile, stream, false);
        }

        /// <summary>
        /// Saves the specified <paramref name="metafile"/> to the specified file as an EMF (Enhanced Metafile) using Windows API.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save. It must have <see cref="ImageFormat.Emf"/> raw format.</param>
        /// <param name="fileName">The name of the file to which to save the <paramref name="metafile"/>. The directory of the specified path is created if it does not exist.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metafile"/> or <paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="Image.RawFormat"/> of the specified <paramref name="metafile"/> is not the <see cref="ImageFormat.Emf"/> format.</exception>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static void SaveAsEmf(this Metafile metafile, string fileName)
        {
            if (metafile == null)
                throw new ArgumentNullException(nameof(metafile), PublicResources.ArgumentNull);
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), PublicResources.ArgumentNull);
            using FileStream fs = Files.CreateWithPath(fileName);
            SaveAsEmf(metafile, fs);
        }

        /// <summary>
        /// Saves the specified <paramref name="metafile"/> as a WMF (Windows Metafile) using Windows API.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save.</param>
        /// <param name="stream">The stream to save the image into.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metafile"/> or <paramref name="stream"/> is <see langword="null"/>.</exception>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static void SaveAsWmf(this Metafile metafile, Stream stream) => Save(metafile, stream, true);

        /// <summary>
        /// Saves the specified <paramref name="metafile"/> to the specified file as a WMF (Windows Metafile) using Windows API.
        /// </summary>
        /// <param name="metafile">The <see cref="Metafile"/> instance to save.</param>
        /// <param name="fileName">The name of the file to which to save the <paramref name="metafile"/>. The directory of the specified path is created if it does not exist.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metafile"/> or <paramref name="fileName"/> is <see langword="null"/>.</exception>
        /// <exception cref="PlatformNotSupportedException">This method is supported on Windows only.</exception>
        /// <remarks>
        /// <note>This method is supported on Windows only.</note>
        /// </remarks>
#if NET
        [SupportedOSPlatform("windows")]
#endif
        public static void SaveAsWmf(this Metafile metafile, string fileName)
        {
            if (metafile == null)
                throw new ArgumentNullException(nameof(metafile), PublicResources.ArgumentNull);
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName), PublicResources.ArgumentNull);
            using FileStream fs = Files.CreateWithPath(fileName);
            SaveAsWmf(metafile, fs);
        }

        #endregion

        #region Private Methods

        [SecurityCritical]
        private static void WriteWmfHeader(Metafile metafile, Stream stream)
        {
            WmfHeader header = new WmfHeader((short)metafile.Width, (short)metafile.Height, (ushort)metafile.HorizontalResolution);
            byte[] rawHeader = BinarySerializer.SerializeValueType(header);
            stream.Write(rawHeader, 0, rawHeader.Length);
        }

        #endregion

        #endregion
    }
}
